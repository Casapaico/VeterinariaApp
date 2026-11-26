using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// MICROSERVICIO 2: VacunasApi. Catálogo de vacunas de la clínica.
// Es independiente de Veterinaria.Api: su propio proyecto, su propio puerto y sus propios datos
// (en memoria, sin base de datos). No comparte clases ni base de datos con la API de la S14.

// Valida el token JWT por su cuenta, con la MISMA clave, emisor y audiencia que Veterinaria.Api.
// La clave no está en el repositorio: en local va en user-secrets y en la nube en una variable de entorno (Jwt__Clave).
var jwt = builder.Configuration.GetSection("Jwt");
var clave = jwt["Clave"];
if (string.IsNullOrEmpty(clave) || clave.Length < 32)
    throw new InvalidOperationException("Falta Jwt:Clave (mínimo 32 caracteres): configúrala con user-secrets o con la variable Jwt__Clave");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwt["Emisor"],
        ValidateAudience = true,
        ValidAudience = jwt["Audiencia"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave))
    });
builder.Services.AddAuthorization();

// Datos en memoria: vacunas que aplica la clínica
var vacunas = new List<Vacuna>
{
    new(1, "Séxtuple canina",        "Perro", "Moquillo, parvovirus, hepatitis, leptospirosis, parainfluenza, coronavirus", 6, 12, 65.00m),
    new(2, "Antirrábica",            "Perro", "Rabia",                                                         12, 12, 35.00m),
    new(3, "Bordetella (tos de las perreras)", "Perro", "Traqueobronquitis infecciosa",                        8, 12, 55.00m),
    new(4, "Triple felina",          "Gato",  "Panleucopenia, rinotraqueítis, calicivirus",                    8, 12, 60.00m),
    new(5, "Leucemia felina",        "Gato",  "Virus de la leucemia felina (FeLV)",                            9, 12, 70.00m),
    new(6, "Antirrábica felina",     "Gato",  "Rabia",                                                         12, 12, 35.00m),
    new(7, "Mixomatosis y VHD",      "Conejo","Mixomatosis y enfermedad hemorrágica vírica",                   10, 6,  80.00m)
};

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Todos los endpoints exigen token (401 sin él)
var api = app.MapGroup("/api/vacunas").RequireAuthorization();

// GET /api/vacunas            lista completa
// GET /api/vacunas?especie=Gato  filtro opcional por especie
api.MapGet("/", (string? especie) =>
    string.IsNullOrWhiteSpace(especie)
        ? vacunas
        : vacunas.Where(v => v.Especie.Equals(especie, StringComparison.OrdinalIgnoreCase)).ToList());

// GET /api/vacunas/2          una vacuna (404 si no existe)
api.MapGet("/{id:int}", (int id) =>
    vacunas.FirstOrDefault(v => v.Id == id) is { } vacuna
        ? Results.Ok(vacuna)
        : Results.NotFound(new { mensaje = $"No existe la vacuna {id}" }));

app.Run();

// Modelo propio del microservicio (no se comparte con la API de la S14)
record Vacuna(int Id, string Nombre, string Especie, string Previene,
              int EdadMinimaSemanas, int RefuerzoMeses, decimal Precio);
