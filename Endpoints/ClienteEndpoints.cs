using DownLabs.Core.Api.Models;
using DownLabs.Core.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DownLabs.Core.Api.Endpoints;

public static class ClienteEndpoints
{
    private const string TableName = "clientes";
    private const string IdColumn = "id_cliente";

    public static void RegisterClienteEndpoints(this WebApplication app)
    {
        app.MapGet("/api/clientes", GetAllClientes);
        app.MapGet("/api/clientes/{id}", GetClienteById);
        app.MapPost("/api/clientes", CreateCliente);
        app.MapPut("/api/clientes/{id}", UpdateCliente);
        app.MapPatch("/api/clientes/{id}", PartialUpdateCliente);
        app.MapDelete("/api/clientes/{id}", DeleteCliente);
    }

    private static async Task<IResult> GetAllClientes(
        [FromServices] ICrudService crudService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(page, 1);

            var clientes = await crudService.GetAllAsync<Cliente>(TableName, cancellationToken)
                .ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(search))
            {
                clientes = clientes.Where(c => 
                    (c.nombre_empresa?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.correo_contacto?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            var total = clientes.Count;
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            var paginatedData = clientes.Skip((page - 1) * pageSize).Take(pageSize).ToList();

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

    private static async Task<IResult> GetClienteById(
        [FromServices] ICrudService crudService,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var cliente = await crudService.GetByIdAsync<Cliente>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (cliente is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Cliente no encontrado" });
            
            return Results.Ok(new { success = true, data = cliente });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> CreateCliente(
        [FromServices] ICrudService crudService,
        [FromBody] Cliente cliente,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cliente.nombre_empresa))
            {
                return Results.BadRequest(new { success = false, error = "ValidationError", message = "El nombre de la empresa es requerido" });
            }

            cliente.id_cliente = Guid.NewGuid();
            cliente.created_at = DateTime.UtcNow;
            cliente.updated_at = DateTime.UtcNow;

            var created = await crudService.CreateAsync<Cliente>(TableName, cliente, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Created($"/api/clientes/{created.id_cliente}", new { success = true, data = created });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> UpdateCliente(
        [FromServices] ICrudService crudService,
        Guid id,
        [FromBody] Cliente cliente,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<Cliente>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Cliente no encontrado" });

            cliente.id_cliente = id;
            cliente.created_at = existing.created_at;
            cliente.updated_at = DateTime.UtcNow;

            var updated = await crudService.UpdateAsync<Cliente>(TableName, IdColumn, id, cliente, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Ok(new { success = true, data = updated });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> PartialUpdateCliente(
        [FromServices] ICrudService crudService,
        Guid id,
        [FromBody] Cliente clienteUpdate,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<Cliente>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Cliente no encontrado" });

            if (clienteUpdate.nombre_empresa is not null)
                existing.nombre_empresa = clienteUpdate.nombre_empresa;
            if (clienteUpdate.correo_contacto is not null)
                existing.correo_contacto = clienteUpdate.correo_contacto;
            if (clienteUpdate.telefono_contacto is not null)
                existing.telefono_contacto = clienteUpdate.telefono_contacto;
            if (clienteUpdate.rfc is not null)
                existing.rfc = clienteUpdate.rfc;
            if (clienteUpdate.codigo_postal_fiscal is not null)
                existing.codigo_postal_fiscal = clienteUpdate.codigo_postal_fiscal;
            
            existing.updated_at = DateTime.UtcNow;

            var updated = await crudService.UpdateAsync<Cliente>(TableName, IdColumn, id, existing, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Ok(new { success = true, data = updated });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteCliente(
        [FromServices] ICrudService crudService,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<Cliente>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Cliente no encontrado" });

            var deleted = await crudService.DeleteAsync(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (!deleted)
                return Results.BadRequest(new { success = false, error = "BadRequest", message = "Error al eliminar el cliente" });
            
            return Results.Ok(new { success = true, message = "Cliente eliminado correctamente" });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }
}