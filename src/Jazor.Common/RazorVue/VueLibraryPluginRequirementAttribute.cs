using System;

namespace Jazor.RazorVue;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class VueLibraryPluginRequirementAttribute : Attribute
{
    public VueLibraryPluginRequirementAttribute(string requirementId)
    {
        RequirementId = requirementId;
    }

    public string RequirementId { get; }
}
