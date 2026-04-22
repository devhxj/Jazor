using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using Jolt.Build;

namespace Jolt.Test;

[TestClass]
public sealed class JoltBuildSliceFixTests
{
    [TestMethod]
    public void CssUrlRewriter_ExtractAssetReferences_IgnoresRootEscapingTraversal()
    {
        var css = """
            .hero {
              background-image: url("../../../secret/logo.png");
            }
            """;

        var references = CssUrlRewriter.ExtractAssetReferences(css, "styles/app.css");

        Assert.AreEqual(0, references.Count);
    }

    [TestMethod]
    public void CssUrlRewriter_RewriteAssetReferences_DoesNotRewriteRootEscapingTraversal()
    {
        var css = """
            .hero {
              background-image: url("../../../secret/logo.png?v=1");
            }
            """;

        var result = CssUrlRewriter.RewriteAssetReferences(
            css,
            "styles/app.css",
            [
                new AssetInfo
                {
                    FileName = "logo-1234.png",
                    FilePath = "assets/logo-1234.png",
                    OriginalPath = "/secret/logo.png",
                    Size = 32
                }
            ]);

        StringAssert.Contains(result, """url("../../../secret/logo.png?v=1")""");
        Assert.IsFalse(result.Contains("logo-1234.png", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_MinifyTrue_PreservesCalcSpacing()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "index.html"),
                """
                <html>
                <body>
                  <script type="module" src="/main.js"></script>
                </body>
                </html>
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "main.js"),
                """import "./app.css";""");
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "app.css"),
                """
                .app {
                  width: calc(100% + 1rem);
                }
                """);

            var result = await new BuildOrchestrator().BuildAsync(
                new BuildOptions
                {
                    RootDirectory = tempDir,
                    OutDir = "dist",
                    SourceMap = SourceMapOption.None,
                    Minify = true,
                    CodeSplitting = false
                },
                CancellationToken.None);

            Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message)));
            var cssAsset = result.CssAssets.Single();
            var cssContent = await File.ReadAllTextAsync(Path.Combine(tempDir, cssAsset.FilePath.Replace('/', Path.DirectorySeparatorChar)));
            StringAssert.Contains(cssContent, "calc(100% + 1rem)");
            Assert.IsFalse(cssContent.Contains("calc(100%+1rem)", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BundlerModuleProxyServer_ProxyAsync_SanitizesUpstreamErrorBody()
    {
        await using var upstream = await TestHttpServer.StartAsync(async context =>
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var buffer = Encoding.UTF8.GetBytes("stack trace: upstream exploded");
            context.Response.ContentType = "text/plain";
            await context.Response.OutputStream.WriteAsync(buffer);
        });
        await using var proxy = await BundlerModuleProxyServer.StartAsync(
            new Uri(upstream.BaseAddress + "entry.js"),
            CancellationToken.None);
        using var client = new HttpClient();

        var response = await client.GetAsync(new Uri(proxy.ListeningUri, "/entry.js"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.ContentType!.ToString());
        StringAssert.Contains(body, "Bundler upstream request failed");
        Assert.IsFalse(body.Contains("stack trace: upstream exploded", StringComparison.Ordinal));
    }

    [TestMethod]
    public void JavaScriptModuleSpecifierScanner_RewriteSpecifiers_IgnoresStringsCommentsAndTemplateLiterals()
    {
        var source = """
            const fakeString = "import('./Fake.jazor')";
            // export { Fake } from "./Commented.vue";
            /* import SideEffect from "./Block.jazor"; */
            const template = `import("./Template.vue")`;
            import Counter from "./Counter.jazor";
            export { helper } from "./helper.vue";
            const Lazy = () => import("./Lazy.vue?raw");
            """;

        var rewritten = JavaScriptModuleSpecifierScanner.RewriteSpecifiers(
            source,
            specifier => specifier.Value.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase)
                || specifier.Value.EndsWith(".vue", StringComparison.OrdinalIgnoreCase)
                || specifier.Value.Contains(".vue?", StringComparison.OrdinalIgnoreCase)
                    ? specifier.Value + ".js"
                    : null);

        StringAssert.Contains(rewritten, """const fakeString = "import('./Fake.jazor')";""");
        StringAssert.Contains(rewritten, """// export { Fake } from "./Commented.vue";""");
        StringAssert.Contains(rewritten, """/* import SideEffect from "./Block.jazor"; */""");
        StringAssert.Contains(rewritten, """const template = `import("./Template.vue")`;""");
        StringAssert.Contains(rewritten, """import Counter from "./Counter.jazor.js";""");
        StringAssert.Contains(rewritten, """export { helper } from "./helper.vue.js";""");
        StringAssert.Contains(rewritten, """import("./Lazy.vue?raw.js")""");
    }

    [TestMethod]
    public void JavaScriptModuleSpecifierScanner_RewriteDynamicImportExpressions_RewritesOnlyRealDynamicImports()
    {
        var source = """
            const fakeString = "import('./lazy-a.js')";
            // import("./lazy-b.js")
            const lazy = () => import("./lazy-c.js");
            """;

        var rewritten = JavaScriptModuleSpecifierScanner.RewriteDynamicImportExpressions(
            source,
            specifier => string.Equals(specifier.Value, "./lazy-c.js", StringComparison.Ordinal)
                ? "__loadCss().then(() => " + source.Substring(specifier.ExpressionStart, specifier.ExpressionLength) + ")"
                : null);

        StringAssert.Contains(rewritten, """const fakeString = "import('./lazy-a.js')";""");
        StringAssert.Contains(rewritten, """// import("./lazy-b.js")""");
        StringAssert.Contains(rewritten, """const lazy = () => __loadCss().then(() => import("./lazy-c.js"));""");
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "jazor-build-slice-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed class TestHttpServer : IAsyncDisposable
    {
        private readonly HttpListener _listener;
        private readonly Func<HttpListenerContext, Task> _handler;
        private readonly CancellationTokenSource _shutdown = new();
        private readonly Task _acceptLoop;

        private TestHttpServer(HttpListener listener, Func<HttpListenerContext, Task> handler, string baseAddress)
        {
            _listener = listener;
            _handler = handler;
            BaseAddress = baseAddress;
            _acceptLoop = Task.Run(AcceptLoopAsync);
        }

        public string BaseAddress { get; }

        public static Task<TestHttpServer> StartAsync(Func<HttpListenerContext, Task> handler)
        {
            var (listener, baseAddress) = StartListener();
            return Task.FromResult(new TestHttpServer(listener, handler, baseAddress));
        }

        public async ValueTask DisposeAsync()
        {
            _shutdown.Cancel();
            if (_listener.IsListening)
            {
                _listener.Stop();
            }

            try
            {
                await _acceptLoop;
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _listener.Close();
                _shutdown.Dispose();
            }
        }

        private async Task AcceptLoopAsync()
        {
            while (!_shutdown.IsCancellationRequested)
            {
                HttpListenerContext? context = null;
                try
                {
                    context = await _listener.GetContextAsync();
                    await _handler(context);
                }
                catch (HttpListenerException) when (_shutdown.IsCancellationRequested || !_listener.IsListening)
                {
                    break;
                }
                catch (ObjectDisposedException) when (_shutdown.IsCancellationRequested)
                {
                    break;
                }
                finally
                {
                    context?.Response.OutputStream.Dispose();
                }
            }
        }

        private static (HttpListener Listener, string BaseAddress) StartListener()
        {
            using var portProbe = new TcpListener(IPAddress.Loopback, 0);
            portProbe.Start();
            var port = ((IPEndPoint)portProbe.LocalEndpoint).Port;
            portProbe.Stop();

            var baseAddress = $"http://127.0.0.1:{port}/";
            var listener = new HttpListener();
            listener.Prefixes.Add(baseAddress);
            listener.Start();
            return (listener, baseAddress);
        }
    }
}
