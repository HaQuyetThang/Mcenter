using MCenter.API.Models.DTOs;
using MCenter.Core.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MCenter.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExecutionsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ExecutionsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExecutionDto>>> GetExecutions()
    {
        return await _context.Executions
            .Include(e => e.Workflow)
            .Include(e => e.Server)
            .Include(e => e.Schedule)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new ExecutionDto
            {
                Id = e.Id,
                ScheduleId = e.ScheduleId,
                ScheduleName = e.Schedule != null ? e.Schedule.Name : null,
                ServerId = e.ServerId,
                ServerName = e.Server != null ? e.Server.Name : null,
                WorkflowId = e.WorkflowId,
                WorkflowName = e.Workflow.Name,
                Status = e.Status,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                LogOutput = e.LogOutput,
                TriggeredBy = e.TriggeredBy,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExecutionDto>> GetExecution(int id)
    {
        var e = await _context.Executions
            .Include(e => e.Workflow)
            .Include(e => e.Server)
            .Include(e => e.Schedule)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (e == null)
        {
            return NotFound();
        }

        return new ExecutionDto
        {
            Id = e.Id,
            ScheduleId = e.ScheduleId,
            ScheduleName = e.Schedule != null ? e.Schedule.Name : null,
            ServerId = e.ServerId,
            ServerName = e.Server != null ? e.Server.Name : null,
            WorkflowId = e.WorkflowId,
            WorkflowName = e.Workflow.Name,
            Status = e.Status,
            StartTime = e.StartTime,
            EndTime = e.EndTime,
            LogOutput = e.LogOutput,
            TriggeredBy = e.TriggeredBy,
            CreatedAt = e.CreatedAt
        };
    }
}
