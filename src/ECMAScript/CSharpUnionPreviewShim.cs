namespace System.Runtime.CompilerServices;

// .NET 11 preview currently requires projects that use C# union types to provide
// these compiler-known contracts. Remove this shim when the target runtime ships
// the BCL definitions and define JAZOR_DISABLE_CSHARP_UNION_PREVIEW_SHIM during
// the transition to detect accidental dependencies.
#if NET11_0_OR_GREATER && !JAZOR_DISABLE_CSHARP_UNION_PREVIEW_SHIM
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class UnionAttribute : Attribute;

public interface IUnion
{
    object? Value { get; }
}
#endif
