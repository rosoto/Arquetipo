using Arquetipo.Api.Models.Response.ApiOperaciones;
using Arquetipo.Api.Services;
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
        // CORRECCIÓN 1: Se agrega el operador "null-forgiving" (!) a los campos.
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

            var inMemorySettings = new Dictionary<string, string> {
                {"ApiOperaciones:Usuario", "testuser"},
                {"ApiOperaciones:Password", "testpass"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings.Select(kv => new KeyValuePair<string, string?>(kv.Key, kv.Value)))
                .Build();

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
                Data = [new() { TasaCambio = 36.5m, FechaCambio = "06-06-2025" }]
            };
            var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonSerializer.Serialize(responsePayload)) };
            _httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponse);

            // Act
            var resultado = await _apiClient.GetTasaDeCambioAsync(new DateTime(2025, 6, 6), "UF");

            // Assert
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Data, Has.Count.EqualTo(1));
            Assert.That(resultado.Data[0].TasaCambio, Is.EqualTo(36.5m));
        }

        [Test]
        public Task GetTasaDeCambioAsync_CuandoApiDevuelveError_DebeLanzarHttpRequestException()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError, Content = new StringContent("Error interno") };
            _httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponse);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _apiClient.GetTasaDeCambioAsync(new DateTime(2025, 6, 6), "UF")
            );
            return Task.CompletedTask;
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
                Data = []
            };
            var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonSerializer.Serialize(responsePayload)) };
            _httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponse);

            // Act
            var resultado = await _apiClient.GetTasaDeCambioAsync(new DateTime(2025, 6, 7), "USD");

            // Assert
            Assert.That(resultado, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(resultado.Data, Is.Empty);
                Assert.That(resultado.Comentario, Is.EqualTo("Sin datos"));
            });
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
                Data = null
            };
            var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonSerializer.Serialize(responsePayload)) };
            _httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponse);

            // Act
            var resultado = await _apiClient.GetFeriadosLegalesAsync(DateTime.Now, DateTime.Now.AddDays(1));

            // Assert
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Data, Is.Empty);
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
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                // CORRECCIÓN 2: Se agrega la comprobación "!= null" antes de acceder a ".Parameter"
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Headers.Authorization != null && req.Headers.Authorization.Parameter == expectedAuthValue),
                ItExpr.IsAny<CancellationToken>()
            );
        }
        #endregion
    }
}