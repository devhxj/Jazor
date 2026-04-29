using System;
using Jazor.RazorVue.Descriptor;

namespace Jazor.RazorVue;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class VueLibraryPropAttribute : Attribute
{
    public VueLibraryPropAttribute(string publicName)
    {
        PublicName = publicName;
    }

    public VueLibraryPropAttribute(string publicName, VuePropKind kind)
        : this(publicName)
    {
        Kind = kind;
        HasKindOverride = true;
    }

    public string PublicName { get; }

    public string? Name { get; set; }

    public bool Required { get; set; }

    public bool AcceptsBinding { get; set; }

    public string? DefaultExpression { get; set; }

    public VuePropKind Kind { get; }

    internal bool HasKindOverride { get; }
}
