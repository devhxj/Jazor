namespace ECMAScript.WebAssembly;

/// <summary>
/// WebIDL dictionary ExceptionOptions。定义于 WebAssembly JavaScript Interface。
/// </summary>
/// <remarks>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#dictdef-exceptionoptions">WebAssembly JavaScript Interface: 5.9 Exceptions</see>
/// </remarks>
/// <param name="TraceStack">ExceptionOptions 字典中的 traceStack 成员，WebIDL 类型为 boolean。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;boolean&quot;,&#10;                  &quot;value&quot;: false&#10;                }。 <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-exceptionoptions-tracestack">WebAssembly JavaScript Interface: 5.9 Exceptions</see></param>
[ECMAScript]
[Description("@#ExceptionOptions")]
public record ExceptionOptions(
    [property: Description("@#traceStack")]bool TraceStack = false);

/// <summary>
/// WebIDL dictionary GlobalDescriptor。定义于 WebAssembly JavaScript Interface。
/// </summary>
/// <remarks>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#dictdef-globaldescriptor">WebAssembly JavaScript Interface: 5.5 Globals</see>
/// </remarks>
/// <param name="Value">GlobalDescriptor 字典中的 value 成员，WebIDL 类型为 ValueType。必须提供该成员。 <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-globaldescriptor-value">WebAssembly JavaScript Interface: 5.5 Globals</see></param>
/// <param name="Mutable">GlobalDescriptor 字典中的 mutable 成员，WebIDL 类型为 boolean。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;boolean&quot;,&#10;                  &quot;value&quot;: false&#10;                }。 <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-globaldescriptor-mutable">WebAssembly JavaScript Interface: 5.5 Globals</see></param>
[ECMAScript]
[Description("@#GlobalDescriptor")]
public record GlobalDescriptor(
    [property: Description("@#value")]ValueType? Value = default,
    [property: Description("@#mutable")]bool Mutable = false);

/// <summary>
/// WebIDL dictionary MemoryDescriptor。定义于 WebAssembly JavaScript Interface。
/// </summary>
/// <remarks>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#dictdef-memorydescriptor">WebAssembly JavaScript Interface: 5.3 Memories</see>
/// </remarks>
/// <param name="Initial">MemoryDescriptor 字典中的 initial 成员，WebIDL 类型为 AddressValue。必须提供该成员。 <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-memorydescriptor-initial">WebAssembly JavaScript Interface: 5.3 Memories</see></param>
/// <param name="Maximum">MemoryDescriptor 字典中的 maximum 成员，WebIDL 类型为 AddressValue。可省略。 <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-memorydescriptor-maximum">WebAssembly JavaScript Interface: 5.3 Memories</see></param>
/// <param name="Address">MemoryDescriptor 字典中的 address 成员，WebIDL 类型为 AddressType。可省略。 <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-memorydescriptor-address">WebAssembly JavaScript Interface: 5.3 Memories</see></param>
[ECMAScript]
[Description("@#MemoryDescriptor")]
public record MemoryDescriptor(
    [property: Description("@#initial")]AddressValue? Initial = default,
    [property: Description("@#maximum")]AddressValue? Maximum = default,
    [property: Description("@#address")]AddressType? Address = default);

/// <summary>
/// WebIDL dictionary ModuleExportDescriptor。定义于 WebAssembly JavaScript Interface。
/// </summary>
/// <remarks>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#dictdef-moduleexportdescriptor">WebAssembly JavaScript Interface: 5.1 Modules</see>
/// </remarks>
/// <param name="Name">ModuleExportDescriptor 字典中的 name 成员，WebIDL 类型为 USVString。必须提供该成员。 <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-moduleexportdescriptor-name">WebAssembly JavaScript Interface: 5.1 Modules</see></param>
/// <param name="Kind">ModuleExportDescriptor 字典中的 kind 成员，WebIDL 类型为 ImportExportKind。必须提供该成员。 <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-moduleexportdescriptor-kind">WebAssembly JavaScript Interface: 5.1 Modules</see></param>
[ECMAScript]
[Description("@#ModuleExportDescriptor")]
public record ModuleExportDescriptor(
    [property: Description("@#name")]string? Name = default,
    [property: Description("@#kind")]ImportExportKind? Kind = default);

/// <summary>
/// WebIDL dictionary ModuleImportDescriptor。定义于 WebAssembly JavaScript Interface。
/// </summary>
/// <remarks>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#dictdef-moduleimportdescriptor">WebAssembly JavaScript Interface: 5.1 Modules</see>
/// </remarks>
/// <param name="Module">ModuleImportDescriptor 字典中的 module 成员，WebIDL 类型为 USVString。必须提供该成员。 <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-moduleimportdescriptor-module">WebAssembly JavaScript Interface: 5.1 Modules</see></param>
/// <param name="Name">ModuleImportDescriptor 字典中的 name 成员，WebIDL 类型为 USVString。必须提供该成员。 <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-moduleimportdescriptor-name">WebAssembly JavaScript Interface: 5.1 Modules</see></param>
/// <param name="Kind">ModuleImportDescriptor 字典中的 kind 成员，WebIDL 类型为 ImportExportKind。必须提供该成员。 <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-moduleimportdescriptor-kind">WebAssembly JavaScript Interface: 5.1 Modules</see></param>
[ECMAScript]
[Description("@#ModuleImportDescriptor")]
public record ModuleImportDescriptor(
    [property: Description("@#module")]string? Module = default,
    [property: Description("@#name")]string? Name = default,
    [property: Description("@#kind")]ImportExportKind? Kind = default);

/// <summary>
/// WebIDL dictionary TableDescriptor。定义于 WebAssembly JavaScript Interface。
/// </summary>
/// <remarks>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#dictdef-tabledescriptor">WebAssembly JavaScript Interface: 5.4 Tables</see>
/// </remarks>
/// <param name="Element">TableDescriptor 字典中的 element 成员，WebIDL 类型为 TableKind。必须提供该成员。 <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-tabledescriptor-element">WebAssembly JavaScript Interface: 5.4 Tables</see></param>
/// <param name="Initial">TableDescriptor 字典中的 initial 成员，WebIDL 类型为 AddressValue。必须提供该成员。 <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-tabledescriptor-initial">WebAssembly JavaScript Interface: 5.4 Tables</see></param>
/// <param name="Maximum">TableDescriptor 字典中的 maximum 成员，WebIDL 类型为 AddressValue。可省略。 <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-tabledescriptor-maximum">WebAssembly JavaScript Interface: 5.4 Tables</see></param>
/// <param name="Address">TableDescriptor 字典中的 address 成员，WebIDL 类型为 AddressType。可省略。 <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-tabledescriptor-address">WebAssembly JavaScript Interface: 5.4 Tables</see></param>
[ECMAScript]
[Description("@#TableDescriptor")]
public record TableDescriptor(
    [property: Description("@#element")]TableKind? Element = default,
    [property: Description("@#initial")]AddressValue? Initial = default,
    [property: Description("@#maximum")]AddressValue? Maximum = default,
    [property: Description("@#address")]AddressType? Address = default);

/// <summary>
/// WebIDL dictionary TagType。定义于 WebAssembly JavaScript Interface。
/// </summary>
/// <remarks>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#dictdef-tagtype">WebAssembly JavaScript Interface: 5.7.1 Tag types</see>
/// </remarks>
/// <param name="Parameters">TagType 字典中的 parameters 成员，WebIDL 类型为 sequence&lt;ValueType&gt;。必须提供该成员。 <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-tagtype-parameters">WebAssembly JavaScript Interface: 5.7.1 Tag types</see></param>
[ECMAScript]
[Description("@#TagType")]
public record TagType(
    [property: Description("@#parameters")]ValueType[]? Parameters = default);

/// <summary>
/// WebIDL dictionary WebAssemblyInstantiatedSource。定义于 WebAssembly JavaScript Interface。
/// </summary>
/// <remarks>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#dictdef-webassemblyinstantiatedsource">WebAssembly JavaScript Interface: 5 The WebAssembly Namespace</see>
/// </remarks>
/// <param name="Module">WebAssemblyInstantiatedSource 字典中的 module 成员，WebIDL 类型为 Module。必须提供该成员。 <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-webassemblyinstantiatedsource-module">WebAssembly JavaScript Interface: 5 The WebAssembly Namespace</see></param>
/// <param name="Instance">WebAssemblyInstantiatedSource 字典中的 instance 成员，WebIDL 类型为 Instance。必须提供该成员。 <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-webassemblyinstantiatedsource-instance">WebAssembly JavaScript Interface: 5 The WebAssembly Namespace</see></param>
[ECMAScript]
[Description("@#WebAssemblyInstantiatedSource")]
public record WebAssemblyInstantiatedSource(
    [property: Description("@#module")]Module? Module = default,
    [property: Description("@#instance")]Instance? Instance = default);

/// <summary>
/// WebIDL dictionary WebAssemblyCompileOptions。定义于 WebAssembly JavaScript Interface。
/// </summary>
/// <remarks>
/// <see href="https://webassembly.github.io/spec/js-api/">WebAssembly JavaScript Interface: WebAssemblyCompileOptions</see>
/// </remarks>
/// <param name="ImportedStringConstants">WebAssemblyCompileOptions 字典中的 importedStringConstants 成员，WebIDL 类型为 USVString?。可省略。 <see href="https://webassembly.github.io/spec/js-api/">WebAssembly JavaScript Interface: WebAssemblyCompileOptions.importedStringConstants</see></param>
/// <param name="Builtins">WebAssemblyCompileOptions 字典中的 builtins 成员，WebIDL 类型为 sequence&lt;USVString&gt;。可省略。 <see href="https://webassembly.github.io/spec/js-api/">WebAssembly JavaScript Interface: WebAssemblyCompileOptions.builtins</see></param>
[ECMAScript]
[Description("@#WebAssemblyCompileOptions")]
public record WebAssemblyCompileOptions(
    [property: Description("@#importedStringConstants")]string? ImportedStringConstants = default,
    [property: Description("@#builtins")]string[]? Builtins = default);