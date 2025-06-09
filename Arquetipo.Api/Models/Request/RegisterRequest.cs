 using System.ComponentModel.DataAnnotations;

namespace Arquetipo.Api.Models.Request
{
    public class RegisterRequest
    {
        [Required]
        [MinLength(3)]
        public required string NombreUsuario { get; set; }
        [Required]
        [MinLength(6)]
        public required string Password { get; set; }
        public string? Roles { get; set; }
    }
}
