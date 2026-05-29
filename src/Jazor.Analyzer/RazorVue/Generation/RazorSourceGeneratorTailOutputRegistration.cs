using System.Collections;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace Jazor.Analyzer.RazorVue.Generation;

internal static class RazorSourceGeneratorTailOutputRegistration
{
    private const string ImplementationSourceOutputRegistrationKind = "implementation-source-output";
    private const string HostOutputRegistrationKind = "host-output";
    private static readonly object Sync = new();
    private static readonly ConditionalWeakTable<object, HookedHostOutputSourceSet> HookedHostOutputSourcesByContext = new();
    private static readonly HashSet<object> HookedHostOutputSourcesWithoutContext = new(ReferenceEqualityComparer.Instance);

    internal static bool TryRegisterTailOutputFromNewOutputNodes(
        IncrementalGeneratorInitializationContext context,
        int outputNodeCountBeforeInitialize)
    {
        try
        {
            var outputNodes = EnumerateOutputNodes(context, outputNodeCountBeforeInitialize).ToArray();
            foreach (var outputNode in outputNodes)
            {
                if (IsImplementationSourceOutputNode(outputNode) &&
                    TryRegisterTailOutputFromOutputNode(
                        context,
                        outputNode,
                        nameof(TryRegisterTailOutputFromValuesProvider)))
                {
                    return true;
                }
            }

            foreach (var outputNode in outputNodes)
            {
                if (IsHostOutputNode(outputNode) &&
                    TryRegisterTailOutputFromOutputNode(
                        context,
                        outputNode,
                        nameof(TryRegisterTailOutputFromValueProvider)))
                {
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            RazorSourceGeneratorBootstrapState.MarkPatchFailed(ex.GetType().FullName + ": " + ex.Message);
            return false;
        }
    }

    internal static int GetOutputNodeCount(IncrementalGeneratorInitializationContext context)
    {
        var outputNodes = GetOutputNodes(context);
        if (outputNodes is null)
            return 0;

        var count = 0;
        foreach (var _ in outputNodes)
            count++;

        return count;
    }

    private static void TryRegisterTailOutputFromValueProvider<TSource>(
        IncrementalGeneratorInitializationContext context,
        object sourceNode)
    {
        RazorSourceGeneratorBootstrapState.MarkHostOutputObserved();
        var contextKey = RazorSourceGeneratorInitializationContextState.GetContextKey(context);
        if (!TryMarkHostOutputSourceHooked(contextKey, sourceNode))
            return;

        var provider = (IncrementalValueProvider<TSource>)CreateIncrementalValueProvider(
            typeof(TSource),
            sourceNode,
            GetCatchAnalyzerExceptions(context));
        var options = context.AnalyzerConfigOptionsProvider.Select(
            static (provider, _) => RazorSourceGeneratorHostOutputHookOptions.CreateTailOutputOptions(provider));
        var gatedSource = provider.Combine(options);

        context.RegisterSourceOutput(gatedSource, EmitTailOutput);
        RazorSourceGeneratorBootstrapState.MarkTailOutputRegistered(contextKey, HostOutputRegistrationKind);
        RazorSourceGeneratorBootstrapState.MarkHostOutputHookInstalled();
    }

    private static void TryRegisterTailOutputFromValuesProvider<TSource>(
        IncrementalGeneratorInitializationContext context,
        object sourceNode)
    {
        RazorSourceGeneratorBootstrapState.MarkHostOutputObserved();
        var contextKey = RazorSourceGeneratorInitializationContextState.GetContextKey(context);
        if (!TryMarkHostOutputSourceHooked(contextKey, sourceNode))
            return;

        var provider = (IncrementalValuesProvider<TSource>)CreateIncrementalValuesProvider(
            typeof(TSource),
            sourceNode,
            GetCatchAnalyzerExceptions(context));
        var options = context.AnalyzerConfigOptionsProvider.Select(
            static (provider, _) => RazorSourceGeneratorHostOutputHookOptions.CreateTailOutputOptions(provider));
        var gatedSource = provider.Collect().Combine(options);

        context.RegisterSourceOutput(gatedSource, EmitCollectedTailOutput);
        RazorSourceGeneratorBootstrapState.MarkTailOutputRegistered(contextKey, ImplementationSourceOutputRegistrationKind);
        RazorSourceGeneratorBootstrapState.MarkHostOutputHookInstalled();
    }

    private static bool TryMarkHostOutputSourceHooked(object? contextKey, object sourceNode)
    {
        lock (Sync)
        {
            if (contextKey is null)
                return HookedHostOutputSourcesWithoutContext.Add(sourceNode);

            return HookedHostOutputSourcesByContext
                .GetOrCreateValue(contextKey)
                .TryAdd(sourceNode);
        }
    }

    private static void EmitTailOutput<TSource>(
        SourceProductionContext productionContext,
        (TSource Source, RazorSourceGeneratorTailOutputOptions Options) input)
    {
        if (!input.Options.Enabled)
            return;

        var compilation = GetCompilation(productionContext);
        if (compilation is null)
            return;

        RazorSourceGeneratorTailOutput.Emit(productionContext, compilation, input.Source!, input.Options);
    }

    private static void EmitCollectedTailOutput<TSource>(
        SourceProductionContext productionContext,
        (ImmutableArray<TSource> Source, RazorSourceGeneratorTailOutputOptions Options) input)
    {
        if (!input.Options.Enabled)
            return;

        var compilation = GetCompilation(productionContext);
        if (compilation is null)
            return;

        RazorSourceGeneratorTailOutput.Emit(productionContext, compilation, input.Source, input.Options);
    }

    private static bool TryRegisterTailOutputFromOutputNode(
        IncrementalGeneratorInitializationContext context,
        object outputNode,
        string registerMethodName)
    {
        var sourceType = GetOutputSourceType(outputNode.GetType());
        if (sourceType is null || !IsRazorGeneratorResultHostOutputSource(sourceType))
            return false;

        var sourceField = outputNode.GetType().GetField("_source", BindingFlags.Instance | BindingFlags.NonPublic);
        var sourceNode = sourceField?.GetValue(outputNode);
        if (sourceNode is null)
            return false;

        var registerMethod = typeof(RazorSourceGeneratorTailOutputRegistration).GetMethod(
            registerMethodName,
            BindingFlags.Static | BindingFlags.NonPublic);
        var closedRegisterMethod = registerMethod?.MakeGenericMethod(sourceType);
        if (closedRegisterMethod is null)
            return false;

        closedRegisterMethod.Invoke(null, [context, sourceNode]);
        return true;
    }

    private static IEnumerable<object> EnumerateOutputNodes(
        IncrementalGeneratorInitializationContext context,
        int outputNodeCountBeforeInitialize)
    {
        var outputNodes = GetOutputNodes(context);
        if (outputNodes is null)
            yield break;

        var index = 0;
        foreach (var node in outputNodes)
        {
            if (index++ < outputNodeCountBeforeInitialize)
                continue;

            if (node is not null)
                yield return node;
        }
    }

    private static IEnumerable? GetOutputNodes(IncrementalGeneratorInitializationContext context)
        => RazorSourceGeneratorInitializationContextState.GetOutputNodes(context);

    private static bool IsHostOutputNode(object outputNode)
    {
        var type = outputNode.GetType();
        return type.IsGenericType &&
               string.Equals(
                   type.GetGenericTypeDefinition().FullName,
                   "Microsoft.CodeAnalysis.HostOutputNode`1",
                   StringComparison.Ordinal);
    }

    private static bool IsImplementationSourceOutputNode(object outputNode)
    {
        var type = outputNode.GetType();
        if (!type.IsGenericType ||
            !string.Equals(
                type.GetGenericTypeDefinition().FullName,
                "Microsoft.CodeAnalysis.SourceOutputNode`1",
                StringComparison.Ordinal))
        {
            return false;
        }

        var kind = type.GetProperty("Kind", BindingFlags.Instance | BindingFlags.Public)?.GetValue(outputNode);
        return string.Equals(kind?.ToString(), "Implementation", StringComparison.Ordinal);
    }

    private static Type? GetOutputSourceType(Type outputNodeType)
        => outputNodeType.IsGenericType
            ? outputNodeType.GetGenericArguments()[0]
            : null;

    private static bool IsRazorGeneratorResultHostOutputSource(Type type)
        => TypeContainsFullName(type, "Microsoft.AspNetCore.Razor.Language.RazorCodeDocument") &&
           TypeContainsFullName(type, "Microsoft.AspNetCore.Razor.Language.RazorCSharpDocument");

    private static bool TypeContainsFullName(Type type, string fullName)
    {
        if (string.Equals(type.FullName, fullName, StringComparison.Ordinal))
            return true;

        if (type.IsArray)
            return TypeContainsFullName(type.GetElementType()!, fullName);

        if (type.IsByRef || type.IsPointer)
            return TypeContainsFullName(type.GetElementType()!, fullName);

        if (!type.IsGenericType)
            return false;

        foreach (var genericArgument in type.GetGenericArguments())
        {
            if (TypeContainsFullName(genericArgument, fullName))
                return true;
        }

        return false;
    }

    private static object CreateIncrementalValueProvider(Type sourceType, object sourceNode, bool catchAnalyzerExceptions)
    {
        var providerType = typeof(IncrementalValueProvider<>).MakeGenericType(sourceType);
        var constructor = providerType
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .SingleOrDefault(static item => item.GetParameters().Length == 2);
        if (constructor is null)
            throw new InvalidOperationException("IncrementalValueProvider<T> internal constructor was not found.");

        return constructor.Invoke([sourceNode, catchAnalyzerExceptions]);
    }

    private static object CreateIncrementalValuesProvider(Type sourceType, object sourceNode, bool catchAnalyzerExceptions)
    {
        var providerType = typeof(IncrementalValuesProvider<>).MakeGenericType(sourceType);
        var constructor = providerType
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .SingleOrDefault(static item => item.GetParameters().Length == 2);
        if (constructor is null)
            throw new InvalidOperationException("IncrementalValuesProvider<T> internal constructor was not found.");

        return constructor.Invoke([sourceNode, catchAnalyzerExceptions]);
    }

    private static bool GetCatchAnalyzerExceptions(IncrementalGeneratorInitializationContext context)
    {
        var property = typeof(IncrementalGeneratorInitializationContext).GetProperty(
            "CatchAnalyzerExceptions",
            BindingFlags.Instance | BindingFlags.NonPublic);
        return property?.GetValue(context) is not bool value || value;
    }

    private static Compilation? GetCompilation(SourceProductionContext context)
    {
        var field = typeof(SourceProductionContext).GetField(
            "Compilation",
            BindingFlags.Instance | BindingFlags.NonPublic);
        return field?.GetValue(context) as Compilation;
    }

    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static ReferenceEqualityComparer Instance { get; } = new();

        public new bool Equals(object? x, object? y)
            => ReferenceEquals(x, y);

        public int GetHashCode(object obj)
            => RuntimeHelpers.GetHashCode(obj);
    }

    private sealed class HookedHostOutputSourceSet
    {
        private readonly HashSet<object> _sources = new(ReferenceEqualityComparer.Instance);

        public bool TryAdd(object sourceNode)
            => _sources.Add(sourceNode);
    }
}
