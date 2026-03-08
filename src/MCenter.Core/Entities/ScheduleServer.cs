namespace MCenter.Core.Entities;

public class ScheduleServer
{
    public int Id { get; set; }
    public int ScheduleId { get; set; }
    public int ServerId { get; set; }

    public Schedule Schedule { get; set; } = null!;
    public Server Server { get; set; } = null!;
}
