using DownLabs.Core.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ISupabaseService, SupabaseService>();
builder.Services.AddOpenApi();

// Permitir peticiones desde el frontend Astro
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAstro", policy =>
    {
        policy.WithOrigins("http://localhost:4321")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowAstro");

// Registro de cliente
app.MapPost("/api/clientes/register", async (ISupabaseService supabaseService, RegisterRequest request) =>
{
    try
    {
        await supabaseService.RegisterClientAsync(
            request.nombre_empresa,
            request.correo_contacto,
            request.contrasena
        );
        return Results.Ok(new { success = true, message = "Cliente registrado exitosamente" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { success = false, message = ex.Message });
    }
});

// Login de cliente
app.MapGet("/api/clientes/login", async (ISupabaseService supabaseService, string correo_contacto, string contrasena) =>
{
    try
    {
        var cliente = await supabaseService.LoginClientAsync(correo_contacto, contrasena);
        
        if (cliente != null)
        {
            return Results.Ok(new { success = true, cliente });
        }
        
        return Results.BadRequest(new { success = false, message = "Credenciales invalidas" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { success = false, message = ex.Message });
    }
});

app.Run();

public record RegisterRequest(string nombre_empresa, string correo_contacto, string contrasena);
