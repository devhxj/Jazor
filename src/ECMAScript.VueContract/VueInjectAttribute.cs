using System;

namespace ECMAScript.VueContract;

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
