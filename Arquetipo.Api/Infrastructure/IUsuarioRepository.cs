using Arquetipo.Api.Models.Response;

namespace Arquetipo.Api.Infrastructure
{
    public interface IUsuarioRepository
    {
        Task<Usuario> GetUsuarioPorNombreAsync(string nombreUsuario);
        Task<Usuario> AddUsuarioAsync(Usuario usuario, string plainPassword);
    }
}