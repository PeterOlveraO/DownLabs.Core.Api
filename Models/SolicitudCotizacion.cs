namespace DownLabs.Core.Api.Models;

public class SolicitudCotizacion
{
    public Guid id_solicitud_cotizacion { get; set; }
    public Guid? id_cliente { get; set; }
    public string? nivel_urgencia { get; set; }
    public string estado_solicitud { get; set; } = "pendiente";
    public DateTime? created_at { get; set; }
    public DateTime? updated_at { get; set; }
    public Guid? id_operador { get; set; }
    public object? productos { get; set; }
    public Guid? id_pedido { get; set; }
}
