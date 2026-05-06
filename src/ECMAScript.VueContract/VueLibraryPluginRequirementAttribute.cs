using System;

namespace ECMAScript.VueContract;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class VueLibraryPluginRequirementAttribute(string requirementId) : Attribute
{
    public string RequirementId { get; } = requirementId;
}
