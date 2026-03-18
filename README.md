# DownLabs.Core.Api

API REST para gestión de clientes con Supabase.

## Estructura del Proyecto

```
DownLabs.Core.Api/
├── Controllers/           # Controladores de la API (futuro)
├── Models/                # Modelos de datos
│   └── Cliente.cs         # Modelo de cliente
├── Services/              # Servicios de negocio
│   ├── ISupabaseService.cs
│   └── SupabaseService.cs
├── Program.cs             # Punto de entrada y endpoints
├── appsettings.json       # Configuración local (NO subir a Git)
├── appsettings.json.example # Plantilla de configuración
├── .gitignore             # Archivos ignorados por Git
└── DownLabs.Core.Api.csproj
```

## Configuración

### 1. Configurar credenciales de Supabase

Después de clonar el repositorio, copia el archivo de configuración de ejemplo:

```bash
cp appsettings.json.example appsettings.json
```

Luego edita `appsettings.json` con tus credenciales de Supabase:

```json
{
  "Supabase": {
    "Url": "https://tu-proyecto.supabase.co",
    "Key": "tu-anon-key"
  }
}
```

### 2. Credenciales de Supabase

1. Ir a [Supabase Dashboard](https://supabase.com/dashboard)
2. Seleccionar proyecto
3. Settings > API
4. Copiar **Project URL** y **anon public key**

## Comandos

### Iniciar la aplicación
```bash
dotnet run
```
La app estará disponible en: `http://localhost:5145`

### Detener la aplicación
Presionar `Ctrl + C` en la terminal donde está corriendo.

### Compilar
```bash
dotnet build
```

---

## Configuración CORS

El backend está configurado para permitir peticiones desde el frontend Astro.

**Origen permitido:** `http://localhost:4321`

Esta configuración está en `Program.cs`:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAstro", policy =>
    {
        policy.WithOrigins("http://localhost:4321")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

---

## Endpoints

### 1. Verificar conexión a Supabase

**GET** `/api/supabase/test`

Verifica que la conexión a Supabase funcione correctamente.

**Respuesta exitosa:**
```json
{
  "success": true,
  "message": "✅ Conexión exitosa a Supabase. Status: OK (OK)"
}
```

---

### 2. Registrar cliente

**POST** `/api/clientes/register`

Registra un nuevo cliente en la base de datos.

**Body (JSON):**
```json
{
  "nombre_empresa": "Mi Empresa",
  "correo_contacto": "correo@ejemplo.com",
  "contrasena": "password123"
}
```

**Respuesta exitosa:**
```json
{
  "success": true,
  "message": "Cliente registrado exitosamente"
}
```

---

### 3. Login de cliente

**GET** `/api/clientes/login`

Busca un cliente por correo y contraseña.

**URL (con query parameters):**
```
GET /api/clientes/login?correo_contacto=correo@ejemplo.com&contrasena=password123
```

**Respuesta exitosa:**
```json
{
  "success": true,
  "cliente": {
    "id_cliente": "uuid-del-cliente",
    "nombre_empresa": "Mi Empresa"
  }
}
```

**Respuesta fallida:**
```json
{
  "success": false,
  "message": "Credenciales invalidas"
}
```

---

## Pruebas con ThunderClient

### Registro (POST)
```
Method: POST
URL: http://localhost:5145/api/clientes/register
Headers:
  Content-Type: application/json
Body:
{
  "nombre_empresa": "Empresa Test",
  "correo_contacto": "test@test.com",
  "contrasena": "password123"
}
```

### Login (GET)
```
Method: GET
URL: http://localhost:5145/api/clientes/login?correo_contacto=test@test.com&contrasena=password123
```

---

## Tabla en Supabase

### Tabla: `clientes`

| Columna | Tipo | Descripción |
|---------|------|-------------|
| id_cliente | uuid | ID único del cliente (PK) |
| nombre_empresa | text | Nombre de la empresa |
| historial_compras | jsonb | Historial de compras |
| calificacion_crediticia | numeric | Calificación crediticia |
| created_at | timestamp | Fecha de creación |
| updated_at | timestamp | Fecha de actualización |
| correo_contacto | text | Correo de contacto |
| telefono_contacto | text | Teléfono de contacto |
| rfc | varchar | RFC fiscal |
| codigo_postal_fiscal | varchar | Código postal fiscal |
| contrasena | text | Contraseña (texto plano) |

---

## Notas de Seguridad

> ⚠️ **Advertencia**: Este proyecto es para propósitos educativos/prueba.
> 
> - Las contraseñas se guardan en **texto plano**
> - No hay autenticación con tokens/JWT
> - No hay validaciones robustas
>
> Para producción, implementar:
> - Hash de contraseñas (BCrypt)
> - Autenticación JWT
> - Validación de datos
> - HTTPS obligatorio

---

## Requisitos

- .NET 10.0 SDK
- Supabase (cuenta y proyecto)

---

## Conexión con Frontend

Este backend está diseñado para trabajar con el frontend Astro.

### Endpoints disponibles para el frontend:

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/clientes/register` | Registrar nuevo cliente |
| GET | `/api/clientes/login` | Iniciar sesión (query params) |
| GET | `/api/supabase/test` | Verificar conexión |

### Cómo correr ambos proyectos:

**Terminal 1 - Backend:**
```bash
cd DownLabs.Core.Api
dotnet run
# Disponible en http://localhost:5145
```

**Terminal 2 - Frontend:**
```bash
cd astro-webMinoristas
pnpm dev
# Disponible en http://localhost:4321
```

### Puerto del Backend

Si el puerto 5145 está en uso, puedes cambiarlo:
```bash
dotnet run --urls="http://localhost:5000"
```

No olvides actualizar la URL en el frontend (`src/components/sections/Header.astro`) si cambias el puerto.

---

## Proyecto Relacionado

- **Frontend Astro:** `../astro-webMinoristas`

---

## Notas sobre Git

### Archivos ignorados

El archivo `.gitignore` está configurado para ignorar:
- `appsettings.json` (contiene credenciales)
- `bin/` y `obj/` (build artifacts)
- `.vs/` y `.vscode/` (configuración IDE)
- `*.log` (archivos de log)

### Después de clonar

```bash
# 1. Clonar el repositorio
git clone https://github.com/tu-usuario/tu-repo.git
cd DownLabs.Core.Api

# 2. Copiar configuración de ejemplo
cp appsettings.json.example appsettings.json

# 3. Agregar tus credenciales
# Editar appsettings.json con tu URL y key de Supabase

# 4. Restaurar dependencias y correr
dotnet restore
dotnet run
```

