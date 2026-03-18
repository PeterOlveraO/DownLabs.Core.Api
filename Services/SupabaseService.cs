using System.Net.Http.Json;
using System.Text.Json;
using DownLabs.Core.Api.Models;
using Supabase;

namespace DownLabs.Core.Api.Services;

public class SupabaseService : ISupabaseService
{
    private readonly Client _client;
    private readonly string _url;
    private readonly string _key;

    public SupabaseService(IConfiguration configuration)
    {
        _url = configuration["Supabase:Url"] 
            ?? throw new InvalidOperationException("Supabase:Url configuration is missing");
        
        _key = configuration["Supabase:Key"] 
            ?? throw new InvalidOperationException("Supabase:Key configuration is missing");
        
        _client = new Client(_url, _key);
    }

    public async Task<string> TestConnectionAsync()
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(10);
        
        httpClient.DefaultRequestHeaders.Add("apikey", _key);
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_key}");
        
        var response = await httpClient.GetAsync($"{_url}/rest/v1/");
        
        return $"✅ Conexión exitosa a Supabase. Status: {response.StatusCode} ({response.ReasonPhrase})";
    }

    public async Task<bool> RegisterClientAsync(string nombreEmpresa, string correo, string contrasena)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("apikey", _key);
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_key}");
        httpClient.DefaultRequestHeaders.Add("Prefer", "return=representation");

        var body = new
        {
            nombre_empresa = nombreEmpresa,
            correo_contacto = correo,
            contrasena = contrasena
        };

        var content = new StringContent(
            JsonSerializer.Serialize(body),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var response = await httpClient.PostAsync($"{_url}/rest/v1/clientes", content);
        
        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        var error = await response.Content.ReadAsStringAsync();
        throw new Exception($"Error al registrar: {error}");
    }

    public async Task<Cliente?> LoginClientAsync(string correo, string contrasena)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("apikey", _key);
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_key}");

        // Busca cliente por correo y contrasena
        var url = $"{_url}/rest/v1/clientes?correo_contacto=eq.{Uri.EscapeDataString(correo)}&contrasena=eq.{Uri.EscapeDataString(contrasena)}&select=id_cliente,nombre_empresa";
        
        var response = await httpClient.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error al buscar cliente: {response.StatusCode}");
        }

        var json = await response.Content.ReadAsStringAsync();
        var clientes = JsonSerializer.Deserialize<List<Cliente>>(json);

        if (clientes != null && clientes.Count > 0)
        {
            return clientes[0];
        }

        return null;
    }
}
