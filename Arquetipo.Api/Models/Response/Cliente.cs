namespace Arquetipo.Api.Models.Response;

public class Cliente
{
    /// <summary>
    /// Id de Cliente
    /// </summary>
    /// <example>1234</example>
    public int Id { get; set; }
    /// <summary>
    /// Nombre de Cliente
    /// </summary>
    /// <example>Juan Pablo</example>
    public required string Nombre { get; set; }
    /// <summary>
    /// Apellidos de Cliente
    /// </summary>
    /// <example>Soto Rojas</example>
    public required string Apellido { get; set; }
    /// <summary>
    /// Mail del Cliente
    /// </summary>
    /// <example>sotorojas@mail.com</example>
    public required string Email { get; set; }
    /// <summary>
    /// Telefono del Cliente
    /// </summary>
    /// <example>457892</example>
    public required string Telefono { get; set; }
}