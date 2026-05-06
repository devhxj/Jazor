using System;

namespace ECMAScript.VueContract;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class VueLibraryStyleAttribute(string styleSpecifier) : Attribute
{
    public string StyleSpecifier { get; } = styleSpecifier;
}
