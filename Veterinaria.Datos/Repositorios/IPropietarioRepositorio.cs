using Veterinaria.Entidades;

namespace Veterinaria.Datos.Repositorios;

public interface IPropietarioRepositorio
{
    Task<List<Propietario>> ListarAsync(string? busqueda);
    Task<Propietario?> ObtenerPorIdAsync(int id);
    Task<bool> ExisteAsync(int id);
    Task<bool> ExisteDniAsync(string dni);
    Task<bool> TieneMascotasAsync(int id);
    Task AgregarAsync(Propietario propietario);
    Task GuardarCambiosAsync();
    Task EliminarAsync(Propietario propietario);
}
