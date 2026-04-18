# Changelog

All notable changes to this project will be documented in this file.

## [1.2.0] - 2026-04-18

### Autenticación con Supabase Auth

- **Sistema de Registro**: Endpoint `/api/auth/register` para crear usuarios en auth.users
- **Sistema de Login**: Endpoint `/api/auth/login` que retorna access_token y rol
- **Migración SQL**: Script para integrar auth.users con tablas públicas
- **Trigger Automático**: `handle_new_user()` crea registro automático en tabla correspondiente
- **FK Users**: Relación de clientes, mayoristas y operadores con auth.users

### Archivos Nuevos

- `01_migracion_auth.sql`: Script de migración para Supabase
- `pruebas/api.http`: Archivo de pruebas completo
- `Endpoints/AuthEndpoints.cs`: Endpoints de autenticación

### API Mejorada

- Auth: register, login
- Clientes: CRUD completo
- Mayoristas: CRUD completo
- Operadores: CRUD completo
- Productos: CRUD completo
- Solicitudes: CRUD completo
- Cotizaciones: CRUD completo
- Pedidos: CRUD completo

---

## [1.1.0] - 2026-04-18

### Nuevas Funcionalidades

- **Datos de Prueba Completos**: Base de datos populada con datos de prueba realistas
  - 29 clientes con contraseñas
  - 17 mayoristas con contraseñas
  - 16 operadores con contraseñas
  - 44 productos del catálogo con variaciones
  - 22 solicitudes de cotización
  - 21 cotizaciones Downlabs
  - 21 pedidos de crédito

### Mejoras

- **Modelos Actualizados**: Modelos C# actualizados para coincidir con esquema de Supabase
  - Operador: columnas id, nombre, apellido, email, telefono, activo, contrasena_hash
  - SolicitudCotizacion: nivel_urgencia (Normal/Urgente), estado_solicitud (Pendiente/Cotizada/Rechazada)
  - CotizacionDownlabs: estado (Negociando/Aceptada/Rechazada)
  - PedidoCredito: estado_pago (Pendiente/Pagado/En mora), tipo_pago, fechas de crédito

- **Endpoints Mejorados**: 
  - Valores por defecto correctos según constraints de base de datos
  - Búsqueda por descripcion en productos
  - Validaciones actualizadas para tipos no nullables

- **Catálogo Expandido**: 30 productos adicionales con variaciones
  - Diferentes colores y tamaños
  - Múltiples mayoristas por producto
  - Precios variables según especificaciones

### Correcciones

- Schema mismatch en Operador: corregido de id_operador, correo, rol → id, email, apellido, activo
- CHECK constraints descubiertos y manejados correctamente
- Tipos de datos corregidos ( Guid, decimal no/nullable)

---

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