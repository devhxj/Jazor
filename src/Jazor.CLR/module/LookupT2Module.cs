namespace Jazor.CLR;

/// <summary>
/// System.Linq.ILookup&lt;TKey, TElement&gt; 的 materialized grouping carrier 映射。
/// </summary>
/// <remarks>
/// lookup 本身是保持 group 首次出现顺序的 Array；每个元素继续是 GroupingT2Module
/// 创建并携带私有 key metadata 的 Array。成员语义由 EnumerableModule 统一实现，确保
/// ToLookup、GroupBy 和 lookup key 查询使用同一 EqualityComparer&lt;TKey&gt; 协议。
/// </remarks>
[ECMAScriptModule("System/Linq/LookupT2Module.js")]
[Jazor(Op.Alias, "System.Linq.ILookup<TKey, TElement>", "Array")]
public static class LookupT2Module<TKey, TElement>
{
}
