using System.Text.RegularExpressions;
using System.Threading;
using Basic.Reference.Assemblies;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECMAScriptVueDraggableTest;

/// <summary>
/// Compiler emission checks for vue-draggable-plus authoring: composable call, option object
/// literal, and the component-proxy render shape.
/// 编译器发射检查：组合式函数调用、选项对象字面量与组件代理渲染形态。
/// </summary>
[TestClass]
public sealed class VueDraggableCompilerBoundaryTests
{
    [TestMethod]
    public async Task VueDraggable_Composable_EmitsCallWithOptions()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.VueDraggable;
            using static ECMAScript.Vue;

            public static class TestClass
            {
                public static VueDraggableReturn Build(Element element, IVueRef<string[]> items)
                {
                    return UseDraggable<string>(element, items, new VueDraggableOptions
                    {
                        Animation = 150,
                        Handle = ".handle",
                        Direction = VueDraggableDirection.Vertical,
                        GhostClass = "drag-ghost"
                    });
                }
            }
            """);

        StringAssert.Contains(script, "useDraggable(element, items, {");
        StringAssert.Contains(script, "animation: 150");
        StringAssert.Contains(script, "handle: \".handle\"");
        StringAssert.Contains(script, "direction: \"vertical\"");
        StringAssert.Contains(script, "ghostClass: \"drag-ghost\"");
        AssertImport(script, "vue-draggable-plus", "useDraggable");
    }

    [TestMethod]
    public async Task VueDraggable_ControlEntries_EmitMethodCalls()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.VueDraggable;

            public static class TestClass
            {
                public static void Control(VueDraggableReturn control)
                {
                    control.Pause();
                    control.Resume();
                    control.Destroy();
                }
            }
            """);

        StringAssert.Contains(script, "control.pause();");
        StringAssert.Contains(script, "control.resume();");
        StringAssert.Contains(script, "control.destroy();");
    }

    private static async Task<string> ConvertAsync(string code)
    {
        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(VueDraggable).Assembly.Location));
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
            "ECMAScript.VueDraggable.Test.Assembly",
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
