namespace Veterinaria.Negocio.Excepciones;

// Se lanza cuando un dato de entrada es válido en forma pero no existe en la base
// (por ejemplo, un PropietarioId que no está registrado).
// El controlador la convierte en 400 Bad Request (ValidationProblem).
public class DatoInvalidoException(string campo, string mensaje) : Exception(mensaje)
{
    public string Campo { get; } = campo;
}
