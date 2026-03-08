using MCenter.API.Models.DTOs;
using MCenter.API.Services.Scheduler;
using MCenter.Core.Data;
using MCenter.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MCenter.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SchedulesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWorkflowScheduler _scheduler;

    public SchedulesController(AppDbContext context, IWorkflowScheduler scheduler)
    {
        _context = context;
        _scheduler = scheduler;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScheduleDto>>> GetSchedules()
    {
        return await _context.Schedules
            .Include(s => s.Workflow)
            .Include(s => s.ScheduleServers)
            .Select(s => new ScheduleDto
            {
                Id = s.Id,
                WorkflowId = s.WorkflowId,
                WorkflowName = s.Workflow.Name,
                Name = s.Name,
                CronExpression = s.CronExpression,
                IsActive = s.IsActive,
                NextRunTime = s.NextRunTime,
                ServerIds = s.ScheduleServers.Select(ss => ss.ServerId).ToList()
            })
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ScheduleDto>> GetSchedule(int id)
    {
        var schedule = await _context.Schedules
            .Include(s => s.Workflow)
            .Include(s => s.ScheduleServers)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (schedule == null)
        {
            return NotFound();
        }

        return new ScheduleDto
        {
            Id = schedule.Id,
            WorkflowId = schedule.WorkflowId,
            WorkflowName = schedule.Workflow.Name,
            Name = schedule.Name,
            CronExpression = schedule.CronExpression,
            IsActive = schedule.IsActive,
            NextRunTime = schedule.NextRunTime,
            ServerIds = schedule.ScheduleServers.Select(ss => ss.ServerId).ToList()
        };
    }

    [HttpPost]
    public async Task<ActionResult<ScheduleDto>> CreateSchedule(ScheduleCreateDto createDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var schedule = new Schedule
            {
                WorkflowId = createDto.WorkflowId,
                Name = createDto.Name,
                CronExpression = createDto.CronExpression,
                IsActive = true
            };

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            foreach (var serverId in createDto.ServerIds)
            {
                _context.ScheduleServers.Add(new ScheduleServer
                {
                    ScheduleId = schedule.Id,
                    ServerId = serverId
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _scheduler.RegisterSchedule(schedule);

            return CreatedAtAction(nameof(GetSchedule), new { id = schedule.Id }, new ScheduleDto
            {
                Id = schedule.Id,
                WorkflowId = schedule.WorkflowId,
                Name = schedule.Name,
                CronExpression = schedule.CronExpression,
                IsActive = schedule.IsActive,
                ServerIds = createDto.ServerIds
            });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return BadRequest("Lỗi khi tạo lịch chạy.");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSchedule(int id, ScheduleCreateDto updateDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var schedule = await _context.Schedules
                .Include(s => s.ScheduleServers)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
            {
                return NotFound();
            }

            schedule.WorkflowId = updateDto.WorkflowId;
            schedule.Name = updateDto.Name;
            schedule.CronExpression = updateDto.CronExpression;

            // Update servers
            _context.ScheduleServers.RemoveRange(schedule.ScheduleServers);
            foreach (var serverId in updateDto.ServerIds)
            {
                _context.ScheduleServers.Add(new ScheduleServer
                {
                    ScheduleId = schedule.Id,
                    ServerId = serverId
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _scheduler.RegisterSchedule(schedule);

            return NoContent();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return BadRequest("Lỗi khi cập nhật lịch chạy.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSchedule(int id)
    {
        var schedule = await _context.Schedules.FindAsync(id);
        if (schedule == null)
        {
            return NotFound();
        }

        _context.Schedules.Remove(schedule);
        await _context.SaveChangesAsync();

        _scheduler.RemoveSchedule(id);

        return NoContent();
    }

    [HttpPost("{id}/toggle")]
    public async Task<IActionResult> ToggleScheduleStatus(int id)
    {
        var schedule = await _context.Schedules.FindAsync(id);
        if (schedule == null)
        {
            return NotFound();
        }

        schedule.IsActive = !schedule.IsActive;
        
        if (!schedule.IsActive)
        {
            // Khi tắt lịch, hủy bỏ tất cả các bản ghi đang chờ (Queued) của lịch này
            var pendingExecutions = await _context.Executions
                .Where(e => e.ScheduleId == id && e.Status == "Queued")
                .ToListAsync();
            
            foreach (var execution in pendingExecutions)
            {
                execution.Status = "Cancelled";
            }
            
            _scheduler.RemoveSchedule(schedule.Id);
        }
        else
        {
            _scheduler.RegisterSchedule(schedule);
        }

        await _context.SaveChangesAsync();
        return Ok(new { isActive = schedule.IsActive });
    }
}
