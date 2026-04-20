using System.Diagnostics;
using Jazor.VueContracts.Protocol;
using Jolt.Rpc;

namespace Jolt.Analysis;

public sealed class ProcessAnalysisRpcTransport : IAnalysisRpcTransport
{
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

        if (!process.Start())
            throw new InvalidOperationException($"Failed to start analysis process '{_command}'.");

        var requestJson = JoltRpcSerializer.Serialize(request);
        await process.StandardInput.WriteLineAsync(requestJson.AsMemory(), cancellationToken);
        await process.StandardInput.FlushAsync();
        process.StandardInput.Close();

        var responseJson = await ReadResponseJsonAsync(process, cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(responseJson))
        {
            var errorOutput = await process.StandardError.ReadToEndAsync(cancellationToken);
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(errorOutput)
                    ? "Analysis process did not return a response."
                    : errorOutput.Trim());
        }

        return JoltRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson)
            ?? throw new InvalidOperationException("Analysis process returned an invalid RPC response envelope.");
    }

    private static async Task<string?> ReadResponseJsonAsync(
        Process process,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            var line = await process.StandardOutput.ReadLineAsync(cancellationToken);
            if (line is null)
                return null;

            var trimmed = line.Trim();
            if (trimmed.StartsWith("{", StringComparison.Ordinal))
                return trimmed;
        }
    }
}
