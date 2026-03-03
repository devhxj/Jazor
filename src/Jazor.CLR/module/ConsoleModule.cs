namespace Jazor.CLR;

/// <summary>
/// System.Console 类型模块映射规则
///
/// C# Console 与 JavaScript console 的对应关系：
/// - C# Console 是静态类，JavaScript console 是全局对象
/// - Write/WriteLine → console.log/info/error/warn
/// - Clear → console.clear
///
/// Op 类型选择原则：
/// - Alias: JavaScript 有原生对应方法（如 WriteLine → log, Clear → clear）
/// - Discard: JavaScript 无对应概念（如输入、光标、窗口、颜色、编码等）
///
/// 类型映射：
/// - char → string（单字符字符串）
/// - char[] → string
/// - decimal → Number
/// - int/uint/float/double → Number
/// - long/ulong → BigInt
/// - object → object
/// </summary>
[ECMAScriptModule("System/ConsoleModule.js")]
[Jazor(Op.Alias, "System.Console", "console")]
public static class ConsoleModule
{
	#region Properties (JavaScript 不支持，使用 Discard)

	[Jazor(Op.Discard, "static System.Console.In.get")]
	public extern static System.IO.TextReader _880f0b69ee8eebd6();

	[Jazor(Op.Discard, "static System.Console.InputEncoding.get")]
	public extern static System.Text.Encoding _c11de5200bca2f20();

	[Jazor(Op.Discard, "static System.Console.InputEncoding.set")]
	public extern static void _009c1ec8c289d826(object value);

	[Jazor(Op.Discard, "static System.Console.OutputEncoding.get")]
	public extern static System.Text.Encoding _dff17221ba3fc15b();

	[Jazor(Op.Discard, "static System.Console.OutputEncoding.set")]
	public extern static void _ea327c486add9f0e(object value);

	[Jazor(Op.Discard, "static System.Console.KeyAvailable.get")]
	public extern static bool _11af9b5bd8f22c76();

	[Jazor(Op.Discard, "static System.Console.Out.get")]
	public extern static System.IO.TextWriter _2cd9efaaec703efd();

	[Jazor(Op.Discard, "static System.Console.Error.get")]
	public extern static System.IO.TextWriter _d71693970ee00fa2();

	[Jazor(Op.Discard, "static System.Console.IsInputRedirected.get")]
	public extern static bool _a9228d076b7d6927();

	[Jazor(Op.Discard, "static System.Console.IsOutputRedirected.get")]
	public extern static bool _489b8bfc494baa29();

	[Jazor(Op.Discard, "static System.Console.IsErrorRedirected.get")]
	public extern static bool _8b2ee33dafe1f26b();

	[Jazor(Op.Discard, "static System.Console.CursorSize.get")]
	public extern static Number _7fb71d51c287d624();

	[Jazor(Op.Discard, "static System.Console.CursorSize.set")]
	public extern static void _a0a70f47636077e6(Number value);

	[Jazor(Op.Discard, "static System.Console.NumberLock.get")]
	public extern static bool _b1d96a12bbcd09fc();

	[Jazor(Op.Discard, "static System.Console.CapsLock.get")]
	public extern static bool _2873ea11d4e971df();

	[Jazor(Op.Discard, "static System.Console.BackgroundColor.get")]
	public extern static System.ConsoleColor _b2913bd8e903362d();

	[Jazor(Op.Discard, "static System.Console.BackgroundColor.set")]
	public extern static void _4ee40b9f5d29e242(object value);

	[Jazor(Op.Discard, "static System.Console.ForegroundColor.get")]
	public extern static System.ConsoleColor _c467047cce83b4a2();

	[Jazor(Op.Discard, "static System.Console.ForegroundColor.set")]
	public extern static void _9c9ee8009ecae95d(object value);

	[Jazor(Op.Discard, "static System.Console.BufferWidth.get")]
	public extern static Number _cc68a62966893c78();

	[Jazor(Op.Discard, "static System.Console.BufferWidth.set")]
	public extern static void _2475b6b64873b657(Number value);

	[Jazor(Op.Discard, "static System.Console.BufferHeight.get")]
	public extern static Number _78f6a2e9149eac8d();

	[Jazor(Op.Discard, "static System.Console.BufferHeight.set")]
	public extern static void _008d0a53c97269db(Number value);

	[Jazor(Op.Discard, "static System.Console.WindowLeft.get")]
	public extern static Number _3b5ec4626962a01d();

	[Jazor(Op.Discard, "static System.Console.WindowLeft.set")]
	public extern static void _147bce3eaeb65540(Number value);

	[Jazor(Op.Discard, "static System.Console.WindowTop.get")]
	public extern static Number _d3338ac931617887();

	[Jazor(Op.Discard, "static System.Console.WindowTop.set")]
	public extern static void _d1df236b3e7c9311(Number value);

	[Jazor(Op.Discard, "static System.Console.WindowWidth.get")]
	public extern static Number _c3b3156e4b50c6e5();

	[Jazor(Op.Discard, "static System.Console.WindowWidth.set")]
	public extern static void _d09ec9051b3189ba(Number value);

	[Jazor(Op.Discard, "static System.Console.WindowHeight.get")]
	public extern static Number _55f4118bad131829();

	[Jazor(Op.Discard, "static System.Console.WindowHeight.set")]
	public extern static void _9536ac2a96b2c04a(Number value);

	[Jazor(Op.Discard, "static System.Console.LargestWindowWidth.get")]
	public extern static Number _0e0b37952b8aeb15();

	[Jazor(Op.Discard, "static System.Console.LargestWindowHeight.get")]
	public extern static Number _112081177aa5379c();

	[Jazor(Op.Discard, "static System.Console.CursorVisible.get")]
	public extern static bool _2eac576eada0d018();

	[Jazor(Op.Discard, "static System.Console.CursorVisible.set")]
	public extern static void _9992e675f25ec4f0(bool value);

	[Jazor(Op.Discard, "static System.Console.CursorLeft.get")]
	public extern static Number _9e7c51b7e5446aa6();

	[Jazor(Op.Discard, "static System.Console.CursorLeft.set")]
	public extern static void _6ccdbe86a5eeabfe(Number value);

	[Jazor(Op.Discard, "static System.Console.CursorTop.get")]
	public extern static Number _5468de6baa878f47();

	[Jazor(Op.Discard, "static System.Console.CursorTop.set")]
	public extern static void _95f880f91d18695f(Number value);

	[Jazor(Op.Discard, "static System.Console.Title.get")]
	public extern static string _0ec0aa8b5bf0d1c4();

	[Jazor(Op.Discard, "static System.Console.Title.set")]
	public extern static void _7559c0c607fb023a(string value);

	[Jazor(Op.Discard, "static System.Console.TreatControlCAsInput.get")]
	public extern static bool _8ab46f86bbb99616();

	[Jazor(Op.Discard, "static System.Console.TreatControlCAsInput.set")]
	public extern static void _7cebd43b37fb9744(bool value);

	#endregion

	#region Input Methods (JavaScript 不支持，使用 Discard)

	[Jazor(Op.Discard, "static System.Console.ReadKey()")]
	public extern static System.ConsoleKeyInfo _cdfbb2b6c7857da4();

	[Jazor(Op.Discard, "static System.Console.ReadKey(bool)")]
	public extern static System.ConsoleKeyInfo _009c6d5dd94f0728(bool intercept);

	[Jazor(Op.Discard, "static System.Console.Read()")]
	public extern static Number _1a344364f77ef0d3();

	[Jazor(Op.Discard, "static System.Console.ReadLine()")]
	public extern static string? _d665efe65ee40f12();

	#endregion

	#region Output Methods (支持，使用 Alias 映射到 console.log)

	/// <summary>
	/// C#: Console.WriteLine()
	/// JS: console.log()
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.WriteLine()", "log")]
	public extern static void _64a3c7e35feaa9f0();

	/// <summary>
	/// C#: Console.WriteLine(bool)
	/// JS: console.log(value)
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.WriteLine(bool)", "log")]
	public extern static void _0657067880cafdd2(bool value);

	/// <summary>
	/// C#: Console.WriteLine(char)
	/// JS: console.log(value)
	/// char 映射为 string（单字符字符串）
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.WriteLine(char)", "log")]
	public extern static void _5a138b02870324cb(string value);

	/// <summary>
	/// C#: Console.WriteLine(char[])
	/// JS: console.log(value)
	/// char[] 映射为 string
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.WriteLine(char[])", "log")]
	public extern static void _bfd6ae4fc98a90ff(string buffer);

	/// <summary>
	/// C#: Console.WriteLine(char[], int, int)
	/// JS: console.log(buffer.substring(index, index + count))
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.WriteLine(char[], int, int)", "log")]
	public extern static void _460b8c2943875e7e(string buffer, Number index, Number count);

	/// <summary>
	/// C#: Console.WriteLine(decimal)
	/// JS: console.log(value)
	/// decimal 映射为 Number
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.WriteLine(System.Decimal)", "log")]
	public extern static void _06770dfb1e3ad0be(Number value);

	/// <summary>
	/// C#: Console.WriteLine(double)
	/// JS: console.log(value)
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.WriteLine(double)", "log")]
	public extern static void _b457fd5c1c5f9568(Number value);

	/// <summary>
	/// C#: Console.WriteLine(float)
	/// JS: console.log(value)
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.WriteLine(float)", "log")]
	public extern static void _38bc406a617b49ca(Number value);

	/// <summary>
	/// C#: Console.WriteLine(int)
	/// JS: console.log(value)
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.WriteLine(int)", "log")]
	public extern static void _8f3980b4b82b99ac(Number value);

	/// <summary>
	/// C#: Console.WriteLine(uint)
	/// JS: console.log(value)
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.WriteLine(uint)", "log")]
	public extern static void _029fda9e4e9b254f(Number value);

	/// <summary>
	/// C#: Console.WriteLine(long)
	/// JS: console.log(value)
	/// long 映射为 BigInt
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.WriteLine(long)", "log")]
	public extern static void _0a1213ea041262ff(BigInt value);

	/// <summary>
	/// C#: Console.WriteLine(ulong)
	/// JS: console.log(value)
	/// ulong 映射为 BigInt
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.WriteLine(ulong)", "log")]
	public extern static void _28da774493da8a92(BigInt value);

	/// <summary>
	/// C#: Console.WriteLine(object)
	/// JS: console.log(value)
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.WriteLine(object)", "log")]
	public extern static void _b1dc4a6e5df341aa(object? value);

	/// <summary>
	/// C#: Console.WriteLine(string)
	/// JS: console.log(value)
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.WriteLine(string)", "log")]
	public extern static void _19f2583beee4f7fb(string? value);

	/// <summary>
	/// C#: Console.WriteLine(ReadOnlySpan<char>)
	/// JS: console.log(value)
	/// ReadOnlySpan<char> 映射为 Uint32Array
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.WriteLine(System.ReadOnlySpan<char>)", "log")]
	public extern static void _fd102c4488f2b5f3(Uint32Array value);

	/// <summary>
	/// C#: Console.WriteLine(string, object)
	/// JS: 需要格式转换，C# 用 {0} 占位符，JS 用 %s/%d 等
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.WriteLine(string, object)", "log")]
	public extern static void _c4e6acf24771bb66(string format, object? arg0);

	/// <summary>
	/// C#: Console.WriteLine(string, object, object)
	/// JS: 需要格式转换
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.WriteLine(string, object, object)", "log")]
	public extern static void _f5c74fa705a4f0b9(string format, object? arg0, object? arg1);

	/// <summary>
	/// C#: Console.WriteLine(string, object, object, object)
	/// JS: 需要格式转换
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.WriteLine(string, object, object, object)", "log")]
	public extern static void _0fa26d5cd312b8d3(string format, object? arg0, object? arg1, object? arg2);

	/// <summary>
	/// C#: Console.WriteLine(string, params object[])
	/// JS: 需要格式转换
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.WriteLine(string, params object[])", "log")]
	public extern static void _7a73fda86982983f(string format, object arg);

	/// <summary>
	/// C#: Console.WriteLine(string, params ReadOnlySpan<object>)
	/// JS: 需要格式转换
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.WriteLine(string, params System.ReadOnlySpan<object>)", "log")]
	public extern static void _e2406d27d341e50b(string format, object arg);

	#endregion

	#region Write Methods (支持，使用 Alias 映射到 console 方法)

	/// <summary>
	/// C#: Console.Write(string)
	/// JS: console.log(value) (注意：JS 的 log 会换行，与 C# Write 不完全一致)
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.Write(string)", "log")]
	public extern static void _89898d51245a9c64(string? value);

	/// <summary>
	/// C#: Console.Write(bool)
	/// JS: console.log(value)
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.Write(bool)", "log")]
	public extern static void _a4ba329944e98b1c(bool value);

	/// <summary>
	/// C#: Console.Write(char)
	/// JS: console.log(value)
	/// char 映射为 string
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.Write(char)", "log")]
	public extern static void _c61ec50b9f9538a3(string value);

	/// <summary>
	/// C#: Console.Write(char[])
	/// JS: console.log(buffer)
	/// char[] 映射为 string
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.Write(char[])", "log")]
	public extern static void _aa7978304cd0bacc(string buffer);

	/// <summary>
	/// C#: Console.Write(char[], int, int)
	/// JS: console.log(buffer.substring(index, index + count))
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.Write(char[], int, int)", "log")]
	public extern static void _10c4068d62648fb5(string buffer, Number index, Number count);

	/// <summary>
	/// C#: Console.Write(double)
	/// JS: console.log(value)
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.Write(double)", "log")]
	public extern static void _c7002c416a3da063(Number value);

	/// <summary>
	/// C#: Console.Write(decimal)
	/// JS: console.log(value)
	/// decimal 映射为 Number
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.Write(System.Decimal)", "log")]
	public extern static void _c37cf10f8516d6b7(Number value);

	/// <summary>
	/// C#: Console.Write(float)
	/// JS: console.log(value)
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.Write(float)", "log")]
	public extern static void _80304f087568bfd4(Number value);

	/// <summary>
	/// C#: Console.Write(int)
	/// JS: console.log(value)
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.Write(int)", "log")]
	public extern static void _9aeb4b39f93efc70(Number value);

	/// <summary>
	/// C#: Console.Write(uint)
	/// JS: console.log(value)
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.Write(uint)", "log")]
	public extern static void _e31e2ab80d13cd13(Number value);

	/// <summary>
	/// C#: Console.Write(long)
	/// JS: console.log(value)
	/// long 映射为 BigInt
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.Write(long)", "log")]
	public extern static void _8950a34699a5bdf8(BigInt value);

	/// <summary>
	/// C#: Console.Write(ulong)
	/// JS: console.log(value)
	/// ulong 映射为 BigInt
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.Write(ulong)", "log")]
	public extern static void _e8ba4c4ca492d5a8(BigInt value);

	/// <summary>
	/// C#: Console.Write(object)
	/// JS: console.log(value)
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.Write(object)", "log")]
	public extern static void _134c0342866ed156(object? value);

	/// <summary>
	/// C#: Console.Write(ReadOnlySpan<char>)
	/// JS: console.log(value)
	/// ReadOnlySpan<char> 映射为 Uint32Array
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.Write(System.ReadOnlySpan<char>)", "log")]
	public extern static void _ec7a704092bf9982(Uint32Array value);

	/// <summary>
	/// C#: Console.Write(string, object)
	/// JS: 需要格式转换
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.Write(string, object)", "log")]
	public extern static void _961ab9b501a6baf0(string format, object? arg0);

	/// <summary>
	/// C#: Console.Write(string, object, object)
	/// JS: 需要格式转换
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.Write(string, object, object)", "log")]
	public extern static void _33daccffa622fc66(string format, object? arg0, object? arg1);

	/// <summary>
	/// C#: Console.Write(string, object, object, object)
	/// JS: 需要格式转换
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.Write(string, object, object, object)", "log")]
	public extern static void _366c851b7a360959(string format, object? arg0, object? arg1, object? arg2);

	/// <summary>
	/// C#: Console.Write(string, params object[])
	/// JS: 需要格式转换
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.Write(string, params object[])", "log")]
	public extern static void _bdb97b77edce5259(string format, object arg);

	/// <summary>
	/// C#: Console.Write(string, params ReadOnlySpan<object>)
	/// JS: 需要格式转换
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.Write(string, params System.ReadOnlySpan<object>)", "log")]
	public extern static void _4a291949ebb466b9(string format, object arg);

	#endregion

	#region Buffer/Window Methods (JavaScript 不支持，使用 Discard)

	[Jazor(Op.Discard, "static System.Console.SetBufferSize(int, int)")]
	public extern static void _2a4c1a9d52c1050c(Number width, Number height);

	[Jazor(Op.Discard, "static System.Console.SetWindowPosition(int, int)")]
	public extern static void _5c2293ddf639923a(Number left, Number top);

	[Jazor(Op.Discard, "static System.Console.SetWindowSize(int, int)")]
	public extern static void _a3a20a010844aaaf(Number width, Number height);

	[Jazor(Op.Discard, "static System.Console.GetCursorPosition()")]
	public extern static (int, int) _b1cc96fdd3c7ac15();

	#endregion

	#region Other Methods (部分支持)

	/// <summary>
	/// C#: Console.Clear()
	/// JS: console.clear()
	/// </summary>
	[Jazor(Op.Alias, "static System.Console.Clear()", "clear")]
	public extern static void _7779d957d8f16481();

	/// <summary>
	/// C#: Console.SetCursorPosition(int, int)
	/// JS: 不支持（浏览器控制台无此概念）
	/// </summary>
	[Jazor(Op.Discard, "static System.Console.SetCursorPosition(int, int)")]
	public extern static void _6954937a857814c5(Number left, Number top);

	[Jazor(Op.Discard, "static System.Console.Beep()")]
	public extern static void _6abcd85656794fda();

	[Jazor(Op.Discard, "static System.Console.Beep(int, int)")]
	public extern static void _e9811cc63b2cf680(Number frequency, Number duration);

	[Jazor(Op.Discard, "static System.Console.MoveBufferArea(int, int, int, int, int, int)")]
	public extern static void _07404b431cdde8e2(Number sourceLeft, Number sourceTop, Number sourceWidth, Number sourceHeight, Number targetLeft, Number targetTop);

	[Jazor(Op.Discard, "static System.Console.MoveBufferArea(int, int, int, int, int, int, char, ConsoleColor, ConsoleColor)")]
	public extern static void _1535b4df4471cca4(Number sourceLeft, Number sourceTop, Number sourceWidth, Number sourceHeight, Number targetLeft, Number targetTop, string sourceChar, object sourceForeColor, object sourceBackColor);

	[Jazor(Op.Discard, "static System.Console.ResetColor()")]
	public extern static void _1c7916d8deb9d83b();

	#endregion

	#region Event Handlers (JavaScript 不支持，使用 Discard)

	[Jazor(Op.Discard, "static System.Console.add_CancelKeyPress(System.ConsoleCancelEventHandler)")]
	public extern static void _34be82877208b27f(object value);

	[Jazor(Op.Discard, "static System.Console.remove_CancelKeyPress(System.ConsoleCancelEventHandler)")]
	public extern static void _a000a19fc5beb9ea(object value);

	#endregion

	#region Stream Methods (JavaScript 不支持，使用 Discard)

	[Jazor(Op.Discard, "static System.Console.OpenStandardInput()")]
	public extern static System.IO.Stream _dc750bd876ac132a();

	[Jazor(Op.Discard, "static System.Console.OpenStandardInput(int)")]
	public extern static System.IO.Stream _fa1cb98329e3050e(Number bufferSize);

	[Jazor(Op.Discard, "static System.Console.OpenStandardOutput()")]
	public extern static System.IO.Stream _a09b71061a11329a();

	[Jazor(Op.Discard, "static System.Console.OpenStandardOutput(int)")]
	public extern static System.IO.Stream _17ea088e4471485a(Number bufferSize);

	[Jazor(Op.Discard, "static System.Console.OpenStandardError()")]
	public extern static System.IO.Stream _fb73e1943bd8a33b();

	[Jazor(Op.Discard, "static System.Console.OpenStandardError(int)")]
	public extern static System.IO.Stream _50377d8e27ed372b(Number bufferSize);

	[Jazor(Op.Discard, "static System.Console.SetIn(System.IO.TextReader)")]
	public extern static void _0d6b03c10896e4ba(object newIn);

	[Jazor(Op.Discard, "static System.Console.SetOut(System.IO.TextWriter)")]
	public extern static void _ca2b7b86f66acd90(object newOut);

	[Jazor(Op.Discard, "static System.Console.SetError(System.IO.TextWriter)")]
	public extern static void _76212a2c41bdd5da(object newError);

	#endregion
}
