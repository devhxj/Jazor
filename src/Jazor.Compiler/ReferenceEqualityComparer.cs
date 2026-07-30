using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Jazor.Compiler;

/// <summary>
/// 使用对象引用而不是 Equals 实现进行比较的内部 comparer。
/// </summary>
/// <remarks>
/// Roslyn operation 和 symbol 相关缓存需要区分同值但不同实例；如果使用值相等比较，可能
/// 把不同语义节点错误合并。该 comparer 只适用于引用类型。
/// </remarks>
internal sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>
    where T : class
{
    public static ReferenceEqualityComparer<T> Instance { get; } = new();

    private ReferenceEqualityComparer()
    {
    }

    public bool Equals(T? x, T? y)
        => ReferenceEquals(x, y);

    public int GetHashCode(T obj)
        => RuntimeHelpers.GetHashCode(obj);
}
