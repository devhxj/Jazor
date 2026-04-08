using System.Diagnostics;
using System.Text;
using Jazor.VueContracts.Protocol;

namespace Jazor.Vite.VueHost;

public sealed class ProcessVueHostRpcClient : IDisposable, IAsyncDisposable
{
    private readonly string _command;
    private readonly string? _arguments;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly List<string> _stderrLines = [];
    private Process? _process;
    private bool _disposed;

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

    public async ValueTask OpenDocumentAsync(
        DocumentSnapshot documentSnapshot,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(documentSnapshot);
        await SendWithoutPayloadAsync(VueHostRpcMethodNames.OpenDocument, documentSnapshot, cancellationToken);
    }

    public async ValueTask UpdateDocumentAsync(
        DocumentSnapshot documentSnapshot,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(documentSnapshot);
        await SendWithoutPayloadAsync(VueHostRpcMethodNames.UpdateDocument, documentSnapshot, cancellationToken);
    }

    public async ValueTask CloseDocumentAsync(
        string documentPath,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentPath);
        await SendWithoutPayloadAsync(VueHostRpcMethodNames.CloseDocument, documentPath, cancellationToken);
    }

    public async ValueTask<IReadOnlyList<DocumentSnapshot>> GetOpenDocumentsAsync(CancellationToken cancellationToken)
        => await SendAsync<IReadOnlyList<DocumentSnapshot>>(VueHostRpcMethodNames.GetOpenDocuments, payload: null, cancellationToken);

    public async ValueTask<GetVirtualArtifactResponse> GetVirtualArtifactAsync(
        GetVirtualArtifactRequest request,
        CancellationToken cancellationToken)
        => await SendAsync<GetVirtualArtifactResponse>(VueHostRpcMethodNames.GetVirtualArtifact, request, cancellationToken);

    public async ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await SendAsync<AnalyzeJazorResponse>(VueHostRpcMethodNames.AnalyzeJazor, request, cancellationToken);
    }

    public int? ProcessId
        => _process?.HasExited == false ? _process.Id : null;

    public void Dispose()
    {
        DisposeCore();
        GC.SuppressFinalize(this);
    }

    public ValueTask DisposeAsync()
    {
        DisposeCore();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    private async ValueTask SendWithoutPayloadAsync(
        string method,
        object payload,
        CancellationToken cancellationToken)
    {
        await SendAsync(
            method,
            payload,
            static response =>
            {
                if (!string.IsNullOrWhiteSpace(response.PayloadJson))
                {
                    throw new InvalidOperationException("Expected empty payload from VueHost RPC method.");
                }

                return true;
            },
            cancellationToken);
    }

    private async ValueTask<T> SendAsync<T>(
        string method,
        object? payload,
        CancellationToken cancellationToken)
    {
        var response = await SendAsync(
            method,
            payload,
            responseEnvelope =>
            {
                if (string.IsNullOrWhiteSpace(responseEnvelope.PayloadJson))
                    throw new InvalidOperationException($"VueHost RPC method '{method}' returned an empty payload.");

                return ProtocolJsonSerializer.Deserialize<T>(responseEnvelope.PayloadJson)
                    ?? throw new InvalidOperationException($"VueHost RPC payload for '{method}' could not be deserialized.");
            },
            cancellationToken);

        return response;
    }

    private async ValueTask<T> SendAsync<T>(
        string method,
        object? payload,
        Func<RpcResponseEnvelope, T> payloadFactory,
        CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var process = EnsureStarted();
            var request = new RpcRequestEnvelope(
                id: Guid.NewGuid().ToString("N"),
                method: method,
                payloadJson: payload is null ? null : ProtocolJsonSerializer.Serialize(payload));
            var requestJson = ProtocolJsonSerializer.Serialize(request);

            await process.StandardInput.WriteLineAsync(requestJson.AsMemory(), cancellationToken);
            await process.StandardInput.FlushAsync();

            var responseJson = await ReadResponseLineAsync(process, cancellationToken);
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

            return payloadFactory(response);
        }
        catch when (_process is { HasExited: true })
        {
            DisposeProcess();
            throw CreateProcessExitedException();
        }
        finally
        {
            _gate.Release();
        }
    }

    private Process EnsureStarted()
    {
        if (_process is { HasExited: false })
        {
            return _process;
        }

        DisposeProcess();
        _stderrLines.Clear();

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _command,
                Arguments = _arguments ?? string.Empty,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardErrorEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8
            },
            EnableRaisingEvents = true
        };
        process.ErrorDataReceived += OnProcessErrorDataReceived;
        process.Exited += OnProcessExited;

        if (!process.Start())
        {
            process.ErrorDataReceived -= OnProcessErrorDataReceived;
            process.Exited -= OnProcessExited;
            process.Dispose();
            throw new InvalidOperationException($"Failed to start VueHost process '{_command}'.");
        }

        process.BeginErrorReadLine();
        _process = process;
        return process;
    }

    private async Task<string> ReadResponseLineAsync(Process process, CancellationToken cancellationToken)
    {
        while (true)
        {
            var line = await process.StandardOutput.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                throw CreateProcessExitedException();
            }

            var trimmed = line.Trim();
            if (trimmed.StartsWith("{", StringComparison.Ordinal))
            {
                return trimmed;
            }
        }
    }

    private Exception CreateProcessExitedException()
    {
        var details = _stderrLines.Count == 0
            ? "VueHost process exited unexpectedly."
            : $"VueHost process exited unexpectedly. {string.Join(" | ", _stderrLines)}";
        return new InvalidOperationException(details);
    }

    private void OnProcessErrorDataReceived(object sender, DataReceivedEventArgs eventArgs)
    {
        if (string.IsNullOrWhiteSpace(eventArgs.Data))
        {
            return;
        }

        _stderrLines.Add(eventArgs.Data.Trim());
        if (_stderrLines.Count > 50)
        {
            _stderrLines.RemoveAt(0);
        }
    }

    private void OnProcessExited(object? sender, EventArgs eventArgs)
    {
        if (!ReferenceEquals(sender, _process))
        {
            return;
        }

        DisposeProcess();
    }

    private void DisposeCore()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        DisposeProcess();
        _gate.Dispose();
    }

    private void DisposeProcess()
    {
        if (_process is null)
        {
            return;
        }

        var process = _process;
        _process = null;
        process.ErrorDataReceived -= OnProcessErrorDataReceived;
        process.Exited -= OnProcessExited;

        try
        {
            if (!process.HasExited)
            {
                try
                {
                    process.StandardInput.Close();
                }
                catch
                {
                    // Ignore teardown on closed pipes.
                }

                process.Kill(entireProcessTree: true);
                process.WaitForExit(250);
            }
        }
        catch
        {
            // Ignore shutdown races.
        }
        finally
        {
            process.Dispose();
        }
    }
}
