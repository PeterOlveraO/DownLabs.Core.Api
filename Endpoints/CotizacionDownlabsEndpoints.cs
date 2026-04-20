using DownLabs.Core.Api.Models;
using DownLabs.Core.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DownLabs.Core.Api.Endpoints;

public static class CotizacionDownlabsEndpoints
{
    private const string TableName = "cotizaciones_downlabs";
    private const string IdColumn = "id_cotizacion";

    public static void RegisterCotizacionDownlabsEndpoints(this WebApplication app)
    {
        app.MapGet("/api/cotizaciones", GetAllCotizaciones);
        app.MapGet("/api/cotizaciones/{id}", GetCotizacionById);
        app.MapPost("/api/cotizaciones", CreateCotizacion);
        app.MapPut("/api/cotizaciones/{id}", UpdateCotizacion);
        app.MapPatch("/api/cotizaciones/{id}", PartialUpdateCotizacion);
        app.MapDelete("/api/cotizaciones/{id}", DeleteCotizacion);
    }

    private static async Task<IResult> GetAllCotizaciones(
        [FromServices] ICrudService crudService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? estado = null,
        [FromQuery] Guid? id_solicitud = null,
        [FromQuery] Guid? id_producto = null,
        [FromQuery] string? id_productos = null,
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

            if (id_producto.HasValue)
            {
                cotizaciones = cotizaciones.Where(c => c.id_producto == id_producto).ToList();
            }

            if (!string.IsNullOrWhiteSpace(id_productos))
            {
                var productoIds = id_productos.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => Guid.TryParse(s.Trim(), out var g) ? g : Guid.Empty)
                    .Where(g => g != Guid.Empty)
                    .ToHashSet();
                cotizaciones = cotizaciones.Where(c => c.id_producto.HasValue && productoIds.Contains(c.id_producto.Value)).ToList();
            }

            var total = cotizaciones.Count;
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            var paginatedData = cotizaciones.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var resultData = await ApplyIncludesAsync(crudService, paginatedData, include, cancellationToken).ConfigureAwait(false);

            return Results.Ok(new 
            { 
                success = true, 
                data = resultData,
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

    private static async Task<List<Dictionary<string, object?>>> ApplyIncludesAsync(
        ICrudService crudService,
        List<CotizacionDownlabs> cotizaciones,
        string? include,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(include))
        {
            return cotizaciones.Select(c => new Dictionary<string, object?>
            {
                ["id_cotizacion"] = c.id_cotizacion,
                ["id_solicitud"] = c.id_solicitud,
                ["id_producto"] = c.id_producto,
                ["precio_final_cliente"] = c.precio_final_cliente,
                ["costo_envio"] = c.costo_envio,
                ["estado"] = c.estado,
                ["created_at"] = c.created_at,
                ["updated_at"] = c.updated_at,
                ["id_operador"] = c.id_operador
            }).ToList();
        }

        var includeList = include.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(i => i.Trim().ToLowerInvariant())
            .ToHashSet();

        var result = new List<Dictionary<string, object?>>();

        var productoIds = cotizaciones.Where(c => c.id_producto.HasValue && includeList.Contains("producto"))
            .Select(c => c.id_producto!.Value)
            .ToList();
        var solicitudIds = cotizaciones.Where(c => c.id_solicitud.HasValue && includeList.Contains("solicitud"))
            .Select(c => c.id_solicitud!.Value)
            .ToList();

        Dictionary<Guid, CatalogoProducto?> productos = new();
        Dictionary<Guid, SolicitudCotizacion?> solicitudes = new();

        if (productoIds.Count > 0)
        {
            var allProductos = await crudService.GetAllAsync<CatalogoProducto>("catalogo_productos", cancellationToken).ConfigureAwait(false);
            productos = allProductos.ToDictionary(p => p.id_producto, p => (CatalogoProducto?)p);
        }

        if (solicitudIds.Count > 0)
        {
            var allSolicitudes = await crudService.GetAllAsync<SolicitudCotizacion>("solicitudes_cotizacion", cancellationToken).ConfigureAwait(false);
            foreach (var s in allSolicitudes.Where(s => solicitudIds.Contains(s.id_solicitud)))
            {
                solicitudes[s.id_solicitud] = s;
            }
            if (includeList.Contains("cliente"))
            {
                var clienteIds = solicitudes.Values.Where(s => s?.id_cliente.HasValue == true)
                    .Select(s => s!.id_cliente!.Value)
                    .ToList();
                if (clienteIds.Count > 0)
                {
                    var allClientes = await crudService.GetAllAsync<Cliente>("clientes", cancellationToken).ConfigureAwait(false);
                    var clientesDict = allClientes.ToDictionary(c => c.id_cliente, c => c);
                    foreach (var cotizacion in cotizaciones)
                    {
                        var sol = cotizacion.id_solicitud.HasValue ? solicitudes.GetValueOrDefault(cotizacion.id_solicitud.Value) : null;
                        if (sol?.id_cliente.HasValue == true && clientesDict.TryGetValue(sol.id_cliente.Value, out var cliente))
                        {
                            result.Add(new Dictionary<string, object?>
                            {
                                ["id_cotizacion"] = cotizacion.id_cotizacion,
                                ["id_solicitud"] = cotizacion.id_solicitud,
                                ["id_producto"] = cotizacion.id_producto,
                                ["precio_final_cliente"] = cotizacion.precio_final_cliente,
                                ["costo_envio"] = cotizacion.costo_envio,
                                ["estado"] = cotizacion.estado,
                                ["created_at"] = cotizacion.created_at,
                                ["updated_at"] = cotizacion.updated_at,
                                ["id_operador"] = cotizacion.id_operador,
                                ["producto"] = includeList.Contains("producto") && cotizacion.id_producto.HasValue ? productos.GetValueOrDefault(cotizacion.id_producto.Value) : null,
                                ["solicitud"] = includeList.Contains("solicitud") ? sol : null,
                                ["cliente"] = cliente
                            });
                            continue;
                        }
                        result.Add(new Dictionary<string, object?>
                        {
                            ["id_cotizacion"] = cotizacion.id_cotizacion,
                            ["id_solicitud"] = cotizacion.id_solicitud,
                            ["id_producto"] = cotizacion.id_producto,
                            ["precio_final_cliente"] = cotizacion.precio_final_cliente,
                            ["costo_envio"] = cotizacion.costo_envio,
                            ["estado"] = cotizacion.estado,
                            ["created_at"] = cotizacion.created_at,
                            ["updated_at"] = cotizacion.updated_at,
                            ["id_operador"] = cotizacion.id_operador,
                            ["producto"] = includeList.Contains("producto") && cotizacion.id_producto.HasValue ? productos.GetValueOrDefault(cotizacion.id_producto.Value) : null,
                            ["solicitud"] = includeList.Contains("solicitud") ? sol : null
                        });
                    }
                    return result;
                }
            }
        }

        foreach (var cotizacion in cotizaciones)
        {
            var dict = new Dictionary<string, object?>
            {
                ["id_cotizacion"] = cotizacion.id_cotizacion,
                ["id_solicitud"] = cotizacion.id_solicitud,
                ["id_producto"] = cotizacion.id_producto,
                ["precio_final_cliente"] = cotizacion.precio_final_cliente,
                ["costo_envio"] = cotizacion.costo_envio,
                ["estado"] = cotizacion.estado,
                ["created_at"] = cotizacion.created_at,
                ["updated_at"] = cotizacion.updated_at,
                ["id_operador"] = cotizacion.id_operador
            };

            if (includeList.Contains("producto") && cotizacion.id_producto.HasValue)
            {
                dict["producto"] = productos.GetValueOrDefault(cotizacion.id_producto.Value);
            }

            if (includeList.Contains("solicitud") && cotizacion.id_solicitud.HasValue)
            {
                dict["solicitud"] = solicitudes.GetValueOrDefault(cotizacion.id_solicitud.Value);
            }

            result.Add(dict);
        }

        return result;
    }

    private static async Task<IResult> GetCotizacionById(
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

    private static async Task<IResult> CreateCotizacion(
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

            if (cotizacion.id_producto == Guid.Empty)
            {
                return Results.BadRequest(new { success = false, error = "ValidationError", message = "El ID de producto es requerido" });
            }

            if (cotizacion.precio_final_cliente <= 0)
            {
                return Results.BadRequest(new { success = false, error = "ValidationError", message = "El precio final es requerido" });
            }

            if (string.IsNullOrWhiteSpace(cotizacion.estado))
            {
                cotizacion.estado = "Negociando";
            }

            cotizacion.id_cotizacion = Guid.NewGuid();
            cotizacion.created_at = DateTime.UtcNow;
            cotizacion.updated_at = DateTime.UtcNow;

            var created = await crudService.CreateAsync<CotizacionDownlabs>(TableName, cotizacion, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Created($"/api/cotizaciones/{created.id_cotizacion}", new { success = true, data = created });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> UpdateCotizacion(
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

            cotizacion.id_cotizacion = id;
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

    private static async Task<IResult> PartialUpdateCotizacion(
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

            if (cotizacionUpdate.precio_final_cliente > 0)
                existing.precio_final_cliente = cotizacionUpdate.precio_final_cliente;
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

    private static async Task<IResult> DeleteCotizacion(
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