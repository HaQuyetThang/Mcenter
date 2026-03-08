using MCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace MCenter.Core.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Server> Servers { get; set; } = null!;
    public DbSet<Workflow> Workflows { get; set; } = null!;
    public DbSet<Schedule> Schedules { get; set; } = null!;
    public DbSet<ScheduleServer> ScheduleServers { get; set; } = null!;
    public DbSet<Execution> Executions { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ScheduleServer>()
            .HasKey(ss => ss.Id);

        modelBuilder.Entity<ScheduleServer>()
            .HasOne(ss => ss.Schedule)
            .WithMany(s => s.ScheduleServers)
            .HasForeignKey(ss => ss.ScheduleId);

        modelBuilder.Entity<ScheduleServer>()
            .HasOne(ss => ss.Server)
            .WithMany(s => s.ScheduleServers)
            .HasForeignKey(ss => ss.ServerId);

        modelBuilder.Entity<Execution>()
            .HasOne(e => e.Schedule)
            .WithMany(s => s.Executions)
            .HasForeignKey(e => e.ScheduleId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Execution>()
            .HasOne(e => e.Server)
            .WithMany(s => s.Executions)
            .HasForeignKey(e => e.ServerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Execution>()
            .HasOne(e => e.Workflow)
            .WithMany(w => w.Executions)
            .HasForeignKey(e => e.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<Schedule>()
            .HasOne(s => s.Workflow)
            .WithMany(w => w.Schedules)
            .HasForeignKey(s => s.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
