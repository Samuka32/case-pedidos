using System.Text.Json;
using PedidosApi.Domain.Entities;
using PedidosApi.Domain.Interfaces;

namespace PedidosApi.Infrastructure.Repositories;

public class JsonPedidoRepository : IPedidoRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public JsonPedidoRepository()
    {
        // Usa o diretório atual ao invés do BaseDirectory
        _filePath = Path.Combine(Directory.GetCurrentDirectory(), "pedidos.json");
    }

    public JsonPedidoRepository(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<List<Pedido>> GetAllAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            return await GetAllInternalAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<List<Pedido>> GetAllInternalAsync()
    {
        if (!File.Exists(_filePath))
            return new List<Pedido>();

        var json = await File.ReadAllTextAsync(_filePath);
        return JsonSerializer.Deserialize<List<Pedido>>(json) ?? new List<Pedido>();
    }

    public async Task<List<Pedido>> GetAtivosAsync()
    {
        var pedidos = await GetAllAsync();
        return pedidos.Where(p => p.Ativo).ToList();
    }

    public async Task<Pedido?> GetByIdAsync(Guid id)
    {
        var pedidos = await GetAllAsync();
        return pedidos.FirstOrDefault(p => p.Id == id);
    }

    public async Task<Pedido> AddAsync(Pedido pedido)
    {
        await _semaphore.WaitAsync();
        try
        {
            var pedidos = await GetAllInternalAsync();
            pedidos.Add(pedido);
            await SalvarPedidosAsync(pedidos);
            return pedido;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> UpdateAsync(Pedido pedido)
    {
        await _semaphore.WaitAsync();
        try
        {
            var pedidos = await GetAllInternalAsync();
            var index = pedidos.FindIndex(p => p.Id == pedido.Id);
            
            if (index == -1)
                return false;

            pedidos[index] = pedido;
            await SalvarPedidosAsync(pedidos);
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> CancelarPedidoAsync(Guid id)
    {
        var pedido = await GetByIdAsync(id);
        if (pedido == null)
            return false;

        pedido.Ativo = false;
        return await UpdateAsync(pedido);
    }

    private async Task SalvarPedidosAsync(List<Pedido> pedidos)
    {
        var json = JsonSerializer.Serialize(pedidos, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        await File.WriteAllTextAsync(_filePath, json);
    }
}
