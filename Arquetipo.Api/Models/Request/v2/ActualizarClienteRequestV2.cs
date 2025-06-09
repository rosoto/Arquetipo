using System.ComponentModel.DataAnnotations;

namespace Arquetipo.Api.Models.Request.v2
{
    public class ActualizarClienteRequestV2
    {
        [Required]
        public int? Id { get; set; }

        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        [EmailAddress(ErrorMessage = "El campo Email no contiene un formato válido.")]
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        [MaxLength(50)]
        public string? PreferenciaContacto { get; set; }
        public string? EstadoCivil { get; set; }
    }
}
