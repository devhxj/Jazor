# MemoryExtensionsModule.cs

当前映射：

```csharp
System.MemoryExtensions.Contains<T>(System.ReadOnlySpan<T>, T)
System.ReadOnlySpan<T>.SequenceEqual<T>(System.ReadOnlySpan<T>)
System.ReadOnlySpan<char>.Trim()
System.ReadOnlySpan<char>.Trim(char)
System.ReadOnlySpan<char>.Trim(System.ReadOnlySpan<char>)
System.ReadOnlySpan<char>.TrimStart()
System.ReadOnlySpan<char>.TrimStart(char)
System.ReadOnlySpan<char>.TrimStart(System.ReadOnlySpan<char>)
System.ReadOnlySpan<char>.TrimEnd()
System.ReadOnlySpan<char>.TrimEnd(char)
System.ReadOnlySpan<char>.TrimEnd(System.ReadOnlySpan<char>)
```

SDK Razor 的默认 imports 可以让 `array.Contains(value)` 绑定到这个 BCL overload。该映射将调用输入
视为 `Array<T>` 的只读 carrier，并使用 `EqualityComparer<T>.Equals` 逐元素比较。

`SequenceEqual` 由 SDK 默认 imports 下的数组实例调用绑定。它先比较长度，再按相同 index
使用默认 comparer 同步比较，在首个不等项短路，且不修改任一输入。

该支持面不引入 `Span<T>` 或 `ReadOnlySpan<T>` 的地址、切片身份、stack-only 生命周期或 unsafe 语义；
只有已经作为 JavaScript Array carrier 出现的调用参数可使用这一映射。

`ReadOnlySpan<char>` trim 映射使用 `string` 或字符数组 carrier，并以文本值返回。无参数版本采用
CLR 的空白 trim 语义；`char` 与 span 参数版本只移除指定字符，空/default trim span 不移除任何字符。
该值投影不保留 span 的 backing-store、offset 或 slice identity，因此不扩展为可写 span、ref 或 unsafe
协议。
