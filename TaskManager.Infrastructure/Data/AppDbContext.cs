using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;


namespace TaskManager.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Proyecto> Proyectos => Set<Proyecto>();
    public DbSet<Tarea> Tareas => Set<Tarea>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Tarea>()
            .HasOne(t => t.Proyecto)
            .WithMany(p => p.Tareas)
            .HasForeignKey(t => t.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tarea>()
            .Property(t => t.Estado)
            .HasConversion<string>();
        
        modelBuilder.Entity<Tarea>()
            .Property(t => t.Prioridad)
            .HasConversion<string>();
    }
}