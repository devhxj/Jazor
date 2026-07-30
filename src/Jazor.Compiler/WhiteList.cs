using ECMAScript.Contract;

namespace Jazor.Compiler;

partial interface IWhiteList { }

internal sealed record RuntimeValueCarrierReference(string Name, string Path);

internal sealed class WhiteListValue
{
	public Op Op { get; }

	public string? Value { get; }

	public string? Path { get; }

	public RuntimeValueCarrierReference? RuntimeValueCarrier { get; }

	public WhiteListValue(Op op) => Op = op;

	public WhiteListValue(Op op, string? value) => (Op, Value) = (op, value);

	public WhiteListValue(Op op, string? value, string? path) => (Op, Value, Path) = (op, value, path);

	public WhiteListValue(
		Op op,
		string? value,
		string? path,
		RuntimeValueCarrierReference runtimeValueCarrier)
		=> (Op, Value, Path, RuntimeValueCarrier) = (op, value, path, runtimeValueCarrier);
}

internal static partial class WhiteList
{
	private static readonly object Gate = new();

	public static readonly Dictionary<string, WhiteListValue> Types;

	public static readonly Dictionary<string, WhiteListValue> Members;

	static WhiteList()
	{
		Types = [];
		Members = [];
		Generate(ref Types, ref Members);
	}

	internal static void ReplaceForCurrentProcess(
		IEnumerable<KeyValuePair<string, WhiteListValue>> types,
		IEnumerable<KeyValuePair<string, WhiteListValue>> members)
	{
		lock (Gate)
		{
			Types.Clear();
			foreach (var pair in types)
				Types[pair.Key] = pair.Value;

			Members.Clear();
			foreach (var pair in members)
				Members[pair.Key] = pair.Value;
		}
	}

	static partial void Generate(ref Dictionary<string, WhiteListValue> types, ref Dictionary<string, WhiteListValue> members);
}
