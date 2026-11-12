using Veterinaria.Entidades;
using Veterinaria.Entidades.Dtos;

namespace Veterinaria.Datos.Repositorios;

public interface IMascotaRepositorio
{
    Task<List<Mascota>> ListarAsync(MascotaFiltroDto filtro);
    Task<List<Mascota>> ListarPorPropietarioAsync(int propietarioId);
    Task<Mascota?> ObtenerPorIdAsync(int id);
    Task<int> ContarPorPropietarioAsync(int propietarioId);
    Task AgregarAsync(Mascota mascota);
    Task GuardarCambiosAsync();
    Task EliminarAsync(Mascota mascota);
}
