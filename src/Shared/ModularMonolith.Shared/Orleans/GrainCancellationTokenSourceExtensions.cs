using Orleans;

namespace ModularMonolith.Shared.Orleans;

public static class GrainCancellationTokenSourceExtensions
{
  public static CancellationTokenRegistration Link(this GrainCancellationTokenSource grainCancellationTokenSource, CancellationToken cancellationToken)
  {
    //link cancellation token to grain cancellation token source for this scope, so grain cancellation token source will be canceled if cancellation token is canceled
    return cancellationToken.Register(async () => await grainCancellationTokenSource.Cancel());
  }
}
