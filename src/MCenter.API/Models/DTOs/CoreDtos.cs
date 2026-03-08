namespace MCenter.API.Models.DTOs;

public class ServerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? LastHeartbeat { get; set; }
}

public class ServerCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
}

public class WorkflowDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PackagePath { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class WorkflowCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string PackagePath { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
}

public class ScheduleDto
{
    public int Id { get; set; }
    public int WorkflowId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? NextRunTime { get; set; }
    public List<int> ServerIds { get; set; } = new();
}

public class ScheduleCreateDto
{
    public int WorkflowId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public List<int> ServerIds { get; set; } = new();
}

public class ExecutionDto
{
    public int Id { get; set; }
    public int? ScheduleId { get; set; }
    public string? ScheduleName { get; set; }
    public int? ServerId { get; set; }
    public string? ServerName { get; set; }
    public int WorkflowId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? LogOutput { get; set; }
    public string TriggeredBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
