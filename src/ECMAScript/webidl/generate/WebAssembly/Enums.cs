namespace ECMAScript.WebAssembly;

/// <summary>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#enumdef-addresstype">WebAssembly JavaScript Interface: 5.1 Modules</see>
/// </summary>
[Description("@#AddressType")]
[ECMAScript]
[String]
public enum AddressType
{
    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-addresstype-i32">WebAssembly JavaScript Interface: 5.1 Modules</see>
    /// </summary>
    [Description("@#i32")]
    I32 = 0,

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-addresstype-i64">WebAssembly JavaScript Interface: 5.1 Modules</see>
    /// </summary>
    [Description("@#i64")]
    I64 = 1
}

/// <summary>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#enumdef-importexportkind">WebAssembly JavaScript Interface: 5.1 Modules</see>
/// </summary>
[Description("@#ImportExportKind")]
[ECMAScript]
[String]
public enum ImportExportKind
{
    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-importexportkind-function">WebAssembly JavaScript Interface: 5.1 Modules</see>
    /// </summary>
    [Description("@#function")]
    Function = 0,

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-importexportkind-table">WebAssembly JavaScript Interface: 5.1 Modules</see>
    /// </summary>
    [Description("@#table")]
    Table = 1,

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-importexportkind-memory">WebAssembly JavaScript Interface: 5.1 Modules</see>
    /// </summary>
    [Description("@#memory")]
    Memory = 2,

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-importexportkind-global">WebAssembly JavaScript Interface: 5.1 Modules</see>
    /// </summary>
    [Description("@#global")]
    Global = 3,

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-importexportkind-tag">WebAssembly JavaScript Interface: 5.1 Modules</see>
    /// </summary>
    [Description("@#tag")]
    Tag = 4
}

/// <summary>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#enumdef-tablekind">WebAssembly JavaScript Interface: 5.4 Tables</see>
/// </summary>
[Description("@#TableKind")]
[ECMAScript]
[String]
public enum TableKind
{
    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-tablekind-externref">WebAssembly JavaScript Interface: 5.4 Tables</see>
    /// </summary>
    [Description("@#externref")]
    Externref = 0,

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-tablekind-anyfunc">WebAssembly JavaScript Interface: 5.4 Tables</see>
    /// </summary>
    [Description("@#anyfunc")]
    Anyfunc = 1
}

/// <summary>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#enumdef-valuetype">WebAssembly JavaScript Interface: 5.5 Globals</see>
/// </summary>
[Description("@#ValueType")]
[ECMAScript]
[String]
public enum ValueType
{
    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-valuetype-i32">WebAssembly JavaScript Interface: 5.5 Globals</see>
    /// </summary>
    [Description("@#i32")]
    I32 = 0,

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-valuetype-i64">WebAssembly JavaScript Interface: 5.5 Globals</see>
    /// </summary>
    [Description("@#i64")]
    I64 = 1,

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-valuetype-f32">WebAssembly JavaScript Interface: 5.5 Globals</see>
    /// </summary>
    [Description("@#f32")]
    F32 = 2,

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-valuetype-f64">WebAssembly JavaScript Interface: 5.5 Globals</see>
    /// </summary>
    [Description("@#f64")]
    F64 = 3,

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-valuetype-v128">WebAssembly JavaScript Interface: 5.5 Globals</see>
    /// </summary>
    [Description("@#v128")]
    V128 = 4,

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-valuetype-externref">WebAssembly JavaScript Interface: 5.5 Globals</see>
    /// </summary>
    [Description("@#externref")]
    Externref = 5,

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-valuetype-anyfunc">WebAssembly JavaScript Interface: 5.5 Globals</see>
    /// </summary>
    [Description("@#anyfunc")]
    Anyfunc = 6
}
