using System;

namespace ECMAScript.VueContract;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class VueLibrarySlotAttribute : Attribute
{
    public VueLibrarySlotAttribute(string publicName)
    {
        PublicName = publicName;
        ContextParameterName = "context";
    }

    public string PublicName { get; }

    public string? Name { get; set; }

    public bool IsDefault { get; set; }

    public bool Required { get; set; }

    public string? ContextTypeName { get; set; }

    public string ContextParameterName { get; set; }
}
