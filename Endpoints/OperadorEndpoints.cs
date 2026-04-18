using DownLabs.Core.Api.Models;
using DownLabs.Core.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DownLabs.Core.Api.Endpoints;

public static class OperadorEndpoints
{
    private const string TableName = "operadores";
    private const string IdColumn = "id_operadores";

    public static void RegisterOperadorEndpoints(this WebApplication app)
    {
        app.MapGet("/api/operadores", GetAllOperadores);
        app.MapGet("/api/operadores/{id}", GetOperadorById);
        app.MapPost("/api/operadores", CreateOperador);
        app.MapPut("/api/operadores/{id}", UpdateOperador);
        app.MapPatch("/api/operadores/{id}", PartialUpdateOperador);
        app.MapDelete("/api/operadores/{id}", DeleteOperador);
    }

    private static async Task<IResult> GetAllOperadores(
        [FromServices] ICrudService crudService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? activo = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(page, 1);

            var operadores = await crudService.GetAllAsync<Operador>(TableName, cancellationToken)
                .ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(search))
            {
                operadores = operadores.Where(o => 
                    (o.nombre?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (o.apellido?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (o.email?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            if (activo.HasValue)
            {
                operadores = operadores.Where(o => o.activo == activo.Value).ToList();
            }

            var total = operadores.Count;
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            var paginatedData = operadores.Skip((page - 1) * pageSize).Take(pageSize).ToList();

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

    private static async Task<IResult> GetOperadorById(
        [FromServices] ICrudService crudService,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var operador = await crudService.GetByIdAsync<Operador>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (operador is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Operador no encontrado" });
            
            return Results.Ok(new { success = true, data = operador });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> CreateOperador(
        [FromServices] ICrudService crudService,
        [FromBody] Operador operador,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(operador.nombre))
            {
                return Results.BadRequest(new { success = false, error = "ValidationError", message = "El nombre es requerido" });
            }

            if (string.IsNullOrWhiteSpace(operador.email))
            {
                return Results.BadRequest(new { success = false, error = "ValidationError", message = "El email es requerido" });
            }

            if (string.IsNullOrWhiteSpace(operador.apellido))
            {
                return Results.BadRequest(new { success = false, error = "ValidationError", message = "El apellido es requerido" });
            }

            operador.id_operadores = Guid.NewGuid();
            operador.activo = true;
            operador.created_at = DateTime.UtcNow;
            operador.updated_at = DateTime.UtcNow;

            var created = await crudService.CreateAsync<Operador>(TableName, operador, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Created($"/api/operadores/{created.id_operadores}", new { success = true, data = created });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> UpdateOperador(
        [FromServices] ICrudService crudService,
        Guid id,
        [FromBody] Operador operador,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<Operador>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Operador no encontrado" });

            operador.id_operadores = id;
            operador.created_at = existing.created_at;
            operador.updated_at = DateTime.UtcNow;

            var updated = await crudService.UpdateAsync<Operador>(TableName, IdColumn, id, operador, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Ok(new { success = true, data = updated });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> PartialUpdateOperador(
        [FromServices] ICrudService crudService,
        Guid id,
        [FromBody] Operador operadorUpdate,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<Operador>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Operador no encontrado" });

            if (operadorUpdate.nombre is not null)
                existing.nombre = operadorUpdate.nombre;
            if (operadorUpdate.apellido is not null)
                existing.apellido = operadorUpdate.apellido;
            if (operadorUpdate.email is not null)
                existing.email = operadorUpdate.email;
            if (operadorUpdate.telefono is not null)
                existing.telefono = operadorUpdate.telefono;
            if (operadorUpdate.activo)
                existing.activo = operadorUpdate.activo;
            
            existing.updated_at = DateTime.UtcNow;

            var updated = await crudService.UpdateAsync<Operador>(TableName, IdColumn, id, existing, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Ok(new { success = true, data = updated });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteOperador(
        [FromServices] ICrudService crudService,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<Operador>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Operador no encontrado" });

            var deleted = await crudService.DeleteAsync(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (!deleted)
                return Results.BadRequest(new { success = false, error = "BadRequest", message = "Error al eliminar el operador" });
            
            return Results.Ok(new { success = true, message = "Operador eliminado correctamente" });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }
}