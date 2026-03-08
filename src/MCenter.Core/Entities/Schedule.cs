namespace MCenter.Core.Entities;

public class Schedule
{
    public int Id { get; set; }
    public int WorkflowId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? NextRunTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Workflow Workflow { get; set; } = null!;
    public ICollection<ScheduleServer> ScheduleServers { get; set; } = new List<ScheduleServer>();
    public ICollection<Execution> Executions { get; set; } = new List<Execution>();
}
