namespace Jazor.CLR;

/// <summary>
/// System.Linq.IGrouping&lt;TKey, TElement&gt; 的 Array carrier 映射。
/// </summary>
/// <remarks>
/// 分组元素保持为普通 JavaScript Array，以便 IEnumerable/IEnumerable&lt;T&gt; 消费方继续使用
/// 原生迭代协议。Key 属于 IGrouping 的附加 CLR 语义，放在模块私有 WeakMap 中，避免污染 Array
/// 实例，也不会与用户定义的数组字段发生冲突。
/// </remarks>
[ECMAScriptModule("System/Linq/GroupingT2Module.js")]
[Jazor(Op.Alias, "System.Linq.IGrouping<TKey, TElement>", "Array")]
public static class GroupingT2Module<TKey, TElement>
{
	private static readonly WeakMap<Array<TElement>, TKey> Keys = new();

	internal static Array<TElement> Create(TKey key)
	{
		var grouping = new Array<TElement>();
		Keys.Set(grouping, key);
		return grouping;
	}

	internal static TKey? GetKey(Array<TElement> grouping)
		=> Keys.Get(grouping);

	/// <summary>
	/// C#: grouping.Key
	/// JS: 从模块私有 carrier 元数据中读取组键。
	/// </summary>
	[Jazor(Op.Import, "System.Linq.IGrouping<TKey, TElement>.Key.get")]
	public static TKey? _44a1c9f2c4f246e9(Array<TElement> instance)
	{
		if (instance is null)
			throw new Error("NullReferenceException: instance is null.");

		return GetKey(instance);
	}
}
