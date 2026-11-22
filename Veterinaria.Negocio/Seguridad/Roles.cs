namespace Veterinaria.Negocio.Seguridad;

// Roles de la clínica. Se usan en el servicio (rol por defecto) y en [Authorize(Roles = ...)].
public static class Roles
{
    // Personal de apoyo o practicante: solo consulta
    public const string Asistente = "Asistente";

    // Atiende el mostrador: registra y edita propietarios y mascotas
    public const string Recepcionista = "Recepcionista";

    // Actualiza la ficha clínica de la mascota y da de baja mascotas
    public const string Veterinario = "Veterinario";

    // Rol con el que nace toda cuenta creada por la API; otro rol solo lo da la clínica (UPDATE en la BD)
    public const string PorDefecto = Asistente;
}
