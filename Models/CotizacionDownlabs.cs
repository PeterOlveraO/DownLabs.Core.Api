namespace DownLabs.Core.Api.Models;

public class CotizacionDownlabs
{
    public Guid id_cotizacion { get; set; }
    public Guid? id_solicitud { get; set; }
    public Guid? id_producto { get; set; }
    public decimal? precio_final_cliente { get; set; }
    public decimal? costo_envio { get; set; }
    public string? estado { get; set; }
    public DateTime? created_at { get; set; }
    public DateTime? updated_at { get; set; }
}