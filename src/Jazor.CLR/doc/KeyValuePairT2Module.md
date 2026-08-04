# KeyValuePairT2Module.cs

`System.Collections.Generic.KeyValuePair<TKey, TValue>` 使用 JavaScript `Map` iterator 的两槽
`Array` entry 作为 carrier：slot `0` 是 `Key`，slot `1` 是 `Value`。

- 显式 `KeyValuePair<TKey, TValue>(key, value)` 直接内联为 `[key, value]`，不建立 wrapper object。
- `Key` 与 `Value` 通过短 `Inline` projection 读取固定槽位。adapter receiver 保持为对应的 typed
  `Array<TKey>` / `Array<TValue>`，不暴露 `object` 形态。
- `foreach` 的 KeyValuePair 解构由编译器的结构化 Array binding 处理；直接
  `Deconstruct(out key, out value)` 尚未接入完整 ref/out adapter，明确保持 unsupported。
- 该模块服务于 `Dictionary<TKey, TValue>` 迭代和返回 key/value entry 的 LINQ API；默认构造和
  `ToString()` 仍不在当前映射面内。
