using System.Globalization;
using System.Text.Json;
using Jazor.VueHost.Lsp;

namespace Jazor.VueHost.LanguageServers;

internal sealed class ExternalLspClient : IAsyncDisposable
{
    private readonly ExternalProcessOptions _options;
    private readonly IReadOnlyList<ILspServerNotificationHandler> _notificationHandlers;
    private readonly SemaphoreSlim _startGate = new(1, 1);
    private readonly SemaphoreSlim _writeGate = new(1, 1);
    private readonly object _pendingGate = new();
    private readonly Dictionary<string, TaskCompletionSource<LspResponseMessage>> _pendingResponses =
        new(StringComparer.Ordinal);
    private ExternalLspProcess? _process;
    private LspMessageReader? _reader;
    private LspMessageWriter? _writer;
    private Task? _readLoop;
    private int _requestId;
    private bool _initialized;

    public ExternalLspClient(
        ExternalProcessOptions options,
        IReadOnlyList<ILspServerNotificationHandler>? notificationHandlers = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _notificationHandlers = notificationHandlers ?? [];
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
            _readLoop = Task.Run(RunReadLoopAsync);
        }
        finally
        {
            _startGate.Release();
        }
    }

    public async ValueTask<ExternalLspInitializeResult?> InitializeAsync(
        string? rootPath,
        CancellationToken cancellationToken)
    {
        await StartAsync(cancellationToken);
        if (_initialized)
        {
            return null;
        }

        var initializeResult = await SendRequestAsync<JsonElement?>(
            "initialize",
            new
            {
                processId = Environment.ProcessId,
                clientInfo = new
                {
                    name = "Jazor.VueHost",
                    version = "0.1"
                },
                rootUri = string.IsNullOrWhiteSpace(rootPath)
                    ? null
                    : new Uri(Path.GetFullPath(rootPath)).AbsoluteUri,
                capabilities = new
                {
                    general = new
                    {
                        positionEncodings = new[] { "utf-16" }
                    },
                    workspace = new
                    {
                        applyEdit = true,
                        workspaceFolders = true
                    },
                    textDocument = new
                    {
                        publishDiagnostics = new
                        {
                            relatedInformation = true
                        }
                    }
                }
            },
            cancellationToken);

        await SendNotificationAsync("initialized", new { }, cancellationToken);
        _initialized = true;
        if (initializeResult is null || initializeResult.Value.ValueKind != JsonValueKind.Object)
        {
            return new ExternalLspInitializeResult(null, null);
        }

        string? serverName = null;
        string? serverVersion = null;
        var resultElement = initializeResult.Value;
        if (resultElement.TryGetProperty("serverInfo", out var serverInfo)
            && serverInfo.ValueKind == JsonValueKind.Object)
        {
            if (serverInfo.TryGetProperty("name", out var nameElement))
            {
                serverName = nameElement.GetString();
            }

            if (serverInfo.TryGetProperty("version", out var versionElement))
            {
                serverVersion = versionElement.GetString();
            }
        }

        return new ExternalLspInitializeResult(serverName, serverVersion);
    }

    public async ValueTask<TResult?> SendRequestAsync<TResult>(
        string method,
        object? parameters,
        CancellationToken cancellationToken)
    {
        await StartAsync(cancellationToken);

        var id = Interlocked.Increment(ref _requestId).ToString(CultureInfo.InvariantCulture);
        var pending = new TaskCompletionSource<LspResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (_pendingGate)
        {
            _pendingResponses[id] = pending;
        }

        await WriteMessageAsync(
            new LspRequestMessage
            {
                Id = id,
                Method = method,
                Params = parameters
            },
            cancellationToken);

        using var registration = cancellationToken.Register(static state =>
        {
            var source = (TaskCompletionSource<LspResponseMessage>)state!;
            source.TrySetCanceled();
        }, pending);

        var response = await pending.Task;
        if (response.Error is not null)
        {
            throw new InvalidOperationException(
                $"External language server '{_options.Name}' returned LSP error {response.Error.Code}: {response.Error.Message}");
        }

        return DeserializePayload<TResult>(response.Result);
    }

    public async ValueTask SendNotificationAsync(
        string method,
        object? parameters,
        CancellationToken cancellationToken)
    {
        await StartAsync(cancellationToken);
        await WriteMessageAsync(
            new LspNotificationMessage
            {
                Method = method,
                Params = parameters
            },
            cancellationToken);
    }

    private async Task WriteMessageAsync<TMessage>(TMessage payload, CancellationToken cancellationToken)
    {
        if (_writer is null)
        {
            throw new InvalidOperationException($"External language server '{_options.Name}' is not ready.");
        }

        await _writeGate.WaitAsync(cancellationToken);
        try
        {
            await _writer.WriteMessageAsync(LspJsonSerializer.Serialize(payload), cancellationToken);
        }
        finally
        {
            _writeGate.Release();
        }
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

                var request = LspJsonSerializer.Deserialize<LspRequestMessage>(messageJson);
                if (request is not null && !string.IsNullOrWhiteSpace(request.Method))
                {
                    if (request.Id is not null)
                    {
                        await WriteMessageAsync(
                            new LspResponseMessage
                            {
                                Id = request.Id,
                                Error = new LspResponseError
                                {
                                    Code = -32601,
                                    Message = $"Jazor.VueHost external client does not handle server request '{request.Method}'."
                                }
                            },
                            CancellationToken.None);
                    }
                    else
                    {
                        foreach (var handler in _notificationHandlers)
                        {
                            if (await handler.HandleNotificationAsync(request.Method, request.Params, CancellationToken.None))
                            {
                                break;
                            }
                        }
                    }

                    continue;
                }

                var response = LspJsonSerializer.Deserialize<LspResponseMessage>(messageJson);
                if (response is null)
                {
                    continue;
                }

                var key = GetMessageIdKey(response.Id);
                TaskCompletionSource<LspResponseMessage>? pending = null;
                lock (_pendingGate)
                {
                    if (_pendingResponses.TryGetValue(key, out pending))
                    {
                        _pendingResponses.Remove(key);
                    }
                }

                pending?.TrySetResult(response);
            }
        }
        catch (Exception ex)
        {
            FailPendingRequests(ex);
        }
        finally
        {
            FailPendingRequests(new EndOfStreamException(
                $"External language server '{_options.Name}' closed its stdio stream."));
        }
    }

    private void FailPendingRequests(Exception exception)
    {
        List<TaskCompletionSource<LspResponseMessage>> pending;
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

    private static TResult? DeserializePayload<TResult>(object? payload)
    {
        if (payload is null)
        {
            return default;
        }

        if (payload is TResult typed)
        {
            return typed;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        if (payload is JsonElement element)
        {
            return JsonSerializer.Deserialize<TResult>(element.GetRawText(), options);
        }

        return JsonSerializer.Deserialize<TResult>(JsonSerializer.Serialize(payload), options);
    }

    private static string GetMessageIdKey(object? id)
        => id switch
        {
            null => string.Empty,
            string text => text,
            int number => number.ToString(CultureInfo.InvariantCulture),
            long number => number.ToString(CultureInfo.InvariantCulture),
            JsonElement element when element.ValueKind == JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonElement element when element.ValueKind == JsonValueKind.Number => element.GetRawText(),
            JsonElement element => element.GetRawText(),
            _ => Convert.ToString(id, CultureInfo.InvariantCulture) ?? string.Empty
        };

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_initialized)
            {
                try
                {
                    await SendRequestAsync<object?>("shutdown", parameters: null, CancellationToken.None);
                }
                catch
                {
                }

                try
                {
                    await SendNotificationAsync("exit", parameters: null, CancellationToken.None);
                }
                catch
                {
                }
            }
        }
        finally
        {
            if (_process is not null)
            {
                await _process.DisposeAsync();
                _process = null;
            }
        }
    }
}

internal sealed record ExternalLspInitializeResult(
    string? Name,
    string? Version);
