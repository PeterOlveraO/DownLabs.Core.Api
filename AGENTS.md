# AGENTS.md - DownLabs.Core.Api

## Project Overview
- **Framework**: .NET 9.0 ASP.NET Core Web API
- **Database**: Supabase (PostgreSQL)
- **Architecture**: Minimal APIs with dependency injection

---

## Build & Run Commands

### Build the project
```bash
dotnet build
```

### Run the application
```bash
dotnet run
# Available at: http://localhost:5145
```

### Run with custom URL
```bash
dotnet run --urls="http://localhost:5000"
```

### Restore dependencies
```bash
dotnet restore
```

### Clean build artifacts
```bash
dotnet clean
```

---

## Testing

**Note**: Currently there is no test project in this solution.

### If tests are added in the future:

Run all tests:
```bash
dotnet test
```

Run a single test by name:
```bash
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"
```

---

## Linting & Code Analysis

### Build with warnings as errors
```bash
dotnet build /p:TreatWarningsAsErrors=true
```

### Check for outdated packages
```bash
dotnet list package --outdated
```

---

## Project Structure

```
DownLabs.Core.Api/
├── Models/                      # Data models (one per table)
│   ├── Cliente.cs
│   ├── Mayorista.cs
│   ├── Operador.cs
│   ├── CatalogoProducto.cs
│   ├── SolicitudCotizacion.cs
│   ├── CotizacionDownlabs.cs
│   └── PedidoCredito.cs
├── Services/                    # Business logic (CRUD genérico)
│   ├── ICrudService.cs         # Interfaz genérica para CRUD
│   └── CrudService.cs          # Implementación
├── Endpoints/                   # API endpoints (uno por modelo)
│   ├── ClienteEndpoints.cs
│   ├── MayoristaEndpoints.cs
│   ├── OperadorEndpoints.cs
│   ├── CatalogoProductoEndpoints.cs
│   ├── SolicitudCotizacionEndpoints.cs
│   ├── CotizacionDownlabsEndpoints.cs
│   └── PedidoCreditoEndpoints.cs
├── Program.cs                   # Entry point
├── appsettings.json             # Config (nunca commitear)
└── DownLabs.Core.Api.csproj
```

---

## Code Style Guidelines

### General Conventions

- **Implicit Usings**: Enabled
- **Nullable**: Enabled - use `?` for nullable reference types
- **Target Framework**: net9.0

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Classes | PascalCase | `Cliente`, `CrudService` |
| Interfaces | PascalCase with 'I' prefix | `ICrudService` |
| Methods | PascalCase + Async suffix | `GetAllAsync`, `CreateAsync` |
| Properties (DB) | snake_case | `id_cliente`, `nombre_empresa` |
| Parameters | camelCase | `tableName`, `idColumn` |

### Modelos (Database Mapping)

```csharp
public class Cliente
{
    public Guid id_cliente { get; set; }
    public string? nombre_empresa { get; set; }
    public DateTime? created_at { get; set; }
}
```

### Imports

- File-scoped namespaces
- Orden: System → Third-party → Project
```csharp
using System.Net.Http.Headers;
using System.Text.Json;
using DownLabs.Core.Api.Models;
using DownLabs.Core.Api.Services;
using Microsoft.AspNetCore.Mvc;
```

---

## API Endpoints Pattern

### Estructura de cada endpoint

Cada archivo en `Endpoints/` sigue el mismo patrón:

```csharp
public static class ClienteEndpoints
{
    private const string TableName = "clientes";
    private const string IdColumn = "id_cliente";

    public static void RegisterClienteEndpoints(this WebApplication app)
    {
        app.MapGet("/api/clientes", GetAll);
        app.MapGet("/api/clientes/{id}", GetById);
        app.MapPost("/api/clientes", Create);
        app.MapPut("/api/clientes/{id}", Update);
        app.MapPatch("/api/clientes/{id}", PartialUpdate);
        app.MapDelete("/api/clientes/{id}", Delete);
    }
}
```

### Métodos HTTP

| Método | Uso | Idempotente |
|--------|-----|-------------|
| GET | Listar / Obtener uno | Sí |
| POST | Crear nuevo | No |
| PUT | Reemplazar completo | Sí |
| PATCH | Actualizar parcial | No |
| DELETE | Eliminar | Sí |

### Paginación y Filtros

Todos los endpoints GET soportan:
- `page` (default: 1)
- `pageSize` (default: 20, max: 100)
- Filtros específicos por tabla

```json
{
  "success": true,
  "data": [...],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "total": 100,
    "totalPages": 5,
    "hasNext": true,
    "hasPrev": false
  }
}
```

### Response Format

```json
// Success
{ "success": true, "data": {...} }

// Error
{ "success": false, "error": "NotFound", "message": "Cliente no encontrado" }

// 500 Error
Results.Problem("Error interno: {message}", statusCode: 500)
```

---

## Error Handling

- **ValidationError**: Datos inválidos en request
- **NotFound**: Recurso no encontrado
- **BadRequest**: Error de negocio
- **500**: Error interno del servidor

```csharp
if (cliente is null)
    return Results.NotFound(new { success = false, error = "NotFound", message = "Cliente no encontrado" });
```

---

## Async/Await Best Practices

- Siempre usar `async Task` o `async Task<T>`
- Usar `ConfigureAwait(false)` en llamadas a servicios
- Usar `CancellationToken` en métodos largos
- Nunca usar `.Result` o `.Wait()`

```csharp
public async Task<T?> GetByIdAsync<T>(string tableName, string idColumn, Guid id, CancellationToken cancellationToken = default) where T : class
{
    var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
}
```

---

## Configuration

- `appsettings.json` - Credenciales Supabase (nunca commitear)
- `IConfiguration` para acceder a settings
- CORS configurado para `http://localhost:4321` (Astro)

---

## Services (CRUD Genérico)

El `CrudService` es un servicio genérico que maneja todas las operaciones CRUD:

```csharp
builder.Services.AddSingleton<ICrudService, CrudService>();
```

Métodos disponibles:
- `GetAllAsync<T>(tableName)` - Obtener todos
- `GetByIdAsync<T>(tableName, idColumn, id)` - Obtener por ID
- `CreateAsync<T>(tableName, data)` - Crear
- `UpdateAsync<T>(tableName, idColumn, id, data)` - Actualizar
- `DeleteAsync(tableName, idColumn, id)` - Eliminar

---

## Security Notes

- Passwords pueden estar en texto plano (proyecto educativo)
- No hay JWT authentication
- Para producción: agregar BCrypt, JWT, validación de input, HTTPS

---

## Common Tasks

### Agregar un package
```bash
dotnet add package <PackageName>
```

### Agregar un nuevo modelo
1. Crear `Models/NuevoModelo.cs`
2. Agregar a la base de datos de Supabase
3. Crear `Endpoints/NuevoModeloEndpoints.cs` (copiar patrón existente)

### Agregar nuevo endpoint a modelo existente
1. Abrir el archivo en `Endpoints/`
2. Agregar método y registrarla en `RegisterXEndpoints()`
3. Usar el `ICrudService` inyectado

---

## Tablas Disponibles

| Tabla | Endpoint | Descripción |
|-------|----------|-------------|
| clientes | `/api/clientes` | Clientes registrados |
| mayoristas | `/api/mayoristas` | Empresas mayoristas |
| operadores | `/api/operadores` | Operadores del sistema |
| catalogo_productos | `/api/catalogo-productos` | Catálogo de productos |
| solicitudes_cotizacion | `/api/solicitudes-cotizacion` | Solicitudes de cotización |
| cotizaciones_downlabs | `/api/cotizaciones` | Cotizaciones generadas |
| pedidos_creditos | `/api/pedidos-credito` | Pedidos con crédito |