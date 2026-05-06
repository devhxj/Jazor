using System;
using ECMAScript.VueContract.Descriptor;

namespace ECMAScript.VueContract;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class VueLibraryComponentFlagsAttribute(VueComponentFlags flags) : Attribute
{
    public VueComponentFlags Flags { get; } = flags;
}
