using System;

namespace ECMAScript.VueContract;

/// <summary>
/// 在程序集级声明容器 contract component 到具体 implementation component 的映射。
/// </summary>
/// <remarks>
/// 该映射供编译期解析使用；它不会通过运行时反射替换组件，也不改变 authored contract 的稳定 API。
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public sealed class VueInjectAttribute : Attribute
{
    public VueInjectAttribute(Type contractComponentType, Type implementationComponentType)
    {
        ContractComponentType = contractComponentType ?? throw new ArgumentNullException(nameof(contractComponentType));
        ImplementationComponentType = implementationComponentType ?? throw new ArgumentNullException(nameof(implementationComponentType));
    }

    public Type ContractComponentType { get; }

    public Type ImplementationComponentType { get; }
}
