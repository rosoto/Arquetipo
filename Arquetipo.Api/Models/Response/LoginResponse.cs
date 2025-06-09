namespace Arquetipo.Api.Models.Response
{
    public class LoginResponse
    {
        public required string Token { get; set; }
        public DateTime Expiration { get; set; }
        public required string NombreUsuario { get; set; }
    }
}
