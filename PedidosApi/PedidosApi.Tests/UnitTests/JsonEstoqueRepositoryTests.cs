using PedidosApi.Domain.Entities;
using PedidosApi.Infrastructure.Repositories;
using Xunit;

namespace PedidosApi.Tests.UnitTests
{
    public class JsonEstoqueRepositoryTests : IDisposable
    {
        private readonly string _testFilePath;
        private readonly JsonEstoqueRepository _repository;

        public JsonEstoqueRepositoryTests()
        {
            _testFilePath = Path.Combine(Path.GetTempPath(), $"test_estoque_{Guid.NewGuid()}.json");
            _repository = new JsonEstoqueRepository(_testFilePath);
        }

        public void Dispose()
        {
            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);
        }

        [Fact]
        public async Task GetAllAsync_ArquivoInexistente_RetornaListaVazia()
        {
            // Act
            var resultado = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(resultado);
            Assert.Empty(resultado);
        }

        [Fact]
        public async Task GetAllAsync_ComEstoques_RetornaTodosEstoques()
        {
            // Arrange
            var estoque1 = new Estoque { ProdutoId = Guid.NewGuid(), NomeProduto = "Produto 1", QuantidadeDisponivel = 10 };
            var estoque2 = new Estoque { ProdutoId = Guid.NewGuid(), NomeProduto = "Produto 2", QuantidadeDisponivel = 20 };
            
            CriarArquivoEstoque(new List<Estoque> { estoque1, estoque2 });

            // Act
            var resultado = await _repository.GetAllAsync();

            // Assert
            Assert.Equal(2, resultado.Count);
            Assert.Contains(resultado, e => e.ProdutoId == estoque1.ProdutoId);
            Assert.Contains(resultado, e => e.ProdutoId == estoque2.ProdutoId);
        }

        [Fact]
        public async Task GetByProdutoIdAsync_ProdutoExistente_RetornaEstoque()
        {
            // Arrange
            var produtoId = Guid.NewGuid();
            var estoque = new Estoque 
            { 
                ProdutoId = produtoId, 
                NomeProduto = "Produto Teste", 
                QuantidadeDisponivel = 50 
            };
            
            CriarArquivoEstoque(new List<Estoque> { estoque });

            // Act
            var resultado = await _repository.GetByProdutoIdAsync(produtoId);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(produtoId, resultado.ProdutoId);
            Assert.Equal("Produto Teste", resultado.NomeProduto);
            Assert.Equal(50, resultado.QuantidadeDisponivel);
        }

        [Fact]
        public async Task GetByProdutoIdAsync_ProdutoInexistente_RetornaNulo()
        {
            // Arrange
            var produtoIdExistente = Guid.NewGuid();
            var produtoIdInexistente = Guid.NewGuid();
            var estoque = new Estoque { ProdutoId = produtoIdExistente, NomeProduto = "Produto", QuantidadeDisponivel = 10 };
            
            CriarArquivoEstoque(new List<Estoque> { estoque });

            // Act
            var resultado = await _repository.GetByProdutoIdAsync(produtoIdInexistente);

            // Assert
            Assert.Null(resultado);
        }

        [Fact]
        public async Task UpdateAsync_EstoqueExistente_AtualizaERetornaTrue()
        {
            // Arrange
            var produtoId = Guid.NewGuid();
            var estoqueOriginal = new Estoque 
            { 
                ProdutoId = produtoId, 
                NomeProduto = "Produto Original", 
                QuantidadeDisponivel = 100 
            };
            
            CriarArquivoEstoque(new List<Estoque> { estoqueOriginal });

            var estoqueAtualizado = new Estoque 
            { 
                ProdutoId = produtoId, 
                NomeProduto = "Produto Atualizado", 
                QuantidadeDisponivel = 75 
            };

            // Act
            var resultado = await _repository.UpdateAsync(estoqueAtualizado);

            // Assert
            Assert.True(resultado);
            
            // Verificar se foi realmente atualizado
            var estoqueVerificacao = await _repository.GetByProdutoIdAsync(produtoId);
            Assert.NotNull(estoqueVerificacao);
            Assert.Equal("Produto Atualizado", estoqueVerificacao.NomeProduto);
            Assert.Equal(75, estoqueVerificacao.QuantidadeDisponivel);
        }

        [Fact]
        public async Task UpdateAsync_EstoqueInexistente_RetornaFalse()
        {
            // Arrange
            var produtoIdExistente = Guid.NewGuid();
            var produtoIdInexistente = Guid.NewGuid();
            var estoqueExistente = new Estoque { ProdutoId = produtoIdExistente, NomeProduto = "Produto", QuantidadeDisponivel = 10 };
            
            CriarArquivoEstoque(new List<Estoque> { estoqueExistente });

            var estoqueInexistente = new Estoque 
            { 
                ProdutoId = produtoIdInexistente, 
                NomeProduto = "Produto Inexistente", 
                QuantidadeDisponivel = 50 
            };

            // Act
            var resultado = await _repository.UpdateAsync(estoqueInexistente);

            // Assert
            Assert.False(resultado);
        }

        [Fact]
        public async Task Repository_ThreadSafety_ConcurrentOperations()
        {
            // Arrange
            var produtoId = Guid.NewGuid();
            var estoque = new Estoque { ProdutoId = produtoId, NomeProduto = "Produto Concorrente", QuantidadeDisponivel = 100 };
            CriarArquivoEstoque(new List<Estoque> { estoque });

            var tasks = new List<Task>();

            // Act - Múltiplas operações simultâneas de atualização
            for (int i = 0; i < 5; i++)
            {
                var index = i; // Capturar variável local
                tasks.Add(Task.Run(async () =>
                {
                    var estoqueAtualizado = new Estoque 
                    { 
                        ProdutoId = produtoId, 
                        NomeProduto = $"Produto Atualizado {index}", 
                        QuantidadeDisponivel = 100 - (index * 10) 
                    };
                    await _repository.UpdateAsync(estoqueAtualizado);
                }));
            }

            await Task.WhenAll(tasks);

            // Assert - Verificar que o estoque ainda existe e foi atualizado
            var estoqueAtualizado = await _repository.GetByProdutoIdAsync(produtoId);
            Assert.NotNull(estoqueAtualizado);
            Assert.Equal(produtoId, estoqueAtualizado.ProdutoId);
        }

        [Fact]
        public async Task Repository_PersistenciaAposBusca_MantémDados()
        {
            // Arrange
            var produtoId = Guid.NewGuid();
            var estoque = new Estoque 
            { 
                ProdutoId = produtoId, 
                NomeProduto = "Teste Persistência", 
                QuantidadeDisponivel = 42 
            };
            
            CriarArquivoEstoque(new List<Estoque> { estoque });

            // Act - Criar nova instância do repositório com mesmo arquivo
            var novoRepository = new JsonEstoqueRepository(_testFilePath);
            var estoqueRecuperado = await novoRepository.GetByProdutoIdAsync(produtoId);

            // Assert
            Assert.NotNull(estoqueRecuperado);
            Assert.Equal(produtoId, estoqueRecuperado.ProdutoId);
            Assert.Equal("Teste Persistência", estoqueRecuperado.NomeProduto);
            Assert.Equal(42, estoqueRecuperado.QuantidadeDisponivel);
        }

        [Fact]
        public async Task Repository_ArquivoJson_FormatoCorreto()
        {
            // Arrange
            var estoque = new Estoque 
            { 
                ProdutoId = Guid.Parse("11111111-1111-1111-1111-111111111111"), 
                NomeProduto = "Produto JSON", 
                QuantidadeDisponivel = 25 
            };

            CriarArquivoEstoque(new List<Estoque> { estoque });

            // Act
            await _repository.GetAllAsync(); // Força a leitura do arquivo

            // Assert - Verificar se o arquivo existe e tem conteúdo válido
            Assert.True(File.Exists(_testFilePath));
            var jsonContent = await File.ReadAllTextAsync(_testFilePath);
            Assert.Contains("11111111-1111-1111-1111-111111111111", jsonContent);
            Assert.Contains("Produto JSON", jsonContent);
            Assert.Contains("25", jsonContent);
        }

        [Fact]
        public async Task UpdateAsync_QuantidadeZero_AtualizaCorretamente()
        {
            // Arrange
            var produtoId = Guid.NewGuid();
            var estoque = new Estoque { ProdutoId = produtoId, NomeProduto = "Produto", QuantidadeDisponivel = 50 };
            CriarArquivoEstoque(new List<Estoque> { estoque });

            var estoqueZerado = new Estoque { ProdutoId = produtoId, NomeProduto = "Produto", QuantidadeDisponivel = 0 };

            // Act
            var resultado = await _repository.UpdateAsync(estoqueZerado);

            // Assert
            Assert.True(resultado);
            
            var estoqueVerificacao = await _repository.GetByProdutoIdAsync(produtoId);
            Assert.NotNull(estoqueVerificacao);
            Assert.Equal(0, estoqueVerificacao.QuantidadeDisponivel);
        }

        [Fact]
        public async Task GetAllAsync_EstoqueComQuantidadesVariadas_RetornaCompleto()
        {
            // Arrange
            var estoques = new List<Estoque>
            {
                new() { ProdutoId = Guid.NewGuid(), NomeProduto = "Produto A", QuantidadeDisponivel = 0 },
                new() { ProdutoId = Guid.NewGuid(), NomeProduto = "Produto B", QuantidadeDisponivel = 1 },
                new() { ProdutoId = Guid.NewGuid(), NomeProduto = "Produto C", QuantidadeDisponivel = 1000 },
                new() { ProdutoId = Guid.NewGuid(), NomeProduto = "Produto D", QuantidadeDisponivel = 999999 }
            };

            CriarArquivoEstoque(estoques);

            // Act
            var resultado = await _repository.GetAllAsync();

            // Assert
            Assert.Equal(4, resultado.Count);
            Assert.Contains(resultado, e => e.QuantidadeDisponivel == 0);
            Assert.Contains(resultado, e => e.QuantidadeDisponivel == 1);
            Assert.Contains(resultado, e => e.QuantidadeDisponivel == 1000);
            Assert.Contains(resultado, e => e.QuantidadeDisponivel == 999999);
        }

        private void CriarArquivoEstoque(List<Estoque> estoques)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(estoques, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(_testFilePath, json);
        }
    }
}
