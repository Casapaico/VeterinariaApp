using System;
using System.Collections.Generic;

namespace Veterinaria.Entidades;

public partial class Mascota
{
    public int MascotaId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Especie { get; set; } = null!;

    public string? Raza { get; set; }

    public string Sexo { get; set; } = null!;

    public DateOnly? FechaNacimiento { get; set; }

    public decimal PesoKg { get; set; }

    public int PropietarioId { get; set; }

    public virtual Propietario Propietario { get; set; } = null!;
}
