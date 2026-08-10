using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Jazor.AspNetCore.Dev;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jazor.EmitTest;

[TestClass]
public sealed class JazorAspNetCoreDevelopmentReloadTests
{
    private static readonly TimeSpan NoDuplicateMessageTimeout = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan TestDebounceInterval = TimeSpan.FromMilliseconds(25);
    private static readonly TimeSpan TestPollingInterval = TimeSpan.FromMilliseconds(100);

    [TestMethod]
    public async Task UseJazorDevelopmentReload_InjectsClientScriptIntoHtmlAndServesClientScriptEndpoint()
    {
        using var workspace = new AspNetCoreHostTestWorkspace();
        var webRoot = Path.Combine(workspace.RootPath, "wwwroot");
        Directory.CreateDirectory(webRoot);
        await File.WriteAllTextAsync(
            Path.Combine(webRoot, "index.html"),
            "<!doctype html><html><head><title>dev</title></head><body><h1>ready</h1></body></html>");

        await using var host = await CreateHostAsync(
            workspace.RootPath,
            Environments.Development,
            services => services.AddJazorDevelopmentReload(),
            app =>
            {
                app.UseJazorDevelopmentReload();
                app.UseStaticFiles(new StaticFileOptions
                {
                    OnPrepareResponse = static context =>
                    {
                        context.Context.Response.Headers["Cache-Control"] = "no-cache";
                    }
                });
            });

        var client = host.GetTestClient();

        var htmlResponse = await client.GetAsync("/index.html");
        var html = await htmlResponse.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, htmlResponse.StatusCode);
        StringAssert.Contains(html, "<script type=\"module\" src=\"/@jazor/client\"></script>");
        Assert.IsTrue(
            html.IndexOf("/@jazor/client", StringComparison.Ordinal) < html.IndexOf("</head>", StringComparison.OrdinalIgnoreCase));

        var clientScriptResponse = await client.GetAsync("/@jazor/client");
        var clientScript = await clientScriptResponse.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, clientScriptResponse.StatusCode);
        Assert.AreEqual("no-store", clientScriptResponse.Headers.CacheControl?.ToString());
        StringAssert.Contains(clientScript, "/@jazor/reload");
        StringAssert.Contains(clientScript, "location.reload()");
        StringAssert.Contains(clientScript, "new WebSocket");
        StringAssert.Contains(clientScript, "const moduleUpdateCapability = \"module-update\";");
        StringAssert.Contains(clientScript, "new CustomEvent(\"jazor:module-update\", {");
        StringAssert.Contains(clientScript, "cancelable: true");
        StringAssert.Contains(clientScript, "Object.defineProperty(window, \"JazorHmr\"");
        StringAssert.Contains(clientScript, "ready: transportReady");
        StringAssert.Contains(clientScript, "const vueComponents = new Map();");
        StringAssert.Contains(clientScript, "registerVueComponent(moduleId, component)");
        StringAssert.Contains(clientScript, "runtime.createRecord(moduleId, component);");
        StringAssert.Contains(clientScript, "runtime.reload(moduleId, component);");
        StringAssert.Contains(clientScript, "moduleUrl.searchParams.set(\"__jazor_hmr\"");
        StringAssert.Contains(clientScript, "void acceptModuleUpdate(payload).then(accepted => {");
        StringAssert.Contains(clientScript, "sendMessage({ type: \"ready\", capabilities: [moduleUpdateCapability] });");
    }

    [TestMethod]
    public async Task UseJazorDevelopmentReload_WhenApplicationUsesPathBase_InjectsPathBaseAwareClientScriptAndServesPathBaseEndpoints()
    {
        using var workspace = new AspNetCoreHostTestWorkspace();
        var webRoot = Path.Combine(workspace.RootPath, "wwwroot");
        Directory.CreateDirectory(webRoot);
        await File.WriteAllTextAsync(
            Path.Combine(webRoot, "index.html"),
            "<!doctype html><html><head><title>dev</title></head><body><h1>ready</h1></body></html>");

        await using var host = await CreateHostAsync(
            workspace.RootPath,
            Environments.Development,
            services => services.AddJazorDevelopmentReload(),
            app =>
            {
                app.UsePathBase("/docs");
                app.UseJazorDevelopmentReload();
                app.UseStaticFiles();
            });

        var client = host.GetTestClient();

        var htmlResponse = await client.GetAsync("/docs/index.html");
        var html = await htmlResponse.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, htmlResponse.StatusCode);
        StringAssert.Contains(html, "data-jazor-path-base=\"/docs\"");
        StringAssert.Contains(html, "<script type=\"module\" src=\"/docs/@jazor/client\"></script>");

        var clientScriptResponse = await client.GetAsync("/docs/@jazor/client");
        var clientScript = await clientScriptResponse.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, clientScriptResponse.StatusCode);
        StringAssert.Contains(clientScript, "const pathBaseExpression = \"data-jazor-path-base\";");
        StringAssert.Contains(clientScript, "const socketPath = \"/@jazor/reload\";");

        var socketClient = host.GetTestServer().CreateWebSocketClient();
        using var socket = await socketClient.ConnectAsync(new Uri("ws://localhost/docs/@jazor/reload"), CancellationToken.None);
        var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
        Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());
    }

    [TestMethod]
    public async Task UseJazorDevelopmentReload_WhenWatchedModuleChanges_BroadcastsSingleFullReloadToClientWithoutModuleUpdateCapability()
    {
        using var workspace = new AspNetCoreHostTestWorkspace();
        var watchRoot = Path.Combine(workspace.RootPath, "jazor");
        Directory.CreateDirectory(watchRoot);
        var watchedFilePath = Path.Combine(watchRoot, "main.mjs");
        var manifestPath = Path.Combine(watchRoot, "jazor-manifest.json");
        await File.WriteAllTextAsync(watchedFilePath, "export const version = 1;\n");
        await WriteHmrManifestAsync(manifestPath, "main.mjs", "hash-v1", "template-v1", "logic-v1", "descriptor-v1");

        await using var host = await CreateHostAsync(
            workspace.RootPath,
            Environments.Development,
            services => services.AddJazorDevelopmentReload(options =>
            {
                options.WatchRootPaths.Clear();
                options.WatchRootPaths.Add("jazor");
                options.HmrModuleMappings.Clear();
                options.HmrModuleMappings.Add(new JazorDevelopmentHmrModuleMapping
                {
                    ArtifactRootPath = "jazor",
                    RequestPath = new PathString("/jazor")
                });
                options.FileChangeDebounceInterval = TestDebounceInterval;
                options.FileChangePollingInterval = TestPollingInterval;
            }),
            app =>
            {
                app.UseJazorDevelopmentReload();
                app.MapGet("/", static () => Results.Content("<html><head></head><body>ready</body></html>", "text/html"));
            });

        var socketClient = host.GetTestServer().CreateWebSocketClient();
        using var socket = await socketClient.ConnectAsync(new Uri("ws://localhost/@jazor/reload"), CancellationToken.None);

        var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
        Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

        await SendWebSocketJsonAsync(socket, new { type = "ready", capabilities = Array.Empty<string>() });
        await Task.Delay(TestPollingInterval);

        await File.WriteAllTextAsync(watchedFilePath, "export const version = 2;\n");
        await WriteHmrManifestAsync(manifestPath, "main.mjs", "hash-v2", "template-v2", "logic-v1", "descriptor-v1");

        var reloadMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
        Assert.AreEqual("full-reload", reloadMessage.GetProperty("type").GetString());

        await AssertNoWebSocketJsonAsync(socket, NoDuplicateMessageTimeout);
    }

    [TestMethod]
    public async Task UseJazorDevelopmentReload_WhenMappedModuleChanges_OffersCancellableModuleUpdateToCapabilityAwareClient()
    {
        using var workspace = new AspNetCoreHostTestWorkspace();
        var watchRoot = Path.Combine(workspace.RootPath, "jazor");
        Directory.CreateDirectory(watchRoot);
        var watchedFilePath = Path.Combine(watchRoot, "main.mjs");
        var manifestPath = Path.Combine(watchRoot, "jazor-manifest.json");
        await File.WriteAllTextAsync(watchedFilePath, "export const version = 1;\n");
        await WriteHmrManifestAsync(manifestPath, "main.mjs", "hash-v1", "template-v1", "logic-v1", "descriptor-v1");

        await using var host = await CreateHostAsync(
            workspace.RootPath,
            Environments.Development,
            services => services.AddJazorDevelopmentReload(options =>
            {
                options.WatchRootPaths.Clear();
                options.HmrModuleMappings.Clear();
                options.HmrModuleMappings.Add(new JazorDevelopmentHmrModuleMapping
                {
                    ArtifactRootPath = "jazor",
                    RequestPath = new PathString("/jazor")
                });
                options.FileChangeDebounceInterval = TestDebounceInterval;
                options.FileChangePollingInterval = TestPollingInterval;
            }),
            app =>
            {
                app.UseJazorDevelopmentReload();
                app.MapGet("/", static () => Results.Content("<html><head></head><body>ready</body></html>", "text/html"));
            });

        var socketClient = host.GetTestServer().CreateWebSocketClient();
        using var socket = await socketClient.ConnectAsync(new Uri("ws://localhost/@jazor/reload"), CancellationToken.None);

        var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
        Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

        await SendWebSocketJsonAsync(socket, new { type = "ready", capabilities = new[] { "module-update" } });
        await Task.Delay(TestPollingInterval);

        await File.WriteAllTextAsync(watchedFilePath, "export const version = 2;\n");
        await WriteHmrManifestAsync(manifestPath, "main.mjs", "hash-v2", "template-v2", "logic-v1", "descriptor-v1");

        var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
        Assert.AreEqual("module-update", updateMessage.GetProperty("type").GetString());
        Assert.AreEqual("hmr-template-only", updateMessage.GetProperty("reason").GetString());
        CollectionAssert.AreEqual(
            new[] { "main.mjs" },
            updateMessage.GetProperty("changedPaths").EnumerateArray().Select(static path => path.GetString()).ToArray());
        var update = updateMessage.GetProperty("moduleUpdates").EnumerateArray().Single();
        Assert.AreEqual("main.mjs", update.GetProperty("path").GetString());
        Assert.AreEqual("/jazor/main.mjs", update.GetProperty("url").GetString());
        Assert.AreEqual("Demo.Pages.Counter", update.GetProperty("componentId").GetString());
        Assert.AreEqual("Demo.Pages.Counter:main.mjs", update.GetProperty("moduleId").GetString());
        Assert.AreEqual("descriptor-v1", update.GetProperty("descriptorHash").GetString());
        Assert.AreEqual("template-v2", update.GetProperty("templateHash").GetString());
        Assert.AreEqual("logic-v1", update.GetProperty("logicHash").GetString());
        Assert.AreEqual("template-only", update.GetProperty("boundaryKind").GetString());

        await AssertNoWebSocketJsonAsync(socket, NoDuplicateMessageTimeout);
    }

    [TestMethod]
    public async Task UseJazorDevelopmentReload_WhenHmrDescriptorChanges_FallsBackToFullReload()
    {
        using var workspace = new AspNetCoreHostTestWorkspace();
        var watchRoot = Path.Combine(workspace.RootPath, "jazor");
        Directory.CreateDirectory(watchRoot);
        var watchedFilePath = Path.Combine(watchRoot, "main.mjs");
        var manifestPath = Path.Combine(watchRoot, "jazor-manifest.json");
        await File.WriteAllTextAsync(watchedFilePath, "export const version = 1;\n");
        await WriteHmrManifestAsync(manifestPath, "main.mjs", "hash-v1", "template-v1", "logic-v1", "descriptor-v1");

        await using var host = await CreateHostAsync(
            workspace.RootPath,
            Environments.Development,
            services => services.AddJazorDevelopmentReload(options =>
            {
                options.WatchRootPaths.Clear();
                options.WatchRootPaths.Add("jazor");
                options.HmrModuleMappings.Clear();
                options.HmrModuleMappings.Add(new JazorDevelopmentHmrModuleMapping
                {
                    ArtifactRootPath = "jazor",
                    RequestPath = new PathString("/jazor")
                });
                options.FileChangeDebounceInterval = TestDebounceInterval;
                options.FileChangePollingInterval = TestPollingInterval;
            }),
            app =>
            {
                app.UseJazorDevelopmentReload();
                app.MapGet("/", static () => Results.Content("<html><head></head><body>ready</body></html>", "text/html"));
            });

        var socketClient = host.GetTestServer().CreateWebSocketClient();
        using var socket = await socketClient.ConnectAsync(new Uri("ws://localhost/@jazor/reload"), CancellationToken.None);

        _ = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
        await SendWebSocketJsonAsync(socket, new { type = "ready", capabilities = new[] { "module-update" } });
        await Task.Delay(TestPollingInterval);

        await File.WriteAllTextAsync(watchedFilePath, "export const version = 2;\n");
        await WriteHmrManifestAsync(manifestPath, "main.mjs", "hash-v2", "template-v2", "logic-v1", "descriptor-v2");

        var reloadMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
        Assert.AreEqual("full-reload", reloadMessage.GetProperty("type").GetString());
        Assert.AreEqual("hmr-descriptor-changed", reloadMessage.GetProperty("reason").GetString());

        await AssertNoWebSocketJsonAsync(socket, NoDuplicateMessageTimeout);
    }

    [TestMethod]
    public async Task UseJazorDevelopmentReload_WhenResponseHasContentSecurityPolicy_InjectsNonceAndAllowsReloadSocket()
    {
        using var workspace = new AspNetCoreHostTestWorkspace();

        await using var host = await CreateHostAsync(
            workspace.RootPath,
            Environments.Development,
            services => services.AddJazorDevelopmentReload(),
            app =>
            {
                app.UseJazorDevelopmentReload();
                app.MapGet(
                    "/",
                    static (HttpContext context) =>
                    {
                        context.Response.Headers["Content-Security-Policy"] =
                            "default-src 'none'; script-src 'nonce-devnonce' 'strict-dynamic'; connect-src 'self';";
                        return Results.Content("<html><head></head><body>secure</body></html>", "text/html");
                    });
            });

        var client = host.GetTestClient();
        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();
        var csp = response.Headers.GetValues("Content-Security-Policy").Single();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        StringAssert.Contains(html, "<script type=\"module\" src=\"/@jazor/client\" nonce=\"devnonce\"></script>");
        StringAssert.Contains(csp, "connect-src 'self' ws: wss:");
        StringAssert.Contains(csp, "script-src 'nonce-devnonce' 'strict-dynamic'");
    }

    [TestMethod]
    public async Task UseJazorDevelopmentReload_BypassesJsonEndpointsAndStaticAssetsThatCannotBeHtmlDocuments()
    {
        using var workspace = new AspNetCoreHostTestWorkspace();
        var webRoot = Path.Combine(workspace.RootPath, "wwwroot");
        Directory.CreateDirectory(webRoot);
        await File.WriteAllTextAsync(
            Path.Combine(webRoot, "site.css"),
            "body{color:black;}");

        await using var host = await CreateHostAsync(
            workspace.RootPath,
            Environments.Development,
            services => services.AddJazorDevelopmentReload(),
            app =>
            {
                app.UseJazorDevelopmentReload();
                app.MapGet(
                    "/api/status",
                    static (HttpContext context) =>
                    {
                        context.Response.Headers["X-Observed-Body-Type"] = context.Response.Body.GetType().FullName;
                        return Results.Json(new { ok = true });
                    });
                app.UseStaticFiles(new StaticFileOptions
                {
                    OnPrepareResponse = static context =>
                    {
                        context.Context.Response.Headers["X-Observed-Body-Type"] =
                            context.Context.Response.Body.GetType().FullName;
                    }
                });
            });

        var client = host.GetTestClient();

        using var jsonRequest = new HttpRequestMessage(HttpMethod.Get, "/api/status");
        jsonRequest.Headers.Accept.ParseAdd("application/json");
        var jsonResponse = await client.SendAsync(jsonRequest);
        var jsonPayload = await jsonResponse.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, jsonResponse.StatusCode);
        Assert.AreEqual("{\"ok\":true}", jsonPayload);
        Assert.IsTrue(jsonResponse.Headers.TryGetValues("X-Observed-Body-Type", out var jsonBodyTypes));
        Assert.IsFalse(jsonBodyTypes.Single().Contains("MemoryStream", StringComparison.Ordinal));

        var staticResponse = await client.GetAsync("/site.css");
        var css = await staticResponse.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, staticResponse.StatusCode);
        Assert.AreEqual("body{color:black;}", css);
        Assert.IsTrue(staticResponse.Headers.TryGetValues("X-Observed-Body-Type", out var staticBodyTypes));
        Assert.IsFalse(staticBodyTypes.Single().Contains("MemoryStream", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task UseJazorDevelopmentReload_WhenExternalBrowserRefreshIsActive_SuppressesReconnectReloadBehaviorInClientScript()
    {
        using var workspace = new AspNetCoreHostTestWorkspace();

        await using var host = await CreateHostAsync(
            workspace.RootPath,
            Environments.Development,
            services =>
            {
                services.AddJazorDevelopmentReload();
                services.AddSingleton<IJazorDevelopmentRuntimeSignals>(new TestRuntimeSignals(isExternalBrowserRefreshActive: true));
            },
            app =>
            {
                app.UseJazorDevelopmentReload();
                app.MapGet("/", static () => Results.Content("<html><head></head><body>ready</body></html>", "text/html"));
            });

        var client = host.GetTestClient();
        var clientScriptResponse = await client.GetAsync("/@jazor/client");
        var clientScript = await clientScriptResponse.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, clientScriptResponse.StatusCode);
        StringAssert.Contains(clientScript, "const reloadOnReconnect = false;");
    }

    [TestMethod]
    public async Task UseJazorDevelopmentReload_WhenExternalBrowserRefreshSuppressionIsDisabled_KeepsReconnectReloadBehavior()
    {
        using var workspace = new AspNetCoreHostTestWorkspace();

        await using var host = await CreateHostAsync(
            workspace.RootPath,
            Environments.Development,
            services =>
            {
                services.AddJazorDevelopmentReload(options =>
                {
                    options.SuppressReloadOnReconnectWhenExternalBrowserRefreshIsActive = false;
                });
                services.AddSingleton<IJazorDevelopmentRuntimeSignals>(new TestRuntimeSignals(isExternalBrowserRefreshActive: true));
            },
            app =>
            {
                app.UseJazorDevelopmentReload();
                app.MapGet("/", static () => Results.Content("<html><head></head><body>ready</body></html>", "text/html"));
            });

        var client = host.GetTestClient();
        var clientScriptResponse = await client.GetAsync("/@jazor/client");
        var clientScript = await clientScriptResponse.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, clientScriptResponse.StatusCode);
        StringAssert.Contains(clientScript, "const reloadOnReconnect = true;");
    }

    [TestMethod]
    public async Task UseJazorDevelopmentReload_WhenExternalBrowserRefreshIsActive_IgnoresWebRootChangesButStillReloadsForJazorArtifacts()
    {
        using var workspace = new AspNetCoreHostTestWorkspace();
        var webRoot = Path.Combine(workspace.RootPath, "wwwroot");
        var jazorRoot = Path.Combine(workspace.RootPath, "jazor");
        Directory.CreateDirectory(webRoot);
        Directory.CreateDirectory(jazorRoot);

        var staticFilePath = Path.Combine(webRoot, "site.css");
        var jazorFilePath = Path.Combine(jazorRoot, "main.mjs");
        await File.WriteAllTextAsync(staticFilePath, "body{color:black;}");
        await File.WriteAllTextAsync(jazorFilePath, "export const version = 1;\n");

        await using var host = await CreateHostAsync(
            workspace.RootPath,
            Environments.Development,
            services =>
            {
                services.AddJazorDevelopmentReload(options =>
                {
                    options.WatchRootPaths.Clear();
                    options.WatchRootPaths.Add("wwwroot");
                    options.WatchRootPaths.Add("jazor");
                    options.FileChangeDebounceInterval = TestDebounceInterval;
                    options.FileChangePollingInterval = TestPollingInterval;
                });
                services.AddSingleton<IJazorDevelopmentRuntimeSignals>(new TestRuntimeSignals(isExternalBrowserRefreshActive: true));
            },
            app =>
            {
                app.UseJazorDevelopmentReload();
                app.UseStaticFiles();
                app.MapGet("/", static () => Results.Content("<html><head></head><body>ready</body></html>", "text/html"));
            });

        var socketClient = host.GetTestServer().CreateWebSocketClient();
        using var socket = await socketClient.ConnectAsync(new Uri("ws://localhost/@jazor/reload"), CancellationToken.None);

        var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
        Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

        await File.WriteAllTextAsync(staticFilePath, "body{color:white;}");
        await AssertNoWebSocketJsonAsync(socket, NoDuplicateMessageTimeout);

        await File.WriteAllTextAsync(jazorFilePath, "export const version = 2;\n");
        var reloadMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));

        Assert.AreEqual("full-reload", reloadMessage.GetProperty("type").GetString());
        Assert.AreEqual("file-change:main.mjs", reloadMessage.GetProperty("reason").GetString());
    }

    [TestMethod]
    public void JazorDevelopmentExternalBrowserRefreshDetector_RequiresBrowserToolsAndAutoReloadSignal()
    {
        static string? GetEnvironmentVariable(IReadOnlyDictionary<string, string?> variables, string name)
            => variables.TryGetValue(name, out var value) ? value : null;

        Assert.IsFalse(
            JazorDevelopmentExternalBrowserRefreshDetector.IsActive(
                name => GetEnvironmentVariable(
                    new Dictionary<string, string?>(StringComparer.Ordinal)
                    {
                        ["DOTNET_WATCH"] = "1"
                    },
                    name)));

        Assert.IsFalse(
            JazorDevelopmentExternalBrowserRefreshDetector.IsActive(
                name => GetEnvironmentVariable(
                    new Dictionary<string, string?>(StringComparer.Ordinal)
                    {
                        ["__ASPNETCORE_BROWSER_TOOLS"] = "true"
                    },
                    name)));

        Assert.IsTrue(
            JazorDevelopmentExternalBrowserRefreshDetector.IsActive(
                name => GetEnvironmentVariable(
                    new Dictionary<string, string?>(StringComparer.Ordinal)
                    {
                        ["__ASPNETCORE_BROWSER_TOOLS"] = "true",
                        ["ASPNETCORE_AUTO_RELOAD_WS_ENDPOINT"] = "ws://127.0.0.1:1234/"
                    },
                    name)));
    }

    [TestMethod]
    public async Task UseJazorDevelopmentReload_DoesNothingOutsideDevelopmentEnvironment()
    {
        using var workspace = new AspNetCoreHostTestWorkspace();
        var webRoot = Path.Combine(workspace.RootPath, "wwwroot");
        Directory.CreateDirectory(webRoot);
        await File.WriteAllTextAsync(
            Path.Combine(webRoot, "index.html"),
            "<!doctype html><html><head><title>prod</title></head><body><h1>ready</h1></body></html>");

        await using var host = await CreateHostAsync(
            workspace.RootPath,
            Environments.Production,
            services => services.AddJazorDevelopmentReload(),
            app =>
            {
                app.UseJazorDevelopmentReload();
                app.UseStaticFiles();
            });

        var client = host.GetTestClient();

        var htmlResponse = await client.GetAsync("/index.html");
        var html = await htmlResponse.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, htmlResponse.StatusCode);
        Assert.IsFalse(html.Contains("/@jazor/client", StringComparison.Ordinal));

        var clientScriptResponse = await client.GetAsync("/@jazor/client");
        Assert.AreEqual(HttpStatusCode.NotFound, clientScriptResponse.StatusCode);
    }

    private static async Task<WebApplication> CreateHostAsync(
        string contentRootPath,
        string environmentName,
        Action<IServiceCollection>? configureServices,
        Action<WebApplication> configure)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = contentRootPath,
            EnvironmentName = environmentName
        });
        builder.WebHost.UseTestServer();
        configureServices?.Invoke(builder.Services);

        var app = builder.Build();
        configure(app);
        await app.StartAsync();
        return app;
    }

    private static Task WriteHmrManifestAsync(
        string manifestPath,
        string relativePath,
        string contentHash,
        string templateHash,
        string logicHash,
        string descriptorHash)
    {
        var directory = Path.GetDirectoryName(manifestPath)
            ?? throw new InvalidOperationException("HMR manifest path must have a parent directory.");
        Directory.CreateDirectory(directory);
        var manifest = new
        {
            schemaVersion = 1,
            runtimeProtocolVersion = 1,
            rootAssemblyName = "Demo.Host",
            entries = new[] { relativePath },
            modules = new[]
            {
                new
                {
                    assemblyName = "Demo.Host",
                    typeName = "Demo.Pages.Counter",
                    id = "Demo.Pages.Counter",
                    path = relativePath,
                    contentHash,
                    hmr = new
                    {
                        componentId = "Demo.Pages.Counter",
                        moduleId = "Demo.Pages.Counter:" + relativePath,
                        descriptorHash,
                        templateHash,
                        logicHash,
                        boundaryKind = "template-only"
                    }
                }
            }
        };
        return File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest));
    }

    private static async Task<JsonElement> ReceiveWebSocketJsonAsync(WebSocket socket, TimeSpan timeout)
    {
        using var cancellationSource = new CancellationTokenSource(timeout);
        var buffer = new byte[4096];
        using var stream = new MemoryStream();

        while (true)
        {
            var result = await socket.ReceiveAsync(buffer, cancellationSource.Token);
            if (result.MessageType == WebSocketMessageType.Close)
                throw new InvalidOperationException("WebSocket closed before a message was received.");

            stream.Write(buffer, 0, result.Count);
            if (!result.EndOfMessage)
                continue;

            var payload = Encoding.UTF8.GetString(stream.ToArray());
            using var document = JsonDocument.Parse(payload);
            return document.RootElement.Clone();
        }
    }

    private static async Task SendWebSocketJsonAsync(WebSocket socket, object payload)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
        await socket.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
    }

    private static async Task AssertNoWebSocketJsonAsync(WebSocket socket, TimeSpan timeout)
    {
        try
        {
            var message = await ReceiveWebSocketJsonAsync(socket, timeout);
            Assert.Fail("Expected no additional WebSocket message, but received: " + message.GetRawText());
        }
        catch (OperationCanceledException)
        {
        }
    }

    private sealed class AspNetCoreHostTestWorkspace : IDisposable
    {
        public AspNetCoreHostTestWorkspace()
        {
            RootPath = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(RootPath);
        }

        public string RootPath { get; }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(RootPath))
                    Directory.Delete(RootPath, recursive: true);
            }
            catch
            {
            }
        }
    }

    private sealed class TestRuntimeSignals(bool isExternalBrowserRefreshActive) : IJazorDevelopmentRuntimeSignals
    {
        public bool IsExternalBrowserRefreshActive { get; } = isExternalBrowserRefreshActive;
    }
}
