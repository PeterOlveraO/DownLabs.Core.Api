# DownLabs.Core.Api

API RESTful para la plataforma DownLabs construida con .NET 9.0 y Supabase.

## Caracteristicas

- **.NET 9.0** con ASP.NET Core Web API
- **Supabase** como base de datos PostgreSQL
- **Supabase Auth** para autenticación de usuarios
- **Patron Minimal API** con endpoints organizados
- **Paginacion** automatica en todos los endpoints de lista
- **Filtrado** avanzado por diferentes campos
- **Async/Await** moderno con CancellationToken

## Requisitos Previos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Supabase](https://supabase.com) - cuenta y proyecto

## Configuracion

Crea `appsettings.json` en la raiz del proyecto:

```json
{
  "Logging": {
    "LogLevel": { "Default": "Information" }
  },
  "AllowedHosts": "*",
  "Supabase": {
    "Url": "https://tu-proyecto.supabase.co",
    "Key": "tu-clave-supabase"
  }
}
```

> [!IMPORTANT]
> Nunca hagas commit de `appsettings.json` - contiene credenciales sensibles.

## Ejecucion

```bash
dotnet run
```

La API estara disponible en `http://localhost:5145`

## Endpoints de API

| Recurso | URL Base | Descripcion |
|----------|----------|-------------|
| Auth | `/api/auth` | Registro y login de usuarios |
| Clientes | `/api/clientes` | Clientes registrados |
| Mayoristas | `/api/mayoristas` | Empresas mayoristas |
| Operadores | `/api/operadores` | Operadores del sistema |
| Productos | `/api/catalogo-productos` | Catalogo de productos |
| Solicitudes | `/api/solicitudes-cotizacion` | Solicitudes de cotizacion |
| Cotizaciones | `/api/cotizaciones` | Cotizaciones generadas |
| Pedidos | `/api/pedidos-credito` | Pedidos con credito |

### Autenticación

```bash
# Registrar usuario
POST /api/auth/register
{
  "email": "user@email.com",
  "password": "password123",
  "rol": "cliente",  # "cliente" | "mayorista" | "operador"
  "nombre_empresa": "Mi Empresa"  # opcional
}

# Iniciar sesión
POST /api/auth/login
{
  "email": "user@email.com",
  "password": "password123"
}
```

Respuesta login:
```json
{
  "success": true,
  "data": {
    "access_token": "...",
    "user_id": "...",
    "email": "user@email.com",
    "rol": "cliente"
  }
}
```

### Metodos HTTP Disponibles

| Metodo | Endpoint | Descripcion |
|--------|----------|-------------|
| GET | `/api/recurso` | Listar todos (paginado) |
| GET | `/api/recurso/{id}` | Obtener por ID |
| POST | `/api/recurso` | Crear nuevo |
| PUT | `/api/recurso/{id}` | Reemplazar |
| PATCH | `/api/recurso/{id}` | Actualizar parcialmente |
| DELETE | `/api/recurso/{id}` | Eliminar |

### Parametros de Consulta

Todos los endpoints GET soportan:

- `page` - Numero de pagina (default: 1)
- `pageSize` - Items por pagina (default: 20, max: 100)

### Ejemplo de Respuesta

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

## Estructura del Proyecto

```
DownLabs.Core.Api/
├── Models/                      # Modelos de datos
│   ├── Cliente.cs
│   ├── Mayorista.cs
│   ├── Operador.cs
│   ├── CatalogoProducto.cs
│   ├── SolicitudCotizacion.cs
│   ├── CotizacionDownlabs.cs
│   └── PedidoCredito.cs
├── Services/                   # Logica de negocio
│   ├── ICrudService.cs
│   └── CrudService.cs
├── Endpoints/                  # Endpoints de API
│   ├── ClienteEndpoints.cs
│   ├── MayoristaEndpoints.cs
│   └── ...
├── Program.cs                  # Punto de entrada
└── appsettings.json           # Configuracion
```

## Manejo de Errores

Todos los errores retornan formato JSON consistente:

```json
{
  "success": false,
  "error": "NotFound",
  "message": "Recurso no encontrado"
}
```

| Tipo de Error | Codigo HTTP | Descripcion |
|------------|-----------|-------------|
| ValidationError | 400 | Datos de solicitud invalidos |
| NotFound | 400 | El recurso no existe |
| BadRequest | 400 | Error de logica de negocio |
| InternalError | 500 | Error del servidor |

## Tablas de Base de Datos

| Tabla | Descripcion |
|-------|-------------|
| clientes | Empresas clientes |
| mayoristas | Empresas mayoristas |
| operadores | Operadores del sistema |
| catalogo_productos | Catalogo de productos |
| solicitudes_cotizacion | Solicitudes de cotizacion |
| cotizaciones_downlabs | Cotizaciones generadas |
| pedidos_creditos | Pedidos con credito |

## Notas de Seguridad

> [!WARNING]
> Este proyecto es solo para fines educativos/demostracion.
>
> - Contraseñas gestionadas por Supabase Auth
> - Tokens JWT proporcionados por Supabase
>
> Para producción: agregar validaciones adicionales y HTTPS.

## Construir

```bash
dotnet build
```

## Limpiar y Reconstruir

```bash
dotnet clean && dotnet build
```

## Paquetes Obsoletos

```bash
dotnet list package --outdated
```

## Tecnologia

- .NET 9.0
- ASP.NET Core 9.0
- Supabase 1.1.1
- PostgreSQL
- C# 12

## Ultimo Lanzamiento

### Version 1.2.0 (2026-04-18)

**Autenticación con Supabase Auth:**

- Sistema de registro (`/api/auth/register`)
- Sistema de login (`/api/auth/login`)
- Retorna access_token y rol del usuario
- Trigger automático crea registro en tabla correspondiente

**Archivos Nuevos:**

- `01_migracion_auth.sql`: Script de migración
- `pruebas/api.http`: Archivo de pruebas API
- `Endpoints/AuthEndpoints.cs`: Endpoint de autenticación

### Version 1.1.0 (2026-04-18)

- Datos de prueba completos en todas las tablas
- Modelos C# actualizados para esquema de Supabase