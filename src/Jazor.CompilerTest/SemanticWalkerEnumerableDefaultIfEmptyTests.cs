using Acornima;
using Acornima.Ast;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerEnumerableDefaultIfEmptyTests
{
    [TestMethod]
    public async Task Convert_DefaultIfEmpty_InjectsClosedDefaultValueThroughExplicitRuntimeContract()
    {
        const string scenarioId = "semantic-walker-enumerable.default-if-empty.closed-defaults";
        var fixture = CompileModule(
            """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            public static class TestModule
            {
                public static int[] Numbers(int[] numbers)
                    => numbers.DefaultIfEmpty().ToArray();

                public static string[] Texts(string[] texts)
                    => texts.DefaultIfEmpty().ToArray();

                public static int[] Explicit(int[] values)
                    => values.DefaultIfEmpty(-5).ToArray();

                public static T[] ReferenceValues<T>(IEnumerable<T> values)
                    where T : class
                    => values.DefaultIfEmpty().ToArray();

                public static int[] Flatten(int[][] values)
                    => values.SelectMany((Func<int[], IEnumerable<int>>)Enumerable.DefaultIfEmpty).ToArray();
            }
            """,
            scenarioId);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(module, scenarioId);
        Assert.IsNotNull(script, scenarioId);
        AssertImport(module, "defaultIfEmpty", scenarioId);
        StringAssert.Contains(script, "defaultIfEmpty(numbers, 0)", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "defaultIfEmpty(texts, null)", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "defaultIfEmpty(values, -5)", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "__enumerableDefaultArg0", StringComparison.Ordinal, scenarioId);
        Assert.IsFalse(script.Contains("DefaultIfEmpty", StringComparison.Ordinal), scenarioId);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_DefaultIfEmpty_UnconstrainedGenericReportsErasedDefaultBoundary()
    {
        const string scenarioId = "semantic-walker-enumerable.default-if-empty.unconstrained-generic";
        var fixture = CompileModule(
            """
            using System.Collections.Generic;
            using System.Linq;

            public static class TestModule
            {
                public static T[] Values<T>(IEnumerable<T> values)
                    => values.DefaultIfEmpty().ToArray();
            }
            """,
            scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var exception = await Assert.ThrowsAsync<OperationTransformationException>(converter.Convert);

        StringAssert.Contains(
            exception.Message ?? string.Empty,
            "default(T) is not supported because the runtime type parameter may be a value type",
            StringComparison.Ordinal,
            scenarioId);
    }

    [TestMethod]
    public async Task Convert_OrDefaultTerminalFamily_UsesClosedFallbackImportsForPlainAndPredicateCalls()
    {
        const string scenarioId = "semantic-walker-enumerable.or-default-terminal-family";
        var fixture = CompileModule(
            """
            using System.Linq;

            public static class TestModule
            {
                public static int First(int[] firstValues)
                    => firstValues.FirstOrDefault();

                public static int FirstWhere(int[] firstWhereValues)
                    => firstWhereValues.FirstOrDefault(value => value > 3);

                public static int Last(int[] lastValues)
                    => lastValues.LastOrDefault();

                public static int LastWhere(int[] lastWhereValues)
                    => lastWhereValues.LastOrDefault(value => value % 2 == 0);

                public static int Single(int[] singleValues)
                    => singleValues.SingleOrDefault();

                public static int SingleWhere(int[] singleWhereValues)
                    => singleWhereValues.SingleOrDefault(value => value < 0);
            }
            """,
            scenarioId);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(module, scenarioId);
        Assert.IsNotNull(script, scenarioId);
        foreach (var importName in new[]
        {
            "firstOrDefault",
            "firstOrDefaultWhere",
            "lastOrDefault",
            "lastOrDefaultWhere",
            "singleOrDefault",
            "singleOrDefaultWhere"
        })
        {
            AssertImport(module, importName, scenarioId);
        }

        StringAssert.Contains(script, "firstOrDefault(firstValues, 0)", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "lastOrDefault(lastValues, 0)", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "singleOrDefault(singleValues, 0)", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "firstOrDefaultWhere(firstWhereValues,", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "lastOrDefaultWhere(lastWhereValues,", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(script, "singleOrDefaultWhere(singleWhereValues,", StringComparison.Ordinal, scenarioId);
        _ = new Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_TranslatedLeftOuterJoin_DefaultIfEmptyUsesBoundFallbackAndQueryRuntimeContracts()
    {
        const string scenarioId = "semantic-walker-enumerable.default-if-empty.left-outer-join";
        var fixture = CompileModule(
            """
            using System.Linq;

            public static class TestModule
            {
                public static int[] LeftOuterJoin(int[] outer, int[] inner)
                {
                    return (from left in outer
                            join right in inner on left equals right into matches
                            from right in matches.DefaultIfEmpty()
                            select left * 10 + right).ToArray();
                }
            }
            """,
            scenarioId);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(module, scenarioId);
        Assert.IsNotNull(script, scenarioId);
        AssertImport(module, "defaultIfEmpty", scenarioId);
        AssertImport(module, "_b61f41d1ac124b69", scenarioId);
        AssertImport(module, "_aacc82f5a0d854d2", scenarioId);
        StringAssert.Contains(script, "defaultIfEmpty(", StringComparison.Ordinal, scenarioId);
        Assert.IsFalse(script.Contains("TranslatedQuery", StringComparison.Ordinal), scenarioId);
        _ = new Parser().ParseModule(script);
    }

    private static void AssertImport(Module module, string importName, string scenarioId)
    {
        var imports = module.Body
            .OfType<ImportDeclaration>()
            .Where(static declaration => declaration.Source.Value == "System/Linq/EnumerableModule.js")
            .SelectMany(static declaration => declaration.Specifiers)
            .OfType<ImportSpecifier>()
            .ToArray();
        Assert.IsTrue(
            imports.Any(specifier =>
                specifier.Imported is Identifier { Name: var name } &&
                string.Equals(name, importName, StringComparison.Ordinal)),
            scenarioId + ": missing System.Linq.Enumerable import '" + importName + "'.");
    }

    private static DefaultIfEmptyFixture CompileModule(string source, string scenarioId)
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "SemanticWalkerEnumerableDefaultIfEmptyScenario.cs");
        var compilation = CSharpCompilation.Create(
            assemblyName: "SemanticWalkerEnumerableDefaultIfEmpty_" + Guid.NewGuid().ToString("N"),
            syntaxTrees: [sourceTree],
            references: TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(
            0,
            errors,
            scenarioId + ":" + Environment.NewLine +
            string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var semanticModel = compilation.GetSemanticModel(sourceTree);
        var module = sourceTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static declaration => declaration.Identifier.ValueText == "TestModule")
            .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
            .OfType<INamedTypeSymbol>()
            .Single();
        return new DefaultIfEmptyFixture(module, semanticModel);
    }

    private sealed record DefaultIfEmptyFixture(
        INamedTypeSymbol Module,
        SemanticModel SemanticModel);
}
