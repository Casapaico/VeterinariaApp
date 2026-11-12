using Microsoft.EntityFrameworkCore;
using Veterinaria.Datos;
using Veterinaria.Datos.Repositorios;
using Veterinaria.Negocio.Mapeos;
using Veterinaria.Negocio.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// La cadena de conexión no está en el repositorio: en local va en user-secrets
// (dotnet user-secrets set "ConnectionStrings:Veterinaria" "...")
var cadena = builder.Configuration.GetConnectionString("Veterinaria")
    ?? throw new InvalidOperationException("Falta la cadena de conexión ConnectionStrings:Veterinaria");

// Inyección de dependencias: DbContext, repositorios, servicios y AutoMapper
builder.Services.AddDbContext<VeterinariaContext>(options => options.UseSqlServer(cadena));

builder.Services.AddScoped<IPropietarioRepositorio, PropietarioRepositorio>();
builder.Services.AddScoped<IMascotaRepositorio, MascotaRepositorio>();

builder.Services.AddScoped<IPropietarioServicio, PropietarioServicio>();
builder.Services.AddScoped<IMascotaServicio, MascotaServicio>();

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

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

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();
