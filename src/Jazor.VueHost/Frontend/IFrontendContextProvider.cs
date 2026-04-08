using Jazor.VueContracts.Protocol;

namespace Jazor.VueHost.Frontend;

public interface IFrontendContextProvider
{
    ValueTask<GetFrontendContextResponse> GetFrontendContextAsync(
        GetFrontendContextRequest request,
        CancellationToken cancellationToken);
}
