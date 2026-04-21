using DownLabs.Core.Api.Models;
using DownLabs.Core.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DownLabs.Core.Api.Endpoints;

public static class CotizacionDownlabsEndpoints
{
    private const string TableName = "cotizaciones_downlabs";
    private const string IdColumn = "id_cotizaciondwnlabs";

    public static void RegisterCotizacionDownlabsEndpoints(this WebApplication app)
    {
        app.MapGet("/api/cotizaciones", GetAllCotizacionesAsync);
        app.MapGet("/api/cotizaciones/{id}", GetCotizacionByIdAsync);
        app.MapPost("/api/cotizaciones", CreateCotizacionAsync);
        app.MapPut("/api/cotizaciones/{id}", UpdateCotizacionAsync);
        app.MapPatch("/api/cotizaciones/{id}", PartialUpdateCotizacionAsync);
        app.MapDelete("/api/cotizaciones/{id}", DeleteCotizacionAsync);
    }

    private static async Task<IResult> GetAllCotizacionesAsync(
        [FromServices] ICrudService crudService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? estado = null,
        [FromQuery] Guid? id_solicitud = null,
        [FromQuery] string? include = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(page, 1);

            var cotizaciones = await crudService.GetAllAsync<CotizacionDownlabs>(TableName, cancellationToken)
                .ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(estado))
            {
                cotizaciones = cotizaciones.Where(c => c.estado?.Equals(estado, StringComparison.OrdinalIgnoreCase) == true).ToList();
            }

            if (id_solicitud.HasValue)
            {
                cotizaciones = cotizaciones.Where(c => c.id_solicitud == id_solicitud).ToList();
            }

            var total = cotizaciones.Count;
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            var paginatedData = cotizaciones.Skip((page - 1) * pageSize).Take(pageSize).ToList();

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

    private static async Task<IResult> GetCotizacionByIdAsync(
        [FromServices] ICrudService crudService,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var cotizacion = await crudService.GetByIdAsync<CotizacionDownlabs>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (cotizacion is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Cotizacion no encontrada" });
            
            return Results.Ok(new { success = true, data = cotizacion });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> CreateCotizacionAsync(
        [FromServices] ICrudService crudService,
        [FromBody] CotizacionDownlabs cotizacion,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!cotizacion.id_solicitud.HasValue)
            {
                return Results.BadRequest(new { success = false, error = "ValidationError", message = "El ID de solicitud es requerido" });
            }

            if (string.IsNullOrWhiteSpace(cotizacion.estado))
            {
                cotizacion.estado = "Negociando";
            }

            cotizacion.id_cotizaciondwnlabs = Guid.NewGuid();
            cotizacion.created_at = DateTime.UtcNow;
            cotizacion.updated_at = DateTime.UtcNow;

            var created = await crudService.CreateAsync<CotizacionDownlabs>(TableName, cotizacion, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Created($"/api/cotizaciones/{created.id_cotizaciondwnlabs}", new { success = true, data = created });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> UpdateCotizacionAsync(
        [FromServices] ICrudService crudService,
        Guid id,
        [FromBody] CotizacionDownlabs cotizacion,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<CotizacionDownlabs>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Cotizacion no encontrada" });

            cotizacion.id_cotizaciondwnlabs = id;
            cotizacion.created_at = existing.created_at;
            cotizacion.updated_at = DateTime.UtcNow;

            var updated = await crudService.UpdateAsync<CotizacionDownlabs>(TableName, IdColumn, id, cotizacion, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Ok(new { success = true, data = updated });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> PartialUpdateCotizacionAsync(
        [FromServices] ICrudService crudService,
        Guid id,
        [FromBody] CotizacionDownlabs cotizacionUpdate,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<CotizacionDownlabs>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Cotizacion no encontrada" });

            if (cotizacionUpdate.costo_envio > 0)
                existing.costo_envio = cotizacionUpdate.costo_envio;
            if (!string.IsNullOrWhiteSpace(cotizacionUpdate.estado))
                existing.estado = cotizacionUpdate.estado;
            
            existing.updated_at = DateTime.UtcNow;

            var updated = await crudService.UpdateAsync<CotizacionDownlabs>(TableName, IdColumn, id, existing, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Ok(new { success = true, data = updated });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteCotizacionAsync(
        [FromServices] ICrudService crudService,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<CotizacionDownlabs>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Cotizacion no encontrada" });

            var deleted = await crudService.DeleteAsync(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (!deleted)
                return Results.BadRequest(new { success = false, error = "BadRequest", message = "Error al eliminar la cotizacion" });
            
            return Results.Ok(new { success = true, message = "Cotizacion eliminada correctamente" });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }
}