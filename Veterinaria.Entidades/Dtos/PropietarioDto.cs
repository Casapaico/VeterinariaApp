namespace Veterinaria.Entidades.Dtos;

// DTO de lectura: es lo que la API devuelve, nunca la entidad Propietario.
public class PropietarioDto
{
    public int PropietarioId { get; set; }
    public string Dni { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Correo { get; set; }
    public string? Direccion { get; set; }
    public DateTime FechaRegistro { get; set; }

    // Se calcula en el MappingProfile a partir de la colección Mascotas
    public int CantidadMascotas { get; set; }
}
