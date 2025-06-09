using Arquetipo.Api.Handlers;
using Arquetipo.Api.Models.Request.v1;
using Arquetipo.Api.Models.Request.v2;
using Arquetipo.Api.Models.Response.v1;
using Arquetipo.Api.Models.Response.v2;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Arquetipo.Api.Controllers
{

    [ApiController]
    [ApiVersion("1")]
    [ApiVersion("2")]
    [Route("api/v{version:apiVersion}/cliente")]
    public class ClienteController(IClienteHandler clienteHandler, ILogger<ClienteController> logger) : ControllerBase
    {
        private readonly IClienteHandler _clienteHandler = clienteHandler;
        private readonly ILogger<ClienteController> _logger = logger;

        [MapToApiVersion("1")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DataClienteResponse))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetAllAsyncV1([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("API V1: Solicitando todos los clientes.");
            var response = await _clienteHandler.GetClientesV1Async(page, pageSize);
            if (response.Data == null || response.Data.Count == 0) return NoContent();
            return Ok(response);
        }

        [MapToApiVersion("2")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DataClienteResponseV2))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetAllAsyncV2([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool? soloActivos = null)
        {
            _logger.LogInformation("API V2: Solicitando todos los clientes (soloActivos: {SoloActivos}).", soloActivos);
            var response = await _clienteHandler.GetClientesV2Async(page, pageSize, soloActivos);
            if (response.Data == null || response.Data.Count == 0) return NoContent();
            return Ok(response);
        }

        [MapToApiVersion("1")]
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DataClienteResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetByIdAsyncV1(int id)
        {
            _logger.LogInformation("API V1: Solicitando cliente por ID: {Id}", id);
            var response = await _clienteHandler.GetClienteByIdV1Async(id);
            if (response.Data == null || response.Data.Count == 0) return NoContent();
            return Ok(response);
        }

        [MapToApiVersion("2")]
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DataClienteResponseV2))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetByIdAsyncV2(int id)
        {
            _logger.LogInformation("API V2: Solicitando cliente por ID: {Id}", id);
            var response = await _clienteHandler.GetClienteByIdV2Async(id);
            if (response.Data == null || response.Data.Count == 0) return NoContent();
            return Ok(response);
        }

        [MapToApiVersion("1")]
        [HttpGet("buscar")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DataClienteResponse))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetByAnyAsyncV1([FromQuery] BuscarClienteRequest query)
        {
            _logger.LogInformation("API V1: Buscando clientes con query.");
            var response = await _clienteHandler.GetClientesByAnyV1Async(query);
            if (response.Data == null || response.Data.Count == 0) return NoContent();
            return Ok(response);
        }

        [MapToApiVersion("2")]
        [HttpGet("buscar")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DataClienteResponseV2))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetByAnyAsyncV2([FromQuery] BuscarClienteRequestV2 query)
        {
            _logger.LogInformation("API V2: Buscando clientes con query.");
            var response = await _clienteHandler.GetClientesByAnyV2Async(query);
            if (response.Data == null || response.Data.Count == 0) return NoContent();
            return Ok(response);
        }

        [MapToApiVersion("1")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAsyncV1([FromBody] List<CrearClienteRequestV1> clientes)
        {
            _logger.LogInformation("API V1: Creando clientes.");
            if (clientes == null || clientes.Count == 0) return BadRequest("La lista de clientes no puede estar vacía.");
            await _clienteHandler.PostClientesV1Async(clientes);
            return StatusCode(StatusCodes.Status201Created, new { Message = "Clientes V1 creados exitosamente." });
        }

        [MapToApiVersion("2")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAsyncV2([FromBody] List<CrearClienteRequestV2> clientes)
        {
            _logger.LogInformation("API V2: Creando clientes.");
            if (clientes == null || clientes.Count == 0) return BadRequest("La lista de clientes no puede estar vacía.");
            await _clienteHandler.PostClientesV2Async(clientes);
            return StatusCode(StatusCodes.Status201Created, new { Message = "Clientes V2 creados exitosamente." });
        }

        [MapToApiVersion("1")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAsyncV1([FromBody] ActualizarClienteRequest cliente)
        {
            _logger.LogInformation("API V1: Actualizando cliente ID: {Id}", cliente.Id);
            if (cliente?.Id == null) return BadRequest("Datos de cliente inválidos o ID nulo.");
            var result = await _clienteHandler.UpdateClienteV1Async(cliente);
            if (!result) return NotFound($"Cliente V1 con ID {cliente.Id} no encontrado.");
            return Ok(new { Message = $"Cliente V1 con ID {cliente.Id} actualizado exitosamente." });
        }

        [MapToApiVersion("2")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAsyncV2([FromBody] ActualizarClienteRequestV2 cliente)
        {
            _logger.LogInformation("API V2: Actualizando cliente ID: {Id}", cliente.Id);
            if (cliente?.Id == null) return BadRequest("Datos de cliente inválidos o ID nulo.");
            var result = await _clienteHandler.UpdateClienteV2Async(cliente);
            if (!result) return NotFound($"Cliente V2 con ID {cliente.Id} no encontrado.");
            return Ok(new { Message = $"Cliente V2 con ID {cliente.Id} actualizado exitosamente." });
        }

        [MapToApiVersion("1")]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsyncV1(int id)
        {
            _logger.LogInformation("API V1: Eliminando cliente ID: {Id}", id);
            var result = await _clienteHandler.DeleteClienteV1Async(id);
            if (!result) return NotFound($"Cliente V1 con ID {id} no encontrado.");
            return Ok(new { Message = $"Cliente V1 con ID {id} eliminado exitosamente." });
        }

        [MapToApiVersion("2")]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsyncV2(int id)
        {
            _logger.LogInformation("API V2: Eliminando cliente ID: {Id}", id);
            var result = await _clienteHandler.DeleteClienteV1Async(id);
            if (!result) return NotFound($"Cliente V2 con ID {id} no encontrado.");
            return Ok(new { Message = $"Cliente V2 con ID {id} eliminado exitosamente." });
        }
    }
}