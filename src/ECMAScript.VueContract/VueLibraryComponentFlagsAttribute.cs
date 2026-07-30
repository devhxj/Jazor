using System;
using ECMAScript.VueContract.Descriptor;

namespace ECMAScript.VueContract;

/// <summary>声明外部 Vue 组件支持的模型、slot 和动态使用能力。</summary>
/// <remarks>Flags 参与编译期组件契约判断，不会创建运行时 flags 对象。</remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class VueLibraryComponentFlagsAttribute(VueComponentFlags flags) : Attribute
{
    public VueComponentFlags Flags { get; } = flags;
}
