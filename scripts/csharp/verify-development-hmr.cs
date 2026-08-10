#!/usr/bin/env dotnet run
#:sdk Microsoft.NET.Sdk.Web
#:project ../../src/Jazor.AspNetCore.Dev/Jazor.AspNetCore.Dev.csproj

using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Jazor.AspNetCore.Dev;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var repoRoot = RequireRepoRoot();
var workspace = Path.Combine(repoRoot, ".tmp", "development-hmr-browser", Guid.NewGuid().ToString("N"));
var webRoot = Path.Combine(workspace, "wwwroot");
var artifactRoot = Path.Combine(webRoot, "jazor");
var componentPath = Path.Combine(artifactRoot, "component.mjs");
var manifestPath = Path.Combine(artifactRoot, "jazor-manifest.json");
var browserSessionName = "jazor-development-hmr-" + Guid.NewGuid().ToString("N");
var browserSessionOpened = false;
WebApplication? app = null;

try
{
    Directory.CreateDirectory(artifactRoot);
    await WriteFixtureAsync(componentPath, manifestPath, "v1", "content-v1", "template-v1");
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
    builder.Services.AddJazorDevelopmentReload(options =>
    {
        // The mapping itself must be enough to observe a custom artifact root.
        options.WatchRootPaths.Clear();
        options.HmrModuleMappings.Clear();
        options.HmrModuleMappings.Add(new JazorDevelopmentHmrModuleMapping
        {
            ArtifactRootPath = "wwwroot/jazor",
            RequestPath = "/jazor"
        });
        options.FileChangeDebounceInterval = TimeSpan.FromMilliseconds(35);
        options.FileChangePollingInterval = TimeSpan.FromMilliseconds(75);
    });

    app = builder.Build();
    app.UseJazorDevelopmentReload();
    app.UseStaticFiles();
    app.MapGet("/", () => Results.File(indexPath, "text/html; charset=utf-8"));
    app.MapPost("/__jazor-hmr-fixture/update", async () =>
    {
        // The host deliberately falls back to a full reload before the browser advertises
        // module-update capability. The browser waits for JazorHmr.ready before this request.
        await Task.Delay(250);
        await WriteFixtureAsync(componentPath, manifestPath, "v2", "content-v2", "template-v2");
        // Keep the request open until watcher/poller delivery can reach the browser.
        await Task.Delay(750);
        return Results.NoContent();
    });
    await app.StartAsync();

    var browserOpen = await RunPlaywrightCliAsync(repoRoot, browserSessionName, "open", baseAddress.ToString());
    EnsureProcessSucceeded(browserOpen, "Playwright could not open the development HMR fixture.");
    browserSessionOpened = true;

    var ready = await WaitForBrowserResultAsync(repoRoot, browserSessionName, TimeSpan.FromSeconds(10));
    if (!ready.Contains("ready:v1", StringComparison.Ordinal))
    {
        var browserState = await ReadBrowserStateAsync(repoRoot, browserSessionName);
        throw new InvalidOperationException(
            "Development HMR fixture did not register its v1 handler: " + ready + Environment.NewLine + browserState);
    }

    using var httpClient = new HttpClient();
    using var updateResponse = await httpClient.PostAsync(
        new Uri(baseAddress, "__jazor-hmr-fixture/update"),
        content: null);
    updateResponse.EnsureSuccessStatusCode();

    var update = await WaitForBrowserResultAsync(repoRoot, browserSessionName, TimeSpan.FromSeconds(10));
    if (!update.Contains("module-update:v2", StringComparison.Ordinal))
    {
        throw new InvalidOperationException(
            "Development HMR browser verification did not apply the v2 module update." + Environment.NewLine +
            update);
    }

    Console.WriteLine("Development HMR browser verification passed.");
    Console.WriteLine("  Browser: Playwright CLI");
    Console.WriteLine("  Flow: custom mapping -> manifest template diff -> WebSocket module-update -> dynamic import -> registered handler");
}
finally
{
    if (browserSessionOpened)
    {
        try
        {
            _ = await RunPlaywrightCliAsync(repoRoot, browserSessionName, "close");
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

static async Task WriteFixtureAsync(
    string componentPath,
    string manifestPath,
    string version,
    string contentHash,
    string templateHash)
{
    await File.WriteAllTextAsync(
        componentPath,
        "export const version = \"" + version + "\";\n",
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

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
    writer.WriteStartArray("modules");
    writer.WriteStartObject();
    writer.WriteString("path", "component.mjs");
    writer.WriteString("contentHash", contentHash);
    writer.WriteStartObject("hmr");
    writer.WriteString("componentId", "Fixture.Component");
    writer.WriteString("moduleId", "fixture:component");
    writer.WriteString("descriptorHash", "descriptor-v1");
    writer.WriteString("templateHash", templateHash);
    writer.WriteString("logicHash", "logic-v1");
    writer.WriteString("boundaryKind", "template-only");
    writer.WriteEndObject();
    writer.WriteEndObject();
    writer.WriteEndArray();
    writer.WriteEndObject();
    await writer.FlushAsync();
}

static string CreateBrowserHarness()
    => """
        <!doctype html>
        <html lang="en">
          <head>
            <meta charset="utf-8">
            <title>Jazor Development HMR Fixture</title>
          </head>
          <body>
            <output id="result">pending</output>
            <script type="module">
              const output = document.getElementById("result");
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
                  output.textContent = initial.version;
                  record("handler-registered");
                  hmr.accept("fixture:component", ({ module }) => {
                    record("handler:" + module.version);
                    output.textContent = module.version;
                    complete("module-update:" + module.version);
                  });
                  complete("ready:" + initial.version);
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

static async Task<string> WaitForBrowserResultAsync(
    string repoRoot,
    string browserSessionName,
    TimeSpan timeout)
{
    ProcessResult? lastFailure = null;
    for (var attempt = 0; attempt < 3; attempt++)
    {
        var result = await RunPlaywrightCliAsync(
            repoRoot,
            browserSessionName,
            "eval",
            "async () => { const deadline = Date.now() + " +
            timeout.TotalMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            "; while (Date.now() < deadline) { const value = document.documentElement.getAttribute('data-jazor-hmr-result'); if (value) return value; await new Promise(resolve => setTimeout(resolve, 50)); } return 'timeout:' + (document.documentElement.getAttribute('data-jazor-hmr-result') || 'pending'); }");
        if (result.ExitCode == 0)
            return result.StandardOutput;

        lastFailure = result;
        await Task.Delay(TimeSpan.FromMilliseconds(250));
    }

    EnsureProcessSucceeded(lastFailure!, "Playwright could not read the development HMR fixture state.");
    throw new InvalidOperationException("Unreachable.");
}

static Task<ProcessResult> RunPlaywrightCliAsync(
    string repoRoot,
    string browserSessionName,
    string command,
    params string[] commandArguments)
{
    var arguments = new List<string>
    {
        "--yes",
        "--package",
        "@playwright/cli",
        "playwright-cli",
        "-s=" + browserSessionName,
        command
    };
    arguments.AddRange(commandArguments);
    var npxExecutable = ResolveNpxExecutable();
    return RunProcessAsync(npxExecutable, repoRoot, arguments, TimeSpan.FromSeconds(90));
}

static async Task<string> ReadBrowserStateAsync(string repoRoot, string browserSessionName)
{
    var result = await RunPlaywrightCliAsync(
        repoRoot,
        browserSessionName,
        "eval",
        "() => ({ href: location.href, result: document.documentElement.getAttribute('data-jazor-hmr-result'), phase: document.documentElement.getAttribute('data-jazor-hmr-phase'), hmr: typeof window.JazorHmr, ready: typeof window.JazorHmr?.ready })");
    return result.StandardOutput + Environment.NewLine + result.StandardError;
}

static string ResolveNpxExecutable()
{
    if (!OperatingSystem.IsWindows())
        return "npx";

    foreach (var directory in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
                 .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
    {
        var candidate = Path.Combine(directory.Trim().Trim('"'), "npx.cmd");
        if (File.Exists(candidate))
            return candidate;
    }

    throw new InvalidOperationException("npx.cmd was not found on PATH. Install Node.js/npm before running this verification.");
}

static void EnsureProcessSucceeded(ProcessResult result, string message)
{
    if (result.ExitCode == 0)
        return;

    throw new InvalidOperationException(
        message + " Exit code: " + result.ExitCode + Environment.NewLine +
        result.StandardOutput + Environment.NewLine + result.StandardError);
}

static async Task<ProcessResult> RunProcessAsync(
    string fileName,
    string workingDirectory,
    IReadOnlyList<string> arguments,
    TimeSpan timeout)
{
    var startInfo = new ProcessStartInfo(fileName)
    {
        WorkingDirectory = workingDirectory,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    };
    foreach (var argument in arguments)
        startInfo.ArgumentList.Add(argument);

    using var process = new Process { StartInfo = startInfo };
    process.Start();
    var standardOutput = process.StandardOutput.ReadToEndAsync();
    var standardError = process.StandardError.ReadToEndAsync();
    using var timeoutSource = new CancellationTokenSource(timeout);
    try
    {
        await process.WaitForExitAsync(timeoutSource.Token);
    }
    catch (OperationCanceledException)
    {
        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException)
        {
        }

        await process.WaitForExitAsync();
        return new ProcessResult(-1, await standardOutput, await standardError);
    }

    return new ProcessResult(process.ExitCode, await standardOutput, await standardError);
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

internal sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);
