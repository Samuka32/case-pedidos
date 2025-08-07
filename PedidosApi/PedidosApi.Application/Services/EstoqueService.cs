using PedidosApi.Domain.Entities;
using PedidosApi.Domain.Interfaces;

namespace PedidosApi.Application.Services;

public class EstoqueService : IEstoqueService
{
    private readonly Interfaces.IEstoqueRepository _estoqueRepository;

    public EstoqueService(Interfaces.IEstoqueRepository estoqueRepository)
    {
        _estoqueRepository = estoqueRepository;
    }

    public async Task<bool> VerificarDisponibilidadeAsync(Guid produtoId, int quantidade)
    {
        var estoque = await _estoqueRepository.GetByProdutoIdAsync(produtoId);
        return estoque != null && estoque.QuantidadeDisponivel >= quantidade;
    }

    public async Task<bool> ReservarEstoqueAsync(Guid produtoId, int quantidade)
    {
        var estoque = await _estoqueRepository.GetByProdutoIdAsync(produtoId);
        
        if (estoque == null || estoque.QuantidadeDisponivel < quantidade)
            return false;

        estoque.QuantidadeDisponivel -= quantidade;
        return await _estoqueRepository.UpdateAsync(estoque);
    }

    public async Task<bool> LiberarEstoqueAsync(Guid produtoId, int quantidade)
    {
        var estoque = await _estoqueRepository.GetByProdutoIdAsync(produtoId);
        
        if (estoque == null)
            return false;

        estoque.QuantidadeDisponivel += quantidade;
        return await _estoqueRepository.UpdateAsync(estoque);
    }

    public async Task<Estoque?> ObterEstoqueProdutoAsync(Guid produtoId)
    {
        return await _estoqueRepository.GetByProdutoIdAsync(produtoId);
    }
}
