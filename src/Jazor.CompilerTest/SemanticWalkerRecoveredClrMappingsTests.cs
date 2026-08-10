using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerRecoveredClrMappingsTests
{
    [TestMethod]
    public void Visit_CarrierStableMembers_EmitInlineClrSemantics()
    {
        var block = GetBlockOperation(
            """
            using System;
            using System.Collections;

            public static class RecoveredClrScenarios
            {
                public static void Evaluate(
                    Array array,
                    ICollection collection,
                    bool flag,
                    char character,
                    string text,
                    string? left,
                    string? right,
                    WeakReference weakReference)
                {
                    var rank = array.Rank;
                    var arrayRoot = array.SyncRoot;
                    var fixedSize = array.IsFixedSize;
                    array.Initialize();
                    var collectionRoot = collection.SyncRoot;
                    var synchronized = collection.IsSynchronized;
                    var booleanText = flag.ToString(null);
                    var characterText = character.ToString(null);
                    var objectEquals = text.Equals((object?)left);
                    var valueEquals = text.Equals(right);
                    var staticEquals = string.Equals(left, right);
                    var clone = text.Clone();
                    var stringText = text.ToString(null);
                    var concat = string.Concat(left, right);
                    var tracksResurrection = weakReference.TrackResurrection;
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        Assert.HasCount(0, argument.FlushImportSpecifiers());
        StringAssert.Contains(body, "let rank = 1;", StringComparison.Ordinal);
        StringAssert.Contains(body, "let arrayRoot = array;", StringComparison.Ordinal);
        StringAssert.Contains(body, "let fixedSize = true;", StringComparison.Ordinal);
        StringAssert.Contains(body, "void array.length;", StringComparison.Ordinal);
        StringAssert.Contains(body, "let collectionRoot = collection;", StringComparison.Ordinal);
        StringAssert.Contains(body, "let synchronized = false;", StringComparison.Ordinal);
        StringAssert.Contains(body, "flag ? \"True\" : \"False\"", StringComparison.Ordinal);
        StringAssert.Contains(body, "let characterText = character;", StringComparison.Ordinal);
        StringAssert.Contains(body, "let objectEquals = text === left;", StringComparison.Ordinal);
        StringAssert.Contains(body, "let valueEquals = text === right;", StringComparison.Ordinal);
        StringAssert.Contains(body, "let staticEquals = left === right;", StringComparison.Ordinal);
        StringAssert.Contains(body, "let clone = text;", StringComparison.Ordinal);
        StringAssert.Contains(body, "let stringText = text;", StringComparison.Ordinal);
        StringAssert.Contains(body, "(left ?? \"\") + (right ?? \"\")", StringComparison.Ordinal);
        StringAssert.Contains(body, "let tracksResurrection = false;", StringComparison.Ordinal);

        _ = new Parser().ParseScript("function verify() " + body);
    }

    [TestMethod]
    public void Visit_WeakReferenceTargetLifecycle_UsesSharedRuntimeImports()
    {
        var block = GetBlockOperation(
            """
            using System;

            public static class RecoveredClrScenarios
            {
                public static object? Evaluate(object initial, object? replacement)
                {
                    var reference = new WeakReference(initial);
                    var before = reference.Target;
                    var initiallyAlive = reference.IsAlive;
                    reference.Target = replacement;
                    return reference.Target;
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(1, imports, body);
        Assert.HasCount(4, imports["System/WeakReferenceModule.js"], body);
        StringAssert.Contains(body, "let reference = _9a41b3fc95053633(initial);", StringComparison.Ordinal);
        StringAssert.Contains(body, "let before = _ba77d80a1e80efa6(reference);", StringComparison.Ordinal);
        StringAssert.Contains(body, "let initiallyAlive = _c3d16f7de644412a(reference);", StringComparison.Ordinal);
        StringAssert.Contains(body, "_6576d2b2ae762786(reference, replacement);", StringComparison.Ordinal);
        StringAssert.Contains(body, "return _ba77d80a1e80efa6(reference);", StringComparison.Ordinal);

        _ = new Parser().ParseScript("function verify(initial, replacement) " + body);
    }

	[TestMethod]
	public void Visit_WeakReferenceConstructorWithoutResurrectionTracking_UsesRuntimeImport()
	{
		var block = GetBlockOperation(
			"""
			using System;

			public static class RecoveredClrScenarios
			{
				public static object? Evaluate(object initial)
				{
					return new WeakReference(initial, false).Target;
				}
			}
			""");

		var argument = new SenseArgument(UseImportAliases: true);
		var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

		Assert.IsNotNull(body);
		var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
		Assert.HasCount(1, imports, body);
		Assert.HasCount(2, imports["System/WeakReferenceModule.js"], body);
		StringAssert.Contains(body, "_bb3cf7219c9626be(initial, false)", StringComparison.Ordinal);
		StringAssert.Contains(body, "return _ba77d80a1e80efa6(", StringComparison.Ordinal);
		_ = new Parser().ParseScript("function verify(initial) " + body);
	}

    [TestMethod]
    public void Visit_LiveReadOnlyArrayViews_UseSharedRuntimeImports()
    {
        var block = GetBlockOperation(
            """
            using System;
            using System.Collections.Generic;
            using System.Collections.ObjectModel;

            public static class RecoveredClrScenarios
            {
                public static int Evaluate(List<int> source, int[] values)
                {
                    var fromList = source.AsReadOnly();
                    var fromArray = Array.AsReadOnly(values);
                    var fromConstructor = new ReadOnlyCollection<int>(source);
                    source.Add(42);
                    return fromList[0] + fromArray[0] + fromConstructor[0];
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(3, imports, body);
        Assert.HasCount(2, imports["System/Collections/Generic/ListT1Module.js"], body);
        Assert.HasCount(1, imports["System/ArrayModule.js"], body);
        Assert.HasCount(2, imports["System/Collections/ObjectModel/ReadOnlyCollectionT1Module.js"], body);
        foreach (var exportName in new[]
        {
            "_f7981b5a4cd02bdb", "_abd52ebcdb6fefcb", "_d4e5f6a7b8c9d0e1", "_b8c9d0e1f2a3b4c5"
        })
        {
            StringAssert.Contains(body, exportName + "(", StringComparison.Ordinal);
        }

        _ = new Parser().ParseScript("function verify(source, values) " + body);
    }

    [TestMethod]
    public void Visit_GenericIListIndexerSet_UsesValidatedRuntimeImport()
    {
        var block = GetBlockOperation(
            """
            using System.Collections.Generic;

            public static class RecoveredClrScenarios
            {
                public static void Evaluate(IList<int> values, int replacement)
                {
                    values[1] = replacement;
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(1, imports, body);
        Assert.HasCount(1, imports["System/Collections/Generic/IListT1Module.js"], body);
        StringAssert.Contains(body, "_72c3ada14c4b312e(values, 1, replacement);", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(values, replacement) " + body);
    }

    [TestMethod]
    public void Visit_ListInterfaces_UseMarkerBoundMutationImports()
    {
        var block = GetBlockOperation(
            """
            using System.Collections;
            using System.Collections.Generic;

            public static class RecoveredClrScenarios
            {
                public static void Evaluate(List<int> source, int value)
                {
                    IList<int> genericList = source;
                    ICollection<int> genericCollection = source;
                    IList list = source;

                    genericList.Insert(0, value);
                    genericList.RemoveAt(0);
                    genericCollection.Add(value);
                    genericCollection.Remove(value);
                    genericCollection.Clear();
                    _ = genericCollection.IsReadOnly;
                    list[0] = value;
                    _ = list.Add(value);
                    list.Clear();
                    _ = list.IsReadOnly;
                    _ = list.IsFixedSize;
                    list.Insert(0, value);
                    list.Remove(value);
                    list.RemoveAt(0);
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(3, imports, body);
        Assert.HasCount(2, imports["System/Collections/Generic/IListT1Module.js"], body);
        Assert.HasCount(4, imports["System/Collections/Generic/ICollectionT1Module.js"], body);
        Assert.HasCount(8, imports["System/Collections/IListModule.js"], body);
        foreach (var exportName in new[]
        {
            "_ad668b5fd142c4f4", "_d5f628d4cac6dafb", "_1257c5832793c86d", "_c0023f4a7a67220a",
            "_d067c092ac624f6a", "_0a859d3497130ea7", "_d1d1f177e5b9f8db", "_436bcdacebfc9159",
            "_00d8476a94b1a75c", "_2ce407a9d9be8186", "_b17a6c1583e0a5af", "_9e2711121aad1093",
            "_305c8313418aa043", "_72d07d6eb16afece"
        })
        {
            StringAssert.Contains(body, exportName + "(", StringComparison.Ordinal);
        }

        _ = new Parser().ParseScript("function verify(source, value) " + body);
    }

    [TestMethod]
    public void Visit_EnumerableToList_TransfersFreshArrayToMutableListCarrier()
    {
        var block = GetBlockOperation(
            """
            using System.Collections.Generic;
            using System.Linq;

            public static class RecoveredClrScenarios
            {
                public static List<int> Evaluate(IEnumerable<int> source)
                {
                    return source.ToList();
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(1, imports, body);
        Assert.HasCount(1, imports["System/RuntimeModule.js"], body);
        StringAssert.Contains(body, "return MarkAsMutableListCarrier(Array.from(__src));", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(source) " + body);
    }

    [TestMethod]
    public void Visit_LiveReadOnlyMapAndSetViews_UseSharedRuntimeImports()
    {
        var block = GetBlockOperation(
            """
            using System.Collections.Generic;
            using System.Collections.ObjectModel;

            public static class RecoveredClrScenarios
            {
                public static int Evaluate(Dictionary<string, int> dictionary, HashSet<int> set)
                {
                    var readOnlyDictionary = new ReadOnlyDictionary<string, int>(dictionary);
                    var readOnlySet = new ReadOnlySet<int>(set);
                    dictionary["release"] = 2;
                    set.Add(2);
                    var total = 0;
                    foreach (var key in readOnlyDictionary.Keys)
                        total += key.Length;
                    foreach (var value in readOnlyDictionary.Values)
                        total += value;
                    return total + readOnlyDictionary["release"] + (readOnlySet.Contains(2) ? 1 : 0);
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(4, imports, body);
        Assert.HasCount(1, imports["System/Collections/Generic/DictionaryT2Module.js"], body);
        Assert.HasCount(1, imports["System/Collections/Generic/HashSetT1Module.js"], body);
        Assert.HasCount(4, imports["System/Collections/ObjectModel/ReadOnlyDictionaryT2Module.js"], body);
        Assert.HasCount(1, imports["System/Collections/ObjectModel/ReadOnlySetT1Module.js"], body);
        foreach (var exportName in new[]
        {
            "_b22e987e1be225aa", "_aede400efbd05842", "_4044dececdd2d744", "_b39da265738457a5", "_ed4a7913b74bfd87"
        })
        {
            StringAssert.Contains(body, exportName + "(", StringComparison.Ordinal);
        }

        _ = new Parser().ParseScript("function verify(dictionary, set) " + body);
    }

    [TestMethod]
    public void Visit_CapacityErasedTrimAndReadOnlyFactories_UseRuntimeImports()
    {
        var block = GetBlockOperation(
            """
            using System;
            using System.Collections.Generic;
            using System.Collections.ObjectModel;

            public static class RecoveredClrScenarios
            {
                public static void Evaluate(
                    List<int> list,
                    Dictionary<int, int> dictionary,
                    HashSet<int> set,
                    ReadOnlySpan<int> values)
                {
                    list.TrimExcess();
                    dictionary.TrimExcess();
                    dictionary.TrimExcess(4);
                    set.TrimExcess();
                    set.TrimExcess(4);
                    var collection = ReadOnlyCollection.CreateCollection<int>(values);
                    var readOnlySet = ReadOnlyCollection.CreateSet<int>(values);
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(4, imports, body);
        foreach (var exportName in new[]
        {
            "_27c95e83eced65e9",
            "_44cc5aa04712525c", "_dd7fceb710b10915",
            "_09f9b6aba126decb", "_e4dd8faf507013ad",
            "_a0cccd63a3a3eee1", "_b80678a096dde585"
        })
        {
            StringAssert.Contains(body, exportName + "(", StringComparison.Ordinal);
        }

        _ = new Parser().ParseScript("function verify(list, dictionary, set, values) " + body);
    }

    [TestMethod]
    public void Visit_UnicodeCharacterClassification_UsesRuntimeImports()
    {
        var block = GetBlockOperation(
            """
            using System;

            public static class RecoveredClrScenarios
            {
                public static void Evaluate(char value, string text, int index)
                {
                    var number = char.IsNumber(value);
                    var indexedNumber = char.IsNumber(text, index);
                    var punctuation = char.IsPunctuation(text, index);
                    var separator = char.IsSeparator(value);
                    var indexedSeparator = char.IsSeparator(text, index);
                    var symbol = char.IsSymbol(value);
                    var indexedSymbol = char.IsSymbol(text, index);
                    var category = char.GetUnicodeCategory(value);
                    var indexedCategory = char.GetUnicodeCategory(text, index);
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(1, imports, body);
        foreach (var exportName in new[]
        {
            "_77e97c648607e65e", "_5180e5acb1d4bcb0", "_5f7e394ed1d09372",
            "_066fc76a18dc824f", "_3d391ade47da71a6", "_0f18b1b6d2524322",
            "_16587492d280e91d", "_226cc4ffd552fcf9", "_e41ad686bd01aff1"
        })
        {
            StringAssert.Contains(body, exportName + "(", StringComparison.Ordinal);
        }

        _ = new Parser().ParseScript("function verify(value, text, index) " + body);
    }

    [TestMethod]
    public void Visit_IndexAndRangeObjectEquality_UsesCarrierImports()
    {
        var block = GetBlockOperation(
            """
            using System;

            public static class RecoveredClrScenarios
            {
                public static void Evaluate(Index index, Range range, object candidate)
                {
                    var indexEquals = index.Equals(candidate);
                    var rangeEquals = range.Equals(candidate);
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(2, imports, body);
        StringAssert.Contains(body, "_2910b3afb47ad8b1(index, candidate)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_31b6c9a4877f04c4(range, candidate)", StringComparison.Ordinal);

        _ = new Parser().ParseScript("function verify(index, range, candidate) " + body);
    }

    [TestMethod]
    public void Visit_ExceptionCauseChain_UsesRuntimeImports()
    {
        var block = GetBlockOperation(
            """
            using System;

            public static class RecoveredClrScenarios
            {
                public static void Evaluate(Exception inner)
                {
                    var outer = new Exception("outer", inner);
                    var nested = outer.InnerException;
                    var root = outer.GetBaseException();
                    outer.HelpLink = "https://example.test/help";
                    var helpLink = outer.HelpLink;
                    outer.Source = "Jazor.App";
                    var source = outer.Source;
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        Assert.HasCount(1, argument.FlushImportSpecifiers(), body);
        StringAssert.Contains(body, "_553ffa41c7b954da(\"outer\", inner)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_463c6b2780b746af(outer)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_f062594f9ecd0366(outer)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_30c969b3bbd3fa2e(outer, \"https://example.test/help\")", StringComparison.Ordinal);
        StringAssert.Contains(body, "_cbc65d16d0767d67(outer)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_48095d5ec6492dcb(outer, \"Jazor.App\")", StringComparison.Ordinal);
        StringAssert.Contains(body, "_21e71d416a10c806(outer)", StringComparison.Ordinal);

        _ = new Parser().ParseScript("function verify(inner) " + body);
    }

    [TestMethod]
    public void Visit_DecimalOaCurrency_UsesExactRuntimeImports()
    {
        var block = GetBlockOperation(
            """
            public static class RecoveredClrScenarios
            {
                public static void Evaluate(long currency, decimal value)
                {
                    var decimalValue = decimal.FromOACurrency(currency);
                    var currencyValue = decimal.ToOACurrency(value);
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        Assert.HasCount(1, argument.FlushImportSpecifiers(), body);
        StringAssert.Contains(body, "_6cd0f8dfbedd7209(currency)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_5d257b5cc33cdaeb(value)", StringComparison.Ordinal);

        _ = new Parser().ParseScript("function verify(currency, value) " + body);
    }

    [TestMethod]
    public void Visit_FloatingNativeAndEstimateMembers_PreserveTargetSemantics()
    {
        var block = GetBlockOperation(
            """
            using System;

            public static class RecoveredClrScenarios
            {
                public static void Evaluate(
                    double d, double dmin, double dmax,
                    float f, float fmin, float fmax,
                    Half h, Half hmin, Half hmax)
                {
                    var dc = double.ClampNative(d, dmin, dmax);
                    var dmaxn = double.MaxNative(d, dmin);
                    var dminn = double.MinNative(d, dmax);
                    var dma = double.MultiplyAddEstimate(d, dmin, dmax);
                    var fc = float.ClampNative(f, fmin, fmax);
                    var fmaxn = float.MaxNative(f, fmin);
                    var fminn = float.MinNative(f, fmax);
                    var fma = float.MultiplyAddEstimate(f, fmin, fmax);
                    var hc = Half.ClampNative(h, hmin, hmax);
                    var hmaxn = Half.MaxNative(h, hmin);
                    var hminn = Half.MinNative(h, hmax);
                    var hma = Half.MultiplyAddEstimate(h, hmin, hmax);
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        Assert.HasCount(3, argument.FlushImportSpecifiers(), body);
        StringAssert.Contains(body, "_ead55aa3a172f045(d, dmin, dmax)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_e50ccb4182ec0a52(f, fmin, fmax)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_de3198267b6b5ced(h, hmin, hmax)", StringComparison.Ordinal);
        StringAssert.Contains(body, "d > dmin ? d : dmin", StringComparison.Ordinal);
        StringAssert.Contains(body, "d < dmax ? d : dmax", StringComparison.Ordinal);
        StringAssert.Contains(body, "Math.fround(f * fmin + fmax)", StringComparison.Ordinal);
        StringAssert.Contains(body, "Math.f16round(h * hmin + hmax)", StringComparison.Ordinal);

        _ = new Parser().ParseScript("function verify(d, dmin, dmax, f, fmin, fmax, h, hmin, hmax) " + body);
    }

    [TestMethod]
    public void Visit_Int128CharacterConversions_UseCheckedAndUncheckedWidths()
    {
        var block = GetBlockOperation(
            """
            using System;

            public static class RecoveredClrScenarios
            {
                public static void Evaluate(Int128 signed, UInt128 unsigned, char character)
                {
                    var signedUnchecked = unchecked((char)signed);
                    var signedChecked = checked((char)signed);
                    var unsignedUnchecked = unchecked((char)unsigned);
                    var unsignedChecked = checked((char)unsigned);
                    Int128 signedFromCharacter = character;
                    UInt128 unsignedFromCharacter = character;
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        Assert.HasCount(2, argument.FlushImportSpecifiers(), body);
        StringAssert.Contains(body, "Number(BigInt.asUintN(16, signed))", StringComparison.Ordinal);
        StringAssert.Contains(body, "_f452363cdf448dd6(signed)", StringComparison.Ordinal);
        StringAssert.Contains(body, "Number(BigInt.asUintN(16, unsigned))", StringComparison.Ordinal);
        StringAssert.Contains(body, "_b68867a4bbf792ed(unsigned)", StringComparison.Ordinal);
        StringAssert.Contains(body, "BigInt(character)", StringComparison.Ordinal);

        _ = new Parser().ParseScript("function verify(signed, unsigned, character) " + body);
    }

    [TestMethod]
    public void Visit_HalfConversions_UseBinary16AndIntegerWidthRuntimeImports()
    {
        var block = GetBlockOperation(
            """
            using System;

            public static class RecoveredClrScenarios
            {
                public static void Evaluate(
                    char character, double doubleValue, short int16, int int32, long int64,
                    float singleValue, ushort uint16, uint uint32, ulong uint64,
                    byte byteValue, sbyte sbyteValue, Half value)
                {
                    var fromCharacter = (Half)character;
                    var fromDouble = (Half)doubleValue;
                    var fromInt16 = (Half)int16;
                    var fromInt32 = (Half)int32;
                    var fromInt64 = (Half)int64;
                    var fromSingle = (Half)singleValue;
                    var fromUInt16 = (Half)uint16;
                    var fromUInt32 = (Half)uint32;
                    var fromUInt64 = (Half)uint64;
                    Half fromByte = byteValue;
                    Half fromSByte = sbyteValue;
                    var toByte = unchecked((byte)value);
                    var toCheckedByte = checked((byte)value);
                    var toCharacter = unchecked((char)value);
                    var toCheckedCharacter = checked((char)value);
                    var toInt16 = unchecked((short)value);
                    var toCheckedInt16 = checked((short)value);
                    var toInt32 = unchecked((int)value);
                    var toCheckedInt32 = checked((int)value);
                    var toInt64 = unchecked((long)value);
                    var toCheckedInt64 = checked((long)value);
                    var toInt128 = unchecked((Int128)value);
                    var toCheckedInt128 = checked((Int128)value);
                    var toSByte = unchecked((sbyte)value);
                    var toCheckedSByte = checked((sbyte)value);
                    var toUInt16 = unchecked((ushort)value);
                    var toCheckedUInt16 = checked((ushort)value);
                    var toUInt32 = unchecked((uint)value);
                    var toCheckedUInt32 = checked((uint)value);
                    var toUInt64 = unchecked((ulong)value);
                    var toCheckedUInt64 = checked((ulong)value);
                    var toUInt128 = unchecked((UInt128)value);
                    var toCheckedUInt128 = checked((UInt128)value);
                    var toDouble = (double)value;
                    var toSingle = (float)value;
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        Assert.HasCount(1, argument.FlushImportSpecifiers(), body);
        var importedFunctions = new[]
        {
            "_688015ce7a06d3a3", "_c15dbcdc3a5121a4", "_5235d3bf6d040ead", "_83d328837e0849f2",
            "_54cc35a643b3964a", "_c698784c1b652292", "_66978b13cd9c4d2c", "_5fe8cbd0191a1261",
            "_7cde86a6784147b9", "_b5ec2ce7adbc5cd7", "_e9ab5db75451afaa", "_4eda3983a0238fe6",
            "_17127d121cc23462", "_a51addf0541517b0", "_0ce814bef1ddcd6b", "_f3478913297420e6",
            "_a97f96a06c928768", "_b72c1f59dbe70e00", "_70697b238a197bc2", "_1d590a5b31b1ced4",
            "_b245ca9db3ecb868", "_24b890794cafdd5b", "_ad10a10a383b6b8c", "_0c7451f23f55d772",
            "_d68498a3229ff278", "_5506dadf5b952671", "_d7ccb4b5709ce4ea", "_6d14496c702de03c",
            "_8e635ebf316e6be7", "_368654d3a116fc21", "_8d52fe89e6ca9452", "_de1cee73a929bf8e",
            "_bd3cc1c48165dbab", "_0cce99536d7741bb", "_e5c3410a6fc7ae9a"
        };

        Assert.HasCount(35, importedFunctions);
        foreach (var function in importedFunctions)
        {
            StringAssert.Contains(body, function + "(", StringComparison.Ordinal);
        }

        _ = new Parser().ParseScript(
            "function verify(character, doubleValue, int16, int32, int64, singleValue, uint16, uint32, uint64, byteValue, sbyteValue, value) " + body);
    }

    [TestMethod]
    public void Visit_WideFloatingConversions_UseSharedBigIntRuntimeImports()
    {
        var block = GetBlockOperation(
            """
            using System;
            using System.Numerics;

            public static class RecoveredClrScenarios
            {
                public static void Evaluate(Int128 signed, UInt128 unsigned, BigInteger arbitrary, double doubleValue, float singleValue)
                {
                    var signedHalf = (Half)signed;
                    var unsignedHalf = (Half)unsigned;
                    var arbitraryHalf = (Half)arbitrary;
                    var signedDouble = unchecked((Int128)doubleValue);
                    var signedCheckedDouble = checked((Int128)doubleValue);
                    var signedSingle = unchecked((Int128)singleValue);
                    var signedCheckedSingle = checked((Int128)singleValue);
                    var unsignedDouble = unchecked((UInt128)doubleValue);
                    var unsignedCheckedDouble = checked((UInt128)doubleValue);
                    var unsignedSingle = unchecked((UInt128)singleValue);
                    var unsignedCheckedSingle = checked((UInt128)singleValue);
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        Assert.HasCount(3, argument.FlushImportSpecifiers(), body);
        foreach (var function in new[]
        {
            "_53c418af5874ca57", "_ebc69a5a022fe3e9", "_7c41bbf7746a0266",
            "_fed29180182d65ba", "_3d7c10f4becbee0b", "_f0c48afd1cde425d", "_1215d60b3aeb2477",
            "_8a2ad347ec233b35", "_5d464c2acf139edb", "_5ac67fecfe01fee0", "_dec2fe2225e51e70"
        })
        {
            StringAssert.Contains(body, function + "(", StringComparison.Ordinal);
        }

        _ = new Parser().ParseScript(
            "function verify(signed, unsigned, arbitrary, doubleValue, singleValue) " + body);
    }

	[TestMethod]
	public void Visit_Utf8NumericTryParse_UsesTypedRuntimeImportsAndOutProtocol()
	{
		var block = GetBlockOperation(
			"""
			using System;

			public static class RecoveredClrScenarios
			{
				public static void Evaluate(ReadOnlySpan<byte> utf8)
				{
					byte.TryParse(utf8, out var byteValue);
					sbyte.TryParse(utf8, out var sbyteValue);
					short.TryParse(utf8, out var int16Value);
					ushort.TryParse(utf8, out var uint16Value);
					int.TryParse(utf8, out var int32Value);
					uint.TryParse(utf8, out var uint32Value);
					long.TryParse(utf8, out var int64Value);
					ulong.TryParse(utf8, out var uint64Value);
					Int128.TryParse(utf8, out var int128Value);
					UInt128.TryParse(utf8, out var uint128Value);
					Half.TryParse(utf8, out var halfValue);
					float.TryParse(utf8, out var singleValue);
					double.TryParse(utf8, out var doubleValue);
					decimal.TryParse(utf8, out var decimalValue);
					decimal.Parse(utf8, System.Globalization.NumberStyles.Number, null);
					decimal.TryParse(utf8, System.Globalization.NumberStyles.Float, null, out var styledDecimalValue);
					decimal.Parse(utf8, null);
					decimal.TryParse(utf8, null, out var providedDecimalValue);
				}
			}
			""");

		var argument = new SenseArgument(UseImportAliases: true);
		var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

		Assert.IsNotNull(body);
		Assert.HasCount(14, argument.FlushImportSpecifiers(), body);
		foreach (var function in new[]
		{
			"_0e02bd74e5960e4d", "_f25602df99a7ca89", "_af732a8ac69b6f6e", "_f90ee83a31a4d447",
			"_2acff5418dba43bd", "_2526f7e27fec4657", "_8bee07df79eb3a90", "_908c702d612b8a82",
			"_b5211e33c4db2da9", "_6b11c1fbc39c3749", "_8ed5272b36771f32", "_35fa5333706d7ec4",
			"_ec88293b6cb03791", "_0111d7c27998205b"
		})
		{
			StringAssert.Contains(body, function + "(utf8", StringComparison.Ordinal);
		}
		foreach (var function in new[]
		{
			"_e81acb76373d457e", "_acbda6e104ca3de4", "_d3d821054d142668", "_8122c647766e18ff"
		})
		{
			StringAssert.Contains(body, function + "(utf8", StringComparison.Ordinal);
		}

		_ = new Parser().ParseScript("function verify(utf8) " + body);
	}

    [TestMethod]
    public void Visit_ReadOnlyCharacterSpanStringAndBuilderMembers_UseTypedRuntimeImports()
    {
        var block = GetBlockOperation(
            """
            using System;
            using System.Text;

            public static class RecoveredClrScenarios
            {
                public static void Evaluate(
                    string text,
                    ReadOnlySpan<char> left,
                    ReadOnlySpan<char> middle,
                    ReadOnlySpan<char> right,
                    StringBuilder builder)
                {
                    ReadOnlySpan<char> converted = text;
                    ReadOnlySpan<char> arrayBacked = new[] { 'J', 'S' };
                    var copy = new string(left);
                    var arrayCopy = new string(arrayBacked);
                    var hash = string.GetHashCode(left);
                    var arrayHash = string.GetHashCode(arrayBacked);
                    var pair = string.Concat(left, right);
                    var mixedPair = string.Concat(arrayBacked, right);
                    var triple = string.Concat(left, middle, right);
                    var quadruple = string.Concat(left, middle, right, converted);
                    var trimmedWhitespace = left.Trim();
                    var trimmedCharacter = left.Trim('x');
                    var trimmed = text.Trim(arrayBacked);
                    var trimmedStartWhitespace = left.TrimStart();
                    var trimmedStartCharacter = left.TrimStart('x');
                    var trimStart = text.TrimStart(arrayBacked);
                    var trimmedEndWhitespace = left.TrimEnd();
                    var trimmedEndCharacter = left.TrimEnd('x');
                    var trimEnd = text.TrimEnd(arrayBacked);
                    builder.Append(left);
                    builder.Append(arrayBacked);
                    builder.Insert(0, middle);
                    builder.Replace(left, right);
                    var equal = builder.Equals(right);
                    builder.Replace(left, right, 0, builder.Length);
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        Assert.HasCount(3, argument.FlushImportSpecifiers(), body);
        StringAssert.Contains(body, "let converted = text;", StringComparison.Ordinal);
        foreach (var function in new[]
        {
            "_009fee2e166a416d", "_4598a18be32f839d", "_a6102c27abe1ff18",
            "_7de0cfb062a343ee", "_5177ae056c5ca775", "_8c68c811d3d42bcf",
			"_ed1b69fd4bc25279", "_c7be232bff90ab62", "_251b340a59afa04d",
			"_5681048ad18a4b3f"
        })
        {
			StringAssert.Contains(body, function + "(", StringComparison.Ordinal);
		}
		foreach (var function in new[]
		{
			"trim", "trimCharacter", "trimCharacters",
			"trimStart", "trimStartCharacter", "trimStartCharacters",
			"trimEnd", "trimEndCharacter", "trimEndCharacters"
		})
		{
			StringAssert.Contains(body, function + "(", StringComparison.Ordinal);
		}

        _ = new Parser().ParseScript("function verify(text, left, middle, right, builder) " + body);
    }

    [TestMethod]
    public void Visit_RecoveredArraySpanAndWeakTableMembers_UseRuntimeImports()
    {
        var block = GetBlockOperation(
            """
            using System;
            using System.Runtime.CompilerServices;

            public static class RecoveredClrScenarios
            {
                public static void Evaluate(
                    Array array,
                    int[] integerIndices,
                    long[] longIndices,
                    object value,
                    string text,
                    ReadOnlySpan<char> trimChars,
                    ConditionalWeakTable<object, string> table)
                {
                    var integerValue = array.GetValue(integerIndices);
                    array.SetValue(value, integerIndices);
                    var longValue = array.GetValue(longIndices);
                    array.SetValue(value, longIndices);
                    var replaced = text.ReplaceLineEndings();
                    var trimmed = text.Trim(trimChars);
                    var trimmedStart = text.TrimStart(trimChars);
                    var trimmedEnd = text.TrimEnd(trimChars);
                    table.Clear();
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        Assert.HasCount(4, argument.FlushImportSpecifiers(), body);
        foreach (var function in new[]
        {
            "_e938260256ca4a08", "_8752076a83fbb3f1", "_6a12948779406121", "_e3923681669a96b5",
            "_3720e4de26fa4c1b", "_57912eda7fd377bb"
        })
        {
            StringAssert.Contains(body, function + "(", StringComparison.Ordinal);
        }
        // String's span overloads converge on the compiler-owned MemoryExtensions protocol.
        // Do not bypass that lowering merely to force the StringModule mapping at this call site.
        StringAssert.Contains(body, "trimCharacters(text, trimChars)", StringComparison.Ordinal);
        StringAssert.Contains(body, "trimStartCharacters(text, trimChars)", StringComparison.Ordinal);
        StringAssert.Contains(body, "trimEndCharacters(text, trimChars)", StringComparison.Ordinal);

        _ = new Parser().ParseScript(
            "function verify(array, integerIndices, longIndices, value, text, trimChars, table) " + body);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "RecoveredClrScenarios",
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
