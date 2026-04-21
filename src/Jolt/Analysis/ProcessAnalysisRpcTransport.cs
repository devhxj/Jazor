using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Jazor.VueContracts.Protocol;
using Jolt.Hosting;
using Jolt.Rpc;

namespace Jolt.Analysis;

public sealed class ProcessAnalysisRpcTransport : IAnalysisRpcTransport
{
    private static readonly TimeSpan DefaultRpcTimeout = TimeSpan.FromSeconds(30);
    private const int MaxResponseProbeLines = 1000;
    private const int MaxCapturedErrorChars = 16 * 1024;
    private const int MaxCapturedOutputLines = 200;

    private readonly string _command;
    private readonly string? _arguments;

    public ProcessAnalysisRpcTransport(string command, string? arguments)
    {
        _command = string.IsNullOrWhiteSpace(command)
            ? throw new ArgumentException("Analysis command must be provided.", nameof(command))
            : command;
        _arguments = arguments;
    }

    public async ValueTask<RpcResponseEnvelope> SendAsync(
        RpcRequestEnvelope request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _command,
                Arguments = _arguments ?? string.Empty,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        using var timeoutSource = new CancellationTokenSource(DefaultRpcTimeout);
        using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutSource.Token);
        var effectiveCancellationToken = linkedSource.Token;
        var processStarted = false;
        Task<string>? errorDrainTask = null;

        try
        {
            processStarted = process.Start();
            if (!processStarted)
            {
                throw new InvalidOperationException($"Failed to start analysis process '{_command}'.");
            }

            errorDrainTask = DrainErrorOutputAsync(process.StandardError, CancellationToken.None);
            var requestJson = JoltRpcSerializer.Serialize(request);
            await process.StandardInput.WriteLineAsync(requestJson.AsMemory(), effectiveCancellationToken);
            await process.StandardInput.FlushAsync(effectiveCancellationToken);
            process.StandardInput.Close();

            var responseJson = await ReadResponseJsonAsync(process.StandardOutput, effectiveCancellationToken);
            await ChildProcessUtilities.WaitForExitOrTerminateOnCancellationAsync(process, effectiveCancellationToken);
            var errorOutput = await AwaitCapturedOutputAsync(errorDrainTask);

            if (string.IsNullOrWhiteSpace(responseJson))
            {
                throw CreateProcessFailure(
                    "Analysis process did not return a response.",
                    errorOutput);
            }

            try
            {
                return JoltRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson)
                    ?? throw new InvalidOperationException("Analysis process returned an invalid RPC response envelope.");
            }
            catch (JsonException exception)
            {
                throw new InvalidOperationException(
                    BuildProcessFailureMessage(
                        "Analysis process returned malformed JSON.",
                        string.IsNullOrWhiteSpace(errorOutput)
                            ? responseJson
                            : errorOutput),
                    exception);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            if (processStarted)
            {
                await ChildProcessUtilities.TerminateProcessAsync(process);
            }

            throw;
        }
        catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested)
        {
            if (processStarted)
            {
                await ChildProcessUtilities.TerminateProcessAsync(process);
            }

            var errorOutput = await AwaitCapturedOutputAsync(errorDrainTask);
            throw new TimeoutException(
                BuildProcessFailureMessage(
                    $"Analysis process '{_command}' timed out after {DefaultRpcTimeout.TotalSeconds:F0}s.",
                    errorOutput));
        }
        catch
        {
            if (processStarted)
            {
                await ChildProcessUtilities.TerminateProcessAsync(process);
            }

            _ = await AwaitCapturedOutputAsync(errorDrainTask);
            throw;
        }
    }

    private static async Task<string?> ReadResponseJsonAsync(
        TextReader output,
        CancellationToken cancellationToken)
    {
        var skippedLineCount = 0;
        var skippedLines = new Queue<string>(MaxCapturedOutputLines);
        while (true)
        {
            var line = await output.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                return null;
            }

            var trimmed = line.Trim();
            if (trimmed.StartsWith("{", StringComparison.Ordinal))
            {
                return trimmed;
            }

            skippedLineCount++;
            AppendCapturedLine(skippedLines, trimmed);
            if (skippedLineCount >= MaxResponseProbeLines)
            {
                throw CreateProcessFailure(
                    $"Analysis process emitted more than {MaxResponseProbeLines} non-JSON stdout lines without returning a response.",
                    string.Join(Environment.NewLine, skippedLines));
            }
        }
    }

    private static async Task<string> DrainErrorOutputAsync(
        StreamReader reader,
        CancellationToken cancellationToken)
    {
        var builder = new StringBuilder();
        while (true)
        {
            string? line;
            try
            {
                line = await reader.ReadLineAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (line is null)
            {
                break;
            }

            AppendBounded(builder, line);
        }

        return builder.ToString().Trim();
    }

    private static async Task<string> AwaitCapturedOutputAsync(Task<string>? outputTask)
    {
        if (outputTask is null)
        {
            return string.Empty;
        }

        try
        {
            return await outputTask;
        }
        catch (OperationCanceledException)
        {
            return string.Empty;
        }
        catch (ObjectDisposedException)
        {
            return string.Empty;
        }
        catch (InvalidOperationException)
        {
            return string.Empty;
        }
    }

    private static void AppendCapturedLine(Queue<string> lines, string line)
    {
        if (lines.Count >= MaxCapturedOutputLines)
        {
            lines.Dequeue();
        }

        lines.Enqueue(line);
    }

    private static void AppendBounded(StringBuilder builder, string line)
    {
        if (builder.Length > 0)
        {
            builder.AppendLine();
        }

        builder.Append(line);
        if (builder.Length <= MaxCapturedErrorChars)
        {
            return;
        }

        builder.Remove(0, builder.Length - MaxCapturedErrorChars);
    }

    private static InvalidOperationException CreateProcessFailure(string message, string capturedOutput)
        => new(BuildProcessFailureMessage(message, capturedOutput));

    private static string BuildProcessFailureMessage(string message, string capturedOutput)
        => string.IsNullOrWhiteSpace(capturedOutput)
            ? message
            : message + Environment.NewLine + capturedOutput;
}
