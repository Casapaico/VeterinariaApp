namespace Veterinaria.Entidades.Dtos;

// DTO de salida del registro (201): nunca incluye la clave ni su hash
public class UsuarioDto
{
    public int UsuarioId { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}
