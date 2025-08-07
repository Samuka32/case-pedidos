using PedidosApi.Domain.Entities;

namespace PedidosApi.Application.Interfaces;

public interface IEstoqueRepository
{
    Task<List<Estoque>> GetAllAsync();
    Task<Estoque?> GetByProdutoIdAsync(Guid produtoId);
    Task<bool> UpdateAsync(Estoque estoque);
}
