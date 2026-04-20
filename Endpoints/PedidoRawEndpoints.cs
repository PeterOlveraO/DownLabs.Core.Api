using DownLabs.Core.Api.Models;
using DownLabs.Core.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DownLabs.Core.Api.Endpoints;

public static class PedidoRawEndpoints
{
    private const string TableName = "pedidos_raw";
    private const string IdColumn = "id_pedido";

    public static void RegisterPedidoRawEndpoints(this WebApplication app)
    {
        app.MapGet("/api/pedidos-raw", GetAllPedidosRawAsync);
        app.MapGet("/api/pedidos-raw/{id}", GetPedidoRawByIdAsync);
        app.MapPost("/api/pedidos-raw", CreatePedidoRawAsync);
        app.MapPut("/api/pedidos-raw/{id}", UpdatePedidoRawAsync);
        app.MapPatch("/api/pedidos-raw/{id}", PartialUpdatePedidoRawAsync);
        app.MapDelete("/api/pedidos-raw/{id}", DeletePedidoRawAsync);
    }

    private static async Task<IResult> GetAllPedidosRawAsync(
        [FromServices] ICrudService crudService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? estado = null,
        [FromQuery] Guid? id_cliente = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(page, 1);

            var pedidos = await crudService.GetAllAsync<PedidoRaw>(TableName, cancellationToken)
                .ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(estado))
            {
                pedidos = pedidos.Where(p => p.estado?.Equals(estado, StringComparison.OrdinalIgnoreCase) == true).ToList();
            }

            if (id_cliente.HasValue)
            {
                pedidos = pedidos.Where(p => p.id_cliente == id_cliente).ToList();
            }

            var total = pedidos.Count;
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            var paginatedData = pedidos.Skip((page - 1) * pageSize).Take(pageSize).ToList();

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

    private static async Task<IResult> GetPedidoRawByIdAsync(
        [FromServices] ICrudService crudService,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var pedido = await crudService.GetByIdAsync<PedidoRaw>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (pedido is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Pedido no encontrado" });
            
            return Results.Ok(new { success = true, data = pedido });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> CreatePedidoRawAsync(
        [FromServices] ICrudService crudService,
        [FromBody] PedidoRaw pedido,
        CancellationToken cancellationToken)
    {
        try
        {
            if (pedido.id_cliente == Guid.Empty)
            {
                return Results.BadRequest(new { success = false, error = "ValidationError", message = "El ID del cliente es requerido" });
            }

            if (string.IsNullOrWhiteSpace(pedido.contenido_raw))
            {
                return Results.BadRequest(new { success = false, error = "ValidationError", message = "El contenido_raw es requerido" });
            }

            if (string.IsNullOrWhiteSpace(pedido.estado))
            {
                pedido.estado = "pendiente";
            }

            pedido.id_pedido = Guid.NewGuid();
            pedido.created_at = DateTime.UtcNow;

            var created = await crudService.CreateAsync<PedidoRaw>(TableName, pedido, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Created($"/api/pedidos-raw/{created.id_pedido}", new { success = true, data = created });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> UpdatePedidoRawAsync(
        [FromServices] ICrudService crudService,
        Guid id,
        [FromBody] PedidoRaw pedido,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<PedidoRaw>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Pedido no encontrado" });

            pedido.id_pedido = id;
            pedido.created_at = existing.created_at;

            var updated = await crudService.UpdateAsync<PedidoRaw>(TableName, IdColumn, id, pedido, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Ok(new { success = true, data = updated });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> PartialUpdatePedidoRawAsync(
        [FromServices] ICrudService crudService,
        Guid id,
        [FromBody] PedidoRaw pedidoUpdate,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<PedidoRaw>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Pedido no encontrado" });

            if (!string.IsNullOrWhiteSpace(pedidoUpdate.contenido_raw))
                existing.contenido_raw = pedidoUpdate.contenido_raw;
            if (!string.IsNullOrWhiteSpace(pedidoUpdate.estado))
                existing.estado = pedidoUpdate.estado;

            var updated = await crudService.UpdateAsync<PedidoRaw>(TableName, IdColumn, id, existing, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Ok(new { success = true, data = updated });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> DeletePedidoRawAsync(
        [FromServices] ICrudService crudService,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<PedidoRaw>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Pedido no encontrado" });

            var deleted = await crudService.DeleteAsync(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (!deleted)
                return Results.BadRequest(new { success = false, error = "BadRequest", message = "Error al eliminar el pedido" });
            
            return Results.Ok(new { success = true, message = "Pedido eliminado correctamente" });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }
}