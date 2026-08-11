using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using DenoHost.Core;
using Microsoft.Extensions.Options;

namespace Jazor.AspNetCore;

/// <summary>
/// Runs one generated Vue root inside a fresh DenoHost-managed Deno process.
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
        var requestPath = Path.Combine(
            Path.GetDirectoryName(runnerPath)!,
            "ssr-request-" + Guid.NewGuid().ToString("N") + ".json");

        string output;
        try
        {
            await File.WriteAllTextAsync(requestPath, executionPayload, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            output = await ExecuteAsync(artifacts, runnerPath, requestPath, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            if (File.Exists(requestPath))
                File.Delete(requestPath);
        }

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
        string requestPath,
        CancellationToken cancellationToken)
    {
        var standardOutput = new StringBuilder();
        var standardError = new StringBuilder();
        using var process = new DenoProcess(
            new DenoExecuteBaseOptions
            {
                WorkingDirectory = artifacts.RootPath
            },
            [
                "run",
                "--no-config",
                "--no-npm",
                "--no-remote",
                "--no-prompt",
                "--allow-read=" + artifacts.RootPath,
                "--import-map",
                artifacts.SsrImportMapPath,
                runnerPath,
                requestPath
            ]);
        process.OutputDataReceived += (_, eventArgs) => AppendLine(standardOutput, eventArgs.Data);
        process.ErrorDataReceived += (_, eventArgs) => AppendLine(standardError, eventArgs.Data);

        try
        {
            await process.StartAsync(cancellationToken).ConfigureAwait(false);
            var exitCode = await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            if (exitCode != 0)
            {
                throw new InvalidOperationException(
                    "Jazor SSR DenoHost execution failed for '" + runnerPath + "'." + Environment.NewLine + standardError);
            }
        }
        catch (OperationCanceledException)
        {
            await StopAsync(process).ConfigureAwait(false);
            throw;
        }

        return standardOutput.ToString();
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

    private static void AppendLine(StringBuilder output, string? line)
    {
        if (line is not null)
            output.AppendLine(line);
    }

    private static async Task StopAsync(DenoProcess process)
    {
        try
        {
            if (process.IsRunning)
                await process.StopAsync(null, CancellationToken.None).ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
            // The process can exit between IsRunning and StopAsync during cancellation cleanup.
        }
    }

    // The runner protocol is a JavaScript-owned ABI. Keep its field names explicit so the
    // host-wide CLR naming policy never becomes an accidental transport convention.
    private sealed record SsrExecutionRequest(
        [property: JsonPropertyName("modulePath")] string ModulePath,
        [property: JsonPropertyName("props")] JsonElement Props);
}
