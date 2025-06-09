using Arquetipo.Api.Infrastructure;
using Arquetipo.Api.Models.Request;
using Arquetipo.Api.Models.Request.v1;
using Arquetipo.Api.Models.Request.v2;
using Arquetipo.Api.Models.Response.v1;
using Arquetipo.Api.Models.Response.v2;
using AutoMapper;

namespace Arquetipo.Api.Handlers;

/// <summary>
/// Manejador para las operaciones de negocio relacionadas con los Clientes.
/// Orquesta las llamadas al repositorio y mapea los resultados a los DTOs de respuesta.
/// </summary>
public class ClienteHandler(IClienteRepository clienteRepository, ILogger<ClienteHandler> logger, IMapper mapper) : IClienteHandler
{
    private readonly IClienteRepository _clienteRepository = clienteRepository;
    private readonly ILogger<ClienteHandler> _logger = logger;
    private readonly IMapper _mapper = mapper;

    #region V1
    public async Task<DataClienteResponse> GetClientesV1Async(int page, int pageSize)
    {
        var result = new DataClienteResponse { Data = [] };
        var repoClientes = await _clienteRepository.GetAllAsync(page, pageSize);
        if (repoClientes != null && repoClientes.Count != 0)
        {
            result.Data = _mapper.Map<List<ClienteResponse>>(repoClientes);
        }
        return result;
    }

    public async Task<DataClienteResponse> GetClienteByIdV1Async(int? id)
    {
        var result = new DataClienteResponse { Data = [] };
        var repoCliente = await _clienteRepository.GetByIdAsync(id);
        if (repoCliente is not null)
        {
            result.Data.Add(_mapper.Map<ClienteResponse>(repoCliente));
        }
        return result;
    }

    public async Task<DataClienteResponse> GetClientesByAnyV1Async(BuscarClienteRequest queryV1)
    {
        _logger.LogInformation("Handler V1: Buscando clientes con query: {@QueryV1}", queryV1);
        var response = new DataClienteResponse { Data = [] };

        var setClienteAnyParaRepo = _mapper.Map<SetClienteAny>(queryV1);
        var repoClientes = await _clienteRepository.GetByAnyAsync(setClienteAnyParaRepo);

        if (repoClientes != null && repoClientes.Count != 0)
        {
            response.Data = _mapper.Map<List<ClienteResponse>>(repoClientes);
            _logger.LogInformation("Handler V1: Se encontraron {Count} clientes para la consulta.", response.Data.Count);
        }
        else
        {
            _logger.LogInformation("Handler V1: No se encontraron clientes para la consulta.");
        }
        return response;
    }

    public async Task PostClientesV1Async(List<CrearClienteRequestV1> clientes)
    {
        var repoClientes = _mapper.Map<List<SetCliente>>(clientes);
        if (repoClientes.Count != 0)
        {
            await _clienteRepository.AddClientesAsync(repoClientes);
        }
    }

    public async Task<bool> UpdateClienteV1Async(ActualizarClienteRequest cliente)
    {
        if (cliente?.Id == null || !await _clienteRepository.ExistsAsync(cliente.Id))
        {
            _logger.LogWarning("Handler V1: Cliente no existe o ID es nulo para actualizar. ID: {ClienteId}", cliente?.Id);
            return false;
        }

        var repoUpdateDto = _mapper.Map<SetCliente>(cliente);
        await _clienteRepository.UpdateAsync(repoUpdateDto);
        return true;
    }

    public async Task<bool> DeleteClienteV1Async(int? idCliente)
    {
        if (idCliente == null || !await _clienteRepository.ExistsAsync(idCliente))
        {
            _logger.LogWarning("Handler V1: Intento de eliminar un cliente que no existe o con ID nulo: {ClienteId}", idCliente);
            return false;
        }
        await _clienteRepository.DeleteAsync(idCliente);
        return true;
    }
    #endregion

    #region V2
    public async Task<DataClienteResponseV2> GetClientesV2Async(int page, int pageSize, bool? soloActivos)
    {
        _logger.LogInformation("Handler: GetClientesV2Async - soloActivos: {SoloActivos}", soloActivos);
        var result = new DataClienteResponseV2 { Data = [] };
        var repoClientes = await _clienteRepository.GetAllAsync(page, pageSize);

        if (repoClientes != null && repoClientes.Count != 0)
        {
            result.Data = _mapper.Map<List<ClienteResponseV2>>(repoClientes);
        }
        return result;
    }

    public async Task<DataClienteResponseV2> GetClienteByIdV2Async(int? id)
    {
        var result = new DataClienteResponseV2 { Data = [] };
        var repoCliente = await _clienteRepository.GetByIdAsync(id);
        if (repoCliente is not null)
        {
            result.Data.Add(_mapper.Map<ClienteResponseV2>(repoCliente));
        }
        return result;
    }

    public async Task<DataClienteResponseV2> GetClientesByAnyV2Async(BuscarClienteRequestV2 query)
    {
        _logger.LogInformation("Handler V2: Buscando clientes con query: {@QueryV2}", query);
        var result = new DataClienteResponseV2 { Data = [] };

        var setClienteAnyParaRepo = _mapper.Map<SetClienteAny>(query);
        var repoClientes = await _clienteRepository.GetByAnyAsync(setClienteAnyParaRepo);

        if (repoClientes != null && repoClientes.Count != 0)
        {
            result.Data = _mapper.Map<List<ClienteResponseV2>>(repoClientes);
            _logger.LogInformation("Handler V2: Se encontraron {Count} clientes para la consulta.", result.Data.Count);
        }
        else
        {
            _logger.LogInformation("Handler V2: No se encontraron clientes para la consulta.");
        }
        return result;
    }

    public async Task PostClientesV2Async(List<CrearClienteRequestV2> clientes)
    {
        var repoSetClienteList = _mapper.Map<List<SetCliente>>(clientes);

        if (repoSetClienteList.Any())
        {
            _logger.LogInformation("Handler V2: Creando {Count} clientes.", repoSetClienteList.Count);
            await _clienteRepository.AddClientesAsync(repoSetClienteList);
        }
        else
        {
            _logger.LogWarning("Handler V2: No hay clientes válidos para crear después del mapeo.");
        }
    }

    public async Task<bool> UpdateClienteV2Async(ActualizarClienteRequestV2 clienteUpdateReq)
    {
        if (clienteUpdateReq?.Id == null)
        {
            _logger.LogWarning("Handler V2: Solicitud de actualización de cliente V2 inválida o ID nulo.");
            return false;
        }

        var entidadCliente = await _clienteRepository.GetByIdAsync(clienteUpdateReq.Id.Value);
        if (entidadCliente == null)
        {
            _logger.LogWarning("Handler V2: Cliente con ID {Id} no encontrado para actualizar.", clienteUpdateReq.Id);
            return false;
        }

        // AutoMapper aplica los cambios del request a la entidad existente
        _mapper.Map(clienteUpdateReq, entidadCliente);

        // Mapeamos la entidad ya actualizada al DTO que espera el repositorio
        var repoUpdateDto = _mapper.Map<SetCliente>(entidadCliente);

        await _clienteRepository.UpdateAsync(repoUpdateDto);
        return true;
    }
    #endregion
}