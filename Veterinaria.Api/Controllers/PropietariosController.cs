using Microsoft.AspNetCore.Mvc;
using Veterinaria.Entidades.Dtos;
using Veterinaria.Negocio.Excepciones;
using Veterinaria.Negocio.Servicios;

namespace Veterinaria.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PropietariosController(
    IPropietarioServicio servicio,
    IMascotaServicio mascotas) : ControllerBase
{
    // GET api/propietarios?busqueda=ram
    [HttpGet]
    [ProducesResponseType<List<PropietarioDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PropietarioDto>>> Listar([FromQuery] string? busqueda)
    {
        return Ok(await servicio.ListarAsync(busqueda));
    }

    // GET api/propietarios/5
    [HttpGet("{id:int}")]
    [ProducesResponseType<PropietarioDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PropietarioDto>> ObtenerPorId(int id)
    {
        var propietario = await servicio.ObtenerPorIdAsync(id);
        return propietario is null ? NotFound() : Ok(propietario);
    }

    // GET api/propietarios/5/mascotas  (ruta anidada)
    [HttpGet("{id:int}/mascotas")]
    [ProducesResponseType<List<MascotaDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<MascotaDto>>> ListarMascotas(int id)
    {
        var lista = await mascotas.ListarPorPropietarioAsync(id);
        return lista is null ? NotFound() : Ok(lista);
    }

    // POST api/propietarios
    [HttpPost]
    [ProducesResponseType<PropietarioDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PropietarioDto>> Crear(PropietarioCrearDto dto)
    {
        try
        {
            var creado = await servicio.CrearAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.PropietarioId }, creado);
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new ProblemDetails { Title = "Regla de negocio", Detail = ex.Message, Status = 409 });
        }
    }

    // PUT api/propietarios/5
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, PropietarioActualizarDto dto)
    {
        return await servicio.ActualizarAsync(id, dto) ? NoContent() : NotFound();
    }

    // DELETE api/propietarios/5
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            return await servicio.EliminarAsync(id) ? NoContent() : NotFound();
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new ProblemDetails { Title = "Regla de negocio", Detail = ex.Message, Status = 409 });
        }
    }
}
