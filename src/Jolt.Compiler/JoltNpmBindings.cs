using Jolt.Npm;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jolt.Compiler;

public static class JoltNpmBindings
{
    public static string CacheDirectory(string projectDirectory) =>
        Path.Combine(Path.GetFullPath(projectDirectory), "obj", "Jolt", "projections");

    public static string GenerateCachedSource(string projectDirectory, IReadOnlyList<NpmValueExport> exports,
        CancellationToken cancellationToken = default) =>
        JoltNpmProjectionCache.LoadBindings(CacheDirectory(projectDirectory), exports, cancellationToken);

    public static string GenerateSource(IReadOnlyList<NpmValueExport> exports, CancellationToken cancellationToken = default)
    {
        var tree = new JoltNpmValueProjection([], exports, new CSharpParseOptions(LanguageVersion.Preview), standalone: true).Tree;
        var compilation = CSharpCompilation.Create("JoltNpmBindings", [tree], JoltProjectCompiler.PlatformReferences.Value,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics(cancellationToken).Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException("Cannot generate npm bindings." + Environment.NewLine + string.Join(Environment.NewLine, errors.AsEnumerable()));

        return tree.GetText(cancellationToken).ToString();
    }
}
