namespace MCenter.API.Models.Agent;

public class HeartbeatRequest
{
    public string ServerName { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Status { get; set; } = "Online";
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
}

public class ExecutionStatusUpdate
{
    public string Status { get; set; } = string.Empty; // Running/Success/Failed
    public string? LogOutput { get; set; }
}
