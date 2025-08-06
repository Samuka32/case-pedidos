using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PedidosApi.Application.DTOs;

public class PedidoDto
{
    public Guid Id { get; set; }
    public Guid ProdutoId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal ValorTotal { get; set; }
    public DateTime DataCriacao { get; set; }
    public bool Ativo { get; set; }
}

