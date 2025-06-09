using System.ComponentModel.DataAnnotations; //

namespace Arquetipo.Api.Models.Request.v1; //

public class ActualizarClienteRequest
{
    /// <summary>
    /// Id de Cliente.
    /// </summary>
    /// <example>8</example>
    [Required] //
    public int? Id { get; set; } //
    /// <summary>
    /// Nombre Cliente.
    /// </summary>
    /// <example>Mauricio</example>
    [Required] //
    public required string Nombre { get; set; } //
    /// <summary>
    /// Apellido Cliente.
    /// </summary>
    /// <example>Perez Tapia</example>
    [Required] //
    public required string Apellido { get; set; } //
    /// <summary>
    /// Mail
    /// </summary>
    /// <example>m.tapia@mail.cl</example>
    [Required] //
    [EmailAddress(ErrorMessage = "El campo Email no contiene un formato válido.")] //
    public required string Email { get; set; } //
    /// <summary>
    /// Telefono
    /// </summary>
    /// <example>975648375</example>
    [Required] //
    public required string Telefono { get; set; } //
}