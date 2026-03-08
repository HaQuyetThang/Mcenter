namespace MCenter.Agent.Models;

public class ExecutionDto
{
    public int Id { get; set; }
    public int WorkflowId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? LogOutput { get; set; } // PackgePath is passed here for Polling simplicity
}

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
    public string Status { get; set; } = string.Empty;
    public string? LogOutput { get; set; }
}

public class HeartbeatResponse
{
    public int ServerId { get; set; }
}
