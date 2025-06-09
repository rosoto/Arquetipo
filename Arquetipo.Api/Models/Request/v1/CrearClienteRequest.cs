using System.ComponentModel.DataAnnotations;

namespace Arquetipo.Api.Models.Request.v1
{
    public class CrearClienteRequestV1
    {
        [Required]
        public required string Nombre { get; set; }
        [Required]
        public required string Apellido { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "El campo Email no contiene un formato válido.")]
        public required string Email { get; set; }
        [Required]
        public required string Telefono { get; set; }
    }
}
