using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Domain.UnitTest;

public class DomainEntitiesTests
{
    [Fact]
    public void Proyecto_ValoresPorDefecto_InicializaNombreYColeccionTareas()
    {
        var proyecto = new Proyecto();

        Assert.Equal(string.Empty, proyecto.Nombre);
        Assert.NotNull(proyecto.Tareas);
        Assert.Empty(proyecto.Tareas);
    }

    [Fact]
    public void Tarea_ValoresPorDefecto_EstadoPendienteYPrioridadMedia()
    {
        var tarea = new Tarea();

        Assert.Equal(string.Empty, tarea.Titulo);
        Assert.Equal(EstadoTarea.Pendiente, tarea.Estado);
        Assert.Equal(PrioridadTarea.Media, tarea.Prioridad);
    }

    [Fact]
    public void ExcepcionesDeDominio_ConservanMensajeOriginal()
    {
        var ruleEx = new BusinessRuleException("Regla violada");
        var notFoundEx = new NotFoundException("Recurso no encontrado");

        Assert.Equal("Regla violada", ruleEx.Message);
        Assert.Equal("Recurso no encontrado", notFoundEx.Message);
    }
}
