using AutoMapper;
using Veterinaria.Datos.Repositorios;
using Veterinaria.Entidades;
using Veterinaria.Entidades.Dtos;
using Veterinaria.Negocio.Excepciones;

namespace Veterinaria.Negocio.Servicios;

public class PropietarioServicio(IPropietarioRepositorio repositorio, IMapper mapper) : IPropietarioServicio
{
    public async Task<List<PropietarioDto>> ListarAsync(string? busqueda)
    {
        var propietarios = await repositorio.ListarAsync(busqueda);
        return mapper.Map<List<PropietarioDto>>(propietarios);
    }

    public async Task<PropietarioDto?> ObtenerPorIdAsync(int id)
    {
        var propietario = await repositorio.ObtenerPorIdAsync(id);
        return propietario is null ? null : mapper.Map<PropietarioDto>(propietario);
    }

    public async Task<PropietarioDto> CrearAsync(PropietarioCrearDto dto)
    {
        // Regla de negocio 1: el DNI no se repite
        if (await repositorio.ExisteDniAsync(dto.Dni.Trim()))
            throw new ReglaNegocioException($"Ya existe un propietario con el DNI {dto.Dni.Trim()}");

        var propietario = mapper.Map<Propietario>(dto);
        await repositorio.AgregarAsync(propietario);

        return mapper.Map<PropietarioDto>(propietario);
    }

    public async Task<bool> ActualizarAsync(int id, PropietarioActualizarDto dto)
    {
        var propietario = await repositorio.ObtenerPorIdAsync(id);
        if (propietario is null) return false;

        // Copia los valores del DTO sobre la entidad que EF ya está siguiendo
        mapper.Map(dto, propietario);
        await repositorio.GuardarCambiosAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var propietario = await repositorio.ObtenerPorIdAsync(id);
        if (propietario is null) return false;

        // Regla de negocio 2: no se elimina un propietario con mascotas registradas
        if (await repositorio.TieneMascotasAsync(id))
            throw new ReglaNegocioException(
                $"No se puede eliminar a {propietario.Nombres} {propietario.Apellidos}: tiene mascotas registradas");

        await repositorio.EliminarAsync(propietario);
        return true;
    }
}
