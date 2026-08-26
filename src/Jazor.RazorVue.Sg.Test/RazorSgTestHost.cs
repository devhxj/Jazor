using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Sg.Test;

internal static class RazorSgTestHost
{
    private static readonly ConcurrentDictionary<string, PortableExecutableReference> MetadataReferenceCache =
        new(StringComparer.OrdinalIgnoreCase);

    public static string[] GetCompilationErrors(Compilation compilation)
        => compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .Select(static diagnostic => diagnostic.ToString())
            .ToArray();

    /// <summary>
    /// Returns a portable source path for an in-memory Razor fixture.
    /// The file does not need to exist: Razor SG consumes the path as source identity while the
    /// document text is supplied through <c>AdditionalText</c> or a <c>RazorSourceDocument</c>.
    /// Keep fixtures under the repository's test tree so source maps and reproductions never
    /// depend on a developer-specific absolute path.
    /// </summary>
    public static string GetTestDocumentPath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new ArgumentException("A relative fixture path is required.", nameof(relativePath));

        if (Path.IsPathRooted(relativePath))
            throw new ArgumentException("Fixture paths must be relative to test/fixtures/razorvue.", nameof(relativePath));

        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        return Path.Combine(FindRepositoryRoot(), "test", "fixtures", "razorvue", normalized);
    }

    /// <summary>
    /// Creates an isolated test artifact directory below the repository test tree.
    /// </summary>
    public static string CreateTestArtifactDirectory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("A test artifact category is required.", nameof(category));

        var root = Path.Combine(
            FindRepositoryRoot(),
            "test",
            "artifacts",
            "razorvue",
            category,
            $"{Environment.ProcessId}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        return root;
    }

    public static string GetLoadedRazorCompilerAssemblyPath()
    {
        var assemblyPath = typeof(Microsoft.AspNetCore.Razor.Language.RazorProjectEngine).Assembly.Location;
        Assert.IsFalse(string.IsNullOrWhiteSpace(assemblyPath), "RazorProjectEngine assembly location was empty.");
        return assemblyPath;
    }

    public static string ComputeFileSha256(string path)
    {
        Assert.IsTrue(File.Exists(path), "Expected file does not exist: " + path);

        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    public static MetadataReference[] CreateMetadataReferences()
    {
        var trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        var referencePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(trustedPlatformAssemblies))
        {
            foreach (var path in trustedPlatformAssemblies.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                referencePaths.Add(path);
            }
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            {
                referencePaths.Add(assembly.Location);
            }
        }

        AddAssemblyLocation(referencePaths, typeof(ECMAScript.Vue));
        AddAssemblyLocation(referencePaths, typeof(ECMAScript.ECMAScriptAttribute));
        AddAssemblyLocation(referencePaths, typeof(ECMAScript.VueContract.VueInjectAttribute));
        AddAssemblyLocation(referencePaths, typeof(global::ECMAScript.Style.css));
        AddAssemblyLocation(referencePaths, typeof(Microsoft.AspNetCore.Components.ComponentBase));
        AddAssemblyLocation(referencePaths, typeof(Microsoft.AspNetCore.Components.Web.MouseEventArgs));
        AddAssemblyLocation(referencePaths, typeof(Compilation));
        AddAssemblyLocation(referencePaths, typeof(Microsoft.CodeAnalysis.CSharp.CSharpCompilation));

        var references = new List<MetadataReference>(referencePaths.Count);
        foreach (var path in referencePaths)
        {
            if (TryCreateCachedPortableExecutableReference(path, out var reference))
            {
                references.Add(reference);
            }
        }

        return [.. references];
    }

    private static void AddAssemblyLocation(HashSet<string> referencePaths, Type markerType)
    {
        var location = markerType.Assembly.Location;
        if (!string.IsNullOrWhiteSpace(location))
        {
            referencePaths.Add(location);
        }
    }

    private static bool TryCreateCachedPortableExecutableReference(
        string path,
        out PortableExecutableReference reference)
    {
        reference = null!;
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var normalizedPath = Path.GetFullPath(path);
        if (!File.Exists(normalizedPath))
        {
            return false;
        }

        reference = MetadataReferenceCache.GetOrAdd(
            normalizedPath,
            static candidatePath => MetadataReference.CreateFromFile(candidatePath));
        return true;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the Jazor repository root from the test output directory.");
    }
}
