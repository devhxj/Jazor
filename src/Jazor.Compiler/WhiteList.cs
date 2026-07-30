using ECMAScript.Contract;

namespace Jazor.Compiler;

partial interface IWhiteList { }

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

	public WhiteListValue(Op op) => Op = op;

	public WhiteListValue(Op op, string? value) => (Op, Value) = (op, value);

	public WhiteListValue(Op op, string? value, string? path) => (Op, Value, Path) = (op, value, path);
}

/// <summary>
/// 保存由生成器产生并由 SemanticWalker 消费的类型、成员和 Compile 分发表。
/// </summary>
/// <remarks>
/// 生成的 partial 文件负责填充静态映射；Gate 保护运行时初始化过程。不要手工修改生成文件，
/// 也不要在这里加入 lookup 侧的 key 改写规则。
/// </remarks>
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
