using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace Jazor.VueHost.Debug;

internal sealed class DapSession
{
    private const string FallbackCallFrameId = "fallback-frame-1";
    private const string FallbackFunctionName = "render";
    private const string FallbackSourcePath = "/__jazor__/Counter.jazor";

    private readonly ICdpClient? _cdpClient;
    private readonly VariableMapper _variableMapper;
    private readonly Lock _breakpointGate = new();
    private readonly Dictionary<string, IReadOnlyList<DapBreakpointBinding>> _breakpointsBySourcePath = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<int, VariablesReferenceEntry> _variablesByReference = [];
    private IReadOnlyList<CdpCallFrame> _currentCallFrames = [];
    private int _nextVariablesReference = 1;

    public DapSession(ICdpClient? cdpClient = null)
    {
        _cdpClient = cdpClient;
        _variableMapper = new VariableMapper();

        if (_cdpClient is not null)
        {
            _cdpClient.Paused += OnCdpPaused;
            _cdpClient.Resumed += OnCdpResumed;
            CurrentCallFrames = _cdpClient.LatestCallFrames;
        }
    }

    public bool IsInitialized { get; set; }

    public bool IsStarted { get; set; }

    public bool HasCdpBackend => _cdpClient is not null;

    public bool IsPaused { get; private set; }

    public IReadOnlyList<CdpCallFrame> CurrentCallFrames
    {
        get => _currentCallFrames;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _currentCallFrames = value;
            IsPaused = value.Count > 0;
            ResetVariableReferences();
        }
    }

    public void SetBreakpoints(string sourcePath, IReadOnlyList<DapBreakpointBinding> breakpoints)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentNullException.ThrowIfNull(breakpoints);

        lock (_breakpointGate)
        {
            _breakpointsBySourcePath[sourcePath] = breakpoints;
        }
    }

    public IReadOnlyList<DapBreakpointBinding> GetBreakpoints(string sourcePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);

        lock (_breakpointGate)
        {
            return _breakpointsBySourcePath.TryGetValue(sourcePath, out var breakpoints)
                ? breakpoints.ToArray()
                : [];
        }
    }

    public async ValueTask<MappedBreakpoint?> BindMappedBreakpointAsync(
        MappedBreakpoint mappedBreakpoint,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mappedBreakpoint);

        if (_cdpClient is null)
        {
            return mappedBreakpoint;
        }

        try
        {
            var resolution = await _cdpClient.SetBreakpointByUrlAsync(
                mappedBreakpoint.GeneratedPath,
                mappedBreakpoint.GeneratedLine,
                mappedBreakpoint.GeneratedColumn,
                cancellationToken);
            return resolution is null
                ? null
                : new MappedBreakpoint(
                    resolution.Location.Url,
                    resolution.Location.LineNumber,
                    resolution.Location.ColumnNumber);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (ObjectDisposedException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }

    public void SeedFallbackPause()
    {
        if (HasCdpBackend || IsPaused || CurrentCallFrames.Count > 0)
        {
            return;
        }

        CurrentCallFrames =
        [
            new CdpCallFrame(
                FallbackCallFrameId,
                FallbackFunctionName,
                new CdpLocation(FallbackSourcePath, 0, 0))
        ];
    }

    public IReadOnlyList<DapScope> CreateScopes(int frameId)
    {
        if (!TryGetCallFrame(frameId, out var frame))
        {
            return [];
        }

        if (_cdpClient is not null
            && frame.ScopeChain is { Count: > 0 } scopeChain)
        {
            var scopes = new List<DapScope>(scopeChain.Count + 1);
            foreach (var scope in scopeChain)
            {
                var variablesReference = RegisterRemoteObject(scope.Object);
                if (variablesReference <= 0)
                {
                    continue;
                }

                scopes.Add(CreateScope(
                    GetScopeDisplayName(scope),
                    variablesReference,
                    presentationHint: GetScopePresentationHint(scope.Type)));
            }

            scopes.Add(CreateScope("Session", CreateSessionVariables(frameId), presentationHint: "registers"));
            return scopes;
        }

        var locals = CreateFrameVariables(frame);
        var session = CreateSessionVariables(frameId);

        return
        [
            CreateScope("Locals", locals),
            CreateScope("Session", session, presentationHint: "registers")
        ];
    }

    public async ValueTask<IReadOnlyList<DapVariable>> GetVariablesAsync(
        int variablesReference,
        CancellationToken cancellationToken = default)
    {
        if (variablesReference <= 0
            || !_variablesByReference.TryGetValue(variablesReference, out var entry))
        {
            return [];
        }

        if (entry.Variables is not null)
        {
            return entry.Variables;
        }

        if (_cdpClient is null || entry.RemoteObject is null)
        {
            return [];
        }

        var variables = await ExpandRemoteObjectAsync(entry.RemoteObject, cancellationToken);
        entry.Variables = variables;
        return variables;
    }

    public async ValueTask<DapEvaluationResult> EvaluateAsync(
        string? expression,
        int? frameId,
        string? context = null,
        CancellationToken cancellationToken = default)
    {
        var trimmedExpression = expression?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedExpression))
        {
            return new DapEvaluationResult("undefined", "undefined", 0);
        }

        if (_cdpClient is null)
        {
            if (TryResolveVariablePath(frameId, trimmedExpression, out var resolvedVariable))
            {
                return new DapEvaluationResult(
                    resolvedVariable.Value,
                    resolvedVariable.Type,
                    resolvedVariable.VariablesReference);
            }

            // Keep fallback evaluate deterministic before CDP Runtime.evaluate is connected.
            var normalizedContext = string.IsNullOrWhiteSpace(context)
                ? "repl"
                : context.Trim();
            return new DapEvaluationResult($"[{normalizedContext}] {trimmedExpression}", "string", 0);
        }

        var callFrameId = ResolveCallFrameId(frameId);
        var remoteObject = await _cdpClient.EvaluateAsync(trimmedExpression, callFrameId, cancellationToken);
        return _variableMapper.ToEvaluationResult(remoteObject, RegisterRemoteObject(remoteObject));
    }

    public async ValueTask ContinueExecutionAsync(CancellationToken cancellationToken = default)
    {
        if (_cdpClient is not null)
        {
            await _cdpClient.ContinueAsync(cancellationToken);
        }

        IsPaused = false;
        CurrentCallFrames = [];
    }

    public void Clear()
    {
        IsInitialized = false;
        IsStarted = false;
        IsPaused = false;
        CurrentCallFrames = [];
        ResetVariableReferences();
        lock (_breakpointGate)
        {
            _breakpointsBySourcePath.Clear();
        }
    }

    private string? ResolveCallFrameId(int? frameId)
    {
        if (frameId is > 0 && TryGetCallFrame(frameId.Value, out var frame))
        {
            return frame.CallFrameId;
        }

        return CurrentCallFrames.FirstOrDefault()?.CallFrameId;
    }

    private void OnCdpPaused(IReadOnlyList<CdpCallFrame> callFrames)
        => CurrentCallFrames = callFrames;

    private void OnCdpResumed()
        => CurrentCallFrames = [];

    private static bool TryResolveVariable(
        IReadOnlyList<DapVariable> variables,
        string expression,
        [NotNullWhen(true)] out DapVariable? variable)
    {
        foreach (var candidate in variables)
        {
            if (string.Equals(candidate.Name, expression, StringComparison.Ordinal))
            {
                variable = candidate;
                return true;
            }
        }

        variable = null;
        return false;
    }

    private bool TryResolveVariablePath(int? frameId, string expression, [NotNullWhen(true)] out DapVariable? variable)
    {
        var segments = expression.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0)
        {
            variable = null;
            return false;
        }

        if (frameId is > 0
            && TryResolveVariablePath(CreateFrameVariablesForLookup(frameId.Value), segments, out variable))
        {
            return true;
        }

        return TryResolveVariablePath(CreateSessionVariables(frameId), segments, out variable);
    }

    private bool TryResolveVariablePath(
        IReadOnlyList<DapVariable> variables,
        IReadOnlyList<string> segments,
        [NotNullWhen(true)] out DapVariable? variable)
    {
        if (segments.Count == 0 || !TryResolveVariable(variables, segments[0], out var current))
        {
            variable = null;
            return false;
        }

        for (var index = 1; index < segments.Count; index++)
        {
            if (current.VariablesReference <= 0
                || !_variablesByReference.TryGetValue(current.VariablesReference, out var childrenEntry)
                || childrenEntry.Variables is null
                || !TryResolveVariable(childrenEntry.Variables, segments[index], out current))
            {
                variable = null;
                return false;
            }
        }

        variable = current;
        return true;
    }

    private bool TryGetCallFrame(int frameId, [NotNullWhen(true)] out CdpCallFrame? frame)
    {
        var frameIndex = frameId - 1;
        if (frameIndex < 0 || frameIndex >= CurrentCallFrames.Count)
        {
            frame = null;
            return false;
        }

        frame = CurrentCallFrames[frameIndex];
        return true;
    }

    private IReadOnlyList<DapVariable> CreateFrameVariablesForLookup(int frameId)
        => TryGetCallFrame(frameId, out var frame)
            ? CreateFrameVariables(frame)
            : [];

    private IReadOnlyList<DapVariable> CreateFrameVariables(CdpCallFrame frame)
        =>
        [
            CreateLeafVariable("callFrameId", frame.CallFrameId, "string"),
            CreateLeafVariable(
                "functionName",
                string.IsNullOrWhiteSpace(frame.FunctionName) ? "(anonymous)" : frame.FunctionName,
                "string"),
            CreateObjectVariable("source", CreateSourcePreview(frame.Location.Url), CreateSourceVariables(frame.Location.Url)),
            CreateObjectVariable("location", CreateLocationPreview(frame.Location), CreateLocationVariables(frame.Location)),
            CreateLeafVariable("backend", HasCdpBackend ? "connected" : "fallback", "string")
        ];

    private IReadOnlyList<DapVariable> CreateSessionVariables(int? frameId)
        =>
        [
            CreateLeafVariable("initialized", ToBooleanLiteral(IsInitialized), "boolean"),
            CreateLeafVariable("started", ToBooleanLiteral(IsStarted), "boolean"),
            CreateLeafVariable("paused", ToBooleanLiteral(IsPaused), "boolean"),
            CreateLeafVariable("callFrameCount", CurrentCallFrames.Count.ToString(CultureInfo.InvariantCulture), "number"),
            CreateLeafVariable("breakpointCount", GetBreakpointCount().ToString(CultureInfo.InvariantCulture), "number"),
            CreateLeafVariable(
                "selectedFrameId",
                (frameId ?? 0).ToString(CultureInfo.InvariantCulture),
                "number")
        ];

    private static string ToBooleanLiteral(bool value)
        => value ? "true" : "false";

    private int GetBreakpointCount()
    {
        lock (_breakpointGate)
        {
            return _breakpointsBySourcePath.Values.Sum(static items => items.Count);
        }
    }

    private DapScope CreateScope(
        string name,
        IReadOnlyList<DapVariable> variables,
        string? presentationHint = null)
        => CreateScope(
            name,
            RegisterVariables(variables),
            presentationHint,
            namedVariables: variables.Count);

    private static DapScope CreateScope(
        string name,
        int variablesReference,
        string? presentationHint = null,
        int? namedVariables = null)
        => new()
        {
            Name = name,
            VariablesReference = variablesReference,
            Expensive = false,
            PresentationHint = presentationHint,
            NamedVariables = namedVariables
        };

    private DapVariable CreateLeafVariable(string name, string value, string type)
        => new()
        {
            Name = name,
            Value = value,
            Type = type,
            VariablesReference = 0
        };

    private DapVariable CreateObjectVariable(
        string name,
        string value,
        IReadOnlyList<DapVariable> children)
    {
        var variablesReference = RegisterVariables(children);
        return new DapVariable
        {
            Name = name,
            Value = value,
            Type = "object",
            VariablesReference = variablesReference,
            NamedVariables = children.Count
        };
    }

    private int RegisterVariables(IReadOnlyList<DapVariable> variables)
    {
        var variablesReference = _nextVariablesReference++;
        _variablesByReference[variablesReference] = VariablesReferenceEntry.FromVariables(variables);
        return variablesReference;
    }

    private int RegisterRemoteObject(CdpRemoteObject? remoteObject)
    {
        if (!CanExpand(remoteObject))
        {
            return 0;
        }

        var variablesReference = _nextVariablesReference++;
        _variablesByReference[variablesReference] = VariablesReferenceEntry.FromRemoteObject(remoteObject!);
        return variablesReference;
    }

    private async ValueTask<IReadOnlyList<DapVariable>> ExpandRemoteObjectAsync(
        CdpRemoteObject remoteObject,
        CancellationToken cancellationToken)
    {
        if (_cdpClient is null || string.IsNullOrWhiteSpace(remoteObject.ObjectId))
        {
            return [];
        }

        var properties = await _cdpClient.GetPropertiesAsync(remoteObject.ObjectId, cancellationToken);
        if (properties.Count == 0)
        {
            return [];
        }

        var variables = new List<DapVariable>(properties.Count);
        foreach (var property in properties)
        {
            variables.Add(_variableMapper.ToVariable(
                property.Name,
                property.Value,
                RegisterRemoteObject(property.Value)));
        }

        return variables;
    }

    private static bool CanExpand(CdpRemoteObject? remoteObject)
        => remoteObject is not null
            && !string.IsNullOrWhiteSpace(remoteObject.ObjectId)
            && !string.Equals(remoteObject.SubType, "null", StringComparison.Ordinal)
            && (string.Equals(remoteObject.Type, "object", StringComparison.Ordinal)
                || string.Equals(remoteObject.Type, "function", StringComparison.Ordinal));

    private static IReadOnlyList<DapVariable> CreateSourceVariables(string sourcePath)
        =>
        [
            new DapVariable
            {
                Name = "name",
                Value = GetSourceName(sourcePath),
                Type = "string",
                VariablesReference = 0
            },
            new DapVariable
            {
                Name = "path",
                Value = sourcePath,
                Type = "string",
                VariablesReference = 0
            }
        ];

    private static IReadOnlyList<DapVariable> CreateLocationVariables(CdpLocation location)
        =>
        [
            new DapVariable
            {
                Name = "url",
                Value = location.Url,
                Type = "string",
                VariablesReference = 0
            },
            new DapVariable
            {
                Name = "line",
                Value = (location.LineNumber + 1).ToString(CultureInfo.InvariantCulture),
                Type = "number",
                VariablesReference = 0
            },
            new DapVariable
            {
                Name = "column",
                Value = (location.ColumnNumber + 1).ToString(CultureInfo.InvariantCulture),
                Type = "number",
                VariablesReference = 0
            }
        ];

    private static string CreateSourcePreview(string sourcePath)
        => "{ name = " + GetSourceName(sourcePath) + ", path = " + sourcePath + " }";

    private static string CreateLocationPreview(CdpLocation location)
        => "{ url = "
            + location.Url
            + ", line = "
            + (location.LineNumber + 1).ToString(CultureInfo.InvariantCulture)
            + ", column = "
            + (location.ColumnNumber + 1).ToString(CultureInfo.InvariantCulture)
            + " }";

    private static string GetSourceName(string sourcePath)
    {
        var sourceName = Path.GetFileName(sourcePath);
        return string.IsNullOrWhiteSpace(sourceName)
            ? sourcePath
            : sourceName;
    }

    private static string GetScopeDisplayName(CdpScope scope)
    {
        if (string.IsNullOrWhiteSpace(scope.Type))
        {
            return "Scope";
        }

        return scope.Type switch
        {
            "closure" when !string.IsNullOrWhiteSpace(scope.Name) => $"Closure ({scope.Name})",
            _ => char.ToUpperInvariant(scope.Type[0]) + scope.Type[1..]
        };
    }

    private static string? GetScopePresentationHint(string scopeType)
        => scopeType switch
        {
            "local" => "locals",
            "global" => "globals",
            _ => null
        };

    private void ResetVariableReferences()
    {
        _variablesByReference.Clear();
        _nextVariablesReference = 1;
    }
}

internal sealed class VariablesReferenceEntry
{
    private VariablesReferenceEntry()
    {
    }

    public IReadOnlyList<DapVariable>? Variables { get; set; }

    public CdpRemoteObject? RemoteObject { get; private init; }

    public static VariablesReferenceEntry FromVariables(IReadOnlyList<DapVariable> variables)
        => new()
        {
            Variables = variables
        };

    public static VariablesReferenceEntry FromRemoteObject(CdpRemoteObject remoteObject)
        => new()
        {
            RemoteObject = remoteObject
        };
}

internal sealed record DapBreakpointBinding(
    int BreakpointId,
    string SourcePath,
    int SourceLine,
    int SourceColumn,
    MappedBreakpoint? GeneratedBreakpoint);

internal sealed record DapEvaluationResult(
    string Result,
    string? Type,
    int VariablesReference);
