using System.Threading;
using Acornima.Ast;
using Basic.Reference.Assemblies;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECMAScriptVueRouteTest;

[TestClass]
public sealed class EcmaScriptVueRouteCompilerBoundaryTests
{
    [TestMethod]
    public async Task VueRoute_HostTypes_AreRecognizedByCompiler_ForRouteObjectConstruction()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static RouteLocationAsPath BuildLocation()
                {
                    return new RouteLocationAsPath
                    {
                        Path = "/users",
                        Hash = "#list"
                    };
                }

                public static RouterOptions BuildOptions(RouterHistory history)
                {
                    return new RouterOptions
                    {
                        History = history,
                        Routes =
                        [
                            new RouteRecordRedirect
                            {
                                Path = "/",
                                Redirect = "/home"
                            }
                        ]
                    };
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "return { path: \"/users\", hash: \"#list\" };");
        StringAssert.Contains(script, "redirect: \"/home\"");
        StringAssert.Contains(script, "history: history");
    }

    [TestMethod]
    public async Task VueRoute_CurrentRoutePathAccess_CompilesThroughVueReadonlyRefValue()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.VueRoute;

            public static class TestClass
            {
                public static string CurrentPath()
                {
                    return VueRoute.UseRouter().CurrentRoute.Value.Path;
                }
            }
            """;

        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.VueRoute).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "useRouter");
        StringAssert.Contains(script, "currentRoute.value.path");
    }

    private static MetadataReference[] BuildCompilationReferences(IEnumerable<MetadataReference>? additionalReferences = null)
    {
        var references = Net100.References.All.Cast<MetadataReference>().ToList();
        if (additionalReferences is not null)
            references.AddRange(additionalReferences);

        return references.ToArray();
    }

    private static SyntaxTree[] BuildSyntaxTrees(string code)
        => new[] { CSharpSyntaxTree.ParseText(code) };

    private static (INamedTypeSymbol, SemanticModel) CompileAndGetSymbol(
        string code,
        string className,
        params MetadataReference[] additionalReferences)
    {
        var compilation = CSharpCompilation.Create(
            "ECMAScript.VueRoute.Test.Assembly",
            BuildSyntaxTrees(code),
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
}
