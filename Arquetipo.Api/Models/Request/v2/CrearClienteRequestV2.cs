using System.ComponentModel.DataAnnotations;

namespace Arquetipo.Api.Models.Request.v2;

public class CrearClienteRequestV2
{
    [Required]
    public required string Nombre { get; set; }
    [Required]
    public required string Apellido { get; set; }
    [Required]
    [EmailAddress(ErrorMessage = "El campo Email no contiene un formato válido.")]
    public required string Email { get; set; }

    public string? Telefono { get; set; } // Telefono opcional en V2

    [MaxLength(50)]
    public string? PreferenciaContacto { get; set; } // Nuevo campo
}