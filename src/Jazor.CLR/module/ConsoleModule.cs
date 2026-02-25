namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "System.Console","System/ConsoleModule.js")]
public static class ConsoleModule
{
	[Jazor(Op.Discard ,"static System.Console.In.get")]
	public extern static System.IO.TextReader _880f0b69ee8eebd6(object instance);

	[Jazor(Op.Discard ,"static System.Console.InputEncoding.get")]
	public extern static System.Text.Encoding _c11de5200bca2f20(object instance);

	[Jazor(Op.Discard ,"static System.Console.InputEncoding.set")]
	public extern static void _009c1ec8c289d826(object instance, object value);

	[Jazor(Op.Discard ,"static System.Console.OutputEncoding.get")]
	public extern static System.Text.Encoding _dff17221ba3fc15b(object instance);

	[Jazor(Op.Discard ,"static System.Console.OutputEncoding.set")]
	public extern static void _ea327c486add9f0e(object instance, object value);

	[Jazor(Op.Discard ,"static System.Console.KeyAvailable.get")]
	public extern static bool _11af9b5bd8f22c76(object instance);

	[Jazor(Op.Discard ,"static System.Console.ReadKey()")]
	public extern static System.ConsoleKeyInfo _cdfbb2b6c7857da4();

	[Jazor(Op.Discard ,"static System.Console.ReadKey(bool)")]
	public extern static System.ConsoleKeyInfo _009c6d5dd94f0728(bool intercept);

	[Jazor(Op.Discard ,"static System.Console.Out.get")]
	public extern static System.IO.TextWriter _2cd9efaaec703efd(object instance);

	[Jazor(Op.Discard ,"static System.Console.Error.get")]
	public extern static System.IO.TextWriter _d71693970ee00fa2(object instance);

	[Jazor(Op.Discard ,"static System.Console.IsInputRedirected.get")]
	public extern static bool _a9228d076b7d6927(object instance);

	[Jazor(Op.Discard ,"static System.Console.IsOutputRedirected.get")]
	public extern static bool _489b8bfc494baa29(object instance);

	[Jazor(Op.Discard ,"static System.Console.IsErrorRedirected.get")]
	public extern static bool _8b2ee33dafe1f26b(object instance);

	[Jazor(Op.Discard ,"static System.Console.CursorSize.get")]
	public extern static Number _7fb71d51c287d624(object instance);

	[Jazor(Op.Discard ,"static System.Console.CursorSize.set")]
	public extern static void _a0a70f47636077e6(object instance, Number value);

	[Jazor(Op.Discard ,"static System.Console.NumberLock.get")]
	public extern static bool _b1d96a12bbcd09fc(object instance);

	[Jazor(Op.Discard ,"static System.Console.CapsLock.get")]
	public extern static bool _2873ea11d4e971df(object instance);

	[Jazor(Op.Discard ,"static System.Console.BackgroundColor.get")]
	public extern static System.ConsoleColor _b2913bd8e903362d(object instance);

	[Jazor(Op.Discard ,"static System.Console.BackgroundColor.set")]
	public extern static void _4ee40b9f5d29e242(object instance, object value);

	[Jazor(Op.Discard ,"static System.Console.ForegroundColor.get")]
	public extern static System.ConsoleColor _c467047cce83b4a2(object instance);

	[Jazor(Op.Discard ,"static System.Console.ForegroundColor.set")]
	public extern static void _9c9ee8009ecae95d(object instance, object value);

	[Jazor(Op.Discard ,"static System.Console.ResetColor()")]
	public extern static void _1c7916d8deb9d83b();

	[Jazor(Op.Discard ,"static System.Console.BufferWidth.get")]
	public extern static Number _cc68a62966893c78(object instance);

	[Jazor(Op.Discard ,"static System.Console.BufferWidth.set")]
	public extern static void _2475b6b64873b657(object instance, Number value);

	[Jazor(Op.Discard ,"static System.Console.BufferHeight.get")]
	public extern static Number _78f6a2e9149eac8d(object instance);

	[Jazor(Op.Discard ,"static System.Console.BufferHeight.set")]
	public extern static void _008d0a53c97269db(object instance, Number value);

	[Jazor(Op.Discard ,"static System.Console.SetBufferSize(int, int)")]
	public extern static void _2a4c1a9d52c1050c(Number width, Number height);

	[Jazor(Op.Discard ,"static System.Console.WindowLeft.get")]
	public extern static Number _3b5ec4626962a01d(object instance);

	[Jazor(Op.Discard ,"static System.Console.WindowLeft.set")]
	public extern static void _147bce3eaeb65540(object instance, Number value);

	[Jazor(Op.Discard ,"static System.Console.WindowTop.get")]
	public extern static Number _d3338ac931617887(object instance);

	[Jazor(Op.Discard ,"static System.Console.WindowTop.set")]
	public extern static void _d1df236b3e7c9311(object instance, Number value);

	[Jazor(Op.Discard ,"static System.Console.WindowWidth.get")]
	public extern static Number _c3b3156e4b50c6e5(object instance);

	[Jazor(Op.Discard ,"static System.Console.WindowWidth.set")]
	public extern static void _d09ec9051b3189ba(object instance, Number value);

	[Jazor(Op.Discard ,"static System.Console.WindowHeight.get")]
	public extern static Number _55f4118bad131829(object instance);

	[Jazor(Op.Discard ,"static System.Console.WindowHeight.set")]
	public extern static void _9536ac2a96b2c04a(object instance, Number value);

	[Jazor(Op.Discard ,"static System.Console.SetWindowPosition(int, int)")]
	public extern static void _5c2293ddf639923a(Number left, Number top);

	[Jazor(Op.Discard ,"static System.Console.SetWindowSize(int, int)")]
	public extern static void _a3a20a010844aaaf(Number width, Number height);

	[Jazor(Op.Discard ,"static System.Console.LargestWindowWidth.get")]
	public extern static Number _0e0b37952b8aeb15(object instance);

	[Jazor(Op.Discard ,"static System.Console.LargestWindowHeight.get")]
	public extern static Number _112081177aa5379c(object instance);

	[Jazor(Op.Discard ,"static System.Console.CursorVisible.get")]
	public extern static bool _2eac576eada0d018(object instance);

	[Jazor(Op.Discard ,"static System.Console.CursorVisible.set")]
	public extern static void _9992e675f25ec4f0(object instance, bool value);

	[Jazor(Op.Discard ,"static System.Console.CursorLeft.get")]
	public extern static Number _9e7c51b7e5446aa6(object instance);

	[Jazor(Op.Discard ,"static System.Console.CursorLeft.set")]
	public extern static void _6ccdbe86a5eeabfe(object instance, Number value);

	[Jazor(Op.Discard ,"static System.Console.CursorTop.get")]
	public extern static Number _5468de6baa878f47(object instance);

	[Jazor(Op.Discard ,"static System.Console.CursorTop.set")]
	public extern static void _95f880f91d18695f(object instance, Number value);

	[Jazor(Op.Discard ,"static System.Console.GetCursorPosition()")]
	public extern static (int, int) _b1cc96fdd3c7ac15();

	[Jazor(Op.Discard ,"static System.Console.Title.get")]
	public extern static string _0ec0aa8b5bf0d1c4(object instance);

	[Jazor(Op.Discard ,"static System.Console.Title.set")]
	public extern static void _7559c0c607fb023a(object instance, string value);

	[Jazor(Op.Discard ,"static System.Console.Beep()")]
	public extern static void _6abcd85656794fda();

	[Jazor(Op.Discard ,"static System.Console.Beep(int, int)")]
	public extern static void _e9811cc63b2cf680(Number frequency, Number duration);

	[Jazor(Op.Discard ,"static System.Console.MoveBufferArea(int, int, int, int, int, int)")]
	public extern static void _07404b431cdde8e2(Number sourceLeft, Number sourceTop, Number sourceWidth, Number sourceHeight, Number targetLeft, Number targetTop);

	[Jazor(Op.Discard ,"static System.Console.MoveBufferArea(int, int, int, int, int, int, char, System.ConsoleColor, System.ConsoleColor)")]
	public extern static void _1535b4df4471cca4(Number sourceLeft, Number sourceTop, Number sourceWidth, Number sourceHeight, Number targetLeft, Number targetTop, Number sourceChar, object sourceForeColor, object sourceBackColor);

	[Jazor(Op.Discard ,"static System.Console.Clear()")]
	public extern static void _7779d957d8f16481();

	[Jazor(Op.Discard ,"static System.Console.SetCursorPosition(int, int)")]
	public extern static void _6954937a857814c5(Number left, Number top);

	[Jazor(Op.Discard ,"static System.Console.add_CancelKeyPress(System.ConsoleCancelEventHandler)")]
	public extern static void _34be82877208b27f(object value);

	[Jazor(Op.Discard ,"static System.Console.remove_CancelKeyPress(System.ConsoleCancelEventHandler)")]
	public extern static void _a000a19fc5beb9ea(object value);

	[Jazor(Op.Discard ,"static System.Console.TreatControlCAsInput.get")]
	public extern static bool _8ab46f86bbb99616(object instance);

	[Jazor(Op.Discard ,"static System.Console.TreatControlCAsInput.set")]
	public extern static void _7cebd43b37fb9744(object instance, bool value);

	[Jazor(Op.Discard ,"static System.Console.OpenStandardInput()")]
	public extern static System.IO.Stream _dc750bd876ac132a();

	[Jazor(Op.Discard ,"static System.Console.OpenStandardInput(int)")]
	public extern static System.IO.Stream _fa1cb98329e3050e(Number bufferSize);

	[Jazor(Op.Discard ,"static System.Console.OpenStandardOutput()")]
	public extern static System.IO.Stream _a09b71061a11329a();

	[Jazor(Op.Discard ,"static System.Console.OpenStandardOutput(int)")]
	public extern static System.IO.Stream _17ea088e4471485a(Number bufferSize);

	[Jazor(Op.Discard ,"static System.Console.OpenStandardError()")]
	public extern static System.IO.Stream _fb73e1943bd8a33b();

	[Jazor(Op.Discard ,"static System.Console.OpenStandardError(int)")]
	public extern static System.IO.Stream _50377d8e27ed372b(Number bufferSize);

	[Jazor(Op.Discard ,"static System.Console.SetIn(System.IO.TextReader)")]
	public extern static void _0d6b03c10896e4ba(object newIn);

	[Jazor(Op.Discard ,"static System.Console.SetOut(System.IO.TextWriter)")]
	public extern static void _ca2b7b86f66acd90(object newOut);

	[Jazor(Op.Discard ,"static System.Console.SetError(System.IO.TextWriter)")]
	public extern static void _76212a2c41bdd5da(object newError);

	[Jazor(Op.Discard ,"static System.Console.Read()")]
	public extern static Number _1a344364f77ef0d3();

	[Jazor(Op.Discard ,"static System.Console.ReadLine()")]
	public extern static string? _d665efe65ee40f12();

	[Jazor(Op.Discard ,"static System.Console.WriteLine()")]
	public extern static void _64a3c7e35feaa9f0();

	[Jazor(Op.Discard ,"static System.Console.WriteLine(bool)")]
	public extern static void _0657067880cafdd2(bool value);

	[Jazor(Op.Discard ,"static System.Console.WriteLine(char)")]
	public extern static void _5a138b02870324cb(Number value);

	[Jazor(Op.Discard ,"static System.Console.WriteLine(char[])")]
	public extern static void _bfd6ae4fc98a90ff(object buffer);

	[Jazor(Op.Discard ,"static System.Console.WriteLine(char[], int, int)")]
	public extern static void _460b8c2943875e7e(object buffer, Number index, Number count);

	[Jazor(Op.Discard ,"static System.Console.WriteLine(System.Decimal)")]
	public extern static void _06770dfb1e3ad0be(object value);

	[Jazor(Op.Discard ,"static System.Console.WriteLine(double)")]
	public extern static void _b457fd5c1c5f9568(Number value);

	[Jazor(Op.Discard ,"static System.Console.WriteLine(float)")]
	public extern static void _38bc406a617b49ca(Number value);

	[Jazor(Op.Discard ,"static System.Console.WriteLine(int)")]
	public extern static void _8f3980b4b82b99ac(Number value);

	[Jazor(Op.Discard ,"static System.Console.WriteLine(uint)")]
	public extern static void _029fda9e4e9b254f(Number value);

	[Jazor(Op.Discard ,"static System.Console.WriteLine(long)")]
	public extern static void _0a1213ea041262ff(BigInt value);

	[Jazor(Op.Discard ,"static System.Console.WriteLine(ulong)")]
	public extern static void _28da774493da8a92(BigInt value);

	[Jazor(Op.Discard ,"static System.Console.WriteLine(object)")]
	public extern static void _b1dc4a6e5df341aa(object? value);

	[Jazor(Op.Discard ,"static System.Console.WriteLine(string)")]
	public extern static void _19f2583beee4f7fb(string? value);

	[Jazor(Op.Discard ,"static System.Console.WriteLine(System.ReadOnlySpan<char>)")]
	public extern static void _fd102c4488f2b5f3(Uint32Array value);

	[Jazor(Op.Discard ,"static System.Console.WriteLine(string, object)")]
	public extern static void _c4e6acf24771bb66(string format, object? arg0);

	[Jazor(Op.Discard ,"static System.Console.WriteLine(string, object, object)")]
	public extern static void _f5c74fa705a4f0b9(string format, object? arg0, object? arg1);

	[Jazor(Op.Discard ,"static System.Console.WriteLine(string, object, object, object)")]
	public extern static void _0fa26d5cd312b8d3(string format, object? arg0, object? arg1, object? arg2);

	[Jazor(Op.Discard ,"static System.Console.WriteLine(string, params object[])")]
	public extern static void _7a73fda86982983f(string format,  object arg);

	[Jazor(Op.Discard ,"static System.Console.WriteLine(string, params System.ReadOnlySpan<object>)")]
	public extern static void _e2406d27d341e50b(string format,  object arg);

	[Jazor(Op.Discard ,"static System.Console.Write(string, object)")]
	public extern static void _961ab9b501a6baf0(string format, object? arg0);

	[Jazor(Op.Discard ,"static System.Console.Write(string, object, object)")]
	public extern static void _33daccffa622fc66(string format, object? arg0, object? arg1);

	[Jazor(Op.Discard ,"static System.Console.Write(string, object, object, object)")]
	public extern static void _366c851b7a360959(string format, object? arg0, object? arg1, object? arg2);

	[Jazor(Op.Discard ,"static System.Console.Write(string, params object[])")]
	public extern static void _bdb97b77edce5259(string format,  object arg);

	[Jazor(Op.Discard ,"static System.Console.Write(string, params System.ReadOnlySpan<object>)")]
	public extern static void _4a291949ebb466b9(string format,  object arg);

	[Jazor(Op.Discard ,"static System.Console.Write(bool)")]
	public extern static void _a4ba329944e98b1c(bool value);

	[Jazor(Op.Discard ,"static System.Console.Write(char)")]
	public extern static void _c61ec50b9f9538a3(Number value);

	[Jazor(Op.Discard ,"static System.Console.Write(char[])")]
	public extern static void _aa7978304cd0bacc(object buffer);

	[Jazor(Op.Discard ,"static System.Console.Write(char[], int, int)")]
	public extern static void _10c4068d62648fb5(object buffer, Number index, Number count);

	[Jazor(Op.Discard ,"static System.Console.Write(double)")]
	public extern static void _c7002c416a3da063(Number value);

	[Jazor(Op.Discard ,"static System.Console.Write(System.Decimal)")]
	public extern static void _c37cf10f8516d6b7(object value);

	[Jazor(Op.Discard ,"static System.Console.Write(float)")]
	public extern static void _80304f087568bfd4(Number value);

	[Jazor(Op.Discard ,"static System.Console.Write(int)")]
	public extern static void _9aeb4b39f93efc70(Number value);

	[Jazor(Op.Discard ,"static System.Console.Write(uint)")]
	public extern static void _e31e2ab80d13cd13(Number value);

	[Jazor(Op.Discard ,"static System.Console.Write(long)")]
	public extern static void _8950a34699a5bdf8(BigInt value);

	[Jazor(Op.Discard ,"static System.Console.Write(ulong)")]
	public extern static void _e8ba4c4ca492d5a8(BigInt value);

	[Jazor(Op.Discard ,"static System.Console.Write(object)")]
	public extern static void _134c0342866ed156(object? value);

	[Jazor(Op.Discard ,"static System.Console.Write(string)")]
	public extern static void _89898d51245a9c64(string? value);

	[Jazor(Op.Discard ,"static System.Console.Write(System.ReadOnlySpan<char>)")]
	public extern static void _ec7a704092bf9982(Uint32Array value);
}
