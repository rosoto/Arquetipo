using System.ComponentModel.DataAnnotations;

namespace Arquetipo.Api.Models.Request
{
    public class LoginRequest
    {
        [Required]
        public required string NombreUsuario { get; set; }
        [Required]
        public required string Password { get; set; }
    }
}
