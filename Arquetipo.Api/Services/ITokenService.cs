using Arquetipo.Api.Models.Response;

namespace Arquetipo.Api.Services
{
    public interface ITokenService
    {
        string GenerarToken(Usuario usuario);
    }
}
