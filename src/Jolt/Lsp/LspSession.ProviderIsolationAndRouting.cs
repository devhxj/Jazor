using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Text.Json;
using Jolt.Extensions;
using Jolt.Jazor.Projection;
using Jolt.Lsp.Aggregation;
using Jolt.Lsp.Coordination;
using Jolt.Lsp.Lanes;
using Jolt.Lsp.Routing;
using Jolt.VirtualDocuments.Registry;
using Jolt.Workspace;
using Jazor.Common.VueContracts.Protocol;

namespace Jolt.Lsp;

internal sealed partial class LspSession
{
    private async ValueTask<ProviderInvocationResult<TResult>> InvokeProviderAsync<TResult>(
        string capability,
        string providerName,
        Func<CancellationToken, ValueTask<TResult>> invocation,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var startedTimestamp = Stopwatch.GetTimestamp();
        if (TryGetProviderIsolationWindow(capability, providerName, out var isolationRemaining))
        {
            _extensionRegistry.ReportProviderInvocation(new ExtensionProviderInvocation(
                ProviderName: providerName,
                Capability: capability,
                Duration: Stopwatch.GetElapsedTime(startedTimestamp),
                Succeeded: false,
                TimedOut: false,
                Skipped: true,
                ErrorMessage: $"Provider isolated for {isolationRemaining.TotalMilliseconds:F0} ms due to recent failures."));
            return ProviderInvocationResult<TResult>.Isolated();
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        Task<TResult> invocationTask;
        try
        {
            invocationTask = invocation(timeoutCts.Token).AsTask();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            RecordProviderFailure(capability, providerName);
            _extensionRegistry.ReportProviderInvocation(new ExtensionProviderInvocation(
                ProviderName: providerName,
                Capability: capability,
                Duration: Stopwatch.GetElapsedTime(startedTimestamp),
                Succeeded: false,
                TimedOut: false,
                Skipped: false,
                ErrorMessage: ex.Message));
            return ProviderInvocationResult<TResult>.Failure();
        }

        try
        {
            var result = await invocationTask.WaitAsync(_extensionProviderTimeout, cancellationToken);
            RecordProviderSuccess(capability, providerName);
            _extensionRegistry.ReportProviderInvocation(new ExtensionProviderInvocation(
                ProviderName: providerName,
                Capability: capability,
                Duration: Stopwatch.GetElapsedTime(startedTimestamp),
                Succeeded: true,
                TimedOut: false,
                Skipped: false,
                ErrorMessage: null));
            return ProviderInvocationResult<TResult>.Success(result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TimeoutException)
        {
            RecordProviderFailure(capability, providerName);
            timeoutCts.Cancel();
            _ = ObserveProviderCompletionAsync(invocationTask);
            _extensionRegistry.ReportProviderInvocation(new ExtensionProviderInvocation(
                ProviderName: providerName,
                Capability: capability,
                Duration: Stopwatch.GetElapsedTime(startedTimestamp),
                Succeeded: false,
                TimedOut: true,
                Skipped: false,
                ErrorMessage: $"Provider timed out after {_extensionProviderTimeout.TotalMilliseconds:F0} ms."));
            return ProviderInvocationResult<TResult>.Timeout();
        }
        catch (Exception ex)
        {
            RecordProviderFailure(capability, providerName);
            _extensionRegistry.ReportProviderInvocation(new ExtensionProviderInvocation(
                ProviderName: providerName,
                Capability: capability,
                Duration: Stopwatch.GetElapsedTime(startedTimestamp),
                Succeeded: false,
                TimedOut: false,
                Skipped: false,
                ErrorMessage: ex.Message));
            return ProviderInvocationResult<TResult>.Failure();
        }
    }

    private bool TryGetProviderIsolationWindow(
        string capability,
        string providerName,
        out TimeSpan remaining)
    {
        var now = DateTimeOffset.UtcNow;
        var key = CreateProviderIsolationKey(capability, providerName);

        lock (_providerIsolationGate)
        {
            if (!_providerIsolationByKey.TryGetValue(key, out var state)
                || state.IsolatedUntil is null)
            {
                remaining = TimeSpan.Zero;
                return false;
            }

            var isolatedUntil = state.IsolatedUntil.Value;
            if (isolatedUntil <= now)
            {
                _providerIsolationByKey[key] = state with { IsolatedUntil = null };
                remaining = TimeSpan.Zero;
                return false;
            }

            remaining = isolatedUntil - now;
            return true;
        }
    }

    private void RecordProviderSuccess(string capability, string providerName)
    {
        var key = CreateProviderIsolationKey(capability, providerName);
        lock (_providerIsolationGate)
        {
            _providerIsolationByKey[key] = new ProviderIsolationState(
                ConsecutiveFailureCount: 0,
                IsolatedUntil: null);
        }
    }

    private void RecordProviderFailure(string capability, string providerName)
    {
        var key = CreateProviderIsolationKey(capability, providerName);
        var now = DateTimeOffset.UtcNow;

        lock (_providerIsolationGate)
        {
            _providerIsolationByKey.TryGetValue(key, out var currentState);

            var currentFailures = currentState.IsolatedUntil is { } isolatedUntil && isolatedUntil > now
                ? 0
                : currentState.ConsecutiveFailureCount;
            var nextFailureCount = currentFailures + 1;
            var nextIsolatedUntil = nextFailureCount >= _extensionProviderIsolationFailureThreshold
                ? now + _extensionProviderIsolationDuration
                : (DateTimeOffset?)null;
            var persistedFailures = nextIsolatedUntil is not null
                ? 0
                : nextFailureCount;

            _providerIsolationByKey[key] = new ProviderIsolationState(
                persistedFailures,
                nextIsolatedUntil);
        }
    }

    private static string CreateProviderIsolationKey(string capability, string providerName)
        => capability.Trim() + "|" + providerName.Trim();

    private static Task ObserveProviderCompletionAsync<TResult>(Task<TResult> task)
    {
        // Timed-out provider work may still fault later; observe faulted completion to avoid
        // unobserved task exception escalation without rethrowing into the server pipeline.
        return task.ContinueWith(
            static completedTask =>
            {
                _ = completedTask.Exception;
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted,
            TaskScheduler.Default);
    }

    private ExtensionObservabilityDashboard CreateObservabilityDashboard()
        => new(
            LoadHealth: _extensionRegistry.GetExtensionLoadHealth(),
            ProviderHealth: _extensionRegistry.GetProviderHealth(),
            RecentLoadEvents: _extensionRegistry.GetRecentExtensionLoadInvocations(maxCount: 200),
            RecentProviderEvents: _extensionRegistry.GetRecentProviderInvocations(maxCount: 500),
            GeneratedAt: DateTimeOffset.UtcNow);

    private async ValueTask RefreshOpenJazorDiagnosticsAsync(
        DocumentSnapshot triggeringDocument,
        CancellationToken cancellationToken)
    {
        if (triggeringDocument.DocumentKind == DocumentKind.Jazor)
        {
            return;
        }

        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        // 前端文件变化后只刷新同项目的 Jazor 诊断，避免一个项目的 CSS/TS 变动
        // 把兄弟项目的诊断也一起拉起来，造成并发争用和无关刷新。
        foreach (var openDocument in openDocuments.Where(candidate =>
                     candidate.DocumentKind == DocumentKind.Jazor
                     && JoltWorkspaceResolver.IsInSameProjectScope(triggeringDocument.DocumentPath, candidate.DocumentPath)))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await PublishDiagnosticsAsync(openDocument, cancellationToken);
        }
    }

    private IReadOnlyList<ILspLane> GetOrderedLanes(ProjectionTarget projectionTarget)
    {
        var orderedLanes = new List<ILspLane>();
        foreach (var laneKind in _laneRouter.GetOrderedLanes(projectionTarget))
        {
            if (_lanes.TryGetValue(laneKind, out var lane))
            {
                orderedLanes.Add(lane);
            }
        }

        return orderedLanes;
    }

    private IReadOnlyList<ILspLane> GetDocumentSymbolLanes(DocumentSnapshot document)
    {
        LaneKind[] laneKinds = document.DocumentKind switch
        {
            DocumentKind.Jazor => [LaneKind.Jazor, LaneKind.Roslyn],
            DocumentKind.CSharp => [LaneKind.Roslyn],
            DocumentKind.Vue or DocumentKind.JavaScript or DocumentKind.TypeScript or DocumentKind.Css => [LaneKind.Volar],
            _ => [LaneKind.Jazor]
        };

        var orderedLanes = new List<ILspLane>();
        foreach (var laneKind in laneKinds)
        {
            if (_lanes.TryGetValue(laneKind, out var lane))
            {
                orderedLanes.Add(lane);
            }
        }

        return orderedLanes;
    }

    private IReadOnlyList<ILspLane> GetDocumentLinkLanes(DocumentSnapshot document)
    {
        LaneKind[] laneKinds = document.DocumentKind switch
        {
            DocumentKind.Jazor => [LaneKind.Jazor, LaneKind.Volar],
            DocumentKind.CSharp => [LaneKind.Roslyn],
            DocumentKind.Vue or DocumentKind.JavaScript or DocumentKind.TypeScript or DocumentKind.Css => [LaneKind.Volar],
            _ => [LaneKind.Jazor]
        };

        var orderedLanes = new List<ILspLane>();
        foreach (var laneKind in laneKinds)
        {
            if (_lanes.TryGetValue(laneKind, out var lane))
            {
                orderedLanes.Add(lane);
            }
        }

        return orderedLanes;
    }

    private IReadOnlyList<ILspLane> GetSemanticTokenLanes(DocumentSnapshot document)
    {
        var orderedLanes = new List<ILspLane>();
        foreach (var laneKind in _laneRouter.GetSemanticTokenLanes(document))
        {
            if (_lanes.TryGetValue(laneKind, out var lane))
            {
                orderedLanes.Add(lane);
            }
        }

        return orderedLanes;
    }

    private IReadOnlyList<ILspLane> GetInlayAndFoldingLanes(DocumentSnapshot document)
    {
        LaneKind[] laneKinds = document.DocumentKind switch
        {
            DocumentKind.Jazor => [LaneKind.Jazor, LaneKind.Volar, LaneKind.Roslyn],
            DocumentKind.CSharp => [LaneKind.Roslyn],
            DocumentKind.Vue or DocumentKind.JavaScript or DocumentKind.TypeScript or DocumentKind.Css => [LaneKind.Volar],
            _ => [LaneKind.Jazor]
        };

        var orderedLanes = new List<ILspLane>();
        foreach (var laneKind in laneKinds)
        {
            if (_lanes.TryGetValue(laneKind, out var lane))
            {
                orderedLanes.Add(lane);
            }
        }

        return orderedLanes;
    }

    private bool TryGetRoslynLaneService([NotNullWhen(true)] out RoslynLaneService? roslynLane)
    {
        if (_lanes.TryGetValue(LaneKind.Roslyn, out var lane)
            && lane is RoslynLaneService typedLane)
        {
            roslynLane = typedLane;
            return true;
        }

        roslynLane = null;
        return false;
    }

    private static bool IsRoslynSemanticTarget(
        DocumentSnapshot document,
        ProjectionTarget projectionTarget)
        => document.DocumentKind == DocumentKind.CSharp
            || projectionTarget.LaneKind == LaneKind.Roslyn
            || projectionTarget.RegionKind == DocumentRegionKind.Code;

    private async ValueTask<DocumentSnapshot> GetRequiredDocumentAsync(
        string documentUri,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentUri))
        {
            throw CreateInvalidParamsException("textDocument.uri is required.");
        }

        var documentPath = GetWorkspaceScopedDocumentPath(documentUri);
        var document = await _workspaceStore.GetDocumentAsync(documentPath, cancellationToken);
        if (document is not null)
        {
            return document;
        }

        if (!File.Exists(documentPath))
        {
            throw new InvalidOperationException($"Document '{documentPath}' is not tracked and does not exist on disk.");
        }

        document = new DocumentSnapshot(
            documentPath,
            MapDocumentKind(languageId: null, documentPath),
            await File.ReadAllTextAsync(documentPath, cancellationToken),
            version: null);
        await _workspaceStore.UpsertDocumentAsync(document, cancellationToken);
        await UpdateProjectionStateAsync(document, cancellationToken);
        return document;
    }

    private bool IsInsideWorkspaceRoots(string documentPath)
    {
        var workspaceRoots = GetWorkspaceFolderRootPaths();
        if (workspaceRoots.Count == 0)
        {
            return true;
        }

        var fullDocumentPath = Path.GetFullPath(documentPath);
        foreach (var rootPath in workspaceRoots)
        {
            var fullRootPath = Path.GetFullPath(rootPath);
            var relativePath = Path.GetRelativePath(fullRootPath, fullDocumentPath);
            if (string.Equals(relativePath, ".", StringComparison.Ordinal)
                || (!string.Equals(relativePath, "..", StringComparison.Ordinal)
                    && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                    && !relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
                    && !Path.IsPathRooted(relativePath)))
            {
                return true;
            }
        }

        return false;
    }

    private static TParams DeserializeParams<TParams>(object? payload)
    {
        try
        {
            if (payload is JsonElement element)
            {
                return element.Deserialize<TParams>() ?? throw new InvalidOperationException("Invalid LSP params payload.");
            }

            if (payload is TParams typed)
            {
                return typed;
            }

            return LspJsonSerializer.Deserialize<TParams>(LspJsonSerializer.Serialize(payload))
                ?? throw new InvalidOperationException("Invalid LSP params payload.");
        }
        catch (LspRequestException)
        {
            throw;
        }
        catch (JsonException exception)
        {
            throw new LspRequestException(
                InvalidParamsErrorCode,
                $"Invalid LSP params payload for '{typeof(TParams).Name}'.",
                exception);
        }
        catch (InvalidOperationException exception)
        {
            throw new LspRequestException(
                InvalidParamsErrorCode,
                $"Invalid LSP params payload for '{typeof(TParams).Name}'.",
                exception);
        }
        catch (ArgumentException exception)
        {
            throw new LspRequestException(
                InvalidParamsErrorCode,
                $"Invalid LSP params payload for '{typeof(TParams).Name}'.",
                exception);
        }
        catch (NotSupportedException exception)
        {
            throw new LspRequestException(
                InvalidParamsErrorCode,
                $"Invalid LSP params payload for '{typeof(TParams).Name}'.",
                exception);
        }
    }

    private static TParams? TryDeserializeParams<TParams>(object? payload)
    {
        if (payload is null)
        {
            return default;
        }

        try
        {
            return DeserializeParams<TParams>(payload);
        }
        catch (LspRequestException) {
            return default;
        }
    }

    private static DocumentKind MapDocumentKind(string? languageId, string documentPath)
        => languageId?.ToLowerInvariant() switch
        {
            "jazor" => DocumentKind.Jazor,
            "csharp" => DocumentKind.CSharp,
            "cs" => DocumentKind.CSharp,
            "vue" => DocumentKind.Vue,
            "javascript" => DocumentKind.JavaScript,
            "typescript" => DocumentKind.TypeScript,
            "css" => DocumentKind.Css,
            _ => Path.GetExtension(documentPath).ToLowerInvariant() switch
            {
                ".jazor" => DocumentKind.Jazor,
                ".cs" => DocumentKind.CSharp,
                ".vue" => DocumentKind.Vue,
                ".js" => DocumentKind.JavaScript,
                ".ts" => DocumentKind.TypeScript,
                ".css" => DocumentKind.Css,
                _ => DocumentKind.Unknown
            }
        };

    private static string GetRequiredTextDocumentUri(LspTextDocumentIdentifier? textDocument)
    {
        if (textDocument is null)
        {
            throw CreateInvalidParamsException("textDocument is required.");
        }

        if (string.IsNullOrWhiteSpace(textDocument.Uri))
        {
            throw CreateInvalidParamsException("textDocument.uri is required.");
        }

        return textDocument.Uri;
    }

    private static LspRequestException CreateInvalidParamsException(string message)
        => new(InvalidParamsErrorCode, message);
}
