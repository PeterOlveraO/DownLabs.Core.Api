namespace DownLabs.Core.Api.Models;

public class Cliente
{
    public Guid id_cliente { get; set; }
    public string nombre_empresa { get; set; } = string.Empty;
    public object? historial_compras { get; set; }
    public decimal? calificacion_crediticia { get; set; }
    public DateTime? created_at { get; set; }
    public DateTime? updated_at { get; set; }
    public string? correo_contacto { get; set; }
    public string? telefono_contacto { get; set; }
    public string? rfc { get; set; }
    public string? codigo_postal_fiscal { get; set; }
    public string? contrasena { get; set; }
    public string? contrasena_hash { get; set; }
}