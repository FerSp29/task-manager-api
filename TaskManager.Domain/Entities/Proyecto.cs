namespace TaskManager.Domain.Entities;

public class Proyecto
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }

    public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
}