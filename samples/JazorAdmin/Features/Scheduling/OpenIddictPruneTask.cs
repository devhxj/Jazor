using OpenIddict.Abstractions;

namespace JazorAdmin.Features.Scheduling;

public sealed class OpenIddictPruneTask(
    IOpenIddictTokenManager tokens,
    IOpenIddictAuthorizationManager authorizations) : IManagedTask
{
    public string Key => "openid-prune";

    public async Task<string> ExecuteAsync(CancellationToken cancellationToken)
    {
        var threshold = DateTimeOffset.UtcNow.AddDays(-14);
        // OpenIddict requires token cleanup before authorization cleanup because active token links
        // protect the owning authorization from deletion.
        // OpenIddict 要求先清理令牌，仍有关联令牌的授权记录不能提前删除。
        var tokenCount = await tokens.PruneAsync(threshold, cancellationToken);
        var authorizationCount = await authorizations.PruneAsync(threshold, cancellationToken);
        return $"Pruned {tokenCount} token(s) and {authorizationCount} authorization(s).";
    }
}
