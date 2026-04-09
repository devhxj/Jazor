using System.Text.Json;
using Jazor.VueHost.Lsp;

namespace Jazor.VueHost.LanguageServers;

internal sealed class TypeScriptServerClient : IAsyncDisposable
{
    private readonly ExternalProcessOptions _options;
    private readonly SemaphoreSlim _startGate = new(1, 1);
    private readonly SemaphoreSlim _writeGate = new(1, 1);
    private readonly object _pendingGate = new();
    private readonly Dictionary<int, TaskCompletionSource<JsonElement?>> _pendingResponses = [];
    private ExternalLspProcess? _process;
    private LspMessageReader? _reader;
    private LspMessageWriter? _writer;
    private int _requestId;

    public TypeScriptServerClient(ExternalProcessOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async ValueTask StartAsync(CancellationToken cancellationToken)
    {
        if (_process is { IsRunning: true })
        {
            return;
        }

        await _startGate.WaitAsync(cancellationToken);
        try
        {
            if (_process is { IsRunning: true })
            {
                return;
            }

            _process = new ExternalLspProcess(_options);
            await _process.StartAsync(cancellationToken);
            _reader = new LspMessageReader(_process.StandardOutput!);
            _writer = new LspMessageWriter(_process.StandardInput!);
            _ = Task.Run(RunReadLoopAsync);
        }
        finally
        {
            _startGate.Release();
        }
    }

    public async ValueTask<JsonElement?> SendRequestAsync(
        string command,
        object? arguments,
        CancellationToken cancellationToken)
    {
        await StartAsync(cancellationToken);
        if (_writer is null)
        {
            throw new InvalidOperationException($"TypeScript server '{_options.Name}' is not ready.");
        }

        var seq = Interlocked.Increment(ref _requestId);
        var pending = new TaskCompletionSource<JsonElement?>(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (_pendingGate)
        {
            _pendingResponses[seq] = pending;
        }

        await _writeGate.WaitAsync(cancellationToken);
        try
        {
            await _writer.WriteMessageAsync(
                JsonSerializer.Serialize(new
                {
                    seq,
                    type = "request",
                    command,
                    arguments
                }),
                cancellationToken);
        }
        finally
        {
            _writeGate.Release();
        }

        using var registration = cancellationToken.Register(static state =>
        {
            var source = (TaskCompletionSource<JsonElement?>)state!;
            source.TrySetCanceled();
        }, pending);

        return await pending.Task;
    }

    private async Task RunReadLoopAsync()
    {
        try
        {
            while (_reader is not null)
            {
                var messageJson = await _reader.ReadMessageAsync(CancellationToken.None);
                if (messageJson is null)
                {
                    break;
                }

                using var message = JsonDocument.Parse(messageJson);
                var root = message.RootElement;
                if (!root.TryGetProperty("type", out var typeElement)
                    || !string.Equals(typeElement.GetString(), "response", StringComparison.Ordinal))
                {
                    continue;
                }

                if (!root.TryGetProperty("request_seq", out var requestSeqElement)
                    || !requestSeqElement.TryGetInt32(out var requestSeq))
                {
                    continue;
                }

                TaskCompletionSource<JsonElement?>? pending = null;
                lock (_pendingGate)
                {
                    if (_pendingResponses.TryGetValue(requestSeq, out pending))
                    {
                        _pendingResponses.Remove(requestSeq);
                    }
                }

                if (pending is null)
                {
                    continue;
                }

                var success = root.TryGetProperty("success", out var successElement)
                    && successElement.ValueKind == JsonValueKind.True;
                if (!success)
                {
                    var messageText = root.TryGetProperty("message", out var errorElement)
                        ? errorElement.GetString()
                        : "Unknown TypeScript server error.";
                    pending.TrySetException(new InvalidOperationException(
                        $"TypeScript server '{_options.Name}' request {requestSeq} failed: {messageText}"));
                    continue;
                }

                JsonElement? body = null;
                if (root.TryGetProperty("body", out var bodyElement))
                {
                    body = bodyElement.Clone();
                }

                pending.TrySetResult(body);
            }
        }
        catch (Exception ex)
        {
            FailPendingRequests(ex);
        }
        finally
        {
            FailPendingRequests(new EndOfStreamException(
                $"TypeScript server '{_options.Name}' closed its stdio stream."));
        }
    }

    private void FailPendingRequests(Exception exception)
    {
        List<TaskCompletionSource<JsonElement?>> pending;
        lock (_pendingGate)
        {
            pending = _pendingResponses.Values.ToList();
            _pendingResponses.Clear();
        }

        foreach (var entry in pending)
        {
            entry.TrySetException(exception);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_process is not null)
        {
            await _process.DisposeAsync();
            _process = null;
        }
    }
}
