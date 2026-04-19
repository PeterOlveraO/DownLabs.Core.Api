namespace DownLabs.Core.Api.Models;

public class Mayorista
{
    public Guid id_mayorista { get; set; }
    public string nombre_empresa { get; set; } = string.Empty;
    public string? ubicacion { get; set; }
    public int? nivel_confianza { get; set; }
    public DateTime? created_at { get; set; }
    public DateTime? updated_at { get; set; }
    public string? correo_contacto { get; set; }
    public string? telefono_contacto { get; set; }
    public string? rfc { get; set; }
    public string? nombre_operador { get; set; }
}