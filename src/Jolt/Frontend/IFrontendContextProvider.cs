using Jazor.VueContracts.Protocol;

namespace Jolt.Frontend;

public interface IFrontendContextProvider
{
    ValueTask<GetFrontendContextResponse> GetFrontendContextAsync(
        GetFrontendContextRequest request,
        CancellationToken cancellationToken);
}
