using Arquetipo.Api.Infrastructure;
using Arquetipo.Api.Infrastructure.Persistence;
using Arquetipo.Api.Models.Response;
using Microsoft.EntityFrameworkCore;

public class UsuarioRepository(ArquetipoDbContext context, ILogger<UsuarioRepository> logger) : IUsuarioRepository
{
    private readonly ArquetipoDbContext _context = context;
    private readonly ILogger<UsuarioRepository> _logger = logger;

    public async Task<Usuario> GetUsuarioPorNombreAsync(string nombreUsuario)
    {
        if (string.IsNullOrEmpty(nombreUsuario)) return null;
        return await _context.Usuarios
                             .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
    }

    public async Task<Usuario> AddUsuarioAsync(Usuario usuario, string plainPassword)
    {
        if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == usuario.NombreUsuario))
        {
            _logger.LogWarning("Intento de registrar usuario con nombre de usuario ya existente: {NombreUsuario}", usuario.NombreUsuario);
            throw new InvalidOperationException("El nombre de usuario ya existe.");
        }

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Usuario {NombreUsuario} registrado exitosamente.", usuario.NombreUsuario);
        return usuario;
    }
}