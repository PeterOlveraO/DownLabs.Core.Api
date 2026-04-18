using System.Net.Http.Headers;
using System.Text.Json;

namespace DownLabs.Core.Api.Services;

public class CrudService : ICrudService
{
    private readonly string _url;
    private readonly string _key;
    private readonly HttpClient _httpClient;

    public CrudService(IConfiguration configuration)
    {
        _url = configuration["Supabase:Url"] 
            ?? throw new InvalidOperationException("Supabase:Url configuration is missing");
        
        _key = configuration["Supabase:Key"] 
            ?? throw new InvalidOperationException("Supabase:Key configuration is missing");
        
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        
        ConfigureHeaders();
    }

    private void ConfigureHeaders()
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("apikey", _key);
        _httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {_key}");
    }

    public async Task<List<T>> GetAllAsync<T>(string tableName, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_url}/rest/v1/{tableName}", cancellationToken)
                .ConfigureAwait(false);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                throw new InvalidOperationException($"Error al obtener {tableName}: {error}");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
        }
        catch (OperationCanceledException)
        {
            throw new OperationCanceledException($"Operacion cancelada al obtener {tableName}");
        }
    }

    public async Task<T?> GetByIdAsync<T>(string tableName, string idColumn, Guid id, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var url = $"{_url}/rest/v1/{tableName}?{idColumn}=eq.{id}";
            var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                throw new InvalidOperationException($"Error al obtener {tableName} por ID: {error}");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var results = JsonSerializer.Deserialize<List<T>>(json);
            return results?.FirstOrDefault();
        }
        catch (OperationCanceledException)
        {
            throw new OperationCanceledException($"Operacion cancelada al obtener {tableName} por ID");
        }
    }

    public async Task<T> CreateAsync<T>(string tableName, object data, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(data),
                System.Text.Encoding.UTF8,
                "application/json"
            );
            content.Headers.Add("Prefer", "return=representation");

            var response = await _httpClient.PostAsync($"{_url}/rest/v1/{tableName}", content, cancellationToken)
                .ConfigureAwait(false);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                throw new InvalidOperationException($"Error al crear en {tableName}: {error}");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<List<T>>(json)?.FirstOrDefault() 
                ?? throw new InvalidOperationException($"No se pudo obtener el registro creado de {tableName}");
        }
        catch (OperationCanceledException)
        {
            throw new OperationCanceledException($"Operacion cancelada al crear en {tableName}");
        }
    }

    public async Task<T> UpdateAsync<T>(string tableName, string idColumn, Guid id, object data, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(data),
                System.Text.Encoding.UTF8,
                "application/json"
            );
            content.Headers.Add("Prefer", "return=representation");

            var url = $"{_url}/rest/v1/{tableName}?{idColumn}=eq.{id}";
            var response = await _httpClient.PatchAsync(url, content, cancellationToken).ConfigureAwait(false);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                throw new InvalidOperationException($"Error al actualizar {tableName}: {error}");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<List<T>>(json)?.FirstOrDefault() 
                ?? throw new InvalidOperationException($"No se pudo obtener el registro actualizado de {tableName}");
        }
        catch (OperationCanceledException)
        {
            throw new OperationCanceledException($"Operacion cancelada al actualizar {tableName}");
        }
    }

    public async Task<bool> DeleteAsync(string tableName, string idColumn, Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_url}/rest/v1/{tableName}?{idColumn}=eq.{id}";
            var response = await _httpClient.DeleteAsync(url, cancellationToken).ConfigureAwait(false);
            
            return response.IsSuccessStatusCode;
        }
        catch (OperationCanceledException)
        {
            throw new OperationCanceledException($"Operacion cancelada al eliminar de {tableName}");
        }
    }
}