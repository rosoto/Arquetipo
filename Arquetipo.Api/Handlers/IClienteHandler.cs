using Arquetipo.Api.Models.Request.v1;
using Arquetipo.Api.Models.Request.v2;
using Arquetipo.Api.Models.Response.v1;
using Arquetipo.Api.Models.Response.v2;

namespace Arquetipo.Api.Handlers;

public interface IClienteHandler
{
    //V1
    Task<DataClienteResponse> GetClientesV1Async(int page, int pageSize);
    Task<DataClienteResponse> GetClienteByIdV1Async(int? id);
    Task<DataClienteResponse> GetClientesByAnyV1Async(BuscarClienteRequest query);
    Task PostClientesV1Async(List<CrearClienteRequestV1> clientes);
    Task<bool> UpdateClienteV1Async(ActualizarClienteRequest cliente);
    Task<bool> DeleteClienteV1Async(int? idCliente);

    //V2
    Task<DataClienteResponseV2> GetClientesV2Async(int page, int pageSize, bool? soloActivos);
    Task<DataClienteResponseV2> GetClienteByIdV2Async(int? id);
    Task<DataClienteResponseV2> GetClientesByAnyV2Async(BuscarClienteRequestV2 query);
    Task PostClientesV2Async(List<CrearClienteRequestV2> clientes);
    Task<bool> UpdateClienteV2Async(ActualizarClienteRequestV2 cliente);

}