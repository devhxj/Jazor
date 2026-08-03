using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerStringBuilderTests
{
    [TestMethod]
    public void Visit_ContentMutationPipeline_UsesStringBuilderImportContract()
    {
        var block = GetBlockOperation(
            """
            using System.Text;

            public static class StringBuilderScenarios
            {
                public static string Evaluate(string text, char[] destination)
                {
                    var builder = new StringBuilder(text, 8);
                    builder.Append(true);
                    builder.Append('|');
                    builder.Append(42);
                    builder.Append(text, 1, 2);
                    builder.Insert(1, "xy", 2);
                    builder[0] = 'R';
                    builder.CopyTo(0, destination, 1, 2);
                    builder.Remove(1, 2);
                    builder.Replace("a", "A", 0, builder.Length);
                    builder.Length = builder.Length + 2;
                    return builder.ToString(0, builder.Length);
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(1, imports, body);
        Assert.HasCount(12, imports["System/Text/StringBuilderModule.js"], body);
        StringAssert.Contains(body, "_8ddc5378f62c27cc(text, 8)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_dded353c61620d12(builder, true)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_a2ce7c5adfc1553c(builder, \"|\")", StringComparison.Ordinal);
        StringAssert.Contains(body, "_212b9738d2ea3b2d(builder, 42)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_643a38ba616afd42(builder, text, 1, 2)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_da897479d9bd6139(builder, 1, \"xy\", 2)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_a970d620cd814959(builder, 0, \"R\")", StringComparison.Ordinal);
        StringAssert.Contains(body, "_e7c76d547b84e1dd(builder, 0, destination, 1, 2)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_152bf60dc35a5bb6(builder, 1, 2)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_34859fdec187084f(builder, \"a\", \"A\", 0, builder.length)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_085925374c6d3abd(builder, builder.length + 2)", StringComparison.Ordinal);
        StringAssert.Contains(body, "return _4941946dde4f03f0(builder, 0, builder.length);", StringComparison.Ordinal);

        _ = new Parser().ParseScript("function verify(text, destination) " + body);
    }

    [TestMethod]
    public void Visit_CapacitySurface_UsesStatefulImportContract()
    {
        var block = GetBlockOperation(
            """
            using System.Text;

            public static class StringBuilderScenarios
            {
                public static int Evaluate(string text)
                {
                    var builder = new StringBuilder(2, 8);
                    builder.Append(text);
                    builder.AppendLine();
                    var expanded = builder.Capacity;
                    builder.Capacity = 7;
                    var ensured = builder.EnsureCapacity(8);
                    return builder.MaxCapacity + expanded + ensured;
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(1, imports, body);
        Assert.HasCount(7, imports["System/Text/StringBuilderModule.js"], body);
        StringAssert.Contains(body, "_f69cee28dea8bcdc(2, 8)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_2879b76db56f25fb(builder, text)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_35fe8bcf463e879b(builder)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_20274b0eadfc0539(builder)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_d58ab6215b243f4f(builder, 7)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_e957bcfaa166161c(builder, 8)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_32a883f2233e3134(builder)", StringComparison.Ordinal);

        _ = new Parser().ParseScript("function verify(text) " + body);
    }

    [TestMethod]
    public void Visit_BuilderEquals_UsesTheContentComparisonImportContract()
    {
        var block = GetBlockOperation(
            """
            using System.Text;

            public static class StringBuilderScenarios
            {
                public static bool Evaluate(StringBuilder left, StringBuilder? right)
                {
                    return left.Equals(right);
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(1, imports, body);
        Assert.HasCount(1, imports["System/Text/StringBuilderModule.js"], body);
        StringAssert.Contains(body, "return _843038bb92e97c63(left, right);", StringComparison.Ordinal);

        _ = new Parser().ParseScript("function verify(left, right) " + body);
    }

    [TestMethod]
    public void Visit_AppendJoinStringCarriers_UsesTypedImportContracts()
    {
        var block = GetBlockOperation(
            """
            using System;
            using System.Text;

            public static class StringBuilderScenarios
            {
                public static string Evaluate(
                    StringBuilder builder,
                    string?[] array,
                    ReadOnlySpan<string?> span,
                    ReadOnlySpan<object?> objectSpan)
                {
                    builder.AppendJoin(",", array);
                    builder.AppendJoin('|', array);
                    builder.AppendJoin("/", span);
                    builder.AppendJoin(':', span);
                    builder.AppendJoin(";", objectSpan);
                    builder.AppendJoin('!', objectSpan);
                    return builder.ToString();
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(1, imports, body);
        Assert.HasCount(6, imports["System/Text/StringBuilderModule.js"], body);
        StringAssert.Contains(body, "_6ceea7a4bfd233b6(builder, \",\", array)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_02a3ec9f0e91877f(builder, \"|\", array)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_035c615b56218700(builder, \"/\", span)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_08c4f86d45c8b851(builder, \":\", span)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_f4377679fddd51ad(builder, \";\", objectSpan)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_f9ca702aaa0e6322(builder, \"!\", objectSpan)", StringComparison.Ordinal);

        _ = new Parser().ParseScript("function verify(builder, array, span, objectSpan) " + body);
    }

    [TestMethod]
    public void Visit_DecimalCarrier_UsesExactStringImports()
    {
        var block = GetBlockOperation(
            """
            using System.Text;

            public static class StringBuilderScenarios
            {
                public static string Evaluate(StringBuilder builder, decimal appended, decimal inserted)
                {
                    builder.Append(appended);
                    builder.Insert(1, inserted);
                    return builder.ToString();
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(1, imports, body);
        Assert.HasCount(2, imports["System/Text/StringBuilderModule.js"], body);
        StringAssert.Contains(body, "_f07022820ca3881f(builder, appended)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_7244d40cd7bdaa7a(builder, 1, inserted)", StringComparison.Ordinal);

        _ = new Parser().ParseScript("function verify(builder, appended, inserted) " + body);
    }

	[TestMethod]
	public void Visit_FloatingPointOverloads_ReuseTheNumericStringContract()
	{
		var block = GetBlockOperation(
			"""
			using System.Text;

			public static class StringBuilderScenarios
			{
				public static string Evaluate(StringBuilder builder, float single, double floating)
				{
					builder.Append(single);
					builder.Append(floating);
					builder.Insert(1, single);
					builder.Insert(2, floating);
					return builder.ToString();
				}
			}
			""");

		var argument = new SenseArgument(UseImportAliases: true);
		var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

		Assert.IsNotNull(body);
		var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
		Assert.HasCount(1, imports, body);
		Assert.HasCount(4, imports["System/Text/StringBuilderModule.js"], body);
		StringAssert.Contains(body, "_ec1b541b6a274b24(builder, single)", StringComparison.Ordinal);
		StringAssert.Contains(body, "_817e46ee3d60bf66(builder, floating)", StringComparison.Ordinal);
		StringAssert.Contains(body, "_5fa422ae348735cc(builder, 1, single)", StringComparison.Ordinal);
		StringAssert.Contains(body, "_7e09aba586586854(builder, 2, floating)", StringComparison.Ordinal);

		_ = new Parser().ParseScript("function verify(builder, single, floating) " + body);
	}

	[TestMethod]
	public void Visit_ObjectCompositionOverloads_UseSharedRuntimeImports()
	{
		var block = GetBlockOperation(
			"""
			using System.Collections.Generic;
			using System.Text;

			public static class StringBuilderScenarios
			{
				public static string Evaluate(
					StringBuilder builder,
					object? value,
					object?[] values,
					IEnumerable<int> numbers)
				{
					builder.Append(value);
					builder.AppendJoin("|", values);
					builder.AppendJoin("-", numbers);
					builder.AppendJoin('/', values);
					builder.AppendJoin(':', numbers);
					builder.Insert(0, value);
					return builder.ToString();
				}
			}
			""");

		var argument = new SenseArgument(UseImportAliases: true);
		var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

		Assert.IsNotNull(body);
		var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
		Assert.HasCount(1, imports, body);
		Assert.HasCount(6, imports["System/Text/StringBuilderModule.js"], body);
		StringAssert.Contains(body, "_06379efa8addb10d(builder, value)", StringComparison.Ordinal);
		StringAssert.Contains(body, "_8bc8cc43c6d93195(builder, \"|\", values)", StringComparison.Ordinal);
		StringAssert.Contains(body, "_8d04089684a00c7b(builder, \"-\", numbers)", StringComparison.Ordinal);
		StringAssert.Contains(body, "_a5aab658026ac255(builder, \"/\", values)", StringComparison.Ordinal);
		StringAssert.Contains(body, "_3510fcab582042e0(builder, \":\", numbers)", StringComparison.Ordinal);
		StringAssert.Contains(body, "_463fe06f693b73f1(builder, 0, value)", StringComparison.Ordinal);
		_ = new Parser().ParseScript("function verify(builder, value, values, numbers) " + body);
	}

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "StringBuilderScenarios",
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
