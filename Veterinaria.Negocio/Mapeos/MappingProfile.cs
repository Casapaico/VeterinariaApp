using AutoMapper;
using Veterinaria.Entidades;
using Veterinaria.Entidades.Dtos;

namespace Veterinaria.Negocio.Mapeos;

// Único perfil de AutoMapper con todos los mapeos de la API
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ---------- Propietario ----------

        // Entidad -> DTO de lectura (lo que devuelve la API)
        CreateMap<Propietario, PropietarioDto>()
            .ForMember(d => d.CantidadMascotas, o => o.MapFrom(s => s.Mascotas.Count));

        // DTO de entrada -> Entidad (alta)
        CreateMap<PropietarioCrearDto, Propietario>()
            .ForMember(d => d.Dni, o => o.MapFrom(s => s.Dni.Trim()));

        // DTO de entrada -> Entidad existente (actualización con Map(origen, destino))
        CreateMap<PropietarioActualizarDto, Propietario>();

        // ---------- Mascota ----------

        // Entidad -> DTO de lectura: el nombre del propietario en lugar del objeto
        CreateMap<Mascota, MascotaDto>()
            .ForMember(d => d.Propietario,
                       o => o.MapFrom(s => s.Propietario.Nombres + " " + s.Propietario.Apellidos));

        // DTO de entrada -> Entidad (alta): el sexo se guarda siempre en mayúscula
        CreateMap<MascotaCrearDto, Mascota>()
            .ForMember(d => d.Sexo, o => o.MapFrom(s => s.Sexo.ToUpper()));

        // DTO de entrada -> Entidad existente (actualización con Map(origen, destino))
        CreateMap<MascotaActualizarDto, Mascota>()
            .ForMember(d => d.Sexo, o => o.MapFrom(s => s.Sexo.ToUpper()));
    }
}
