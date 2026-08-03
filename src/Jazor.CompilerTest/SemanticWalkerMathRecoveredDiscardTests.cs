using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerMathRecoveredDiscardTests
{
    [TestMethod]
    public void Visit_FixedWidthDivRemBigMulAndBigIntegerConstruction_UseTypedContracts()
    {
        var block = GetBlockOperation(
            """
            using System;
            using System.Numerics;

            public static class MathScenarios
            {
                public static void Evaluate(
                    sbyte signed8,
                    byte unsigned8,
                    short signed16,
                    ushort unsigned16,
                    int signed32,
                    uint unsigned32,
                    long signed64,
                    ulong unsigned64)
                {
                    var pair8 = Math.DivRem(signed8, (sbyte)3);
                    var pairU8 = Math.DivRem(unsigned8, (byte)3);
                    var pair16 = Math.DivRem(signed16, (short)3);
                    var pairU16 = Math.DivRem(unsigned16, (ushort)3);
                    var pair32 = Math.DivRem(signed32, 3);
                    var pairU32 = Math.DivRem(unsigned32, 3u);
                    var pair64 = Math.DivRem(signed64, 3L);
                    var pairU64 = Math.DivRem(unsigned64, 3UL);
                    var quotient32 = Math.DivRem(signed32, 3, out var remainder32);
                    var quotient64 = Math.DivRem(signed64, 3L, out var remainder64);
                    var highSigned = Math.BigMul(signed64, -1L, out var lowSigned);
                    var highUnsigned = Math.BigMul(unsigned64, 2UL, out var lowUnsigned);
                    var product = uint.BigMul(unsigned32, 2u);
                    var fromInt = new BigInteger(signed32);
                    var fromUInt = new BigInteger(unsigned32);
                    var fromLong = new BigInteger(signed64);
                    var fromULong = new BigInteger(unsigned64);
                    var hash = fromLong.GetHashCode();
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(2, imports, body);
        Assert.HasCount(12, imports["System/MathModule.js"], body);
        Assert.HasCount(1, imports["System/Numerics/BigIntegerModule.js"], body);
        foreach (var exportName in new[]
        {
            "_e0661118fd9ce98d",
            "_09ec2eababe53085",
            "_f6eb115003bc623f",
            "_267e04d7693208d4",
            "_45a4ab35fd8b6be8",
            "_c8e57fe110813408",
            "_96f1b2c20bd2e40b",
            "_4d9536a1220a7365",
            "_2a90cb0f64781864",
            "_1961d3558bd76ea4",
            "_1f2b3fb549b0a774",
            "_99697fddb05f0646"
        })
        {
            StringAssert.Contains(body, exportName + "(", StringComparison.Ordinal);
        }

        StringAssert.Contains(body, "BigInt(unsigned32) * BigInt(2)", StringComparison.Ordinal);
        StringAssert.Contains(body, "let fromInt = BigInt(signed32);", StringComparison.Ordinal);
        StringAssert.Contains(body, "let fromUInt = BigInt(unsigned32);", StringComparison.Ordinal);
        StringAssert.Contains(body, "let fromLong = signed64;", StringComparison.Ordinal);
        StringAssert.Contains(body, "let fromULong = unsigned64;", StringComparison.Ordinal);
        StringAssert.Contains(body, "_fe64082374302a77(fromLong)", StringComparison.Ordinal);

        _ = new Parser().ParseScript("function verify(signed8, unsigned8, signed16, unsigned16, signed32, unsigned32, signed64, unsigned64) " + body);
    }

    [TestMethod]
    public void Visit_Binary64BoundaryOperations_UseSharedImportHelpers()
    {
        var block = GetBlockOperation(
            """
            using System;

            public static class Binary64Scenarios
            {
                public static void Evaluate(double value, double other)
                {
                    var rounded = double.Round(value);
                    var incremented = double.BitIncrement(value);
                    var decremented = double.BitDecrement(value);
                    var remainder = double.Ieee754Remainder(value, other);
                    var exponent = double.ILogB(value);
                    var mathRounded = Math.Round(value);
                    var mathIncremented = Math.BitIncrement(value);
                    var mathDecremented = Math.BitDecrement(value);
                    var mathRemainder = Math.IEEERemainder(value, other);
                    var mathExponent = Math.ILogB(value);
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(2, imports, body);
        Assert.HasCount(5, imports["System/DoubleModule.js"], body);
        Assert.HasCount(5, imports["System/MathModule.js"], body);
        foreach (var exportName in new[]
        {
            "_0bc6b7459346bc5f",
            "_a83d47e386f63de0",
            "_4ce9474a7b3b7534",
            "_092bc2bc891d33a8",
            "_48628732b1dc8ac9",
            "_6cd7f67f98eae0bc",
            "_655bd4d428ca20ea",
            "_bc28ec82e8385202",
            "_288c181b5d9cf968",
            "_51e4d6005e6e11ef"
        })
        {
            StringAssert.Contains(body, exportName + "(", StringComparison.Ordinal);
        }

        _ = new Parser().ParseScript("function verify(value, other) " + body);
    }

    [TestMethod]
    public void Visit_Binary32BoundaryOperations_UseFloatRoundedImportHelpers()
    {
        var block = GetBlockOperation(
            """
            public static class Binary32Scenarios
            {
                public static void Evaluate(float value, float other)
                {
                    var rounded = float.Round(value);
                    var incremented = float.BitIncrement(value);
                    var decremented = float.BitDecrement(value);
                    var remainder = float.Ieee754Remainder(value, other);
                    var exponent = float.ILogB(value);
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(1, imports, body);
        Assert.HasCount(5, imports["System/SingleModule.js"], body);
        foreach (var exportName in new[]
        {
            "_99c8e34b34aa762c",
            "_eac91380a48fb7bd",
            "_9840b2a560428b4a",
            "_e54bb5d6b1fb386d",
            "_390f9dfb01584a29"
        })
        {
            StringAssert.Contains(body, exportName + "(", StringComparison.Ordinal);
        }

        _ = new Parser().ParseScript("function verify(value, other) " + body);
    }

    [TestMethod]
    public void Visit_Binary16AdjacentOperations_UseHalfImportHelpers()
    {
        var block = GetBlockOperation(
            """
            using System;

            public static class Binary16Scenarios
            {
                public static void Evaluate(Half value)
                {
                    var incremented = Half.BitIncrement(value);
                    var decremented = Half.BitDecrement(value);
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(1, imports, body);
        Assert.HasCount(2, imports["System/HalfModule.js"], body);
        StringAssert.Contains(body, "_3bbda0fdee7bad1d(value)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_c976c1d81370babf(value)", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(value) " + body);
    }

    [TestMethod]
    public void Visit_FloatingRoundOverloads_UseWidthSpecificImportHelpers()
    {
        var block = GetBlockOperation(
            """
            using System;

            public static class FloatingRoundScenarios
            {
                public static void Evaluate(double doubleValue, float singleValue, Half halfValue)
                {
                    var doubleDigits = double.Round(doubleValue, 2);
                    var doubleMode = double.Round(doubleValue, MidpointRounding.AwayFromZero);
                    var doubleBoth = double.Round(doubleValue, 2, MidpointRounding.ToNegativeInfinity);
                    var singleDigits = float.Round(singleValue, 2);
                    var singleMode = float.Round(singleValue, MidpointRounding.AwayFromZero);
                    var singleBoth = float.Round(singleValue, 2, MidpointRounding.ToNegativeInfinity);
                    var halfDigits = Half.Round(halfValue, 2);
                    var halfMode = Half.Round(halfValue, MidpointRounding.AwayFromZero);
                    var halfBoth = Half.Round(halfValue, 2, MidpointRounding.ToNegativeInfinity);
                    var mathDigits = Math.Round(doubleValue, 2);
                    var mathMode = Math.Round(doubleValue, MidpointRounding.AwayFromZero);
                    var mathBoth = Math.Round(doubleValue, 2, MidpointRounding.ToNegativeInfinity);
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(4, imports, body);
        Assert.HasCount(3, imports["System/DoubleModule.js"], body);
        Assert.HasCount(3, imports["System/SingleModule.js"], body);
        Assert.HasCount(3, imports["System/HalfModule.js"], body);
        Assert.HasCount(3, imports["System/MathModule.js"], body);
        foreach (var exportName in new[]
        {
            "_b439595e3752c6a9", "_7aeacc68b27f02f7", "_6e429701c9779ef6",
            "_a0ef44092a5b0a96", "_34bdf4b36464daa4", "_b0f1294dc766b202",
            "_a977225c7ea195c2", "_a3bd625b8647d19e", "_df8d144bad4e8a0b",
            "_dab059b61a5b7428", "_a7f99c51d0db12b5", "_ef441dda2abcc022"
        })
        {
            StringAssert.Contains(body, exportName + "(", StringComparison.Ordinal);
        }

        _ = new Parser().ParseScript("function verify(doubleValue, singleValue, halfValue) " + body);
    }

    [TestMethod]
    public void Visit_BigIntegerConversions_PreserveCheckedAndCarrierErasedSemantics()
    {
        var block = GetBlockOperation(
            """
            using System;
            using System.Numerics;

            public static class BigIntegerConversionScenarios
            {
                public static void Evaluate(
                    byte unsigned8,
                    char character,
                    short signed16,
                    int signed32,
                    long signed64,
                    Int128 signed128,
                    sbyte signed8,
                    ushort unsigned16,
                    uint unsigned32,
                    ulong unsigned64,
                    UInt128 unsigned128,
                    float singleValue,
                    double doubleValue,
                    Half halfValue,
                    decimal decimalValue,
                    BigInteger value)
                {
                    BigInteger fromByte = unsigned8;
                    BigInteger fromChar = character;
                    BigInteger fromShort = signed16;
                    BigInteger fromInt = signed32;
                    BigInteger fromLong = signed64;
                    BigInteger fromInt128 = signed128;
                    BigInteger fromSByte = signed8;
                    BigInteger fromUShort = unsigned16;
                    BigInteger fromUInt = unsigned32;
                    BigInteger fromULong = unsigned64;
                    BigInteger fromUInt128 = unsigned128;
                    var floatCtor = new BigInteger(singleValue);
                    var doubleCtor = new BigInteger(doubleValue);
                    var decimalCtor = new BigInteger(decimalValue);
                    var fromFloat = (BigInteger)singleValue;
                    var fromDouble = (BigInteger)doubleValue;
                    var fromHalf = (BigInteger)halfValue;
                    var fromDecimal = (BigInteger)decimalValue;
                    var toByte = (byte)value;
                    var toChar = (char)value;
                    var toSByte = (sbyte)value;
                    var toShort = (short)value;
                    var toUShort = (ushort)value;
                    var toInt = (int)value;
                    var toUInt = (uint)value;
                    var toLong = (long)value;
                    var toULong = (ulong)value;
                    var toInt128 = (Int128)value;
                    var toUInt128 = (UInt128)value;
                    var toDecimal = (decimal)value;
                    var toDouble = (double)value;
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(1, imports, body);
        Assert.HasCount(19, imports["System/Numerics/BigIntegerModule.js"], body);
        foreach (var exportName in new[]
        {
            "_cfd2038efd505e1f",
            "_38c7caccfd5e120e",
            "_f715f85cc5dcfe92",
            "_c1afe3218f0f82f9",
            "_ac2920ee8216c023",
            "_9d2085a2aa8febea",
            "_c57fc79b767bf069",
            "_7c261f922cc43235",
            "_15fe350cf299c580",
            "_5958070a15559320",
            "_63d8cc7789144528",
            "_b2311568a6faa3b8",
            "_385437ecb9a2b10a",
            "_6043725cddf263dd",
            "_f8ae8a4213449843",
            "_8e505e0ce7efa99c",
            "_933b3164355c792a",
            "_c186238bc3a46d2b",
            "_212b6e60ce4e6836"
        })
        {
            StringAssert.Contains(body, exportName + "(", StringComparison.Ordinal);
        }

        StringAssert.Contains(body, "let fromByte = BigInt(unsigned8);", StringComparison.Ordinal);
        StringAssert.Contains(body, "let fromLong = signed64;", StringComparison.Ordinal);
        StringAssert.Contains(body, "let fromInt128 = signed128;", StringComparison.Ordinal);
        StringAssert.Contains(body, "let toDouble = Number(value);", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(unsigned8, character, signed16, signed32, signed64, signed128, signed8, unsigned16, unsigned32, unsigned64, unsigned128, singleValue, doubleValue, halfValue, decimalValue, value) " + body);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "MathScenarios",
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
