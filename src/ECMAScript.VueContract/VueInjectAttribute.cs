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
    /// <summary>
    /// 注入位置声明的组件契约类型，编译器以此确定消费端的强类型接口。
    /// </summary>
    public Type ContractComponentType { get; } = contractComponentType ?? throw new ArgumentNullException(nameof(contractComponentType));

    /// <summary>
    /// 实际提供注入值的组件类型，用于将契约绑定到具体组件实现。
    /// </summary>
    public Type ImplementationComponentType { get; } = implementationComponentType ?? throw new ArgumentNullException(nameof(implementationComponentType));
}