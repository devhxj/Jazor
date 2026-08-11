namespace ECMAScript.WebAssembly;

/// <summary>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#exception">WebAssembly JavaScript Interface: 5.9 Exceptions</see>
/// </summary>
[ECMAScript]
[Description("@#Exception")]
public class Exception
{
    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-exception-exception">WebAssembly JavaScript Interface: 5.9 Exceptions</see>
    /// </summary>
    /// <param name="exceptionTag"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-exception-exception-exceptiontag-payload-options-exceptiontag">WebAssembly JavaScript Interface: 5.9 Exceptions</see></param>
    /// <param name="payload"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-exception-exception-exceptiontag-payload-options-payload">WebAssembly JavaScript Interface: 5.9 Exceptions</see></param>
    /// <param name="options"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-exception-exception-exceptiontag-payload-options-options">WebAssembly JavaScript Interface: 5.9 Exceptions</see></param>
    public extern Exception(Tag exceptionTag, object[] payload, ExceptionOptions? options = default);

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-exception-getarg">WebAssembly JavaScript Interface: 5.9 Exceptions</see>
    /// </summary>
    [Description("@#getArg")]
    public extern object GetArg(Tag exceptionTag, uint index);

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-exception-is">WebAssembly JavaScript Interface: 5.9 Exceptions</see>
    /// </summary>
    /// <param name="exceptionTag"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-exception-is-exceptiontag-exceptiontag">WebAssembly JavaScript Interface: 5.9 Exceptions</see></param>
    [Description("@#is")]
    public extern bool Is(Tag exceptionTag);

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-exception-stack">WebAssembly JavaScript Interface: 5.9 Exceptions</see>
    /// </summary>
    [Description("@#stack")]
    public extern string? Stack { get; }
}

/// <summary>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#global">WebAssembly JavaScript Interface: 5.5 Globals</see>
/// </summary>
[ECMAScript]
[Description("@#Global")]
public class Global
{
    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-global-global">WebAssembly JavaScript Interface: 5.5 Globals</see>
    /// </summary>
    /// <param name="descriptor"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-global-global-descriptor-v-descriptor">WebAssembly JavaScript Interface: 5.5 Globals</see></param>
    /// <param name="v"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-global-global-descriptor-v-v">WebAssembly JavaScript Interface: 5.5 Globals</see></param>
    public extern Global(GlobalDescriptor descriptor, object? v = default);

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-global-valueof">WebAssembly JavaScript Interface: 5.5 Globals</see>
    /// </summary>
    [Description("@#valueOf")]
    public extern object ValueOf();

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-global-value">WebAssembly JavaScript Interface: 5.5 Globals</see>
    /// </summary>
    [Description("@#value")]
    public extern object Value { get; set; }
}

/// <summary>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#instance">WebAssembly JavaScript Interface: 5.2 Instances</see>
/// </summary>
[ECMAScript]
[Description("@#Instance")]
public class Instance
{
    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-instance-instance">WebAssembly JavaScript Interface: 5.2 Instances</see>
    /// </summary>
    /// <param name="module"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-instance-instance-module-importobject-module">WebAssembly JavaScript Interface: 5.2 Instances</see></param>
    /// <param name="importObject"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-instance-instance-module-importobject-importobject">WebAssembly JavaScript Interface: 5.2 Instances</see></param>
    public extern Instance(Module module, object? importObject = default);

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-instance-exports">WebAssembly JavaScript Interface: 5.2 Instances</see>
    /// </summary>
    [Description("@#exports")]
    public extern object Exports { get; }
}

/// <summary>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#memory">WebAssembly JavaScript Interface: 5.3 Memories</see>
/// </summary>
[ECMAScript]
[Description("@#Memory")]
public class Memory
{
    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-memory-memory">WebAssembly JavaScript Interface: 5.3 Memories</see>
    /// </summary>
    /// <param name="descriptor"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-memory-memory-descriptor-descriptor">WebAssembly JavaScript Interface: 5.3 Memories</see></param>
    public extern Memory(MemoryDescriptor descriptor);

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-memory-grow">WebAssembly JavaScript Interface: 5.3 Memories</see>
    /// </summary>
    /// <param name="delta"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-memory-grow-delta-delta">WebAssembly JavaScript Interface: 5.3 Memories</see></param>
    [Description("@#grow")]
    public extern AddressValue Grow(AddressValue delta);

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-memory-tofixedlengthbuffer">WebAssembly JavaScript Interface: 5.3 Memories</see>
    /// </summary>
    [Description("@#toFixedLengthBuffer")]
    public extern ArrayBuffer ToFixedLengthBuffer();

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-memory-toresizablebuffer">WebAssembly JavaScript Interface: 5.3 Memories</see>
    /// </summary>
    [Description("@#toResizableBuffer")]
    public extern ArrayBuffer ToResizableBuffer();

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-memory-buffer">WebAssembly JavaScript Interface: 5.3 Memories</see>
    /// </summary>
    [Description("@#buffer")]
    public extern ArrayBuffer Buffer { get; }
}

/// <summary>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#table">WebAssembly JavaScript Interface: 5.4 Tables</see>
/// </summary>
[ECMAScript]
[Description("@#Table")]
public class Table
{
    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-table-table">WebAssembly JavaScript Interface: 5.4 Tables</see>
    /// </summary>
    /// <param name="descriptor"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-table-table-descriptor-value-descriptor">WebAssembly JavaScript Interface: 5.4 Tables</see></param>
    /// <param name="value"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-table-table-descriptor-value-value">WebAssembly JavaScript Interface: 5.4 Tables</see></param>
    public extern Table(TableDescriptor descriptor, object? value = default);

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-table-grow">WebAssembly JavaScript Interface: 5.4 Tables</see>
    /// </summary>
    /// <param name="delta"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-table-grow-delta-value-delta">WebAssembly JavaScript Interface: 5.4 Tables</see></param>
    /// <param name="value"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-table-grow-delta-value-value">WebAssembly JavaScript Interface: 5.4 Tables</see></param>
    [Description("@#grow")]
    public extern AddressValue Grow(AddressValue delta, object? value = default);

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-table-get">WebAssembly JavaScript Interface: 5.4 Tables</see>
    /// </summary>
    /// <param name="index"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-table-get-index-index">WebAssembly JavaScript Interface: 5.4 Tables</see></param>
    [Description("@#get")]
    public extern object Get(AddressValue index);

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-table-set">WebAssembly JavaScript Interface: 5.4 Tables</see>
    /// </summary>
    /// <param name="index"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-table-set-index-value-index">WebAssembly JavaScript Interface: 5.4 Tables</see></param>
    /// <param name="value"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-table-set-index-value-value">WebAssembly JavaScript Interface: 5.4 Tables</see></param>
    [Description("@#set")]
    public extern void Set(AddressValue index, object? value = default);

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-table-length">WebAssembly JavaScript Interface: 5.4 Tables</see>
    /// </summary>
    [Description("@#length")]
    public extern AddressValue Length { get; }
}

/// <summary>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#tag">WebAssembly JavaScript Interface: 5.7.1 Tag types</see>
/// </summary>
[ECMAScript]
[Description("@#Tag")]
public class Tag
{
    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-tag-tag">WebAssembly JavaScript Interface: 5.7.1 Tag types</see>
    /// </summary>
    /// <param name="type"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-tag-tag-type-type">WebAssembly JavaScript Interface: 5.7.1 Tag types</see></param>
    public extern Tag(TagType type);
}

/// <summary>
/// The maximum size of a module is 1,073,741,824 bytes (1 GiB).
/// </summary>
/// <remarks>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#module">WebAssembly JavaScript Interface: 5.1 Modules</see>
/// </remarks>
[ECMAScript]
[Description("@#Module")]
public class Module
{
    /// <summary>
    /// The maximum size of a module is 1,073,741,824 bytes (1 GiB).
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-module-module">WebAssembly JavaScript Interface: 5.1 Modules</see>
    /// </remarks>
    public extern Module(IAllowSharedBufferSource bytes, WebAssemblyCompileOptions? options = default);

    /// <summary>
    /// The maximum number of exports declared in a module is 1,000,000.
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-module-exports">WebAssembly JavaScript Interface: 5.1 Modules</see>
    /// </remarks>
    /// <param name="moduleObject"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-module-exports-moduleobject-moduleobject">WebAssembly JavaScript Interface: 5.1 Modules</see></param>
    [Description("@#exports")]
    public static extern ModuleExportDescriptor[] Exports(Module moduleObject);

    /// <summary>
    /// The maximum number of imports declared in a module is 1,000,000.
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-module-imports">WebAssembly JavaScript Interface: 5.1 Modules</see>
    /// </remarks>
    /// <param name="moduleObject"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-module-imports-moduleobject-moduleobject">WebAssembly JavaScript Interface: 5.1 Modules</see></param>
    [Description("@#imports")]
    public static extern ModuleImportDescriptor[] Imports(Module moduleObject);

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-module-customsections">WebAssembly JavaScript Interface: 5.1 Modules</see>
    /// </summary>
    /// <param name="moduleObject"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-module-customsections-moduleobject-sectionname-moduleobject">WebAssembly JavaScript Interface: 5.1 Modules</see></param>
    /// <param name="sectionName"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-module-customsections-moduleobject-sectionname-sectionname">WebAssembly JavaScript Interface: 5.1 Modules</see></param>
    [Description("@#customSections")]
    public static extern ArrayBuffer[] CustomSections(Module moduleObject, string sectionName);
}
