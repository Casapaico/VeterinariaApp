using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// GATEWAY: único punto de entrada. El cliente solo conoce la dirección del gateway;
// las rutas hacia cada microservicio están en ocelot.json.
// En la nube (ASPNETCORE_ENVIRONMENT=Production) ocelot.Production.json reemplaza los hosts
// locales por los hosts públicos de cada servicio (https, puerto 443).
builder.Configuration
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// El gateway valida el token ANTES de reenviar la solicitud, con la misma clave, emisor y audiencia
// que Veterinaria.Api. "Bearer" es el nombre que ocelot.json usa en AuthenticationProviderKeys.
var jwt = builder.Configuration.GetSection("Jwt");
var clave = jwt["Clave"];
if (string.IsNullOrEmpty(clave) || clave.Length < 32)
    throw new InvalidOperationException("Falta Jwt:Clave (mínimo 32 caracteres): configúrala con user-secrets o con la variable Jwt__Clave");

builder.Services.AddAuthentication()
    .AddJwtBearer("Bearer", options => options.TokenValidationParameters = new TokenValidationParameters
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

builder.Services.AddOcelot();

var app = builder.Build();

await app.UseOcelot();      // el middleware de Ocelot autentica, enruta y reenvía

app.Run();
