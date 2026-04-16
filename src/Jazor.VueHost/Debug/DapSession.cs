using System.Globalization;

namespace Jazor.VueHost.Debug;

internal sealed class DapSession(bool hasCdpBackend = false)
{
    private const string FallbackCallFrameId = "fallback-frame-1";
    private const string FallbackFunctionName = "render";
    private const string FallbackSourcePath = "/__jazor__/Counter.jazor";

    private readonly Dictionary<string, IReadOnlyList<DapBreakpointBinding>> _breakpointsBySourcePath = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<int, IReadOnlyList<DapVariable>> _variablesByReference = [];
    private IReadOnlyList<CdpCallFrame> _currentCallFrames = [];
    private int _nextVariablesReference = 1;

    public bool IsInitialized { get; set; }

    public bool IsStarted { get; set; }

    public bool HasCdpBackend { get; } = hasCdpBackend;

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

        _breakpointsBySourcePath[sourcePath] = breakpoints;
    }

    public IReadOnlyList<DapBreakpointBinding> GetBreakpoints(string sourcePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);

        return _breakpointsBySourcePath.TryGetValue(sourcePath, out var breakpoints)
            ? breakpoints
            : [];
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

        var locals = CreateFrameVariables(frame);
        var session = CreateSessionVariables(frameId);

        return
        [
            CreateScope("Locals", locals),
            CreateScope("Session", session, presentationHint: "registers")
        ];
    }

    public IReadOnlyList<DapVariable> GetVariables(int variablesReference)
        => variablesReference > 0 && _variablesByReference.TryGetValue(variablesReference, out var variables)
            ? variables
            : [];

    public DapEvaluationResult Evaluate(string? expression, int? frameId, string? context = null)
    {
        var trimmedExpression = expression?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedExpression))
        {
            return new DapEvaluationResult("undefined", "undefined", 0);
        }

        if (TryResolveVariablePath(frameId, trimmedExpression, out var resolvedVariable))
        {
            return new DapEvaluationResult(
                resolvedVariable.Value,
                resolvedVariable.Type,
                resolvedVariable.VariablesReference);
        }

        if (!HasCdpBackend)
        {
            // Keep fallback evaluate deterministic before CDP Runtime.evaluate is connected.
            var normalizedContext = string.IsNullOrWhiteSpace(context)
                ? "repl"
                : context.Trim();
            return new DapEvaluationResult($"[{normalizedContext}] {trimmedExpression}", "string", 0);
        }

        // TODO: Replace this placeholder once a browser CDP Runtime.evaluate transport is available.
        return new DapEvaluationResult(
            "Evaluation unavailable (CDP Runtime.evaluate not connected).",
            "string",
            0);
    }

    public void ContinueExecution()
    {
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
        _breakpointsBySourcePath.Clear();
    }

    private static bool TryResolveVariable(
        IReadOnlyList<DapVariable> variables,
        string expression,
        out DapVariable variable)
    {
        foreach (var candidate in variables)
        {
            if (string.Equals(candidate.Name, expression, StringComparison.Ordinal))
            {
                variable = candidate;
                return true;
            }
        }

        variable = null!;
        return false;
    }

    private bool TryResolveVariablePath(int? frameId, string expression, out DapVariable variable)
    {
        var segments = expression.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0)
        {
            variable = null!;
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
        out DapVariable variable)
    {
        if (segments.Count == 0 || !TryResolveVariable(variables, segments[0], out var current))
        {
            variable = null!;
            return false;
        }

        for (var index = 1; index < segments.Count; index++)
        {
            if (current.VariablesReference <= 0
                || !_variablesByReference.TryGetValue(current.VariablesReference, out var children)
                || !TryResolveVariable(children, segments[index], out current))
            {
                variable = null!;
                return false;
            }
        }

        variable = current;
        return true;
    }

    private bool TryGetCallFrame(int frameId, out CdpCallFrame frame)
    {
        var frameIndex = frameId - 1;
        if (frameIndex < 0 || frameIndex >= CurrentCallFrames.Count)
        {
            frame = null!;
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
        => _breakpointsBySourcePath.Values.Sum(static items => items.Count);

    private DapScope CreateScope(
        string name,
        IReadOnlyList<DapVariable> variables,
        string? presentationHint = null)
        => new()
        {
            Name = name,
            VariablesReference = RegisterVariables(variables),
            Expensive = false,
            PresentationHint = presentationHint,
            NamedVariables = variables.Count
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
        _variablesByReference[variablesReference] = variables;
        return variablesReference;
    }

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

    private void ResetVariableReferences()
    {
        _variablesByReference.Clear();
        _nextVariablesReference = 1;
    }
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
