# VeterinariaApp

Laboratorio Semana 13: Web API REST en capas con DTOs y AutoMapper (ASP.NET Core .NET 10 + Entity Framework Core + SQL Server).

API para una clínica veterinaria: registra a los propietarios y a sus mascotas, permite buscar mascotas por especie, raza, peso o dueño y aplica las reglas de la clínica (DNI único, máximo de mascotas por propietario y no borrar a un propietario que aún tiene mascotas).

## Arquitectura

```
Veterinaria.Api  ──►  Veterinaria.Negocio  ──►  Veterinaria.Datos  ──►  Veterinaria.Entidades
(controladores,       (servicios, reglas,       (DbContext,             (entidades y DTOs)
 Program.cs)           MappingProfile)           repositorios)
```

```
VeterinariaApp/
├── db/                       VeterinariaDB.sql (tablas + datos de ejemplo)
├── scripts/                  cargar-bd.sh · scaffold.sh · comandos.txt
├── Veterinaria.Entidades/    Propietario · Mascota · Dtos/
├── Veterinaria.Datos/        VeterinariaContext · Repositorios/
├── Veterinaria.Negocio/      Servicios/ · Mapeos/MappingProfile · Excepciones/
└── Veterinaria.Api/          Controllers/ · Program.cs · Veterinaria.Api.http
```

El controlador solo conoce las interfaces de los servicios y los DTOs; nunca el DbContext ni las entidades.

## Modelo de datos

- **Propietario** (1) ── (N) **Mascota**: `Mascotas.PropietarioId` es la clave foránea; `Mascota.Propietario` y `Propietario.Mascotas` son las propiedades de navegación.

## Endpoints

| Método | Ruta | Respuestas |
|---|---|---|
| GET | `/api/propietarios?busqueda=` | 200 |
| GET | `/api/propietarios/{id}` | 200, 404 |
| GET | `/api/propietarios/{id}/mascotas` | 200, 404 |
| POST | `/api/propietarios` | 201, 400, 409 |
| PUT | `/api/propietarios/{id}` | 204, 400, 404 |
| DELETE | `/api/propietarios/{id}` | 204, 404, 409 |
| GET | `/api/mascotas?nombre=&especie=&raza=&propietarioId=&pesoMin=&pesoMax=` | 200 |
| GET | `/api/mascotas/{id}` | 200, 404 |
| POST | `/api/mascotas` | 201, 400, 409 |
| PUT | `/api/mascotas/{id}` | 204, 400, 404, 409 |
| DELETE | `/api/mascotas/{id}` | 204, 404 |

## Reglas de negocio (409 Conflict)

Viven en la capa Negocio y lanzan `ReglaNegocioException`; el controlador la convierte en 409.

1. El DNI del propietario no se repite.
2. No se elimina un propietario que tiene mascotas registradas.
3. Un propietario tiene como máximo 5 mascotas (al registrar una mascota o al cambiarla de dueño).

## Cómo ejecutarlo

```bash
./scripts/cargar-bd.sh                      # crea VeterinariaDB (pide SA_PASSWORD en scripts/.env)
cd Veterinaria.Api
dotnet user-secrets set "ConnectionStrings:Veterinaria" "Server=localhost,1433;Database=VeterinariaDB;User Id=sa;Password=...;TrustServerCertificate=True"
dotnet run --launch-profile http            # http://localhost:5130/swagger
```

La cadena de conexión no se guarda en el repositorio. Las pruebas de todos los endpoints están en `Veterinaria.Api/Veterinaria.Api.http`.
