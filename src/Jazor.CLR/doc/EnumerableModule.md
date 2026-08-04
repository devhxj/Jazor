# EnumerableModule.cs

## 已支持的物化 LINQ 子集

`EnumerableModule<TSource>` 将高频 `System.Linq.Enumerable` 调用映射为明确物化的 JavaScript Array
操作。它不是完整 LINQ provider，也不承诺延迟枚举器或 `Queryable` 语义。

当前包含：

- 筛选与投影：`Where`、`Select`、`SelectMany`、`Concat`、`Append`、`Prepend`、`DefaultIfEmpty`
- source factory 与分页：`Empty`、`Range`、`Repeat`、`AsEnumerable`、`Sequence`、`Index`、`Skip`、`Take(int / Range)`、`SkipWhile`、`TakeWhile`、`SkipLast`、`TakeLast`、`Chunk`、`Zip`、`Any`、`All`、`Count`、`CountBy`、`LongCount`、`Contains`、`SequenceEqual`
- 终端选择：`ElementAt(int / Index)`、compiler-owned `ElementAtOrDefault(int / Index)`、`First` / `Last` / `Single` 及其 predicate overload、`FirstOrDefault` / `LastOrDefault` / `SingleOrDefault` 及其 predicate overload
- 聚合：`Aggregate(func)`、`Aggregate(seed, func)`、`Aggregate(seed, func, resultSelector)`、`AggregateBy`（固定 seed / key seed selector）
- 集合运算：`Distinct`、`DistinctBy`、`Union`、`Except`、`Intersect`、`UnionBy`、`ExceptBy`、`IntersectBy`
- 选择器终端：`MinBy`、`MaxBy`（默认 comparer overload）
- 数值终端：非 nullable `int`、`long`、`float`、`double`、`decimal` 的 `Min`、`Max`、`Sum`、`Average`，以及相同五种 carrier 的 direct nullable numeric terminals
- 分组与联接：`GroupBy`（key、element、result selector overload）、`ToLookup`（key、element selector overload）、`Join`、`GroupJoin`
- 排序：`Order`、`OrderDescending`、`OrderBy`、`OrderByDescending`、由本模块排序产物直接承接的 `ThenBy`、`ThenByDescending`
- 顺序反转：`Reverse`，返回新的 materialized Array，不改变输入 source
- 物化：`ToList`、`ToArray`

## 相等性与顺序

默认相等性集合不直接使用 JavaScript `Set` 或 `Map` 的键规则。实现先用
`EqualityComparer<T>.GetHashCode` 收窄 bucket，再用 `EqualityComparer<T>.Equals` 确认，因此
hash collision、`NaN` 与有符号零在 `Distinct`、集合运算、分组和联接中遵循同一 CLR adapter 合约。

- `Distinct` 保留首次出现值。
- `DistinctBy` 对每个 source 项调用 key selector 一次，按 key 的默认 `EqualityComparer<TKey>` 保留首项。
- `GroupBy` 的 key-only、element selector、result selector 和 element/result selector overload 均按同一 equality bucket 建立 stable grouping。result selector 仅在所有 source 的 key/element selector 完成后，按首 key 的 group 顺序调用一次；其 BCL `IEnumerable<TElement>` 参数使用已有 `IGrouping<TKey, TElement>` 的 Array carrier，保留 `Key` 私有 metadata 和 group 元素顺序。当前不支持 comparer overload。
- `ToLookup` 复用相同 grouping layout，并将 `ILookup<TKey, TElement>` 投影为外层 group Array。`Count`、`Contains` 和 indexer 都经 `EqualityComparer<TKey>` 查找；缺失 key 返回新的空 Array，而不是向 JavaScript `Map` 泄漏不一致的 key identity。当前不支持 comparer overload。
- `MinBy` / `MaxBy` 对每个 source 项调用 key selector 一次，以默认 `Comparer<TKey>` 选出极值，并保留首个并列 key 的 source 项。
- `Union` 保留第一序列的唯一值，再追加第二序列中新值。
- `Except` 先物化第二序列的排除集合，再按第一序列顺序输出唯一的未命中值。
- `Intersect` 先物化第二序列集合，再按第一序列顺序输出首次匹配值。
- `UnionBy` 先观察 first 再观察 second，使用 `EqualityComparer<TKey>` 保留每个 key 的首个 source representative；selector 对两个 source 的每个已观察项恰好调用一次。
- `ExceptBy` 先物化第二序列的 key 集合，再按 first 顺序输出 key 未命中且此前未输出的 source 项；`IntersectBy` 同样先物化 key 集合，并按 first 顺序输出每个命中 key 的首项。二者的第二序列是 `TKey`，不会错误地应用 source key selector。
- `SequenceEqual` 只接受当前 Array carrier，并按相同 index 使用默认 `EqualityComparer<T>` 同步比较；长度不同或首个不等项立即返回 `false`，不会改写输入。
- `Concat` 先完整枚举 first，再枚举 second，返回新的 Array，不调用 JavaScript `Array.concat` 以避免弱化 `IEnumerable<T>` 的可观察枚举顺序。
- `Append` 先完整枚举 source，再在新 Array 尾部写入 element；`Prepend` 先写入 element，再完整枚举 source。两者都不改写输入。
- `CountBy` 与 `AggregateBy` 复用 `GroupBy`/`Join` 的 comparer-aware hash bucket 协议：hash 仅缩小候选桶，`IEqualityComparer<TKey>` 仍决定相等性，因此碰撞、NaN、signed zero、首次 key representative 与插入顺序都遵循同一 CLR 合约。结果统一物化为 `[key, value]` carrier 的标准 `KeyValuePair<TKey, TValue>` entry sequence。`CountBy` 为每个等价 key 维持 Int32 count 并在溢出前失败；固定 seed 的 `AggregateBy` 将同一 seed 值传给每个新 key，key seed selector 只在每个新 key 首次出现时调用一次。当前三条 API 均使用 BCL 的 comparer 参数（省略时由 Roslyn 传入默认 null），不将相等性退化为 JavaScript Map key identity。
- `DefaultIfEmpty(source, defaultValue)` 先一次物化 source；仅当结果为空时写入 explicit fallback。无参 C# overload 由 compiler 在闭合调用点生成 `default(TSource)` 后进入这条 runtime contract，因此不会在 erased JavaScript generic 中猜测默认值。
- `FirstOrDefault` / `LastOrDefault` / `SingleOrDefault` 同样由 compiler 将无显式 fallback 的 C# overload 绑定为 explicit fallback runtime export。`First` 在首项或首个 predicate match 即停止；`Last` 按 source order 观察最终项或最终 match；`Single` 在第二项或第二个 match 时失败。
- `ElementAtOrDefault(source, int)` 由 compiler 在闭合调用点生成 `default(TSource)`，再以单次求值 IIFE 和 `for...of` 完成遍历。负 index、越界和空 source 返回该默认值；命中 index 后立即返回并关闭迭代器，不伪造 BCL 的 explicit-default overload。
- `ElementAtOrDefault(source, Index)` 由同一 Compile contract 按 bound parameter type 选择协议：from-start 仍在命中时关闭迭代器；from-end 完整枚举 source，并仅保留 Index 值大小的环形 tail buffer。Index 状态通过 `IndexModule` 白名单 getter 获取，compiler 不读取内部 carrier 字段；`^0`、空 source 和越界返回 closed `default(TSource)`。
- `Take(source, Range)` 使用 `JRange.GetOffsetAndLength` 的 checked offset/length 协议：source 先一次物化，再从新 Array 中按左闭右开范围切片，因此 `..n`、`m..^n`、`^m..` 和 `Range.All` 均保持 from-start/from-end 边界；inverted 或越界 range 抛出 `ArgumentOutOfRangeException`，输入不变。该模块维持 eager/materialized 契约，不承诺 BCL 的延迟 Take iterator。
- throwing `ElementAt(source, Index)` 经独立 `elementAtIndex` Import 进入 CLR runtime：from-start 命中即停止，from-end 使用相同的有界环形 tail traversal；`^0`、空 source 和两侧越界抛 `ArgumentOutOfRangeException`。该 overload 不需要 closed generic default，因此不增加 Compile 特例。
- `SkipLast(source, count)` 与 `TakeLast(source, count)` 在 runtime 使用固定大小环形 tail buffer，而不是要求 Array source。`SkipLast` 在 buffer 满后依次输出最旧项；`TakeLast` 完整观察 source 后按 ring 顺序物化尾项。`TakeLast(count <= 0)` 返回空且不枚举，`SkipLast(count <= 0)` 按本模块 eager contract 物化全部 source；两者都不改写输入。
- `Empty` 每次返回独立空 Array carrier；`Range(start, count)` 保留 Int32 的负 count/上界失败规则，zero count 不要求 start 可递增；`Repeat(element, count)` 在每个 result slot 保留同一 element reference 并拒绝负 count。`AsEnumerable` 直接返回原始 carrier（包括 null），不复制也不附加 runtime null guard。
- `Sequence(first, second, third)` 为每次调用创建新的三项 Array carrier，保持三个已绑定 C# 参数的求值顺序和值/引用身份。
- `Count` 保留 Int32 结果域，在第 `Int32.MaxValue + 1` 个匹配项抛出 `OverflowException`；`LongCount` 使用 `BigInt` carrier 计数，在第 `Int64.MaxValue + 1` 个匹配项抛出同类错误。两者均提供无 predicate 与 predicate overload，predicate 仅对实际 source 项调用一次。
- `Index` 按 source 顺序物化为 `(int Index, TSource Item)` tuple carrier。tuple 成员继续遵从全局 runtime naming，产物属性为 `index` / `item`；第 `Int32.MaxValue + 1` 个 source 项抛出 `OverflowException`。
- 数值 `Min` / `Max` 保留各自 carrier：`int` / `float` / `double` 使用 Number、`long` 使用 BigInt、`decimal` 保持现有 decimal carrier，不以字符串字典序或通用 JavaScript accumulator 近似。non-nullable 空序列抛出 `InvalidOperationException`，null source 抛出 `ArgumentNullException`。五种 non-nullable 与 nullable 数值结果的 `Func<TSource, TResult>` selector overload 都复用对应 carrier comparer，按 source 顺序对每个项恰好调用 selector 一次；null selector 抛出 `ArgumentNullException`。float/double 与当前 BCL 一致：`Min` 观察到 `NaN` 即返回 `NaN`；`Max` 跳过 `NaN`，但全 `NaN` source 仍返回 `NaN`。direct 或 selector 的 nullable result overload 忽略 null，空或全 null source 返回 null，并保留相同 Number/BigInt/decimal 比较和 float/double NaN 规则。comparer overload 尚未映射。
- 数值 `Sum` 使用同一 carrier 分层：`int` 在每次相加前检查 Int32 上下界，`long` 保持 BigInt 并检查 Int64 上下界，float 以 Number 累加后仅在返回时 `Math.fround`，double 保持 Number，decimal 通过已映射的 `decimal.Parse` / `decimal.Add` 维持精确 string-backed carrier。所有已支持 overload 的空 source 返回相应零值，null source 抛出 `ArgumentNullException`。五种 non-nullable 与 nullable 数值结果的 `Func<TSource, TResult>` selector overload 都复用这些 cores，按 source 顺序对每个项恰好调用 selector 一次；null selector 抛出 `ArgumentNullException`。direct 或 selector 的 nullable result overload 忽略 null，并在空或全 null source 上返回带零值的 `T?`；其 runtime carrier 仍是 Number、BigInt 或 decimal string，不以 JavaScript `null` 冒充零。
- 数值 `Average` 的 non-nullable overload 对空 source 抛出 `InvalidOperationException`。`int` / `long` 使用 checked Int64 BigInt accumulator，再转换为 Number 返回 double 结果；float / double 按宽 Number 累加，float 仅在最终结果 `fround`；decimal 经 `decimal.Add` 和 `decimal.Divide` 维持精确 carrier。`long` / decimal 的累加溢出不因最终平均值可表示而被掩盖。五种 non-nullable 与 nullable 数值结果的 `Func<TSource, TResult>` selector overload 保留相同 accumulator 与返回 carrier，并按 source 顺序对每个项恰好调用 selector 一次；null selector 抛出 `ArgumentNullException`。direct 或 selector 的 nullable result overload 忽略 null；无有效元素时返回 null，`int?` / `long?` 返回 `double?` Number carrier，float? / double? / decimal? 保持各自 nullable carrier。
- `SkipWhile` 在首个 predicate false 前只保留跳过状态；首个 false 项和后续项按 source 顺序进入新 Array，后续项不再调用 predicate。`TakeWhile` 在首个 false 项停止，并依赖 `foreach` 的 iterator-close 协议结束未知 source。indexed overload 只在 predicate 实际执行时使用原始 source index，不把后续未检查项误计入 callback。
- `Zip` 的两个 overload 由 compiler-owned ESTree protocol 降为新的 Array carrier。它按 first、second 顺序创建 JavaScript iterator，每轮先推进 first 再推进 second，任一侧结束即停止，并在 finally 中按 second、first 逆序关闭。无 selector overload 使用 Roslyn tuple 元素的运行时名称构造 pair；selector overload 不走 runtime string 模板。显式 `IEnumerator<T>` 仍不是公共 CLR 映射面。
- `Chunk` 按指定正 size 顺序物化为独立的 Array chunks，最后不足 size 的 tail chunk 仍会返回。

## 边界

- 不支持 comparer overload，也不推断未知外部 `IOrderedEnumerable<T>` 状态。
- `ToHashSet` 与 `ToDictionary` 当前不映射：既有 `HashSet<T>` / `Dictionary<TKey, TValue>` carrier 仍基于 JavaScript `Set` / `Map`，尚未统一到 `EqualityComparer<T>` 的 hash + equality contract。不能因为 LINQ factory 本身易于物化就泄漏不一致的 CLR key semantics。
- 每次调用均产出 Array carrier，不保留 CLR 的延迟枚举、枚举器生命周期或 provider identity。
- `IGrouping<TKey, TElement>` 的 Array carrier 和 `Key` 元数据由 `GroupingT2Module` 管理。
