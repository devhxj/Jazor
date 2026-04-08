using System.Diagnostics;
using Jazor.VueContracts.Protocol;

namespace Jazor.Vite.VueHost;

public sealed class ProcessVueHostRpcClient
{
    private readonly string _command;
    private readonly string? _arguments;

    public ProcessVueHostRpcClient(string command, string? arguments)
    {
        _command = string.IsNullOrWhiteSpace(command)
            ? throw new ArgumentException("VueHost command must be provided.", nameof(command))
            : command;
        _arguments = arguments;
    }

    public async ValueTask<PingResponse> PingAsync(CancellationToken cancellationToken)
        => await SendAsync<PingResponse>(VueHostRpcMethodNames.Ping, payload: null, cancellationToken);

    public async ValueTask<GetHostInfoResponse> GetHostInfoAsync(CancellationToken cancellationToken)
        => await SendAsync<GetHostInfoResponse>(VueHostRpcMethodNames.GetHostInfo, payload: null, cancellationToken);

    private async ValueTask<T> SendAsync<T>(
        string method,
        object? payload,
        CancellationToken cancellationToken)
    {
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
            throw new InvalidOperationException($"Failed to start VueHost process '{_command}'.");

        var request = new RpcRequestEnvelope(
            id: Guid.NewGuid().ToString("N"),
            method: method,
            payloadJson: payload is null ? null : ProtocolJsonSerializer.Serialize(payload));
        var requestJson = ProtocolJsonSerializer.Serialize(request);

        await process.StandardInput.WriteLineAsync(requestJson.AsMemory(), cancellationToken);
        await process.StandardInput.FlushAsync();
        process.StandardInput.Close();

        var responseJson = await process.StandardOutput.ReadLineAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(responseJson))
        {
            var errorOutput = await process.StandardError.ReadToEndAsync(cancellationToken);
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(errorOutput)
                    ? "VueHost process did not return a response."
                    : errorOutput.Trim());
        }

        var response = ProtocolJsonSerializer.Deserialize<RpcResponseEnvelope>(responseJson)
            ?? throw new InvalidOperationException("VueHost process returned an invalid RPC response envelope.");

        if (!response.Success)
        {
            var code = response.Error?.Code;
            var message = response.Error?.Message ?? "VueHost RPC call failed.";
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(code)
                    ? message
                    : code + ": " + message);
        }

        if (string.IsNullOrWhiteSpace(response.PayloadJson))
            throw new InvalidOperationException($"VueHost RPC method '{method}' returned an empty payload.");

        return ProtocolJsonSerializer.Deserialize<T>(response.PayloadJson)
            ?? throw new InvalidOperationException($"VueHost RPC payload for '{method}' could not be deserialized.");
    }
}
