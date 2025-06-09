using Arquetipo.Api.Handlers;
using Arquetipo.Api.Infrastructure;
using Arquetipo.Api.Models.Request;
using Arquetipo.Api.Models.Request.v1;
using Arquetipo.Api.Models.Request.v2;
using Arquetipo.Api.Models.Response;
using Arquetipo.Api.Models.Response.v1;
using Arquetipo.Api.Models.Response.v2;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;

namespace Arquetipo.Api.UnitTests
{
    [TestFixture]
    public class ClienteHandlerTests
    {
        // Mocks para las dependencias del handler
        private Mock<IClienteRepository> _clienteRepositoryMock;
        private Mock<ILogger<ClienteHandler>> _loggerMock;
        private Mock<IMapper> _mapperMock;

        // La instancia de la clase que vamos a probar
        private ClienteHandler _clienteHandler;

        [SetUp]
        public void Setup()
        {
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            _loggerMock = new Mock<ILogger<ClienteHandler>>();
            _mapperMock = new Mock<IMapper>();
            _clienteHandler = new ClienteHandler(_clienteRepositoryMock.Object, _loggerMock.Object, _mapperMock.Object);
        }

        #region Pruebas V1 Existentes y Corregidas
        [Test]
        public async Task GetClienteByIdV1Async_CuandoClienteExiste_DebeDevolverDatosDelCliente()
        {
            // Arrange
            var idCliente = 1;
            var clienteDePrueba = new Cliente { Id = idCliente, Nombre = "Juan", Apellido = "Perez", Email = "juan.perez@test.com", Telefono = "12345678" };
            var clienteResponse = new ClienteResponse { Id = idCliente, Nombre = "Juan", Apellido = "Perez", Email = "juan.perez@test.com", Telefono = "12345678" };

            _clienteRepositoryMock.Setup(repo => repo.GetByIdAsync(idCliente)).ReturnsAsync(clienteDePrueba);
            _mapperMock.Setup(m => m.Map<ClienteResponse>(clienteDePrueba)).Returns(clienteResponse);

            // Act
            var resultado = await _clienteHandler.GetClienteByIdV1Async(idCliente);

            // Assert
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Data, Has.Count.EqualTo(1));
            Assert.That(resultado.Data[0].Id, Is.EqualTo(idCliente));
        }

        [Test]
        public async Task UpdateClienteV1Async_CuandoClienteExiste_DebeLlamarAlRepositorioYDevolverTrue()
        {
            // Arrange
            var idCliente = 1;
            var solicitudActualizar = new ActualizarClienteRequest { Id = idCliente, Nombre = "Juan Actualizado", Apellido = "Perez", Email = "juan.perez@test.com", Telefono = "12345678" };
            var setClienteMapeado = new SetCliente { Id = idCliente, Nombre = "Juan Actualizado", Apellido = "Perez", Email = "juan.perez@test.com", Telefono = "12345678" };

            _clienteRepositoryMock.Setup(repo => repo.ExistsAsync(idCliente)).ReturnsAsync(true);
            _mapperMock.Setup(m => m.Map<SetCliente>(solicitudActualizar)).Returns(setClienteMapeado);
            _clienteRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<SetCliente>())).Returns(Task.CompletedTask);

            // Act
            var resultado = await _clienteHandler.UpdateClienteV1Async(solicitudActualizar);

            // Assert
            Assert.That(resultado, Is.True);
            _clienteRepositoryMock.Verify(repo => repo.ExistsAsync(idCliente), Times.Once);
            _clienteRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<SetCliente>()), Times.Once);
        }

        [Test]
        public async Task UpdateClienteV1Async_CuandoClienteNoExiste_NoDebeLlamarAlRepositorioYDevolverFalse()
        {
            // Arrange
            var idCliente = 999;
            var solicitudActualizar = new ActualizarClienteRequest { Id = idCliente, Nombre = "No", Apellido = "Existe", Email = "no@existe.com", Telefono = "0" };

            _clienteRepositoryMock.Setup(repo => repo.ExistsAsync(idCliente)).ReturnsAsync(false);

            // Act
            var resultado = await _clienteHandler.UpdateClienteV1Async(solicitudActualizar);

            // Assert
            Assert.That(resultado, Is.False);
            _clienteRepositoryMock.Verify(repo => repo.ExistsAsync(idCliente), Times.Once);
            _clienteRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<SetCliente>()), Times.Never);
        }
        #endregion

        #region Pruebas para DeleteClienteV1Async
        [Test]
        public async Task DeleteClienteV1Async_CuandoClienteExiste_DebeLlamarAlRepositorioYDevolverTrue()
        {
            // Arrange
            var idCliente = 1;
            _clienteRepositoryMock.Setup(repo => repo.ExistsAsync(idCliente)).ReturnsAsync(true);
            _clienteRepositoryMock.Setup(repo => repo.DeleteAsync(idCliente)).Returns(Task.CompletedTask);

            // Act
            var resultado = await _clienteHandler.DeleteClienteV1Async(idCliente);

            // Assert
            Assert.That(resultado, Is.True);
            _clienteRepositoryMock.Verify(repo => repo.ExistsAsync(idCliente), Times.Once);
            _clienteRepositoryMock.Verify(repo => repo.DeleteAsync(idCliente), Times.Once);
        }

        [Test]
        public async Task DeleteClienteV1Async_CuandoClienteNoExiste_DebeDevolverFalse()
        {
            // Arrange
            var idCliente = 999;
            _clienteRepositoryMock.Setup(repo => repo.ExistsAsync(idCliente)).ReturnsAsync(false);

            // Act
            var resultado = await _clienteHandler.DeleteClienteV1Async(idCliente);

            // Assert
            Assert.That(resultado, Is.False);
            _clienteRepositoryMock.Verify(repo => repo.ExistsAsync(idCliente), Times.Once);
            _clienteRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<int?>()), Times.Never);
        }
        #endregion

        #region Pruebas para Métodos V2
        [Test]
        public async Task GetClientesV2Async_CuandoHayClientes_DebeMapearAResponseV2()
        {
            // Arrange
            var clientesDePrueba = new List<Cliente> { new() { Id = 1, Nombre = "Ana", Apellido = "Sosa", Email = "ana@v2.com", Telefono = "111" } };
            var responseDePrueba = new List<ClienteResponseV2> { new() { Id = 1, Nombre = "Ana", Apellido = "Sosa", Email = "ana@v2.com" } };

            _clienteRepositoryMock.Setup(repo => repo.GetAllAsync(1, 10)).ReturnsAsync(clientesDePrueba);
            _mapperMock.Setup(m => m.Map<List<ClienteResponseV2>>(clientesDePrueba)).Returns(responseDePrueba);

            // Act
            var resultado = await _clienteHandler.GetClientesV2Async(1, 10, null);

            // Assert
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Data, Has.Count.EqualTo(1));
            Assert.That(resultado.Data[0].Email, Is.EqualTo("ana@v2.com"));
        }

        [Test]
        public async Task PostClientesV2Async_ConClientesValidos_DebeLlamarAlRepositorio()
        {
            // Arrange
            var request = new List<CrearClienteRequestV2> { new() { Nombre = "Nuevo", Apellido = "Cliente V2", Email = "v2@test.com" } };
            var repoList = new List<SetCliente> { new() { Nombre = "Nuevo", Apellido = "Cliente V2", Email = "v2@test.com", Telefono = "N/A" } };

            _mapperMock.Setup(m => m.Map<List<SetCliente>>(request)).Returns(repoList);
            _clienteRepositoryMock.Setup(r => r.AddClientesAsync(repoList)).Returns(Task.CompletedTask);

            // Act
            await _clienteHandler.PostClientesV2Async(request);

            // Assert
            _clienteRepositoryMock.Verify(r => r.AddClientesAsync(repoList), Times.Once);
        }

        [Test]
        public async Task UpdateClienteV2Async_CuandoClienteExiste_DebeActualizarYDevolverTrue()
        {
            // Arrange
            var idCliente = 2;
            var updateRequest = new ActualizarClienteRequestV2 { Id = idCliente, Nombre = "NombreV2" };
            var entidadExistente = new Cliente { Id = idCliente, Nombre = "Original", Apellido = "Apellido", Email = "test@test.com", Telefono = "123" };
            var setClienteFinal = new SetCliente { Id = idCliente, Nombre = "NombreV2", Apellido = "Apellido", Email = "test@test.com", Telefono = "123" };

            _clienteRepositoryMock.Setup(r => r.GetByIdAsync(idCliente)).ReturnsAsync(entidadExistente);
            _mapperMock.Setup(m => m.Map(updateRequest, entidadExistente));
            _mapperMock.Setup(m => m.Map<SetCliente>(entidadExistente)).Returns(setClienteFinal);
            _clienteRepositoryMock.Setup(r => r.UpdateAsync(setClienteFinal)).Returns(Task.CompletedTask);

            // Act
            var resultado = await _clienteHandler.UpdateClienteV2Async(updateRequest);

            // Assert
            Assert.That(resultado, Is.True);
            _clienteRepositoryMock.Verify(r => r.GetByIdAsync(idCliente), Times.Once);
            _clienteRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<SetCliente>()), Times.Once);
        }

        [Test]
        public async Task UpdateClienteV2Async_CuandoClienteNoExiste_DebeDevolverFalse()
        {
            // Arrange
            var idCliente = 999;
            var updateRequest = new ActualizarClienteRequestV2 { Id = idCliente };

            _clienteRepositoryMock.Setup(r => r.GetByIdAsync(idCliente)).ReturnsAsync((Cliente?)null);

            // Act
            var resultado = await _clienteHandler.UpdateClienteV2Async(updateRequest);

            // Assert
            Assert.That(resultado, Is.False);
            _clienteRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<SetCliente>()), Times.Never);
        }
        #endregion
    }
}