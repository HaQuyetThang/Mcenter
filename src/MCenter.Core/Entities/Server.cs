namespace MCenter.Core.Entities;

public class Server
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Status { get; set; } = "Offline"; // Online/Offline/Busy
    public DateTime? LastHeartbeat { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ScheduleServer> ScheduleServers { get; set; } = new List<ScheduleServer>();
    public ICollection<Execution> Executions { get; set; } = new List<Execution>();
}
