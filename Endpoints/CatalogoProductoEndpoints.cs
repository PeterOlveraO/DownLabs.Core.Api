using DownLabs.Core.Api.Models;
using DownLabs.Core.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DownLabs.Core.Api.Endpoints;

public static class CatalogoProductoEndpoints
{
    private const string TableName = "catalogo_productos";
    private const string IdColumn = "id_producto";

    public static void RegisterCatalogoProductoEndpoints(this WebApplication app)
    {
        app.MapGet("/api/catalogo-productos", GetAllCatalogoProductos);
        app.MapGet("/api/catalogo-productos/{id}", GetCatalogoProductoById);
        app.MapPost("/api/catalogo-productos", CreateCatalogoProducto);
        app.MapPut("/api/catalogo-productos/{id}", UpdateCatalogoProducto);
        app.MapPatch("/api/catalogo-productos/{id}", PartialUpdateCatalogoProducto);
        app.MapDelete("/api/catalogo-productos/{id}", DeleteCatalogoProducto);
    }

    private static async Task<IResult> GetAllCatalogoProductos(
        [FromServices] ICrudService crudService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? categoria = null,
        [FromQuery] Guid? id_mayorista = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(page, 1);

            var productos = await crudService.GetAllAsync<CatalogoProducto>(TableName, cancellationToken)
                .ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(search))
            {
                productos = productos.Where(p => 
                    (p.nombre_articulo?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.categoria?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.materiales?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                productos = productos.Where(p => p.categoria?.Equals(categoria, StringComparison.OrdinalIgnoreCase) == true).ToList();
            }

            if (id_mayorista.HasValue)
            {
                productos = productos.Where(p => p.id_mayorista == id_mayorista).ToList();
            }

            var total = productos.Count;
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            var paginatedData = productos.Skip((page - 1) * pageSize).Take(pageSize).ToList();

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

    private static async Task<IResult> GetCatalogoProductoById(
        [FromServices] ICrudService crudService,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var producto = await crudService.GetByIdAsync<CatalogoProducto>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (producto is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Producto no encontrado" });
            
            return Results.Ok(new { success = true, data = producto });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> CreateCatalogoProducto(
        [FromServices] ICrudService crudService,
        [FromBody] CatalogoProducto producto,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(producto.nombre_articulo))
            {
                return Results.BadRequest(new { success = false, error = "ValidationError", message = "El nombre del articulo es requerido" });
            }

            if (!producto.id_mayorista.HasValue)
            {
                return Results.BadRequest(new { success = false, error = "ValidationError", message = "El ID del mayorista es requerido" });
            }

            producto.id_producto = Guid.NewGuid();
            producto.created_at = DateTime.UtcNow;
            producto.updated_at = DateTime.UtcNow;

            var created = await crudService.CreateAsync<CatalogoProducto>(TableName, producto, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Created($"/api/catalogo-productos/{created.id_producto}", new { success = true, data = created });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> UpdateCatalogoProducto(
        [FromServices] ICrudService crudService,
        Guid id,
        [FromBody] CatalogoProducto producto,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<CatalogoProducto>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Producto no encontrado" });

            producto.id_producto = id;
            producto.created_at = existing.created_at;
            producto.updated_at = DateTime.UtcNow;

            var updated = await crudService.UpdateAsync<CatalogoProducto>(TableName, IdColumn, id, producto, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Ok(new { success = true, data = updated });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> PartialUpdateCatalogoProducto(
        [FromServices] ICrudService crudService,
        Guid id,
        [FromBody] CatalogoProducto productoUpdate,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<CatalogoProducto>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Producto no encontrado" });

            if (productoUpdate.nombre_articulo is not null)
                existing.nombre_articulo = productoUpdate.nombre_articulo;
            if (productoUpdate.categoria is not null)
                existing.categoria = productoUpdate.categoria;
            if (productoUpdate.precio_mayorista is not null)
                existing.precio_mayorista = productoUpdate.precio_mayorista;
            if (productoUpdate.precio_dwlabs is not null)
                existing.precio_dwlabs = productoUpdate.precio_dwlabs;
            if (productoUpdate.materiales is not null)
                existing.materiales = productoUpdate.materiales;
            if (productoUpdate.moq is not null)
                existing.moq = productoUpdate.moq;
            
            existing.updated_at = DateTime.UtcNow;

            var updated = await crudService.UpdateAsync<CatalogoProducto>(TableName, IdColumn, id, existing, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Ok(new { success = true, data = updated });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteCatalogoProducto(
        [FromServices] ICrudService crudService,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<CatalogoProducto>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Producto no encontrado" });

            var deleted = await crudService.DeleteAsync(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (!deleted)
                return Results.BadRequest(new { success = false, error = "BadRequest", message = "Error al eliminar el producto" });
            
            return Results.Ok(new { success = true, message = "Producto eliminado correctamente" });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }
}