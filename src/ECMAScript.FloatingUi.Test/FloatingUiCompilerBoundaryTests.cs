using System.Threading;
using Basic.Reference.Assemblies;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECMAScriptFloatingUiTest;

/// <summary>
/// Compiler emission checks for Floating UI authoring: entry import, middleware chains,
/// option object literals, string enums, and the auto-update/compute-position call forms.
/// 编译器发射检查：入口导入、中间件链、选项对象字面量、字符串枚举与自动更新/计算调用的形态。
/// </summary>
[TestClass]
public sealed class FloatingUiCompilerBoundaryTests
{
    [TestMethod]
    public async Task FloatingUi_UseFloating_EmitsAuthorEntryImportAndMiddlewareChain()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.FloatingUi;
            using static ECMAScript.Vue;

            public static class TestClass
            {
                public static FloatingUseReturn Build(VueReadonlyRef<Element?> reference, VueReadonlyRef<Element?> floating)
                {
                    return UseFloating(reference, floating, new FloatingUseOptions
                    {
                        Placement = FloatingPlacement.BottomStart,
                        Middleware = [Offset(8), Flip(), Shift()]
                    });
                }
            }
            """);

        StringAssert.Contains(script, "return useFloating(reference, floating, { placement: \"bottom-start\", middleware: [offset(8), flip(), shift()] });");
        AssertImport(script, "@floating-ui/vue", "useFloating", "offset", "flip", "shift");
    }

    [TestMethod]
    public async Task FloatingUi_MiddlewareFactories_EmitNumericAndObjectOffsets()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.FloatingUi;

            public static class TestClass
            {
                public static FloatingMiddleware Numeric()
                {
                    return Offset(12);
                }

                public static FloatingMiddleware Axes()
                {
                    return Offset(new FloatingOffsetValue { MainAxis = 4, CrossAxis = 2 });
                }
            }
            """);

        StringAssert.Contains(script, "return offset(12);");
        StringAssert.Contains(script, "return offset({ mainAxis: 4, crossAxis: 2 });");
        AssertImport(script, "@floating-ui/vue", "offset");
    }

    [TestMethod]
    public async Task FloatingUi_AutoUpdateAndComputePosition_EmitCleanupAndPromiseForms()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.FloatingUi;

            public static class TestClass
            {
                public static System.Action Bind(Element reference, Element floating, System.Action update)
                {
                    return AutoUpdate(reference, floating, update, new FloatingAutoUpdateOptions
                    {
                        AncestorScroll = true,
                        ElementResize = false
                    });
                }

                public static IPromise<FloatingComputePositionReturn> Compute(Element reference, Element floating)
                {
                    return ComputePosition(reference, floating, new FloatingComputePositionConfig
                    {
                        Placement = FloatingPlacement.Top,
                        Strategy = FloatingStrategy.Fixed
                    });
                }
            }
            """);

        StringAssert.Contains(script, "return autoUpdate(reference, floating, update, { ancestorScroll: true, elementResize: false });");
        StringAssert.Contains(script, "return computePosition(reference, floating, { placement: \"top\", strategy: \"fixed\" });");
        AssertImport(script, "@floating-ui/vue", "autoUpdate", "computePosition");
    }

    [TestMethod]
    public async Task FloatingUi_ShiftOptions_EmitBoundaryUnionAndRootBoundary()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.FloatingUi;

            public static class TestClass
            {
                public static FloatingMiddleware Build(Element boundary)
                {
                    return Shift(new FloatingShiftOptions
                    {
                        MainAxis = true,
                        Boundary = boundary,
                        RootBoundary = FloatingRootBoundary.Viewport,
                        Padding = 8
                    });
                }
            }
            """);

        StringAssert.Contains(script, "return shift({ mainAxis: true, boundary: boundary, rootBoundary: \"viewport\", padding: 8 });");
        AssertImport(script, "@floating-ui/vue", "shift");
    }

    private static async Task<string> ConvertAsync(string code)
    {
        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(FloatingUi).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        Assert.IsNotNull(module);
        // Collapse insignificant whitespace so assertions pin the emitted expression, not its
        // line wrapping, which the writer may change without altering semantics.
        // 归一化无关空白，使断言锁定发射语义而非换行形式。
        return System.Text.RegularExpressions.Regex.Replace(module.ToKnRECMAScript(), @"\s+", " ");
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
            "ECMAScript.FloatingUi.Test.Assembly",
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
