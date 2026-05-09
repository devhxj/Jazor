using System.Collections;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Jazor.Analyzer.RazorVue.Generation;

internal static class RazorSourceGeneratorInitializationContextState
{
    private const string OutputNodesFieldName = "_outputNodes";
    private static readonly FieldInfo? OutputNodesField = typeof(IncrementalGeneratorInitializationContext)
        .GetField(OutputNodesFieldName, BindingFlags.Instance | BindingFlags.NonPublic);

    internal static object? GetContextKey(IncrementalGeneratorInitializationContext context)
        => GetOutputNodesObject(context);

    internal static IEnumerable? GetOutputNodes(IncrementalGeneratorInitializationContext context)
        => GetOutputNodesObject(context) as IEnumerable;

    private static object? GetOutputNodesObject(IncrementalGeneratorInitializationContext context)
    {
        try
        {
            return OutputNodesField?.GetValue(context);
        }
        catch
        {
            return null;
        }
    }
}
