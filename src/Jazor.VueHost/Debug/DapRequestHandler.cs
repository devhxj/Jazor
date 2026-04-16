using System.Text.Json;

namespace Jazor.VueHost.Debug;

internal sealed class DapRequestHandler(
    DapSession session,
    BreakpointManager breakpointManager,
    CallStackMapper callStackMapper)
{
    private const int MainThreadId = 1;

    private readonly DapSession _session = session ?? throw new ArgumentNullException(nameof(session));
    private readonly BreakpointManager _breakpointManager = breakpointManager ?? throw new ArgumentNullException(nameof(breakpointManager));
    private readonly CallStackMapper _callStackMapper = callStackMapper ?? throw new ArgumentNullException(nameof(callStackMapper));
    private int _nextProtocolSeq = 1;
    private int _nextBreakpointId = 1;

    public ValueTask<DapDispatchResult> HandleAsync(DapRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        return request.Command switch
        {
            "initialize" => ValueTask.FromResult(HandleInitialize(request)),
            "launch" => ValueTask.FromResult(HandleSimpleSuccess(request)),
            "attach" => ValueTask.FromResult(HandleSimpleSuccess(request)),
            "configurationDone" => ValueTask.FromResult(HandleConfigurationDone(request)),
            "setExceptionBreakpoints" => ValueTask.FromResult(HandleSimpleSuccess(request)),
            "threads" => ValueTask.FromResult(HandleThreads(request)),
            "setBreakpoints" => ValueTask.FromResult(HandleSetBreakpoints(request)),
            "stackTrace" => ValueTask.FromResult(HandleStackTrace(request)),
            "scopes" => ValueTask.FromResult(HandleScopes(request)),
            "variables" => ValueTask.FromResult(HandleVariables(request)),
            "continue" => ValueTask.FromResult(HandleContinue(request)),
            "evaluate" => ValueTask.FromResult(HandleEvaluate(request)),
            "disconnect" => ValueTask.FromResult(HandleDisconnect(request)),
            _ => ValueTask.FromResult(HandleUnsupported(request))
        };
    }

    private DapDispatchResult HandleInitialize(DapRequest request)
    {
        _session.IsInitialized = true;
        return new DapDispatchResult
        {
            Response = CreateSuccessResponse(
                request,
                new
                {
                    supportsConfigurationDoneRequest = true,
                    supportsFunctionBreakpoints = false,
                    supportsConditionalBreakpoints = false,
                    supportsHitConditionalBreakpoints = false,
                    supportsEvaluateForHovers = true,
                    supportsRestartRequest = false,
                    supportsTerminateRequest = false
                }),
            Events =
            [
                CreateEvent("initialized")
            ]
        };
    }

    private DapDispatchResult HandleConfigurationDone(DapRequest request)
    {
        _session.IsStarted = true;
        return new DapDispatchResult
        {
            Response = CreateSuccessResponse(request)
        };
    }

    private DapDispatchResult HandleSimpleSuccess(DapRequest request)
        => new()
        {
            Response = CreateSuccessResponse(request)
        };

    private DapDispatchResult HandleThreads(DapRequest request)
        => new()
        {
            Response = CreateSuccessResponse(
                request,
                new
                {
                    threads = new[]
                    {
                        new DapThread
                        {
                            Id = MainThreadId,
                            Name = "main"
                        }
                    }
                })
        };

    private DapDispatchResult HandleSetBreakpoints(DapRequest request)
    {
        var arguments = request.Arguments;
        var sourcePath = TryGetString(arguments, "source", "path");
        var bindings = new List<DapBreakpointBinding>();
        var breakpoints = new List<DapBreakpoint>();

        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return new DapDispatchResult
            {
                Response = CreateSuccessResponse(request, new { breakpoints })
            };
        }

        if (TryGetProperty(arguments, "breakpoints", out var breakpointsElement)
            && breakpointsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var breakpointElement in breakpointsElement.EnumerateArray())
            {
                var sourceLine = TryGetInt32(breakpointElement, "line") ?? 1;
                var sourceColumn = TryGetInt32(breakpointElement, "column") ?? 1;
                var breakpointId = NextBreakpointId();
                var mapped = _breakpointManager.MapBreakpoint(
                    sourcePath,
                    Math.Max(sourceLine - 1, 0),
                    Math.Max(sourceColumn - 1, 0));

                bindings.Add(new DapBreakpointBinding(
                    breakpointId,
                    sourcePath,
                    sourceLine,
                    sourceColumn,
                    mapped));

                breakpoints.Add(new DapBreakpoint
                {
                    Id = breakpointId,
                    Verified = mapped is not null,
                    Line = sourceLine,
                    Column = sourceColumn,
                    Message = mapped is null
                        ? "Source position could not be mapped to a generated module."
                        : null
                });
            }
        }

        _session.SetBreakpoints(sourcePath, bindings);

        return new DapDispatchResult
        {
            Response = CreateSuccessResponse(
                request,
                new
                {
                    breakpoints
                })
        };
    }

    private DapDispatchResult HandleStackTrace(DapRequest request)
    {
        var arguments = request.Arguments;
        var startFrame = Math.Max(TryGetInt32(arguments, "startFrame") ?? 0, 0);
        var levels = TryGetInt32(arguments, "levels");
        var mappedFrames = _callStackMapper.MapCallStack(_session.CurrentCallFrames);
        var stackFrames = mappedFrames
            .Skip(startFrame)
            .Take(levels is > 0 ? levels.Value : mappedFrames.Count)
            .ToArray();

        return new DapDispatchResult
        {
            Response = CreateSuccessResponse(
                request,
                new
                {
                    stackFrames,
                    totalFrames = mappedFrames.Count
                })
        };
    }

    private DapDispatchResult HandleScopes(DapRequest request)
    {
        var frameId = TryGetInt32(request.Arguments, "frameId") ?? 0;
        var scopes = _session.CreateScopes(frameId);

        return new DapDispatchResult
        {
            Response = CreateSuccessResponse(
                request,
                new
                {
                    scopes
                })
        };
    }

    private DapDispatchResult HandleVariables(DapRequest request)
    {
        var variablesReference = TryGetInt32(request.Arguments, "variablesReference") ?? 0;
        var variables = _session.GetVariables(variablesReference);

        return new DapDispatchResult
        {
            Response = CreateSuccessResponse(
                request,
                new
                {
                    variables
                })
        };
    }

    private DapDispatchResult HandleContinue(DapRequest request)
    {
        var threadId = Math.Max(TryGetInt32(request.Arguments, "threadId") ?? MainThreadId, MainThreadId);
        _session.ContinueExecution();

        return new DapDispatchResult
        {
            Response = CreateSuccessResponse(
                request,
                new
                {
                    allThreadsContinued = true
                }),
            Events =
            [
                CreateEvent(
                    "continued",
                    new
                    {
                        threadId,
                        allThreadsContinued = true
                    })
            ]
        };
    }

    private DapDispatchResult HandleEvaluate(DapRequest request)
    {
        var expression = TryGetString(request.Arguments, "expression");
        var frameId = TryGetInt32(request.Arguments, "frameId");
        var context = TryGetString(request.Arguments, "context");
        var evaluation = _session.Evaluate(expression, frameId, context);

        return new DapDispatchResult
        {
            Response = CreateSuccessResponse(
                request,
                new
                {
                    result = evaluation.Result,
                    type = evaluation.Type,
                    variablesReference = evaluation.VariablesReference
                })
        };
    }

    private DapDispatchResult HandleDisconnect(DapRequest request)
    {
        _session.Clear();
        return new DapDispatchResult
        {
            Response = CreateSuccessResponse(request),
            ShouldTerminate = true
        };
    }

    private DapDispatchResult HandleUnsupported(DapRequest request)
        => new()
        {
            Response = new DapResponse
            {
                Seq = NextProtocolSeq(),
                RequestSeq = request.Seq,
                Command = request.Command,
                Success = false,
                Message = $"Unsupported DAP request '{request.Command}'."
            }
        };

    private DapResponse CreateSuccessResponse(DapRequest request, object? body = null)
        => new()
        {
            Seq = NextProtocolSeq(),
            RequestSeq = request.Seq,
            Command = request.Command,
            Success = true,
            Body = body
        };

    private DapEvent CreateEvent(string eventName, object? body = null)
        => new()
        {
            Seq = NextProtocolSeq(),
            Event = eventName,
            Body = body
        };

    private int NextProtocolSeq() => _nextProtocolSeq++;

    private int NextBreakpointId() => _nextBreakpointId++;

    private static bool TryGetProperty(JsonElement? element, string propertyName, out JsonElement value)
    {
        if (element is { } jsonElement
            && jsonElement.ValueKind == JsonValueKind.Object
            && jsonElement.TryGetProperty(propertyName, out value))
        {
            return true;
        }

        value = default;
        return false;
    }

    private static string? TryGetString(JsonElement? element, string propertyName, string nestedPropertyName)
    {
        if (!TryGetProperty(element, propertyName, out var childElement)
            || childElement.ValueKind != JsonValueKind.Object
            || !childElement.TryGetProperty(nestedPropertyName, out var nestedElement))
        {
            return null;
        }

        return nestedElement.GetString();
    }

    private static string? TryGetString(JsonElement? element, string propertyName)
    {
        if (!TryGetProperty(element, propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.True => bool.TrueString.ToLowerInvariant(),
            JsonValueKind.False => bool.FalseString.ToLowerInvariant(),
            _ => null
        };
    }

    private static int? TryGetInt32(JsonElement? element, string propertyName)
    {
        if (!TryGetProperty(element, propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetInt32(out var number) => number,
            JsonValueKind.String when int.TryParse(value.GetString(), out var parsed) => parsed,
            _ => null
        };
    }
}
