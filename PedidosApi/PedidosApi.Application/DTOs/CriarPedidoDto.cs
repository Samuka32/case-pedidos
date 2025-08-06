using System.ComponentModel.DataAnnotations;

namespace PedidosApi.Application.DTOs;

public class CriarPedidoDto
{
    [Required(ErrorMessage = "ProdutoId é obrigatório")]
    public Guid ProdutoId { get; set; }

    [Required(ErrorMessage = "Descrição é obrigatória")]
    [StringLength(500, ErrorMessage = "Descrição deve ter no máximo 500 caracteres")]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "Quantidade é obrigatória")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero")]
    public int Quantidade { get; set; }

    [Required(ErrorMessage = "Preço unitário é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Preço unitário deve ser maior que zero")]
    public decimal PrecoUnitario { get; set; }
}
