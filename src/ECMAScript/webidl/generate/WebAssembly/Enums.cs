namespace ECMAScript.WebAssembly;

/// <summary>
/// WebIDL enum AddressType。定义于 WebAssembly JavaScript Interface。
/// </summary>
/// <remarks>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#enumdef-addresstype">WebAssembly JavaScript Interface: 5.1 Modules</see>
/// </remarks>
[Description("@#AddressType")]
[ECMAScript]
[String]
public enum AddressType
{
    /// <summary>
    /// JavaScript 字符串取值 “i32”；属于 AddressType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-addresstype-i32">WebAssembly JavaScript Interface: 5.1 Modules</see>
    /// </remarks>
    [Description("@#i32")]
    I32 = 0,

    /// <summary>
    /// JavaScript 字符串取值 “i64”；属于 AddressType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-addresstype-i64">WebAssembly JavaScript Interface: 5.1 Modules</see>
    /// </remarks>
    [Description("@#i64")]
    I64 = 1
}

/// <summary>
/// WebIDL enum ImportExportKind。定义于 WebAssembly JavaScript Interface。
/// </summary>
/// <remarks>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#enumdef-importexportkind">WebAssembly JavaScript Interface: 5.1 Modules</see>
/// </remarks>
[Description("@#ImportExportKind")]
[ECMAScript]
[String]
public enum ImportExportKind
{
    /// <summary>
    /// JavaScript 字符串取值 “function”；属于 ImportExportKind 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-importexportkind-function">WebAssembly JavaScript Interface: 5.1 Modules</see>
    /// </remarks>
    [Description("@#function")]
    Function = 0,

    /// <summary>
    /// JavaScript 字符串取值 “table”；属于 ImportExportKind 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-importexportkind-table">WebAssembly JavaScript Interface: 5.1 Modules</see>
    /// </remarks>
    [Description("@#table")]
    Table = 1,

    /// <summary>
    /// JavaScript 字符串取值 “memory”；属于 ImportExportKind 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-importexportkind-memory">WebAssembly JavaScript Interface: 5.1 Modules</see>
    /// </remarks>
    [Description("@#memory")]
    Memory = 2,

    /// <summary>
    /// JavaScript 字符串取值 “global”；属于 ImportExportKind 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-importexportkind-global">WebAssembly JavaScript Interface: 5.1 Modules</see>
    /// </remarks>
    [Description("@#global")]
    Global = 3,

    /// <summary>
    /// JavaScript 字符串取值 “tag”；属于 ImportExportKind 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-importexportkind-tag">WebAssembly JavaScript Interface: 5.1 Modules</see>
    /// </remarks>
    [Description("@#tag")]
    Tag = 4
}

/// <summary>
/// WebIDL enum TableKind。定义于 WebAssembly JavaScript Interface。
/// </summary>
/// <remarks>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#enumdef-tablekind">WebAssembly JavaScript Interface: 5.4 Tables</see>
/// </remarks>
[Description("@#TableKind")]
[ECMAScript]
[String]
public enum TableKind
{
    /// <summary>
    /// JavaScript 字符串取值 “externref”；属于 TableKind 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-tablekind-externref">WebAssembly JavaScript Interface: 5.4 Tables</see>
    /// </remarks>
    [Description("@#externref")]
    Externref = 0,

    /// <summary>
    /// JavaScript 字符串取值 “anyfunc”；属于 TableKind 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-tablekind-anyfunc">WebAssembly JavaScript Interface: 5.4 Tables</see>
    /// </remarks>
    [Description("@#anyfunc")]
    Anyfunc = 1
}

/// <summary>
/// WebIDL enum ValueType。定义于 WebAssembly JavaScript Interface。
/// </summary>
/// <remarks>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#enumdef-valuetype">WebAssembly JavaScript Interface: 5.5 Globals</see>
/// </remarks>
[Description("@#ValueType")]
[ECMAScript]
[String]
public enum ValueType
{
    /// <summary>
    /// JavaScript 字符串取值 “i32”；属于 ValueType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-valuetype-i32">WebAssembly JavaScript Interface: 5.5 Globals</see>
    /// </remarks>
    [Description("@#i32")]
    I32 = 0,

    /// <summary>
    /// JavaScript 字符串取值 “i64”；属于 ValueType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-valuetype-i64">WebAssembly JavaScript Interface: 5.5 Globals</see>
    /// </remarks>
    [Description("@#i64")]
    I64 = 1,

    /// <summary>
    /// JavaScript 字符串取值 “f32”；属于 ValueType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-valuetype-f32">WebAssembly JavaScript Interface: 5.5 Globals</see>
    /// </remarks>
    [Description("@#f32")]
    F32 = 2,

    /// <summary>
    /// JavaScript 字符串取值 “f64”；属于 ValueType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-valuetype-f64">WebAssembly JavaScript Interface: 5.5 Globals</see>
    /// </remarks>
    [Description("@#f64")]
    F64 = 3,

    /// <summary>
    /// JavaScript 字符串取值 “v128”；属于 ValueType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-valuetype-v128">WebAssembly JavaScript Interface: 5.5 Globals</see>
    /// </remarks>
    [Description("@#v128")]
    V128 = 4,

    /// <summary>
    /// JavaScript 字符串取值 “externref”；属于 ValueType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-valuetype-externref">WebAssembly JavaScript Interface: 5.5 Globals</see>
    /// </remarks>
    [Description("@#externref")]
    Externref = 5,

    /// <summary>
    /// JavaScript 字符串取值 “anyfunc”；属于 ValueType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-valuetype-anyfunc">WebAssembly JavaScript Interface: 5.5 Globals</see>
    /// </remarks>
    [Description("@#anyfunc")]
    Anyfunc = 6
}