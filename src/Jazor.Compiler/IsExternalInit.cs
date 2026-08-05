// File: IsExternalInit.cs
// Purpose: Supplies the compiler-recognized init-only setter marker for target compatibility.
// 这是编译期垫片，不表示 Jazor 在运行时实现任何额外的 CLR init-only 协议。
// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices;

/// <summary>
/// Polyfill for C# 9.0 init-only properties support in netstandard2.0
/// </summary>
internal static class IsExternalInit
{
}
