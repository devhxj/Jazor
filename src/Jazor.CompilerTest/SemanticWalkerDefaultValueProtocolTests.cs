using Acornima;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerDefaultValueProtocolTests
{
    [TestMethod]
    public void Visit_DefaultEnumValues_PreserveAllUnderlyingNumericWidths()
    {
        var block = GetBlockOperation(
            """
            var sbyteValue = default(SByteKind);
            var byteValue = default(ByteKind);
            var int16Value = default(Int16Kind);
            var uint16Value = default(UInt16Kind);
            var int32Value = default(Int32Kind);
            var uint32Value = default(UInt32Kind);
            var int64Value = default(Int64Kind);
            var uint64Value = default(UInt64Kind);
            """);

        var script = VisitBlock(block);

        Assert.AreEqual(
            """
            {
              let sbyteValue = 0;
              let byteValue = 0;
              let int16Value = 0;
              let uint16Value = 0;
              let int32Value = 0;
              let uint32Value = 0;
              let int64Value = 0n;
              let uint64Value = 0n;
            }
            """.ReplaceLineEndings(),
            script.ReplaceLineEndings());
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_DefaultStringEnum_UsesDeclaredZeroMemberMapping()
    {
        var block = GetBlockOperation("var state = default(StringState);");

        var script = VisitBlock(block);

        Assert.AreEqual(
            """
            {
              let state = "none";
            }
            """.ReplaceLineEndings(),
            script.ReplaceLineEndings());
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_DefaultStringEnumWithoutZeroMember_ReportsMappingFailure()
    {
        var block = GetBlockOperation("var state = default(StringStateWithoutZero);");

        var exception = Assert.Throws<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "string enums require a declared zero-valued member mapping");
    }

    [TestMethod]
    public void Visit_DefaultReferenceConstrainedTypeParameter_ProducesNull()
    {
        var block = GetBlockOperation("T Local<T>() where T : class => default(T); var value = Local<string>();");
        var operation = block.DescendantsAndSelf().OfType<IDefaultValueOperation>().Single();

        var script = new SemanticWalker(true).VisitDefaultValue(operation, new SenseArgument())?.ToKnRECMAScript();

        Assert.AreEqual("null", script);
        _ = new Parser().ParseExpression(script!);
    }

    private static string VisitBlock(IBlockOperation block)
    {
        var first = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        var second = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(first);
        Assert.AreEqual(first, second);
        return first;
    }

    private static IBlockOperation GetBlockOperation(string body)
    {
        var source = $$"""
            using System.ComponentModel;
            using ECMAScript;

            public sealed class TestClass
            {
                public enum SByteKind : sbyte { None }
                public enum ByteKind : byte { None }
                public enum Int16Kind : short { None }
                public enum UInt16Kind : ushort { None }
                public enum Int32Kind : int { None }
                public enum UInt32Kind : uint { None }
                public enum Int64Kind : long { None }
                public enum UInt64Kind : ulong { None }

                [String]
                public enum StringState
                {
                    [Description("@#none")]
                    None = 0,
                    Ready = 1
                }

                [String]
                public enum StringStateWithoutZero
                {
                    Ready = 1
                }

                public void TestMethod()
                {
                    {{body}}
                }
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            assemblyName: "DefaultValueProtocolScenarios",
            syntaxTrees: [syntaxTree],
            references:
            [
                .. TestMetadataReferences.Net11,
                MetadataReference.CreateFromFile(typeof(ECMAScript.StringAttribute).Assembly.Location)
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "TestMethod");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
