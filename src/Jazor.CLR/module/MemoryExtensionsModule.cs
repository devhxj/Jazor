namespace Jazor.CLR;

/// <summary>
/// System.MemoryExtensions 的常用 span 查询映射。
/// </summary>
/// <remarks>
/// ReadOnlySpan&lt;T&gt; 在此处是 Array carrier 的只读调用视图，不建立 CLR span 的地址或切片身份模型。
/// Razor SDK 的默认 imports 会优先将 array.Contains(value) 绑定到这个 BCL overload，因此它与
/// Enumerable.Contains 使用同一默认 EqualityComparer 语义。
/// </remarks>
[ECMAScriptModule("System/MemoryExtensionsModule.js")]
[Jazor(Op.Alias, "System.MemoryExtensions", "Array")]
public static class MemoryExtensionsModule<T>
{
	/// <summary>
	/// C#: ReadOnlySpan&lt;char&gt;.Trim()
	/// JS: materialize the supported carrier and return its trimmed value projection.
	/// </summary>
	/// <remarks>
	/// The value projection is deliberate: supported span values do not carry address, offset, or
	/// mutable backing-store identity in JavaScript. Empty/default input remains an empty carrier.
	/// </remarks>
	[Jazor(Op.Import, "System.ReadOnlySpan<char>.Trim()", "trim")]
	public static RuntimeModule.JReadOnlyCharSpan Trim(RuntimeModule.JReadOnlyCharSpan source)
		=> RuntimeModule.MaterializeReadOnlyCharSpan(source).Trim();

	[Jazor(Op.Import, "System.ReadOnlySpan<char>.Trim(char)", "trimCharacter")]
	public static RuntimeModule.JReadOnlyCharSpan TrimCharacter(
		RuntimeModule.JReadOnlyCharSpan source,
		Number trimChar)
		=> TrimCharacters(source, trimChar.ToString());

	[Jazor(Op.Import, "System.ReadOnlySpan<char>.Trim(System.ReadOnlySpan<char>)", "trimCharacters")]
	public static RuntimeModule.JReadOnlyCharSpan TrimCharacters(
		RuntimeModule.JReadOnlyCharSpan source,
		RuntimeModule.JReadOnlyCharSpan trimChars)
		=> TrimCharacterSet(source, trimChars, trimStart: true, trimEnd: true);

	[Jazor(Op.Import, "System.ReadOnlySpan<char>.TrimStart()", "trimStart")]
	public static RuntimeModule.JReadOnlyCharSpan TrimStart(RuntimeModule.JReadOnlyCharSpan source)
		=> RuntimeModule.MaterializeReadOnlyCharSpan(source).TrimStart();

	[Jazor(Op.Import, "System.ReadOnlySpan<char>.TrimStart(char)", "trimStartCharacter")]
	public static RuntimeModule.JReadOnlyCharSpan TrimStartCharacter(
		RuntimeModule.JReadOnlyCharSpan source,
		Number trimChar)
		=> TrimCharacterSet(source, trimChar.ToString(), trimStart: true, trimEnd: false);

	[Jazor(Op.Import, "System.ReadOnlySpan<char>.TrimStart(System.ReadOnlySpan<char>)", "trimStartCharacters")]
	public static RuntimeModule.JReadOnlyCharSpan TrimStartCharacters(
		RuntimeModule.JReadOnlyCharSpan source,
		RuntimeModule.JReadOnlyCharSpan trimChars)
		=> TrimCharacterSet(source, trimChars, trimStart: true, trimEnd: false);

	[Jazor(Op.Import, "System.ReadOnlySpan<char>.TrimEnd()", "trimEnd")]
	public static RuntimeModule.JReadOnlyCharSpan TrimEnd(RuntimeModule.JReadOnlyCharSpan source)
		=> RuntimeModule.MaterializeReadOnlyCharSpan(source).TrimEnd();

	[Jazor(Op.Import, "System.ReadOnlySpan<char>.TrimEnd(char)", "trimEndCharacter")]
	public static RuntimeModule.JReadOnlyCharSpan TrimEndCharacter(
		RuntimeModule.JReadOnlyCharSpan source,
		Number trimChar)
		=> TrimCharacterSet(source, trimChar.ToString(), trimStart: false, trimEnd: true);

	[Jazor(Op.Import, "System.ReadOnlySpan<char>.TrimEnd(System.ReadOnlySpan<char>)", "trimEndCharacters")]
	public static RuntimeModule.JReadOnlyCharSpan TrimEndCharacters(
		RuntimeModule.JReadOnlyCharSpan source,
		RuntimeModule.JReadOnlyCharSpan trimChars)
		=> TrimCharacterSet(source, trimChars, trimStart: false, trimEnd: true);

	[Jazor(Op.Import, "static System.MemoryExtensions.Contains<T>(System.ReadOnlySpan<T>, T)")]
	public static bool _a4ed2b50c69946de(Array<T> source, T value)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");

		for (Number index = 0; index < source.Length; index++)
		{
			if (EqualityComparerT1Module<T>.EqualsCore(source[index], value))
				return true;
		}

		return false;
	}

	/// <summary>
	/// C#: ReadOnlySpan&lt;T&gt;.SequenceEqual(ReadOnlySpan&lt;T&gt;)
	/// JS: 两个 Array carrier 的默认相等性逐项比较。
	/// </summary>
	/// <remarks>
	/// SDK 的默认 imports 会将数组实例调用绑定到这个 ReadOnlySpan 扩展成员。
	/// 这不是 span 地址模型：只承诺已 materialize Array 的长度和 index 语义。
	/// </remarks>
	[Jazor(Op.Import, "System.ReadOnlySpan<T>.SequenceEqual<T>(System.ReadOnlySpan<T>)", "sequenceEqual")]
	public static bool SequenceEqual(Array<T> first, Array<T> second)
	{
		if (first == null)
			throw new Error("ArgumentNullException: first is null");
		if (second == null)
			throw new Error("ArgumentNullException: second is null");
		if (first.Length != second.Length)
			return false;

		for (Number index = 0; index < first.Length; index++)
		{
			if (!EqualityComparerT1Module<T>.EqualsCore(first[index], second[index]))
				return false;
		}

		return true;
	}

	private static RuntimeModule.JReadOnlyCharSpan TrimCharacterSet(
		RuntimeModule.JReadOnlyCharSpan source,
		RuntimeModule.JReadOnlyCharSpan trimChars,
		bool trimStart,
		bool trimEnd)
	{
		var text = RuntimeModule.MaterializeReadOnlyCharSpan(source);
		var characters = RuntimeModule.MaterializeReadOnlyCharSpan(trimChars);
		if (characters.Length == 0)
			return text;

		var start = 0;
		var end = text.Length - 1;
		if (trimStart)
		{
			while (start <= end && characters.IndexOf(text[start].ToString()) >= 0)
				start++;
		}
		if (trimEnd)
		{
			while (end >= start && characters.IndexOf(text[end].ToString()) >= 0)
				end--;
		}

		if (start == 0 && end == text.Length - 1)
			return text;
		return start > end ? string.Empty : text.Substring(start, end - start + 1);
	}
}
