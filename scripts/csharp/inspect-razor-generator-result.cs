#!/usr/bin/env dotnet run
#:package Microsoft.CodeAnalysis.CSharp@5.10.0-1.26329.5
#:package Basic.Reference.Assemblies.Net110@1.8.7
#:property EnableTrimAnalyzer=false

using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

var razorCompilerPath = RazorCompilerPathResolver.Resolve();
Console.WriteLine("Razor compiler: " + razorCompilerPath);
var razorAssembly = Assembly.LoadFrom(razorCompilerPath);
var generatorType = razorAssembly.GetType("Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator", throwOnError: true)!;
var generator = (IIncrementalGenerator)Activator.CreateInstance(generatorType)!;

const string projectDirectory = @"D:\repo\Demo";
const string documentPath = @"D:\repo\Demo\Pages\Counter.razor";
const string documentText = """
    @page "/counter"
    <h1>Hello</h1>
    """;

var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
var compilation = CSharpCompilation.Create(
    assemblyName: "Inspect.RazorGeneratorResult",
    syntaxTrees:
    [
        CSharpSyntaxTree.ParseText(
            "internal static class EntryPoint { }",
            options: parseOptions,
            path: "EntryPoint.cs")
    ],
    references: Basic.Reference.Assemblies.Net110.References.All,
    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

var additionalText = new InMemoryAdditionalText(documentPath, documentText);
var optionsProvider = new TestAnalyzerConfigOptionsProvider(
    new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["build_property.RazorLangVersion"] = "11.0",
        ["build_property.RootNamespace"] = "Demo",
        ["build_property.SupportLocalizedComponentNames"] = "true",
        ["build_property.GenerateRazorMetadataSourceChecksumAttributes"] = "false",
        ["build_property.MSBuildProjectDirectory"] = projectDirectory
    },
    new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
    {
        [documentPath] = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["build_metadata.AdditionalFiles.TargetPath"] = Convert.ToBase64String(Encoding.UTF8.GetBytes("Pages/Counter.razor"))
        }
    });

GeneratorDriver driver = CSharpGeneratorDriver.Create(
    generators: [generator.AsSourceGenerator()],
    additionalTexts: [additionalText],
    parseOptions: parseOptions,
    optionsProvider: optionsProvider);
driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var finalCompilation, out var diagnostics);

var result = driver.GetRunResult().Results.Single();
Console.WriteLine("Generated sources: " + result.GeneratedSources.Length);
Console.WriteLine("Final compilation trees: " + finalCompilation.SyntaxTrees.Count());
Console.WriteLine("Razor generated trees: " + finalCompilation.SyntaxTrees.Count(static tree => tree.FilePath.EndsWith("_razor.g.cs", StringComparison.Ordinal)));
Console.WriteLine("Diagnostics: " + diagnostics.Length);

internal sealed class InMemoryAdditionalText(string path, string text) : AdditionalText
{
    private readonly SourceText _text = SourceText.From(text);

    public override string Path { get; } = path;

    public override SourceText GetText(CancellationToken cancellationToken = default)
        => _text;
}

internal sealed class TestAnalyzerConfigOptionsProvider(
    IReadOnlyDictionary<string, string> globalOptions,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> additionalFileOptions) : AnalyzerConfigOptionsProvider
{
    private readonly AnalyzerConfigOptions _globalOptions = new TestAnalyzerConfigOptions(globalOptions);
    private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> _additionalFileOptions = additionalFileOptions;
    private static readonly AnalyzerConfigOptions EmptyOptions = new TestAnalyzerConfigOptions(new Dictionary<string, string>(StringComparer.Ordinal));

    public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
        => EmptyOptions;

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
        => _additionalFileOptions.TryGetValue(textFile.Path, out var values)
            ? new TestAnalyzerConfigOptions(values)
            : EmptyOptions;
}

internal sealed class TestAnalyzerConfigOptions(IReadOnlyDictionary<string, string> values) : AnalyzerConfigOptions
{
    private readonly IReadOnlyDictionary<string, string> _values = values;

    public override bool TryGetValue(string key, out string value)
        => _values.TryGetValue(key, out value!);
}

internal static class RazorCompilerPathResolver
{
    public static string Resolve()
    {
        var sdkVersion = ReadSdkVersionFromGlobalJson()
            ?? throw new InvalidOperationException("global.json with sdk.version was not found from the current directory upward.");

        foreach (var root in EnumerateDotNetRoots())
        {
            foreach (var candidateVersion in EnumerateSdkVersions(root, sdkVersion))
            {
                var candidate = Path.Combine(
                    root,
                    "sdk",
                    candidateVersion,
                    "Sdks",
                    "Microsoft.NET.Sdk.Razor",
                    "source-generators",
                    "Microsoft.CodeAnalysis.Razor.Compiler.dll");
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        throw new FileNotFoundException("Microsoft.CodeAnalysis.Razor.Compiler.dll was not found for SDK " + sdkVersion + ".");
    }

    private static IEnumerable<string> EnumerateSdkVersions(string root, string requestedVersion)
    {
        yield return requestedVersion;

        var sdkRoot = Path.Combine(root, "sdk");
        if (!Directory.Exists(sdkRoot))
            yield break;

        foreach (var directory in Directory.EnumerateDirectories(sdkRoot, "11.0.100-preview.*")
                     .OrderByDescending(static path => path, StringComparer.Ordinal))
        {
            var version = Path.GetFileName(directory);
            if (!string.Equals(version, requestedVersion, StringComparison.Ordinal))
                yield return version;
        }
    }

    private static IEnumerable<string> EnumerateDotNetRoots()
    {
        var seen = new HashSet<string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

        foreach (var variable in new[] { "DOTNET_ROOT", "DOTNET_ROOT_X64", "DOTNET_ROOT_ARM64", "DOTNET_ROOT(x86)" })
        {
            if (TryAddRoot(Environment.GetEnvironmentVariable(variable), seen, out var root))
            {
                yield return root;
            }
        }

        var dotnetInfoRoot = TryGetDotNetRootFromInfo();
        if (TryAddRoot(dotnetInfoRoot, seen, out var infoRoot))
        {
            yield return infoRoot;
        }

        if (OperatingSystem.IsWindows())
        {
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            if (TryAddRoot(Path.Combine(programFiles, "dotnet"), seen, out var programFilesRoot))
            {
                yield return programFilesRoot;
            }

            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (TryAddRoot(Path.Combine(programFilesX86, "dotnet"), seen, out var x86Root))
            {
                yield return x86Root;
            }
        }
        else
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (TryAddRoot(Path.Combine(home, ".dotnet"), seen, out var homeRoot))
            {
                yield return homeRoot;
            }

            foreach (var candidate in new[] { "/usr/share/dotnet", "/usr/local/share/dotnet", "/usr/lib/dotnet", "/usr/lib64/dotnet", "/opt/dotnet" })
            {
                if (TryAddRoot(candidate, seen, out var root))
                {
                    yield return root;
                }
            }
        }
    }

    private static bool TryAddRoot(string? path, ISet<string> seen, out string root)
    {
        root = string.Empty;
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        try
        {
            root = Path.GetFullPath(path);
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or NotSupportedException)
        {
            return false;
        }

        return Directory.Exists(root) && seen.Add(root);
    }

    private static string? ReadSdkVersionFromGlobalJson()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            var globalJsonPath = Path.Combine(directory.FullName, "global.json");
            if (File.Exists(globalJsonPath))
            {
                using var stream = File.OpenRead(globalJsonPath);
                using var document = JsonDocument.Parse(stream);
                if (document.RootElement.TryGetProperty("sdk", out var sdk)
                    && sdk.TryGetProperty("version", out var version)
                    && !string.IsNullOrWhiteSpace(version.GetString()))
                {
                    return version.GetString();
                }
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static string? TryGetDotNetRootFromInfo()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--info",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            if (process is null)
            {
                return null;
            }

            var output = process.StandardOutput.ReadToEnd();
            _ = process.StandardError.ReadToEnd();
            if (!process.WaitForExit(milliseconds: 3_000))
            {
                process.Kill(entireProcessTree: true);
                return null;
            }

            foreach (var line in output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = line.Trim();
                if (!trimmed.StartsWith("Base Path:", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var basePath = trimmed["Base Path:".Length..].Trim().Trim('"');
                var normalized = basePath.Replace('\\', '/').TrimEnd('/');
                var sdkIndex = normalized.LastIndexOf("/sdk/", StringComparison.OrdinalIgnoreCase);
                if (sdkIndex <= 0)
                {
                    continue;
                }

                var root = normalized[..sdkIndex];
                return basePath.Contains('\\', StringComparison.Ordinal)
                    ? root.Replace('/', '\\')
                    : root;
            }
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception or IOException)
        {
        }

        return null;
    }
}
