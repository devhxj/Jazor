#!/usr/bin/env dotnet run
#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:sdk Microsoft.NET.Sdk.Web
#:project ../../src/Jazor.AspNetCore/Jazor.AspNetCore.csproj
#:project ../../src/Jazor.AspNetCore.Dev/Jazor.AspNetCore.Dev.csproj
#:project ../../src/Jazor.Common/Jazor.Common.csproj

using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Jazor.AspNetCore;
using Jazor.AspNetCore.Dev;
using Jazor.Common.SourceMaps;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var repoRoot = RequireRepoRoot();
var temporaryRoot = Path.Combine(repoRoot, "test", "jolt", "hmr");
ConfigureRepositoryEnvironment(repoRoot, temporaryRoot);
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
if (args is ["--jolt-debug", var debugJolt, var debugBrowser])
{
    await VerifyJoltDebugAsync(repoRoot, temporaryRoot, Path.GetFullPath(debugJolt, repoRoot), Path.GetFullPath(debugBrowser, repoRoot));
    return;
}
throw new ArgumentException("Usage: verify-development-hmr.cs --jolt-force-stop <feed> <version> | --jolt-proxy <jolt> <chromium> <feed> <version> | --jolt-debug <jolt> <chromium>");

static async Task VerifyJoltForceStopAsync(string repo, string temporaryRoot, string feed, string version)
{
    var root = Directory.CreateDirectory(Path.Combine(temporaryRoot, "jazor-jolt-force-stop-" + Guid.NewGuid().ToString("N"))).FullName;
    var frontend = root;
    var toolDirectory = Path.Combine(root, "tool");
    string tool = "";
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
        CopyAspNetSample(repo, root, version);
        var program = Path.Combine(root, "Program.cs");
        var original = await File.ReadAllTextAsync(program, token);
        var isolated = original.Replace("http://127.0.0.1:15658", $"http://127.0.0.1:{vitePort}", StringComparison.Ordinal);
        if (isolated == original)
            throw new InvalidOperationException("The sample Vite origin could not be isolated.");
        await File.WriteAllTextAsync(program, isolated.Replace("options.PathBase", """
            options.Vite.LaunchServer = builder.Configuration["JazorForceStop:Attach"] != "true";
                options.PathBase
            """, StringComparison.Ordinal), token);
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
        var nugetConfig = Path.Combine(root, "NuGet.Config");
        new XElement("configuration", new XElement("packageSources", new XElement("clear"),
            new XElement("add", new XAttribute("key", "candidate"), new XAttribute("value", feed)),
            new XElement("add", new XAttribute("key", "nuget.org"), new XAttribute("value", "https://api.nuget.org/v3/index.json"))))
            .Save(nugetConfig);
        using (var restore = Start("dotnet", "tool", "install", "Jazor.Jolt", "--version", version, "--tool-path", toolDirectory, "--configfile", nugetConfig))
            await RequireSuccess(restore);
        tool = Directory.EnumerateFiles(toolDirectory, OperatingSystem.IsWindows() ? "Jolt.Cli.exe" : "Jolt.Cli",
            SearchOption.AllDirectories).Single();
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
        using var external = attached ? Start(tool, "dev", frontend, "--port", vitePort.ToString()) : null;
        Process? host = null;
        Process? vite = null;
        try
        {
            if (external is not null)
                await WaitReadyAsync(vitePort, external);
            host = Start("dotnet", Path.Combine(root, "out", "Jolt.AspNetCore.dll"), "--environment", "Development",
                "--urls", $"http://127.0.0.1:{hostPort}", "--JazorForceStop:Attach", attached ? "true" : "false");
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
            string[] arguments = ["dev", frontend, "--port", vitePort.ToString(),
                .. optIn ? new[] { "--stop-on-stdin-close" } : []];
            using var cli = StartProcess(tool, arguments, redirectInput: true);
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
            using var response = await http.GetAsync($"http://127.0.0.1:{port}/portal/frontend/Counter.mjs", token);
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

static string CopyAspNetSample(string repo, string destination, string version)
{
    var sample = Path.Combine(repo, "samples", "Jolt.AspNetCore");
    Directory.CreateDirectory(destination);
    foreach (var name in new[] { "Program.cs", "Counter.jazor", "index.html", "main.js", "package.json", "pnpm-lock.yaml", "vite.config.js" })
        File.Copy(Path.Combine(sample, name), Path.Combine(destination, name));
    var project = XDocument.Load(Path.Combine(sample, "Jolt.AspNetCore.csproj"));

    project.Descendants("ProjectReference").Single(reference =>
        Path.GetFileNameWithoutExtension(reference.Attribute("Include")!.Value) == "Jolt.Cli").Remove();
    foreach (var element in project.Descendants().Where(element => element.Name.LocalName is "ProjectReference" or "Import"))
    {
        var attribute = element.Attribute(element.Name.LocalName == "Import" ? "Project" : "Include")!;
        attribute.Value = Path.GetFullPath(attribute.Value, sample);
    }
    var joltImport = project.Descendants("Import").Single(element => element.Attribute("Project")!.Value.EndsWith("Jazor.Jolt.targets", StringComparison.Ordinal));
    joltImport.AddBeforeSelf(new XElement("PropertyGroup", new XElement("_JoltToolVersion", version)));
    var path = Path.Combine(destination, "Jolt.AspNetCore.csproj");
    project.Save(path);
    return path;
}

static async Task VerifyJoltProxyAsync(string repo, string temporaryRoot, string jolt, string chromium, string feed, string version)
{
    var root = Directory.CreateDirectory(Path.Combine(temporaryRoot, "jolt-proxy-" + Guid.NewGuid().ToString("N"))).FullName;
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
        var nugetConfig = Path.Combine(Directory.CreateDirectory(toolRoot).FullName, "NuGet.Config");
        new XElement("configuration", new XElement("packageSources",
            new XElement("clear"), new XElement("add", new XAttribute("key", "candidate"), new XAttribute("value", feed))))
            .Save(nugetConfig);
        var packageCache = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
        var cliHome = Environment.GetEnvironmentVariable("DOTNET_CLI_HOME");
        Environment.SetEnvironmentVariable("NUGET_PACKAGES", Path.Combine(toolRoot, "packages"));
        Environment.SetEnvironmentVariable("DOTNET_CLI_HOME", toolRoot);
        try
        {
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
                catch (InvalidOperationException exception) when (
                    exception.Message.Contains("Jolt dev exited with code", StringComparison.Ordinal) &&
                    exception.Message.Contains($"Port {vitePort} is already in use", StringComparison.Ordinal))
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
        var csproj = CopyAspNetSample(repo, project, version);
        // dotnet publish restores and invokes this fixture's pinned tool on its own.
        new XElement("configuration", new XElement("packageSources",
            new XElement("add", new XAttribute("key", "candidate"), new XAttribute("value", feed))))
            .Save(Path.Combine(project, "NuGet.Config"));
        var frontend = project;
        var buildLog = Path.Combine(project, "vite-builds.log");
        var config = Path.Combine(frontend, "vite.config.js");
        await File.WriteAllTextAsync(config, """
            import { appendFileSync } from 'node:fs';
            import { defineConfig } from 'vite';
            export default defineConfig({
                base: '/portal/frontend/',
                plugins: [{ name: 'fixture-build-count', buildStart() {
                    appendFileSync(new URL('./vite-builds.log', import.meta.url), 'build\n');
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
        var publish = Path.Combine(root, "published-sample");
        await RunAsync(project, "dotnet", "publish", csproj, "--nologo", "-o", publish);
        if (!(await File.ReadAllLinesAsync(buildLog, token)).SequenceEqual(["build"]))
            throw new InvalidOperationException("Expected dotnet publish to run exactly one Vite build.");
        var dist = Path.Combine(publish, "wwwroot");
        var expected = Directory.EnumerateFiles(Path.Combine(frontend, ".jazor", "dist"), "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(Path.Combine(frontend, ".jazor", "dist"), path)).ToHashSet(StringComparer.Ordinal);
        var actual = Directory.EnumerateFiles(Path.Combine(publish, "wwwroot"), "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(Path.Combine(publish, "wwwroot"), path));
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
        var application = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName($"JoltProxyFixture_{Guid.NewGuid():N}"), AssemblyBuilderAccess.Run);
        application.SetCustomAttribute(new CustomAttributeBuilder(
            typeof(AssemblyMetadataAttribute).GetConstructor([typeof(string), typeof(string)])!,
            ["JoltTool", "Jazor.Jolt@" + version]));
        builder.Environment.ApplicationName = application.GetName().Name!;
        builder.AddJazorSPA(options =>
        {
            options.ProjectRootPath = root;
            options.PathBase = "/prefix";
            options.RequestPath = "/jazor";
            options.DevelopmentEntryRelativePath = "index.html";
            options.Vite.ServerOrigin = vite;
            options.Vite.LaunchServer = owned;
        });
        var proxy = builder.Build();
        proxy.UseJazorPathBase();
        proxy.UseJazorSPA();
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

static async Task VerifyJoltDebugAsync(string repo, string temporaryRoot, string jolt, string chromium)
{
    var root = Directory.CreateDirectory(Path.Combine(temporaryRoot, "debug-" + Guid.NewGuid().ToString("N"))).FullName;
    var sample = Path.Combine(repo, "samples", "Jolt.AspNetCore");
    foreach (var file in new[] { "package.json", "pnpm-lock.yaml", "index.html", "vite.config.js", "main.js", "Counter.jazor" })
        File.Copy(Path.Combine(sample, file), Path.Combine(root, file));
    await File.AppendAllTextAsync(Path.Combine(root, "main.js"), """

        import.meta.hot?.on('vite:afterUpdate', () => { window.joltUpdateCount = (window.joltUpdateCount ?? 0) + 1; });
        """);
    var module = Path.Combine(root, "IncrementStep.cs");
    const string original = """
        using ECMAScript;

        [ECMAScriptModule("increment-step.js")]
        public static partial class IncrementStep
        {
            public static int Value() => Local() + ReferencedStep.Value();
        }
        """;
    await File.WriteAllTextAsync(module, original);
    var helper = Path.Combine(Directory.CreateDirectory(Path.Combine(root, "Logic")).FullName, "IncrementStep.Local.cs");
    const string helperSource = "public static partial class IncrementStep { private static int Local() => 1; }";
    await File.WriteAllTextAsync(helper, helperSource);
    var component = Path.Combine(root, "Counter.jazor");
    var source = (await File.ReadAllTextAsync(component)).Replace("Count++;", "Count += IncrementStep.Value();", StringComparison.Ordinal);
    await File.WriteAllTextAsync(component, source);
    var sourceLine = Array.FindIndex(source.ReplaceLineEndings("\n").Split('\n'), line => line.Contains("Count += ", StringComparison.Ordinal));
    var host = new Uri($"http://127.0.0.1:{AllocateLoopbackPort()}/");
    var vitePort = AllocateLoopbackPort();
    var sourceRoot = Path.Combine(repo, "src").Replace('\\', '/') + "/";
    var referencedDirectory = Directory.CreateDirectory(root + "-reference").FullName;
    var referencedProject = Path.Combine(referencedDirectory, "ReferencedStep.csproj");
    var referencedSourcePath = Path.Combine(referencedDirectory, "ReferencedStep.cs");
    const string referencedSource = """
        using ECMAScript;
        [ECMAScriptModule("referenced-step.js")]
        public static class ReferencedStep { public static int Value() => 0; }
        """;
    await File.WriteAllTextAsync(referencedSourcePath, referencedSource);
    var analyzer = $$"""
          <ItemGroup>
            <ProjectReference Include="{{sourceRoot}}ECMAScript.Vue/ECMAScript.Vue.csproj" />
            <ProjectReference Include="{{sourceRoot}}Jazor.Analyzer/Jazor.Analyzer.csproj" ReferenceOutputAssembly="false" />
          </ItemGroup>
          <Target Name="ResolveFixtureAnalyzer" BeforeTargets="CoreCompile">
            <MSBuild Projects="{{sourceRoot}}Jazor.Analyzer/Jazor.Analyzer.csproj" Targets="GetTargetPath">
              <Output TaskParameter="TargetOutputs" ItemName="_FixtureAnalyzer" />
            </MSBuild>
            <PropertyGroup>
              <_FixtureAnalyzerDirectory>@(_FixtureAnalyzer->'%(RootDir)%(Directory)')</_FixtureAnalyzerDirectory>
            </PropertyGroup>
            <ItemGroup>
              <Analyzer Include="$(_FixtureAnalyzerDirectory)Jazor.Analyzer.dll;$(_FixtureAnalyzerDirectory)Jazor.Compiler.dll;$(_FixtureAnalyzerDirectory)Jazor.Common.dll;$(_FixtureAnalyzerDirectory)ECMAScript.Contract.dll;$(_FixtureAnalyzerDirectory)Acornima.dll;$(_FixtureAnalyzerDirectory)Acornima.Extras.dll;$(_FixtureAnalyzerDirectory)ICSharpCode.Decompiler.dll" />
            </ItemGroup>
          </Target>
        """;
    await File.WriteAllTextAsync(referencedProject, $$"""
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup><TargetFramework>net11.0</TargetFramework></PropertyGroup>
        {{analyzer}}
        </Project>
        """);
    var project = Path.Combine(root, "DebugFixture.csproj");
    await File.WriteAllTextAsync(project, $$"""
        <Project Sdk="Microsoft.NET.Sdk.Web">
          <Import Project="{{sourceRoot}}Jazor/build/Jazor.props" />
          <PropertyGroup>
            <TargetFramework>net11.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <_JazorVueProject>true</_JazorVueProject>
            <DefaultItemExcludes>$(DefaultItemExcludes);node_modules/**;.jazor/**</DefaultItemExcludes>
          </PropertyGroup>
          <ItemGroup>
            <AssemblyMetadata Include="JoltHost" Value="{{System.Security.SecurityElement.Escape(jolt)}}" />
            <ProjectReference Include="{{referencedProject}}" />
            <ProjectReference Include="{{sourceRoot}}Jazor.AspNetCore.Dev/Jazor.AspNetCore.Dev.csproj" />
            <ProjectReference Include="{{sourceRoot}}Jazor.Emit/Jazor.Emit.csproj" ReferenceOutputAssembly="false" />
          </ItemGroup>
        {{analyzer}}
          <Import Project="{{sourceRoot}}Jazor/build/Jazor.targets" />
        </Project>
        """);
    var program = (await File.ReadAllTextAsync(Path.Combine(sample, "Program.cs"))).Replace(":15658", $":{vitePort}", StringComparison.Ordinal);
    await File.WriteAllTextAsync(Path.Combine(root, "Program.cs"), program);
    using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(5));
    var token = timeout.Token;
    Console.WriteLine($"Debug fixture: {root}");
    using (var build = StartProcess("dotnet", "build", project, "--artifacts-path", Path.Combine(root, ".jazor", "initial-build"),
        "-p:_JoltRefreshModules=true"))
    {
        await build.WaitForExitAsync(token);
        if (build.ExitCode != 0)
            throw new InvalidOperationException($"Debug fixture build failed with {build.ExitCode}.");
    }
    using var app = StartProcess("dotnet", Path.Combine(root, ".jazor", "initial-build", "bin", "DebugFixture", "debug", "DebugFixture.dll"),
        "--environment", "Development", "--urls", host.AbsoluteUri);
    using var health = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
    try
    {
    await WaitAsync(async () =>
    {
        if (app.HasExited)
            throw new InvalidOperationException($"Debug host exited with {app.ExitCode}.");
        try
        {
            using var response = await health.GetAsync(new Uri(host, "portal/frontend/"), token);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException) { return false; }
        catch (TaskCanceledException) when (!token.IsCancellationRequested) { return false; }
    });
    var profile = Path.Combine(temporaryRoot, Path.GetFileName(root) + "-chromium");
    var launch = new ProcessStartInfo(chromium) { UseShellExecute = false };
    foreach (var argument in new[] { "--headless", "--disable-gpu", "--remote-debugging-port=0", $"--user-data-dir={profile}", "about:blank" })
        launch.ArgumentList.Add(argument);
    using var browser = Process.Start(launch)!;
    try
    {
        using var http = new HttpClient();
        using var socket = new ClientWebSocket();
        await WaitAsync(async () =>
        {
            if (browser.HasExited)
                throw new InvalidOperationException($"Chromium exited with code {browser.ExitCode}.");
            try
            {
                var portFile = Path.Combine(profile, "DevToolsActivePort");
                if (!File.Exists(portFile) || !int.TryParse((await File.ReadAllLinesAsync(portFile, token)).FirstOrDefault(), out var port))
                    return false;
                using var targets = JsonDocument.Parse(await http.GetStringAsync($"http://127.0.0.1:{port}/json/list", token));
                var page = targets.RootElement.EnumerateArray().Single(target => target.GetProperty("type").GetString() == "page");
                await socket.ConnectAsync(new Uri(page.GetProperty("webSocketDebuggerUrl").GetString()!), token);
                return true;
            }
            catch (IOException) { return false; }
        });

        var parsedScripts = new List<JsonElement>();
        JsonElement stopped = default;
        var sameOriginSockets = new HashSet<string>(StringComparer.Ordinal);
        var sameOriginHmrConnected = false;
        var request = 0;
        await SendAsync("Page.enable", new { });
        await SendAsync("Network.enable", new { });
        await SendAsync("Debugger.enable", new { });
        await SendAsync("Page.navigate", new { url = new Uri(host, "portal/frontend/").AbsoluteUri });
        await WaitAsync(async () => await EvaluateAsync("document.querySelector('#count')?.textContent === '0'") && sameOriginHmrConnected);
        var script = parsedScripts.Last(parameters =>
            parameters.TryGetProperty("sourceMapURL", out _) && parameters.GetProperty("url").GetString()!.Contains("/Counter.mjs", StringComparison.Ordinal));
        var scriptUri = new Uri(script.GetProperty("url").GetString()!);
        var mapUrl = script.GetProperty("sourceMapURL").GetString()!;
        var mapJson = mapUrl.StartsWith("data:", StringComparison.Ordinal)
            ? Encoding.UTF8.GetString(Convert.FromBase64String(mapUrl[(mapUrl.IndexOf(',') + 1)..]))
            : await http.GetStringAsync(new Uri(scriptUri, mapUrl), token);
        var map = new SourceMapReader().Read(mapJson);
        var scriptId = script.GetProperty("scriptId").GetString();
        // CDP speaks generated coordinates. Resolve and check both directions using the map Chromium reported.
        var mapped = map.Segments.First(segment => segment.SourceLine == sourceLine && IsComponent(segment));
        var binding = await SendAsync("Debugger.setBreakpoint", new
        {
            location = new { scriptId, lineNumber = mapped.GeneratedLine, columnNumber = mapped.GeneratedColumn }
        });
        var breakpointId = binding.GetProperty("breakpointId").GetString();
        var actual = binding.GetProperty("actualLocation");
        if (actual.GetProperty("scriptId").GetString() != scriptId || !MapsToSourceLine(actual))
            throw new InvalidOperationException($"Chromium bound outside Counter.jazor:{sourceLine + 1}: {actual}");
        await SendAsync("Runtime.evaluate", new { expression = "setTimeout(() => document.querySelector('#increment').click(), 0)" });
        while (stopped.ValueKind == JsonValueKind.Undefined)
        {
            var message = await ReceiveAsync();
            if (message.TryGetProperty("method", out var method) && method.GetString() == "Debugger.paused")
                stopped = message.GetProperty("params");
        }
        var frame = stopped.GetProperty("callFrames")[0].GetProperty("location");
        if (!stopped.GetProperty("hitBreakpoints").EnumerateArray().Any(id => id.GetString() == breakpointId) ||
            frame.GetProperty("scriptId").GetString() != scriptId || !MapsToSourceLine(frame))
            throw new InvalidOperationException($"Unexpected breakpoint stop: {frame}");
        Console.WriteLine($"Chromium bound and hit Counter.jazor:{sourceLine + 1} through the ASP.NET host.");
        await SendAsync("Debugger.removeBreakpoint", new { breakpointId });
        await SendAsync("Debugger.resume", new { });
        await WaitAsync(() => EvaluateAsync("document.querySelector('#count')?.textContent === '1'"));

        await EvaluateAsync("(window.debugHmrPage = true) && ((window.joltUpdateCount = 0) === 0)");
        File.Delete(helper);
        var replacementDirectory = Directory.CreateDirectory(Path.Combine(root, "ReplacementLogic")).FullName;
        await File.WriteAllTextAsync(Path.Combine(replacementDirectory, "IncrementStep.Local.cs"),
            helperSource.Replace("=> 1;", "=> 2;", StringComparison.Ordinal), token);
        // A plain module update must reach the running application before its next click.
        await WaitAsync(() => EvaluateAsync("""
            window.joltUpdateCount > 0 && performance.getEntriesByType('resource').some(entry =>
                entry.name.includes('/increment-step.js?') && new URL(entry.name).searchParams.has('t'))
            """));
        await EvaluateAsync("document.querySelector('#increment').click()");
        await WaitAsync(() => EvaluateAsync("document.querySelector('#count')?.textContent === '3' && window.debugHmrPage === true"));
        await EvaluateAsync("window.joltUpdateCount = 0");
        File.Delete(referencedSourcePath);
        var referenceReplacement = Directory.CreateDirectory(Path.Combine(referencedDirectory, "Nested")).FullName;
        await File.WriteAllTextAsync(Path.Combine(referenceReplacement, "ReferencedStep.cs"),
            referencedSource.Replace("=> 0;", "=> 4;", StringComparison.Ordinal), token);
        await WaitAsync(() => EvaluateAsync("""
            window.joltUpdateCount > 0 && performance.getEntriesByType('resource').some(entry =>
                entry.name.includes('/referenced-step.js?') && new URL(entry.name).searchParams.has('t'))
            """));
        await EvaluateAsync("document.querySelector('#increment').click()");
        await WaitAsync(() => EvaluateAsync("document.querySelector('#count')?.textContent === '9' && window.debugHmrPage === true"));
        await File.WriteAllTextAsync(component, source.Replace("Jolt counter", "Updated Jolt counter", StringComparison.Ordinal), token);
        await WaitAsync(() => EvaluateAsync("document.querySelector('h1')?.textContent === 'Updated Jolt counter' && document.querySelector('#count')?.textContent === '9' && window.debugHmrPage === true"));
        Console.WriteLine("Host helper, referenced C# module and .jazor saves reached Chromium over same-origin HMR; count and page identity were preserved.");

        bool IsComponent(SourceMapSegment segment) =>
            map.Sources[segment.SourceIndex].Path.Replace('\\', '/').Split('/')[^1] == "Counter.jazor";

        bool MapsToSourceLine(JsonElement location)
        {
            var line = location.GetProperty("lineNumber").GetInt32();
            var column = location.GetProperty("columnNumber").GetInt32();
            return map.Segments.LastOrDefault(segment => segment.GeneratedLine == line && segment.GeneratedColumn <= column) is { } segment
                && segment.SourceLine == sourceLine && IsComponent(segment);
        }

        async Task<bool> EvaluateAsync(string expression)
        {
            var response = await SendAsync("Runtime.evaluate", new { expression, returnByValue = true });
            if (response.TryGetProperty("exceptionDetails", out var error))
                throw new InvalidOperationException(error.ToString());
            return response.GetProperty("result").TryGetProperty("value", out var value) && value.ValueKind == JsonValueKind.True;
        }

        async Task<JsonElement> SendAsync(string method, object parameters)
        {
            var id = ++request;
            await socket.SendAsync(JsonSerializer.SerializeToUtf8Bytes(new { id, method, @params = parameters }),
                WebSocketMessageType.Text, true, token);
            while (true)
            {
                var message = await ReceiveAsync();
                if (message.TryGetProperty("id", out var responseId) && responseId.GetInt32() == id)
                {
                    if (message.TryGetProperty("error", out var error))
                        throw new InvalidOperationException($"{method}: {error}");
                    return message.GetProperty("result");
                }
                if (message.TryGetProperty("method", out var eventMethod))
                {
                    if (eventMethod.GetString() == "Debugger.scriptParsed")
                        parsedScripts.Add(message.GetProperty("params"));
                    if (eventMethod.GetString() == "Debugger.paused")
                        stopped = message.GetProperty("params");
                    if (eventMethod.GetString() == "Network.webSocketCreated")
                    {
                        var details = message.GetProperty("params");
                        var endpoint = new Uri(details.GetProperty("url").GetString()!);
                        if (endpoint.Authority == host.Authority && endpoint.AbsolutePath == "/portal/frontend/")
                            sameOriginSockets.Add(details.GetProperty("requestId").GetString()!);
                    }
                    if (eventMethod.GetString() == "Network.webSocketHandshakeResponseReceived")
                    {
                        var details = message.GetProperty("params");
                        sameOriginHmrConnected |= sameOriginSockets.Contains(details.GetProperty("requestId").GetString()!)
                            && details.GetProperty("response").GetProperty("status").GetInt32() == 101;
                    }
                }
            }
        }

        async Task<JsonElement> ReceiveAsync()
        {
            using var message = new MemoryStream();
            var buffer = new byte[16 * 1024];
            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                if (result.MessageType == WebSocketMessageType.Close)
                    throw new InvalidOperationException("Chromium closed the debug connection.");
                message.Write(buffer, 0, result.Count);
            } while (!result.EndOfMessage);
            return JsonSerializer.Deserialize<JsonElement>(message.ToArray());
        }
    }
    finally
    {
        if (!browser.HasExited)
            browser.Kill(entireProcessTree: true);
        await browser.WaitForExitAsync();
    }
    }
    finally
    {
        if (!app.HasExited)
            app.Kill(entireProcessTree: true);
        await app.WaitForExitAsync();
    }

    Process StartProcess(string file, params string[] arguments)
    {
        var processStart = new ProcessStartInfo(file) { WorkingDirectory = root, UseShellExecute = false };
        foreach (var argument in arguments)
            processStart.ArgumentList.Add(argument);
        return Process.Start(processStart)!;
    }
    async Task WaitAsync(Func<Task<bool>> condition)
    {
        while (!await condition())
            await Task.Delay(100, token);
    }
}
static int AllocateLoopbackPort()
{
    using var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    return ((IPEndPoint)listener.LocalEndpoint).Port;
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

static void ConfigureRepositoryEnvironment(string repoRoot, string tempDirectory)
{
    var nugetPackages = Path.Combine(repoRoot, ".dotnet", ".nuget", "packages") + Path.DirectorySeparatorChar;
    var nugetHttpCache = Path.Combine(tempDirectory, "nuget-http-cache");
    var npmCache = Path.Combine(tempDirectory, "npm-cache");

    foreach (var directory in new[] { tempDirectory, nugetPackages, nugetHttpCache, npmCache, Path.Combine(repoRoot, ".dotnet") })
    {
        Directory.CreateDirectory(directory);
    }

    Environment.SetEnvironmentVariable("TEMP", tempDirectory);
    Environment.SetEnvironmentVariable("TMP", tempDirectory);
    Environment.SetEnvironmentVariable("TMPDIR", tempDirectory);
    Environment.SetEnvironmentVariable("DOTNET_CLI_HOME", Path.Combine(repoRoot, ".dotnet"));
    Environment.SetEnvironmentVariable("NUGET_PACKAGES", nugetPackages);
    Environment.SetEnvironmentVariable("NUGET_HTTP_CACHE_PATH", nugetHttpCache);
    Environment.SetEnvironmentVariable("JazorPnpmStoreDirectory", Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory")
        ?? Path.Combine(repoRoot, "test", "jolt", "pnpm-store"));
    Environment.SetEnvironmentVariable("NPM_CONFIG_CACHE", npmCache);
    Environment.SetEnvironmentVariable("npm_config_cache", npmCache);
    Environment.SetEnvironmentVariable("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1");
    Environment.SetEnvironmentVariable("MSBUILDDISABLENODEREUSE", "1");
}
