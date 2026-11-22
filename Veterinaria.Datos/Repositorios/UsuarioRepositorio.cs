using Microsoft.EntityFrameworkCore;
using Veterinaria.Entidades;

namespace Veterinaria.Datos.Repositorios;

public class UsuarioRepositorio(VeterinariaContext context) : IUsuarioRepositorio
{
    public async Task<Usuario?> BuscarPorCorreoAsync(string correo) =>
        await context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Correo == correo);

    public async Task AgregarAsync(Usuario usuario) =>
        await context.Usuarios.AddAsync(usuario);

    public async Task GuardarCambiosAsync() => await context.SaveChangesAsync();
}
