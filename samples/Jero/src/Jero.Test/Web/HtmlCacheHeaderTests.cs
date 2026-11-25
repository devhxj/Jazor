using System.Reflection;
using Microsoft.AspNetCore.Http;
using Jero.Web;
using Xunit;

namespace Jero.Test.Web;

public class HtmlCacheHeaderTests
{
    [Fact]
    public void ApplyHtmlCacheHeaders_WithHtmlResponse_DisablesCaching()
    {
        var response = new DefaultHttpContext().Response;
        response.ContentType = "text/html; charset=utf-8";

        ApplyHtmlCacheHeaders(response);

        Assert.Equal("no-store, no-cache, must-revalidate", response.Headers.CacheControl.ToString());
        Assert.Equal("no-cache", response.Headers.Pragma.ToString());
        Assert.Equal("0", response.Headers.Expires.ToString());
    }

    [Fact]
    public void ApplyHtmlCacheHeaders_WithCssResponse_LeavesCacheHeadersUntouched()
    {
        var response = new DefaultHttpContext().Response;
        response.ContentType = "text/css";

        ApplyHtmlCacheHeaders(response);

        Assert.False(response.Headers.ContainsKey("Cache-Control"));
        Assert.False(response.Headers.ContainsKey("Pragma"));
        Assert.False(response.Headers.ContainsKey("Expires"));
    }

    private static void ApplyHtmlCacheHeaders(HttpResponse response)
    {
        var method = typeof(Startup).GetMethod(
            "ApplyHtmlCacheHeaders",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);
        method!.Invoke(null, new object[] { response });
    }
}
