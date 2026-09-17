using System.Text.RegularExpressions;
using System.Threading;
using Basic.Reference.Assemblies;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECMAScriptVueI18nTest;

/// <summary>
/// Compiler emission checks for vue-i18n authoring: createI18n/useI18n entry calls, option
/// object literals, string-enum scope, and composer message lookup / formatting.
/// 编译器发射检查：createI18n/useI18n 入口、选项对象字面量、字符串枚举作用域与 Composer 消息查找/格式化。
/// </summary>
[TestClass]
public sealed class VueI18nCompilerBoundaryTests
{
    [TestMethod]
    public async Task VueI18n_CreateAndUse_EmitEntryCallsWithOptions()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.VueI18n;

            public static class TestClass
            {
                public static VueI18nInstance Build()
                {
                    return CreateI18n(new VueI18nCreateOptions
                    {
                        Locale = "en-US",
                        FallbackLocale = "en-US"
                    });
                }

                public static VueI18nComposer Current()
                {
                    return UseI18n(new VueI18nUseOptions { UseScope = VueI18nScope.Global });
                }
            }
            """);

        StringAssert.Contains(script, "return createI18n({ locale: \"en-US\", fallbackLocale: \"en-US\" });");
        StringAssert.Contains(script, "return useI18n({ useScope: \"global\" });");
        AssertImport(script, "vue-i18n", "createI18n", "useI18n");
    }

    [TestMethod]
    public async Task VueI18n_ComposerLookupAndFormatting_EmitCalls()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.VueI18n;

            public static class TestClass
            {
                public static string Hello(VueI18nComposer composer)
                {
                    return composer.T("greeting.hello");
                }

                public static string Counted(VueI18nComposer composer, Number count)
                {
                    return composer.T("cart.items", count);
                }

                public static bool Has(VueI18nComposer composer)
                {
                    return composer.Te("greeting.hello");
                }

                public static string When(VueI18nComposer composer, Date value)
                {
                    return composer.D(value, "short");
                }
            }
            """);

        StringAssert.Contains(script, "return composer.t(\"greeting.hello\");");
        StringAssert.Contains(script, "return composer.t(\"cart.items\", count);");
        StringAssert.Contains(script, "return composer.te(\"greeting.hello\");");
        StringAssert.Contains(script, "return composer.d(value, \"short\");");
    }

    [TestMethod]
    public async Task VueI18n_LocaleRefSwitch_EmitWritableRefReads()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.VueI18n;

            public static class TestClass
            {
                public static void Switch(VueI18nComposer composer, string locale)
                {
                    composer.Locale.Value = locale;
                }

                public static string Read(VueI18nComposer composer)
                {
                    return composer.Locale.Value;
                }
            }
            """);

        StringAssert.Contains(script, "composer.locale.value = locale;");
        StringAssert.Contains(script, "return composer.locale.value;");
    }

    private static async Task<string> ConvertAsync(string code)
    {
        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(VueI18n).Assembly.Location));
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
            "ECMAScript.VueI18n.Test.Assembly",
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
