using Microsoft.EntityFrameworkCore;
using Veterinaria.Entidades;

namespace Veterinaria.Datos.Repositorios;

public class PropietarioRepositorio(VeterinariaContext context) : IPropietarioRepositorio
{
    public async Task<List<Propietario>> ListarAsync(string? busqueda)
    {
        var consulta = context.Propietarios
            .AsNoTracking()
            .Include(p => p.Mascotas)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
            consulta = consulta.Where(p => p.Nombres.Contains(busqueda)
                                        || p.Apellidos.Contains(busqueda)
                                        || p.Dni.StartsWith(busqueda));

        return await consulta
            .OrderBy(p => p.Apellidos)
            .ThenBy(p => p.Nombres)
            .ToListAsync();
    }

    // Con seguimiento: el servicio puede modificar la entidad y luego guardar
    public async Task<Propietario?> ObtenerPorIdAsync(int id) =>
        await context.Propietarios
            .Include(p => p.Mascotas)
            .FirstOrDefaultAsync(p => p.PropietarioId == id);

    public async Task<bool> ExisteAsync(int id) =>
        await context.Propietarios.AnyAsync(p => p.PropietarioId == id);

    public async Task<bool> ExisteDniAsync(string dni) =>
        await context.Propietarios.AnyAsync(p => p.Dni == dni);

    public async Task<bool> TieneMascotasAsync(int id) =>
        await context.Mascotas.AnyAsync(m => m.PropietarioId == id);

    public async Task AgregarAsync(Propietario propietario)
    {
        context.Propietarios.Add(propietario);
        await context.SaveChangesAsync();
    }

    public async Task GuardarCambiosAsync() => await context.SaveChangesAsync();

    public async Task EliminarAsync(Propietario propietario)
    {
        context.Propietarios.Remove(propietario);
        await context.SaveChangesAsync();
    }
}
