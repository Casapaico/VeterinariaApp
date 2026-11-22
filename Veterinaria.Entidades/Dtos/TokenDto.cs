namespace Veterinaria.Entidades.Dtos;

// DTO de salida del login: el token y cuándo vence (UTC)
public class TokenDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expira { get; set; }
}
