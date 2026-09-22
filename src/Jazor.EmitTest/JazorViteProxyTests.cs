using System.Diagnostics;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Jazor.AspNetCore.Dev;
using Jazor.Emit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Jazor.EmitTest;

[TestClass]
public sealed class JazorViteProxyTests
{
    [TestMethod]
    public async Task Proxy_ForwardsRealViteModulesQueriesAndHmrUpdates()
    {
        var root = Path.Combine(Path.GetTempPath(), "Jazor.ViteProxyTest", Guid.NewGuid().ToString("N"));
        var deno = Path.Combine(AppContext.BaseDirectory, "runtimes", RuntimeInformation.RuntimeIdentifier,
            "native", OperatingSystem.IsWindows() ? "deno.exe" : "deno");
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        Directory.CreateDirectory(root);
        Process? vite = null;
        Task<string>? errors = null;
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "package.json"),
                JsonSerializer.Serialize(new { type = "module", devDependencies = new { vite = ViteProjectWriter.Version } }), timeout.Token);
            const string module = "export const value = 1;\nif (import.meta.hot) import.meta.hot.accept();\n";
            var modulePath = Path.Combine(root, "app.js");
            await File.WriteAllTextAsync(modulePath, module, timeout.Token);
            await File.WriteAllTextAsync(Path.Combine(root, "query-probe.txt"), "query preserved", timeout.Token);
            var restore = await DenoPackageRestorer.RunAsync(deno, root,
                ["install", "--package-json", "--node-modules-dir=manual", "--node-modules-linker=hoisted"], timeout.Token);
            Assert.IsTrue(restore.Succeeded, restore.StandardError);
            // Port zero gives each test its own listener; Vite owns the filesystem watcher.
            await File.WriteAllTextAsync(Path.Combine(root, "server.js"), """
                import { createServer } from 'vite';
                const server = await createServer({
                  configFile: false, base: '/docs/jazor/', logLevel: 'silent',
                  server: { host: '127.0.0.1', port: 0 }
                });
                await server.listen();
                console.log(server.resolvedUrls.local[0]);
                """, timeout.Token);
            var start = new ProcessStartInfo(deno)
            {
                WorkingDirectory = root, UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardOutput = true, RedirectStandardError = true
            };
            foreach (var argument in new[] { "run", "-A", "server.js" })
                start.ArgumentList.Add(argument);
            vite = Process.Start(start)!;
            errors = vite.StandardError.ReadToEndAsync();
            var address = await vite.StandardOutput.ReadLineAsync(timeout.Token);
            Assert.IsNotNull(address, vite.HasExited ? await errors : "Vite did not publish its URL.");

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { ContentRootPath = root });
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Services.AddJazorViteProxy(options => options.ServerOrigin = new Uri(new Uri(address), "/"));
            await using var proxy = builder.Build();
            proxy.UsePathBase("/docs");
            proxy.UseJazorViteProxy();
            await proxy.StartAsync(timeout.Token);
            using var http = new HttpClient { BaseAddress = new Uri(proxy.Urls.Single()) };
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
                Scheme = "ws", Path = "/docs/jazor/", Query = "token=" + Uri.EscapeDataString(token)
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
            await proxy.StopAsync(timeout.Token);
        }
        finally
        {
            if (vite is not null)
            {
                if (!vite.HasExited)
                    vite.Kill(entireProcessTree: true);
                await vite.WaitForExitAsync();
                if (errors is not null)
                    _ = await errors;
                vite.Dispose();
            }
            Directory.Delete(root, recursive: true);
        }
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
