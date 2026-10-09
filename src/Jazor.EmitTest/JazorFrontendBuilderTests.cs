using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Jazor.AspNetCore.Dev;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jazor.EmitTest;

[TestClass]
public sealed class JazorFrontendBuilderTests
{
    [TestMethod]
    [DataRow("")]
    [DataRow("/portal")]
    public async Task Frontend_StartupFilter_ForwardsHttpBodyQueryHeadersAndResponse(string pathBase)
    {
        using var workspace = new Workspace();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var publicPrefix = pathBase + "/frontend";
        await using var upstream = await CreateUpstreamAsync(workspace.RootPath, async context =>
        {
            if (context.Request.Path == publicPrefix + "/entry.js")
            {
                await context.Response.WriteAsync("export {};", timeout.Token);
                return;
            }

            if (context.Request.Path == publicPrefix + "/redirect")
            {
                context.Response.StatusCode = StatusCodes.Status307TemporaryRedirect;
                context.Response.Headers.Location = publicPrefix + "/target";
                return;
            }

            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync(timeout.Token);
            context.Response.StatusCode = StatusCodes.Status201Created;
            context.Response.Headers["X-Upstream"] = "received";
            context.Response.Headers.SetCookie = new[] { "one=1; Path=/", "two=2; Path=/" };
            await context.Response.WriteAsJsonAsync(new
            {
                method = context.Request.Method,
                path = context.Request.Path.Value,
                query = context.Request.QueryString.Value,
                body,
                contentType = context.Request.ContentType,
                customHeader = context.Request.Headers["X-Request"].ToString(),
                host = context.Request.Host.Value
            }, timeout.Token);
        }, timeout.Token);
        using var host = await CreateFrameworkHostAsync(workspace.RootPath, Environments.Development,
            pathBase, new Uri(upstream.Urls.Single()), timeout.Token);
        using var client = CreateClient(host);

        using var request = new HttpRequestMessage(HttpMethod.Post, publicPrefix + "/upload?name=a%2Bb&name=c&flag");
        request.Content = new StringContent("file body\n雪花 ID: 9223372036854775807", Encoding.UTF8, "application/x-probe");
        request.Headers.Add("X-Request", "kept");
        using var response = await client.SendAsync(request, timeout.Token);
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        Assert.AreEqual("received", response.Headers.GetValues("X-Upstream").Single());
        CollectionAssert.AreEqual(new[] { "one=1; Path=/", "two=2; Path=/" }, response.Headers.GetValues("Set-Cookie").ToArray());
        Assert.AreEqual("application/json", response.Content.Headers.ContentType?.MediaType);
        AssertSharedHeaders(response);
        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(timeout.Token));
        var echoed = payload.RootElement;
        Assert.AreEqual("POST", echoed.GetProperty("method").GetString());
        Assert.AreEqual(publicPrefix + "/upload", echoed.GetProperty("path").GetString());
        Assert.AreEqual("?name=a%2Bb&name=c&flag", echoed.GetProperty("query").GetString());
        Assert.AreEqual("file body\n雪花 ID: 9223372036854775807", echoed.GetProperty("body").GetString());
        Assert.AreEqual("application/x-probe; charset=utf-8", echoed.GetProperty("contentType").GetString());
        Assert.AreEqual("kept", echoed.GetProperty("customHeader").GetString());
        Assert.AreEqual(new Uri(upstream.Urls.Single()).Authority, echoed.GetProperty("host").GetString());

        using var redirect = await client.GetAsync(publicPrefix + "/redirect", timeout.Token);
        Assert.AreEqual(HttpStatusCode.TemporaryRedirect, redirect.StatusCode);
        Assert.AreEqual(publicPrefix + "/target", redirect.Headers.Location?.OriginalString);
        using var business = await client.GetAsync(pathBase + "/api/missing", timeout.Token);
        Assert.AreEqual(HttpStatusCode.NotFound, business.StatusCode);
        Assert.AreEqual(pathBase + "|/api/missing", await business.Content.ReadAsStringAsync(timeout.Token));
        AssertSharedHeaders(business);
        await host.StopAsync(timeout.Token);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("/portal")]
    public async Task Frontend_StartupFilter_RelaysWebSocketBothDirectionsAndClose(string pathBase)
    {
        using var workspace = new Workspace();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var publicPrefix = pathBase + "/frontend";
        var handshake = new TaskCompletionSource<(string Path, string Query, string Header)>(TaskCreationOptions.RunContinuationsAsynchronously);
        var close = new TaskCompletionSource<(WebSocketCloseStatus? Status, string? Reason)>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var upstream = await CreateUpstreamAsync(workspace.RootPath, async context =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await context.Response.WriteAsync("export {};", timeout.Token);
                return;
            }

            handshake.SetResult((context.Request.Path.Value!, context.Request.QueryString.Value!, context.Request.Headers["X-Request"].ToString()));
            using var socket = await context.WebSockets.AcceptWebSocketAsync("vite-hmr");
            // Unsolicited upstream traffic proves the reverse relay is active before the browser sends.
            await socket.SendAsync(Encoding.UTF8.GetBytes("connected"), WebSocketMessageType.Text, true, timeout.Token);
            for (var index = 0; index < 2; index++)
            {
                var message = await ReceiveMessageAsync(socket, timeout.Token);
                await socket.SendAsync(message.Bytes, message.Type, true, timeout.Token);
            }

            var result = await socket.ReceiveAsync(new byte[1], timeout.Token);
            close.SetResult((result.CloseStatus, result.CloseStatusDescription));
            await socket.CloseOutputAsync(result.CloseStatus!.Value, result.CloseStatusDescription, timeout.Token);
        }, timeout.Token);
        using var host = await CreateFrameworkHostAsync(workspace.RootPath, Environments.Development,
            pathBase, new Uri(upstream.Urls.Single()), timeout.Token);
        using var socket = new ClientWebSocket();
        socket.Options.AddSubProtocol("vite-hmr");
        socket.Options.SetRequestHeader("X-Request", "socket-header");
        var socketUri = new UriBuilder(GetHostAddress(host))
        {
            Scheme = "ws",
            Path = publicPrefix + "/hmr",
            Query = "token=a%2Bb"
        }.Uri;
        await socket.ConnectAsync(socketUri, timeout.Token);
        Assert.AreEqual("vite-hmr", socket.SubProtocol);
        var captured = await handshake.Task.WaitAsync(timeout.Token);
        Assert.AreEqual(publicPrefix + "/hmr", captured.Path);
        Assert.AreEqual("?token=a%2Bb", captured.Query);
        Assert.AreEqual("socket-header", captured.Header);
        var greeting = await ReceiveMessageAsync(socket, timeout.Token);
        Assert.AreEqual(WebSocketMessageType.Text, greeting.Type);
        Assert.AreEqual("connected", Encoding.UTF8.GetString(greeting.Bytes));

        await socket.SendAsync(Encoding.UTF8.GetBytes("hello "), WebSocketMessageType.Text, false, timeout.Token);
        await socket.SendAsync(Encoding.UTF8.GetBytes("上游"), WebSocketMessageType.Text, true, timeout.Token);
        var textEcho = await ReceiveMessageAsync(socket, timeout.Token);
        Assert.AreEqual(WebSocketMessageType.Text, textEcho.Type);
        Assert.AreEqual("hello 上游", Encoding.UTF8.GetString(textEcho.Bytes));
        var binary = Enumerable.Range(0, 150_000).Select(index => (byte)index).ToArray();
        await socket.SendAsync(binary, WebSocketMessageType.Binary, true, timeout.Token);
        var binaryEcho = await ReceiveMessageAsync(socket, timeout.Token);
        Assert.AreEqual(WebSocketMessageType.Binary, binaryEcho.Type);
        CollectionAssert.AreEqual(binary, binaryEcho.Bytes);
        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "browser done", timeout.Token);
        var capturedClose = await close.Task.WaitAsync(timeout.Token);
        Assert.AreEqual(WebSocketCloseStatus.NormalClosure, capturedClose.Status);
        Assert.AreEqual("browser done", capturedClose.Reason);
        Assert.AreEqual(WebSocketState.Closed, socket.State);
        await host.StopAsync(timeout.Token);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("/portal")]
    public async Task Frontend_StartupFilter_ReleaseServesArtifactsWithoutStartingVite(string pathBase)
    {
        using var workspace = new Workspace();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var dist = Path.Combine(workspace.RootPath, "generated", "dist");
        Directory.CreateDirectory(Path.Combine(dist, "chunks"));
        await File.WriteAllTextAsync(Path.Combine(dist, "bundle.js"), "import './chunks/feature.js';", timeout.Token);
        await File.WriteAllTextAsync(Path.Combine(dist, "chunks", "feature.js"), "export const ready = true;", timeout.Token);
        Directory.CreateDirectory(Path.Combine(dist, "assets"));
        await File.WriteAllTextAsync(Path.Combine(dist, "assets", "shared.css"), ".shared { color: blue; }", timeout.Token);
        await File.WriteAllTextAsync(Path.Combine(dist, "manifest.json"), """
            {
              "entry.js": { "file": "bundle.js", "imports": ["_shared.js", "_other.js"], "css": ["assets/site.css"], "dynamicImports": ["lazy.js"] },
              "_shared.js": { "file": "assets/shared.js", "css": ["assets/shared.css"] },
              "_other.js": { "file": "assets/other.js", "imports": ["_shared.js"] },
              "lazy.js": { "file": "chunks/feature.js", "css": ["assets/lazy.css"] }
            }
            """, timeout.Token);
        // An unreachable origin with LaunchServer=false makes any accidental development probe fail startup.
        using var host = await CreateFrameworkHostAsync(workspace.RootPath, Environments.Production,
            pathBase, new Uri("http://127.0.0.1:1"), timeout.Token);
        using var client = CreateClient(host);
        using var bundle = await client.GetAsync(pathBase + "/frontend/dist/bundle.js", timeout.Token);
        Assert.AreEqual(HttpStatusCode.OK, bundle.StatusCode);
        Assert.AreEqual("import './chunks/feature.js';", await bundle.Content.ReadAsStringAsync(timeout.Token));
        AssertSharedHeaders(bundle);
        Assert.IsTrue(bundle.Headers.CacheControl?.NoCache);
        Assert.IsTrue(bundle.Headers.CacheControl?.MustRevalidate);
        using var chunk = await client.GetAsync(pathBase + "/frontend/dist/chunks/feature.js", timeout.Token);
        Assert.AreEqual(HttpStatusCode.OK, chunk.StatusCode);
        Assert.AreEqual("export const ready = true;", await chunk.Content.ReadAsStringAsync(timeout.Token));
        using var urls = await client.GetAsync(pathBase + "/asset-urls", timeout.Token);
        using var payload = JsonDocument.Parse(await urls.Content.ReadAsStringAsync(timeout.Token));
        Assert.AreEqual(pathBase + "/frontend/dist/bundle.js", payload.RootElement.GetProperty("entry").GetString());
        CollectionAssert.AreEqual(new[] { pathBase + "/frontend/dist/assets/shared.css", pathBase + "/frontend/dist/assets/site.css" },
            payload.RootElement.GetProperty("styles").EnumerateArray().Select(value => value.GetString()).ToArray());
        foreach (var path in new[] { "/dist/bundle.js", "/dist/chunks/feature.js", "/dist/assets/shared.css" })
        {
            using var head = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, pathBase + "/frontend" + path), timeout.Token);
            Assert.AreEqual(HttpStatusCode.OK, head.StatusCode);
            Assert.AreEqual(0, (await head.Content.ReadAsByteArrayAsync(timeout.Token)).Length);
            AssertSharedHeaders(head);
        }
        using var missing = await client.GetAsync(pathBase + "/frontend/missing.js", timeout.Token);
        Assert.AreEqual(HttpStatusCode.NotFound, missing.StatusCode);
        using var business = await client.GetAsync(pathBase + "/api/missing", timeout.Token);
        Assert.AreEqual(HttpStatusCode.NotFound, business.StatusCode);
        Assert.AreEqual(pathBase + "|/api/missing", await business.Content.ReadAsStringAsync(timeout.Token));
        AssertSharedHeaders(business);
        await host.StopAsync(timeout.Token);
    }

    private static async Task<WebApplication> CreateUpstreamAsync(string root, RequestDelegate handler, CancellationToken cancellationToken)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { ContentRootPath = root });
        builder.Logging.ClearProviders();
        builder.WebHost.UseKestrel(options => options.Listen(IPAddress.Loopback, 0));
        var app = builder.Build();
        app.UseWebSockets();
        app.Run(handler);
        await app.StartAsync(cancellationToken);
        return app;
    }

    private static async Task<IHost> CreateFrameworkHostAsync(string root, string environment,
        string pathBase, Uri origin, CancellationToken cancellationToken)
    {
        var host = Host.CreateDefaultBuilder([])
            .UseEnvironment(environment)
            .UseContentRoot(root)
            .ConfigureLogging(logging => logging.ClearProviders())
            .ConfigureWebHost(web => web
                .UseKestrel(options => options.Listen(IPAddress.Loopback, 0))
                .ConfigureServices(services =>
                {
                    services.AddJazorFrontend(options =>
                    {
                        options.PathBase = pathBase;
                        options.RequestPath = "/frontend";
                        options.ProjectRootPath = "generated";
                        options.Vite.ServerOrigin = origin;
                        options.Vite.LaunchServer = false;
                    });
                    services.AddSingleton<IStartupFilter>(new FrontendStartupFilter());
                })
                .Configure(app => app.Run(async context =>
                {
                    if (context.Request.Path == "/asset-urls")
                    {
                        await context.Response.WriteAsJsonAsync(new
                        {
                            entry = JazorFrontendUrls.GetBrowserEntry(context),
                            styles = JazorFrontendUrls.GetStylesheets(context)
                        }, cancellationToken);
                        return;
                    }
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync(context.Request.PathBase + "|" + context.Request.Path, cancellationToken);
                })))
            .Build();
        await host.StartAsync(cancellationToken);
        return host;
    }

    private static Uri GetHostAddress(IHost host)
        => new(host.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single());

    private static HttpClient CreateClient(IHost host)
        => new(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = GetHostAddress(host) };

    private static void AssertSharedHeaders(HttpResponseMessage response)
    {
        Assert.AreEqual("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.AreEqual("strict-origin-when-cross-origin", response.Headers.GetValues("Referrer-Policy").Single());
        Assert.AreEqual("registered", response.Headers.GetValues("X-Framework-Frontend").Single());
    }

    private static async Task<(WebSocketMessageType Type, byte[] Bytes)> ReceiveMessageAsync(WebSocket socket, CancellationToken cancellationToken)
    {
        using var output = new MemoryStream();
        var buffer = new byte[4096];
        WebSocketReceiveResult result;
        do
        {
            result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            Assert.AreNotEqual(WebSocketMessageType.Close, result.MessageType);
            output.Write(buffer, 0, result.Count);
        } while (!result.EndOfMessage);
        return (result.MessageType, output.ToArray());
    }

    private sealed class FrontendStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            Assert.IsFalse(app is WebApplication, "This regression must exercise the framework's actual inner builder.");
            Assert.AreSame(app, app.UseJazorPathBase());
            Assert.AreSame(app, app.UseJazorPathBase());
            Assert.AreSame(app, app.UseJazorFrontend(options => options.SecurityHeaders.AdditionalHeaders["X-Framework-Frontend"] = "registered"));
            app.UseJazorFrontend(_ => Assert.Fail("Repeated registration must use the existing middleware."));
            next(app);
        };
    }

    private sealed class Workspace : IDisposable
    {
        public string RootPath { get; } = Path.Combine(RepositoryTemp.Root, "Jazor.FrontendBuilderTests", Guid.NewGuid().ToString("N"));

        public Workspace() => Directory.CreateDirectory(Path.Combine(RootPath, "wwwroot"));

        public void Dispose() => Directory.Delete(RootPath, recursive: true);
    }
}
