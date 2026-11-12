using System;
using System.Collections.Generic;

namespace Veterinaria.Entidades;

public partial class Propietario
{
    public int PropietarioId { get; set; }

    public string Dni { get; set; } = null!;

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string? Correo { get; set; }

    public string? Direccion { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();
}
