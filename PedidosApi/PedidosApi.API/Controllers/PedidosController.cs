using Microsoft.AspNetCore.Mvc;
using PedidosApi.Application.DTOs;
using PedidosApi.Domain.Interfaces;
using PedidosApi.Domain.Exceptions;

namespace PedidosApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly IPedidoService _pedidoService;

    public PedidosController(IPedidoService pedidoService)
    {
        _pedidoService = pedidoService;
    }

    /// <summary>
    /// Lista todos os pedidos ativos
    /// </summary>
    /// <returns>Lista de pedidos ativos</returns>
    [HttpGet]
    public async Task<ActionResult<List<PedidoDto>>> ListarPedidos()
    {
        try
        {
            var pedidos = await _pedidoService.ListarPedidosAtivosAsync();
            var pedidosDto = pedidos.Select(p => new PedidoDto
            {
                Id = p.Id,
                ProdutoId = p.ProdutoId,
                Descricao = p.Descricao,
                Quantidade = p.Quantidade,
                PrecoUnitario = p.PrecoUnitario,
                ValorTotal = p.ValorTotal,
                DataCriacao = p.DataCriacao,
                Ativo = p.Ativo
            }).ToList();

            return Ok(pedidosDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Efetua um novo pedido
    /// </summary>
    /// <param name="pedidoDto">Dados do pedido a ser criado</param>
    /// <returns>Pedido criado</returns>
    [HttpPost]
    public async Task<ActionResult<PedidoDto>> EfetuarPedido([FromBody] CriarPedidoDto pedidoDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var pedido = await _pedidoService.CriarPedidoAsync(
                pedidoDto.ProdutoId, 
                pedidoDto.Descricao, 
                pedidoDto.Quantidade, 
                pedidoDto.PrecoUnitario);

            var resultado = new PedidoDto
            {
                Id = pedido.Id,
                ProdutoId = pedido.ProdutoId,
                Descricao = pedido.Descricao,
                Quantidade = pedido.Quantidade,
                PrecoUnitario = pedido.PrecoUnitario,
                ValorTotal = pedido.ValorTotal,
                DataCriacao = pedido.DataCriacao,
                Ativo = pedido.Ativo
            };

            return CreatedAtAction(nameof(ListarPedidos), new { id = pedido.Id }, resultado);
        }
        catch (PedidoException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Cancela um pedido existente
    /// </summary>
    /// <param name="id">ID do pedido a ser cancelado</param>
    /// <returns>Resultado da operação</returns>
    [HttpPost("{id}/cancelar")]
    public async Task<ActionResult> CancelarPedido(Guid id)
    {
        try
        {
            var sucesso = await _pedidoService.CancelarPedidoAsync(id);
            
            if (!sucesso)
                return NotFound("Pedido não encontrado");

            return Ok("Pedido cancelado com sucesso");
        }
        catch (PedidoException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno: {ex.Message}");
        }
    }
}