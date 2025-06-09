namespace Arquetipo.Api.Models.Request.v2
{
    public class BuscarClienteRequestV2
    {
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Email { get; set; }
        public string? Nickname { get; set; }
    }
}
