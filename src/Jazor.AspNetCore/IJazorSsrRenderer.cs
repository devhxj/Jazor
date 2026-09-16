namespace Jazor.AspNetCore;

/// <summary>Executes a generated Vue module in the local Jazor SSR runtime.</summary>
public interface IJazorSsrRenderer
{
    /// <summary>在本地 Deno runtime 中渲染一个生成的 Vue 根组件。</summary>
    /// <remarks>返回组件 HTML 与用于 hydration 的 JSON 快照，不生成完整 HTTP 文档。传入 HttpContext.RequestAborted 可取消排队/渲染；模块加载、序列化与渲染失败向调用方传播。由 DI 负责 renderer 的释放。</remarks>
    /// <param name="request">根模块、props 和 provider 快照；HTTP 固定请求重载会跨请求复用此对象。</param>
    /// <param name="cancellationToken">取消排队或渲染的令牌；HTTP 调用通常传 RequestAborted。</param>
    /// <returns>异步返回 HTML 片段及对应的 JSON 状态。</returns>
    Task<JazorSsrRenderResult> RenderAsync(
        JazorSsrRequest request,
        CancellationToken cancellationToken = default);
}
