using System;

namespace ECMAScript.VueContract;

/// <summary>声明组件需要的 Vue library/plugin 能力标识。</summary>
/// <remarks>RequirementId 只参与编译期依赖收集，不负责安装、加载或运行时探测插件。</remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class VueLibraryPluginRequirementAttribute(string requirementId) : Attribute
{
    public string RequirementId { get; } = requirementId;
}
