namespace Jazor.AspNetCore;

/// <summary>Executes a generated Vue module in the local Jazor SSR runtime.</summary>
public interface IJazorSSRRenderer
{
    Task<JazorSSRRenderResult> RenderAsync(
        JazorSSRRequest request,
        CancellationToken cancellationToken = default);
}
