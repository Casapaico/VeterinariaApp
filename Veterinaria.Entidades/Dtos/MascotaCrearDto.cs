using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Entidades.Dtos;

// DTO de entrada para POST /api/mascotas
public class MascotaCrearDto : IValidatableObject
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La especie es obligatoria")]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "La especie debe tener entre 2 y 30 caracteres")]
    public string Especie { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Raza { get; set; }

    [Required(ErrorMessage = "El sexo es obligatorio")]
    [RegularExpression("^[MHmh]$", ErrorMessage = "El sexo debe ser M (macho) o H (hembra)")]
    public string Sexo { get; set; } = string.Empty;

    public DateOnly? FechaNacimiento { get; set; }

    [Range(0.01, 200, ErrorMessage = "El peso debe estar entre 0.01 y 200 kg")]
    public decimal PesoKg { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Indica un propietario válido")]
    public int PropietarioId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FechaNacimiento > DateOnly.FromDateTime(DateTime.Today))
            yield return new ValidationResult("La fecha de nacimiento no puede ser futura", [nameof(FechaNacimiento)]);
    }
}
