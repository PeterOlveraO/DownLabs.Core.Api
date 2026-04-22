using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace DownLabs.Core.Api.Endpoints;

public static class StorageEndpoints
{
    private static readonly string[] AllowedTypes = { "image/jpeg", "image/png", "image/webp", "image/gif" };
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    public static void RegisterStorageEndpoints(this WebApplication app)
    {
        // POST /api/storage/upload-imagen
        // Modo archivo : multipart/form-data  con campos file (IFormFile) + proveedor_id
        // Modo URL     : application/json     con campos url  (string)    + proveedor_id
        // Retorna: { url: string }
        app.MapPost("/api/storage/upload-imagen", async (
            HttpRequest request,
            IConfiguration config) =>
        {
            // ── MODO URL (JSON) ────────────────────────────────────────────────
            if (request.ContentType != null &&
                request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
            {
                UrlImagenRequest? body;
                try
                {
                    body = await JsonSerializer.DeserializeAsync<UrlImagenRequest>(
                        request.Body,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch
                {
                    return Results.BadRequest(new { error = "JSON inválido" });
                }

                if (body is null || string.IsNullOrWhiteSpace(body.Url))
                    return Results.BadRequest(new { error = "El campo 'url' es requerido" });

                if (!Uri.TryCreate(body.Url, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                    return Results.BadRequest(new { error = "La URL no es válida" });

                var ext = Path.GetExtension(uri.AbsolutePath).ToLowerInvariant();
                if (!string.IsNullOrEmpty(ext) && !AllowedExtensions.Contains(ext))
                    return Results.BadRequest(new { error = "La URL no apunta a una imagen válida (.jpg, .png, .webp, .gif)" });

                return Results.Ok(new { url = body.Url });
            }

            // ── MODO ARCHIVO (multipart) ────────────────────────────────────────
            if (!request.HasFormContentType)
                return Results.BadRequest(new { error = "Se requiere multipart/form-data o application/json" });

            var form        = await request.ReadFormAsync();
            var file        = form.Files.GetFile("file");
            var proveedorId = form["proveedor_id"].ToString();

            if (file is null || file.Length == 0)
                return Results.BadRequest(new { error = "No se recibió ningún archivo" });

            if (file.Length > 5 * 1024 * 1024)
                return Results.BadRequest(new { error = "El archivo supera el límite de 5MB" });

            if (!AllowedTypes.Contains(file.ContentType))
                return Results.BadRequest(new { error = "Tipo de archivo no permitido. Use jpg, png, webp o gif" });

            var supabaseUrl = config["Supabase:Url"]
                ?? throw new InvalidOperationException("Supabase:Url configuration is missing");
            var serviceKey = config["Supabase:Key"]
                ?? throw new InvalidOperationException("Supabase:Key configuration is missing");

            var fileExt  = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = string.IsNullOrWhiteSpace(proveedorId)
                ? $"general/{Guid.NewGuid()}{fileExt}"
                : $"{proveedorId}/{Guid.NewGuid()}{fileExt}";
            var bucket = "productos-imagenes";

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("apikey", serviceKey);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {serviceKey}");
            httpClient.DefaultRequestHeaders.Add("x-upsert", "true"); // ← agrega esto

            using var stream  = file.OpenReadStream();
            using var content = new StreamContent(stream);
            content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

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

    private record UrlImagenRequest(string Url, string? ProveedorId);
}
