using System.Text.RegularExpressions;
using System.Threading;
using Basic.Reference.Assemblies;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECMAScriptMonacoTest;

/// <summary>验证 Monaco C# API 发射为声明的 ESM 入口与上游成员名。</summary>
[TestClass]
public sealed class MonacoCompilerBoundaryTests
{
    [TestMethod]
    public async Task Monaco_CreateEditor_EmitsTheAuthoredPackageExport()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.Monaco;

            public static class TestClass
            {
                public static string Build(Element container)
                {
                    var editor = Create(container, new MonacoEditorConstructionOptions
                    {
                        Value = "const answer = 42;",
                        Language = "csharp"
                    });
                    return editor.GetValue();
                }
            }
            """);

        StringAssert.Contains(script, "import { create } from \"monaco-editor/editor/editor.api.js\";");
        StringAssert.Contains(script, "let editor = create(container, { value: \"const answer = 42;\", language: \"csharp\" });");
        StringAssert.Contains(script, "return editor.getValue();");
    }

    private static async Task<string> ConvertAsync(string code)
    {
        var compilation = CSharpCompilation.Create(
            "ECMAScript.Monaco.Test.Assembly",
            [CSharpSyntaxTree.ParseText(code, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview))],
            BuildCompilationReferences(
                MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Monaco).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var diagnostics = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsFalse(diagnostics.Length > 0, string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));

        var syntaxTree = compilation.SyntaxTrees.Single();
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var classDeclaration = syntaxTree.GetRoot().DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.Text == "TestClass");
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
        Assert.IsNotNull(classSymbol);

        var module = await new AstConverter(classSymbol, semanticModel).Convert(CancellationToken.None);
        Assert.IsNotNull(module);
        return Regex.Replace(module.ToKnRECMAScript(), @"\s+", " ");
    }

    private static MetadataReference[] BuildCompilationReferences(params MetadataReference[] additionalReferences)
    {
        var references = CurrentRuntimeReferences().ToList();
        references.Add(MetadataReference.CreateFromFile(typeof(Number).Assembly.Location));
        references.AddRange(additionalReferences);
        return references.ToArray();
    }

    private static IEnumerable<MetadataReference> CurrentRuntimeReferences()
    {
        var trustedPlatformAssemblies = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
        if (string.IsNullOrWhiteSpace(trustedPlatformAssemblies))
            return Net110.References.All.Cast<MetadataReference>();

        return trustedPlatformAssemblies
            .Split(Path.PathSeparator)
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(static path => MetadataReference.CreateFromFile(path));
    }
}
