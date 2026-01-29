using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Shared.Orleans;

public class BaseGrain : Grain, IIncomingGrainCallFilter
{
  private IServiceProvider? _requestServices;
  protected IServiceProvider RequestServices => _requestServices!;

  public async Task Invoke(IIncomingGrainCallContext context)
  {
    using (var scope = ServiceProvider.CreateScope())
    {
      try
      {
        _requestServices = scope.ServiceProvider;
        await context.Invoke();
      }
      finally
      {
        _requestServices = null;
      }
    }
  }
}
