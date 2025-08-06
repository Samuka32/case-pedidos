using PedidosApi.Domain.Entities;

namespace PedidosApi.Domain.Interfaces;

public interface IEstoqueRepository
{
    Task<List<Estoque>> GetAllAsync();
    Task<Estoque?> GetByProdutoIdAsync(Guid produtoId);
    Task<bool> UpdateAsync(Estoque estoque);
    Task<bool> ReservarEstoqueAsync(Guid produtoId, int quantidade);
    Task<bool> LiberarEstoqueAsync(Guid produtoId, int quantidade);
}
