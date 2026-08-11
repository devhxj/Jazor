namespace ECMAScript.WebAssembly;

/// <summary>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#dictdef-exceptionoptions">WebAssembly JavaScript Interface: 5.9 Exceptions</see>
/// </summary>
/// <param name="TraceStack"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-exceptionoptions-tracestack">WebAssembly JavaScript Interface: 5.9 Exceptions</see></param>
[ECMAScript]
[Description("@#ExceptionOptions")]
public record ExceptionOptions(
    [property: Description("@#traceStack")]bool TraceStack = false);

/// <summary>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#dictdef-globaldescriptor">WebAssembly JavaScript Interface: 5.5 Globals</see>
/// </summary>
/// <param name="Value"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-globaldescriptor-value">WebAssembly JavaScript Interface: 5.5 Globals</see></param>
/// <param name="Mutable"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-globaldescriptor-mutable">WebAssembly JavaScript Interface: 5.5 Globals</see></param>
[ECMAScript]
[Description("@#GlobalDescriptor")]
public record GlobalDescriptor(
    [property: Description("@#value")]ValueType? Value = default,
    [property: Description("@#mutable")]bool Mutable = false);

/// <summary>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#dictdef-memorydescriptor">WebAssembly JavaScript Interface: 5.3 Memories</see>
/// </summary>
/// <param name="Initial"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-memorydescriptor-initial">WebAssembly JavaScript Interface: 5.3 Memories</see></param>
/// <param name="Maximum"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-memorydescriptor-maximum">WebAssembly JavaScript Interface: 5.3 Memories</see></param>
/// <param name="Address"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-memorydescriptor-address">WebAssembly JavaScript Interface: 5.3 Memories</see></param>
[ECMAScript]
[Description("@#MemoryDescriptor")]
public record MemoryDescriptor(
    [property: Description("@#initial")]AddressValue? Initial = default,
    [property: Description("@#maximum")]AddressValue? Maximum = default,
    [property: Description("@#address")]AddressType? Address = default);

/// <summary>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#dictdef-moduleexportdescriptor">WebAssembly JavaScript Interface: 5.1 Modules</see>
/// </summary>
/// <param name="Name"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-moduleexportdescriptor-name">WebAssembly JavaScript Interface: 5.1 Modules</see></param>
/// <param name="Kind"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-moduleexportdescriptor-kind">WebAssembly JavaScript Interface: 5.1 Modules</see></param>
[ECMAScript]
[Description("@#ModuleExportDescriptor")]
public record ModuleExportDescriptor(
    [property: Description("@#name")]string? Name = default,
    [property: Description("@#kind")]ImportExportKind? Kind = default);

/// <summary>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#dictdef-moduleimportdescriptor">WebAssembly JavaScript Interface: 5.1 Modules</see>
/// </summary>
/// <param name="Module"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-moduleimportdescriptor-module">WebAssembly JavaScript Interface: 5.1 Modules</see></param>
/// <param name="Name"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-moduleimportdescriptor-name">WebAssembly JavaScript Interface: 5.1 Modules</see></param>
/// <param name="Kind"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-moduleimportdescriptor-kind">WebAssembly JavaScript Interface: 5.1 Modules</see></param>
[ECMAScript]
[Description("@#ModuleImportDescriptor")]
public record ModuleImportDescriptor(
    [property: Description("@#module")]string? Module = default,
    [property: Description("@#name")]string? Name = default,
    [property: Description("@#kind")]ImportExportKind? Kind = default);

/// <summary>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#dictdef-tabledescriptor">WebAssembly JavaScript Interface: 5.4 Tables</see>
/// </summary>
/// <param name="Element"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-tabledescriptor-element">WebAssembly JavaScript Interface: 5.4 Tables</see></param>
/// <param name="Initial"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-tabledescriptor-initial">WebAssembly JavaScript Interface: 5.4 Tables</see></param>
/// <param name="Maximum"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-tabledescriptor-maximum">WebAssembly JavaScript Interface: 5.4 Tables</see></param>
/// <param name="Address"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-tabledescriptor-address">WebAssembly JavaScript Interface: 5.4 Tables</see></param>
[ECMAScript]
[Description("@#TableDescriptor")]
public record TableDescriptor(
    [property: Description("@#element")]TableKind? Element = default,
    [property: Description("@#initial")]AddressValue? Initial = default,
    [property: Description("@#maximum")]AddressValue? Maximum = default,
    [property: Description("@#address")]AddressType? Address = default);

/// <summary>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#dictdef-tagtype">WebAssembly JavaScript Interface: 5.7.1 Tag types</see>
/// </summary>
/// <param name="Parameters"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-tagtype-parameters">WebAssembly JavaScript Interface: 5.7.1 Tag types</see></param>
[ECMAScript]
[Description("@#TagType")]
public record TagType(
    [property: Description("@#parameters")]ValueType[]? Parameters = default);

/// <summary>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#dictdef-webassemblyinstantiatedsource">WebAssembly JavaScript Interface: 5 The WebAssembly Namespace</see>
/// </summary>
/// <param name="Module"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-webassemblyinstantiatedsource-module">WebAssembly JavaScript Interface: 5 The WebAssembly Namespace</see></param>
/// <param name="Instance"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-webassemblyinstantiatedsource-instance">WebAssembly JavaScript Interface: 5 The WebAssembly Namespace</see></param>
[ECMAScript]
[Description("@#WebAssemblyInstantiatedSource")]
public record WebAssemblyInstantiatedSource(
    [property: Description("@#module")]Module? Module = default,
    [property: Description("@#instance")]Instance? Instance = default);

[ECMAScript]
[Description("@#WebAssemblyCompileOptions")]
public record WebAssemblyCompileOptions(
    [property: Description("@#importedStringConstants")]string? ImportedStringConstants = default,
    [property: Description("@#builtins")]string[]? Builtins = default);
