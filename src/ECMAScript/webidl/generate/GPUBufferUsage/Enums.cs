namespace ECMAScript.GPUBufferUsage;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpuaddressmode">WebGPU: 7.1.1 GPUSamplerDescriptor</see>
/// </summary>
[Description("@#GPUAddressMode")]
[ECMAScript]
[String]
public enum GPUAddressMode
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuaddressmode-clamp-to-edge">WebGPU: 7.1.1 GPUSamplerDescriptor</see>
    /// </summary>
    [Description("@#clamp-to-edge")]
    ClampToEdge = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuaddressmode-repeat">WebGPU: 7.1.1 GPUSamplerDescriptor</see>
    /// </summary>
    [Description("@#repeat")]
    Repeat = 1,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuaddressmode-mirror-repeat">WebGPU: 7.1.1 GPUSamplerDescriptor</see>
    /// </summary>
    [Description("@#mirror-repeat")]
    MirrorRepeat = 2
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpuautolayoutmode">WebGPU: 10.1 Base pipelines</see>
/// </summary>
[Description("@#GPUAutoLayoutMode")]
[ECMAScript]
[String]
public enum GPUAutoLayoutMode
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuautolayoutmode-auto">WebGPU: 10.1 Base pipelines</see>
    /// </summary>
    [Description("@#auto")]
    Auto = 0
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpublendfactor">WebGPU: 10.3.5.1 Blend State</see>
/// </summary>
[Description("@#GPUBlendFactor")]
[ECMAScript]
[String]
public enum GPUBlendFactor
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendfactor-zero">WebGPU: 10.3.5.1 Blend State</see>
    /// </summary>
    [Description("@#zero")]
    Zero = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendfactor-one">WebGPU: 10.3.5.1 Blend State</see>
    /// </summary>
    [Description("@#one")]
    One = 1,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendfactor-src">WebGPU: 10.3.5.1 Blend State</see>
    /// </summary>
    [Description("@#src")]
    Src = 2,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendfactor-one-minus-src">WebGPU: 10.3.5.1 Blend State</see>
    /// </summary>
    [Description("@#one-minus-src")]
    OneMinusSrc = 3,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendfactor-src-alpha">WebGPU: 10.3.5.1 Blend State</see>
    /// </summary>
    [Description("@#src-alpha")]
    SrcAlpha = 4,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendfactor-one-minus-src-alpha">WebGPU: 10.3.5.1 Blend State</see>
    /// </summary>
    [Description("@#one-minus-src-alpha")]
    OneMinusSrcAlpha = 5,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendfactor-dst">WebGPU: 10.3.5.1 Blend State</see>
    /// </summary>
    [Description("@#dst")]
    Dst = 6,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendfactor-one-minus-dst">WebGPU: 10.3.5.1 Blend State</see>
    /// </summary>
    [Description("@#one-minus-dst")]
    OneMinusDst = 7,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendfactor-dst-alpha">WebGPU: 10.3.5.1 Blend State</see>
    /// </summary>
    [Description("@#dst-alpha")]
    DstAlpha = 8,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendfactor-one-minus-dst-alpha">WebGPU: 10.3.5.1 Blend State</see>
    /// </summary>
    [Description("@#one-minus-dst-alpha")]
    OneMinusDstAlpha = 9,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendfactor-src-alpha-saturated">WebGPU: 10.3.5.1 Blend State</see>
    /// </summary>
    [Description("@#src-alpha-saturated")]
    SrcAlphaSaturated = 10,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendfactor-constant">WebGPU: 10.3.5.1 Blend State</see>
    /// </summary>
    [Description("@#constant")]
    Constant = 11,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendfactor-one-minus-constant">WebGPU: 10.3.5.1 Blend State</see>
    /// </summary>
    [Description("@#one-minus-constant")]
    OneMinusConstant = 12,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendfactor-src1">WebGPU: 10.3.5.1 Blend State</see>
    /// </summary>
    [Description("@#src1")]
    Src1 = 13,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendfactor-one-minus-src1">WebGPU: 10.3.5.1 Blend State</see>
    /// </summary>
    [Description("@#one-minus-src1")]
    OneMinusSrc1 = 14,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendfactor-src1-alpha">WebGPU: 10.3.5.1 Blend State</see>
    /// </summary>
    [Description("@#src1-alpha")]
    Src1Alpha = 15,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendfactor-one-minus-src1-alpha">WebGPU: 10.3.5.1 Blend State</see>
    /// </summary>
    [Description("@#one-minus-src1-alpha")]
    OneMinusSrc1Alpha = 16
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpublendoperation">WebGPU: 10.3.5.1 Blend State</see>
/// </summary>
[Description("@#GPUBlendOperation")]
[ECMAScript]
[String]
public enum GPUBlendOperation
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendoperation-add">WebGPU: 10.3.5.1 Blend State</see>
    /// </summary>
    [Description("@#add")]
    Add = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendoperation-subtract">WebGPU: 10.3.5.1 Blend State</see>
    /// </summary>
    [Description("@#subtract")]
    Subtract = 1,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendoperation-reverse-subtract">WebGPU: 10.3.5.1 Blend State</see>
    /// </summary>
    [Description("@#reverse-subtract")]
    ReverseSubtract = 2,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendoperation-min">WebGPU: 10.3.5.1 Blend State</see>
    /// </summary>
    [Description("@#min")]
    Min = 3,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendoperation-max">WebGPU: 10.3.5.1 Blend State</see>
    /// </summary>
    [Description("@#max")]
    Max = 4
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpubufferbindingtype">WebGPU: 8.1.1 Bind Group Layout Creation</see>
/// </summary>
[Description("@#GPUBufferBindingType")]
[ECMAScript]
[String]
public enum GPUBufferBindingType
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferbindingtype-uniform">WebGPU: 8.1.1 Bind Group Layout Creation</see>
    /// </summary>
    [Description("@#uniform")]
    Uniform = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferbindingtype-storage">WebGPU: 8.1.1 Bind Group Layout Creation</see>
    /// </summary>
    [Description("@#storage")]
    Storage = 1,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferbindingtype-read-only-storage">WebGPU: 8.1.1 Bind Group Layout Creation</see>
    /// </summary>
    [Description("@#read-only-storage")]
    ReadOnlyStorage = 2
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpubuffermapstate">WebGPU: 5.1 GPUBuffer</see>
/// </summary>
[Description("@#GPUBufferMapState")]
[ECMAScript]
[String]
public enum GPUBufferMapState
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubuffermapstate-unmapped">WebGPU: 5.1 GPUBuffer</see>
    /// </summary>
    [Description("@#unmapped")]
    Unmapped = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubuffermapstate-pending">WebGPU: 5.1 GPUBuffer</see>
    /// </summary>
    [Description("@#pending")]
    Pending = 1,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubuffermapstate-mapped">WebGPU: 5.1 GPUBuffer</see>
    /// </summary>
    [Description("@#mapped")]
    Mapped = 2
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpucomparefunction">WebGPU: 7.1.1 GPUSamplerDescriptor</see>
/// </summary>
[Description("@#GPUCompareFunction")]
[ECMAScript]
[String]
public enum GPUCompareFunction
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucomparefunction-never">WebGPU: 7.1.1 GPUSamplerDescriptor</see>
    /// </summary>
    [Description("@#never")]
    Never = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucomparefunction-less">WebGPU: 7.1.1 GPUSamplerDescriptor</see>
    /// </summary>
    [Description("@#less")]
    Less = 1,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucomparefunction-equal">WebGPU: 7.1.1 GPUSamplerDescriptor</see>
    /// </summary>
    [Description("@#equal")]
    Equal = 2,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucomparefunction-less-equal">WebGPU: 7.1.1 GPUSamplerDescriptor</see>
    /// </summary>
    [Description("@#less-equal")]
    LessEqual = 3,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucomparefunction-greater">WebGPU: 7.1.1 GPUSamplerDescriptor</see>
    /// </summary>
    [Description("@#greater")]
    Greater = 4,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucomparefunction-not-equal">WebGPU: 7.1.1 GPUSamplerDescriptor</see>
    /// </summary>
    [Description("@#not-equal")]
    NotEqual = 5,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucomparefunction-greater-equal">WebGPU: 7.1.1 GPUSamplerDescriptor</see>
    /// </summary>
    [Description("@#greater-equal")]
    GreaterEqual = 6,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucomparefunction-always">WebGPU: 7.1.1 GPUSamplerDescriptor</see>
    /// </summary>
    [Description("@#always")]
    Always = 7
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpucompilationmessagetype">WebGPU: 9.1.2 Shader Module Compilation Information</see>
/// </summary>
[Description("@#GPUCompilationMessageType")]
[ECMAScript]
[String]
public enum GPUCompilationMessageType
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucompilationmessagetype-error">WebGPU: 9.1.2 Shader Module Compilation Information</see>
    /// </summary>
    [Description("@#error")]
    Error = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucompilationmessagetype-warning">WebGPU: 9.1.2 Shader Module Compilation Information</see>
    /// </summary>
    [Description("@#warning")]
    Warning = 1,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucompilationmessagetype-info">WebGPU: 9.1.2 Shader Module Compilation Information</see>
    /// </summary>
    [Description("@#info")]
    Info = 2
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpucullmode">WebGPU: 10.3.2 Primitive State</see>
/// </summary>
[Description("@#GPUCullMode")]
[ECMAScript]
[String]
public enum GPUCullMode
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucullmode-none">WebGPU: 10.3.2 Primitive State</see>
    /// </summary>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucullmode-front">WebGPU: 10.3.2 Primitive State</see>
    /// </summary>
    [Description("@#front")]
    Front = 1,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucullmode-back">WebGPU: 10.3.2 Primitive State</see>
    /// </summary>
    [Description("@#back")]
    Back = 2
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpudevicelostreason">WebGPU: 22.1 Fatal Errors</see>
/// </summary>
[Description("@#GPUDeviceLostReason")]
[ECMAScript]
[String]
public enum GPUDeviceLostReason
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevicelostreason-unknown">WebGPU: 22.1 Fatal Errors</see>
    /// </summary>
    [Description("@#unknown")]
    Unknown = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevicelostreason-destroyed">WebGPU: 22.1 Fatal Errors</see>
    /// </summary>
    [Description("@#destroyed")]
    Destroyed = 1
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpuerrorfilter">WebGPU: 22.3 Error Scopes</see>
/// </summary>
[Description("@#GPUErrorFilter")]
[ECMAScript]
[String]
public enum GPUErrorFilter
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuerrorfilter-validation">WebGPU: 22.3 Error Scopes</see>
    /// </summary>
    [Description("@#validation")]
    Validation = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuerrorfilter-out-of-memory">WebGPU: 22.3 Error Scopes</see>
    /// </summary>
    [Description("@#out-of-memory")]
    OutOfMemory = 1,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuerrorfilter-internal">WebGPU: 22.3 Error Scopes</see>
    /// </summary>
    [Description("@#internal")]
    Internal = 2
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpufiltermode">WebGPU: 7.1.1 GPUSamplerDescriptor</see>
/// </summary>
[Description("@#GPUFilterMode")]
[ECMAScript]
[String]
public enum GPUFilterMode
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpufiltermode-nearest">WebGPU: 7.1.1 GPUSamplerDescriptor</see>
    /// </summary>
    [Description("@#nearest")]
    Nearest = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpufiltermode-linear">WebGPU: 7.1.1 GPUSamplerDescriptor</see>
    /// </summary>
    [Description("@#linear")]
    Linear = 1
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpufrontface">WebGPU: 10.3.2 Primitive State</see>
/// </summary>
[Description("@#GPUFrontFace")]
[ECMAScript]
[String]
public enum GPUFrontFace
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpufrontface-ccw">WebGPU: 10.3.2 Primitive State</see>
    /// </summary>
    [Description("@#ccw")]
    Ccw = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpufrontface-cw">WebGPU: 10.3.2 Primitive State</see>
    /// </summary>
    [Description("@#cw")]
    Cw = 1
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpuindexformat">WebGPU: 10.3.7 Vertex State</see>
/// </summary>
[Description("@#GPUIndexFormat")]
[ECMAScript]
[String]
public enum GPUIndexFormat
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuindexformat-uint16">WebGPU: 10.3.7 Vertex State</see>
    /// </summary>
    [Description("@#uint16")]
    Uint16 = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuindexformat-uint32">WebGPU: 10.3.7 Vertex State</see>
    /// </summary>
    [Description("@#uint32")]
    Uint32 = 1
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpuloadop">WebGPU: 17.1.1.3 Load &amp; Store Operations</see>
/// </summary>
[Description("@#GPULoadOp")]
[ECMAScript]
[String]
public enum GPULoadOp
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuloadop-load">WebGPU: 17.1.1.3 Load &amp; Store Operations</see>
    /// </summary>
    [Description("@#load")]
    Load = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuloadop-clear">WebGPU: 17.1.1.3 Load &amp; Store Operations</see>
    /// </summary>
    [Description("@#clear")]
    Clear = 1
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpumipmapfiltermode">WebGPU: 7.1.1 GPUSamplerDescriptor</see>
/// </summary>
[Description("@#GPUMipmapFilterMode")]
[ECMAScript]
[String]
public enum GPUMipmapFilterMode
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpumipmapfiltermode-nearest">WebGPU: 7.1.1 GPUSamplerDescriptor</see>
    /// </summary>
    [Description("@#nearest")]
    Nearest = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpumipmapfiltermode-linear">WebGPU: 7.1.1 GPUSamplerDescriptor</see>
    /// </summary>
    [Description("@#linear")]
    Linear = 1
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpupipelineerrorreason">WebGPU: 10. Pipelines</see>
/// </summary>
[Description("@#GPUPipelineErrorReason")]
[ECMAScript]
[String]
public enum GPUPipelineErrorReason
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpupipelineerrorreason-validation">WebGPU: 10. Pipelines</see>
    /// </summary>
    [Description("@#validation")]
    Validation = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpupipelineerrorreason-internal">WebGPU: 10. Pipelines</see>
    /// </summary>
    [Description("@#internal")]
    Internal = 1
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpupowerpreference">WebGPU: 4.2.2 Adapter Selection</see>
/// </summary>
[Description("@#GPUPowerPreference")]
[ECMAScript]
[String]
public enum GPUPowerPreference
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpupowerpreference-low-power">WebGPU: 4.2.2 Adapter Selection</see>
    /// </summary>
    [Description("@#low-power")]
    LowPower = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpupowerpreference-high-performance">WebGPU: 4.2.2 Adapter Selection</see>
    /// </summary>
    [Description("@#high-performance")]
    HighPerformance = 1
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpuprimitivetopology">WebGPU: 10.3.2 Primitive State</see>
/// </summary>
[Description("@#GPUPrimitiveTopology")]
[ECMAScript]
[String]
public enum GPUPrimitiveTopology
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuprimitivetopology-point-list">WebGPU: 10.3.2 Primitive State</see>
    /// </summary>
    [Description("@#point-list")]
    PointList = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuprimitivetopology-line-list">WebGPU: 10.3.2 Primitive State</see>
    /// </summary>
    [Description("@#line-list")]
    LineList = 1,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuprimitivetopology-line-strip">WebGPU: 10.3.2 Primitive State</see>
    /// </summary>
    [Description("@#line-strip")]
    LineStrip = 2,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuprimitivetopology-triangle-list">WebGPU: 10.3.2 Primitive State</see>
    /// </summary>
    [Description("@#triangle-list")]
    TriangleList = 3,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuprimitivetopology-triangle-strip">WebGPU: 10.3.2 Primitive State</see>
    /// </summary>
    [Description("@#triangle-strip")]
    TriangleStrip = 4
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpuquerytype">WebGPU: 20.2 QueryType</see>
/// </summary>
[Description("@#GPUQueryType")]
[ECMAScript]
[String]
public enum GPUQueryType
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuquerytype-occlusion">WebGPU: 20.2 QueryType</see>
    /// </summary>
    [Description("@#occlusion")]
    Occlusion = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuquerytype-timestamp">WebGPU: 20.2 QueryType</see>
    /// </summary>
    [Description("@#timestamp")]
    Timestamp = 1
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpusamplerbindingtype">WebGPU: 8.1.1 Bind Group Layout Creation</see>
/// </summary>
[Description("@#GPUSamplerBindingType")]
[ECMAScript]
[String]
public enum GPUSamplerBindingType
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerbindingtype-filtering">WebGPU: 8.1.1 Bind Group Layout Creation</see>
    /// </summary>
    [Description("@#filtering")]
    Filtering = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerbindingtype-non-filtering">WebGPU: 8.1.1 Bind Group Layout Creation</see>
    /// </summary>
    [Description("@#non-filtering")]
    NonFiltering = 1,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerbindingtype-comparison">WebGPU: 8.1.1 Bind Group Layout Creation</see>
    /// </summary>
    [Description("@#comparison")]
    Comparison = 2
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpustenciloperation">WebGPU: 10.3.6 Depth/Stencil State</see>
/// </summary>
[Description("@#GPUStencilOperation")]
[ECMAScript]
[String]
public enum GPUStencilOperation
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpustenciloperation-keep">WebGPU: 10.3.6 Depth/Stencil State</see>
    /// </summary>
    [Description("@#keep")]
    Keep = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpustenciloperation-zero">WebGPU: 10.3.6 Depth/Stencil State</see>
    /// </summary>
    [Description("@#zero")]
    Zero = 1,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpustenciloperation-replace">WebGPU: 10.3.6 Depth/Stencil State</see>
    /// </summary>
    [Description("@#replace")]
    Replace = 2,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpustenciloperation-invert">WebGPU: 10.3.6 Depth/Stencil State</see>
    /// </summary>
    [Description("@#invert")]
    Invert = 3,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpustenciloperation-increment-clamp">WebGPU: 10.3.6 Depth/Stencil State</see>
    /// </summary>
    [Description("@#increment-clamp")]
    IncrementClamp = 4,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpustenciloperation-decrement-clamp">WebGPU: 10.3.6 Depth/Stencil State</see>
    /// </summary>
    [Description("@#decrement-clamp")]
    DecrementClamp = 5,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpustenciloperation-increment-wrap">WebGPU: 10.3.6 Depth/Stencil State</see>
    /// </summary>
    [Description("@#increment-wrap")]
    IncrementWrap = 6,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpustenciloperation-decrement-wrap">WebGPU: 10.3.6 Depth/Stencil State</see>
    /// </summary>
    [Description("@#decrement-wrap")]
    DecrementWrap = 7
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpustoragetextureaccess">WebGPU: 8.1.1 Bind Group Layout Creation</see>
/// </summary>
[Description("@#GPUStorageTextureAccess")]
[ECMAScript]
[String]
public enum GPUStorageTextureAccess
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpustoragetextureaccess-write-only">WebGPU: 8.1.1 Bind Group Layout Creation</see>
    /// </summary>
    [Description("@#write-only")]
    WriteOnly = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpustoragetextureaccess-read-only">WebGPU: 8.1.1 Bind Group Layout Creation</see>
    /// </summary>
    [Description("@#read-only")]
    ReadOnly = 1,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpustoragetextureaccess-read-write">WebGPU: 8.1.1 Bind Group Layout Creation</see>
    /// </summary>
    [Description("@#read-write")]
    ReadWrite = 2
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpustoreop">WebGPU: 17.1.1.3 Load &amp; Store Operations</see>
/// </summary>
[Description("@#GPUStoreOp")]
[ECMAScript]
[String]
public enum GPUStoreOp
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpustoreop-store">WebGPU: 17.1.1.3 Load &amp; Store Operations</see>
    /// </summary>
    [Description("@#store")]
    Store = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpustoreop-discard">WebGPU: 17.1.1.3 Load &amp; Store Operations</see>
    /// </summary>
    [Description("@#discard")]
    Discard = 1
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gputextureaspect">WebGPU: 6.2.1 Texture View Creation</see>
/// </summary>
[Description("@#GPUTextureAspect")]
[ECMAScript]
[String]
public enum GPUTextureAspect
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureaspect-all">WebGPU: 6.2.1 Texture View Creation</see>
    /// </summary>
    [Description("@#all")]
    All = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureaspect-stencil-only">WebGPU: 6.2.1 Texture View Creation</see>
    /// </summary>
    [Description("@#stencil-only")]
    StencilOnly = 1,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureaspect-depth-only">WebGPU: 6.2.1 Texture View Creation</see>
    /// </summary>
    [Description("@#depth-only")]
    DepthOnly = 2
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gputexturedimension">WebGPU: 6.1.1 GPUTextureDescriptor</see>
/// </summary>
[Description("@#GPUTextureDimension")]
[ECMAScript]
[String]
public enum GPUTextureDimension
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputexturedimension-1d">WebGPU: 6.1.1 GPUTextureDescriptor</see>
    /// </summary>
    [Description("@#1d")]
    _1d = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputexturedimension-2d">WebGPU: 6.1.1 GPUTextureDescriptor</see>
    /// </summary>
    [Description("@#2d")]
    _2d = 1,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputexturedimension-3d">WebGPU: 6.1.1 GPUTextureDescriptor</see>
    /// </summary>
    [Description("@#3d")]
    _3d = 2
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gputextureformat">WebGPU: 6.3 Texture Formats</see>
/// </summary>
[Description("@#GPUTextureFormat")]
[ECMAScript]
[String]
public enum GPUTextureFormat
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-r8unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#r8unorm")]
    R8unorm = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-r8snorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#r8snorm")]
    R8snorm = 1,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-r8uint">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#r8uint")]
    R8uint = 2,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-r8sint">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#r8sint")]
    R8sint = 3,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-r16unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#r16unorm")]
    R16unorm = 4,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-r16snorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#r16snorm")]
    R16snorm = 5,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-r16uint">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#r16uint")]
    R16uint = 6,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-r16sint">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#r16sint")]
    R16sint = 7,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-r16float">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#r16float")]
    R16float = 8,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rg8unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#rg8unorm")]
    Rg8unorm = 9,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rg8snorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#rg8snorm")]
    Rg8snorm = 10,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rg8uint">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#rg8uint")]
    Rg8uint = 11,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rg8sint">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#rg8sint")]
    Rg8sint = 12,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-r32uint">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#r32uint")]
    R32uint = 13,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-r32sint">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#r32sint")]
    R32sint = 14,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-r32float">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#r32float")]
    R32float = 15,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rg16unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#rg16unorm")]
    Rg16unorm = 16,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rg16snorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#rg16snorm")]
    Rg16snorm = 17,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rg16uint">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#rg16uint")]
    Rg16uint = 18,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rg16sint">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#rg16sint")]
    Rg16sint = 19,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rg16float">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#rg16float")]
    Rg16float = 20,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rgba8unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#rgba8unorm")]
    Rgba8unorm = 21,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rgba8unorm-srgb">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#rgba8unorm-srgb")]
    Rgba8unormSrgb = 22,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rgba8snorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#rgba8snorm")]
    Rgba8snorm = 23,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rgba8uint">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#rgba8uint")]
    Rgba8uint = 24,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rgba8sint">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#rgba8sint")]
    Rgba8sint = 25,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-bgra8unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#bgra8unorm")]
    Bgra8unorm = 26,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-bgra8unorm-srgb">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#bgra8unorm-srgb")]
    Bgra8unormSrgb = 27,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rgb9e5ufloat">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#rgb9e5ufloat")]
    Rgb9e5ufloat = 28,

    /// <summary>
    /// &quot;unfilterable-float&quot; ✓ If &quot;float32-blendable&quot; is enabled ✓ ✓ If &quot;texture-formats-tier2&quot; is enabled 16 mixed component width, 32 bits per texel (4-byte render target component alignment) rgb10a2uint &quot;uint&quot; ✓ If &quot;core-features-and-limits&quot; is enabled If &quot;texture-formats-tier1&quot; is enabled 4 8 rgb10a2unorm &quot;float&quot;, &quot;unfilterable-float&quot; ✓ ✓ ✓ ✓ If &quot;texture-formats-tier1&quot; is enabled 4 8 rg11b10ufloat &quot;float&quot;, &quot;unfilterable-float&quot; If &quot;rg11b10ufloat-renderable&quot; is enabled If &quot;texture-formats-tier1&quot; is enabled 4 8
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rgb10a2uint">WebGPU: 6.3 Texture Formats</see>
    /// </remarks>
    [Description("@#rgb10a2uint")]
    Rgb10a2uint = 29,

    /// <summary>
    /// &quot;unfilterable-float&quot; ✓ If &quot;float32-blendable&quot; is enabled ✓ ✓ If &quot;texture-formats-tier2&quot; is enabled 16 mixed component width, 32 bits per texel (4-byte render target component alignment) rgb10a2uint &quot;uint&quot; ✓ If &quot;core-features-and-limits&quot; is enabled If &quot;texture-formats-tier1&quot; is enabled 4 8 rgb10a2unorm &quot;float&quot;, &quot;unfilterable-float&quot; ✓ ✓ ✓ ✓ If &quot;texture-formats-tier1&quot; is enabled 4 8 rg11b10ufloat &quot;float&quot;, &quot;unfilterable-float&quot; If &quot;rg11b10ufloat-renderable&quot; is enabled If &quot;texture-formats-tier1&quot; is enabled 4 8
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rgb10a2unorm">WebGPU: 6.3 Texture Formats</see>
    /// </remarks>
    [Description("@#rgb10a2unorm")]
    Rgb10a2unorm = 30,

    /// <summary>
    /// &quot;unfilterable-float&quot; ✓ If &quot;float32-blendable&quot; is enabled ✓ ✓ If &quot;texture-formats-tier2&quot; is enabled 16 mixed component width, 32 bits per texel (4-byte render target component alignment) rgb10a2uint &quot;uint&quot; ✓ If &quot;core-features-and-limits&quot; is enabled If &quot;texture-formats-tier1&quot; is enabled 4 8 rgb10a2unorm &quot;float&quot;, &quot;unfilterable-float&quot; ✓ ✓ ✓ ✓ If &quot;texture-formats-tier1&quot; is enabled 4 8 rg11b10ufloat &quot;float&quot;, &quot;unfilterable-float&quot; If &quot;rg11b10ufloat-renderable&quot; is enabled If &quot;texture-formats-tier1&quot; is enabled 4 8
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rg11b10ufloat">WebGPU: 6.3 Texture Formats</see>
    /// </remarks>
    [Description("@#rg11b10ufloat")]
    Rg11b10ufloat = 31,

    /// <summary>
    /// &quot;unfilterable-float&quot; ✓ If &quot;float32-blendable&quot; is enabled If &quot;core-features-and-limits&quot; is enabled ✓ ✓ ✓ 4 rg32uint &quot;uint&quot; ✓ If &quot;core-features-and-limits&quot; is enabled 8 rg32sint &quot;sint&quot; ✓ If &quot;core-features-and-limits&quot; is enabled 8 rg32float
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rg32uint">WebGPU: 6.3 Texture Formats</see>
    /// </remarks>
    [Description("@#rg32uint")]
    Rg32uint = 32,

    /// <summary>
    /// &quot;unfilterable-float&quot; ✓ If &quot;float32-blendable&quot; is enabled If &quot;core-features-and-limits&quot; is enabled ✓ ✓ ✓ 4 rg32uint &quot;uint&quot; ✓ If &quot;core-features-and-limits&quot; is enabled 8 rg32sint &quot;sint&quot; ✓ If &quot;core-features-and-limits&quot; is enabled 8 rg32float
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rg32sint">WebGPU: 6.3 Texture Formats</see>
    /// </remarks>
    [Description("@#rg32sint")]
    Rg32sint = 33,

    /// <summary>
    /// &quot;unfilterable-float&quot; ✓ If &quot;float32-blendable&quot; is enabled If &quot;core-features-and-limits&quot; is enabled ✓ ✓ ✓ 4 rg32uint &quot;uint&quot; ✓ If &quot;core-features-and-limits&quot; is enabled 8 rg32sint &quot;sint&quot; ✓ If &quot;core-features-and-limits&quot; is enabled 8 rg32float
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rg32float">WebGPU: 6.3 Texture Formats</see>
    /// </remarks>
    [Description("@#rg32float")]
    Rg32float = 34,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rgba16unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#rgba16unorm")]
    Rgba16unorm = 35,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rgba16snorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#rgba16snorm")]
    Rgba16snorm = 36,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rgba16uint">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#rgba16uint")]
    Rgba16uint = 37,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rgba16sint">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#rgba16sint")]
    Rgba16sint = 38,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rgba16float">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#rgba16float")]
    Rgba16float = 39,

    /// <summary>
    /// &quot;unfilterable-float&quot; ✓ If &quot;float32-blendable&quot; is enabled If &quot;core-features-and-limits&quot; is enabled 8 rgba32uint &quot;uint&quot; ✓ ✓ ✓ If &quot;texture-formats-tier2&quot; is enabled 16 rgba32sint &quot;sint&quot; ✓ ✓ ✓ If &quot;texture-formats-tier2&quot; is enabled 16 rgba32float
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rgba32uint">WebGPU: 6.3 Texture Formats</see>
    /// </remarks>
    [Description("@#rgba32uint")]
    Rgba32uint = 40,

    /// <summary>
    /// &quot;unfilterable-float&quot; ✓ If &quot;float32-blendable&quot; is enabled If &quot;core-features-and-limits&quot; is enabled 8 rgba32uint &quot;uint&quot; ✓ ✓ ✓ If &quot;texture-formats-tier2&quot; is enabled 16 rgba32sint &quot;sint&quot; ✓ ✓ ✓ If &quot;texture-formats-tier2&quot; is enabled 16 rgba32float
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rgba32sint">WebGPU: 6.3 Texture Formats</see>
    /// </remarks>
    [Description("@#rgba32sint")]
    Rgba32sint = 41,

    /// <summary>
    /// &quot;unfilterable-float&quot; ✓ If &quot;float32-blendable&quot; is enabled If &quot;core-features-and-limits&quot; is enabled 8 rgba32uint &quot;uint&quot; ✓ ✓ ✓ If &quot;texture-formats-tier2&quot; is enabled 16 rgba32sint &quot;sint&quot; ✓ ✓ ✓ If &quot;texture-formats-tier2&quot; is enabled 16 rgba32float
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-rgba32float">WebGPU: 6.3 Texture Formats</see>
    /// </remarks>
    [Description("@#rgba32float")]
    Rgba32float = 42,

    /// <summary>
    /// The depth component of the &quot;depth24plus&quot; and &quot;depth24plus-stencil8&quot; formats may be implemented as either a 24-bit depth value or a &quot;depth32float&quot; value.
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-stencil8">WebGPU: 6.3 Texture Formats</see>
    /// </remarks>
    [Description("@#stencil8")]
    Stencil8 = 43,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-depth16unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#depth16unorm")]
    Depth16unorm = 44,

    /// <summary>
    /// The depth component of the &quot;depth24plus&quot; and &quot;depth24plus-stencil8&quot; formats may be implemented as either a 24-bit depth value or a &quot;depth32float&quot; value.
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-depth24plus">WebGPU: 6.3 Texture Formats</see>
    /// </remarks>
    [Description("@#depth24plus")]
    Depth24plus = 45,

    /// <summary>
    /// The depth component of the &quot;depth24plus&quot; and &quot;depth24plus-stencil8&quot; formats may be implemented as either a 24-bit depth value or a &quot;depth32float&quot; value.
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-depth24plus-stencil8">WebGPU: 6.3 Texture Formats</see>
    /// </remarks>
    [Description("@#depth24plus-stencil8")]
    Depth24plusStencil8 = 46,

    /// <summary>
    /// The depth component of the &quot;depth24plus&quot; and &quot;depth24plus-stencil8&quot; formats may be implemented as either a 24-bit depth value or a &quot;depth32float&quot; value.
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-depth32float">WebGPU: 6.3 Texture Formats</see>
    /// </remarks>
    [Description("@#depth32float")]
    Depth32float = 47,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-depth32float-stencil8">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#depth32float-stencil8")]
    Depth32floatStencil8 = 48,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-bc1-rgba-unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#bc1-rgba-unorm")]
    Bc1RgbaUnorm = 49,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-bc1-rgba-unorm-srgb">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#bc1-rgba-unorm-srgb")]
    Bc1RgbaUnormSrgb = 50,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-bc2-rgba-unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#bc2-rgba-unorm")]
    Bc2RgbaUnorm = 51,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-bc2-rgba-unorm-srgb">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#bc2-rgba-unorm-srgb")]
    Bc2RgbaUnormSrgb = 52,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-bc3-rgba-unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#bc3-rgba-unorm")]
    Bc3RgbaUnorm = 53,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-bc3-rgba-unorm-srgb">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#bc3-rgba-unorm-srgb")]
    Bc3RgbaUnormSrgb = 54,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-bc4-r-unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#bc4-r-unorm")]
    Bc4RUnorm = 55,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-bc4-r-snorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#bc4-r-snorm")]
    Bc4RSnorm = 56,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-bc5-rg-unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#bc5-rg-unorm")]
    Bc5RgUnorm = 57,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-bc5-rg-snorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#bc5-rg-snorm")]
    Bc5RgSnorm = 58,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-bc6h-rgb-ufloat">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#bc6h-rgb-ufloat")]
    Bc6hRgbUfloat = 59,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-bc6h-rgb-float">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#bc6h-rgb-float")]
    Bc6hRgbFloat = 60,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-bc7-rgba-unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#bc7-rgba-unorm")]
    Bc7RgbaUnorm = 61,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-bc7-rgba-unorm-srgb">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#bc7-rgba-unorm-srgb")]
    Bc7RgbaUnormSrgb = 62,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-etc2-rgb8unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#etc2-rgb8unorm")]
    Etc2Rgb8unorm = 63,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-etc2-rgb8unorm-srgb">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#etc2-rgb8unorm-srgb")]
    Etc2Rgb8unormSrgb = 64,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-etc2-rgb8a1unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#etc2-rgb8a1unorm")]
    Etc2Rgb8a1unorm = 65,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-etc2-rgb8a1unorm-srgb">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#etc2-rgb8a1unorm-srgb")]
    Etc2Rgb8a1unormSrgb = 66,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-etc2-rgba8unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#etc2-rgba8unorm")]
    Etc2Rgba8unorm = 67,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-etc2-rgba8unorm-srgb">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#etc2-rgba8unorm-srgb")]
    Etc2Rgba8unormSrgb = 68,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-eac-r11unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#eac-r11unorm")]
    EacR11unorm = 69,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-eac-r11snorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#eac-r11snorm")]
    EacR11snorm = 70,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-eac-rg11unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#eac-rg11unorm")]
    EacRg11unorm = 71,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-eac-rg11snorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#eac-rg11snorm")]
    EacRg11snorm = 72,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-4x4-unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-4x4-unorm")]
    Astc4x4Unorm = 73,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-4x4-unorm-srgb">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-4x4-unorm-srgb")]
    Astc4x4UnormSrgb = 74,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-5x4-unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-5x4-unorm")]
    Astc5x4Unorm = 75,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-5x4-unorm-srgb">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-5x4-unorm-srgb")]
    Astc5x4UnormSrgb = 76,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-5x5-unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-5x5-unorm")]
    Astc5x5Unorm = 77,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-5x5-unorm-srgb">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-5x5-unorm-srgb")]
    Astc5x5UnormSrgb = 78,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-6x5-unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-6x5-unorm")]
    Astc6x5Unorm = 79,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-6x5-unorm-srgb">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-6x5-unorm-srgb")]
    Astc6x5UnormSrgb = 80,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-6x6-unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-6x6-unorm")]
    Astc6x6Unorm = 81,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-6x6-unorm-srgb">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-6x6-unorm-srgb")]
    Astc6x6UnormSrgb = 82,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-8x5-unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-8x5-unorm")]
    Astc8x5Unorm = 83,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-8x5-unorm-srgb">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-8x5-unorm-srgb")]
    Astc8x5UnormSrgb = 84,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-8x6-unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-8x6-unorm")]
    Astc8x6Unorm = 85,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-8x6-unorm-srgb">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-8x6-unorm-srgb")]
    Astc8x6UnormSrgb = 86,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-8x8-unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-8x8-unorm")]
    Astc8x8Unorm = 87,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-8x8-unorm-srgb">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-8x8-unorm-srgb")]
    Astc8x8UnormSrgb = 88,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-10x5-unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-10x5-unorm")]
    Astc10x5Unorm = 89,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-10x5-unorm-srgb">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-10x5-unorm-srgb")]
    Astc10x5UnormSrgb = 90,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-10x6-unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-10x6-unorm")]
    Astc10x6Unorm = 91,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-10x6-unorm-srgb">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-10x6-unorm-srgb")]
    Astc10x6UnormSrgb = 92,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-10x8-unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-10x8-unorm")]
    Astc10x8Unorm = 93,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-10x8-unorm-srgb">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-10x8-unorm-srgb")]
    Astc10x8UnormSrgb = 94,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-10x10-unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-10x10-unorm")]
    Astc10x10Unorm = 95,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-10x10-unorm-srgb">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-10x10-unorm-srgb")]
    Astc10x10UnormSrgb = 96,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-12x10-unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-12x10-unorm")]
    Astc12x10Unorm = 97,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-12x10-unorm-srgb">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-12x10-unorm-srgb")]
    Astc12x10UnormSrgb = 98,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-12x12-unorm">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-12x12-unorm")]
    Astc12x12Unorm = 99,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureformat-astc-12x12-unorm-srgb">WebGPU: 6.3 Texture Formats</see>
    /// </summary>
    [Description("@#astc-12x12-unorm-srgb")]
    Astc12x12UnormSrgb = 100
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gputexturesampletype">WebGPU: 8.1.1 Bind Group Layout Creation</see>
/// </summary>
[Description("@#GPUTextureSampleType")]
[ECMAScript]
[String]
public enum GPUTextureSampleType
{
    /// <summary>
    /// &quot;float&quot; if &quot;float32-filterable&quot; is enabled
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputexturesampletype-float">WebGPU: 8.1.1 Bind Group Layout Creation</see>
    /// </remarks>
    [Description("@#float")]
    Float = 0,

    /// <summary>
    /// &quot;unfilterable-float&quot; ✓ If &quot;float32-blendable&quot; is enabled If &quot;core-features-and-limits&quot; is enabled ✓ ✓ ✓ 4 rg32uint &quot;uint&quot; ✓ If &quot;core-features-and-limits&quot; is enabled 8 rg32sint &quot;sint&quot; ✓ If &quot;core-features-and-limits&quot; is enabled 8 rg32float
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputexturesampletype-unfilterable-float">WebGPU: 8.1.1 Bind Group Layout Creation</see>
    /// </remarks>
    [Description("@#unfilterable-float")]
    UnfilterableFloat = 1,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputexturesampletype-depth">WebGPU: 8.1.1 Bind Group Layout Creation</see>
    /// </summary>
    [Description("@#depth")]
    Depth = 2,

    /// <summary>
    /// &quot;unfilterable-float&quot; ✓ If &quot;float32-blendable&quot; is enabled If &quot;core-features-and-limits&quot; is enabled ✓ ✓ ✓ 4 rg32uint &quot;uint&quot; ✓ If &quot;core-features-and-limits&quot; is enabled 8 rg32sint &quot;sint&quot; ✓ If &quot;core-features-and-limits&quot; is enabled 8 rg32float
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputexturesampletype-sint">WebGPU: 8.1.1 Bind Group Layout Creation</see>
    /// </remarks>
    [Description("@#sint")]
    Sint = 3,

    /// <summary>
    /// &quot;unfilterable-float&quot; ✓ If &quot;float32-blendable&quot; is enabled If &quot;core-features-and-limits&quot; is enabled ✓ ✓ ✓ 4 rg32uint &quot;uint&quot; ✓ If &quot;core-features-and-limits&quot; is enabled 8 rg32sint &quot;sint&quot; ✓ If &quot;core-features-and-limits&quot; is enabled 8 rg32float
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputexturesampletype-uint">WebGPU: 8.1.1 Bind Group Layout Creation</see>
    /// </remarks>
    [Description("@#uint")]
    Uint = 4
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gputextureviewdimension">WebGPU: 6.2.1 Texture View Creation</see>
/// </summary>
[Description("@#GPUTextureViewDimension")]
[ECMAScript]
[String]
public enum GPUTextureViewDimension
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureviewdimension-1d">WebGPU: 6.2.1 Texture View Creation</see>
    /// </summary>
    [Description("@#1d")]
    _1d = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureviewdimension-2d">WebGPU: 6.2.1 Texture View Creation</see>
    /// </summary>
    [Description("@#2d")]
    _2d = 1,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureviewdimension-2d-array">WebGPU: 6.2.1 Texture View Creation</see>
    /// </summary>
    [Description("@#2d-array")]
    _2dArray = 2,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureviewdimension-cube">WebGPU: 6.2.1 Texture View Creation</see>
    /// </summary>
    [Description("@#cube")]
    Cube = 3,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureviewdimension-cube-array">WebGPU: 6.2.1 Texture View Creation</see>
    /// </summary>
    [Description("@#cube-array")]
    CubeArray = 4,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureviewdimension-3d">WebGPU: 6.2.1 Texture View Creation</see>
    /// </summary>
    [Description("@#3d")]
    _3d = 5
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpuvertexformat">WebGPU: 10.3.7.1 Vertex Formats</see>
/// </summary>
[Description("@#GPUVertexFormat")]
[ECMAScript]
[String]
public enum GPUVertexFormat
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-uint8">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#uint8")]
    Uint8 = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-uint8x2">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#uint8x2")]
    Uint8x2 = 1,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-uint8x4">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#uint8x4")]
    Uint8x4 = 2,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-sint8">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#sint8")]
    Sint8 = 3,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-sint8x2">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#sint8x2")]
    Sint8x2 = 4,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-sint8x4">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#sint8x4")]
    Sint8x4 = 5,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-unorm8">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#unorm8")]
    Unorm8 = 6,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-unorm8x2">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#unorm8x2")]
    Unorm8x2 = 7,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-unorm8x4">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#unorm8x4")]
    Unorm8x4 = 8,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-snorm8">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#snorm8")]
    Snorm8 = 9,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-snorm8x2">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#snorm8x2")]
    Snorm8x2 = 10,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-snorm8x4">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#snorm8x4")]
    Snorm8x4 = 11,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-uint16">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#uint16")]
    Uint16 = 12,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-uint16x2">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#uint16x2")]
    Uint16x2 = 13,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-uint16x4">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#uint16x4")]
    Uint16x4 = 14,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-sint16">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#sint16")]
    Sint16 = 15,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-sint16x2">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#sint16x2")]
    Sint16x2 = 16,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-sint16x4">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#sint16x4")]
    Sint16x4 = 17,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-unorm16">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#unorm16")]
    Unorm16 = 18,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-unorm16x2">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#unorm16x2")]
    Unorm16x2 = 19,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-unorm16x4">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#unorm16x4")]
    Unorm16x4 = 20,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-snorm16">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#snorm16")]
    Snorm16 = 21,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-snorm16x2">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#snorm16x2")]
    Snorm16x2 = 22,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-snorm16x4">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#snorm16x4")]
    Snorm16x4 = 23,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-float16">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#float16")]
    Float16 = 24,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-float16x2">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#float16x2")]
    Float16x2 = 25,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-float16x4">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#float16x4")]
    Float16x4 = 26,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-float32">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#float32")]
    Float32 = 27,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-float32x2">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#float32x2")]
    Float32x2 = 28,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-float32x3">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#float32x3")]
    Float32x3 = 29,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-float32x4">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#float32x4")]
    Float32x4 = 30,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-uint32">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#uint32")]
    Uint32 = 31,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-uint32x2">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#uint32x2")]
    Uint32x2 = 32,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-uint32x3">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#uint32x3")]
    Uint32x3 = 33,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-uint32x4">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#uint32x4")]
    Uint32x4 = 34,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-sint32">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#sint32")]
    Sint32 = 35,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-sint32x2">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#sint32x2")]
    Sint32x2 = 36,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-sint32x3">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#sint32x3")]
    Sint32x3 = 37,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-sint32x4">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#sint32x4")]
    Sint32x4 = 38,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-unorm10-10-10-2">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#unorm10-10-10-2")]
    Unorm1010102 = 39,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexformat-unorm8x4-bgra">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#unorm8x4-bgra")]
    Unorm8x4Bgra = 40
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#enumdef-gpuvertexstepmode">WebGPU: 10.3.7.1 Vertex Formats</see>
/// </summary>
[Description("@#GPUVertexStepMode")]
[ECMAScript]
[String]
public enum GPUVertexStepMode
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexstepmode-vertex">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#vertex")]
    Vertex = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexstepmode-instance">WebGPU: 10.3.7.1 Vertex Formats</see>
    /// </summary>
    [Description("@#instance")]
    Instance = 1
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpucanvasalphamode">WebGPU: 21.6 GPUCanvasAlphaMode</see>
/// </summary>
[Description("@#GPUCanvasAlphaMode")]
[ECMAScript]
[String]
public enum GPUCanvasAlphaMode
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvasalphamode-opaque">WebGPU: 21.6 GPUCanvasAlphaMode</see>
    /// </summary>
    [Description("@#opaque")]
    Opaque = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvasalphamode-premultiplied">WebGPU: 21.6 GPUCanvasAlphaMode</see>
    /// </summary>
    [Description("@#premultiplied")]
    Premultiplied = 1
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpucanvastonemappingmode">WebGPU: 21.5 GPUCanvasToneMappingMode</see>
/// </summary>
[Description("@#GPUCanvasToneMappingMode")]
[ECMAScript]
[String]
public enum GPUCanvasToneMappingMode
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvastonemappingmode-standard">WebGPU: 21.5 GPUCanvasToneMappingMode</see>
    /// </summary>
    [Description("@#standard")]
    Standard = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvastonemappingmode-extended">WebGPU: 21.5 GPUCanvasToneMappingMode</see>
    /// </summary>
    [Description("@#extended")]
    Extended = 1
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpufeaturename">WebGPU: 4.3.1.1 GPUFeatureName</see>
/// </summary>
[Description("@#GPUFeatureName")]
[ECMAScript]
[String]
public enum GPUFeatureName
{
    /// <summary>
    /// &quot;unfilterable-float&quot; ✓ If &quot;float32-blendable&quot; is enabled If &quot;core-features-and-limits&quot; is enabled ✓ ✓ ✓ 4 rg32uint &quot;uint&quot; ✓ If &quot;core-features-and-limits&quot; is enabled 8 rg32sint &quot;sint&quot; ✓ If &quot;core-features-and-limits&quot; is enabled 8 rg32float
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#core-features-and-limits">WebGPU: 25.1 &quot;core-features-and-limits&quot;</see>
    /// </remarks>
    [Description("@#core-features-and-limits")]
    CoreFeaturesAndLimits = 0,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#depth-clip-control">WebGPU: 25.2 &quot;depth-clip-control&quot;</see>
    /// </summary>
    [Description("@#depth-clip-control")]
    DepthClipControl = 1,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#depth32float-stencil8">WebGPU: 25.3 &quot;depth32float-stencil8&quot;</see>
    /// </summary>
    [Description("@#depth32float-stencil8")]
    Depth32floatStencil8 = 2,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#texture-compression-bc">WebGPU: 25.4 &quot;texture-compression-bc&quot;</see>
    /// </summary>
    [Description("@#texture-compression-bc")]
    TextureCompressionBc = 3,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#texture-compression-bc-sliced-3d">WebGPU: 25.5 &quot;texture-compression-bc-sliced-3d&quot;</see>
    /// </summary>
    [Description("@#texture-compression-bc-sliced-3d")]
    TextureCompressionBcSliced3d = 4,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#texture-compression-etc2">WebGPU: 25.6 &quot;texture-compression-etc2&quot;</see>
    /// </summary>
    [Description("@#texture-compression-etc2")]
    TextureCompressionEtc2 = 5,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#texture-compression-astc">WebGPU: 25.7 &quot;texture-compression-astc&quot;</see>
    /// </summary>
    [Description("@#texture-compression-astc")]
    TextureCompressionAstc = 6,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#texture-compression-astc-sliced-3d">WebGPU: 25.8 &quot;texture-compression-astc-sliced-3d&quot;</see>
    /// </summary>
    [Description("@#texture-compression-astc-sliced-3d")]
    TextureCompressionAstcSliced3d = 7,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#timestamp-query">WebGPU: 25.9 &quot;timestamp-query&quot;</see>
    /// </summary>
    [Description("@#timestamp-query")]
    TimestampQuery = 8,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#indirect-first-instance">WebGPU: 25.10 &quot;indirect-first-instance&quot;</see>
    /// </summary>
    [Description("@#indirect-first-instance")]
    IndirectFirstInstance = 9,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#shader-f16">WebGPU: 25.11 &quot;shader-f16&quot;</see>
    /// </summary>
    [Description("@#shader-f16")]
    ShaderF16 = 10,

    /// <summary>
    /// &quot;unfilterable-float&quot; ✓ If &quot;float32-blendable&quot; is enabled ✓ ✓ If &quot;texture-formats-tier2&quot; is enabled 16 mixed component width, 32 bits per texel (4-byte render target component alignment) rgb10a2uint &quot;uint&quot; ✓ If &quot;core-features-and-limits&quot; is enabled If &quot;texture-formats-tier1&quot; is enabled 4 8 rgb10a2unorm &quot;float&quot;, &quot;unfilterable-float&quot; ✓ ✓ ✓ ✓ If &quot;texture-formats-tier1&quot; is enabled 4 8 rg11b10ufloat &quot;float&quot;, &quot;unfilterable-float&quot; If &quot;rg11b10ufloat-renderable&quot; is enabled If &quot;texture-formats-tier1&quot; is enabled 4 8
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#rg11b10ufloat-renderable">WebGPU: 25.12 &quot;rg11b10ufloat-renderable&quot;</see>
    /// </remarks>
    [Description("@#rg11b10ufloat-renderable")]
    Rg11b10ufloatRenderable = 11,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#bgra8unorm-storage">WebGPU: 25.13 &quot;bgra8unorm-storage&quot;</see>
    /// </summary>
    [Description("@#bgra8unorm-storage")]
    Bgra8unormStorage = 12,

    /// <summary>
    /// &quot;float&quot; if &quot;float32-filterable&quot; is enabled
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#float32-filterable">WebGPU: 25.14 &quot;float32-filterable&quot;</see>
    /// </remarks>
    [Description("@#float32-filterable")]
    Float32Filterable = 13,

    /// <summary>
    /// &quot;unfilterable-float&quot; ✓ If &quot;float32-blendable&quot; is enabled If &quot;core-features-and-limits&quot; is enabled ✓ ✓ ✓ 4 rg32uint &quot;uint&quot; ✓ If &quot;core-features-and-limits&quot; is enabled 8 rg32sint &quot;sint&quot; ✓ If &quot;core-features-and-limits&quot; is enabled 8 rg32float
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#float32-blendable">WebGPU: 25.15 &quot;float32-blendable&quot;</see>
    /// </remarks>
    [Description("@#float32-blendable")]
    Float32Blendable = 14,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpufeaturename-clip-distances">WebGPU: 25.16 &quot;clip-distances&quot;</see>
    /// </summary>
    [Description("@#clip-distances")]
    ClipDistances = 15,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpufeaturename-dual-source-blending">WebGPU: 25.17 &quot;dual-source-blending&quot;</see>
    /// </summary>
    [Description("@#dual-source-blending")]
    DualSourceBlending = 16,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#subgroups">WebGPU: 25.18 &quot;subgroups&quot;</see>
    /// </summary>
    [Description("@#subgroups")]
    Subgroups = 17,

    /// <summary>
    /// &quot;unfilterable-float&quot; ✓ If &quot;float32-blendable&quot; is enabled ✓ ✓ If &quot;texture-formats-tier2&quot; is enabled 16 mixed component width, 32 bits per texel (4-byte render target component alignment) rgb10a2uint &quot;uint&quot; ✓ If &quot;core-features-and-limits&quot; is enabled If &quot;texture-formats-tier1&quot; is enabled 4 8 rgb10a2unorm &quot;float&quot;, &quot;unfilterable-float&quot; ✓ ✓ ✓ ✓ If &quot;texture-formats-tier1&quot; is enabled 4 8 rg11b10ufloat &quot;float&quot;, &quot;unfilterable-float&quot; If &quot;rg11b10ufloat-renderable&quot; is enabled If &quot;texture-formats-tier1&quot; is enabled 4 8
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#texture-formats-tier1">WebGPU: 25.19 &quot;texture-formats-tier1&quot;</see>
    /// </remarks>
    [Description("@#texture-formats-tier1")]
    TextureFormatsTier1 = 18,

    /// <summary>
    /// &quot;unfilterable-float&quot; ✓ If &quot;float32-blendable&quot; is enabled If &quot;core-features-and-limits&quot; is enabled 8 rgba32uint &quot;uint&quot; ✓ ✓ ✓ If &quot;texture-formats-tier2&quot; is enabled 16 rgba32sint &quot;sint&quot; ✓ ✓ ✓ If &quot;texture-formats-tier2&quot; is enabled 16 rgba32float
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#texture-formats-tier2">WebGPU: 25.20 &quot;texture-formats-tier2&quot;</see>
    /// </remarks>
    [Description("@#texture-formats-tier2")]
    TextureFormatsTier2 = 19,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpufeaturename-primitive-index">WebGPU: 25.21 &quot;primitive-index&quot;</see>
    /// </summary>
    [Description("@#primitive-index")]
    PrimitiveIndex = 20,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpufeaturename-texture-component-swizzle">WebGPU: 25.22 &quot;texture-component-swizzle&quot;</see>
    /// </summary>
    [Description("@#texture-component-swizzle")]
    TextureComponentSwizzle = 21,

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpufeaturename-subgroup-size-control">WebGPU: 25.23 &quot;subgroup-size-control&quot;</see>
    /// </summary>
    [Description("@#subgroup-size-control")]
    SubgroupSizeControl = 22
}
