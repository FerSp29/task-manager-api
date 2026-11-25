using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Application.Services;

public class TareaService(ITareaRepository tareaRepo, IProyectoRepository proyectoRepo) : ITareaService
{
    public async Task<IEnumerable<TareaResponseDto>> ObtenerTodasAsync(EstadoTarea? estado, PrioridadTarea? prioridad)
    {
        var tareas = await tareaRepo.GetAllAsync(estado, prioridad);
        return tareas.Select(MapToDto);
    }

    public async Task<IEnumerable<TareaResponseDto>> ObtenerPorProyectoAsync(int proyectoId)
    {
        _ = await proyectoRepo.GetByIdAsync(proyectoId)
            ?? throw new NotFoundException($"No se encontró el proyecto con ID {proyectoId}.");

        var tareas = await tareaRepo.GetByProyectoIdAsync(proyectoId);
        return tareas.Select(MapToDto);
    }

    public async Task<IEnumerable<TareaResponseDto>> ObtenerVencidasAsync()
    {
        var tareas = await tareaRepo.GetVencidasAsync();
        return tareas.Select(MapToDto);
    }

    public async Task<TareaResponseDto> ObtenerPorIdAsync(int id)
    {
        var tarea = await tareaRepo.GetByIdAsync(id)
            ?? throw new NotFoundException($"No se encontró la tarea con ID {id}.");
        return MapToDto(tarea);
    }

    public async Task<TareaResponseDto> CrearAsync(CrearTareaDto dto)
    {
        var proyecto = await proyectoRepo.GetByIdAsync(dto.ProyectoId)
            ?? throw new NotFoundException($"No se encontró el proyecto con ID {dto.ProyectoId}.");

        var fechaLimiteUtc = DateTime.SpecifyKind(dto.FechaLimite, DateTimeKind.Utc);
        if (fechaLimiteUtc > proyecto.FechaFin)
            throw new BusinessRuleException("La fecha límite de la tarea no puede superar la fecha de finalización del proyecto.");

        var tarea = new Tarea
        {
            Titulo = dto.Titulo,
            Descripcion = dto.Descripcion,
            Estado = EstadoTarea.Pendiente,
            Prioridad = dto.Prioridad,
            FechaLimite = fechaLimiteUtc,
            ProyectoId = dto.ProyectoId
        };

        var creada = await tareaRepo.AddAsync(tarea);
        return MapToDto(creada);
    }

    public async Task<TareaResponseDto> ActualizarAsync(int id, ActualizarTareaDto dto)
    {
        var tarea = await tareaRepo.GetByIdAsync(id)
            ?? throw new NotFoundException($"No se encontró la tarea con ID {id}.");

        var proyecto = await proyectoRepo.GetByIdAsync(tarea.ProyectoId)
            ?? throw new NotFoundException($"No se encontró el proyecto con ID {tarea.ProyectoId}.");

        var fechaLimiteUtc = DateTime.SpecifyKind(dto.FechaLimite, DateTimeKind.Utc);
        if (fechaLimiteUtc > proyecto.FechaFin)
            throw new BusinessRuleException("La fecha límite de la tarea no puede superar la fecha de finalización del proyecto.");

        tarea.Titulo = dto.Titulo;
        tarea.Descripcion = dto.Descripcion;
        tarea.Estado = dto.Estado;
        tarea.Prioridad = dto.Prioridad;
        tarea.FechaLimite = fechaLimiteUtc;

        await tareaRepo.UpdateAsync(tarea);
        return MapToDto(tarea);
    }

    public async Task<TareaResponseDto> CambiarEstadoAsync(int id, CambiarEstadoTareaDto dto)
    {
        var tarea = await tareaRepo.GetByIdAsync(id)
            ?? throw new NotFoundException($"No se encontró la tarea con ID {id}.");

        tarea.Estado = dto.Estado;
        await tareaRepo.UpdateAsync(tarea);
        return MapToDto(tarea);
    }

    public async Task EliminarAsync(int id)
    {
        var tarea = await tareaRepo.GetByIdAsync(id)
            ?? throw new NotFoundException($"No se encontró la tarea con ID {id}.");

        await tareaRepo.DeleteAsync(tarea);
    }

    private static TareaResponseDto MapToDto(Tarea t) => new(
        t.Id,
        t.Titulo,
        t.Descripcion,
        t.Estado,
        t.Prioridad,
        t.FechaLimite,
        t.ProyectoId
    );
}