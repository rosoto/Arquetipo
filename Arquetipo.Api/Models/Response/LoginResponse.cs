namespace Arquetipo.Api.Models.Response
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string NombreUsuario { get; set; }
    }
}
