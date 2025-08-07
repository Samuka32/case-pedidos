using System.Text.Json;
using PedidosApi.Application.Interfaces;
using PedidosApi.Domain.Entities;

namespace PedidosApi.Infrastructure.Repositories;

public class JsonEstoqueRepository : IEstoqueRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public JsonEstoqueRepository()
    {
        // Calcula o caminho para a pasta Data na raiz do repositório
        var currentDirectory = Directory.GetCurrentDirectory();
        var projectRoot = Path.GetDirectoryName(currentDirectory);
        _filePath = Path.Combine(projectRoot ?? currentDirectory, "Data", "estoque.json");
    }

    public JsonEstoqueRepository(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<List<Estoque>> GetAllAsync()
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

    private async Task<List<Estoque>> GetAllInternalAsync()
    {
        if (!File.Exists(_filePath))
            return new List<Estoque>();

        var json = await File.ReadAllTextAsync(_filePath);
        return JsonSerializer.Deserialize<List<Estoque>>(json) ?? new List<Estoque>();
    }

    public async Task<Estoque?> GetByProdutoIdAsync(Guid produtoId)
    {
        var estoques = await GetAllAsync();
        return estoques.FirstOrDefault(e => e.ProdutoId == produtoId);
    }

    public async Task<bool> UpdateAsync(Estoque estoque)
    {
        await _semaphore.WaitAsync();
        try
        {
            var estoques = await GetAllInternalAsync();
            var index = estoques.FindIndex(e => e.ProdutoId == estoque.ProdutoId);
            
            if (index == -1)
                return false;

            estoques[index] = estoque;
            await SalvarEstoquesAsync(estoques);
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task SalvarEstoquesAsync(List<Estoque> estoques)
    {
        var json = JsonSerializer.Serialize(estoques, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        await File.WriteAllTextAsync(_filePath, json);
    }
}
