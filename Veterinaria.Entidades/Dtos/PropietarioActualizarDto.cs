using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Entidades.Dtos;

// DTO de entrada para PUT /api/propietarios/{id}.
// No incluye el DNI: identifica al propietario y no se modifica.
public class PropietarioActualizarDto
{
    [Required(ErrorMessage = "Los nombres son obligatorios")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "Los nombres deben tener entre 2 y 60 caracteres")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener entre 2 y 60 caracteres")]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [StringLength(15, MinimumLength = 6, ErrorMessage = "El teléfono debe tener entre 6 y 15 caracteres")]
    public string Telefono { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido")]
    [StringLength(100)]
    public string? Correo { get; set; }

    [StringLength(150)]
    public string? Direccion { get; set; }
}
