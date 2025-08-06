using PedidosApi.Application.Services;
using PedidosApi.Domain.Entities;
using PedidosApi.Domain.Interfaces;
using PedidosApi.Domain.Exceptions;
using PedidosApi.Infrastructure.Repositories;
using Xunit;

namespace PedidosApi.Tests.UnitTests
{
    public class PedidoServiceTests : IDisposable
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IEstoqueRepository _estoqueRepository;
        private readonly PedidoService _service;
        private readonly string _testPedidosFilePath;
        private readonly string _testEstoqueFilePath;

        public PedidoServiceTests()
        {
            _testPedidosFilePath = Path.Combine(Path.GetTempPath(), $"test_pedidos_{Guid.NewGuid()}.json");
            _testEstoqueFilePath = Path.Combine(Path.GetTempPath(), $"test_estoque_{Guid.NewGuid()}.json");
            
            _pedidoRepository = new JsonPedidoRepository(_testPedidosFilePath);
            _estoqueRepository = new JsonEstoqueRepository(_testEstoqueFilePath);
            _service = new PedidoService(_pedidoRepository, _estoqueRepository);
            
            // Criar arquivo de estoque para testes
            CriarEstoqueParaTeste();
        }

        private void CriarEstoqueParaTeste()
        {
            var estoque = new List<Estoque>
            {
                new() { ProdutoId = Guid.Parse("11111111-1111-1111-1111-111111111111"), NomeProduto = "Produto Teste 1", QuantidadeDisponivel = 100 },
                new() { ProdutoId = Guid.Parse("22222222-2222-2222-2222-222222222222"), NomeProduto = "Produto Teste 2", QuantidadeDisponivel = 50 },
                new() { ProdutoId = Guid.Parse("33333333-3333-3333-3333-333333333333"), NomeProduto = "Produto Teste 3", QuantidadeDisponivel = 200 }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(estoque, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_testEstoqueFilePath, json);
        }

        public void Dispose()
        {
            if (File.Exists(_testPedidosFilePath))
            {
                File.Delete(_testPedidosFilePath);
            }
            
            if (File.Exists(_testEstoqueFilePath))
            {
                File.Delete(_testEstoqueFilePath);
            }
        }

        [Fact]
        public async Task CriarPedido_ProdutoInexistente_LancaExcecao()
        {
            // Arrange
            var produtoIdInexistente = Guid.NewGuid();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<PedidoException>(() =>
                _service.CriarPedidoAsync(produtoIdInexistente, "Teste", 1, 10m));
            
            Assert.Equal("Produto não encontrado", exception.Message);
        }

        [Fact]
        public async Task CriarPedido_EstoqueInsuficiente_LancaExcecao()
        {
            // Arrange
            var produtoId = new Guid("11111111-1111-1111-1111-111111111111");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<PedidoException>(() =>
                _service.CriarPedidoAsync(produtoId, "Teste", 1000, 10m));
            
            Assert.Equal("Estoque insuficiente", exception.Message);
        }

        [Fact]
        public async Task CriarPedido_DadosValidos_CriaPedido()
        {
            // Arrange
            var produtoId = new Guid("11111111-1111-1111-1111-111111111111");

            // Act
            var result = await _service.CriarPedidoAsync(produtoId, "Notebook Dell", 2, 1500m);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(produtoId, result.ProdutoId);
            Assert.Equal("Notebook Dell", result.Descricao);
            Assert.Equal(2, result.Quantidade);
            Assert.Equal(1500m, result.PrecoUnitario);
            Assert.Equal(3000m, result.ValorTotal);
            Assert.True(result.Ativo);
            Assert.True(result.DataCriacao <= DateTime.UtcNow);
        }

        [Fact]
        public async Task CriarPedido_CalculaValorTotalCorretamente()
        {
            // Arrange
            var produtoId = new Guid("22222222-2222-2222-2222-222222222222");

            // Act
            var result = await _service.CriarPedidoAsync(produtoId, "Mouse Gamer", 3, 85.50m);

            // Assert
            Assert.Equal(256.50m, result.ValorTotal);
        }

        [Fact]
        public async Task ListarPedidosAtivos_SemPedidos_RetornaListaVazia()
        {
            // Act
            var result = await _service.ListarPedidosAtivosAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ListarPedidosAtivos_ComPedidos_RetornaApenasPedidosAtivos()
        {
            // Arrange
            var produtoId = new Guid("11111111-1111-1111-1111-111111111111");
            var pedido1 = await _service.CriarPedidoAsync(produtoId, "Produto 1", 1, 10m);
            var pedido2 = await _service.CriarPedidoAsync(produtoId, "Produto 2", 2, 15m);
            
            // Cancelar um pedido
            await _service.CancelarPedidoAsync(pedido1.Id);

            // Act
            var result = await _service.ListarPedidosAtivosAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal(pedido2.Id, result.First().Id);
            Assert.True(result.First().Ativo);
        }

        [Fact]
        public async Task BuscarPorId_PedidoExistente_RetornaPedido()
        {
            // Arrange
            var produtoId = new Guid("11111111-1111-1111-1111-111111111111");
            var pedidoCriado = await _service.CriarPedidoAsync(produtoId, "Teste Busca", 1, 100m);

            // Act
            var result = await _service.BuscarPorIdAsync(pedidoCriado.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(pedidoCriado.Id, result.Id);
            Assert.Equal("Teste Busca", result.Descricao);
        }

        [Fact]
        public async Task BuscarPorId_PedidoInexistente_RetornaNulo()
        {
            // Act
            var result = await _service.BuscarPorIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CancelarPedido_PedidoExistente_RetornaTrue()
        {
            // Arrange
            var produtoId = new Guid("11111111-1111-1111-1111-111111111111");
            var pedido = await _service.CriarPedidoAsync(produtoId, "Teste Cancelamento", 1, 10m);

            // Act
            var result = await _service.CancelarPedidoAsync(pedido.Id);

            // Assert
            Assert.True(result);
            
            // Verificar se o pedido foi realmente cancelado
            var pedidoCancelado = await _pedidoRepository.GetByIdAsync(pedido.Id);
            Assert.NotNull(pedidoCancelado);
            Assert.False(pedidoCancelado.Ativo);
        }

        [Fact]
        public async Task CancelarPedido_PedidoInexistente_LancaExcecao()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<PedidoException>(() =>
                _service.CancelarPedidoAsync(Guid.NewGuid()));
            
            Assert.Equal("Pedido não encontrado", exception.Message);
        }

        [Fact]
        public async Task CancelarPedido_PedidoJaCancelado_LancaExcecao()
        {
            // Arrange
            var produtoId = new Guid("11111111-1111-1111-1111-111111111111");
            var pedido = await _service.CriarPedidoAsync(produtoId, "Teste", 1, 10m);
            await _service.CancelarPedidoAsync(pedido.Id);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<PedidoException>(() =>
                _service.CancelarPedidoAsync(pedido.Id));
            
            Assert.Equal("Pedido já está cancelado", exception.Message);
        }

        [Fact]
        public async Task CriarPedido_DeveReduzirEstoque()
        {
            // Arrange
            var produtoId = new Guid("33333333-3333-3333-3333-333333333333"); // Estoque inicial: 200

            // Act - Criar primeiro pedido
            await _service.CriarPedidoAsync(produtoId, "Produto 1", 50, 10m);
            
            // Act - Criar segundo pedido
            await _service.CriarPedidoAsync(produtoId, "Produto 2", 100, 15m);

            // Assert - O terceiro pedido deve falhar por estoque insuficiente
            var exception = await Assert.ThrowsAsync<PedidoException>(() =>
                _service.CriarPedidoAsync(produtoId, "Produto 3", 100, 20m)); // Restam apenas 50
            
            Assert.Equal("Estoque insuficiente", exception.Message);
        }

        [Fact]
        public async Task CancelarPedido_DeveRestaurarEstoque()
        {
            // Arrange
            var produtoId = new Guid("22222222-2222-2222-2222-222222222222"); // Estoque inicial: 50
            var pedido = await _service.CriarPedidoAsync(produtoId, "Produto", 30, 10m);

            // Act - Cancelar pedido (deve restaurar 30 unidades)
            await _service.CancelarPedidoAsync(pedido.Id);

            // Assert - Agora deve conseguir criar um pedido de 50 unidades novamente
            var novoPedido = await _service.CriarPedidoAsync(produtoId, "Novo Produto", 50, 15m);
            Assert.NotNull(novoPedido);
        }
    }
}