using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Jazor.VueHost.Debug;

internal interface ICdpClient : IAsyncDisposable
{
    IReadOnlyList<CdpCallFrame> LatestCallFrames { get; }

    event Action<IReadOnlyList<CdpCallFrame>>? Paused;

    event Action? Resumed;

    Task ContinueAsync(CancellationToken cancellationToken);

    Task<CdpRemoteObject?> EvaluateAsync(
        string expression,
        string? callFrameId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CdpPropertyDescriptor>> GetPropertiesAsync(
        string objectId,
        CancellationToken cancellationToken);

    Task<CdpBreakpointResolution?> SetBreakpointByUrlAsync(
        string generatedUrl,
        int generatedLine,
        int generatedColumn,
        CancellationToken cancellationToken);
}

internal sealed class CdpClient(CdpConnection connection) : ICdpClient
{
    private readonly CdpConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    private readonly ConcurrentDictionary<int, TaskCompletionSource<JsonElement>> _pendingById = [];
    private readonly ConcurrentDictionary<string, string> _scriptUrlById = new(StringComparer.Ordinal);
    private readonly CancellationTokenSource _readLoopCancellation = new();
    private Task? _readLoopTask;
    private int _nextRequestId = 1;
    private IReadOnlyList<CdpCallFrame> _latestCallFrames = [];

    public IReadOnlyList<CdpCallFrame> LatestCallFrames => _latestCallFrames;

    public event Action<IReadOnlyList<CdpCallFrame>>? Paused;

    public event Action? Resumed;

    public async Task ConnectAsync(Uri endpoint, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        await _connection.ConnectAsync(endpoint, cancellationToken);
        _readLoopTask = Task.Run(() => ReadLoopAsync(_readLoopCancellation.Token), CancellationToken.None);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await SendCommandAsync("Debugger.enable", parameters: null, cancellationToken);
        await SendCommandAsync("Runtime.enable", parameters: null, cancellationToken);
    }

    public async Task ContinueAsync(CancellationToken cancellationToken)
    {
        await SendCommandAsync("Debugger.resume", parameters: null, cancellationToken);
    }

    public async Task<CdpRemoteObject?> EvaluateAsync(
        string expression,
        string? callFrameId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);

        JsonElement response = string.IsNullOrWhiteSpace(callFrameId)
            ? await SendCommandAsync(
                "Runtime.evaluate",
                new
                {
                    expression,
                    includeCommandLineAPI = true,
                    returnByValue = false,
                    generatePreview = true,
                    awaitPromise = true
                },
                cancellationToken)
            : await SendCommandAsync(
                "Debugger.evaluateOnCallFrame",
                new
                {
                    callFrameId,
                    expression,
                    includeCommandLineAPI = true,
                    returnByValue = false,
                    generatePreview = true
                },
                cancellationToken);

        if (!TryGetProperty(response, "result", out var result)
            || !TryGetProperty(result, "result", out var remoteObjectElement))
        {
            return null;
        }

        return ParseRemoteObject(remoteObjectElement);
    }

    public async Task<IReadOnlyList<CdpPropertyDescriptor>> GetPropertiesAsync(
        string objectId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectId);

        var response = await SendCommandAsync(
            "Runtime.getProperties",
            new
            {
                objectId,
                ownProperties = true,
                accessorPropertiesOnly = false,
                generatePreview = true
            },
            cancellationToken);

        if (!TryGetProperty(response, "result", out var result)
            || !TryGetProperty(result, "result", out var propertyDescriptorsElement))
        {
            return [];
        }

        return ParsePropertyDescriptors(propertyDescriptorsElement);
    }

    public async Task<CdpBreakpointResolution?> SetBreakpointByUrlAsync(
        string generatedUrl,
        int generatedLine,
        int generatedColumn,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(generatedUrl);

        var response = await SendCommandAsync(
            "Debugger.setBreakpointByUrl",
            new
            {
                url = generatedUrl,
                lineNumber = generatedLine,
                columnNumber = generatedColumn
            },
            cancellationToken);

        var resolution = ParseBreakpointResolution(response, generatedUrl);
        if (resolution is not null)
        {
            return resolution;
        }

        var urlRegex = BuildBreakpointUrlRegex(generatedUrl);
        if (string.IsNullOrWhiteSpace(urlRegex))
        {
            return null;
        }

        var fallbackResponse = await SendCommandAsync(
            "Debugger.setBreakpointByUrl",
            new
            {
                urlRegex,
                lineNumber = generatedLine,
                columnNumber = generatedColumn
            },
            cancellationToken);

        return ParseBreakpointResolution(fallbackResponse, generatedUrl);
    }

    private async Task<JsonElement> SendCommandAsync(
        string method,
        object? parameters,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(method);

        var requestId = Interlocked.Increment(ref _nextRequestId);
        var completion = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingById[requestId] = completion;

        var requestJson = DapProtocolSerializer.Serialize(new
        {
            id = requestId,
            method,
            @params = parameters
        });

        await _connection.SendAsync(requestJson, cancellationToken);

        using var registration = cancellationToken.Register(() => completion.TrySetCanceled(cancellationToken));
        JsonElement response;
        try
        {
            response = await completion.Task;
        }
        finally
        {
            _pendingById.TryRemove(requestId, out _);
        }

        if (TryGetProperty(response, "error", out var errorElement))
        {
            throw new InvalidOperationException(
                $"CDP request '{method}' failed: {errorElement.GetRawText()}");
        }

        return response;
    }

    private async Task ReadLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var payloadJson = await _connection.ReceiveAsync(cancellationToken);
                if (payloadJson is null)
                {
                    break;
                }

                using var payload = JsonDocument.Parse(payloadJson);
                var message = payload.RootElement.Clone();
                if (TryGetProperty(message, "id", out var idElement)
                    && idElement.TryGetInt32(out var responseId)
                    && _pendingById.TryGetValue(responseId, out var completion))
                {
                    completion.TrySetResult(message);
                    continue;
                }

                if (!TryGetProperty(message, "method", out var methodElement)
                    || methodElement.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                HandleEvent(methodElement.GetString() ?? string.Empty, message);
            }
        }
        catch (OperationCanceledException)
        {
            // Shutdown.
        }
        catch (System.Net.WebSockets.WebSocketException exception)
        {
            foreach (var pending in _pendingById.Values)
            {
                pending.TrySetException(exception);
            }
        }
        catch (JsonException exception)
        {
            foreach (var pending in _pendingById.Values)
            {
                pending.TrySetException(exception);
            }
        }
        catch (IOException exception)
        {
            foreach (var pending in _pendingById.Values)
            {
                pending.TrySetException(exception);
            }
        }
        catch (ObjectDisposedException exception)
        {
            foreach (var pending in _pendingById.Values)
            {
                pending.TrySetException(exception);
            }
        }
        catch (InvalidOperationException exception)
        {
            foreach (var pending in _pendingById.Values)
            {
                pending.TrySetException(exception);
            }
        }
    }

    private void HandleEvent(string method, JsonElement message)
    {
        switch (method)
        {
            case "Debugger.scriptParsed":
                TryTrackScriptParsed(message);
                break;
            case "Debugger.paused":
            {
                var callFrames = TryGetProperty(message, "params", out var parameters)
                    ? ParsePausedCallFrames(parameters)
                    : [];
                callFrames = ResolveScriptUrls(callFrames);
                _latestCallFrames = callFrames;
                Paused?.Invoke(callFrames);
                break;
            }
            case "Debugger.resumed":
                _latestCallFrames = [];
                Resumed?.Invoke();
                break;
        }
    }

    private void TryTrackScriptParsed(JsonElement message)
    {
        if (!TryParseScriptParsed(message, out var scriptId, out var scriptUrl))
        {
            return;
        }

        _scriptUrlById[scriptId] = scriptUrl;
    }

    private IReadOnlyList<CdpCallFrame> ResolveScriptUrls(IReadOnlyList<CdpCallFrame> callFrames)
        => ResolveScriptUrls(callFrames, _scriptUrlById);

    internal static IReadOnlyList<CdpCallFrame> ResolveScriptUrls(
        IReadOnlyList<CdpCallFrame> callFrames,
        IReadOnlyDictionary<string, string> scriptUrlById)
    {
        ArgumentNullException.ThrowIfNull(callFrames);
        ArgumentNullException.ThrowIfNull(scriptUrlById);

        if (callFrames.Count == 0)
        {
            return callFrames;
        }

        CdpCallFrame[]? rewrittenFrames = null;
        for (var index = 0; index < callFrames.Count; index++)
        {
            var frame = callFrames[index];
            if (string.IsNullOrWhiteSpace(frame.Location.Url)
                || !scriptUrlById.TryGetValue(frame.Location.Url, out var scriptUrl))
            {
                continue;
            }

            rewrittenFrames ??= callFrames.ToArray();
            rewrittenFrames[index] = frame with
            {
                Location = frame.Location with
                {
                    Url = scriptUrl
                }
            };
        }

        return rewrittenFrames ?? callFrames;
    }

    internal static bool TryParseScriptParsed(
        JsonElement message,
        out string scriptId,
        out string scriptUrl)
    {
        scriptId = string.Empty;
        scriptUrl = string.Empty;

        if (!TryGetProperty(message, "params", out var parameters)
            || !TryGetProperty(parameters, "scriptId", out var scriptIdElement)
            || scriptIdElement.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        var parsedScriptId = scriptIdElement.GetString();
        if (string.IsNullOrWhiteSpace(parsedScriptId))
        {
            return false;
        }

        if (!TryGetProperty(parameters, "url", out var urlElement)
            || urlElement.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        var parsedScriptUrl = urlElement.GetString();
        if (string.IsNullOrWhiteSpace(parsedScriptUrl))
        {
            return false;
        }

        scriptId = parsedScriptId;
        scriptUrl = parsedScriptUrl;
        return true;
    }

    internal static IReadOnlyList<CdpCallFrame> ParsePausedCallFrames(JsonElement parameters)
    {
        if (!TryGetProperty(parameters, "callFrames", out var callFramesElement)
            || callFramesElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var callFrames = new List<CdpCallFrame>();
        foreach (var frameElement in callFramesElement.EnumerateArray())
        {
            var callFrameId = TryGetProperty(frameElement, "callFrameId", out var callFrameIdElement)
                && callFrameIdElement.ValueKind == JsonValueKind.String
                ? callFrameIdElement.GetString() ?? string.Empty
                : string.Empty;
            var functionName = TryGetProperty(frameElement, "functionName", out var functionNameElement)
                && functionNameElement.ValueKind == JsonValueKind.String
                ? functionNameElement.GetString() ?? string.Empty
                : string.Empty;

            if (!TryGetProperty(frameElement, "location", out var locationElement))
            {
                continue;
            }

            var lineNumber = TryGetProperty(locationElement, "lineNumber", out var lineElement)
                && lineElement.TryGetInt32(out var parsedLine)
                ? parsedLine
                : 0;
            var columnNumber = TryGetProperty(locationElement, "columnNumber", out var columnElement)
                && columnElement.TryGetInt32(out var parsedColumn)
                ? parsedColumn
                : 0;

            var locationUrl = TryGetProperty(frameElement, "url", out var urlElement)
                && urlElement.ValueKind == JsonValueKind.String
                ? urlElement.GetString()
                : null;

            if (string.IsNullOrWhiteSpace(locationUrl)
                && TryGetProperty(locationElement, "scriptId", out var scriptIdElement)
                && scriptIdElement.ValueKind == JsonValueKind.String)
            {
                locationUrl = scriptIdElement.GetString();
            }

            var scopeChain = TryGetProperty(frameElement, "scopeChain", out var scopeChainElement)
                ? ParseScopeChain(scopeChainElement)
                : [];

            callFrames.Add(new CdpCallFrame(
                callFrameId,
                functionName,
                new CdpLocation(locationUrl ?? string.Empty, lineNumber, columnNumber),
                scopeChain));
        }

        return callFrames;
    }

    internal static IReadOnlyList<CdpScope> ParseScopeChain(JsonElement scopeChainElement)
    {
        if (scopeChainElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var scopeChain = new List<CdpScope>();
        foreach (var scopeElement in scopeChainElement.EnumerateArray())
        {
            var type = TryGetString(scopeElement, "type") ?? string.Empty;
            if (!TryGetProperty(scopeElement, "object", out var remoteObjectElement))
            {
                continue;
            }

            scopeChain.Add(new CdpScope(
                type,
                TryGetString(scopeElement, "name"),
                ParseRemoteObject(remoteObjectElement)));
        }

        return scopeChain;
    }

    internal static CdpRemoteObject ParseRemoteObject(JsonElement remoteObjectElement)
    {
        var type = TryGetString(remoteObjectElement, "type");
        var subType = TryGetString(remoteObjectElement, "subtype");
        var description = TryGetString(remoteObjectElement, "description");
        var objectId = TryGetString(remoteObjectElement, "objectId");
        var unserializableValue = TryGetString(remoteObjectElement, "unserializableValue");

        string? value = null;
        if (TryGetProperty(remoteObjectElement, "value", out var valueElement))
        {
            value = valueElement.ValueKind switch
            {
                JsonValueKind.String => valueElement.GetString(),
                JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => valueElement.GetRawText(),
                JsonValueKind.Null => "null",
                _ => null
            };
        }

        return new CdpRemoteObject(
            type,
            subType,
            description,
            value,
            unserializableValue,
            objectId);
    }

    internal static IReadOnlyList<CdpPropertyDescriptor> ParsePropertyDescriptors(JsonElement propertyDescriptorsElement)
    {
        if (propertyDescriptorsElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var properties = new List<CdpPropertyDescriptor>();
        foreach (var propertyElement in propertyDescriptorsElement.EnumerateArray())
        {
            var propertyName = TryGetString(propertyElement, "name");
            if (string.IsNullOrWhiteSpace(propertyName)
                || !TryGetProperty(propertyElement, "value", out var propertyValueElement))
            {
                continue;
            }

            properties.Add(new CdpPropertyDescriptor(
                propertyName,
                ParseRemoteObject(propertyValueElement)));
        }

        return properties;
    }

    internal static CdpBreakpointResolution? ParseBreakpointResolution(JsonElement response, string fallbackUrl)
    {
        if (!TryGetProperty(response, "result", out var result)
            || !TryGetProperty(result, "locations", out var locations)
            || locations.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var location in locations.EnumerateArray())
        {
            if (!TryGetProperty(location, "lineNumber", out var lineElement)
                || !lineElement.TryGetInt32(out var lineNumber))
            {
                continue;
            }

            var columnNumber = TryGetProperty(location, "columnNumber", out var columnElement)
                && columnElement.TryGetInt32(out var parsedColumn)
                ? parsedColumn
                : 0;
            var url = TryGetProperty(location, "url", out var urlElement)
                && urlElement.ValueKind == JsonValueKind.String
                ? urlElement.GetString() ?? fallbackUrl
                : fallbackUrl;
            var breakpointId = TryGetProperty(result, "breakpointId", out var breakpointIdElement)
                && breakpointIdElement.ValueKind == JsonValueKind.String
                ? breakpointIdElement.GetString()
                : null;

            return new CdpBreakpointResolution(
                breakpointId,
                new CdpLocation(url, lineNumber, columnNumber));
        }

        return null;
    }

    internal static string? BuildBreakpointUrlRegex(string generatedUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(generatedUrl);

        var normalizedUrl = generatedUrl.Replace('\\', '/');
        if (Uri.TryCreate(normalizedUrl, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            return null;
        }

        var pathOnly = StripQueryAndFragment(normalizedUrl)
            .Trim()
            .TrimStart('.');
        if (string.IsNullOrWhiteSpace(pathOnly))
        {
            return null;
        }

        var suffix = pathOnly.TrimStart('/');
        if (string.IsNullOrWhiteSpace(suffix))
        {
            return null;
        }

        return $"^.*(?:/|^){Regex.Escape(suffix)}(?:[?#].*)?$";
    }

    private static string StripQueryAndFragment(string value)
    {
        var queryIndex = value.IndexOfAny(['?', '#']);
        return queryIndex >= 0
            ? value[..queryIndex]
            : value;
    }

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out value))
        {
            return true;
        }

        value = default;
        return false;
    }

    private static string? TryGetString(JsonElement element, string propertyName)
        => TryGetProperty(element, propertyName, out var value)
            && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    public async ValueTask DisposeAsync()
    {
        _readLoopCancellation.Cancel();
        foreach (var pending in _pendingById.Values)
        {
            pending.TrySetCanceled();
        }

        if (_readLoopTask is not null)
        {
            await _readLoopTask;
        }

        _readLoopCancellation.Dispose();
        await _connection.DisposeAsync();
    }
}

internal sealed record CdpBreakpointResolution(
    string? BreakpointId,
    CdpLocation Location);

internal sealed record CdpRemoteObject(
    string? Type,
    string? SubType,
    string? Description,
    string? Value,
    string? UnserializableValue,
    string? ObjectId);

internal sealed record CdpPropertyDescriptor(
    string Name,
    CdpRemoteObject Value);
