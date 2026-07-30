using ECMAScript;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class UniqueNameInfrastructureScenarioTests
{
    public static IEnumerable<TestDataRow<UniqueNameLoweringSiteScenario>> LoweringSiteCases
        => UniqueNameInfrastructureScenarioCatalog.LoweringSites.Select(static testCase =>
            new TestDataRow<UniqueNameLoweringSiteScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    public static IEnumerable<TestDataRow<UniqueNameScopeSiteScenario>> ScopeSiteCases
        => UniqueNameInfrastructureScenarioCatalog.ScopeSites.Select(static testCase =>
            new TestDataRow<UniqueNameScopeSiteScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    public static IEnumerable<TestDataRow<UniqueNameHashScenario>> HashCases
        => UniqueNameInfrastructureScenarioCatalog.HashCases.Select(static testCase =>
            new TestDataRow<UniqueNameHashScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    public static IEnumerable<TestDataRow<UniqueNameBehaviorScenario>> BehaviorCases
        => UniqueNameInfrastructureScenarioCatalog.BehaviorCases.Select(static testCase =>
            new TestDataRow<UniqueNameBehaviorScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    public static IEnumerable<TestDataRow<UniqueNameValidationScenario>> ValidationCases
        => UniqueNameInfrastructureScenarioCatalog.ValidationCases.Select(static testCase =>
            new TestDataRow<UniqueNameValidationScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalogs_HaveUniqueIdsDimensionsAndKinds()
    {
        var allIds = UniqueNameInfrastructureScenarioCatalog.LoweringSites.Select(static testCase => testCase.Id)
            .Concat(UniqueNameInfrastructureScenarioCatalog.ScopeSites.Select(static testCase => testCase.Id))
            .Concat(UniqueNameInfrastructureScenarioCatalog.HashCases.Select(static testCase => testCase.Id))
            .Concat(UniqueNameInfrastructureScenarioCatalog.BehaviorCases.Select(static testCase => testCase.Id))
            .Concat(UniqueNameInfrastructureScenarioCatalog.ValidationCases.Select(static testCase => testCase.Id))
            .ToArray();
        var allDimensions = UniqueNameInfrastructureScenarioCatalog.LoweringSites.Select(static testCase => testCase.Dimension)
            .Concat(UniqueNameInfrastructureScenarioCatalog.ScopeSites.Select(static testCase => testCase.Dimension))
            .Concat(UniqueNameInfrastructureScenarioCatalog.HashCases.Select(static testCase => testCase.Dimension))
            .Concat(UniqueNameInfrastructureScenarioCatalog.BehaviorCases.Select(static testCase => testCase.Dimension))
            .Concat(UniqueNameInfrastructureScenarioCatalog.ValidationCases.Select(static testCase => testCase.Dimension))
            .ToArray();

        Assert.IsNotEmpty(allIds);
        Assert.HasCount(allIds.Length, allIds.Distinct(StringComparer.Ordinal));
        Assert.IsTrue(allIds.All(static id => id.StartsWith("unique-name.", StringComparison.Ordinal)));
        Assert.IsTrue(allDimensions.All(static dimension => !string.IsNullOrWhiteSpace(dimension)));
        Assert.HasCount(
            Enum.GetValues<UniqueNameLoweringSiteScenarioKind>().Length,
            UniqueNameInfrastructureScenarioCatalog.LoweringSites.Select(static testCase => testCase.Kind).Distinct());
        Assert.HasCount(
            Enum.GetValues<UniqueNameScopeSiteScenarioKind>().Length,
            UniqueNameInfrastructureScenarioCatalog.ScopeSites.Select(static testCase => testCase.Kind).Distinct());
        Assert.HasCount(
            Enum.GetValues<UniqueNameBehaviorScenarioKind>().Length,
            UniqueNameInfrastructureScenarioCatalog.BehaviorCases.Select(static testCase => testCase.Kind).Distinct());
        Assert.HasCount(
            Enum.GetValues<UniqueNameValidationScenarioKind>().Length,
            UniqueNameInfrastructureScenarioCatalog.ValidationCases.Select(static testCase => testCase.Kind).Distinct());
    }

    [TestMethod]
    [DynamicData(nameof(LoweringSiteCases))]
    public void CreateName_MatchesLoweringSiteContract(UniqueNameLoweringSiteScenario testCase)
    {
        var block = GetBlockOperation(SimpleMethodSource);
        var session = new UniqueNameSession(block, ScopeSite.RootFragment());
        var site = CreateLoweringSite(testCase.Kind);
        var owner = new LoweringNameOwner("stable-owner", "identity-owner");

        var expected = session.CreateName(site, session.RootScope.ScopeKey, owner, "p");
        var actual = session.CreateName(site, session.RootScope.ScopeKey, owner, "p");

        Assert.AreEqual(testCase.ExpectedTag, site.Tag, testCase.Id);
        Assert.AreEqual(testCase.ExpectedSlot, site.Slot, testCase.Id);
        Assert.AreEqual(expected, actual, testCase.Id);
        Assert.IsTrue(actual.StartsWith($"__{testCase.ExpectedTag}$", StringComparison.Ordinal), testCase.Id);
        Assert.AreEqual(testCase.ExpectedTag.Length + 27, actual.Length, testCase.Id);
        Assert.IsTrue(actual[(testCase.ExpectedTag.Length + 3)..].All(static character =>
            character is >= '0' and <= '9' or >= 'a' and <= 'f'), testCase.Id);
    }

    [TestMethod]
    [DynamicData(nameof(ScopeSiteCases))]
    public void CreateScopeKey_MatchesScopeSiteContract(UniqueNameScopeSiteScenario testCase)
    {
        var block = GetBlockOperation(SimpleMethodSource);
        var session = new UniqueNameSession(block, ScopeSite.RootFragment());
        var site = CreateScopeSite(testCase.Kind);

        var expected = session.CreateScopeKey("parent-scope", site);
        var actual = session.CreateScopeKey("parent-scope", site);
        var rootParent = session.CreateScopeKey(parentScopeKey: null, site);

        Assert.AreEqual(testCase.ExpectedKind, site.Kind.ToString(), testCase.Id);
        Assert.AreEqual(expected, actual, testCase.Id);
        Assert.AreNotEqual(rootParent, actual, testCase.Id);
        Assert.IsTrue(actual.StartsWith("sc_", StringComparison.Ordinal), testCase.Id);
        Assert.AreEqual(27, actual.Length, testCase.Id);
    }

    [TestMethod]
    [DynamicData(nameof(HashCases))]
    public void HashHex_MatchesUtf8Sha256PrefixContract(UniqueNameHashScenario testCase)
    {
        var actual = UniqueNameSession.HashHex(testCase.Text, testCase.RequestedLength);
        var expectedLength = Math.Min(testCase.RequestedLength, testCase.FullHash.Length);

        Assert.AreEqual(testCase.FullHash[..expectedLength], actual, testCase.Id);
        Assert.AreEqual(expectedLength, actual.Length, testCase.Id);
    }

    [TestMethod]
    [DynamicData(nameof(BehaviorCases))]
    public void Infrastructure_MatchesBehaviorScenarioContract(UniqueNameBehaviorScenario testCase)
    {
        switch (testCase.Kind)
        {
            case UniqueNameBehaviorScenarioKind.RootScopeContract:
                AssertRootScopeContract(testCase.Id);
                break;
            case UniqueNameBehaviorScenarioKind.RootScopeCrossSessionStability:
                AssertRootScopeCrossSessionStability(testCase.Id);
                break;
            case UniqueNameBehaviorScenarioKind.ChildScopeContract:
                AssertChildScopeContract(testCase.Id);
                break;
            case UniqueNameBehaviorScenarioKind.SiblingScopeStability:
                AssertSiblingScopeStability(testCase.Id);
                break;
            case UniqueNameBehaviorScenarioKind.ScopeAllocationIsolation:
                AssertScopeAllocationIsolation(testCase.Id);
                break;
            case UniqueNameBehaviorScenarioKind.OwnerIdentityExcludedFromVisibleName:
                AssertOwnerIdentityExcludedFromVisibleName(testCase.Id);
                break;
            case UniqueNameBehaviorScenarioKind.NameSaltSeparation:
                AssertNameSaltSeparation(testCase.Id);
                break;
            case UniqueNameBehaviorScenarioKind.OperationTreePaths:
                AssertOperationTreePaths(testCase.Id);
                break;
            case UniqueNameBehaviorScenarioKind.DetachedOperationCaching:
                AssertDetachedOperationCaching(testCase.Id);
                break;
            case UniqueNameBehaviorScenarioKind.OwnerCheckoutPathStability:
                AssertOwnerCheckoutPathStability(testCase.Id);
                break;
            case UniqueNameBehaviorScenarioKind.OwnerSymbolSeparation:
                AssertOwnerSymbolSeparation(testCase.Id);
                break;
            case UniqueNameBehaviorScenarioKind.OwnerBodyEditStability:
                AssertOwnerBodyEditStability(testCase.Id);
                break;
            default:
                Assert.Fail($"{testCase.Id}: unsupported behavior kind '{testCase.Kind}'.");
                break;
        }
    }

    [TestMethod]
    [DynamicData(nameof(ValidationCases))]
    public void Infrastructure_RejectsInvalidInputs(UniqueNameValidationScenario testCase)
    {
        var block = GetBlockOperation(SimpleMethodSource);

        Exception exception = testCase.Kind switch
        {
            UniqueNameValidationScenarioKind.NullSessionRoot => Assert.ThrowsExactly<ArgumentNullException>(() =>
                new UniqueNameSession(null!, ScopeSite.RootFragment())),
            UniqueNameValidationScenarioKind.NullIdentityIndexRoot => Assert.ThrowsExactly<ArgumentNullException>(() =>
                new OperationIdentityIndex(null!)),
            UniqueNameValidationScenarioKind.NullIdentityOperation => Assert.ThrowsExactly<ArgumentNullException>(() =>
                new OperationIdentityIndex(block).GetIdentity(null!)),
            UniqueNameValidationScenarioKind.ZeroHashLength => Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
                UniqueNameSession.HashHex("value", 0)),
            UniqueNameValidationScenarioKind.NegativeHashLength => Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
                UniqueNameSession.HashHex("value", -1)),
            _ => throw new InvalidOperationException(
                $"{testCase.Id}: unsupported validation kind '{testCase.Kind}'.")
        };

        Assert.AreEqual(testCase.ExpectedParameterName, (exception as ArgumentException)?.ParamName, testCase.Id);
    }

    private static void AssertRootScopeContract(string scenarioId)
    {
        var block = GetBlockOperation(SimpleMethodSource);
        var session = new UniqueNameSession(block, ScopeSite.RootFragment());

        Assert.IsTrue(session.OwnerKey.StartsWith("symbol|", StringComparison.Ordinal), scenarioId);
        Assert.AreSame(session, session.RootScope.Session, scenarioId);
        Assert.AreSame(block, session.RootScope.Anchor, scenarioId);
        Assert.AreEqual("r", session.GetOperationIdentity(block), scenarioId);
    }

    private static void AssertRootScopeCrossSessionStability(string scenarioId)
    {
        var first = GetBlockOperation(SimpleMethodSource, @"C:\checkout-a\src\Feature\Handler.cs");
        var second = GetBlockOperation(SimpleMethodSource, @"D:\checkout-b\src\Feature\Handler.cs");
        var firstSession = new UniqueNameSession(first, ScopeSite.RootFragment());
        var secondSession = new UniqueNameSession(second, ScopeSite.RootFragment());

        Assert.AreEqual(firstSession.OwnerKey, secondSession.OwnerKey, scenarioId);
        Assert.AreEqual(firstSession.RootScope.ScopeKey, secondSession.RootScope.ScopeKey, scenarioId);
    }

    private static void AssertChildScopeContract(string scenarioId)
    {
        var block = GetBlockOperation(TwoStatementMethodSource);
        var session = new UniqueNameSession(block, ScopeSite.RootFragment());
        var anchor = block.ChildOperations.First();
        var child = session.RootScope.Enter(anchor, ScopeSite.NestedBlock());

        Assert.AreSame(session, child.Session, scenarioId);
        Assert.AreSame(anchor, child.Anchor, scenarioId);
        Assert.AreNotEqual(session.RootScope.ScopeKey, child.ScopeKey, scenarioId);
    }

    private static void AssertSiblingScopeStability(string scenarioId)
    {
        var block = GetBlockOperation(TwoStatementMethodSource);
        var session = new UniqueNameSession(block, ScopeSite.RootFragment());
        var operations = block.ChildOperations.ToArray();
        var first = session.RootScope.Enter(operations[0], ScopeSite.NestedBlock());
        var second = session.RootScope.Enter(operations[1], ScopeSite.NestedBlock());
        var owner = new LoweringNameOwner("stable-owner", "identity-owner");

        Assert.AreEqual(first.ScopeKey, second.ScopeKey, scenarioId);
        Assert.AreEqual(
            first.Allocate(owner, LoweringSite.ReferenceTemp()),
            second.Allocate(owner, LoweringSite.ReferenceTemp()),
            scenarioId);
    }

    private static void AssertScopeAllocationIsolation(string scenarioId)
    {
        var block = GetBlockOperation(TwoStatementMethodSource);
        var session = new UniqueNameSession(block, ScopeSite.RootFragment());
        var child = session.RootScope.Enter(block.ChildOperations.First(), ScopeSite.NestedBlock());
        var owner = new LoweringNameOwner("stable-owner", "identity-owner");
        var site = LoweringSite.CreationTemp();

        var rootName = session.RootScope.Allocate(owner, site);
        var childName = child.Allocate(owner, site);

        Assert.AreNotEqual(rootName, childName, scenarioId);
        Assert.AreEqual(rootName, session.RootScope.Allocate(owner, site), scenarioId);
        Assert.AreEqual(childName, child.Allocate(owner, site), scenarioId);
    }

    private static void AssertOwnerIdentityExcludedFromVisibleName(string scenarioId)
    {
        var block = GetBlockOperation(SimpleMethodSource);
        var session = new UniqueNameSession(block, ScopeSite.RootFragment());
        var firstOwner = new LoweringNameOwner("stable-owner", "identity-a");
        var secondOwner = new LoweringNameOwner("stable-owner", "identity-b");

        var first = session.CreateName(LoweringSite.ReferenceTemp(), session.RootScope.ScopeKey, firstOwner, "p");
        var second = session.CreateName(LoweringSite.ReferenceTemp(), session.RootScope.ScopeKey, secondOwner, "p");

        Assert.AreEqual(first, second, scenarioId);
    }

    private static void AssertNameSaltSeparation(string scenarioId)
    {
        var block = GetBlockOperation(SimpleMethodSource);
        var session = new UniqueNameSession(block, ScopeSite.RootFragment());
        var owner = new LoweringNameOwner("stable-owner", "identity-owner");

        var primary = session.CreateName(LoweringSite.ReferenceTemp(), session.RootScope.ScopeKey, owner, "p");
        var fallback = session.CreateName(LoweringSite.ReferenceTemp(), session.RootScope.ScopeKey, owner, "f1");

        Assert.AreNotEqual(primary, fallback, scenarioId);
    }

    private static void AssertOperationTreePaths(string scenarioId)
    {
        var block = GetBlockOperation(TwoStatementMethodSource);
        var index = new OperationIdentityIndex(block);
        var rootChildren = block.ChildOperations.ToArray();
        var declarationGroup = rootChildren[0];
        var returnOperation = rootChildren[1];
        var declaration = declarationGroup.ChildOperations.Single();
        var declarator = declaration.ChildOperations.Single();
        var initializer = declarator.ChildOperations.Single();

        Assert.AreEqual("r", index.GetIdentity(block), scenarioId);
        Assert.AreEqual("r/0", index.GetIdentity(declarationGroup), scenarioId);
        Assert.AreEqual("r/1", index.GetIdentity(returnOperation), scenarioId);
        Assert.AreEqual("r/0/0", index.GetIdentity(declaration), scenarioId);
        Assert.AreEqual("r/0/0/0", index.GetIdentity(declarator), scenarioId);
        Assert.AreEqual("r/0/0/0/0", index.GetIdentity(initializer), scenarioId);
    }

    private static void AssertDetachedOperationCaching(string scenarioId)
    {
        var root = GetBlockOperation(SimpleMethodSource);
        var firstDetached = GetBlockOperation(TwoStatementMethodSource);
        var secondDetached = GetBlockOperation("class TestClass { int TestMethod() { return 2; } }");
        var index = new OperationIdentityIndex(root);

        Assert.AreEqual("d:0", index.GetIdentity(firstDetached), scenarioId);
        Assert.AreEqual("d:0", index.GetIdentity(firstDetached), scenarioId);
        Assert.AreEqual("d:1", index.GetIdentity(firstDetached.ChildOperations.First()), scenarioId);
        Assert.AreEqual("d:2", index.GetIdentity(secondDetached), scenarioId);
    }

    private static void AssertOwnerCheckoutPathStability(string scenarioId)
    {
        var first = GetBlockOperation(SimpleMethodSource, @"C:\agent-a\work\Source.cs");
        var second = GetBlockOperation(SimpleMethodSource, @"E:\agent-b\work\Renamed.cs");

        Assert.AreEqual(
            new UniqueNameSession(first, ScopeSite.RootFragment()).OwnerKey,
            new UniqueNameSession(second, ScopeSite.RootFragment()).OwnerKey,
            scenarioId);
    }

    private static void AssertOwnerSymbolSeparation(string scenarioId)
    {
        var first = GetBlockOperation("class FirstClass { int TestMethod() { return 1; } }");
        var second = GetBlockOperation("class SecondClass { int TestMethod() { return 1; } }");

        Assert.AreNotEqual(
            new UniqueNameSession(first, ScopeSite.RootFragment()).OwnerKey,
            new UniqueNameSession(second, ScopeSite.RootFragment()).OwnerKey,
            scenarioId);
    }

    private static void AssertOwnerBodyEditStability(string scenarioId)
    {
        var first = GetBlockOperation("class TestClass { int TestMethod() { return 1; } }");
        var second = GetBlockOperation("class TestClass { int TestMethod() { int value = 2; return value; } }");

        Assert.AreEqual(
            new UniqueNameSession(first, ScopeSite.RootFragment()).OwnerKey,
            new UniqueNameSession(second, ScopeSite.RootFragment()).OwnerKey,
            scenarioId);
    }

    private static LoweringSite CreateLoweringSite(UniqueNameLoweringSiteScenarioKind kind)
        => kind switch
        {
            UniqueNameLoweringSiteScenarioKind.CreationTemp => LoweringSite.CreationTemp(),
            UniqueNameLoweringSiteScenarioKind.ConditionalAccessInput => LoweringSite.ConditionalAccessInput(),
            UniqueNameLoweringSiteScenarioKind.LockValueTemp => LoweringSite.LockValueTemp("monitor"),
            UniqueNameLoweringSiteScenarioKind.UsingResourceTemp => LoweringSite.UsingResourceTemp("resource"),
            UniqueNameLoweringSiteScenarioKind.MethodReferenceReceiver => LoweringSite.MethodReferenceReceiver(),
            UniqueNameLoweringSiteScenarioKind.PropertyMutationTemp => LoweringSite.PropertyMutationTemp("receiver"),
            UniqueNameLoweringSiteScenarioKind.MethodReferenceProxy => LoweringSite.MethodReferenceProxy(),
            UniqueNameLoweringSiteScenarioKind.ReferenceTemp => LoweringSite.ReferenceTemp(),
            UniqueNameLoweringSiteScenarioKind.SwitchExpressionInput => LoweringSite.SwitchExpressionInput(),
            UniqueNameLoweringSiteScenarioKind.SwitchPatternInput => LoweringSite.SwitchPatternInput(),
            UniqueNameLoweringSiteScenarioKind.PatternInputCache => LoweringSite.PatternInputCache("property"),
            UniqueNameLoweringSiteScenarioKind.MultiCatchParameter => LoweringSite.MultiCatchParameter(),
            UniqueNameLoweringSiteScenarioKind.SyntheticCatchParameter => LoweringSite.SyntheticCatchParameter(),
            UniqueNameLoweringSiteScenarioKind.TupleProjectionSource => LoweringSite.TupleProjectionSource(),
            UniqueNameLoweringSiteScenarioKind.TupleDeconstructionSource => LoweringSite.TupleDeconstructionSource(),
            UniqueNameLoweringSiteScenarioKind.TupleFieldCache => LoweringSite.TupleFieldCache(2),
            UniqueNameLoweringSiteScenarioKind.TupleNestedArgument => LoweringSite.TupleNestedArgument(3),
            UniqueNameLoweringSiteScenarioKind.DeconstructResult => LoweringSite.DeconstructResult(),
            UniqueNameLoweringSiteScenarioKind.TupleBinaryOperandCache => LoweringSite.TupleBinaryOperandCache(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };

    private static ScopeSite CreateScopeSite(UniqueNameScopeSiteScenarioKind kind)
        => kind switch
        {
            UniqueNameScopeSiteScenarioKind.RootFragment => ScopeSite.RootFragment(),
            UniqueNameScopeSiteScenarioKind.FunctionBody => ScopeSite.FunctionBody(),
            UniqueNameScopeSiteScenarioKind.StaticBlock => ScopeSite.StaticBlock(),
            UniqueNameScopeSiteScenarioKind.NestedBlock => ScopeSite.NestedBlock(),
            UniqueNameScopeSiteScenarioKind.LocalFunctionBody => ScopeSite.LocalFunctionBody(),
            UniqueNameScopeSiteScenarioKind.LambdaBody => ScopeSite.LambdaBody(),
            UniqueNameScopeSiteScenarioKind.TryBody => ScopeSite.TryBody(),
            UniqueNameScopeSiteScenarioKind.CatchBody => ScopeSite.CatchBody(),
            UniqueNameScopeSiteScenarioKind.FinallyBody => ScopeSite.FinallyBody(),
            UniqueNameScopeSiteScenarioKind.SwitchCaseBody => ScopeSite.SwitchCaseBody(),
            UniqueNameScopeSiteScenarioKind.PatternIife => ScopeSite.PatternIife(),
            UniqueNameScopeSiteScenarioKind.SwitchExpressionIife => ScopeSite.SwitchExpressionIife(),
            UniqueNameScopeSiteScenarioKind.ObjectInitializerIife => ScopeSite.ObjectInitializerIife(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };

    private static IBlockOperation GetBlockOperation(string code, string? filePath = null)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            code,
            TestMetadataReferences.PreviewParseOptions,
            path: filePath ?? string.Empty);
        var compilation = CSharpCompilation.Create(
            assemblyName: "UniqueNameInfrastructureScenarios",
            syntaxTrees: [syntaxTree],
            references: TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "TestMethod");
        return (IBlockOperation?)compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!)
            ?? throw new InvalidOperationException("Expected a block operation for TestMethod.");
    }

    private const string SimpleMethodSource =
        "class TestClass { int TestMethod(int input) { return input + 1; } }";

    private const string TwoStatementMethodSource =
        "class TestClass { int TestMethod(int input) { int value = input + 1; return value; } }";
}

public enum UniqueNameLoweringSiteScenarioKind
{
    CreationTemp,
    ConditionalAccessInput,
    LockValueTemp,
    UsingResourceTemp,
    MethodReferenceReceiver,
    PropertyMutationTemp,
    MethodReferenceProxy,
    ReferenceTemp,
    SwitchExpressionInput,
    SwitchPatternInput,
    PatternInputCache,
    MultiCatchParameter,
    SyntheticCatchParameter,
    TupleProjectionSource,
    TupleDeconstructionSource,
    TupleFieldCache,
    TupleNestedArgument,
    DeconstructResult,
    TupleBinaryOperandCache
}

public sealed record UniqueNameLoweringSiteScenario(
    string Id,
    string Dimension,
    UniqueNameLoweringSiteScenarioKind Kind,
    string ExpectedTag,
    string ExpectedSlot);

public enum UniqueNameScopeSiteScenarioKind
{
    RootFragment,
    FunctionBody,
    StaticBlock,
    NestedBlock,
    LocalFunctionBody,
    LambdaBody,
    TryBody,
    CatchBody,
    FinallyBody,
    SwitchCaseBody,
    PatternIife,
    SwitchExpressionIife,
    ObjectInitializerIife
}

public sealed record UniqueNameScopeSiteScenario(
    string Id,
    string Dimension,
    UniqueNameScopeSiteScenarioKind Kind,
    string ExpectedKind);

public sealed record UniqueNameHashScenario(
    string Id,
    string Dimension,
    string Text,
    int RequestedLength,
    string FullHash);

public enum UniqueNameBehaviorScenarioKind
{
    RootScopeContract,
    RootScopeCrossSessionStability,
    ChildScopeContract,
    SiblingScopeStability,
    ScopeAllocationIsolation,
    OwnerIdentityExcludedFromVisibleName,
    NameSaltSeparation,
    OperationTreePaths,
    DetachedOperationCaching,
    OwnerCheckoutPathStability,
    OwnerSymbolSeparation,
    OwnerBodyEditStability
}

public sealed record UniqueNameBehaviorScenario(
    string Id,
    string Dimension,
    UniqueNameBehaviorScenarioKind Kind);

public enum UniqueNameValidationScenarioKind
{
    NullSessionRoot,
    NullIdentityIndexRoot,
    NullIdentityOperation,
    ZeroHashLength,
    NegativeHashLength
}

public sealed record UniqueNameValidationScenario(
    string Id,
    string Dimension,
    UniqueNameValidationScenarioKind Kind,
    string ExpectedParameterName);

internal static class UniqueNameInfrastructureScenarioCatalog
{
    private const string EmptyHash =
        "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
    private const string JazorHash =
        "38662020b07cf3168c24348ff59dc85e748a1febdeb9b2cf5510de1148cd534c";
    private const string ScopeHash =
        "7d7f68c8ee074701ea5b13897dd70a87ad0177089212e86ac029157f75b114fe";

    public static IReadOnlyList<UniqueNameLoweringSiteScenario> LoweringSites { get; } =
    [
        Lowering("creation-temp", "object-initializer-temporary", UniqueNameLoweringSiteScenarioKind.CreationTemp, "creation"),
        Lowering("conditional-access-input", "single-evaluation-conditional-input", UniqueNameLoweringSiteScenarioKind.ConditionalAccessInput, "cacc"),
        Lowering("lock-value-temp", "lock-monitor-temporary", UniqueNameLoweringSiteScenarioKind.LockValueTemp, "lock", "monitor"),
        Lowering("using-resource-temp", "using-resource-temporary", UniqueNameLoweringSiteScenarioKind.UsingResourceTemp, "using", "resource"),
        Lowering("method-reference-receiver", "bound-method-receiver", UniqueNameLoweringSiteScenarioKind.MethodReferenceReceiver, "mrecv"),
        Lowering("property-mutation-temp", "property-mutation-receiver", UniqueNameLoweringSiteScenarioKind.PropertyMutationTemp, "pmut", "receiver"),
        Lowering("method-reference-proxy", "method-group-proxy", UniqueNameLoweringSiteScenarioKind.MethodReferenceProxy, "mref"),
        Lowering("reference-temp", "ref-out-storage", UniqueNameLoweringSiteScenarioKind.ReferenceTemp, "ref"),
        Lowering("switch-expression-input", "switch-expression-single-evaluation", UniqueNameLoweringSiteScenarioKind.SwitchExpressionInput, "swexpr"),
        Lowering("switch-pattern-input", "switch-pattern-single-evaluation", UniqueNameLoweringSiteScenarioKind.SwitchPatternInput, "swpat"),
        Lowering("pattern-input-cache", "recursive-pattern-input-cache", UniqueNameLoweringSiteScenarioKind.PatternInputCache, "patin", "property"),
        Lowering("multi-catch-parameter", "multi-catch-binding", UniqueNameLoweringSiteScenarioKind.MultiCatchParameter, "mcatch"),
        Lowering("synthetic-catch-parameter", "catch-without-source-binding", UniqueNameLoweringSiteScenarioKind.SyntheticCatchParameter, "scatch"),
        Lowering("tuple-projection-source", "tuple-projection-single-evaluation", UniqueNameLoweringSiteScenarioKind.TupleProjectionSource, "tproj"),
        Lowering("tuple-deconstruction-source", "deconstruction-source-single-evaluation", UniqueNameLoweringSiteScenarioKind.TupleDeconstructionSource, "tdecon"),
        Lowering("tuple-field-cache", "tuple-field-index-slot-cache", UniqueNameLoweringSiteScenarioKind.TupleFieldCache, "tfield", "2"),
        Lowering("tuple-nested-argument", "nested-tuple-index-argument", UniqueNameLoweringSiteScenarioKind.TupleNestedArgument, "tnest", "3"),
        Lowering("deconstruct-result", "custom-deconstruct-result", UniqueNameLoweringSiteScenarioKind.DeconstructResult, "decon"),
        Lowering("tuple-binary-operand-cache", "tuple-comparison-operand-cache", UniqueNameLoweringSiteScenarioKind.TupleBinaryOperandCache, "tbin")
    ];

    public static IReadOnlyList<UniqueNameScopeSiteScenario> ScopeSites { get; } =
    [
        Scope("root-fragment", "module-or-root-fragment", UniqueNameScopeSiteScenarioKind.RootFragment),
        Scope("function-body", "method-function-body", UniqueNameScopeSiteScenarioKind.FunctionBody),
        Scope("static-block", "class-static-block", UniqueNameScopeSiteScenarioKind.StaticBlock),
        Scope("nested-block", "lexical-nested-block", UniqueNameScopeSiteScenarioKind.NestedBlock),
        Scope("local-function-body", "local-function-body", UniqueNameScopeSiteScenarioKind.LocalFunctionBody),
        Scope("lambda-body", "lambda-function-body", UniqueNameScopeSiteScenarioKind.LambdaBody),
        Scope("try-body", "try-statement-body", UniqueNameScopeSiteScenarioKind.TryBody),
        Scope("catch-body", "catch-clause-body", UniqueNameScopeSiteScenarioKind.CatchBody),
        Scope("finally-body", "finally-clause-body", UniqueNameScopeSiteScenarioKind.FinallyBody),
        Scope("switch-case-body", "switch-section-body", UniqueNameScopeSiteScenarioKind.SwitchCaseBody),
        Scope("pattern-iife", "pattern-lowering-iife", UniqueNameScopeSiteScenarioKind.PatternIife),
        Scope("switch-expression-iife", "switch-expression-iife", UniqueNameScopeSiteScenarioKind.SwitchExpressionIife),
        Scope("object-initializer-iife", "object-initializer-iife", UniqueNameScopeSiteScenarioKind.ObjectInitializerIife)
    ];

    public static IReadOnlyList<UniqueNameHashScenario> HashCases { get; } =
    [
        Hash("empty-minimum-prefix", "empty-utf8-one-hex-digit", string.Empty, 1, EmptyHash),
        Hash("jazor-short-prefix", "ascii-eight-hex-digits", "jazor", 8, JazorHash),
        Hash("scope-standard-prefix", "stable-key-standard-24-digits", "scope-jz3-owner", 24, ScopeHash),
        Hash("jazor-complete-digest", "complete-sha256-digest", "jazor", 64, JazorHash),
        Hash("jazor-overlong-request", "request-beyond-sha256-capacity", "jazor", 80, JazorHash)
    ];

    public static IReadOnlyList<UniqueNameBehaviorScenario> BehaviorCases { get; } =
    [
        Behavior("root-scope-contract", "root-session-anchor-and-identity", UniqueNameBehaviorScenarioKind.RootScopeContract),
        Behavior("root-scope-cross-session-stability", "checkout-independent-root-key", UniqueNameBehaviorScenarioKind.RootScopeCrossSessionStability),
        Behavior("child-scope-contract", "child-session-anchor-and-key", UniqueNameBehaviorScenarioKind.ChildScopeContract),
        Behavior("sibling-scope-stability", "equivalent-sibling-scope-key", UniqueNameBehaviorScenarioKind.SiblingScopeStability),
        Behavior("scope-allocation-isolation", "root-child-allocation-cache-isolation", UniqueNameBehaviorScenarioKind.ScopeAllocationIsolation),
        Behavior("owner-identity-erased", "visible-name-uses-stable-owner-key", UniqueNameBehaviorScenarioKind.OwnerIdentityExcludedFromVisibleName),
        Behavior("name-salt-separation", "primary-fallback-name-separation", UniqueNameBehaviorScenarioKind.NameSaltSeparation),
        Behavior("operation-tree-paths", "root-and-child-structural-identities", UniqueNameBehaviorScenarioKind.OperationTreePaths),
        Behavior("detached-operation-caching", "detached-identity-order-and-reuse", UniqueNameBehaviorScenarioKind.DetachedOperationCaching),
        Behavior("owner-checkout-path-stability", "symbol-owner-path-independence", UniqueNameBehaviorScenarioKind.OwnerCheckoutPathStability),
        Behavior("owner-symbol-separation", "different-containing-symbols", UniqueNameBehaviorScenarioKind.OwnerSymbolSeparation),
        Behavior("owner-body-edit-stability", "same-symbol-body-edit-independence", UniqueNameBehaviorScenarioKind.OwnerBodyEditStability)
    ];

    public static IReadOnlyList<UniqueNameValidationScenario> ValidationCases { get; } =
    [
        Validation("validation.null-session-root", "session-root-required", UniqueNameValidationScenarioKind.NullSessionRoot, "root"),
        Validation("validation.null-index-root", "identity-index-root-required", UniqueNameValidationScenarioKind.NullIdentityIndexRoot, "root"),
        Validation("validation.null-identity-operation", "identity-operation-required", UniqueNameValidationScenarioKind.NullIdentityOperation, "operation"),
        Validation("validation.zero-hash-length", "positive-hash-length-zero-boundary", UniqueNameValidationScenarioKind.ZeroHashLength, "hexLength"),
        Validation("validation.negative-hash-length", "positive-hash-length-negative-boundary", UniqueNameValidationScenarioKind.NegativeHashLength, "hexLength")
    ];

    private static UniqueNameLoweringSiteScenario Lowering(
        string id,
        string dimension,
        UniqueNameLoweringSiteScenarioKind kind,
        string expectedTag,
        string expectedSlot = "")
        => new($"unique-name.lowering.{id}", dimension, kind, expectedTag, expectedSlot);

    private static UniqueNameScopeSiteScenario Scope(
        string id,
        string dimension,
        UniqueNameScopeSiteScenarioKind kind)
        => new($"unique-name.scope.{id}", dimension, kind, kind.ToString());

    private static UniqueNameHashScenario Hash(
        string id,
        string dimension,
        string text,
        int requestedLength,
        string fullHash)
        => new($"unique-name.hash.{id}", dimension, text, requestedLength, fullHash);

    private static UniqueNameBehaviorScenario Behavior(
        string id,
        string dimension,
        UniqueNameBehaviorScenarioKind kind)
        => new($"unique-name.behavior.{id}", dimension, kind);

    private static UniqueNameValidationScenario Validation(
        string id,
        string dimension,
        UniqueNameValidationScenarioKind kind,
        string expectedParameterName)
        => new($"unique-name.{id}", dimension, kind, expectedParameterName);
}
