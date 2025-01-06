namespace ModularMonolith.Shared.Masstransit;
public class MtOptions
{
  public string? EndpointNamePrefix { get; set; }
  public string? EntityNamePrefix { get; set; }
  public MtOptionsRabbitMq RabbitMq { get; set; } = new();
}

public class MtOptionsRabbitMq
{
  public string Host { get; set; } = "localhost";
  public ushort Port { get; set; } = 5672;
  public string VirtualHost { get; set; } = "/";
  public string Username { get; set; } = "guest";
  public string Password { get; set; } = "guest";
}
