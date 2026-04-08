using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Frontend.Deno.Hosting;
using Jazor.VueHost.Lsp.Routing;
using System.Reflection;

namespace Jazor.VueHost.Lsp.Lanes;

internal sealed class FrontendLaneService : ILspLane
{
    private readonly JazorLspDocumentService _documentService;
    private readonly IDenoFrontendHost? _denoFrontendHost;

    public FrontendLaneService(
        JazorLspDocumentService documentService,
        IDenoFrontendHost? denoFrontendHost = null)
    {
        _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        _denoFrontendHost = denoFrontendHost;
    }

    public LaneKind LaneKind => LaneKind.Frontend;

    public ValueTask<IReadOnlyList<LspDiagnostic>> GetDiagnosticsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
        => ValueTask.FromResult<IReadOnlyList<LspDiagnostic>>(Array.Empty<LspDiagnostic>());

    public async ValueTask<LspHoverResult?> GetHoverAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsTemplateTarget(projectionTarget))
        {
            return null;
        }

        var denoResult = await TryInvokeDenoAsync<LspHoverResult?>(
            ["GetHoverAsync", "GetTemplateHoverAsync"],
            cancellationToken,
            document,
            position,
            projectionTarget);
        if (denoResult is not null)
        {
            return denoResult;
        }

        return await _documentService.GetHoverAsync(document, position, cancellationToken);
    }

    public async ValueTask<IReadOnlyList<LspCompletionItem>> GetCompletionItemsAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsTemplateTarget(projectionTarget))
        {
            return Array.Empty<LspCompletionItem>();
        }

        var denoResult = await TryInvokeDenoAsync<IReadOnlyList<LspCompletionItem>>(
            ["GetCompletionItemsAsync", "GetTemplateCompletionItemsAsync"],
            cancellationToken,
            document,
            position,
            projectionTarget);
        if (denoResult is { Count: > 0 })
        {
            return denoResult;
        }

        return await _documentService.GetCompletionItemsAsync(document, position, cancellationToken);
    }

    public async ValueTask<IReadOnlyList<LspLocation>> GetDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsTemplateTarget(projectionTarget))
        {
            return Array.Empty<LspLocation>();
        }

        var denoResult = await TryInvokeDenoAsync<IReadOnlyList<LspLocation>>(
            ["GetDefinitionAsync", "GetTemplateDefinitionAsync"],
            cancellationToken,
            document,
            position,
            projectionTarget);
        if (denoResult is { Count: > 0 })
        {
            return denoResult;
        }

        return await _documentService.GetDefinitionAsync(document, position, cancellationToken);
    }

    public async ValueTask<IReadOnlyList<LspLocation>> GetReferencesAsync(
        DocumentSnapshot document,
        LspPosition position,
        bool includeDeclaration,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsTemplateTarget(projectionTarget))
        {
            return Array.Empty<LspLocation>();
        }

        var denoResult = await TryInvokeDenoAsync<IReadOnlyList<LspLocation>>(
            ["GetReferencesAsync", "GetTemplateReferencesAsync"],
            cancellationToken,
            document,
            position,
            includeDeclaration,
            projectionTarget);
        if (denoResult is { Count: > 0 })
        {
            return denoResult;
        }

        return await _documentService.GetReferencesAsync(document, position, includeDeclaration, cancellationToken);
    }

    public async ValueTask<LspWorkspaceEdit?> GetRenameAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsTemplateTarget(projectionTarget))
        {
            return null;
        }

        var denoResult = await TryInvokeDenoAsync<LspWorkspaceEdit?>(
            ["GetRenameAsync", "GetTemplateRenameAsync"],
            cancellationToken,
            document,
            position,
            newName,
            projectionTarget);
        if (denoResult is not null)
        {
            return denoResult;
        }

        return await _documentService.GetRenameAsync(document, position, newName, cancellationToken);
    }

    public ValueTask<IReadOnlyList<LspCodeAction>> GetCodeActionsAsync(
        DocumentSnapshot document,
        LspRange range,
        IReadOnlyList<LspDiagnostic> diagnostics,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
        => IsTemplateTarget(projectionTarget)
            ? _documentService.GetCodeActionsAsync(document, diagnostics, cancellationToken)
            : ValueTask.FromResult<IReadOnlyList<LspCodeAction>>(Array.Empty<LspCodeAction>());

    private static bool IsTemplateTarget(ProjectionTarget projectionTarget)
        => projectionTarget.LaneKind == LaneKind.Frontend
            || projectionTarget.RegionKind == DocumentRegionKind.Template;

    private async ValueTask<TResult?> TryInvokeDenoAsync<TResult>(
        string[] methodNames,
        CancellationToken cancellationToken,
        params object?[] arguments)
    {
        if (_denoFrontendHost is null || !_denoFrontendHost.IsRunning)
        {
            return default;
        }

        var hostType = _denoFrontendHost.GetType();
        foreach (var methodName in methodNames)
        {
            var methods = hostType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(candidate => string.Equals(candidate.Name, methodName, StringComparison.Ordinal))
                .ToArray();
            foreach (var method in methods)
            {
                if (!TryBuildInvocationArguments(method, cancellationToken, arguments, out var invocationArguments))
                {
                    continue;
                }

                try
                {
                    var invocationResult = method.Invoke(_denoFrontendHost, invocationArguments);
                    return await ConvertResultAsync<TResult>(invocationResult);
                }
                catch
                {
                    return default;
                }
            }
        }

        return default;
    }

    private static bool TryBuildInvocationArguments(
        MethodInfo method,
        CancellationToken cancellationToken,
        object?[] providedArguments,
        out object?[] invocationArguments)
    {
        var parameters = method.GetParameters();
        invocationArguments = new object?[parameters.Length];
        var usedArguments = new bool[providedArguments.Length];

        for (var parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
        {
            var parameter = parameters[parameterIndex];
            if (parameter.ParameterType == typeof(CancellationToken))
            {
                invocationArguments[parameterIndex] = cancellationToken;
                continue;
            }

            var matched = false;
            for (var argumentIndex = 0; argumentIndex < providedArguments.Length; argumentIndex++)
            {
                if (usedArguments[argumentIndex])
                {
                    continue;
                }

                var argument = providedArguments[argumentIndex];
                if (argument is null)
                {
                    if (!parameter.ParameterType.IsValueType || Nullable.GetUnderlyingType(parameter.ParameterType) is not null)
                    {
                        invocationArguments[parameterIndex] = null;
                        usedArguments[argumentIndex] = true;
                        matched = true;
                        break;
                    }

                    continue;
                }

                if (!parameter.ParameterType.IsInstanceOfType(argument))
                {
                    continue;
                }

                invocationArguments[parameterIndex] = argument;
                usedArguments[argumentIndex] = true;
                matched = true;
                break;
            }

            if (!matched)
            {
                return false;
            }
        }

        return true;
    }

    private static async ValueTask<TResult?> ConvertResultAsync<TResult>(object? invocationResult)
    {
        if (invocationResult is null)
        {
            return default;
        }

        if (invocationResult is TResult typedResult)
        {
            return typedResult;
        }

        if (invocationResult is ValueTask<TResult> valueTaskResult)
        {
            return await valueTaskResult;
        }

        if (invocationResult is Task<TResult> taskResult)
        {
            return await taskResult;
        }

        var invocationType = invocationResult.GetType();
        if (invocationType.IsGenericType && invocationType.GetGenericTypeDefinition() == typeof(ValueTask<>))
        {
            var asTask = invocationType.GetMethod("AsTask", BindingFlags.Instance | BindingFlags.Public);
            var task = asTask?.Invoke(invocationResult, null) as Task;
            if (task is not null)
            {
                await task;
                var resultProperty = task.GetType().GetProperty("Result", BindingFlags.Instance | BindingFlags.Public);
                if (resultProperty?.GetValue(task) is TResult reflectedResult)
                {
                    return reflectedResult;
                }
            }
        }

        if (invocationResult is Task nonGenericTask)
        {
            await nonGenericTask;
            var resultProperty = nonGenericTask.GetType().GetProperty("Result", BindingFlags.Instance | BindingFlags.Public);
            if (resultProperty?.GetValue(nonGenericTask) is TResult reflectedTaskResult)
            {
                return reflectedTaskResult;
            }
        }

        return default;
    }
}
