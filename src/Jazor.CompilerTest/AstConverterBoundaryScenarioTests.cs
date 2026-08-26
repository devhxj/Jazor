using Acornima.Ast;
using ECMAScript;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class AstConverterBoundaryScenarioTests
{
    public static IEnumerable<TestDataRow<AstConverterImportSuccessScenario>> ImportSuccessCases
        => AstConverterBoundaryScenarioCatalog.ImportSuccesses.Select(static scenario =>
            new TestDataRow<AstConverterImportSuccessScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    public static IEnumerable<TestDataRow<AstConverterImportFailureScenario>> ImportFailureCases
        => AstConverterBoundaryScenarioCatalog.ImportFailures.Select(static scenario =>
            new TestDataRow<AstConverterImportFailureScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    public static IEnumerable<TestDataRow<AstConverterDeclaredNameScenario>> DeclaredNameCases
        => AstConverterBoundaryScenarioCatalog.DeclaredNames.Select(static scenario =>
            new TestDataRow<AstConverterDeclaredNameScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    public static IEnumerable<TestDataRow<AstConverterRuntimeClassValidationScenario>> RuntimeClassValidationCases
        => AstConverterBoundaryScenarioCatalog.RuntimeClassValidations.Select(static scenario =>
            new TestDataRow<AstConverterRuntimeClassValidationScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    [TestMethod]
    public void ScenarioCatalogs_HaveUniqueIdsDimensionsKindsAndInputs()
    {
        var allIds = AstConverterBoundaryScenarioCatalog.ImportSuccesses.Select(static scenario => scenario.Id)
            .Concat(AstConverterBoundaryScenarioCatalog.ImportFailures.Select(static scenario => scenario.Id))
            .Concat(AstConverterBoundaryScenarioCatalog.DeclaredNames.Select(static scenario => scenario.Id))
            .Concat(AstConverterBoundaryScenarioCatalog.RuntimeClassValidations.Select(static scenario => scenario.Id))
            .ToArray();
        var allInputs = AstConverterBoundaryScenarioCatalog.ImportSuccesses.Select(static scenario => scenario.InputIdentity)
            .Concat(AstConverterBoundaryScenarioCatalog.ImportFailures.Select(static scenario => scenario.InputIdentity))
            .Concat(AstConverterBoundaryScenarioCatalog.DeclaredNames.Select(static scenario => scenario.InputIdentity))
            .Concat(AstConverterBoundaryScenarioCatalog.RuntimeClassValidations.Select(static scenario => scenario.InputIdentity))
            .ToArray();

        Assert.IsNotEmpty(allIds);
        Assert.HasCount(allIds.Length, allIds.Distinct(StringComparer.Ordinal));
        Assert.HasCount(allInputs.Length, allInputs.Distinct(StringComparer.Ordinal));
        Assert.IsTrue(allIds.All(static id => id.StartsWith("ast-converter-boundary.", StringComparison.Ordinal)));
        Assert.IsTrue(AstConverterBoundaryScenarioCatalog.ImportSuccesses.All(static scenario =>
            !string.IsNullOrWhiteSpace(scenario.Dimension)));
        Assert.IsTrue(AstConverterBoundaryScenarioCatalog.ImportFailures.All(static scenario =>
            !string.IsNullOrWhiteSpace(scenario.Dimension) && scenario.ExpectedImport is not null));
        Assert.IsTrue(AstConverterBoundaryScenarioCatalog.DeclaredNames.All(static scenario =>
            !string.IsNullOrWhiteSpace(scenario.Dimension) &&
            !string.IsNullOrWhiteSpace(scenario.ReservedName)));
        Assert.IsTrue(AstConverterBoundaryScenarioCatalog.RuntimeClassValidations.All(static scenario =>
            !string.IsNullOrWhiteSpace(scenario.Dimension) &&
            scenario.ExpectedMessageFragments.Count > 0));
        Assert.HasCount(
            Enum.GetValues<AstConverterImportSuccessKind>().Length,
            AstConverterBoundaryScenarioCatalog.ImportSuccesses.Select(static scenario => scenario.Kind).Distinct());
        Assert.HasCount(
            Enum.GetValues<AstConverterImportFailureKind>().Length,
            AstConverterBoundaryScenarioCatalog.ImportFailures.Select(static scenario => scenario.Kind).Distinct());
        Assert.HasCount(
            Enum.GetValues<AstConverterDeclaredNameKind>().Length,
            AstConverterBoundaryScenarioCatalog.DeclaredNames.Select(static scenario => scenario.Kind).Distinct());
        Assert.HasCount(
            Enum.GetValues<AstConverterRuntimeClassValidationKind>().Length,
            AstConverterBoundaryScenarioCatalog.RuntimeClassValidations.Select(static scenario => scenario.Kind).Distinct());
    }

    [TestMethod]
    [DynamicData(nameof(ImportSuccessCases))]
    public async Task Convert_ImportBoundaryRetainsOnlyReferencedAstBindings(AstConverterImportSuccessScenario scenario)
    {
        var fixture = CompileModule(scenario.Source, scenario.Id);
        var host = new ImportProbeSemanticWalkerHost(scenario.Import);
        var converter = new AstConverter(
            fixture.Module,
            fixture.SemanticModel,
            new AstConverterOptions(AstConverterProfile.Standard, Host: host));

        var module = await converter.Convert();

        Assert.IsNotNull(module, scenario.Id);
        var imports = module.Body.OfType<ImportDeclaration>().ToArray();
        AssertImportShape(imports, scenario.ExpectedImport, scenario.Id);

        var nonImportStatements = module.Body
            .Where(static statement => statement is not ImportDeclaration)
            .ToArray();
        var flushed = converter.FlushImportDeclarations(nonImportStatements);
        AssertImportShape(flushed, scenario.ExpectedImport, scenario.Id);
    }

    [TestMethod]
    [DynamicData(nameof(ImportFailureCases))]
    public async Task Convert_ExternalSpecifierMatchingGeneratedModuleStemRemainsIndependent(AstConverterImportFailureScenario scenario)
    {
        var fixture = CompileModule(scenario.Source, scenario.Id);
        var host = new ImportProbeSemanticWalkerHost(scenario.Import);
        var converter = new AstConverter(
            fixture.Module,
            fixture.SemanticModel,
            new AstConverterOptions(AstConverterProfile.Standard, Host: host));

        var module = await converter.Convert();

        Assert.IsNotNull(module, scenario.Id);
        AssertImportShape(
            module.Body.OfType<ImportDeclaration>().ToArray(),
            scenario.ExpectedImport,
            scenario.Id);
    }

    [TestMethod]
    [DynamicData(nameof(DeclaredNameCases))]
    public async Task Convert_DeclaredNameSyntaxReservesStableImportAlias(AstConverterDeclaredNameScenario scenario)
    {
        var fixture = CompileModule(scenario.Source, scenario.Id);
        var import = new AstConverterImportProbe(
            "runtime",
            scenario.ReservedName,
            scenario.ReservedName,
            AstConverterImportProbeKind.BoundNamed,
            Referenced: true);
        var host = new ImportProbeSemanticWalkerHost(import);
        var converter = new AstConverter(
            fixture.Module,
            fixture.SemanticModel,
            new AstConverterOptions(
                AstConverterProfile.Standard,
                MemberFilter: static symbol => !string.Equals(
                    symbol.Name,
                    "ReserveNames",
                    StringComparison.Ordinal),
                Host: host));

        var module = await converter.Convert();

        Assert.IsNotNull(module, scenario.Id);
        var declaration = module.Body.OfType<ImportDeclaration>().Single();
        Assert.AreEqual("runtime", ((StringLiteral)declaration.Source).Value, scenario.Id);
        var specifier = declaration.Specifiers.OfType<ImportSpecifier>().Single();
        Assert.IsInstanceOfType<Identifier>(specifier.Imported, scenario.Id);
        Assert.AreEqual(scenario.ReservedName, ((Identifier)specifier.Imported).Name, scenario.Id);
        Assert.AreNotEqual(scenario.ReservedName, specifier.Local.Name, scenario.Id);
        Assert.IsFalse(
            module.Body.OfType<ExportNamedDeclaration>()
                .Select(static export => export.Declaration)
                .OfType<FunctionDeclaration>()
                .Any(static function => string.Equals(
                    function.Id?.Name,
                    "reserveNames",
                    StringComparison.Ordinal)),
            scenario.Id);
    }

    [TestMethod]
    [DynamicData(nameof(RuntimeClassValidationCases))]
    public void ConvertRuntimeClass_RejectsInvalidPublicInput(AstConverterRuntimeClassValidationScenario scenario)
    {
        var fixture = CompileRuntimeClassValidation(scenario.Id);
        INamedTypeSymbol? target = scenario.Kind switch
        {
            AstConverterRuntimeClassValidationKind.NullSymbol => null,
            AstConverterRuntimeClassValidationKind.Interface => fixture.GetNamedType("TargetInterface"),
            AstConverterRuntimeClassValidationKind.Struct => fixture.GetNamedType("TargetStruct"),
            AstConverterRuntimeClassValidationKind.Record => fixture.GetNamedType("TargetRecord"),
            _ => throw new InvalidOperationException(
                $"{scenario.Id}: unsupported runtime-class validation kind '{scenario.Kind}'.")
        };
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        Exception exception;
        if (scenario.Kind == AstConverterRuntimeClassValidationKind.NullSymbol)
            exception = Assert.ThrowsExactly<ArgumentNullException>(() => converter.ConvertRuntimeClass(target!));
        else
            exception = Assert.ThrowsExactly<NotSupportedException>(() => converter.ConvertRuntimeClass(target!));

        foreach (var expected in scenario.ExpectedMessageFragments)
            StringAssert.Contains(exception.Message, expected, StringComparison.Ordinal, scenario.Id);
        if (exception is ArgumentNullException argumentException)
            Assert.AreEqual("symbol", argumentException.ParamName, scenario.Id);
    }

    private static void AssertImportShape(
        IReadOnlyCollection<ImportDeclaration> actual,
        AstConverterImportExpectation? expected,
        string scenarioId)
    {
        if (expected is null)
        {
            Assert.HasCount(0, actual, scenarioId);
            return;
        }

        Assert.HasCount(1, actual, scenarioId);
        var declaration = actual.Single();
        Assert.AreEqual(expected.ModulePath, ((StringLiteral)declaration.Source).Value, scenarioId);
        Assert.HasCount(1, declaration.Specifiers, scenarioId);
        var specifier = declaration.Specifiers[0];

        switch (expected.Kind)
        {
            case AstConverterImportExpectationKind.Default:
                Assert.IsInstanceOfType<ImportDefaultSpecifier>(specifier, scenarioId);
                Assert.AreEqual(expected.LocalName, specifier.Local.Name, scenarioId);
                break;
            case AstConverterImportExpectationKind.Namespace:
                Assert.IsInstanceOfType<ImportNamespaceSpecifier>(specifier, scenarioId);
                Assert.AreEqual(expected.LocalName, specifier.Local.Name, scenarioId);
                break;
            case AstConverterImportExpectationKind.NamedString:
                Assert.IsInstanceOfType<ImportSpecifier>(specifier, scenarioId);
                var named = (ImportSpecifier)specifier;
                Assert.IsInstanceOfType<StringLiteral>(named.Imported, scenarioId);
                Assert.AreEqual(expected.ImportedName, ((StringLiteral)named.Imported).Value, scenarioId);
                Assert.AreEqual(expected.LocalName, named.Local.Name, scenarioId);
                break;
            default:
                throw new InvalidOperationException(
                    $"{scenarioId}: unsupported import expectation kind '{expected.Kind}'.");
        }
    }

    private static AstConverterModuleFixture CompileModule(string source, string scenarioId)
    {
        const string usings = """
            global using System;
            global using System.Collections.Generic;
            global using System.Linq;
            global using ECMAScript;
            global using static ECMAScript.Global;
            """;
        var sourceTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "AstConverterBoundaryScenario.cs");
        var compilation = CSharpCompilation.Create(
            assemblyName: "AstConverterBoundaryScenarios_" + Guid.NewGuid().ToString("N"),
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    usings,
                    TestMetadataReferences.PreviewParseOptions,
                    path: "GlobalUsings.g.cs"),
                sourceTree
            ],
            references: TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        AssertCompilationSucceeded(compilation, scenarioId);

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

    private static AstConverterRuntimeClassValidationFixture CompileRuntimeClassValidation(string scenarioId)
    {
        const string source = """
            public static class TestModule
            {
            }

            public interface TargetInterface
            {
            }

            public struct TargetStruct
            {
            }

            public sealed record TargetRecord(int Value);
            """;
        var sourceTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "AstConverterRuntimeClassValidation.cs");
        var compilation = CSharpCompilation.Create(
            assemblyName: "AstConverterRuntimeClassValidations_" + Guid.NewGuid().ToString("N"),
            syntaxTrees: [sourceTree],
            references: TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        AssertCompilationSucceeded(compilation, scenarioId);

        var semanticModel = compilation.GetSemanticModel(sourceTree);
        var types = sourceTree.GetRoot()
            .DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
            .OfType<INamedTypeSymbol>()
            .ToDictionary(static symbol => symbol.Name, StringComparer.Ordinal);
        return new AstConverterRuntimeClassValidationFixture(
            types["TestModule"],
            semanticModel,
            types);
    }

    private static void AssertCompilationSucceeded(CSharpCompilation compilation, string scenarioId)
    {
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(
            0,
            errors,
            $"{scenarioId}:{Environment.NewLine}" +
            string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));
    }

    private sealed record AstConverterModuleFixture(
        INamedTypeSymbol Module,
        SemanticModel SemanticModel);

    private sealed record AstConverterRuntimeClassValidationFixture(
        INamedTypeSymbol Module,
        SemanticModel SemanticModel,
        IReadOnlyDictionary<string, INamedTypeSymbol> Types)
    {
        public INamedTypeSymbol GetNamedType(string name) => Types[name];
    }

    private sealed class ImportProbeSemanticWalkerHost(AstConverterImportProbe import) : SemanticWalkerHost
    {
        public override Expression? RewriteInvocationPreorder(
            IInvocationOperation operation,
            SenseArgument argument)
        {
            if (!string.Equals(operation.TargetMethod.ContainingType.Name, "ImportProbe", StringComparison.Ordinal) ||
                !string.Equals(operation.TargetMethod.Name, "Use", StringComparison.Ordinal))
            {
                return null;
            }

            var local = new Identifier(import.LocalName);
            Expression result;
            switch (import.Kind)
            {
                case AstConverterImportProbeKind.BoundNamed:
                    result = argument.BindImportSpecifier(import.ModulePath, import.ImportedName);
                    break;
                case AstConverterImportProbeKind.Default:
                    argument.MergeImportSpecifier(import.ModulePath, new ImportDefaultSpecifier(local));
                    result = local;
                    break;
                case AstConverterImportProbeKind.Namespace:
                    argument.MergeImportSpecifier(import.ModulePath, new ImportNamespaceSpecifier(local));
                    result = local;
                    break;
                case AstConverterImportProbeKind.NamedString:
                    argument.MergeImportSpecifier(
                        import.ModulePath,
                        new ImportSpecifier(
                            JavaScriptAstFactory.CreateStringLiteral(import.ImportedName),
                            local));
                    result = local;
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Unsupported import probe kind '{import.Kind}'.");
            }

            return import.Referenced
                ? result
                : new NumericLiteral(0, "0");
        }
    }
}

public enum AstConverterImportSuccessKind
{
    ReferencedDefault,
    ReferencedNamespace,
    ReferencedNamedString,
    UnreferencedDefault,
    UnreferencedNamespace
}

public enum AstConverterImportFailureKind
{
    CurrentModuleDefault,
    CurrentModuleNamespace,
    CurrentModuleNamedString
}

public enum AstConverterDeclaredNameKind
{
    LocalFunction,
    QueryFrom,
    QueryJoin,
    QueryJoinInto,
    QueryLet,
    QueryContinuation
}

public enum AstConverterRuntimeClassValidationKind
{
    NullSymbol,
    Interface,
    Struct,
    Record
}

public enum AstConverterImportProbeKind
{
    BoundNamed,
    Default,
    Namespace,
    NamedString
}

public enum AstConverterImportExpectationKind
{
    Default,
    Namespace,
    NamedString
}

public sealed record AstConverterImportProbe(
    string ModulePath,
    string ImportedName,
    string LocalName,
    AstConverterImportProbeKind Kind,
    bool Referenced);

public sealed record AstConverterImportExpectation(
    string ModulePath,
    string ImportedName,
    string LocalName,
    AstConverterImportExpectationKind Kind);

public sealed record AstConverterImportSuccessScenario(
    string Id,
    string Dimension,
    AstConverterImportSuccessKind Kind,
    string Source,
    AstConverterImportProbe Import,
    AstConverterImportExpectation? ExpectedImport)
{
    public string InputIdentity => $"{Kind}|{Import}|{Source}";
}

public sealed record AstConverterImportFailureScenario(
    string Id,
    string Dimension,
    AstConverterImportFailureKind Kind,
    string Source,
    AstConverterImportProbe Import,
    AstConverterImportExpectation ExpectedImport)
{
    public string InputIdentity => $"{Kind}|{Import}|{ExpectedImport}|{Source}";
}

public sealed record AstConverterDeclaredNameScenario(
    string Id,
    string Dimension,
    AstConverterDeclaredNameKind Kind,
    string Source,
    string ReservedName)
{
    public string InputIdentity => $"{Kind}|{ReservedName}|{Source}";
}

public sealed record AstConverterRuntimeClassValidationScenario(
    string Id,
    string Dimension,
    AstConverterRuntimeClassValidationKind Kind,
    IReadOnlyList<string> ExpectedMessageFragments)
{
    public string InputIdentity => $"{Kind}|{string.Join("|", ExpectedMessageFragments)}";
}

internal static class AstConverterBoundaryScenarioCatalog
{
    private const string ExternalModuleSource = """
        [ECMAScriptModule("./module")]
        public static class TestModule
        {
            public static int Run() => ImportProbe.Use();
        }

        public static class ImportProbe
        {
            public static int Use() => 0;
        }
        """;

    private const string CurrentModuleSource = """
        [ECMAScriptModule("./current")]
        public static class TestModule
        {
            public static int Run() => ImportProbe.Use();
        }

        public static class ImportProbe
        {
            public static int Use() => 0;
        }
        """;

    public static IReadOnlyList<AstConverterImportSuccessScenario> ImportSuccesses { get; } =
    [
        ImportSuccess(
            "referenced-default",
            "referenced-default-import-retained",
            AstConverterImportSuccessKind.ReferencedDefault,
            Probe("runtime/default", "default", "RuntimeDefault", AstConverterImportProbeKind.Default, true),
            Expected("runtime/default", "default", "RuntimeDefault", AstConverterImportExpectationKind.Default)),
        ImportSuccess(
            "referenced-namespace",
            "referenced-namespace-import-retained",
            AstConverterImportSuccessKind.ReferencedNamespace,
            Probe("runtime/namespace", "*", "RuntimeNamespace", AstConverterImportProbeKind.Namespace, true),
            Expected("runtime/namespace", "*", "RuntimeNamespace", AstConverterImportExpectationKind.Namespace)),
        ImportSuccess(
            "referenced-named-string",
            "referenced-string-named-import-retained",
            AstConverterImportSuccessKind.ReferencedNamedString,
            Probe("runtime/string", "external-name", "externalName", AstConverterImportProbeKind.NamedString, true),
            Expected("runtime/string", "external-name", "externalName", AstConverterImportExpectationKind.NamedString)),
        ImportSuccess(
            "unreferenced-default",
            "unreferenced-default-import-pruned-from-final-ast",
            AstConverterImportSuccessKind.UnreferencedDefault,
            Probe("runtime/default", "default", "UnusedDefault", AstConverterImportProbeKind.Default, false),
            null),
        ImportSuccess(
            "unreferenced-namespace",
            "unreferenced-namespace-import-pruned-from-final-ast",
            AstConverterImportSuccessKind.UnreferencedNamespace,
            Probe("runtime/namespace", "*", "UnusedNamespace", AstConverterImportProbeKind.Namespace, false),
            null)
    ];

    public static IReadOnlyList<AstConverterImportFailureScenario> ImportFailures { get; } =
    [
        ImportFailure(
            "current-module-default",
            "current-module-default-external-specifier-retained",
            AstConverterImportFailureKind.CurrentModuleDefault,
            Probe("./current", "default", "CurrentDefault", AstConverterImportProbeKind.Default, true),
            Expected("./current", "default", "CurrentDefault", AstConverterImportExpectationKind.Default)),
        ImportFailure(
            "current-module-namespace",
            "current-module-namespace-external-specifier-retained",
            AstConverterImportFailureKind.CurrentModuleNamespace,
            Probe("./current", "*", "CurrentNamespace", AstConverterImportProbeKind.Namespace, true),
            Expected("./current", "*", "CurrentNamespace", AstConverterImportExpectationKind.Namespace)),
        ImportFailure(
            "current-module-named-string",
            "current-module-string-named-external-specifier-retained",
            AstConverterImportFailureKind.CurrentModuleNamedString,
            Probe("./current", "external-name", "externalName", AstConverterImportProbeKind.NamedString, true),
            Expected("./current", "external-name", "externalName", AstConverterImportExpectationKind.NamedString))
    ];

    public static IReadOnlyList<AstConverterDeclaredNameScenario> DeclaredNames { get; } =
    [
        DeclaredName(
            "local-function",
            "local-function-name-reserves-import-binding",
            AstConverterDeclaredNameKind.LocalFunction,
            """
            private static int ReserveNames()
            {
                int make() => 1;
                return make();
            }
            """),
        DeclaredName(
            "query-from",
            "query-from-range-variable-reserves-import-binding",
            AstConverterDeclaredNameKind.QueryFrom,
            """
            private static IEnumerable<int> ReserveNames(IEnumerable<int> values) =>
                from make in values
                select make;
            """),
        DeclaredName(
            "query-join",
            "query-join-range-variable-reserves-import-binding",
            AstConverterDeclaredNameKind.QueryJoin,
            """
            private static IEnumerable<int> ReserveNames(IEnumerable<int> values) =>
                from left in values
                join make in values on left equals make
                select make;
            """),
        DeclaredName(
            "query-join-into",
            "query-join-continuation-reserves-import-binding",
            AstConverterDeclaredNameKind.QueryJoinInto,
            """
            private static IEnumerable<int> ReserveNames(IEnumerable<int> values) =>
                from left in values
                join right in values on left equals right into make
                select make.Count();
            """),
        DeclaredName(
            "query-let",
            "query-let-variable-reserves-import-binding",
            AstConverterDeclaredNameKind.QueryLet,
            """
            private static IEnumerable<int> ReserveNames(IEnumerable<int> values) =>
                from value in values
                let make = value + 1
                select make;
            """),
        DeclaredName(
            "query-continuation",
            "query-into-continuation-reserves-import-binding",
            AstConverterDeclaredNameKind.QueryContinuation,
            """
            private static IEnumerable<int> ReserveNames(IEnumerable<int> values) =>
                from value in values
                select value into make
                select make;
            """)
    ];

    public static IReadOnlyList<AstConverterRuntimeClassValidationScenario> RuntimeClassValidations { get; } =
    [
        RuntimeClassValidation(
            "null-symbol",
            "null-runtime-class-symbol-validation",
            AstConverterRuntimeClassValidationKind.NullSymbol,
            ["Value cannot be null", "symbol"]),
        RuntimeClassValidation(
            "interface",
            "interface-runtime-shape-rejection",
            AstConverterRuntimeClassValidationKind.Interface,
            ["does not support", "NamedType:TargetInterface"]),
        RuntimeClassValidation(
            "struct",
            "struct-runtime-shape-rejection",
            AstConverterRuntimeClassValidationKind.Struct,
            ["does not support", "NamedType:TargetStruct"]),
        RuntimeClassValidation(
            "record",
            "record-runtime-shape-rejection",
            AstConverterRuntimeClassValidationKind.Record,
            ["does not support", "NamedType:TargetRecord"])
    ];

    private static AstConverterImportSuccessScenario ImportSuccess(
        string id,
        string dimension,
        AstConverterImportSuccessKind kind,
        AstConverterImportProbe import,
        AstConverterImportExpectation? expectedImport)
        => new(
            $"ast-converter-boundary.import-success.{id}",
            dimension,
            kind,
            ExternalModuleSource,
            import,
            expectedImport);

    private static AstConverterImportFailureScenario ImportFailure(
        string id,
        string dimension,
        AstConverterImportFailureKind kind,
        AstConverterImportProbe import,
        AstConverterImportExpectation expectedImport)
        => new(
            $"ast-converter-boundary.import-failure.{id}",
            dimension,
            kind,
            CurrentModuleSource,
            import,
            expectedImport);

    private static AstConverterDeclaredNameScenario DeclaredName(
        string id,
        string dimension,
        AstConverterDeclaredNameKind kind,
        string reserveMember)
        => new(
            $"ast-converter-boundary.declared-name.{id}",
            dimension,
            kind,
            $$"""
            [ECMAScriptModule("./module")]
            public static class TestModule
            {
                public static int Run() => ImportProbe.Use();

                {{reserveMember}}
            }

            public static class ImportProbe
            {
                public static int Use() => 0;
            }
            """,
            "make");

    private static AstConverterRuntimeClassValidationScenario RuntimeClassValidation(
        string id,
        string dimension,
        AstConverterRuntimeClassValidationKind kind,
        IReadOnlyList<string> expectedMessageFragments)
        => new(
            $"ast-converter-boundary.runtime-class.{id}",
            dimension,
            kind,
            expectedMessageFragments);

    private static AstConverterImportProbe Probe(
        string modulePath,
        string importedName,
        string localName,
        AstConverterImportProbeKind kind,
        bool referenced)
        => new(modulePath, importedName, localName, kind, referenced);

    private static AstConverterImportExpectation Expected(
        string modulePath,
        string importedName,
        string localName,
        AstConverterImportExpectationKind kind)
        => new(modulePath, importedName, localName, kind);
}
