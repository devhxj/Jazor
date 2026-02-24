namespace Jazor.CLR;

[Jazor(Op.Replace, "System.Console", "console")]
public static class ConsoleModule
{
	[Jazor(Op.Discard ,"static object.Clear()")]
	public extern static void _7779d957d8f16481();

	[Jazor(Op.Replace ,"static object.WriteLine()","log")]
	public extern static void _64a3c7e35feaa9f0();

	[Jazor(Op.Replace, "static object.WriteLine(bool)", "log")]
	public extern static void _0657067880cafdd2(object value);

	[Jazor(Op.Replace, "static object.WriteLine(char)", "log")]
	public extern static void _5a138b02870324cb(Number value);

	[Jazor(Op.Replace, "static object.WriteLine(char[])", "log")]
	public extern static void _bfd6ae4fc98a90ff(object buffer);

	[Jazor(Op.Discard ,"static object.WriteLine(char[], int, int)")]
	public extern static void _460b8c2943875e7e(object buffer, Number index, Number count);

	[Jazor(Op.Discard ,"static object.WriteLine(System.Decimal)")]
	public extern static void _06770dfb1e3ad0be(object value);

	[Jazor(Op.Discard ,"static object.WriteLine(double)")]
	public extern static void _b457fd5c1c5f9568(Number value);

	[Jazor(Op.Discard ,"static object.WriteLine(float)")]
	public extern static void _38bc406a617b49ca(Number value);

	[Jazor(Op.Discard ,"static object.WriteLine(int)")]
	public extern static void _8f3980b4b82b99ac(Number value);

	[Jazor(Op.Discard ,"static object.WriteLine(uint)")]
	public extern static void _029fda9e4e9b254f(Number value);

	[Jazor(Op.Discard ,"static object.WriteLine(long)")]
	public extern static void _0a1213ea041262ff(BigInt value);

	[Jazor(Op.Discard ,"static object.WriteLine(ulong)")]
	public extern static void _28da774493da8a92(BigInt value);

	[Jazor(Op.Discard ,"static object.WriteLine(object)")]
	public extern static void _b1dc4a6e5df341aa(Object? value);

	[Jazor(Op.Discard ,"static object.WriteLine(string)")]
	public extern static void _19f2583beee4f7fb(object value);

	[Jazor(Op.Discard ,"static object.WriteLine(System.ReadOnlySpan<char>)")]
	public extern static void _fd102c4488f2b5f3(Uint32Array value);

	[Jazor(Op.Discard ,"static object.WriteLine(string, object)")]
	public extern static void _c4e6acf24771bb66(object format, Object? arg0);

	[Jazor(Op.Discard ,"static object.WriteLine(string, object, object)")]
	public extern static void _f5c74fa705a4f0b9(object format, Object? arg0, Object? arg1);

	[Jazor(Op.Discard ,"static object.WriteLine(string, object, object, object)")]
	public extern static void _0fa26d5cd312b8d3(object format, Object? arg0, Object? arg1, Object? arg2);

	[Jazor(Op.Discard ,"static object.WriteLine(string, params object[])")]
	public extern static void _7a73fda86982983f(object format,  object arg);

	[Jazor(Op.Discard ,"static object.WriteLine(string, params System.ReadOnlySpan<object>)")]
	public extern static void _e2406d27d341e50b(object format,  object arg);

	[Jazor(Op.Discard ,"static object.Write(string, object)")]
	public extern static void _961ab9b501a6baf0(object format, Object? arg0);

	[Jazor(Op.Discard ,"static object.Write(string, object, object)")]
	public extern static void _33daccffa622fc66(object format, Object? arg0, Object? arg1);

	[Jazor(Op.Discard ,"static object.Write(string, object, object, object)")]
	public extern static void _366c851b7a360959(object format, Object? arg0, Object? arg1, Object? arg2);

	[Jazor(Op.Discard ,"static object.Write(string, params object[])")]
	public extern static void _bdb97b77edce5259(object format,  object arg);

	[Jazor(Op.Discard ,"static object.Write(string, params System.ReadOnlySpan<object>)")]
	public extern static void _4a291949ebb466b9(object format,  object arg);

	[Jazor(Op.Discard ,"static object.Write(bool)")]
	public extern static void _a4ba329944e98b1c(object value);

	[Jazor(Op.Discard ,"static object.Write(char)")]
	public extern static void _c61ec50b9f9538a3(Number value);

	[Jazor(Op.Discard ,"static object.Write(char[])")]
	public extern static void _aa7978304cd0bacc(object buffer);

	[Jazor(Op.Discard ,"static object.Write(char[], int, int)")]
	public extern static void _10c4068d62648fb5(object buffer, Number index, Number count);

	[Jazor(Op.Discard ,"static object.Write(double)")]
	public extern static void _c7002c416a3da063(Number value);

	[Jazor(Op.Discard ,"static object.Write(System.Decimal)")]
	public extern static void _c37cf10f8516d6b7(object value);

	[Jazor(Op.Discard ,"static object.Write(float)")]
	public extern static void _80304f087568bfd4(Number value);

	[Jazor(Op.Discard ,"static object.Write(int)")]
	public extern static void _9aeb4b39f93efc70(Number value);

	[Jazor(Op.Discard ,"static object.Write(uint)")]
	public extern static void _e31e2ab80d13cd13(Number value);

	[Jazor(Op.Discard ,"static object.Write(long)")]
	public extern static void _8950a34699a5bdf8(BigInt value);

	[Jazor(Op.Discard ,"static object.Write(ulong)")]
	public extern static void _e8ba4c4ca492d5a8(BigInt value);

	[Jazor(Op.Discard ,"static object.Write(object)")]
	public extern static void _134c0342866ed156(Object? value);

	[Jazor(Op.Discard ,"static object.Write(string)")]
	public extern static void _89898d51245a9c64(object value);

	[Jazor(Op.Discard ,"static object.Write(System.ReadOnlySpan<char>)")]
	public extern static void _ec7a704092bf9982(Uint32Array value);
}
