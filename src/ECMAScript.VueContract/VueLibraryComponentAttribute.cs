using System;

namespace ECMAScript.VueContract;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class VueLibraryComponentAttribute(string importSpecifier, string exportName) : Attribute
{
    public string ImportSpecifier { get; } = importSpecifier;

    public string ExportName { get; } = exportName;
}
