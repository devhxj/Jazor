#:package Microsoft.CodeAnalysis.CSharp@5.7.0-1.26207.106

using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

var repoRoot = Directory.GetCurrentDirectory();
var nupkgExtractRoot = Path.Combine(repoRoot, ".tmp", "Jazor.EmitTest", "nupkg-inspect-csharp");
if (!Directory.Exists(nupkgExtractRoot))
{
    var nupkgPath = Directory.EnumerateFiles(Path.Combine(repoRoot, ".tmp", "Jazor.EmitTest", "nupkg"), "Jazor.*.nupkg", SearchOption.AllDirectories)
        .OrderByDescending(File.GetLastWriteTimeUtc)
        .First();
    System.IO.Compression.ZipFile.ExtractToDirectory(nupkgPath, nupkgExtractRoot);
}

var libRoot = Path.Combine(nupkgExtractRoot, "lib", "net11.0");
var analyzerRoot = Path.Combine(nupkgExtractRoot, "analyzers", "dotnet", "cs");
var code = """
    using ECMAScript;
    using static ECMAScript.Vue3;
    using static ECMAScript.VueRoute;

    namespace Probe;

    [ECMAScriptModule("host/app.mjs")]
    public static class AppModule
    {
        public static string Build(Router router)
        {
            var injectedRouter = Inject(VueRoute.RouterKey)!;
            return injectedRouter.CurrentRoute.Value.Path + router.CurrentRoute.Value.Path;
        }
    }
    """;

var references = Directory.EnumerateFiles(libRoot, "*.dll")
    .Select(static path => MetadataReference.CreateFromFile(path))
    .Concat(Directory.EnumerateFiles(GetNetCoreAppRefRoot(), "*.dll")
        .Select(static path => MetadataReference.CreateFromFile(path)))
    .Cast<MetadataReference>()
    .ToArray();

var compilation = CSharpCompilation.Create(
    "Probe",
    [CSharpSyntaxTree.ParseText(code, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview))],
    references,
    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

foreach (var diagnostic in compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
{
    Console.WriteLine(diagnostic);
}

var routerType = compilation.GetTypeByMetadataName("ECMAScript.Router")!;
var currentRoute = routerType.GetMembers("CurrentRoute").OfType<IPropertySymbol>().Single();
var getter = currentRoute.GetMethod!;

PrintSymbol("Router", routerType);
PrintSymbol("CurrentRoute", currentRoute);
PrintSymbol("CurrentRoute.get", getter);

var syntaxRoot = compilation.SyntaxTrees.Single().GetRoot();
var semanticModel = compilation.GetSemanticModel(compilation.SyntaxTrees.Single());
foreach (var node in syntaxRoot.DescendantNodes().Where(static node => node.ToString().EndsWith(".CurrentRoute", StringComparison.Ordinal)))
{
    var symbol = semanticModel.GetSymbolInfo(node).Symbol;
    PrintSymbol($"Use site: {node}", symbol);
}

AssemblyLoadContext.Default.Resolving += (_, assemblyName) =>
{
    foreach (var root in new[] { analyzerRoot, libRoot })
    {
        var candidate = Path.Combine(root, $"{assemblyName.Name}.dll");
        if (File.Exists(candidate))
            return AssemblyLoadContext.Default.LoadFromAssemblyPath(candidate);
    }

    return null;
};

var analyzerAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(analyzerRoot, "Jazor.Analyzer.dll"));
var analyzerType = analyzerAssembly.GetType("Jazor.Analyzer.Analyzer", throwOnError: true)!;
var inEcmaScriptAttribute = analyzerType.GetMethod("InECMAScriptAttribute", BindingFlags.NonPublic | BindingFlags.Static)!;
var hasEcmaScriptAttribute = analyzerType.GetMethod("HasECMAScriptAttribute", BindingFlags.NonPublic | BindingFlags.Static)!;
Console.WriteLine("[Analyzer private predicates]");
Console.WriteLine($"  HasECMAScriptAttribute(Router)={hasEcmaScriptAttribute.Invoke(null, [routerType])}");
Console.WriteLine($"  InECMAScriptAttribute(Router)={inEcmaScriptAttribute.Invoke(null, [routerType])}");
Console.WriteLine($"  InECMAScriptAttribute(CurrentRoute.ContainingType)={inEcmaScriptAttribute.Invoke(null, [currentRoute.ContainingType])}");
var analyzer = (DiagnosticAnalyzer)Activator.CreateInstance(analyzerType)!;
var analyzerDiagnostics = await compilation.WithAnalyzers([analyzer]).GetAnalyzerDiagnosticsAsync();
Console.WriteLine("[Analyzer diagnostics]");
foreach (var diagnostic in analyzerDiagnostics.OrderBy(static diagnostic => diagnostic.Location.SourceSpan.Start))
{
    Console.WriteLine($"  {diagnostic.Id}: {diagnostic.GetMessage()} @ {diagnostic.Location.GetLineSpan()}");
}

static string GetNetCoreAppRefRoot()
{
    var runtimeRoot = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
    var version = Path.GetFileName(runtimeRoot);
    var dotnetRoot = Directory.GetParent(runtimeRoot)!.Parent!.Parent!.FullName;
    var refRoot = Path.Combine(dotnetRoot, "packs", "Microsoft.NETCore.App.Ref", version, "ref", "net11.0");
    if (Directory.Exists(refRoot))
        return refRoot;

    var packRoot = Path.Combine(dotnetRoot, "packs", "Microsoft.NETCore.App.Ref");
    return Directory.EnumerateDirectories(packRoot)
        .OrderByDescending(static path => path, StringComparer.OrdinalIgnoreCase)
        .Select(static path => Path.Combine(path, "ref", "net11.0"))
        .First(Directory.Exists);
}

static void PrintSymbol(string title, ISymbol? symbol)
{
    Console.WriteLine($"[{title}]");
    if (symbol is null)
    {
        Console.WriteLine("  <null>");
        return;
    }

    Console.WriteLine($"  Display={symbol.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}");
    Console.WriteLine($"  NameFormat={symbol.OriginalDefinition.ToDisplayString(JazorNameFormat())}");
    Console.WriteLine($"  ContainingAssembly={symbol.ContainingAssembly?.Identity}");
    Console.WriteLine($"  ContainingType={symbol.ContainingType?.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}");
    Console.WriteLine("  Attributes:");
    foreach (var attribute in symbol.GetAttributes())
    {
        Console.WriteLine($"    {attribute.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} | {attribute.AttributeClass?.ToDisplayString()}");
    }
}

static SymbolDisplayFormat JazorNameFormat()
    => new(
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        memberOptions: SymbolDisplayMemberOptions.IncludeContainingType | SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeType,
        parameterOptions: SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes);
