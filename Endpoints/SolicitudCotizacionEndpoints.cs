using DownLabs.Core.Api.Models;
using DownLabs.Core.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DownLabs.Core.Api.Endpoints;

public static class SolicitudCotizacionEndpoints
{
    private const string TableName = "solicitudes_cotizacion";
    private const string IdColumn = "id_solicitud";

    public static void RegisterSolicitudCotizacionEndpoints(this WebApplication app)
    {
        app.MapGet("/api/solicitudes-cotizacion", GetAllSolicitudes);
        app.MapGet("/api/solicitudes-cotizacion/{id}", GetSolicitudById);
        app.MapPost("/api/solicitudes-cotizacion", CreateSolicitud);
        app.MapPut("/api/solicitudes-cotizacion/{id}", UpdateSolicitud);
        app.MapPatch("/api/solicitudes-cotizacion/{id}", PartialUpdateSolicitud);
        app.MapDelete("/api/solicitudes-cotizacion/{id}", DeleteSolicitud);
    }

    private static async Task<IResult> GetAllSolicitudes(
        [FromServices] ICrudService crudService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? estado = null,
        [FromQuery] string? nivel_urgencia = null,
        [FromQuery] Guid? id_cliente = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(page, 1);

            var solicitudes = await crudService.GetAllAsync<SolicitudCotizacion>(TableName, cancellationToken)
                .ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(search))
            {
                solicitudes = solicitudes.Where(s => 
                    (s.descripcion_articulo?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                solicitudes = solicitudes.Where(s => s.estado_solicitud?.Equals(estado, StringComparison.OrdinalIgnoreCase) == true).ToList();
            }

            if (!string.IsNullOrWhiteSpace(nivel_urgencia))
            {
                solicitudes = solicitudes.Where(s => s.nivel_urgencia?.Equals(nivel_urgencia, StringComparison.OrdinalIgnoreCase) == true).ToList();
            }

            if (id_cliente.HasValue)
            {
                solicitudes = solicitudes.Where(s => s.id_cliente == id_cliente).ToList();
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

    private static async Task<IResult> GetSolicitudById(
        [FromServices] ICrudService crudService,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var solicitud = await crudService.GetByIdAsync<SolicitudCotizacion>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (solicitud is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Solicitud no encontrada" });
            
            return Results.Ok(new { success = true, data = solicitud });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> CreateSolicitud(
        [FromServices] ICrudService crudService,
        [FromBody] SolicitudCotizacion solicitud,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(solicitud.descripcion_articulo))
            {
                return Results.BadRequest(new { success = false, error = "ValidationError", message = "La descripcion del articulo es requerida" });
            }

            if (!solicitud.id_cliente.HasValue)
            {
                return Results.BadRequest(new { success = false, error = "ValidationError", message = "El ID del cliente es requerido" });
            }

            if (string.IsNullOrWhiteSpace(solicitud.nivel_urgencia))
            {
                solicitud.nivel_urgencia = "Normal";
            }

            if (string.IsNullOrWhiteSpace(solicitud.estado_solicitud))
            {
                solicitud.estado_solicitud = "Pendiente";
            }

            solicitud.id_solicitud = Guid.NewGuid();
            solicitud.created_at = DateTime.UtcNow;
            solicitud.updated_at = DateTime.UtcNow;

            var created = await crudService.CreateAsync<SolicitudCotizacion>(TableName, solicitud, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Created($"/api/solicitudes-cotizacion/{created.id_solicitud}", new { success = true, data = created });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> UpdateSolicitud(
        [FromServices] ICrudService crudService,
        Guid id,
        [FromBody] SolicitudCotizacion solicitud,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<SolicitudCotizacion>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Solicitud no encontrada" });

            solicitud.id_solicitud = id;
            solicitud.created_at = existing.created_at;
            solicitud.updated_at = DateTime.UtcNow;

            var updated = await crudService.UpdateAsync<SolicitudCotizacion>(TableName, IdColumn, id, solicitud, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Ok(new { success = true, data = updated });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> PartialUpdateSolicitud(
        [FromServices] ICrudService crudService,
        Guid id,
        [FromBody] SolicitudCotizacion solicitudUpdate,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<SolicitudCotizacion>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Solicitud no encontrada" });

            if (solicitudUpdate.descripcion_articulo is not null)
                existing.descripcion_articulo = solicitudUpdate.descripcion_articulo;
            if (solicitudUpdate.especificaciones_requeridas is not null)
                existing.especificaciones_requeridas = solicitudUpdate.especificaciones_requeridas;
            if (solicitudUpdate.nivel_urgencia is not null)
                existing.nivel_urgencia = solicitudUpdate.nivel_urgencia;
            if (solicitudUpdate.estado_solicitud is not null)
                existing.estado_solicitud = solicitudUpdate.estado_solicitud;
            
            existing.updated_at = DateTime.UtcNow;

            var updated = await crudService.UpdateAsync<SolicitudCotizacion>(TableName, IdColumn, id, existing, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Ok(new { success = true, data = updated });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteSolicitud(
        [FromServices] ICrudService crudService,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<SolicitudCotizacion>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Solicitud no encontrada" });

            var deleted = await crudService.DeleteAsync(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (!deleted)
                return Results.BadRequest(new { success = false, error = "BadRequest", message = "Error al eliminar la solicitud" });
            
            return Results.Ok(new { success = true, message = "Solicitud eliminada correctamente" });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }
}