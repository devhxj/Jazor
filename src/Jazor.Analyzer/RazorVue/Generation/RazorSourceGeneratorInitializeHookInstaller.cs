using System.Collections;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Microsoft.CodeAnalysis;

namespace Jazor.Analyzer.RazorVue.Generation;

internal static class RazorSourceGeneratorInitializeHookInstaller
{
    private static int _initialized;
    private static int _patchAttempted;
    private static readonly object Sync = new();
    private static readonly HashSet<object> HookedHostOutputSources = new(ReferenceEqualityComparer.Instance);
    [ThreadStatic]
    private static int _outputNodeCountBeforeInitialize;

    internal static bool TryInstall()
    {
        if (Interlocked.Exchange(ref _initialized, 1) != 0)
        {
            return RazorSourceGeneratorBootstrapState.IsInstalled();
        }

        try
        {
            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (TryPatchAssembly(assembly))
                {
                    return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static void OnAssemblyLoad(object? sender, AssemblyLoadEventArgs args)
    {
        _ = sender;
        TryPatchAssembly(args.LoadedAssembly);
    }

    private static bool TryPatchAssembly(Assembly assembly)
    {
        if (!string.Equals(assembly.GetName().Name, "Microsoft.CodeAnalysis.Razor.Compiler", StringComparison.Ordinal))
        {
            return false;
        }

        RazorSourceGeneratorBootstrapState.MarkRazorAssemblyObserved();

        if (Interlocked.Exchange(ref _patchAttempted, 1) != 0)
        {
            return RazorSourceGeneratorBootstrapState.IsInstalled();
        }

        try
        {
            RazorSourceGeneratorBootstrapState.MarkPatchAttempted();

            var compatibility = ValidateAssemblyForPatch(assembly);
            if (!compatibility.Success)
            {
                RazorSourceGeneratorBootstrapState.MarkPatchFailed(
                    compatibility.Failure ?? "RazorSourceGenerator compatibility validation failed.");
                return false;
            }

            var generatorType = assembly.GetType("Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator", throwOnError: false);
            if (generatorType is not null)
            {
                RazorSourceGeneratorBootstrapState.MarkGeneratorTypeFound();
            }

            var target = generatorType?.GetMethod(
                "Initialize",
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                types: [typeof(IncrementalGeneratorInitializationContext)],
                modifiers: null);
            if (target is not null)
            {
                RazorSourceGeneratorBootstrapState.MarkInitializeMethodFound();
            }

            var postfix = typeof(RazorSourceGeneratorInitializeHookInstaller).GetMethod(
                nameof(InitializePostfix),
                BindingFlags.Static | BindingFlags.NonPublic);
            var prefix = typeof(RazorSourceGeneratorInitializeHookInstaller).GetMethod(
                nameof(InitializePrefix),
                BindingFlags.Static | BindingFlags.NonPublic);
            var finalizer = typeof(RazorSourceGeneratorInitializeHookInstaller).GetMethod(
                nameof(InitializeFinalizer),
                BindingFlags.Static | BindingFlags.NonPublic);
            if (postfix is not null)
            {
                RazorSourceGeneratorBootstrapState.MarkPostfixMethodFound();
            }

            if (target is null || postfix is null || prefix is null || finalizer is null)
            {
                RazorSourceGeneratorBootstrapState.MarkPatchFailed("RazorSourceGenerator.Initialize or hook methods were not found.");
                return false;
            }

            var harmony = new Harmony("Jazor.RazorVue.RazorSourceGenerator.InitializeHook");
            harmony.Patch(
                target,
                prefix: new HarmonyMethod(prefix),
                postfix: new HarmonyMethod(postfix),
                finalizer: new HarmonyMethod(finalizer));
            RazorSourceGeneratorBootstrapState.MarkInstalled();
            RazorSourceGeneratorBootstrapState.MarkPatchSucceeded();
            return true;
        }
        catch (Exception ex)
        {
            RazorSourceGeneratorBootstrapState.MarkPatchFailed(ex.GetType().FullName + ": " + ex.Message);
            return false;
        }
    }

    internal static RazorSourceGeneratorCompatibilityValidationResult ValidateAssemblyForPatch(Assembly assembly)
    {
        if (assembly is null)
            throw new ArgumentNullException(nameof(assembly));

        return RazorSourceGeneratorCompatibilityGuard.Validate(
            RazorSourceGeneratorCompatibilityProbe.Collect(assembly));
    }

    private static void InitializePostfix(ref IncrementalGeneratorInitializationContext __0)
    {
        RazorSourceGeneratorBootstrapState.MarkPostfixInvoked();
        TryRegisterTailOutputFromHostOutputNode(__0);
    }

    private static void InitializePrefix(ref IncrementalGeneratorInitializationContext __0)
    {
        _outputNodeCountBeforeInitialize = GetOutputNodeCount(__0);
    }

    private static void InitializeFinalizer()
    {
    }

    private static void TryRegisterTailOutputFromValueProvider<TSource>(
        IncrementalGeneratorInitializationContext context,
        object sourceNode)
    {
        RazorSourceGeneratorBootstrapState.MarkHostOutputObserved();
        lock (Sync)
        {
            if (!HookedHostOutputSources.Add(sourceNode))
                return;
        }

        var provider = (IncrementalValueProvider<TSource>)CreateIncrementalValueProvider(
            typeof(TSource),
            sourceNode,
            GetCatchAnalyzerExceptions(context));
        var options = context.AnalyzerConfigOptionsProvider.Select(
            static (provider, _) => RazorSourceGeneratorHostOutputHookOptions.CreateTailOutputOptions(provider));
        var gatedSource = provider.Combine(options);

        context.RegisterSourceOutput(gatedSource, EmitTailOutput);
        RazorSourceGeneratorBootstrapState.MarkTailOutputRegistered();
        RazorSourceGeneratorBootstrapState.MarkHostOutputHookInstalled();
    }

    private static void TryRegisterTailOutputFromValuesProvider<TSource>(
        IncrementalGeneratorInitializationContext context,
        object sourceNode)
    {
        RazorSourceGeneratorBootstrapState.MarkHostOutputObserved();
        lock (Sync)
        {
            if (!HookedHostOutputSources.Add(sourceNode))
                return;
        }

        var provider = (IncrementalValuesProvider<TSource>)CreateIncrementalValuesProvider(
            typeof(TSource),
            sourceNode,
            GetCatchAnalyzerExceptions(context));
        var options = context.AnalyzerConfigOptionsProvider.Select(
            static (provider, _) => RazorSourceGeneratorHostOutputHookOptions.CreateTailOutputOptions(provider));
        var gatedSource = provider.Collect().Combine(options);

        context.RegisterSourceOutput(gatedSource, EmitCollectedTailOutput);
        RazorSourceGeneratorBootstrapState.MarkTailOutputRegistered();
        RazorSourceGeneratorBootstrapState.MarkHostOutputHookInstalled();
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

    private static void TryRegisterTailOutputFromHostOutputNode(IncrementalGeneratorInitializationContext context)
    {
        try
        {
            foreach (var outputNode in EnumerateNewOutputNodes(context))
            {
                if (IsImplementationSourceOutputNode(outputNode))
                {
                    if (TryRegisterTailOutputFromOutputNode(
                            context,
                            outputNode,
                            nameof(TryRegisterTailOutputFromValuesProvider)))
                    {
                        return;
                    }

                    continue;
                }

                if (!IsHostOutputNode(outputNode))
                    continue;

                if (TryRegisterTailOutputFromOutputNode(
                        context,
                        outputNode,
                        nameof(TryRegisterTailOutputFromValueProvider)))
                {
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            RazorSourceGeneratorBootstrapState.MarkPatchFailed(ex.GetType().FullName + ": " + ex.Message);
        }
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

        var registerMethod = typeof(RazorSourceGeneratorInitializeHookInstaller).GetMethod(
            registerMethodName,
            BindingFlags.Static | BindingFlags.NonPublic);
        var closedRegisterMethod = registerMethod?.MakeGenericMethod(sourceType);
        if (closedRegisterMethod is null)
            return false;

        closedRegisterMethod.Invoke(null, [context, sourceNode]);
        return true;
    }

    private static IEnumerable<object> EnumerateNewOutputNodes(IncrementalGeneratorInitializationContext context)
    {
        var outputNodes = GetOutputNodes(context);
        if (outputNodes is null)
        {
            yield break;
        }

        var index = 0;
        foreach (var node in outputNodes)
        {
            if (node is null)
                continue;

            if (index++ < _outputNodeCountBeforeInitialize)
                continue;

            yield return node;
        }
    }

    private static int GetOutputNodeCount(IncrementalGeneratorInitializationContext context)
    {
        var outputNodes = GetOutputNodes(context);
        if (outputNodes is null)
            return 0;

        var count = 0;
        foreach (var _ in outputNodes)
            count++;

        return count;
    }

    private static IEnumerable? GetOutputNodes(IncrementalGeneratorInitializationContext context)
    {
        var field = typeof(IncrementalGeneratorInitializationContext).GetField(
            "_outputNodes",
            BindingFlags.Instance | BindingFlags.NonPublic);
        return field?.GetValue(context) as IEnumerable;
    }

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
            => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}
