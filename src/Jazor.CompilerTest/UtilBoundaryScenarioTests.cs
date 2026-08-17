using ECMAScript;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;
using System.Runtime.Versioning;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class UtilBoundaryScenarioTests
{
    private static readonly Lazy<UtilSymbolFixture> SymbolFixture = new(CreateSymbolFixture);

    public static IEnumerable<TestDataRow<UtilLineEndingScenario>> LineEndingCases
        => UtilBoundaryScenarioCatalog.LineEndings.Select(static scenario =>
            new TestDataRow<UtilLineEndingScenario>(scenario) { DisplayName = scenario.Id });

    public static IEnumerable<TestDataRow<UtilSymbolNameScenario>> SymbolNameCases
        => UtilBoundaryScenarioCatalog.SymbolNames.Select(static scenario =>
            new TestDataRow<UtilSymbolNameScenario>(scenario) { DisplayName = scenario.Id });

    public static IEnumerable<TestDataRow<UtilBooleanScenario>> BooleanCases
        => UtilBoundaryScenarioCatalog.Booleans.Select(static scenario =>
            new TestDataRow<UtilBooleanScenario>(scenario) { DisplayName = scenario.Id });

    public static IEnumerable<TestDataRow<UtilModulePathScenario>> ModulePathCases
        => UtilBoundaryScenarioCatalog.ModulePaths.Select(static scenario =>
            new TestDataRow<UtilModulePathScenario>(scenario) { DisplayName = scenario.Id });

    [TestMethod]
    public void ScenarioCatalogs_HaveUniqueIdsDimensionsKindsAndInputs()
    {
        var allIds = UtilBoundaryScenarioCatalog.LineEndings.Select(static scenario => scenario.Id)
            .Concat(UtilBoundaryScenarioCatalog.SymbolNames.Select(static scenario => scenario.Id))
            .Concat(UtilBoundaryScenarioCatalog.Booleans.Select(static scenario => scenario.Id))
            .Concat(UtilBoundaryScenarioCatalog.ModulePaths.Select(static scenario => scenario.Id))
            .ToArray();
        var allInputs = UtilBoundaryScenarioCatalog.LineEndings.Select(static scenario => scenario.InputIdentity)
            .Concat(UtilBoundaryScenarioCatalog.SymbolNames.Select(static scenario => scenario.InputIdentity))
            .Concat(UtilBoundaryScenarioCatalog.Booleans.Select(static scenario => scenario.InputIdentity))
            .Concat(UtilBoundaryScenarioCatalog.ModulePaths.Select(static scenario => scenario.InputIdentity))
            .ToArray();

        Assert.IsNotEmpty(allIds);
        Assert.HasCount(allIds.Length, allIds.Distinct(StringComparer.Ordinal));
        Assert.HasCount(allInputs.Length, allInputs.Distinct(StringComparer.Ordinal));
        Assert.IsTrue(allIds.All(static id => id.StartsWith("util-boundary.", StringComparison.Ordinal)));
        Assert.IsTrue(UtilBoundaryScenarioCatalog.LineEndings.All(static scenario => !string.IsNullOrWhiteSpace(scenario.Dimension)));
        Assert.IsTrue(UtilBoundaryScenarioCatalog.SymbolNames.All(static scenario => !string.IsNullOrWhiteSpace(scenario.Dimension)));
        Assert.IsTrue(UtilBoundaryScenarioCatalog.Booleans.All(static scenario => !string.IsNullOrWhiteSpace(scenario.Dimension)));
        Assert.IsTrue(UtilBoundaryScenarioCatalog.ModulePaths.All(static scenario => !string.IsNullOrWhiteSpace(scenario.Dimension)));
        Assert.HasCount(
            Enum.GetValues<UtilSymbolNameKind>().Length,
            UtilBoundaryScenarioCatalog.SymbolNames.Select(static scenario => scenario.Kind).Distinct());
        Assert.HasCount(
            Enum.GetValues<UtilBooleanKind>().Length,
            UtilBoundaryScenarioCatalog.Booleans.Select(static scenario => scenario.Kind).Distinct());
        Assert.HasCount(
            Enum.GetValues<UtilModulePathKind>().Length,
            UtilBoundaryScenarioCatalog.ModulePaths.Select(static scenario => scenario.Kind).Distinct());
    }

    [TestMethod]
    public void JavaScriptNameMetadata_ReportsOnlyDistinctNonEmptyExplicitNamesAsConflicts()
    {
        var cases = new (Util.JavaScriptNameMetadata Metadata, bool Expected)[]
        {
            (new(false, "ecma", "description", false), false),
            (new(true, null, "description", false), false),
            (new(true, string.Empty, "description", false), false),
            (new(true, "ecma", null, false), false),
            (new(true, "ecma", string.Empty, false), false),
            (new(true, "same", "same", false), false),
            (new(true, "ecma", "description", false), true)
        };

        foreach (var (metadata, expected) in cases)
            Assert.AreEqual(expected, metadata.HasConflictingExplicitNames);
    }

    [TestMethod]
    [DynamicData(nameof(LineEndingCases))]
    public void NormalizeLineEndingsToLf_UsesPlatformIndependentContract(UtilLineEndingScenario scenario)
    {
        var actual = Util.NormalizeLineEndingsToLf(scenario.Input);

        Assert.AreEqual(scenario.Expected, actual, scenario.Id);
        Assert.IsFalse(actual.Contains('\r'), scenario.Id);
    }

    [TestMethod]
    [DynamicData(nameof(SymbolNameCases))]
    public void SymbolNaming_UsesExplicitMetadataAndStableFallbacks(UtilSymbolNameScenario scenario)
    {
        var fixture = SymbolFixture.Value;

        switch (scenario.Kind)
        {
            case UtilSymbolNameKind.NoArgumentAttributeThenDescription:
                AssertSymbolName(
                    fixture.GetMethod("NamingHost", "NoArgumentAttributeThenDescription"),
                    "descriptionAlias",
                    hasBoundary: false,
                    "descriptionAlias",
                    scenario.Id);
                break;
            case UtilSymbolNameKind.ExplicitNameOverridesDescription:
                AssertSymbolName(
                    fixture.GetMethod("NamingHost", "ExplicitNameOverridesDescription"),
                    "explicitAlias",
                    hasBoundary: false,
                    "explicitAlias",
                    scenario.Id);
                break;
            case UtilSymbolNameKind.BlankExplicitNameSuppressesDescription:
                AssertSymbolName(
                    fixture.GetMethod("NamingHost", "BlankExplicitNameSuppressesDescription"),
                    expectedConfigName: null,
                    hasBoundary: false,
                    "BlankExplicitNameSuppressesDescription",
                    scenario.Id);
                break;
            case UtilSymbolNameKind.DescriptionBoundary:
                AssertSymbolName(
                    fixture.GetMethod("NamingHost", "DescriptionBoundary"),
                    expectedConfigName: null,
                    hasBoundary: true,
                    "DescriptionBoundary",
                    scenario.Id);
                break;
            case UtilSymbolNameKind.PlainDescriptionIgnored:
                AssertSymbolName(
                    fixture.GetMethod("NamingHost", "PlainDescriptionIgnored"),
                    expectedConfigName: null,
                    hasBoundary: false,
                    "PlainDescriptionIgnored",
                    scenario.Id);
                break;
            case UtilSymbolNameKind.UnconfiguredMethod:
                Assert.AreEqual(
                    "Execute",
                    Util.GetConfigOrSymbolName(fixture.GetMethod("NamingHost", "Execute")),
                    scenario.Id);
                break;
            case UtilSymbolNameKind.OrdinaryOverloads:
                AssertOrdinaryOverloadNames(fixture, scenario.Id);
                break;
            case UtilSymbolNameKind.RuntimeHostOverloads:
                AssertRuntimeOverloadNames(fixture, scenario.Id);
                break;
            case UtilSymbolNameKind.PropertyFallback:
                Assert.AreEqual(
                    "Count",
                    Util.GetConfigOrSymbolName(fixture.GetProperty("NamingHost", "Count")),
                    scenario.Id);
                break;
            case UtilSymbolNameKind.FieldFallback:
                Assert.AreEqual(
                    "Total",
                    Util.GetConfigOrSymbolName(fixture.GetField("NamingHost", "Total")),
                    scenario.Id);
                break;
            case UtilSymbolNameKind.EventFallback:
                Assert.AreEqual(
                    "ValueChanged",
                    Util.GetConfigOrSymbolName(fixture.GetEvent("NamingHost", "ValueChanged")),
                    scenario.Id);
                break;
            case UtilSymbolNameKind.ExplicitEventName:
                AssertSymbolName(
                    fixture.GetEvent("NamingHost", "ExplicitlyNamedEvent"),
                    "renamedEvent",
                    hasBoundary: false,
                    "renamedEvent",
                    scenario.Id);
                break;
            case UtilSymbolNameKind.LocalFunctionPreservesSourceName:
                Assert.AreEqual(
                    "LocalWork",
                    Util.GetConfigOrSymbolName(fixture.LocalFunction),
                    scenario.Id);
                break;
            case UtilSymbolNameKind.TupleElementUsesCanonicalField:
                Assert.AreEqual(
                    "Alias",
                    Util.GetConfigOrSymbolName(fixture.TupleElement),
                    scenario.Id);
                break;
            case UtilSymbolNameKind.BackingFieldUsesPropertyHash:
                var property = fixture.GetProperty("NamingHost", "Count");
                Assert.AreEqual(
                    Jazor.Common.Format.HashName(property.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)),
                    Util.GetConfigOrSymbolName(fixture.BackingField),
                    scenario.Id);
                break;
            case UtilSymbolNameKind.ConstructorHelpersStableAndDistinct:
                AssertConstructorHelpers(fixture, scenario.Id);
                break;
            default:
                Assert.Fail($"{scenario.Id}: unsupported symbol-name kind '{scenario.Kind}'.");
                break;
        }
    }

    [TestMethod]
    public void ECMAScriptNameAttribute_AllowsEveryAttributeTarget()
    {
        var usage = typeof(ECMAScriptNameAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), inherit: false)
            .Cast<AttributeUsageAttribute>()
            .Single();

        Assert.AreEqual(AttributeTargets.All, usage.ValidOn);
        Assert.IsFalse(usage.Inherited);
        Assert.IsNull(typeof(ECMAScriptNameAttribute).GetCustomAttribute<SupportedOSPlatformAttribute>());
    }

    [TestMethod]
    [DynamicData(nameof(BooleanCases))]
    public void SymbolClassification_UsesRoslynTypeAndHostContracts(UtilBooleanScenario scenario)
    {
        var actual = EvaluateBooleanScenario(SymbolFixture.Value, scenario.Kind);

        Assert.AreEqual(scenario.Expected, actual, scenario.Id);
    }

    [TestMethod]
    [DynamicData(nameof(ModulePathCases))]
    public void GetECMAScriptModuleImportPath_ValidatesAndNormalizesConstructorValue(UtilModulePathScenario scenario)
    {
        var fixture = SymbolFixture.Value;
        var type = scenario.Kind switch
        {
            UtilModulePathKind.RuntimeImport => fixture.GetType("ImportedRuntime"),
            UtilModulePathKind.ModuleExport => fixture.GetType("ExportedModule"),
            UtilModulePathKind.MarkerWithoutArgument => fixture.GetType("RuntimeBase"),
            UtilModulePathKind.BlankImport => fixture.GetType("BlankImportedRuntime"),
            UtilModulePathKind.UnrelatedAttribute => fixture.GetType("BoundaryOnly"),
            _ => throw new InvalidOperationException($"{scenario.Id}: unsupported module-path kind '{scenario.Kind}'.")
        };

        Assert.AreEqual(scenario.Expected, Util.GetECMAScriptModuleImportPath(type), scenario.Id);
    }

    private static void AssertSymbolName(
        ISymbol symbol,
        string? expectedConfigName,
        bool hasBoundary,
        string expectedFinalName,
        string scenarioId)
    {
        Assert.AreEqual(expectedConfigName, Util.GetSymbolConfigName(symbol), scenarioId);
        Assert.AreEqual(hasBoundary, Util.HasNameResolutionBoundary(symbol), scenarioId);
        Assert.AreEqual(expectedFinalName, Util.GetConfigOrSymbolName(symbol), scenarioId);
    }

    private static void AssertOrdinaryOverloadNames(UtilSymbolFixture fixture, string scenarioId)
    {
        var overloads = fixture.GetMethods("NamingHost", "Run");
        var names = overloads.Select(Util.GetConfigOrSymbolName).ToArray();

        Assert.HasCount(2, names, scenarioId);
        Assert.HasCount(2, names.Distinct(StringComparer.Ordinal), scenarioId);
        Assert.IsTrue(names.All(static name => name.StartsWith("Run_", StringComparison.Ordinal)), scenarioId);
    }

    private static void AssertRuntimeOverloadNames(UtilSymbolFixture fixture, string scenarioId)
    {
        var names = fixture.GetMethods("RuntimeNamingHost", "Run")
            .Select(Util.GetConfigOrSymbolName)
            .ToArray();

        Assert.HasCount(2, names, scenarioId);
        Assert.IsTrue(names.All(static name => string.Equals(name, "Run", StringComparison.Ordinal)), scenarioId);
    }

    private static void AssertConstructorHelpers(UtilSymbolFixture fixture, string scenarioId)
    {
        var constructors = fixture.GetType("NamingHost").InstanceConstructors
            .Where(static constructor => !constructor.IsImplicitlyDeclared)
            .OrderBy(static constructor => constructor.Parameters.Length)
            .ToArray();
        var first = Util.GetMemberConstructorHelperName(constructors[0]);
        var second = Util.GetMemberConstructorHelperName(constructors[1]);

        Assert.AreEqual(first, Util.GetMemberConstructorHelperName(constructors[0]), scenarioId);
        Assert.AreNotEqual(first, second, scenarioId);
        Assert.IsTrue(first.StartsWith("$ctor_", StringComparison.Ordinal), scenarioId);
        Assert.IsTrue(second.StartsWith("$ctor_", StringComparison.Ordinal), scenarioId);
    }

    private static bool EvaluateBooleanScenario(UtilSymbolFixture fixture, UtilBooleanKind kind)
        => kind switch
        {
            UtilBooleanKind.SupportAttributeNull => Util.IsECMAScriptSupportMarkerAttribute(null),
            UtilBooleanKind.SupportAttributeRuntime => Util.IsECMAScriptSupportMarkerAttribute(fixture.ECMAScriptAttribute),
            UtilBooleanKind.SupportAttributeModule => Util.IsECMAScriptSupportMarkerAttribute(fixture.ECMAScriptModuleAttribute),
            UtilBooleanKind.SupportAttributeUnrelated => Util.IsECMAScriptSupportMarkerAttribute(fixture.DescriptionAttribute),
            UtilBooleanKind.SupportNull => Util.HasECMAScriptSupportMarker(null),
            UtilBooleanKind.SupportDirectType => Util.HasECMAScriptSupportMarker(fixture.GetType("RuntimeBase")),
            UtilBooleanKind.SupportContainingType => Util.HasECMAScriptSupportMarker(fixture.GetType("RuntimeOuter+Nested")),
            UtilBooleanKind.SupportMember => Util.HasECMAScriptSupportMarker(fixture.GetMethod("RuntimeBase", "Work")),
            UtilBooleanKind.SupportUnmarked => Util.HasECMAScriptSupportMarker(fixture.GetType("PlainClass")),
            UtilBooleanKind.SupportBaseType => Util.HasECMAScriptSupportMarkerBaseType(fixture.GetType("RuntimeDerived")),
            UtilBooleanKind.SupportMissingBaseType => Util.HasECMAScriptSupportMarkerBaseType(fixture.GetType("PlainClass")),
            UtilBooleanKind.RuntimeSymbolNull => Util.IsECMAScriptRuntimeSymbol(null),
            UtilBooleanKind.RuntimeSymbolType => Util.IsECMAScriptRuntimeSymbol(fixture.GetType("RuntimeBase")),
            UtilBooleanKind.RuntimeSymbolMember => Util.IsECMAScriptRuntimeSymbol(fixture.GetMethod("RuntimeBase", "Work")),
            UtilBooleanKind.RuntimeSymbolUnmarked => Util.IsECMAScriptRuntimeSymbol(fixture.GetMethod("PlainClass", "Work")),
            UtilBooleanKind.ObjectLiteralArray => Util.IsObjectLiteralHostType(fixture.GetArrayType("RuntimeBase")),
            UtilBooleanKind.ObjectLiteralRecord => Util.IsObjectLiteralHostType(fixture.GetType("PlainRecord")),
            UtilBooleanKind.ObjectLiteralMarkedWithoutBoundary => Util.IsObjectLiteralHostType(fixture.GetType("RuntimeBase")),
            UtilBooleanKind.ObjectLiteralMarkedBoundary => Util.IsObjectLiteralHostType(fixture.GetType("RuntimeObjectHost")),
            UtilBooleanKind.ObjectLiteralInheritedMarkerBoundary => Util.IsObjectLiteralHostType(fixture.GetType("RuntimeDerivedObjectHost")),
            UtilBooleanKind.ObjectLiteralBoundaryWithoutMarker => Util.IsObjectLiteralHostType(fixture.GetType("BoundaryOnly")),
            UtilBooleanKind.RecordProxyNull => Util.IsECMAScriptRecordProxyMember(null),
            UtilBooleanKind.RecordProxyNonRecord => Util.IsECMAScriptRecordProxyMember(fixture.GetMethod("PlainClass", "Work")),
            UtilBooleanKind.RecordProxyUnmarkedRecord => Util.IsECMAScriptRecordProxyMember(fixture.GetProperty("PlainRecord", "Value")),
            UtilBooleanKind.RecordProxyAutoProperty => Util.IsECMAScriptRecordProxyMember(fixture.GetProperty("RuntimeRecord", "Value")),
            UtilBooleanKind.RecordProxyConfiguredProperty => Util.IsECMAScriptRecordProxyMember(fixture.GetProperty("RuntimeRecord", "Named")),
            UtilBooleanKind.RecordProxyConfiguredPropertyGetter => Util.IsECMAScriptRecordProxyMember(fixture.GetProperty("RuntimeRecord", "Named").GetMethod),
            UtilBooleanKind.RecordProxyIndexer => Util.IsECMAScriptRecordProxyMember(fixture.GetIndexer("RuntimeRecord")),
            UtilBooleanKind.RecordProxyConfiguredMethod => Util.IsECMAScriptRecordProxyMember(fixture.GetMethod("RuntimeRecord", "Configured")),
            UtilBooleanKind.RecordProxyExternMethod => Util.IsECMAScriptRecordProxyMember(fixture.GetMethod("RuntimeRecord", "Imported")),
            UtilBooleanKind.RecordProxyInlineMethod => Util.IsECMAScriptRecordProxyMember(fixture.GetMethod("RuntimeRecord", "Inline")),
            UtilBooleanKind.RecordProxyNormalMethod => Util.IsECMAScriptRecordProxyMember(fixture.GetMethod("RuntimeRecord", "Normal")),
            UtilBooleanKind.RecordProxyField => Util.IsECMAScriptRecordProxyMember(fixture.GetField("RuntimeRecord", "Field")),
            UtilBooleanKind.RecordProxyBaseMarkedHost => Util.IsECMAScriptRecordProxyMember(fixture.GetProperty("DerivedRuntimeRecord", "Named")),
            UtilBooleanKind.UnionMarkerNull => Util.IsSystemUnionMarkerAttribute(null),
            UtilBooleanKind.UnionTypeNull => Util.IsSystemUnionType(null),
            UtilBooleanKind.UnionNativePlain => Util.IsSystemUnionType(fixture.GetType("PlainChoice")),
            UtilBooleanKind.UnionNativeRuntime => Util.IsSystemUnionType(fixture.GetType("RuntimeChoice")),
            UtilBooleanKind.UnionTaggedRuntime => Util.IsSystemUnionType(fixture.TaggedRuntimeUnion),
            UtilBooleanKind.RuntimeUnionNull => Util.IsRuntimeIUnionType(null),
            UtilBooleanKind.RuntimeUnionNative => Util.IsRuntimeIUnionType(fixture.GetType("RuntimeChoice")),
            UtilBooleanKind.RuntimeUnionTagged => Util.IsRuntimeIUnionType(fixture.TaggedRuntimeUnion),
            UtilBooleanKind.HostErasedUnionNull => Util.IsHostErasedUnionType(null),
            UtilBooleanKind.HostErasedUnionPlain => Util.IsHostErasedUnionType(fixture.GetType("PlainChoice")),
            UtilBooleanKind.HostErasedUnionRuntime => Util.IsHostErasedUnionType(fixture.GetType("RuntimeChoice")),
            UtilBooleanKind.HostErasedUnionTagged => Util.IsHostErasedUnionType(fixture.TaggedRuntimeUnion),
            UtilBooleanKind.StringMarkerNull => Util.IsStringEnumMarkerAttribute(null),
            UtilBooleanKind.StringMarkerAttribute => Util.IsStringEnumMarkerAttribute(fixture.StringAttribute),
            UtilBooleanKind.StringMarkerUnrelated => Util.IsStringEnumMarkerAttribute(fixture.DescriptionAttribute),
            UtilBooleanKind.StringEnumNull => Util.IsStringEnumType(null),
            UtilBooleanKind.StringEnumMarked => Util.IsStringEnumType(fixture.GetType("StringStatus")),
            UtilBooleanKind.StringEnumPlain => Util.IsStringEnumType(fixture.GetType("PlainStatus")),
            UtilBooleanKind.StringEnumNonEnum => Util.IsStringEnumType(fixture.GetType("RuntimeBase")),
            _ => throw new InvalidOperationException($"Unsupported boolean scenario kind '{kind}'.")
        };

    private static UtilSymbolFixture CreateSymbolFixture()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            UtilBoundaryScenarioCatalog.SymbolSource,
            TestMetadataReferences.PreviewParseOptions,
            path: "UtilBoundaryScenario.g.cs");
        var compilation = CSharpCompilation.Create(
            assemblyName: "UtilBoundaryScenarios_" + Guid.NewGuid().ToString("N"),
            syntaxTrees: [sourceTree],
            references: TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        if (errors.Length > 0)
        {
            throw new InvalidOperationException(
                string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));
        }

        var model = compilation.GetSemanticModel(sourceTree);
        var localFunctionSyntax = sourceTree.GetRoot()
            .DescendantNodes()
            .OfType<LocalFunctionStatementSyntax>()
            .Single(static local => local.Identifier.ValueText == "LocalWork");
        var localFunction = model.GetDeclaredSymbol(localFunctionSyntax)
            ?? throw new InvalidOperationException("LocalWork symbol was not available.");
        var tupleMethod = compilation.GetTypeByMetadataName("TupleHost")?
            .GetMembers("Create")
            .OfType<IMethodSymbol>()
            .Single()
            ?? throw new InvalidOperationException("TupleHost.Create symbol was not available.");
        var tupleElement = ((INamedTypeSymbol)tupleMethod.ReturnType).TupleElements[0];
        var namingHost = compilation.GetTypeByMetadataName("NamingHost")
            ?? throw new InvalidOperationException("NamingHost symbol was not available.");
        var backingField = namingHost.GetMembers()
            .OfType<IFieldSymbol>()
            .Single(static field => field.AssociatedSymbol?.Name == "Count");
        var taggedRuntimeUnion = compilation.GetTypeByMetadataName(typeof(ECMAScript.Vue.VueNumberPair).FullName!)
            ?? throw new InvalidOperationException("VueNumberPair metadata symbol was not available.");

        return new UtilSymbolFixture(
            compilation,
            localFunction,
            tupleElement,
            backingField,
            taggedRuntimeUnion);
    }
}

public enum UtilSymbolNameKind
{
    NoArgumentAttributeThenDescription,
    ExplicitNameOverridesDescription,
    BlankExplicitNameSuppressesDescription,
    DescriptionBoundary,
    PlainDescriptionIgnored,
    UnconfiguredMethod,
    OrdinaryOverloads,
    RuntimeHostOverloads,
    PropertyFallback,
    FieldFallback,
    EventFallback,
    ExplicitEventName,
    LocalFunctionPreservesSourceName,
    TupleElementUsesCanonicalField,
    BackingFieldUsesPropertyHash,
    ConstructorHelpersStableAndDistinct
}

public enum UtilModulePathKind
{
    RuntimeImport,
    ModuleExport,
    MarkerWithoutArgument,
    BlankImport,
    UnrelatedAttribute
}

public enum UtilBooleanKind
{
    SupportAttributeNull,
    SupportAttributeRuntime,
    SupportAttributeModule,
    SupportAttributeUnrelated,
    SupportNull,
    SupportDirectType,
    SupportContainingType,
    SupportMember,
    SupportUnmarked,
    SupportBaseType,
    SupportMissingBaseType,
    RuntimeSymbolNull,
    RuntimeSymbolType,
    RuntimeSymbolMember,
    RuntimeSymbolUnmarked,
    ObjectLiteralArray,
    ObjectLiteralRecord,
    ObjectLiteralMarkedWithoutBoundary,
    ObjectLiteralMarkedBoundary,
    ObjectLiteralInheritedMarkerBoundary,
    ObjectLiteralBoundaryWithoutMarker,
    RecordProxyNull,
    RecordProxyNonRecord,
    RecordProxyUnmarkedRecord,
    RecordProxyAutoProperty,
    RecordProxyConfiguredProperty,
    RecordProxyConfiguredPropertyGetter,
    RecordProxyIndexer,
    RecordProxyConfiguredMethod,
    RecordProxyExternMethod,
    RecordProxyInlineMethod,
    RecordProxyNormalMethod,
    RecordProxyField,
    RecordProxyBaseMarkedHost,
    UnionMarkerNull,
    UnionTypeNull,
    UnionNativePlain,
    UnionNativeRuntime,
    UnionTaggedRuntime,
    RuntimeUnionNull,
    RuntimeUnionNative,
    RuntimeUnionTagged,
    HostErasedUnionNull,
    HostErasedUnionPlain,
    HostErasedUnionRuntime,
    HostErasedUnionTagged,
    StringMarkerNull,
    StringMarkerAttribute,
    StringMarkerUnrelated,
    StringEnumNull,
    StringEnumMarked,
    StringEnumPlain,
    StringEnumNonEnum
}

public sealed record UtilLineEndingScenario(
    string Id,
    string Dimension,
    string? Input,
    string Expected)
{
    public string InputIdentity => $"line-ending|{Input?.Replace("\r", "<cr>").Replace("\n", "<lf>") ?? "<null>"}";
}

public sealed record UtilSymbolNameScenario(
    string Id,
    string Dimension,
    UtilSymbolNameKind Kind)
{
    public string InputIdentity => $"symbol-name|{Kind}";
}

public sealed record UtilBooleanScenario(
    string Id,
    string Dimension,
    UtilBooleanKind Kind,
    bool Expected)
{
    public string InputIdentity => $"boolean|{Kind}";
}

public sealed record UtilModulePathScenario(
    string Id,
    string Dimension,
    UtilModulePathKind Kind,
    string? Expected)
{
    public string InputIdentity => $"module-path|{Kind}";
}

internal sealed record UtilSymbolFixture(
    CSharpCompilation Compilation,
    IMethodSymbol LocalFunction,
    IFieldSymbol TupleElement,
    IFieldSymbol BackingField,
    INamedTypeSymbol TaggedRuntimeUnion)
{
    public INamedTypeSymbol ECMAScriptAttribute => GetType("ECMAScript.ECMAScriptAttribute");

    public INamedTypeSymbol ECMAScriptModuleAttribute => GetType("ECMAScript.ECMAScriptModuleAttribute");

    public INamedTypeSymbol DescriptionAttribute => GetType("System.ComponentModel.DescriptionAttribute");

    public INamedTypeSymbol StringAttribute => GetType("ECMAScript.StringAttribute");

    public INamedTypeSymbol GetType(string metadataName)
        => Compilation.GetTypeByMetadataName(metadataName)
            ?? throw new InvalidOperationException($"Type '{metadataName}' was not available.");

    public IArrayTypeSymbol GetArrayType(string elementMetadataName)
        => Compilation.CreateArrayTypeSymbol(GetType(elementMetadataName));

    public IMethodSymbol GetMethod(string typeMetadataName, string name)
        => GetMethods(typeMetadataName, name).Single();

    public IReadOnlyList<IMethodSymbol> GetMethods(string typeMetadataName, string name)
        => GetType(typeMetadataName).GetMembers(name).OfType<IMethodSymbol>().ToArray();

    public IPropertySymbol GetProperty(string typeMetadataName, string name)
        => GetType(typeMetadataName).GetMembers(name).OfType<IPropertySymbol>().Single();

    public IPropertySymbol GetIndexer(string typeMetadataName)
        => GetType(typeMetadataName).GetMembers().OfType<IPropertySymbol>().Single(static property => property.IsIndexer);

    public IFieldSymbol GetField(string typeMetadataName, string name)
        => GetType(typeMetadataName).GetMembers(name).OfType<IFieldSymbol>().Single();

    public IEventSymbol GetEvent(string typeMetadataName, string name)
        => GetType(typeMetadataName).GetMembers(name).OfType<IEventSymbol>().Single();
}

internal static class UtilBoundaryScenarioCatalog
{
    public static IReadOnlyList<UtilLineEndingScenario> LineEndings { get; } =
    [
        LineEnding("null", "null-normalizes-to-empty", null, ""),
        LineEnding("empty", "empty-remains-empty", "", ""),
        LineEnding("lf-only", "existing-lf-content-preserved", "alpha\nbeta", "alpha\nbeta"),
        LineEnding("crlf", "windows-crlf-normalizes-to-lf", "alpha\r\nbeta", "alpha\nbeta"),
        LineEnding("mixed", "mixed-crlf-and-cr-normalize-to-lf", "alpha\r\nbeta\rgamma", "alpha\nbeta\ngamma")
    ];

    public static IReadOnlyList<UtilSymbolNameScenario> SymbolNames { get; } =
        Enum.GetValues<UtilSymbolNameKind>()
            .Select(static kind => new UtilSymbolNameScenario(
                $"util-boundary.symbol-name.{ToId(kind)}",
                ToId(kind),
                kind))
            .ToArray();

    public static IReadOnlyList<UtilBooleanScenario> Booleans { get; } =
    [
        .. False(
            UtilBooleanKind.SupportAttributeNull,
            UtilBooleanKind.SupportAttributeUnrelated,
            UtilBooleanKind.SupportNull,
            UtilBooleanKind.SupportUnmarked,
            UtilBooleanKind.SupportMissingBaseType,
            UtilBooleanKind.RuntimeSymbolNull,
            UtilBooleanKind.RuntimeSymbolUnmarked,
            UtilBooleanKind.ObjectLiteralArray,
            UtilBooleanKind.ObjectLiteralMarkedWithoutBoundary,
            UtilBooleanKind.ObjectLiteralBoundaryWithoutMarker,
            UtilBooleanKind.RecordProxyNull,
            UtilBooleanKind.RecordProxyNonRecord,
            UtilBooleanKind.RecordProxyUnmarkedRecord,
            UtilBooleanKind.RecordProxyAutoProperty,
            UtilBooleanKind.RecordProxyNormalMethod,
            UtilBooleanKind.RecordProxyField,
            UtilBooleanKind.UnionMarkerNull,
            UtilBooleanKind.UnionTypeNull,
            UtilBooleanKind.RuntimeUnionNull,
            UtilBooleanKind.HostErasedUnionNull,
            UtilBooleanKind.HostErasedUnionPlain,
            UtilBooleanKind.StringMarkerNull,
            UtilBooleanKind.StringMarkerUnrelated,
            UtilBooleanKind.StringEnumNull,
            UtilBooleanKind.StringEnumPlain,
            UtilBooleanKind.StringEnumNonEnum),
        .. True(
            UtilBooleanKind.SupportAttributeRuntime,
            UtilBooleanKind.SupportAttributeModule,
            UtilBooleanKind.SupportDirectType,
            UtilBooleanKind.SupportContainingType,
            UtilBooleanKind.SupportMember,
            UtilBooleanKind.SupportBaseType,
            UtilBooleanKind.RuntimeSymbolType,
            UtilBooleanKind.RuntimeSymbolMember,
            UtilBooleanKind.ObjectLiteralRecord,
            UtilBooleanKind.ObjectLiteralMarkedBoundary,
            UtilBooleanKind.ObjectLiteralInheritedMarkerBoundary,
            UtilBooleanKind.RecordProxyConfiguredProperty,
            UtilBooleanKind.RecordProxyConfiguredPropertyGetter,
            UtilBooleanKind.RecordProxyIndexer,
            UtilBooleanKind.RecordProxyConfiguredMethod,
            UtilBooleanKind.RecordProxyExternMethod,
            UtilBooleanKind.RecordProxyInlineMethod,
            UtilBooleanKind.RecordProxyBaseMarkedHost,
            UtilBooleanKind.UnionNativePlain,
            UtilBooleanKind.UnionNativeRuntime,
            UtilBooleanKind.UnionTaggedRuntime,
            UtilBooleanKind.RuntimeUnionNative,
            UtilBooleanKind.RuntimeUnionTagged,
            UtilBooleanKind.HostErasedUnionRuntime,
            UtilBooleanKind.HostErasedUnionTagged,
            UtilBooleanKind.StringMarkerAttribute,
            UtilBooleanKind.StringEnumMarked)
    ];

    public static IReadOnlyList<UtilModulePathScenario> ModulePaths { get; } =
    [
        ModulePath("runtime-import", "runtime-import-trims-and-normalizes-separators", UtilModulePathKind.RuntimeImport, "./runtime/bridge.mjs"),
        ModulePath("module-export", "module-marker-normalizes-extension", UtilModulePathKind.ModuleExport, "./components/widget.mjs"),
        ModulePath("marker-without-argument", "parameterless-marker-has-no-import-path", UtilModulePathKind.MarkerWithoutArgument, null),
        ModulePath("blank-import", "blank-import-path-is-ignored", UtilModulePathKind.BlankImport, null),
        ModulePath("unrelated-attribute", "non-runtime-attribute-is-ignored", UtilModulePathKind.UnrelatedAttribute, null)
    ];

    public const string SymbolSource = """
        using System;
        using System.ComponentModel;
        using ECMAScript;

        [AttributeUsage(AttributeTargets.Method)]
        public sealed class NoArgumentAttribute : Attribute
        {
        }

        public sealed class NamingHost
        {
            public NamingHost()
            {
            }

            public NamingHost(int value)
            {
            }

            [NoArgument]
            [Description("@#descriptionAlias")]
            public void NoArgumentAttributeThenDescription()
            {
            }

            [Description("@#ignoredAlias")]
            [ECMAScriptName(" explicitAlias ")]
            public void ExplicitNameOverridesDescription()
            {
            }

            [ECMAScriptName(" ")]
            [Description("@#ignoredAlias")]
            public void BlankExplicitNameSuppressesDescription()
            {
            }

            [Description("@#")]
            public void DescriptionBoundary()
            {
            }

            [Description("plain description")]
            public void PlainDescriptionIgnored()
            {
            }

            public void Execute()
            {
            }

            public void Run()
            {
            }

            public void Run(int value)
            {
            }

            public int Count { get; set; }

            public int Total;

            public event Action? ValueChanged;

            [ECMAScriptName("renamedEvent")]
            public event Action? ExplicitlyNamedEvent;

            public void DeclareLocal()
            {
                void LocalWork()
                {
                }

                LocalWork();
            }
        }

        [ECMAScript]
        public static class RuntimeNamingHost
        {
            public static extern void Run();

            public static extern void Run(int value);
        }

        public static class TupleHost
        {
            public static (int Alias, string Label) Create() => (1, "label");
        }

        [ECMAScript]
        public class RuntimeBase
        {
            public void Work()
            {
            }
        }

        public sealed class RuntimeDerived : RuntimeBase
        {
        }

        [ECMAScript]
        public static class RuntimeOuter
        {
            public sealed class Nested
            {
            }
        }

        public sealed class PlainClass
        {
            public void Work()
            {
            }
        }

        [Description("@#")]
        public sealed class BoundaryOnly
        {
        }

        [ECMAScript]
        [Description("@#")]
        public sealed class RuntimeObjectHost
        {
        }

        [Description("@#")]
        public sealed class RuntimeDerivedObjectHost : RuntimeBase
        {
        }

        public sealed record PlainRecord(int Value);

        [ECMAScript]
        public record RuntimeRecord
        {
            public int Value { get; set; }

            [Description("@#named")]
            public int Named { get; set; }

            public int this[int index] => index;

            [Description("@#configured")]
            public void Configured()
            {
            }

            public extern void Imported();

            [ECMAScriptInline("__arg1")]
            public static RuntimeRecord Inline(RuntimeRecord value) => value;

            public void Normal()
            {
            }

            public int Field;
        }

        [ECMAScript]
        public record RuntimeRecordBase;

        public sealed record DerivedRuntimeRecord : RuntimeRecordBase
        {
            [Description("@#named")]
            public int Named { get; set; }
        }

        [ECMAScript(" ./runtime\\bridge ")]
        public sealed class ImportedRuntime
        {
        }

        [ECMAScriptModule("./components/widget")]
        public static class ExportedModule
        {
        }

        [ECMAScript(" ")]
        public sealed class BlankImportedRuntime
        {
        }

        public readonly union PlainChoice(string, int);

        [ECMAScript]
        public readonly union RuntimeChoice(string, int);

        [ECMAScript.String]
        public enum StringStatus
        {
            Ready
        }

        public enum PlainStatus
        {
            Ready
        }
        """;

    private static IReadOnlyList<UtilBooleanScenario> False(params UtilBooleanKind[] kinds)
        => kinds.Select(static kind => Boolean(kind, expected: false)).ToArray();

    private static IReadOnlyList<UtilBooleanScenario> True(params UtilBooleanKind[] kinds)
        => kinds.Select(static kind => Boolean(kind, expected: true)).ToArray();

    private static UtilBooleanScenario Boolean(UtilBooleanKind kind, bool expected)
        => new($"util-boundary.boolean.{ToId(kind)}", ToId(kind), kind, expected);

    private static UtilLineEndingScenario LineEnding(string id, string dimension, string? input, string expected)
        => new($"util-boundary.line-ending.{id}", dimension, input, expected);

    private static UtilModulePathScenario ModulePath(
        string id,
        string dimension,
        UtilModulePathKind kind,
        string? expected)
        => new($"util-boundary.module-path.{id}", dimension, kind, expected);

    private static string ToId<TEnum>(TEnum value)
        where TEnum : struct, Enum
    {
        var name = value.ToString();
        var builder = new System.Text.StringBuilder(name.Length + 8);
        for (var index = 0; index < name.Length; index++)
        {
            var character = name[index];
            if (index > 0 && char.IsUpper(character))
                builder.Append('-');
            builder.Append(char.ToLowerInvariant(character));
        }

        return builder.ToString();
    }
}
