using System.Text.Json.Serialization;

namespace DownLabs.Core.Api.Models;

public class Operador
{
    [JsonPropertyName("id_operadores")]
    public Guid id_operadores { get; set; }
    
    public string nombre { get; set; } = string.Empty;
    public string apellido { get; set; } = string.Empty;
    public string? email { get; set; }
    public string? telefono { get; set; }
    public bool activo { get; set; } = true;
    public DateTime? created_at { get; set; }
    public DateTime? updated_at { get; set; }
}