using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using DenoHost.Core;
using Jazor.AspNetCore.Dev;
using Jazor.Common;
using Jazor.Emit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jazor.EmitTest;

[TestClass]
public sealed class JazorViteProxyTests
{
    [TestMethod]
    public async Task Frontend_FailsBeforeLaunchingWhenDevelopmentEntryIsMissing()
    {
        var root = Path.Combine(RepositoryTemp.Root, "Jazor.MissingViteEntryTest", Guid.NewGuid().ToString("N"));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        Directory.CreateDirectory(root);
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(root, "package.json"),
                "{\"private\":true,\"type\":\"module\",\"scripts\":{\"dev\":\"vite\"}}",
                timeout.Token);
            var vitePort = ReserveLoopbackPort();
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ContentRootPath = root,
                EnvironmentName = Environments.Development
            });
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Logging.ClearProviders();
            builder.AddJazorFrontend(options =>
            {
                options.ProjectRootPath = root;
                options.Vite.ServerOrigin = new Uri($"http://127.0.0.1:{vitePort}");
            });

            await using var host = builder.Build();
            host.UseJazorFrontend();
            Exception? startupError = null;
            try
            {
                await host.StartAsync(timeout.Token);
            }
            catch (Exception exception)
            {
                startupError = exception;
            }

            Assert.IsNotNull(startupError, "Starting without the generated development entry must fail.");
            var errors = startupError is AggregateException aggregate
                ? aggregate.Flatten().InnerExceptions
                : [startupError];
            var entryError = errors.SingleOrDefault(exception =>
                exception is InvalidOperationException &&
                exception.Message.Contains(JazorArtifactDefaults.DevelopmentEntryRelativePath, StringComparison.Ordinal));
            Assert.IsNotNull(entryError, startupError.ToString());
            StringAssert.Contains(entryError.Message, JazorArtifactDefaults.DevelopmentEntryRelativePath);
            StringAssert.Contains(entryError.Message, "Build the Jazor host before starting it.");
            Assert.IsTrue(
                await WaitUntilUnavailableAsync(
                    new Uri($"http://127.0.0.1:{vitePort}{JazorArtifactDefaults.RequestPath}/"),
                    timeout.Token),
                "Missing generated input must fail before DenoHost launches Vite.");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task Frontend_StartsProxiesAndStopsDenoHostOwnedVite()
    {
        var root = Path.Combine(RepositoryTemp.Root, "Jazor.ViteProxyTest", Guid.NewGuid().ToString("N"));
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        Directory.CreateDirectory(root);
        try
        {
            await WriteProjectAsync(root, timeout.Token);
            const string module = "export const value = 1;\nif (import.meta.hot) import.meta.hot.accept();\n";
            var modulePath = Path.Combine(root, "app.js");
            await File.WriteAllTextAsync(modulePath, module, timeout.Token);
            await File.WriteAllTextAsync(Path.Combine(root, "query-probe.txt"), "query preserved", timeout.Token);

            var vitePort = ReserveLoopbackPort();
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ContentRootPath = root,
                EnvironmentName = Environments.Development
            });
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.AddJazorFrontend(options =>
            {
                options.PathBase = "/docs";
                options.ProjectRootPath = root;
                options.Vite.ServerOrigin = new Uri($"http://127.0.0.1:{vitePort}");
            });

            await using var host = builder.Build();
            host.UseJazorPathBase();
            host.UseJazorFrontend();
            await host.StartAsync(timeout.Token);

            using var http = new HttpClient { BaseAddress = new Uri(host.Urls.Single()) };
            using var response = await http.GetAsync("/docs/jazor/app.js", timeout.Token);
            response.EnsureSuccessStatusCode();
            var transformed = await response.Content.ReadAsStringAsync(timeout.Token);
            StringAssert.Contains(transformed, "import.meta.hot");
            StringAssert.Contains(transformed, "/docs/jazor/@vite/client");
            Assert.IsNotNull(response.Headers.ETag);

            var raw = await http.GetStringAsync("/docs/jazor/query-probe.txt?raw&import", timeout.Token);
            StringAssert.Contains(raw, "export default");
            StringAssert.Contains(raw, "query preserved");

            // Use the token Vite publishes to its ordinary browser client.
            var client = await http.GetStringAsync("/docs/jazor/@vite/client", timeout.Token);
            var token = Regex.Match(client, "(?:const|let) wsToken = \"([^\"]+)\"").Groups[1].Value;
            Assert.IsFalse(string.IsNullOrEmpty(token), "Vite client did not expose its WebSocket token.");
            using var socket = new ClientWebSocket();
            socket.Options.AddSubProtocol("vite-hmr");
            var socketUri = new UriBuilder(http.BaseAddress!)
            {
                Scheme = "ws",
                Path = "/docs/jazor/",
                Query = "token=" + Uri.EscapeDataString(token)
            }.Uri;
            await socket.ConnectAsync(socketUri, timeout.Token);
            Assert.AreEqual("vite-hmr", socket.SubProtocol);
            using var connected = JsonDocument.Parse(await ReceiveAsync(socket, timeout.Token));
            Assert.AreEqual("connected", connected.RootElement.GetProperty("type").GetString());

            await File.WriteAllTextAsync(modulePath, module.Replace("value = 1", "value = 2"), timeout.Token);
            using var update = JsonDocument.Parse(await ReceiveAsync(socket, timeout.Token));
            Assert.AreEqual("update", update.RootElement.GetProperty("type").GetString());
            Assert.AreEqual("/app.js", update.RootElement.GetProperty("updates")[0].GetProperty("path").GetString());
            StringAssert.Contains(await http.GetStringAsync("/docs/jazor/app.js?t=2", timeout.Token), "value = 2");
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "test complete", timeout.Token);

            await host.StopAsync(timeout.Token);
            Assert.IsTrue(
                await WaitUntilUnavailableAsync(new Uri($"http://127.0.0.1:{vitePort}/docs/jazor/entry.js"), timeout.Token),
                "The DenoHost-owned Vite process remained reachable after the ASP.NET Core host stopped.");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task Frontend_UsesButDoesNotStopExistingVite()
    {
        var root = Path.Combine(RepositoryTemp.Root, "Jazor.ExternalViteTest", Guid.NewGuid().ToString("N"));
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        Directory.CreateDirectory(root);
        DenoProcess? vite = null;
        try
        {
            await WriteProjectAsync(root, timeout.Token);
            await File.WriteAllTextAsync(Path.Combine(root, "app.js"), "export const existing = true;\n", timeout.Token);
            var vitePort = ReserveLoopbackPort();
            var origin = new Uri($"http://127.0.0.1:{vitePort}");
            vite = new DenoProcess(
                [
                    "task",
                    JazorArtifactDefaults.DevelopmentTaskName,
                    "--host",
                    JazorArtifactDefaults.DevelopmentServerHost,
                    "--port",
                    vitePort.ToString(),
                    "--strictPort",
                    "--base",
                    JazorArtifactDefaults.RequestPath + "/"
                ],
                root);
            await vite.StartAsync(CancellationToken.None);
            await WaitUntilReadyAsync(new Uri(origin, "/jazor/entry.js"), timeout.Token);

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ContentRootPath = root,
                EnvironmentName = Environments.Development
            });
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.AddJazorFrontend(options =>
            {
                options.ProjectRootPath = root;
                options.Vite.ServerOrigin = origin;
                options.Vite.LaunchServer = false;
            });

            await using var host = builder.Build();
            host.UseJazorFrontend();
            await host.StartAsync(timeout.Token);
            using var client = new HttpClient { BaseAddress = new Uri(host.Urls.Single()) };
            StringAssert.Contains(
                await client.GetStringAsync("/jazor/app.js", timeout.Token),
                "existing = true");

            await host.StopAsync(timeout.Token);
            using var direct = new HttpClient();
            using var response = await direct.GetAsync(new Uri(origin, "/jazor/entry.js"), timeout.Token);
            response.EnsureSuccessStatusCode();
            Assert.IsTrue(vite.IsRunning, "Stopping the ASP.NET Core host stopped an externally owned Vite process.");
        }
        finally
        {
            if (vite is not null)
            {
                if (vite.IsRunning)
                    await vite.StopAsync(TimeSpan.Zero, CancellationToken.None);
                vite.Dispose();
            }
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task WriteProjectAsync(string root, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.Combine(root, "wwwroot"));
        await File.WriteAllTextAsync(Path.Combine(root, "package.json"),
            JsonSerializer.Serialize(new
            {
                name = "@jazor/vite-proxy-test",
                @private = true,
                type = "module",
                scripts = new Dictionary<string, string>
                {
                    [JazorArtifactDefaults.DevelopmentTaskName] = "vite"
                },
                devDependencies = new { vite = ViteProjectWriter.Version }
            }), cancellationToken);
        await File.WriteAllTextAsync(
            Path.Combine(root, JazorArtifactDefaults.DevelopmentEntryRelativePath),
            "export const ready = true;\n",
            cancellationToken);

        var restore = await DenoPackageRestorer.RunAsync(
            "DenoHost",
            root,
            ["install", "--package-json", "--node-modules-dir=manual", "--node-modules-linker=hoisted"],
            cancellationToken);
        Assert.IsTrue(restore.Succeeded, restore.StandardError);
    }

    private static int ReserveLoopbackPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private static async Task WaitUntilReadyAsync(Uri uri, CancellationToken cancellationToken)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(1) };
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                using var response = await client.GetAsync(uri, cancellationToken);
                if (response.IsSuccessStatusCode)
                    return;
            }
            catch (HttpRequestException)
            {
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
            }

            await Task.Delay(50, cancellationToken);
        }
    }

    private static async Task<bool> WaitUntilUnavailableAsync(Uri uri, CancellationToken cancellationToken)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromMilliseconds(250) };
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);
        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                using var response = await client.GetAsync(uri, cancellationToken);
            }
            catch (HttpRequestException)
            {
                return true;
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return true;
            }

            await Task.Delay(50, cancellationToken);
        }

        return false;
    }

    private static async Task<string> ReceiveAsync(WebSocket socket, CancellationToken cancellationToken)
    {
        using var output = new MemoryStream();
        var buffer = new byte[4096];
        WebSocketReceiveResult result;
        do
        {
            result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            Assert.AreEqual(WebSocketMessageType.Text, result.MessageType);
            output.Write(buffer, 0, result.Count);
        } while (!result.EndOfMessage);
        return Encoding.UTF8.GetString(output.ToArray());
    }
}
