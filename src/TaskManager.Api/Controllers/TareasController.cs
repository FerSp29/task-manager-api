using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Enums;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TareasController(ITareaService tareaService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TareaResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] EstadoTarea? estado, [FromQuery] PrioridadTarea? prioridad) =>
        Ok(await tareaService.ObtenerTodasAsync(estado, prioridad));

    [HttpGet("vencidas")]
    [ProducesResponseType(typeof(IEnumerable<TareaResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVencidas() =>
        Ok(await tareaService.ObtenerVencidasAsync());

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TareaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await tareaService.ObtenerPorIdAsync(id));

    [HttpPost]
    [ProducesResponseType(typeof(TareaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CrearTareaDto dto)
    {
        var creada = await tareaService.CrearAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = creada.Id }, creada);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TareaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] ActualizarTareaDto dto) =>
        Ok(await tareaService.ActualizarAsync(id, dto));

    [HttpPatch("{id:int}/estado")]
    [ProducesResponseType(typeof(TareaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoTareaDto dto) =>
        Ok(await tareaService.CambiarEstadoAsync(id, dto));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await tareaService.EliminarAsync(id);
        return NoContent();
    }
}