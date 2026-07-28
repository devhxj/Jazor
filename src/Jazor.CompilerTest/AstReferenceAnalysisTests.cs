using Acornima;
using Acornima.Ast;
using Jazor.Compiler;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class AstReferenceAnalysisTests
{
    [TestMethod]
    public void CollectIdentifiers_ExcludesBindingsAndNonComputedPropertyNames()
    {
        var objectExpression = new ObjectExpression(NodeList.From<Node>(
            new ObjectProperty(
                PropertyKind.Init,
                new Identifier("header"),
                new MemberExpression(
                    new Identifier("source"),
                    new Identifier("header"),
                    computed: false,
                    optional: false),
                computed: false,
                shorthand: false,
                method: false),
            new ObjectProperty(
                PropertyKind.Init,
                new Identifier("computedKey"),
                new Identifier("value"),
                computed: true,
                shorthand: false,
                method: false)));
        var declaration = new VariableDeclaration(
            VariableDeclarationKind.Const,
            NodeList.From(new VariableDeclarator(new Identifier("local"), objectExpression)));

        var names = AstReferenceAnalysis.CollectIdentifiers([declaration]);

        CollectionAssert.AreEquivalent(
            new[] { "computedKey", "source", "value" },
            names.ToArray());
    }

    [TestMethod]
    public void ImportDeclarationFactory_OrdersPrefixNamesLexically()
    {
        var declarations = ImportDeclarationFactory.Create(
            "vue",
            [
                new ImportSpecifier(new Identifier("toRefs")),
                new ImportSpecifier(new Identifier("toRef"))
            ]);

        var importedNames = declarations
            .Single()
            .Specifiers
            .OfType<ImportSpecifier>()
            .Select(static specifier => ((Identifier)specifier.Imported).Name)
            .ToArray();

        CollectionAssert.AreEqual(new[] { "toRef", "toRefs" }, importedNames);
    }
}
