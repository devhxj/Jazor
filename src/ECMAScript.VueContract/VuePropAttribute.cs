using System;
using ECMAScript.VueContract.Descriptor;

namespace ECMAScript.VueContract;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class VuePropAttribute : Attribute
{
    public VuePropAttribute(string publicName)
    {
        PublicName = publicName;
    }

    public VuePropAttribute(string publicName, VuePropKind kind)
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
