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
            // Leer multipart form
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

            // Configuración de Supabase
            var supabaseUrl = config["Supabase:Url"];
            var serviceKey  = config["Supabase:ServiceKey"];

            if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(serviceKey))
                return Results.Problem("Configuración de Supabase incompleta");

            // Generar nombre único para el archivo
            var ext      = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{proveedorId}/{Guid.NewGuid()}{ext}";
            var bucket   = "productos-imagenes";

            // Subir a Supabase Storage via REST API
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

            // Construir URL pública
            var publicUrl = $"{supabaseUrl}/storage/v1/object/public/{bucket}/{fileName}";
            return Results.Ok(new { url = publicUrl });
        })
        .DisableAntiforgery()
        .WithTags("Storage");
    }
}
