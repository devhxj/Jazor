using System.Text.Json;

namespace Jazor.VueHost.Lsp;

internal sealed class StdioLspServer
{
    private readonly LspSession _session;
    private readonly Lock _requestGate = new();
    private readonly Dictionary<string, CancellationTokenSource> _activeRequests = new(StringComparer.Ordinal);

    public StdioLspServer(LspSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    public async ValueTask RunAsync(
        Stream input,
        Stream output,
        CancellationToken cancellationToken)
    {
        var reader = new LspMessageReader(input);
        var writer = new LspMessageWriter(output);

        try
        {
            while (true)
            {
                var messageJson = await reader.ReadMessageAsync(cancellationToken);
                if (messageJson is null)
                {
                    break;
                }

                var request = LspJsonSerializer.Deserialize<LspRequestMessage>(messageJson);
                if (request is null || string.IsNullOrWhiteSpace(request.Method))
                {
                    continue;
                }

                if (string.Equals(request.Method, "$/cancelRequest", StringComparison.Ordinal))
                {
                    CancelRequest(request.Params);
                    continue;
                }

                if (request.Id is null)
                {
                    var shouldContinue = await _session.HandleNotificationAsync(request, cancellationToken);
                    if (!shouldContinue)
                    {
                        break;
                    }

                    continue;
                }

                await HandleRequestAsync(request, writer, cancellationToken);
            }
        }
        finally
        {
            CancelAndDisposeActiveRequests();
        }
    }

    private async ValueTask HandleRequestAsync(
        LspRequestMessage request,
        LspMessageWriter writer,
        CancellationToken sessionCancellationToken)
    {
        using var requestCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(sessionCancellationToken);
        var requestKey = CreateRequestKey(request.Id);
        if (requestKey is not null)
        {
            lock (_requestGate)
            {
                if (_activeRequests.TryGetValue(requestKey, out var existing))
                {
                    existing.Cancel();
                    existing.Dispose();
                }

                _activeRequests[requestKey] = requestCancellationSource;
            }
        }

        LspResponseMessage? response;
        try
        {
            response = await _session.HandleRequestAsync(request, requestCancellationSource.Token);
        }
        catch (OperationCanceledException) when (requestCancellationSource.IsCancellationRequested)
        {
            response = new LspResponseMessage
            {
                Id = request.Id,
                Error = new LspResponseError
                {
                    Code = -32800,
                    Message = "Request cancelled."
                }
            };
        }
        catch (Exception ex)
        {
            response = new LspResponseMessage
            {
                Id = request.Id,
                Error = new LspResponseError
                {
                    Code = -32603,
                    Message = ex.Message
                }
            };
        }
        finally
        {
            if (requestKey is not null)
            {
                lock (_requestGate)
                {
                    _activeRequests.Remove(requestKey);
                }
            }
        }

        if (response is null)
        {
            return;
        }

        try
        {
            await writer.WriteMessageAsync(
                LspJsonSerializer.Serialize(response),
                CancellationToken.None);
        }
        catch
        {
            // Output stream can be closed during shutdown; suppress late write failures.
        }
    }

    private void CancelRequest(object? payload)
    {
        var requestId = TryExtractCancelRequestId(payload);
        var requestKey = CreateRequestKey(requestId);
        if (requestKey is null)
        {
            return;
        }

        lock (_requestGate)
        {
            if (_activeRequests.TryGetValue(requestKey, out var cancellationTokenSource))
            {
                cancellationTokenSource.Cancel();
            }
        }
    }

    private void CancelAndDisposeActiveRequests()
    {
        CancellationTokenSource[] activeSources;
        lock (_requestGate)
        {
            activeSources = _activeRequests.Values.ToArray();
            _activeRequests.Clear();
        }

        foreach (var source in activeSources)
        {
            try
            {
                source.Cancel();
            }
            catch
            {
            }
        }

        foreach (var source in activeSources)
        {
            source.Dispose();
        }
    }

    private static object? TryExtractCancelRequestId(object? payload)
    {
        if (payload is null)
        {
            return null;
        }

        if (payload is LspCancelRequestParams typed)
        {
            return typed.Id;
        }

        if (payload is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object
                && element.TryGetProperty("id", out var idProperty))
            {
                return idProperty.Clone();
            }

            return null;
        }

        try
        {
            var raw = LspJsonSerializer.Serialize(payload);
            var deserialized = LspJsonSerializer.Deserialize<LspCancelRequestParams>(raw);
            return deserialized?.Id;
        }
        catch
        {
            return null;
        }
    }

    private static string? CreateRequestKey(object? id)
    {
        if (id is null)
        {
            return null;
        }

        if (id is JsonElement jsonElement)
        {
            return jsonElement.ValueKind switch
            {
                JsonValueKind.String => "s:" + (jsonElement.GetString() ?? string.Empty),
                JsonValueKind.Number => "n:" + jsonElement.GetRawText(),
                _ => "j:" + jsonElement.GetRawText()
            };
        }

        if (id is string text)
        {
            return "s:" + text;
        }

        return "o:" + id.ToString();
    }
}
