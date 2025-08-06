using PedidosApi.Domain.Entities;

namespace PedidosApi.Domain.Interfaces;

public interface IPedidoRepository
{
    Task<List<Pedido>> GetAllAsync();
    Task<List<Pedido>> GetAtivosAsync();
    Task<Pedido?> GetByIdAsync(Guid id);
    Task<Pedido> AddAsync(Pedido pedido);
    Task<bool> UpdateAsync(Pedido pedido);
    Task<bool> CancelarPedidoAsync(Guid id);
}