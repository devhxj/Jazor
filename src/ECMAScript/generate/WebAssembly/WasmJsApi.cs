namespace ECMAScript.WebAssembly;

/// <summary>
/// WebAssemblyInstantiatedSource
/// </summary>
[ECMAScript]
[Description("@#WebAssemblyInstantiatedSource")]
public record WebAssemblyInstantiatedSource(
    [property: Description("@#module")]Module? Module = default,
	[property: Description("@#instance")]Instance? Instance = default);

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
/// MemoryDescriptor
/// </summary>
[ECMAScript]
[Description("@#MemoryDescriptor")]
public record MemoryDescriptor(
    [property: Description("@#initial")]uint Initial = default,
	[property: Description("@#maximum")]uint Maximum = default);

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
/// GlobalDescriptor
/// </summary>
[ECMAScript]
[Description("@#GlobalDescriptor")]
public record GlobalDescriptor(
    [property: Description("@#value")]ValueType? Value = default,
	[property: Description("@#mutable")]bool Mutable = false);

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
    public extern static ModuleExportDescriptor[] Exports(Module moduleObject);

    /// <summary>
    /// imports
    /// </summary>
    /// <param name="moduleObject">moduleObject</param>
    [Description("@#imports")]
    public extern static ModuleImportDescriptor[] Imports(Module moduleObject);

    /// <summary>
    /// customSections
    /// </summary>
    /// <param name="moduleObject">moduleObject</param>
    /// <param name="sectionName">sectionName</param>
    [Description("@#customSections")]
    public extern static ArrayBuffer[] CustomSections(Module moduleObject, string sectionName);
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
    public extern uint Grow(uint delta, object value);

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
    public extern void Set(uint index, object value);

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }
}

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