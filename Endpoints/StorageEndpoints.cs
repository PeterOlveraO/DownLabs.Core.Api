using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace DownLabs.Core.Api.Endpoints;

public static class StorageEndpoints
{
    public static void RegisterStorageEndpoints(this WebApplication app)
    {
        // POST /api/storage/upload-imagen
        // Recibe multipart/form-data con campos: file (IFormFile), proveedor_id (string)
        // Sube la imagen al bucket 'productos-imagenes' en Supabase Storage
        // Retorna: { url: string } con la URL pública del archivo
        app.MapPost("/api/storage/upload-imagen", async (
            HttpRequest request,
            IConfiguration config) =>
        {
            if (!request.HasFormContentType)
                return Results.BadRequest(new { error = "Se requiere multipart/form-data" });

            var form = await request.ReadFormAsync();
            var file = form.Files.GetFile("file");
            var proveedorId = form["proveedor_id"].ToString();

            if (file is null || file.Length == 0)
                return Results.BadRequest(new { error = "No se recibió ningún archivo" });

            if (file.Length > 5 * 1024 * 1024)
                return Results.BadRequest(new { error = "El archivo supera el límite de 5MB" });

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (!allowedTypes.Contains(file.ContentType))
                return Results.BadRequest(new { error = "Tipo de archivo no permitido" });

            var supabaseUrl = config["Supabase:Url"]
                ?? throw new InvalidOperationException("Supabase:Url configuration is missing");
            var serviceKey = config["Supabase:Key"]
                ?? throw new InvalidOperationException("Supabase:Key configuration is missing");

            var ext      = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{proveedorId}/{Guid.NewGuid()}{ext}";
            var bucket   = "productos-imagenes";

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("apikey", serviceKey);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {serviceKey}");

            using var stream  = file.OpenReadStream();
            using var content = new StreamContent(stream);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

            var uploadUrl = $"{supabaseUrl}/storage/v1/object/{bucket}/{fileName}";
            var response  = await httpClient.PostAsync(uploadUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                return Results.Problem($"Error al subir imagen: {err}");
            }

            var publicUrl = $"{supabaseUrl}/storage/v1/object/public/{bucket}/{fileName}";
            return Results.Ok(new { url = publicUrl });
        })
        .DisableAntiforgery()
        .WithTags("Storage");
    }
}
