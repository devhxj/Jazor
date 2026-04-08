using System;
using Jazor.RazorVue.Descriptor;

namespace Jazor.RazorVue;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class VueLibraryComponentFlagsAttribute : Attribute
{
    public VueLibraryComponentFlagsAttribute(VueComponentFlags flags)
    {
        Flags = flags;
    }

    public VueComponentFlags Flags { get; }
}
