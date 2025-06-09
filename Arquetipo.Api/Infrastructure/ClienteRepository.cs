using Arquetipo.Api.Infrastructure.Persistence;
using Arquetipo.Api.Models.Request;
using Arquetipo.Api.Models.Request.v1;
using Arquetipo.Api.Models.Response;
using Microsoft.EntityFrameworkCore;

namespace Arquetipo.Api.Infrastructure;

public class ClienteRepository(ArquetipoDbContext context, ILogger<ClienteRepository> logger) : IClienteRepository
{
    private readonly ArquetipoDbContext _context = context;
    private readonly ILogger<ClienteRepository> _logger = logger;

    public async Task<List<Cliente>> GetAllAsync(int page, int pageSize)
    {
        _logger.LogInformation("Obteniendo clientes paginados desde EF Core. Página: {Page}, Tamaño: {PageSize}", page, pageSize);

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        return await _context.Clientes
            .OrderBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Cliente?> GetByIdAsync(int? id)
    {
        _logger.LogInformation("Obteniendo cliente por ID {Id} desde EF Core", id);
        if (id == null) return null;
        return await _context.Clientes.FindAsync(id);
    }

    public async Task<List<Cliente>> GetByAnyAsync(SetClienteAny clienteParams)
    {
        _logger.LogInformation("Buscando clientes desde EF Core con parámetros: {@ClienteParams}", clienteParams);

        var query = _context.Clientes.AsQueryable();

        if (clienteParams == null)
        {
            _logger.LogWarning("GetByAnyAsync fue llamado con clienteParams nulo. Se devolverán los primeros 10 clientes si existen.");
            return await query.Take(10).ToListAsync();
        }

        if (!string.IsNullOrWhiteSpace(clienteParams.Nombre))
        {
            query = query.Where(c => c.Nombre.Contains(clienteParams.Nombre));
        }
        if (!string.IsNullOrWhiteSpace(clienteParams.Apellido))
        {
            query = query.Where(c => c.Apellido.Contains(clienteParams.Apellido));
        }
        if (!string.IsNullOrWhiteSpace(clienteParams.Email))
        {
            query = query.Where(c => c.Email.Contains(clienteParams.Email));
        }
        if (!string.IsNullOrWhiteSpace(clienteParams.Telefono))
        {
            query = query.Where(c => c.Telefono.Contains(clienteParams.Telefono));
        }

        return await query.Take(10).ToListAsync();
    }

    public async Task<bool> ExistsAsync(int? id)
    {
        if (id == null) return false;
        return await _context.Clientes.AnyAsync(c => c.Id == id);
    }

    public async Task AddClientesAsync(List<SetCliente> clientesDto)
    {
        if (clientesDto == null || clientesDto.Count == 0)
        {
            _logger.LogWarning("Intento de agregar una lista de clientes nula o vacía.");
            return;
        }

        var clientesEntities = new List<Cliente>();
        foreach (var dto in clientesDto)
        {
            clientesEntities.Add(new Cliente
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Email = dto.Email,
                Telefono = dto.Telefono
            });
        }

        await _context.Clientes.AddRangeAsync(clientesEntities);
        await _context.SaveChangesAsync();
        _logger.LogInformation("{Count} clientes agregados a través de EF Core.", clientesEntities.Count);
    }

    public async Task DeleteAsync(int? id)
    {
        if (id == null)
        {
            _logger.LogWarning("Intento de eliminar un cliente con ID nulo.");
            return;
        }

        var clienteEntity = await _context.Clientes.FindAsync(id);

        if (clienteEntity == null)
        {
            _logger.LogWarning("No se encontró el cliente con ID {Id} para eliminar.", id);
            return;
        }

        _context.Clientes.Remove(clienteEntity);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Cliente con ID {Id} eliminado a través de EF Core.", id);
    }

    public async Task UpdateAsync(SetCliente clienteDto)
    {
        if (clienteDto?.Id == null)
        {
            _logger.LogWarning("Intento de actualizar un cliente con DTO nulo o ID nulo.");
            return;
        }

        // Crea la entidad desde el DTO. No es necesario buscarla primero en la BD.
        var clienteEntity = new Cliente
        {
            Id = clienteDto.Id.Value,
            Nombre = clienteDto.Nombre,
            Apellido = clienteDto.Apellido,
            Email = clienteDto.Email,
            Telefono = clienteDto.Telefono
        };

        _context.Clientes.Attach(clienteEntity);
        _context.Entry(clienteEntity).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Cliente con ID {Id} actualizado exitosamente a través de EF Core.", clienteDto.Id);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Error de concurrencia al actualizar el cliente con ID {Id}.", clienteDto.Id);
            throw;
        }
    }
}