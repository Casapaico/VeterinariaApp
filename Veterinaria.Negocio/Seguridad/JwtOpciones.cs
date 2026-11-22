namespace Veterinaria.Negocio.Seguridad;

// Sección "Jwt" de la configuración. Se usa para GENERAR el token (AuthServicio)
// y para VALIDARLO (Program.cs). La clave no está en appsettings.json: va en user-secrets.
public class JwtOpciones
{
    public string Clave { get; set; } = string.Empty;      // firma HMAC-SHA256 (mínimo 32 caracteres)
    public string Emisor { get; set; } = string.Empty;     // quién emite el token
    public string Audiencia { get; set; } = string.Empty;  // para quién es el token
    public int Minutos { get; set; }                       // vida del token
}
