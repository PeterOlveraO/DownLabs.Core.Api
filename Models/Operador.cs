namespace DownLabs.Core.Api.Models;

public class Operador
{
    public Guid id_operador { get; set; }
    public string? nombre { get; set; }
    public string? correo { get; set; }
    public string? contrasena { get; set; }
    public string? rol { get; set; }
    public DateTime? created_at { get; set; }
    public DateTime? updated_at { get; set; }
}