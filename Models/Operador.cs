namespace DownLabs.Core.Api.Models;

public class Operador
{
    public Guid id { get; set; }
    public string? nombre { get; set; }
    public string? apellido { get; set; }
    public string? email { get; set; }
    public string? telefono { get; set; }
    public bool activo { get; set; }
    public string? contrasena { get; set; }
    public string? contrasena_hash { get; set; }
    public DateTime? created_at { get; set; }
    public DateTime? updated_at { get; set; }
}