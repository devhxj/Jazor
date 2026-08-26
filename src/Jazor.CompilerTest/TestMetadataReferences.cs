using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.ComplierTest;

public static class TestMetadataReferences
{
    public static readonly CSharpParseOptions PreviewParseOptions =
        CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);

    public static ImmutableArray<MetadataReference> Net11 { get; } =
        ResolveNetCoreAppReferences("net11.0")
            .Append(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptAttribute).Assembly.Location))
            .ToImmutableArray();

    private static IEnumerable<MetadataReference> ResolveNetCoreAppReferences(string targetFramework)
    {
        var refDirectory = ResolveReferencePackDirectory(targetFramework);

        return Directory.EnumerateFiles(refDirectory, "*.dll", SearchOption.TopDirectoryOnly)
            .OrderBy(static path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
            .Select(static path => MetadataReference.CreateFromFile(path));
    }

    private static string ResolveReferencePackDirectory(string targetFramework)
    {
        var dotnetRoots = GetCandidateDotnetRoots().Distinct(StringComparer.OrdinalIgnoreCase);
        foreach (var dotnetRoot in dotnetRoots)
        {
            var packRoot = Path.Combine(dotnetRoot, "packs", "Microsoft.NETCore.App.Ref");
            if (!Directory.Exists(packRoot))
                continue;

            var exactRuntimeVersion = Path.GetFileName(Path.GetDirectoryName(typeof(object).Assembly.Location));
            if (!string.IsNullOrWhiteSpace(exactRuntimeVersion))
            {
                var exactRefDirectory = Path.Combine(packRoot, exactRuntimeVersion, "ref", targetFramework);
                if (Directory.Exists(exactRefDirectory))
                    return exactRefDirectory;
            }

            var latestRefDirectory = Directory.EnumerateDirectories(packRoot)
                .Select(versionDirectory => Path.Combine(versionDirectory, "ref", targetFramework))
                .Where(Directory.Exists)
                .OrderByDescending(static path => Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(path))), StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();

            if (latestRefDirectory is not null)
                return latestRefDirectory;
        }

        throw new DirectoryNotFoundException(
            $"Unable to locate Microsoft.NETCore.App.Ref reference assemblies for {targetFramework}. " +
            "Install the matching .NET SDK or set DOTNET_ROOT to the SDK installation root.");
    }

    private static IEnumerable<string> GetCandidateDotnetRoots()
    {
        foreach (var variable in new[] { "DOTNET_ROOT", "DOTNET_ROOT(x86)" })
        {
            var value = Environment.GetEnvironmentVariable(variable);
            if (!string.IsNullOrWhiteSpace(value))
                yield return value;
        }

        var runtimeDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location);
        if (!string.IsNullOrWhiteSpace(runtimeDirectory))
        {
            var current = new DirectoryInfo(runtimeDirectory);
            while (current is not null)
            {
                if (current.Name.Equals("dotnet", StringComparison.OrdinalIgnoreCase))
                {
                    yield return current.FullName;
                    break;
                }

                current = current.Parent;
            }
        }

        yield return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "dotnet");
    }
}
