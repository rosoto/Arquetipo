using Arquetipo.Api.Models.Response;
using Microsoft.EntityFrameworkCore;

namespace Arquetipo.Api.Infrastructure.Persistence
{
    public class ArquetipoDbContext(DbContextOptions<ArquetipoDbContext> options) : DbContext(options)
    {
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
    }
}
