using System.ComponentModel.DataAnnotations;

namespace Arquetipo.Api.Models.Request
{
    public class LoginRequest
    {
        [Required]
        public string NombreUsuario { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
