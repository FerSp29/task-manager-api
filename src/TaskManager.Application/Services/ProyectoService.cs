using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Application.Services;

public class ProyectoService(IProyectoRepository proyectoRepo) : IProyectoService
{
    public async Task<IEnumerable<ProyectoResponseDto>> ObtenerTodosAsync()
    {
        var proyectos = await proyectoRepo.GetAllAsync();
        return proyectos.Select(MapToDto);
    }

    public async Task<ProyectoResponseDto> ObtenerPorIdAsync(int id)
    {
        var proyecto = await proyectoRepo.GetByIdAsync(id)
            ?? throw new NotFoundException($"No se encontró el proyecto con ID {id}.");
        return MapToDto(proyecto);
    }

    public async Task<ProyectoResponseDto> CrearAsync(CrearProyectoDto dto)
    {
        if (dto.FechaFin < dto.FechaInicio)
            throw new BusinessRuleException("La fecha de finalización no puede ser anterior a la fecha de inicio.");

        var proyecto = new Proyecto
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            FechaInicio = DateTime.SpecifyKind(dto.FechaInicio, DateTimeKind.Utc),
            FechaFin = DateTime.SpecifyKind(dto.FechaFin, DateTimeKind.Utc)
        };

        var creado = await proyectoRepo.AddAsync(proyecto);
        return MapToDto(creado);
    }

    public async Task<ProyectoResponseDto> ActualizarAsync(int id, ActualizarProyectoDto dto)
    {
        var proyecto = await proyectoRepo.GetByIdAsync(id)
            ?? throw new NotFoundException($"No se encontró el proyecto con ID {id}.");

        if (dto.FechaFin < dto.FechaInicio)
            throw new BusinessRuleException("La fecha de finalización no puede ser anterior a la fecha de inicio.");

        var fechaFinUtc = DateTime.SpecifyKind(dto.FechaFin, DateTimeKind.Utc);
        if (proyecto.Tareas.Any(t => t.FechaLimite > fechaFinUtc))
            throw new BusinessRuleException("La fecha de finalización del proyecto no puede ser anterior a la fecha límite de sus tareas existentes.");

        proyecto.Nombre = dto.Nombre;
        proyecto.Descripcion = dto.Descripcion;
        proyecto.FechaInicio = DateTime.SpecifyKind(dto.FechaInicio, DateTimeKind.Utc);
        proyecto.FechaFin = fechaFinUtc;

        await proyectoRepo.UpdateAsync(proyecto);
        return MapToDto(proyecto);
    }

    public async Task EliminarAsync(int id)
    {
        var proyecto = await proyectoRepo.GetByIdAsync(id)
            ?? throw new NotFoundException($"No se encontró el proyecto con ID {id}.");

        if (await proyectoRepo.TieneTareasPendientesAsync(id))
            throw new BusinessRuleException("No se puede eliminar un proyecto que aún tiene tareas pendientes o en progreso.");

        await proyectoRepo.DeleteAsync(proyecto);
    }

    private static ProyectoResponseDto MapToDto(Proyecto p)
    {
        int total = p.Tareas.Count;
        int completadas = p.Tareas.Count(t => t.Estado == EstadoTarea.Completada);
        double avance = total == 0 ? 0 : Math.Round((double)completadas / total * 100, 2);

        return new ProyectoResponseDto(
            p.Id,
            p.Nombre,
            p.Descripcion,
            p.FechaInicio,
            p.FechaFin,
            total,
            completadas,
            avance
        );
    }
}