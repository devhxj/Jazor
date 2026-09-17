using System.Text.RegularExpressions;
using System.Threading;
using Basic.Reference.Assemblies;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECMAScriptVueUseTest;

/// <summary>
/// Compiler emission checks for VueUse authoring: entry imports, ref access, option object
/// literals, string enums, generic composables, and callback/cleanup lowering.
/// 编译器发射检查：入口导入、ref 访问、选项对象字面量、字符串枚举、泛型 composable 与回调/清理 lowering。
/// </summary>
[TestClass]
public sealed class VueUseCompilerBoundaryTests
{
    [TestMethod]
    public async Task VueUse_RefReturningComposables_EmitValueAccessAndOptions()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.VueUse;

            public static class TestClass
            {
                public static double ReadMouse()
                {
                    var mouse = UseMouse(new VueUseMouseOptions
                    {
                        Type = VueUseMouseCoordType.Client,
                        Touch = true
                    });
                    return mouse.X.Value;
                }
            }
            """);

        StringAssert.Contains(script, "let mouse = useMouse({ type: \"client\", touch: true });");
        StringAssert.Contains(script, "return mouse.x.value;");
        AssertImport(script, "@vueuse/core", "useMouse");
    }

    [TestMethod]
    public async Task VueUse_ElementTargetedComposables_EmitUnionTargetPassthrough()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.Vue;
            using static ECMAScript.VueUse;

            public static class TestClass
            {
                public static double Size(VueReadonlyRef<Element?> target)
                {
                    return UseElementSize(target).Width.Value;
                }

                public static bool Visible(Element element)
                {
                    return UseElementVisibility(element).Value;
                }
            }
            """);

        StringAssert.Contains(script, "return useElementSize(target).width.value;");
        StringAssert.Contains(script, "return useElementVisibility(element).value;");
        AssertImport(script, "@vueuse/core", "useElementSize", "useElementVisibility");
    }

    [TestMethod]
    public async Task VueUse_StorageAndClipboard_EmitReactiveCalls()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.VueUse;

            public static class TestClass
            {
                public static string ReadStored()
                {
                    var stored = UseStorage("token", "");
                    return stored.Value;
                }

                public static string ReadClipboard()
                {
                    var clipboard = UseClipboard();
                    return clipboard.Text.Value;
                }

                public static bool Dark()
                {
                    return UseDark().Value;
                }
            }
            """);

        StringAssert.Contains(script, "let stored = useStorage(\"token\", \"\");");
        StringAssert.Contains(script, "return stored.value;");
        StringAssert.Contains(script, "let clipboard = useClipboard();");
        StringAssert.Contains(script, "return clipboard.text.value;");
        StringAssert.Contains(script, "return useDark().value;");
        AssertImport(script, "@vueuse/core", "useStorage", "useClipboard", "useDark");
    }

    [TestMethod]
    public async Task VueUse_EventAndTimingComposables_EmitCallbacksAndCleanup()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.VueUse;

            public static class TestClass
            {
                public static System.Action Bind(Element target)
                {
                    return OnClickOutside(target, (@event) => { });
                }

                public static VueUsePausableControl Tick()
                {
                    return UseIntervalFn(() => { }, 1000);
                }
            }
            """);

        StringAssert.Contains(script, "return onClickOutside(target, event => {");
        StringAssert.Contains(script, "return useIntervalFn(() => {");
        AssertImport(script, "@vueuse/core", "onClickOutside", "useIntervalFn");
    }

    [TestMethod]
    public async Task VueUse_GenericComposables_EmitTypedCallsWithoutObjectFallback()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.Vue;
            using static ECMAScript.VueUse;

            public static class TestClass
            {
                public static VueComputedRef<string> Async()
                {
                    return ComputedAsync(() => Promise<string>.Resolve("ready"), "loading");
                }

                public static VueUseEventHook<string> Hook()
                {
                    return CreateEventHook<string>();
                }
            }
            """);

        StringAssert.Contains(script, "return computedAsync(() => {");
        StringAssert.Contains(script, "return Promise.resolve(\"ready\");");
        StringAssert.Contains(script, "return createEventHook();");
        AssertImport(script, "@vueuse/core", "computedAsync", "createEventHook");
    }

    private static async Task<string> ConvertAsync(string code)
    {
        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(VueUse).Assembly.Location));
        var converter = new AstConverter(classSymbol, semanticModel);

        var module = await converter.Convert(CancellationToken.None);
        Assert.IsNotNull(module);
        // Collapse insignificant whitespace so assertions pin the emitted expression, not its wrapping.
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
            "ECMAScript.VueUse.Test.Assembly",
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
