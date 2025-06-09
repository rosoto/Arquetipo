using Arquetipo.Api.Models.Response.ApiOperaciones;
using Arquetipo.Api.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Arquetipo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [ApiVersion("2")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class UtilController(
        IApiOperacionesClient operacionesApiClient,
        ILogger<UtilController> logger) : ControllerBase
    {
        private readonly IApiOperacionesClient _operacionesApiClient = operacionesApiClient;
        private readonly ILogger<UtilController> _logger = logger;
        private const string DefaultMoneda = "UF";

        /// <summary>
        /// Obtiene la tasa de cambio desde el servicio externo de Operaciones.
        /// </summary>
        /// <param name="fecha">Fecha de la consulta en formato dd-MM-yyyy.</param>
        /// <param name="moneda">Código de la moneda (ej. UF).</param>
        /// <returns>La información de la tasa de cambio.</returns>
        [ProducesResponseType(typeof(OperacionesApiResponse<TasaDeCambioItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("tasadecambio")]
        public async Task<IActionResult> GetTasaDeCambio(
            [FromQuery] string fecha,
            [FromQuery] string moneda = DefaultMoneda)
        {
            if (string.IsNullOrWhiteSpace(fecha) || string.IsNullOrWhiteSpace(moneda))
            {
                return BadRequest("Los parámetros 'fecha' y 'moneda' son requeridos.");
            }

            if (!DateTime.TryParseExact(fecha, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaConsulta))
            {
                return BadRequest("Formato de fecha inválido. Use dd-MM-yyyy.");
            }

            try
            {
                _logger.LogInformation("Solicitando tasa de cambio para fecha {FechaConsulta} y moneda {CodigoMoneda}", fechaConsulta, moneda);
                var resultado = await _operacionesApiClient.GetTasaDeCambioAsync(fechaConsulta, moneda);

                if (resultado?.Data == null || resultado.Data.Count == 0)
                {
                    _logger.LogWarning("Servicio de tasa de cambio devolvió status {Status} pero sin datos en el array 'data'. Comentario: {Comentario}", resultado?.Status, resultado?.Comentario);
                    return NotFound(new { Message = resultado?.Comentario ?? "No se encontró la tasa de cambio para los parámetros especificados." });
                }

                _logger.LogInformation("Tasa de cambio obtenida exitosamente.");
                return Ok(resultado);
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HttpRequestException al obtener tasa de cambio. StatusCode: {StatusCode}", httpEx.StatusCode);
                return StatusCode((int?)httpEx.StatusCode ?? StatusCodes.Status500InternalServerError, new { Message = $"Error al comunicarse con el servicio de tasas: {httpEx.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener tasa de cambio.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Ocurrió un error interno al procesar la solicitud de tasa de cambio." });
            }
        }

        /// <summary>
        /// Obtiene los feriados legales desde el servicio externo de Operaciones para un rango de fechas.
        /// </summary>
        /// <param name="fechaInicioStr">Fecha de inicio del rango en formato yyyy-MM-dd.</param>
        /// <param name="fechaFinStr">Fecha de fin del rango en formato yyyy-MM-dd.</param>
        /// <returns>Una lista de feriados legales.</returns>
        [ProducesResponseType(typeof(OperacionesApiResponse<FeriadoLegalItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("feriadoslegales")]
        public async Task<IActionResult> GetFeriadosLegales(
            [FromQuery] string fechaInicioStr,
            [FromQuery] string fechaFinStr)
        {
            if (string.IsNullOrWhiteSpace(fechaInicioStr) || string.IsNullOrWhiteSpace(fechaFinStr))
            {
                return BadRequest("Los parámetros 'fechaInicioStr' y 'fechaFinStr' son requeridos.");
            }

            if (!DateTime.TryParseExact(fechaInicioStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaInicio) ||
                !DateTime.TryParseExact(fechaFinStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaFin))
            {
                return BadRequest("Formato de fecha inválido para fechaInicioStr o fechaFinStr. Use yyyy-MM-dd.");
            }

            if (fechaFin < fechaInicio)
            {
                return BadRequest("La fecha de fin no puede ser anterior a la fecha de inicio.");
            }

            try
            {
                _logger.LogInformation("Solicitando feriados legales desde {FechaInicio} hasta {FechaFin}", fechaInicio, fechaFin);
                var resultado = await _operacionesApiClient.GetFeriadosLegalesAsync(fechaInicio, fechaFin);

                if (resultado?.Data == null && resultado?.Status != "200")
                {
                    _logger.LogWarning("Servicio de feriados devolvió status {Status} y data es null. Comentario: {Comentario}", resultado?.Status, resultado?.Comentario);
                    return NotFound(new { Message = resultado?.Comentario ?? "No se encontraron feriados para el rango especificado o hubo un error." });
                }

                _logger.LogInformation("Feriados legales obtenidos exitosamente. Comentario: {Comentario}", resultado?.Comentario);
                return Ok(resultado);
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HttpRequestException al obtener feriados legales. StatusCode: {StatusCode}", httpEx.StatusCode);
                return StatusCode((int?)httpEx.StatusCode ?? StatusCodes.Status500InternalServerError, new { Message = $"Error al comunicarse con el servicio de feriados: {httpEx.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener feriados legales.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Ocurrió un error interno al procesar la solicitud de feriados." });
            }
        }
    }
}
