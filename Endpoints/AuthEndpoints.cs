using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using DownLabs.Core.Api.Services;

namespace DownLabs.Core.Api.Endpoints;

public static class AuthEndpoints
{
    private const string AuthApiPath = "/auth/v1";

    private static string GetAuthUrl(IConfiguration config)
        => $"{config["Supabase:Url"]}{AuthApiPath}";

    private static string GetSupabaseKey(IConfiguration config)
        => config["Supabase:Key"] ?? throw new InvalidOperationException("Supabase:Key no configurado");

    public static void RegisterAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");
        group.MapPost("/register", RegisterAsync);
        group.MapPost("/login", LoginAsync);
    }

    private static async Task<IResult> RegisterAsync(
        [FromBody] RegisterRequest request,
        [FromServices] IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return Results.BadRequest(new 
                { 
                    success = false, 
                    error = "ValidationError", 
                    message = "El email es requerido" 
                });
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new 
                { 
                    success = false, 
                    error = "ValidationError", 
                    message = "La contraseña es requerida" 
                });
            }

            if (string.IsNullOrWhiteSpace(request.Rol))
            {
                return Results.BadRequest(new 
                { 
                    success = false, 
                    error = "ValidationError", 
                    message = "El rol es requerido" 
                });
            }

            var validRoles = new[] { "cliente", "mayorista", "operador" };
            if (!validRoles.Contains(request.Rol.ToLowerInvariant()))
            {
                return Results.BadRequest(new 
                { 
                    success = false, 
                    error = "ValidationError", 
                    message = "Rol inválido. Debe ser: cliente, mayorista u operador" 
                });
            }

            var metadata = new Dictionary<string, object>
            {
                { "rol", request.Rol }
            };

            if (!string.IsNullOrWhiteSpace(request.NombreEmpresa))
            {
                metadata["nombre_empresa"] = request.NombreEmpresa;
            }

            if (!string.IsNullOrWhiteSpace(request.Nombre))
            {
                metadata["nombre"] = request.Nombre;
            }

            if (!string.IsNullOrWhiteSpace(request.Apellido))
            {
                metadata["apellido"] = request.Apellido;
            }

            var signupPayload = new
            {
                email = request.Email,
                password = request.Password,
                email_confirm = true,
                user_metadata = metadata
            };

            var supabaseKey = GetSupabaseKey(configuration);
            
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("apikey", supabaseKey);
            httpClient.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse($"Bearer {supabaseKey}");
            
            var content = new StringContent(
                JsonSerializer.Serialize(signupPayload),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await httpClient.PostAsync($"{GetAuthUrl(configuration)}/admin/users", content, cancellationToken)
                .ConfigureAwait(false);

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorObj = JsonSerializer.Deserialize<JsonElement>(responseJson);
                var errorMsg = errorObj.TryGetProperty("msg", out var msg) 
                    ? msg.GetString() ?? "Error al registrar usuario" 
                    : "Error al registrar usuario";

                return Results.BadRequest(new 
                { 
                    success = false, 
                    error = "BadRequest", 
                    message = errorMsg 
                });
            }

            return Results.Ok(new 
            { 
                success = true, 
                message = "Usuario registrado correctamente.",
                data = new
                {
                    email = request.Email,
                    rol = request.Rol
                }
            });
        }
        catch (OperationCanceledException)
        {
            return Results.Problem("Operación cancelada", statusCode: 499);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error interno: {ex.Message}", statusCode: 500);
        }
    }

    private static async Task<IResult> LoginAsync(
        [FromBody] LoginRequest request,
        [FromServices] IConfiguration configuration,
        [FromServices] ICrudService crudService,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return Results.BadRequest(new 
                { 
                    success = false, 
                    error = "ValidationError", 
                    message = "El email es requerido" 
                });
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new 
                { 
                    success = false, 
                    error = "ValidationError", 
                    message = "La contraseña es requerida" 
                });
            }

            var supabaseKey = GetSupabaseKey(configuration);

            var loginPayload = new
            {
                email = request.Email,
                password = request.Password
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("apikey", supabaseKey);
            
            var content = new StringContent(
                JsonSerializer.Serialize(loginPayload),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await httpClient.PostAsync($"{GetAuthUrl(configuration)}/token?grant_type=password", content, cancellationToken)
                .ConfigureAwait(false);

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return Results.BadRequest(new 
                { 
                    success = false, 
                    error = "Unauthorized", 
                    message = $"Email o contraseña incorrectos." 
                });
            }

            var session = JsonSerializer.Deserialize<JsonElement>(responseJson);

            var accessToken = session.GetProperty("access_token").GetString();
            var user = session.GetProperty("user");
            var userId = user.GetProperty("id").GetString();
            var userEmail = user.GetProperty("email").GetString();
            var userMetadata = user.GetProperty("user_metadata");

            var rol = userMetadata.TryGetProperty("rol", out var rolProp) 
                ? rolProp.GetString() ?? "cliente" 
                : "cliente";

                if (string.IsNullOrWhiteSpace(userId))
                    return Results.Problem("No se pudo obtener el ID del usuario de Supabase", statusCode: 500);

                if (string.IsNullOrWhiteSpace(accessToken))
                    return Results.Problem("No se pudo obtener el token de acceso", statusCode: 500);

            // Obtener perfil según el rol
            object? perfil = rol.ToLowerInvariant() switch
            {
                "cliente" => await crudService.GetByIdAsync<Models.Cliente>("clientes", "id_cliente", Guid.Parse(userId), cancellationToken).ConfigureAwait(false),
                "mayorista" => await crudService.GetByIdAsync<Models.Mayorista>("mayoristas", "id_mayorista", Guid.Parse(userId), cancellationToken).ConfigureAwait(false),
                "operador" => await crudService.GetByIdAsync<Models.Operador>("operadores", "id_operadores", Guid.Parse(userId), cancellationToken).ConfigureAwait(false),
                _ => null
            };

            return Results.Ok(new 
            { 
                success = true, 
                data = new
                {
                    access_token = accessToken,
                    token_type = "Bearer",
                    user_id = userId,
                    email = userEmail,
                    rol = rol,
                    perfil = perfil
                }
            });
        }
        catch (OperationCanceledException)
        {
            return Results.Problem("Operación cancelada", statusCode: 499);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error interno: {ex.Message}", statusCode: 500);
        }
    }
}

public class RegisterRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("rol")]
    public string Rol { get; set; } = string.Empty;

    [JsonPropertyName("nombre_empresa")]
    public string? NombreEmpresa { get; set; }

    [JsonPropertyName("nombre")]
    public string? Nombre { get; set; }

    [JsonPropertyName("apellido")]
    public string? Apellido { get; set; }
}

public class LoginRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}