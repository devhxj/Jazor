using Acornima.Ast;
using ECMAScript;
using Jazor.Common;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class AstConverterModuleNameScenarioTests
{
    public static IEnumerable<TestDataRow<AstConverterModuleNameScenario>> Cases
        => AstConverterModuleNameScenarioCatalog.All.Select(static scenario =>
            new TestDataRow<AstConverterModuleNameScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsKindsAndInputs()
    {
        var scenarios = AstConverterModuleNameScenarioCatalog.All;

        Assert.IsNotEmpty(scenarios);
        Assert.HasCount(scenarios.Count, scenarios.Select(static scenario => scenario.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(
            scenarios.Count,
            scenarios.Select(static scenario => scenario.InputIdentity).Distinct(StringComparer.Ordinal));
        Assert.HasCount(
            Enum.GetValues<AstConverterModuleDeclarationKind>().Length,
            scenarios.Select(static scenario => scenario.DeclarationKind).Distinct());
        Assert.HasCount(
            Enum.GetValues<AstConverterLocalNameExpectationKind>().Length,
            scenarios.Select(static scenario => scenario.LocalNameKind).Distinct());
        Assert.HasCount(
            Enum.GetValues<AstConverterExportNameNodeKind>().Length,
            scenarios.Select(static scenario => scenario.ExportNameNodeKind).Distinct());
        Assert.IsTrue(scenarios.All(static scenario =>
            scenario.Id.StartsWith("ast-converter-module-name.", StringComparison.Ordinal) &&
            !string.IsNullOrWhiteSpace(scenario.Dimension) &&
            !string.IsNullOrWhiteSpace(scenario.SourceMemberName) &&
            !string.IsNullOrWhiteSpace(scenario.ExportName)));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public async Task Convert_ConfiguredExportName_PreservesValidLocalBindingAndModuleExportName(
        AstConverterModuleNameScenario scenario)
    {
        var fixture = CompileModule(scenario.Source, scenario.Id);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();

        Assert.IsNotNull(module, scenario.Id);
        var export = module.Body.OfType<ExportNamedDeclaration>().Single();
        var expectedLocalName = ResolveExpectedLocalName(fixture.Module, scenario);
        if (scenario.LocalNameKind == AstConverterLocalNameExpectationKind.DirectExport)
        {
            Assert.IsNotNull(export.Declaration, scenario.Id);
            Assert.HasCount(0, export.Specifiers, scenario.Id);
            Assert.AreEqual(
                expectedLocalName,
                GetDeclarationIdentifier(export.Declaration, scenario.DeclarationKind, scenario.Id),
                scenario.Id);
            Assert.AreEqual(expectedLocalName, scenario.ExportName, scenario.Id);
            return;
        }

        Assert.IsNull(export.Declaration, scenario.Id);
        var specifier = export.Specifiers.Single();
        Assert.IsInstanceOfType<Identifier>(specifier.Local, scenario.Id);
        Assert.AreEqual(expectedLocalName, ((Identifier)specifier.Local).Name, scenario.Id);
        AssertExportName(specifier.Exported, scenario, scenario.Id);

        var declarationNames = module.Body
            .Where(static statement => statement is not ExportNamedDeclaration)
            .Select(statement => TryGetDeclarationIdentifier(statement, scenario.DeclarationKind))
            .Where(static name => name is not null)
            .ToArray();
        CollectionAssert.Contains(declarationNames, expectedLocalName, scenario.Id);
    }

    private static string ResolveExpectedLocalName(
        INamedTypeSymbol module,
        AstConverterModuleNameScenario scenario)
        => scenario.LocalNameKind switch
        {
            AstConverterLocalNameExpectationKind.ExactFallback => scenario.ExpectedLocalName!,
            AstConverterLocalNameExpectationKind.DirectExport => scenario.ExportName,
            AstConverterLocalNameExpectationKind.StableHash => ResolveHashedLocalName(module, scenario),
            _ => throw new InvalidOperationException(
                $"{scenario.Id}: unsupported local-name expectation '{scenario.LocalNameKind}'.")
        };

    private static string ResolveHashedLocalName(
        INamedTypeSymbol module,
        AstConverterModuleNameScenario scenario)
    {
        var symbol = module.GetMembers(scenario.SourceMemberName).Single();
        var displayString = symbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
        return $"m${Format.HashName(displayString).TrimStart('_')}";
    }

    private static void AssertExportName(
        Expression exported,
        AstConverterModuleNameScenario scenario,
        string scenarioId)
    {
        switch (scenario.ExportNameNodeKind)
        {
            case AstConverterExportNameNodeKind.Identifier:
                Assert.IsInstanceOfType<Identifier>(exported, scenarioId);
                Assert.AreEqual(scenario.ExportName, ((Identifier)exported).Name, scenarioId);
                break;
            case AstConverterExportNameNodeKind.StringLiteral:
                Assert.IsInstanceOfType<StringLiteral>(exported, scenarioId);
                Assert.AreEqual(scenario.ExportName, ((StringLiteral)exported).Value, scenarioId);
                break;
            default:
                throw new InvalidOperationException(
                    $"{scenario.Id}: unsupported export-name node kind '{scenario.ExportNameNodeKind}'.");
        }
    }

    private static string GetDeclarationIdentifier(
        Statement declaration,
        AstConverterModuleDeclarationKind kind,
        string scenarioId)
        => TryGetDeclarationIdentifier(declaration, kind)
            ?? throw new AssertFailedException(
                $"{scenarioId}: expected a {kind} declaration, got '{declaration.Type}'.");

    private static string? TryGetDeclarationIdentifier(
        Statement declaration,
        AstConverterModuleDeclarationKind kind)
        => (declaration, kind) switch
        {
            (VariableDeclaration variable, AstConverterModuleDeclarationKind.Field) =>
                (variable.Declarations.Single().Id as Identifier)?.Name,
            (FunctionDeclaration function, AstConverterModuleDeclarationKind.Method) => function.Id?.Name,
            (ClassDeclaration @class, AstConverterModuleDeclarationKind.RuntimeClass) => @class.Id?.Name,
            _ => null
        };

    private static AstConverterModuleFixture CompileModule(string source, string scenarioId)
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "AstConverterModuleNameScenario.cs");
        var compilation = CSharpCompilation.Create(
            assemblyName: "AstConverterModuleNameScenarios_" + Guid.NewGuid().ToString("N"),
            syntaxTrees: [sourceTree],
            references: TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScriptNameAttribute).Assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(
            0,
            errors,
            $"{scenarioId}:{Environment.NewLine}" +
            string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var semanticModel = compilation.GetSemanticModel(sourceTree);
        var module = sourceTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static declaration => declaration.Identifier.ValueText == "TestModule")
            .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
            .OfType<INamedTypeSymbol>()
            .Single();
        return new AstConverterModuleFixture(module, semanticModel);
    }

    private sealed record AstConverterModuleFixture(
        INamedTypeSymbol Module,
        SemanticModel SemanticModel);
}

public enum AstConverterModuleDeclarationKind
{
    Field,
    Method,
    RuntimeClass
}

public enum AstConverterLocalNameExpectationKind
{
    ExactFallback,
    StableHash,
    DirectExport
}

public enum AstConverterExportNameNodeKind
{
    Identifier,
    StringLiteral
}

public sealed record AstConverterModuleNameScenario(
    string Id,
    string Dimension,
    AstConverterModuleDeclarationKind DeclarationKind,
    AstConverterLocalNameExpectationKind LocalNameKind,
    AstConverterExportNameNodeKind ExportNameNodeKind,
    string Source,
    string SourceMemberName,
    string ExportName,
    string? ExpectedLocalName)
{
    public string InputIdentity =>
        $"{DeclarationKind}|{LocalNameKind}|{ExportNameNodeKind}|{SourceMemberName}|{ExportName}|{Source}";
}

internal static class AstConverterModuleNameScenarioCatalog
{
    public static IReadOnlyList<AstConverterModuleNameScenario> All { get; } =
    [
        Scenario(
            "field-hyphenated-export",
            "field-uses-source-name-for-non-binding-export-name",
            AstConverterModuleDeclarationKind.Field,
            AstConverterLocalNameExpectationKind.ExactFallback,
            AstConverterExportNameNodeKind.StringLiteral,
            """
            using ECMAScript;

            [ECMAScriptModule("./module")]
            public static class TestModule
            {
                [ECMAScriptName("release-name")]
                public static int Release = 1;
            }
            """,
            "Release",
            "release-name",
            "Release"),
        Scenario(
            "method-reserved-export",
            "method-uses-source-name-for-reserved-binding-export-name",
            AstConverterModuleDeclarationKind.Method,
            AstConverterLocalNameExpectationKind.ExactFallback,
            AstConverterExportNameNodeKind.Identifier,
            """
            using ECMAScript;

            [ECMAScriptModule("./module")]
            public static class TestModule
            {
                [ECMAScriptName("class")]
                public static int Build() => 1;
            }
            """,
            "Build",
            "class",
            "Build"),
        Scenario(
            "runtime-class-hyphenated-export",
            "runtime-class-uses-source-name-for-non-binding-export-name",
            AstConverterModuleDeclarationKind.RuntimeClass,
            AstConverterLocalNameExpectationKind.ExactFallback,
            AstConverterExportNameNodeKind.StringLiteral,
            """
            using ECMAScript;

            [ECMAScriptModule("./module")]
            public static class TestModule
            {
                [ECMAScriptName("worker-item")]
                public sealed class Worker
                {
                }
            }
            """,
            "Worker",
            "worker-item",
            "Worker"),
        Scenario(
            "field-reserved-source-fallback",
            "non-binding-export-and-reserved-source-name-use-stable-hash",
            AstConverterModuleDeclarationKind.Field,
            AstConverterLocalNameExpectationKind.StableHash,
            AstConverterExportNameNodeKind.StringLiteral,
            """
            using ECMAScript;

            [ECMAScriptModule("./module")]
            public static class TestModule
            {
                [ECMAScriptName("release-name")]
                public static int @class = 1;
            }
            """,
            "class",
            "release-name",
            null),
        Scenario(
            "method-await-export",
            "await-export-name-does-not-become-module-binding",
            AstConverterModuleDeclarationKind.Method,
            AstConverterLocalNameExpectationKind.ExactFallback,
            AstConverterExportNameNodeKind.Identifier,
            """
            using ECMAScript;

            [ECMAScriptModule("./module")]
            public static class TestModule
            {
                [ECMAScriptName("await")]
                public static int Load() => 1;
            }
            """,
            "Load",
            "await",
            "Load"),
        Scenario(
            "field-dollar-binding",
            "dollar-prefixed-configured-name-remains-direct-binding",
            AstConverterModuleDeclarationKind.Field,
            AstConverterLocalNameExpectationKind.DirectExport,
            AstConverterExportNameNodeKind.Identifier,
            """
            using ECMAScript;

            [ECMAScriptModule("./module")]
            public static class TestModule
            {
                [ECMAScriptName("$release")]
                public static int Release = 1;
            }
            """,
            "Release",
            "$release",
            null),
        Scenario(
            "method-unicode-binding",
            "unicode-configured-name-remains-direct-binding",
            AstConverterModuleDeclarationKind.Method,
            AstConverterLocalNameExpectationKind.DirectExport,
            AstConverterExportNameNodeKind.Identifier,
            """
            using ECMAScript;

            [ECMAScriptModule("./module")]
            public static class TestModule
            {
                [ECMAScriptName("发布")]
                public static int Publish() => 1;
            }
            """,
            "Publish",
            "发布",
            null)
    ];

    private static AstConverterModuleNameScenario Scenario(
        string id,
        string dimension,
        AstConverterModuleDeclarationKind declarationKind,
        AstConverterLocalNameExpectationKind localNameKind,
        AstConverterExportNameNodeKind exportNameNodeKind,
        string source,
        string sourceMemberName,
        string exportName,
        string? expectedLocalName)
        => new(
            $"ast-converter-module-name.{id}",
            dimension,
            declarationKind,
            localNameKind,
            exportNameNodeKind,
            source,
            sourceMemberName,
            exportName,
            expectedLocalName);
}
