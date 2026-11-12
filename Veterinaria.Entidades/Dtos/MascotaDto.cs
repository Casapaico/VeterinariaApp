namespace Veterinaria.Entidades.Dtos;

// DTO de lectura: es lo que la API devuelve, nunca la entidad Mascota.
public class MascotaDto
{
    public int MascotaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public string? Raza { get; set; }
    public string Sexo { get; set; } = string.Empty;
    public DateOnly? FechaNacimiento { get; set; }
    public decimal PesoKg { get; set; }
    public int PropietarioId { get; set; }

    // Nombre completo del propietario en lugar del objeto Propietario (ForMember)
    public string Propietario { get; set; } = string.Empty;
}
