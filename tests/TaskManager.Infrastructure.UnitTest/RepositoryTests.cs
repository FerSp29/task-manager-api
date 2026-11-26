using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Repositories;

namespace TaskManager.Infrastructure.UnitTest;

public class RepositoryTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task ProyectoRepository_TieneTareasPendientesAsync_RetornaTrueSiHayPendienteOEnProgreso()
    {
        using var context = CreateInMemoryContext();
        var proyecto = new Proyecto
        {
            Nombre = "Infra Proyecto",
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(10),
            Tareas = new List<Tarea>
            {
                new() { Titulo = "En progreso", Estado = EstadoTarea.EnProgreso, FechaLimite = DateTime.UtcNow.AddDays(2) }
            }
        };
        context.Proyectos.Add(proyecto);
        await context.SaveChangesAsync();

        var repo = new ProyectoRepository(context);

        Assert.True(await repo.TieneTareasPendientesAsync(proyecto.Id));
    }

    [Fact]
    public async Task ProyectoRepository_TieneTareasPendientesAsync_RetornaFalseSiTodasCompletadas()
    {
        using var context = CreateInMemoryContext();
        var proyecto = new Proyecto
        {
            Nombre = "Infra Completado",
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(10),
            Tareas = new List<Tarea>
            {
                new() { Titulo = "Hecha", Estado = EstadoTarea.Completada, FechaLimite = DateTime.UtcNow.AddDays(2) }
            }
        };
        context.Proyectos.Add(proyecto);
        await context.SaveChangesAsync();

        var repo = new ProyectoRepository(context);

        Assert.False(await repo.TieneTareasPendientesAsync(proyecto.Id));
    }

    [Fact]
    public async Task TareaRepository_GetByProyectoIdAsync_RetornaSoloTareasDelProyecto()
    {
        using var context = CreateInMemoryContext();
        var p1 = new Proyecto { Nombre = "P1", FechaInicio = DateTime.UtcNow, FechaFin = DateTime.UtcNow.AddDays(5) };
        var p2 = new Proyecto { Nombre = "P2", FechaInicio = DateTime.UtcNow, FechaFin = DateTime.UtcNow.AddDays(5) };
        context.Proyectos.AddRange(p1, p2);
        await context.SaveChangesAsync();

        context.Tareas.AddRange(
            new Tarea { Titulo = "De P1", ProyectoId = p1.Id, FechaLimite = DateTime.UtcNow.AddDays(1) },
            new Tarea { Titulo = "De P2", ProyectoId = p2.Id, FechaLimite = DateTime.UtcNow.AddDays(1) }
        );
        await context.SaveChangesAsync();

        var repo = new TareaRepository(context);
        var tareasP1 = (await repo.GetByProyectoIdAsync(p1.Id)).ToList();

        Assert.Single(tareasP1);
        Assert.Equal("De P1", tareasP1[0].Titulo);
    }
}
