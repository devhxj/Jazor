namespace ECMAScript.WebAssembly;

/// <summary>
/// WebAssembly
/// </summary>
[ECMAScript]
[Description("@#WebAssembly")]
public static partial class WebAssembly
{
    /// <summary>
    /// validate
    /// </summary>
    /// <param name="bytes">bytes</param>
    [Description("@#validate")]
    public static extern bool Validate(IBufferSource bytes);

    /// <summary>
    /// compile
    /// </summary>
    /// <param name="bytes">bytes</param>
    [Description("@#compile")]
    public static extern PromiseResult<Module> Compile(IBufferSource bytes);

    /// <summary>
    /// instantiate
    /// </summary>
    /// <param name="bytes">bytes</param>
    /// <param name="importObject">importObject</param>
    [Description("@#instantiate")]
    public static extern PromiseResult<WebAssemblyInstantiatedSource> Instantiate(IBufferSource bytes, object? importObject = default);

    /// <summary>
    /// instantiate
    /// </summary>
    /// <param name="moduleObject">moduleObject</param>
    /// <param name="importObject">importObject</param>
    [Description("@#instantiate")]
    public static extern PromiseResult<Instance> Instantiate(Module moduleObject, object? importObject = default);

    /// <summary>
    /// compileStreaming
    /// </summary>
    /// <param name="source">source</param>
    [Description("@#compileStreaming")]
    public static extern PromiseResult<Module> CompileStreaming(PromiseResult<Response> source);

    /// <summary>
    /// instantiateStreaming
    /// </summary>
    /// <param name="source">source</param>
    /// <param name="importObject">importObject</param>
    [Description("@#instantiateStreaming")]
    public static extern PromiseResult<WebAssemblyInstantiatedSource> InstantiateStreaming(PromiseResult<Response> source, object? importObject = default);
}
