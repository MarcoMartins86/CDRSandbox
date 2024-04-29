using System.Text.Json;

namespace CDRSandboxTests.Base;

public abstract class TestServerBase : RandomDataGeneratorsBase, IDisposable, IAsyncDisposable
{
    // TODO: for now there's not problem, but if more than one class inherits this base one, this should be a shared instance
    private CustomWebApplicationFactory _webApplicationFactory;
    private JsonSerializerOptions _serverNamingPolicy = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    
    protected HttpClient GetClient() => _webApplicationFactory.CreateClient();

    protected TestServerBase()
    {
        _webApplicationFactory = new(ConfigServices);
    }

    protected virtual void ConfigServices(IServiceCollection services)
    {
    }
    
    protected T? Deserialize<T>(string? json) where T : class
    {
        return string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<T>(json, _serverNamingPolicy);
    }

    public void Dispose()
    {
        _webApplicationFactory.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _webApplicationFactory.DisposeAsync();
    }
}