namespace DownLabs.Core.Api.Models;

public class SolicitudMayorista
{
    public Guid id_solicitud_mayorista { get; set; }
    public Guid? id_solicitud { get; set; }
    public Guid? id_mayorista { get; set; }
    public object? productos { get; set; }
    public string estado { get; set; } = "pendiente";
    public DateTime? created_at { get; set; }
    public DateTime? updated_at { get; set; }
    public DateTime? respondido_at { get; set; }
    public decimal? costo_envio        { get; set; }
    public int?     tiempo_entrega_dias { get; set; }
    public string?  notas_adicionales   { get; set; }
}
