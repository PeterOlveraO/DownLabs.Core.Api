namespace DownLabs.Core.Api.Models;

public class PedidoRaw
{
    public Guid id_pedido { get; set; }
    public Guid id_cliente { get; set; }
    public string contenido_raw { get; set; } = string.Empty;
    public DateTime? created_at { get; set; }
    public string estado { get; set; } = "pendiente";
}
