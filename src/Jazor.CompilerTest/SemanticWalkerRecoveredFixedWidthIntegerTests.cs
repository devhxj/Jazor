using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerRecoveredFixedWidthIntegerTests
{
    [TestMethod]
    public void Visit_ScalarDefaultsAndFixedWidthBigIntMembers_UseInlineAndImportContracts()
    {
        var block = GetBlockOperation(
            """
            using System;
            using System.Numerics;

            public static class FixedWidthIntegerScenarios
            {
                public static void Evaluate(long signed, ulong unsigned, Int128 wideSigned, UInt128 wideUnsigned)
                {
                    var defaultHalf = new Half();
                    var defaultInt = new int();
                    var defaultLong = new long();
                    var defaultUInt128 = new UInt128();
                    var signedLeading = long.LeadingZeroCount(signed);
                    var signedLog = long.Log2(signed);
                    var unsignedPair = ulong.DivRem(unsigned, 3);
                    var unsignedLeading = ulong.LeadingZeroCount(unsigned);
                    var unsignedBits = ulong.PopCount(unsigned);
                    var unsignedLeft = ulong.RotateLeft(unsigned, 7);
                    var unsignedRight = ulong.RotateRight(unsigned, 7);
                    var unsignedTrailing = ulong.TrailingZeroCount(unsigned);
                    var wideSignedLog = Int128.Log2(wideSigned);
                    var wideUnsignedLog = UInt128.Log2(wideUnsigned);
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(3, imports, body);
        Assert.HasCount(2, imports["System/Int64Module.js"], body);
        Assert.HasCount(6, imports["System/UInt64Module.js"], body);
        Assert.HasCount(1, imports["System/Int128Module.js"], body);
        StringAssert.Contains(body, "let defaultHalf = 0;", StringComparison.Ordinal);
        StringAssert.Contains(body, "let defaultInt = 0;", StringComparison.Ordinal);
        StringAssert.Contains(body, "let defaultLong = 0n;", StringComparison.Ordinal);
        StringAssert.Contains(body, "let defaultUInt128 = 0n;", StringComparison.Ordinal);
        StringAssert.Contains(body, "BigInt(wideUnsigned.toString(2).length - 1)", StringComparison.Ordinal);

        _ = new Parser().ParseScript("function verify(signed, unsigned, wideSigned, wideUnsigned) " + body);
    }

    [TestMethod]
    public void Visit_Int128CheckedArithmeticAndSpanParsing_UseWidthSpecificImports()
    {
        var block = GetBlockOperation(
            """
            using System;
            using System.Numerics;

            public static class FixedWidthIntegerScenarios
            {
                public static void Evaluate(
                    Int128 signedLeft,
                    Int128 signedRight,
                    UInt128 unsignedLeft,
                    UInt128 unsignedRight,
                    ReadOnlySpan<char> text)
                {
                    Int128.TryParse(text, out var signedParsed);
                    var signedProviderParsed = Int128.Parse(text, null);
                    Int128.TryParse(text, null, out var signedProviderResult);
                    var signedAdd = checked(signedLeft + signedRight);
                    var signedSubtract = checked(signedLeft - signedRight);
                    var signedMultiply = checked(signedLeft * signedRight);
                    var signedDivide = checked(signedLeft / signedRight);
                    var signedNegate = checked(-signedLeft);
                    checked { signedLeft++; signedRight--; }

                    UInt128.TryParse(text, out var unsignedParsed);
                    var unsignedProviderParsed = UInt128.Parse(text, null);
                    UInt128.TryParse(text, null, out var unsignedProviderResult);
                    var unsignedAdd = checked(unsignedLeft + unsignedRight);
                    var unsignedSubtract = checked(unsignedLeft - unsignedRight);
                    var unsignedMultiply = checked(unsignedLeft * unsignedRight);
                    var unsignedDivide = checked(unsignedLeft / unsignedRight);
                    var unsignedNegate = checked(-unsignedLeft);
                    checked { unsignedLeft++; unsignedRight--; }
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(2, imports, body);
        Assert.HasCount(10, imports["System/Int128Module.js"], body);
        Assert.HasCount(10, imports["System/UInt128Module.js"], body);
        foreach (var exportName in new[]
        {
            "_b0e356aabfe72ec2", "_4d90655f04c3cb26", "_18dfb394fe14fa70",
            "_5e6d45782cb5e4a5", "_bce2a2f696e0d716", "_056e8fba577b7eeb",
            "_830753b6d4a84cc4", "_9f88084238b2cecc", "_6dacb4c587ca3df1", "_1b31f1ebb654733d",
            "_4d3bd14dc2810a3c", "_c88639ae1d5401bd", "_76b9708fc50ff818",
            "_c754a5da22221b5c", "_9b4d82822297f055", "_7b7dc120501d3144",
            "_b0d1618f64eba0cd", "_86264fa0bd6d25be", "_cf08bccf56129f82", "_2570268944e834ba"
        })
        {
            StringAssert.Contains(body, exportName + "(", StringComparison.Ordinal);
        }

        _ = new Parser().ParseScript("function verify(signedLeft, signedRight, unsignedLeft, unsignedRight, text) " + body);
    }

    [TestMethod]
    public void Visit_Int128IntegralDecimalAndDoubleConversions_PreserveCheckedAndWrappingSemantics()
    {
        var block = GetBlockOperation(
            """
            using System;
            using System.Numerics;

            public static class FixedWidthIntegerScenarios
            {
                public static void Evaluate(
                    Int128 signed,
                    UInt128 unsigned,
                    BigInteger arbitrary,
                    decimal decimalValue,
                    byte unsigned8,
                    sbyte signed8,
                    short signed16,
                    ushort unsigned16,
                    int signed32,
                    uint unsigned32,
                    long signed64,
                    ulong unsigned64)
                {
                    var signedByte = (byte)signed;
                    var signedCheckedByte = checked((byte)signed);
                    var signedShort = (short)signed;
                    var signedCheckedShort = checked((short)signed);
                    var signedInt = (int)signed;
                    var signedCheckedInt = checked((int)signed);
                    var signedLong = (long)signed;
                    var signedCheckedLong = checked((long)signed);
                    var signedSByte = (sbyte)signed;
                    var signedCheckedSByte = checked((sbyte)signed);
                    var signedUShort = (ushort)signed;
                    var signedCheckedUShort = checked((ushort)signed);
                    var signedUInt = (uint)signed;
                    var signedCheckedUInt = checked((uint)signed);
                    var signedULong = (ulong)signed;
                    var signedCheckedULong = checked((ulong)signed);
                    var signedUInt128 = (UInt128)signed;
                    var signedCheckedUInt128 = checked((UInt128)signed);
                    var signedDecimal = (decimal)signed;
                    var signedDouble = (double)signed;
                    var signedSingle = (float)signed;
                    var signedFromDecimal = (Int128)decimalValue;
                    Int128 signedFromByte = unsigned8;
                    Int128 signedFromSByte = signed8;
                    Int128 signedFromShort = signed16;
                    Int128 signedFromUShort = unsigned16;
                    Int128 signedFromInt = signed32;
                    Int128 signedFromUInt = unsigned32;
                    Int128 signedFromLong = signed64;
                    Int128 signedFromULong = unsigned64;

                    var unsignedByte = (byte)unsigned;
                    var unsignedCheckedByte = checked((byte)unsigned);
                    var unsignedShort = (short)unsigned;
                    var unsignedCheckedShort = checked((short)unsigned);
                    var unsignedInt = (int)unsigned;
                    var unsignedCheckedInt = checked((int)unsigned);
                    var unsignedLong = (long)unsigned;
                    var unsignedCheckedLong = checked((long)unsigned);
                    var unsignedInt128 = (Int128)unsigned;
                    var unsignedCheckedInt128 = checked((Int128)unsigned);
                    var unsignedSByte = (sbyte)unsigned;
                    var unsignedCheckedSByte = checked((sbyte)unsigned);
                    var unsignedUShort = (ushort)unsigned;
                    var unsignedCheckedUShort = checked((ushort)unsigned);
                    var unsignedUInt = (uint)unsigned;
                    var unsignedCheckedUInt = checked((uint)unsigned);
                    var unsignedULong = (ulong)unsigned;
                    var unsignedCheckedULong = checked((ulong)unsigned);
                    var unsignedDecimal = (decimal)unsigned;
                    var unsignedDouble = (double)unsigned;
                    var unsignedSingle = (float)unsigned;
                    var arbitrarySingle = (float)arbitrary;
                    var unsignedFromDecimal = (UInt128)decimalValue;
                    var unsignedFromShort = (UInt128)signed16;
                    var unsignedCheckedFromShort = checked((UInt128)signed16);
                    var unsignedFromInt = (UInt128)signed32;
                    var unsignedCheckedFromInt = checked((UInt128)signed32);
                    var unsignedFromLong = (UInt128)signed64;
                    var unsignedCheckedFromLong = checked((UInt128)signed64);
                    var unsignedFromSByte = (UInt128)signed8;
                    var unsignedCheckedFromSByte = checked((UInt128)signed8);
                    UInt128 unsignedFromByte = unsigned8;
                    UInt128 unsignedFromUShort = unsigned16;
                    UInt128 unsignedFromUInt = unsigned32;
                    UInt128 unsignedFromULong = unsigned64;
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(2, imports, body);
        Assert.HasCount(11, imports["System/Int128Module.js"], body);
        Assert.HasCount(15, imports["System/UInt128Module.js"], body);
        foreach (var exportName in new[]
        {
            "_75b77707d8797fe4", "_9e21259a765be818", "_2f789a7c53d14d8c",
            "_93c11f1447efb175", "_4d6353a3d3f19b88", "_d08bfb41d3ab6ee2",
            "_304df15d6a44df74", "_0ad5d1d4d4f5f677", "_0c7f2cd86870d034",
            "_d9f967e451f57e1b", "_ee13322cacfa030d",
            "_64e60de5b1e03760", "_cfc7a729e04a71ab", "_5efef087d1235b8b",
            "_ab4813fe5941ad49", "_191ebf43930db2a5", "_c572f7b29eaf324c",
            "_95c576d9e4841566", "_b68ba902309cfb9a", "_4b86a17a8f47b33f",
            "_b7d11ef0703deabf", "_7a73b169cb4a8694", "_958e84ffc74ece86",
            "_06d213d11ddf681c", "_1ef649fc443738a2", "_8366585a071ba8b1"
        })
        {
            StringAssert.Contains(body, exportName + "(", StringComparison.Ordinal);
        }

        StringAssert.Contains(body, "BigInt.asUintN(8, signed)", StringComparison.Ordinal);
        StringAssert.Contains(body, "BigInt.asIntN(128, unsigned)", StringComparison.Ordinal);
        StringAssert.Contains(body, "Number(signed)", StringComparison.Ordinal);
        StringAssert.Contains(body, "Number(unsigned)", StringComparison.Ordinal);
		StringAssert.Contains(body, "Math.fround(Number(signed))", StringComparison.Ordinal);
		StringAssert.Contains(body, "Math.fround(Number(unsigned))", StringComparison.Ordinal);
		StringAssert.Contains(body, "Math.fround(Number(arbitrary))", StringComparison.Ordinal);
		_ = new Parser().ParseScript("function verify(signed, unsigned, arbitrary, decimalValue, unsigned8, signed8, signed16, unsigned16, signed32, unsigned32, signed64, unsigned64) " + body);
    }

	[TestMethod]
	public void Visit_Int128Log10AndBigMul_UseExactFixedWidthImportsAndOutProtocol()
	{
		var block = GetBlockOperation(
			"""
			using System;

			public static class FixedWidthIntegerScenarios
			{
				public static void Evaluate(Int128 signedLeft, Int128 signedRight, UInt128 unsignedLeft, UInt128 unsignedRight)
				{
					var signedDigits = Int128.Log10(signedLeft);
					var signedHigh = Int128.BigMul(signedLeft, signedRight, out var signedLow);
					var unsignedDigits = UInt128.Log10(unsignedLeft);
					var unsignedHigh = UInt128.BigMul(unsignedLeft, unsignedRight, out var unsignedLow);
				}
			}
			""");

		var argument = new SenseArgument(UseImportAliases: true);
		var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

		Assert.IsNotNull(body);
		var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
		Assert.HasCount(2, imports, body);
		Assert.HasCount(2, imports["System/Int128Module.js"], body);
		Assert.HasCount(2, imports["System/UInt128Module.js"], body);
		foreach (var exportName in new[] { "_f729da8a5282b658", "_d32138c04ddcda2e", "_4ae42163ca5ab057", "_08f69578289009db" })
			StringAssert.Contains(body, exportName + "(", StringComparison.Ordinal);
		StringAssert.Contains(body, "signedLow = ", StringComparison.Ordinal);
		StringAssert.Contains(body, "unsignedLow = ", StringComparison.Ordinal);

		_ = new Parser().ParseScript("function verify(signedLeft, signedRight, unsignedLeft, unsignedRight) " + body);
	}

	[TestMethod]
	public void Visit_FixedWidthIntegerReadOnlyCharSpanTryParse_UsesPerWidthImportsAndOutProtocol()
	{
		var block = GetBlockOperation(
			"""
			using System;

			public static class FixedWidthIntegerScenarios
			{
				public static void Evaluate(ReadOnlySpan<char> text)
				{
					byte.TryParse(text, out var byteValue);
					sbyte.TryParse(text, out var sbyteValue);
					short.TryParse(text, out var shortValue);
					ushort.TryParse(text, out var ushortValue);
					int.TryParse(text, out var intValue);
					uint.TryParse(text, out var uintValue);
					long.TryParse(text, out var longValue);
					ulong.TryParse(text, out var ulongValue);
				}
			}
			""");

		var argument = new SenseArgument(UseImportAliases: true);
		var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

		Assert.IsNotNull(body);
		var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
		Assert.HasCount(8, imports, body);
		foreach (var (module, exportName) in new[]
		{
			("System/ByteModule.js", "_413c6f7752002edf"),
			("System/SByteModule.js", "_a3ccaa03549862bc"),
			("System/Int16Module.js", "_f06bf367c8a26691"),
			("System/UInt16Module.js", "_0103a8bec9e9dfd7"),
			("System/Int32Module.js", "_f6a664534980b0f4"),
			("System/UInt32Module.js", "_104b334d48c2aecd"),
			("System/Int64Module.js", "_f65dcae3cb8d9ffc"),
			("System/UInt64Module.js", "_6563986efd5413c0")
		})
		{
			Assert.HasCount(1, imports[module], module);
			StringAssert.Contains(body, exportName + "(text, ", StringComparison.Ordinal);
		}

		_ = new Parser().ParseScript("function verify(text) " + body);
	}

	[TestMethod]
	public void Visit_FloatingPointAndBigIntegerReadOnlyCharSpanTryParse_UseTypedOutImports()
	{
		var block = GetBlockOperation(
			"""
			using System;
			using System.Numerics;

			public static class FixedWidthIntegerScenarios
			{
				public static void Evaluate(ReadOnlySpan<char> text)
				{
					Half.TryParse(text, out var halfValue);
					float.TryParse(text, out var singleValue);
					double.TryParse(text, out var doubleValue);
					BigInteger.TryParse(text, out var bigIntegerValue);
				}
			}
			""");

		var argument = new SenseArgument(UseImportAliases: true);
		var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

		Assert.IsNotNull(body);
		var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
		foreach (var (module, exportName) in new[]
		{
			("System/HalfModule.js", "_f5bea48e2d45cf92"),
			("System/SingleModule.js", "_8f337f9f610204bb"),
			("System/DoubleModule.js", "_059799e0a3b763c1"),
			("System/Numerics/BigIntegerModule.js", "_ded03bf84977945f")
		})
		{
			Assert.HasCount(1, imports[module], module);
			StringAssert.Contains(body, exportName + "(text, ", StringComparison.Ordinal);
		}

		_ = new Parser().ParseScript("function verify(text) " + body);
	}

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "FixedWidthIntegerScenarios",
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
