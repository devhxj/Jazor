namespace ECMAScript.WebAssembly;

/// <summary>
/// Since the original release 1.0 of the WebAssembly specification, a number of proposals for extensions have been integrated. The following sections provide an overview of what has changed.
/// </summary>
/// <remarks>
/// <see href="https://webassembly.github.io/content-security-policy/js-api/#namespacedef-webassembly">WebAssembly JavaScript Interface: 5 The WebAssembly Namespace</see>
/// </remarks>
[ECMAScript]
[Description("@#WebAssembly")]
public static partial class WebAssembly
{
    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-webassembly-validate">WebAssembly JavaScript Interface: 5 The WebAssembly Namespace</see>
    /// </summary>
    [Description("@#validate")]
    public static extern bool Validate(IAllowSharedBufferSource bytes, WebAssemblyCompileOptions? options = default);

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-webassembly-compile">WebAssembly JavaScript Interface: 5 The WebAssembly Namespace</see>
    /// </summary>
    [Description("@#compile")]
    public static extern PromiseResult<Module> Compile(IAllowSharedBufferSource bytes, WebAssemblyCompileOptions? options = default);

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-webassembly-instantiate">WebAssembly JavaScript Interface: 5 The WebAssembly Namespace</see>
    /// </summary>
    [Description("@#instantiate")]
    public static extern PromiseResult<WebAssemblyInstantiatedSource> Instantiate(IAllowSharedBufferSource bytes, object? importObject = default, WebAssemblyCompileOptions? options = default);

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-webassembly-instantiate-moduleobject-importobject">WebAssembly JavaScript Interface: 5 The WebAssembly Namespace</see>
    /// </summary>
    /// <param name="moduleObject"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-webassembly-instantiate-moduleobject-importobject-moduleobject">WebAssembly JavaScript Interface: 5 The WebAssembly Namespace</see></param>
    /// <param name="importObject"><see href="https://webassembly.github.io/content-security-policy/js-api/#dom-webassembly-instantiate-moduleobject-importobject-importobject">WebAssembly JavaScript Interface: 5 The WebAssembly Namespace</see></param>
    [Description("@#instantiate")]
    public static extern PromiseResult<Instance> Instantiate(Module moduleObject, object? importObject = default);

    /// <summary>
    /// <see href="https://webassembly.github.io/content-security-policy/js-api/#dom-webassembly-jstag">WebAssembly JavaScript Interface: 5 The WebAssembly Namespace</see>
    /// </summary>
    [Description("@#JSTag")]
    public static extern Tag JSTag { get; }

    /// <summary>
    /// <see href="https://webassembly.github.io/spec/web-api/#dom-webassembly-compilestreaming">WebAssembly Web API: 2 Streaming Module Compilation and Instantiation</see>
    /// </summary>
    /// <param name="source"><see href="https://webassembly.github.io/spec/web-api/#dom-webassembly-compilestreaming-source-options-source">WebAssembly Web API: 2 Streaming Module Compilation and Instantiation</see></param>
    /// <param name="options"><see href="https://webassembly.github.io/spec/web-api/#dom-webassembly-compilestreaming-source-options-options">WebAssembly Web API: 2 Streaming Module Compilation and Instantiation</see></param>
    [Description("@#compileStreaming")]
    public static extern PromiseResult<Module> CompileStreaming(PromiseResult<Response> source, WebAssemblyCompileOptions? options = default);

    /// <summary>
    /// <see href="https://webassembly.github.io/spec/web-api/#dom-webassembly-instantiatestreaming">WebAssembly Web API: 2 Streaming Module Compilation and Instantiation</see>
    /// </summary>
    /// <param name="source"><see href="https://webassembly.github.io/spec/web-api/#dom-webassembly-instantiatestreaming-source-importobject-options-source">WebAssembly Web API: 2 Streaming Module Compilation and Instantiation</see></param>
    /// <param name="importObject"><see href="https://webassembly.github.io/spec/web-api/#dom-webassembly-instantiatestreaming-source-importobject-options-importobject">WebAssembly Web API: 2 Streaming Module Compilation and Instantiation</see></param>
    /// <param name="options"><see href="https://webassembly.github.io/spec/web-api/#dom-webassembly-instantiatestreaming-source-importobject-options-options">WebAssembly Web API: 2 Streaming Module Compilation and Instantiation</see></param>
    [Description("@#instantiateStreaming")]
    public static extern PromiseResult<WebAssemblyInstantiatedSource> InstantiateStreaming(PromiseResult<Response> source, object? importObject = default, WebAssemblyCompileOptions? options = default);
}
