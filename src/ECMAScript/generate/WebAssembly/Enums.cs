namespace ECMAScript.WebAssembly;

/// <summary>
/// ImportExportKind
/// </summary>
[Description("@#ImportExportKind")]
[ECMAScript]
public enum ImportExportKind
{
    [Description("@#Function")]
    Function = 0,

    [Description("@#Table")]
    Table = 1,

    [Description("@#Memory")]
    Memory = 2,

    [Description("@#Global")]
    Global = 3
}

/// <summary>
/// TableKind
/// </summary>
[Description("@#TableKind")]
[ECMAScript]
public enum TableKind
{
    [Description("@#Externref")]
    Externref = 0,

    [Description("@#Anyfunc")]
    Anyfunc = 1
}

/// <summary>
/// ValueType
/// </summary>
[Description("@#ValueType")]
[ECMAScript]
public enum ValueType
{
    [Description("@#I32")]
    I32 = 0,

    [Description("@#I64")]
    I64 = 1,

    [Description("@#F32")]
    F32 = 2,

    [Description("@#F64")]
    F64 = 3,

    [Description("@#V128")]
    V128 = 4,

    [Description("@#Externref")]
    Externref = 5,

    [Description("@#Anyfunc")]
    Anyfunc = 6
}