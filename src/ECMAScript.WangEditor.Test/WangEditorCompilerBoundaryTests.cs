using System.Text.RegularExpressions;
using System.Threading;
using Basic.Reference.Assemblies;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECMAScriptWangEditorTest;

/// <summary>
/// Compiler emission checks for WangEditor authoring: entry calls, option object literals, and
/// string-enum domains.
/// 编译器发射检查：入口调用、选项对象字面量与字符串枚举值域。
/// </summary>
[TestClass]
public sealed class WangEditorCompilerBoundaryTests
{
    [TestMethod]
    public async Task WangEditor_ModeAndOptions_EmitAdapterModule()
    {
        // 组件渲染由 RazorVue 层负责；本层锁定适配器入口模块与选项字面量的形状。
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.WangEditor;

            public static class TestClass
            {
                public static WangEditorMode Mode()
                {
                    return WangEditorMode.Simple;
                }
            }
            """);

        StringAssert.Contains(script, "\"simple\"");
    }

    private static async Task<string> ConvertAsync(string code)
    {
        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(WangEditor).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        Assert.IsNotNull(module);
        // 归一化无关空白，使断言锁定发射语义而非换行形式。
        return Regex.Replace(module.ToKnRECMAScript(), @"\s+", " ");
    }

    private static void AssertImport(string script, string specifier, params string[] names)
    {
        var ordered = names.Order(StringComparer.Ordinal).ToArray();
        StringAssert.Contains(
            script,
            $"import {{ {string.Join(", ", ordered)} }} from \"{specifier}\";",
            $"Missing consolidated import of {string.Join(", ", ordered)} from {specifier}.");
    }

    private static (INamedTypeSymbol, SemanticModel) CompileAndGetSymbol(
        string code,
        string className,
        params MetadataReference[] additionalReferences)
    {
        var compilation = CSharpCompilation.Create(
            "ECMAScript.WangEditor.Test.Assembly",
            [CSharpSyntaxTree.ParseText(code, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview))],
            BuildCompilationReferences(additionalReferences),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var diagnostics = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsFalse(diagnostics.Length > 0, string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var classDeclaration = syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(node => node.Identifier.Text == className);

            if (classDeclaration is null)
                continue;

            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
            Assert.IsNotNull(classSymbol);
            return (classSymbol, semanticModel);
        }

        throw new InvalidOperationException($"Cannot locate class '{className}'.");
    }

    private static MetadataReference[] BuildCompilationReferences(IEnumerable<MetadataReference>? additionalReferences = null)
    {
        var references = CurrentRuntimeReferences().ToList();
        references.Add(MetadataReference.CreateFromFile(typeof(Number).Assembly.Location));
        if (additionalReferences is not null)
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
