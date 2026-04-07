using System;

namespace Jazor.RazorVue;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class VueLibraryStyleAttribute : Attribute
{
    public VueLibraryStyleAttribute(string styleSpecifier)
    {
        StyleSpecifier = styleSpecifier;
    }

    public string StyleSpecifier { get; }
}
