#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Xml.Linq;

if (args is not [var packageSource])
    throw new ArgumentException("Usage: verify-jolt-spa-package.cs <package-source>");

var root = Directory.GetCurrentDirectory();
var fixture = Path.Combine(root, "samples", "Jolt.AspNetCore");
var feed = Path.GetFullPath(packageSource);
var version = Directory.EnumerateFiles(feed, "Jazor.*.nupkg")
    .Select(Path.GetFileNameWithoutExtension)
    .Single(name => name is { Length: > 6 } && char.IsAsciiDigit(name[6]))![6..];
var work = Directory.CreateTempSubdirectory("jazor-spa-consumer-");
try
{
    var hostPort = FreePort();
    var vitePort = FreePort();
    foreach (var file in new[] { "Counter.jazor", "index.html", "main.js", "package.json", "pnpm-lock.yaml", "vite.config.js" })
        File.Copy(Path.Combine(fixture, file), Path.Combine(work.FullName, file));
    var program = File.ReadAllText(Path.Combine(fixture, "Program.cs"));
    File.WriteAllText(Path.Combine(work.FullName, "Program.cs"), program.Replace("127.0.0.1:15658", $"127.0.0.1:{vitePort}", StringComparison.Ordinal));
    new XDocument(new XElement("Project", new XAttribute("Sdk", "Microsoft.NET.Sdk.Web"),
        new XElement("PropertyGroup", new XElement("TargetFramework", "net11.0"), new XElement("ImplicitUsings", "enable")),
        new XElement("ItemGroup", new[] { "Jazor", "ECMAScript.Vue" }.Select(id =>
            new XElement("PackageReference", new XAttribute("Include", id), new XAttribute("Version", version))))))
        .Save(Path.Combine(work.FullName, "Consumer.csproj"));
    new XDocument(new XElement("configuration", new XElement("packageSources", new XElement("clear"),
        new XElement("add", new XAttribute("key", "release"), new XAttribute("value", feed)),
        new XElement("add", new XAttribute("key", "nuget.org"), new XAttribute("value", "https://api.nuget.org/v3/index.json")))))
        .Save(Path.Combine(work.FullName, "NuGet.Config"));

    using (var build = Start("dotnet", "build", "--configuration", "Debug"))
    {
        await build.WaitForExitAsync();
        if (build.ExitCode != 0)
            throw new InvalidOperationException($"Packaged SPA consumer build failed with exit code {build.ExitCode}.");
    }

    // The apphost embeds the package-selected CLI metadata and the Windows CET setting.
    var executable = Path.Combine(work.FullName, "bin", "Debug", "net11.0", OperatingSystem.IsWindows() ? "Consumer.exe" : "Consumer");
    using var app = Start(executable, "--urls", $"http://127.0.0.1:{hostPort}", "--environment", "Development");
    try
    {
        using var client = new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{hostPort}/portal/frontend/") };
        using var startup = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        while (true)
        {
            if (app.HasExited)
                throw new InvalidOperationException($"Packaged SPA host exited with code {app.ExitCode}.");
            try
            {
                var index = await client.GetStringAsync("index.html", startup.Token);
                if (!index.Contains("/portal/frontend/@vite/client", StringComparison.Ordinal))
                    throw new InvalidOperationException("Packaged SPA HTML is missing the same-origin Vite client.");
                break;
            }
            catch (HttpRequestException error) when (error.HttpRequestError == HttpRequestError.ConnectionError)
            {
                await Task.Delay(200, startup.Token);
            }
        }
        foreach (var (route, marker) in new[] { ("@vite/client", "createHotContext"), ("Counter.mjs", "export default") })
            if (!(await client.GetStringAsync(route, startup.Token)).Contains(marker, StringComparison.Ordinal))
                throw new InvalidOperationException($"Packaged SPA returned unexpected content for {route}.");
    }
    finally
    {
        // Terminate only the host. Its closed stdin pipe must shut down the owned CLI.
        if (!app.HasExited)
            app.Kill();
        await app.WaitForExitAsync();
        using var shutdown = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        while (await IsListening(vitePort))
            await Task.Delay(200, shutdown.Token);
    }
    Console.WriteLine($"Verified Jazor {version} and its pinned Jolt CLI through ASP.NET SPA startup and shutdown.");
}
finally
{
    work.Delete(recursive: true);
}

Process Start(string executable, params string[] arguments)
{
    var start = new ProcessStartInfo(executable) { WorkingDirectory = work.FullName, UseShellExecute = false };
    foreach (var argument in arguments)
        start.ArgumentList.Add(argument);
    return Process.Start(start) ?? throw new InvalidOperationException($"Could not start {executable}.");
}

static int FreePort()
{
    using var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    return ((IPEndPoint)listener.LocalEndpoint).Port;
}

static async Task<bool> IsListening(int port)
{
    using var socket = new TcpClient();
    try
    {
        await socket.ConnectAsync(IPAddress.Loopback, port);
        return true;
    }
    catch (SocketException error) when (error.SocketErrorCode == SocketError.ConnectionRefused)
    {
        return false;
    }
}
