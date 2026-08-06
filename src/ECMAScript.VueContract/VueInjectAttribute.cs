using System;

namespace ECMAScript.VueContract;

/// <summary>
/// 在程序集级声明容器 contract component 到具体 implementation component 的映射。
/// </summary>
/// <remarks>
/// 该映射供编译期解析使用；它不会通过运行时反射替换组件，也不改变 authored contract 的稳定 API。
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public sealed class VueInjectAttribute(Type contractComponentType, Type implementationComponentType) : Attribute
{
    public Type ContractComponentType { get; } = contractComponentType ?? throw new ArgumentNullException(nameof(contractComponentType));

    public Type ImplementationComponentType { get; } = implementationComponentType ?? throw new ArgumentNullException(nameof(implementationComponentType));
}
