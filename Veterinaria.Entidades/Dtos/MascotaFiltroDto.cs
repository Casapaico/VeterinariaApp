namespace Veterinaria.Entidades.Dtos;

// Filtros opcionales de GET /api/mascotas (llegan por query string)
public class MascotaFiltroDto
{
    public string? Nombre { get; set; }
    public string? Especie { get; set; }
    public string? Raza { get; set; }
    public int? PropietarioId { get; set; }
    public decimal? PesoMin { get; set; }
    public decimal? PesoMax { get; set; }
}
