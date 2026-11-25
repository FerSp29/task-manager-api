using System.ComponentModel.DataAnnotations;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

public record CrearTareaDto(
    [Required(ErrorMessage = "El título es obligatorio")] [MaxLength(150)] string Titulo,
    [MaxLength(500)] string? Descripcion,
    PrioridadTarea Prioridad,
    [Required] DateTime FechaLimite,
    [Required] int ProyectoId
);

public record ActualizarTareaDto(
    [Required(ErrorMessage = "El título es obligatorio")] [MaxLength(150)] string Titulo,
    [MaxLength(500)] string? Descripcion,
    EstadoTarea Estado,
    PrioridadTarea Prioridad,
    [Required] DateTime FechaLimite
);

public record CambiarEstadoTareaDto(
    [Required] EstadoTarea Estado
);

public record TareaResponseDto(
    int Id,
    string Titulo,
    string? Descripcion,
    EstadoTarea Estado,
    PrioridadTarea Prioridad,
    DateTime FechaLimite,
    int ProyectoId
);