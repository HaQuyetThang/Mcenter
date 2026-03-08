using MCenter.API.Models.Agent;
using MCenter.API.Models.DTOs;
using MCenter.Core.Data;
using MCenter.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MCenter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AgentController> _logger;

    public AgentController(AppDbContext context, ILogger<AgentController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("pending-jobs/{serverId}")]
    public async Task<ActionResult<IEnumerable<ExecutionDto>>> GetPendingJobs(int serverId)
    {
        var server = await _context.Servers.FindAsync(serverId);
        if (server == null) return NotFound("Server không tồn tại.");

        var jobs = await _context.Executions
            .Include(e => e.Workflow)
            .Include(e => e.Schedule)
            .Where(e => e.ServerId == serverId && e.Status == "Queued")
            .Where(e => e.Schedule == null || e.Schedule.IsActive) // Chỉ lấy việc nếu lịch trình đang kích hoạt
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();

        return jobs.Select(e => new ExecutionDto
        {
            Id = e.Id,
            ScheduleId = e.ScheduleId,
            ScheduleName = e.Schedule?.Name,
            ServerId = e.ServerId,
            WorkflowId = e.WorkflowId,
            WorkflowName = e.Workflow.Name,
            Status = e.Status,
            CreatedAt = e.CreatedAt,
            // Cung cấp đường dẫn package cho Agent
            LogOutput = e.Workflow.PackagePath 
        }).ToList();
    }

    [HttpPost("heartbeat/{serverId}")]
    public async Task<IActionResult> Heartbeat(int serverId, HeartbeatRequest request)
    {
        var server = await _context.Servers.FindAsync(serverId);
        
        if (server == null)
        {
            // Tìm theo Tên máy chủ hoặc IP để tránh đăng ký trùng khi Agent restart
            server = await _context.Servers
                .FirstOrDefaultAsync(s => s.Name == request.ServerName || s.IpAddress == request.IpAddress);
            
            if (server == null)
            {
                server = new Server { 
                    IpAddress = request.IpAddress, 
                    Name = request.ServerName,
                    Status = "Online",
                    LastHeartbeat = DateTime.UtcNow
                };
                _context.Servers.Add(server);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Máy chủ mới tự động đăng ký: {Name} ({IP})", server.Name, server.IpAddress);
            }
        }

        server.LastHeartbeat = DateTime.UtcNow;
        server.Status = request.Status;
        server.Name = string.IsNullOrEmpty(request.ServerName) ? server.Name : request.ServerName;
        
        await _context.SaveChangesAsync();
        return Ok(new { serverId = server.Id });
    }

    [HttpPut("executions/{executionId}/status")]
    public async Task<IActionResult> UpdateExecutionStatus(int executionId, ExecutionStatusUpdate update)
    {
        var execution = await _context.Executions.FindAsync(executionId);
        if (execution == null) return NotFound();

        execution.Status = update.Status;
        if (update.Status == "Running" && execution.StartTime == null)
        {
            execution.StartTime = DateTime.UtcNow;
        }
        else if (update.Status == "Success" || update.Status == "Failed")
        {
            execution.EndTime = DateTime.UtcNow;
        }

        if (!string.IsNullOrEmpty(update.LogOutput))
        {
            execution.LogOutput = (execution.LogOutput ?? "") + "\n" + update.LogOutput;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }
}
