# LookupT2Module.cs

`System.Linq.ILookup<TKey, TElement>` 映射为 materialized grouping `Array` carrier：外层 Array
按 key 首次出现顺序保存各个 group，内部 group 继续由 `GroupingT2Module` 创建，并通过私有
metadata 保留 CLR-facing `Key`。

`EnumerableModule` 统一提供 `ToLookup` 及 `ILookup.Count`、`Contains(TKey)`、indexer getter 的
runtime members。key 查询使用 `EqualityComparer<TKey>`，因此 `NaN` 和有符号零与 `GroupBy`、
`Join`、`Distinct` 的默认相等性协议一致，不使用 JavaScript `Map` 的键规则。

- `ToLookup(source, keySelector)` 与 `ToLookup(source, keySelector, elementSelector)` 一次物化 source；每个 selector 对实际 source 项恰好调用一次。
- `Count` 返回 group 数量；`Contains` 查询已存在 key；indexer 对缺失 key 返回新的空 Array carrier。
- `ILookup` 可作为 `IEnumerable<IGrouping<TKey, TElement>>` 消费，group 元素与 key metadata 保持已有 `GroupingT2Module` 契约。
- comparer overload 尚未映射；模块不承诺 BCL lookup 的延迟构建或运行时 identity。
