extern alias BouncyCastleV2;

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using BouncyCastleV2::Org.BouncyCastle.Asn1.GM;
using BouncyCastleV2::Org.BouncyCastle.Math;
using Xunit;

namespace Jero.Test;

public sealed class BrowserHost : IAsyncLifetime
{
    private Process? _host;
    private string? _workspace;

    public string BaseUrl { get; private set; } = null!;

    public bool IsExternal { get; private set; }

    public async ValueTask InitializeAsync()
    {
        if (Environment.GetEnvironmentVariable("JERO_TEST_BASE_URL")?.Trim() is { Length: > 0 } external)
        {
            if (!Uri.TryCreate(external, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                throw new InvalidOperationException("JERO_TEST_BASE_URL must be an absolute HTTP or HTTPS URL.");

            IsExternal = true;
            BaseUrl = external.TrimEnd('/');
            return;
        }

        _workspace = TestWorkspace.Create("browser-host");
        var projectDirectory = ProjectDirectory();
        var publishDirectory = Path.Combine(_workspace, "publish");
        var configuration = typeof(BrowserHost).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()!.Configuration;
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(4));
        try
        {
            var publish = StartInfo("dotnet", projectDirectory);
            foreach (var argument in new[] { "publish", "Jero.Web/Jero.Web.csproj", "--no-build", "-c", configuration, "-o", publishDirectory })
                publish.ArgumentList.Add(argument);
            using (var process = Process.Start(publish)!)
            {
                var output = process.StandardOutput.ReadToEndAsync();
                var errors = process.StandardError.ReadToEndAsync();
                try
                {
                    await process.WaitForExitAsync(timeout.Token);
                }
                finally
                {
                    await StopAsync(process);
                    await File.WriteAllTextAsync(Path.Combine(_workspace, "publish.log"), await output + await errors);
                }
                if (process.ExitCode != 0)
                    throw new InvalidOperationException($"Jero publish exited with code {process.ExitCode}.");
            }

            PrepareConfiguration(publishDirectory);

            var appHost = Path.Combine(publishDirectory, OperatingSystem.IsWindows() ? "Jero.Web.exe" : "Jero.Web");
            var start = StartInfo(appHost, publishDirectory);
            start.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
            start.Environment["DOTNET_ENVIRONMENT"] = "Production";
            using var port = new TcpListener(IPAddress.Loopback, 0);
            port.Start();
            BaseUrl = $"http://127.0.0.1:{((IPEndPoint)port.LocalEndpoint).Port}";
            port.Stop();
            start.Environment["Kestrel__Endpoints__Http__Url"] = BaseUrl;
            start.Environment["JWTSettings__IssuerSigningKey"] = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
            start.Environment["Logging__EventLog__LogLevel__Default"] = "None";
            start.Environment["SPA__PathBase"] = "";
            _host = Process.Start(start)!;
            _standardOutput = LogAsync(_host.StandardOutput, "host.log");
            _standardError = LogAsync(_host.StandardError, "host-errors.log");
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            while (!_host.HasExited)
            {
                try
                {
                    using var response = await client.GetAsync(BaseUrl, timeout.Token);
                    response.EnsureSuccessStatusCode();
                    return;
                }
                catch (Exception error) when (error is HttpRequestException || error is TaskCanceledException && !timeout.IsCancellationRequested)
                {
                    await Task.Delay(250, timeout.Token);
                }
            }
            throw new InvalidOperationException($"The host exited with code {_host.ExitCode}.");
        }
        catch (Exception error)
        {
            await DisposeAsync();
            throw new InvalidOperationException($"Could not start the isolated Jero host. Logs: {_workspace}", error);
        }
    }

    private Task? _standardOutput;
    private Task? _standardError;

    private async Task LogAsync(StreamReader source, string name)
    {
        await using var log = File.CreateText(Path.Combine(_workspace!, name));
        while (await source.ReadLineAsync() is { } line)
            await log.WriteLineAsync(line);
    }

    public async ValueTask DisposeAsync()
    {
        if (_host is not { } host)
            return;

        _host = null;
        using (host)
        {
            await StopAsync(host);
            await Task.WhenAll(_standardOutput!, _standardError!);
        }
    }

    private ProcessStartInfo StartInfo(string executable, string workingDirectory)
    {
        var start = new ProcessStartInfo(executable)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var variable in new[] { "TEMP", "TMP", "TMPDIR" })
            start.Environment[variable] = _workspace;
        return start;
    }

    private static async Task StopAsync(Process process)
    {
        if (!process.HasExited)
            process.Kill(entireProcessTree: true);
        await process.WaitForExitAsync();
    }

    private void PrepareConfiguration(string publishDirectory)
    {
        const string configuration = "Configuration";
        // Furion loads modular configuration after environment providers.
        var appPath = Path.Combine(publishDirectory, configuration, "App.json");
        var appOptions = ReadOptions(appPath);
        var curve = GMNamedCurves.GetByName("sm2p256v1");
        var key = new BigInteger(1, RandomNumberGenerator.GetBytes(32)).Mod(curve.N.Subtract(BigInteger.One)).Add(BigInteger.One);
        appOptions["Cryptogram"]!["PrivateKey"] = Convert.ToHexString(key.ToByteArrayUnsigned());
        appOptions["Cryptogram"]!["PublicKey"] = Convert.ToHexString(curve.G.Multiply(key).GetEncoded(false));
        File.WriteAllText(appPath, appOptions.ToJsonString());

        var databasePath = Path.Combine(publishDirectory, configuration, "Database.json");
        var databaseOptions = ReadOptions(databasePath);
        databaseOptions["DbConnection"]!["ConnectionConfigs"]![0]!["ConnectionString"] =
            $"DataSource={Path.Combine(_workspace!, "Jero.db")}";
        File.WriteAllText(databasePath, databaseOptions.ToJsonString());
    }

    private static string ProjectDirectory([CallerFilePath] string source = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(source)!, ".."));

    private static JsonNode ReadOptions(string path) => JsonNode.Parse(File.ReadAllText(path),
        documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true })!;
}
