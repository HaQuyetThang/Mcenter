namespace MCenter.Core.Entities;

public class Execution
{
    public int Id { get; set; }
    public int? ScheduleId { get; set; }
    public int? ServerId { get; set; }
    public int WorkflowId { get; set; }
    public string Status { get; set; } = "Queued"; // Queued/Running/Success/Failed
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? LogOutput { get; set; }
    public string TriggeredBy { get; set; } = "Schedule"; // Schedule/Manual

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Schedule? Schedule { get; set; }
    public Server? Server { get; set; }
    public Workflow Workflow { get; set; } = null!;
}
