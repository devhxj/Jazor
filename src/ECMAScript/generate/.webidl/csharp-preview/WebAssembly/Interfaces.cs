namespace ECMAScript.WebAssembly;

/// <summary>
/// Global
/// </summary>
[ECMAScript]
[Description("@#Global")]
public class Global
{
    /// <summary>
/// Constructor 
/// </summary>
/// <param name="descriptor">descriptor</param>
    /// <param name="v">v</param>
    public extern Global(GlobalDescriptor descriptor, object v);

    /// <summary>
    /// valueOf
    /// </summary>
    [Description("@#valueOf")]
    public extern object ValueOf();

    /// <summary>
/// value
/// </summary>
[Description("@#value")]
public extern object Value { get; set; }
}

/// <summary>
/// Instance
/// </summary>
[ECMAScript]
[Description("@#Instance")]
public class Instance
{
    /// <summary>
/// Constructor 
/// </summary>
/// <param name="module">module</param>
    /// <param name="importObject">importObject</param>
    public extern Instance(Module module, object importObject);

    /// <summary>
/// exports
/// </summary>
[Description("@#exports")]
public extern object Exports { get; }
}

/// <summary>
/// Memory
/// </summary>
[ECMAScript]
[Description("@#Memory")]
public class Memory
{
    /// <summary>
/// Constructor 
/// </summary>
/// <param name="descriptor">descriptor</param>
    public extern Memory(MemoryDescriptor descriptor);

    /// <summary>
    /// grow
    /// </summary>
    /// <param name="delta">delta</param>
    [Description("@#grow")]
    public extern uint Grow(uint delta);

    /// <summary>
    /// toFixedLengthBuffer
    /// </summary>
    [Description("@#toFixedLengthBuffer")]
    public extern ArrayBuffer ToFixedLengthBuffer();

    /// <summary>
    /// toResizableBuffer
    /// </summary>
    [Description("@#toResizableBuffer")]
    public extern ArrayBuffer ToResizableBuffer();

    /// <summary>
/// buffer
/// </summary>
[Description("@#buffer")]
public extern ArrayBuffer Buffer { get; }
}

/// <summary>
/// Module
/// </summary>
[ECMAScript]
[Description("@#Module")]
public class Module
{
    /// <summary>
/// Constructor 
/// </summary>
/// <param name="bytes">bytes</param>
    public extern Module(IBufferSource bytes);

    /// <summary>
    /// exports
    /// </summary>
    /// <param name="moduleObject">moduleObject</param>
    [Description("@#exports")]
    public static extern ModuleExportDescriptor[] Exports(Module moduleObject);

    /// <summary>
    /// imports
    /// </summary>
    /// <param name="moduleObject">moduleObject</param>
    [Description("@#imports")]
    public static extern ModuleImportDescriptor[] Imports(Module moduleObject);

    /// <summary>
    /// customSections
    /// </summary>
    /// <param name="moduleObject">moduleObject</param>
    /// <param name="sectionName">sectionName</param>
    [Description("@#customSections")]
    public static extern ArrayBuffer[] CustomSections(Module moduleObject, string sectionName);
}

/// <summary>
/// Table
/// </summary>
[ECMAScript]
[Description("@#Table")]
public class Table
{
    /// <summary>
/// Constructor 
/// </summary>
/// <param name="descriptor">descriptor</param>
    /// <param name="value">value</param>
    public extern Table(TableDescriptor descriptor, object value);

    /// <summary>
    /// grow
    /// </summary>
    /// <param name="delta">delta</param>
    /// <param name="value">value</param>
    [Description("@#grow")]
    public extern uint Grow(uint delta, object? value = default);

    /// <summary>
    /// get
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#get")]
    public extern object Get(uint index);

    /// <summary>
    /// set
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="value">value</param>
    [Description("@#set")]
    public extern void Set(uint index, object? value = default);

    /// <summary>
/// length
/// </summary>
[Description("@#length")]
public extern uint Length { get; }
}
