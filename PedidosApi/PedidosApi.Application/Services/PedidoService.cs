using PedidosApi.Application.Interfaces;
using PedidosApi.Domain.Entities;
using PedidosApi.Domain.Interfaces;
using PedidosApi.Domain.Exceptions;

namespace PedidosApi.Application.Services;

public class PedidoService : IPedidoService
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IEstoqueService _estoqueService;

    public PedidoService(IPedidoRepository pedidoRepository, IEstoqueService estoqueService)
    {
        _pedidoRepository = pedidoRepository;
        _estoqueService = estoqueService;
    }

    public async Task<List<Pedido>> ListarPedidosAtivosAsync()
    {
        var todosPedidos = await _pedidoRepository.GetAllAsync();
        return todosPedidos.Where(p => p.Ativo).ToList();
    }

    public async Task<Pedido?> BuscarPorIdAsync(Guid id)
    {
        return await _pedidoRepository.GetByIdAsync(id);
    }

    public async Task<Pedido> CriarPedidoAsync(Guid produtoId, string descricao, int quantidade, decimal precoUnitario)
    {
        // Verificar se produto existe
        var produto = await _estoqueService.ObterEstoqueProdutoAsync(produtoId);
        if (produto == null)
        {
            throw new PedidoException("Produto não encontrado");
        }

        // Verificar disponibilidade e reservar estoque atomicamente
        var estoqueReservado = await _estoqueService.ReservarEstoqueAsync(produtoId, quantidade);
        if (!estoqueReservado)
        {
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
            await _estoqueService.LiberarEstoqueAsync(produtoId, quantidade);
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
        await _estoqueService.LiberarEstoqueAsync(pedido.ProdutoId, pedido.Quantidade);

        // Cancelar pedido (alterar status)
        pedido.Ativo = false;
        return await _pedidoRepository.UpdateAsync(pedido);
    }
}