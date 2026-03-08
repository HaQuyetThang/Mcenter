namespace MCenter.Core.Entities;

public class Workflow
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PackagePath { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    public ICollection<Execution> Executions { get; set; } = new List<Execution>();
}
