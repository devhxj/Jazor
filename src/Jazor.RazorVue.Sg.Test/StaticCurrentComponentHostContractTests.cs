using Acornima.Ast;
using Jazor.Compiler;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class StaticCurrentComponentHostContractTests
{
    [TestMethod]
    public void StaticStorageProjection_UsesLexicalFieldAndAccessorBindings()
    {
        var fixture = CreateFixture();
        var component = fixture.Component;
        var staticField = component.GetMembers("staticField").OfType<IFieldSymbol>().Single();
        var staticAuto = GetProperty(component, "StaticAuto");
        var staticComputed = GetProperty(component, "StaticComputed");
        var autoBackingField = component
            .GetMembers("<StaticAuto>k__BackingField")
            .OfType<IFieldSymbol>()
            .Single();
        Assert.IsNotNull(staticComputed.GetMethod);
        Assert.IsNotNull(staticComputed.SetMethod);

        var host = new CurrentComponentSemanticWalkerHost(
            component,
            memberRuntimeNames: new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default)
            {
                [staticField.OriginalDefinition] = "moduleField",
                [autoBackingField.OriginalDefinition] = "moduleAuto",
                [staticComputed.GetMethod!.OriginalDefinition] = "readComputed",
                [staticComputed.SetMethod!.OriginalDefinition] = "writeComputed"
            });

        Assert.AreEqual(
            "moduleField",
            host.RewriteFieldReference(
                GetInitializer<IFieldReferenceOperation>(fixture, "field"),
                new SenseArgument(),
                null)!
                .ToKnRECMAScript());
        Assert.AreEqual(
            "moduleAuto",
            host.RewritePropertyReference(
                GetInitializer<IPropertyReferenceOperation>(fixture, "auto"),
                new SenseArgument(),
                null,
                [])!
                .ToKnRECMAScript());
        Assert.AreEqual(
            "readComputed()",
            host.RewritePropertyReference(
                GetInitializer<IPropertyReferenceOperation>(fixture, "computed"),
                new SenseArgument(),
                null,
                [])!
                .ToKnRECMAScript());

        Assert.AreEqual(
            "moduleAuto = value",
            host.RewriteSimpleAssignmentPostorder(
                GetAssignment(fixture, "StaticAuto"),
                new SenseArgument(),
                new Identifier("value"))!
                .ToKnRECMAScript());
        Assert.AreEqual(
            "writeComputed(value)",
            host.RewriteSimpleAssignmentPostorder(
                GetAssignment(fixture, "StaticComputed"),
                new SenseArgument(),
                new Identifier("value"))!
                .ToKnRECMAScript());
    }

    private static Fixture CreateFixture()
    {
        var tree = CSharpSyntaxTree.ParseText(
            """
            namespace StaticHostContracts;

            public sealed class StaticHostComponent
            {
                private static int staticField;

                public static int StaticAuto { get; set; }

                public static int StaticComputed
                {
                    get => staticField;
                    set => staticField = value;
                }

                public void ReadAndWrite()
                {
                    var field = staticField;
                    var auto = StaticAuto;
                    var computed = StaticComputed;
                    StaticAuto = 1;
                    StaticComputed = 2;
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "StaticCurrentComponentHostContracts.cs");
        var compilation = CSharpCompilation.Create(
            "Jazor.RazorVue.StaticCurrentComponentHost.Contracts",
            [tree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

        var component = compilation.GetTypeByMetadataName("StaticHostContracts.StaticHostComponent");
        Assert.IsNotNull(component);
        return new Fixture(compilation.GetSemanticModel(tree), component!);
    }

    private static TOperation GetInitializer<TOperation>(Fixture fixture, string localName)
        where TOperation : class, IOperation
    {
        var declarator = GetReadAndWriteMethod(fixture)
            .DescendantNodes()
            .OfType<VariableDeclaratorSyntax>()
            .Single(candidate => candidate.Identifier.ValueText == localName);
        var operation = fixture.SemanticModel.GetOperation(declarator.Initializer!.Value) as TOperation;
        Assert.IsNotNull(operation, localName);
        return operation!;
    }

    private static ISimpleAssignmentOperation GetAssignment(Fixture fixture, string memberName)
    {
        var assignment = GetReadAndWriteMethod(fixture)
            .DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .Single(candidate => candidate.Left is IdentifierNameSyntax identifier &&
                                 identifier.Identifier.ValueText == memberName);
        var operation = fixture.SemanticModel.GetOperation(assignment) as ISimpleAssignmentOperation;
        Assert.IsNotNull(operation, memberName);
        return operation!;
    }

    private static MethodDeclarationSyntax GetReadAndWriteMethod(Fixture fixture)
        => fixture.SemanticModel.SyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static method => method.Identifier.ValueText == "ReadAndWrite");

    private static IPropertySymbol GetProperty(INamedTypeSymbol type, string name)
        => type.GetMembers(name).OfType<IPropertySymbol>().Single();

    private sealed record Fixture(
        SemanticModel SemanticModel,
        INamedTypeSymbol Component);
}
