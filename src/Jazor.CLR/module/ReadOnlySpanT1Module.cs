namespace Jazor.CLR;

/// <summary>
/// System.ReadOnlySpan&lt;T&gt; 在受控 Array view 场景下的类型映射。
/// </summary>
/// <remarks>
/// 这不是 span 的地址、切片身份或 stack-only 生命周期实现。它只允许已经映射为
/// JavaScript Array 的值进入明确映射的只读 span 成员，例如 SequenceEqual。
/// </remarks>
[Jazor(Op.Alias, "System.ReadOnlySpan<T>", "Array")]
public static class ReadOnlySpanT1Module<T>
{
}
