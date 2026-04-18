namespace DownLabs.Core.Api.Models;

public class SolicitudCotizacion
{
    public Guid id_solicitud { get; set; }
    public Guid? id_cliente { get; set; }
    public string? descripcion_articulo { get; set; }
    public object? especificaciones_requeridas { get; set; }
    public string nivel_urgencia { get; set; } = "Normal";
    public string estado_solicitud { get; set; } = "Pendiente";
    public DateTime? created_at { get; set; }
    public DateTime? updated_at { get; set; }
    public Guid? id_operador { get; set; }
}