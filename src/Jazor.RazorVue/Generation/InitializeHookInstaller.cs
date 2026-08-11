using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.Generation;

/// <summary>
/// Installs the Roslyn initialization hook that exposes the post-Razor-SG compilation.
/// The hook is isolated here because its runtime patching is outside normal generator lowering.
/// </summary>
internal static class InitializeHookInstaller
{
    private static readonly object InstallationSync = new();
    private static int _initialized;
    private static int _platformValidated;
    private static InitializeNativeHook? _hook;
    private static string? _failure;
    private const string ArtifactCatalogPath = "obj/Jazor.RazorVue/Jazor.Generated.ArtifactCatalog.g.cs";

    internal static bool TryInstall()
    {
        lock (InstallationSync)
        {
            if (Volatile.Read(ref _initialized) != 0)
            {
                var hook = Volatile.Read(ref _hook);
                if (hook is null)
                    return false;

                // Long-running hosts can promote GeneratorDriver's target method to a
                // different JIT body. Reinstall rather than silently losing tail output.
                return hook.IsCurrentTargetPatched() || TryPatchGeneratorDriver();
            }

            try
            {
                if (Interlocked.Exchange(ref _platformValidated, 1) == 0 &&
                    !InitializeNativeHook.TryValidateCurrentPlatform(out var platformFailure))
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
            finally
            {
                // The hook is process-global. Publish completion only after _hook has
                // been assigned, otherwise concurrent Razor drivers can miss tail output.
                Volatile.Write(ref _initialized, 1);
            }
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
            var replacement = typeof(InitializeHookInstaller).GetMethod(
                nameof(InitializeReplacement),
                BindingFlags.Static | BindingFlags.NonPublic);

            if (target is null || replacement is null)
            {
                SetFailure("GeneratorDriver.RunGeneratorsAndUpdateCompilation or its RazorVue replacement was not found.");
                return false;
            }

            var hook = InitializeNativeHook.Install(target, replacement);
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

        // Calling the patched method's original body required temporarily restoring its
        // process-wide machine code. A concurrent driver could enter during that window and
        // bypass RazorVue tail output entirely. Reproduce Roslyn's public completion contract
        // from RunGenerators/GetRunResult so the hook remains installed for every thread.
        var result = instance.RunGenerators(compilation, cancellationToken);
        var runResult = result.GetRunResult();
        using var sourceTextScope = RazorSourceTextRegistry.PushGeneratedTrees(
            runResult.GeneratedTrees,
            cancellationToken);
        outputCompilation = compilation.AddSyntaxTrees(
            runResult.GeneratedTrees.Where(static tree => !RazorSourceTextRegistry.IsCarrierTree(tree)));
        diagnostics = runResult.Diagnostics;
        if (ContainsArtifactCatalog(outputCompilation))
            return result;

        // Only a successful final Compilation is a valid lowering input. Razor SG diagnostics
        // remain authoritative for invalid Razor authoring, and existing C# errors likewise
        // must not be masked by a secondary RazorVue failure or produce a partial catalog.
        if (diagnostics.Any(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error) ||
            outputCompilation.GetDiagnostics().Any(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
            return result;

        if (!RazorTailOutput.TryBuildFinalCompilationCatalog(
                outputCompilation,
                cancellationToken,
                out var catalogSource,
                out var failure))
        {
            diagnostics = diagnostics.Add(Diagnostic.Create(
                Diagnostics.TailOutputFailed,
                Location.None,
                failure ?? "Unknown final Compilation render catalog generation failure."));
            return result;
        }

        if (catalogSource is { Length: > 0 })
            outputCompilation = outputCompilation.AddSyntaxTrees(CreateCatalogSyntaxTree(outputCompilation, catalogSource));

        return result;
    }

    private static bool ContainsArtifactCatalog(Compilation compilation)
        => compilation.SyntaxTrees.Any(static tree => string.Equals(
            tree.FilePath,
            ArtifactCatalogPath,
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
            ArtifactCatalogPath);
    }
}
