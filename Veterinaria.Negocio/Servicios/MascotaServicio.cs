using AutoMapper;
using Veterinaria.Datos.Repositorios;
using Veterinaria.Entidades;
using Veterinaria.Entidades.Dtos;
using Veterinaria.Negocio.Excepciones;

namespace Veterinaria.Negocio.Servicios;

public class MascotaServicio(
    IMascotaRepositorio repositorio,
    IPropietarioRepositorio propietarios,
    IMapper mapper) : IMascotaServicio
{
    // Regla de negocio 3: un propietario puede tener como máximo 5 mascotas
    public const int MaximoMascotasPorPropietario = 5;

    public async Task<List<MascotaDto>> ListarAsync(MascotaFiltroDto filtro)
    {
        var mascotas = await repositorio.ListarAsync(filtro);
        return mapper.Map<List<MascotaDto>>(mascotas);
    }

    public async Task<List<MascotaDto>?> ListarPorPropietarioAsync(int propietarioId)
    {
        if (!await propietarios.ExisteAsync(propietarioId)) return null;

        var mascotas = await repositorio.ListarPorPropietarioAsync(propietarioId);
        return mapper.Map<List<MascotaDto>>(mascotas);
    }

    public async Task<MascotaDto?> ObtenerPorIdAsync(int id)
    {
        var mascota = await repositorio.ObtenerPorIdAsync(id);
        return mascota is null ? null : mapper.Map<MascotaDto>(mascota);
    }

    public async Task<MascotaDto> CrearAsync(MascotaCrearDto dto)
    {
        await ValidarPropietarioAsync(dto.PropietarioId);

        var mascota = mapper.Map<Mascota>(dto);
        await repositorio.AgregarAsync(mascota);

        // Se vuelve a leer con el propietario incluido para devolver su nombre
        var creada = await repositorio.ObtenerPorIdAsync(mascota.MascotaId);
        return mapper.Map<MascotaDto>(creada!);
    }

    public async Task<bool> ActualizarAsync(int id, MascotaActualizarDto dto)
    {
        var mascota = await repositorio.ObtenerPorIdAsync(id);
        if (mascota is null) return false;

        // Solo si cambia de dueño se valida el nuevo propietario
        if (dto.PropietarioId != mascota.PropietarioId)
            await ValidarPropietarioAsync(dto.PropietarioId);

        // Copia los valores del DTO sobre la entidad que EF ya está siguiendo
        mapper.Map(dto, mascota);
        await repositorio.GuardarCambiosAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var mascota = await repositorio.ObtenerPorIdAsync(id);
        if (mascota is null) return false;

        await repositorio.EliminarAsync(mascota);
        return true;
    }

    private async Task ValidarPropietarioAsync(int propietarioId)
    {
        if (!await propietarios.ExisteAsync(propietarioId))
            throw new DatoInvalidoException(nameof(MascotaCrearDto.PropietarioId),
                $"No existe un propietario con el id {propietarioId}");

        var cantidad = await repositorio.ContarPorPropietarioAsync(propietarioId);
        if (cantidad >= MaximoMascotasPorPropietario)
            throw new ReglaNegocioException(
                $"El propietario {propietarioId} ya tiene {cantidad} mascotas: el máximo es {MaximoMascotasPorPropietario}");
    }
}
