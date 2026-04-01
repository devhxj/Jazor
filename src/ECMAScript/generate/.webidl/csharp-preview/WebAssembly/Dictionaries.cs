namespace ECMAScript.WebAssembly;

/// <summary>
/// GlobalDescriptor
/// </summary>
[ECMAScript]
[Description("@#GlobalDescriptor")]
public record GlobalDescriptor(
    [property: Description("@#value")]ValueType? Value = default,
    [property: Description("@#mutable")]bool Mutable = false);

/// <summary>
/// MemoryDescriptor
/// </summary>
[ECMAScript]
[Description("@#MemoryDescriptor")]
public record MemoryDescriptor(
    [property: Description("@#initial")]uint Initial = default,
    [property: Description("@#maximum")]uint Maximum = default);

/// <summary>
/// ModuleExportDescriptor
/// </summary>
[ECMAScript]
[Description("@#ModuleExportDescriptor")]
public record ModuleExportDescriptor(
    [property: Description("@#name")]string? Name = default,
    [property: Description("@#kind")]ImportExportKind? Kind = default);

/// <summary>
/// ModuleImportDescriptor
/// </summary>
[ECMAScript]
[Description("@#ModuleImportDescriptor")]
public record ModuleImportDescriptor(
    [property: Description("@#module")]string? Module = default,
    [property: Description("@#name")]string? Name = default,
    [property: Description("@#kind")]ImportExportKind? Kind = default);

/// <summary>
/// TableDescriptor
/// </summary>
[ECMAScript]
[Description("@#TableDescriptor")]
public record TableDescriptor(
    [property: Description("@#element")]TableKind? Element = default,
    [property: Description("@#initial")]uint Initial = default,
    [property: Description("@#maximum")]uint Maximum = default);

/// <summary>
/// WebAssemblyInstantiatedSource
/// </summary>
[ECMAScript]
[Description("@#WebAssemblyInstantiatedSource")]
public record WebAssemblyInstantiatedSource(
    [property: Description("@#module")]Module? Module = default,
    [property: Description("@#instance")]Instance? Instance = default);
