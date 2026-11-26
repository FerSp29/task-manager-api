using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Interfaces;

public interface IProyectoService
{
    Task<IEnumerable<ProyectoResponseDto>> ObtenerTodosAsync();
    Task<ProyectoResponseDto> ObtenerPorIdAsync(int id);
    Task<ProyectoResponseDto> CrearAsync(CrearProyectoDto dto);
    Task<ProyectoResponseDto> ActualizarAsync(int id, ActualizarProyectoDto dto);
    Task EliminarAsync(int id);
}

public interface ITareaService
{
    Task<IEnumerable<TareaResponseDto>> ObtenerTodasAsync(EstadoTarea? estado, PrioridadTarea? prioridad);
    Task<IEnumerable<TareaResponseDto>> ObtenerPorProyectoAsync(int proyectoId);
    Task<IEnumerable<TareaResponseDto>> ObtenerVencidasAsync();
    Task<TareaResponseDto> ObtenerPorIdAsync(int id);
    Task<TareaResponseDto> CrearAsync(CrearTareaDto dto);
    Task<TareaResponseDto> ActualizarAsync(int id, ActualizarTareaDto dto);
    Task<TareaResponseDto> CambiarEstadoAsync(int id, CambiarEstadoTareaDto dto);
    Task EliminarAsync(int id);
}