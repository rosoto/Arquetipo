using System.ComponentModel.DataAnnotations;

namespace Arquetipo.Api.Models.Request.v2
{
    public class ActualizarClienteRequestV2
    {
        [Required]
        public int? Id { get; set; } // El ID sigue siendo requerido para identificar al cliente

        public string? Nombre { get; set; } // Campos opcionales para actualización parcial
        public string? Apellido { get; set; }
        [EmailAddress(ErrorMessage = "El campo Email no contiene un formato válido.")]
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        [MaxLength(50)]
        public string? PreferenciaContacto { get; set; } // Actualizable
        public string? EstadoCivil { get; set; } // Actualizable
    }
}
