using PedidosApi.Application.Services;
using PedidosApi.Domain.Entities;
using PedidosApi.Domain.Interfaces;
using PedidosApi.Infrastructure.Repositories;
using Xunit;
using AppInterfaces = PedidosApi.Application.Interfaces;

namespace PedidosApi.Tests.UnitTests
{
    public class EstoqueServiceTests : IDisposable
    {
        private readonly AppInterfaces.IEstoqueRepository _estoqueRepository;
        private readonly EstoqueService _service;
        private readonly string _testEstoqueFilePath;

        public EstoqueServiceTests()
        {
            _testEstoqueFilePath = Path.Combine(Path.GetTempPath(), $"test_estoque_{Guid.NewGuid()}.json");
            _estoqueRepository = new JsonEstoqueRepository(_testEstoqueFilePath);
            _service = new EstoqueService(_estoqueRepository);
            
            // Criar arquivo de estoque para testes
            CriarEstoqueParaTeste();
        }

        private void CriarEstoqueParaTeste()
        {
            var estoque = new List<Estoque>
            {
                new() { ProdutoId = Guid.Parse("11111111-1111-1111-1111-111111111111"), NomeProduto = "Produto Teste 1", QuantidadeDisponivel = 100 },
                new() { ProdutoId = Guid.Parse("22222222-2222-2222-2222-222222222222"), NomeProduto = "Produto Teste 2", QuantidadeDisponivel = 50 },
                new() { ProdutoId = Guid.Parse("33333333-3333-3333-3333-333333333333"), NomeProduto = "Produto Teste 3", QuantidadeDisponivel = 0 }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(estoque, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_testEstoqueFilePath, json);
        }

        public void Dispose()
        {
            if (File.Exists(_testEstoqueFilePath))
            {
                File.Delete(_testEstoqueFilePath);
            }
        }

        [Fact]
        public async Task VerificarDisponibilidadeAsync_ProdutoComEstoque_RetornaTrue()
        {
            // Arrange
            var produtoId = new Guid("11111111-1111-1111-1111-111111111111");

            // Act
            var resultado = await _service.VerificarDisponibilidadeAsync(produtoId, 50);

            // Assert
            Assert.True(resultado);
        }

        [Fact]
        public async Task VerificarDisponibilidadeAsync_EstoqueInsuficiente_RetornaFalse()
        {
            // Arrange
            var produtoId = new Guid("22222222-2222-2222-2222-222222222222"); // Estoque: 50

            // Act
            var resultado = await _service.VerificarDisponibilidadeAsync(produtoId, 100);

            // Assert
            Assert.False(resultado);
        }

        [Fact]
        public async Task VerificarDisponibilidadeAsync_ProdutoInexistente_RetornaFalse()
        {
            // Arrange
            var produtoId = Guid.NewGuid();

            // Act
            var resultado = await _service.VerificarDisponibilidadeAsync(produtoId, 1);

            // Assert
            Assert.False(resultado);
        }

        [Fact]
        public async Task ReservarEstoqueAsync_EstoqueDisponivel_RetornaTrueEReduzEstoque()
        {
            // Arrange
            var produtoId = new Guid("11111111-1111-1111-1111-111111111111"); // Estoque inicial: 100

            // Act
            var resultado = await _service.ReservarEstoqueAsync(produtoId, 30);

            // Assert
            Assert.True(resultado);
            
            // Verificar se o estoque foi reduzido
            var estoque = await _service.ObterEstoqueProdutoAsync(produtoId);
            Assert.NotNull(estoque);
            Assert.Equal(70, estoque.QuantidadeDisponivel);
        }

        [Fact]
        public async Task ReservarEstoqueAsync_EstoqueInsuficiente_RetornaFalse()
        {
            // Arrange
            var produtoId = new Guid("22222222-2222-2222-2222-222222222222"); // Estoque: 50

            // Act
            var resultado = await _service.ReservarEstoqueAsync(produtoId, 100);

            // Assert
            Assert.False(resultado);
            
            // Verificar se o estoque não foi alterado
            var estoque = await _service.ObterEstoqueProdutoAsync(produtoId);
            Assert.NotNull(estoque);
            Assert.Equal(50, estoque.QuantidadeDisponivel);
        }

        [Fact]
        public async Task ReservarEstoqueAsync_ProdutoInexistente_RetornaFalse()
        {
            // Arrange
            var produtoId = Guid.NewGuid();

            // Act
            var resultado = await _service.ReservarEstoqueAsync(produtoId, 10);

            // Assert
            Assert.False(resultado);
        }

        [Fact]
        public async Task LiberarEstoqueAsync_ProdutoExistente_RetornaTrueEAumentaEstoque()
        {
            // Arrange
            var produtoId = new Guid("33333333-3333-3333-3333-333333333333"); // Estoque inicial: 0

            // Act
            var resultado = await _service.LiberarEstoqueAsync(produtoId, 25);

            // Assert
            Assert.True(resultado);
            
            // Verificar se o estoque foi aumentado
            var estoque = await _service.ObterEstoqueProdutoAsync(produtoId);
            Assert.NotNull(estoque);
            Assert.Equal(25, estoque.QuantidadeDisponivel);
        }

        [Fact]
        public async Task LiberarEstoqueAsync_ProdutoInexistente_RetornaFalse()
        {
            // Arrange
            var produtoId = Guid.NewGuid();

            // Act
            var resultado = await _service.LiberarEstoqueAsync(produtoId, 10);

            // Assert
            Assert.False(resultado);
        }

        [Fact]
        public async Task ObterEstoqueProdutoAsync_ProdutoExistente_RetornaEstoque()
        {
            // Arrange
            var produtoId = new Guid("11111111-1111-1111-1111-111111111111");

            // Act
            var resultado = await _service.ObterEstoqueProdutoAsync(produtoId);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(produtoId, resultado.ProdutoId);
            Assert.Equal("Produto Teste 1", resultado.NomeProduto);
            Assert.Equal(100, resultado.QuantidadeDisponivel);
        }

        [Fact]
        public async Task ObterEstoqueProdutoAsync_ProdutoInexistente_RetornaNulo()
        {
            // Arrange
            var produtoId = Guid.NewGuid();

            // Act
            var resultado = await _service.ObterEstoqueProdutoAsync(produtoId);

            // Assert
            Assert.Null(resultado);
        }

        [Fact]
        public async Task FluxoCompleto_ReservarELiberar_FuncionaCorretamente()
        {
            // Arrange
            var produtoId = new Guid("22222222-2222-2222-2222-222222222222"); // Estoque inicial: 50

            // Act 1 - Reservar estoque
            var reservaOk = await _service.ReservarEstoqueAsync(produtoId, 20);
            var estoqueAposReserva = await _service.ObterEstoqueProdutoAsync(produtoId);

            // Act 2 - Liberar estoque
            var liberacaoOk = await _service.LiberarEstoqueAsync(produtoId, 20);
            var estoqueAposLiberacao = await _service.ObterEstoqueProdutoAsync(produtoId);

            // Assert
            Assert.True(reservaOk);
            Assert.NotNull(estoqueAposReserva);
            Assert.Equal(30, estoqueAposReserva.QuantidadeDisponivel);

            Assert.True(liberacaoOk);
            Assert.NotNull(estoqueAposLiberacao);
            Assert.Equal(50, estoqueAposLiberacao.QuantidadeDisponivel);
        }
    }
}
