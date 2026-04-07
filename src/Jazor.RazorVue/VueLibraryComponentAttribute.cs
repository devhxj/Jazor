using System;

namespace Jazor.RazorVue;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class VueLibraryComponentAttribute : Attribute
{
    public VueLibraryComponentAttribute(string importSpecifier, string exportName)
    {
        ImportSpecifier = importSpecifier;
        ExportName = exportName;
    }

    public string ImportSpecifier { get; }

    public string ExportName { get; }
}
