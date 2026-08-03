using System.Reflection;
using ECMAScript.Contract;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class RecoveredDiscardWhitelistTests
{
	[TestMethod]
	public void ObjectStringMembers_UseSharedImportRuntime()
	{
		var stringMappings = GetMappings(typeof(Jazor.CLR.StringModule));
		foreach (var member in new[]
		{
			"static string.Copy(string)",
			"static string.Concat(object)",
			"static string.Concat(object, object)",
			"static string.Concat(object, object, object)",
			"static string.Concat(params object[])",
			"static string.Concat(params System.ReadOnlySpan<object>)",
			"static string.Concat<T>(System.Collections.Generic.IEnumerable<T>)",
			"static string.Join(string, params object[])",
			"static string.Join(string, params System.ReadOnlySpan<object>)",
			"static string.Join<T>(string, System.Collections.Generic.IEnumerable<T>)",
			"static string.Join(char, params object[])",
			"static string.Join(char, params System.ReadOnlySpan<object>)",
			"static string.Join<T>(char, System.Collections.Generic.IEnumerable<T>)"
		})
		{
			Assert.AreEqual(Op.Import, stringMappings[member]?.Op, member);
		}

		var builderMappings = GetMappings(typeof(Jazor.CLR.StringBuilderModule));
		foreach (var member in new[]
		{
			"System.Text.StringBuilder.Append(object)",
			"System.Text.StringBuilder.AppendJoin(string, params object[])",
			"System.Text.StringBuilder.AppendJoin(string, params System.ReadOnlySpan<object>)",
			"System.Text.StringBuilder.AppendJoin<T>(string, System.Collections.Generic.IEnumerable<T>)",
			"System.Text.StringBuilder.AppendJoin(char, params object[])",
			"System.Text.StringBuilder.AppendJoin(char, params System.ReadOnlySpan<object>)",
			"System.Text.StringBuilder.AppendJoin<T>(char, System.Collections.Generic.IEnumerable<T>)",
			"System.Text.StringBuilder.Insert(int, object)"
		})
		{
			Assert.AreEqual(Op.Import, builderMappings[member]?.Op, member);
		}
	}

	[TestMethod]
	public void LiveReadOnlyArrayViews_UseSharedRuntimeImports()
	{
		var cases = new (Type Module, string Member)[]
		{
			(typeof(Jazor.CLR.ArrayModule<>), "static System.Array.AsReadOnly<T>(T[])"),
			(typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>.AsReadOnly()"),
			(typeof(Jazor.CLR.ReadOnlyCollectionT1Module<>), "System.Collections.ObjectModel.ReadOnlyCollection<T>.ReadOnlyCollection(System.Collections.Generic.IList<T>)"),
			(typeof(Jazor.CLR.ReadOnlyDictionaryT2Module<,>), "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.ReadOnlyDictionary(System.Collections.Generic.IDictionary<TKey, TValue>)"),
			(typeof(Jazor.CLR.ReadOnlySetT1Module<>), "System.Collections.ObjectModel.ReadOnlySet<T>.ReadOnlySet(System.Collections.Generic.ISet<T>)")
		};

		foreach (var (module, member) in cases)
			Assert.AreEqual(Op.Import, GetMappings(module)[member]?.Op, member);
	}

	[TestMethod]
	public void ReadOnlyDictionaryKeyAndValueViews_UseRuntimeProjections()
	{
		var mappings = GetMappings(typeof(Jazor.CLR.ReadOnlyDictionaryT2Module<,>));
		foreach (var member in new[]
		{
			"System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Keys.get",
			"System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Values.get"
		})
		{
			Assert.AreEqual(Op.Import, mappings[member]?.Op, member);
		}
	}

    [TestMethod]
    public void EnumerableArrayLikeMembers_UseCompilerOwnedProtocol()
    {
        var members = new[]
        {
            "static System.Linq.Enumerable.Where<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)",
            "static System.Linq.Enumerable.Where<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int, bool>)",
            "static System.Linq.Enumerable.Select<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TResult>)",
            "static System.Linq.Enumerable.Select<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int, TResult>)",
            "static System.Linq.Enumerable.ToList<TSource>(System.Collections.Generic.IEnumerable<TSource>)",
            "static System.Linq.Enumerable.ToArray<TSource>(System.Collections.Generic.IEnumerable<TSource>)"
        };
        var mappings = typeof(Jazor.CLR.EnumerableModule<>)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(static method => method.GetCustomAttribute<JazorAttribute>())
            .Where(static attribute => attribute is not null)
            .ToDictionary(static attribute => attribute!.Member, StringComparer.Ordinal);

        foreach (var member in members)
        {
            Assert.IsTrue(mappings.TryGetValue(member, out var mapping), member);
            Assert.IsNotNull(mapping, member);
            Assert.AreEqual(Op.Compile, mapping.Op, member);
            Assert.AreEqual("EnumerableArrayLike", mapping.Value, member);
        }
    }

    [TestMethod]
    public void CarrierStableMembers_UseExplicitMappings()
    {
        var cases = new (Type Module, string Member, Op Op)[]
        {
            (typeof(Jazor.CLR.ArrayModule<>), "System.Array.Rank.get", Op.Inline),
            (typeof(Jazor.CLR.ArrayModule<>), "System.Array.SyncRoot.get", Op.Inline),
            (typeof(Jazor.CLR.ArrayModule<>), "System.Array.IsFixedSize.get", Op.Inline),
            (typeof(Jazor.CLR.ArrayModule<>), "System.Array.Initialize()", Op.Inline),
            (typeof(Jazor.CLR.ArrayModule<>), "System.Array.GetValue(params int[])", Op.Import),
            (typeof(Jazor.CLR.ArrayModule<>), "System.Array.SetValue(object, params int[])", Op.Import),
            (typeof(Jazor.CLR.ArrayModule<>), "System.Array.GetValue(params long[])", Op.Import),
            (typeof(Jazor.CLR.ArrayModule<>), "System.Array.SetValue(object, params long[])", Op.Import),
            (typeof(Jazor.CLR.ICollectionModule), "System.Collections.ICollection.SyncRoot.get", Op.Inline),
            (typeof(Jazor.CLR.ICollectionModule), "System.Collections.ICollection.IsSynchronized.get", Op.Inline),
            (typeof(Jazor.CLR.BooleanModule), "bool.ToString(System.IFormatProvider)", Op.Inline),
            (typeof(Jazor.CLR.CharModule), "char.Char()", Op.Inline),
            (typeof(Jazor.CLR.CharModule), "char.ToString(System.IFormatProvider)", Op.Inline),
            (typeof(Jazor.CLR.StringModule), "override string.Equals(object)", Op.Inline),
            (typeof(Jazor.CLR.StringModule), "string.Equals(string)", Op.Inline),
            (typeof(Jazor.CLR.StringModule), "static string.Equals(string, string)", Op.Inline),
            (typeof(Jazor.CLR.StringModule), "string.Clone()", Op.Inline),
            (typeof(Jazor.CLR.StringModule), "string.ToString(System.IFormatProvider)", Op.Inline),
			(typeof(Jazor.CLR.WeakReferenceModule), "virtual System.WeakReference.TrackResurrection.get", Op.Inline),
			(typeof(Jazor.CLR.WeakReferenceModule), "System.WeakReference.WeakReference(object)", Op.Import),
			(typeof(Jazor.CLR.WeakReferenceModule), "System.WeakReference.WeakReference(object, bool)", Op.Import),
			(typeof(Jazor.CLR.WeakReferenceModule), "virtual System.WeakReference.IsAlive.get", Op.Import),
			(typeof(Jazor.CLR.WeakReferenceModule), "virtual System.WeakReference.Target.get", Op.Import),
			(typeof(Jazor.CLR.WeakReferenceModule), "virtual System.WeakReference.Target.set", Op.Import),
            (typeof(Jazor.CLR.StringModule), "string.String(char[])", Op.Import),
            (typeof(Jazor.CLR.StringModule), "string.String(char[], int, int)", Op.Import),
            (typeof(Jazor.CLR.StringModule), "string.String(char, int)", Op.Import),
            (typeof(Jazor.CLR.StringModule), "string.IsNormalized()", Op.Import),
            (typeof(Jazor.CLR.StringModule), "string.IsNormalized(System.Text.NormalizationForm)", Op.Import),
            (typeof(Jazor.CLR.StringModule), "string.Normalize()", Op.Import),
            (typeof(Jazor.CLR.StringModule), "string.Normalize(System.Text.NormalizationForm)", Op.Import),
            (typeof(Jazor.CLR.StringModule), "static string.Concat(System.Collections.Generic.IEnumerable<string>)", Op.Import),
            (typeof(Jazor.CLR.StringModule), "static string.Concat(params string[])", Op.Import),
            (typeof(Jazor.CLR.StringModule), "static string.Concat(params System.ReadOnlySpan<string>)", Op.Import),
            (typeof(Jazor.CLR.StringModule), "static string.Join(char, params string[])", Op.Import),
            (typeof(Jazor.CLR.StringModule), "static string.Join(char, params System.ReadOnlySpan<string>)", Op.Import),
            (typeof(Jazor.CLR.StringModule), "static string.Join(string, params string[])", Op.Import),
            (typeof(Jazor.CLR.StringModule), "static string.Join(string, params System.ReadOnlySpan<string>)", Op.Import),
            (typeof(Jazor.CLR.StringModule), "static string.Join(char, string[], int, int)", Op.Import),
            (typeof(Jazor.CLR.StringModule), "static string.Join(string, string[], int, int)", Op.Import),
            (typeof(Jazor.CLR.StringModule), "static string.Join(string, System.Collections.Generic.IEnumerable<string>)", Op.Import),
            (typeof(Jazor.CLR.StringModule), "string.PadLeft(int)", Op.Import),
            (typeof(Jazor.CLR.StringModule), "string.PadLeft(int, char)", Op.Import),
            (typeof(Jazor.CLR.StringModule), "string.PadRight(int)", Op.Import),
            (typeof(Jazor.CLR.StringModule), "string.PadRight(int, char)", Op.Import),
            (typeof(Jazor.CLR.StringModule), "string.ReplaceLineEndings(string)", Op.Import),
            (typeof(Jazor.CLR.StringModule), "string.ReplaceLineEndings()", Op.Import),
            (typeof(Jazor.CLR.StringModule), "string.Trim(params System.ReadOnlySpan<char>)", Op.Import),
            (typeof(Jazor.CLR.StringModule), "string.TrimStart(params System.ReadOnlySpan<char>)", Op.Import),
            (typeof(Jazor.CLR.StringModule), "string.TrimEnd(params System.ReadOnlySpan<char>)", Op.Import),
			(typeof(Jazor.CLR.StringModule), "static string.Intern(string)", Op.Import),
			(typeof(Jazor.CLR.StringModule), "string.GetHashCode(System.StringComparison)", Op.Import),
			(typeof(Jazor.CLR.StringModule), "static string.GetHashCode(System.ReadOnlySpan<char>, System.StringComparison)", Op.Import),
			(typeof(Jazor.CLR.ObjectModule), "virtual object.GetHashCode()", Op.Import),
			(typeof(Jazor.CLR.ConditionalWeakTableT2Module<,>), "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Clear()", Op.Import),
            (typeof(Jazor.CLR.NullableT1Module<>), "static System.Nullable.Compare<T>(T?, T?)", Op.Import),
            (typeof(Jazor.CLR.NullableT1Module<>), "static System.Nullable.Equals<T>(T?, T?)", Op.Import)
        };

        foreach (var testCase in cases)
        {
            var mapping = testCase.Module
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Select(static method => method.GetCustomAttribute<JazorAttribute>())
                .Single(attribute => string.Equals(attribute?.Member, testCase.Member, StringComparison.Ordinal));

            Assert.IsNotNull(mapping, testCase.Member);
            Assert.AreEqual(testCase.Op, mapping.Op, testCase.Member);
        }
    }

    [TestMethod]
    public void ScalarConstructorsAndFixedWidthBigIntMembers_UseExplicitMappings()
    {
        var cases = new (Type Module, string Member, Op Op)[]
        {
            (typeof(Jazor.CLR.HalfModule), "System.Half.Half()", Op.Inline),
            (typeof(Jazor.CLR.ByteModule), "byte.Byte()", Op.Inline),
            (typeof(Jazor.CLR.DoubleModule), "double.Double()", Op.Inline),
            (typeof(Jazor.CLR.Int128Module), "System.Int128.Int128()", Op.Inline),
            (typeof(Jazor.CLR.Int64Module), "long.Int64()", Op.Inline),
            (typeof(Jazor.CLR.Int32Module), "int.Int32()", Op.Inline),
            (typeof(Jazor.CLR.Int16Module), "short.Int16()", Op.Inline),
            (typeof(Jazor.CLR.SingleModule), "float.Single()", Op.Inline),
            (typeof(Jazor.CLR.SByteModule), "sbyte.SByte()", Op.Inline),
            (typeof(Jazor.CLR.UInt128Module), "System.UInt128.UInt128()", Op.Inline),
            (typeof(Jazor.CLR.UInt64Module), "ulong.UInt64()", Op.Inline),
            (typeof(Jazor.CLR.UInt32Module), "uint.UInt32()", Op.Inline),
            (typeof(Jazor.CLR.UInt16Module), "ushort.UInt16()", Op.Inline),
            (typeof(Jazor.CLR.Int64Module), "static long.LeadingZeroCount(long)", Op.Import),
            (typeof(Jazor.CLR.Int64Module), "static long.Log2(long)", Op.Import),
            (typeof(Jazor.CLR.Int128Module), "static System.Int128.Log2(System.Int128)", Op.Import),
            (typeof(Jazor.CLR.UInt128Module), "static System.UInt128.Log2(System.UInt128)", Op.Inline),
            (typeof(Jazor.CLR.UInt64Module), "static ulong.DivRem(ulong, ulong)", Op.Import),
            (typeof(Jazor.CLR.UInt64Module), "static ulong.LeadingZeroCount(ulong)", Op.Import),
            (typeof(Jazor.CLR.UInt64Module), "static ulong.PopCount(ulong)", Op.Import),
            (typeof(Jazor.CLR.UInt64Module), "static ulong.RotateLeft(ulong, int)", Op.Import),
            (typeof(Jazor.CLR.UInt64Module), "static ulong.RotateRight(ulong, int)", Op.Import),
            (typeof(Jazor.CLR.UInt64Module), "static ulong.TrailingZeroCount(ulong)", Op.Import)
        };

        foreach (var testCase in cases)
        {
            var mapping = testCase.Module
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Select(static method => method.GetCustomAttribute<JazorAttribute>())
                .Single(attribute => string.Equals(attribute?.Member, testCase.Member, StringComparison.Ordinal));

            Assert.IsNotNull(mapping, testCase.Member);
            Assert.AreEqual(testCase.Op, mapping.Op, testCase.Member);
        }
    }

    [TestMethod]
    public void SpanParsingCapacityErasureAndReadOnlyFactories_UseTypedImports()
    {
        var cases = new (Type Module, string Member)[]
        {
            (typeof(Jazor.CLR.HalfModule), "static System.Half.TryParse(System.ReadOnlySpan<char>, out System.Half)"),
            (typeof(Jazor.CLR.SingleModule), "static float.TryParse(System.ReadOnlySpan<char>, out float)"),
            (typeof(Jazor.CLR.DoubleModule), "static double.TryParse(System.ReadOnlySpan<char>, out double)"),
            (typeof(Jazor.CLR.BigIntegerModule), "static System.Numerics.BigInteger.TryParse(System.ReadOnlySpan<char>, out System.Numerics.BigInteger)"),
			(typeof(Jazor.CLR.ByteModule), "static byte.TryParse(System.ReadOnlySpan<byte>, out byte)"),
			(typeof(Jazor.CLR.SByteModule), "static sbyte.TryParse(System.ReadOnlySpan<byte>, out sbyte)"),
			(typeof(Jazor.CLR.Int16Module), "static short.TryParse(System.ReadOnlySpan<byte>, out short)"),
			(typeof(Jazor.CLR.UInt16Module), "static ushort.TryParse(System.ReadOnlySpan<byte>, out ushort)"),
			(typeof(Jazor.CLR.Int32Module), "static int.TryParse(System.ReadOnlySpan<byte>, out int)"),
			(typeof(Jazor.CLR.UInt32Module), "static uint.TryParse(System.ReadOnlySpan<byte>, out uint)"),
			(typeof(Jazor.CLR.Int64Module), "static long.TryParse(System.ReadOnlySpan<byte>, out long)"),
			(typeof(Jazor.CLR.UInt64Module), "static ulong.TryParse(System.ReadOnlySpan<byte>, out ulong)"),
			(typeof(Jazor.CLR.Int128Module), "static System.Int128.TryParse(System.ReadOnlySpan<byte>, out System.Int128)"),
			(typeof(Jazor.CLR.UInt128Module), "static System.UInt128.TryParse(System.ReadOnlySpan<byte>, out System.UInt128)"),
			(typeof(Jazor.CLR.HalfModule), "static System.Half.TryParse(System.ReadOnlySpan<byte>, out System.Half)"),
			(typeof(Jazor.CLR.SingleModule), "static float.TryParse(System.ReadOnlySpan<byte>, out float)"),
			(typeof(Jazor.CLR.DoubleModule), "static double.TryParse(System.ReadOnlySpan<byte>, out double)"),
			(typeof(Jazor.CLR.DecimalModule), "static decimal.TryParse(System.ReadOnlySpan<byte>, out decimal)"),
			(typeof(Jazor.CLR.DecimalModule), "static decimal.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)"),
			(typeof(Jazor.CLR.DecimalModule), "static decimal.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out decimal)"),
			(typeof(Jazor.CLR.DecimalModule), "static decimal.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)"),
			(typeof(Jazor.CLR.DecimalModule), "static decimal.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out decimal)"),
            (typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>.TrimExcess()"),
            (typeof(Jazor.CLR.DictionaryT2Module<,>), "System.Collections.Generic.Dictionary<TKey, TValue>.TrimExcess()"),
            (typeof(Jazor.CLR.DictionaryT2Module<,>), "System.Collections.Generic.Dictionary<TKey, TValue>.TrimExcess(int)"),
            (typeof(Jazor.CLR.HashSetT1Module<>), "System.Collections.Generic.HashSet<T>.TrimExcess()"),
            (typeof(Jazor.CLR.HashSetT1Module<>), "System.Collections.Generic.HashSet<T>.TrimExcess(int)"),
            (typeof(Jazor.CLR.ReadOnlyCollectionT1Module<>), "static System.Collections.ObjectModel.ReadOnlyCollection.CreateCollection<T>(params System.ReadOnlySpan<T>)"),
            (typeof(Jazor.CLR.ReadOnlyCollectionT1Module<>), "static System.Collections.ObjectModel.ReadOnlyCollection.CreateSet<T>(params System.ReadOnlySpan<T>)")
        };

        foreach (var testCase in cases)
        {
            var mapping = testCase.Module
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Select(static method => method.GetCustomAttribute<JazorAttribute>())
                .Single(attribute => string.Equals(attribute?.Member, testCase.Member, StringComparison.Ordinal));

            Assert.IsNotNull(mapping, testCase.Member);
            Assert.AreEqual(Op.Import, mapping.Op, testCase.Member);
        }
    }

    [TestMethod]
    public void CollectionCapacityMembers_UseRuntimeImports()
    {
        var cases = new (Type Module, string Member)[]
        {
            (typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>.List()"),
            (typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>.List(int)"),
            (typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>.List(System.Collections.Generic.IEnumerable<T>)"),
            (typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>.Capacity.get"),
            (typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>.Capacity.set"),
            (typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>.Add(T)"),
            (typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>.EnsureCapacity(int)"),
            (typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>.FindAll(System.Predicate<T>)"),
            (typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>.Slice(int, int)"),
            (typeof(Jazor.CLR.DictionaryT2Module<,>), "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary()"),
            (typeof(Jazor.CLR.DictionaryT2Module<,>), "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IDictionary<TKey, TValue>)"),
            (typeof(Jazor.CLR.DictionaryT2Module<,>), "System.Collections.Generic.Dictionary<TKey, TValue>.Capacity.get"),
            (typeof(Jazor.CLR.DictionaryT2Module<,>), "System.Collections.Generic.Dictionary<TKey, TValue>.this[TKey].set"),
            (typeof(Jazor.CLR.DictionaryT2Module<,>), "System.Collections.Generic.Dictionary<TKey, TValue>.EnsureCapacity(int)"),
            (typeof(Jazor.CLR.HashSetT1Module<>), "System.Collections.Generic.HashSet<T>.HashSet()"),
            (typeof(Jazor.CLR.HashSetT1Module<>), "System.Collections.Generic.HashSet<T>.HashSet(System.Collections.Generic.IEnumerable<T>)"),
            (typeof(Jazor.CLR.HashSetT1Module<>), "System.Collections.Generic.HashSet<T>.Capacity.get"),
            (typeof(Jazor.CLR.HashSetT1Module<>), "System.Collections.Generic.HashSet<T>.EnsureCapacity(int)")
        };

        foreach (var testCase in cases)
        {
            var mapping = testCase.Module
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Select(static method => method.GetCustomAttribute<JazorAttribute>())
                .Single(attribute => string.Equals(attribute?.Member, testCase.Member, StringComparison.Ordinal));

            Assert.IsNotNull(mapping, testCase.Member);
            Assert.AreEqual(Op.Import, mapping.Op, testCase.Member);
        }
    }

    [TestMethod]
    public void GregorianCalendarConstructorMembers_UseSharedDateRuntimeImports()
    {
        var cases = new (Type Module, string Member)[]
        {
            (typeof(Jazor.CLR.DateTimeModule), "System.DateTime.DateTime(int, int, int, System.Globalization.Calendar)"),
            (typeof(Jazor.CLR.DateTimeModule), "System.DateTime.DateTime(int, int, int, int, int, int, int, System.Globalization.Calendar, System.DateTimeKind)"),
            (typeof(Jazor.CLR.DateTimeModule), "System.DateTime.DateTime(int, int, int, int, int, int, System.Globalization.Calendar)"),
            (typeof(Jazor.CLR.DateTimeModule), "System.DateTime.DateTime(int, int, int, int, int, int, int, System.Globalization.Calendar)"),
            (typeof(Jazor.CLR.DateTimeModule), "System.DateTime.DateTime(int, int, int, int, int, int, int, int, System.Globalization.Calendar)"),
            (typeof(Jazor.CLR.DateTimeModule), "System.DateTime.DateTime(int, int, int, int, int, int, int, int, System.Globalization.Calendar, System.DateTimeKind)"),
            (typeof(Jazor.CLR.DateTimeOffsetModule), "System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, int, System.Globalization.Calendar, System.TimeSpan)"),
            (typeof(Jazor.CLR.DateTimeOffsetModule), "System.DateTimeOffset.DateTimeOffset(int, int, int, int, int, int, int, int, System.Globalization.Calendar, System.TimeSpan)")
        };

        foreach (var testCase in cases)
        {
            var mapping = testCase.Module
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Select(static method => method.GetCustomAttribute<JazorAttribute>())
                .Single(attribute => string.Equals(attribute?.Member, testCase.Member, StringComparison.Ordinal));

            Assert.IsNotNull(mapping, testCase.Member);
            Assert.AreEqual(Op.Import, mapping.Op, testCase.Member);
        }
    }

    [TestMethod]
    public void UnicodeCharacterClassification_UsesSharedImportRuntime()
    {
        var members = new[]
        {
            "static char.IsDigit(char)",
            "static char.IsDigit(string, int)",
            "static char.IsLetter(char)",
            "static char.IsLetter(string, int)",
            "static char.IsUpper(char)",
            "static char.IsUpper(string, int)",
            "static char.IsLower(char)",
            "static char.IsLower(string, int)",
            "static char.IsPunctuation(char)",
            "static char.IsPunctuation(string, int)",
            "static char.IsLetterOrDigit(char)",
            "static char.IsLetterOrDigit(string, int)",
            "static char.IsNumber(char)",
            "static char.IsNumber(string, int)",
            "static char.IsSeparator(char)",
            "static char.IsSeparator(string, int)",
            "static char.IsSymbol(char)",
            "static char.IsSymbol(string, int)",
            "static char.GetUnicodeCategory(char)",
            "static char.GetUnicodeCategory(string, int)"
        };
        var mappings = typeof(Jazor.CLR.CharModule)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(static method => method.GetCustomAttribute<JazorAttribute>())
            .Where(static attribute => attribute is not null)
            .ToDictionary(static attribute => attribute!.Member, StringComparer.Ordinal);

        foreach (var member in members)
        {
            Assert.IsTrue(mappings.TryGetValue(member, out var mapping), member);
            Assert.IsNotNull(mapping, member);
            Assert.AreEqual(Op.Import, mapping.Op, member);
        }
    }

    [TestMethod]
    public void ExceptionCauseChain_UsesErrorCauseRuntimeImports()
    {
        var members = new[]
        {
            "System.Exception.Exception(string, System.Exception)",
            "System.Exception.InnerException.get",
            "virtual System.Exception.GetBaseException()"
        };
        var mappings = typeof(Jazor.CLR.ExceptionModule)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(static method => method.GetCustomAttribute<JazorAttribute>())
            .Where(static attribute => attribute is not null)
            .ToDictionary(static attribute => attribute!.Member, StringComparer.Ordinal);

        foreach (var member in members)
        {
            Assert.IsTrue(mappings.TryGetValue(member, out var mapping), member);
            Assert.IsNotNull(mapping, member);
            Assert.AreEqual(Op.Import, mapping.Op, member);
        }
    }

    [TestMethod]
    public void ExceptionSource_UsesPerErrorRuntimeImports()
    {
        var members = new[]
        {
            "virtual System.Exception.Source.get",
            "virtual System.Exception.Source.set"
        };
        var mappings = typeof(Jazor.CLR.ExceptionModule)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(static method => method.GetCustomAttribute<JazorAttribute>())
            .Where(static attribute => attribute is not null)
            .ToDictionary(static attribute => attribute!.Member, StringComparer.Ordinal);

        foreach (var member in members)
        {
            Assert.IsTrue(mappings.TryGetValue(member, out var mapping), member);
            Assert.IsNotNull(mapping, member);
            Assert.AreEqual(Op.Import, mapping.Op, member);
        }
    }

    [TestMethod]
    public void DecimalOaCurrencyMembers_UseExactBigIntRuntimeImports()
    {
        var mappings = GetMappings(typeof(Jazor.CLR.DecimalModule));
        foreach (var member in new[]
        {
            "static decimal.FromOACurrency(long)",
            "static decimal.ToOACurrency(decimal)"
        })
        {
            Assert.AreEqual(Op.Import, mappings[member]?.Op, member);
        }

        foreach (var member in new[]
        {
            "decimal.Decimal(int[])",
            "decimal.Decimal(System.ReadOnlySpan<int>)",
            "decimal.Decimal(int, int, int, bool, byte)",
            "static decimal.GetBits(decimal)"
        })
        {
            Assert.AreEqual(Op.Discard, mappings[member]?.Op, member);
        }
    }

    [TestMethod]
    public void StringBuilderContentMembers_UseSharedImportRuntime()
    {
        var members = new[]
        {
            "System.Text.StringBuilder.StringBuilder()",
            "System.Text.StringBuilder.StringBuilder(int)",
            "System.Text.StringBuilder.StringBuilder(string)",
            "System.Text.StringBuilder.StringBuilder(string, int)",
            "System.Text.StringBuilder.StringBuilder(string, int, int, int)",
            "System.Text.StringBuilder.StringBuilder(int, int)",
            "System.Text.StringBuilder.Capacity.get",
            "System.Text.StringBuilder.Capacity.set",
            "System.Text.StringBuilder.MaxCapacity.get",
            "System.Text.StringBuilder.EnsureCapacity(int)",
            "System.Text.StringBuilder.ToString(int, int)",
            "System.Text.StringBuilder.Length.set",
            "System.Text.StringBuilder.this[int].get",
            "System.Text.StringBuilder.this[int].set",
            "System.Text.StringBuilder.Append(char, int)",
            "System.Text.StringBuilder.Append(char[], int, int)",
            "System.Text.StringBuilder.Append(string)",
            "System.Text.StringBuilder.Append(string, int, int)",
            "System.Text.StringBuilder.Append(System.Text.StringBuilder)",
            "System.Text.StringBuilder.Append(System.Text.StringBuilder, int, int)",
            "System.Text.StringBuilder.AppendLine()",
            "System.Text.StringBuilder.AppendLine(string)",
            "System.Text.StringBuilder.CopyTo(int, char[], int, int)",
            "System.Text.StringBuilder.Insert(int, string, int)",
            "System.Text.StringBuilder.Remove(int, int)",
            "System.Text.StringBuilder.Append(bool)",
            "System.Text.StringBuilder.Append(char)",
            "System.Text.StringBuilder.Append(sbyte)",
            "System.Text.StringBuilder.Append(byte)",
            "System.Text.StringBuilder.Append(short)",
            "System.Text.StringBuilder.Append(int)",
            "System.Text.StringBuilder.Append(long)",
			"System.Text.StringBuilder.Append(float)",
			"System.Text.StringBuilder.Append(double)",
            "System.Text.StringBuilder.Append(decimal)",
            "System.Text.StringBuilder.Append(ushort)",
            "System.Text.StringBuilder.Append(uint)",
            "System.Text.StringBuilder.Append(ulong)",
            "System.Text.StringBuilder.Append(char[])",
            "System.Text.StringBuilder.Append(System.ReadOnlySpan<char>)",
            "System.Text.StringBuilder.AppendJoin(string, params string[])",
            "System.Text.StringBuilder.AppendJoin(string, params System.ReadOnlySpan<string>)",
            "System.Text.StringBuilder.AppendJoin(char, params string[])",
            "System.Text.StringBuilder.AppendJoin(char, params System.ReadOnlySpan<string>)",
            "System.Text.StringBuilder.Insert(int, string)",
            "System.Text.StringBuilder.Insert(int, bool)",
            "System.Text.StringBuilder.Insert(int, sbyte)",
            "System.Text.StringBuilder.Insert(int, byte)",
            "System.Text.StringBuilder.Insert(int, short)",
            "System.Text.StringBuilder.Insert(int, char)",
            "System.Text.StringBuilder.Insert(int, char[])",
            "System.Text.StringBuilder.Insert(int, char[], int, int)",
            "System.Text.StringBuilder.Insert(int, int)",
            "System.Text.StringBuilder.Insert(int, long)",
			"System.Text.StringBuilder.Insert(int, float)",
			"System.Text.StringBuilder.Insert(int, double)",
            "System.Text.StringBuilder.Insert(int, decimal)",
            "System.Text.StringBuilder.Insert(int, ushort)",
            "System.Text.StringBuilder.Insert(int, uint)",
            "System.Text.StringBuilder.Insert(int, ulong)",
            "System.Text.StringBuilder.Insert(int, System.ReadOnlySpan<char>)",
            "System.Text.StringBuilder.Replace(string, string)",
            "System.Text.StringBuilder.Replace(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)",
            "System.Text.StringBuilder.Equals(System.Text.StringBuilder)",
            "System.Text.StringBuilder.Equals(System.ReadOnlySpan<char>)",
            "System.Text.StringBuilder.Replace(string, string, int, int)",
            "System.Text.StringBuilder.Replace(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, int, int)",
            "System.Text.StringBuilder.Replace(char, char)",
            "System.Text.StringBuilder.Replace(char, char, int, int)"
        };
        var mappings = typeof(Jazor.CLR.StringBuilderModule)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(static method => method.GetCustomAttribute<JazorAttribute>())
            .Where(static attribute => attribute is not null)
            .ToDictionary(static attribute => attribute!.Member, StringComparer.Ordinal);

        foreach (var member in members)
        {
            Assert.IsTrue(mappings.TryGetValue(member, out var mapping), member);
            Assert.IsNotNull(mapping, member);
            Assert.AreEqual(Op.Import, mapping.Op, member);
        }
    }

    [TestMethod]
    public void StringRangeReadOnlySpanAndCopyMembers_UseImports()
    {
        var mappings = typeof(Jazor.CLR.StringModule)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(static method => method.GetCustomAttribute<JazorAttribute>())
            .Where(static attribute => attribute is not null)
            .ToDictionary(static attribute => attribute!.Member, StringComparer.Ordinal);

        Assert.AreEqual(
            Op.Import,
            mappings["static string.CompareOrdinal(string, int, string, int, int)"]?.Op);
        Assert.AreEqual(
            Op.Import,
            mappings["string.CopyTo(int, char[], int, int)"]?.Op);
        foreach (var member in new[]
        {
            "string.String(System.ReadOnlySpan<char>)",
            "static string.implicit operator System.ReadOnlySpan<char>(string)",
            "static string.GetHashCode(System.ReadOnlySpan<char>)",
            "static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)",
            "static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)",
            "static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)"
        })
        {
            Assert.AreEqual(Op.Import, mappings[member]?.Op, member);
        }
        Assert.AreEqual(
            Op.Import,
            mappings["static string.Copy(string)"]?.Op);

		var spanMappings = typeof(Jazor.CLR.MemoryExtensionsModule<>)
			.GetMethods(BindingFlags.Public | BindingFlags.Static)
			.Select(static method => method.GetCustomAttribute<JazorAttribute>())
			.Where(static attribute => attribute is not null)
			.ToDictionary(static attribute => attribute!.Member, StringComparer.Ordinal);
		foreach (var member in new[]
		{
			"System.ReadOnlySpan<char>.Trim()",
			"System.ReadOnlySpan<char>.Trim(char)",
			"System.ReadOnlySpan<char>.Trim(System.ReadOnlySpan<char>)",
			"System.ReadOnlySpan<char>.TrimStart()",
			"System.ReadOnlySpan<char>.TrimStart(char)",
			"System.ReadOnlySpan<char>.TrimStart(System.ReadOnlySpan<char>)",
			"System.ReadOnlySpan<char>.TrimEnd()",
			"System.ReadOnlySpan<char>.TrimEnd(char)",
			"System.ReadOnlySpan<char>.TrimEnd(System.ReadOnlySpan<char>)"
		})
		{
			Assert.AreEqual(Op.Import, spanMappings[member]?.Op, member);
		}
    }

    [TestMethod]
    public void MathDivRemAndBigIntegerIntegerConstruction_UseFixedWidthContracts()
    {
        var mathImports = new[]
        {
            "static System.Math.BigMul(ulong, ulong, out ulong)",
            "static System.Math.BigMul(long, long, out long)",
            "static System.Math.DivRem(int, int, out int)",
            "static System.Math.DivRem(long, long, out long)",
            "static System.Math.DivRem(sbyte, sbyte)",
            "static System.Math.DivRem(byte, byte)",
            "static System.Math.DivRem(short, short)",
            "static System.Math.DivRem(ushort, ushort)",
            "static System.Math.DivRem(int, int)",
            "static System.Math.DivRem(uint, uint)",
            "static System.Math.DivRem(long, long)",
            "static System.Math.DivRem(ulong, ulong)"
        };
        var mathMappings = GetMappings(typeof(Jazor.CLR.MathModule));
        foreach (var member in mathImports)
            Assert.AreEqual(Op.Import, mathMappings[member]?.Op, member);

        var bigIntegerMappings = GetMappings(typeof(Jazor.CLR.BigIntegerModule));
        foreach (var member in new[]
        {
            "System.Numerics.BigInteger.BigInteger(int)",
            "System.Numerics.BigInteger.BigInteger(uint)",
            "System.Numerics.BigInteger.BigInteger(long)",
            "System.Numerics.BigInteger.BigInteger(ulong)"
        })
        {
            Assert.AreEqual(Op.Inline, bigIntegerMappings[member]?.Op, member);
        }

        Assert.AreEqual(
            Op.Import,
            bigIntegerMappings["override System.Numerics.BigInteger.GetHashCode()"]?.Op);
        Assert.AreEqual(
            Op.Inline,
            GetMappings(typeof(Jazor.CLR.UInt32Module))["static uint.BigMul(uint, uint)"]?.Op);
    }

    [TestMethod]
    public void Binary64BoundaryMembers_UseSharedImportRuntime()
    {
        var doubleMappings = GetMappings(typeof(Jazor.CLR.DoubleModule));
        foreach (var member in new[]
        {
            "static double.Round(double)",
            "static double.Round(double, int)",
            "static double.Round(double, System.MidpointRounding)",
            "static double.Round(double, int, System.MidpointRounding)",
            "static double.BitDecrement(double)",
            "static double.BitIncrement(double)",
            "static double.Ieee754Remainder(double, double)",
            "static double.ILogB(double)"
        })
        {
            Assert.AreEqual(Op.Import, doubleMappings[member]?.Op, member);
        }

        var mathMappings = GetMappings(typeof(Jazor.CLR.MathModule));
        foreach (var member in new[]
        {
            "static System.Math.Round(double)",
            "static System.Math.Round(double, int)",
            "static System.Math.Round(double, System.MidpointRounding)",
            "static System.Math.Round(double, int, System.MidpointRounding)",
            "static System.Math.BitDecrement(double)",
            "static System.Math.BitIncrement(double)",
            "static System.Math.IEEERemainder(double, double)",
            "static System.Math.ILogB(double)"
        })
        {
            Assert.AreEqual(Op.Import, mathMappings[member]?.Op, member);
        }
    }

    [TestMethod]
    public void Binary32BoundaryMembers_UseFloatRoundedImportRuntime()
    {
        var mappings = GetMappings(typeof(Jazor.CLR.SingleModule));
        foreach (var member in new[]
        {
            "static float.Round(float)",
            "static float.Round(float, int)",
            "static float.Round(float, System.MidpointRounding)",
            "static float.Round(float, int, System.MidpointRounding)",
            "static float.BitDecrement(float)",
            "static float.BitIncrement(float)",
            "static float.Ieee754Remainder(float, float)",
            "static float.ILogB(float)"
        })
        {
            Assert.AreEqual(Op.Import, mappings[member]?.Op, member);
        }
    }

    [TestMethod]
    public void FloatingNativeAndEstimateMembers_UseTargetPrecisionContracts()
    {
        var cases = new (Type Module, string Prefix)[]
        {
            (typeof(Jazor.CLR.DoubleModule), "double"),
            (typeof(Jazor.CLR.SingleModule), "float"),
            (typeof(Jazor.CLR.HalfModule), "System.Half")
        };

        foreach (var (module, prefix) in cases)
        {
            var mappings = GetMappings(module);
            Assert.AreEqual(Op.Import, mappings[$"static {prefix}.ClampNative({prefix}, {prefix}, {prefix})"]?.Op);
            Assert.AreEqual(Op.Inline, mappings[$"static {prefix}.MaxNative({prefix}, {prefix})"]?.Op);
            Assert.AreEqual(Op.Inline, mappings[$"static {prefix}.MinNative({prefix}, {prefix})"]?.Op);
            Assert.AreEqual(Op.Inline, mappings[$"static {prefix}.MultiplyAddEstimate({prefix}, {prefix}, {prefix})"]?.Op);
        }
    }

    [TestMethod]
    public void HalfConversions_UseBinary16AndIntegerWidthRuntimeImports()
    {
        var mappings = GetMappings(typeof(Jazor.CLR.HalfModule));
        var importedMembers = new[]
        {
            "static System.Half.explicit operator System.Half(char)",
            "static System.Half.explicit operator System.Half(double)",
            "static System.Half.explicit operator System.Half(short)",
            "static System.Half.explicit operator System.Half(int)",
            "static System.Half.explicit operator System.Half(long)",
            "static System.Half.explicit operator System.Half(float)",
            "static System.Half.explicit operator System.Half(ushort)",
            "static System.Half.explicit operator System.Half(uint)",
            "static System.Half.explicit operator System.Half(ulong)",
            "static System.Half.explicit operator byte(System.Half)",
            "static System.Half.explicit operator checked byte(System.Half)",
            "static System.Half.explicit operator char(System.Half)",
            "static System.Half.explicit operator checked char(System.Half)",
            "static System.Half.explicit operator short(System.Half)",
            "static System.Half.explicit operator checked short(System.Half)",
            "static System.Half.explicit operator int(System.Half)",
            "static System.Half.explicit operator checked int(System.Half)",
            "static System.Half.explicit operator long(System.Half)",
            "static System.Half.explicit operator checked long(System.Half)",
            "static System.Half.explicit operator System.Int128(System.Half)",
            "static System.Half.explicit operator checked System.Int128(System.Half)",
            "static System.Half.explicit operator sbyte(System.Half)",
            "static System.Half.explicit operator checked sbyte(System.Half)",
            "static System.Half.explicit operator ushort(System.Half)",
            "static System.Half.explicit operator checked ushort(System.Half)",
            "static System.Half.explicit operator uint(System.Half)",
            "static System.Half.explicit operator checked uint(System.Half)",
            "static System.Half.explicit operator ulong(System.Half)",
            "static System.Half.explicit operator checked ulong(System.Half)",
            "static System.Half.explicit operator System.UInt128(System.Half)",
            "static System.Half.explicit operator checked System.UInt128(System.Half)",
            "static System.Half.implicit operator System.Half(byte)",
            "static System.Half.implicit operator System.Half(sbyte)",
            "static System.Half.explicit operator double(System.Half)",
            "static System.Half.explicit operator float(System.Half)"
        };

        Assert.HasCount(35, importedMembers);
        foreach (var member in importedMembers)
        {
            Assert.AreEqual(Op.Import, mappings[member]?.Op, member);
        }

        foreach (var member in new[]
        {
            "static System.Half.explicit operator System.Half(decimal)",
            "static System.Half.explicit operator System.Half(nint)",
            "static System.Half.explicit operator System.Half(nuint)",
            "static System.Half.explicit operator decimal(System.Half)",
            "static System.Half.explicit operator nint(System.Half)",
            "static System.Half.explicit operator checked nint(System.Half)",
            "static System.Half.explicit operator nuint(System.Half)",
            "static System.Half.explicit operator checked nuint(System.Half)"
        })
        {
            Assert.AreEqual(Op.Discard, mappings[member]?.Op, member);
        }
    }

    [TestMethod]
    public void WideFloatingConversions_UseSharedBigIntRuntimeImports()
    {
        var cases = new (Type Module, string[] ImportedMembers)[]
        {
            (typeof(Jazor.CLR.Int128Module),
            [
                "static System.Int128.explicit operator System.Half(System.Int128)",
                "static System.Int128.explicit operator System.Int128(double)",
                "static System.Int128.explicit operator checked System.Int128(double)",
                "static System.Int128.explicit operator System.Int128(float)",
                "static System.Int128.explicit operator checked System.Int128(float)"
            ]),
            (typeof(Jazor.CLR.UInt128Module),
            [
                "static System.UInt128.explicit operator System.Half(System.UInt128)",
                "static System.UInt128.explicit operator System.UInt128(double)",
                "static System.UInt128.explicit operator checked System.UInt128(double)",
                "static System.UInt128.explicit operator System.UInt128(float)",
                "static System.UInt128.explicit operator checked System.UInt128(float)"
            ]),
            (typeof(Jazor.CLR.BigIntegerModule),
            [
                "static System.Numerics.BigInteger.explicit operator System.Half(System.Numerics.BigInteger)"
            ])
        };

        foreach (var (module, importedMembers) in cases)
        {
            var mappings = GetMappings(module);
            foreach (var member in importedMembers)
            {
                Assert.AreEqual(Op.Import, mappings[member]?.Op, member);
            }
        }

        foreach (var (module, member) in new[]
        {
            (typeof(Jazor.CLR.Int128Module), "static System.Int128.explicit operator float(System.Int128)"),
            (typeof(Jazor.CLR.UInt128Module), "static System.UInt128.explicit operator float(System.UInt128)"),
            (typeof(Jazor.CLR.BigIntegerModule), "static System.Numerics.BigInteger.explicit operator float(System.Numerics.BigInteger)")
        })
        {
            Assert.AreEqual(Op.Inline, GetMappings(module)[member]?.Op, member);
        }
    }

    [TestMethod]
    public void Int128CharacterConversions_UseFixedWidthContracts()
    {
        var cases = new (Type Module, string Prefix)[]
        {
            (typeof(Jazor.CLR.Int128Module), "System.Int128"),
            (typeof(Jazor.CLR.UInt128Module), "System.UInt128")
        };

        foreach (var (module, prefix) in cases)
        {
            var mappings = GetMappings(module);
            Assert.AreEqual(Op.Inline, mappings[$"static {prefix}.explicit operator char({prefix})"]?.Op);
            Assert.AreEqual(Op.Import, mappings[$"static {prefix}.explicit operator checked char({prefix})"]?.Op);
            Assert.AreEqual(Op.Inline, mappings[$"static {prefix}.implicit operator {prefix}(char)"]?.Op);
        }
    }

    [TestMethod]
    public void BigIntegerConversions_UseTypedInlineAndCheckedImportContracts()
    {
        var mappings = GetMappings(typeof(Jazor.CLR.BigIntegerModule));
        foreach (var member in new[]
        {
            "System.Numerics.BigInteger.BigInteger(float)",
            "System.Numerics.BigInteger.BigInteger(double)",
            "System.Numerics.BigInteger.BigInteger(decimal)",
            "static System.Numerics.BigInteger.explicit operator byte(System.Numerics.BigInteger)",
            "static System.Numerics.BigInteger.explicit operator char(System.Numerics.BigInteger)",
            "static System.Numerics.BigInteger.explicit operator decimal(System.Numerics.BigInteger)",
            "static System.Numerics.BigInteger.explicit operator short(System.Numerics.BigInteger)",
            "static System.Numerics.BigInteger.explicit operator int(System.Numerics.BigInteger)",
            "static System.Numerics.BigInteger.explicit operator long(System.Numerics.BigInteger)",
            "static System.Numerics.BigInteger.explicit operator System.Int128(System.Numerics.BigInteger)",
            "static System.Numerics.BigInteger.explicit operator sbyte(System.Numerics.BigInteger)",
            "static System.Numerics.BigInteger.explicit operator ushort(System.Numerics.BigInteger)",
            "static System.Numerics.BigInteger.explicit operator uint(System.Numerics.BigInteger)",
            "static System.Numerics.BigInteger.explicit operator ulong(System.Numerics.BigInteger)",
            "static System.Numerics.BigInteger.explicit operator System.UInt128(System.Numerics.BigInteger)",
            "static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(decimal)",
            "static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(double)",
            "static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(System.Half)",
            "static System.Numerics.BigInteger.explicit operator System.Numerics.BigInteger(float)"
        })
        {
            Assert.AreEqual(Op.Import, mappings[member]?.Op, member);
        }

        foreach (var member in new[]
        {
            "static System.Numerics.BigInteger.explicit operator double(System.Numerics.BigInteger)",
            "static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(byte)",
            "static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(char)",
            "static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(short)",
            "static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(int)",
            "static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(long)",
            "static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(System.Int128)",
            "static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(sbyte)",
            "static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(ushort)",
            "static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(uint)",
            "static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(ulong)",
            "static System.Numerics.BigInteger.implicit operator System.Numerics.BigInteger(System.UInt128)"
        })
        {
            Assert.AreEqual(Op.Inline, mappings[member]?.Op, member);
        }

        var conversionMethods = typeof(Jazor.CLR.BigIntegerModule)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(static method => method.GetCustomAttribute<JazorAttribute>()?.Member.Contains(" operator ", StringComparison.Ordinal) == true)
            .Where(static method => method.GetCustomAttribute<JazorAttribute>()?.Op != Op.Discard);
        foreach (var method in conversionMethods)
            Assert.HasCount(1, method.GetParameters(), method.Name);
    }

    private static IReadOnlyDictionary<string, JazorAttribute?> GetMappings(Type module)
        => module
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(static method => method.GetCustomAttribute<JazorAttribute>())
            .Where(static attribute => attribute is not null)
            .ToDictionary(static attribute => attribute!.Member, StringComparer.Ordinal);
}
