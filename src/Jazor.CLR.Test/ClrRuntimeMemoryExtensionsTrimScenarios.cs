namespace Jazor.CLR.Test;

internal static class ClrRuntimeMemoryExtensionsTrimScenarios
{
	private const string ModulePath = "System/MemoryExtensionsModule.js";

	public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
	[
		Success("memory-extensions.trim.whitespace-string", "System.ReadOnlySpan<char>.Trim()", [Text("  Jazor  ")], Text("Jazor")),
		Success("memory-extensions.trim.character", "System.ReadOnlySpan<char>.Trim(char)", [Text("[[Jazor[["), Text("[")], Text("Jazor")),
		Success("memory-extensions.trim.character-set-array", "System.ReadOnlySpan<char>.Trim(System.ReadOnlySpan<char>)", [Array(Text("x"), Text("x"), Text("J"), Text("S"), Text("y")), Array(Text("x"), Text("y"))], Text("JS")),
		Success("memory-extensions.trim.default-source", "System.ReadOnlySpan<char>.Trim()", [Null()], Text("")),
		Success("memory-extensions.trim.empty-character-set-preserves-source", "System.ReadOnlySpan<char>.Trim(System.ReadOnlySpan<char>)", [Text("[Jazor]"), Array()], Text("[Jazor]")),
		Success("memory-extensions.trim-start.character-set", "System.ReadOnlySpan<char>.TrimStart(System.ReadOnlySpan<char>)", [Text("xxJazorxx"), Text("x")], Text("Jazorxx")),
		Success("memory-extensions.trim-start.whitespace-array", "System.ReadOnlySpan<char>.TrimStart()", [Array(Text(" "), Text("J"), Text("S"))], Text("JS")),
		Success("memory-extensions.trim-start.character", "System.ReadOnlySpan<char>.TrimStart(char)", [Text("--Jazor"), Text("-")], Text("Jazor")),
		Success("memory-extensions.trim-end.character-set", "System.ReadOnlySpan<char>.TrimEnd(System.ReadOnlySpan<char>)", [Text("xxJazorxy"), Array(Text("x"), Text("y"))], Text("xxJazor")),
		Success("memory-extensions.trim-end.whitespace-array", "System.ReadOnlySpan<char>.TrimEnd()", [Array(Text("J"), Text("S"), Text(" "))], Text("JS")),
		Success("memory-extensions.trim-end.character", "System.ReadOnlySpan<char>.TrimEnd(char)", [Text("Jazor--"), Text("-")], Text("Jazor"))
	];

	private static ClrRuntimeScenario Success(
		string id,
		string member,
		IReadOnlyList<ClrRuntimeValue> arguments,
		ClrRuntimeValue expected)
		=> new(id, member, ModulePath, arguments, expected);

	private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();

	private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);

	private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
}
