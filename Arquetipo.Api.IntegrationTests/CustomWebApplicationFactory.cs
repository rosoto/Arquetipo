using Arquetipo.Api.Infrastructure.Persistence;
using Arquetipo.Api.Models.Response;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Arquetipo.Api.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ApplicationName", "Arquetipo.Api");

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ArquetipoDbContext>));

                if (descriptor != null)
                    services.Remove(descriptor);

                // 3. Agregamos el DbContext usando una base de datos en memoria CON UN NOMBRE FIJO.
                //    El nombre "TestDatabase" asegura que todas las instancias del DbContext
                //    en la prueba se conecten a la misma base de datos.
                services.AddDbContext<ArquetipoDbContext>(options =>
                {
                    options.UseInMemoryDatabase("Database");
                });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ArquetipoDbContext>();
                db.Database.EnsureCreated();

                db.Clientes.Add(new Cliente { Id = 1, Nombre = "Ana", Apellido = "Garcia", Email = "ana.garcia@test.com", Telefono = "87654321" });
                db.SaveChanges();
            });

            builder.UseEnvironment("Development");
        }
    }
}