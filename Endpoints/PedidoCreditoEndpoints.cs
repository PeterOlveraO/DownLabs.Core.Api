using DownLabs.Core.Api.Models;
using DownLabs.Core.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DownLabs.Core.Api.Endpoints;

public static class PedidoCreditoEndpoints
{
    private const string TableName = "pedidos_creditos";
    private const string IdColumn = "id_pedido";

    public static void RegisterPedidoCreditoEndpoints(this WebApplication app)
    {
        app.MapGet("/api/pedidos-credito", GetAllPedidosCredito);
        app.MapGet("/api/pedidos-credito/{id}", GetPedidoCreditoById);
        app.MapPost("/api/pedidos-credito", CreatePedidoCredito);
        app.MapPut("/api/pedidos-credito/{id}", UpdatePedidoCredito);
        app.MapPatch("/api/pedidos-credito/{id}", PartialUpdatePedidoCredito);
        app.MapDelete("/api/pedidos-credito/{id}", DeletePedidoCredito);
    }

    private static async Task<IResult> GetAllPedidosCredito(
        [FromServices] ICrudService crudService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? estado_pago = null,
        [FromQuery] bool? requiere_credito = null,
        [FromQuery] Guid? id_cotizacion = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(page, 1);

            var pedidos = await crudService.GetAllAsync<PedidoCredito>(TableName, cancellationToken)
                .ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(estado_pago))
            {
                pedidos = pedidos.Where(p => p.estado_pago?.Equals(estado_pago, StringComparison.OrdinalIgnoreCase) == true).ToList();
            }

            if (requiere_credito.HasValue)
            {
                pedidos = pedidos.Where(p => p.requiere_credito == requiere_credito).ToList();
            }

            if (id_cotizacion.HasValue)
            {
                pedidos = pedidos.Where(p => p.id_cotizacion == id_cotizacion).ToList();
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

    private static async Task<IResult> GetPedidoCreditoById(
        [FromServices] ICrudService crudService,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var pedido = await crudService.GetByIdAsync<PedidoCredito>(TableName, IdColumn, id, cancellationToken)
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

    private static async Task<IResult> CreatePedidoCredito(
        [FromServices] ICrudService crudService,
        [FromBody] PedidoCredito pedido,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!pedido.id_cotizacion.HasValue)
            {
                return Results.BadRequest(new { success = false, error = "ValidationError", message = "El ID de cotizacion es requerido" });
            }

            if (string.IsNullOrWhiteSpace(pedido.estado_pago))
            {
                pedido.estado_pago = "pendiente";
            }

            pedido.id_pedido = Guid.NewGuid();
            pedido.created_at = DateTime.UtcNow;
            pedido.updated_at = DateTime.UtcNow;

            var created = await crudService.CreateAsync<PedidoCredito>(TableName, pedido, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Created($"/api/pedidos-credito/{created.id_pedido}", new { success = true, data = created });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> UpdatePedidoCredito(
        [FromServices] ICrudService crudService,
        Guid id,
        [FromBody] PedidoCredito pedido,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<PedidoCredito>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Pedido no encontrado" });

            pedido.id_pedido = id;
            pedido.created_at = existing.created_at;
            pedido.updated_at = DateTime.UtcNow;

            var updated = await crudService.UpdateAsync<PedidoCredito>(TableName, IdColumn, id, pedido, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Ok(new { success = true, data = updated });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> PartialUpdatePedidoCredito(
        [FromServices] ICrudService crudService,
        Guid id,
        [FromBody] PedidoCredito pedidoUpdate,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<PedidoCredito>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Pedido no encontrado" });

            if (pedidoUpdate.requiere_credito != default)
                existing.requiere_credito = pedidoUpdate.requiere_credito;
            if (pedidoUpdate.cargo_financiamiento is not null)
                existing.cargo_financiamiento = pedidoUpdate.cargo_financiamiento;
            if (pedidoUpdate.monto_total_deuda is not null)
                existing.monto_total_deuda = pedidoUpdate.monto_total_deuda;
            if (pedidoUpdate.estado_pago is not null)
                existing.estado_pago = pedidoUpdate.estado_pago;
            if (pedidoUpdate.fecha_inicio_credito is not null)
                existing.fecha_inicio_credito = pedidoUpdate.fecha_inicio_credito;
            if (pedidoUpdate.fecha_vencimiento_credito is not null)
                existing.fecha_vencimiento_credito = pedidoUpdate.fecha_vencimiento_credito;
            if (pedidoUpdate.requiere_factura != default)
                existing.requiere_factura = pedidoUpdate.requiere_factura;
            if (pedidoUpdate.tipo_pago is not null)
                existing.tipo_pago = pedidoUpdate.tipo_pago;
            
            existing.updated_at = DateTime.UtcNow;

            var updated = await crudService.UpdateAsync<PedidoCredito>(TableName, IdColumn, id, existing, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Ok(new { success = true, data = updated });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> DeletePedidoCredito(
        [FromServices] ICrudService crudService,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<PedidoCredito>(TableName, IdColumn, id, cancellationToken)
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