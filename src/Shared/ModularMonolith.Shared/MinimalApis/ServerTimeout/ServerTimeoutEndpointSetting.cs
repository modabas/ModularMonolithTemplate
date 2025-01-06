namespace ModularMonolith.Shared.MinimalApis.ServerTimeout;

public record ServerTimeoutEndpointSetting(bool Exists, TimeSpan Timeout);
