using Acornima.Ast;
using Jazor.Compiler;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class ImportDeclarationFactoryTests
{
    public static IEnumerable<TestDataRow<ImportFactoryCase>> Cases
        => ImportFactoryScenarioCatalog.All.Select(static testCase =>
            new TestDataRow<ImportFactoryCase>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndInputs()
    {
        var cases = ImportFactoryScenarioCatalog.All;

        Assert.IsNotEmpty(cases);
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(
            cases.Count,
            cases.Select(static testCase => testCase.InputIdentity).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(cases.All(static testCase => testCase.Id.StartsWith("import-factory.", StringComparison.Ordinal)));
        Assert.IsTrue(cases.All(static testCase => !string.IsNullOrWhiteSpace(testCase.Dimension)));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void Create_MatchesCanonicalDeclarationContract(ImportFactoryCase testCase)
    {
        var declarations = ImportDeclarationFactory.Create(
            testCase.ModulePath,
            testCase.Specifiers.Select(static specifier => specifier.Create()));

        Assert.HasCount(testCase.ExpectedDeclarations.Count, declarations, testCase.Id);
        for (var declarationIndex = 0; declarationIndex < declarations.Length; declarationIndex++)
        {
            var declaration = declarations[declarationIndex];
            var source = declaration.Source as StringLiteral;
            Assert.IsNotNull(source, testCase.Id);
            Assert.AreEqual(testCase.ModulePath, source.Value, testCase.Id);

            var actualSpecifiers = declaration.Specifiers
                .Select(ToShape)
                .ToArray();
            CollectionAssert.AreEqual(
                testCase.ExpectedDeclarations[declarationIndex].Specifiers.ToArray(),
                actualSpecifiers,
                $"{testCase.Id}: declaration {declarationIndex}.");
        }
    }

    [TestMethod]
    public void NormalizeSpecifiers_OrdersEveryKindImportedNameAndLocalName()
    {
        ImportSpecifierSpec[] input =
        [
            new ImportSpecifierSpec.NamedIdentifier("watch", "watchB"),
            new ImportSpecifierSpec.Namespace("namespaceB"),
            new ImportSpecifierSpec.Default("defaultB"),
            new ImportSpecifierSpec.NamedIdentifier("computed", "computed"),
            new ImportSpecifierSpec.Default("defaultA"),
            new ImportSpecifierSpec.Namespace("namespaceA"),
            new ImportSpecifierSpec.NamedIdentifier("watch", "watchA")
        ];

        var normalized = ImportDeclarationFactory.NormalizeSpecifiers(
            input.Select(static specifier => specifier.Create()));

        CollectionAssert.AreEqual(
            new[]
            {
                Shape(ImportSpecifierShapeKind.Default, null, "defaultA"),
                Shape(ImportSpecifierShapeKind.Default, null, "defaultB"),
                Shape(ImportSpecifierShapeKind.Namespace, null, "namespaceA"),
                Shape(ImportSpecifierShapeKind.Namespace, null, "namespaceB"),
                Shape(ImportSpecifierShapeKind.Named, "computed", "computed"),
                Shape(ImportSpecifierShapeKind.Named, "watch", "watchA"),
                Shape(ImportSpecifierShapeKind.Named, "watch", "watchB")
            },
            normalized.Select(ToShape).ToArray());
    }

    [TestMethod]
    public void WithModulePath_RebindsOnlySourceAndPreservesDeclarationMetadata()
    {
        var declaration = ImportDeclarationFactory.Create(
            "old-module",
            [new ImportSpecifier(new Identifier("computed"))]).Single();

        var rebound = ImportDeclarationFactory.WithModulePath(declaration, "@scope/new-module");

        Assert.AreNotSame(declaration, rebound);
        Assert.AreEqual("@scope/new-module", ((StringLiteral)rebound.Source).Value);
        CollectionAssert.AreEqual(declaration.Specifiers.ToArray(), rebound.Specifiers.ToArray());
        CollectionAssert.AreEqual(declaration.Attributes.ToArray(), rebound.Attributes.ToArray());
        Assert.AreEqual(declaration.Phase, rebound.Phase);
    }

    [TestMethod]
    public void PublicMethods_NullArguments_ThrowNamedArgumentExceptions()
    {
        var specifiers = Array.Empty<ImportDeclarationSpecifier>();
        var declaration = ImportDeclarationFactory.Create(
            "module",
            [new ImportSpecifier(new Identifier("computed"))]).Single();

        var createPathException = Assert.Throws<ArgumentNullException>(
            () => ImportDeclarationFactory.Create(null!, specifiers));
        var createSpecifiersException = Assert.Throws<ArgumentNullException>(
            () => ImportDeclarationFactory.Create("module", null!));
        var normalizeException = Assert.Throws<ArgumentNullException>(
            () => ImportDeclarationFactory.NormalizeSpecifiers(null!));
        var declarationException = Assert.Throws<ArgumentNullException>(
            () => ImportDeclarationFactory.WithModulePath(null!, "module"));
        var reboundPathException = Assert.Throws<ArgumentNullException>(
            () => ImportDeclarationFactory.WithModulePath(declaration, null!));

        Assert.AreEqual("modulePath", createPathException.ParamName);
        Assert.AreEqual("specifiers", createSpecifiersException.ParamName);
        Assert.AreEqual("specifiers", normalizeException.ParamName);
        Assert.AreEqual("declaration", declarationException.ParamName);
        Assert.AreEqual("modulePath", reboundPathException.ParamName);
    }

    [TestMethod]
    public void NormalizeSpecifiers_UnsupportedNamedImportKey_ThrowsControlledFailure()
    {
        var malformedSpecifier = new ImportSpecifier(
            new NumericLiteral(1, "1"),
            new Identifier("one"));

        var exception = Assert.Throws<NotSupportedException>(
            () => ImportDeclarationFactory.NormalizeSpecifiers([malformedSpecifier]));

        StringAssert.Contains(exception.Message, "Unsupported ECMAScript named import key");
        StringAssert.Contains(exception.Message, malformedSpecifier.Imported.Type.ToString());
    }

    private static ImportSpecifierShape ToShape(ImportDeclarationSpecifier specifier)
        => specifier switch
        {
            ImportDefaultSpecifier value => Shape(ImportSpecifierShapeKind.Default, null, value.Local.Name),
            ImportNamespaceSpecifier value => Shape(ImportSpecifierShapeKind.Namespace, null, value.Local.Name),
            ImportSpecifier value when value.Imported is Identifier identifier =>
                Shape(ImportSpecifierShapeKind.Named, identifier.Name, value.Local.Name),
            ImportSpecifier value when value.Imported is StringLiteral literal =>
                Shape(ImportSpecifierShapeKind.Named, literal.Value, value.Local.Name),
            _ => throw new InvalidOperationException($"Unexpected import specifier '{specifier.Type}'.")
        };

    private static ImportSpecifierShape Shape(
        ImportSpecifierShapeKind kind,
        string? importedName,
        string localName)
        => new(kind, importedName, localName);
}

public sealed record ImportFactoryCase(
    string Id,
    string Dimension,
    string ModulePath,
    IReadOnlyList<ImportSpecifierSpec> Specifiers,
    IReadOnlyList<ImportDeclarationExpectation> ExpectedDeclarations)
{
    public string InputIdentity
        => $"{ModulePath}|{string.Join("|", Specifiers)}";
}

public sealed record ImportDeclarationExpectation(IReadOnlyList<ImportSpecifierShape> Specifiers);

public enum ImportSpecifierShapeKind
{
    Default,
    Namespace,
    Named
}

public sealed record ImportSpecifierShape(
    ImportSpecifierShapeKind Kind,
    string? ImportedName,
    string LocalName);

public abstract record ImportSpecifierSpec
{
    public sealed record Default(string LocalName) : ImportSpecifierSpec;

    public sealed record Namespace(string LocalName) : ImportSpecifierSpec;

    public sealed record NamedIdentifier(string ImportedName, string LocalName) : ImportSpecifierSpec;

    public sealed record NamedString(string ImportedName, string LocalName) : ImportSpecifierSpec;

    public ImportDeclarationSpecifier Create()
        => this switch
        {
            Default value => new ImportDefaultSpecifier(new Identifier(value.LocalName)),
            Namespace value => new ImportNamespaceSpecifier(new Identifier(value.LocalName)),
            NamedIdentifier value => new ImportSpecifier(
                new Identifier(value.ImportedName),
                new Identifier(value.LocalName)),
            NamedString value => new ImportSpecifier(
                new StringLiteral(value.ImportedName, $"\"{value.ImportedName}\""),
                new Identifier(value.LocalName)),
            _ => throw new InvalidOperationException($"Unknown import specifier spec '{GetType().Name}'.")
        };
}

internal static class ImportFactoryScenarioCatalog
{
    public static IReadOnlyList<ImportFactoryCase> All { get; } =
    [
        Case("import-factory.empty", "empty-input", "empty-module", [], []),
        Case(
            "import-factory.named.single",
            "named-import",
            "vue",
            [Named("computed")],
            [Declaration(NamedShape("computed"))]),
        Case(
            "import-factory.named.prefix-order",
            "ordinal-prefix-order",
            "vue",
            [Named("toRefs"), Named("toRef")],
            [Declaration(NamedShape("toRef"), NamedShape("toRefs"))]),
        Case(
            "import-factory.named.exact-dedup",
            "exact-deduplication",
            "vue",
            [Named("watch"), Named("watch"), Named("watch")],
            [Declaration(NamedShape("watch"))]),
        Case(
            "import-factory.named.alias-order",
            "imported-and-local-order",
            "vue",
            [Named("watch", "watchB"), Named("computed"), Named("watch", "watchA")],
            [Declaration(NamedShape("computed"), NamedShape("watch", "watchA"), NamedShape("watch", "watchB"))]),
        Case(
            "import-factory.default.single",
            "default-import",
            "component",
            [Default("component")],
            [Declaration(DefaultShape("component"))]),
        Case(
            "import-factory.default.exact-dedup",
            "exact-deduplication",
            "component",
            [Default("component"), Default("component")],
            [Declaration(DefaultShape("component"))]),
        Case(
            "import-factory.default.multiple-locals",
            "preserve-distinct-default-bindings",
            "component",
            [Default("componentB"), Default("componentA")],
            [Declaration(DefaultShape("componentA")), Declaration(DefaultShape("componentB"))]),
        Case(
            "import-factory.namespace.single",
            "namespace-import",
            "runtime",
            [Namespace("runtime")],
            [Declaration(NamespaceShape("runtime"))]),
        Case(
            "import-factory.namespace.multiple-locals",
            "preserve-distinct-namespace-bindings",
            "runtime",
            [Namespace("runtimeB"), Namespace("runtimeA")],
            [Declaration(NamespaceShape("runtimeA")), Declaration(NamespaceShape("runtimeB"))]),
        Case(
            "import-factory.default-namespace",
            "legal-combined-import",
            "runtime",
            [Namespace("runtime"), Default("runtimeDefault")],
            [Declaration(DefaultShape("runtimeDefault"), NamespaceShape("runtime"))]),
        Case(
            "import-factory.default-named",
            "legal-combined-import",
            "vue",
            [Named("computed"), Default("Vue")],
            [Declaration(DefaultShape("Vue"), NamedShape("computed"))]),
        Case(
            "import-factory.namespace-named-split",
            "grammar-required-split",
            "vue",
            [Named("computed"), Namespace("Vue")],
            [Declaration(NamespaceShape("Vue")), Declaration(NamedShape("computed"))]),
        Case(
            "import-factory.all-kinds-split",
            "grammar-required-split",
            "vue",
            [Named("computed"), Namespace("VueRuntime"), Default("Vue")],
            [
                Declaration(DefaultShape("Vue"), NamespaceShape("VueRuntime")),
                Declaration(NamedShape("computed"))
            ]),
        Case(
            "import-factory.multiple-default-namespace-preserved",
            "lossless-multi-declaration",
            "runtime",
            [
                Namespace("namespaceB"),
                Default("defaultC"),
                Named("helper"),
                Default("defaultA"),
                Namespace("namespaceA"),
                Default("defaultB")
            ],
            [
                Declaration(DefaultShape("defaultA"), NamespaceShape("namespaceA")),
                Declaration(DefaultShape("defaultB"), NamespaceShape("namespaceB")),
                Declaration(DefaultShape("defaultC"), NamedShape("helper"))
            ]),
        Case(
            "import-factory.named-string-key",
            "string-imported-key",
            "features",
            [NamedString("feature-name", "featureName")],
            [Declaration(NamedShape("feature-name", "featureName"))]),
        Case(
            "import-factory.named-string-identifier-dedup",
            "semantic-key-deduplication",
            "features",
            [NamedString("feature", "feature"), Named("feature")],
            [Declaration(NamedShape("feature"))]),
        Case(
            "import-factory.module-path-preserved",
            "module-specifier-value",
            "@scope/package/subpath",
            [Named("helper")],
            [Declaration(NamedShape("helper"))])
    ];

    private static ImportFactoryCase Case(
        string id,
        string dimension,
        string modulePath,
        IReadOnlyList<ImportSpecifierSpec> specifiers,
        IReadOnlyList<ImportDeclarationExpectation> expectedDeclarations)
        => new(id, dimension, modulePath, specifiers, expectedDeclarations);

    private static ImportSpecifierSpec.Default Default(string localName)
        => new(localName);

    private static ImportSpecifierSpec.Namespace Namespace(string localName)
        => new(localName);

    private static ImportSpecifierSpec.NamedIdentifier Named(string importedName, string? localName = null)
        => new(importedName, localName ?? importedName);

    private static ImportSpecifierSpec.NamedString NamedString(string importedName, string localName)
        => new(importedName, localName);

    private static ImportDeclarationExpectation Declaration(params ImportSpecifierShape[] specifiers)
        => new(specifiers);

    private static ImportSpecifierShape DefaultShape(string localName)
        => new(ImportSpecifierShapeKind.Default, null, localName);

    private static ImportSpecifierShape NamespaceShape(string localName)
        => new(ImportSpecifierShapeKind.Namespace, null, localName);

    private static ImportSpecifierShape NamedShape(string importedName, string? localName = null)
        => new(ImportSpecifierShapeKind.Named, importedName, localName ?? importedName);
}
