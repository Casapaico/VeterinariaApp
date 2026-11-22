using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Veterinaria.Datos;
using Veterinaria.Datos.Repositorios;
using Veterinaria.Negocio.Mapeos;
using Veterinaria.Negocio.Seguridad;
using Veterinaria.Negocio.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// OpenAPI con el esquema "Bearer": habilita el botón Authorize de Swagger
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Pega el token del login (sin la palabra Bearer)"
            }
        };
        return Task.CompletedTask;
    });

    // El candado solo aparece en los endpoints que piden token ([Authorize] sin [AllowAnonymous]);
    // así registro y login se ven públicos en Swagger, igual que en la matriz de permisos
    options.AddOperationTransformer((operation, context, ct) =>
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        if (metadata.OfType<IAuthorizeData>().Any() && !metadata.OfType<IAllowAnonymous>().Any())
        {
            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
            });
        }
        return Task.CompletedTask;
    });
});

// La cadena de conexión y la clave del token no están en el repositorio: en local van en user-secrets
// (dotnet user-secrets set "ConnectionStrings:Veterinaria" "..." / "Jwt:Clave" "...")
var cadena = builder.Configuration.GetConnectionString("Veterinaria")
    ?? throw new InvalidOperationException("Falta la cadena de conexión ConnectionStrings:Veterinaria");

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOpciones>()
    ?? throw new InvalidOperationException("Falta la sección Jwt en la configuración");
if (jwt.Clave.Length < 32)
    throw new InvalidOperationException("Jwt:Clave debe tener al menos 32 caracteres (configúrala con user-secrets)");

// Inyección de dependencias: DbContext, repositorios, servicios y AutoMapper
builder.Services.AddDbContext<VeterinariaContext>(options => options.UseSqlServer(cadena));

builder.Services.AddScoped<IPropietarioRepositorio, PropietarioRepositorio>();
builder.Services.AddScoped<IMascotaRepositorio, MascotaRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();     // S14

builder.Services.AddScoped<IPropietarioServicio, PropietarioServicio>();
builder.Services.AddScoped<IMascotaServicio, MascotaServicio>();
builder.Services.AddScoped<IAuthServicio, AuthServicio>();                 // S14

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

// JWT: cada solicitud trae el token en el encabezado Authorization: Bearer <token>
builder.Services.AddSingleton(jwt);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwt.Emisor,
        ValidateAudience = true,
        ValidAudience = jwt.Audiencia,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,          // el token vence exactamente a su hora (por defecto da 5 min de gracia)
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Clave))
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Veterinaria API v1");
        options.DocumentTitle = "Veterinaria API";
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();    // 1.º lee y valida el token: ¿quién eres?
app.UseAuthorization();     // 2.º aplica [Authorize] y los roles: ¿puedes hacerlo?

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();
