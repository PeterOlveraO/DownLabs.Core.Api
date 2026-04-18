namespace DownLabs.Core.Api.Models;

public class CatalogoProducto
{
    public Guid id_producto { get; set; }
    public Guid? id_mayorista { get; set; }
    public string? nombre_articulo { get; set; }
    public string? categoria { get; set; }
    public decimal? precio_mayorista { get; set; }
    public string? materiales { get; set; }
    public int? moq { get; set; }
    public DateTime? created_at { get; set; }
    public DateTime? updated_at { get; set; }
    public decimal? precio_dwlabs { get; set; }
}