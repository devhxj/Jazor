#!/usr/bin/env dotnet run
#:package Microsoft.CodeAnalysis.CSharp@5.7.0-1.26207.106
#:package Basic.Reference.Assemblies.Net110@1.8.4

using System.Collections;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

var sdkRoot = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
    "dotnet",
    "sdk",
    "11.0.100-preview.3.26207.106",
    "Sdks",
    "Microsoft.NET.Sdk.Razor",
    "source-generators",
    "Microsoft.CodeAnalysis.Razor.Compiler.dll");
var razorAssembly = Assembly.LoadFrom(sdkRoot);
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
        ["build_property.MSBuildProjectDirectory"] = projectDirectory,
        ["build_property.EnableRazorHostOutputs"] = "true"
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
driver = driver.RunGenerators(compilation);

var result = driver.GetRunResult().Results.Single();
var hostOutputs = result.GetType().GetProperty("HostOutputs")!.GetValue(result)!;
foreach (var entry in (IEnumerable)hostOutputs)
{
    var key = entry.GetType().GetProperty("Key")?.GetValue(entry);
    var value = entry.GetType().GetProperty("Value")?.GetValue(entry);
    Console.WriteLine("HostOutput: " + key + " -> " + value?.GetType().FullName);
    if (value is null)
        continue;

    Console.WriteLine("Properties:");
    foreach (var property in value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        Console.WriteLine("  " + property.PropertyType.FullName + " " + property.Name);

    Console.WriteLine("Methods:");
    foreach (var method in value.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).OrderBy(static item => item.Name, StringComparer.Ordinal))
        Console.WriteLine("  " + method);

    var codeDocument = value.GetType().GetMethod("GetCodeDocument", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, [typeof(string)], null)?.Invoke(value, [documentPath]);
    Console.WriteLine("CodeDocument: " + codeDocument?.GetType().FullName);
    if (codeDocument is null)
        continue;

    foreach (var method in codeDocument.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(static item => item.Name.Contains("CSharp", StringComparison.Ordinal)))
        Console.WriteLine("  CodeDocument method " + method);
    foreach (var property in codeDocument.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(static item => item.Name.Contains("CSharp", StringComparison.Ordinal)))
        Console.WriteLine("  CodeDocument property " + property.PropertyType.FullName + " " + property.Name);
}

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
