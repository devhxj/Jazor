#!/usr/bin/env dotnet run
#:property PublishAot=false
#:package Microsoft.CodeAnalysis.CSharp@5.11.0-1.26425.128

using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Run from the repository root. --no-build checks existing Debug output.
// --packages DIR also validates the XML inside every shipping binding package.
// --baseline REF verifies that this documentation-only change preserves C# tokens.
// --library NAME[,NAME] selects binding or ASP.NET Core libraries for focused checks.
var root = Directory.GetCurrentDirectory();
if (!File.Exists(Path.Combine(root, "Jazor.slnx")))
    throw new InvalidOperationException("Run from the repository root.");
var libraries = new[]
{
    "ECMAScript", "ECMAScript.Contract", "ECMAScript.Vue", "ECMAScript.VueContract",
    "ECMAScript.Pinia", "ECMAScript.Pinia.Testing", "ECMAScript.VueRoute",
    "ECMAScript.DateFns", "ECMAScript.Vue.Devtools", "ECMAScript.VueDataUi", "ECMAScript.VuIcons",
    "ECMAScript.Style", "ECMAScript.ElementPlus", "ECMAScript.Vuetify", "ECMAScript.TDesign",
    "Jazor.AspNetCore", "Jazor.AspNetCore.Dev"
};
if (Option("--library") is { } selection)
{
    var selected = selection.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
    if (selected.Length == 0 || selected.Except(libraries).Any())
        throw new ArgumentException("--library must name known documentation libraries: " + string.Join(", ", libraries));
    libraries = selected;
}
var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview)
    .WithDocumentationMode(DocumentationMode.Diagnose);
var configuration = Option("--configuration") ?? "Debug";
var output = Path.Combine(root, "artifacts", "binding-documentation");
Directory.CreateDirectory(output);
var failures = new List<string>();
var results = new List<object>();
var documentedXml = new Dictionary<string, (string Path, XDocument Document)>();
foreach (var library in libraries)
{
    var projectDir = Path.Combine(root, "src", library);
    var projectPath = Path.Combine(projectDir, library + ".csproj");
    var project = XDocument.Load(projectPath);
    if (!project.Descendants("GenerateDocumentationFile").Any(x => x.Value == "true"))
        failures.Add(library + ": GenerateDocumentationFile must be true.");
    foreach (var nuspecPath in Directory.EnumerateFiles(projectDir, "*.nuspec", SearchOption.TopDirectoryOnly))
    {
        var nuspec = XDocument.Load(nuspecPath);
        var xmlFiles = nuspec.Descendants().Where(e => e.Name.LocalName == "file")
            .Select(e => e.Attribute("src")?.Value ?? string.Empty)
            .Where(value => value.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)).ToArray();
        if (library is not "ECMAScript" && library is not "ECMAScript.Contract" &&
            !xmlFiles.Any(value => value.Contains(library + ".xml", StringComparison.OrdinalIgnoreCase)))
            failures.Add(library + ": nuspec must ship its XML documentation file.");
    }
    var declarations = new List<(string Key, string Location, bool Documented)>();
    var enumValues = 0;
    foreach (var path in Directory.EnumerateFiles(projectDir, "*.cs", SearchOption.AllDirectories)
        .Where(p => !Path.GetRelativePath(projectDir, p).Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj")))
    {
        var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(path), parseOptions, path);
        foreach (var diagnostic in tree.GetDiagnostics().Where(d => d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning))
            failures.Add(diagnostic.ToString());
        foreach (var member in tree.GetRoot().DescendantNodes().OfType<MemberDeclarationSyntax>().Where(IsVisible))
        {
            var docs = member.GetLeadingTrivia().Select(t => t.GetStructure()).OfType<DocumentationCommentTriviaSyntax>().ToArray();
            var elements = docs.SelectMany(d => d.Content.OfType<XmlElementSyntax>()).ToArray();
            var summaries = elements.Where(e => e.StartTag.Name.LocalName.ValueText == "summary").ToArray();
            var inherited = docs.Any(d => d.DescendantNodes().OfType<XmlEmptyElementSyntax>()
                .Any(e => e.Name.LocalName.ValueText == "inheritdoc"));
            var location = Path.GetRelativePath(root, path).Replace('\\', '/') + ":" + (tree.GetLineSpan(member.Span).StartLinePosition.Line + 1);
            bool HasText(XmlElementSyntax element) => element.Content.Any(c =>
                !string.IsNullOrWhiteSpace(Regex.Replace(c.ToString(), @"(?m)^\s*///\s?", "")));
            foreach (var element in elements.Where(e => e.StartTag.Name.LocalName.ValueText is "summary" or "param" or "typeparam" or "returns"))
                if (!HasText(element)) failures.Add(location + ": empty <" + element.StartTag.Name + ">.");
            var key = member is BaseTypeDeclarationSyntax type
                ? string.Join(".", member.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().Reverse().Select(n => n.Name.ToString())
                    .Concat(member.AncestorsAndSelf().OfType<BaseTypeDeclarationSyntax>().Reverse()
                    .Select(t => t.Identifier.ValueText + (t is TypeDeclarationSyntax td ? td.TypeParameterList?.ToString() : ""))))
                : location;
            declarations.Add((key, location, inherited || summaries.Any(HasText)));
            if (member is EnumMemberDeclarationSyntax) enumValues++;
        }
    }
    foreach (var group in declarations.GroupBy(d => d.Key).Where(g => !g.Any(d => d.Documented)))
        failures.Add(group.First().Location + ": public API requires a nonempty summary or inheritdoc: " + group.Key);

    if (!args.Contains("--no-build"))
    {
        // Force XML generation even if an incremental build would otherwise reuse an old file.
        var build = await Run("dotnet", "build", projectPath, "-c", configuration, "-v:q", "-t:Rebuild",
            "-p:BuildProjectReferences=false", "-p:WarningsAsErrors=CS0419%3BCS1570%3BCS1571%3BCS1572%3BCS1573%3BCS1574%3BCS1580%3BCS1581%3BCS1584%3BCS1587%3BCS1591%3BCS1710%3BCS1711%3BCS1712");
        File.WriteAllText(Path.Combine(output, library + ".build.log"), build.Output);
        if (build.Code != 0) failures.Add(library + ": build failed; see " + library + ".build.log.");
    }
    var tfm = project.Descendants("TargetFramework").Single().Value;
    var xmlPath = Path.Combine(projectDir, "bin", configuration, tfm, library + ".xml");
    var xmlCount = 0;
    if (!File.Exists(xmlPath)) failures.Add(library + ": XML output missing: " + xmlPath);
    else
    {
        var xml = XDocument.Load(xmlPath);
        var members = xml.Descendants("member").ToArray();
        xmlCount = members.Length;
        if (xmlCount == 0) failures.Add(library + ": XML has no members.");
        if (xml.Descendants("see").Any(e => e.Attribute("cref")?.Value.StartsWith("!:", StringComparison.Ordinal) == true))
            failures.Add(library + ": XML contains unresolved cref references.");
        documentedXml.Add(library, (xmlPath, xml));
    }
    var total = declarations.Select(d => d.Key).Distinct().Count();
    results.Add(new { Library = library, PublicDeclarations = total, EnumValues = enumValues, XmlMembers = xmlCount });
    Console.WriteLine($"{library}: {total} public declarations, {enumValues} enum values, {xmlCount} XML members.");
}
if (Option("--packages") is { } packages)
{
    var archives = Directory.GetFiles(packages, "*.nupkg").Select(path =>
    {
        using var zip = ZipFile.OpenRead(path);
        using var nuspec = zip.Entries.Single(e => e.FullName.EndsWith(".nuspec", StringComparison.Ordinal)).Open();
        var id = XDocument.Load(nuspec).Descendants().Single(e => e.Name.LocalName == "id").Value;
        return (Id: id, Path: path);
    }).ToArray();
    foreach (var (library, built) in documentedXml)
    {
        var packageId = library switch
        {
            "ECMAScript" or "ECMAScript.Contract" or "Jazor.AspNetCore" or "Jazor.AspNetCore.Dev" => "Jazor",
            "ECMAScript.Vue" or "ECMAScript.VueContract" => "Jazor.Vue",
            _ => library
        };
        var candidates = archives.Where(p => p.Id == packageId).ToArray();
        if (candidates.Length != 1) { failures.Add(packageId + ": expected exactly one package."); continue; }
        using var zip = ZipFile.OpenRead(candidates[0].Path);
        var dll = zip.Entries.SingleOrDefault(e => e.FullName.StartsWith("lib/", StringComparison.Ordinal) && e.Name == library + ".dll");
        var xmlEntry = dll is null ? null : zip.GetEntry(Path.ChangeExtension(dll.FullName, ".xml"));
        if (xmlEntry is null) { failures.Add(packageId + ": missing XML alongside " + library + ".dll."); continue; }
        using var stream = xmlEntry.Open();
        if (!XNode.DeepEquals(built.Document, XDocument.Load(stream)))
            failures.Add(packageId + ": packaged XML differs from build output for " + library + ".");
    }
}
if (Option("--baseline") is { } baseline)
{
    var diff = await Run("git", "diff", "--name-only", baseline, "--", "src");
    if (diff.Code != 0) throw new InvalidOperationException(diff.Output);
    foreach (var path in diff.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()))
    {
        if (!path.EndsWith(".cs", StringComparison.Ordinal) || !libraries.Contains(path.Split('/')[1])) continue;
        var before = await Run("git", "show", baseline + ":" + path);
        if (before.Code != 0) { failures.Add(path + ": missing baseline source."); continue; }
        var oldTokens = CodeTokens(before.Output);
        var newTokens = CodeTokens(File.ReadAllText(path));
        if (!oldTokens.SequenceEqual(newTokens)) failures.Add(path + ": C# code tokens changed.");
    }
}
File.WriteAllText(Path.Combine(output, "report.json"),
    JsonSerializer.Serialize(new { Results = results, Failures = failures }, new JsonSerializerOptions { WriteIndented = true }));
foreach (var failure in failures.Take(30)) Console.Error.WriteLine(failure);
Console.WriteLine($"Documentation verification: {failures.Count} failures. Report: {Path.Combine(output, "report.json")}");
return failures.Count == 0 ? 0 : 1;

string? Option(string name)
{
    var index = Array.IndexOf(args, name);
    return index < 0 ? null : index + 1 < args.Length ? args[index + 1]
        : throw new ArgumentException("Missing value for " + name);
}
static bool IsVisible(MemberDeclarationSyntax member)
{
    if (member is NamespaceDeclarationSyntax or FileScopedNamespaceDeclarationSyntax or GlobalStatementSyntax or ExtensionBlockDeclarationSyntax) return false;
    if (member.Ancestors().OfType<BaseTypeDeclarationSyntax>()
        .Any(parent => parent is not ExtensionBlockDeclarationSyntax && !IsVisible(parent))) return false;
    if (member is EnumMemberDeclarationSyntax) return true;
    if (member.Modifiers.Any(SyntaxKind.PrivateKeyword)) return false;
    return member.Modifiers.Any(SyntaxKind.PublicKeyword) || member.Modifiers.Any(SyntaxKind.ProtectedKeyword)
        || member.Parent is InterfaceDeclarationSyntax;
}
static IEnumerable<(int RawKind, string Text)> CodeTokens(string source)
    => CSharpSyntaxTree.ParseText(string.Join("\n", source.Split('\n').Where(line =>
        !line.TrimStart().StartsWith("///", StringComparison.Ordinal) &&
        !line.Contains("[Category(\"optional\")]", StringComparison.Ordinal))), CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview)).GetRoot().DescendantTokens()
        .Select(token => (token.RawKind, token.Text));
static async Task<(int Code, string Output)> Run(string executable, params string[] arguments)
{
    var start = new ProcessStartInfo(executable) { RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
    foreach (var argument in arguments) start.ArgumentList.Add(argument);
    using var process = Process.Start(start)!;
    var stdout = process.StandardOutput.ReadToEndAsync();
    var stderr = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();
    return (process.ExitCode, await stdout + await stderr);
}
