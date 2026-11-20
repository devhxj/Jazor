#!/usr/bin/env dotnet run
#:sdk Microsoft.NET.Sdk.Web
#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:project ../../src/Jazor.NodeHost/Jazor.NodeHost.csproj

using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Jazor.NodeHost;

using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(4));
var cancellationToken = timeout.Token;
if (args is ["--build-consumer", var projectDirectory])
{
    await NodeToolchain.BuildAsync(projectDirectory, ssr: false, cancellationToken, Console.Out);
    return;
}

if (args.Length is not (0 or 2))
    throw new ArgumentException("Usage: verify-jolt-library.cs [jolt-apphost browser-executable | --browser browser-executable]");
var appHost = args.Length == 2 && args[0] != "--browser" ? Path.GetFullPath(args[0]) : null;
var browser = args.Length == 2 ? Path.GetFullPath(args[1]) : FindBrowser();
var repository = new DirectoryInfo(Directory.GetCurrentDirectory());
while (repository is not null && !File.Exists(Path.Combine(repository.FullName, "Jazor.slnx")))
    repository = repository.Parent;
var root = repository?.FullName ?? throw new InvalidOperationException("Cannot locate Jazor.slnx.");
var fixtures = Path.Combine(root, "scripts", "csharp", "fixtures");
var scratch = Directory.CreateDirectory(Path.Combine(root, ".tmp", "jolt-library-" + Guid.NewGuid().ToString("N")[..8]));
try
{
    var library = Path.Combine(scratch.FullName, "library");
    var package = Path.Combine(scratch.FullName, "meter-package");
    var consumer = Path.Combine(scratch.FullName, "consumer");
    Copy(Path.Combine(fixtures, "jolt-library", "library"), library);
    Copy(Path.Combine(fixtures, "jolt-library", "consumer"), consumer);
    var store = Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory") ?? Path.Combine(root, ".tmp", "nodeapi-store");
    if (appHost is null)
        await RunAsync("dotnet", root, store, cancellationToken,
            "run", "--project", Path.Combine(root, "src", "Jolt.Cli", "Jolt.Cli.csproj"),
            "--artifacts-path", Path.Combine(scratch.FullName, "build"), "--", "library", library, "--output", package);
    else
        await RunAsync(appHost, library, store, cancellationToken, "library", library, "--output", package);

    using var restore = JsonDocument.Parse(await NodeToolchain.RestoreAsync(consumer, store, cancellationToken, Console.Out));
    if (!restore.RootElement.GetProperty("frozen").GetBoolean())
        throw new InvalidOperationException("The Vue consumer fixture must restore with its committed pnpm lockfile.");
    var typecheck = Path.Combine(fixtures, "vue-typecheck.cjs");
    using (var node = new NodeEnvironment(consumer, Console.Out))
    {
        using var valid = JsonDocument.Parse(await node.InvokeAsync(typecheck, "typeCheck", cancellationToken,
            Path.Combine(consumer, "tsconfig.json")));
        if (valid.RootElement.GetProperty("exitCode").GetInt32() != 0)
            throw new InvalidOperationException(valid.RootElement.GetProperty("diagnostics").GetString());
        using var invalid = JsonDocument.Parse(await node.InvokeAsync(typecheck, "typeCheck", cancellationToken,
            Path.Combine(consumer, "tsconfig.invalid.json")));
        var diagnostics = invalid.RootElement.GetProperty("diagnostics").GetString()!;
        if (invalid.RootElement.GetProperty("exitCode").GetInt32() == 0)
            throw new InvalidOperationException("Invalid Vue consumers unexpectedly passed the type check.");
        foreach (var (file, lines) in new[]
        {
            ("Invalid.vue", new[] { 7, 11, 12, 13, 15 }),
            ("InvalidJsx.tsx", new[] { 3, 4, 5, 7, 10 }),
            ("InvalidRows.vue", new[] { 9, 10, 13, 14, 15 })
        })
            foreach (var line in lines)
                if (!Regex.IsMatch(diagnostics, Regex.Escape(file) + @"\(" + line + @",\d+\): error TS\d+"))
                    throw new InvalidOperationException($"Expected a type error in {file} on line {line}.\n{diagnostics}");
        Console.WriteLine("Vue Language Tools accepted both consumers and rejected invalid props, models, slots and exposed method calls.");
    }
    // Windows keeps Rolldown's native addon loaded until the apphost exits.
    // Finish the build in this same script's apphost before deleting node_modules.
    var worker = Environment.ProcessPath ?? throw new InvalidOperationException("Cannot locate the verification apphost.");
    Console.Write(await RunAsync(worker, consumer, store, cancellationToken, "--build-consumer", consumer));

    var output = Path.Combine(consumer, "dist");
    var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { WebRootPath = output });
    builder.Logging.SetMinimumLevel(LogLevel.Warning);
    var app = builder.Build();
    await using (app)
    {
        app.Urls.Add("http://127.0.0.1:0");
        app.UseDefaultFiles();
        app.UseStaticFiles();
        await app.StartAsync(cancellationToken);
        var page = await RunAsync(browser, consumer, store, cancellationToken,
            "--headless", "--no-sandbox", "--disable-gpu", "--dump-dom",
            $"--user-data-dir={Path.Combine(scratch.FullName, "chrome")}", app.Urls.Single());
        if (!page.Contains("<output id=\"result\">Passed</output>", StringComparison.Ordinal))
            throw new InvalidOperationException("Production browser checks failed.\n" + page);
        await app.StopAsync(cancellationToken);
    }
    Console.WriteLine("Chrome verified root and subpath imports, model updates, scoped slot props, exposed methods and package styles.");
}
finally
{
    scratch.Delete(recursive: true);
}

static void Copy(string source, string destination)
{
    foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
    {
        var target = Path.Combine(destination, Path.GetRelativePath(source, file));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.Copy(file, target);
    }
}

static async Task<string> RunAsync(string executable, string directory, string store, CancellationToken cancellationToken, params string[] arguments)
{
    var start = new ProcessStartInfo(executable)
    {
        WorkingDirectory = directory,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };
    start.Environment["JazorPnpmStoreDirectory"] = store;
    foreach (var argument in arguments)
        start.ArgumentList.Add(argument);
    using var process = Process.Start(start) ?? throw new InvalidOperationException($"Could not start {executable}.");
    var stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
    var stderr = process.StandardError.ReadToEndAsync(cancellationToken);
    try
    {
        await process.WaitForExitAsync(cancellationToken);
        var output = await stdout;
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"{executable} exited with {process.ExitCode}.\n{output}\n{await stderr}");
        return output;
    }
    finally
    {
        if (!process.HasExited)
        {
            process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
        }
    }
}

static string FindBrowser()
{
    var locations = OperatingSystem.IsWindows()
        ? new[]
        {
            @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "Application", "chrome.exe")
        }
        : new[] { "/usr/bin/google-chrome", "/usr/bin/chromium", "/usr/bin/chromium-browser" };
    return locations.FirstOrDefault(File.Exists) ??
        throw new FileNotFoundException("Pass --browser with the Chrome or Chromium executable path.");
}
