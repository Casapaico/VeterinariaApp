# VeterinariaApp

Web API REST en capas para una clínica veterinaria (ASP.NET Core .NET 10 + Entity Framework Core + SQL Server).

- **Semana 13:** capas, DTOs y AutoMapper.
- **Semana 14:** autenticación con JWT, contraseñas con hash BCrypt y permisos por rol.
- **Semana 15:** microservicios con un API Gateway (Ocelot) y despliegue en la nube. Ver [Semana 15](#semana-15-microservicios-con-ocelot).

La API registra a los propietarios y a sus mascotas, permite buscar mascotas por especie, raza, peso o dueño y aplica las reglas de la clínica (DNI único, máximo de mascotas por propietario, no borrar a un propietario que aún tiene mascotas y correo de usuario único).

## Roles de la clínica

| Rol | Quién es | Qué puede hacer |
|---|---|---|
| **Asistente** | Personal de apoyo o practicante | Solo consulta propietarios y mascotas. Es el rol con el que nace toda cuenta registrada por la API. |
| **Recepcionista** | Atiende el mostrador | Registra, edita y elimina propietarios; registra y edita mascotas. |
| **Veterinario** | Atiende a las mascotas | Actualiza los datos de la mascota (peso, raza, etc.) y la da de baja. |

El cliente nunca elige el rol: el registro siempre crea un Asistente y la clínica asciende a la persona con un `UPDATE` en la base de datos.

## Matriz de permisos

| Endpoint | Sin token | Asistente | Recepcionista | Veterinario |
|---|---|---|---|---|
| `POST /api/auth/registro` | Sí | Sí | Sí | Sí |
| `POST /api/auth/login` | Sí | Sí | Sí | Sí |
| `GET /api/auth/perfil` | No (401) | Sí | Sí | Sí |
| `GET /api/propietarios`, `/{id}`, `/{id}/mascotas` | No (401) | Sí | Sí | Sí |
| `POST /api/propietarios` | No (401) | No (403) | Sí | No (403) |
| `PUT /api/propietarios/{id}` | No (401) | No (403) | Sí | No (403) |
| `DELETE /api/propietarios/{id}` | No (401) | No (403) | Sí | No (403) |
| `GET /api/mascotas`, `/{id}` | No (401) | Sí | Sí | Sí |
| `POST /api/mascotas` | No (401) | No (403) | Sí | No (403) |
| `PUT /api/mascotas/{id}` | No (401) | No (403) | Sí | Sí |
| `DELETE /api/mascotas/{id}` | No (401) | No (403) | No (403) | Sí |

- **Público** (`[AllowAnonymous]`): registro y login.
- **Solo autenticado** (`[Authorize]` en el controlador): perfil y todas las consultas GET.
- **Rol específico** (`[Authorize(Roles = ...)]`): las escrituras.

## Arquitectura

```
Veterinaria.Api  ──►  Veterinaria.Negocio  ──►  Veterinaria.Datos  ──►  Veterinaria.Entidades
(controladores,       (servicios, reglas,       (DbContext,             (entidades y DTOs)
 Program.cs)           MappingProfile, JWT)      repositorios)
```

| Capa | Piezas de la S14 |
|---|---|
| `Veterinaria.Entidades` | `Usuario`, `LoginDto`, `RegistroDto`, `TokenDto`, `UsuarioDto`, `PerfilDto` |
| `Veterinaria.Datos` | `IUsuarioRepositorio` / `UsuarioRepositorio` y `DbSet<Usuario>` en el contexto |
| `Veterinaria.Negocio` | `AuthServicio` (hash BCrypt y generación del token), `Seguridad/JwtOpciones`, `Seguridad/Roles` |
| `Veterinaria.Api` | `AuthController`, JwtBearer y Swagger con Authorize en `Program.cs`, `[Authorize]` en los controladores |

El token lleva los claims `sub` (id del usuario), `email`, `role` y `jti`, y vence a los 60 minutos.

## Cómo ejecutarlo

```bash
# 1. Base de datos (en este orden)
sqlcmd -C -S localhost,1433 -U sa -i db/VeterinariaDB.sql
sqlcmd -C -S localhost,1433 -U sa -i db/Usuarios.sql

# 2. Secretos (no están en el repositorio)
cd Veterinaria.Api
dotnet user-secrets set "ConnectionStrings:Veterinaria" "Server=localhost,1433;Database=VeterinariaDB;User Id=sa;Password=...;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Clave" "una-clave-aleatoria-de-al-menos-32-caracteres"

# 3. Ejecutar
dotnet run --launch-profile http            # http://localhost:5130/swagger
```

`appsettings.json` solo guarda el emisor, la audiencia y los minutos de vida del token. Si falta `Jwt:Clave` o tiene menos de 32 caracteres, la API no arranca.

Usuarios de prueba (clave `Vet2026*`): `asistente@vetarequipa.com`, `recepcion@vetarequipa.com` y `veterinario@vetarequipa.com`.

## Probar

1. `POST /api/auth/login` y copia el `token`.
2. En Swagger, botón **Authorize**: pega el token (sin la palabra Bearer).
3. Ejecuta los endpoints protegidos.

Todas las pruebas están en `Veterinaria.Api/Veterinaria.Api.http`, con una variable de token por rol.

## Endpoints y respuestas

| Método | Ruta | Respuestas |
|---|---|---|
| POST | `/api/auth/registro` | 201, 400, 409 |
| POST | `/api/auth/login` | 200, 400, 401 |
| GET | `/api/auth/perfil` | 200, 401 |
| GET | `/api/propietarios?busqueda=` | 200, 401 |
| GET | `/api/propietarios/{id}` | 200, 401, 404 |
| GET | `/api/propietarios/{id}/mascotas` | 200, 401, 404 |
| POST | `/api/propietarios` | 201, 400, 401, 403, 409 |
| PUT | `/api/propietarios/{id}` | 204, 400, 401, 403, 404 |
| DELETE | `/api/propietarios/{id}` | 204, 401, 403, 404, 409 |
| GET | `/api/mascotas?nombre=&especie=&raza=&propietarioId=&pesoMin=&pesoMax=` | 200, 401 |
| GET | `/api/mascotas/{id}` | 200, 401, 404 |
| POST | `/api/mascotas` | 201, 400, 401, 403, 409 |
| PUT | `/api/mascotas/{id}` | 204, 400, 401, 403, 404, 409 |
| DELETE | `/api/mascotas/{id}` | 204, 401, 403, 404 |

## Reglas de negocio (409 Conflict)

Viven en la capa Negocio y lanzan `ReglaNegocioException`; el controlador la convierte en 409.

1. El DNI del propietario no se repite.
2. No se elimina un propietario que tiene mascotas registradas.
3. Un propietario tiene como máximo 5 mascotas (al registrar una mascota o al cambiarla de dueño).
4. El correo del usuario no se repite (sin distinguir mayúsculas).

## Semana 15: microservicios con Ocelot

El cliente solo conoce la dirección del **gateway**; el gateway enruta hacia dos microservicios independientes.

```
                       ┌──────────────────────────────┐
 Cliente  ──HTTP──►    │ GatewayApi (Ocelot)          │ ──► Veterinaria.Api  (S14, 4 capas, JWT, BD)
 (Postman, .http)      │ valida JWT + enruta          │ ──► VacunasApi       (datos en memoria, JWT)
                       └──────────────────────────────┘
```

| Proyecto | Puerto local | Nube (Render) | Qué es |
|---|---|---|---|
| `GatewayApi` | 5100 | https://vetarequipa-gateway.onrender.com | Único punto de entrada: rutas en `ocelot.json` y validación del token |
| `Veterinaria.Api` | 5130 | https://vetarequipa-api.onrender.com | La API en capas con JWT de la S14, sin cambios en su lógica |
| `VacunasApi` | 5140 | https://vetarequipa-vacunas.onrender.com | Catálogo de vacunas de la clínica (en memoria); también valida el token |

Los tres comparten solo la configuración `Jwt` (misma clave, emisor `Veterinaria.Api` y audiencia `Veterinaria.Clientes`). No comparten base de datos ni clases.

### Rutas del gateway

| Upstream (cliente → gateway) | Método | Downstream local | Downstream nube | Token en el gateway |
|---|---|---|---|---|
| `/gateway/auth/{everything}` | POST | `localhost:5130/api/auth/{everything}` | `vetarequipa-api.onrender.com:443` | No |
| `/gateway/propietarios{everything}` | GET | `localhost:5130/api/propietarios{everything}` | `vetarequipa-api.onrender.com:443` | No (la API sí lo pide) |
| `/gateway/propietarios{everything}` | POST, PUT, DELETE | `localhost:5130/api/propietarios{everything}` | `vetarequipa-api.onrender.com:443` | Sí |
| `/gateway/mascotas{everything}` | GET | `localhost:5130/api/mascotas{everything}` | `vetarequipa-api.onrender.com:443` | No (la API sí lo pide) |
| `/gateway/mascotas{everything}` | POST, PUT, DELETE | `localhost:5130/api/mascotas{everything}` | `vetarequipa-api.onrender.com:443` | Sí |
| `/gateway/vacunas{everything}` | GET | `localhost:5140/api/vacunas{everything}` | `vetarequipa-vacunas.onrender.com:443` | Sí |

- `{everything}` pegado al recurso cubre `/gateway/mascotas`, `/gateway/mascotas/3` y `/gateway/mascotas?especie=Gato`.
- En la S14 todas las consultas ya exigían token: en el gateway los GET son públicos, pero la API sigue respondiendo 401 sin token porque su lógica no se cambió.
- `ocelot.json` apunta a `localhost`; en la nube (`ASPNETCORE_ENVIRONMENT=Production`) se carga además `ocelot.Production.json`, que reemplaza los hosts por los públicos con `https` y puerto 443.

### Ejecutar en local

```bash
# La clave JWT debe ser la MISMA en los tres proyectos (user-secrets, no está en el repo)
dotnet user-secrets set "Jwt:Clave" "<la-misma-clave>" --project GatewayApi
dotnet user-secrets set "Jwt:Clave" "<la-misma-clave>" --project VacunasApi

# Tres terminales
dotnet run --project Veterinaria.Api --launch-profile http
dotnet run --project VacunasApi
dotnet run --project GatewayApi
```

Pruebas: `Gateway.http` (REST Client) o `./scripts/probar-gateway.sh [url-del-gateway]`.

| Caso | Respuesta | Quién responde |
|---|---|---|
| Login, GET con token | 200 | API / VacunasApi |
| Crear propietario con rol Recepcionista | 201 | API |
| Actualizar o eliminar propietario | 204 | API |
| Escritura o vacunas sin token, o token alterado | 401 | Gateway |
| GET de la API sin token | 401 | API |
| Crear propietario con rol Asistente, eliminar mascota con rol Recepcionista | 403 | API |
| Id inexistente | 404 | API / VacunasApi |
| Ruta o método que no está en `ocelot.json` | 404 | Gateway |
| VacunasApi detenido | 502 | Gateway |
| VacunasApi directo sin token | 401 | VacunasApi |

### Despliegue

- **Hosting:** Render, plan gratuito, con Docker (`render.yaml` crea los tres servicios como Blueprint).
- **Base de datos:** Neon (PostgreSQL serverless, plan gratuito). Script: `db/postgres/VeterinariaDB-postgres.sql`.
- La API elige el proveedor con `BaseDatos__Proveedor` (`SqlServer` por defecto, `Postgres` en la nube); solo cambia el `UseSqlServer`/`UseNpgsql`.
- Variables de entorno en Render (no están en el repositorio): `Jwt__Clave` en los tres servicios y `ConnectionStrings__Veterinaria` en la API.
