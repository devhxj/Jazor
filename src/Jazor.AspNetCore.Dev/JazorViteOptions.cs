using Microsoft.AspNetCore.Http;

namespace Jazor.AspNetCore.Dev;

/// <summary>Configures the optional proxy from ASP.NET Core to the project's Vite server.</summary>
public sealed class JazorViteOptions
{
    /// <summary>Vite development server origin. The server is started by the project's Deno task.</summary>
    public Uri ServerOrigin { get; set; } = new("http://127.0.0.1:5173", UriKind.Absolute);

    /// <summary>Request prefix forwarded to Vite, without the host application PathBase.</summary>
    public PathString RequestPath { get; set; } = new("/jazor");

    internal Uri BuildTarget(HttpRequest request)
    {
        var origin = new Uri(ServerOrigin.ToString().TrimEnd('/') + "/", UriKind.Absolute);
        // Map() moves the prefix into PathBase. Preserve the public URL including any
        // host PathBase; the project's static Vite base must match that same URL.
        var path = request.PathBase.Add(request.Path).ToUriComponent();
        return new Uri(origin, path.TrimStart('/') + request.QueryString);
    }
}
