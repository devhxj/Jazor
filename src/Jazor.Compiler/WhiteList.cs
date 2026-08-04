using ECMAScript.Contract;

namespace Jazor.Compiler;

partial interface IWhiteList { }

internal sealed record RuntimeValueCarrierReference(string Name, string Path);

internal sealed record WhiteListCatalog(
	Dictionary<string, WhiteListValue> Types,
	Dictionary<string, WhiteListValue> Members);

/// <summary>
/// 表示一条编译器消费侧白名单记录。
/// </summary>
/// <remarks>
/// Op 决定映射策略，Value 保存 alias 或 inline 内容，Path 保存 import 模块路径。
/// 该对象只表达已生成的白名单数据，不负责执行 lookup 兼容探测；探测逻辑集中在
/// <see cref="WhiteListLookup"/> 中。
/// </remarks>
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

/// <summary>
/// 保存由生成器产生并由 SemanticWalker 消费的类型、成员和 Compile 分发表。
/// </summary>
/// <remarks>
/// 生成的 partial 文件负责填充初始映射；generator 刷新源码后会整体替换进程内快照。
/// 不要手工修改生成文件，也不要在这里加入 lookup 侧的 key 改写规则。
/// </remarks>
internal static partial class WhiteList
{
	private static WhiteListCatalog _catalog;

	public static Dictionary<string, WhiteListValue> Types => _catalog.Types;

	public static Dictionary<string, WhiteListValue> Members => _catalog.Members;

	static WhiteList()
	{
		Dictionary<string, WhiteListValue> types = [];
		Dictionary<string, WhiteListValue> members = [];
		Generate(ref types, ref members);
		_catalog = new(types, members);
	}

	// Generator 先刷新白名单源码，再在同一进程生成 CLR runtime catalog。
	// 整体替换快照可避免读者观察到 Types/Members 只更新一半的中间状态。
	internal static void ReplaceForCurrentProcess(
		IEnumerable<KeyValuePair<string, WhiteListValue>> types,
		IEnumerable<KeyValuePair<string, WhiteListValue>> members)
		=> _catalog = new(
			types.ToDictionary(static pair => pair.Key, static pair => pair.Value, StringComparer.Ordinal),
			members.ToDictionary(static pair => pair.Key, static pair => pair.Value, StringComparer.Ordinal));

	static partial void Generate(ref Dictionary<string, WhiteListValue> types, ref Dictionary<string, WhiteListValue> members);
}
