using Microsoft.EntityFrameworkCore;
using Veterinaria.Entidades;
using Veterinaria.Entidades.Dtos;

namespace Veterinaria.Datos.Repositorios;

public class MascotaRepositorio(VeterinariaContext context) : IMascotaRepositorio
{
    public async Task<List<Mascota>> ListarAsync(MascotaFiltroDto filtro)
    {
        var consulta = context.Mascotas
            .AsNoTracking()
            .Include(m => m.Propietario)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro.Nombre))
            consulta = consulta.Where(m => m.Nombre.Contains(filtro.Nombre));

        if (!string.IsNullOrWhiteSpace(filtro.Especie))
            consulta = consulta.Where(m => m.Especie == filtro.Especie);

        if (!string.IsNullOrWhiteSpace(filtro.Raza))
            consulta = consulta.Where(m => m.Raza != null && m.Raza.Contains(filtro.Raza));

        if (filtro.PropietarioId.HasValue)
            consulta = consulta.Where(m => m.PropietarioId == filtro.PropietarioId);

        if (filtro.PesoMin.HasValue)
            consulta = consulta.Where(m => m.PesoKg >= filtro.PesoMin);

        if (filtro.PesoMax.HasValue)
            consulta = consulta.Where(m => m.PesoKg <= filtro.PesoMax);

        return await consulta.OrderBy(m => m.Nombre).ToListAsync();
    }

    public async Task<List<Mascota>> ListarPorPropietarioAsync(int propietarioId) =>
        await context.Mascotas
            .AsNoTracking()
            .Include(m => m.Propietario)
            .Where(m => m.PropietarioId == propietarioId)
            .OrderBy(m => m.Nombre)
            .ToListAsync();

    // Con seguimiento: el servicio puede modificar la entidad y luego guardar
    public async Task<Mascota?> ObtenerPorIdAsync(int id) =>
        await context.Mascotas
            .Include(m => m.Propietario)
            .FirstOrDefaultAsync(m => m.MascotaId == id);

    public async Task<int> ContarPorPropietarioAsync(int propietarioId) =>
        await context.Mascotas.CountAsync(m => m.PropietarioId == propietarioId);

    public async Task AgregarAsync(Mascota mascota)
    {
        context.Mascotas.Add(mascota);
        await context.SaveChangesAsync();
    }

    public async Task GuardarCambiosAsync() => await context.SaveChangesAsync();

    public async Task EliminarAsync(Mascota mascota)
    {
        context.Mascotas.Remove(mascota);
        await context.SaveChangesAsync();
    }
}
