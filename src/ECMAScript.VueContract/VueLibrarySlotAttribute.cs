using System;

namespace ECMAScript.VueContract;

/// <summary>
/// Describes a Vue slot exposed by a library component.
/// This attribute is intentionally class-scoped because slot names, default-slot
/// status, dynamic name patterns, and requiredness are part of the concrete Vue
/// component contract. Class-level metadata can also map inherited
/// RenderFragment parameters without polluting or shadowing the shared base
/// parameter surface.
/// </summary>
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

    public string? NamePattern { get; set; }

    public bool PatternOnly { get; set; }

    public bool IsDefault { get; set; }

    public bool Required { get; set; }

    public string? ContextTypeName { get; set; }

    public string ContextParameterName { get; set; }
}
