using Arquetipo.Api.Infrastructure.Persistence;
using Arquetipo.Api.Models.Request.v1;
using Arquetipo.Api.Models.Response.v1;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Arquetipo.Api.IntegrationTests
{
    [TestFixture]
    public class ClienteControllerTests
    {
        private CustomWebApplicationFactory _factory;
        private HttpClient _client;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Se crea la fábrica una sola vez para todas las pruebas de esta clase.
            _factory = new CustomWebApplicationFactory();
        }

        [SetUp]
        public void SetUp()
        {
            // Se crea un nuevo cliente HTTP para cada prueba.
            _client = _factory.CreateClient();
        }

        [Test]
        public async Task GetAllAsyncV1_CuandoHayClientes_DebeDevolverOkYListaDeClientes()
        {
            // --- ARRANGE ---
            // La base de datos ya fue poblada con un cliente en la CustomWebApplicationFactory.

            // --- ACT ---
            // Hacemos una llamada HTTP GET real al endpoint de nuestra API en memoria.
            var response = await _client.GetAsync("/api/v1/cliente");

            // --- ASSERT ---
            // 1. Verificamos el código de estado de la respuesta HTTP.
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // 2. Deserializamos el cuerpo de la respuesta JSON a nuestro modelo.
            var resultado = await response.Content.ReadFromJsonAsync<DataClienteResponse>();

            // 3. Verificamos el contenido.
            resultado.Should().NotBeNull();
            resultado.Data.Should().NotBeNull();
            resultado.Data.Should().HaveCount(2);
            resultado.Data[0].Nombre.Should().Be("Ana");
            resultado.Data[1].Nombre.Should().Be("Roberto");
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _factory.Dispose();
        }

        [Test]
        public async Task AddAsyncV1_ConDatosValidos_DebeDevolverCreatedYGuardarEnBaseDeDatos()
        {
            // --- ARRANGE ---
            // 1. Creamos el cuerpo de la solicitud (el payload JSON).
            var nuevaSolicitudCliente = new List<CrearClienteRequestV1>
            {
                new() {
                    Nombre = "Roberto",
                    Apellido = "Rojas",
                    Email = "roberto.rojas@test.com",
                    Telefono = "987654321"
                }
            };

            // --- ACT ---
            // 2. Realizamos la solicitud POST a la API en memoria.
            var response = await _client.PostAsJsonAsync("/api/v1/cliente", nuevaSolicitudCliente);

            // --- ASSERT ---
            // 3. Verificamos la respuesta HTTP.
            response.StatusCode.Should().Be(HttpStatusCode.Created); // El estándar para un POST exitoso es 201 Created.

            // 4. VERIFICACIÓN CLAVE: Nos aseguramos de que el cliente fue realmente guardado en la BD en memoria.
            // Para ello, accedemos directamente al DbContext a través de la factory de la aplicación.
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ArquetipoDbContext>();

                // Buscamos en la BD el cliente que acabamos de crear.
                var clienteEnLaBD = await context.Clientes.FirstOrDefaultAsync(c => c.Email == "roberto.rojas@test.com");

                // Afirmamos que el cliente existe y sus datos son correctos.
                clienteEnLaBD.Should().NotBeNull();
                clienteEnLaBD.Nombre.Should().Be("Roberto");
            }
        }
    }
}