using System.Net;
using System.Net.Http.Json;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;

namespace TaskManager.Tests;

public class TareasIntegrationTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TareasIntegrationTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task PostTarea_DatosValidos_Retorna201Created()
    {
        var proyecto = await CrearProyectoAsync("Proyecto Tareas", DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(15));

        var dto = new CrearTareaDto(
            Titulo: "Implementar pruebas de integración",
            Descripcion: "Cobertura E2E con WebApplicationFactory",
            Prioridad: PrioridadTarea.Alta,
            FechaLimite: DateTime.UtcNow.AddDays(5),
            ProyectoId: proyecto.Id
        );

        var response = await _client.PostAsJsonAsync("/api/Tareas", dto, CustomWebApplicationFactory.JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var creada = await response.Content.ReadFromJsonAsync<TareaResponseDto>(CustomWebApplicationFactory.JsonOptions);
        Assert.NotNull(creada);
        Assert.True(creada.Id > 0);
        Assert.Equal(dto.Titulo, creada.Titulo);
        Assert.Equal(EstadoTarea.Pendiente, creada.Estado);
        Assert.Equal(PrioridadTarea.Alta, creada.Prioridad);
        Assert.Equal(proyecto.Id, creada.ProyectoId);
    }

    [Fact]
    public async Task PostTarea_FechaLimiteSuperiorAFechaFinProyecto_Retorna400BadRequest()
    {
        var proyecto = await CrearProyectoAsync("Proyecto Corto", DateTime.UtcNow, DateTime.UtcNow.AddDays(5));

        var dto = new CrearTareaDto(
            Titulo: "Tarea Fuera de Plazo",
            Descripcion: "Supera FechaFin del proyecto",
            Prioridad: PrioridadTarea.Media,
            FechaLimite: DateTime.UtcNow.AddDays(10),
            ProyectoId: proyecto.Id
        );

        var response = await _client.PostAsJsonAsync("/api/Tareas", dto, CustomWebApplicationFactory.JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTareasVencidas_Retorna200YSoloVencidasNoCompletadas()
    {
        var proyecto = await CrearProyectoAsync("Proyecto Vencimientos", DateTime.UtcNow.AddDays(-15), DateTime.UtcNow.AddDays(15));

        var vencidaPendiente = await CrearTareaAsync(proyecto.Id, "Vencida Pendiente", PrioridadTarea.Alta, DateTime.UtcNow.AddDays(-3));
        var vencidaEnProgreso = await CrearTareaAsync(proyecto.Id, "Vencida En Progreso", PrioridadTarea.Media, DateTime.UtcNow.AddDays(-1));
        var vencidaCompletada = await CrearTareaAsync(proyecto.Id, "Vencida Completada", PrioridadTarea.Baja, DateTime.UtcNow.AddDays(-2));
        await CrearTareaAsync(proyecto.Id, "Vigente Pendiente", PrioridadTarea.Alta, DateTime.UtcNow.AddDays(5));

        await _client.PatchAsJsonAsync(
            $"/api/Tareas/{vencidaEnProgreso.Id}/estado",
            new CambiarEstadoTareaDto(EstadoTarea.EnProgreso),
            CustomWebApplicationFactory.JsonOptions);

        await _client.PatchAsJsonAsync(
            $"/api/Tareas/{vencidaCompletada.Id}/estado",
            new CambiarEstadoTareaDto(EstadoTarea.Completada),
            CustomWebApplicationFactory.JsonOptions);

        var response = await _client.GetAsync("/api/Tareas/vencidas");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var vencidas = await response.Content.ReadFromJsonAsync<List<TareaResponseDto>>(CustomWebApplicationFactory.JsonOptions);
        Assert.NotNull(vencidas);
        Assert.Equal(2, vencidas.Count);
        Assert.Contains(vencidas, t => t.Id == vencidaPendiente.Id);
        Assert.Contains(vencidas, t => t.Id == vencidaEnProgreso.Id);
        Assert.DoesNotContain(vencidas, t => t.Id == vencidaCompletada.Id);
    }

    [Fact]
    public async Task GetTareas_ConFiltrosEstadoYPrioridad_Retorna200YTareasCoincidentes()
    {
        var proyecto = await CrearProyectoAsync("Proyecto Filtros", DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(20));

        var objetivo = await CrearTareaAsync(proyecto.Id, "Objetivo EnProgreso Alta", PrioridadTarea.Alta, DateTime.UtcNow.AddDays(4));
        await CrearTareaAsync(proyecto.Id, "Pendiente Alta", PrioridadTarea.Alta, DateTime.UtcNow.AddDays(4));
        var enProgresoBaja = await CrearTareaAsync(proyecto.Id, "EnProgreso Baja", PrioridadTarea.Baja, DateTime.UtcNow.AddDays(4));

        await _client.PatchAsJsonAsync(
            $"/api/Tareas/{objetivo.Id}/estado",
            new CambiarEstadoTareaDto(EstadoTarea.EnProgreso),
            CustomWebApplicationFactory.JsonOptions);

        await _client.PatchAsJsonAsync(
            $"/api/Tareas/{enProgresoBaja.Id}/estado",
            new CambiarEstadoTareaDto(EstadoTarea.EnProgreso),
            CustomWebApplicationFactory.JsonOptions);

        var responseCombinado = await _client.GetAsync("/api/Tareas?estado=EnProgreso&prioridad=Alta");
        Assert.Equal(HttpStatusCode.OK, responseCombinado.StatusCode);

        var filtradas = await responseCombinado.Content.ReadFromJsonAsync<List<TareaResponseDto>>(CustomWebApplicationFactory.JsonOptions);
        Assert.NotNull(filtradas);
        Assert.Single(filtradas);
        Assert.Equal(objetivo.Id, filtradas[0].Id);

        var responsePorEstado = await _client.GetAsync("/api/Tareas?estado=EnProgreso");
        Assert.Equal(HttpStatusCode.OK, responsePorEstado.StatusCode);

        var enProgreso = await responsePorEstado.Content.ReadFromJsonAsync<List<TareaResponseDto>>(CustomWebApplicationFactory.JsonOptions);
        Assert.NotNull(enProgreso);
        Assert.Equal(2, enProgreso.Count);
    }

    [Fact]
    public async Task PutTarea_FechaLimiteSuperiorAFechaFinProyecto_Retorna400BadRequest()
    {
        var proyecto = await CrearProyectoAsync("Proyecto Update Tarea", DateTime.UtcNow, DateTime.UtcNow.AddDays(10));
        var tarea = await CrearTareaAsync(proyecto.Id, "Tarea Inicial", PrioridadTarea.Media, DateTime.UtcNow.AddDays(5));

        var updateDto = new ActualizarTareaDto(
            Titulo: "Tarea Actualizada",
            Descripcion: "Fecha fuera de rango",
            Estado: EstadoTarea.EnProgreso,
            Prioridad: PrioridadTarea.Alta,
            FechaLimite: DateTime.UtcNow.AddDays(20)
        );

        var response = await _client.PutAsJsonAsync($"/api/Tareas/{tarea.Id}", updateDto, CustomWebApplicationFactory.JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<ProyectoResponseDto> CrearProyectoAsync(string nombre, DateTime inicio, DateTime fin)
    {
        var dto = new CrearProyectoDto(nombre, "Descripción", inicio, fin);
        var response = await _client.PostAsJsonAsync("/api/Proyectos", dto, CustomWebApplicationFactory.JsonOptions);
        response.EnsureSuccessStatusCode();
        var creado = await response.Content.ReadFromJsonAsync<ProyectoResponseDto>(CustomWebApplicationFactory.JsonOptions);
        return creado!;
    }

    private async Task<TareaResponseDto> CrearTareaAsync(int proyectoId, string titulo, PrioridadTarea prioridad, DateTime fechaLimite)
    {
        var dto = new CrearTareaDto(titulo, "Descripción", prioridad, fechaLimite, proyectoId);
        var response = await _client.PostAsJsonAsync("/api/Tareas", dto, CustomWebApplicationFactory.JsonOptions);
        response.EnsureSuccessStatusCode();
        var creada = await response.Content.ReadFromJsonAsync<TareaResponseDto>(CustomWebApplicationFactory.JsonOptions);
        return creada!;
    }
}
