# VeterinariaApp

Web API REST en capas para una clínica veterinaria (ASP.NET Core .NET 10 + Entity Framework Core + SQL Server).

- **Semana 13:** capas, DTOs y AutoMapper.
- **Semana 14:** autenticación con JWT, contraseñas con hash BCrypt y permisos por rol.

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
