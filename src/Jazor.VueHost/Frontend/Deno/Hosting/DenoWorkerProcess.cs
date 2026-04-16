using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Jazor.VueHost.Frontend.Deno.Protocol;

namespace Jazor.VueHost.Frontend.Deno.Hosting;

internal sealed class DenoWorkerProcess : IDenoWorkerProcess
{
    private readonly DenoVolarHostOptions _options;
    private readonly SemaphoreSlim _requestGate = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };
    private Process? _process;
    private StreamWriter? _writer;
    private StreamReader? _reader;

    public DenoWorkerProcess(DenoVolarHostOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public bool IsRunning => _process is { HasExited: false };

    public ValueTask StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (IsRunning)
        {
            return ValueTask.CompletedTask;
        }

        if (string.IsNullOrWhiteSpace(_options.ExecutablePath))
        {
            throw new InvalidOperationException("No Deno runtime path was configured for the VueHost Volar worker.");
        }

        if (!_options.HasExplicitExecutableOverride &&
            Path.IsPathRooted(_options.ExecutablePath) &&
            !File.Exists(_options.ExecutablePath))
        {
            throw new InvalidOperationException(
                DenoRuntimeAssetResolver.CreateMissingRuntimeMessage(_options.ExecutablePath));
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = _options.ExecutablePath,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardErrorEncoding = Encoding.UTF8,
            StandardOutputEncoding = Encoding.UTF8
        };

        foreach (var argument in _options.Arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        if (!string.IsNullOrWhiteSpace(_options.WorkingDirectory))
        {
            startInfo.WorkingDirectory = _options.WorkingDirectory;
        }

        if (!string.IsNullOrWhiteSpace(_options.CacheDirectory))
        {
            Directory.CreateDirectory(_options.CacheDirectory);
            startInfo.Environment["DENO_DIR"] = _options.CacheDirectory;
        }

        _process = new Process
        {
            StartInfo = startInfo
        };

        try
        {
            if (!_process.Start())
            {
                throw new InvalidOperationException($"Failed to start Deno Volar worker '{_options.ExecutablePath}'.");
            }
        }
        catch (Win32Exception ex) when (!_options.HasExplicitExecutableOverride)
        {
            throw new InvalidOperationException(
                DenoRuntimeAssetResolver.CreateMissingRuntimeMessage(_options.ExecutablePath),
                ex);
        }

        _writer = new StreamWriter(_process.StandardInput.BaseStream, new UTF8Encoding(false))
        {
            AutoFlush = true,
            NewLine = "\n"
        };
        _reader = new StreamReader(_process.StandardOutput.BaseStream, Encoding.UTF8);
        return ValueTask.CompletedTask;
    }

    public async ValueTask<TResult?> SendRequestAsync<TResult>(
        string method,
        object payload,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(method);
        ArgumentNullException.ThrowIfNull(payload);
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsRunning || _writer is null || _reader is null)
        {
            return default;
        }

        await _requestGate.WaitAsync(cancellationToken);
        try
        {
            if (!IsRunning || _writer is null || _reader is null)
            {
                return default;
            }

            var request = new DenoFrontendRequestEnvelope
            {
                Id = Guid.NewGuid().ToString("N"),
                Method = method,
                Payload = payload
            };

            await _writer.WriteLineAsync(JsonSerializer.Serialize(request, _jsonOptions));
            var responseLine = await _reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(responseLine))
            {
                throw new InvalidOperationException(
                    $"Deno frontend worker returned no response for request '{method}'.");
            }

            var response = JsonSerializer.Deserialize<DenoFrontendResponseEnvelope>(responseLine, _jsonOptions);
            if (response is null)
            {
                throw new InvalidOperationException(
                    $"Deno frontend worker returned an invalid response for request '{method}'.");
            }

            if (!response.Success)
            {
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(response.Error)
                        ? $"Deno frontend worker request '{method}' failed."
                        : $"Deno frontend worker request '{method}' failed: {response.Error}");
            }

            if (response.Result is null)
            {
                return default;
            }

            return response.Result.Value.Deserialize<TResult>(_jsonOptions);
        }
        finally
        {
            _requestGate.Release();
        }
    }

    public async ValueTask StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_process is null)
        {
            return;
        }

        try
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
                await _process.WaitForExitAsync(cancellationToken);
            }
        }
        finally
        {
            _writer?.Dispose();
            _writer = null;
            _reader?.Dispose();
            _reader = null;
            _process.Dispose();
            _process = null;
        }
    }
}
