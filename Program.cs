using DownLabs.Core.Api.Endpoints;
using DownLabs.Core.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ICrudService, CrudService>();
builder.Services.AddOpenApi();

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

app.RegisterClienteEndpoints();
app.RegisterMayoristaEndpoints();
app.RegisterOperadorEndpoints();
app.RegisterCatalogoProductoEndpoints();
app.RegisterSolicitudCotizacionEndpoints();
app.RegisterCotizacionDownlabsEndpoints();
app.RegisterPedidoCreditoEndpoints();
app.RegisterAuthEndpoints();

app.MapGet("/api/health", () => Results.Ok(new { status = "OK", timestamp = DateTime.UtcNow }));

app.Run();