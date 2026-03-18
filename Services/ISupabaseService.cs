using DownLabs.Core.Api.Models;

namespace DownLabs.Core.Api.Services;

public interface ISupabaseService
{
    Task<string> TestConnectionAsync();
    Task<bool> RegisterClientAsync(string nombreEmpresa, string correo, string contrasena);
    Task<Cliente?> LoginClientAsync(string correo, string contrasena);
}
