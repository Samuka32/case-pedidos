using System.Text.Json.Serialization;

namespace PedidosApi.Domain.Entities;

public class Pedido
{
    [JsonPropertyName("Id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonPropertyName("ProdutoId")]
    public Guid ProdutoId { get; set; }

    [JsonPropertyName("Descricao")]
    public string Descricao { get; set; } = string.Empty;

    [JsonPropertyName("Quantidade")]
    public int Quantidade { get; set; }

    [JsonPropertyName("PrecoUnitario")]
    public decimal PrecoUnitario { get; set; }

    [JsonPropertyName("DataCriacao")]
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("Ativo")]
    public bool Ativo { get; set; } = true;

    [JsonPropertyName("ValorTotal")]
    public decimal ValorTotal { get; set; }

    public Pedido() { }

    public Pedido(Guid produtoId, string descricao, int quantidade, decimal precoUnitario)
    {
        ProdutoId = produtoId;
        Descricao = descricao;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
        ValorTotal = quantidade * precoUnitario;
    }
}