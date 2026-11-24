#!/usr/bin/env dotnet run
#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:project ../../src/Jazor.NodeHost/Jazor.NodeHost.csproj
#:sdk Microsoft.NET.Sdk.Web
#:project ../../src/Jazor.AspNetCore/Jazor.AspNetCore.csproj
#:project ../../src/Jazor.AspNetCore.Dev/Jazor.AspNetCore.Dev.csproj
#:include ../../samples/Jolt.AspNetCore/JoltDevSession.cs

using Jazor.NodeHost;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Jazor.AspNetCore;
using Jazor.AspNetCore.Dev;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var repoRoot = RequireRepoRoot();
var temporaryRoot = Path.GetTempPath();
ConfigureRepositoryEnvironment(repoRoot);
if (args is ["--jolt-force-stop", var stopFeed, var stopVersion])
{
    await VerifyJoltForceStopAsync(repoRoot, temporaryRoot, Path.GetFullPath(stopFeed, repoRoot), stopVersion);
    return;
}
if (args is ["--jolt-proxy", var jolt, var chromium, var feed, var version])
{
    await VerifyJoltProxyAsync(repoRoot, temporaryRoot, Path.GetFullPath(jolt, repoRoot), Path.GetFullPath(chromium, repoRoot),
        Path.GetFullPath(feed, repoRoot), version);
    return;
}
var workspace = Path.Combine(repoRoot, ".tmp", "development-hmr-browser", Guid.NewGuid().ToString("N"));
var webRoot = Path.Combine(workspace, "wwwroot");
var artifactRoot = Path.Combine(workspace, "jazor");
var componentPath = Path.Combine(artifactRoot, "component.mjs");
var componentSourceMapPath = componentPath + ".map";
var manifestPath = Path.Combine(artifactRoot, "jazor-manifest.json");
var vueRuntimePath = Path.Combine(webRoot, "vue.runtime.esm-browser.js");
var browserSessionName = "jazor-development-hmr-" + Guid.NewGuid().ToString("N");
var browserSessionOpened = false;
WebApplication? app = null;

try
{
    Directory.CreateDirectory(webRoot);
    Directory.CreateDirectory(artifactRoot);
    await RestoreVueRuntimeAsync(repoRoot, artifactRoot);
    var vueRuntimeSourcePath = Path.Combine(
        artifactRoot,
        "node_modules",
        "vue",
        "dist",
        "vue.runtime.esm-browser.js");
    if (!File.Exists(vueRuntimeSourcePath))
        throw new InvalidOperationException("pnpm restore did not produce the Vue browser runtime at '" + vueRuntimeSourcePath + "'.");
    File.Copy(vueRuntimeSourcePath, vueRuntimePath);
    await WriteFixtureAsync(componentPath, componentSourceMapPath, manifestPath, "v1", "content-v1", "template-v1");
    await File.WriteAllTextAsync(Path.Combine(artifactRoot, "entry.js"), "export {};\n");
    var indexPath = Path.Combine(webRoot, "index.html");
    await File.WriteAllTextAsync(indexPath, CreateBrowserHarness(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

    var port = AllocateLoopbackPort();
    var baseAddress = new Uri("http://127.0.0.1:" + port + "/");
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        ContentRootPath = workspace,
        WebRootPath = webRoot,
        EnvironmentName = Environments.Development
    });
    builder.WebHost.UseUrls(baseAddress.ToString());
    builder.Logging.SetMinimumLevel(LogLevel.Warning);
    builder.Logging.AddFilter("Jazor.AspNetCore.Dev", LogLevel.Debug);
    builder.Services.AddJazorReload(options =>
    {
        // The mapping itself must be enough to observe a custom artifact root.
        options.WatchPaths.Clear();
        options.HmrMappings.Clear();
        options.HmrMappings.Add(new JazorHmrMapping
        {
            ArtifactRootPath = "jazor",
            RequestPath = "/jazor"
        });
        options.DebounceInterval = TimeSpan.FromMilliseconds(35);
        options.PollingInterval = TimeSpan.FromMilliseconds(75);
    });

    app = builder.Build();
    app.UseJazorReload();
    app.UseJazorArtifacts();
    app.UseStaticFiles();
    app.MapGet("/", () => Results.File(indexPath, "text/html; charset=utf-8"));
    app.MapPost("/__jazor-hmr-fixture/update", async () =>
    {
        // The host deliberately falls back to a full reload before the browser advertises
        // module-update capability. The browser waits for JazorHmr.ready before this request.
        await Task.Delay(250);
        await WriteFixtureAsync(componentPath, componentSourceMapPath, manifestPath, "v2", "content-v2", "template-v2");
        // Keep the request open until watcher/poller delivery can reach the browser.
        await Task.Delay(750);
        return Results.NoContent();
    });
    await app.StartAsync();

    var browserOpen = await RunPlaywrightApiAsync(repoRoot, browserSessionName, "open", baseAddress.ToString());
    EnsureProcessSucceeded(browserOpen, "Playwright could not open the development HMR fixture.");
    browserSessionOpened = true;

    using var httpClient = new HttpClient();
    await VerifyBrowserSourceMapAsync(httpClient, baseAddress, "v1");

    var ready = await WaitForBrowserConditionAsync(
        repoRoot,
        browserSessionName,
        "(() => { const component = document.getElementById('component-version')?.textContent; const count = document.getElementById('parent-count')?.textContent; return component === 'child:v1' && count === '0' ? 'ready:v1' : null; })()",
        TimeSpan.FromSeconds(10));
    if (!ready.Contains("ready:v1", StringComparison.Ordinal))
    {
        var browserState = await ReadBrowserStateAsync(repoRoot, browserSessionName);
        throw new InvalidOperationException(
            "Development HMR fixture did not register its v1 handler: " + ready + Environment.NewLine + browserState);
    }

    var click = await RunPlaywrightApiAsync(
        repoRoot,
        browserSessionName,
        "eval",
        "() => { document.getElementById('increment').click(); return 'clicked'; }");
    EnsureProcessSucceeded(click, "Playwright could not increment the parent component state.");
    var incremented = await WaitForBrowserConditionAsync(
        repoRoot,
        browserSessionName,
        "(() => document.getElementById('parent-count')?.textContent === '1' ? 'parent-count:1' : null)()",
        TimeSpan.FromSeconds(10));
    if (!incremented.Contains("parent-count:1", StringComparison.Ordinal))
        throw new InvalidOperationException("Development HMR fixture did not render the parent state increment: " + incremented);

    using var updateResponse = await httpClient.PostAsync(
        new Uri(baseAddress, "__jazor-hmr-fixture/update"),
        content: null);
    updateResponse.EnsureSuccessStatusCode();

    var update = await WaitForBrowserConditionAsync(
        repoRoot,
        browserSessionName,
        "(() => { const loads = sessionStorage.getItem('jazor-hmr-fixture-loads'); if (loads !== '1') return 'full-reload:' + loads; const component = document.getElementById('component-version')?.textContent; const count = document.getElementById('parent-count')?.textContent; return component === 'child:v2' && count === '1' ? 'module-update:v2:loads=' + loads : null; })()",
        TimeSpan.FromSeconds(10));
    if (!update.Contains("module-update:v2:loads=1", StringComparison.Ordinal))
    {
        throw new InvalidOperationException(
            "Development HMR browser verification did not apply the v2 module update." + Environment.NewLine +
            update);
    }

    await VerifyBrowserSourceMapAsync(httpClient, baseAddress, "v2");

    Console.WriteLine("Development HMR browser verification passed.");
    Console.WriteLine("  Browser: Playwright API / NodeApi");
    Console.WriteLine("  Flow: generated-component registration -> Vue HMR reload -> parent state preserved -> authored Razor source map available");
}
finally
{
    if (browserSessionOpened)
    {
        try
        {
            _ = await RunPlaywrightApiAsync(repoRoot, browserSessionName, "close");
        }
        catch (System.ComponentModel.Win32Exception)
        {
        }
    }

    if (app is not null)
    {
        await app.StopAsync();
        await app.DisposeAsync();
    }

    try
    {
        if (Directory.Exists(workspace))
            Directory.Delete(workspace, recursive: true);
    }
    catch (IOException)
    {
    }
    catch (UnauthorizedAccessException)
    {
    }
}

static async Task VerifyJoltForceStopAsync(string repo, string temporaryRoot, string feed, string version)
{
    var root = Directory.CreateDirectory(Path.Combine(temporaryRoot, "jazor-jolt-force-stop-" + Guid.NewGuid().ToString("N"))).FullName;
    var frontend = Path.Combine(root, "frontend");
    var pidPath = Path.Combine(root, "vite.pid");
    var closePath = Path.Combine(root, "vite.closed");
    var vitePort = AllocateLoopbackPort();
    var hostPort = AllocateLoopbackPort();
    while (hostPort == vitePort)
        hostPort = AllocateLoopbackPort();
    using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(3));
    var token = deadline.Token;
    using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(1) };
    try
    {
        CopyAspNetSample(repo, root);
        var program = Path.Combine(root, "Program.cs");
        var original = await File.ReadAllTextAsync(program, token);
        var isolated = original.Replace("http://127.0.0.1:15658", $"http://127.0.0.1:{vitePort}", StringComparison.Ordinal);
        if (isolated == original)
            throw new InvalidOperationException("The sample Vite origin could not be isolated.");
        await File.WriteAllTextAsync(program, isolated, token);
        var config = Path.Combine(frontend, "vite.config.js");
        await File.AppendAllTextAsync(config,
            "\nimport { writeFileSync, appendFileSync } from 'node:fs';\nwriteFileSync(" + JsonSerializer.Serialize(pidPath) + ", String(process.pid));\n", token);
        var configText = await File.ReadAllTextAsync(config, token);
        const string export = "export default ";
        if (!configText.Contains(export, StringComparison.Ordinal))
            throw new InvalidOperationException("Expected the sample Vite default export.");
        configText = configText.Replace(export, "const fixtureConfig = ", StringComparison.Ordinal);
        await File.WriteAllTextAsync(config, configText + $$"""

            fixtureConfig.plugins = [...(fixtureConfig.plugins ?? []), {
                name: 'verify-force-stop',
                configureServer(server) {
                    server.httpServer.once('close', () => appendFileSync({{JsonSerializer.Serialize(closePath)}}, `http:${process.pid}\n`));
                },
                closeBundle() {
                    appendFileSync({{JsonSerializer.Serialize(closePath)}}, `bundle:${process.pid}\n`);
                },
            }];
            export default fixtureConfig;
            """, token);
        await WriteJoltToolManifestAsync(root, version, token);
        var nugetConfig = Path.Combine(root, "NuGet.Config");
        new XElement("configuration", new XElement("packageSources", new XElement("clear"),
            new XElement("add", new XAttribute("key", "candidate"), new XAttribute("value", feed)),
            new XElement("add", new XAttribute("key", "nuget.org"), new XAttribute("value", "https://api.nuget.org/v3/index.json"))))
            .Save(nugetConfig);
        using (var restore = Start("dotnet", "tool", "restore", "--configfile", nugetConfig))
            await RequireSuccess(restore);
        using (var build = Start("dotnet", "build", "-o", "out"))
            await RequireSuccess(build);

        await CheckAsync(attached: true);
        await CheckAsync(attached: false);
        await CheckEofAsync();
    }
    finally
    {
        // The dotnet tool wrapper exits after its apphost. Windows can retain its
        // working directory for a moment even after the Vite process has exited.
        for (var retry = 0; ; retry++)
        {
            try
            {
                Directory.Delete(root, recursive: true);
                break;
            }
            catch (IOException) when (retry < 20)
            {
                await Task.Delay(250);
            }
        }
        Console.WriteLine($"Removed force-stop fixture {root}");
    }

    async Task CheckAsync(bool attached)
    {
        File.Delete(pidPath);
        File.Delete(closePath);
        using var external = attached ? Start("dotnet", "tool", "run", "jolt", "dev", frontend, "--port", vitePort.ToString()) : null;
        Process? host = null;
        Process? vite = null;
        try
        {
            if (external is not null)
                await WaitReadyAsync(vitePort, external);
            host = Start("dotnet", Path.Combine(root, "out", "Jolt.AspNetCore.dll"), "--environment", "Development",
                "--urls", $"http://127.0.0.1:{hostPort}");
            await WaitReadyAsync(hostPort, host);
            var pid = int.Parse(await File.ReadAllTextAsync(pidPath, token), System.Globalization.CultureInfo.InvariantCulture);
            vite = Process.GetProcessById(pid);
            // Hold the original process handle during the kill and cleanup, so a reused PID
            // cannot accidentally identify a different test or a user's process.
            _ = vite.Handle;
            if (vite.Id == host.Id || vite.HasExited)
                throw new InvalidOperationException("The fixture did not identify its live Jolt process.");
            Console.WriteLine($"{(attached ? "Attached" : "Owned")}: host PID {host.Id}, Jolt PID {vite.Id}. Killing only the host PID.");
            host.Kill();
            await host.WaitForExitAsync(token);
            if (!attached)
            {
                try { await vite.WaitForExitAsync(token).WaitAsync(TimeSpan.FromSeconds(15), token); }
                catch (TimeoutException ex)
                {
                    throw new InvalidOperationException($"Owned Jolt PID {vite.Id} remained alive after host termination. Vite still answers HTTP: {await IsReadyAsync(vitePort)}.", ex);
                }
            }
            await WaitForPortAsync(hostPort);
            if (!attached)
                await WaitForPortAsync(vitePort);
            var hostFree = CanBind(hostPort);
            var viteFree = CanBind(vitePort);
            var viteReady = await IsReadyAsync(vitePort);
            Console.WriteLine($"Host port reusable: {hostFree}. Vite port reusable: {viteFree}. Vite answers HTTP: {viteReady}.");
            if (!hostFree || (attached ? viteFree || !viteReady || vite.HasExited : !viteFree || viteReady || !vite.HasExited))
                throw new InvalidOperationException($"{(attached ? "Attached" : "Owned")} force-stop failed (host port {hostPort}, Vite port {vitePort}, Jolt process exited: {vite.HasExited}).");
            if (attached ? File.Exists(closePath) : !ClosedNormally(vite.Id))
                throw new InvalidOperationException($"Unexpected Vite shutdown hooks after force-stopping the {(attached ? "attached" : "owning")} host.");
        }
        finally
        {
            await StopAsync(host);
            await StopAsync(vite);
            await StopAsync(external);
            host?.Dispose();
            vite?.Dispose();
        }
    }

    async Task WaitForPortAsync(int port)
    {
        var wait = Stopwatch.StartNew();
        while (!CanBind(port) && wait.Elapsed < TimeSpan.FromSeconds(5))
            await Task.Delay(100, token);
    }

    async Task CheckEofAsync()
    {
        foreach (var optIn in new[] { false, true })
        {
            File.Delete(pidPath);
            File.Delete(closePath);
            string[] arguments = ["tool", "run", "jolt", "dev", frontend, "--port", vitePort.ToString(),
                .. optIn ? new[] { "--stop-on-stdin-close" } : []];
            using var cli = StartProcess("dotnet", arguments, redirectInput: true);
            Process? vite = null;
            try
            {
                await WaitReadyAsync(vitePort, cli);
                vite = Process.GetProcessById(int.Parse(await File.ReadAllTextAsync(pidPath, token), System.Globalization.CultureInfo.InvariantCulture));
                _ = vite.Handle;
                cli.StandardInput.Close();
                if (optIn)
                {
                    await cli.WaitForExitAsync(token).WaitAsync(TimeSpan.FromSeconds(15), token);
                    if (cli.ExitCode != 0 || !vite.HasExited || !CanBind(vitePort) || !ClosedNormally(vite.Id))
                        throw new InvalidOperationException("Opt-in stdin EOF did not exit successfully and close Vite normally.");
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), token);
                    if (cli.HasExited || vite.HasExited || File.Exists(closePath) || !await IsReadyAsync(vitePort))
                        throw new InvalidOperationException("Standalone Jolt stopped after stdin EOF without opting in.");
                }
            }
            finally
            {
                await StopAsync(cli);
                await StopAsync(vite);
                vite?.Dispose();
            }
        }
    }

    bool ClosedNormally(int pid)
    {
        var events = File.Exists(closePath) ? File.ReadAllLines(closePath) : [];
        return events.Contains($"http:{pid}") && events.Contains($"bundle:{pid}");
    }

    async Task WaitReadyAsync(int port, Process owner)
    {
        while (!await IsReadyAsync(port))
        {
            if (owner.HasExited)
                throw new InvalidOperationException($"Fixture process {owner.Id} exited with {owner.ExitCode} before readiness.");
            await Task.Delay(100, token);
        }
    }

    async Task<bool> IsReadyAsync(int port)
    {
        try
        {
            using var response = await http.GetAsync($"http://127.0.0.1:{port}/portal/frontend/.jolt/Counter.mjs", token);
            return response.IsSuccessStatusCode && (await response.Content.ReadAsStringAsync(token)).Contains("Jolt counter", StringComparison.Ordinal);
        }
        catch (HttpRequestException) { return false; }
        catch (OperationCanceledException) when (!token.IsCancellationRequested) { return false; }
    }

    Process Start(string executable, params string[] arguments) => StartProcess(executable, arguments);

    Process StartProcess(string executable, string[] arguments, bool redirectInput = false)
    {
        var start = new ProcessStartInfo(executable) { WorkingDirectory = root, UseShellExecute = false, RedirectStandardInput = redirectInput };
        start.Environment["DOTNET_CLI_HOME"] = Path.Combine(root, "tool-home");
        if (arguments[0] == "tool" || executable == "dotnet" && arguments[0].EndsWith(".dll", StringComparison.Ordinal))
            start.Environment["NUGET_PACKAGES"] = Path.Combine(root, "tool-packages");
        foreach (var argument in arguments)
            start.ArgumentList.Add(argument);
        return Process.Start(start) ?? throw new InvalidOperationException($"Could not start {executable}.");
    }

    async Task RequireSuccess(Process process)
    {
        try
        {
            await process.WaitForExitAsync(token);
            if (process.ExitCode != 0)
                throw new InvalidOperationException($"Fixture setup exited with code {process.ExitCode}.");
        }
        finally { await StopAsync(process); }
    }

    static async Task StopAsync(Process? process)
    {
        if (process is null || process.HasExited)
            return;
        try { process.Kill(entireProcessTree: true); }
        catch (InvalidOperationException) when (process.HasExited) { }
        await process.WaitForExitAsync();
    }

    static bool CanBind(int port)
    {
        using var listener = new TcpListener(IPAddress.Loopback, port);
        try { listener.Start(); return true; }
        catch (SocketException exception) when (exception.SocketErrorCode == SocketError.AddressAlreadyInUse)
        {
            return false;
        }
    }
}

static async Task WriteJoltToolManifestAsync(string root, string version, CancellationToken token)
{
    var manifest = Path.Combine(Directory.CreateDirectory(Path.Combine(root, ".config")).FullName, "dotnet-tools.json");
    await File.WriteAllTextAsync(manifest, JsonSerializer.Serialize(new
    {
        version = 1,
        isRoot = true,
        tools = new Dictionary<string, object> { ["jazor.jolt"] = new { version, commands = new[] { "jolt" } } }
    }), token);
}

static string CopyAspNetSample(string repo, string destination)
{
    var sample = Path.Combine(repo, "samples", "Jolt.AspNetCore");
    var frontend = Directory.CreateDirectory(Path.Combine(destination, "frontend")).FullName;
    foreach (var name in new[] { "Program.cs", "JoltDevSession.cs" })
        File.Copy(Path.Combine(sample, name), Path.Combine(destination, name));
    foreach (var file in Directory.EnumerateFiles(Path.Combine(sample, "frontend")))
        File.Copy(file, Path.Combine(frontend, Path.GetFileName(file)));
    var project = XDocument.Load(Path.Combine(sample, "Jolt.AspNetCore.csproj"));
    foreach (var reference in project.Descendants("ProjectReference"))
        reference.SetAttributeValue("Include", Path.GetFullPath(reference.Attribute("Include")!.Value, sample));
    var path = Path.Combine(destination, "Jolt.AspNetCore.csproj");
    project.Save(path);
    return path;
}

static async Task VerifyJoltProxyAsync(string repo, string temporaryRoot, string jolt, string chromium, string feed, string version)
{
    var root = Directory.CreateDirectory(Path.Combine(repo, ".tmp", "jolt-proxy-" + Guid.NewGuid().ToString("N"))).FullName;
    var toolRoot = Path.Combine(temporaryRoot, "jolt-proxy-tool-" + Guid.NewGuid().ToString("N"));
    var sample = Path.Combine(repo, "samples", "Jolt.Counter");
    foreach (var name in new[] { "package.json", "pnpm-lock.yaml", "index.html", "main.js", "vite.config.js" })
        File.Copy(Path.Combine(sample, name), Path.Combine(root, name));
    foreach (var file in Directory.EnumerateFiles(sample, "*.jazor"))
        File.Copy(file, Path.Combine(root, Path.GetFileName(file)));
    var config = Path.Combine(root, "vite.config.js");
    await File.WriteAllTextAsync(config, (await File.ReadAllTextAsync(config))
        .Replace("defineConfig({", "defineConfig({\n    base: '/prefix/jazor/',", StringComparison.Ordinal));
    using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(3));
    var token = deadline.Token;
    var vitePort = AllocateLoopbackPort();
    var vite = new Uri($"http://127.0.0.1:{vitePort}/");
    var host = new Uri($"http://127.0.0.1:{AllocateLoopbackPort()}/");
    using var external = Start(root, jolt, "dev", root, "--port", vitePort.ToString());
    using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
    try
    {
        var entry = new Uri(vite, "prefix/jazor/index.html");
        while (!await IsReadyAsync(entry))
        {
            if (external.HasExited)
                throw new InvalidOperationException($"External Jolt exited with {external.ExitCode}. Fixture: {root}");
            await Task.Delay(200, token);
        }
        await using (var proxy = CreateProxy(owned: false))
        {
            await proxy.StartAsync(token);
            await VerifyBrowserAsync();
            await proxy.StopAsync(token);
        }
        if (external.HasExited || !await IsReadyAsync(entry))
            throw new InvalidOperationException("ASP.NET shutdown stopped the external Jolt server.");
        Console.WriteLine("ASP.NET proxied Jolt HTTP and WebSocket HMR, and the external Jolt server survived host shutdown.");
    }
    catch
    {
        Console.Error.WriteLine($"Jolt proxy fixture preserved at {root}.");
        throw;
    }
    finally
    {
        await StopProcessAsync(external);
    }
    try
    {
        await WriteJoltToolManifestAsync(toolRoot, version, token);
        var nugetConfig = Path.Combine(toolRoot, "NuGet.Config");
        new XElement("configuration", new XElement("packageSources",
            new XElement("clear"), new XElement("add", new XAttribute("key", "candidate"), new XAttribute("value", feed))))
            .Save(nugetConfig);
        var packageCache = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
        var cliHome = Environment.GetEnvironmentVariable("DOTNET_CLI_HOME");
        Environment.SetEnvironmentVariable("NUGET_PACKAGES", Path.Combine(toolRoot, "packages"));
        Environment.SetEnvironmentVariable("DOTNET_CLI_HOME", toolRoot);
        try
        {
            await RunAsync(toolRoot, "dotnet", "tool", "restore", "--configfile", nugetConfig);
            await VerifyPortConflictAsync();
            await using var proxy = CreateProxy(owned: true);
            await proxy.StartAsync(token);
            await VerifyBrowserAsync();
            await proxy.StopAsync(token);
        }
        finally
        {
            Environment.SetEnvironmentVariable("NUGET_PACKAGES", packageCache);
            Environment.SetEnvironmentVariable("DOTNET_CLI_HOME", cliHome);
        }
        if (await IsReadyAsync(new Uri(vite, "prefix/jazor/index.html")))
            throw new InvalidOperationException("The ASP.NET-owned Jolt server still answered after host shutdown.");
        var listener = new TcpListener(IPAddress.Loopback, vitePort);
        listener.Start();
        listener.Stop();
        Console.WriteLine("ASP.NET launched the installed Jolt tool, proxied HMR, and released Vite's port at shutdown.");
        await VerifyPublishedSampleAsync();
    }
    catch
    {
        Console.Error.WriteLine($"Jolt proxy fixture preserved at {root}.");
        throw;
    }
    finally
    {
        if (Directory.Exists(toolRoot))
            Directory.Delete(toolRoot, recursive: true);
    }
    Directory.Delete(root, recursive: true);

    async Task VerifyPortConflictAsync()
    {
        var listener = new TcpListener(IPAddress.Loopback, vitePort);
        listener.Start();
        try
        {
            using var rejectProbe = CancellationTokenSource.CreateLinkedTokenSource(token);
            var rejection = RejectReadinessAsync(rejectProbe.Token);
            try
            {
                using var cli = Start(root, jolt, "dev", root, "--port", vitePort.ToString());
                try
                {
                    using var conflictDeadline = CancellationTokenSource.CreateLinkedTokenSource(token);
                    conflictDeadline.CancelAfter(TimeSpan.FromSeconds(30));
                    await cli.WaitForExitAsync(conflictDeadline.Token);
                    if (cli.ExitCode == 0)
                        throw new InvalidOperationException("Jolt accepted an occupied Vite port.");
                }
                finally
                {
                    await StopProcessAsync(cli);
                }
                await using var proxy = CreateProxy(owned: true);
                try
                {
                    await proxy.StartAsync(token);
                    throw new InvalidOperationException("The ASP.NET host reported readiness on an occupied Vite port.");
                }
                catch (InvalidOperationException exception) when (exception.Message.Contains("Jolt dev exited with code", StringComparison.Ordinal))
                {
                    Console.WriteLine("The CLI and ASP.NET host failed on the occupied port. The next host launch retries that same port.");
                }
            }
            finally
            {
                await rejectProbe.CancelAsync();
                await rejection;
            }
        }
        finally
        {
            listener.Stop();
        }

        async Task RejectReadinessAsync(CancellationToken stop)
        {
            var response = Encoding.ASCII.GetBytes("HTTP/1.1 503 Service Unavailable\r\nContent-Length: 0\r\nConnection: close\r\n\r\n");
            try
            {
                while (true)
                {
                    using var connection = await listener.AcceptTcpClientAsync(stop);
                    await connection.GetStream().WriteAsync(response, stop);
                }
            }
            catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
        }
    }

    async Task VerifyPublishedSampleAsync()
    {
        var project = Path.Combine(root, "production-project");
        var csproj = CopyAspNetSample(repo, project);
        var frontend = Path.Combine(project, "frontend");
        var buildLog = Path.Combine(project, "vite-builds.log");
        var config = Path.Combine(frontend, "vite.config.js");
        await File.WriteAllTextAsync(config, """
            import { appendFileSync } from 'node:fs';
            import { defineConfig } from 'vite';
            export default defineConfig({
                base: '/portal/frontend/',
                plugins: [{ name: 'fixture-build-count', buildStart() {
                    appendFileSync(new URL('../vite-builds.log', import.meta.url), 'build\n');
                } }]
            });
            """, token);
        await File.WriteAllTextAsync(Path.Combine(frontend, "lazy-proof.js"),
            "export const message = 'from the lazy chunk';", token);
        await File.AppendAllTextAsync(Path.Combine(frontend, "main.js"), """

            const load = document.createElement('button');
            load.id = 'load-lazy';
            load.textContent = 'Load optional module';
            document.body.append(load);
            load.onclick = async () => {
                const { message } = await import('./lazy-proof.js');
                load.textContent = message;
            };
            """, token);
        await RunAsync(project, jolt, "bundle", frontend);
        var publish = Path.Combine(root, "published-sample");
        await RunAsync(project, "dotnet", "publish", csproj, "--nologo", "-o", publish);
        if (!(await File.ReadAllLinesAsync(buildLog, token)).SequenceEqual(["build"]))
            throw new InvalidOperationException("Expected exactly one Vite build across Jolt bundle and .NET publish.");
        var dist = Path.Combine(publish, "frontend", "dist");
        var expected = Directory.EnumerateFiles(Path.Combine(frontend, "dist"), "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(frontend, path)).ToHashSet(StringComparer.Ordinal);
        var actual = Directory.EnumerateFiles(Path.Combine(publish, "frontend"), "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(Path.Combine(publish, "frontend"), path));
        if (!File.Exists(Path.Combine(dist, "index.html")) || !expected.SetEquals(actual))
            throw new InvalidOperationException("The published sample must contain exactly the built frontend assets.");
        var production = new Uri($"http://127.0.0.1:{AllocateLoopbackPort()}/portal/frontend/index.html");
        using var server = Start(publish, "dotnet", "Jolt.AspNetCore.dll", "--environment", "Production",
            "--urls", production.GetLeftPart(UriPartial.Authority));
        try
        {
            while (!await IsReadyAsync(production))
            {
                if (server.HasExited)
                    throw new InvalidOperationException($"The published sample exited with {server.ExitCode}.");
                await Task.Delay(100, token);
            }
            await RunAsync(root, "dotnet", "run", "--file", Path.Combine(sample, "verify-browser.cs"), "--",
                chromium, production.ToString(), "--published-counter", "--published-lazy");
        }
        finally
        {
            await StopProcessAsync(server);
        }
        Console.WriteLine("One Vite build produced the published SPA. Chromium clicked the counter and loaded the lazy chunk on demand.");
    }

    Task VerifyBrowserAsync() => RunAsync(root, "dotnet", "run", "--file", Path.Combine(sample, "verify-browser.cs"), "--",
        chromium, new Uri(host, "prefix/jazor/").ToString(), Path.Combine(root, "CounterButton.jazor"), "--proxy-hmr");

    async Task RunAsync(string directory, string executable, params string[] arguments)
    {
        using var child = Start(directory, executable, arguments);
        try
        {
            await child.WaitForExitAsync(token);
            if (child.ExitCode != 0)
                throw new InvalidOperationException($"{executable} {arguments[0]} exited with {child.ExitCode}.");
        }
        finally
        {
            await StopProcessAsync(child);
        }
    }

    WebApplication CreateProxy(bool owned)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = owned ? toolRoot : root,
            EnvironmentName = Environments.Development
        });
        builder.WebHost.UseUrls(host.ToString());
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
        if (owned)
            builder.Services.AddHostedService<JoltDevSession>();
        builder.AddJazorFrontend(options =>
        {
            options.ProjectRootPath = root;
            options.PathBase = "/prefix";
            options.RequestPath = "/jazor";
            options.DevelopmentEntryRelativePath = "index.html";
            options.Vite.ServerOrigin = vite;
            options.Vite.LaunchServer = false;
        });
        var proxy = builder.Build();
        proxy.UseJazorPathBase();
        proxy.UseJazorFrontend();
        return proxy;
    }

    async Task<bool> IsReadyAsync(Uri url)
    {
        try
        {
            using var response = await http.GetAsync(url, token);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException) { return false; }
        catch (TaskCanceledException) when (!token.IsCancellationRequested) { return false; }
    }

    static Process Start(string directory, string executable, params string[] arguments)
    {
        var start = new ProcessStartInfo(executable) { WorkingDirectory = directory, UseShellExecute = false };
        foreach (var argument in arguments)
            start.ArgumentList.Add(argument);
        return Process.Start(start) ?? throw new InvalidOperationException($"Could not start {executable}.");
    }

    static async Task StopProcessAsync(Process process)
    {
        if (!process.HasExited)
        {
            process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
        }
    }
}

static async Task WriteFixtureAsync(
    string componentPath,
    string sourceMapPath,
    string manifestPath,
    string version,
    string contentHash,
    string templateHash)
{
    await File.WriteAllTextAsync(
        componentPath,
        CreateVueComponentModule(version),
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    await using (var sourceMapStream = new FileStream(
                     sourceMapPath,
                     FileMode.Create,
                     FileAccess.Write,
                     FileShare.Read,
                     bufferSize: 4096,
                     useAsync: true))
    using (var sourceMapWriter = new Utf8JsonWriter(sourceMapStream))
    {
        sourceMapWriter.WriteStartObject();
        sourceMapWriter.WriteNumber("version", 3);
        sourceMapWriter.WriteString("file", "component.mjs");
        sourceMapWriter.WriteStartArray("sources");
        sourceMapWriter.WriteStringValue("Pages/HmrFixture.razor");
        sourceMapWriter.WriteEndArray();
        sourceMapWriter.WriteStartArray("sourcesContent");
        sourceMapWriter.WriteStringValue("<span>child:" + version + "</span>");
        sourceMapWriter.WriteEndArray();
        sourceMapWriter.WriteStartArray("names");
        sourceMapWriter.WriteEndArray();
        sourceMapWriter.WriteString("mappings", "AAAA");
        sourceMapWriter.WriteEndObject();
        await sourceMapWriter.FlushAsync();
    }

    await using var manifestStream = new FileStream(
        manifestPath,
        FileMode.Create,
        FileAccess.Write,
        FileShare.Read,
        bufferSize: 4096,
        useAsync: true);
    using var writer = new Utf8JsonWriter(manifestStream);
    writer.WriteStartObject();
    writer.WriteNumber("schemaVersion", 1);
    writer.WriteNumber("runtimeProtocolVersion", 1);
    writer.WriteString("rootAssemblyName", "HmrFixture");
    writer.WriteStartArray("entries");
    writer.WriteStringValue("component.mjs");
    writer.WriteEndArray();
    writer.WriteStartArray("modules");
    writer.WriteStartObject();
    writer.WriteString("assemblyName", "HmrFixture");
    writer.WriteString("typeName", "Fixture.Component");
    writer.WriteString("id", "Fixture.Component");
    writer.WriteString("path", "component.mjs");
    writer.WriteString("contentHash", contentHash);
    writer.WriteStartObject("hmr");
    writer.WriteString("providerId", "jazor.vue");
    writer.WriteString("moduleId", "fixture:component");
    writer.WriteStartObject("data");
    writer.WriteString("componentId", "Fixture.Component");
    writer.WriteString("descriptorHash", "descriptor-v1");
    writer.WriteString("templateHash", templateHash);
    writer.WriteString("logicHash", "logic-v1");
    writer.WriteString("boundaryKind", "template-only");
    writer.WriteEndObject();
    writer.WriteEndObject();
    writer.WriteEndObject();
    writer.WriteEndArray();
    writer.WriteEndObject();
    await writer.FlushAsync();
}

static string CreateVueComponentModule(string version)
    => $$"""
        import { defineComponent, h } from "/vue.runtime.esm-browser.js";
        const __jazorComponent = defineComponent({
          name: "JazorHmrFixtureComponent",
          setup() {
            return () => h("span", { id: "component-version" }, "child:{{version}}");
          }
        });
        if (globalThis.JazorHmr && typeof globalThis.JazorHmr.registerVueComponent === "function") {
          globalThis.JazorHmr.registerVueComponent("fixture:component", __jazorComponent);
        }
        export default __jazorComponent;
        //# sourceMappingURL=component.mjs.map
        """;

static string CreateBrowserHarness()
    => """
        <!doctype html>
        <html lang="en">
          <head>
            <meta charset="utf-8">
            <title>Jazor Development HMR Fixture</title>
          </head>
          <body>
            <div id="app"></div>
            <script type="module">
              import { createApp, defineComponent, h, ref } from "/vue.runtime.esm-browser.js";

              const trace = JSON.parse(sessionStorage.getItem("jazor-hmr-fixture-trace") || "[]");
              const record = value => {
                trace.push(value);
                sessionStorage.setItem("jazor-hmr-fixture-trace", JSON.stringify(trace));
              };
              const complete = value => {
                sessionStorage.setItem("jazor-hmr-fixture-result", value);
                document.documentElement.setAttribute("data-jazor-hmr-result", value);
              };
              const loads = Number(sessionStorage.getItem("jazor-hmr-fixture-loads") || "0") + 1;
              sessionStorage.setItem("jazor-hmr-fixture-loads", String(loads));
              window.addEventListener("jazor:module-update", event => {
                record("event:" + event.detail.moduleUpdates.map(update => update.moduleId).join(","));
              });

              async function waitForHmr() {
                for (let index = 0; index < 120; index++) {
                  if (window.JazorHmr) {
                    return window.JazorHmr;
                  }
                  await new Promise(resolve => setTimeout(resolve, 25));
                }
                throw new Error("JazorHmr was not initialized");
              }

              try {
                if (loads > 1) {
                  complete("full-reload:" + (sessionStorage.getItem("jazor-hmr-fixture-result") || "before-handler") + ":" + trace.join(","));
                } else {
                  const hmr = await waitForHmr();
                  document.documentElement.setAttribute("data-jazor-hmr-phase", "transport-pending");
                  await hmr.ready;
                  document.documentElement.setAttribute("data-jazor-hmr-phase", "transport-ready");
                  const initial = await import("/jazor/component.mjs");
                  const host = defineComponent({
                    setup() {
                      const count = ref(0);
                      return () => h("section", null, [
                        h("button", { id: "increment", onClick: () => { count.value++; } }, "increment"),
                        h("output", { id: "parent-count" }, String(count.value)),
                        h(initial.default)
                      ]);
                    }
                  });
                  createApp(host).mount("#app");
                  record("component-registered");
                  complete("ready:" + document.getElementById("component-version")?.textContent);
                }
              } catch (error) {
                complete("error:" + (error instanceof Error ? error.message : String(error)));
              }
            </script>
          </body>
        </html>
        """;

static int AllocateLoopbackPort()
{
    using var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    return ((IPEndPoint)listener.LocalEndpoint).Port;
}

static async Task<string> WaitForBrowserConditionAsync(
    string repoRoot,
    string browserSessionName,
    string conditionExpression,
    TimeSpan timeout)
{
    ProcessResult? lastFailure = null;
    for (var attempt = 0; attempt < 3; attempt++)
    {
        var result = await RunPlaywrightApiAsync(
            repoRoot,
            browserSessionName,
            "eval",
            "async () => { const deadline = Date.now() + " +
            timeout.TotalMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            "; while (Date.now() < deadline) { const error = document.documentElement.getAttribute('data-jazor-hmr-result'); if (error && error.startsWith('error:')) return error; const value = (" +
            conditionExpression +
            "); if (value) return value; await new Promise(resolve => setTimeout(resolve, 50)); } return 'timeout:' + (document.documentElement.getAttribute('data-jazor-hmr-result') || 'pending'); }");
        if (result.ExitCode == 0)
            return result.StandardOutput;

        lastFailure = result;
        await Task.Delay(TimeSpan.FromMilliseconds(250));
    }

    EnsureProcessSucceeded(lastFailure!, "Playwright could not read the development HMR fixture state.");
    throw new InvalidOperationException("Unreachable.");
}

static Task<ProcessResult> RunPlaywrightApiAsync(
    string repoRoot, string browserSessionName, string command, params string[] commandArguments)
    => HmrBrowser.RunAsync(repoRoot, command, commandArguments);
static async Task<string> ReadBrowserStateAsync(string repoRoot, string browserSessionName)
{
    var result = await RunPlaywrightApiAsync(
        repoRoot,
        browserSessionName,
        "eval",
        "() => ({ href: location.href, result: document.documentElement.getAttribute('data-jazor-hmr-result'), phase: document.documentElement.getAttribute('data-jazor-hmr-phase'), component: document.getElementById('component-version')?.textContent, count: document.getElementById('parent-count')?.textContent, loads: sessionStorage.getItem('jazor-hmr-fixture-loads'), hmr: typeof window.JazorHmr, ready: typeof window.JazorHmr?.ready })");
    return result.StandardOutput + Environment.NewLine + result.StandardError;
}

static async Task VerifyBrowserSourceMapAsync(HttpClient httpClient, Uri baseAddress, string expectedVersion)
{
    var moduleText = await httpClient.GetStringAsync(new Uri(baseAddress, "jazor/component.mjs"));
    if (!moduleText.Contains("//# sourceMappingURL=component.mjs.map", StringComparison.Ordinal))
    {
        throw new InvalidOperationException("Development HMR fixture module did not declare its source map.");
    }

    var sourceMapResponse = await httpClient.GetAsync(new Uri(baseAddress, "jazor/component.mjs.map?version=" + expectedVersion));
    sourceMapResponse.EnsureSuccessStatusCode();
    using var sourceMap = JsonDocument.Parse(await sourceMapResponse.Content.ReadAsStringAsync());
    if (!sourceMap.RootElement.TryGetProperty("sources", out var sources) ||
        !sources.EnumerateArray().Any(static source => source.GetString() == "Pages/HmrFixture.razor") ||
        !sourceMap.RootElement.TryGetProperty("sourcesContent", out var sourceContents) ||
        !sourceContents.EnumerateArray().Any(source =>
            source.ValueKind == JsonValueKind.String &&
            source.GetString()?.Contains("child:" + expectedVersion, StringComparison.Ordinal) == true))
    {
        throw new InvalidOperationException("Development HMR fixture source map did not expose authored Razor v" + expectedVersion + ".");
    }
}

static async Task RestoreVueRuntimeAsync(string repoRoot, string artifactRoot)
{
    var packageJsonPath = Path.Combine(artifactRoot, "package.json");
    await File.WriteAllTextAsync(
        packageJsonPath,
        """
        {
          "name": "@jazor/development-hmr-fixture",
          "private": true,
          "type": "module",
          "dependencies": {
            "vue": "3.5.42"
          }
        }
        """ + Environment.NewLine,
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

    await NodeToolchain.RestoreAsync(artifactRoot, Path.Combine(repoRoot, ".tmp", "nodeapi-store"),
        CancellationToken.None, Console.Out);
}
static void EnsureProcessSucceeded(ProcessResult result, string message)
{
    if (result.ExitCode == 0)
        return;

    throw new InvalidOperationException(
        message + " Exit code: " + result.ExitCode + Environment.NewLine +
        result.StandardOutput + Environment.NewLine + result.StandardError);
}

static string RequireRepoRoot()
{
    var current = new DirectoryInfo(Environment.CurrentDirectory);
    while (current is not null)
    {
        if (File.Exists(Path.Combine(current.FullName, "Jazor.slnx")))
            return current.FullName;
        current = current.Parent;
    }

    throw new InvalidOperationException("Run this script from the Jazor repository root.");
}

static void ConfigureRepositoryEnvironment(string repoRoot)
{
    var tempDirectory = Path.Combine(repoRoot, ".tmp", "development-hmr-temp");
    var nugetPackages = Path.Combine(repoRoot, ".dotnet", ".nuget", "packages") + Path.DirectorySeparatorChar;
    var nugetHttpCache = Path.Combine(repoRoot, ".tmp", "nuget-http-cache");
    var npmCache = Path.Combine(repoRoot, ".tmp", "npm-cache");

    foreach (var directory in new[] { tempDirectory, nugetPackages, nugetHttpCache, npmCache, Path.Combine(repoRoot, ".dotnet") })
    {
        Directory.CreateDirectory(directory);
    }

    Environment.SetEnvironmentVariable("TEMP", tempDirectory);
    Environment.SetEnvironmentVariable("TMP", tempDirectory);
    Environment.SetEnvironmentVariable("DOTNET_CLI_HOME", Path.Combine(repoRoot, ".dotnet"));
    Environment.SetEnvironmentVariable("NUGET_PACKAGES", nugetPackages);
    Environment.SetEnvironmentVariable("NUGET_HTTP_CACHE_PATH", nugetHttpCache);
    Environment.SetEnvironmentVariable("JazorPnpmStoreDirectory", Path.Combine(repoRoot, ".tmp", "nodeapi-store"));
    Environment.SetEnvironmentVariable("NPM_CONFIG_CACHE", npmCache);
    Environment.SetEnvironmentVariable("npm_config_cache", npmCache);
    Environment.SetEnvironmentVariable("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1");
    Environment.SetEnvironmentVariable("MSBUILDDISABLENODEREUSE", "1");
}

internal sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);

static class HmrBrowser
{
    private static NodeEnvironment? _environment;
    private static string? _module;

    public static async Task<ProcessResult> RunAsync(string repoRoot, string command, string[] arguments)
    {
        try
        {
            if (_environment is null)
            {
                var root = Path.Combine(repoRoot, ".tmp", "nodeapi-browser");
                Directory.CreateDirectory(root);
                await File.WriteAllTextAsync(Path.Combine(root, "package.json"),
                    """{"private":true,"type":"module","dependencies":{"playwright-core":"1.65.0"}}""");
                await NodeToolchain.RestoreAsync(root, Path.Combine(repoRoot, ".tmp", "nodeapi-store"),
                    CancellationToken.None, Console.Out);
                _module = Path.Combine(root, "hmr-browser.cjs");
                await File.WriteAllTextAsync(_module, """
                    const { chromium } = require('playwright-core');
                    let browser, page;
                    exports.run = async (requestJson) => {
                        const { command, arguments: args, executablePath } = JSON.parse(requestJson);
                        switch (command) {
                            case 'open':
                                browser = await chromium.launch(executablePath
                                    ? { executablePath, headless: true }
                                    : { channel: 'chrome', headless: true });
                                page = await browser.newPage();
                                await page.goto(args[0]);
                                return 'opened';
                            case 'eval':
                                return JSON.stringify(await page.evaluate('(' + args[0] + ')()'));
                            case 'close':
                                await browser.close();
                                browser = page = undefined;
                                return 'closed';
                            default:
                                throw new Error('Unsupported browser operation: ' + command);
                        }
                    };
                    """);
                _environment = new NodeEnvironment(root);
            }
            var output = await _environment.InvokeAsync(_module!, "run",
                JsonSerializer.Serialize(new { command, arguments, executablePath = Environment.GetEnvironmentVariable("JAZOR_BROWSER_EXE") }));
            return new ProcessResult(0, output, "");
        }
        catch (Exception exception)
        {
            return new ProcessResult(1, "", exception.ToString());
        }
        finally
        {
            if (command == "close")
            {
                _environment?.Dispose();
                _environment = null;
            }
        }
    }
}
