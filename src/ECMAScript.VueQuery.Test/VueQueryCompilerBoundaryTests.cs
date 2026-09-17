using System.Text.RegularExpressions;
using System.Threading;
using Basic.Reference.Assemblies;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ECMAScriptVueQueryTest;

/// <summary>
/// Compiler emission checks for vue-query authoring: query/mutation composables, query-key
/// union arrays, option object literals, status refs, and the mutation write entries.
/// 编译器发射检查：查询/提交组合式函数、query-key 联合数组、选项对象字面量、状态引用与提交写入入口。
/// </summary>
[TestClass]
public sealed class VueQueryCompilerBoundaryTests
{
    [TestMethod]
    public async Task VueQuery_QueryComposable_EmitsQueryKeyUnionArrayAndOptions()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.VueQuery;

            public static class TestClass
            {
                public static VueQueryQueryReturn<string> Build()
                {
                    return UseQuery(new VueQueryQueryOptions<string>
                    {
                        QueryKey = [new VueQueryKeyPart("todos"), new VueQueryKeyPart(true)],
                        Enabled = true,
                        StaleTime = 30000
                    });
                }
            }
            """);

        StringAssert.Contains(script, "return useQuery({");
        StringAssert.Contains(script, "queryKey: [\"todos\", true]");
        StringAssert.Contains(script, "enabled: true");
        StringAssert.Contains(script, "staleTime: 30000");
        AssertImport(script, "@tanstack/vue-query", "useQuery");
    }

    [TestMethod]
    public async Task VueQuery_QueryStateAndRefetch_EmitRefReadsAndCalls()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.VueQuery;

            public static class TestClass
            {
                public static bool Loading(VueQueryQueryReturn<string> query)
                {
                    return query.IsLoading.Value;
                }

                public static VueQueryStatus Status(VueQueryQueryReturn<string> query)
                {
                    return query.Status.Value;
                }

                public static PromiseResult Refresh(VueQueryQueryReturn<string> query)
                {
                    return query.Refetch();
                }
            }
            """);

        StringAssert.Contains(script, "return query.isLoading.value;");
        StringAssert.Contains(script, "return query.status.value;");
        StringAssert.Contains(script, "return query.refetch();");
    }

    [TestMethod]
    public async Task VueQuery_MutationComposable_EmitsMutationWritesAndPendingCount()
    {
        var script = await ConvertAsync("""
            using ECMAScript;
            using static ECMAScript.VueQuery;

            public static class TestClass
            {
                public static VueQueryMutationReturn<string, string> Build()
                {
                    return UseMutation(new VueQueryMutationOptions<string, string>
                    {
                        MutationKey = ["save"]
                    });
                }

                public static void Submit(VueQueryMutationReturn<string, string> mutation, string value)
                {
                    mutation.Mutate(value);
                }

                public static double Pending()
                {
                    return UseIsMutating().Value;
                }
            }
            """);

        StringAssert.Contains(script, "return useMutation({");
        StringAssert.Contains(script, "mutationKey: [\"save\"]");
        StringAssert.Contains(script, "mutation.mutate(value);");
        StringAssert.Contains(script, "return useIsMutating().value;");
        AssertImport(script, "@tanstack/vue-query", "useMutation", "useIsMutating");
    }

    private static async Task<string> ConvertAsync(string code)
    {
        var (classSymbol, semanticModel) = CompileAndGetSymbol(
            code,
            "TestClass",
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(VueQuery).Assembly.Location));
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
            "ECMAScript.VueQuery.Test.Assembly",
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
