using MCenter.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace MCenter.API.Services.Monitoring;

public class ServerMonitorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ServerMonitorService> _logger;
    private readonly TimeSpan _timeout = TimeSpan.FromMinutes(1);
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

    public ServerMonitorService(IServiceProvider serviceProvider, ILogger<ServerMonitorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Dịch vụ giám sát máy chủ đã bắt đầu.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckServerStatus(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra trạng thái máy chủ.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckServerStatus(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var threshold = DateTime.UtcNow.Subtract(_timeout);

        // Tìm các server Online/Busy nhưng quá lâu không gửi heartbeat
        var staleServers = await context.Servers
            .Where(s => (s.Status == "Online" || s.Status == "Busy") && s.LastHeartbeat < threshold)
            .ToListAsync(ct);

        if (staleServers.Any())
        {
            foreach (var server in staleServers)
            {
                _logger.LogInformation("Máy chủ {Name} (ID: {Id}) mất kết nối. Chuyển sang Offline.", server.Name, server.Id);
                server.Status = "Offline";
            }

            await context.SaveChangesAsync(ct);
        }
    }
}
