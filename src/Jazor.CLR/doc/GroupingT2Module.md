# GroupingT2Module.cs

`IGrouping<TKey, TElement>` 使用普通 `Array<TElement>` 作为元素 carrier，以便所有已支持的
`IEnumerable<T>` 路径继续按 JavaScript iterable 消费。

组键不写入 Array 的用户可见字段，而保存在模块私有 `WeakMap<Array<TElement>, TKey>`：

- `GroupBy` 创建分组时登记首次出现的键。
- `IGrouping<TKey, TElement>.Key` 通过 `Op.Import` 从该私有元数据读取。
- Array 本身不会带额外属性，用户数组字段不会被该 CLR adapter 污染。

这只是 `IGrouping` 的受控物化表示，不承诺完整 `Lookup<TKey, TElement>`、延迟 group enumeration
或可跨运行时边界序列化的 CLR object identity。
