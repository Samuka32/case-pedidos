using PedidosApi.Domain.Entities;

namespace PedidosApi.Domain.Entities;

public class Estoque
{
    public Guid ProdutoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public int QuantidadeDisponivel { get; set; }
}
