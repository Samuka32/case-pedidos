using PedidosApi.Domain.Entities;

namespace PedidosApi.Domain.Interfaces;

public interface IPedidoService
{
    Task<List<Pedido>> ListarPedidosAtivosAsync();
    Task<Pedido?> BuscarPorIdAsync(Guid id);
    Task<Pedido> CriarPedidoAsync(Guid produtoId, string descricao, int quantidade, decimal precoUnitario);
    Task<bool> CancelarPedidoAsync(Guid id);
}