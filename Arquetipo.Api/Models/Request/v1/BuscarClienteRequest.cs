namespace Arquetipo.Api.Models.Request.v1
{
    public class BuscarClienteRequest
    {
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
    }
}
