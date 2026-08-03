using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerStringRecoveredDiscardTests
{
	[TestMethod]
	public void Visit_Intern_UsesTheErasedStringCarrierImportContract()
	{
		var block = GetBlockOperation(
			"""
			public static class StringScenarios
			{
				public static string Evaluate(string value)
				{
					return string.Intern(value);
				}
			}
			""");

		var argument = new SenseArgument(UseImportAliases: true);
		var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

		Assert.IsNotNull(body);
		var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
		Assert.HasCount(1, imports, body);
		Assert.HasCount(1, imports["System/StringModule.js"], body);
		StringAssert.Contains(body, "return _1234444e218b96c3(value);", StringComparison.Ordinal);

		_ = new Parser().ParseScript("function verify(value) " + body);
	}

    [TestMethod]
    public void Visit_OrdinalRangeAndCharacterCopy_UseStringRuntimeImports()
    {
        var block = GetBlockOperation(
            """
            public static class StringScenarios
            {
                public static int Evaluate(string left, string right, char[] destination)
                {
                    var comparison = string.CompareOrdinal(left, 1, right, 2, 3);
                    left.CopyTo(0, destination, 1, 2);
                    return comparison;
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(1, imports, body);
        Assert.HasCount(2, imports["System/StringModule.js"], body);
        StringAssert.Contains(body, "_dc789454b6ef6bcb(left, 1, right, 2, 3)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_45bb6097c28a2f1e(left, 0, destination, 1, 2)", StringComparison.Ordinal);

        _ = new Parser().ParseScript("function verify(left, right, destination) " + body);
    }

    [TestMethod]
    public void Visit_OrdinalStringHashCodes_UseTypedRuntimeImports()
    {
        var block = GetBlockOperation(
            """
            using System;

            public static class StringScenarios
            {
                public static int Evaluate(string value, ReadOnlySpan<char> characters)
                {
                    return value.GetHashCode(StringComparison.Ordinal) + string.GetHashCode(characters, StringComparison.Ordinal);
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(1, imports, body);
        Assert.HasCount(2, imports["System/StringModule.js"], body);
        StringAssert.Contains(body, "_04edfc3090710ca7(value, 4)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_d123047f69d911f5(characters, 4)", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(value, characters) " + body);
    }

	[TestMethod]
	public void Visit_ObjectStringComposition_UsesSharedTypedRuntimeImports()
	{
		var block = GetBlockOperation(
			"""
			using System;
			using System.Collections.Generic;

			public static class StringScenarios
			{
				public static string Evaluate(string value, object? item, object?[] items, ReadOnlySpan<object?> span, IEnumerable<int> numbers)
				{
					return string.Copy(value)
						+ string.Concat(item)
						+ string.Concat(items)
						+ string.Concat(span)
						+ string.Concat(numbers)
						+ string.Join("|", items)
						+ string.Join("|", span)
						+ string.Join("-", numbers)
						+ string.Join('/', items)
						+ string.Join('/', span)
						+ string.Join(':', numbers);
				}
			}
			""");

		var argument = new SenseArgument(UseImportAliases: true);
		var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

		Assert.IsNotNull(body);
		var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
		Assert.HasCount(1, imports, body);
		Assert.HasCount(11, imports["System/StringModule.js"], body);
		StringAssert.Contains(body, "_0dc0a16fd99401f8(value)", StringComparison.Ordinal);
		StringAssert.Contains(body, "_db938b9c2eb90d32(item)", StringComparison.Ordinal);
		StringAssert.Contains(body, "_e102498b82e5b869(items)", StringComparison.Ordinal);
		StringAssert.Contains(body, "_2d6a291b64a11ba3(span)", StringComparison.Ordinal);
		StringAssert.Contains(body, "_68574aee669f440f(numbers)", StringComparison.Ordinal);
		StringAssert.Contains(body, "_c69ae51b8f3b72f0(\"|\", items)", StringComparison.Ordinal);
		StringAssert.Contains(body, "_f8903c473c9e5f05(\"|\", span)", StringComparison.Ordinal);
		StringAssert.Contains(body, "_c78854b22e947a4f(\"-\", numbers)", StringComparison.Ordinal);
		StringAssert.Contains(body, "_5ac0762c6816a423(\"/\", items)", StringComparison.Ordinal);
		StringAssert.Contains(body, "_477a1f45d63f93c2(\"/\", span)", StringComparison.Ordinal);
		StringAssert.Contains(body, "_1c599eccbbc8f2b8(\":\", numbers)", StringComparison.Ordinal);
		_ = new Parser().ParseScript("function verify(value, item, items, span, numbers) " + body);
	}

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "StringScenarios",
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
