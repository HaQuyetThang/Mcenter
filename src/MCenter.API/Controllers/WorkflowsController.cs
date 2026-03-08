using MCenter.API.Models.DTOs;
using MCenter.Core.Data;
using MCenter.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MCenter.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WorkflowsController : ControllerBase
{
    private readonly AppDbContext _context;

    public WorkflowsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkflowDto>>> GetWorkflows()
    {
        return await _context.Workflows
            .Select(w => new WorkflowDto
            {
                Id = w.Id,
                Name = w.Name,
                PackagePath = w.PackagePath,
                Version = w.Version,
                IsActive = w.IsActive
            })
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkflowDto>> GetWorkflow(int id)
    {
        var workflow = await _context.Workflows.FindAsync(id);

        if (workflow == null)
        {
            return NotFound();
        }

        return new WorkflowDto
        {
            Id = workflow.Id,
            Name = workflow.Name,
            PackagePath = workflow.PackagePath,
            Version = workflow.Version,
            IsActive = workflow.IsActive
        };
    }

    [HttpPost]
    public async Task<ActionResult<WorkflowDto>> CreateWorkflow(WorkflowCreateDto createDto)
    {
        var workflow = new Workflow
        {
            Name = createDto.Name,
            PackagePath = createDto.PackagePath,
            Version = createDto.Version,
            IsActive = true
        };

        _context.Workflows.Add(workflow);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWorkflow), new { id = workflow.Id }, new WorkflowDto
        {
            Id = workflow.Id,
            Name = workflow.Name,
            PackagePath = workflow.PackagePath,
            Version = workflow.Version,
            IsActive = workflow.IsActive
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWorkflow(int id, WorkflowCreateDto updateDto)
    {
        var workflow = await _context.Workflows.FindAsync(id);

        if (workflow == null)
        {
            return NotFound();
        }

        workflow.Name = updateDto.Name;
        workflow.PackagePath = updateDto.PackagePath;
        workflow.Version = updateDto.Version;
        workflow.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkflow(int id)
    {
        var workflow = await _context.Workflows.FindAsync(id);
        if (workflow == null)
        {
            return NotFound();
        }

        _context.Workflows.Remove(workflow);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
