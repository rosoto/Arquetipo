namespace Arquetipo.Api.Models.Response.v2
{
    public class ClienteResponseV2
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public required string Apellido { get; set; }
        public required string Email { get; set; }

        [Obsolete("Este campo será discontinuado en futuras versiones. Utilice la lista de contactos del cliente.")]
        public string? Telefono { get; set; }

        public string? EstadoCivil { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
