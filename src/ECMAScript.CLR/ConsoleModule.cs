using ECMAScript.Common;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.Console", WhiteListOp.Replace, "console")]
public static class ConsoleModule
{
	//[WhiteList("static System.Console.BackgroundColor.get", WhiteListOp.Discard)]
	//public extern static System.ConsoleColor _b2913bd8e903362d(System.Console instance);

	//[WhiteList("static System.Console.BackgroundColor.set", WhiteListOp.Discard)]
	//public extern static void _4ee40b9f5d29e242(System.Console instance, object value);

	//[WhiteList("static System.Console.BufferHeight.get", WhiteListOp.Discard)]
	//public extern static Number _78f6a2e9149eac8d(System.Console instance);

	//[WhiteList("static System.Console.BufferHeight.set", WhiteListOp.Discard)]
	//public extern static void _008d0a53c97269db(System.Console instance, Number value);

	//[WhiteList("static System.Console.BufferWidth.get", WhiteListOp.Discard)]
	//public extern static Number _cc68a62966893c78(System.Console instance);

	//[WhiteList("static System.Console.BufferWidth.set", WhiteListOp.Discard)]
	//public extern static void _2475b6b64873b657(System.Console instance, Number value);

	//[WhiteList("static System.Console.CapsLock.get", WhiteListOp.Discard)]
	//public extern static bool _2873ea11d4e971df(System.Console instance);

	//[WhiteList("static System.Console.CursorLeft.get", WhiteListOp.Discard)]
	//public extern static Number _9e7c51b7e5446aa6(System.Console instance);

	//[WhiteList("static System.Console.CursorLeft.set", WhiteListOp.Discard)]
	//public extern static void _6ccdbe86a5eeabfe(System.Console instance, Number value);

	//[WhiteList("static System.Console.CursorSize.get", WhiteListOp.Discard)]
	//public extern static Number _7fb71d51c287d624(System.Console instance);

	//[WhiteList("static System.Console.CursorSize.set", WhiteListOp.Discard)]
	//public extern static void _a0a70f47636077e6(System.Console instance, Number value);

	//[WhiteList("static System.Console.CursorTop.get", WhiteListOp.Discard)]
	//public extern static Number _5468de6baa878f47(System.Console instance);

	//[WhiteList("static System.Console.CursorTop.set", WhiteListOp.Discard)]
	//public extern static void _95f880f91d18695f(System.Console instance, Number value);

	//[WhiteList("static System.Console.CursorVisible.get", WhiteListOp.Discard)]
	//public extern static bool _2eac576eada0d018(System.Console instance);

	//[WhiteList("static System.Console.CursorVisible.set", WhiteListOp.Discard)]
	//public extern static void _9992e675f25ec4f0(System.Console instance, object value);

	//[WhiteList("static System.Console.Error.get", WhiteListOp.Discard)]
	//public extern static System.IO.TextWriter _d71693970ee00fa2(System.Console instance);

	//[WhiteList("static System.Console.ForegroundColor.get", WhiteListOp.Discard)]
	//public extern static System.ConsoleColor _c467047cce83b4a2(System.Console instance);

	//[WhiteList("static System.Console.ForegroundColor.set", WhiteListOp.Discard)]
	//public extern static void _9c9ee8009ecae95d(System.Console instance, object value);

	//[WhiteList("static System.Console.In.get", WhiteListOp.Discard)]
	//public extern static System.IO.TextReader _880f0b69ee8eebd6(System.Console instance);

	//[WhiteList("static System.Console.InputEncoding.get", WhiteListOp.Discard)]
	//public extern static System.Text.Encoding _c11de5200bca2f20(System.Console instance);

	//[WhiteList("static System.Console.InputEncoding.set", WhiteListOp.Discard)]
	//public extern static void _009c1ec8c289d826(System.Console instance, object value);

	//[WhiteList("static System.Console.IsErrorRedirected.get", WhiteListOp.Discard)]
	//public extern static bool _8b2ee33dafe1f26b(System.Console instance);

	//[WhiteList("static System.Console.IsInputRedirected.get", WhiteListOp.Discard)]
	//public extern static bool _a9228d076b7d6927(System.Console instance);

	//[WhiteList("static System.Console.IsOutputRedirected.get", WhiteListOp.Discard)]
	//public extern static bool _489b8bfc494baa29(System.Console instance);

	//[WhiteList("static System.Console.KeyAvailable.get", WhiteListOp.Discard)]
	//public extern static bool _11af9b5bd8f22c76(System.Console instance);

	//[WhiteList("static System.Console.LargestWindowHeight.get", WhiteListOp.Discard)]
	//public extern static Number _112081177aa5379c(System.Console instance);

	//[WhiteList("static System.Console.LargestWindowWidth.get", WhiteListOp.Discard)]
	//public extern static Number _0e0b37952b8aeb15(System.Console instance);

	//[WhiteList("static System.Console.NumberLock.get", WhiteListOp.Discard)]
	//public extern static bool _b1d96a12bbcd09fc(System.Console instance);

	//[WhiteList("static System.Console.Out.get", WhiteListOp.Discard)]
	//public extern static System.IO.TextWriter _2cd9efaaec703efd(System.Console instance);

	//[WhiteList("static System.Console.OutputEncoding.get", WhiteListOp.Discard)]
	//public extern static System.Text.Encoding _dff17221ba3fc15b(System.Console instance);

	//[WhiteList("static System.Console.OutputEncoding.set", WhiteListOp.Discard)]
	//public extern static void _ea327c486add9f0e(System.Console instance, object value);

	//[WhiteList("static System.Console.Title.get", WhiteListOp.Discard)]
	//public extern static string _0ec0aa8b5bf0d1c4(System.Console instance);

	//[WhiteList("static System.Console.Title.set", WhiteListOp.Discard)]
	//public extern static void _7559c0c607fb023a(System.Console instance, object value);

	//[WhiteList("static System.Console.TreatControlCAsInput.get", WhiteListOp.Discard)]
	//public extern static bool _8ab46f86bbb99616(System.Console instance);

	//[WhiteList("static System.Console.TreatControlCAsInput.set", WhiteListOp.Discard)]
	//public extern static void _7cebd43b37fb9744(System.Console instance, object value);

	//[WhiteList("static System.Console.WindowHeight.get", WhiteListOp.Discard)]
	//public extern static Number _55f4118bad131829(System.Console instance);

	//[WhiteList("static System.Console.WindowHeight.set", WhiteListOp.Discard)]
	//public extern static void _9536ac2a96b2c04a(System.Console instance, Number value);

	//[WhiteList("static System.Console.WindowLeft.get", WhiteListOp.Discard)]
	//public extern static Number _3b5ec4626962a01d(System.Console instance);

	//[WhiteList("static System.Console.WindowLeft.set", WhiteListOp.Discard)]
	//public extern static void _147bce3eaeb65540(System.Console instance, Number value);

	//[WhiteList("static System.Console.WindowTop.get", WhiteListOp.Discard)]
	//public extern static Number _d3338ac931617887(System.Console instance);

	//[WhiteList("static System.Console.WindowTop.set", WhiteListOp.Discard)]
	//public extern static void _d1df236b3e7c9311(System.Console instance, Number value);

	//[WhiteList("static System.Console.WindowWidth.get", WhiteListOp.Discard)]
	//public extern static Number _c3b3156e4b50c6e5(System.Console instance);

	//[WhiteList("static System.Console.WindowWidth.set", WhiteListOp.Discard)]
	//public extern static void _d09ec9051b3189ba(System.Console instance, Number value);

	//[WhiteList("static System.Console.CancelKeyPress.add", WhiteListOp.Discard)]
	//public extern static void _d22bf76da4925970(object value);

	//[WhiteList("static System.Console.CancelKeyPress.remove", WhiteListOp.Discard)]
	//public extern static void _426a04cfc56ff11a(object value);

	//[WhiteList("static System.Console.Beep()", WhiteListOp.Discard)]
	//public extern static void _6abcd85656794fda();

	//[WhiteList("static System.Console.Beep(int, int)", WhiteListOp.Discard)]
	//public extern static void _e9811cc63b2cf680(Number frequency, Number duration);

	//[WhiteList("static System.Console.Clear()", WhiteListOp.Discard)]
	//public extern static void _7779d957d8f16481();

	//[WhiteList("static System.Console.GetCursorPosition()", WhiteListOp.Discard)]
	//public extern static (int Left, int Top) _b1cc96fdd3c7ac15();

	//[WhiteList("static System.Console.MoveBufferArea(int, int, int, int, int, int)", WhiteListOp.Discard)]
	//public extern static void _07404b431cdde8e2(Number sourceLeft, Number sourceTop, Number sourceWidth, Number sourceHeight, Number targetLeft, Number targetTop);

	//[WhiteList("static System.Console.MoveBufferArea(int, int, int, int, int, int, char, System.ConsoleColor, System.ConsoleColor)", WhiteListOp.Discard)]
	//public extern static void _1535b4df4471cca4(Number sourceLeft, Number sourceTop, Number sourceWidth, Number sourceHeight, Number targetLeft, Number targetTop, Number sourceChar, object sourceForeColor, object sourceBackColor);

	//[WhiteList("static System.Console.OpenStandardError()", WhiteListOp.Discard)]
	//public extern static System.IO.Stream _fb73e1943bd8a33b();

	//[WhiteList("static System.Console.OpenStandardError(int)", WhiteListOp.Discard)]
	//public extern static System.IO.Stream _50377d8e27ed372b(Number bufferSize);

	//[WhiteList("static System.Console.OpenStandardInput()", WhiteListOp.Discard)]
	//public extern static System.IO.Stream _dc750bd876ac132a();

	//[WhiteList("static System.Console.OpenStandardInput(int)", WhiteListOp.Discard)]
	//public extern static System.IO.Stream _fa1cb98329e3050e(Number bufferSize);

	//[WhiteList("static System.Console.OpenStandardOutput()", WhiteListOp.Discard)]
	//public extern static System.IO.Stream _a09b71061a11329a();

	//[WhiteList("static System.Console.OpenStandardOutput(int)", WhiteListOp.Discard)]
	//public extern static System.IO.Stream _17ea088e4471485a(Number bufferSize);

	//[WhiteList("static System.Console.Read()", WhiteListOp.Discard)]
	//public extern static Number _1a344364f77ef0d3();

	//[WhiteList("static System.Console.ReadKey()", WhiteListOp.Discard)]
	//public extern static System.ConsoleKeyInfo _cdfbb2b6c7857da4();

	//[WhiteList("static System.Console.ReadKey(bool)", WhiteListOp.Discard)]
	//public extern static System.ConsoleKeyInfo _009c6d5dd94f0728(object intercept);

	//[WhiteList("static System.Console.ReadLine()", WhiteListOp.Discard)]
	//public extern static string? _d665efe65ee40f12();

	//[WhiteList("static System.Console.ResetColor()", WhiteListOp.Discard)]
	//public extern static void _1c7916d8deb9d83b();

	//[WhiteList("static System.Console.SetBufferSize(int, int)", WhiteListOp.Discard)]
	//public extern static void _2a4c1a9d52c1050c(Number width, Number height);

	//[WhiteList("static System.Console.SetCursorPosition(int, int)", WhiteListOp.Discard)]
	//public extern static void _6954937a857814c5(Number left, Number top);

	//[WhiteList("static System.Console.SetError(System.IO.TextWriter)", WhiteListOp.Discard)]
	//public extern static void _76212a2c41bdd5da(object newError);

	//[WhiteList("static System.Console.SetIn(System.IO.TextReader)", WhiteListOp.Discard)]
	//public extern static void _0d6b03c10896e4ba(object newIn);

	//[WhiteList("static System.Console.SetOut(System.IO.TextWriter)", WhiteListOp.Discard)]
	//public extern static void _ca2b7b86f66acd90(object newOut);

	//[WhiteList("static System.Console.SetWindowPosition(int, int)", WhiteListOp.Discard)]
	//public extern static void _5c2293ddf639923a(Number left, Number top);

	//[WhiteList("static System.Console.SetWindowSize(int, int)", WhiteListOp.Discard)]
	//public extern static void _a3a20a010844aaaf(Number width, Number height);

	[WhiteList("static System.Console.Write(bool)", WhiteListOp.Replace, "log")]
	public extern static void _a4ba329944e98b1c(object value);

	[WhiteList("static System.Console.Write(char)", WhiteListOp.Replace, "log")]
	public extern static void _c61ec50b9f9538a3(Number value);

	[WhiteList("static System.Console.Write(char[])", WhiteListOp.Discard)]
	public extern static void _aa7978304cd0bacc(object buffer);

	[WhiteList("static System.Console.Write(char[], int, int)", WhiteListOp.Discard)]
	public extern static void _10c4068d62648fb5(object buffer, Number index, Number count);

	[WhiteList("static System.Console.Write(decimal)", WhiteListOp.Replace, "log")]
	public extern static void _fe7b9d5b136e9441(String value);

	[WhiteList("static System.Console.Write(double)", WhiteListOp.Replace, "log")]
	public extern static void _c7002c416a3da063(Number value);

	[WhiteList("static System.Console.Write(int)", WhiteListOp.Replace, "log")]
	public extern static void _9aeb4b39f93efc70(Number value);

	[WhiteList("static System.Console.Write(long)", WhiteListOp.Replace, "log")]
	public extern static void _8950a34699a5bdf8(BigInt value);

	[WhiteList("static System.Console.Write(object)", WhiteListOp.Replace, "log")]
	public extern static void _134c0342866ed156(Object? value);

	[WhiteList("static System.Console.Write(float)", WhiteListOp.Replace, "log")]
	public extern static void _80304f087568bfd4(Number value);

	[WhiteList("static System.Console.Write(string)", WhiteListOp.Replace, "log")]
	public extern static void _89898d51245a9c64(object value);

	[WhiteList("static System.Console.Write(System.ReadOnlySpan<char>)", WhiteListOp.Discard)]
	public extern static void _ec7a704092bf9982(Uint32Array value);

	[WhiteList("static System.Console.Write(string, object)", WhiteListOp.Replace, "log")]
	public extern static void _961ab9b501a6baf0(object format, Object? arg0);

	[WhiteList("static System.Console.Write(string, object, object)", WhiteListOp.Replace, "log")]
	public extern static void _33daccffa622fc66(object format, Object? arg0, Object? arg1);

	[WhiteList("static System.Console.Write(string, object, object, object)", WhiteListOp.Replace, "log")]
	public extern static void _366c851b7a360959(object format, Object? arg0, Object? arg1, Object? arg2);

	[WhiteList("static System.Console.Write(string, params object[])", WhiteListOp.Replace, "log")]
	public extern static void _bdb97b77edce5259(object format, object arg);

	[WhiteList("static System.Console.Write(string, params System.ReadOnlySpan<object>)", WhiteListOp.Discard)]
	public extern static void _4a291949ebb466b9(object format, object arg);

	[WhiteList("static System.Console.Write(uint)", WhiteListOp.Replace, "log")]
	public extern static void _e31e2ab80d13cd13(Number value);

	[WhiteList("static System.Console.Write(ulong)", WhiteListOp.Replace, "log")]
	public extern static void _e8ba4c4ca492d5a8(BigInt value);

	[WhiteList("static System.Console.WriteLine()", WhiteListOp.Replace, "log")]
	public extern static void _64a3c7e35feaa9f0();

	[WhiteList("static System.Console.WriteLine(bool)", WhiteListOp.Replace, "log")]
	public extern static void _0657067880cafdd2(object value);

	[WhiteList("static System.Console.WriteLine(char)", WhiteListOp.Replace, "log")]
	public extern static void _5a138b02870324cb(Number value);

	[WhiteList("static System.Console.WriteLine(char[])", WhiteListOp.Discard)]
	public extern static void _bfd6ae4fc98a90ff(object buffer);

	[WhiteList("static System.Console.WriteLine(char[], int, int)", WhiteListOp.Discard)]
	public extern static void _460b8c2943875e7e(object buffer, Number index, Number count);

	[WhiteList("static System.Console.WriteLine(decimal)", WhiteListOp.Replace, "log")]
	public extern static void _24ba609cace0df9d(String value);

	[WhiteList("static System.Console.WriteLine(double)", WhiteListOp.Replace, "log")]
	public extern static void _b457fd5c1c5f9568(Number value);

	[WhiteList("static System.Console.WriteLine(int)", WhiteListOp.Replace, "log")]
	public extern static void _8f3980b4b82b99ac(Number value);

	[WhiteList("static System.Console.WriteLine(long)", WhiteListOp.Replace, "log")]
	public extern static void _0a1213ea041262ff(BigInt value);

	[WhiteList("static System.Console.WriteLine(object)", WhiteListOp.Replace, "log")]
	public extern static void _b1dc4a6e5df341aa(Object? value);

	[WhiteList("static System.Console.WriteLine(float)", WhiteListOp.Replace, "log")]
	public extern static void _38bc406a617b49ca(Number value);

	[WhiteList("static System.Console.WriteLine(string)", WhiteListOp.Replace, "log")]
	public extern static void _19f2583beee4f7fb(object value);

	[WhiteList("static System.Console.WriteLine(System.ReadOnlySpan<char>)", WhiteListOp.Discard)]
	public extern static void _fd102c4488f2b5f3(Uint32Array value);

	[WhiteList("static System.Console.WriteLine(string, object)", WhiteListOp.Replace, "log")]
	public extern static void _c4e6acf24771bb66(object format, Object? arg0);

	[WhiteList("static System.Console.WriteLine(string, object, object)", WhiteListOp.Replace, "log")]
	public extern static void _f5c74fa705a4f0b9(object format, Object? arg0, Object? arg1);

	[WhiteList("static System.Console.WriteLine(string, object, object, object)", WhiteListOp.Replace, "log")]
	public extern static void _0fa26d5cd312b8d3(object format, Object? arg0, Object? arg1, Object? arg2);

	[WhiteList("static System.Console.WriteLine(string, params object[])", WhiteListOp.Replace, "log")]
	public extern static void _7a73fda86982983f(object format, object arg);

	[WhiteList("static System.Console.WriteLine(string, params System.ReadOnlySpan<object>)", WhiteListOp.Discard)]
	public extern static void _e2406d27d341e50b(object format, object arg);

	[WhiteList("static System.Console.WriteLine(uint)", WhiteListOp.Replace, "log")]
	public extern static void _029fda9e4e9b254f(Number value);

	[WhiteList("static System.Console.WriteLine(ulong)", WhiteListOp.Replace, "log")]
	public extern static void _28da774493da8a92(BigInt value);
}
