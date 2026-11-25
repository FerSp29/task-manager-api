using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Enums;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TareasController(ITareaService tareaService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EstadoTarea? estado, [FromQuery] PrioridadTarea? prioridad) =>
        Ok(await tareaService.ObtenerTodasAsync(estado, prioridad));

    [HttpGet("vencidas")]
    public async Task<IActionResult> GetVencidas() =>
        Ok(await tareaService.ObtenerVencidasAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await tareaService.ObtenerPorIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearTareaDto dto)
    {
        var creada = await tareaService.CrearAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = creada.Id }, creada);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ActualizarTareaDto dto) =>
        Ok(await tareaService.ActualizarAsync(id, dto));

    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoTareaDto dto) =>
        Ok(await tareaService.CambiarEstadoAsync(id, dto));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await tareaService.EliminarAsync(id);
        return NoContent();
    }
}