#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

// P3 bindings browser smoke: builds the showcase host, serves the generated Jazor artifacts, then
// drives a real Chromium/Chrome instance over CDP to assert that every P3 binding renders and that
// the vue-i18n locale switch is user-observable.
// P3 绑定浏览器验收：构建 showcase 宿主并托管生成的 Jazor 产物，再用真实 Chrome 经 CDP 断言
// 每个 P3 绑定都能渲染，以及 vue-i18n locale 切换可被用户观察。
//
// Usage:
//   dotnet run --file samples/ECMAScript.BindingsShowcase/verify-smoke.cs
//   dotnet run --file samples/ECMAScript.BindingsShowcase/verify-smoke.cs -- --skip-build
var options = SmokeOptions.Parse(args);
var repoRoot = FindRepositoryRoot();
var sampleRoot = Path.Combine(repoRoot, "samples", "ECMAScript.BindingsShowcase");
var projectPath = Path.Combine(sampleRoot, "BindingsShowcase.Host", "BindingsShowcase.Host.csproj");
var hostProjectRoot = Path.Combine(sampleRoot, "BindingsShowcase.Host");
var outputRoot = Path.Combine(hostProjectRoot, "bin", "Debug", "net11.0");

var browserPath = ResolveBrowserExecutable()
    ?? throw new FileNotFoundException(
        "Google Chrome or Chromium is required for the bindings showcase browser smoke. " +
        "Set RAZORVUE_BROWSER_EXE to the browser executable path.");

if (!options.SkipBuild)
{
    await RunAsync("dotnet", ["build", projectPath, "-c", "Debug", "-v", "minimal"], repoRoot);
}

var artifactRoot = Path.Combine(sampleRoot, "BindingsShowcase.Host", "jazor");
if (!Directory.Exists(artifactRoot))
    throw new DirectoryNotFoundException($"Generated Jazor artifacts were not found: {artifactRoot}");
var showcaseModulePath = Path.Combine(artifactRoot, "components", "showcase-landing.js");
var originalShowcaseModule = await File.ReadAllTextAsync(showcaseModulePath);

var port = ReservePort();
var vitePort = ReservePort();
var pageUrl = $"http://127.0.0.1:{port}/";
var profileRoot = Path.Combine(repoRoot, ".tmp", "bindings-showcase-smoke", Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(profileRoot);

using var vite = StartVite(artifactRoot, repoRoot, vitePort);
using var host = StartHost(outputRoot, artifactRoot, port, vitePort);
try
{
    await WaitForViteAsync($"http://127.0.0.1:{vitePort}/jazor/entry.js");
    await WaitForHostAsync(pageUrl);
    using var browser = await BrowserSession.StartAsync(browserPath, profileRoot, pageUrl);

    // Every P3 binding must produce its rendered marker. DateFns and VueI18n assert a concrete
    // value; the rest assert the binding produced a live value rather than an empty slot.
    // 每个 P3 绑定都必须渲染出标记；DateFns/VueI18n 断言具体值，其余断言绑定产出了真实值。
    var assertions = new (string Expression, string Expect, string Description)[]
    {
        ("document.querySelector('[data-showcase=\"landing\"]') !== null", "true", "landing page root rendered"),
        ("document.querySelector('[data-datefns-formatted]').getAttribute('data-datefns-formatted')", "2026-09-17", "date-fns format output"),
        ("document.querySelector('[data-i18n-locale]').getAttribute('data-i18n-locale')", "en-US", "vue-i18n current locale"),
        ("document.querySelector('[data-i18n-greeting]').getAttribute('data-i18n-greeting')", "Hello from vue-i18n", "vue-i18n translated message"),
        ("document.querySelector('[data-vueuse-query]').getAttribute('data-vueuse-query')", "true", "vueuse media query ref"),
        ("document.querySelector('[data-vueuse-prefers-dark]').getAttribute('data-vueuse-prefers-dark')", "true", "vueuse prefers-dark ref"),
        ("document.querySelector('[data-floating-loaded]').getAttribute('data-floating-loaded')", "true", "floating-ui middleware factory"),
        ("document.querySelector('[data-vee-validate-loaded]').getAttribute('data-vee-validate-loaded')", "true", "vee-validate configure entry"),
        ("document.querySelector('[data-vue-query-loaded]').getAttribute('data-vue-query-loaded')", "true", "vue-query plugin export")
    };

    foreach (var (expression, expect, description) in assertions)
    {
        var actual = await browser.EvaluateAsync(expression);
        RequireEqual(expect, actual, description);
    }

    // 用户可观察交互：点击 locale 切换后，locale 与翻译消息都应随之改变。
    await browser.EvaluateAsync("document.querySelector('[data-i18n-action=\"switch-locale\"]').click()");
    await browser.EvaluateAsync("new Promise(r => requestAnimationFrame(() => requestAnimationFrame(r)))");
    RequireEqual("zh-CN", await browser.EvaluateAsync(
        "document.querySelector('[data-i18n-locale]').getAttribute('data-i18n-locale')"), "locale switched to zh-CN");
    RequireEqual("来自 vue-i18n 的问候", await browser.EvaluateAsync(
        "document.querySelector('[data-i18n-greeting]').getAttribute('data-i18n-greeting')"), "message re-translated after switch");

    // A generated render module is a normal Vite HMR boundary. Change its source after the
    // mounted component owns local state; Vue should replace the definition without recreating
    // the instance, so the counter remains 1 while the static heading updates.
    await browser.EvaluateAsync("document.querySelector('[data-hmr-action=\"increment\"]').click()");
    await browser.EvaluateAsync("new Promise(r => requestAnimationFrame(() => requestAnimationFrame(r)))");
    RequireEqual("1", await browser.EvaluateAsync(
        "document.querySelector('[data-hmr-counter]').getAttribute('data-hmr-counter')"), "HMR counter primed");

    var updatedShowcaseModule = originalShowcaseModule.Replace(
        "Vue ecosystem bindings showcase",
        "Vue ecosystem bindings showcase v2",
        StringComparison.Ordinal);
    if (string.Equals(updatedShowcaseModule, originalShowcaseModule, StringComparison.Ordinal))
        throw new InvalidOperationException("The generated showcase module did not contain the HMR update marker.");
    await File.WriteAllTextAsync(showcaseModulePath, updatedShowcaseModule);
    await browser.WaitUntilAsync(
        "document.querySelector('h1')?.textContent === 'Vue ecosystem bindings showcase v2'",
        "generated Vue component HMR update",
        TimeSpan.FromSeconds(30));
    RequireEqual("1", await browser.EvaluateAsync(
        "document.querySelector('[data-hmr-counter]').getAttribute('data-hmr-counter')"), "HMR state preserved");

    Console.WriteLine("ECMAScript bindings showcase browser smoke passed.");
    Console.WriteLine("Verified: DateFns, VueI18n (locale switch), VueUse, Floating UI, VeeValidate, and Vue Query render in a real browser.");
}
finally
{
    try
    {
        await File.WriteAllTextAsync(showcaseModulePath, originalShowcaseModule);
    }
    catch (Exception)
    {
        // Restore is best-effort cleanup after the browser and project processes are stopped.
    }
    TryKill(vite);
    TryKill(host);
    TryDelete(profileRoot);
}

static async Task RunAsync(string fileName, string[] arguments, string workingDirectory)
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

    using var process = Process.Start(startInfo)
        ?? throw new InvalidOperationException($"Failed to start '{fileName}'.");
    // Read both pipes concurrently: draining stdout to completion first can deadlock when the
    // child fills its stderr pipe in the meantime.
    // 必须并发读取两个管道；先读完 stdout 会在子进程写满 stderr 时死锁。
    var stdoutTask = process.StandardOutput.ReadToEndAsync();
    var stderrTask = process.StandardError.ReadToEndAsync();
    await Task.WhenAll(stdoutTask, stderrTask);
    await process.WaitForExitAsync();
    var stdout = stdoutTask.Result;
    var stderr = stderrTask.Result;
    if (process.ExitCode != 0)
        throw new InvalidOperationException($"{fileName} {string.Join(' ', arguments)} failed ({process.ExitCode}).\n{stdout}\n{stderr}");
}

static Process StartHost(string outputRoot, string artifactRoot, int port, int vitePort)
{
    var executable = Path.Combine(outputRoot, "BindingsShowcase.Host.dll");
    if (!File.Exists(executable))
        throw new FileNotFoundException($"Showcase host assembly was not built: {executable}");

    var startInfo = new ProcessStartInfo("dotnet")
    {
        WorkingDirectory = outputRoot,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    };
    startInfo.Environment["ASPNETCORE_URLS"] = $"http://127.0.0.1:{port}";
    startInfo.Environment["Showcase__JazorRoot"] = artifactRoot;
    startInfo.Environment["Showcase__JavaScriptServer"] = $"http://127.0.0.1:{vitePort}";
    // The browser smoke exercises the standard project's Vite dev server and HMR proxy.
    // Development selects the ordinary entry.js shell; production is covered by the Vite build
    // and release smoke lanes.
    startInfo.Environment["DOTNET_ENVIRONMENT"] = "Development";
    startInfo.Environment["Logging__LogLevel__Default"] = "Warning";
    startInfo.ArgumentList.Add(executable);
    var process = Process.Start(startInfo)
        ?? throw new InvalidOperationException("Failed to start the showcase host.");
    // Drain both streams: a redirected pipe that is never read fills up and blocks the host.
    // 必须持续读取被重定向的输出管道，否则管道填满后宿主会阻塞。
    DrainProcessOutput(process, "bindings-showcase-host");
    return process;
}

static Process StartVite(string artifactRoot, string repoRoot, int port)
{
    var deno = ResolveDenoHostRuntime(repoRoot);
    var startInfo = new ProcessStartInfo(deno)
    {
        WorkingDirectory = artifactRoot,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    };
    startInfo.ArgumentList.Add("task");
    startInfo.ArgumentList.Add("dev");
    startInfo.ArgumentList.Add("--host");
    startInfo.ArgumentList.Add("127.0.0.1");
    startInfo.ArgumentList.Add("--port");
    startInfo.ArgumentList.Add(port.ToString(System.Globalization.CultureInfo.InvariantCulture));
    startInfo.ArgumentList.Add("--strictPort");
    startInfo.Environment["DENO_NO_PROMPT"] = "1";
    var process = Process.Start(startInfo)
        ?? throw new InvalidOperationException("Failed to start the generated project's Vite server.");
    // Keep both pipes drained so Vite cannot block on redirected diagnostics.
    DrainProcessOutput(process, "bindings-showcase-vite");
    return process;
}

static void DrainProcessOutput(Process process, string name)
{
    _ = Task.Run(async () =>
    {
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await Task.WhenAll(stdout, stderr);
        if (process.ExitCode is not 0 and not -1)
        {
            Console.Error.WriteLine($"{name} exited with code {process.ExitCode}.");
            Console.Error.WriteLine(stdout.Result);
            Console.Error.WriteLine(stderr.Result);
        }
    });
}

static async Task WaitForViteAsync(string url)
{
    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
    for (var attempt = 0; attempt < 60; attempt++)
    {
        try
        {
            using var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
                return;
        }
        catch (HttpRequestException)
        {
        }

        await Task.Delay(500);
    }

    throw new TimeoutException($"The generated project's Vite server did not respond at {url}.");
}

static async Task WaitForHostAsync(string url)
{
    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
    using var request = new HttpRequestMessage(HttpMethod.Get, url);
    request.Headers.Add("Accept", "text/html");
    for (var attempt = 0; attempt < 60; attempt++)
    {
        try
        {
            using var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
                return;
        }
        catch (HttpRequestException)
        {
        }

        await Task.Delay(500);
    }

    throw new TimeoutException($"The showcase host did not respond at {url}.");
}

static int ReservePort()
{
    var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    var port = ((IPEndPoint)listener.LocalEndpoint).Port;
    listener.Stop();
    return port;
}

static string? ResolveBrowserExecutable()
{
    var explicitPath = Environment.GetEnvironmentVariable("RAZORVUE_BROWSER_EXE")?.Trim();
    if (!string.IsNullOrWhiteSpace(explicitPath))
        return File.Exists(explicitPath) ? explicitPath : null;

    var candidates = OperatingSystem.IsWindows()
        ? new[]
        {
            @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
            @"C:\Program Files\Chromium\Application\chrome.exe"
        }
        : new[]
        {
            "/usr/bin/google-chrome",
            "/usr/bin/chromium",
            "/usr/bin/chromium-browser"
        };

    return candidates.FirstOrDefault(File.Exists);
}

static string FindRepositoryRoot()
{
    for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory()); directory is not null; directory = directory.Parent)
    {
        if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            return directory.FullName;
    }

    throw new InvalidOperationException("Run from the repository root (Jazor.slnx not found).");
}

static string ResolveDenoHostRuntime(string repoRoot)
{
    var explicitPath = Environment.GetEnvironmentVariable("JAZOR_DENO_EXE")?.Trim();
    if (!string.IsNullOrWhiteSpace(explicitPath) && File.Exists(explicitPath))
        return explicitPath;

    var executableName = OperatingSystem.IsWindows() ? "deno.exe" : "deno";
    var packageRoot = Path.Combine(repoRoot, ".dotnet", ".nuget", "packages");
    var candidate = Directory.Exists(packageRoot)
        ? Directory.EnumerateDirectories(packageRoot, "denohost.runtime.*")
            .SelectMany(static root => Directory.EnumerateFiles(root, "deno*", SearchOption.AllDirectories))
            .Where(path => string.Equals(Path.GetFileName(path), executableName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(static path => path, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault()
        : null;
    if (candidate is not null)
        return candidate;

    throw new FileNotFoundException(
        "DenoHost runtime was not restored. Set JAZOR_DENO_EXE to a Deno executable or restore Jazor.Emit.");
}

static void RequireEqual(string expected, string? actual, string description)
{
    if (!string.Equals(expected, actual, StringComparison.Ordinal))
        throw new InvalidOperationException($"Assertion failed: {description}. Expected '{expected}', actual '{actual}'.");
}

static void TryKill(Process process)
{
    try
    {
        if (!process.HasExited)
            process.Kill(entireProcessTree: true);
        process.WaitForExit(5000);
    }
    catch (Exception)
    {
        // Best-effort teardown only.
    }
}

static void TryDelete(string directory)
{
    try
    {
        if (Directory.Exists(directory))
            Directory.Delete(directory, recursive: true);
    }
    catch (Exception)
    {
        // Best-effort teardown only.
    }
}

/// <summary>Minimal CDP client: launch headless Chrome, evaluate expressions, read results.</summary>
internal sealed class BrowserSession : IDisposable
{
    private readonly Process process;
    private readonly string profileRoot;
    private readonly ClientWebSocket socket;
    private int nextId = 1;

    private BrowserSession(Process process, string profileRoot, ClientWebSocket socket)
    {
        this.process = process;
        this.profileRoot = profileRoot;
        this.socket = socket;
    }

    public static async Task<BrowserSession> StartAsync(string browserPath, string profileRoot, string pageUrl)
    {
        var port = ReservePort();
        var startInfo = new ProcessStartInfo(browserPath)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.Environment["Logging__LogLevel__Default"] = "Warning";
        foreach (var argument in new[]
        {
            "--headless=new",
            "--disable-gpu",
            "--disable-dev-shm-usage",
            "--no-first-run",
            "--no-default-browser-check",
            "--no-sandbox",
            $"--remote-debugging-port={port}",
            $"--user-data-dir={profileRoot}",
            "about:blank"
        })
        {
            startInfo.ArgumentList.Add(argument);
        }

        var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("The browser process could not be started.");
        _ = process.StandardOutput.ReadToEndAsync();
        _ = process.StandardError.ReadToEndAsync();

        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
        string? webSocketUrl = null;
        for (var attempt = 0; attempt < 60 && webSocketUrl is null; attempt++)
        {
            try
            {
                var payload = await client.GetStringAsync($"http://127.0.0.1:{port}/json/list");
                using var document = JsonDocument.Parse(payload);
                webSocketUrl = document.RootElement.EnumerateArray()
                    .Where(static item => item.TryGetProperty("type", out var type) && type.GetString() == "page")
                    .Select(static item => item.GetProperty("webSocketDebuggerUrl").GetString())
                    .FirstOrDefault();
            }
            catch (Exception)
            {
            }

            if (webSocketUrl is null)
                await Task.Delay(500);
        }

        if (webSocketUrl is null)
        {
            TryKillProcess(process);
            throw new InvalidOperationException("Could not resolve the CDP page target.");
        }

        var socket = new ClientWebSocket();
        await socket.ConnectAsync(new Uri(webSocketUrl), CancellationToken.None);
        var session = new BrowserSession(process, profileRoot, socket);
        await session.SendAsync("Runtime.enable");
        await session.SendAsync("Page.enable");
        await session.SendAsync("Page.navigate", new Dictionary<string, object?> { ["url"] = pageUrl });

        // Wait for the document, then for the bindings page to mount.
        await session.WaitUntilAsync("document.readyState === 'complete'", "completed browser document", TimeSpan.FromSeconds(30));
        await session.WaitUntilAsync(
            "document.querySelector('[data-showcase=\"landing\"]') !== null",
            "mounted bindings page",
            TimeSpan.FromSeconds(30));
        return session;
    }

    public async Task<string?> EvaluateAsync(string expression)
    {
        using var result = await SendAsync("Runtime.evaluate", new Dictionary<string, object?>
        {
            ["expression"] = expression,
            ["awaitPromise"] = true,
            ["returnByValue"] = true
        });
        var payload = result.RootElement.GetProperty("result");
        if (payload.TryGetProperty("exceptionDetails", out var exception))
            throw new InvalidOperationException("Runtime.evaluate failed: " + exception.GetRawText());
        var value = payload.GetProperty("result");
        if (value.TryGetProperty("value", out var v) && v.ValueKind != JsonValueKind.Null)
            return v.ValueKind switch
            {
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => v.ToString()
            };
        if (value.TryGetProperty("type", out var type) && type.GetString() == "undefined")
            return null;

        throw new InvalidOperationException("Runtime.evaluate returned no value: " + result.RootElement.GetRawText());
    }

    public async Task WaitUntilAsync(string expression, string description, TimeSpan timeout)
    {
        var deadline = DateTimeOffset.UtcNow + timeout;
        while (DateTimeOffset.UtcNow < deadline)
        {
            if (await EvaluateAsync(expression) == "true")
                return;
            await Task.Delay(250);
        }

        var location = await EvaluateAsync("location.href") ?? "unknown";
        var bodyLength = await EvaluateAsync("document.body ? document.body.innerHTML.length : -1") ?? "unknown";
        throw new TimeoutException($"Timed out waiting for {description} (location={location}, bodyLength={bodyLength}).");
    }

    private async Task<JsonDocument> SendAsync(string method, Dictionary<string, object?>? parameters = null)
    {
        var id = nextId++;
        var message = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["id"] = id,
            ["method"] = method,
            ["params"] = parameters ?? new Dictionary<string, object?>()
        });

        await socket.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, CancellationToken.None);

        var buffer = new byte[1 << 16];
        using var receiveTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        while (true)
        {
            var builder = new StringBuilder();
            WebSocketReceiveResult received;
            do
            {
                received = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), receiveTimeout.Token);
                builder.Append(Encoding.UTF8.GetString(buffer, 0, received.Count));
            }
            while (!received.EndOfMessage);

            var document = JsonDocument.Parse(builder.ToString());
            if (document.RootElement.TryGetProperty("id", out var responseId) && responseId.GetInt32() == id)
                return document;
            document.Dispose();
        }
    }

    public void Dispose()
    {
        try
        {
            socket.Dispose();
        }
        catch (Exception)
        {
        }

        TryKillProcess(process);
        TryDeleteDirectory(profileRoot);
    }

    private static void TryKillProcess(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
            process.WaitForExit(5000);
        }
        catch (Exception)
        {
        }
    }

    private static void TryDeleteDirectory(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
        catch (Exception)
        {
        }
    }

    private static int ReservePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}

internal sealed record SmokeOptions(bool SkipBuild)
{
    public static SmokeOptions Parse(string[] args)
    {
        var skipBuild = false;
        foreach (var argument in args)
        {
            if (argument == "--skip-build")
                skipBuild = true;
            else
                throw new InvalidOperationException("Unknown argument: " + argument);
        }

        return new SmokeOptions(skipBuild);
    }
}
