using ECMAScript.Internal.VueContracts.Protocol;

namespace Jolt.Volar;

public interface IVolarContextProvider
{
    ValueTask<GetVolarContextResponse> GetVolarContextAsync(
        GetVolarContextRequest request,
        CancellationToken cancellationToken);
}
