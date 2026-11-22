using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Veterinaria.Entidades.Dtos;
using Veterinaria.Negocio.Excepciones;
using Veterinaria.Negocio.Seguridad;
using Veterinaria.Negocio.Servicios;

namespace Veterinaria.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]     // S14: por defecto, todos los endpoints exigen un token válido (401 sin él)
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class MascotasController(IMascotaServicio servicio) : ControllerBase
{
    // GET api/mascotas?especie=Perro&pesoMin=10&pesoMax=30&propietarioId=1&raza=&nombre=
    [HttpGet]
    [ProducesResponseType<List<MascotaDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MascotaDto>>> Listar([FromQuery] MascotaFiltroDto filtro)
    {
        return Ok(await servicio.ListarAsync(filtro));
    }

    // GET api/mascotas/5
    [HttpGet("{id:int}")]
    [ProducesResponseType<MascotaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MascotaDto>> ObtenerPorId(int id)
    {
        var mascota = await servicio.ObtenerPorIdAsync(id);
        return mascota is null ? NotFound() : Ok(mascota);
    }

    // POST api/mascotas
    [Authorize(Roles = Roles.Recepcionista)]   // registra la mascota al llegar a la clínica
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [HttpPost]
    [ProducesResponseType<MascotaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MascotaDto>> Crear(MascotaCrearDto dto)
    {
        try
        {
            var creada = await servicio.CrearAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.MascotaId }, creada);
        }
        catch (DatoInvalidoException ex)
        {
            ModelState.AddModelError(ex.Campo, ex.Message);
            return ValidationProblem(ModelState);
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new ProblemDetails { Title = "Regla de negocio", Detail = ex.Message, Status = 409 });
        }
    }

    // PUT api/mascotas/5
    // Varios roles separados por coma = cualquiera de ellos
    [Authorize(Roles = Roles.Recepcionista + "," + Roles.Veterinario)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(int id, MascotaActualizarDto dto)
    {
        try
        {
            return await servicio.ActualizarAsync(id, dto) ? NoContent() : NotFound();
        }
        catch (DatoInvalidoException ex)
        {
            ModelState.AddModelError(ex.Campo, ex.Message);
            return ValidationProblem(ModelState);
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new ProblemDetails { Title = "Regla de negocio", Detail = ex.Message, Status = 409 });
        }
    }

    // DELETE api/mascotas/5
    [Authorize(Roles = Roles.Veterinario)]     // la baja de una mascota la decide el veterinario
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id)
    {
        return await servicio.EliminarAsync(id) ? NoContent() : NotFound();
    }
}
