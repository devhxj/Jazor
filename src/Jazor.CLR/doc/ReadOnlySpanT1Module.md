# ReadOnlySpanT1Module.cs

`System.ReadOnlySpan<T>` 在当前支持面只作为已经 materialize 的 `Array<T>` 的只读静态视图。

- 不建立地址、切片 identity、stack-only 生命周期或 unsafe 协议。
- 仅允许已具备正式 member mapping 的调用使用该 alias；未映射的 span 成员仍在使用点失败。
- 当前由 `MemoryExtensionsModule` 的 `ReadOnlySpan<T>.SequenceEqual<T>(ReadOnlySpan<T>)` 消费。
