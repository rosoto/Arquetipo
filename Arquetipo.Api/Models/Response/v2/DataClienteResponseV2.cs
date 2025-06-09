using Arquetipo.Api.Models.Header;

namespace Arquetipo.Api.Models.Response.v2
{
    public class DataClienteResponseV2 : HeaderBase
    {
        public required List<ClienteResponseV2> Data { get; set; }
    }
}
