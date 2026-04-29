using System;
using Jazor.RazorVue.Descriptor;

namespace Jazor.RazorVue;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class VueLibraryEmitAttribute : Attribute
{
    public VueLibraryEmitAttribute(string razorAlias)
    {
        RazorAlias = razorAlias;
    }

    public VueLibraryEmitAttribute(string razorAlias, VueEmitKind kind)
        : this(razorAlias)
    {
        Kind = kind;
        HasKindOverride = true;
    }

    public string RazorAlias { get; }

    public string? Name { get; set; }

    public string? PayloadTypeName { get; set; }

    public VueEmitKind Kind { get; }

    internal bool HasKindOverride { get; }
}
