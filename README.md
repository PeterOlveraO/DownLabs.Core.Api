# DownLabs.Core.Api

<p align="center">
  <img src="https://img.shields.io/badge/.NET-9.0-blue?style=flat-square&logo=.net" alt=".NET Version">
  <img src="https://img.shields.io/badge/ASP.NET-Core-Web%20API-bd5e32?style=flat-square" alt="Framework">
  <img src="https://img.shields.io/badge/Supabase-PostgreSQL-3fbf8c?style=flat-square&logo=supabase" alt="Database">
  <img src="https://img.shields.io/badge/License-MIT-yellow?style=flat-square" alt="License">
</p>

RESTful API backend for DownLabs platform built with .NET 9.0 and Supabase.

## Overview

DownLabs.Core.Api provides a complete REST API for managing clients, wholesalers, operators, product catalogs, quote requests, quotes, and credit orders. The API follows RESTful design principles with pagination, filtering, and consistent error handling.

## Features

- **CRUD Operations** - Full Create, Read, Update, Delete for all entities
- **Pagination** - Automatic pagination for all list endpoints (default 20 items, max 100)
- **Filtering** - Query parameters for filtering and searching
- **Async/Await** - Modern async patterns with CancellationToken support
- **Generic Service** - Reusable CRUD service for all tables
- **CORS Ready** - Pre-configured for frontend integration

## Quick Start

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Supabase](https://supabase.com) account and project

### Configuration

Create `appsettings.json` in the project root:

```json
{
  "Logging": {
    "LogLevel": { "Default": "Information" }
  },
  "AllowedHosts": "*",
  "Supabase": {
    "Url": "https://your-project.supabase.co",
    "Key": "your-supabase-key"
  }
}
```

> [!IMPORTANT]
> Never commit `appsettings.json` - it contains sensitive credentials.

### Run the API

```bash
dotnet run
```

The API will be available at `http://localhost:5145`

## API Endpoints

| Resource | Base URL | Description |
|----------|----------|-------------|
| Clientes | `/api/clientes` | Registered clients |
| Mayoristas | `/api/mayoristas` | Wholesaler companies |
| Operadores | `/api/operadores` | System operators |
| Productos | `/api/catalogo-productos` | Product catalog |
| Solicitudes | `/api/solicitudes-cotizacion` | Quote requests |
| Cotizaciones | `/api/cotizaciones` | Generated quotes |
| Pedidos | `/api/pedidos-credito` | Credit orders |

### Available Endpoints (per resource)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/{resource}` | List all (paginated) |
| GET | `/api/{resource}/{id}` | Get by ID |
| POST | `/api/{resource}` | Create new |
| PUT | `/api/{resource}/{id}` | Replace |
| PATCH | `/api/{resource}/{id}` | Partial update |
| DELETE | `/api/{resource}/{id}` | Delete |

### Query Parameters

All GET endpoints support:

- `page` - Page number (default: 1)
- `pageSize` - Items per page (default: 20, max: 100)

### Example Response

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

## Project Structure

```
DownLabs.Core.Api/
├── Models/                      # Data models
│   ├── Cliente.cs
│   ├── Mayorista.cs
│   ├── Operador.cs
│   ├── CatalogoProducto.cs
│   ├── SolicitudCotizacion.cs
│   ├── CotizacionDownlabs.cs
│   └── PedidoCredito.cs
├── Services/                   # Business logic
│   ├── ICrudService.cs
│   └── CrudService.cs
├── Endpoints/                  # API endpoints
│   ├── ClienteEndpoints.cs
│   ├── MayoristaEndpoints.cs
│   └── ...
├── Program.cs                  # Entry point
└── appsettings.json           # Configuration
```

## Error Handling

All errors return consistent JSON format:

```json
{
  "success": false,
  "error": "NotFound",
  "message": "Resource not found"
}
```

| Error Type | HTTP Code | Description |
|------------|-----------|-------------|
| ValidationError | 400 | Invalid request data |
| NotFound | 404 | Resource doesn't exist |
| BadRequest | 400 | Business logic error |
| InternalError | 500 | Server error |

## Development

### Build

```bash
dotnet build
```

### Clean and Rebuild

```bash
dotnet clean && dotnet build
```

### Check for outdated packages

```bash
dotnet list package --outdated
```

## Database Tables

| Table | Description |
|-------|-------------|
| clientes | Client companies |
| mayoristas | Wholesaler companies |
| operadores | System operators |
| catalogo_productos | Product catalog |
| solicitudes_cotizacion | Quote requests |
| cotizaciones_downlabs | Generated quotes |
| pedidos_creditos | Credit orders |

## Security Notes

> [!WARNING]
> This project is for educational/demonstration purposes.
>
> - Passwords stored in plain text
> - No JWT authentication
> - No input validation
>
> For production: add BCrypt, JWT, input validation, and HTTPS.

## Related Projects

- [Frontend Astro](https://github.com/your-org/astro-webMinoristas) - Frontend client

## License

MIT License - See LICENSE file for details.