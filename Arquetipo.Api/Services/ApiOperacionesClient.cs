using Arquetipo.Api.Models.Response.ApiOperaciones;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;

namespace Arquetipo.Api.Services
{
    public class OperacionesApiClient(HttpClient httpClient, ILogger<OperacionesApiClient> logger, IConfiguration configuration) : IApiOperacionesClient
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<OperacionesApiClient> _logger = logger;
        private readonly string _usuario = configuration["ApiOperaciones:Usuario"] ?? throw new ArgumentNullException(nameof(_usuario), "El usuario no está configurado.");
        private readonly string _password = configuration["ApiOperaciones:Password"] ?? throw new ArgumentNullException(nameof(_password), "La contraseña no está configurada.");

        private void AddAuthorizationHeader(HttpRequestMessage request)
        {
            if (!string.IsNullOrEmpty(_usuario) && !string.IsNullOrEmpty(_password))
            {
                var byteArray = Encoding.ASCII.GetBytes($"{_usuario}:{_password}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }
        }

        public async Task<OperacionesApiResponse<TasaDeCambioItem>> GetTasaDeCambioAsync(DateTime fechaConsulta, string codigoMoneda)
        {
            const string requestUri = "service/generales/gettasadecambio/";

            _logger.LogInformation("Llamando a Operaciones API - GetTasaDeCambio en {RequestUri}", requestUri);
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            AddAuthorizationHeader(request);

            request.Headers.Add("FECHACONSULTA", fechaConsulta.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture));
            request.Headers.Add("CODIGOMONEDA", codigoMoneda);

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadFromJsonAsync<OperacionesApiResponse<TasaDeCambioItem>>();
                if (apiResponse == null || apiResponse.Data == null || apiResponse.Data.Count == 0)
                {
                    _logger.LogWarning("GetTasaDeCambio devolvió una respuesta exitosa pero sin datos o con formato inesperado.");
                    return new OperacionesApiResponse<TasaDeCambioItem>
                    {
                        Status = apiResponse?.Status ?? "204",
                        Comentario = apiResponse?.Comentario ?? "Respuesta sin datos.",
                        SessionId = apiResponse?.SessionId ?? string.Empty,
                        Data = []
                    };
                }
                return apiResponse;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error al llamar a GetTasaDeCambio. Status: {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Error en GetTasaDeCambio: {response.StatusCode} - {errorContent}", null, response.StatusCode);
            }
        }

        public async Task<OperacionesApiResponse<FeriadoLegalItem>> GetFeriadosLegalesAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            string requestUri = "service/generales/getferiadoslegales/";
            _logger.LogInformation("Llamando a Operaciones API - GetFeriadosLegales en {RequestUri}", requestUri);

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            AddAuthorizationHeader(request);

            request.Headers.Add("FechaInicioAnio", fechaInicio.Year.ToString());
            request.Headers.Add("FechaInicioMes", fechaInicio.Month.ToString("D2"));
            request.Headers.Add("FechaInicioDia", fechaInicio.Day.ToString("D2"));
            request.Headers.Add("FechaFinAnio", fechaFin.Year.ToString());
            request.Headers.Add("FechaFinMes", fechaFin.Month.ToString("D2"));
            request.Headers.Add("FechaFinDia", fechaFin.Day.ToString("D2"));

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadFromJsonAsync<OperacionesApiResponse<FeriadoLegalItem>>();
                if (apiResponse == null || apiResponse.Data == null)
                {
                    _logger.LogWarning("GetFeriadosLegales devolvió una respuesta exitosa pero sin 'data' o con formato inesperado.");
                    return new OperacionesApiResponse<FeriadoLegalItem>
                    {
                        Status = apiResponse?.Status ?? "204",
                        Comentario = apiResponse?.Comentario ?? "Respuesta sin datos.",
                        SessionId = apiResponse?.SessionId ?? string.Empty,
                        Data = []
                    };
                }
                return apiResponse;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error al llamar a GetFeriadosLegales. Status: {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Error en GetFeriadosLegales: {response.StatusCode} - {errorContent}", null, response.StatusCode);
            }
        }
    }
}