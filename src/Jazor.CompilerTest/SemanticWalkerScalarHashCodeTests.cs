using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerScalarHashCodeTests
{
    [TestMethod]
    public void Visit_ScalarHashCodes_UseInlineAndRuntimeMappingsByCarrierWidth()
    {
        var block = GetBlockOperation(
            """
            using System;

            public static class ScalarHashCodeScenarios
            {
                public static int[] Evaluate(
                    bool flag,
                    byte byteValue,
                    sbyte sbyteValue,
                    short shortValue,
                    ushort ushortValue,
                    int intValue,
                    uint uintValue,
                    char charValue,
                    long longValue,
                    ulong ulongValue,
                    Int128 int128Value,
                    UInt128 uint128Value,
                    Half halfValue,
                    float singleValue,
                    double doubleValue,
                    string text)
                {
                    return
                    [
                        flag.GetHashCode(),
                        byteValue.GetHashCode(),
                        sbyteValue.GetHashCode(),
                        shortValue.GetHashCode(),
                        ushortValue.GetHashCode(),
                        intValue.GetHashCode(),
                        uintValue.GetHashCode(),
                        charValue.GetHashCode(),
                        longValue.GetHashCode(),
                        ulongValue.GetHashCode(),
                        int128Value.GetHashCode(),
                        uint128Value.GetHashCode(),
                        halfValue.GetHashCode(),
                        singleValue.GetHashCode(),
                        doubleValue.GetHashCode(),
                        text.GetHashCode()
                    ];
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        AssertImport(imports, "System/Int64Module.js", "_a6f06b90e3618c16");
        AssertImport(imports, "System/UInt64Module.js", "_19d2adbbe01a8cf8");
        AssertImport(imports, "System/Int128Module.js", "_2de13ea6377940aa");
        AssertImport(imports, "System/UInt128Module.js", "_bd5a3a9523f573e7");
        AssertImport(imports, "System/HalfModule.js", "_f9dc2d5b5c5cdf31");
        AssertImport(imports, "System/SingleModule.js", "_96e065ea302b67da");
        AssertImport(imports, "System/DoubleModule.js", "_73dea7106d8085a6");
        AssertImport(imports, "System/StringModule.js", "_bccdd3f386a6fbbc");
        Assert.HasCount(8, imports);
        StringAssert.Contains(body, "flag ? 1 : 0", StringComparison.Ordinal);
        StringAssert.Contains(body, "uintValue | 0", StringComparison.Ordinal);
        StringAssert.Contains(body, "charValue.charCodeAt(0)", StringComparison.Ordinal);
        Assert.IsFalse(body.Contains("GetHashCode", StringComparison.Ordinal), body);

        _ = new Parser().ParseScript("function verify() " + body);
    }

	[TestMethod]
	public void Visit_ObjectHashCode_UsesRuntimeImportAndPreservesVirtualDispatchBoundary()
	{
		var block = GetBlockOperation(
			"""
			public sealed class CustomHash
			{
				public override int GetHashCode() => 713;
			}

			public static class ObjectHashCodeScenarios
			{
				public static int Evaluate(object value)
				{
					return value.GetHashCode();
				}
			}
			""");

		var argument = new SenseArgument(UseImportAliases: true);
		var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

		Assert.IsNotNull(body);
		var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
		AssertImport(imports, "System/ObjectModule.js", "_97891de43f43ceb4");
		Assert.HasCount(1, imports);
		StringAssert.Contains(body, "return _97891de43f43ceb4(value);", StringComparison.Ordinal);
		_ = new Parser().ParseScript("function verify(value) " + body);
	}

    private static void AssertImport(
        IReadOnlyDictionary<string, Acornima.Ast.NodeList<Acornima.Ast.ImportDeclarationSpecifier>> imports,
        string modulePath,
        string exportName)
    {
        Assert.IsTrue(imports.TryGetValue(modulePath, out var specifiers), modulePath);
        CollectionAssert.AreEqual(new[] { exportName }, specifiers.Select(static specifier => specifier.ToECMAScript()).ToArray());
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "ScalarHashCodeScenarios",
            [syntaxTree],
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "Evaluate");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
