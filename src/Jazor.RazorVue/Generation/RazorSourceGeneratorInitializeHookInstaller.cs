using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.Generation;

internal static class RazorSourceGeneratorInitializeHookInstaller
{
    private static int _initialized;
    private static int _platformValidated;
    private static RazorSourceGeneratorInitializeNativeHook? _hook;
    private static string? _failure;
    private const string VueRenderCatalogPath = "obj/Jazor.RazorVue/Jazor.Generated.VueRenderCatalog.g.cs";

    internal static bool TryInstall()
    {
        if (Interlocked.Exchange(ref _initialized, 1) != 0)
            return Volatile.Read(ref _hook) is not null;

        try
        {
            if (Interlocked.Exchange(ref _platformValidated, 1) == 0 &&
                !RazorSourceGeneratorInitializeNativeHook.TryValidateCurrentPlatform(out var platformFailure))
            {
                SetFailure(platformFailure);
                return false;
            }

            return TryPatchGeneratorDriver();
        }
        catch (Exception ex)
        {
            SetFailure(ex.GetType().FullName + ": " + ex.Message);
            return false;
        }
    }

    private static bool TryPatchGeneratorDriver()
    {
        try
        {
            var target = typeof(GeneratorDriver).GetMethod(
                "RunGeneratorsAndUpdateCompilation",
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                types:
                [
                    typeof(Compilation),
                    typeof(Compilation).MakeByRefType(),
                    typeof(ImmutableArray<Diagnostic>).MakeByRefType(),
                    typeof(CancellationToken)
                ],
                modifiers: null);
            var replacement = typeof(RazorSourceGeneratorInitializeHookInstaller).GetMethod(
                nameof(InitializeReplacement),
                BindingFlags.Static | BindingFlags.NonPublic);

            if (target is null || replacement is null)
            {
                SetFailure("GeneratorDriver.RunGeneratorsAndUpdateCompilation or its RazorVue replacement was not found.");
                return false;
            }

            var hook = RazorSourceGeneratorInitializeNativeHook.Install(target, replacement);
            Volatile.Write(ref _hook, hook);
            return true;
        }
        catch (Exception ex)
        {
            SetFailure(ex.GetType().FullName + ": " + ex.Message);
            return false;
        }
    }

    internal static string? GetInstallFailure()
        => Volatile.Read(ref _failure);

    private static void SetFailure(string failure)
        => Volatile.Write(ref _failure, failure);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static GeneratorDriver InitializeReplacement(
        GeneratorDriver instance,
        Compilation compilation,
        out Compilation outputCompilation,
        out ImmutableArray<Diagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        var hook = Volatile.Read(ref _hook);
        if (hook is null)
        {
            throw new InvalidOperationException("GeneratorDriver hook was invoked before its hook handle was published.");
        }

        var arguments = new object?[]
        {
            compilation,
            null,
            default(ImmutableArray<Diagnostic>),
            cancellationToken
        };
        var result = (GeneratorDriver)hook.InvokeOriginal(instance, arguments)!;
        outputCompilation = (Compilation)arguments[1]!;
        diagnostics = (ImmutableArray<Diagnostic>)arguments[2]!;
        if (ContainsVueRenderCatalog(outputCompilation))
            return result;

        if (!RazorSourceGeneratorTailOutput.TryBuildFinalCompilationCatalog(
                outputCompilation,
                cancellationToken,
                out var catalogSource,
                out var failure))
        {
            diagnostics = diagnostics.Add(Diagnostic.Create(
                RazorSourceGeneratorDiagnostics.RazorSgTailOutputFailed,
                Location.None,
                failure ?? "Unknown final Compilation render catalog generation failure."));
            return result;
        }

        if (catalogSource is { Length: > 0 })
            outputCompilation = outputCompilation.AddSyntaxTrees(CreateCatalogSyntaxTree(outputCompilation, catalogSource));

        return result;
    }

    private static bool ContainsVueRenderCatalog(Compilation compilation)
        => compilation.SyntaxTrees.Any(static tree => string.Equals(
            tree.FilePath,
            VueRenderCatalogPath,
            StringComparison.Ordinal));

    private static SyntaxTree CreateCatalogSyntaxTree(Compilation compilation, string catalogSource)
    {
        var parseOptions = compilation.SyntaxTrees
            .Select(static tree => tree.Options)
            .OfType<CSharpParseOptions>()
            .FirstOrDefault() ?? CSharpParseOptions.Default;
        return CSharpSyntaxTree.ParseText(
            SourceText.From(catalogSource, System.Text.Encoding.UTF8),
            parseOptions,
            VueRenderCatalogPath);
    }
}
