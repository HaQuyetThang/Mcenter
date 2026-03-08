using Hangfire;
using MCenter.Core.Data;
using MCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace MCenter.API.Services.Scheduler;

public interface IWorkflowScheduler
{
    void RegisterSchedule(Schedule schedule);
    void RemoveSchedule(int scheduleId);
    Task ExecuteScheduleAsync(int scheduleId);
}

public class WorkflowScheduler : IWorkflowScheduler
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IServiceProvider _serviceProvider;

    public WorkflowScheduler(IRecurringJobManager recurringJobManager, IServiceProvider serviceProvider)
    {
        _recurringJobManager = recurringJobManager;
        _serviceProvider = serviceProvider;
    }

    public void RegisterSchedule(Schedule schedule)
    {
        _recurringJobManager.AddOrUpdate(
            $"schedule-{schedule.Id}",
            () => ExecuteScheduleAsync(schedule.Id),
            schedule.CronExpression);
    }

    public void RemoveSchedule(int scheduleId)
    {
        _recurringJobManager.RemoveIfExists($"schedule-{scheduleId}");
    }

    public async Task ExecuteScheduleAsync(int scheduleId)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var schedule = await context.Schedules
            .Include(s => s.ScheduleServers)
            .FirstOrDefaultAsync(s => s.Id == scheduleId);

        if (schedule == null || !schedule.IsActive) return;

        // For each server assigned to this schedule, create a Queued execution
        foreach (var serverMapping in schedule.ScheduleServers)
        {
            var execution = new Execution
            {
                ScheduleId = schedule.Id,
                WorkflowId = schedule.WorkflowId,
                ServerId = serverMapping.ServerId,
                Status = "Queued",
                TriggeredBy = "Schedule",
                CreatedAt = DateTime.UtcNow
            };

            context.Executions.Add(execution);
        }

        await context.SaveChangesAsync();
    }
}
