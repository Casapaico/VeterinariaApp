namespace Veterinaria.Entidades;

// Entidad de la tabla Usuarios (S14: autenticación JWT)
public partial class Usuario
{
    public int UsuarioId { get; set; }

    public string Correo { get; set; } = null!;

    // Nunca se guarda la contraseña: solo su hash BCrypt
    public string ClaveHash { get; set; } = null!;

    // Asistente, Recepcionista o Veterinario
    public string Rol { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }
}
