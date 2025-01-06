namespace ModularMonolith.Shared.MinimalApis.ServerTimeout;

public class ServerTimeoutOptions
{
  public TimeSpan DefaultTimeout { get; set; } = TimeSpan.Zero;

  private readonly IDictionary<string, ServerTimeoutOptionsEndpoint> _settings = new Dictionary<string, ServerTimeoutOptionsEndpoint>();

  public IEnumerable<ServerTimeoutOptionsEndpoint> Endpoints
  {
    get
    {
      return _settings.Select(kvp => kvp.Value);
    }
    set
    {
      _settings.Clear();
      foreach (var item in value)
      {
        _settings.TryAdd(item.Type, item);
      }
    }
  }

  public ServerTimeoutOptionsEndpoint? GetEndpointSetting(string endpointType)
  {
    _settings.TryGetValue(endpointType, out var option);
    return option;
  }
}

public record ServerTimeoutOptionsEndpoint(
  string Type,
  TimeSpan Timeout);

