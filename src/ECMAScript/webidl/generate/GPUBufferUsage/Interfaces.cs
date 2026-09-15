namespace ECMAScript.GPUBufferUsage;

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPU interface of the WebGPU API is the starting point for using WebGPU. It can be used to return a GPUAdapter from which you can request devices, configure features and limits, and more. The GPU object for the current context is accessed via the Navigator.gpu or WorkerNavigator.gpu properties.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPU">MDN Web Docs: GPU</see>
/// </remarks>
[ECMAScript]
[Description("@#GPU")]
public class GPU
{
    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The requestAdapter() method of the GPU interface returns a Promise that fulfills with a GPUAdapter object instance. From this you can request a GPUDevice, adapter info, features, and limits. Note that the user agent chooses whether to return an adapter. If so, it chooses according to the provided options. If no options are provided, the device will provide access to the default adapter, which is usually good enough for most purposes.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPU/requestAdapter">MDN Web Docs: GPU.requestAdapter</see>
    /// </remarks>
    /// <param name="options">An object that can contain the following properties: featureLevel Optional An enumerated value that specifies the set of features the returned adapter will support. Available values are: core The default value. Specifies that the GPUAdapter supports all core WebGPU features and limits, which allows applications to support devices with modern platform graphics APIs. This is referred to as &quot;core&quot; WebGPU. Adapters that support core WebGPU will have the core-features-and-limits feature available (see GPUSupportedFeatures). compatibility Specifies that the GPUAdapter supports a restricted subset of the WebGPU API capable of running in older graphics APIs such as OpenGL ES 3.1 and Direct3D 11. This setting opts WebGPU into compatibility mode. powerPreference Optional An enumerated value that can be used to provide a hint to the user agent indicating what class of adapter should be chosen from the system&apos;s available adapters. Available values are: undefined (or not specified) Provides no hint. &quot;low-power&quot; Provides a hint to prioritize power savings over performance. If your app runs OK with this setting, it is recommended to use it, as it can significantly improve battery life on portable devices. This is usually the default if no options are provided. &quot;high-performance&quot; Provides a hint to prioritize performance over power consumption. You are encouraged to only specify this value if absolutely necessary, since it may significantly decrease battery life on portable devices. It may also result in increased GPUDevice loss — the system will sometimes elect to switch to a lower-power adapter to save power. This hint&apos;s primary purpose is to influence which GPU is used in a multi-GPU system. For instance, some laptops have a low-power integrated GPU and a high-performance discrete GPU. Different factors may affect which adapter is returned including battery status, attached displays, or removable GPUs. Note: On Chrome running on dual-GPU macOS devices, if requestAdapter() is called without a powerPreference option, the high-performance discrete GPU is returned when the user&apos;s device is on AC power. Otherwise, the low-power integrated GPU is returned. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPU/requestAdapter">MDN Web Docs: options</see></param>
    /// <returns>A Promise that fulfills with a GPUAdapter object instance if the request is successful. requestAdapter() will resolve to null if an appropriate adapter is not available.</returns>
    [Description("@#requestAdapter")]
    public extern PromiseResult<GPUAdapter?> RequestAdapter(GPURequestAdapterOptions? options = default);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The getPreferredCanvasFormat() method of the GPU interface returns the optimal canvas texture format for displaying 8-bit depth, standard dynamic range content on the current system. This is commonly used to provide a GPUCanvasContext.configure() call with the optimal format value for the current system. This is recommended — if you don&apos;t use the preferred format when configuring the canvas context, you may incur additional overhead, such as additional texture copies, depending on the platform.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPU/getPreferredCanvasFormat">MDN Web Docs: GPU.getPreferredCanvasFormat</see>
    /// </remarks>
    /// <returns>A string indicating a canvas texture format. The value can be rgba8unorm or bgra8unorm.</returns>
    [Description("@#getPreferredCanvasFormat")]
    public extern GPUTextureFormat GetPreferredCanvasFormat();

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The wgslLanguageFeatures read-only property of the GPU interface returns a WGSLLanguageFeatures object that reports the WGSL language extensions supported by the WebGPU implementation. Note: Not all WGSL language extensions are available to WebGPU in all browsers that support the API. We recommend you thoroughly test any extensions you choose to use.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPU/wgslLanguageFeatures">MDN Web Docs: GPU.wgslLanguageFeatures</see>
    /// </remarks>
    [Description("@#wgslLanguageFeatures")]
    public extern WGSLLanguageFeatures WgslLanguageFeatures { get; }
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUAdapter interface of the WebGPU API represents a GPU adapter. From this you can request a GPUDevice, adapter info, features, and limits. A GPUAdapter object is requested using the GPU.requestAdapter() method.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUAdapter">MDN Web Docs: GPUAdapter</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUAdapter")]
public class GPUAdapter
{
    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The features read-only property of the GPUAdapter interface returns a GPUSupportedFeatures object that describes additional functionality supported by the adapter. You should note that not all features will be available to WebGPU in all browsers that support it, even if the features are supported by the underlying hardware. This could be due to constraints in the underlying system, browser, or adapter. For example: The underlying system might not be able to guarantee exposure of a feature in a way that is compatible with a certain browser. The browser vendor might not have found a secure way to implement support for that feature, or might just not have gotten round to it yet. If you are hoping to take advantage of a specific additional feature in a WebGPU app, thorough testing is advised.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUAdapter/features">MDN Web Docs: GPUAdapter.features</see>
    /// </remarks>
    [Description("@#features")]
    public extern GPUSupportedFeatures Features { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The limits read-only property of the GPUAdapter interface returns a GPUSupportedLimits object that describes the limits supported by the adapter. You should note that, rather than reporting the exact limits of each GPU, browsers will likely report different tier values of different limits to reduce the unique information available to drive-by fingerprinting. For example, the tiers of a certain limit might be 2048, 8192, and 32768. If your GPU&apos;s actual limit is 16384, the browser will still report 8192. Given that different browsers will handle this differently and the tier values may change over time, it is hard to provide an accurate account of what limit values to expect — thorough testing is advised.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUAdapter/limits">MDN Web Docs: GPUAdapter.limits</see>
    /// </remarks>
    [Description("@#limits")]
    public extern GPUSupportedLimits Limits { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The info read-only property of the GPUAdapter interface returns a GPUAdapterInfo object containing identifying information about the adapter.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUAdapter/info">MDN Web Docs: GPUAdapter.info</see>
    /// </remarks>
    [Description("@#info")]
    public extern GPUAdapterInfo Info { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The requestDevice() method of the GPUAdapter interface returns a Promise that fulfills with a GPUDevice object, which is the primary interface for communicating with the GPU.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUAdapter/requestDevice">MDN Web Docs: GPUAdapter.requestDevice</see>
    /// </remarks>
    /// <param name="descriptor">An object containing the following properties: defaultQueue Optional An object that provides information for the device&apos;s default GPUQueue (as returned by GPUDevice.queue). This object has a single property — label — which provides the default queue with a label value. If no value is provided, this defaults to an empty object, and the default queue&apos;s label will be an empty string. label Optional A string providing a label that can be used to identify the GPUDevice, for example in GPUError messages or console warnings. requiredFeatures Optional An array of strings representing additional functionality that you want supported by the returned GPUDevice. The requestDevice() call will fail if the GPUAdapter cannot provide these features. See GPUSupportedFeatures for a full list of possible features. This defaults to an empty array if no value is provided. requiredLimits Optional An object containing properties representing the limits that you want supported by the returned GPUDevice. The requestDevice() call will fail if the GPUAdapter cannot provide these limits. Each key with a non-undefined value must be the name of a member of GPUSupportedLimits. Note: You can request unknown limits when requesting a GPU device without causing an error. Such limits will be undefined. This is useful because it makes WebGPU code less brittle — a codebase won&apos;t stop working because a limit no longer exists in the adapter. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUAdapter/requestDevice">MDN Web Docs: descriptor</see></param>
    /// <returns>A Promise that fulfills with a GPUDevice object instance. If you make a duplicate call, i.e., call requestDevice() on a GPUAdapter that requestDevice() was already called on, the promise rejects with an OperationError because the associated GPUAdapter is consumed when a GPUDevice is created.</returns>
    [Description("@#requestDevice")]
    public extern PromiseResult<GPUDevice> RequestDevice(GPUDeviceDescriptor? descriptor = default);
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUAdapterInfo interface of the WebGPU API contains identifying information about a GPUAdapter. An adapter&apos;s GPUAdapterInfo can be retrieved using the GPUAdapter.info property of the adapter itself, or the GPUDevice.adapterInfo property of a device that originated from the adapter. This object allows developers to access specific details about the user&apos;s GPU so that they can preemptively apply workarounds for GPU-specific bugs, or provide different codepaths to better suit different GPU architectures. Providing such information does present a security risk — it could be used for fingerprinting — therefore the information shared is kept at a minimum, and different browser vendors are likely to share different information types and granularities.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUAdapterInfo">MDN Web Docs: GPUAdapterInfo</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUAdapterInfo")]
public class GPUAdapterInfo
{
    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The vendor read-only property of the GPUAdapterInfo interface returns the name of the adapter vendor, or an empty string if it is not available.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUAdapterInfo/vendor">MDN Web Docs: GPUAdapterInfo.vendor</see>
    /// </remarks>
    [Description("@#vendor")]
    public extern string Vendor { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The architecture read-only property of the GPUAdapterInfo interface returns the name of the family or class of GPUs the adapter belongs to, or an empty string if it is not available.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUAdapterInfo/architecture">MDN Web Docs: GPUAdapterInfo.architecture</see>
    /// </remarks>
    [Description("@#architecture")]
    public extern string Architecture { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The device read-only property of the GPUAdapterInfo interface returns a vendor-specific identifier for the adapter, or an empty string if it is not available.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUAdapterInfo/device">MDN Web Docs: GPUAdapterInfo.device</see>
    /// </remarks>
    [Description("@#device")]
    public extern string Device { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The description read-only property of the GPUAdapterInfo interface returns a human-readable string describing the adapter, or an empty string if it is not available.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUAdapterInfo/description">MDN Web Docs: GPUAdapterInfo.description</see>
    /// </remarks>
    [Description("@#description")]
    public extern string Description { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The subgroupMinSize read-only property of the GPUAdapterInfo interface returns the minimum supported subgroup size for the GPUAdapter. This can be used along with the subgroups feature.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUAdapterInfo/subgroupMinSize">MDN Web Docs: GPUAdapterInfo.subgroupMinSize</see>
    /// </remarks>
    [Description("@#subgroupMinSize")]
    public extern uint SubgroupMinSize { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The subgroupMaxSize read-only property of the GPUAdapterInfo interface returns the maximum supported subgroup size for the GPUAdapter. This can be used along with the subgroups feature.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUAdapterInfo/subgroupMaxSize">MDN Web Docs: GPUAdapterInfo.subgroupMaxSize</see>
    /// </remarks>
    [Description("@#subgroupMaxSize")]
    public extern uint SubgroupMaxSize { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The isFallbackAdapter read-only property of the GPUAdapterInfo interface returns true if the adapter is a fallback adapter, and false if not.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUAdapterInfo/isFallbackAdapter">MDN Web Docs: GPUAdapterInfo.isFallbackAdapter</see>
    /// </remarks>
    [Description("@#isFallbackAdapter")]
    public extern bool IsFallbackAdapter { get; }
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUBindGroup interface of the WebGPU API is based on a GPUBindGroupLayout and defines a set of resources to be bound together in a group and how those resources are used in shader stages. A GPUBindGroup object instance is created using the GPUDevice.createBindGroup() method.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUBindGroup">MDN Web Docs: GPUBindGroup</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUBindGroup")]
public class GPUBindGroup
{
    #region mixin GPUObjectBase
    /// <summary>
    /// JavaScript 属性 GPUObjectBase.label：USVString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </remarks>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUBindGroupLayout interface of the WebGPU API defines the structure and purpose of related GPU resources such as buffers that will be used in a pipeline, and is used as a template when creating GPUBindGroups. A GPUBindGroupLayout object instance is created using the GPUDevice.createBindGroupLayout() method.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUBindGroupLayout">MDN Web Docs: GPUBindGroupLayout</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUBindGroupLayout")]
public class GPUBindGroupLayout
{
    #region mixin GPUObjectBase
    /// <summary>
    /// JavaScript 属性 GPUObjectBase.label：USVString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </remarks>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUBuffer interface of the WebGPU API represents a block of memory that can be used to store raw data to use in GPU operations. A GPUBuffer object instance is created using the GPUDevice.createBuffer() method.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUBuffer">MDN Web Docs: GPUBuffer</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUBuffer")]
public class GPUBuffer
{
    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The size read-only property of the GPUBuffer interface represents the length of the GPUBuffer&apos;s memory allocation, in bytes. size is set via the size property in the descriptor object passed into the originating GPUDevice.createBuffer() call.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUBuffer/size">MDN Web Docs: GPUBuffer.size</see>
    /// </remarks>
    [Description("@#size")]
    public extern GPUSize64Out Size { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The usage read-only property of the GPUBuffer interface contains the bitwise flags representing the allowed usages of the GPUBuffer. usage is set via the usage property in the descriptor object passed into the originating GPUDevice.createBuffer() call.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUBuffer/usage">MDN Web Docs: GPUBuffer.usage</see>
    /// </remarks>
    [Description("@#usage")]
    public extern GPUFlagsConstant Usage { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The mapState read-only property of the GPUBuffer interface represents the mapped state of the GPUBuffer.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUBuffer/mapState">MDN Web Docs: GPUBuffer.mapState</see>
    /// </remarks>
    [Description("@#mapState")]
    public extern GPUBufferMapState MapState { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The mapAsync() method of the GPUBuffer interface maps the specified range of the GPUBuffer. It returns a Promise that resolves when the GPUBuffer&apos;s content is ready to be accessed. While the GPUBuffer is mapped it cannot be used in any GPU commands. Once the buffer is successfully mapped (which can be checked via GPUBuffer.mapState), calls to GPUBuffer.getMappedRange() will return an ArrayBuffer containing the GPUBuffer&apos;s current values, to be read and updated by JavaScript as required. When you have finished working with the GPUBuffer values, call GPUBuffer.unmap() to unmap it, making it accessible to the GPU again.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUBuffer/mapAsync">MDN Web Docs: GPUBuffer.mapAsync</see>
    /// </remarks>
    /// <param name="mode">A bitwise flag that specifies whether the GPUBuffer is mapped for reading or writing. Possible values are: GPUMapMode.READ The GPUBuffer is mapped for reading. Values can be read, but any changes made to the ArrayBuffer returned by GPUBuffer.getMappedRange() will be discarded once GPUBuffer.unmap() is called. Read-mode mapping can only be used on GPUBuffers that have a usage of GPUBufferUsage.MAP_READ set on them (i.e., when created with GPUDevice.createBuffer()). GPUMapMode.WRITE The GPUBuffer is mapped for writing. Values can be read and updated — any changes made to the ArrayBuffer returned by GPUBuffer.getMappedRange() will be saved to the GPUBuffer once GPUBuffer.unmap() is called. Write-mode mapping can only be used on GPUBuffers that have a usage of GPUBufferUsage.MAP_WRITE set on them (i.e., when created with GPUDevice.createBuffer()). <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUBuffer/mapAsync">MDN Web Docs: mode</see></param>
    /// <param name="offset">A number representing the offset, in bytes, from the start of the buffer to the start of the range to be mapped. If offset is omitted, it defaults to 0. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUBuffer/mapAsync">MDN Web Docs: offset</see></param>
    /// <param name="size">A number representing the size, in bytes, of the range to be mapped. If size is omitted, the range mapped extends to the end of the GPUBuffer. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUBuffer/mapAsync">MDN Web Docs: size</see></param>
    /// <returns>A Promise that resolves to undefined when the GPUBuffer&apos;s content is ready to be accessed.</returns>
    [Description("@#mapAsync")]
    public extern PromiseResult MapAsync(GPUMapModeFlags mode, GPUSize64? offset = default, GPUSize64? size = default);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The getMappedRange() method of the GPUBuffer interface returns an ArrayBuffer containing the mapped contents of the GPUBuffer in the specified range. This can only happen once the GPUBuffer has been successfully mapped with GPUBuffer.mapAsync() (this can be checked via GPUBuffer.mapState). While the GPUBuffer is mapped it cannot be used in any GPU commands. When you have finished working with the GPUBuffer values, call GPUBuffer.unmap() to unmap it, making it accessible to the GPU again. A TypeError is thrown if an attempt is made to detach the ArrayBuffer in any way other than via GPUBuffer.unmap(), such as by calling transfer().
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUBuffer/getMappedRange">MDN Web Docs: GPUBuffer.getMappedRange</see>
    /// </remarks>
    /// <param name="offset">A number representing the offset, in bytes, from the start of the GPUBuffer&apos;s mapped range to the start of the range to be returned in the ArrayBuffer. If offset is omitted, it defaults to 0. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUBuffer/getMappedRange">MDN Web Docs: offset</see></param>
    /// <param name="size">A number representing the size, in bytes, of the ArrayBuffer to return. If size is omitted, the range extends to the end of the GPUBuffer&apos;s mapped range. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUBuffer/getMappedRange">MDN Web Docs: size</see></param>
    /// <returns>An ArrayBuffer.</returns>
    [Description("@#getMappedRange")]
    public extern ArrayBuffer GetMappedRange(GPUSize64? offset = default, GPUSize64? size = default);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The unmap() method of the GPUBuffer interface unmaps the mapped range of the GPUBuffer, making its contents available for use by the GPU again after it has previously been mapped with GPUBuffer.mapAsync() (the GPU cannot access a mapped GPUBuffer). When unmap() is called, any ArrayBuffers created via GPUBuffer.getMappedRange() are detached.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUBuffer/unmap">MDN Web Docs: GPUBuffer.unmap</see>
    /// </remarks>
    [Description("@#unmap")]
    public extern void Unmap();

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The destroy() method of the GPUBuffer interface destroys the GPUBuffer.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUBuffer/destroy">MDN Web Docs: GPUBuffer.destroy</see>
    /// </remarks>
    [Description("@#destroy")]
    public extern void Destroy();

    #region mixin GPUObjectBase
    /// <summary>
    /// JavaScript 属性 GPUObjectBase.label：USVString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </remarks>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUCanvasContext interface of the WebGPU API represents the WebGPU rendering context of a &lt;canvas&gt; element, returned via an HTMLCanvasElement.getContext() call with a contextType of &quot;webgpu&quot;.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCanvasContext">MDN Web Docs: GPUCanvasContext</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUCanvasContext")]
public class GPUCanvasContext
{
    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The canvas read-only property of the GPUCanvasContext interface returns a reference to the canvas that the context was created from.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCanvasContext/canvas">MDN Web Docs: GPUCanvasContext.canvas</see>
    /// </remarks>
    [Description("@#canvas")]
    public extern GPUCanvasContextCanvas Canvas { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The configure() method of the GPUCanvasContext interface configures the context to use for rendering with a given GPUDevice. When called the canvas will initially be cleared to transparent black.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCanvasContext/configure">MDN Web Docs: GPUCanvasContext.configure</see>
    /// </remarks>
    /// <param name="configuration">An object containing the following properties: alphaMode Optional An enumerated value that specifies the effect that alpha values will have on the content of textures returned by getCurrentTexture() when read, displayed, or used as an image source. Possible values are: opaque: Alpha values are ignored — if a texture is not already opaque, the alpha channel is cleared to 1.0 when it is used as an image source or displayed to the screen. This is the default value. premultiplied: Color values are premultiplied by their alpha value. For example, 100% red at 50% alpha is [0.5, 0, 0, 0.5]. colorSpace Optional The color space that values written into textures returned by getCurrentTexture() should be displayed with. Possible values are srgb (the default) and display-p3. device The GPUDevice that the rendering information for the context will come from. format The format that textures returned by getCurrentTexture() will have. This can be bgra8unorm, rgba8unorm, or rgba16float. The optimal canvas texture format for the current system can be returned by GPU.getPreferredCanvasFormat(). Using this is recommended — if you don&apos;t use the preferred format when configuring the canvas context, you may incur additional overhead, such as additional texture copies, depending on the platform. toneMapping Optional An object specifying parameters that define the tone mapping for the context — how the content of associated textures are to be displayed. This allows WebGPU to draw colors brighter than white (#FFFFFF). Possible properties are: mode Optional An enumerated value specifying the tone mapping mode for the canvas. Possible values include: standard The default value. Restricts rendered content to the Standard Dynamic Range (SDR) of the display. This mode is accomplished by clamping all color values in the color space of the screen to the [0, 1] interval. extended Allows content to be rendered in the full High Dynamic Range (HDR) of the display, where available. HDR mode allows a wider range of colors and brightness levels to be displayed, with more precise instructions as to what color should be displayed in each case. This mode matches &quot;standard&quot; in the [0, 1] range of the screen. Clamping or projection is done to the extended dynamic range of the screen but not [0, 1]. usage Optional Bitwise flags specifying the allowed usage for textures returned by getCurrentTexture(). Possible values are: GPUTextureUsage.COPY_SRC: The texture can be used as the source of a copy operation, for example the source argument of a GPUCommandEncoder.copyTextureToBuffer() call. GPUTextureUsage.COPY_DST: The texture can be used as the destination of a copy/write operation, for example the destination argument of a GPUCommandEncoder.copyTextureToTexture() call. GPUTextureUsage.RENDER_ATTACHMENT: The texture can be used as a color attachment in a render pass, for example in a color attachment view in a GPUCommandEncoder.beginRenderPass() call. GPUTextureUsage.RENDER_ATTACHMENT is the default usage, but note that it is not automatically included if a different value is explicitly set; in such cases you need to include it in addition. GPUTextureUsage.TEXTURE_BINDING: The texture can be bound for use as a sampled texture in a shader, for example in a bind group entry in a GPUDevice.createBindGroup() call. GPUTextureUsage.STORAGE_BINDING: The texture can be bound for use as a storage texture in a shader, for example in a bind group entry in a GPUDevice.createBindGroup() call. Note that multiple possible usages can be specified using the bitwise OR operator. For example, usage: GPUTextureUsage.COPY_SRC | GPUTextureUsage.RENDER_ATTACHMENT. viewFormats Optional An array of formats that views created from textures returned by getCurrentTexture() may use. See Texture Formats for all the possible values. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCanvasContext/configure">MDN Web Docs: configuration</see></param>
    [Description("@#configure")]
    public extern void Configure(GPUCanvasConfiguration configuration);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The unconfigure() method of the GPUCanvasContext interface removes any previously-set context configuration, and destroys any textures returned via getCurrentTexture() while the canvas context was configured.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCanvasContext/unconfigure">MDN Web Docs: GPUCanvasContext.unconfigure</see>
    /// </remarks>
    [Description("@#unconfigure")]
    public extern void Unconfigure();

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The getConfiguration() method of the GPUCanvasContext interface returns the current configuration set for the context.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCanvasContext/getConfiguration">MDN Web Docs: GPUCanvasContext.getConfiguration</see>
    /// </remarks>
    /// <returns>An object containing the configuration options set on the context (i.e., via the GPUCanvasContext.configure() method), or null if no configuration is set (either no configuration was previously set, or a configuration was set and then GPUCanvasContext.unconfigure() was called on the context).</returns>
    [Description("@#getConfiguration")]
    public extern GPUCanvasConfiguration? GetConfiguration();

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The getCurrentTexture() method of the GPUCanvasContext interface returns the next GPUTexture to be composited to the document by the canvas context.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCanvasContext/getCurrentTexture">MDN Web Docs: GPUCanvasContext.getCurrentTexture</see>
    /// </remarks>
    /// <returns>A GPUTexture object instance.</returns>
    [Description("@#getCurrentTexture")]
    public extern GPUTexture GetCurrentTexture();
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUCommandBuffer interface of the WebGPU API represents a pre-recorded list of GPU commands that can be submitted to a GPUQueue for execution. A GPUCommandBuffer is created via the GPUCommandEncoder.finish() method; the GPU commands recorded within are submitted for execution by passing the GPUCommandBuffer into the parameter of a GPUQueue.submit() call. Note: Once a GPUCommandBuffer object has been submitted, it cannot be used again.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandBuffer">MDN Web Docs: GPUCommandBuffer</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUCommandBuffer")]
public class GPUCommandBuffer
{
    #region mixin GPUObjectBase
    /// <summary>
    /// JavaScript 属性 GPUObjectBase.label：USVString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </remarks>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUCommandEncoder interface of the WebGPU API represents an encoder that collects a sequence of GPU commands to be issued to the GPU. A GPUCommandEncoder object instance is created via the GPUDevice.createCommandEncoder() property.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder">MDN Web Docs: GPUCommandEncoder</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUCommandEncoder")]
public class GPUCommandEncoder
{
    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The beginRenderPass() method of the GPUCommandEncoder interface starts encoding a render pass, returning a GPURenderPassEncoder that can be used to control rendering.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/beginRenderPass">MDN Web Docs: GPUCommandEncoder.beginRenderPass</see>
    /// </remarks>
    /// <param name="descriptor">An object containing the following properties: colorAttachments An array of objects (see Color attachment object structure) defining the color attachments that will be output to when executing this render pass. depthStencilAttachment Optional An object (see Depth/stencil attachment object structure) defining the depth/stencil attachment that will be output to and tested against when executing this render pass. label Optional A string providing a label that can be used to identify the object, for example in GPUError messages or console warnings. maxDrawCount Optional A number indicating the maximum number of draw calls that will be done in the render pass. This is used by some implementations to size work injected before the render pass. You should keep the default value — 50000000 — unless you know that more draw calls will be done. occlusionQuerySet Optional The GPUQuerySet that will store the occlusion query results for this pass. timestampWrites Optional An array of objects defining where and when timestamp query values will be written for this pass. These objects have the following properties: querySet A GPUQuerySet of type &quot;timestamp&quot; that the timestamp query results will be written to. beginningOfPassWriteIndex A number specifying the query index in querySet where the timestamp at the beginning of the render pass will be written. This is optional - if not defined, no timestamp will be written for the beginning of the pass. endOfPassWriteIndex A number specifying the query index in querySet where the timestamp at the end of the render pass will be written. This is optional - if not defined, no timestamp will be written for the end of the pass. Note: The timestamp-query feature needs to be enabled to use timestamp queries. Timestamp query values are written in nanoseconds, but how the value is determined is implementation-defined. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/beginRenderPass">MDN Web Docs: descriptor</see></param>
    /// <returns>A GPURenderPassEncoder object instance.</returns>
    [Description("@#beginRenderPass")]
    public extern GPURenderPassEncoder BeginRenderPass(GPURenderPassDescriptor descriptor);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The beginComputePass() method of the GPUCommandEncoder interface starts encoding a compute pass, returning a GPUComputePassEncoder that can be used to control computation.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/beginComputePass">MDN Web Docs: GPUCommandEncoder.beginComputePass</see>
    /// </remarks>
    /// <param name="descriptor">An object containing the following properties: label Optional A string providing a label that can be used to identify the object, for example in GPUError messages or console warnings. timestampWrites Optional An array of objects defining where and when timestamp query values will be written for this pass. These objects have the following properties: querySet A GPUQuerySet of type &quot;timestamp&quot; that the timestamp query results will be written to. beginningOfPassWriteIndex A number specifying the query index in querySet where the timestamp at the beginning of the render pass will be written. This is optional - if not defined, no timestamp will be written for the beginning of the pass. endOfPassWriteIndex A number specifying the query index in querySet where the timestamp at the end of the render pass will be written. This is optional - if not defined, no timestamp will be written for the end of the pass. Note: The timestamp-query feature needs to be enabled to use timestamp queries. Timestamp query values are written in nanoseconds, but how the value is determined is implementation-defined. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/beginComputePass">MDN Web Docs: descriptor</see></param>
    /// <returns>A GPUComputePassEncoder object instance.</returns>
    [Description("@#beginComputePass")]
    public extern GPUComputePassEncoder BeginComputePass(GPUComputePassDescriptor? descriptor = default);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. The copyBufferToBuffer() method of the GPUCommandEncoder interface encodes a command that copies data from one GPUBuffer to another.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/copyBufferToBuffer">MDN Web Docs: GPUCommandEncoder.copyBufferToBuffer</see>
    /// </remarks>
    /// <param name="source">The GPUBuffer to copy from. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/copyBufferToBuffer">MDN Web Docs: source</see></param>
    /// <param name="destination">The GPUBuffer to copy to. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/copyBufferToBuffer">MDN Web Docs: destination</see></param>
    /// <param name="size">The number of bytes to copy. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/copyBufferToBuffer">MDN Web Docs: size</see></param>
    [Description("@#copyBufferToBuffer")]
    public extern void CopyBufferToBuffer(GPUBuffer source, GPUBuffer destination, GPUSize64? size = default);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. The copyBufferToBuffer() method of the GPUCommandEncoder interface encodes a command that copies data from one GPUBuffer to another.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/copyBufferToBuffer">MDN Web Docs: GPUCommandEncoder.copyBufferToBuffer</see>
    /// </remarks>
    /// <param name="source">The GPUBuffer to copy from. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/copyBufferToBuffer">MDN Web Docs: source</see></param>
    /// <param name="sourceOffset">The offset, in bytes, into the source to begin copying from. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/copyBufferToBuffer">MDN Web Docs: sourceOffset</see></param>
    /// <param name="destination">The GPUBuffer to copy to. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/copyBufferToBuffer">MDN Web Docs: destination</see></param>
    /// <param name="destinationOffset">The offset, in bytes, into the destination to begin copying to. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/copyBufferToBuffer">MDN Web Docs: destinationOffset</see></param>
    /// <param name="size">The number of bytes to copy. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/copyBufferToBuffer">MDN Web Docs: size</see></param>
    [Description("@#copyBufferToBuffer")]
    public extern void CopyBufferToBuffer(GPUBuffer source, GPUSize64 sourceOffset, GPUBuffer destination, GPUSize64 destinationOffset, GPUSize64? size = default);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. The copyBufferToTexture() method of the GPUCommandEncoder interface encodes a command that copies data from a GPUBuffer to a GPUTexture.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/copyBufferToTexture">MDN Web Docs: GPUCommandEncoder.copyBufferToTexture</see>
    /// </remarks>
    /// <param name="source">An object that defines the buffer to copy from, plus the layout of the data in the buffer to be copied to the texture. Combined with copySize, it defines the region of the source buffer. source can take the following properties: buffer The GPUBuffer to copy from. offset Optional The offset, in bytes, from the beginning of data to the start of the image data to be copied. If omitted, offset defaults to 0. bytesPerRow Optional A number representing the stride, in bytes, between the start of each block row (i.e., a row of complete texel blocks) and the subsequent block row. This is required if there are multiple block rows (i.e., the copy height or depth is more than one block). rowsPerImage Optional The number of block rows per single image inside the data. bytesPerRow × rowsPerImage will give you the stride, in bytes, between the start of each complete image. This is required if there are multiple images to copy. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/copyBufferToTexture">MDN Web Docs: source</see></param>
    /// <param name="destination">An object defining the texture to write the data to. Combined with copySize, defines the region of the destination texture subresource. destination can take the following properties: aspect Optional An enumerated value defining which aspects of the texture to write the data to. Possible values are: &quot;all&quot; All available aspects of the texture format will be written to, which can mean all or any of color, depth, and stencil, depending on what kind of format you are dealing with. &quot;depth-only&quot; Only the depth aspect of a depth-or-stencil format will be written to. &quot;stencil-only&quot; Only the stencil aspect of a depth-or-stencil format will be written to. If omitted, aspect takes a value of &quot;all&quot;. mipLevel Optional A number representing the mip-map level of the texture to write the data to. If omitted, mipLevel defaults to 0. origin Optional An object or array specifying the origin of the copy — the minimum corner of the texture region to write the data to. Together with size, this defines the full extent of the region to copy to. The x, y, and z values default to 0 if any of all of origin is omitted. For example, you can pass an array like [0, 0, 0], or its equivalent object { x: 0, y: 0, z: 0 }. texture A GPUTexture object representing the texture to write the data to. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/copyBufferToTexture">MDN Web Docs: destination</see></param>
    /// <param name="copySize">An object or array specifying the width, height, and depth/array layer count of the copied data. The width value must always be specified, while the height and depth/array layer count values are optional and will default to 1 if omitted. For example, you can pass an array [16, 16, 2], or its equivalent object { width: 16, height: 16, depthOrArrayLayers: 2 }. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/copyBufferToTexture">MDN Web Docs: copySize</see></param>
    [Description("@#copyBufferToTexture")]
    public extern void CopyBufferToTexture(GPUTexelCopyBufferInfo source, GPUTexelCopyTextureInfo destination, GPUExtent3D copySize);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. The copyTextureToBuffer() method of the GPUCommandEncoder interface encodes a command that copies data from a GPUTexture to a GPUBuffer.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/copyTextureToBuffer">MDN Web Docs: GPUCommandEncoder.copyTextureToBuffer</see>
    /// </remarks>
    /// <param name="source">An object defining the texture to copy the data from. Combined with copySize, defines the region of the source texture subresource. source can take the following properties: aspect Optional An enumerated value defining which aspects of the texture to copy the data from. Possible values are: &quot;all&quot; All available aspects of the texture format will be copied from, which can mean all or any of color, depth, and stencil, depending on what kind of format you are dealing with. &quot;depth-only&quot; Only the depth aspect of a depth-or-stencil format will be copied from. &quot;stencil-only&quot; Only the stencil aspect of a depth-or-stencil format will be copied from. If omitted, aspect takes a value of &quot;all&quot;. mipLevel Optional A number representing the mip-map level of the texture to copy the data from. If omitted, mipLevel defaults to 0. origin Optional An object or array specifying the origin of the copy — the minimum corner of the texture region to copy the data from. Together with size, this defines the full extent of the region to copy from. The x, y, and z values default to 0 if any of all of origin is omitted. For example, you can pass an array [0, 0, 0], or its equivalent object { x: 0, y: 0, z: 0 }. texture A GPUTexture object representing the texture to copy the data from. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/copyTextureToBuffer">MDN Web Docs: source</see></param>
    /// <param name="destination">An object that defines the buffer to write to, plus the layout of the data to write to the buffer. Combined with copySize, it defines the region of the destination buffer. source can take the following properties: buffer The GPUBuffer to write to. offset Optional The offset, in bytes, from the beginning of data to the start position to write the copied data to. If omitted, offset defaults to 0. bytesPerRow Optional A number representing the stride, in bytes, between the start of each block row (i.e., a row of complete texel blocks) and the subsequent block row. This is required if there are multiple block rows (i.e., the copy height or depth is more than one block). rowsPerImage Optional The number of block rows per single image inside the data. bytesPerRow × rowsPerImage will give you the stride, in bytes, between the start of each complete image. This is required if there are multiple images to copy. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/copyTextureToBuffer">MDN Web Docs: destination</see></param>
    /// <param name="copySize">An object or array specifying the width, height, and depth/array layer count of the copied data. The width value must always be specified, while the height and depth/array layer count values are optional and will default to 1 if omitted. For example, you can pass an array [16, 16, 2], or its equivalent object { width: 16, height: 16, depthOrArrayLayers: 2 }. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/copyTextureToBuffer">MDN Web Docs: copySize</see></param>
    [Description("@#copyTextureToBuffer")]
    public extern void CopyTextureToBuffer(GPUTexelCopyTextureInfo source, GPUTexelCopyBufferInfo destination, GPUExtent3D copySize);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. The copyTextureToTexture() method of the GPUCommandEncoder interface encodes a command that copies data from one GPUTexture to another.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/copyTextureToTexture">MDN Web Docs: GPUCommandEncoder.copyTextureToTexture</see>
    /// </remarks>
    /// <param name="source">An object (see Copy texture object structure) defining the texture to copy the data from. Combined with copySize, this defines the region of the source texture subresource. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/copyTextureToTexture">MDN Web Docs: source</see></param>
    /// <param name="destination">An object (see Copy texture object structure) defining the texture to write the data to. Combined with copySize, this defines the region of the destination texture subresource. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/copyTextureToTexture">MDN Web Docs: destination</see></param>
    /// <param name="copySize">An object or array specifying the width, height, and depth/array layer count of the copied data. The width value must always be specified, while the height and depth/array layer count values are optional and will default to 1 if omitted. For example, you can pass an array [16, 16, 2], or its equivalent object { width: 16, height: 16, depthOrArrayLayers: 2 }. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/copyTextureToTexture">MDN Web Docs: copySize</see></param>
    [Description("@#copyTextureToTexture")]
    public extern void CopyTextureToTexture(GPUTexelCopyTextureInfo source, GPUTexelCopyTextureInfo destination, GPUExtent3D copySize);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The clearBuffer() method of the GPUCommandEncoder interface encodes a command that fills a region of a GPUBuffer with zeroes.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/clearBuffer">MDN Web Docs: GPUCommandEncoder.clearBuffer</see>
    /// </remarks>
    /// <param name="buffer">A GPUBuffer object representing the buffer to clear. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/clearBuffer">MDN Web Docs: buffer</see></param>
    /// <param name="offset">A number representing the offset, in bytes, from the start of the buffer to the sub-region to clear. If omitted, offset defaults to 0. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/clearBuffer">MDN Web Docs: offset</see></param>
    /// <param name="size">A number representing the size, in bytes, of the sub-region to clear. If omitted, size defaults to the buffer size - offset. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/clearBuffer">MDN Web Docs: size</see></param>
    [Description("@#clearBuffer")]
    public extern void ClearBuffer(GPUBuffer buffer, GPUSize64? offset = default, GPUSize64? size = default);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The resolveQuerySet() method of the GPUCommandEncoder interface encodes a command that resolves a GPUQuerySet, copying the results into a specified GPUBuffer.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/resolveQuerySet">MDN Web Docs: GPUCommandEncoder.resolveQuerySet</see>
    /// </remarks>
    /// <param name="querySet">A GPUQuerySet object representing the query set to be resolved. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/resolveQuerySet">MDN Web Docs: querySet</see></param>
    /// <param name="firstQuery">The index number of the first query value to be copied over to the buffer. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/resolveQuerySet">MDN Web Docs: firstQuery</see></param>
    /// <param name="queryCount">The number of queries to be copied over to the buffer, starting from firstQuery. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/resolveQuerySet">MDN Web Docs: queryCount</see></param>
    /// <param name="destination">A GPUBuffer representing the buffer to copy the query values to. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/resolveQuerySet">MDN Web Docs: destination</see></param>
    /// <param name="destinationOffset">A number representing the offset, in bytes, from the start of the buffer to start writing the query values at. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/resolveQuerySet">MDN Web Docs: destinationOffset</see></param>
    [Description("@#resolveQuerySet")]
    public extern void ResolveQuerySet(GPUQuerySet querySet, GPUSize32 firstQuery, GPUSize32 queryCount, GPUBuffer destination, GPUSize64 destinationOffset);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The finish() method of the GPUCommandEncoder interface completes recording of the command sequence encoded on this GPUCommandEncoder, returning a corresponding GPUCommandBuffer.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/finish">MDN Web Docs: GPUCommandEncoder.finish</see>
    /// </remarks>
    /// <param name="descriptor">An object that can contain the following properties: label Optional A string providing a label for the returned GPUCommandBuffer that can be used to identify it, for example in GPUError messages or console warnings. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCommandEncoder/finish">MDN Web Docs: descriptor</see></param>
    /// <returns>A GPUCommandBuffer object instance.</returns>
    [Description("@#finish")]
    public extern GPUCommandBuffer Finish(GPUCommandBufferDescriptor? descriptor = default);

    #region mixin GPUObjectBase
    /// <summary>
    /// JavaScript 属性 GPUObjectBase.label：USVString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </remarks>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion

    #region mixin GPUDebugCommandsMixin
    /// <summary>
    /// JavaScript GPUDebugCommandsMixin.pushDebugGroup(groupLabel) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-pushdebuggroup">WebGPU: 15. Debug Markers</see>
    /// </remarks>
    /// <param name="groupLabel"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-pushdebuggroup-grouplabel-grouplabel">WebGPU: 15. Debug Markers</see></param>
    [Description("@#pushDebugGroup")]
    public extern void PushDebugGroup(string groupLabel);

    /// <summary>
    /// JavaScript GPUDebugCommandsMixin.popDebugGroup() 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-popdebuggroup">WebGPU: 15. Debug Markers</see>
    /// </remarks>
    [Description("@#popDebugGroup")]
    public extern void PopDebugGroup();

    /// <summary>
    /// JavaScript GPUDebugCommandsMixin.insertDebugMarker(markerLabel) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-insertdebugmarker">WebGPU: 15. Debug Markers</see>
    /// </remarks>
    /// <param name="markerLabel"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-insertdebugmarker-markerlabel-markerlabel">WebGPU: 15. Debug Markers</see></param>
    [Description("@#insertDebugMarker")]
    public extern void InsertDebugMarker(string markerLabel);
    #endregion
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUCompilationInfo interface of the WebGPU API represents an array of GPUCompilationMessage objects generated by the GPU shader module compiler to help diagnose problems with shader code. GPUCompilationInfo is accessed via GPUShaderModule.getCompilationInfo().
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCompilationInfo">MDN Web Docs: GPUCompilationInfo</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUCompilationInfo")]
public class GPUCompilationInfo
{
    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The messages read-only property of the GPUCompilationInfo interface is an array of GPUCompilationMessage objects, each one containing the details of an individual shader compilation message. Messages can be informational, warnings, or errors.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCompilationInfo/messages">MDN Web Docs: GPUCompilationInfo.messages</see>
    /// </remarks>
    [Description("@#messages")]
    public extern FrozenSet<GPUCompilationMessage> Messages { get; }
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUCompilationMessage interface of the WebGPU API represents a single informational, warning, or error message generated by the GPU shader module compiler. An array of GPUCompilationMessage objects is available in the messages property of the GPUCompilationInfo object accessed via GPUShaderModule.getCompilationInfo().
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCompilationMessage">MDN Web Docs: GPUCompilationMessage</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUCompilationMessage")]
public class GPUCompilationMessage
{
    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The message read-only property of the GPUCompilationMessage interface is a string representing human-readable message text.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCompilationMessage/message">MDN Web Docs: GPUCompilationMessage.message</see>
    /// </remarks>
    [Description("@#message")]
    public extern string Message { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The type read-only property of the GPUCompilationMessage interface is an enumerated value representing the type of the message. Each type represents a different severity level.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCompilationMessage/type">MDN Web Docs: GPUCompilationMessage.type</see>
    /// </remarks>
    [Description("@#type")]
    public extern GPUCompilationMessageType Type { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The lineNum read-only property of the GPUCompilationMessage interface is a number representing the line number in the shader code that the message corresponds to.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCompilationMessage/lineNum">MDN Web Docs: GPUCompilationMessage.lineNum</see>
    /// </remarks>
    [Description("@#lineNum")]
    public extern Number LineNum { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The linePos read-only property of the GPUCompilationMessage interface is a number representing the position in the code line that the message corresponds to. This could be an exact point, or the start of the relevant substring.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCompilationMessage/linePos">MDN Web Docs: GPUCompilationMessage.linePos</see>
    /// </remarks>
    [Description("@#linePos")]
    public extern Number LinePos { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The offset read-only property of the GPUCompilationMessage interface is a number representing the offset from the start of the shader code to the exact point, or the start of the relevant substring, that the message corresponds to.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCompilationMessage/offset">MDN Web Docs: GPUCompilationMessage.offset</see>
    /// </remarks>
    [Description("@#offset")]
    public extern Number Offset { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The length read-only property of the GPUCompilationMessage interface is a number representing the length of the substring that the message corresponds to.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUCompilationMessage/length">MDN Web Docs: GPUCompilationMessage.length</see>
    /// </remarks>
    [Description("@#length")]
    public extern Number Length { get; }
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUComputePassEncoder interface of the WebGPU API encodes commands related to controlling the compute shader stage, as issued by a GPUComputePipeline. It forms part of the overall encoding activity of a GPUCommandEncoder. A compute pipeline contains a single compute stage in which a compute shader takes general data, processes it in parallel across a specified number of workgroups, then returns the result in one or more buffers. A GPUComputePassEncoder object instance is created via the GPUCommandEncoder.beginComputePass() property.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUComputePassEncoder">MDN Web Docs: GPUComputePassEncoder</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUComputePassEncoder")]
public class GPUComputePassEncoder
{
    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The setPipeline() method of the GPUComputePassEncoder interface sets the GPUComputePipeline to use for this compute pass.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUComputePassEncoder/setPipeline">MDN Web Docs: GPUComputePassEncoder.setPipeline</see>
    /// </remarks>
    /// <param name="pipeline">The GPUComputePipeline to use for this compute pass. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUComputePassEncoder/setPipeline">MDN Web Docs: pipeline</see></param>
    [Description("@#setPipeline")]
    public extern void SetPipeline(GPUComputePipeline pipeline);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The dispatchWorkgroups() method of the GPUComputePassEncoder interface dispatches a specific grid of workgroups to perform the work being done by the current GPUComputePipeline (i.e., set via GPUComputePassEncoder.setPipeline()).
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUComputePassEncoder/dispatchWorkgroups">MDN Web Docs: GPUComputePassEncoder.dispatchWorkgroups</see>
    /// </remarks>
    /// <param name="workgroupCountX">The X dimension of the grid of workgroups to dispatch. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUComputePassEncoder/dispatchWorkgroups">MDN Web Docs: workgroupCountX</see></param>
    /// <param name="workgroupCountY">The Y dimension of the grid of workgroups to dispatch. If omitted, workgroupCountY defaults to 1. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUComputePassEncoder/dispatchWorkgroups">MDN Web Docs: workgroupCountY</see></param>
    /// <param name="workgroupCountZ">The Z dimension of the grid of workgroups to dispatch. If omitted, workgroupCountZ defaults to 1. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUComputePassEncoder/dispatchWorkgroups">MDN Web Docs: workgroupCountZ</see></param>
    [Description("@#dispatchWorkgroups")]
    public extern void DispatchWorkgroups(GPUSize32 workgroupCountX, GPUSize32? workgroupCountY = default, GPUSize32? workgroupCountZ = default);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The dispatchWorkgroupsIndirect() method of the GPUComputePassEncoder interface dispatches a grid of workgroups, defined by the parameters of a GPUBuffer, to perform the work being done by the current GPUComputePipeline (i.e., set via GPUComputePassEncoder.setPipeline()).
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUComputePassEncoder/dispatchWorkgroupsIndirect">MDN Web Docs: GPUComputePassEncoder.dispatchWorkgroupsIndirect</see>
    /// </remarks>
    /// <param name="indirectBuffer">A GPUBuffer containing the X, Y, and Z dimensions of the grid of workgroups to dispatch. The buffer must contain a tightly packed block of three 32-bit unsigned integer values representing the dimensions (12 bytes total), given in the same order as the arguments for GPUComputePassEncoder.dispatchWorkgroups(). So for example: jsconst uint32 = new Uint32Array(3); uint32[0] = 25; // The X value uint32[1] = 1; // The Y value uint32[2] = 1; // The Z value // Write values into a GPUBuffer device.queue.writeBuffer(buffer, 0, uint32, 0, uint32.length); <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUComputePassEncoder/dispatchWorkgroupsIndirect">MDN Web Docs: indirectBuffer</see></param>
    /// <param name="indirectOffset">The offset, in bytes, into indirectBuffer where the dimension data begins. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUComputePassEncoder/dispatchWorkgroupsIndirect">MDN Web Docs: indirectOffset</see></param>
    [Description("@#dispatchWorkgroupsIndirect")]
    public extern void DispatchWorkgroupsIndirect(GPUBuffer indirectBuffer, GPUSize64 indirectOffset);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The end() method of the GPUComputePassEncoder interface completes recording of the current compute pass command sequence.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUComputePassEncoder/end">MDN Web Docs: GPUComputePassEncoder.end</see>
    /// </remarks>
    [Description("@#end")]
    public extern void End();

    #region mixin GPUObjectBase
    /// <summary>
    /// JavaScript 属性 GPUObjectBase.label：USVString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </remarks>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion

    #region mixin GPUDebugCommandsMixin
    /// <summary>
    /// JavaScript GPUDebugCommandsMixin.pushDebugGroup(groupLabel) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-pushdebuggroup">WebGPU: 15. Debug Markers</see>
    /// </remarks>
    /// <param name="groupLabel"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-pushdebuggroup-grouplabel-grouplabel">WebGPU: 15. Debug Markers</see></param>
    [Description("@#pushDebugGroup")]
    public extern void PushDebugGroup(string groupLabel);

    /// <summary>
    /// JavaScript GPUDebugCommandsMixin.popDebugGroup() 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-popdebuggroup">WebGPU: 15. Debug Markers</see>
    /// </remarks>
    [Description("@#popDebugGroup")]
    public extern void PopDebugGroup();

    /// <summary>
    /// JavaScript GPUDebugCommandsMixin.insertDebugMarker(markerLabel) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-insertdebugmarker">WebGPU: 15. Debug Markers</see>
    /// </remarks>
    /// <param name="markerLabel"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-insertdebugmarker-markerlabel-markerlabel">WebGPU: 15. Debug Markers</see></param>
    [Description("@#insertDebugMarker")]
    public extern void InsertDebugMarker(string markerLabel);
    #endregion

    #region mixin GPUBindingCommandsMixin
    /// <summary>
    /// JavaScript GPUBindingCommandsMixin.setBindGroup(index, bindGroup, dynamicOffsets) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup">WebGPU: 14.1 Bind Groups</see>
    /// </remarks>
    /// <param name="index"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsets-index">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="bindGroup"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsets-bindgroup">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="dynamicOffsets"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsets-dynamicoffsets">WebGPU: 14.1 Bind Groups</see></param>
    [Description("@#setBindGroup")]
    public extern void SetBindGroup(GPUIndex32 index, GPUBindGroup? bindGroup, GPUBufferDynamicOffset[]? dynamicOffsets = default);

    /// <summary>
    /// JavaScript GPUBindingCommandsMixin.setBindGroup(index, bindGroup, dynamicOffsetsData, dynamicOffsetsDataStart, dynamicOffsetsDataLength) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength">WebGPU: 14.1 Bind Groups</see>
    /// </remarks>
    /// <param name="index"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-index">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="bindGroup"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-bindgroup">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="dynamicOffsetsData"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-dynamicoffsetsdata">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="dynamicOffsetsDataStart"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-dynamicoffsetsdatastart">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="dynamicOffsetsDataLength"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-dynamicoffsetsdatalength">WebGPU: 14.1 Bind Groups</see></param>
    [Description("@#setBindGroup")]
    public extern void SetBindGroup(GPUIndex32 index, GPUBindGroup? bindGroup, Uint32Array dynamicOffsetsData, GPUSize64 dynamicOffsetsDataStart, GPUSize32 dynamicOffsetsDataLength);

    /// <summary>
    /// JavaScript GPUBindingCommandsMixin.setImmediates(rangeOffset, data, dataOffset, dataSize) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates">WebGPU: 14.2 Immediate Data</see>
    /// </remarks>
    /// <param name="rangeOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates-rangeoffset-data-dataoffset-datasize-rangeoffset">WebGPU: 14.2 Immediate Data</see></param>
    /// <param name="data"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates-rangeoffset-data-dataoffset-datasize-data">WebGPU: 14.2 Immediate Data</see></param>
    /// <param name="dataOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates-rangeoffset-data-dataoffset-datasize-dataoffset">WebGPU: 14.2 Immediate Data</see></param>
    /// <param name="dataSize"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates-rangeoffset-data-dataoffset-datasize-datasize">WebGPU: 14.2 Immediate Data</see></param>
    [Description("@#setImmediates")]
    public extern void SetImmediates(GPUSize32 rangeOffset, IAllowSharedBufferSource data, GPUSize64? dataOffset = default, GPUSize64? dataSize = default);
    #endregion
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUComputePipeline interface of the WebGPU API represents a pipeline that controls the compute shader stage and can be used in a GPUComputePassEncoder. A GPUComputePipeline object instance can be created using the GPUDevice.createComputePipeline() or GPUDevice.createComputePipelineAsync() methods.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUComputePipeline">MDN Web Docs: GPUComputePipeline</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUComputePipeline")]
public class GPUComputePipeline
{
    #region mixin GPUObjectBase
    /// <summary>
    /// JavaScript 属性 GPUObjectBase.label：USVString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </remarks>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion

    #region mixin GPUPipelineBase
    /// <summary>
    /// JavaScript GPUPipelineBase.getBindGroupLayout(index) 的强类型绑定，WebIDL 返回类型为 GPUBindGroupLayout。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpupipelinebase-getbindgrouplayout">WebGPU: 10.1 Base pipelines</see>
    /// </remarks>
    /// <param name="index"><see href="https://gpuweb.github.io/gpuweb/#dom-gpupipelinebase-getbindgrouplayout-index-index">WebGPU: 10.1 Base pipelines</see></param>
    [Description("@#getBindGroupLayout")]
    public extern GPUBindGroupLayout GetBindGroupLayout(uint index);
    #endregion
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUDevice interface of the WebGPU API represents a logical GPU device. This is the main interface through which the majority of WebGPU functionality is accessed. A GPUDevice object is requested using the GPUAdapter.requestDevice() method.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice">MDN Web Docs: GPUDevice</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUDevice")]
public partial class GPUDevice : EventTarget
{
    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The features read-only property of the GPUDevice interface returns a GPUSupportedFeatures object that describes additional functionality supported by the device. Only features requested during the creation of the device (i.e., when GPUAdapter.requestDevice() is called) are included. Note: Not all features will be available to WebGPU in all browsers that support it, even if the features are supported by the underlying hardware. See GPUAdapter.features for more details.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/features">MDN Web Docs: GPUDevice.features</see>
    /// </remarks>
    [Description("@#features")]
    public extern GPUSupportedFeatures Features { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The limits read-only property of the GPUDevice interface returns a GPUSupportedLimits object that describes the limits supported by the device. All limit values will be included, and the limits requested during the creation of the device (i.e., when GPUAdapter.requestDevice() is called) will be reflected in those values. Note: Not all limits will be reported as expected, even if they are supported by the underlying hardware. See GPUAdapter.limits for more details.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/limits">MDN Web Docs: GPUDevice.limits</see>
    /// </remarks>
    [Description("@#limits")]
    public extern GPUSupportedLimits Limits { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The adapterInfo read-only property of the GPUDevice interface returns a GPUAdapterInfo object containing identifying information about the device&apos;s originating adapter.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/adapterInfo">MDN Web Docs: GPUDevice.adapterInfo</see>
    /// </remarks>
    [Description("@#adapterInfo")]
    public extern GPUAdapterInfo AdapterInfo { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The queue read-only property of the GPUDevice interface returns the primary GPUQueue for the device.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/queue">MDN Web Docs: GPUDevice.queue</see>
    /// </remarks>
    [Description("@#queue")]
    public extern GPUQueue Queue { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The destroy() method of the GPUDevice interface destroys the device, preventing further operations on it. Note that: Any commands currently enqueued on the device&apos;s GPUQueue will be executed before the device is destroyed. Any WebGPU resources created using the device (buffers, textures, etc.) are also destroyed. Any mapped buffers created using the device will be unmapped.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/destroy">MDN Web Docs: GPUDevice.destroy</see>
    /// </remarks>
    [Description("@#destroy")]
    public extern void Destroy();

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The createBuffer() method of the GPUDevice interface creates a GPUBuffer in which to store raw data to use in GPU operations.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createBuffer">MDN Web Docs: GPUDevice.createBuffer</see>
    /// </remarks>
    /// <param name="descriptor">An object containing the following properties: label Optional A string providing a label that can be used to identify the object, for example in GPUError messages or console warnings. mappedAtCreation Optional A boolean. If set to true, the buffer will be mapped upon creation, meaning that you can set the values inside the buffer immediately by calling GPUBuffer.getMappedRange(). The default value is false. Note that it is valid to set mappedAtCreation: true so you can set the buffer&apos;s initial data, even if the GPUBufferUsage.MAP_READ or GPUBufferUsage.MAP_WRITE usage flags are not set. size A number representing the size of the buffer, in bytes. If mappedAtCreation is set to true, this must be a multiple of 4. usage The bitwise flags representing the allowed usages for the GPUBuffer. The possible values are in the GPUBuffer.usage value table. Note that multiple possible usages can be specified by separating values with bitwise OR, for example: GPUBufferUsage.COPY_SRC | GPUBufferUsage.MAP_WRITE. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createBuffer">MDN Web Docs: descriptor</see></param>
    /// <returns>A GPUBuffer object instance.</returns>
    [Description("@#createBuffer")]
    public extern GPUBuffer CreateBuffer(GPUBufferDescriptor descriptor);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The createTexture() method of the GPUDevice interface creates a GPUTexture in which to store 1D, 2D, or 3D arrays of data, such as images, to use in GPU rendering operations.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createTexture">MDN Web Docs: GPUDevice.createTexture</see>
    /// </remarks>
    /// <param name="descriptor">An object containing the following properties: dimension Optional An enumerated value indicating the dimension level of the texture. Possible values are: &quot;1d&quot;: The texture is one-dimensional. &quot;2d&quot;: The texture is two-dimensional or an array of two-dimensional layers. &quot;3d&quot;: The texture is three-dimensional. dimension defaults to &quot;2d&quot; if the value is omitted. format An enumerated value specifying the format of the texture. See the Texture formats section of the specification for all the possible values. Note: The depth32float-stencil8 feature needs to be enabled to create depth32float-stencil8-format GPUTextures. The texture-compression-bc feature needs to be enabled to create two-dimensional (dimension: &quot;2d&quot;) BC compressed GPUTextures: bc1-rgba-unorm, bc1-rgba-unorm-srgb, bc2-rgba-unorm, bc2-rgba-unorm-srgb, bc3-rgba-unorm, bc3-rgba-unorm-srgb, bc4-r-unorm, bc4-r-snorm, bc5-rg-unorm, bc5-rg-snorm, bc6h-rgb-ufloat, bc6h-rgb-float, bc7-rgba-unorm, and bc7-rgba-unorm-srgb formats. The texture-compression-bc and texture-compression-bc-sliced-3d features need to be enabled to create three-dimensional BC compressed GPUTextures (the same format values specified in the previous bullet, but with dimension set to 3d). The texture-compression-astc feature needs to be enabled to create two-dimensional (dimension: &quot;2d&quot;) ASTC compressed GPUTextures: astc-4x4-unorm, astc-4x4-unorm-srgb, astc-5x4-unorm, astc-5x4-unorm-srgb, astc-5x5-unorm, astc-5x5-unorm-srgb, astc-6x5-unorm, astc-6x5-unorm-srgb, astc-6x6-unorm, astc-6x6-unorm-srgb, astc-8x5-unorm, astc-8x5-unorm-srgb, astc-8x6-unorm, astc-8x6-unorm-srgb, astc-8x8-unorm, astc-8x8-unorm-srgb, astc-10x5-unorm, astc-10x5-unorm-srgb, astc-10x6-unorm, astc-10x6-unorm-srgb, astc-10x8-unorm, astc-10x8-unorm-srgb, astc-10x10-unorm, astc-10x10-unorm-srgb, astc-12x10-unorm, astc-12x10-unorm-srgb, astc-12x12-unorm, and astc-12x12-unorm-srgb formats. The texture-compression-astc and texture-compression-astc-sliced-3d features need to be enabled to create three-dimensional BC compressed GPUTextures (the same format values specified in the previous bullet, but with dimension set to 3d). The texture-compression-etc2 feature needs to be enabled to create two-dimensional ETC2 compressed GPUTextures: etc2-rgb8unorm, etc2-rgb8unorm-srgb, etc2-rgb8a1unorm, etc2-rgb8a1unorm-srgb, etc2-rgba8unorm, etc2-rgba8unorm-srgb, eac-r11unorm, eac-r11snorm, eac-rg11unorm, and eac-rg11snorm formats. See the Tier 1 and Tier 2 texture formats section for more information about those texture format sets and the requirements to create them. label Optional A string providing a label that can be used to identify the object, for example in GPUError messages or console warnings. mipLevelCount Optional A number specifying the number of mip levels the texture will contain. If omitted, this defaults to 1. sampleCount Optional A number specifying the texture&apos;s sample count. To be valid, the value must be 1 or 4. If omitted, this defaults to 1. A value higher than 1 indicates a multi-sampled texture. size An object or array specifying the width, height, and depth/array layer count of the texture. The width value must always be specified, while the height and depth/array layer count values are optional and will default to 1 if omitted. For example, you can pass an array like [16, 16, 2], or its equivalent object { width: 16, height: 16, depthOrArrayLayers: 2 }. usage The bitwise flags representing the allowed usages for the GPUTexture. The possible values are in the GPUTexture.usage value table. Note that multiple possible usages can be specified by separating values with bitwise OR, for example: GPUTextureUsage.COPY_DST | GPUTextureUsage.RENDER_ATTACHMENT. Note: The bgra8unorm-storage feature needs to be enabled to specify STORAGE_BINDING usage for a bgra8unorm-format GPUTexture. The rg11b10ufloat-renderable feature needs to be enabled to specify RENDER_ATTACHMENT usage for a rg11b10ufloat-format GPUTexture, as well as its blending and multisampling. viewFormats Optional An array of enumerated values specifying other texture formats permitted when calling GPUTexture.createView() on this texture, in addition to the texture format specified in its format value. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createTexture">MDN Web Docs: descriptor</see></param>
    /// <returns>A GPUTexture object instance.</returns>
    [Description("@#createTexture")]
    public extern GPUTexture CreateTexture(GPUTextureDescriptor descriptor);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The createSampler() method of the GPUDevice interface creates a GPUSampler, which controls how shaders transform and filter texture resource data.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createSampler">MDN Web Docs: GPUDevice.createSampler</see>
    /// </remarks>
    /// <param name="descriptor">An object containing the following properties: addressModeU Optional An enumerated value specifying the behavior of the sampler when the sample footprint width extends beyond the width of the texture. Possible values are: &quot;clamp-to-edge&quot;: The texture coordinates are clamped between 0.0 and 1.0, inclusive. &quot;repeat&quot;: The texture coordinates wrap to the other side of the texture. &quot;mirror-repeat&quot;: The texture coordinates wrap to the other side of the texture, but the texture is flipped when the integer part of the coordinate is odd. If omitted, addressModeU defaults to &quot;clamp-to-edge&quot;. addressModeV Optional An enumerated value specifying the behavior of the sampler when the sample footprint height extends beyond the height of the texture. Possible and default values are the same as for addressModeU. addressModeW Optional An enumerated value specifying the behavior of the sampler when the sample footprint depth extends beyond the depth of the texture. Possible and default values are the same as for addressModeU. compare Optional If specified, the sampler will be a comparison sampler of the specified type. Possible (enumerated) values are: &quot;never&quot;: Comparison tests never pass. &quot;less&quot;: A provided value passes the comparison test if it is less than the sampled value. &quot;equal&quot;: A provided value passes the comparison test if it is equal to the sampled value. &quot;less-equal&quot;: A provided value passes the comparison test if it is less than or equal to the sampled value. &quot;greater&quot;: A provided value passes the comparison test if it is greater than the sampled value. &quot;not-equal&quot;: A provided value passes the comparison test if it is not equal to the sampled value. &quot;greater-equal&quot;: A provided value passes the comparison test if it is greater than or equal to the sampled value. &quot;always&quot;: Comparison tests always pass. Comparison samplers may use filtering, but the sampling results will be implementation-dependent and may differ from the normal filtering rules. label Optional A string providing a label that can be used to identify the object, for example in GPUError messages or console warnings. lodMinClamp Optional A number specifying the minimum level of detail used internally when sampling a texture. If omitted, lodMinClamp defaults to 0. lodMaxClamp Optional A number specifying the maximum level of detail used internally when sampling a texture. If omitted, lodMaxClamp defaults to 32. maxAnisotropy Optional Specifies the maximum anisotropy value clamp used by the sampler. If omitted, maxAnisotropy defaults to 1. Most implementations support maxAnisotropy values in a range between 1 and 16, inclusive. The value used will be clamped to the maximum value that the underlying platform supports. magFilter Optional An enumerated value specifying the sampling behavior when the sample footprint is smaller than or equal to one texel. Possible values are: &quot;nearest&quot;: Return the value of the texel nearest to the texture coordinates. &quot;linear&quot;: Select two texels in each dimension and return a linear interpolation between their values. If omitted, magFilter defaults to &quot;nearest&quot;. Note: The float32-filterable feature needs to be enabled for r32float-, rg32float-, and rgba32float-format GPUTextures to be filterable. minFilter Optional An enumerated value specifying the sampling behavior when the sample footprint is larger than one texel. Possible and default values are the same as for magFilter. mipmapFilter Optional An enumerated value specifying the behavior when sampling between mipmap levels. Possible and default values are the same as for magFilter. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createSampler">MDN Web Docs: descriptor</see></param>
    /// <returns>A GPUSampler object instance.</returns>
    [Description("@#createSampler")]
    public extern GPUSampler CreateSampler(GPUSamplerDescriptor? descriptor = default);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The importExternalTexture() method of the GPUDevice interface takes an HTMLVideoElement or a VideoFrame object as an input and returns a GPUExternalTexture wrapper object containing a snapshot of the video that can be used as a frame in GPU rendering operations.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/importExternalTexture">MDN Web Docs: GPUDevice.importExternalTexture</see>
    /// </remarks>
    /// <param name="descriptor">An object containing the following properties: colorSpace Optional An enumerated value specifying the color space to use for the video frame. Possible values are &quot;srgb&quot; and &quot;display-p3&quot;. If omitted, colorSpace defaults to &quot;srgb&quot;. label Optional A string providing a label that can be used to identify the object, for example in GPUError messages or console warnings. source The HTMLVideoElement or VideoFrame source of the video snapshot. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/importExternalTexture">MDN Web Docs: descriptor</see></param>
    /// <returns>A GPUExternalTexture object instance. Note that the moment when the GPUExternalTexture object expires (is destroyed) depends on what its source is: GPUExternalTexture objects with an HTMLVideoElement source expire as soon as they are used (for example in a bind group). GPUExternalTexture objects with a VideoFrame source expire only when the VideoFrame is closed, for example via a VideoFrame.close() call.</returns>
    [Description("@#importExternalTexture")]
    public extern GPUExternalTexture ImportExternalTexture(GPUExternalTextureDescriptor descriptor);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The createBindGroupLayout() method of the GPUDevice interface creates a GPUBindGroupLayout that defines the structure and purpose of related GPU resources such as buffers that will be used in a pipeline, and is used as a template when creating GPUBindGroups.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createBindGroupLayout">MDN Web Docs: GPUDevice.createBindGroupLayout</see>
    /// </remarks>
    /// <param name="descriptor">An object containing the following properties: entries An array of entry objects, each one of which describes a single shader resource binding to be included in the GPUBindGroupLayout. Each entry will correspond to an entry defined in a GPUBindGroup (created via a GPUDevice.createBindGroup() call) that uses this GPUBindGroupLayout object as a template. label Optional A string providing a label that can be used to identify the object, for example in GPUError messages or console warnings. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createBindGroupLayout">MDN Web Docs: descriptor</see></param>
    /// <returns>A GPUBindGroupLayout object instance.</returns>
    [Description("@#createBindGroupLayout")]
    public extern GPUBindGroupLayout CreateBindGroupLayout(GPUBindGroupLayoutDescriptor descriptor);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The createPipelineLayout() method of the GPUDevice interface creates a GPUPipelineLayout that defines the GPUBindGroupLayouts used by a pipeline. GPUBindGroups used with the pipeline during command encoding must have compatible GPUBindGroupLayouts.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createPipelineLayout">MDN Web Docs: GPUDevice.createPipelineLayout</see>
    /// </remarks>
    /// <param name="descriptor">An object containing the following properties: bindGroupLayouts An array of values representing the bind group layouts for a pipeline. Each value can be: A GPUBindGroupLayout object, created via a call to GPUDevice.createBindGroupLayout(). Each object corresponds to a @group attribute in the shader code contained in the GPUShaderModule used in a related pipeline. null, which represents an empty bind group layout. null values are ignored when creating a pipeline layout. label Optional A string providing a label that can be used to identify the object, for example in GPUError messages or console warnings. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createPipelineLayout">MDN Web Docs: descriptor</see></param>
    /// <returns>A GPUPipelineLayout object instance.</returns>
    [Description("@#createPipelineLayout")]
    public extern GPUPipelineLayout CreatePipelineLayout(GPUPipelineLayoutDescriptor descriptor);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The createBindGroup() method of the GPUDevice interface creates a GPUBindGroup based on a GPUBindGroupLayout that defines a set of resources to be bound together in a group and how those resources are used in shader stages.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createBindGroup">MDN Web Docs: GPUDevice.createBindGroup</see>
    /// </remarks>
    /// <param name="descriptor">An object containing the following properties: entries An array of entry objects describing the resources to expose to the shader. There will be one for each corresponding entry described by the GPUBindGroupLayout referenced in layout. Each entry object has the following properties: binding A number representing a unique identifier for this resource binding, which matches the binding value of a corresponding GPUBindGroupLayout entry. In addition, it matches the n index value of the corresponding @binding(n) attribute in the shader (GPUShaderModule) used in the related pipeline. resource The resource to bind. This can be one of the following: GPUBufferBinding: Wraps a GPUBuffer; see GPUBufferBinding objects for a definition. GPUBuffer: Can be used directly rather than being wrapped in a GPUBufferBinding, provided the default offset and size values are being used. GPUExternalTexture GPUTextureView: Can be used in place of a GPUExternalTexture provided it is compatible (a 2D format with a single subresource, that is, dimension: &quot;2d&quot;). GPUTexture: Can be used in place of a GPUTextureView, provided a default view is desired. When used in this context, GPUTexture is equivalent to a GPUTextureView object created using a GPUTexture.createView() call with no argument specified. GPUSampler label Optional A string providing a label that can be used to identify the object, for example in GPUError messages or console warnings. layout The GPUBindGroupLayout that the entries of this bind group will conform to. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createBindGroup">MDN Web Docs: descriptor</see></param>
    /// <returns>A GPUBindGroup object instance.</returns>
    [Description("@#createBindGroup")]
    public extern GPUBindGroup CreateBindGroup(GPUBindGroupDescriptor descriptor);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The createShaderModule() method of the GPUDevice interface creates a GPUShaderModule from a string of WGSL source code.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createShaderModule">MDN Web Docs: GPUDevice.createShaderModule</see>
    /// </remarks>
    /// <param name="descriptor">An object containing the following properties: code A string representing the WGSL source code for the shader module. hints Optional A sequence of record types, with the structure (&quot;string&quot;, compilationHint). These behave like ordered maps. In each case, the &quot;string&quot; is a key used to identify or select the record, and the compilationHint is either a GPUPipelineLayout object instance or an enumerated value of &quot;auto&quot;. The point of hints is to provide information about the pipeline layout as early as possible to improve performance. The idea is to maximize the amount of compilation that can be done once by createShaderModule(), rather than multiple times in multiple calls to GPUDevice.createComputePipeline() and GPUDevice.createRenderPipeline(). Note: Different implementations may handle hints in different ways, including possibly ignoring them entirely. Providing hints does not guarantee improved shader compilation performance on all browsers/systems. label Optional A string providing a label that can be used to identify the object, for example in GPUError messages or console warnings. sourceMap Optional A source map definition to provide developer tool integration such as source-language debugging. WGSL names (identifiers) in source maps should follow the rules defined in WGSL identifier comparison. If defined, the source map may be interpreted as a source-map-v3 format. Note: Different implementations may handle sourceMaps in different ways, including possibly ignoring them entirely. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createShaderModule">MDN Web Docs: descriptor</see></param>
    /// <returns>A GPUShaderModule object instance.</returns>
    [Description("@#createShaderModule")]
    public extern GPUShaderModule CreateShaderModule(GPUShaderModuleDescriptor descriptor);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The createComputePipeline() method of the GPUDevice interface creates a GPUComputePipeline that can control the compute shader stage and be used in a GPUComputePassEncoder.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createComputePipeline">MDN Web Docs: GPUDevice.createComputePipeline</see>
    /// </remarks>
    /// <param name="descriptor">An object containing the following properties: compute An object describing the compute shader entry point of the pipeline. This object can contain the following properties: constants Optional A sequence of record types, with the structure (id, value), representing override values for WGSL constants that can be overridden in the pipeline. These behave like ordered maps. In each case, the id is a key used to identify or select the record, and the constant is an enumerated value representing a WGSL. Depending on which constant you want to override, the id may take the form of the numeric ID of the constant, if one is specified, or otherwise the constant&apos;s identifier name. A code snippet providing override values for several overridable constants might look like this: js({ // … constants: { 0: false, 1200: 3.0, 1300: 2.0, width: 20, depth: -1, height: 15, }, }); entryPoint Optional The name of the function in the module that this stage will use to perform its work. The corresponding shader function must have the @compute attribute to be identified as this entry point. See Entry Point Declaration for more information. You can omit the entryPoint property if your shader code contains a single function with the @compute attribute set — the browser will use this as the default entry point. If entryPoint is omitted and the browser cannot determine a default entry point, a GPUValidationError is generated and the resulting GPUComputePipeline will be invalid. module A GPUShaderModule object containing the WGSL code that this programmable stage will execute. label Optional A string providing a label that can be used to identify the object, for example in GPUError messages or console warnings. layout Defines the layout (structure, purpose, and type) of all the GPU resources (buffers, textures, etc.) used during the execution of the pipeline. Possible values are: A GPUPipelineLayout object, created using GPUDevice.createPipelineLayout(), which allows the GPU to figure out how to run the pipeline most efficiently ahead of time. A string of &quot;auto&quot;, which causes the pipeline to generate an implicit bind group layout based on any bindings defined in the shader code. If &quot;auto&quot; is used, the generated bind group layouts may only be used with the current pipeline. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createComputePipeline">MDN Web Docs: descriptor</see></param>
    /// <returns>A GPUComputePipeline object instance.</returns>
    [Description("@#createComputePipeline")]
    public extern GPUComputePipeline CreateComputePipeline(GPUComputePipelineDescriptor descriptor);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The createRenderPipeline() method of the GPUDevice interface creates a GPURenderPipeline that can control the vertex and fragment shader stages and be used in a GPURenderPassEncoder or GPURenderBundleEncoder.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createRenderPipeline">MDN Web Docs: GPUDevice.createRenderPipeline</see>
    /// </remarks>
    /// <param name="descriptor">An object containing the following properties: depthStencil Optional An object (see depthStencil object structure) describing depth-stencil properties including testing, operations, and bias. fragment Optional An object (see fragment object structure) describing the fragment shader entry point of the pipeline and its output colors. If no fragment shader entry point is defined, the pipeline will not produce any color attachment outputs, but it still performs rasterization and produces depth values based on the vertex position output. Depth testing and stencil operations can still be used. label Optional A string providing a label that can be used to identify the object, for example in GPUError messages or console warnings. layout Defines the layout (structure, purpose, and type) of all the GPU resources (buffers, textures, etc.) used during the execution of the pipeline. Possible values are: A GPUPipelineLayout object, created using GPUDevice.createPipelineLayout(), which allows the GPU to figure out how to run the pipeline most efficiently ahead of time. A string of &quot;auto&quot;, which causes the pipeline to generate an implicit bind group layout based on any bindings defined in the shader code. If &quot;auto&quot; is used, the generated bind group layouts may only be used with the current pipeline. multisample Optional An object (see multisample object structure) describing how the pipeline interacts with a render pass&apos;s multisampled attachments. primitive Optional An object (see primitive object structure) describing how a pipeline constructs and rasterizes primitives from its vertex inputs. vertex An object (see vertex object structure) describing the vertex shader entry point of the pipeline and its input buffer layouts. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createRenderPipeline">MDN Web Docs: descriptor</see></param>
    /// <returns>A GPURenderPipeline object instance.</returns>
    [Description("@#createRenderPipeline")]
    public extern GPURenderPipeline CreateRenderPipeline(GPURenderPipelineDescriptor descriptor);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The createComputePipelineAsync() method of the GPUDevice interface returns a Promise that fulfills with a GPUComputePipeline, which can control the compute shader stage and be used in a GPUComputePassEncoder, once the pipeline can be used without any stalling. Note: It is generally preferable to use this method over GPUDevice.createComputePipeline() whenever possible, as it prevents blocking of GPU operation execution on pipeline compilation.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createComputePipelineAsync">MDN Web Docs: GPUDevice.createComputePipelineAsync</see>
    /// </remarks>
    /// <param name="descriptor">See the descriptor definition for the GPUDevice.createComputePipeline() method. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createComputePipelineAsync">MDN Web Docs: descriptor</see></param>
    /// <returns>A Promise that fulfills with a GPUComputePipeline object instance when the created pipeline is ready to be used without additional delay.</returns>
    [Description("@#createComputePipelineAsync")]
    public extern PromiseResult<GPUComputePipeline> CreateComputePipelineAsync(GPUComputePipelineDescriptor descriptor);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The createRenderPipelineAsync() method of the GPUDevice interface returns a Promise that fulfills with a GPURenderPipeline, which can control the vertex and fragment shader stages and be used in a GPURenderPassEncoder or GPURenderBundleEncoder, once the pipeline can be used without any stalling. Note: It is generally preferable to use this method over GPUDevice.createRenderPipeline() whenever possible, as it prevents blocking of GPU operation execution on pipeline compilation.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createRenderPipelineAsync">MDN Web Docs: GPUDevice.createRenderPipelineAsync</see>
    /// </remarks>
    /// <param name="descriptor">See the descriptor definition for the GPUDevice.createRenderPipeline() method. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createRenderPipelineAsync">MDN Web Docs: descriptor</see></param>
    /// <returns>A Promise that fulfills with a GPURenderPipeline object instance when the created pipeline is ready to be used without additional delay.</returns>
    [Description("@#createRenderPipelineAsync")]
    public extern PromiseResult<GPURenderPipeline> CreateRenderPipelineAsync(GPURenderPipelineDescriptor descriptor);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The createCommandEncoder() method of the GPUDevice interface creates a GPUCommandEncoder, used to encode commands to be issued to the GPU.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createCommandEncoder">MDN Web Docs: GPUDevice.createCommandEncoder</see>
    /// </remarks>
    /// <param name="descriptor">An object containing the following properties: label Optional A string providing a label that can be used to identify the object, for example in GPUError messages or console warnings. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createCommandEncoder">MDN Web Docs: descriptor</see></param>
    /// <returns>A GPUCommandEncoder object instance.</returns>
    [Description("@#createCommandEncoder")]
    public extern GPUCommandEncoder CreateCommandEncoder(GPUCommandEncoderDescriptor? descriptor = default);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The createRenderBundleEncoder() method of the GPUDevice interface creates a GPURenderBundleEncoder that can be used to pre-record bundles of commands. These can be reused in GPURenderPassEncoders via the executeBundles() method, as many times as required.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createRenderBundleEncoder">MDN Web Docs: GPUDevice.createRenderBundleEncoder</see>
    /// </remarks>
    /// <param name="descriptor">An object containing the following properties: colorFormats An array of enumerated values specifying the expected color formats for render targets. For possible values, see the GPUTextureFormat definition in the spec. depthReadOnly Optional A boolean. If true, specifies that executing any GPURenderBundle created by the GPURenderBundleEncoder will not modify the depth component of the depthStencilFormat when executed. If omitted, depthReadOnly will default to false. depthStencilFormat Optional An enumerated value that specifies the expected depth-or-stencil format for render targets. For possible values, see the Depth-stencil formats section of the spec. label Optional A string providing a label that can be used to identify the object, for example in GPUError messages or console warnings. sampleCount Optional A number representing the expected sample count for render targets. stencilReadOnly Optional A boolean. If true, specifies that executing any GPURenderBundle created by the GPURenderBundleEncoder will not modify the stencil component of the depthStencilFormat when executed. If omitted, stencilReadOnly will default to false. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createRenderBundleEncoder">MDN Web Docs: descriptor</see></param>
    /// <returns>A GPURenderBundleEncoder object instance.</returns>
    [Description("@#createRenderBundleEncoder")]
    public extern GPURenderBundleEncoder CreateRenderBundleEncoder(GPURenderBundleEncoderDescriptor descriptor);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The createQuerySet() method of the GPUDevice interface creates a GPUQuerySet that can be used to record the results of queries on passes, such as occlusion or timestamp queries.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createQuerySet">MDN Web Docs: GPUDevice.createQuerySet</see>
    /// </remarks>
    /// <param name="descriptor">An object containing the following properties: count A number specifying the number of queries to be managed by the resulting GPUQuerySet. label Optional A string providing a label that can be used to identify the object, for example in GPUError messages or console warnings. type An enumerated value specifying the type of queries to be managed by the resulting GPUQuerySet. Possible values are: &quot;occlusion&quot; Occlusion queries are available on render passes to query the number of fragment samples that pass all the per-fragment tests for a set of drawing commands (including scissor, sample mask, alpha to coverage, stencil, and depth tests). To run an occlusion query, an appropriate GPUQuerySet must be provided as the value of the occlusionQuerySet descriptor property when invoking GPUCommandEncoder.beginRenderPass() to run a render pass. &quot;timestamp&quot; Timestamp queries allow applications to write timestamps to a GPUQuerySet. To run a timestamp query, appropriate GPUQuerySets must be provided inside the value of the timestampWrites descriptor property when invoking GPUCommandEncoder.beginRenderPass() to run a render pass, or GPUCommandEncoder.beginComputePass() to run a compute pass. Alternatively, you can run a single timestamp query at any time by invoking GPUCommandEncoder.writeTimeStamp() with an appropriate GPUQuerySet as a parameter. Note: The timestamp-query feature needs to be enabled to use timestamp queries. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/createQuerySet">MDN Web Docs: descriptor</see></param>
    /// <returns>A GPUQuerySet object instance.</returns>
    [Description("@#createQuerySet")]
    public extern GPUQuerySet CreateQuerySet(GPUQuerySetDescriptor descriptor);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The lost read-only property of the GPUDevice interface contains a Promise that remains pending throughout the device&apos;s lifetime and resolves with a GPUDeviceLostInfo object when the device is lost. GPUAdapter.requestDevice() will never return null, and it will reject only if the request is invalid, i.e., it exceeds the capabilities of the GPUAdapter. If a valid device request can&apos;t be fulfilled for some reason however it may resolve to a device that has already been lost. Additionally, devices can be lost at any time after creation for a variety of reasons (such as browser resource management or driver updates), so it&apos;s a good idea to always handle lost devices gracefully. Many causes for lost devices are transient, so you should try getting a new device once a previous one has been lost unless the loss was caused by the application intentionally destroying the device (i.e., with GPUDevice.destroy()). Note that any WebGPU resources created with a previous device (buffers, textures, etc.) will need to be re-created with the new one. Note: Also bear in mind that a GPUAdapter may become unavailable, e.g., if the physical GPU is unplugged from the system or disabled to save power. From then on, the adapter can no longer return valid devices, and will always return already-lost devices.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/lost">MDN Web Docs: GPUDevice.lost</see>
    /// </remarks>
    [Description("@#lost")]
    public extern PromiseResult<GPUDeviceLostInfo> Lost { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The pushErrorScope() method of the GPUDevice interface pushes a new GPU error scope onto the device&apos;s error scope stack, allowing you to capture errors of a particular type. Once you are done capturing errors, you can end capture by invoking GPUDevice.popErrorScope(). This pops the scope from the stack and returns a Promise that resolves to an object describing the first error captured in the scope, or null if no errors were captured.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/pushErrorScope">MDN Web Docs: GPUDevice.pushErrorScope</see>
    /// </remarks>
    /// <param name="filter">An enumerated value that specifies what type of error will be caught in this particular error scope. Possible values are: &quot;internal&quot; The error scope will catch a GPUInternalError. &quot;out-of-memory&quot; The error scope will catch a GPUOutOfMemoryError. &quot;validation&quot; The error scope will catch a GPUValidationError. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/pushErrorScope">MDN Web Docs: filter</see></param>
    [Description("@#pushErrorScope")]
    public extern void PushErrorScope(GPUErrorFilter filter);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The popErrorScope() method of the GPUDevice interface pops an existing GPU error scope from the error scope stack (originally pushed using GPUDevice.pushErrorScope()) and returns a Promise that resolves to an object describing the first error captured in the scope, or null if no error occurred.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDevice/popErrorScope">MDN Web Docs: GPUDevice.popErrorScope</see>
    /// </remarks>
    /// <returns>A Promise that resolves to an object describing the first error captured in the scope. This can be of type: GPUInternalError GPUOutOfMemoryError GPUValidationError If no error occurred, it resolves to null.</returns>
    [Description("@#popErrorScope")]
    public extern PromiseResult<GPUError?> PopErrorScope();

    /// <summary>
    /// JavaScript 属性 GPUDevice.onuncapturederror：EventHandler。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-onuncapturederror">WebGPU: 22.4 Telemetry</see>
    /// </remarks>
    [Description("@#onuncapturederror")]
    public extern EventHandler Onuncapturederror { get; set; }

    #region mixin GPUObjectBase
    /// <summary>
    /// JavaScript 属性 GPUObjectBase.label：USVString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </remarks>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUDeviceLostInfo interface of the WebGPU API represents the object returned when the GPUDevice.lost Promise resolves. This provides information as to why a device has been lost. See the GPUDevice.lost page for more information about &quot;lost&quot; state.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDeviceLostInfo">MDN Web Docs: GPUDeviceLostInfo</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUDeviceLostInfo")]
public class GPUDeviceLostInfo
{
    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The reason read-only property of the GPUDeviceLostInfo interface defines the reason the device was lost in a machine-readable way.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDeviceLostInfo/reason">MDN Web Docs: GPUDeviceLostInfo.reason</see>
    /// </remarks>
    [Description("@#reason")]
    public extern GPUDeviceLostReason Reason { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The message read-only property of the GPUDeviceLostInfo interface provides a human-readable message that explains why the device was lost.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUDeviceLostInfo/message">MDN Web Docs: GPUDeviceLostInfo.message</see>
    /// </remarks>
    [Description("@#message")]
    public extern string Message { get; }
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUError interface of the WebGPU API is the base interface for errors surfaced by GPUDevice.popErrorScope and the uncapturederror event.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUError">MDN Web Docs: GPUError</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUError")]
public class GPUError
{
    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The message read-only property of the GPUError interface provides a human-readable message that explains why the error occurred.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUError/message">MDN Web Docs: GPUError.message</see>
    /// </remarks>
    [Description("@#message")]
    public extern string Message { get; }
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUExternalTexture interface of the WebGPU API represents a wrapper object containing an HTMLVideoElement snapshot that can be used as a texture in GPU rendering operations. A GPUExternalTexture object instance is created using GPUDevice.importExternalTexture().
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUExternalTexture">MDN Web Docs: GPUExternalTexture</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUExternalTexture")]
public class GPUExternalTexture
{
    #region mixin GPUObjectBase
    /// <summary>
    /// JavaScript 属性 GPUObjectBase.label：USVString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </remarks>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUInternalError interface of the WebGPU API describes an application error indicating that an operation failed for a system or implementation-specific reason, even when all validation requirements were satisfied. It represents one of the types of errors surfaced by GPUDevice.popErrorScope and the uncapturederror event. Internal errors occur when something happens in the WebGPU implementation that wasn&apos;t caught by validation and wasn&apos;t clearly identifiable as an out-of-memory error. It generally means that an operation your code performed hit a system limit in a way that was difficult to express with WebGPU&apos;s supported limits. The same operation might succeed on a different device. These can only be raised by pipeline creation, usually if the shader is too complex for the device.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUInternalError">MDN Web Docs: GPUInternalError</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUInternalError")]
public class GPUInternalError : GPUError
{
    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUInternalError() constructor creates a new GPUInternalError object instance.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUInternalError/GPUInternalError">MDN Web Docs: GPUInternalError.GPUInternalError</see>
    /// </remarks>
    /// <param name="message">A string providing a human-readable message that explains why the error occurred. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUInternalError/GPUInternalError">MDN Web Docs: message</see></param>
    public extern GPUInternalError(string message);
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUOutOfMemoryError interface of the WebGPU API describes an out-of-memory (oom) error indicating that there was not enough free memory to complete the requested operation. It represents one of the types of errors surfaced by GPUDevice.popErrorScope and the uncapturederror event. Out-of-memory errors should be relatively rare in a well-behaved app but are less predictable than GPUValidationErrors. This is because they are dependent on the device your app is running on as well as other apps that are using GPU resources at the time.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUOutOfMemoryError">MDN Web Docs: GPUOutOfMemoryError</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUOutOfMemoryError")]
public class GPUOutOfMemoryError : GPUError
{
    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUOutOfMemoryError() constructor creates a new GPUOutOfMemoryError object instance.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUOutOfMemoryError/GPUOutOfMemoryError">MDN Web Docs: GPUOutOfMemoryError.GPUOutOfMemoryError</see>
    /// </remarks>
    /// <param name="message">A string providing a human-readable message that explains why the error occurred. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUOutOfMemoryError/GPUOutOfMemoryError">MDN Web Docs: message</see></param>
    public extern GPUOutOfMemoryError(string message);
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUPipelineError interface of the WebGPU API describes a pipeline failure. This is the value received when a Promise returned by a GPUDevice.createComputePipelineAsync() or GPUDevice.createRenderPipelineAsync() call rejects.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUPipelineError">MDN Web Docs: GPUPipelineError</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUPipelineError")]
public class GPUPipelineError(string message, string name) : DOMException(message, name)
{
    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUPipelineError() constructor creates a new GPUPipelineError object instance.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUPipelineError/GPUPipelineError">MDN Web Docs: GPUPipelineError.GPUPipelineError</see>
    /// </remarks>
    /// <param name="message">A string providing a human-readable message that explains why the error occurred. If not specified, message defaults to an empty string (&quot;&quot;). <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUPipelineError/GPUPipelineError">MDN Web Docs: message</see></param>
    /// <param name="options">An object, which can contain the following properties: reason An enumerated value that defines the reason the pipeline creation failed in a machine-readable way. The value can be one of: &quot;internal&quot;: Pipeline creation failed because of an internal error (see GPUInternalError for more information about these kinds of error). &quot;validation&quot;: Pipeline creation failed because of a validation error (see GPUValidationError for more information about these kinds of error). <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUPipelineError/GPUPipelineError">MDN Web Docs: options</see></param>
    public extern GPUPipelineError(string message = "", GPUPipelineErrorInit? options = default);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The reason read-only property of the GPUPipelineError interface defines the reason the pipeline creation failed in a machine-readable way.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUPipelineError/reason">MDN Web Docs: GPUPipelineError.reason</see>
    /// </remarks>
    [Description("@#reason")]
    public extern GPUPipelineErrorReason Reason { get; }
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUPipelineLayout interface of the WebGPU API defines the GPUBindGroupLayouts used by a pipeline. GPUBindGroups used with the pipeline during command encoding must have compatible GPUBindGroupLayouts. A GPUPipelineLayout object instance is created using the GPUDevice.createPipelineLayout() method.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUPipelineLayout">MDN Web Docs: GPUPipelineLayout</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUPipelineLayout")]
public class GPUPipelineLayout
{
    #region mixin GPUObjectBase
    /// <summary>
    /// JavaScript 属性 GPUObjectBase.label：USVString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </remarks>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUQuerySet interface of the WebGPU API is used to record the results of queries on passes, such as occlusion or timestamp queries. Occlusion queries are available on render passes to query whether any fragment samples pass all the per-fragment tests for a set of drawing commands (including scissor, sample mask, alpha to coverage, stencil, and depth tests). To run an occlusion query, an appropriate GPUQuerySet must be provided as the value of the occlusionQuerySet descriptor property when invoking GPUCommandEncoder.beginRenderPass() to run a render pass. Timestamp queries allow applications to write timestamps to a GPUQuerySet. To run a timestamp query, appropriate GPUQuerySets must be provided inside the value of the timestampWrites descriptor property when invoking GPUCommandEncoder.beginRenderPass() to run a render pass, or GPUCommandEncoder.beginComputePass() to run a compute pass. Note: The timestamp-query feature needs to be enabled to use timestamp queries. A GPUQuerySet object instance is created using the GPUDevice.createQuerySet() method.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUQuerySet">MDN Web Docs: GPUQuerySet</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUQuerySet")]
public class GPUQuerySet
{
    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The destroy() method of the GPUQuerySet interface destroys the GPUQuerySet.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUQuerySet/destroy">MDN Web Docs: GPUQuerySet.destroy</see>
    /// </remarks>
    [Description("@#destroy")]
    public extern void Destroy();

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The type read-only property of the GPUQuerySet interface is an enumerated value specifying the type of queries managed by the GPUQuerySet.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUQuerySet/type">MDN Web Docs: GPUQuerySet.type</see>
    /// </remarks>
    [Description("@#type")]
    public extern GPUQueryType Type { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The count read-only property of the GPUQuerySet interface is a number specifying the number of queries managed by the GPUQuerySet.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUQuerySet/count">MDN Web Docs: GPUQuerySet.count</see>
    /// </remarks>
    [Description("@#count")]
    public extern GPUSize32Out Count { get; }

    #region mixin GPUObjectBase
    /// <summary>
    /// JavaScript 属性 GPUObjectBase.label：USVString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </remarks>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUQueue interface of the WebGPU API controls execution of encoded commands on the GPU. A device&apos;s primary queue is accessed via the GPUDevice.queue property.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUQueue">MDN Web Docs: GPUQueue</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUQueue")]
public class GPUQueue
{
    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The submit() method of the GPUQueue interface schedules the execution of command buffers represented by one or more GPUCommandBuffer objects by the GPU.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUQueue/submit">MDN Web Docs: GPUQueue.submit</see>
    /// </remarks>
    /// <param name="commandBuffers">An array of GPUCommandBuffer objects containing the commands to be enqueued for processing by the GPU. The array must not contain duplicate GPUCommandBuffer objects — each one can only be submitted once per submit() call. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUQueue/submit">MDN Web Docs: commandBuffers</see></param>
    [Description("@#submit")]
    public extern void Submit(GPUCommandBuffer[] commandBuffers);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The onSubmittedWorkDone() method of the GPUQueue interface returns a Promise that resolves when all the work submitted to the GPU via this GPUQueue at the point the method is called has been processed. This includes the completion of any mapAsync() calls made on GPUBuffers used in commands submitted to the queue, before onSubmittedWorkDone() is called. Note: In most cases, you do not need to call onSubmittedWorkDone(). You do not need to call it for mapping a buffer. mapAsync guarantees work submitted to the queue before calling mapAsync happens before the mapAsync returns (see WebGPU spec). The two use cases for onSubmittedWorkDone Waiting for multiple buffer mapping (slow) js// good await Promise.all([ buffer1.mapAsync(), buffer2.mapAsync(), buffer3.mapAsync(), ]); data1 = buffer1.getMappedRange(); data2 = buffer2.getMappedRange(); data3 = buffer3.getMappedRange(); js// works but slow buffer1.mapAsync(); buffer2.mapAsync(); buffer3.mapAsync(); await device.queue.onSubmittedWorkDone(); data1 = buffer1.getMappedRange(); data2 = buffer2.getMappedRange(); data3 = buffer3.getMappedRange(); The reason the second method is slow is, the implementation may be able to map the buffers before all the submitted work is done. For example, if all the buffers are finished being used, but more work (unrelated to the buffers) is already submitted, then you&apos;ll end up waiting longer using the second method than the first. Throttling work If you are doing heavy compute work and you submit too much work at once, the browser may kill your work. You can throttle the work by only submitting more work when the work you&apos;ve already submitted is done.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUQueue/onSubmittedWorkDone">MDN Web Docs: GPUQueue.onSubmittedWorkDone</see>
    /// </remarks>
    /// <returns>A Promise that resolves with undefined.</returns>
    [Description("@#onSubmittedWorkDone")]
    public extern PromiseResult OnSubmittedWorkDone();

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The writeBuffer() method of the GPUQueue interface writes a provided data source into a given GPUBuffer. This is a convenience function, which provides an alternative to setting buffer data via buffer mapping and buffer-to-buffer copies. It lets the user agent determine the most efficient way to copy the data over.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUQueue/writeBuffer">MDN Web Docs: GPUQueue.writeBuffer</see>
    /// </remarks>
    /// <param name="buffer">A GPUBuffer object representing the buffer to write data to. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUQueue/writeBuffer">MDN Web Docs: buffer</see></param>
    /// <param name="bufferOffset">A number representing the offset, in bytes, to start writing the data at inside the GPUBuffer. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUQueue/writeBuffer">MDN Web Docs: bufferOffset</see></param>
    /// <param name="data">An object representing the data source to write into the GPUBuffer. This can be an ArrayBuffer, TypedArray, or DataView. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUQueue/writeBuffer">MDN Web Docs: data</see></param>
    /// <param name="dataOffset">A number representing the offset to start writing the data from inside the data source. This value is a number of elements if data is a TypedArray, and a number of bytes if not. If omitted, dataOffset defaults to 0. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUQueue/writeBuffer">MDN Web Docs: dataOffset</see></param>
    /// <param name="size">A number representing the size of the content to write from data to buffer. This value is a number of elements if data is a TypedArray, and a number of bytes if not. If omitted, size will be equal to the overall size of data, minus dataOffset. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUQueue/writeBuffer">MDN Web Docs: size</see></param>
    [Description("@#writeBuffer")]
    public extern void WriteBuffer(GPUBuffer buffer, GPUSize64 bufferOffset, IAllowSharedBufferSource data, GPUSize64? dataOffset = default, GPUSize64? size = default);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The writeTexture() method of the GPUQueue interface writes a provided data source into a given GPUTexture. This is a convenience function, which provides an alternative to setting texture data via buffer mapping and buffer-to-texture copies. It lets the user agent determine the most efficient way to copy the data over.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUQueue/writeTexture">MDN Web Docs: GPUQueue.writeTexture</see>
    /// </remarks>
    /// <param name="destination">An object defining the texture subresource and origin to write the data source to, which can take the following properties: aspect Optional An enumerated value defining which aspects of the texture to write the data to. Possible values are: &quot;all&quot; All available aspects of the texture format will be written to, which can mean all or any of color, depth, and stencil, depending on what kind of format you are dealing with. &quot;depth-only&quot; Only the depth aspect of a depth-or-stencil format will be written to. &quot;stencil-only&quot; Only the stencil aspect of a depth-or-stencil format will be written to. If omitted, aspect takes a value of &quot;all&quot;. mipLevel Optional A number representing the mip-map level of the texture to write the data to. If omitted, mipLevel defaults to 0. origin Optional An object or array specifying the origin of the copy — the minimum corner of the texture region to write the data to. Together with size, this defines the full extent of the region to copy to. The x, y, and z values default to 0 if any of all of origin is omitted. For example, you can pass an array like [0, 0, 0], or its equivalent object { x: 0, y: 0, z: 0 }. texture A GPUTexture object representing the texture to write the data to. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUQueue/writeTexture">MDN Web Docs: destination</see></param>
    /// <param name="data">An object representing the data source to write into the GPUTexture. This can be an ArrayBuffer, TypedArray, or DataView. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUQueue/writeTexture">MDN Web Docs: data</see></param>
    /// <param name="dataLayout">An object that defines the layout of the content contained in data. Possible values are: offset Optional The offset, in bytes, from the beginning of data to the start of the image data to be copied. If omitted, offset defaults to 0. bytesPerRow Optional A number representing the stride, in bytes, between the start of each block row (i.e., a row of complete texel blocks) and the subsequent block row. This is required if there are multiple block rows (i.e., the copy height or depth is more than one block). rowsPerImage Optional The number of block rows per single image of the texture. bytesPerRow × rowsPerImage will give you the stride, in bytes, between the start of each complete image. This is required if there are multiple images to copy. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUQueue/writeTexture">MDN Web Docs: dataLayout</see></param>
    /// <param name="size">An object or array specifying the extent of the copy — the far corner of the texture region to write the data to. Together with destination.origin, this defines the full extent of the region to copy to. See destination.origin for examples of the object/array structure. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUQueue/writeTexture">MDN Web Docs: size</see></param>
    [Description("@#writeTexture")]
    public extern void WriteTexture(GPUTexelCopyTextureInfo destination, IAllowSharedBufferSource data, GPUTexelCopyBufferLayout dataLayout, GPUExtent3D size);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The copyExternalImageToTexture() method of the GPUQueue interface copies a snapshot taken from a source image, video, or canvas into a given GPUTexture. Using this function allows the user agent to determine the most efficient way to copy the data over for each source type.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUQueue/copyExternalImageToTexture">MDN Web Docs: GPUQueue.copyExternalImageToTexture</see>
    /// </remarks>
    /// <param name="source">An object providing the source of the snapshot to copy. This can be an HTMLCanvasElement, HTMLImageElement, HTMLVideoElement, ImageBitmap, ImageData, OffscreenCanvas, or VideoFrame object. The image source data is captured at the exact moment copyExternalImageToTexture() is invoked. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUQueue/copyExternalImageToTexture">MDN Web Docs: source</see></param>
    /// <param name="destination">An object defining the texture subresource and origin to write the captured image to, plus encoding metadata. This can take the following properties: aspect Optional An enumerated value defining which aspects of the texture to write the image to. Possible values are: &quot;all&quot; All available aspects of the texture format will be written to, which can mean all or any of color, depth, and stencil, depending on what kind of format you are dealing with. &quot;depth-only&quot; Only the depth aspect of a depth-or-stencil format will be written to. &quot;stencil-only&quot; Only the stencil aspect of a depth-or-stencil format will be written to. If omitted, aspect takes a value of &quot;all&quot;. colorSpace Optional An enumerated value describing the color space and encoding used to encode data into the destination texture. Possible values are &quot;srgb&quot; and &quot;display-p3&quot;. If omitted, colorSpace defaults to &quot;srgb&quot;. Note: The encoding may result in values outside of the range [0, 1] being written to the target texture, if its format can represent them. Otherwise, the results are clamped to the target texture format&apos;s range. Conversion may not be necessary if colorSpace matches the source image color space. mipLevel Optional A number representing the mip-map level of the texture to write the image to. If omitted, mipLevel defaults to 0. origin Optional An object or array specifying the origin of the copy — the minimum corner of the texture region to write the image data to. Together with copySize, this defines the full extent of the region to copy to. The x, y, and z values default to 0 if any of all of origin is omitted. For example, you can pass an array like [0, 0, 0], or its equivalent object { x: 0, y: 0, z: 0 }. premultipliedAlpha Optional A boolean. If set to true, the image data written into the texture will have its RGB channels premultiplied by the alpha channel. If omitted, premultipliedAlpha defaults to false. Note: If this option is set to true and the source is also premultiplied, the source RGB values must be preserved even if they exceed their corresponding alpha values. texture A GPUTexture object representing the texture to write the data to. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUQueue/copyExternalImageToTexture">MDN Web Docs: destination</see></param>
    /// <param name="copySize">An object or array specifying width, height, and depthOrArrayLayers — of the region to copy from/to. For example, you can pass an array like [16, 1, 1], or its equivalent object { width: 16, height: 1, depthOrArrayLayers: 1 }. The width value has to be included. If the height or depthOrArrayLayers values are omitted, they default to 1. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUQueue/copyExternalImageToTexture">MDN Web Docs: copySize</see></param>
    [Description("@#copyExternalImageToTexture")]
    public extern void CopyExternalImageToTexture(GPUCopyExternalImageSourceInfo source, GPUCopyExternalImageDestInfo destination, GPUExtent3D copySize);

    #region mixin GPUObjectBase
    /// <summary>
    /// JavaScript 属性 GPUObjectBase.label：USVString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </remarks>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPURenderBundle interface of the WebGPU API represents a container for pre-recorded bundles of commands. The command bundles are encoded using a GPURenderBundleEncoder; once the desired commands have been encoded, they are recorded into a GPURenderBundle object instance using the GPURenderBundleEncoder.finish() method. These command bundles can then be reused across multiple render passes by passing the GPURenderBundle objects into GPURenderPassEncoder.executeBundles() calls. Reusing pre-recoded commands can significantly improve app performance in situations where JavaScript draw call overhead is a bottleneck. Render bundles are most effective in situations where a batch of objects will be drawn the same way across multiple views or frames, with the only differences being the buffer content being used (such as updated matrix uniforms). A good example is VR rendering. Recording the rendering as a render bundle and then tweaking the view matrix and replaying it for each eye is a more efficient way to issue draw calls for both renderings of the scene.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderBundle">MDN Web Docs: GPURenderBundle</see>
/// </remarks>
[ECMAScript]
[Description("@#GPURenderBundle")]
public class GPURenderBundle
{
    #region mixin GPUObjectBase
    /// <summary>
    /// JavaScript 属性 GPUObjectBase.label：USVString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </remarks>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPURenderBundleEncoder interface of the WebGPU API is used to pre-record bundles of commands. The command bundles are encoded by calling the methods of GPURenderBundleEncoder; once the desired commands have been encoded, they are recorded into a GPURenderBundle object instance using the GPURenderBundleEncoder.finish() method. These render bundles can then be reused across multiple render passes by passing the GPURenderBundle objects into GPURenderPassEncoder.executeBundles() calls. In effect, this is like a partial render pass — GPURenderBundleEncoders have all the same functionality available as GPURenderPassEncoders, except that they can&apos;t begin and end occlusion queries, and can&apos;t set the scissor rect, viewport, blend constant, and stencil reference. The GPURenderBundle will inherit all these values from the GPURenderPassEncoder that executes it. Note: Currently set vertex buffers, index buffers, bind groups, and pipeline are all cleared prior to executing a render bundle, and once the render bundle has finished executing. Reusing pre-recoded commands can significantly improve app performance in situations where JavaScript draw call overhead is a bottleneck. Render bundles are most effective in situations where a batch of objects will be drawn the same way across multiple views or frames, with the only differences being the buffer content being used (such as updated matrix uniforms). A good example is VR rendering. Recording the rendering as a render bundle and then tweaking the view matrix and replaying it for each eye is a more efficient way to issue draw calls for both renderings of the scene. A GPURenderBundleEncoder object instance is created via the GPUDevice.createRenderBundleEncoder() property. Note: The methods of GPURenderBundleEncoder are functionally identical to their equivalents available on GPURenderPassEncoder, except for GPURenderBundleEncoder.finish(), which is similar in purpose to GPUCommandEncoder.finish().
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderBundleEncoder">MDN Web Docs: GPURenderBundleEncoder</see>
/// </remarks>
[ECMAScript]
[Description("@#GPURenderBundleEncoder")]
public class GPURenderBundleEncoder
{
    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The finish() method of the GPURenderBundleEncoder interface completes recording of the current render bundle command sequence, returning a GPURenderBundle object that can be passed into a GPURenderPassEncoder.executeBundles() call to execute those commands in a specific render pass.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderBundleEncoder/finish">MDN Web Docs: GPURenderBundleEncoder.finish</see>
    /// </remarks>
    /// <param name="descriptor">An object containing the following properties: label Optional A string providing a label that can be used to identify the object, for example in GPUError messages or console warnings. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderBundleEncoder/finish">MDN Web Docs: descriptor</see></param>
    /// <returns>A GPURenderBundle object instance.</returns>
    [Description("@#finish")]
    public extern GPURenderBundle Finish(GPURenderBundleDescriptor? descriptor = default);

    #region mixin GPUObjectBase
    /// <summary>
    /// JavaScript 属性 GPUObjectBase.label：USVString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </remarks>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion

    #region mixin GPUDebugCommandsMixin
    /// <summary>
    /// JavaScript GPUDebugCommandsMixin.pushDebugGroup(groupLabel) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-pushdebuggroup">WebGPU: 15. Debug Markers</see>
    /// </remarks>
    /// <param name="groupLabel"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-pushdebuggroup-grouplabel-grouplabel">WebGPU: 15. Debug Markers</see></param>
    [Description("@#pushDebugGroup")]
    public extern void PushDebugGroup(string groupLabel);

    /// <summary>
    /// JavaScript GPUDebugCommandsMixin.popDebugGroup() 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-popdebuggroup">WebGPU: 15. Debug Markers</see>
    /// </remarks>
    [Description("@#popDebugGroup")]
    public extern void PopDebugGroup();

    /// <summary>
    /// JavaScript GPUDebugCommandsMixin.insertDebugMarker(markerLabel) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-insertdebugmarker">WebGPU: 15. Debug Markers</see>
    /// </remarks>
    /// <param name="markerLabel"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-insertdebugmarker-markerlabel-markerlabel">WebGPU: 15. Debug Markers</see></param>
    [Description("@#insertDebugMarker")]
    public extern void InsertDebugMarker(string markerLabel);
    #endregion

    #region mixin GPUBindingCommandsMixin
    /// <summary>
    /// JavaScript GPUBindingCommandsMixin.setBindGroup(index, bindGroup, dynamicOffsets) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup">WebGPU: 14.1 Bind Groups</see>
    /// </remarks>
    /// <param name="index"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsets-index">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="bindGroup"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsets-bindgroup">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="dynamicOffsets"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsets-dynamicoffsets">WebGPU: 14.1 Bind Groups</see></param>
    [Description("@#setBindGroup")]
    public extern void SetBindGroup(GPUIndex32 index, GPUBindGroup? bindGroup, GPUBufferDynamicOffset[]? dynamicOffsets = default);

    /// <summary>
    /// JavaScript GPUBindingCommandsMixin.setBindGroup(index, bindGroup, dynamicOffsetsData, dynamicOffsetsDataStart, dynamicOffsetsDataLength) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength">WebGPU: 14.1 Bind Groups</see>
    /// </remarks>
    /// <param name="index"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-index">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="bindGroup"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-bindgroup">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="dynamicOffsetsData"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-dynamicoffsetsdata">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="dynamicOffsetsDataStart"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-dynamicoffsetsdatastart">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="dynamicOffsetsDataLength"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-dynamicoffsetsdatalength">WebGPU: 14.1 Bind Groups</see></param>
    [Description("@#setBindGroup")]
    public extern void SetBindGroup(GPUIndex32 index, GPUBindGroup? bindGroup, Uint32Array dynamicOffsetsData, GPUSize64 dynamicOffsetsDataStart, GPUSize32 dynamicOffsetsDataLength);

    /// <summary>
    /// JavaScript GPUBindingCommandsMixin.setImmediates(rangeOffset, data, dataOffset, dataSize) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates">WebGPU: 14.2 Immediate Data</see>
    /// </remarks>
    /// <param name="rangeOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates-rangeoffset-data-dataoffset-datasize-rangeoffset">WebGPU: 14.2 Immediate Data</see></param>
    /// <param name="data"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates-rangeoffset-data-dataoffset-datasize-data">WebGPU: 14.2 Immediate Data</see></param>
    /// <param name="dataOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates-rangeoffset-data-dataoffset-datasize-dataoffset">WebGPU: 14.2 Immediate Data</see></param>
    /// <param name="dataSize"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates-rangeoffset-data-dataoffset-datasize-datasize">WebGPU: 14.2 Immediate Data</see></param>
    [Description("@#setImmediates")]
    public extern void SetImmediates(GPUSize32 rangeOffset, IAllowSharedBufferSource data, GPUSize64? dataOffset = default, GPUSize64? dataSize = default);
    #endregion

    #region mixin GPURenderCommandsMixin
    /// <summary>
    /// JavaScript GPURenderCommandsMixin.setPipeline(pipeline) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setpipeline">WebGPU: 17.2.1 Drawing</see>
    /// </remarks>
    /// <param name="pipeline"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setpipeline-pipeline-pipeline">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#setPipeline")]
    public extern void SetPipeline(GPURenderPipeline pipeline);

    /// <summary>
    /// JavaScript GPURenderCommandsMixin.setIndexBuffer(buffer, indexFormat, offset, size) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setindexbuffer">WebGPU: 17.2.1 Drawing</see>
    /// </remarks>
    /// <param name="buffer"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setindexbuffer-buffer-indexformat-offset-size-buffer">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="indexFormat"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setindexbuffer-buffer-indexformat-offset-size-indexformat">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="offset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setindexbuffer-buffer-indexformat-offset-size-offset">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="size"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setindexbuffer-buffer-indexformat-offset-size-size">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#setIndexBuffer")]
    public extern void SetIndexBuffer(GPUBuffer buffer, GPUIndexFormat indexFormat, GPUSize64? offset = default, GPUSize64? size = default);

    /// <summary>
    /// JavaScript GPURenderCommandsMixin.setVertexBuffer(slot, buffer, offset, size) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setvertexbuffer">WebGPU: 17.2.1 Drawing</see>
    /// </remarks>
    /// <param name="slot"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setvertexbuffer-slot-buffer-offset-size-slot">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="buffer"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setvertexbuffer-slot-buffer-offset-size-buffer">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="offset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setvertexbuffer-slot-buffer-offset-size-offset">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="size"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setvertexbuffer-slot-buffer-offset-size-size">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#setVertexBuffer")]
    public extern void SetVertexBuffer(GPUIndex32 slot, GPUBuffer? buffer, GPUSize64? offset = default, GPUSize64? size = default);

    /// <summary>
    /// JavaScript GPURenderCommandsMixin.draw(vertexCount, instanceCount, firstVertex, firstInstance) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-draw">WebGPU: 17.2.1 Drawing</see>
    /// </remarks>
    /// <param name="vertexCount"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-draw-vertexcount-instancecount-firstvertex-firstinstance-vertexcount">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="instanceCount"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-draw-vertexcount-instancecount-firstvertex-firstinstance-instancecount">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="firstVertex"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-draw-vertexcount-instancecount-firstvertex-firstinstance-firstvertex">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="firstInstance"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-draw-vertexcount-instancecount-firstvertex-firstinstance-firstinstance">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#draw")]
    public extern void Draw(GPUSize32 vertexCount, GPUSize32? instanceCount = default, GPUSize32? firstVertex = default, GPUSize32? firstInstance = default);

    /// <summary>
    /// JavaScript GPURenderCommandsMixin.drawIndexed(indexCount, instanceCount, firstIndex, baseVertex, firstInstance) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexed">WebGPU: 17.2.1 Drawing</see>
    /// </remarks>
    /// <param name="indexCount"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexed-indexcount-instancecount-firstindex-basevertex-firstinstance-indexcount">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="instanceCount"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexed-indexcount-instancecount-firstindex-basevertex-firstinstance-instancecount">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="firstIndex"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexed-indexcount-instancecount-firstindex-basevertex-firstinstance-firstindex">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="baseVertex"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexed-indexcount-instancecount-firstindex-basevertex-firstinstance-basevertex">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="firstInstance"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexed-indexcount-instancecount-firstindex-basevertex-firstinstance-firstinstance">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#drawIndexed")]
    public extern void DrawIndexed(GPUSize32 indexCount, GPUSize32? instanceCount = default, GPUSize32? firstIndex = default, GPUSignedOffset32? baseVertex = default, GPUSize32? firstInstance = default);

    /// <summary>
    /// JavaScript GPURenderCommandsMixin.drawIndirect(indirectBuffer, indirectOffset) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindirect">WebGPU: 17.2.1 Drawing</see>
    /// </remarks>
    /// <param name="indirectBuffer"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindirect-indirectbuffer-indirectoffset-indirectbuffer">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="indirectOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindirect-indirectbuffer-indirectoffset-indirectoffset">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#drawIndirect")]
    public extern void DrawIndirect(GPUBuffer indirectBuffer, GPUSize64 indirectOffset);

    /// <summary>
    /// JavaScript GPURenderCommandsMixin.drawIndexedIndirect(indirectBuffer, indirectOffset) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexedindirect">WebGPU: 17.2.1 Drawing</see>
    /// </remarks>
    /// <param name="indirectBuffer"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexedindirect-indirectbuffer-indirectoffset-indirectbuffer">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="indirectOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexedindirect-indirectbuffer-indirectoffset-indirectoffset">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#drawIndexedIndirect")]
    public extern void DrawIndexedIndirect(GPUBuffer indirectBuffer, GPUSize64 indirectOffset);
    #endregion
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPURenderPassEncoder interface of the WebGPU API encodes commands related to controlling the vertex and fragment shader stages, as issued by a GPURenderPipeline. It forms part of the overall encoding activity of a GPUCommandEncoder. A render pipeline renders graphics to GPUTexture attachments, typically intended for display in a &lt;canvas&gt; element, but it could also render to textures used for other purposes that never appear onscreen. It has two main stages: A vertex stage, in which a vertex shader takes positioning data fed into the GPU and uses it to position a series of vertices in 3D space by applying specified effects like rotation, translation, or perspective. The vertices are then assembled into primitives such as triangles (the basic building block of rendered graphics) and rasterized by the GPU to figure out what pixels each one should cover on the drawing canvas. A fragment stage, in which a fragment shader computes the color for each pixel covered by the primitives produced by the vertex shader. These computations frequently use inputs such as images (in the form of textures) that provide surface details and the position and color of virtual lights. A GPURenderPassEncoder object instance is created via the GPUCommandEncoder.beginRenderPass() property.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderPassEncoder">MDN Web Docs: GPURenderPassEncoder</see>
/// </remarks>
[ECMAScript]
[Description("@#GPURenderPassEncoder")]
public class GPURenderPassEncoder
{
    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The setViewport() method of the GPURenderPassEncoder interface sets the viewport used during the rasterization stage to linearly map from normalized device coordinates to viewport coordinates.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderPassEncoder/setViewport">MDN Web Docs: GPURenderPassEncoder.setViewport</see>
    /// </remarks>
    /// <param name="x">A number representing the minimum X value of the viewport, in pixels. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderPassEncoder/setViewport">MDN Web Docs: x</see></param>
    /// <param name="y">A number representing the minimum Y value of the viewport, in pixels. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderPassEncoder/setViewport">MDN Web Docs: y</see></param>
    /// <param name="width">A number representing the width of the viewport, in pixels. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderPassEncoder/setViewport">MDN Web Docs: width</see></param>
    /// <param name="height">A number representing the height of the viewport, in pixels. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderPassEncoder/setViewport">MDN Web Docs: height</see></param>
    /// <param name="minDepth">A number representing the minimum depth value of the viewport. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderPassEncoder/setViewport">MDN Web Docs: minDepth</see></param>
    /// <param name="maxDepth">A number representing the maximum depth value of the viewport. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderPassEncoder/setViewport">MDN Web Docs: maxDepth</see></param>
    [Description("@#setViewport")]
    public extern void SetViewport(float x, float y, float width, float height, float minDepth, float maxDepth);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The setScissorRect() method of the GPURenderPassEncoder interface sets the scissor rectangle used during the rasterization stage. After transformation into viewport coordinates any fragments that fall outside the scissor rectangle will be discarded.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderPassEncoder/setScissorRect">MDN Web Docs: GPURenderPassEncoder.setScissorRect</see>
    /// </remarks>
    /// <param name="x">A number representing the minimum X value of the scissor rectangle, in pixels. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderPassEncoder/setScissorRect">MDN Web Docs: x</see></param>
    /// <param name="y">A number representing the minimum Y value of the scissor rectangle, in pixels. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderPassEncoder/setScissorRect">MDN Web Docs: y</see></param>
    /// <param name="width">A number representing the width of the scissor rectangle, in pixels. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderPassEncoder/setScissorRect">MDN Web Docs: width</see></param>
    /// <param name="height">A number representing the height of the scissor rectangle, in pixels. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderPassEncoder/setScissorRect">MDN Web Docs: height</see></param>
    [Description("@#setScissorRect")]
    public extern void SetScissorRect(GPUIntegerCoordinate x, GPUIntegerCoordinate y, GPUIntegerCoordinate width, GPUIntegerCoordinate height);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The setBlendConstant() method of the GPURenderPassEncoder interface sets the constant blend color and alpha values used with &quot;constant&quot; and &quot;one-minus-constant&quot; blend factors (as set in the descriptor of the GPUDevice.createRenderPipeline() method, in the blend property).
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderPassEncoder/setBlendConstant">MDN Web Docs: GPURenderPassEncoder.setBlendConstant</see>
    /// </remarks>
    /// <param name="color">An object or array representing the color to use when blending — the r, g, b, and a components are represented as floating point numbers between 0.0 and 1.0. What follows is an object example: jsconst color = { r: 0.0, g: 0.5, b: 1.0, a: 1.0 }; The array equivalent would look like this: jsconst color = [0.0, 0.5, 1.0, 1.0]; <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderPassEncoder/setBlendConstant">MDN Web Docs: color</see></param>
    [Description("@#setBlendConstant")]
    public extern void SetBlendConstant(GPUColor color);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The setStencilReference() method of the GPURenderPassEncoder interface sets the stencil reference value using during stencil tests with the &quot;replace&quot; stencil operation (as set in the descriptor of the GPUDevice.createRenderPipeline() method, in the properties defining the various stencil operations).
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderPassEncoder/setStencilReference">MDN Web Docs: GPURenderPassEncoder.setStencilReference</see>
    /// </remarks>
    /// <param name="reference">A number representing the new stencil reference value to set for the render pass. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderPassEncoder/setStencilReference">MDN Web Docs: reference</see></param>
    [Description("@#setStencilReference")]
    public extern void SetStencilReference(GPUStencilValue reference);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The beginOcclusionQuery() method of the GPURenderPassEncoder interface begins an occlusion query at the specified index of the relevant GPUQuerySet (provided as the value of the occlusionQuerySet descriptor property when invoking GPUCommandEncoder.beginRenderPass() to run the render pass).
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderPassEncoder/beginOcclusionQuery">MDN Web Docs: GPURenderPassEncoder.beginOcclusionQuery</see>
    /// </remarks>
    /// <param name="queryIndex">The index in the GPUQuerySet to begin the occlusion query at. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderPassEncoder/beginOcclusionQuery">MDN Web Docs: queryIndex</see></param>
    [Description("@#beginOcclusionQuery")]
    public extern void BeginOcclusionQuery(GPUSize32 queryIndex);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The endOcclusionQuery() method of the GPURenderPassEncoder interface ends an active occlusion query previously started with beginOcclusionQuery().
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderPassEncoder/endOcclusionQuery">MDN Web Docs: GPURenderPassEncoder.endOcclusionQuery</see>
    /// </remarks>
    [Description("@#endOcclusionQuery")]
    public extern void EndOcclusionQuery();

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The executeBundles() method of the GPURenderPassEncoder interface executes commands previously recorded into the referenced GPURenderBundles, as part of this render pass. Note: After calling executeBundles() the currently set vertex buffers, index buffers, bind groups, and pipeline are all cleared, even if no bundles are actually executed.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderPassEncoder/executeBundles">MDN Web Docs: GPURenderPassEncoder.executeBundles</see>
    /// </remarks>
    /// <param name="bundles">An array of GPURenderBundle objects, containing the pre-recorded commands to execute. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderPassEncoder/executeBundles">MDN Web Docs: bundles</see></param>
    [Description("@#executeBundles")]
    public extern void ExecuteBundles(GPURenderBundle[] bundles);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The end() method of the GPURenderPassEncoder interface completes recording of the current render pass command sequence.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderPassEncoder/end">MDN Web Docs: GPURenderPassEncoder.end</see>
    /// </remarks>
    [Description("@#end")]
    public extern void End();

    #region mixin GPUObjectBase
    /// <summary>
    /// JavaScript 属性 GPUObjectBase.label：USVString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </remarks>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion

    #region mixin GPUDebugCommandsMixin
    /// <summary>
    /// JavaScript GPUDebugCommandsMixin.pushDebugGroup(groupLabel) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-pushdebuggroup">WebGPU: 15. Debug Markers</see>
    /// </remarks>
    /// <param name="groupLabel"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-pushdebuggroup-grouplabel-grouplabel">WebGPU: 15. Debug Markers</see></param>
    [Description("@#pushDebugGroup")]
    public extern void PushDebugGroup(string groupLabel);

    /// <summary>
    /// JavaScript GPUDebugCommandsMixin.popDebugGroup() 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-popdebuggroup">WebGPU: 15. Debug Markers</see>
    /// </remarks>
    [Description("@#popDebugGroup")]
    public extern void PopDebugGroup();

    /// <summary>
    /// JavaScript GPUDebugCommandsMixin.insertDebugMarker(markerLabel) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-insertdebugmarker">WebGPU: 15. Debug Markers</see>
    /// </remarks>
    /// <param name="markerLabel"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-insertdebugmarker-markerlabel-markerlabel">WebGPU: 15. Debug Markers</see></param>
    [Description("@#insertDebugMarker")]
    public extern void InsertDebugMarker(string markerLabel);
    #endregion

    #region mixin GPUBindingCommandsMixin
    /// <summary>
    /// JavaScript GPUBindingCommandsMixin.setBindGroup(index, bindGroup, dynamicOffsets) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup">WebGPU: 14.1 Bind Groups</see>
    /// </remarks>
    /// <param name="index"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsets-index">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="bindGroup"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsets-bindgroup">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="dynamicOffsets"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsets-dynamicoffsets">WebGPU: 14.1 Bind Groups</see></param>
    [Description("@#setBindGroup")]
    public extern void SetBindGroup(GPUIndex32 index, GPUBindGroup? bindGroup, GPUBufferDynamicOffset[]? dynamicOffsets = default);

    /// <summary>
    /// JavaScript GPUBindingCommandsMixin.setBindGroup(index, bindGroup, dynamicOffsetsData, dynamicOffsetsDataStart, dynamicOffsetsDataLength) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength">WebGPU: 14.1 Bind Groups</see>
    /// </remarks>
    /// <param name="index"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-index">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="bindGroup"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-bindgroup">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="dynamicOffsetsData"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-dynamicoffsetsdata">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="dynamicOffsetsDataStart"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-dynamicoffsetsdatastart">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="dynamicOffsetsDataLength"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-dynamicoffsetsdatalength">WebGPU: 14.1 Bind Groups</see></param>
    [Description("@#setBindGroup")]
    public extern void SetBindGroup(GPUIndex32 index, GPUBindGroup? bindGroup, Uint32Array dynamicOffsetsData, GPUSize64 dynamicOffsetsDataStart, GPUSize32 dynamicOffsetsDataLength);

    /// <summary>
    /// JavaScript GPUBindingCommandsMixin.setImmediates(rangeOffset, data, dataOffset, dataSize) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates">WebGPU: 14.2 Immediate Data</see>
    /// </remarks>
    /// <param name="rangeOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates-rangeoffset-data-dataoffset-datasize-rangeoffset">WebGPU: 14.2 Immediate Data</see></param>
    /// <param name="data"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates-rangeoffset-data-dataoffset-datasize-data">WebGPU: 14.2 Immediate Data</see></param>
    /// <param name="dataOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates-rangeoffset-data-dataoffset-datasize-dataoffset">WebGPU: 14.2 Immediate Data</see></param>
    /// <param name="dataSize"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates-rangeoffset-data-dataoffset-datasize-datasize">WebGPU: 14.2 Immediate Data</see></param>
    [Description("@#setImmediates")]
    public extern void SetImmediates(GPUSize32 rangeOffset, IAllowSharedBufferSource data, GPUSize64? dataOffset = default, GPUSize64? dataSize = default);
    #endregion

    #region mixin GPURenderCommandsMixin
    /// <summary>
    /// JavaScript GPURenderCommandsMixin.setPipeline(pipeline) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setpipeline">WebGPU: 17.2.1 Drawing</see>
    /// </remarks>
    /// <param name="pipeline"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setpipeline-pipeline-pipeline">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#setPipeline")]
    public extern void SetPipeline(GPURenderPipeline pipeline);

    /// <summary>
    /// JavaScript GPURenderCommandsMixin.setIndexBuffer(buffer, indexFormat, offset, size) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setindexbuffer">WebGPU: 17.2.1 Drawing</see>
    /// </remarks>
    /// <param name="buffer"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setindexbuffer-buffer-indexformat-offset-size-buffer">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="indexFormat"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setindexbuffer-buffer-indexformat-offset-size-indexformat">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="offset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setindexbuffer-buffer-indexformat-offset-size-offset">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="size"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setindexbuffer-buffer-indexformat-offset-size-size">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#setIndexBuffer")]
    public extern void SetIndexBuffer(GPUBuffer buffer, GPUIndexFormat indexFormat, GPUSize64? offset = default, GPUSize64? size = default);

    /// <summary>
    /// JavaScript GPURenderCommandsMixin.setVertexBuffer(slot, buffer, offset, size) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setvertexbuffer">WebGPU: 17.2.1 Drawing</see>
    /// </remarks>
    /// <param name="slot"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setvertexbuffer-slot-buffer-offset-size-slot">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="buffer"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setvertexbuffer-slot-buffer-offset-size-buffer">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="offset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setvertexbuffer-slot-buffer-offset-size-offset">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="size"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setvertexbuffer-slot-buffer-offset-size-size">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#setVertexBuffer")]
    public extern void SetVertexBuffer(GPUIndex32 slot, GPUBuffer? buffer, GPUSize64? offset = default, GPUSize64? size = default);

    /// <summary>
    /// JavaScript GPURenderCommandsMixin.draw(vertexCount, instanceCount, firstVertex, firstInstance) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-draw">WebGPU: 17.2.1 Drawing</see>
    /// </remarks>
    /// <param name="vertexCount"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-draw-vertexcount-instancecount-firstvertex-firstinstance-vertexcount">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="instanceCount"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-draw-vertexcount-instancecount-firstvertex-firstinstance-instancecount">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="firstVertex"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-draw-vertexcount-instancecount-firstvertex-firstinstance-firstvertex">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="firstInstance"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-draw-vertexcount-instancecount-firstvertex-firstinstance-firstinstance">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#draw")]
    public extern void Draw(GPUSize32 vertexCount, GPUSize32? instanceCount = default, GPUSize32? firstVertex = default, GPUSize32? firstInstance = default);

    /// <summary>
    /// JavaScript GPURenderCommandsMixin.drawIndexed(indexCount, instanceCount, firstIndex, baseVertex, firstInstance) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexed">WebGPU: 17.2.1 Drawing</see>
    /// </remarks>
    /// <param name="indexCount"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexed-indexcount-instancecount-firstindex-basevertex-firstinstance-indexcount">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="instanceCount"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexed-indexcount-instancecount-firstindex-basevertex-firstinstance-instancecount">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="firstIndex"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexed-indexcount-instancecount-firstindex-basevertex-firstinstance-firstindex">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="baseVertex"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexed-indexcount-instancecount-firstindex-basevertex-firstinstance-basevertex">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="firstInstance"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexed-indexcount-instancecount-firstindex-basevertex-firstinstance-firstinstance">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#drawIndexed")]
    public extern void DrawIndexed(GPUSize32 indexCount, GPUSize32? instanceCount = default, GPUSize32? firstIndex = default, GPUSignedOffset32? baseVertex = default, GPUSize32? firstInstance = default);

    /// <summary>
    /// JavaScript GPURenderCommandsMixin.drawIndirect(indirectBuffer, indirectOffset) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindirect">WebGPU: 17.2.1 Drawing</see>
    /// </remarks>
    /// <param name="indirectBuffer"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindirect-indirectbuffer-indirectoffset-indirectbuffer">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="indirectOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindirect-indirectbuffer-indirectoffset-indirectoffset">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#drawIndirect")]
    public extern void DrawIndirect(GPUBuffer indirectBuffer, GPUSize64 indirectOffset);

    /// <summary>
    /// JavaScript GPURenderCommandsMixin.drawIndexedIndirect(indirectBuffer, indirectOffset) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexedindirect">WebGPU: 17.2.1 Drawing</see>
    /// </remarks>
    /// <param name="indirectBuffer"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexedindirect-indirectbuffer-indirectoffset-indirectbuffer">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="indirectOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexedindirect-indirectbuffer-indirectoffset-indirectoffset">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#drawIndexedIndirect")]
    public extern void DrawIndexedIndirect(GPUBuffer indirectBuffer, GPUSize64 indirectOffset);
    #endregion
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPURenderPipeline interface of the WebGPU API represents a pipeline that controls the vertex and fragment shader stages and can be used in a GPURenderPassEncoder or GPURenderBundleEncoder. A GPURenderPipeline object instance can be created using the GPUDevice.createRenderPipeline() or GPUDevice.createRenderPipelineAsync() methods.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPURenderPipeline">MDN Web Docs: GPURenderPipeline</see>
/// </remarks>
[ECMAScript]
[Description("@#GPURenderPipeline")]
public class GPURenderPipeline
{
    #region mixin GPUObjectBase
    /// <summary>
    /// JavaScript 属性 GPUObjectBase.label：USVString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </remarks>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion

    #region mixin GPUPipelineBase
    /// <summary>
    /// JavaScript GPUPipelineBase.getBindGroupLayout(index) 的强类型绑定，WebIDL 返回类型为 GPUBindGroupLayout。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpupipelinebase-getbindgrouplayout">WebGPU: 10.1 Base pipelines</see>
    /// </remarks>
    /// <param name="index"><see href="https://gpuweb.github.io/gpuweb/#dom-gpupipelinebase-getbindgrouplayout-index-index">WebGPU: 10.1 Base pipelines</see></param>
    [Description("@#getBindGroupLayout")]
    public extern GPUBindGroupLayout GetBindGroupLayout(uint index);
    #endregion
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUSampler interface of the WebGPU API represents an object that can control how shaders transform and filter texture resource data. A GPUSampler object instance is created using the GPUDevice.createSampler() method.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUSampler">MDN Web Docs: GPUSampler</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUSampler")]
public class GPUSampler
{
    #region mixin GPUObjectBase
    /// <summary>
    /// JavaScript 属性 GPUObjectBase.label：USVString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </remarks>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUShaderModule interface of the WebGPU API represents an internal shader module object, a container for WGSL shader code that can be submitted to the GPU for execution by a pipeline. A GPUShaderModule object instance is created using GPUDevice.createShaderModule().
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUShaderModule">MDN Web Docs: GPUShaderModule</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUShaderModule")]
public class GPUShaderModule
{
    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The getCompilationInfo() method of the GPUShaderModule interface returns a Promise that fulfills with a GPUCompilationInfo object containing messages generated during the GPUShaderModule&apos;s compilation.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUShaderModule/getCompilationInfo">MDN Web Docs: GPUShaderModule.getCompilationInfo</see>
    /// </remarks>
    /// <returns>A Promise that fulfills with a GPUCompilationInfo object. GPUCompilationInfo contains a messages property, which is an array of GPUCompilationMessage objects, each one containing the details of an individual compilation message.</returns>
    [Description("@#getCompilationInfo")]
    public extern PromiseResult<GPUCompilationInfo> GetCompilationInfo();

    #region mixin GPUObjectBase
    /// <summary>
    /// JavaScript 属性 GPUObjectBase.label：USVString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </remarks>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUSupportedFeatures interface of the WebGPU API is a Set-like object that describes additional functionality supported by a GPUAdapter. The GPUSupportedFeatures object for the current adapter is accessed via the GPUAdapter.features property — use this to test what features your current setup supports. To create a GPUDevice with a specific feature enabled, you need to specify it in the requiredFeatures array of the GPUAdapter.requestDevice() descriptor. You should note that not all features will be available to WebGPU in all browsers that support it, even if the features are supported by the underlying hardware. This could be due to constraints in the underlying system, browser, or adapter. For example: The underlying system might not be able to guarantee exposure of a feature in a way that is compatible with a certain browser. The browser vendor might not have found a secure way to implement support for that feature, or might just not have gotten round to it yet. If you are hoping to take advantage of a specific additional feature in a WebGPU app, thorough testing is advised.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUSupportedFeatures">MDN Web Docs: GPUSupportedFeatures</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUSupportedFeatures")]
public class GPUSupportedFeatures : ISet<string>
{
    #region Set
    /// <summary>
    /// GPUSupportedFeatures 的 WebIDL 集合接口；元素类型为 DOMString。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/">WebGPU: GPUSupportedFeatures.setlike</see>
    /// </remarks>
extern int ICollection<string>.Count { get; }
    extern bool ICollection<string>.IsReadOnly { get; }
    extern bool ISet<string>.Add(string item);
    extern void ICollection<string>.Clear();
    extern bool ICollection<string>.Contains(string item);
    extern void ICollection<string>.CopyTo(string[] array, int arrayIndex);
    extern void ISet<string>.ExceptWith(IEnumerable<string> other);
    extern IEnumerator<string> IEnumerable<string>.GetEnumerator();
    extern void ISet<string>.IntersectWith(IEnumerable<string> other);
    extern bool ISet<string>.IsProperSubsetOf(IEnumerable<string> other);
    extern bool ISet<string>.IsProperSupersetOf(IEnumerable<string> other);
    extern bool ISet<string>.IsSubsetOf(IEnumerable<string> other);
    extern bool ISet<string>.IsSupersetOf(IEnumerable<string> other);
    extern bool ISet<string>.Overlaps(IEnumerable<string> other);
    extern bool ICollection<string>.Remove(string item);
    extern bool ISet<string>.SetEquals(IEnumerable<string> other);
    extern void ISet<string>.SymmetricExceptWith(IEnumerable<string> other);
    extern void ISet<string>.UnionWith(IEnumerable<string> other);
    extern void ICollection<string>.Add(string item);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUSupportedLimits interface of the WebGPU API describes the limits supported by a GPUAdapter.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUSupportedLimits">MDN Web Docs: GPUSupportedLimits</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUSupportedLimits")]
public class GPUSupportedLimits
{
    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxTextureDimension1D：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxtexturedimension1d">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxTextureDimension1D")]
    public extern uint MaxTextureDimension1D { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxTextureDimension2D：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxtexturedimension2d">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxTextureDimension2D")]
    public extern uint MaxTextureDimension2D { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxTextureDimension3D：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxtexturedimension3d">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxTextureDimension3D")]
    public extern uint MaxTextureDimension3D { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxTextureArrayLayers：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxtexturearraylayers">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxTextureArrayLayers")]
    public extern uint MaxTextureArrayLayers { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxBindGroups：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxbindgroups">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxBindGroups")]
    public extern uint MaxBindGroups { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxBindGroupsPlusVertexBuffers：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxbindgroupsplusvertexbuffers">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxBindGroupsPlusVertexBuffers")]
    public extern uint MaxBindGroupsPlusVertexBuffers { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxImmediateSize：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maximmediatesize">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxImmediateSize")]
    public extern uint MaxImmediateSize { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxBindingsPerBindGroup：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxbindingsperbindgroup">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxBindingsPerBindGroup")]
    public extern uint MaxBindingsPerBindGroup { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxDynamicUniformBuffersPerPipelineLayout：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxdynamicuniformbuffersperpipelinelayout">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxDynamicUniformBuffersPerPipelineLayout")]
    public extern uint MaxDynamicUniformBuffersPerPipelineLayout { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxDynamicStorageBuffersPerPipelineLayout：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxdynamicstoragebuffersperpipelinelayout">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxDynamicStorageBuffersPerPipelineLayout")]
    public extern uint MaxDynamicStorageBuffersPerPipelineLayout { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxSampledTexturesPerShaderStage：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxsampledtexturespershaderstage">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxSampledTexturesPerShaderStage")]
    public extern uint MaxSampledTexturesPerShaderStage { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxSamplersPerShaderStage：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxsamplerspershaderstage">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxSamplersPerShaderStage")]
    public extern uint MaxSamplersPerShaderStage { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxStorageBuffersPerShaderStage：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxstoragebufferspershaderstage">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxStorageBuffersPerShaderStage")]
    public extern uint MaxStorageBuffersPerShaderStage { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxStorageBuffersInVertexStage：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxstoragebuffersinvertexstage">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxStorageBuffersInVertexStage")]
    public extern uint MaxStorageBuffersInVertexStage { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxStorageBuffersInFragmentStage：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxstoragebuffersinfragmentstage">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxStorageBuffersInFragmentStage")]
    public extern uint MaxStorageBuffersInFragmentStage { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxStorageTexturesPerShaderStage：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxstoragetexturespershaderstage">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxStorageTexturesPerShaderStage")]
    public extern uint MaxStorageTexturesPerShaderStage { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxStorageTexturesInVertexStage：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxstoragetexturesinvertexstage">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxStorageTexturesInVertexStage")]
    public extern uint MaxStorageTexturesInVertexStage { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxStorageTexturesInFragmentStage：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxstoragetexturesinfragmentstage">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxStorageTexturesInFragmentStage")]
    public extern uint MaxStorageTexturesInFragmentStage { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxUniformBuffersPerShaderStage：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxuniformbufferspershaderstage">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxUniformBuffersPerShaderStage")]
    public extern uint MaxUniformBuffersPerShaderStage { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxUniformBufferBindingSize：unsigned long long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxuniformbufferbindingsize">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxUniformBufferBindingSize")]
    public extern Number MaxUniformBufferBindingSize { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxStorageBufferBindingSize：unsigned long long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxstoragebufferbindingsize">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxStorageBufferBindingSize")]
    public extern Number MaxStorageBufferBindingSize { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.minUniformBufferOffsetAlignment：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-minuniformbufferoffsetalignment">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#minUniformBufferOffsetAlignment")]
    public extern uint MinUniformBufferOffsetAlignment { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.minStorageBufferOffsetAlignment：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-minstoragebufferoffsetalignment">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#minStorageBufferOffsetAlignment")]
    public extern uint MinStorageBufferOffsetAlignment { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxVertexBuffers：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxvertexbuffers">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxVertexBuffers")]
    public extern uint MaxVertexBuffers { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxBufferSize：unsigned long long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxbuffersize">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxBufferSize")]
    public extern Number MaxBufferSize { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxVertexAttributes：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxvertexattributes">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxVertexAttributes")]
    public extern uint MaxVertexAttributes { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxVertexBufferArrayStride：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxvertexbufferarraystride">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxVertexBufferArrayStride")]
    public extern uint MaxVertexBufferArrayStride { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxInterStageShaderVariables：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxinterstageshadervariables">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxInterStageShaderVariables")]
    public extern uint MaxInterStageShaderVariables { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxColorAttachments：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxcolorattachments">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxColorAttachments")]
    public extern uint MaxColorAttachments { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxColorAttachmentBytesPerSample：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxcolorattachmentbytespersample">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxColorAttachmentBytesPerSample")]
    public extern uint MaxColorAttachmentBytesPerSample { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxComputeWorkgroupStorageSize：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxcomputeworkgroupstoragesize">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxComputeWorkgroupStorageSize")]
    public extern uint MaxComputeWorkgroupStorageSize { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxComputeInvocationsPerWorkgroup：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxcomputeinvocationsperworkgroup">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxComputeInvocationsPerWorkgroup")]
    public extern uint MaxComputeInvocationsPerWorkgroup { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxComputeWorkgroupSizeX：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxcomputeworkgroupsizex">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxComputeWorkgroupSizeX")]
    public extern uint MaxComputeWorkgroupSizeX { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxComputeWorkgroupSizeY：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxcomputeworkgroupsizey">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxComputeWorkgroupSizeY")]
    public extern uint MaxComputeWorkgroupSizeY { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxComputeWorkgroupSizeZ：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxcomputeworkgroupsizez">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxComputeWorkgroupSizeZ")]
    public extern uint MaxComputeWorkgroupSizeZ { get; }

    /// <summary>
    /// JavaScript 属性 GPUSupportedLimits.maxComputeWorkgroupsPerDimension：unsigned long。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxcomputeworkgroupsperdimension">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </remarks>
    [Description("@#maxComputeWorkgroupsPerDimension")]
    public extern uint MaxComputeWorkgroupsPerDimension { get; }
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUTexture interface of the WebGPU API represents a container used to store 1D, 2D, or 3D arrays of data, such as images, to use in GPU rendering operations. A GPUTexture object instance is created using the GPUDevice.createTexture() method.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUTexture">MDN Web Docs: GPUTexture</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUTexture")]
public class GPUTexture
{
    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The createView() method of the GPUTexture interface creates a GPUTextureView representing a specific view of the GPUTexture.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUTexture/createView">MDN Web Docs: GPUTexture.createView</see>
    /// </remarks>
    /// <param name="descriptor">An object containing the following properties: arrayLayerCount Optional A number defining how many array layers are accessible to the view, starting with the baseArrayLayer value. If arrayLayerCount is omitted, it is given a value as follows: If dimension is &quot;1d&quot;, &quot;2d&quot;, or &quot;3d&quot;, arrayLayerCount is 1. If dimension is &quot;cube&quot;, arrayLayerCount is 6. If dimension is &quot;2d-array&quot;, or &quot;cube-array&quot;, arrayLayerCount is GPUTexture.depthOrArrayLayers - baseArrayLayer. aspect Optional An enumerated value specifying which aspect(s) of the texture are accessible to the texture view. Possible values are: &quot;all&quot; All available aspects of the texture format will be accessible to the view, which can mean all or any of color, depth, and stencil, depending on what kind of format you are dealing with. &quot;depth-only&quot; Only the depth aspect of a depth-or-stencil format will be accessible to the view. &quot;stencil-only&quot; Only the stencil aspect of a depth-or-stencil format will be accessible to the view. If omitted, aspect takes a value of &quot;all&quot;. baseArrayLayer Optional A number defining the index of the first array layer accessible to the view. If omitted, baseArrayLayer takes a value of 0. baseMipLevel Optional A number representing the first (most detailed) mipmap level accessible to the view. If omitted, baseMipLevel takes a value of 0. dimension Optional An enumerated value specifying the format to view the texture as. Possible values are: &quot;1d&quot;: The texture is viewed as a one-dimensional image. &quot;2d&quot;: The texture is viewed as a single two-dimensional image. &quot;2d-array&quot;: The texture is viewed as an array of two-dimensional images. &quot;cube&quot;: The texture is viewed as a cubemap. The view has 6 array layers, corresponding to the [+X, -X, +Y, -Y, +Z, -Z] faces of the cube. Sampling is done seamlessly across the faces of the cubemap. &quot;cube-array&quot;: The texture is viewed as a packed array of N cubemaps, each with 6 array layers corresponding to the [+X, -X, +Y, -Y, +Z, -Z] faces of the cube. Sampling is done seamlessly across the faces of the cubemaps. &quot;3d&quot;: The texture is viewed as a three-dimensional image. If dimension is omitted, it is given a value as follows: If GPUTexture.dimension is &quot;1d&quot;, dimension is &quot;1d&quot;. If GPUTexture.dimension is &quot;2d&quot; and GPUTexture.depthOrArrayLayers is 1, dimension is &quot;2d&quot;. If GPUTexture.dimension is &quot;2d&quot; and GPUTexture.depthOrArrayLayers is more than 1, dimension is &quot;2d-array&quot;. If GPUTexture.dimension is &quot;3d&quot;, dimension is &quot;3d&quot;. format Optional An enumerated value specifying the format of the texture view. See the Texture formats section of the specification for all the possible values. If format is omitted, it will be given a value as follows: If aspect is &quot;depth-only&quot; or &quot;stencil-only&quot;, and GPUTexture.format is a depth-or-stencil format, format will be set equal to the appropriate aspect-specific format. Otherwise it will be set equal to GPUTexture.format. label Optional A string providing a label that can be used to identify the object, for example in GPUError messages or console warnings. mipLevelCount Optional A number defining how many mipmap levels are accessible to the view, starting with the baseMipLevel value. If mipLevelCount is omitted, it will be given a value of GPUTexture.mipLevelCount - baseMipLevel. swizzle Optional A string containing four characters. The position of each character maps to the texture view&apos;s red, green, blue, and alpha channel values, respectively. The value of each character specifies the value each of those channels will take when the view is accessed by a shader. Possible values are: r The texture&apos;s red channel value. g The texture&apos;s green channel value. b The texture&apos;s blue channel value. a The texture&apos;s alpha channel value. 0 Enforces a value of 0. 1 Enforces a value of 1. For example, swizzle: &quot;grba&quot; would result in the texture&apos;s red and green channel values being swapped when a shader accesses the view. Texture component swizzle allows developers to optimize performance, correct component ordering mismatches, and reuse shader code across various texture formats when sampling textures. Note: To use the swizzle property, you must enable the texture-component-swizzle feature in your GPUDevice by specifying it in the requiredFeatures array of the GPUAdapter.requestDevice() descriptor. If this feature is not enabled, the swizzle property will have no effect. usage Optional A set of bitwise flags representing a subset of the source texture&apos;s usage flags (available in the GPUTexture.usage property) that are compatible with the chosen view format. This can be used to restrict the allowed view usage in cases where the view format is incompatible with certain usages. The available usage flags are listed in the GPUTexture.usage value table. The default value is 0, which represents the source texture&apos;s full set of usage flags. If the view&apos;s format doesn&apos;t support all of the texture&apos;s usages, the default will fail, and the view&apos;s usage must be specified explicitly. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUTexture/createView">MDN Web Docs: descriptor</see></param>
    /// <returns>A GPUTextureView object instance.</returns>
    [Description("@#createView")]
    public extern GPUTextureView CreateView(GPUTextureViewDescriptor? descriptor = default);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The destroy() method of the GPUTexture interface destroys the GPUTexture.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUTexture/destroy">MDN Web Docs: GPUTexture.destroy</see>
    /// </remarks>
    [Description("@#destroy")]
    public extern void Destroy();

    /// <summary>
    /// &quot;unfilterable-float&quot; ✓ If &quot;float32-blendable&quot; is enabled ✓ ✓ If &quot;texture-formats-tier2&quot; is enabled 16 mixed component width, 32 bits per texel (4-byte render target component alignment) rgb10a2uint &quot;uint&quot; ✓ If &quot;core-features-and-limits&quot; is enabled If &quot;texture-formats-tier1&quot; is enabled 4 8 rgb10a2unorm &quot;float&quot;, &quot;unfilterable-float&quot; ✓ ✓ ✓ ✓ If &quot;texture-formats-tier1&quot; is enabled 4 8 rg11b10ufloat &quot;float&quot;, &quot;unfilterable-float&quot; If &quot;rg11b10ufloat-renderable&quot; is enabled If &quot;texture-formats-tier1&quot; is enabled 4 8
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputexture-width">WebGPU: 6.1 GPUTexture</see>
    /// </remarks>
    [Description("@#width")]
    public extern GPUIntegerCoordinateOut Width { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The height read-only property of the GPUTexture interface represents the height of the GPUTexture. This is set based on the value of the size property in the descriptor object passed into the originating GPUDevice.createTexture() call.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUTexture/height">MDN Web Docs: GPUTexture.height</see>
    /// </remarks>
    [Description("@#height")]
    public extern GPUIntegerCoordinateOut Height { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The depthOrArrayLayers read-only property of the GPUTexture interface represents the depth or layer count of the GPUTexture. This is set based on the size property in the descriptor object passed into the originating GPUDevice.createTexture() call.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUTexture/depthOrArrayLayers">MDN Web Docs: GPUTexture.depthOrArrayLayers</see>
    /// </remarks>
    [Description("@#depthOrArrayLayers")]
    public extern GPUIntegerCoordinateOut DepthOrArrayLayers { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The mipLevelCount read-only property of the GPUTexture interface represents the number of mip levels of the GPUTexture. This is set via the mipLevelCount property in the descriptor object passed into the originating GPUDevice.createTexture() call. If omitted, this defaults to 1.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUTexture/mipLevelCount">MDN Web Docs: GPUTexture.mipLevelCount</see>
    /// </remarks>
    [Description("@#mipLevelCount")]
    public extern GPUIntegerCoordinateOut MipLevelCount { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The sampleCount read-only property of the GPUTexture interface represents the sample count of the GPUTexture. This is set via the sampleCount property in the descriptor object passed into the originating GPUDevice.createTexture() call. If omitted, this defaults to 1.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUTexture/sampleCount">MDN Web Docs: GPUTexture.sampleCount</see>
    /// </remarks>
    [Description("@#sampleCount")]
    public extern GPUSize32Out SampleCount { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The dimension read-only property of the GPUTexture interface represents the dimension of the set of texels for each GPUTexture subresource. This is set via the dimension property in the descriptor object passed into the originating GPUDevice.createTexture() call, which defaults to &quot;2d&quot; if omitted.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUTexture/dimension">MDN Web Docs: GPUTexture.dimension</see>
    /// </remarks>
    [Description("@#dimension")]
    public extern GPUTextureDimension Dimension { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The format read-only property of the GPUTexture interface represents the format of the GPUTexture. This is set via the format property in the descriptor object passed into the originating GPUDevice.createTexture() call.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUTexture/format">MDN Web Docs: GPUTexture.format</see>
    /// </remarks>
    [Description("@#format")]
    public extern GPUTextureFormat Format { get; }

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The usage read-only property of the GPUTexture interface is the bitwise flags representing the allowed usages of the GPUTexture. This is set via the usage property in the descriptor object passed into the originating GPUDevice.createTexture() call.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUTexture/usage">MDN Web Docs: GPUTexture.usage</see>
    /// </remarks>
    [Description("@#usage")]
    public extern GPUFlagsConstant Usage { get; }

    /// <summary>
    /// JavaScript 属性 GPUTexture.textureBindingViewDimension：GPUTextureViewDimension, undefined。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputexture-texturebindingviewdimension">WebGPU: 6.1 GPUTexture</see>
    /// </remarks>
    [Description("@#textureBindingViewDimension")]
    public extern GPUTextureViewDimension? TextureBindingViewDimension { get; }

    #region mixin GPUObjectBase
    /// <summary>
    /// JavaScript 属性 GPUObjectBase.label：USVString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </remarks>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUTextureView interface of the WebGPU API represents a view into a subset of the texture resources defined by a particular GPUTexture. A GPUTextureView object instance is created using the GPUTexture.createView() method.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUTextureView">MDN Web Docs: GPUTextureView</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUTextureView")]
public class GPUTextureView
{
    #region mixin GPUObjectBase
    /// <summary>
    /// JavaScript 属性 GPUObjectBase.label：USVString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </remarks>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUUncapturedErrorEvent interface of the WebGPU API is the event object type for the GPUDevice uncapturederror event, used for telemetry and to report unexpected errors. Known error cases should be handled using pushErrorScope() and popErrorScope().
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUUncapturedErrorEvent">MDN Web Docs: GPUUncapturedErrorEvent</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUUncapturedErrorEvent")]
public class GPUUncapturedErrorEvent(string type, EventInit eventInitDict) : EventRef(type, eventInitDict)
{
    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUUncapturedErrorEvent() constructor creates a new GPUUncapturedErrorEvent object instance.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUUncapturedErrorEvent/GPUUncapturedErrorEvent">MDN Web Docs: GPUUncapturedErrorEvent.GPUUncapturedErrorEvent</see>
    /// </remarks>
    /// <param name="type">An enumerated value specifying the type of error. Possible values are: &quot;internal&quot; The error is a GPUInternalError. &quot;out-of-memory&quot; The error is a GPUOutOfMemoryError. &quot;validation&quot; The error is a GPUValidationError. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUUncapturedErrorEvent/GPUUncapturedErrorEvent">MDN Web Docs: type</see></param>
    /// <param name="gpuUncapturedErrorEventInitDict"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuuncapturederrorevent-gpuuncapturederrorevent-type-gpuuncapturederroreventinitdict-gpuuncapturederroreventinitdict">WebGPU: 22.4 Telemetry</see></param>
    public extern GPUUncapturedErrorEvent(string type, GPUUncapturedErrorEventInit gpuUncapturedErrorEventInitDict);

    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The error read-only property of the GPUUncapturedErrorEvent interface is a GPUError object instance providing access to the details of the error.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUUncapturedErrorEvent/error">MDN Web Docs: GPUUncapturedErrorEvent.error</see>
    /// </remarks>
    [Description("@#error")]
    public extern GPUError Error { get; }
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUValidationError interface of the WebGPU API describes an application error indicating that an operation did not pass the WebGPU API&apos;s validation constraints. It represents one of the types of errors surfaced by GPUDevice.popErrorScope and the uncapturederror event. Validation errors occur whenever invalid inputs are given to a WebGPU call. These are consistent, predictable, and should not occur provided your app is well-formed. They will occur in the same way on every device your code runs on, so once you&apos;ve fixed any errors that show up during development you probably don&apos;t need to observe them directly most of the time. An exception to that rule is if you&apos;re consuming user-supplied assets, shaders, etc., in which case watching for validation errors while loading could be helpful. Note: We have attempted to provide useful information to help you understand why validation errors are occurring in your WebGPU code in &quot;Validation&quot; sections where appropriate, which list criteria to meet to avoid validation errors. See for example the GPUDevice.createBindGroup() Validation section.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUValidationError">MDN Web Docs: GPUValidationError</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUValidationError")]
public class GPUValidationError : GPUError
{
    /// <summary>
    /// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The GPUValidationError() constructor creates a new GPUValidationError object instance.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUValidationError/GPUValidationError">MDN Web Docs: GPUValidationError.GPUValidationError</see>
    /// </remarks>
    /// <param name="message">A string providing a human-readable message that explains why the error occurred. <see href="https://developer.mozilla.org/en-US/docs/Web/API/GPUValidationError/GPUValidationError">MDN Web Docs: message</see></param>
    public extern GPUValidationError(string message);
}

/// <summary>
/// Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. Note: This feature is available in Web Workers. The WGSLLanguageFeatures interface of the WebGPU API is a setlike object that reports the WGSL language extensions supported by the WebGPU implementation. The WGSLLanguageFeatures object is accessed via the GPU.wgslLanguageFeatures property. Note: Not all WGSL language extensions are available to WebGPU in all browsers that support the API. We recommend you thoroughly test any extensions you choose to use.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/WGSLLanguageFeatures">MDN Web Docs: WGSLLanguageFeatures</see>
/// </remarks>
[ECMAScript]
[Description("@#WGSLLanguageFeatures")]
public class WGSLLanguageFeatures : ISet<string>
{
    #region Set
    /// <summary>
    /// WGSLLanguageFeatures 的 WebIDL 集合接口；元素类型为 DOMString。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/">WebGPU: WGSLLanguageFeatures.setlike</see>
    /// </remarks>
extern int ICollection<string>.Count { get; }
    extern bool ICollection<string>.IsReadOnly { get; }
    extern bool ISet<string>.Add(string item);
    extern void ICollection<string>.Clear();
    extern bool ICollection<string>.Contains(string item);
    extern void ICollection<string>.CopyTo(string[] array, int arrayIndex);
    extern void ISet<string>.ExceptWith(IEnumerable<string> other);
    extern IEnumerator<string> IEnumerable<string>.GetEnumerator();
    extern void ISet<string>.IntersectWith(IEnumerable<string> other);
    extern bool ISet<string>.IsProperSubsetOf(IEnumerable<string> other);
    extern bool ISet<string>.IsProperSupersetOf(IEnumerable<string> other);
    extern bool ISet<string>.IsSubsetOf(IEnumerable<string> other);
    extern bool ISet<string>.IsSupersetOf(IEnumerable<string> other);
    extern bool ISet<string>.Overlaps(IEnumerable<string> other);
    extern bool ICollection<string>.Remove(string item);
    extern bool ISet<string>.SetEquals(IEnumerable<string> other);
    extern void ISet<string>.SymmetricExceptWith(IEnumerable<string> other);
    extern void ISet<string>.UnionWith(IEnumerable<string> other);
    extern void ICollection<string>.Add(string item);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion
}