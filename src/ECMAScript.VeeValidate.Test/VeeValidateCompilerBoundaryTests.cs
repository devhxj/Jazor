using System.Text.RegularExpressions;
using System.Threading;
using Basic.Reference.Assemblies;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECMAScriptVeeValidateTest;

/// <summary>
/// Compiler emission checks for vee-validate authoring: form/field composables, option object
/// literals, state-read refs, and the writable form context write entries.
/// 编译器发射检查：表单/字段组合式函数、选项对象字面量、状态读取引用与可写表单上下文写入入口。
/// </summary>
[TestClass]
public sealed class VeeValidateCompilerBoundaryTests
{
    [TestMethod]
    public async Task VeeValidate_FormAndFieldComposables_EmitCallsWithOptions()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.VeeValidate;

            public record LoginForm
            {
                public string Email { get; init; } = "";
            }

            public static class TestClass
            {
                public static VeeValidateFormReturn<LoginForm> Build()
                {
                    return UseForm(new VeeValidateFormOptions<LoginForm>
                    {
                        InitialValues = new LoginForm { Email = "a@b.c" },
                        ValidateOnMount = false
                    });
                }

                public static VeeValidateFieldReturn<string> Field()
                {
                    return UseField<string>("email", "required|email");
                }
            }
            """);

        StringAssert.Contains(script, "return useForm({ initialValues: { Email: \"a@b.c\" }, validateOnMount: false });");
        StringAssert.Contains(script, "return useField(\"email\", \"required|email\");");
        AssertImport(script, "vee-validate", "useForm", "useField");
    }

    [TestMethod]
    public async Task VeeValidate_StateReads_EmitRefReads()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.VeeValidate;

            public static class TestClass
            {
                public static bool Valid()
                {
                    return UseIsFormValid().Value;
                }

                public static string Value()
                {
                    return UseFieldValue<string>("email").Value;
                }

                public static double Count()
                {
                    return UseSubmitCount().Value;
                }
            }
            """);

        StringAssert.Contains(script, "return useIsFormValid().value;");
        StringAssert.Contains(script, "return useFieldValue(\"email\").value;");
        StringAssert.Contains(script, "return useSubmitCount().value;");
        AssertImport(script, "vee-validate", "useIsFormValid", "useFieldValue", "useSubmitCount");
    }

    [TestMethod]
    public async Task VeeValidate_FormContextWriteEntries_EmitMethodCalls()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.VeeValidate;

            public record LoginForm
            {
                public string Email { get; init; } = "";
            }

            public static class TestClass
            {
                public static void Setup(VeeValidateFormReturn<LoginForm> form)
                {
                    form.SetFieldValue("email", "x@y.z");
                    form.SetFieldError("email", "bad");
                    form.SetFieldTouched("email", true);
                }
            }
            """);

        StringAssert.Contains(script, "form.setFieldValue(\"email\", \"x@y.z\");");
        StringAssert.Contains(script, "form.setFieldError(\"email\", \"bad\");");
        StringAssert.Contains(script, "form.setFieldTouched(\"email\", true);");
    }

    [TestMethod]
    public async Task VeeValidate_SubmitHandler_EmitValidationFirstHandler()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.VeeValidate;

            public record LoginForm
            {
                public string Email { get; init; } = "";
            }

            public static class TestClass
            {
                public static VeeValidateSubmitHandler Bind(VeeValidateFormReturn<LoginForm> form)
                {
                    return form.HandleSubmit(values => { });
                }
            }
            """);

        StringAssert.Contains(script, "return form.handleSubmit(");
        StringAssert.Contains(script, "values =>");
    }

    private static async Task<string> ConvertAsync(string code)
    {
        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(VeeValidate).Assembly.Location));
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
            "ECMAScript.VeeValidate.Test.Assembly",
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
