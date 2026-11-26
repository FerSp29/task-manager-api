using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class ProyectoRepository(AppDbContext context) : IProyectoRepository
{
    public async Task<IEnumerable<Proyecto>> GetAllAsync() =>
        await context.Proyectos.Include(p => p.Tareas).AsNoTracking().ToListAsync();

    public async Task<Proyecto?> GetByIdAsync(int id) =>
        await context.Proyectos.Include(p => p.Tareas).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Proyecto> AddAsync(Proyecto proyecto)
    {
        context.Proyectos.Add(proyecto);
        await context.SaveChangesAsync();
        return proyecto;
    }

    public async Task UpdateAsync(Proyecto proyecto)
    {
        context.Proyectos.Update(proyecto);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Proyecto proyecto)
    {
        context.Proyectos.Remove(proyecto);
        await context.SaveChangesAsync();
    }

    public async Task<bool> TieneTareasPendientesAsync(int proyectoId) =>
        await context.Tareas.AnyAsync(t => t.ProyectoId == proyectoId && t.Estado != EstadoTarea.Completada);
}