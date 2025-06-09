using Arquetipo.Api.Models.Response.ApiOperaciones;
using Arquetipo.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Arquetipo.Api.UnitTests
{
    [TestFixture]
    public class ApiOperacionesClientTests
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private Mock<ILogger<OperacionesApiClient>> _loggerMock;
        private HttpClient _httpClient;
        private OperacionesApiClient _apiClient;

        [SetUp]
        public void Setup()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _loggerMock = new Mock<ILogger<OperacionesApiClient>>();

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://tests.com/apioperaciones/")
            };

            // 1. Crea un diccionario con la configuración necesaria.
            var inMemorySettings = new Dictionary<string, string> {
                {"ApiOperaciones:Usuario", "testuser"},
                {"ApiOperaciones:Password", "testpass"}
            };

            // 2. Construye un objeto IConfiguration REAL a partir del diccionario.
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings.Select(kv => new KeyValuePair<string, string?>(kv.Key, kv.Value)))
                .Build();

            // 3. Pasa este objeto de configuración real al constructor.
            _apiClient = new OperacionesApiClient(_httpClient, _loggerMock.Object, configuration);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
        }

        #region Pruebas Existentes
        [Test]
        public async Task GetTasaDeCambioAsync_CuandoApiEsExitosaConDatos_DebeDevolverTasaDeCambio()
        {
            // Arrange
            var responsePayload = new OperacionesApiResponse<TasaDeCambioItem>
            {
                Status = "200",
                Comentario = "OK",
                SessionId = "session-123",
                Data = new List<TasaDeCambioItem> { new() { TasaCambio = 36.5m, FechaCambio = "06-06-2025" } }
            };
            var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonSerializer.Serialize(responsePayload)) };
            _httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponse);

            // Act
            var resultado = await _apiClient.GetTasaDeCambioAsync(new DateTime(2025, 6, 6), "UF");

            // Assert
            resultado.Should().NotBeNull();
            resultado.Data.Should().HaveCount(1);
            resultado.Data[0].TasaCambio.Should().Be(36.5m);
        }

        [Test]
        public async Task GetTasaDeCambioAsync_CuandoApiDevuelveError_DebeLanzarHttpRequestException()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError, Content = new StringContent("Error interno") };
            _httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponse);

            // Act & Assert
            Func<Task> act = async () => await _apiClient.GetTasaDeCambioAsync(new DateTime(2025, 6, 6), "UF");
            await act.Should().ThrowAsync<HttpRequestException>();
        }
        #endregion

        #region Pruebas para Casos de Datos Vacíos
        [Test]
        public async Task GetTasaDeCambioAsync_CuandoApiEsExitosaPeroSinDatos_DebeDevolverRespuestaVacia()
        {
            // Arrange
            var responsePayload = new OperacionesApiResponse<TasaDeCambioItem>
            {
                Status = "200",
                Comentario = "Sin datos",
                SessionId = "session-empty",
                Data = new List<TasaDeCambioItem>()
            };
            var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonSerializer.Serialize(responsePayload)) };
            _httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponse);

            // Act
            var resultado = await _apiClient.GetTasaDeCambioAsync(new DateTime(2025, 6, 7), "USD");

            // Assert
            resultado.Should().NotBeNull();
            resultado.Data.Should().BeEmpty();
            resultado.Comentario.Should().Be("Sin datos");
        }

        [Test]
        public async Task GetFeriadosLegalesAsync_CuandoApiEsExitosaPeroSinData_DebeDevolverRespuestaVacia()
        {
            // Arrange
            var responsePayload = new OperacionesApiResponse<FeriadoLegalItem>
            {
                Status = "200",
                Comentario = "OK",
                SessionId = "session-no-data",
                Data = null // Probando el caso de Data nulo
            };
            var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonSerializer.Serialize(responsePayload)) };
            _httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponse);

            // Act
            var resultado = await _apiClient.GetFeriadosLegalesAsync(DateTime.Now, DateTime.Now.AddDays(1));

            // Assert
            resultado.Should().NotBeNull();
            resultado.Data.Should().BeEmpty(); // El cliente debe convertir el nulo en una lista vacía
        }
        #endregion

        #region Prueba para Verificar Cabeceras
        [Test]
        public async Task GetTasaDeCambioAsync_DebeIncluirCabeceraDeAutorizacionCorrecta()
        {
            // Arrange
            var responsePayload = new OperacionesApiResponse<TasaDeCambioItem> { Status = "200", SessionId = "session-auth" };
            var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonSerializer.Serialize(responsePayload)) };

            var expectedAuthValue = Convert.ToBase64String(Encoding.ASCII.GetBytes("testuser:testpass"));

            // Configuramos el mock para que devuelva la respuesta, pero lo más importante es que nos permite verificar la petición
            _httpMessageHandlerMock.Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.Is<HttpRequestMessage>(req =>
                        req.Headers.Authorization != null &&
                        req.Headers.Authorization.Scheme == "Basic" &&
                        req.Headers.Authorization.Parameter == expectedAuthValue
                   ),
                   ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(httpResponse);

            // Act
            await _apiClient.GetTasaDeCambioAsync(DateTime.Now, "UF");

            
            // Assert
            // La aserción principal está implícita en la configuración del mock.
            // Si la petición no cumple con las condiciones, el mock lanzará una excepción.
            // Podemos añadir una verificación explícita para mayor claridad.
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req => req.Headers.Authorization.Parameter == expectedAuthValue),
                ItExpr.IsAny<CancellationToken>()
            );
        }
        #endregion
    }
}