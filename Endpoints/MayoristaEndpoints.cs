using DownLabs.Core.Api.Models;
using DownLabs.Core.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DownLabs.Core.Api.Endpoints;

public static class MayoristaEndpoints
{
    private const string TableName = "mayoristas";
    private const string IdColumn = "id_mayorista";

    public static void RegisterMayoristaEndpoints(this WebApplication app)
    {
        app.MapGet("/api/mayoristas", GetAllMayoristas);
        app.MapGet("/api/mayoristas/{id}", GetMayoristaById);
        app.MapPost("/api/mayoristas", CreateMayorista);
        app.MapPut("/api/mayoristas/{id}", UpdateMayorista);
        app.MapPatch("/api/mayoristas/{id}", PartialUpdateMayorista);
        app.MapDelete("/api/mayoristas/{id}", DeleteMayorista);
    }

    private static async Task<IResult> GetAllMayoristas(
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

            var mayoristas = await crudService.GetAllAsync<Mayorista>(TableName, cancellationToken)
                .ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLowerInvariant();
                mayoristas = mayoristas.Where(m => 
                    (m.nombre_empresa?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                    (m.ubicacion?.ToLowerInvariant().Contains(searchLower) ?? false)
                ).ToList();
            }

            var total = mayoristas.Count;
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            var paginatedData = mayoristas.Skip((page - 1) * pageSize).Take(pageSize).ToList();

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

    private static async Task<IResult> GetMayoristaById(
        [FromServices] ICrudService crudService,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var mayorista = await crudService.GetByIdAsync<Mayorista>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (mayorista is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Mayorista no encontrado" });
            
            return Results.Ok(new { success = true, data = mayorista });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> CreateMayorista(
        [FromServices] ICrudService crudService,
        [FromBody] Mayorista mayorista,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(mayorista.nombre_empresa))
            {
                return Results.BadRequest(new { success = false, error = "ValidationError", message = "El nombre de la empresa es requerido" });
            }

            mayorista.id_mayorista = Guid.NewGuid();
            mayorista.created_at = DateTime.UtcNow;
            mayorista.updated_at = DateTime.UtcNow;

            var created = await crudService.CreateAsync<Mayorista>(TableName, mayorista, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Created($"/api/mayoristas/{created.id_mayorista}", new { success = true, data = created });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> UpdateMayorista(
        [FromServices] ICrudService crudService,
        Guid id,
        [FromBody] Mayorista mayorista,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<Mayorista>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Mayorista no encontrado" });

            mayorista.id_mayorista = id;
            mayorista.created_at = existing.created_at;
            mayorista.updated_at = DateTime.UtcNow;

            var updated = await crudService.UpdateAsync<Mayorista>(TableName, IdColumn, id, mayorista, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Ok(new { success = true, data = updated });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> PartialUpdateMayorista(
        [FromServices] ICrudService crudService,
        Guid id,
        [FromBody] Mayorista mayoristaUpdate,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<Mayorista>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Mayorista no encontrado" });

            if (mayoristaUpdate.nombre_empresa is not null)
                existing.nombre_empresa = mayoristaUpdate.nombre_empresa;
            if (mayoristaUpdate.ubicacion is not null)
                existing.ubicacion = mayoristaUpdate.ubicacion;
            if (mayoristaUpdate.correo_contacto is not null)
                existing.correo_contacto = mayoristaUpdate.correo_contacto;
            if (mayoristaUpdate.telefono_contacto is not null)
                existing.telefono_contacto = mayoristaUpdate.telefono_contacto;
            if (mayoristaUpdate.nivel_confianza > 0)
                existing.nivel_confianza = mayoristaUpdate.nivel_confianza;
            
            existing.updated_at = DateTime.UtcNow;

            var updated = await crudService.UpdateAsync<Mayorista>(TableName, IdColumn, id, existing, cancellationToken)
                .ConfigureAwait(false);
            
            return Results.Ok(new { success = true, data = updated });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteMayorista(
        [FromServices] ICrudService crudService,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await crudService.GetByIdAsync<Mayorista>(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (existing is null)
                return Results.NotFound(new { success = false, error = "NotFound", message = "Mayorista no encontrado" });

            var deleted = await crudService.DeleteAsync(TableName, IdColumn, id, cancellationToken)
                .ConfigureAwait(false);
            
            if (!deleted)
                return Results.BadRequest(new { success = false, error = "BadRequest", message = "Error al eliminar el mayorista" });
            
            return Results.Ok(new { success = true, message = "Mayorista eliminado correctamente" });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { success = false, error = "BadRequest", message = ex.Message });
        }
    }
}