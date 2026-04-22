using DownLabs.Core.Api.Models;
using DownLabs.Core.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DownLabs.Core.Api.Endpoints;

public static class SolicitudMayoristaEndpoints
{
    private const string TableName = "solicitudes_mayorista";
    private const string IdColumn = "id_solicitud_mayorista";

    public static void RegisterSolicitudMayoristaEndpoints(this WebApplication app)
    {
        app.MapGet("/api/solicitudes-mayorista", GetAllSolicitudesMayoristaAsync);
        app.MapGet("/api/solicitudes-mayorista/{id}", GetSolicitudMayoristaByIdAsync);
        app.MapPost("/api/solicitudes-mayorista", CreateSolicitudMayoristaAsync);
        app.MapPut("/api/solicitudes-mayorista/{id}", UpdateSolicitudMayoristaAsync);
        app.MapPatch("/api/solicitudes-mayorista/{id}", PartialUpdateSolicitudMayoristaAsync);
        app.MapDelete("/api/solicitudes-mayorista/{id}", DeleteSolicitudMayoristaAsync);
    }

    private static async Task<IResult> GetAllSolicitudesMayoristaAsync(
        [FromServices] ICrudService crudService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? estado = null,
        [FromQuery] Guid? id_solicitud = null,
        [FromQuery] Guid? id_mayorista = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(page, 1);

            var solicitudes = await crudService.GetAllAsync<SolicitudMayorista>(TableName, cancellationToken)
                .ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(estado))
            {
                var estados = estado.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(e => e.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                solicitudes = solicitudes.Where(s => s.estado is not null && estados.Contains(s.estado)).ToList();
            }

            if (id_solicitud.HasValue)
            {
                solicitudes = solicitudes.Where(s => s.id_solicitud == id_solicitud).ToList();
            }

            if (id_mayorista.HasValue)
            {
                solicitudes = solicitudes.Where(s => s.id_mayorista == id_mayorista).ToList();
            }

            var total = solicitudes.Count;
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            var paginatedData = solicitudes.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Results.Ok(new 
            { 
                success = true, 
                data = paginatedData,
                pagination = new
                {
                    page,
                    pageSize,
                    total,
                    totalPages,
                    hasNext = page < totalPages,
                    hasPrev = page > 1
                }
            });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error interno: {ex.Message}", statusCode: 500);
        }
    }

    private static async Task<IResult> GetSolicitudMayoristaByIdAsync(
        [FromServices] ICrudService crudService,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var solicitud = await crudService.GetByIdAsync<SolicitudMayorista>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (solicitud is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Solicitud a mayorista no encontrada" });
            
            return Results.Ok(new { success = true, data = solicitud });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> CreateSolicitudMayoristaAsync(
        [FromServices] ICrudService crudService,
        [FromBody] SolicitudMayorista solicitud,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!solicitud.id_solicitud.HasValue)
            {
                return Results.BadRequest(new { success = false, error = "ValidationError", message = "El ID de solicitud es requerido" });
            }

            if (!solicitud.id_mayorista.HasValue)
            {
                return Results.BadRequest(new { success = false, error = "ValidationError", message = "El ID de mayorista es requerido" });
            }

            if (string.IsNullOrWhiteSpace(solicitud.estado))
            {
                solicitud.estado = "pendiente";
            }

            solicitud.id_solicitud_mayorista = Guid.NewGuid();
            solicitud.created_at = DateTime.UtcNow;

            var created = await crudService.CreateAsync<SolicitudMayorista>(TableName, solicitud, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Created($"/api/solicitudes-mayorista/{created.id_solicitud_mayorista}", new { success = true, data = created });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> UpdateSolicitudMayoristaAsync(
        [FromServices] ICrudService crudService,
        Guid id,
        [FromBody] SolicitudMayorista solicitud,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<SolicitudMayorista>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Solicitud a mayorista no encontrada" });

            solicitud.id_solicitud_mayorista = id;
            solicitud.created_at = existing.created_at;
            solicitud.updated_at = DateTime.UtcNow;

            var updated = await crudService.UpdateAsync<SolicitudMayorista>(TableName, IdColumn, id, solicitud, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Ok(new { success = true, data = updated });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> PartialUpdateSolicitudMayoristaAsync(
        [FromServices] ICrudService crudService,
        Guid id,
        [FromBody] SolicitudMayorista solicitudUpdate,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<SolicitudMayorista>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (solicitudUpdate.costo_envio.HasValue) 
                existing.costo_envio = solicitudUpdate.costo_envio;
            if (solicitudUpdate.tiempo_entrega_dias.HasValue)
                existing.tiempo_entrega_dias = solicitudUpdate.tiempo_entrega_dias;
            if (!string.IsNullOrWhiteSpace(solicitudUpdate.notas_mayorista))
                existing.notas_mayorista = solicitudUpdate.notas_mayorista;
            

            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Solicitud a mayorista no encontrada" });

            if (solicitudUpdate.productos is not null)
                existing.productos = solicitudUpdate.productos;
            if (!string.IsNullOrWhiteSpace(solicitudUpdate.estado))
                existing.estado = solicitudUpdate.estado;
            if (solicitudUpdate.respondido_at.HasValue)
                existing.respondido_at = solicitudUpdate.respondido_at;
            
            existing.updated_at = DateTime.UtcNow;

            var updated = await crudService.UpdateAsync<SolicitudMayorista>(TableName, IdColumn, id, existing, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Ok(new { success = true, data = updated });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteSolicitudMayoristaAsync(
        [FromServices] ICrudService crudService,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<SolicitudMayorista>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Solicitud a mayorista no encontrada" });

            var deleted = await crudService.DeleteAsync(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (!deleted)
                return Results.BadRequest(new { success = false, error = "BadRequest", message = "Error al eliminar la solicitud" });
            
            return Results.Ok(new { success = true, message = "Solicitud a mayorista eliminada correctamente" });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }
}