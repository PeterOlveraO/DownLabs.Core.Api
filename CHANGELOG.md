# Changelog

All notable changes to this project will be documented in this file.

## [1.0.0] - 2026-04-17

### Nuevas Funcionalidades

- **API REST Completa**: Implementación de endpoints CRUD para todas las tablas de la base de datos
- **Sistema de Paginación**: Todos los endpoints de lista soportan paginación automática con parámetros `page` y `pageSize`
- **Sistema de Filtros**: Filtrado avanzado por diferentes campos según cada recurso
- **Servicio Genérico CRUD**: Creación de `ICrudService` y `CrudService` reutilizables para todas las tablas
- **Endpoints para 7 Recursos**:
  - Clientes (`/api/clientes`)
  - Mayoristas (`/api/mayoristas`)
  - Operadores (`/api/operadores`)
  - Productos (`/api/catalogo-productos`)
  - Solicitudes de Cotización (`/api/solicitudes-cotizacion`)
  - Cotizaciones (`/api/cotizaciones`)
  - Pedidos de Crédito (`/api/pedidos-credito`)

### Mejoras

- **Patrón de Diseño**: Estructura organizada con Models, Services y Endpoints separados
- **Mejores Prácticas Async**: Implementación de CancellationToken y ConfigureAwait(false)
- **Manejo de Errores**: Formato consistente de errores con códigos (ValidationError, NotFound, BadRequest)
- **Documentación**: Creación de AGENTS.md con guías para desarrolladores
- **Validación de Entrada**: Validación de campos requeridos en endpoints POST

### Modelos Añadidos

- Mayorista
- Operador
- CatalogoProducto
- SolicitudCotizacion
- CotizacionDownlabs
- PedidoCredito

### Configuración

- CORS configurado para frontend Astro (http://localhost:4321)
- Configuración de Supabase en appsettings.json

---

## [0.1.0] - 2026-03-18

### Primer Lanzamiento

- **Proyecto Inicial**: Creación del backend .NET 9.0 con Supabase
- **Modelo Cliente**: Estructura de datos para clientes
- **Servicio de Supabase**: Conexión y operaciones con base de datos
- **Endpoints de Autenticación**:
  - Registro de clientes (`POST /api/clientes/register`)
  - Login de clientes (`GET /api/clientes/login`)
  - Test de conexión (`GET /api/supabase/test`)
- **Configuración**: Archivos de configuración para desarrollo y producción
- **Documentación**: README.md con guías de uso