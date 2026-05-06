using System;

namespace ECMAScript.VueContract.Descriptor;

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
