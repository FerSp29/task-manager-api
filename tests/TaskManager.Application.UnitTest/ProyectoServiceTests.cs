using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Repositories;

namespace TaskManager.Application.UnitTest;

public class ProyectoServiceTests
{
    private static AppDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_CalculaPorcentajeAvanceCorrectamente()
    {
        using var context = GetInMemoryContext();
        var proyecto = new Proyecto
        {
            Nombre = "Proyecto Alpha",
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(10),
            Tareas = new List<Tarea>
            {
                new() { Titulo = "T1", Estado = EstadoTarea.Completada, FechaLimite = DateTime.UtcNow.AddDays(2) },
                new() { Titulo = "T2", Estado = EstadoTarea.Completada, FechaLimite = DateTime.UtcNow.AddDays(3) },
                new() { Titulo = "T3", Estado = EstadoTarea.EnProgreso, FechaLimite = DateTime.UtcNow.AddDays(4) },
                new() { Titulo = "T4", Estado = EstadoTarea.Pendiente, FechaLimite = DateTime.UtcNow.AddDays(5) }
            }
        };
        context.Proyectos.Add(proyecto);
        await context.SaveChangesAsync();

        var service = new ProyectoService(new ProyectoRepository(context));
        var resultado = await service.ObtenerPorIdAsync(proyecto.Id);

        Assert.Equal(4, resultado.TotalTareas);
        Assert.Equal(2, resultado.TareasCompletadas);
        Assert.Equal(50.0, resultado.PorcentajeAvance);
    }

    [Fact]
    public async Task EliminarAsync_ConTareasPendientes_LanzaBusinessRuleException()
    {
        using var context = GetInMemoryContext();
        var proyecto = new Proyecto
        {
            Nombre = "Proyecto Bloqueado",
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(10),
            Tareas = new List<Tarea>
            {
                new() { Titulo = "Pendiente", Estado = EstadoTarea.Pendiente, FechaLimite = DateTime.UtcNow.AddDays(2) }
            }
        };
        context.Proyectos.Add(proyecto);
        await context.SaveChangesAsync();

        var service = new ProyectoService(new ProyectoRepository(context));

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.EliminarAsync(proyecto.Id));
    }

    [Fact]
    public async Task EliminarAsync_ConTodasTareasCompletadas_EliminaProyecto()
    {
        using var context = GetInMemoryContext();
        var proyecto = new Proyecto
        {
            Nombre = "Proyecto Listo",
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(10),
            Tareas = new List<Tarea>
            {
                new() { Titulo = "Hecha", Estado = EstadoTarea.Completada, FechaLimite = DateTime.UtcNow.AddDays(2) }
            }
        };
        context.Proyectos.Add(proyecto);
        await context.SaveChangesAsync();

        var service = new ProyectoService(new ProyectoRepository(context));
        await service.EliminarAsync(proyecto.Id);

        Assert.Empty(context.Proyectos);
    }
}