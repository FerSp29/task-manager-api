using Microsoft.EntityFrameworkCore;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Repositories;

namespace TaskManager.Application.UnitTest;

public class TareaServiceTests
{
    private static AppDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task ObtenerVencidasAsync_RetornaSoloVencidasNoCompletadas()
    {
        using var context = GetInMemoryContext();
        var proyecto = new Proyecto { Nombre = "P1", FechaInicio = DateTime.UtcNow.AddDays(-10), FechaFin = DateTime.UtcNow.AddDays(10) };
        context.Proyectos.Add(proyecto);
        await context.SaveChangesAsync();

        context.Tareas.AddRange(
            new Tarea { Titulo = "Vencida Pendiente", Estado = EstadoTarea.Pendiente, FechaLimite = DateTime.UtcNow.AddDays(-2), ProyectoId = proyecto.Id },
            new Tarea { Titulo = "Vencida Completada", Estado = EstadoTarea.Completada, FechaLimite = DateTime.UtcNow.AddDays(-2), ProyectoId = proyecto.Id },
            new Tarea { Titulo = "Vigente Pendiente", Estado = EstadoTarea.Pendiente, FechaLimite = DateTime.UtcNow.AddDays(3), ProyectoId = proyecto.Id }
        );
        await context.SaveChangesAsync();

        var service = new TareaService(new TareaRepository(context), new ProyectoRepository(context));
        var vencidas = (await service.ObtenerVencidasAsync()).ToList();

        Assert.Single(vencidas);
        Assert.Equal("Vencida Pendiente", vencidas[0].Titulo);
    }

    [Fact]
    public async Task ObtenerTodasAsync_FiltraPorEstadoYPrioridad()
    {
        using var context = GetInMemoryContext();
        var proyecto = new Proyecto { Nombre = "P1", FechaInicio = DateTime.UtcNow, FechaFin = DateTime.UtcNow.AddDays(10) };
        context.Proyectos.Add(proyecto);
        await context.SaveChangesAsync();

        context.Tareas.AddRange(
            new Tarea { Titulo = "Target", Estado = EstadoTarea.EnProgreso, Prioridad = PrioridadTarea.Alta, FechaLimite = DateTime.UtcNow.AddDays(2), ProyectoId = proyecto.Id },
            new Tarea { Titulo = "Otra", Estado = EstadoTarea.Pendiente, Prioridad = PrioridadTarea.Alta, FechaLimite = DateTime.UtcNow.AddDays(2), ProyectoId = proyecto.Id }
        );
        await context.SaveChangesAsync();

        var service = new TareaService(new TareaRepository(context), new ProyectoRepository(context));
        var filtradas = (await service.ObtenerTodasAsync(EstadoTarea.EnProgreso, PrioridadTarea.Alta)).ToList();

        Assert.Single(filtradas);
        Assert.Equal("Target", filtradas[0].Titulo);
    }

    [Fact]
    public async Task CambiarEstadoAsync_ActualizaEstadoCorrectamente()
    {
        using var context = GetInMemoryContext();
        var proyecto = new Proyecto { Nombre = "P1", FechaInicio = DateTime.UtcNow, FechaFin = DateTime.UtcNow.AddDays(10) };
        context.Proyectos.Add(proyecto);
        await context.SaveChangesAsync();

        var tarea = new Tarea { Titulo = "T1", Estado = EstadoTarea.Pendiente, FechaLimite = DateTime.UtcNow.AddDays(2), ProyectoId = proyecto.Id };
        context.Tareas.Add(tarea);
        await context.SaveChangesAsync();

        var service = new TareaService(new TareaRepository(context), new ProyectoRepository(context));
        var actualizada = await service.CambiarEstadoAsync(tarea.Id, new CambiarEstadoTareaDto(EstadoTarea.Completada));

        Assert.Equal(EstadoTarea.Completada, actualizada.Estado);
    }
}