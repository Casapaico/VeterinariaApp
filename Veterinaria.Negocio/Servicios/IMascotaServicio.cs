using Veterinaria.Entidades.Dtos;

namespace Veterinaria.Negocio.Servicios;

public interface IMascotaServicio
{
    Task<List<MascotaDto>> ListarAsync(MascotaFiltroDto filtro);

    // Devuelve null si el propietario no existe (para responder 404)
    Task<List<MascotaDto>?> ListarPorPropietarioAsync(int propietarioId);

    Task<MascotaDto?> ObtenerPorIdAsync(int id);
    Task<MascotaDto> CrearAsync(MascotaCrearDto dto);
    Task<bool> ActualizarAsync(int id, MascotaActualizarDto dto);
    Task<bool> EliminarAsync(int id);
}
