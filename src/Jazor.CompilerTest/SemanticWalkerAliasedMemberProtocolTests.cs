using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerAliasedMemberProtocolTests
{
    [TestMethod]
    public void Visit_ConfiguredPropertyAliases_UseTheDeclaredJavaScriptKeyShape()
    {
        var block = GetBlockOperation("""
            using ECMAScript;

            class AliasedMemberScenarios
            {
                private sealed class Payload
                {
                    [ECMAScriptName("[0]")]
                    public int First { get; set; }

                    [ECMAScriptName("[\"data-id\"]")]
                    public int Identifier { get; set; }

                    [ECMAScriptName("aria-label")]
                    public string Label { get; set; } = "";
                }

                void TestMethod(Payload payload)
                {
                    int first = payload.First;
                    int identifier = payload.Identifier;
                    string label = payload.Label;
                    payload.First = identifier;
                    payload.Identifier = first;
                    payload.Label = label;
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "payload[0]", StringComparison.Ordinal);
        StringAssert.Contains(script, "payload[\"data-id\"]", StringComparison.Ordinal);
        StringAssert.Contains(script, "payload[\"aria-label\"]", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_ConfiguredMethodAliases_UseTheDeclaredJavaScriptKeyShapeForCallsAndMethodGroups()
    {
        var block = GetBlockOperation("""
            using System;
            using ECMAScript;

            class AliasedMemberScenarios
            {
                private sealed class Payload
                {
                    [ECMAScriptName("[0]")]
                    public int First(int value) => value;

                    [ECMAScriptName("[\"data-id\"]")]
                    public int Identifier(int value) => value;

                    [ECMAScriptName("aria-label")]
                    public int Label(int value) => value;
                }

                void TestMethod(Payload payload)
                {
                    int first = payload.First(1);
                    int identifier = payload.Identifier(2);
                    int label = payload.Label(3);
                    Func<int, int> firstMethod = payload.First;
                    Func<int, int> identifierMethod = payload.Identifier;
                    Func<int, int> labelMethod = payload.Label;
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "payload[0](1)", StringComparison.Ordinal);
        StringAssert.Contains(script, "payload[\"data-id\"](2)", StringComparison.Ordinal);
        StringAssert.Contains(script, "payload[\"aria-label\"](3)", StringComparison.Ordinal);
        StringAssert.Contains(script, "payload[0].bind(payload)", StringComparison.Ordinal);
        StringAssert.Contains(script, "payload[\"data-id\"].bind(payload)", StringComparison.Ordinal);
        StringAssert.Contains(script, "payload[\"aria-label\"].bind(payload)", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_ConfiguredStaticAliases_UseTheDeclaredJavaScriptKeyShapeForReadsWritesAndCalls()
    {
        var block = GetBlockOperation("""
            using ECMAScript;

            [ECMAScript]
            static class PropertyHub
            {
                [ECMAScriptName("[0]")]
                public static int First { get; set; }

                [ECMAScriptName("[\"data-id\"]")]
                public static int Identifier { get; set; }

                [ECMAScriptName("aria-label")]
                public static int Label { get; set; }

                [ECMAScriptName("[\"call-id\"]")]
                public static int Invoke(int value) => value;
            }

            class AliasedMemberScenarios
            {
                void TestMethod()
                {
                    int first = PropertyHub.First;
                    int identifier = PropertyHub.Identifier;
                    int label = PropertyHub.Label;
                    PropertyHub.First = identifier;
                    PropertyHub.Identifier = label;
                    PropertyHub.Label = first;
                    int invoked = PropertyHub.Invoke(first);
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "PropertyHub[0]", StringComparison.Ordinal);
        StringAssert.Contains(script, "PropertyHub[\"data-id\"]", StringComparison.Ordinal);
        StringAssert.Contains(script, "PropertyHub[\"aria-label\"]", StringComparison.Ordinal);
        StringAssert.Contains(script, "PropertyHub[\"call-id\"](first)", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "SemanticWalkerAliasedMemberProtocolTests",
            [syntaxTree],
            TestMetadataReferences.Net11.Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "TestMethod");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
