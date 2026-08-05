using Acornima;
using Acornima.Ast;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerBoundOperationProtocolTests
{
    [TestMethod]
    public void Visit_ExternalModuleStaticMembers_UseNamedImportsForReadWriteAndCallContracts()
    {
        var block = GetBlockOperation("""
            using System;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class)]
                public sealed class ECMAScriptModuleAttribute(string import) : Attribute;

                [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
                public sealed class ECMAScriptNameAttribute(string name) : Attribute;
            }

            namespace Demo
            {
                [ECMAScript.ECMAScriptModule("./remote-state.mjs")]
                public static class RemoteState
                {
                    [ECMAScript.ECMAScriptName("current-value")]
                    public static int CurrentValue { get; set; }

                    [ECMAScript.ECMAScriptName("refresh-value")]
                    public static int Refresh(int seed) => seed;
                }

                public sealed class Consumer
                {
                    void TestMethod(int seed)
                    {
                        int current = RemoteState.CurrentValue;
                        RemoteState.CurrentValue = seed;
                        int refreshed = RemoteState.Refresh(seed);
                    }
                }
            }
            """);
        var argument = new SenseArgument(UseImportAliases: true);

        var script = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript();
        var imports = argument.FlushImportSpecifiers();

        Assert.IsNotNull(script);
        Assert.HasCount(1, imports);
        Assert.AreEqual("./remote-state.mjs", imports[0].Key);
        CollectionAssert.Contains(
            imports[0].Value.Select(GetImportedName).ToArray(),
            "current-value");
        CollectionAssert.Contains(
            imports[0].Value.Select(GetImportedName).ToArray(),
            "refresh-value");
        StringAssert.Contains(script, "seed", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_ExternalModuleDefaultExportMember_UsesDefaultImportProtocol()
    {
        var block = GetBlockOperation("""
            using System;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class)]
                public sealed class ECMAScriptModuleAttribute(string import) : Attribute;

                [AttributeUsage(AttributeTargets.Property)]
                public sealed class ECMAScriptNameAttribute(string name) : Attribute;
            }

            namespace Demo
            {
                [ECMAScript.ECMAScriptModule("./remote-default.mjs")]
                public static class RemoteState
                {
                    [ECMAScript.ECMAScriptName("default")]
                    public static int Value { get; }
                }

                public sealed class Consumer
                {
                    void TestMethod()
                    {
                        int value = RemoteState.Value;
                    }
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var script = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript();
        var imports = argument.FlushImportSpecifiers();

        Assert.IsNotNull(script);
        Assert.HasCount(1, imports);
        Assert.AreEqual("./remote-default.mjs", imports[0].Key);
        var specifier = imports[0].Value.OfType<ImportDefaultSpecifier>().Single();
        StringAssert.StartsWith(specifier.Local.Name, "i$", StringComparison.Ordinal);
        StringAssert.Contains(script, "let value = " + specifier.Local.Name, StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_ArrayRankAndIndexCarrierConversion_UseBoundRuntimeContracts()
    {
        var block = GetBlockOperation("""
            using System;

            sealed class BoundScenarios
            {
                void TestMethod(int[,] matrix, int offset)
                {
                    int rank = matrix.Rank;
                    Index fromStart = offset;
                    Index fromEnd = ^offset;
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let rank = 2", StringComparison.Ordinal);
        StringAssert.Contains(script, "_1e1b56e4e760a5d5(offset)", StringComparison.Ordinal);
        StringAssert.Contains(script, "_ce8b9229a41c8545(offset)", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_TryCastWithSideEffectingOperand_EvaluatesSourceOnceBeforeTypeCheck()
    {
        var block = GetBlockOperation("""
            sealed class BoundScenarios
            {
                void TestMethod()
                {
                    string? text = NextValue() as string;
                }

                static object? NextValue() => "ready";
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        Assert.AreEqual(1, CountOccurrences(script, "BoundScenarios.nextValue()"), script);
        StringAssert.Contains(script, "typeof", StringComparison.Ordinal);
        StringAssert.Contains(script, "?", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    private static int CountOccurrences(string value, string fragment)
    {
        var count = 0;
        var offset = 0;
        while ((offset = value.IndexOf(fragment, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += fragment.Length;
        }

        return count;
    }

    private static string GetImportedName(ImportDeclarationSpecifier specifier)
    {
        if (specifier is not ImportSpecifier named)
        {
            Assert.Fail("Expected a named import.");
            return string.Empty;
        }

        return named.Imported switch
        {
            Identifier identifier => identifier.Name,
            StringLiteral literal => literal.Value,
            _ => throw new AssertFailedException("Expected an identifier or string imported name.")
        };
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            "bound-operation-protocol.cs");
        var compilation = CSharpCompilation.Create(
            "SemanticWalkerBoundOperationProtocolTests_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "TestMethod");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
