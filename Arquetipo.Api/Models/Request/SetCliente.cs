using System.ComponentModel.DataAnnotations;

namespace Arquetipo.Api.Models.Request
{
    public class SetCliente
    {
        public int? Id { get; set; }
        public required string Nombre { get; set; }
        public required string Apellido { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "El campo Email no contiene un formato válido.")]
        public required string Email { get; set; }

        public required string Telefono { get; set; }
    }
}
