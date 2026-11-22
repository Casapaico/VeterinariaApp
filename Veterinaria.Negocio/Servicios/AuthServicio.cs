using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using Veterinaria.Datos.Repositorios;
using Veterinaria.Entidades;
using Veterinaria.Entidades.Dtos;
using Veterinaria.Negocio.Excepciones;
using Veterinaria.Negocio.Seguridad;

namespace Veterinaria.Negocio.Servicios;

// No conoce HTTP: devuelve null o lanza una excepción y el controlador elige el código (401, 409)
public class AuthServicio(IUsuarioRepositorio repositorio, JwtOpciones jwt, IMapper mapper) : IAuthServicio
{
    public async Task<UsuarioDto> RegistrarAsync(RegistroDto dto)
    {
        var correo = Normalizar(dto.Correo);

        // Regla de negocio: el correo no se repite
        if (await repositorio.BuscarPorCorreoAsync(correo) is not null)
            throw new ReglaNegocioException($"Ya existe un usuario con el correo {correo}");

        var usuario = new Usuario
        {
            Correo = correo,
            ClaveHash = BCrypt.Net.BCrypt.HashPassword(dto.Clave),  // se guarda el hash, nunca la clave
            Rol = Roles.PorDefecto                                  // el rol lo decide el servidor
        };
        await repositorio.AgregarAsync(usuario);
        await repositorio.GuardarCambiosAsync();

        return mapper.Map<UsuarioDto>(usuario);
    }

    public async Task<TokenDto?> LoginAsync(LoginDto dto)
    {
        var usuario = await repositorio.BuscarPorCorreoAsync(Normalizar(dto.Correo));

        // Mismo resultado si el correo no existe o si la clave no coincide:
        // así no se revela qué correos están registrados
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Clave, usuario.ClaveHash))
            return null;

        return GenerarToken(usuario);
    }

    private TokenDto GenerarToken(Usuario usuario)
    {
        // Claims mínimos: quién es (sub), su correo y su rol. Nada sensible: el token se puede leer.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.UsuarioId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Correo),
            new Claim(ClaimTypes.Role, usuario.Rol),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Clave));
        var expira = DateTime.UtcNow.AddMinutes(jwt.Minutos);

        var token = new JwtSecurityToken(
            issuer: jwt.Emisor,
            audience: jwt.Audiencia,
            claims: claims,
            expires: expira,
            signingCredentials: new SigningCredentials(clave, SecurityAlgorithms.HmacSha256));

        return new TokenDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expira = expira
        };
    }

    private static string Normalizar(string correo) => correo.Trim().ToLowerInvariant();
}
