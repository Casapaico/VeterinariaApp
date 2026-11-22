using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Entidades.Dtos;

// DTO de entrada para POST /api/auth/registro.
// No tiene rol: el rol lo asigna el servidor, nunca el cliente.
public class RegistroDto
{
    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido")]
    [StringLength(100)]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La clave es obligatoria")]
    [StringLength(50, MinimumLength = 8, ErrorMessage = "La clave debe tener entre 8 y 50 caracteres")]
    public string Clave { get; set; } = string.Empty;
}
