namespace PedidosApi.Domain.Exceptions;

public class PedidoException : Exception
{
    public PedidoException(string message) : base(message) { }
    public PedidoException(string message, Exception innerException) : base(message, innerException) { }
}
