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
public class ServersController : ControllerBase
{
    private readonly AppDbContext _context;

    public ServersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServerDto>>> GetServers()
    {
        return await _context.Servers
            .Select(s => new ServerDto
            {
                Id = s.Id,
                Name = s.Name,
                IpAddress = s.IpAddress,
                Status = s.Status,
                LastHeartbeat = s.LastHeartbeat
            })
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServerDto>> GetServer(int id)
    {
        var server = await _context.Servers.FindAsync(id);

        if (server == null)
        {
            return NotFound();
        }

        return new ServerDto
        {
            Id = server.Id,
            Name = server.Name,
            IpAddress = server.IpAddress,
            Status = server.Status,
            LastHeartbeat = server.LastHeartbeat
        };
    }

    [HttpPost]
    public async Task<ActionResult<ServerDto>> CreateServer(ServerCreateDto createDto)
    {
        var server = new Server
        {
            Name = createDto.Name,
            IpAddress = createDto.IpAddress,
            Status = "Offline"
        };

        _context.Servers.Add(server);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetServer), new { id = server.Id }, new ServerDto
        {
            Id = server.Id,
            Name = server.Name,
            IpAddress = server.IpAddress,
            Status = server.Status
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateServer(int id, ServerCreateDto updateDto)
    {
        var server = await _context.Servers.FindAsync(id);

        if (server == null)
        {
            return NotFound();
        }

        server.Name = updateDto.Name;
        server.IpAddress = updateDto.IpAddress;
        server.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteServer(int id)
    {
        var server = await _context.Servers.FindAsync(id);
        if (server == null)
        {
            return NotFound();
        }

        _context.Servers.Remove(server);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
