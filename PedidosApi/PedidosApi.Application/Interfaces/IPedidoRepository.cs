using PedidosApi.Domain.Entities;

namespace PedidosApi.Application.Interfaces;

public interface IPedidoRepository
{
    Task<List<Pedido>> GetAllAsync();
    Task<Pedido?> GetByIdAsync(Guid id);
    Task<Pedido> AddAsync(Pedido pedido);
    Task<bool> UpdateAsync(Pedido pedido);
}
