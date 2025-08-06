using PedidosApi.Domain.Entities;

namespace PedidosApi.Domain.Interfaces;

public interface IEstoqueService
{
    Task<bool> VerificarDisponibilidadeAsync(Guid produtoId, int quantidade);
    Task<bool> ReservarEstoqueAsync(Guid produtoId, int quantidade);
    Task<bool> LiberarEstoqueAsync(Guid produtoId, int quantidade);
    Task<Estoque?> ObterEstoqueProdutoAsync(Guid produtoId);
}
