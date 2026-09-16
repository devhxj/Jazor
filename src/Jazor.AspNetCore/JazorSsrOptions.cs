namespace Jazor.AspNetCore;

/// <summary>Configures Jazor SSR artifacts.</summary>
public sealed class JazorSsrOptions
{
    /// <summary>
    /// Overrides the generated Jazor artifact root. Relative paths are resolved from the
    /// ASP.NET Core content root. Leave empty to discover the current debug or SSR release output.
    /// </summary>
    /// <remarks>默认自动发现；SSR 目录必须包含 jazor-manifest.json、importmap.json、ssr-importmap.json 和 manifest.json，仅有 bundle.js 不够。</remarks>
    public string? ArtifactRootPath { get; set; }

    /// <summary>
    /// Overrides the browser request-path prefix for the generated artifact root, for example <c>/jazor</c>.
    /// Leave empty to derive it from the discovered output directory.
    /// </summary>
    /// <remarks>默认由发现的目录推导。必须与资源中间件 URL 前缀一致；浏览器 URL 自动加上 Request.PathBase，请勿重复填写。</remarks>
    public string? RequestPath { get; set; }

    /// <summary>Identifies the element that receives both the server HTML and client hydration.</summary>
    /// <remarks>默认 app；不得为空或包含空白字符。服务端 HTML 与浏览器 hydration 使用同一元素。</remarks>
    public string MountElementId { get; set; } = "app";

    /// <summary>
    /// Limits persistent Deno SSR workers and concurrent renders for one application instance.
    /// 默认按 CPU 数取 <c>[1, 4]</c> 区间，避免小站点无请求时预留过多运行时资源。
    /// </summary>
    /// <remarks>必须大于零；默认 CPU 数限制在 1 至 4。worker 复用模块环境，但每次渲染创建独立 Vue app。</remarks>
    public int WorkerCount { get; set; } = Math.Clamp(Environment.ProcessorCount, 1, 4);
}
