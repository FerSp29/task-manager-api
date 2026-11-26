using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Interfaces;

public interface IProyectoRepository
{
    Task<IEnumerable<Proyecto>> GetAllAsync();
    Task<Proyecto?> GetByIdAsync(int id);
    Task<Proyecto> AddAsync(Proyecto proyecto);
    Task UpdateAsync(Proyecto proyecto);
    Task DeleteAsync(Proyecto proyecto);
    Task<bool> TieneTareasPendientesAsync(int proyectoId);
}

public interface ITareaRepository
{
    Task<IEnumerable<Tarea>> GetAllAsync(EstadoTarea? estado, PrioridadTarea? prioridad);
    Task<IEnumerable<Tarea>> GetByProyectoIdAsync(int proyectoId);
    Task<IEnumerable<Tarea>> GetVencidasAsync();
    Task<Tarea?> GetByIdAsync(int id);
    Task<Tarea> AddAsync(Tarea tarea);
    Task UpdateAsync(Tarea tarea);
    Task DeleteAsync(Tarea tarea);
}