using System.Net;
using System.Net.Http.Json;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;

namespace TaskManager.Tests;

public class ProyectosIntegrationTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProyectosIntegrationTests()
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
    public async Task PostProyecto_DatosValidos_Retorna201Created()
    {
        var dto = new CrearProyectoDto(
            Nombre: "Proyecto Integración Alpha",
            Descripcion: "Prueba de creación de proyecto vía HTTP",
            FechaInicio: DateTime.UtcNow,
            FechaFin: DateTime.UtcNow.AddDays(15)
        );

        var response = await _client.PostAsJsonAsync("/api/v1/Proyectos", dto, CustomWebApplicationFactory.JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var creado = await response.Content.ReadFromJsonAsync<ProyectoResponseDto>(CustomWebApplicationFactory.JsonOptions);
        Assert.NotNull(creado);
        Assert.True(creado.Id > 0);
        Assert.Equal(dto.Nombre, creado.Nombre);
        Assert.Equal(0, creado.TotalTareas);
        Assert.Equal(0, creado.TareasCompletadas);
        Assert.Equal(0.0, creado.PorcentajeAvance);
    }

    [Fact]
    public async Task PostProyecto_FechaFinAnteriorAFechaInicio_Retorna400BadRequest()
    {
        var dto = new CrearProyectoDto(
            Nombre: "Proyecto Fechas Inválidas",
            Descripcion: "FechaFin menor que FechaInicio",
            FechaInicio: DateTime.UtcNow.AddDays(10),
            FechaFin: DateTime.UtcNow
        );

        var response = await _client.PostAsJsonAsync("/api/v1/Proyectos", dto, CustomWebApplicationFactory.JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProyecto_ConTareasPendientes_Retorna400BadRequest()
    {
        var proyecto = await CrearProyectoAsync("Proyecto Con Pendientes");
        await CrearTareaAsync(proyecto.Id, "Tarea Pendiente 1", PrioridadTarea.Alta, DateTime.UtcNow.AddDays(5));

        var deleteResponse = await _client.DeleteAsync($"/api/v1/Proyectos/{proyecto.Id}");

        Assert.Equal(HttpStatusCode.BadRequest, deleteResponse.StatusCode);

        var body = await deleteResponse.Content.ReadAsStringAsync();
        Assert.Contains("pendientes", body, StringComparison.OrdinalIgnoreCase);

        var getResponse = await _client.GetAsync($"/api/v1/Proyectos/{proyecto.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task PatchEstadoTarea_YGetProyecto_Calcula50PorCientoAvance()
    {
        var proyecto = await CrearProyectoAsync("Proyecto Avance 50");
        var tarea1 = await CrearTareaAsync(proyecto.Id, "Tarea 1", PrioridadTarea.Alta, DateTime.UtcNow.AddDays(3));
        await CrearTareaAsync(proyecto.Id, "Tarea 2", PrioridadTarea.Media, DateTime.UtcNow.AddDays(4));

        var patchResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/Tareas/{tarea1.Id}/estado",
            new CambiarEstadoTareaDto(EstadoTarea.Completada),
            CustomWebApplicationFactory.JsonOptions);

        Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/v1/Proyectos/{proyecto.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var detalle = await getResponse.Content.ReadFromJsonAsync<ProyectoResponseDto>(CustomWebApplicationFactory.JsonOptions);
        Assert.NotNull(detalle);
        Assert.Equal(2, detalle.TotalTareas);
        Assert.Equal(1, detalle.TareasCompletadas);
        Assert.Equal(50.0, detalle.PorcentajeAvance);
    }

    [Fact]
    public async Task DeleteProyecto_ConTodasLasTareasCompletadas_Retorna204NoContent()
    {
        var proyecto = await CrearProyectoAsync("Proyecto Completado");
        var tarea1 = await CrearTareaAsync(proyecto.Id, "Tarea Final 1", PrioridadTarea.Media, DateTime.UtcNow.AddDays(2));
        var tarea2 = await CrearTareaAsync(proyecto.Id, "Tarea Final 2", PrioridadTarea.Baja, DateTime.UtcNow.AddDays(3));

        var patch1 = await _client.PatchAsJsonAsync(
            $"/api/v1/Tareas/{tarea1.Id}/estado",
            new CambiarEstadoTareaDto(EstadoTarea.Completada),
            CustomWebApplicationFactory.JsonOptions);
        var patch2 = await _client.PatchAsJsonAsync(
            $"/api/v1/Tareas/{tarea2.Id}/estado",
            new CambiarEstadoTareaDto(EstadoTarea.Completada),
            CustomWebApplicationFactory.JsonOptions);

        Assert.Equal(HttpStatusCode.OK, patch1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, patch2.StatusCode);

        var deleteResponse = await _client.DeleteAsync($"/api/v1/Proyectos/{proyecto.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfterDelete = await _client.GetAsync($"/api/v1/Proyectos/{proyecto.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
    }

    [Fact]
    public async Task PutProyecto_FechaFinMenorAFechaLimiteDeTarea_Retorna400BadRequest()
    {
        var proyecto = await CrearProyectoAsync("Proyecto Con Tarea Larga");
        await CrearTareaAsync(proyecto.Id, "Tarea Larga", PrioridadTarea.Alta, DateTime.UtcNow.AddDays(15));

        var updateDto = new ActualizarProyectoDto(
            Nombre: proyecto.Nombre,
            Descripcion: proyecto.Descripcion,
            FechaInicio: proyecto.FechaInicio,
            FechaFin: DateTime.UtcNow.AddDays(5)
        );

        var response = await _client.PutAsJsonAsync($"/api/v1/Proyectos/{proyecto.Id}", updateDto, CustomWebApplicationFactory.JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetSwaggerJson_Retorna200YContieneSchemasAvanceProyectosYAvanceTareas()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"v1\"", json);
        Assert.Contains("TaskManager API", json);
        Assert.Contains("\"/api/v1/Proyectos\"", json);
        Assert.Contains("\"/api/v1/Tareas\"", json);
        Assert.Contains("\"AvanceProyectos\"", json);
        Assert.Contains("\"AvanceTareas\"", json);
    }

    private async Task<ProyectoResponseDto> CrearProyectoAsync(string nombre)
    {
        var dto = new CrearProyectoDto(
            Nombre: nombre,
            Descripcion: "Descripción de prueba",
            FechaInicio: DateTime.UtcNow.AddDays(-5),
            FechaFin: DateTime.UtcNow.AddDays(20)
        );

        var response = await _client.PostAsJsonAsync("/api/v1/Proyectos", dto, CustomWebApplicationFactory.JsonOptions);
        response.EnsureSuccessStatusCode();
        var creado = await response.Content.ReadFromJsonAsync<ProyectoResponseDto>(CustomWebApplicationFactory.JsonOptions);
        return creado!;
    }

    private async Task<TareaResponseDto> CrearTareaAsync(int proyectoId, string titulo, PrioridadTarea prioridad, DateTime fechaLimite)
    {
        var dto = new CrearTareaDto(
            Titulo: titulo,
            Descripcion: "Descripción tarea",
            Prioridad: prioridad,
            FechaLimite: fechaLimite,
            ProyectoId: proyectoId
        );

        var response = await _client.PostAsJsonAsync("/api/v1/Tareas", dto, CustomWebApplicationFactory.JsonOptions);
        response.EnsureSuccessStatusCode();
        var creada = await response.Content.ReadFromJsonAsync<TareaResponseDto>(CustomWebApplicationFactory.JsonOptions);
        return creada!;
    }
}
