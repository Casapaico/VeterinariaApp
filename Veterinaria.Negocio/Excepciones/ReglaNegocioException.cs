namespace Veterinaria.Negocio.Excepciones;

// Se lanza cuando una operación incumple una regla de negocio.
// El controlador la convierte en 409 Conflict.
public class ReglaNegocioException(string mensaje) : Exception(mensaje);
