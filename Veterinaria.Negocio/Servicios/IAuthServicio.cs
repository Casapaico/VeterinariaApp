using Veterinaria.Entidades.Dtos;

namespace Veterinaria.Negocio.Servicios;

public interface IAuthServicio
{
    Task<UsuarioDto> RegistrarAsync(RegistroDto dto);
    Task<TokenDto?> LoginAsync(LoginDto dto);   // null = credenciales no válidas
}
