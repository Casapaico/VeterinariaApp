namespace Veterinaria.Entidades.Dtos;

// DTO de GET /api/auth/perfil: se arma con los claims del token, no con la base de datos
public class PerfilDto
{
    public string Correo { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}
