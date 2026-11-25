using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProyectosController(IProyectoService proyectoService, ITareaService tareaService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await proyectoService.ObtenerTodosAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await proyectoService.ObtenerPorIdAsync(id));

    [HttpGet("{id:int}/tareas")]
    public async Task<IActionResult> GetTareasByProyecto(int id) =>
        Ok(await tareaService.ObtenerPorProyectoAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearProyectoDto dto)
    {
        var creado = await proyectoService.CrearAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ActualizarProyectoDto dto) =>
        Ok(await proyectoService.ActualizarAsync(id, dto));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await proyectoService.EliminarAsync(id);
        return NoContent();
    }
}