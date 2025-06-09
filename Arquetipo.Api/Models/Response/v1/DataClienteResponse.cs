using Arquetipo.Api.Models.Header;

namespace Arquetipo.Api.Models.Response.v1
{
    public class DataClienteResponse : HeaderBase
    {
        public required List<ClienteResponse> Data { get; set; }
    }
}