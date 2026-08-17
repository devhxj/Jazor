using Acornima;
using Acornima.Ast;
using Jazor.Common;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class SenseArgumentScenarioTests
{
    public static IEnumerable<TestDataRow<SenseArgumentContextScenario>> ContextCases
        => SenseArgumentScenarioCatalog.Contexts.Select(static testCase =>
            new TestDataRow<SenseArgumentContextScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    public static IEnumerable<TestDataRow<SenseArgumentValidationScenario>> ValidationCases
        => SenseArgumentScenarioCatalog.Validations.Select(static testCase =>
            new TestDataRow<SenseArgumentValidationScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    public static IEnumerable<TestDataRow<SenseArgumentDeclaratorScenario>> DeclaratorCases
        => SenseArgumentScenarioCatalog.Declarators.Select(static testCase =>
            new TestDataRow<SenseArgumentDeclaratorScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    public static IEnumerable<TestDataRow<SenseArgumentImportCollectionScenario>> ImportCollectionCases
        => SenseArgumentScenarioCatalog.ImportCollections.Select(static testCase =>
            new TestDataRow<SenseArgumentImportCollectionScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    public static IEnumerable<TestDataRow<SenseArgumentImportBindingScenario>> ImportBindingCases
        => SenseArgumentScenarioCatalog.ImportBindings.Select(static testCase =>
            new TestDataRow<SenseArgumentImportBindingScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalogs_HaveUniqueIdsDimensionsAndKinds()
    {
        var allIds = SenseArgumentScenarioCatalog.Contexts.Select(static testCase => testCase.Id)
            .Concat(SenseArgumentScenarioCatalog.Validations.Select(static testCase => testCase.Id))
            .Concat(SenseArgumentScenarioCatalog.Declarators.Select(static testCase => testCase.Id))
            .Concat(SenseArgumentScenarioCatalog.ImportCollections.Select(static testCase => testCase.Id))
            .Concat(SenseArgumentScenarioCatalog.ImportBindings.Select(static testCase => testCase.Id))
            .ToArray();
        var allDimensions = SenseArgumentScenarioCatalog.Contexts.Select(static testCase => testCase.Dimension)
            .Concat(SenseArgumentScenarioCatalog.Validations.Select(static testCase => testCase.Dimension))
            .Concat(SenseArgumentScenarioCatalog.Declarators.Select(static testCase => testCase.Dimension))
            .Concat(SenseArgumentScenarioCatalog.ImportCollections.Select(static testCase => testCase.Dimension))
            .Concat(SenseArgumentScenarioCatalog.ImportBindings.Select(static testCase => testCase.Dimension))
            .ToArray();

        Assert.IsNotEmpty(allIds);
        Assert.HasCount(allIds.Length, allIds.Distinct(StringComparer.Ordinal));
        Assert.IsTrue(allIds.All(static id => id.StartsWith("sense-argument.", StringComparison.Ordinal)));
        Assert.IsTrue(allDimensions.All(static dimension => !string.IsNullOrWhiteSpace(dimension)));
        AssertCatalogCoversEveryKind(
            SenseArgumentScenarioCatalog.Contexts,
            static testCase => testCase.Kind,
            Enum.GetValues<SenseArgumentContextScenarioKind>());
        AssertCatalogCoversEveryKind(
            SenseArgumentScenarioCatalog.Validations,
            static testCase => testCase.Kind,
            Enum.GetValues<SenseArgumentValidationScenarioKind>());
        AssertCatalogCoversEveryKind(
            SenseArgumentScenarioCatalog.Declarators,
            static testCase => testCase.Kind,
            Enum.GetValues<SenseArgumentDeclaratorScenarioKind>());
        AssertCatalogCoversEveryKind(
            SenseArgumentScenarioCatalog.ImportCollections,
            static testCase => testCase.Kind,
            Enum.GetValues<SenseArgumentImportCollectionScenarioKind>());
        AssertCatalogCoversEveryKind(
            SenseArgumentScenarioCatalog.ImportBindings,
            static testCase => testCase.Kind,
            Enum.GetValues<SenseArgumentImportBindingScenarioKind>());
    }

    [TestMethod]
    [DynamicData(nameof(ContextCases))]
    public void Context_MatchesCopyAndScopeContract(SenseArgumentContextScenario testCase)
    {
        switch (testCase.Kind)
        {
            case SenseArgumentContextScenarioKind.ParameterlessConstructor:
                AssertInitializedDefault(new SenseArgument(), testCase.Id);
                break;
            case SenseArgumentContextScenarioKind.DefaultFactory:
                AssertInitializedDefault(SenseArgument.Default, testCase.Id);
                break;
            case SenseArgumentContextScenarioKind.FullConstructor:
                AssertFullConstructor(testCase.Id);
                break;
            case SenseArgumentContextScenarioKind.WithSense:
                AssertWithSense(testCase.Id);
                break;
            case SenseArgumentContextScenarioKind.WithPatternInput:
                AssertWithPatternInput(testCase.Id);
                break;
            case SenseArgumentContextScenarioKind.WithCatchVariable:
                AssertWithCatchVariable(testCase.Id);
                break;
            case SenseArgumentContextScenarioKind.WithSwitchVariable:
                AssertWithSwitchVariable(testCase.Id);
                break;
            case SenseArgumentContextScenarioKind.WithSenseAndPattern:
                AssertWithSenseAndPattern(testCase.Id);
                break;
            case SenseArgumentContextScenarioKind.WithImportAliases:
                AssertWithImportAliases(testCase.Id);
                break;
            case SenseArgumentContextScenarioKind.WithNewScope:
                AssertWithNewScope(testCase.Id);
                break;
            case SenseArgumentContextScenarioKind.WithScopeContext:
                AssertWithScopeContext(testCase.Id);
                break;
            case SenseArgumentContextScenarioKind.EnterChildScope:
                AssertEnterChildScope(testCase.Id);
                break;
            case SenseArgumentContextScenarioKind.EnterEmissionScope:
                AssertEnterEmissionScope(testCase.Id);
                break;
            case SenseArgumentContextScenarioKind.AllocateStableName:
                AssertAllocateStableName(testCase.Id);
                break;
            default:
                Assert.Fail($"{testCase.Id}: unsupported context kind '{testCase.Kind}'.");
                break;
        }
    }

    [TestMethod]
    [DynamicData(nameof(ValidationCases))]
    public void Context_RejectsInvalidScopeAndDeclaratorInputs(SenseArgumentValidationScenario testCase)
    {
        var block = GetBlockOperation();

        Exception exception = testCase.Kind switch
        {
            SenseArgumentValidationScenarioKind.NullChildScopeAnchor =>
                Assert.ThrowsExactly<InvalidOperationException>(() =>
                    new SenseArgument().EnterScope(null!, ScopeSite.NestedBlock())),
            SenseArgumentValidationScenarioKind.MissingParentScope =>
                Assert.ThrowsExactly<InvalidOperationException>(() =>
                    new SenseArgument().EnterScope(block, ScopeSite.NestedBlock())),
            SenseArgumentValidationScenarioKind.NullEmissionScopeAnchor =>
                Assert.ThrowsExactly<InvalidOperationException>(() =>
                    new SenseArgument().EnterEmissionScope(null!, ScopeSite.PatternIife())),
            SenseArgumentValidationScenarioKind.MissingEmissionParentScope =>
                Assert.ThrowsExactly<InvalidOperationException>(() =>
                    new SenseArgument().EnterEmissionScope(block, ScopeSite.PatternIife())),
            SenseArgumentValidationScenarioKind.MissingAllocationScope =>
                Assert.ThrowsExactly<InvalidOperationException>(() =>
                    new SenseArgument().AllocateName(
                        new LoweringNameOwner("stable", "identity"),
                        LoweringSite.CreationTemp())),
            SenseArgumentValidationScenarioKind.NonIdentifierDeclarator =>
                Assert.ThrowsExactly<NotSupportedException>(() =>
                    new SenseArgument().AddVarDeclarator(
                        new VariableDeclarator(
                            new ObjectPattern(NodeList.From<Node>()),
                            init: null),
                        depth: 0)),
            _ => throw new InvalidOperationException(
                $"{testCase.Id}: unsupported validation kind '{testCase.Kind}'.")
        };

        Assert.AreEqual(testCase.ExpectedMessage, exception.Message, testCase.Id);
    }

    [TestMethod]
    [DynamicData(nameof(DeclaratorCases))]
    public void DeclaratorCollection_MatchesDepthDedupeAndFlushContract(
        SenseArgumentDeclaratorScenario testCase)
    {
        switch (testCase.Kind)
        {
            case SenseArgumentDeclaratorScenarioKind.UninitializedNoOp:
                AssertUninitializedDeclaratorNoOp(testCase.Id);
                break;
            case SenseArgumentDeclaratorScenarioKind.SingleDeclarator:
                AssertSingleDeclarator(testCase.Id);
                break;
            case SenseArgumentDeclaratorScenarioKind.DuplicateDepthAndName:
                AssertDuplicateDepthAndName(testCase.Id);
                break;
            case SenseArgumentDeclaratorScenarioKind.SameNameDifferentDepth:
                AssertSameNameDifferentDepth(testCase.Id);
                break;
            case SenseArgumentDeclaratorScenarioKind.DifferentNamesSameDepth:
                AssertDifferentNamesSameDepth(testCase.Id);
                break;
            case SenseArgumentDeclaratorScenarioKind.FlushResetsCollection:
                AssertDeclaratorFlushResetsCollection(testCase.Id);
                break;
            default:
                Assert.Fail($"{testCase.Id}: unsupported declarator kind '{testCase.Kind}'.");
                break;
        }
    }

    [TestMethod]
    [DynamicData(nameof(ImportCollectionCases))]
    public void ImportCollection_MatchesGroupingNormalizationAndFlushContract(
        SenseArgumentImportCollectionScenario testCase)
    {
        switch (testCase.Kind)
        {
            case SenseArgumentImportCollectionScenarioKind.UninitializedNoOp:
                AssertUninitializedImportNoOp(testCase.Id);
                break;
            case SenseArgumentImportCollectionScenarioKind.SeparateModuleGroups:
                AssertSeparateModuleGroups(testCase.Id);
                break;
            case SenseArgumentImportCollectionScenarioKind.NormalizeWithinModule:
                AssertNormalizeWithinModule(testCase.Id);
                break;
            case SenseArgumentImportCollectionScenarioKind.MixedSpecifierKinds:
                AssertMixedSpecifierKinds(testCase.Id);
                break;
            case SenseArgumentImportCollectionScenarioKind.FlushResetsCollection:
                AssertImportFlushResetsCollection(testCase.Id);
                break;
            default:
                Assert.Fail($"{testCase.Id}: unsupported import collection kind '{testCase.Kind}'.");
                break;
        }
    }

    [TestMethod]
    [DynamicData(nameof(ImportBindingCases))]
    public void BindImportSpecifier_MatchesModuleAndAliasContract(
        SenseArgumentImportBindingScenario testCase)
    {
        switch (testCase.Kind)
        {
            case SenseArgumentImportBindingScenarioKind.NullImportedName:
                AssertNullImportedName(testCase.Id);
                break;
            case SenseArgumentImportBindingScenarioKind.WhitespaceImportedName:
                AssertWhitespaceImportedName(testCase.Id);
                break;
            case SenseArgumentImportBindingScenarioKind.NullModulePath:
                AssertNullModulePath(testCase.Id);
                break;
            case SenseArgumentImportBindingScenarioKind.WhitespaceModulePath:
                AssertWhitespaceModulePath(testCase.Id);
                break;
            case SenseArgumentImportBindingScenarioKind.UninitializedStorage:
                AssertUninitializedBindingStorage(testCase.Id);
                break;
            case SenseArgumentImportBindingScenarioKind.AliasesDisabled:
                AssertAliasesDisabled(testCase.Id);
                break;
            case SenseArgumentImportBindingScenarioKind.RawNameAvailable:
                AssertRawNameAvailable(testCase.Id);
                break;
            case SenseArgumentImportBindingScenarioKind.ReservedNameCollision:
                AssertReservedNameCollision(testCase.Id);
                break;
            case SenseArgumentImportBindingScenarioKind.DefaultImportAlias:
                AssertDefaultImportAlias(testCase.Id);
                break;
            case SenseArgumentImportBindingScenarioKind.CurrentModuleBinding:
                AssertCurrentModuleBinding(testCase.Id);
                break;
            case SenseArgumentImportBindingScenarioKind.CurrentModuleMissingBinding:
                AssertCurrentModuleMissingBinding(testCase.Id);
                break;
            case SenseArgumentImportBindingScenarioKind.CachedBinding:
                AssertCachedBinding(testCase.Id);
                break;
            case SenseArgumentImportBindingScenarioKind.CrossModuleNameCollision:
                AssertCrossModuleNameCollision(testCase.Id);
                break;
            case SenseArgumentImportBindingScenarioKind.SameModuleMixedAliases:
                AssertSameModuleMixedAliases(testCase.Id);
                break;
            default:
                Assert.Fail($"{testCase.Id}: unsupported import binding kind '{testCase.Kind}'.");
                break;
        }
    }

    [TestMethod]
    public void BindImportSpecifier_GeneratedAliasSkipsReservedAndOccupiedHashCandidates()
    {
        const string modulePath = "vue";
        const string importedName = "watch";
        var key = ImportBindingKey(modulePath, importedName);
        var prefix = $"i${Format.HashName(key).TrimStart('_')}";

        var reservedState = CreateImportBindingState(reservedNames: [importedName, prefix]);
        var reservedAlias = reservedState.Argument.BindImportSpecifier(modulePath, importedName);
        Assert.AreEqual(prefix + "1", reservedAlias.Name);

        var occupiedState = CreateImportBindingState(reservedNames: [importedName]);
        occupiedState.LocalBindings.Add(prefix, "existing-module\0existing-import");
        var occupiedAlias = occupiedState.Argument.BindImportSpecifier(modulePath, importedName);
        Assert.AreEqual(prefix + "1", occupiedAlias.Name);
    }

    private static void AssertInitializedDefault(SenseArgument argument, string scenarioId)
    {
        AssertContext(
            argument,
            Sense.Any,
            useImportAliases: false,
            patternInput: null,
            catchVariable: null,
            switchVariable: null,
            scenarioId);
        Assert.IsFalse(argument.HasVarDeclarator, scenarioId);
        Assert.IsFalse(argument.HasVarImportDeclarationSpecifier, scenarioId);

        argument.AddVarDeclarator(Declarator("value", 1), depth: 0);
        argument.MergeImportSpecifier("vue", NamedImport("computed"));

        Assert.IsTrue(argument.HasVarDeclarator, scenarioId);
        Assert.IsTrue(argument.HasVarImportDeclarationSpecifier, scenarioId);
    }

    private static void AssertFullConstructor(string scenarioId)
    {
        var pattern = new Identifier("candidate");
        var argument = new SenseArgument(
            Sense.PatternCase,
            UseImportAliases: true,
            PatternInput: pattern,
            CatchExceptionVar: "failure",
            SwitchExpressionVar: "selection");

        AssertContext(
            argument,
            Sense.PatternCase,
            useImportAliases: true,
            pattern,
            "failure",
            "selection",
            scenarioId);
    }

    private static void AssertWithSense(string scenarioId)
    {
        var source = CreatePopulatedContext();
        var result = source.With(Sense.LeftValue);

        AssertContext(
            result,
            Sense.LeftValue,
            useImportAliases: true,
            source.PatternInput,
            "failure",
            "selection",
            scenarioId);
        AssertCollectorsShared(source, result, scenarioId);
    }

    private static void AssertWithPatternInput(string scenarioId)
    {
        var source = CreatePopulatedContext();
        var replacement = new Identifier("replacement");
        var result = source.WithPatternInput(replacement);

        AssertContext(
            result,
            Sense.RightValue,
            useImportAliases: true,
            replacement,
            "failure",
            "selection",
            scenarioId);
        AssertCollectorsShared(source, result, scenarioId);
    }

    private static void AssertWithCatchVariable(string scenarioId)
    {
        var source = CreatePopulatedContext();
        var result = source.WithCatchVar("caught");

        AssertContext(
            result,
            Sense.RightValue,
            useImportAliases: true,
            source.PatternInput,
            "caught",
            "selection",
            scenarioId);
        AssertCollectorsShared(source, result, scenarioId);
    }

    private static void AssertWithSwitchVariable(string scenarioId)
    {
        var source = CreatePopulatedContext();
        var result = source.WithSwitchVar("switched");

        AssertContext(
            result,
            Sense.RightValue,
            useImportAliases: true,
            source.PatternInput,
            "failure",
            "switched",
            scenarioId);
        AssertCollectorsShared(source, result, scenarioId);
    }

    private static void AssertWithSenseAndPattern(string scenarioId)
    {
        var source = CreatePopulatedContext();
        var replacement = new Identifier("replacement");
        var result = source.With(Sense.PatternInput, replacement);

        AssertContext(
            result,
            Sense.PatternInput,
            useImportAliases: true,
            replacement,
            "failure",
            "selection",
            scenarioId);
        AssertCollectorsShared(source, result, scenarioId);
    }

    private static void AssertWithImportAliases(string scenarioId)
    {
        var source = CreatePopulatedContext().WithImportAliases(useImportAliases: false);
        var result = source.WithImportAliases();

        AssertContext(
            result,
            Sense.RightValue,
            useImportAliases: true,
            source.PatternInput,
            "failure",
            "selection",
            scenarioId);
        AssertCollectorsShared(source, result, scenarioId);
    }

    private static void AssertWithNewScope(string scenarioId)
    {
        var source = CreatePopulatedContext();
        var sourceDeclarator = Declarator("parent", 1);
        source.AddVarDeclarator(sourceDeclarator, depth: 0);
        var result = source.WithNewScope();

        Assert.IsTrue(source.HasVarDeclarator, scenarioId);
        Assert.IsFalse(result.HasVarDeclarator, scenarioId);
        var childDeclarator = Declarator("child", 2);
        result.AddVarDeclarator(childDeclarator, depth: 0);
        result.MergeImportSpecifier("vue", NamedImport("computed"));

        CollectionAssert.AreEqual(
            new[] { sourceDeclarator },
            source.FlushVarDeclarator().ToArray(),
            scenarioId);
        CollectionAssert.AreEqual(
            new[] { childDeclarator },
            result.FlushVarDeclarator().ToArray(),
            scenarioId);
        Assert.IsTrue(source.HasVarImportDeclarationSpecifier, scenarioId);
        AssertContext(
            result,
            Sense.RightValue,
            useImportAliases: true,
            source.PatternInput,
            "failure",
            "selection",
            scenarioId);
    }

    private static void AssertWithScopeContext(string scenarioId)
    {
        var block = GetBlockOperation();
        var session = new UniqueNameSession(block, ScopeSite.RootFragment());
        var source = CreatePopulatedContext();
        var result = source.WithScope(session.RootScope);

        Assert.AreSame(session.RootScope, result.ScopeContext, scenarioId);
        AssertCollectorsShared(source, result, scenarioId);
    }

    private static void AssertEnterChildScope(string scenarioId)
    {
        var block = GetBlockOperation();
        var session = new UniqueNameSession(block, ScopeSite.RootFragment());
        var source = CreatePopulatedContext().WithScope(session.RootScope);
        source.AddVarDeclarator(Declarator("parent", 1), depth: 0);
        var anchor = block.ChildOperations.Single();

        var result = source.EnterScope(anchor, ScopeSite.NestedBlock());

        Assert.AreSame(anchor, result.ScopeContext?.Anchor, scenarioId);
        Assert.AreEqual(
            session.CreateScopeKey(session.RootScope.ScopeKey, ScopeSite.NestedBlock()),
            result.ScopeContext?.ScopeKey,
            scenarioId);
        Assert.IsTrue(source.HasVarDeclarator, scenarioId);
        Assert.IsFalse(result.HasVarDeclarator, scenarioId);
        result.MergeImportSpecifier("vue", NamedImport("computed"));
        Assert.IsTrue(source.HasVarImportDeclarationSpecifier, scenarioId);
    }

    private static void AssertEnterEmissionScope(string scenarioId)
    {
        var block = GetBlockOperation();
        var session = new UniqueNameSession(block, ScopeSite.RootFragment());
        var source = CreatePopulatedContext().WithScope(session.RootScope);
        var anchor = block.ChildOperations.Single();

        var result = source.EnterEmissionScope(anchor, ScopeSite.PatternIife());

        Assert.AreSame(anchor, result.ScopeContext?.Anchor, scenarioId);
        Assert.AreEqual(
            session.CreateScopeKey(session.RootScope.ScopeKey, ScopeSite.PatternIife()),
            result.ScopeContext?.ScopeKey,
            scenarioId);
        result.AddVarDeclarator(Declarator("emitted", 1), depth: 0);
        Assert.IsFalse(source.HasVarDeclarator, scenarioId);
        Assert.IsTrue(result.HasVarDeclarator, scenarioId);
    }

    private static void AssertAllocateStableName(string scenarioId)
    {
        var block = GetBlockOperation();
        var session = new UniqueNameSession(block, ScopeSite.RootFragment());
        var argument = new SenseArgument().WithScope(session.RootScope);
        var owner = new LoweringNameOwner("source-owner", "source-owner@0");
        var site = LoweringSite.PatternInputCache("property");

        var first = argument.AllocateName(owner, site);
        var second = argument.AllocateName(owner, site);

        Assert.AreEqual(first, second, scenarioId);
        Assert.IsTrue(first.StartsWith("__patin$", StringComparison.Ordinal), scenarioId);
    }

    private static void AssertUninitializedDeclaratorNoOp(string scenarioId)
    {
        var argument = default(SenseArgument);

        argument.AddVarDeclarator(Declarator("ignored", 1), depth: 0);

        Assert.IsFalse(argument.HasVarDeclarator, scenarioId);
        Assert.HasCount(0, argument.FlushVarDeclarator(), scenarioId);
    }

    private static void AssertSingleDeclarator(string scenarioId)
    {
        var argument = new SenseArgument();
        var declarator = Declarator("value", 1);

        argument.AddVarDeclarator(declarator, depth: 2);
        var flushed = argument.FlushVarDeclarator();

        Assert.HasCount(1, flushed, scenarioId);
        Assert.AreSame(declarator, flushed[0], scenarioId);
    }

    private static void AssertDuplicateDepthAndName(string scenarioId)
    {
        var argument = new SenseArgument();
        var first = Declarator("value", 1);
        var duplicate = Declarator("value", 2);

        argument.AddVarDeclarator(first, depth: 3);
        argument.AddVarDeclarator(duplicate, depth: 3);
        var flushed = argument.FlushVarDeclarator();

        Assert.HasCount(1, flushed, scenarioId);
        Assert.AreSame(first, flushed[0], scenarioId);
    }

    private static void AssertSameNameDifferentDepth(string scenarioId)
    {
        var argument = new SenseArgument();
        var outer = Declarator("value", 1);
        var inner = Declarator("value", 2);

        argument.AddVarDeclarator(outer, depth: 1);
        argument.AddVarDeclarator(inner, depth: 2);

        CollectionAssert.AreEqual(
            new[] { outer, inner },
            argument.FlushVarDeclarator().ToArray(),
            scenarioId);
    }

    private static void AssertDifferentNamesSameDepth(string scenarioId)
    {
        var argument = new SenseArgument();
        var first = Declarator("first", 1);
        var second = Declarator("second", 2);

        argument.AddVarDeclarator(first, depth: 1);
        argument.AddVarDeclarator(second, depth: 1);

        CollectionAssert.AreEqual(
            new[] { first, second },
            argument.FlushVarDeclarator().ToArray(),
            scenarioId);
    }

    private static void AssertDeclaratorFlushResetsCollection(string scenarioId)
    {
        var argument = new SenseArgument();
        argument.AddVarDeclarator(Declarator("value", 1), depth: 0);

        Assert.HasCount(1, argument.FlushVarDeclarator(), scenarioId);
        Assert.IsFalse(argument.HasVarDeclarator, scenarioId);
        Assert.HasCount(0, argument.FlushVarDeclarator(), scenarioId);
    }

    private static void AssertUninitializedImportNoOp(string scenarioId)
    {
        var argument = default(SenseArgument);

        argument.MergeImportSpecifier("vue", NamedImport("computed"));

        Assert.IsFalse(argument.HasVarImportDeclarationSpecifier, scenarioId);
        Assert.HasCount(0, argument.FlushImportSpecifiers(), scenarioId);
    }

    private static void AssertSeparateModuleGroups(string scenarioId)
    {
        var argument = new SenseArgument();
        argument.MergeImportSpecifier("vue", NamedImport("computed"));
        argument.MergeImportSpecifier("router", NamedImport("createRouter"));

        var groups = argument.FlushImportSpecifiers();

        AssertImportGroups(
            groups,
            [
                new ImportGroupShape("vue", ["named:computed->computed"]),
                new ImportGroupShape("router", ["named:createRouter->createRouter"])
            ],
            scenarioId);
    }

    private static void AssertNormalizeWithinModule(string scenarioId)
    {
        var argument = new SenseArgument();
        argument.MergeImportSpecifier("vue", NamedImport("watch"));
        argument.MergeImportSpecifier("vue", NamedImport("computed"));
        argument.MergeImportSpecifier("vue", NamedImport("watch"));

        AssertImportGroups(
            argument.FlushImportSpecifiers(),
            [new ImportGroupShape("vue", ["named:computed->computed", "named:watch->watch"])],
            scenarioId);
    }

    private static void AssertMixedSpecifierKinds(string scenarioId)
    {
        var argument = new SenseArgument();
        argument.MergeImportSpecifier("package", NamedImport("helper"));
        argument.MergeImportSpecifier("package", new ImportDefaultSpecifier(new Identifier("Package")));

        AssertImportGroups(
            argument.FlushImportSpecifiers(),
            [new ImportGroupShape("package", ["default:Package", "named:helper->helper"])],
            scenarioId);
    }

    private static void AssertImportFlushResetsCollection(string scenarioId)
    {
        var argument = new SenseArgument();
        argument.MergeImportSpecifier("vue", NamedImport("computed"));

        Assert.HasCount(1, argument.FlushImportSpecifiers(), scenarioId);
        Assert.IsFalse(argument.HasVarImportDeclarationSpecifier, scenarioId);
        Assert.HasCount(0, argument.FlushImportSpecifiers(), scenarioId);
    }

    private static void AssertNullImportedName(string scenarioId)
    {
        var argument = new SenseArgument(UseImportAliases: true);

        var identifier = argument.BindImportSpecifier("vue", null!);

        Assert.AreEqual(string.Empty, identifier.Name, scenarioId);
        Assert.HasCount(0, argument.FlushImportSpecifiers(), scenarioId);
    }

    private static void AssertWhitespaceImportedName(string scenarioId)
    {
        var argument = new SenseArgument(UseImportAliases: true);

        var identifier = argument.BindImportSpecifier("vue", "  ");

        Assert.AreEqual("  ", identifier.Name, scenarioId);
        Assert.HasCount(0, argument.FlushImportSpecifiers(), scenarioId);
    }

    private static void AssertNullModulePath(string scenarioId)
    {
        var argument = new SenseArgument(UseImportAliases: true);

        var identifier = argument.BindImportSpecifier(null, "computed");

        Assert.AreEqual("computed", identifier.Name, scenarioId);
        Assert.HasCount(0, argument.FlushImportSpecifiers(), scenarioId);
    }

    private static void AssertWhitespaceModulePath(string scenarioId)
    {
        var argument = new SenseArgument(UseImportAliases: true);

        var identifier = argument.BindImportSpecifier("  ", "computed");

        Assert.AreEqual("computed", identifier.Name, scenarioId);
        Assert.HasCount(0, argument.FlushImportSpecifiers(), scenarioId);
    }

    private static void AssertUninitializedBindingStorage(string scenarioId)
    {
        var argument = default(SenseArgument).WithImportAliases();

        var identifier = argument.BindImportSpecifier("vue", "computed");

        Assert.AreEqual("computed", identifier.Name, scenarioId);
        Assert.HasCount(0, argument.FlushImportSpecifiers(), scenarioId);
    }

    private static void AssertAliasesDisabled(string scenarioId)
    {
        var argument = new SenseArgument();

        var first = argument.BindImportSpecifier("vue", "computed");
        var second = argument.BindImportSpecifier("vue", "computed");

        Assert.AreEqual("computed", first.Name, scenarioId);
        Assert.AreEqual(first.Name, second.Name, scenarioId);
        AssertImportGroups(
            argument.FlushImportSpecifiers(),
            [new ImportGroupShape("vue", ["named:computed->computed"])],
            scenarioId);
    }

    private static void AssertRawNameAvailable(string scenarioId)
    {
        var state = CreateImportBindingState();

        var identifier = state.Argument.BindImportSpecifier("vue", "computed");

        var key = ImportBindingKey("vue", "computed");
        Assert.AreEqual("computed", identifier.Name, scenarioId);
        Assert.AreEqual("computed", state.Bindings[key], scenarioId);
        Assert.AreEqual(key, state.LocalBindings["computed"], scenarioId);
        AssertImportGroups(
            state.Argument.FlushImportSpecifiers(),
            [new ImportGroupShape("vue", ["named:computed->computed"])],
            scenarioId);
    }

    private static void AssertReservedNameCollision(string scenarioId)
    {
        var state = CreateImportBindingState(reservedNames: ["watch"]);

        var identifier = state.Argument.BindImportSpecifier("vue", "watch");

        var expectedAlias = ExpectedAlias("vue", "watch");
        Assert.AreEqual(expectedAlias, identifier.Name, scenarioId);
        AssertImportGroups(
            state.Argument.FlushImportSpecifiers(),
            [new ImportGroupShape("vue", [$"named:watch->{expectedAlias}"])],
            scenarioId);
    }

    private static void AssertDefaultImportAlias(string scenarioId)
    {
        var state = CreateImportBindingState();

        var identifier = state.Argument.BindImportSpecifier("component", "default");

        var expectedAlias = ExpectedAlias("component", "default");
        Assert.AreEqual(expectedAlias, identifier.Name, scenarioId);
        AssertImportGroups(
            state.Argument.FlushImportSpecifiers(),
            [new ImportGroupShape("component", [$"default:{expectedAlias}"])],
            scenarioId);
    }

    private static void AssertCurrentModuleBinding(string scenarioId)
    {
        var state = CreateImportBindingState(
            currentModuleImportPath: "./current.mjs",
            currentModuleBindings: ["localHelper"]);

        var identifier = state.Argument.BindImportSpecifier("./current", "localHelper");

        Assert.AreEqual("localHelper", identifier.Name, scenarioId);
        Assert.HasCount(0, state.Bindings, scenarioId);
        Assert.HasCount(0, state.LocalBindings, scenarioId);
        Assert.HasCount(0, state.Argument.FlushImportSpecifiers(), scenarioId);
    }

    private static void AssertCurrentModuleMissingBinding(string scenarioId)
    {
        var state = CreateImportBindingState(
            currentModuleImportPath: "./current.mjs",
            currentModuleBindings: []);

        var identifier = state.Argument.BindImportSpecifier("./current", "externalHelper");

        Assert.AreEqual("externalHelper", identifier.Name, scenarioId);
        AssertImportGroups(
            state.Argument.FlushImportSpecifiers(),
            [new ImportGroupShape("./current", ["named:externalHelper->externalHelper"])],
            scenarioId);
    }

    private static void AssertCachedBinding(string scenarioId)
    {
        var state = CreateImportBindingState(reservedNames: ["watch"]);

        var first = state.Argument.BindImportSpecifier("vue", "watch");
        var second = state.Argument.BindImportSpecifier("vue", "watch");

        Assert.AreEqual(ExpectedAlias("vue", "watch"), first.Name, scenarioId);
        Assert.AreEqual(first.Name, second.Name, scenarioId);
        Assert.HasCount(1, state.Bindings, scenarioId);
        Assert.HasCount(1, state.LocalBindings, scenarioId);
        AssertImportGroups(
            state.Argument.FlushImportSpecifiers(),
            [new ImportGroupShape("vue", [$"named:watch->{first.Name}"])],
            scenarioId);
    }

    private static void AssertCrossModuleNameCollision(string scenarioId)
    {
        var state = CreateImportBindingState();

        var first = state.Argument.BindImportSpecifier("vue", "watch");
        var second = state.Argument.BindImportSpecifier("runtime", "watch");

        var expectedAlias = ExpectedAlias("runtime", "watch");
        Assert.AreEqual("watch", first.Name, scenarioId);
        Assert.AreEqual(expectedAlias, second.Name, scenarioId);
        AssertImportGroups(
            state.Argument.FlushImportSpecifiers(),
            [
                new ImportGroupShape("vue", ["named:watch->watch"]),
                new ImportGroupShape("runtime", [$"named:watch->{expectedAlias}"])
            ],
            scenarioId);
    }

    private static void AssertSameModuleMixedAliases(string scenarioId)
    {
        var state = CreateImportBindingState(reservedNames: ["watch"]);

        var raw = state.Argument.BindImportSpecifier("vue", "computed");
        var aliased = state.Argument.BindImportSpecifier("vue", "watch");

        var expectedAlias = ExpectedAlias("vue", "watch");
        Assert.AreEqual("computed", raw.Name, scenarioId);
        Assert.AreEqual(expectedAlias, aliased.Name, scenarioId);
        AssertImportGroups(
            state.Argument.FlushImportSpecifiers(),
            [
                new ImportGroupShape(
                    "vue",
                    ["named:computed->computed", $"named:watch->{expectedAlias}"])
            ],
            scenarioId);
    }

    private static SenseArgument CreatePopulatedContext()
        => new(
            Sense.RightValue,
            UseImportAliases: true,
            PatternInput: new Identifier("candidate"),
            CatchExceptionVar: "failure",
            SwitchExpressionVar: "selection");

    private static void AssertCollectorsShared(
        SenseArgument source,
        SenseArgument copy,
        string scenarioId)
    {
        copy.AddVarDeclarator(Declarator("shared", 1), depth: 0);
        copy.MergeImportSpecifier("vue", NamedImport("computed"));

        Assert.IsTrue(source.HasVarDeclarator, scenarioId);
        Assert.IsTrue(source.HasVarImportDeclarationSpecifier, scenarioId);
    }

    private static void AssertContext(
        SenseArgument argument,
        Sense sense,
        bool useImportAliases,
        Expression? patternInput,
        string? catchVariable,
        string? switchVariable,
        string scenarioId)
    {
        Assert.AreEqual(sense, argument.Sense, scenarioId);
        Assert.AreEqual(useImportAliases, argument.UseImportAliases, scenarioId);
        Assert.AreSame(patternInput, argument.PatternInput, scenarioId);
        Assert.AreEqual(catchVariable, argument.CatchExceptionVar, scenarioId);
        Assert.AreEqual(switchVariable, argument.SwitchExpressionVar, scenarioId);
    }

    private static VariableDeclarator Declarator(string name, int value)
        => new(
            new Identifier(name),
            new NumericLiteral(value, value.ToString(System.Globalization.CultureInfo.InvariantCulture)));

    private static ImportSpecifier NamedImport(string name, string? localName = null)
        => new(new Identifier(name), new Identifier(localName ?? name));

    private static ImportBindingState CreateImportBindingState(
        IReadOnlyCollection<string>? reservedNames = null,
        string? currentModuleImportPath = null,
        IReadOnlyCollection<string>? currentModuleBindings = null)
    {
        var bindings = new Dictionary<string, string>(StringComparer.Ordinal);
        var localBindings = new Dictionary<string, string>(StringComparer.Ordinal);
        var reserved = new HashSet<string>(reservedNames ?? [], StringComparer.Ordinal);
        var currentBindings = new HashSet<string>(currentModuleBindings ?? [], StringComparer.Ordinal);
        var argument = new SenseArgument(UseImportAliases: true)
            .WithImportContext(
                bindings,
                localBindings,
                reserved,
                currentModuleImportPath,
                currentBindings);
        return new ImportBindingState(argument, bindings, localBindings);
    }

    private static string ImportBindingKey(string modulePath, string importedName)
        => $"{modulePath}\0{importedName}";

    private static string ExpectedAlias(string modulePath, string importedName)
        => $"i${Format.HashName(ImportBindingKey(modulePath, importedName)).TrimStart('_')}";

    private static void AssertImportGroups(
        IReadOnlyList<KeyValuePair<string, NodeList<ImportDeclarationSpecifier>>> actual,
        IReadOnlyList<ImportGroupShape> expected,
        string scenarioId)
    {
        Assert.HasCount(expected.Count, actual, scenarioId);
        for (var index = 0; index < expected.Count; index++)
        {
            Assert.AreEqual(expected[index].ModulePath, actual[index].Key, scenarioId);
            CollectionAssert.AreEqual(
                expected[index].Specifiers.ToArray(),
                actual[index].Value.Select(DescribeImportSpecifier).ToArray(),
                $"{scenarioId}: module '{expected[index].ModulePath}'.");
        }
    }

    private static string DescribeImportSpecifier(ImportDeclarationSpecifier specifier)
        => specifier switch
        {
            ImportDefaultSpecifier value => $"default:{value.Local.Name}",
            ImportNamespaceSpecifier value => $"namespace:{value.Local.Name}",
            ImportSpecifier value when value.Imported is Identifier imported =>
                $"named:{imported.Name}->{value.Local.Name}",
            ImportSpecifier value when value.Imported is StringLiteral imported =>
                $"named:{imported.Value}->{value.Local.Name}",
            _ => throw new InvalidOperationException(
                $"Unexpected import specifier '{specifier.Type}'.")
        };

    private static IBlockOperation GetBlockOperation()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            "class TestClass { int TestMethod(int input) { return input + 1; } }",
            TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            assemblyName: "SenseArgumentScenarios",
            syntaxTrees: [syntaxTree],
            references: TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Global).Assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(
            0,
            errors,
            string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single();
        return (IBlockOperation?)compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!)
            ?? throw new InvalidOperationException("Expected a block operation for TestMethod.");
    }

    private static void AssertCatalogCoversEveryKind<TScenario, TKind>(
        IReadOnlyList<TScenario> scenarios,
        Func<TScenario, TKind> getKind,
        IReadOnlyCollection<TKind> expectedKinds)
        where TKind : struct, Enum
    {
        Assert.HasCount(expectedKinds.Count, scenarios.Select(getKind).Distinct());
    }

    private sealed record ImportBindingState(
        SenseArgument Argument,
        Dictionary<string, string> Bindings,
        Dictionary<string, string> LocalBindings);

    private sealed record ImportGroupShape(
        string ModulePath,
        IReadOnlyList<string> Specifiers);
}

public enum SenseArgumentContextScenarioKind
{
    ParameterlessConstructor,
    DefaultFactory,
    FullConstructor,
    WithSense,
    WithPatternInput,
    WithCatchVariable,
    WithSwitchVariable,
    WithSenseAndPattern,
    WithImportAliases,
    WithNewScope,
    WithScopeContext,
    EnterChildScope,
    EnterEmissionScope,
    AllocateStableName
}

public enum SenseArgumentValidationScenarioKind
{
    NullChildScopeAnchor,
    MissingParentScope,
    NullEmissionScopeAnchor,
    MissingEmissionParentScope,
    MissingAllocationScope,
    NonIdentifierDeclarator
}

public enum SenseArgumentDeclaratorScenarioKind
{
    UninitializedNoOp,
    SingleDeclarator,
    DuplicateDepthAndName,
    SameNameDifferentDepth,
    DifferentNamesSameDepth,
    FlushResetsCollection
}

public enum SenseArgumentImportCollectionScenarioKind
{
    UninitializedNoOp,
    SeparateModuleGroups,
    NormalizeWithinModule,
    MixedSpecifierKinds,
    FlushResetsCollection
}

public enum SenseArgumentImportBindingScenarioKind
{
    NullImportedName,
    WhitespaceImportedName,
    NullModulePath,
    WhitespaceModulePath,
    UninitializedStorage,
    AliasesDisabled,
    RawNameAvailable,
    ReservedNameCollision,
    DefaultImportAlias,
    CurrentModuleBinding,
    CurrentModuleMissingBinding,
    CachedBinding,
    CrossModuleNameCollision,
    SameModuleMixedAliases
}

public sealed record SenseArgumentContextScenario(
    string Id,
    string Dimension,
    SenseArgumentContextScenarioKind Kind);

public sealed record SenseArgumentValidationScenario(
    string Id,
    string Dimension,
    SenseArgumentValidationScenarioKind Kind,
    string ExpectedMessage);

public sealed record SenseArgumentDeclaratorScenario(
    string Id,
    string Dimension,
    SenseArgumentDeclaratorScenarioKind Kind);

public sealed record SenseArgumentImportCollectionScenario(
    string Id,
    string Dimension,
    SenseArgumentImportCollectionScenarioKind Kind);

public sealed record SenseArgumentImportBindingScenario(
    string Id,
    string Dimension,
    SenseArgumentImportBindingScenarioKind Kind);

internal static class SenseArgumentScenarioCatalog
{
    public static IReadOnlyList<SenseArgumentContextScenario> Contexts { get; } =
    [
        Context("constructor.parameterless", "initialized-empty-dependency-state", SenseArgumentContextScenarioKind.ParameterlessConstructor),
        Context("constructor.default-factory", "default-factory-initialization", SenseArgumentContextScenarioKind.DefaultFactory),
        Context("constructor.full", "full-context-state", SenseArgumentContextScenarioKind.FullConstructor),
        Context("copy.with-sense", "sense-copy-shared-collectors", SenseArgumentContextScenarioKind.WithSense),
        Context("copy.with-pattern", "pattern-input-copy-shared-collectors", SenseArgumentContextScenarioKind.WithPatternInput),
        Context("copy.with-catch", "catch-variable-copy-shared-collectors", SenseArgumentContextScenarioKind.WithCatchVariable),
        Context("copy.with-switch", "switch-variable-copy-shared-collectors", SenseArgumentContextScenarioKind.WithSwitchVariable),
        Context("copy.with-sense-pattern", "combined-sense-pattern-copy", SenseArgumentContextScenarioKind.WithSenseAndPattern),
        Context("copy.with-import-aliases", "import-alias-mode-copy", SenseArgumentContextScenarioKind.WithImportAliases),
        Context("scope.new", "declarator-isolation-import-sharing", SenseArgumentContextScenarioKind.WithNewScope),
        Context("scope.attach", "root-emission-scope-attachment", SenseArgumentContextScenarioKind.WithScopeContext),
        Context("scope.enter-child", "child-scope-anchor-and-collector-boundary", SenseArgumentContextScenarioKind.EnterChildScope),
        Context("scope.enter-emission", "emission-scope-anchor-and-declarator-boundary", SenseArgumentContextScenarioKind.EnterEmissionScope),
        Context("scope.allocate-name", "scope-backed-stable-name-allocation", SenseArgumentContextScenarioKind.AllocateStableName)
    ];

    public static IReadOnlyList<SenseArgumentValidationScenario> Validations { get; } =
    [
        Validation(
            "validation.child-null-anchor",
            "null-child-scope-anchor",
            SenseArgumentValidationScenarioKind.NullChildScopeAnchor,
            "Jazor 无法进入空的发射作用域。"),
        Validation(
            "validation.child-missing-parent",
            "missing-child-scope-parent",
            SenseArgumentValidationScenarioKind.MissingParentScope,
            "Jazor 无法为 Block 创建子作用域，因为当前上下文缺少发射作用域。"),
        Validation(
            "validation.emission-null-anchor",
            "null-emission-scope-anchor",
            SenseArgumentValidationScenarioKind.NullEmissionScopeAnchor,
            "Jazor 无法进入空的发射作用域。"),
        Validation(
            "validation.emission-missing-parent",
            "missing-emission-scope-parent",
            SenseArgumentValidationScenarioKind.MissingEmissionParentScope,
            "Jazor 无法为 Block 创建发射作用域，因为当前上下文缺少父作用域。"),
        Validation(
            "validation.allocate-missing-scope",
            "stable-name-allocation-without-scope",
            SenseArgumentValidationScenarioKind.MissingAllocationScope,
            "Jazor 无法分配稳定名称，因为当前上下文缺少发射作用域。"),
        Validation(
            "validation.declarator-non-identifier",
            "non-identifier-declarator-rejection",
            SenseArgumentValidationScenarioKind.NonIdentifierDeclarator,
            "Collected JavaScript variable declarators require an identifier binding, but received 'ObjectPattern'.")
    ];

    public static IReadOnlyList<SenseArgumentDeclaratorScenario> Declarators { get; } =
    [
        Declarator("declarator.uninitialized", "uninitialized-struct-no-op", SenseArgumentDeclaratorScenarioKind.UninitializedNoOp),
        Declarator("declarator.single", "single-identifier-collection", SenseArgumentDeclaratorScenarioKind.SingleDeclarator),
        Declarator("declarator.duplicate-key", "depth-name-key-deduplication", SenseArgumentDeclaratorScenarioKind.DuplicateDepthAndName),
        Declarator("declarator.same-name-depths", "same-name-distinct-depths", SenseArgumentDeclaratorScenarioKind.SameNameDifferentDepth),
        Declarator("declarator.same-depth-names", "same-depth-distinct-names", SenseArgumentDeclaratorScenarioKind.DifferentNamesSameDepth),
        Declarator("declarator.flush-reset", "flush-clears-declarator-state", SenseArgumentDeclaratorScenarioKind.FlushResetsCollection)
    ];

    public static IReadOnlyList<SenseArgumentImportCollectionScenario> ImportCollections { get; } =
    [
        ImportCollection("import-collection.uninitialized", "uninitialized-struct-no-op", SenseArgumentImportCollectionScenarioKind.UninitializedNoOp),
        ImportCollection("import-collection.module-groups", "module-path-grouping", SenseArgumentImportCollectionScenarioKind.SeparateModuleGroups),
        ImportCollection("import-collection.normalize", "per-module-order-and-deduplication", SenseArgumentImportCollectionScenarioKind.NormalizeWithinModule),
        ImportCollection("import-collection.mixed-kinds", "default-and-named-normalization", SenseArgumentImportCollectionScenarioKind.MixedSpecifierKinds),
        ImportCollection("import-collection.flush-reset", "flush-clears-import-state", SenseArgumentImportCollectionScenarioKind.FlushResetsCollection)
    ];

    public static IReadOnlyList<SenseArgumentImportBindingScenario> ImportBindings { get; } =
    [
        ImportBinding("import-binding.null-name", "null-imported-name-bypass", SenseArgumentImportBindingScenarioKind.NullImportedName),
        ImportBinding("import-binding.whitespace-name", "whitespace-imported-name-bypass", SenseArgumentImportBindingScenarioKind.WhitespaceImportedName),
        ImportBinding("import-binding.null-module", "null-module-path-bypass", SenseArgumentImportBindingScenarioKind.NullModulePath),
        ImportBinding("import-binding.whitespace-module", "whitespace-module-path-bypass", SenseArgumentImportBindingScenarioKind.WhitespaceModulePath),
        ImportBinding("import-binding.uninitialized", "uninitialized-binding-storage-bypass", SenseArgumentImportBindingScenarioKind.UninitializedStorage),
        ImportBinding("import-binding.aliases-disabled", "raw-binding-with-normalized-deduplication", SenseArgumentImportBindingScenarioKind.AliasesDisabled),
        ImportBinding("import-binding.raw-name", "available-raw-local-name", SenseArgumentImportBindingScenarioKind.RawNameAvailable),
        ImportBinding("import-binding.reserved-collision", "module-declaration-name-collision", SenseArgumentImportBindingScenarioKind.ReservedNameCollision),
        ImportBinding("import-binding.default", "default-import-legal-local-alias", SenseArgumentImportBindingScenarioKind.DefaultImportAlias),
        ImportBinding("import-binding.current-module", "current-module-binding-bypass", SenseArgumentImportBindingScenarioKind.CurrentModuleBinding),
        ImportBinding("import-binding.current-module-missing", "current-module-unbound-symbol-import", SenseArgumentImportBindingScenarioKind.CurrentModuleMissingBinding),
        ImportBinding("import-binding.cached", "repeated-binding-cache-stability", SenseArgumentImportBindingScenarioKind.CachedBinding),
        ImportBinding("import-binding.cross-module-collision", "cross-module-local-name-uniqueness", SenseArgumentImportBindingScenarioKind.CrossModuleNameCollision),
        ImportBinding("import-binding.same-module-mixed", "same-module-raw-and-aliased-bindings", SenseArgumentImportBindingScenarioKind.SameModuleMixedAliases)
    ];

    private static SenseArgumentContextScenario Context(
        string id,
        string dimension,
        SenseArgumentContextScenarioKind kind)
        => new($"sense-argument.{id}", dimension, kind);

    private static SenseArgumentValidationScenario Validation(
        string id,
        string dimension,
        SenseArgumentValidationScenarioKind kind,
        string expectedMessage)
        => new($"sense-argument.{id}", dimension, kind, expectedMessage);

    private static SenseArgumentDeclaratorScenario Declarator(
        string id,
        string dimension,
        SenseArgumentDeclaratorScenarioKind kind)
        => new($"sense-argument.{id}", dimension, kind);

    private static SenseArgumentImportCollectionScenario ImportCollection(
        string id,
        string dimension,
        SenseArgumentImportCollectionScenarioKind kind)
        => new($"sense-argument.{id}", dimension, kind);

    private static SenseArgumentImportBindingScenario ImportBinding(
        string id,
        string dimension,
        SenseArgumentImportBindingScenarioKind kind)
        => new($"sense-argument.{id}", dimension, kind);
}
