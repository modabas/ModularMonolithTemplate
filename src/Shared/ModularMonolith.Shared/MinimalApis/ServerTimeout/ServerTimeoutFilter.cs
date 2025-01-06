using Microsoft.AspNetCore.Http;

namespace ModularMonolith.Shared.MinimalApis.ServerTimeout;
public class ServerTimeoutFilter : IEndpointFilter
{
  public async ValueTask<object?> InvokeAsync(
    EndpointFilterInvocationContext context,
    EndpointFilterDelegate next)
  {
    var timeout = context.HttpContext.GetEndpoint()?.Metadata
      .GetMetadata<ServerTimeoutMetadata>()?.Setting.Timeout;
    if (timeout is not null && timeout.Value != TimeSpan.Zero)
    {
      var requestCt = context.HttpContext.RequestAborted;
      using (var serverTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(requestCt))
      {
        serverTimeoutCts.CancelAfter(timeout.Value);
        //replace request cancellation token with server timeout token
        context.HttpContext.RequestAborted = serverTimeoutCts.Token;
        try
        {
          return await next(context);
        }
        finally
        {
          //restore original cancellation token
          context.HttpContext.RequestAborted = requestCt;
        }
      }
    }
    return await next(context);
  }
}
