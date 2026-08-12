namespace Jazor.AspNetCore;

/// <summary>Executes a generated Vue module in the local Jazor SSR runtime.</summary>
public interface IJazorSsrRenderer
{
    Task<JazorSsrRenderResult> RenderAsync(
        JazorSsrRequest request,
        CancellationToken cancellationToken = default);
}
