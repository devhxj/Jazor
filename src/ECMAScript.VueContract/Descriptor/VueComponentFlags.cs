using System;

namespace ECMAScript.VueContract.Descriptor;

/// <summary>以位标志表示外部 Vue 组件的编译期能力。</summary>
/// <remarks>Flags 不产生 runtime 对象，只用于组件 wrapper 和宿主 lowering 的静态判断。</remarks>
[Flags]
public enum VueComponentFlags
{
    None = 0,
    SupportsModelValue = 1,
    SupportsMultipleModels = 2,
    RequiresExplicitChildren = 4,
    IsDynamicSafe = 8,
    IsFormControl = 16
}
