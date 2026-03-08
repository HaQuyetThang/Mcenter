using System.Net.Http.Json;
using MCenter.Agent.Models;

namespace MCenter.Agent.Services;

public class AgentWorker : BackgroundService
{
    private readonly ILogger<AgentWorker> _logger;
    private readonly IConfiguration _configuration;
    private readonly UiPathExecutor _executor;
    private readonly HttpClient _httpClient;
    private int _serverId;
    private readonly string _serverName;
    private readonly string _apiBaseUrl;

    public AgentWorker(ILogger<AgentWorker> logger, IConfiguration configuration, UiPathExecutor executor)
    {
        _logger = logger;
        _configuration = configuration;
        _executor = executor;
        _httpClient = new HttpClient();
        
        _serverId = _configuration.GetValue<int>("Agent:ServerId", 0);
        _serverName = _configuration.GetValue("Agent:ServerName", Environment.MachineName) ?? "Unknown";
        _apiBaseUrl = _configuration.GetValue("Agent:ApiBaseUrl", "http://localhost:5000")?.TrimEnd('/') ?? "";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MCenter Agent đã bắt đầu.");

        // Khởi động vòng lặp Heartbeat (chạy song song)
        var heartbeatTask = RunHeartbeatLoop(stoppingToken);

        // Vòng lặp chính để Polling công việc
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_serverId == 0)
            {
                await Task.Delay(5000, stoppingToken);
                continue;
            }

            try
            {
                var jobs = await _httpClient.GetFromJsonAsync<List<ExecutionDto>>(
                    $"{_apiBaseUrl}/api/agent/pending-jobs/{_serverId}", stoppingToken);

                if (jobs != null && jobs.Any())
                {
                    foreach (var job in jobs)
                    {
                        await ProcessJob(job, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi polling công việc.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_configuration.GetValue("Agent:PollingIntervalSeconds", 10)), stoppingToken);
        }

        await heartbeatTask;
    }

    private async Task RunHeartbeatLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                // Lấy IP thật của máy
                string localIp = "127.0.0.1";
                try {
                    using (System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, 0))
                    {
                        socket.Connect("8.8.8.8", 65530);
                        var endPoint = socket.LocalEndPoint as System.Net.IPEndPoint;
                        localIp = endPoint?.Address.ToString() ?? "127.0.0.1";
                    }
                } catch { /* Fallback to 127.0.0.1 */ }

                var request = new HeartbeatRequest
                {
                    ServerName = _serverName,
                    IpAddress = localIp,
                    Status = "Online"
                };

                var response = await _httpClient.PostAsJsonAsync($"{_apiBaseUrl}/api/agent/heartbeat/{_serverId}", request, ct);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<HeartbeatResponse>(cancellationToken: ct);
                    if (result != null && _serverId == 0)
                    {
                        _serverId = result.ServerId;
                        _logger.LogInformation("Đã tự động đăng ký Server. ID mới: {Id}", _serverId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Không thể gửi heartbeat: {Msg}", ex.Message);
            }

            await Task.Delay(TimeSpan.FromSeconds(30), ct);
        }
    }

    private async Task ProcessJob(ExecutionDto job, CancellationToken ct)
    {
        _logger.LogInformation("Nhận Job mới: {JobId} - Workflow: {Workflow}", job.Id, job.WorkflowName);

        // 1. Cập nhật trạng thái Running
        await _httpClient.PutAsJsonAsync($"{_apiBaseUrl}/api/agent/executions/{job.Id}/status", 
            new ExecutionStatusUpdate { Status = "Running" }, ct);

        // 2. Thực thi UiPath (LogOutput bên API đang gửi tạm PackagePath)
        var result = await _executor.ExecuteAsync(job.LogOutput ?? "", ct);

        // 3. Báo cáo kết quả
        await _httpClient.PutAsJsonAsync($"{_apiBaseUrl}/api/agent/executions/{job.Id}/status", 
            new ExecutionStatusUpdate 
            { 
                Status = result.Success ? "Success" : "Failed",
                LogOutput = result.Output
            }, ct);
            
        _logger.LogInformation("Đã hoàn thành Job: {JobId}. Kết quả: {Res}", job.Id, result.Success);
    }
}
