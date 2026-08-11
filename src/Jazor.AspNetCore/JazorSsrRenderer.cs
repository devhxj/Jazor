using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Jazor.AspNetCore;

/// <summary>
/// Runs one generated Vue root inside a fresh local Deno process.
/// Deno is an explicit transition backend; the public renderer contract is kept independent
/// so a Jint executor can later consume a Netpack-produced SSR bundle.
/// </summary>
internal sealed class JazorSsrRenderer : IJazorSsrRenderer
{
    private const string RunnerResourceName = "Jazor.AspNetCore.Runtime.ssr-runner.mjs";
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.Default
    };

    private readonly JazorSsrArtifactLocator _artifacts;
    private readonly JazorSsrOptions _options;
    private readonly object _runnerGate = new();
    private string? _preparedRunnerRoot;
    private string? _preparedRunnerPath;

    public JazorSsrRenderer(
        JazorSsrArtifactLocator artifacts,
        IOptions<JazorSsrOptions> options)
    {
        _artifacts = artifacts ?? throw new ArgumentNullException(nameof(artifacts));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<JazorSsrRenderResult> RenderAsync(
        JazorSsrRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var artifacts = _artifacts.Resolve();
        var modulePath = JazorSsrArtifactLocator.NormalizeRelativePath(request.ModulePath, "module path");
        var serializedProps = JsonSerializer.Serialize(request.Props, JsonOptions);
        using var propsDocument = JsonDocument.Parse(serializedProps);
        var executionPayload = JsonSerializer.Serialize(
            new SsrExecutionRequest(modulePath, propsDocument.RootElement),
            JsonOptions);
        var runnerPath = EnsureRunner(artifacts.RootPath);
        var output = await ExecuteAsync(artifacts, runnerPath, executionPayload, cancellationToken)
            .ConfigureAwait(false);

        using var response = JsonDocument.Parse(output);
        if (!response.RootElement.TryGetProperty("html", out var htmlElement) ||
            htmlElement.ValueKind != JsonValueKind.String ||
            htmlElement.GetString() is not { } html)
        {
            throw new InvalidOperationException("Jazor SSR runner returned an invalid response payload.");
        }

        return new JazorSsrRenderResult(modulePath, html, serializedProps);
    }

    private async Task<string> ExecuteAsync(
        JazorSsrArtifacts artifacts,
        string runnerPath,
        string executionPayload,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = ResolveDenoExecutable(),
            WorkingDirectory = artifacts.RootPath,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--no-config");
        startInfo.ArgumentList.Add("--no-npm");
        startInfo.ArgumentList.Add("--no-remote");
        startInfo.ArgumentList.Add("--no-prompt");
        startInfo.ArgumentList.Add("--allow-read=" + artifacts.RootPath);
        startInfo.ArgumentList.Add("--import-map");
        startInfo.ArgumentList.Add(artifacts.SsrImportMapPath);
        startInfo.ArgumentList.Add(runnerPath);
        startInfo.Environment["DENO_NO_UPDATE_CHECK"] = "1";

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Jazor SSR failed to start the configured Deno executable.");
        var standardOutput = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var standardError = process.StandardError.ReadToEndAsync(cancellationToken);
        try
        {
            await process.StandardInput.WriteAsync(executionPayload.AsMemory(), cancellationToken).ConfigureAwait(false);
            process.StandardInput.Close();
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            TryStop(process);
            throw;
        }

        var output = await standardOutput.ConfigureAwait(false);
        var error = await standardError.ConfigureAwait(false);
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                "Jazor SSR Deno execution failed for '" + runnerPath + "'." + Environment.NewLine + error);
        }

        return output;
    }

    private string EnsureRunner(string artifactRoot)
    {
        lock (_runnerGate)
        {
            if (string.Equals(_preparedRunnerRoot, artifactRoot, StringComparison.Ordinal) &&
                _preparedRunnerPath is not null &&
                File.Exists(_preparedRunnerPath))
            {
                return _preparedRunnerPath;
            }

            var runnerPath = Path.Combine(artifactRoot, "@jazor", "ssr-runner.mjs");
            var runnerSource = ReadRunnerSource();
            Directory.CreateDirectory(Path.GetDirectoryName(runnerPath)!);
            if (!File.Exists(runnerPath) || !string.Equals(File.ReadAllText(runnerPath), runnerSource, StringComparison.Ordinal))
                File.WriteAllText(runnerPath, runnerSource, Utf8WithoutBom);

            _preparedRunnerRoot = artifactRoot;
            _preparedRunnerPath = runnerPath;
            return runnerPath;
        }
    }

    private static string ReadRunnerSource()
    {
        var assembly = typeof(JazorSsrRenderer).Assembly;
        using var stream = assembly.GetManifestResourceStream(RunnerResourceName)
            ?? throw new InvalidOperationException("Jazor SSR runner resource was not embedded in Jazor.AspNetCore.");
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd().ReplaceLineEndings("\n");
    }

    private string ResolveDenoExecutable()
    {
        if (!string.IsNullOrWhiteSpace(_options.DenoExecutablePath))
        {
            var configuredPath = Path.GetFullPath(_options.DenoExecutablePath);
            if (File.Exists(configuredPath))
                return configuredPath;

            throw new FileNotFoundException(
                "The configured Jazor SSR Deno executable was not found.",
                configuredPath);
        }

        throw new InvalidOperationException(
            "The temporary Jazor Deno SSR renderer requires JazorSsrOptions.DenoExecutablePath. " +
            "Deno is not distributed as an implicit ASP.NET Core host runtime.");
    }

    private static void TryStop(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch
        {
        }
    }

    // The runner protocol is a JavaScript-owned ABI. Keep its field names explicit so the
    // host-wide CLR naming policy never becomes an accidental transport convention.
    private sealed record SsrExecutionRequest(
        [property: JsonPropertyName("modulePath")] string ModulePath,
        [property: JsonPropertyName("props")] JsonElement Props);
}
