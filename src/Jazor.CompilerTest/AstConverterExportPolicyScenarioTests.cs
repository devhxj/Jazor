using Acornima.Ast;
using ECMAScript;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class AstConverterExportPolicyScenarioTests
{
    public static IEnumerable<TestDataRow<AstConverterExportCollisionScenario>> CollisionCases
        => AstConverterExportPolicyScenarioCatalog.Collisions.Select(static scenario =>
            new TestDataRow<AstConverterExportCollisionScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsKindsAndInputs()
    {
        var scenarios = AstConverterExportPolicyScenarioCatalog.Collisions;

        Assert.IsNotEmpty(scenarios);
        Assert.HasCount(scenarios.Count, scenarios.Select(static scenario => scenario.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(
            scenarios.Count,
            scenarios.Select(static scenario => scenario.InputIdentity).Distinct(StringComparer.Ordinal));
        Assert.HasCount(
            Enum.GetValues<AstConverterExportCollisionKind>().Length,
            scenarios.Select(static scenario => scenario.Kind).Distinct());
        Assert.IsTrue(scenarios.All(static scenario =>
            scenario.Id.StartsWith("ast-converter-export-policy.", StringComparison.Ordinal) &&
            !string.IsNullOrWhiteSpace(scenario.Dimension) &&
            !string.IsNullOrWhiteSpace(scenario.ExportName) &&
            scenario.ExpectedSymbolFragments.Count == 2));
    }

    [TestMethod]
    [DynamicData(nameof(CollisionCases))]
    public async Task Convert_DuplicateNamedExport_RejectsBothConflictingSymbols(
        AstConverterExportCollisionScenario scenario)
    {
        var fixture = CompileModule(scenario.Source, scenario.Id);
        var converter = new AstConverter(
            fixture.Module,
            fixture.SemanticModel,
            new AstConverterOptions(scenario.Profile, ModulePolicy: scenario.ModulePolicy));

        var exception = await Assert.ThrowsExactlyAsync<NotSupportedException>(() => converter.Convert());

        StringAssert.Contains(exception.Message, "duplicate named export", StringComparison.Ordinal, scenario.Id);
        StringAssert.Contains(exception.Message, $"'{scenario.ExportName}'", StringComparison.Ordinal, scenario.Id);
        foreach (var expectedSymbolFragment in scenario.ExpectedSymbolFragments)
            StringAssert.Contains(exception.Message, expectedSymbolFragment, StringComparison.Ordinal, scenario.Id);
    }

    [TestMethod]
    public async Task Convert_PrivateMemberSharingPublicExportName_EmitsOnlyPublicNamedExport()
    {
        const string scenarioId = "ast-converter-export-policy.private-member-not-exported";
        const string source = """
            using ECMAScript;

            [ECMAScriptModule("./module")]
            public static class TestModule
            {
                [ECMAScriptName("shared")]
                private static int Hidden = 1;

                [ECMAScriptName("shared")]
                public static int Visible = 2;
            }
            """;
        var fixture = CompileModule(source, scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();

        Assert.IsNotNull(module, scenarioId);
        var export = module.Body.OfType<ExportNamedDeclaration>().Single();
        Assert.IsNull(export.Declaration, scenarioId);
        var specifier = export.Specifiers.Single();
        Assert.IsInstanceOfType<Identifier>(specifier.Local, scenarioId);
        Assert.IsInstanceOfType<Identifier>(specifier.Exported, scenarioId);
        Assert.AreEqual("Visible", ((Identifier)specifier.Local).Name, scenarioId);
        Assert.AreEqual("shared", ((Identifier)specifier.Exported).Name, scenarioId);
    }

    [TestMethod]
    public async Task Convert_FilteredConflictingMember_ValidatesOnlyIncludedExportSurface()
    {
        const string scenarioId = "ast-converter-export-policy.filtered-member-not-exported";
        const string source = """
            using ECMAScript;

            [ECMAScriptModule("./module")]
            public static class TestModule
            {
                [ECMAScriptName("shared")]
                public static int First = 1;

                [ECMAScriptName("shared")]
                public static int Second = 2;
            }
            """;
        var fixture = CompileModule(source, scenarioId);
        var converter = new AstConverter(
            fixture.Module,
            fixture.SemanticModel,
            new AstConverterOptions(
                AstConverterProfile.Standard,
                MemberFilter: static member => !string.Equals(member.Name, "Second", StringComparison.Ordinal)));

        var module = await converter.Convert();

        Assert.IsNotNull(module, scenarioId);
        var export = module.Body.OfType<ExportNamedDeclaration>().Single();
        Assert.IsInstanceOfType<VariableDeclaration>(export.Declaration, scenarioId);
        var declaration = (VariableDeclaration)export.Declaration;
        var identifier = declaration.Declarations.Single().Id;
        Assert.IsInstanceOfType<Identifier>(identifier, scenarioId);
        Assert.AreEqual("shared", ((Identifier)identifier).Name, scenarioId);
        Assert.HasCount(0, export.Specifiers, scenarioId);
    }

    [TestMethod]
    public async Task Convert_UnannotatedModuleOverloads_UseDistinctStableExportNames()
    {
        const string scenarioId = "ast-converter-export-policy.unannotated-overloads-use-distinct-names";
        const string source = """
            using ECMAScript;

            [ECMAScriptModule("./module")]
            public static class TestModule
            {
                public static int Read(int value) => value;
                public static int Read(string value) => value.Length;
            }
            """;
        var fixture = CompileModule(source, scenarioId);
        var overloads = fixture.Module.GetMembers("Read")
            .OfType<IMethodSymbol>()
            .OrderBy(static method => method.Parameters[0].Type.ToDisplayString(Format.NameFormat), StringComparer.Ordinal)
            .ToArray();
        var expectedNames = overloads.Select(Util.GetConfigOrSymbolName).ToArray();
        Assert.HasCount(2, expectedNames.Distinct(StringComparer.Ordinal), scenarioId);

        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();
        var script = module?.ToKnRECMAScript();

        Assert.IsNotNull(script, scenarioId);
        foreach (var expectedName in expectedNames)
            StringAssert.Contains(script, $"function {expectedName}(", StringComparison.Ordinal, scenarioId);
        _ = new Acornima.Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_DescriptionAndECMAScriptNameCollision_RejectsDuplicateExport()
    {
        const string scenarioId = "ast-converter-export-policy.description-and-ecmascript-name-collision";
        const string source = """
            using System.ComponentModel;
            using ECMAScript;

            [ECMAScriptModule("./module")]
            public static class TestModule
            {
                [Description("@#shared")]
                public static int First = 1;

                [ECMAScriptName("shared")]
                public static int Second = 2;
            }
            """;
        var fixture = CompileModule(source, scenarioId);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var exception = await Assert.ThrowsExactlyAsync<NotSupportedException>(() => converter.Convert());

        StringAssert.Contains(exception.Message, "duplicate named export", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(exception.Message, "shared", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(exception.Message, "First", StringComparison.Ordinal, scenarioId);
        StringAssert.Contains(exception.Message, "Second", StringComparison.Ordinal, scenarioId);
    }

    [TestMethod]
    public async Task Convert_InheritedModulePolicy_EmitsBaseDeclarationsBeforeDerivedReferences()
    {
        const string scenarioId = "ast-converter-export-policy.inherited-module-success";
        const string source = """
            using ECMAScript;

            public class BaseModule
            {
                [ECMAScriptName("baseValue")]
                public static int BaseValue = 1;
            }

            [ECMAScriptModule("./module")]
            internal sealed class TestModule : BaseModule
            {
                [ECMAScriptName("derivedValue")]
                public static int DerivedValue = BaseValue + 1;
            }
            """;
        var fixture = CompileModule(source, scenarioId);
        var converter = new AstConverter(
            fixture.Module,
            fixture.SemanticModel,
            new AstConverterOptions(
                AstConverterProfile.Standard,
                ModulePolicy: InheritedModuleProjectionPolicy.Instance));

        var module = await converter.Convert();

        Assert.IsNotNull(module, scenarioId);
        var script = module!.ToKnRECMAScript();
        var baseValueOffset = script.IndexOf("baseValue", StringComparison.Ordinal);
        var derivedValueOffset = script.IndexOf("derivedValue", StringComparison.Ordinal);
        Assert.IsTrue(baseValueOffset >= 0, scenarioId);
        Assert.IsTrue(derivedValueOffset > baseValueOffset, scenarioId);
        StringAssert.Contains(script, "baseValue + 1", StringComparison.Ordinal, scenarioId);
        _ = new Acornima.Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Convert_MixedNestedTypePolicies_ExportsRuntimeSurfaceWithoutLeakingPrivateOrStructuralTypes()
    {
        const string scenarioId = "ast-converter-export-policy.mixed-nested-type-policies";
        const string source = """
            using ECMAScript;

            [ECMAScriptModule("./module")]
            public static class TestModule
            {
                private sealed class HiddenWorker
                {
                }

                internal sealed class SharedWorker
                {
                    private interface IMarker
                    {
                    }

                    private sealed record HiddenShape(int Value);

                    private enum HiddenKind
                    {
                        None
                    }
                }

                public static int Run() => 1;
            }
            """;
        var fixture = CompileModule(source, scenarioId);
        var module = await new AstConverter(fixture.Module, fixture.SemanticModel).Convert();

        Assert.IsNotNull(module, scenarioId);
        var convertedModule = module!;

        var directClasses = convertedModule.Body.OfType<ClassDeclaration>()
            .Select(declaration => GetRuntimeClassName(declaration, scenarioId))
            .ToArray();
        CollectionAssert.AreEqual(new[] { "HiddenWorker" }, directClasses, scenarioId);

        var exportedClasses = convertedModule.Body
            .OfType<ExportNamedDeclaration>()
            .Select(static declaration => declaration.Declaration)
            .OfType<ClassDeclaration>()
            .Select(declaration => GetRuntimeClassName(declaration, scenarioId))
            .ToArray();
        CollectionAssert.AreEqual(new[] { "SharedWorker" }, exportedClasses, scenarioId);

        var declaredRuntimeTypeNames = directClasses.Concat(exportedClasses).ToArray();
        CollectionAssert.DoesNotContain(declaredRuntimeTypeNames, "IMarker", scenarioId);
        CollectionAssert.DoesNotContain(declaredRuntimeTypeNames, "HiddenShape", scenarioId);
        CollectionAssert.DoesNotContain(declaredRuntimeTypeNames, "HiddenKind", scenarioId);

        _ = new Acornima.Parser().ParseModule(convertedModule.ToKnRECMAScript());
    }

    private static string GetRuntimeClassName(ClassDeclaration declaration, string scenarioId)
        => declaration.Id?.Name ?? throw new AssertFailedException(
            $"{scenarioId}: runtime class declaration is missing an identifier.");

    private static AstConverterModuleFixture CompileModule(string source, string scenarioId)
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "AstConverterExportPolicyScenario.cs");
        var compilation = CSharpCompilation.Create(
            assemblyName: "AstConverterExportPolicyScenarios_" + Guid.NewGuid().ToString("N"),
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

public enum AstConverterExportCollisionKind
{
    FieldWithField,
    FieldWithMethod,
    MethodWithRuntimeClass,
    ExplicitlyNamedOverloads,
    InternalWithPublic,
    InheritedProjectedMembers
}

public sealed record AstConverterExportCollisionScenario(
    string Id,
    string Dimension,
    AstConverterExportCollisionKind Kind,
    AstConverterProfile Profile,
    AstConverterModulePolicy? ModulePolicy,
    string Source,
    string ExportName,
    IReadOnlyList<string> ExpectedSymbolFragments)
{
    public string InputIdentity => $"{Kind}|{Profile}|{ModulePolicy?.GetType().Name}|{Source}";
}

internal static class AstConverterExportPolicyScenarioCatalog
{
    public static IReadOnlyList<AstConverterExportCollisionScenario> Collisions { get; } =
    [
        Collision(
            "field-field",
            "two-public-fields-share-explicit-export-name",
            AstConverterExportCollisionKind.FieldWithField,
            AstConverterProfile.Standard,
            """
            using ECMAScript;

            [ECMAScriptModule("./module")]
            public static class TestModule
            {
                [ECMAScriptName("shared")]
                public static int First = 1;

                [ECMAScriptName("shared")]
                public static int Second = 2;

                private static int ReserveSecondName()
                {
                    var Second = 0;
                    return Second;
                }
            }
            """,
            ["First", "Second"]),
        Collision(
            "field-method",
            "public-field-and-method-share-explicit-export-name",
            AstConverterExportCollisionKind.FieldWithMethod,
            AstConverterProfile.Standard,
            """
            using ECMAScript;

            [ECMAScriptModule("./module")]
            public static class TestModule
            {
                [ECMAScriptName("shared")]
                public static int Value = 1;

                [ECMAScriptName("shared")]
                public static int Read() => Value;
            }
            """,
            ["Value", "Read"]),
        Collision(
            "method-runtime-class",
            "public-method-and-runtime-member-class-share-explicit-export-name",
            AstConverterExportCollisionKind.MethodWithRuntimeClass,
            AstConverterProfile.Standard,
            """
            using ECMAScript;

            [ECMAScriptModule("./module")]
            public static class TestModule
            {
                [ECMAScriptName("shared")]
                public static int Create() => 1;

                [ECMAScriptName("shared")]
                public sealed class Worker
                {
                }
            }
            """,
            ["Create", "Worker"]),
        Collision(
            "explicitly-named-overloads",
            "method-overloads-collapse-to-one-explicit-export-name",
            AstConverterExportCollisionKind.ExplicitlyNamedOverloads,
            AstConverterProfile.Standard,
            """
            using ECMAScript;

            [ECMAScriptModule("./module")]
            public static class TestModule
            {
                [ECMAScriptName("shared")]
                public static int Run(int value) => value;

                [ECMAScriptName("shared")]
                public static int Run(string value) => value.Length;
            }
            """,
            ["Run(int)", "Run(string)"]),
        Collision(
            "internal-public",
            "internal-module-member-participates-in-public-export-surface",
            AstConverterExportCollisionKind.InternalWithPublic,
            AstConverterProfile.Standard,
            """
            using ECMAScript;

            [ECMAScriptModule("./module")]
            public static class TestModule
            {
                [ECMAScriptName("shared")]
                internal static int InternalValue = 1;

                [ECMAScriptName("shared")]
                public static int PublicValue() => InternalValue;
            }
            """,
            ["InternalValue", "PublicValue"]),
        Collision(
            "inherited-projected-members",
            "module-policy-validates-base-and-derived-export-surface",
            AstConverterExportCollisionKind.InheritedProjectedMembers,
            AstConverterProfile.Standard,
            """
            using ECMAScript;

            public class BaseModule
            {
                [ECMAScriptName("shared")]
                public static int BaseValue = 1;
            }

            [ECMAScriptModule("./module")]
            public sealed class TestModule : BaseModule
            {
                [ECMAScriptName("shared")]
                public static int DerivedValue = 2;
            }
            """,
            ["BaseValue", "DerivedValue"],
            InheritedModuleProjectionPolicy.Instance)
    ];

    private static AstConverterExportCollisionScenario Collision(
        string id,
        string dimension,
        AstConverterExportCollisionKind kind,
        AstConverterProfile profile,
        string source,
        IReadOnlyList<string> expectedSymbolFragments,
        AstConverterModulePolicy? modulePolicy = null)
        => new(
            $"ast-converter-export-policy.{id}",
            dimension,
            kind,
            profile,
            modulePolicy,
            source,
            "shared",
            expectedSymbolFragments);
}
