using System.Diagnostics;
using System.Text.Json;

namespace MCenter.Agent.Services;

public class UiPathExecutor
{
    private readonly string _uiRobotPath;
    private readonly string _logDirectory;
    private readonly ILogger<UiPathExecutor> _logger;

    public UiPathExecutor(IConfiguration configuration, ILogger<UiPathExecutor> logger)
    {
        _uiRobotPath = configuration["UiPath:RobotPath"] ?? @"C:\Program Files\UiPath\Studio\UiRobot.exe";
        _logDirectory = configuration["UiPath:LogDirectory"]
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UiPath", "Logs");
        _logger = logger;
    }

    public async Task<(bool Success, string Output)> ExecuteAsync(string projectPath, CancellationToken ct)
    {
        if (!File.Exists(_uiRobotPath))
        {
            return (false, $"Lỗi: Không tìm thấy UiRobot.exe tại {_uiRobotPath}");
        }

        _logger.LogInformation("Bắt đầu thực thi workflow: {Path}", projectPath);

        var startTime = DateTime.Now;

        var processInfo = new ProcessStartInfo
        {
            FileName = _uiRobotPath,
            Arguments = $"execute --file \"{projectPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = new Process { StartInfo = processInfo };
            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync(ct);
            string error = await process.StandardError.ReadToEndAsync(ct);

            await process.WaitForExitAsync(ct);
            var endTime = DateTime.Now;

            bool success = process.ExitCode == 0;

            _logger.LogInformation("Kết thúc thực thi workflow. ExitCode: {Code}", process.ExitCode);

            // Đọc log UiPath từ file, retry tối đa 3 lần
            var uiPathLog = await ReadUiPathLogsWithRetry(startTime, endTime, maxRetries: 3, ct: ct);
            
            // Kết hợp stdout/stderr (nếu có) với log UiPath
            var combinedOutput = success
                ? uiPathLog
                : $"[ExitCode: {process.ExitCode}]\n{(string.IsNullOrWhiteSpace(error) ? "" : $"StdErr: {error}\n")}{uiPathLog}";

            return (success, combinedOutput.TrimEnd());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi chạy UiPath Process.");
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Đọc log UiPath từ file, lọc theo khoảng thời gian thực thi. Retry tối đa <paramref name="maxRetries"/> lần.
    /// </summary>
    private async Task<string> ReadUiPathLogsWithRetry(DateTime startTime, DateTime endTime, int maxRetries, CancellationToken ct)
    {
        var logFilePath = Path.Combine(_logDirectory, $"{startTime:yyyy-MM-dd}_Execution.log");

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                if (!File.Exists(logFilePath))
                {
                    _logger.LogWarning("Không tìm thấy file log UiPath: {Path}", logFilePath);
                    return "[Log không khả dụng: file không tồn tại]";
                }

                // Mở với FileShare.ReadWrite để tránh bị block khi UiPath vẫn đang ghi
                using var stream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);

                var lines = new List<string>();
                string? line;
                while ((line = await reader.ReadLineAsync(ct)) != null)
                {
                    // Mỗi dòng log có dạng: "HH:mm:ss.ffff Level {json}"
                    // Lấy phần JSON để parse timestamp
                    var jsonStart = line.IndexOf('{');
                    if (jsonStart < 0) continue;

                    try
                    {
                        var jsonPart = line[jsonStart..];
                        using var doc = JsonDocument.Parse(jsonPart);
                        if (doc.RootElement.TryGetProperty("timeStamp", out var ts) &&
                            DateTime.TryParse(ts.GetString(), out var logTime))
                        {
                            // Lọc theo khoảng thời gian thực thi (buffer thêm 2s để tránh lệch clock)
                            if (logTime >= startTime.AddSeconds(-2) && logTime <= endTime.AddSeconds(2))
                            {
                                // Format hiển thị thân thiện: [HH:mm:ss] [level] message
                                var msg = doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() : "";
                                var level = doc.RootElement.TryGetProperty("level", out var l) ? l.GetString() : "";
                                lines.Add($"[{logTime:HH:mm:ss}] [{level}] {msg}");
                            }
                        }
                    }
                    catch (JsonException) { /* Bỏ qua dòng không hợp lệ */ }
                }

                if (lines.Count > 0)
                {
                    _logger.LogInformation("Đọc được {Count} dòng log UiPath.", lines.Count);
                    return string.Join("\n", lines);
                }

                // Nếu chưa có log, có thể UiPath chưa flush xong → retry
                _logger.LogWarning("Chưa tìm thấy log trong khoảng thời gian thực thi (lần {Attempt}/{Max}). Chờ 1 giây...", attempt, maxRetries);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi đọc log UiPath (lần {Attempt}/{Max}).", attempt, maxRetries);
            }

            if (attempt < maxRetries)
            {
                await Task.Delay(1000, ct);
            }
        }

        return "[Log không khả dụng: đã thử {maxRetries} lần nhưng không đọc được]";
    }
}
