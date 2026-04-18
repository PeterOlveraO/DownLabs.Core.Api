namespace DownLabs.Core.Api.Services;

public interface ICrudService
{
    Task<List<T>> GetAllAsync<T>(string tableName, CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync<T>(string tableName, string idColumn, Guid id, CancellationToken cancellationToken = default) where T : class;
    Task<T> CreateAsync<T>(string tableName, object data, CancellationToken cancellationToken = default) where T : class;
    Task<T> UpdateAsync<T>(string tableName, string idColumn, Guid id, object data, CancellationToken cancellationToken = default) where T : class;
    Task<bool> DeleteAsync(string tableName, string idColumn, Guid id, CancellationToken cancellationToken = default);
}