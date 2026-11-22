using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Veterinaria.Entidades.Dtos;
using Veterinaria.Negocio.Excepciones;
using Veterinaria.Negocio.Servicios;

namespace Veterinaria.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController(IAuthServicio servicio) : ControllerBase
{
    // POST api/auth/registro  (público: nadie tiene token todavía)
    [AllowAnonymous]
    [HttpPost("registro")]
    [ProducesResponseType<UsuarioDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioDto>> Registro(RegistroDto dto)
    {
        try
        {
            var creado = await servicio.RegistrarAsync(dto);
            return StatusCode(StatusCodes.Status201Created, creado);
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new ProblemDetails { Title = "Regla de negocio", Detail = ex.Message, Status = 409 });
        }
    }

    // POST api/auth/login  (público)
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<TokenDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenDto>> Login(LoginDto dto)
    {
        var token = await servicio.LoginAsync(dto);

        // Mismo 401 si el correo no existe o si la clave es incorrecta
        return token is null
            ? Unauthorized(new ProblemDetails { Title = "No autorizado", Detail = "Correo o clave incorrectos", Status = 401 })
            : Ok(token);
    }

    // GET api/auth/perfil  (cualquier usuario autenticado)
    // Los datos salen de los claims del token ya validado, no de la base de datos
    [Authorize]
    [HttpGet("perfil")]
    [ProducesResponseType<PerfilDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<PerfilDto> Perfil()
    {
        var perfil = new PerfilDto
        {
            Correo = User.Claims.FirstOrDefault(c => c.Type is ClaimTypes.Email or JwtRegisteredClaimNames.Email)?.Value ?? "",
            Rol = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? ""
        };
        return Ok(perfil);
    }
}
