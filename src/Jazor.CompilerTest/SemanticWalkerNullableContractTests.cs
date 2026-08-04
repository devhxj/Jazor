using Acornima.Ast;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerNullableContractTests
{
    [TestMethod]
    public void CompileNullableValue_RejectsInvalidHostInvocationShapes()
    {
        var fixture = CompileFixture();
        var walker = new SemanticWalker(true);
        var valueOperation = fixture.NullableValue;
        var symbol = valueOperation.Property;
        var context = new SenseArgument();
        var handler = new Identifier("value");

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            walker.CompileNullableValue(symbol, context, null, [], valueOperation));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            walker.CompileNullableValue(
                symbol,
                context,
                handler,
                [new NumericLiteral(1, "1")],
                valueOperation));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            walker.CompileNullableValue(symbol, context, handler, [], fixture.Touch));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            walker.CompileNullableValue(symbol, context, handler, [], fixture.PlainProperty));
    }

    [TestMethod]
    public void CompileNullableGetValueOrDefault_RejectsInvalidHostInvocationShapes()
    {
        var fixture = CompileFixture();
        var walker = new SemanticWalker(true);
        var invocation = fixture.NullableDefault;
        var symbol = invocation.TargetMethod;
        var context = new SenseArgument();
        var handler = new Identifier("value");

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            walker.CompileNullableGetValueOrDefault(symbol, context, null, [], invocation));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            walker.CompileNullableGetValueOrDefault(
                symbol,
                context,
                handler,
                [new NumericLiteral(1, "1")],
                invocation));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            walker.CompileNullableGetValueOrDefault(symbol, context, handler, [], fixture.Touch));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            walker.CompileNullableGetValueOrDefault(symbol, context, handler, [], fixture.StaticParse));
    }

    private static NullableFixture CompileFixture()
    {
        const string source =
            """
            using System;

            class TestClass
            {
                int PlainProperty => 1;

                static int Parse(string value) => int.Parse(value);

                void Touch() { }

                void TestMethod(int? value)
                {
                    var current = value.Value;
                    var fallback = value.GetValueOrDefault();
                    var plain = PlainProperty;
                    Touch();
                    var parsed = Parse("1");
                }
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "SemanticWalkerNullableContractTests",
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var model = compilation.GetSemanticModel(syntaxTree);
        var method = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "TestMethod");
        var block = Assert.IsInstanceOfType<IBlockOperation>(model.GetOperation(method.Body!));
        return new NullableFixture(
            block.DescendantsAndSelf()
                .OfType<IPropertyReferenceOperation>()
                .Single(static operation => operation.Property.Name == "Value"),
            block.DescendantsAndSelf()
                .OfType<IInvocationOperation>()
                .Single(static operation => operation.TargetMethod.Name == "GetValueOrDefault"),
            block.DescendantsAndSelf()
                .OfType<IInvocationOperation>()
                .Single(static operation => operation.TargetMethod.Name == "Touch"),
            block.DescendantsAndSelf()
                .OfType<IPropertyReferenceOperation>()
                .Single(static operation => operation.Property.Name == "PlainProperty"),
            block.DescendantsAndSelf()
                .OfType<IInvocationOperation>()
                .Single(static operation => operation.TargetMethod.Name == "Parse"));
    }

    private sealed record NullableFixture(
        IPropertyReferenceOperation NullableValue,
        IInvocationOperation NullableDefault,
        IInvocationOperation Touch,
        IPropertyReferenceOperation PlainProperty,
        IInvocationOperation StaticParse);
}
