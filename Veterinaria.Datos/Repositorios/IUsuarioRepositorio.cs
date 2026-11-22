using Veterinaria.Entidades;

namespace Veterinaria.Datos.Repositorios;

public interface IUsuarioRepositorio
{
    Task<Usuario?> BuscarPorCorreoAsync(string correo);
    Task AgregarAsync(Usuario usuario);
    Task GuardarCambiosAsync();
}
