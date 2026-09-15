namespace ECMAScript.WebAssembly;

/// <summary>
/// WebIDL interface Exception。定义于 WebAssembly JavaScript Interface。
/// </summary>
/// <remarks>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#exception">WebAssembly JavaScript Interface: 5.9 Exceptions</see>
/// </remarks>
[ECMAScript]
[Description("@#Exception")]
public class Exception
{
    /// <summary>
    /// 构造浏览器提供的 Exception 对象；参数按对应 WebIDL 构造签名传递。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-exception-exception">WebAssembly JavaScript Interface: 5.9 Exceptions</see>
    /// </remarks>
    /// <param name="exceptionTag"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-exception-exception-exceptiontag-payload-options-exceptiontag">WebAssembly JavaScript Interface: 5.9 Exceptions</see></param>
    /// <param name="payload"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-exception-exception-exceptiontag-payload-options-payload">WebAssembly JavaScript Interface: 5.9 Exceptions</see></param>
    /// <param name="options"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-exception-exception-exceptiontag-payload-options-options">WebAssembly JavaScript Interface: 5.9 Exceptions</see></param>
    public extern Exception(Tag exceptionTag, object[] payload, ExceptionOptions? options = default);

    /// <summary>
    /// JavaScript Exception.getArg(exceptionTag, index) 的强类型绑定，WebIDL 返回类型为 any。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-exception-getarg">WebAssembly JavaScript Interface: 5.9 Exceptions</see>
    /// </remarks>
    /// <param name="exceptionTag">WebIDL 参数 exceptionTag：Tag。调用时必须传入。 <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-exception-getarg">WebAssembly JavaScript Interface: exceptionTag</see></param>
    /// <param name="index">WebIDL 参数 index：unsigned long。调用时必须传入。 <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-exception-getarg">WebAssembly JavaScript Interface: index</see></param>
    [Description("@#getArg")]
    public extern object GetArg(Tag exceptionTag, uint index);

    /// <summary>
    /// JavaScript Exception.is(exceptionTag) 的强类型绑定，WebIDL 返回类型为 boolean。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-exception-is">WebAssembly JavaScript Interface: 5.9 Exceptions</see>
    /// </remarks>
    /// <param name="exceptionTag"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-exception-is-exceptiontag-exceptiontag">WebAssembly JavaScript Interface: 5.9 Exceptions</see></param>
    [Description("@#is")]
    public extern bool Is(Tag exceptionTag);

    /// <summary>
    /// JavaScript 属性 Exception.stack：DOMString, undefined。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-exception-stack">WebAssembly JavaScript Interface: 5.9 Exceptions</see>
    /// </remarks>
    [Description("@#stack")]
    public extern string? Stack { get; }
}

/// <summary>
/// WebIDL interface Global。定义于 WebAssembly JavaScript Interface。
/// </summary>
/// <remarks>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#global">WebAssembly JavaScript Interface: 5.5 Globals</see>
/// </remarks>
[ECMAScript]
[Description("@#Global")]
public class Global
{
    /// <summary>
    /// 构造浏览器提供的 Global 对象；参数按对应 WebIDL 构造签名传递。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-global-global">WebAssembly JavaScript Interface: 5.5 Globals</see>
    /// </remarks>
    /// <param name="descriptor"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-global-global-descriptor-v-descriptor">WebAssembly JavaScript Interface: 5.5 Globals</see></param>
    /// <param name="v"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-global-global-descriptor-v-v">WebAssembly JavaScript Interface: 5.5 Globals</see></param>
    public extern Global(GlobalDescriptor descriptor, object? v = default);

    /// <summary>
    /// JavaScript Global.valueOf() 的强类型绑定，WebIDL 返回类型为 any。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-global-valueof">WebAssembly JavaScript Interface: 5.5 Globals</see>
    /// </remarks>
    [Description("@#valueOf")]
    public extern object ValueOf();

    /// <summary>
    /// JavaScript 属性 Global.value：any。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-global-value">WebAssembly JavaScript Interface: 5.5 Globals</see>
    /// </remarks>
    [Description("@#value")]
    public extern object Value { get; set; }
}

/// <summary>
/// WebIDL interface Instance。定义于 WebAssembly JavaScript Interface。
/// </summary>
/// <remarks>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#instance">WebAssembly JavaScript Interface: 5.2 Instances</see>
/// </remarks>
[ECMAScript]
[Description("@#Instance")]
public class Instance
{
    /// <summary>
    /// 构造浏览器提供的 Instance 对象；参数按对应 WebIDL 构造签名传递。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-instance-instance">WebAssembly JavaScript Interface: 5.2 Instances</see>
    /// </remarks>
    /// <param name="module"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-instance-instance-module-importobject-module">WebAssembly JavaScript Interface: 5.2 Instances</see></param>
    /// <param name="importObject"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-instance-instance-module-importobject-importobject">WebAssembly JavaScript Interface: 5.2 Instances</see></param>
    public extern Instance(Module module, object? importObject = default);

    /// <summary>
    /// JavaScript 属性 Instance.exports：object。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-instance-exports">WebAssembly JavaScript Interface: 5.2 Instances</see>
    /// </remarks>
    [Description("@#exports")]
    public extern object Exports { get; }
}

/// <summary>
/// WebIDL interface Memory。定义于 WebAssembly JavaScript Interface。
/// </summary>
/// <remarks>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#memory">WebAssembly JavaScript Interface: 5.3 Memories</see>
/// </remarks>
[ECMAScript]
[Description("@#Memory")]
public class Memory
{
    /// <summary>
    /// 构造浏览器提供的 Memory 对象；参数按对应 WebIDL 构造签名传递。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-memory-memory">WebAssembly JavaScript Interface: 5.3 Memories</see>
    /// </remarks>
    /// <param name="descriptor"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-memory-memory-descriptor-descriptor">WebAssembly JavaScript Interface: 5.3 Memories</see></param>
    public extern Memory(MemoryDescriptor descriptor);

    /// <summary>
    /// JavaScript Memory.grow(delta) 的强类型绑定，WebIDL 返回类型为 AddressValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-memory-grow">WebAssembly JavaScript Interface: 5.3 Memories</see>
    /// </remarks>
    /// <param name="delta"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-memory-grow-delta-delta">WebAssembly JavaScript Interface: 5.3 Memories</see></param>
    [Description("@#grow")]
    public extern AddressValue Grow(AddressValue delta);

    /// <summary>
    /// JavaScript Memory.toFixedLengthBuffer() 的强类型绑定，WebIDL 返回类型为 ArrayBuffer。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-memory-tofixedlengthbuffer">WebAssembly JavaScript Interface: 5.3 Memories</see>
    /// </remarks>
    [Description("@#toFixedLengthBuffer")]
    public extern ArrayBuffer ToFixedLengthBuffer();

    /// <summary>
    /// JavaScript Memory.toResizableBuffer() 的强类型绑定，WebIDL 返回类型为 ArrayBuffer。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-memory-toresizablebuffer">WebAssembly JavaScript Interface: 5.3 Memories</see>
    /// </remarks>
    [Description("@#toResizableBuffer")]
    public extern ArrayBuffer ToResizableBuffer();

    /// <summary>
    /// JavaScript 属性 Memory.buffer：ArrayBuffer。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-memory-buffer">WebAssembly JavaScript Interface: 5.3 Memories</see>
    /// </remarks>
    [Description("@#buffer")]
    public extern ArrayBuffer Buffer { get; }
}

/// <summary>
/// WebIDL interface Table。定义于 WebAssembly JavaScript Interface。
/// </summary>
/// <remarks>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#table">WebAssembly JavaScript Interface: 5.4 Tables</see>
/// </remarks>
[ECMAScript]
[Description("@#Table")]
public class Table
{
    /// <summary>
    /// 构造浏览器提供的 Table 对象；参数按对应 WebIDL 构造签名传递。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-table-table">WebAssembly JavaScript Interface: 5.4 Tables</see>
    /// </remarks>
    /// <param name="descriptor"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-table-table-descriptor-value-descriptor">WebAssembly JavaScript Interface: 5.4 Tables</see></param>
    /// <param name="value"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-table-table-descriptor-value-value">WebAssembly JavaScript Interface: 5.4 Tables</see></param>
    public extern Table(TableDescriptor descriptor, object? value = default);

    /// <summary>
    /// JavaScript Table.grow(delta, value) 的强类型绑定，WebIDL 返回类型为 AddressValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-table-grow">WebAssembly JavaScript Interface: 5.4 Tables</see>
    /// </remarks>
    /// <param name="delta"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-table-grow-delta-value-delta">WebAssembly JavaScript Interface: 5.4 Tables</see></param>
    /// <param name="value"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-table-grow-delta-value-value">WebAssembly JavaScript Interface: 5.4 Tables</see></param>
    [Description("@#grow")]
    public extern AddressValue Grow(AddressValue delta, object? value = default);

    /// <summary>
    /// JavaScript Table.get(index) 的强类型绑定，WebIDL 返回类型为 any。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-table-get">WebAssembly JavaScript Interface: 5.4 Tables</see>
    /// </remarks>
    /// <param name="index"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-table-get-index-index">WebAssembly JavaScript Interface: 5.4 Tables</see></param>
    [Description("@#get")]
    public extern object Get(AddressValue index);

    /// <summary>
    /// JavaScript Table.set(index, value) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-table-set">WebAssembly JavaScript Interface: 5.4 Tables</see>
    /// </remarks>
    /// <param name="index"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-table-set-index-value-index">WebAssembly JavaScript Interface: 5.4 Tables</see></param>
    /// <param name="value"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-table-set-index-value-value">WebAssembly JavaScript Interface: 5.4 Tables</see></param>
    [Description("@#set")]
    public extern void Set(AddressValue index, object? value = default);

    /// <summary>
    /// JavaScript 属性 Table.length：AddressValue。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-table-length">WebAssembly JavaScript Interface: 5.4 Tables</see>
    /// </remarks>
    [Description("@#length")]
    public extern AddressValue Length { get; }
}

/// <summary>
/// WebIDL interface Tag。定义于 WebAssembly JavaScript Interface。
/// </summary>
/// <remarks>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#tag">WebAssembly JavaScript Interface: 5.7.1 Tag types</see>
/// </remarks>
[ECMAScript]
[Description("@#Tag")]
public class Tag
{
    /// <summary>
    /// 构造浏览器提供的 Tag 对象；参数按对应 WebIDL 构造签名传递。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-tag-tag">WebAssembly JavaScript Interface: 5.7.1 Tag types</see>
    /// </remarks>
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
    /// <param name="bytes">WebIDL 参数 bytes：AllowSharedBufferSource。调用时必须传入。 <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-module-module">WebAssembly JavaScript Interface: bytes</see></param>
    /// <param name="options">WebIDL 参数 options：WebAssemblyCompileOptions。可省略。WebIDL 默认值：{&#10;                      &quot;type&quot;: &quot;dictionary&quot;&#10;                    }。 <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-module-module">WebAssembly JavaScript Interface: options</see></param>
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
    /// JavaScript Module.customSections(moduleObject, sectionName) 的强类型绑定，WebIDL 返回类型为 sequence&lt;ArrayBuffer&gt;。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-module-customsections">WebAssembly JavaScript Interface: 5.1 Modules</see>
    /// </remarks>
    /// <param name="moduleObject"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-module-customsections-moduleobject-sectionname-moduleobject">WebAssembly JavaScript Interface: 5.1 Modules</see></param>
    /// <param name="sectionName"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-module-customsections-moduleobject-sectionname-sectionname">WebAssembly JavaScript Interface: 5.1 Modules</see></param>
    [Description("@#customSections")]
    public static extern ArrayBuffer[] CustomSections(Module moduleObject, string sectionName);
}