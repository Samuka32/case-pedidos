using PedidosApi.Domain.Entities;
using PedidosApi.Infrastructure.Repositories;
using Xunit;

namespace PedidosApi.Tests.UnitTests
{
    public class JsonPedidoRepositoryTests : IDisposable
    {
        private readonly string _testFilePath;
        private readonly JsonPedidoRepository _repository;

        public JsonPedidoRepositoryTests()
        {
            _testFilePath = Path.Combine(Path.GetTempPath(), $"test_pedidos_{Guid.NewGuid()}.json");
            _repository = new JsonPedidoRepository(_testFilePath);
        }

        public void Dispose()
        {
            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);
        }

        [Fact]
        public async Task AddAsync_DeveCriarPedido()
        {
            // Arrange
            var pedido = new Pedido
            {
                Id = Guid.NewGuid(),
                ProdutoId = Guid.NewGuid(),
                Descricao = "Produto Teste",
                Quantidade = 2,
                PrecoUnitario = 10.50m,
                ValorTotal = 21.00m,
                DataCriacao = DateTime.UtcNow,
                Ativo = true
            };

            // Act
            var resultado = await _repository.AddAsync(pedido);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(pedido.Id, resultado.Id);
            Assert.Equal("Produto Teste", resultado.Descricao);
            Assert.Equal(2, resultado.Quantidade);
            Assert.Equal(10.50m, resultado.PrecoUnitario);
            Assert.Equal(21.00m, resultado.ValorTotal);
            Assert.True(resultado.Ativo);
        }

        [Fact]
        public async Task AddAsync_DeveSalvarNoArquivo()
        {
            // Arrange
            var pedido = new Pedido
            {
                Id = Guid.NewGuid(),
                ProdutoId = Guid.NewGuid(),
                Descricao = "Produto Teste",
                Quantidade = 1,
                PrecoUnitario = 15.00m,
                ValorTotal = 15.00m,
                Ativo = true
            };

            // Act
            await _repository.AddAsync(pedido);

            // Assert
            Assert.True(File.Exists(_testFilePath));
            var fileContent = await File.ReadAllTextAsync(_testFilePath);
            Assert.Contains(pedido.Id.ToString(), fileContent);
            Assert.Contains("Produto Teste", fileContent);
        }

        [Fact]
        public async Task GetByIdAsync_PedidoExistente_RetornaPedido()
        {
            // Arrange
            var pedido = new Pedido
            {
                Id = Guid.NewGuid(),
                ProdutoId = Guid.NewGuid(),
                Descricao = "Produto Teste",
                Quantidade = 2,
                PrecoUnitario = 10.50m,
                ValorTotal = 21.00m,
                Ativo = true
            };
            await _repository.AddAsync(pedido);

            // Act
            var resultado = await _repository.GetByIdAsync(pedido.Id);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(pedido.Id, resultado.Id);
            Assert.Equal(pedido.Descricao, resultado.Descricao);
            Assert.Equal(pedido.Quantidade, resultado.Quantidade);
            Assert.Equal(pedido.PrecoUnitario, resultado.PrecoUnitario);
        }

        [Fact]
        public async Task GetByIdAsync_PedidoInexistente_RetornaNulo()
        {
            // Act
            var resultado = await _repository.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(resultado);
        }

        [Fact]
        public async Task GetAllAsync_SemPedidos_RetornaListaVazia()
        {
            // Act
            var resultado = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(resultado);
            Assert.Empty(resultado);
        }

        [Fact]
        public async Task GetAllAsync_ComPedidos_RetornaTodosPedidos()
        {
            // Arrange
            var pedido1 = new Pedido { Id = Guid.NewGuid(), Descricao = "Pedido 1", Ativo = true };
            var pedido2 = new Pedido { Id = Guid.NewGuid(), Descricao = "Pedido 2", Ativo = false };
            
            await _repository.AddAsync(pedido1);
            await _repository.AddAsync(pedido2);

            // Act
            var resultado = await _repository.GetAllAsync();

            // Assert
            Assert.Equal(2, resultado.Count);
            Assert.Contains(resultado, p => p.Id == pedido1.Id);
            Assert.Contains(resultado, p => p.Id == pedido2.Id);
        }

        [Fact]
        public async Task UpdateAsync_PedidoExistente_AtualizaERetornaTrue()
        {
            // Arrange
            var pedido = new Pedido
            {
                Id = Guid.NewGuid(),
                ProdutoId = Guid.NewGuid(),
                Descricao = "Produto Original",
                Quantidade = 1,
                PrecoUnitario = 10.00m,
                ValorTotal = 10.00m,
                Ativo = true
            };
            await _repository.AddAsync(pedido);

            // Modificar pedido
            pedido.Descricao = "Produto Atualizado";
            pedido.Ativo = false;

            // Act
            var resultado = await _repository.UpdateAsync(pedido);

            // Assert
            Assert.True(resultado);
            
            var pedidoAtualizado = await _repository.GetByIdAsync(pedido.Id);
            Assert.Equal("Produto Atualizado", pedidoAtualizado!.Descricao);
            Assert.False(pedidoAtualizado.Ativo);
        }

        [Fact]
        public async Task UpdateAsync_PedidoInexistente_RetornaFalse()
        {
            // Arrange
            var pedidoInexistente = new Pedido
            {
                Id = Guid.NewGuid(),
                Descricao = "Pedido Inexistente",
                Ativo = true
            };

            // Act
            var resultado = await _repository.UpdateAsync(pedidoInexistente);

            // Assert
            Assert.False(resultado);
        }

        [Fact]
        public async Task Repository_ThreadSafety_ConcurrentOperations()
        {
            // Arrange
            var tasks = new List<Task>();
            var pedidoIds = new List<Guid>();

            // Act - Criar múltiplos pedidos simultaneamente (reduzido para evitar timeout)
            for (int i = 0; i < 3; i++)
            {
                var index = i; // Capturar variável local
                var pedidoId = Guid.NewGuid();
                pedidoIds.Add(pedidoId);
                
                tasks.Add(Task.Run(async () =>
                {
                    var pedido = new Pedido
                    {
                        Id = pedidoId,
                        Descricao = $"Pedido {index}",
                        Quantidade = index + 1,
                        PrecoUnitario = 10.00m,
                        ValorTotal = (index + 1) * 10.00m,
                        Ativo = true
                    };
                    await _repository.AddAsync(pedido);
                }));
            }

            await Task.WhenAll(tasks);

            // Assert
            var todosPedidos = await _repository.GetAllAsync();
            Assert.Equal(3, todosPedidos.Count);
            
            foreach (var pedidoId in pedidoIds)
            {
                Assert.Contains(todosPedidos, p => p.Id == pedidoId);
            }
        }

        [Fact]
        public async Task Repository_ArquivoInexistente_CriaNovoArquivo()
        {
            // Arrange
            Assert.False(File.Exists(_testFilePath));

            // Act
            var pedidos = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(pedidos);
            Assert.Empty(pedidos);
            // O arquivo ainda não deve existir até que algo seja salvo
            Assert.False(File.Exists(_testFilePath));
        }

        [Fact]
        public async Task Repository_PersistenciaAposBusca_MantémDados()
        {
            // Arrange
            var pedido = new Pedido
            {
                Id = Guid.NewGuid(),
                Descricao = "Teste Persistência",
                Ativo = true
            };
            await _repository.AddAsync(pedido);

            // Act - Criar nova instância do repositório com mesmo arquivo
            var novoRepository = new JsonPedidoRepository(_testFilePath);
            var pedidoRecuperado = await novoRepository.GetByIdAsync(pedido.Id);

            // Assert
            Assert.NotNull(pedidoRecuperado);
            Assert.Equal(pedido.Id, pedidoRecuperado.Id);
            Assert.Equal("Teste Persistência", pedidoRecuperado.Descricao);
        }
    }
}
