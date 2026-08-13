namespace Jazor.AspNetCore;

/// <summary>Configures Jazor SSR artifacts.</summary>
public sealed class JazorSsrOptions
{
    /// <summary>
    /// Overrides the generated Jazor artifact root. Relative paths are resolved from the
    /// ASP.NET Core content root. Leave empty to discover the current debug or SSR release output.
    /// </summary>
    public string? ArtifactRootPath { get; set; }

    /// <summary>
    /// Overrides the browser request-path prefix for the generated artifact root, for example <c>/jazor</c>.
    /// Leave empty to derive it from the discovered output directory.
    /// </summary>
    public string? RequestPath { get; set; }

    /// <summary>Identifies the element that receives both the server HTML and client hydration.</summary>
    public string MountElementId { get; set; } = "app";

    /// <summary>
    /// Limits persistent Deno SSR workers and concurrent renders for one application instance.
    /// 默认按 CPU 数取 <c>[1, 4]</c> 区间，避免小站点无请求时预留过多运行时资源。
    /// </summary>
    public int WorkerCount { get; set; } = Math.Clamp(Environment.ProcessorCount, 1, 4);
}
