namespace ECMAScript.WebAssembly;

/// <summary>
/// ImportExportKind
/// </summary>
[Description("@#ImportExportKind")]
[ECMAScript]
[String]
public enum ImportExportKind
{
    [Description("@#function")]
    Function = 0,

    [Description("@#table")]
    Table = 1,

    [Description("@#memory")]
    Memory = 2,

    [Description("@#global")]
    Global = 3
}

/// <summary>
/// TableKind
/// </summary>
[Description("@#TableKind")]
[ECMAScript]
[String]
public enum TableKind
{
    [Description("@#externref")]
    Externref = 0,

    [Description("@#anyfunc")]
    Anyfunc = 1
}

/// <summary>
/// ValueType
/// </summary>
[Description("@#ValueType")]
[ECMAScript]
[String]
public enum ValueType
{
    [Description("@#i32")]
    I32 = 0,

    [Description("@#i64")]
    I64 = 1,

    [Description("@#f32")]
    F32 = 2,

    [Description("@#f64")]
    F64 = 3,

    [Description("@#v128")]
    V128 = 4,

    [Description("@#externref")]
    Externref = 5,

    [Description("@#anyfunc")]
    Anyfunc = 6
}
