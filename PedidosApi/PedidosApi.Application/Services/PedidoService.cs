using PedidosApi.Domain.Entities;
using PedidosApi.Domain.Interfaces;
using PedidosApi.Domain.Exceptions;

namespace PedidosApi.Application.Services;

public class PedidoService : IPedidoService
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IEstoqueRepository _estoqueRepository;

    public PedidoService(IPedidoRepository pedidoRepository, IEstoqueRepository estoqueRepository)
    {
        _pedidoRepository = pedidoRepository;
        _estoqueRepository = estoqueRepository;
    }

    public async Task<List<Pedido>> ListarPedidosAtivosAsync()
    {
        return await _pedidoRepository.GetAtivosAsync();
    }

    public async Task<Pedido?> BuscarPorIdAsync(Guid id)
    {
        return await _pedidoRepository.GetByIdAsync(id);
    }

    public async Task<Pedido> CriarPedidoAsync(Guid produtoId, string descricao, int quantidade, decimal precoUnitario)
    {
        // Verificar se produto existe e tem estoque suficiente atomicamente
        var estoqueReservado = await _estoqueRepository.ReservarEstoqueAsync(produtoId, quantidade);
        if (!estoqueReservado)
        {
            var produto = await _estoqueRepository.GetByProdutoIdAsync(produtoId);
            if (produto == null)
            {
                throw new PedidoException("Produto não encontrado");
            }
            throw new PedidoException("Estoque insuficiente");
        }

        try
        {
            var pedido = new Pedido
            {
                Id = Guid.NewGuid(),
                ProdutoId = produtoId,
                Descricao = descricao,
                Quantidade = quantidade,
                PrecoUnitario = precoUnitario,
                ValorTotal = quantidade * precoUnitario,
                DataCriacao = DateTime.UtcNow,
                Ativo = true
            };

            return await _pedidoRepository.AddAsync(pedido);
        }
        catch
        {
            // Se falhar ao criar pedido, liberar estoque
            await _estoqueRepository.LiberarEstoqueAsync(produtoId, quantidade);
            throw;
        }
    }

    public async Task<bool> CancelarPedidoAsync(Guid id)
    {
        var pedido = await _pedidoRepository.GetByIdAsync(id);
        
        if (pedido == null)
        {
            throw new PedidoException("Pedido não encontrado");
        }

        if (!pedido.Ativo)
        {
            throw new PedidoException("Pedido já está cancelado");
        }

        // Liberar estoque
        await _estoqueRepository.LiberarEstoqueAsync(pedido.ProdutoId, pedido.Quantidade);

        // Cancelar pedido
        return await _pedidoRepository.CancelarPedidoAsync(id);
    }
}