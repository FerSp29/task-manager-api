using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using TaskManager.Api.Controllers;
using TaskManager.Api.Middlewares;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Api.UnitTest;

public class ControllersUnitTests
{
    [Fact]
    public async Task ProyectosController_GetById_RetornaOkConProyectoResponseDto()
    {
        var esperado = new ProyectoResponseDto(1, "Demo", null, DateTime.UtcNow, DateTime.UtcNow.AddDays(5), 2, 1, 50.0);
        var controller = new ProyectosController(new StubProyectoService(esperado), new StubTareaService());

        var result = await controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<ProyectoResponseDto>(ok.Value);
        Assert.Equal(50.0, dto.PorcentajeAvance);
    }

    [Fact]
    public async Task TareasController_CambiarEstado_RetornaOkConEstadoActualizado()
    {
        var controller = new TareasController(new StubTareaService());

        var result = await controller.CambiarEstado(5, new CambiarEstadoTareaDto(EstadoTarea.Completada));

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<TareaResponseDto>(ok.Value);
        Assert.Equal(EstadoTarea.Completada, dto.Estado);
    }

    [Fact]
    public async Task ExceptionMiddleware_BusinessRuleException_Retorna400Json()
    {
        var middleware = new ExceptionMiddleware(
            _ => throw new BusinessRuleException("Regla de negocio"),
            NullLogger<ExceptionMiddleware>.Instance);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal(400, doc.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Regla de negocio", doc.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task ExceptionMiddleware_NotFoundException_Retorna404Json()
    {
        var middleware = new ExceptionMiddleware(
            _ => throw new NotFoundException("No existe"),
            NullLogger<ExceptionMiddleware>.Instance);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
    }

    private sealed class StubProyectoService(ProyectoResponseDto item) : IProyectoService
    {
        public Task<IEnumerable<ProyectoResponseDto>> ObtenerTodosAsync() => Task.FromResult<IEnumerable<ProyectoResponseDto>>([item]);
        public Task<ProyectoResponseDto> ObtenerPorIdAsync(int id) => Task.FromResult(item);
        public Task<ProyectoResponseDto> CrearAsync(CrearProyectoDto dto) => Task.FromResult(item);
        public Task<ProyectoResponseDto> ActualizarAsync(int id, ActualizarProyectoDto dto) => Task.FromResult(item);
        public Task EliminarAsync(int id) => Task.CompletedTask;
    }

    private sealed class StubTareaService : ITareaService
    {
        private static readonly TareaResponseDto Sample = new(5, "T", null, EstadoTarea.Completada, PrioridadTarea.Media, DateTime.UtcNow, 1);

        public Task<IEnumerable<TareaResponseDto>> ObtenerTodasAsync(EstadoTarea? estado, PrioridadTarea? prioridad) => Task.FromResult<IEnumerable<TareaResponseDto>>([Sample]);
        public Task<IEnumerable<TareaResponseDto>> ObtenerPorProyectoAsync(int proyectoId) => Task.FromResult<IEnumerable<TareaResponseDto>>([Sample]);
        public Task<IEnumerable<TareaResponseDto>> ObtenerVencidasAsync() => Task.FromResult<IEnumerable<TareaResponseDto>>([Sample]);
        public Task<TareaResponseDto> ObtenerPorIdAsync(int id) => Task.FromResult(Sample);
        public Task<TareaResponseDto> CrearAsync(CrearTareaDto dto) => Task.FromResult(Sample);
        public Task<TareaResponseDto> ActualizarAsync(int id, ActualizarTareaDto dto) => Task.FromResult(Sample);
        public Task<TareaResponseDto> CambiarEstadoAsync(int id, CambiarEstadoTareaDto dto) => Task.FromResult(Sample with { Estado = dto.Estado });
        public Task EliminarAsync(int id) => Task.CompletedTask;
    }
}
