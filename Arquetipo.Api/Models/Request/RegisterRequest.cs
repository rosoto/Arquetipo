using System.ComponentModel.DataAnnotations;

namespace Arquetipo.Api.Models.Request
{
    public class RegisterRequest
    {
        [Required]
        [MinLength(3)]
        public required string NombreUsuario { get; set; }
        [Required]
        [MinLength(6)] // Ejemplo de validación de longitud mínima
        public required string Password { get; set; }
        public string? Roles { get; set; } // Opcional, ej. "User" o "Admin,User"
    }
}
