namespace DownLabs.Core.Api.Models;

public class PedidoCredito
{
    public Guid id_pedido { get; set; }
    public Guid? id_cotizacion { get; set; }
    public bool requiere_credito { get; set; }
    public decimal cargo_financiamiento { get; set; }
    public decimal monto_total_deuda { get; set; }
    public string estado_pago { get; set; } = "Pendiente";
    public DateTime? fecha_inicio_credito { get; set; }
    public DateTime? fecha_vencimiento_credito { get; set; }
    public DateTime? created_at { get; set; }
    public DateTime? updated_at { get; set; }
    public bool requiere_factura { get; set; }
    public string? tipo_pago { get; set; }
}