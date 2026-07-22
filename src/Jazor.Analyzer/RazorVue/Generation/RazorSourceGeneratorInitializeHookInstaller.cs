using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Jazor.Analyzer.RazorVue.Generation;

internal static class RazorSourceGeneratorInitializeHookInstaller
{
    private static int _initialized;
    private static int _patchAttempted;
    private static int _platformValidated;
    private static RazorSourceGeneratorInitializeNativeHook? _hook;

    internal static bool TryInstall()
    {
        if (Interlocked.Exchange(ref _initialized, 1) != 0)
            return RazorSourceGeneratorBootstrapState.IsInstalled();

        try
        {
            if (Interlocked.Exchange(ref _platformValidated, 1) == 0 &&
                !RazorSourceGeneratorInitializeNativeHook.TryValidateCurrentPlatform(out var platformFailure))
            {
                RazorSourceGeneratorBootstrapState.MarkPatchUnavailable(platformFailure);
                return false;
            }

            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (TryPatchAssembly(assembly))
                    return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            RazorSourceGeneratorBootstrapState.MarkPatchFailed(ex.GetType().FullName + ": " + ex.Message);
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
            return false;

        RazorSourceGeneratorBootstrapState.MarkRazorAssemblyObserved();

        if (Interlocked.Exchange(ref _patchAttempted, 1) != 0)
            return RazorSourceGeneratorBootstrapState.IsInstalled();

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

            RazorSourceGeneratorBootstrapState.MarkCompatibilityValidated(compatibility.Shape!);

            var generatorType = assembly.GetType(
                RazorSourceGeneratorCompatibilityGuard.RazorSourceGeneratorTypeName,
                throwOnError: false);
            if (generatorType is not null)
                RazorSourceGeneratorBootstrapState.MarkGeneratorTypeFound();

            var target = generatorType?.GetMethod(
                "Initialize",
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                types: [typeof(IncrementalGeneratorInitializationContext)],
                modifiers: null);
            if (target is not null)
                RazorSourceGeneratorBootstrapState.MarkInitializeMethodFound();

            var replacement = typeof(RazorSourceGeneratorInitializeHookInstaller).GetMethod(
                nameof(InitializeReplacement),
                BindingFlags.Static | BindingFlags.NonPublic);
            if (replacement is not null)
                RazorSourceGeneratorBootstrapState.MarkPostfixMethodFound();

            if (target is null || replacement is null)
            {
                RazorSourceGeneratorBootstrapState.MarkPatchFailed("RazorSourceGenerator.Initialize or RazorVue hook replacement method was not found.");
                return false;
            }

            var hook = RazorSourceGeneratorInitializeNativeHook.Install(target, replacement);
            Volatile.Write(ref _hook, hook);
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

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void InitializeReplacement(
        object instance,
        IncrementalGeneratorInitializationContext context)
    {
        var outputNodeCountBeforeInitialize = RazorSourceGeneratorTailOutputRegistration.GetOutputNodeCount(context);
        var hook = Volatile.Read(ref _hook);
        if (hook is null)
        {
            RazorSourceGeneratorBootstrapState.MarkPatchFailed("RazorSourceGenerator.Initialize hook was invoked before the hook handle was published.");
            return;
        }

        hook.InvokeOriginal(instance, context);
        RazorSourceGeneratorBootstrapState.MarkPostfixInvoked();
        _ = RazorSourceGeneratorTailOutputRegistration.TryRegisterTailOutputFromNewOutputNodes(
            context,
            outputNodeCountBeforeInitialize);
    }
}
