using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.DTOs;

public record CrearProyectoDto(
    [Required(ErrorMessage = "El nombre es obligatorio")] [MaxLength(120)] string Nombre,
    [MaxLength(500)] string? Descripcion,
    [Required] DateTime FechaInicio,
    [Required] DateTime FechaFin
);

public record ActualizarProyectoDto(
    [Required(ErrorMessage = "El nombre es obligatorio")] [MaxLength(120)] string Nombre,
    [MaxLength(500)] string? Descripcion,
    [Required] DateTime FechaInicio,
    [Required] DateTime FechaFin
);

public record ProyectoResponseDto(
    int Id,
    string Nombre,
    string? Descripcion,
    DateTime FechaInicio,
    DateTime FechaFin,
    int TotalTareas,
    int TareasCompletadas,
    double PorcentajeAvance
);