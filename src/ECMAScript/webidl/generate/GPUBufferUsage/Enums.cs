namespace ECMAScript.GPUBufferUsage;

/// <summary>
/// GPUAddressMode
/// </summary>
[Description("@#GPUAddressMode")]
[ECMAScript]
[String]
public enum GPUAddressMode
{
    [Description("@#clamp-to-edge")]
    ClampToEdge = 0,

    [Description("@#repeat")]
    Repeat = 1,

    [Description("@#mirror-repeat")]
    MirrorRepeat = 2
}

/// <summary>
/// GPUAutoLayoutMode
/// </summary>
[Description("@#GPUAutoLayoutMode")]
[ECMAScript]
[String]
public enum GPUAutoLayoutMode
{
    [Description("@#auto")]
    Auto = 0
}

/// <summary>
/// GPUBlendFactor
/// </summary>
[Description("@#GPUBlendFactor")]
[ECMAScript]
[String]
public enum GPUBlendFactor
{
    [Description("@#zero")]
    Zero = 0,

    [Description("@#one")]
    One = 1,

    [Description("@#src")]
    Src = 2,

    [Description("@#one-minus-src")]
    OneMinusSrc = 3,

    [Description("@#src-alpha")]
    SrcAlpha = 4,

    [Description("@#one-minus-src-alpha")]
    OneMinusSrcAlpha = 5,

    [Description("@#dst")]
    Dst = 6,

    [Description("@#one-minus-dst")]
    OneMinusDst = 7,

    [Description("@#dst-alpha")]
    DstAlpha = 8,

    [Description("@#one-minus-dst-alpha")]
    OneMinusDstAlpha = 9,

    [Description("@#src-alpha-saturated")]
    SrcAlphaSaturated = 10,

    [Description("@#constant")]
    Constant = 11,

    [Description("@#one-minus-constant")]
    OneMinusConstant = 12
}

/// <summary>
/// GPUBlendOperation
/// </summary>
[Description("@#GPUBlendOperation")]
[ECMAScript]
[String]
public enum GPUBlendOperation
{
    [Description("@#add")]
    Add = 0,

    [Description("@#subtract")]
    Subtract = 1,

    [Description("@#reverse-subtract")]
    ReverseSubtract = 2,

    [Description("@#min")]
    Min = 3,

    [Description("@#max")]
    Max = 4
}

/// <summary>
/// GPUBufferBindingType
/// </summary>
[Description("@#GPUBufferBindingType")]
[ECMAScript]
[String]
public enum GPUBufferBindingType
{
    [Description("@#uniform")]
    Uniform = 0,

    [Description("@#storage")]
    Storage = 1,

    [Description("@#read-only-storage")]
    ReadOnlyStorage = 2
}

/// <summary>
/// GPUBufferMapState
/// </summary>
[Description("@#GPUBufferMapState")]
[ECMAScript]
[String]
public enum GPUBufferMapState
{
    [Description("@#unmapped")]
    Unmapped = 0,

    [Description("@#pending")]
    Pending = 1,

    [Description("@#mapped")]
    Mapped = 2
}

/// <summary>
/// GPUCanvasAlphaMode
/// </summary>
[Description("@#GPUCanvasAlphaMode")]
[ECMAScript]
[String]
public enum GPUCanvasAlphaMode
{
    [Description("@#opaque")]
    Opaque = 0,

    [Description("@#premultiplied")]
    Premultiplied = 1
}

/// <summary>
/// GPUCompareFunction
/// </summary>
[Description("@#GPUCompareFunction")]
[ECMAScript]
[String]
public enum GPUCompareFunction
{
    [Description("@#never")]
    Never = 0,

    [Description("@#less")]
    Less = 1,

    [Description("@#equal")]
    Equal = 2,

    [Description("@#less-equal")]
    LessEqual = 3,

    [Description("@#greater")]
    Greater = 4,

    [Description("@#not-equal")]
    NotEqual = 5,

    [Description("@#greater-equal")]
    GreaterEqual = 6,

    [Description("@#always")]
    Always = 7
}

/// <summary>
/// GPUCompilationMessageType
/// </summary>
[Description("@#GPUCompilationMessageType")]
[ECMAScript]
[String]
public enum GPUCompilationMessageType
{
    [Description("@#error")]
    Error = 0,

    [Description("@#warning")]
    Warning = 1,

    [Description("@#info")]
    Info = 2
}

/// <summary>
/// GPUCullMode
/// </summary>
[Description("@#GPUCullMode")]
[ECMAScript]
[String]
public enum GPUCullMode
{
    [Description("@#none")]
    None = 0,

    [Description("@#front")]
    Front = 1,

    [Description("@#back")]
    Back = 2
}

/// <summary>
/// GPUDeviceLostReason
/// </summary>
[Description("@#GPUDeviceLostReason")]
[ECMAScript]
[String]
public enum GPUDeviceLostReason
{
    [Description("@#unknown")]
    Unknown = 0,

    [Description("@#destroyed")]
    Destroyed = 1
}

/// <summary>
/// GPUErrorFilter
/// </summary>
[Description("@#GPUErrorFilter")]
[ECMAScript]
[String]
public enum GPUErrorFilter
{
    [Description("@#validation")]
    Validation = 0,

    [Description("@#out-of-memory")]
    OutOfMemory = 1,

    [Description("@#internal")]
    Internal = 2
}

/// <summary>
/// GPUFeatureName
/// </summary>
[Description("@#GPUFeatureName")]
[ECMAScript]
[String]
public enum GPUFeatureName
{
    [Description("@#depth-clip-control")]
    DepthClipControl = 0,

    [Description("@#depth32float-stencil8")]
    Depth32floatStencil8 = 1,

    [Description("@#texture-compression-bc")]
    TextureCompressionBc = 2,

    [Description("@#texture-compression-etc2")]
    TextureCompressionEtc2 = 3,

    [Description("@#texture-compression-astc")]
    TextureCompressionAstc = 4,

    [Description("@#timestamp-query")]
    TimestampQuery = 5,

    [Description("@#indirect-first-instance")]
    IndirectFirstInstance = 6,

    [Description("@#shader-f16")]
    ShaderF16 = 7,

    [Description("@#rg11b10ufloat-renderable")]
    Rg11b10ufloatRenderable = 8,

    [Description("@#bgra8unorm-storage")]
    Bgra8unormStorage = 9,

    [Description("@#float32-filterable")]
    Float32Filterable = 10
}

/// <summary>
/// GPUFilterMode
/// </summary>
[Description("@#GPUFilterMode")]
[ECMAScript]
[String]
public enum GPUFilterMode
{
    [Description("@#nearest")]
    Nearest = 0,

    [Description("@#linear")]
    Linear = 1
}

/// <summary>
/// GPUFrontFace
/// </summary>
[Description("@#GPUFrontFace")]
[ECMAScript]
[String]
public enum GPUFrontFace
{
    [Description("@#ccw")]
    Ccw = 0,

    [Description("@#cw")]
    Cw = 1
}

/// <summary>
/// GPUIndexFormat
/// </summary>
[Description("@#GPUIndexFormat")]
[ECMAScript]
[String]
public enum GPUIndexFormat
{
    [Description("@#uint16")]
    Uint16 = 0,

    [Description("@#uint32")]
    Uint32 = 1
}

/// <summary>
/// GPULoadOp
/// </summary>
[Description("@#GPULoadOp")]
[ECMAScript]
[String]
public enum GPULoadOp
{
    [Description("@#load")]
    Load = 0,

    [Description("@#clear")]
    Clear = 1
}

/// <summary>
/// GPUMipmapFilterMode
/// </summary>
[Description("@#GPUMipmapFilterMode")]
[ECMAScript]
[String]
public enum GPUMipmapFilterMode
{
    [Description("@#nearest")]
    Nearest = 0,

    [Description("@#linear")]
    Linear = 1
}

/// <summary>
/// GPUPipelineErrorReason
/// </summary>
[Description("@#GPUPipelineErrorReason")]
[ECMAScript]
[String]
public enum GPUPipelineErrorReason
{
    [Description("@#validation")]
    Validation = 0,

    [Description("@#internal")]
    Internal = 1
}

/// <summary>
/// GPUPowerPreference
/// </summary>
[Description("@#GPUPowerPreference")]
[ECMAScript]
[String]
public enum GPUPowerPreference
{
    [Description("@#low-power")]
    LowPower = 0,

    [Description("@#high-performance")]
    HighPerformance = 1
}

/// <summary>
/// GPUPrimitiveTopology
/// </summary>
[Description("@#GPUPrimitiveTopology")]
[ECMAScript]
[String]
public enum GPUPrimitiveTopology
{
    [Description("@#point-list")]
    PointList = 0,

    [Description("@#line-list")]
    LineList = 1,

    [Description("@#line-strip")]
    LineStrip = 2,

    [Description("@#triangle-list")]
    TriangleList = 3,

    [Description("@#triangle-strip")]
    TriangleStrip = 4
}

/// <summary>
/// GPUQueryType
/// </summary>
[Description("@#GPUQueryType")]
[ECMAScript]
[String]
public enum GPUQueryType
{
    [Description("@#occlusion")]
    Occlusion = 0,

    [Description("@#timestamp")]
    Timestamp = 1
}

/// <summary>
/// GPUSamplerBindingType
/// </summary>
[Description("@#GPUSamplerBindingType")]
[ECMAScript]
[String]
public enum GPUSamplerBindingType
{
    [Description("@#filtering")]
    Filtering = 0,

    [Description("@#non-filtering")]
    NonFiltering = 1,

    [Description("@#comparison")]
    Comparison = 2
}

/// <summary>
/// GPUStencilOperation
/// </summary>
[Description("@#GPUStencilOperation")]
[ECMAScript]
[String]
public enum GPUStencilOperation
{
    [Description("@#keep")]
    Keep = 0,

    [Description("@#zero")]
    Zero = 1,

    [Description("@#replace")]
    Replace = 2,

    [Description("@#invert")]
    Invert = 3,

    [Description("@#increment-clamp")]
    IncrementClamp = 4,

    [Description("@#decrement-clamp")]
    DecrementClamp = 5,

    [Description("@#increment-wrap")]
    IncrementWrap = 6,

    [Description("@#decrement-wrap")]
    DecrementWrap = 7
}

/// <summary>
/// GPUStorageTextureAccess
/// </summary>
[Description("@#GPUStorageTextureAccess")]
[ECMAScript]
[String]
public enum GPUStorageTextureAccess
{
    [Description("@#write-only")]
    WriteOnly = 0,

    [Description("@#read-only")]
    ReadOnly = 1,

    [Description("@#read-write")]
    ReadWrite = 2
}

/// <summary>
/// GPUStoreOp
/// </summary>
[Description("@#GPUStoreOp")]
[ECMAScript]
[String]
public enum GPUStoreOp
{
    [Description("@#store")]
    Store = 0,

    [Description("@#discard")]
    Discard = 1
}

/// <summary>
/// GPUTextureAspect
/// </summary>
[Description("@#GPUTextureAspect")]
[ECMAScript]
[String]
public enum GPUTextureAspect
{
    [Description("@#all")]
    All = 0,

    [Description("@#stencil-only")]
    StencilOnly = 1,

    [Description("@#depth-only")]
    DepthOnly = 2
}

/// <summary>
/// GPUTextureDimension
/// </summary>
[Description("@#GPUTextureDimension")]
[ECMAScript]
[String]
public enum GPUTextureDimension
{
    [Description("@#1d")]
    _1d = 0,

    [Description("@#2d")]
    _2d = 1,

    [Description("@#3d")]
    _3d = 2
}

/// <summary>
/// GPUTextureFormat
/// </summary>
[Description("@#GPUTextureFormat")]
[ECMAScript]
[String]
public enum GPUTextureFormat
{
    [Description("@#r8unorm")]
    R8unorm = 0,

    [Description("@#r8snorm")]
    R8snorm = 1,

    [Description("@#r8uint")]
    R8uint = 2,

    [Description("@#r8sint")]
    R8sint = 3,

    [Description("@#r16uint")]
    R16uint = 4,

    [Description("@#r16sint")]
    R16sint = 5,

    [Description("@#r16float")]
    R16float = 6,

    [Description("@#rg8unorm")]
    Rg8unorm = 7,

    [Description("@#rg8snorm")]
    Rg8snorm = 8,

    [Description("@#rg8uint")]
    Rg8uint = 9,

    [Description("@#rg8sint")]
    Rg8sint = 10,

    [Description("@#r32uint")]
    R32uint = 11,

    [Description("@#r32sint")]
    R32sint = 12,

    [Description("@#r32float")]
    R32float = 13,

    [Description("@#rg16uint")]
    Rg16uint = 14,

    [Description("@#rg16sint")]
    Rg16sint = 15,

    [Description("@#rg16float")]
    Rg16float = 16,

    [Description("@#rgba8unorm")]
    Rgba8unorm = 17,

    [Description("@#rgba8unorm-srgb")]
    Rgba8unormSrgb = 18,

    [Description("@#rgba8snorm")]
    Rgba8snorm = 19,

    [Description("@#rgba8uint")]
    Rgba8uint = 20,

    [Description("@#rgba8sint")]
    Rgba8sint = 21,

    [Description("@#bgra8unorm")]
    Bgra8unorm = 22,

    [Description("@#bgra8unorm-srgb")]
    Bgra8unormSrgb = 23,

    [Description("@#rgb9e5ufloat")]
    Rgb9e5ufloat = 24,

    [Description("@#rgb10a2uint")]
    Rgb10a2uint = 25,

    [Description("@#rgb10a2unorm")]
    Rgb10a2unorm = 26,

    [Description("@#rg11b10ufloat")]
    Rg11b10ufloat = 27,

    [Description("@#rg32uint")]
    Rg32uint = 28,

    [Description("@#rg32sint")]
    Rg32sint = 29,

    [Description("@#rg32float")]
    Rg32float = 30,

    [Description("@#rgba16uint")]
    Rgba16uint = 31,

    [Description("@#rgba16sint")]
    Rgba16sint = 32,

    [Description("@#rgba16float")]
    Rgba16float = 33,

    [Description("@#rgba32uint")]
    Rgba32uint = 34,

    [Description("@#rgba32sint")]
    Rgba32sint = 35,

    [Description("@#rgba32float")]
    Rgba32float = 36,

    [Description("@#stencil8")]
    Stencil8 = 37,

    [Description("@#depth16unorm")]
    Depth16unorm = 38,

    [Description("@#depth24plus")]
    Depth24plus = 39,

    [Description("@#depth24plus-stencil8")]
    Depth24plusStencil8 = 40,

    [Description("@#depth32float")]
    Depth32float = 41,

    [Description("@#depth32float-stencil8")]
    Depth32floatStencil8 = 42,

    [Description("@#bc1-rgba-unorm")]
    Bc1RgbaUnorm = 43,

    [Description("@#bc1-rgba-unorm-srgb")]
    Bc1RgbaUnormSrgb = 44,

    [Description("@#bc2-rgba-unorm")]
    Bc2RgbaUnorm = 45,

    [Description("@#bc2-rgba-unorm-srgb")]
    Bc2RgbaUnormSrgb = 46,

    [Description("@#bc3-rgba-unorm")]
    Bc3RgbaUnorm = 47,

    [Description("@#bc3-rgba-unorm-srgb")]
    Bc3RgbaUnormSrgb = 48,

    [Description("@#bc4-r-unorm")]
    Bc4RUnorm = 49,

    [Description("@#bc4-r-snorm")]
    Bc4RSnorm = 50,

    [Description("@#bc5-rg-unorm")]
    Bc5RgUnorm = 51,

    [Description("@#bc5-rg-snorm")]
    Bc5RgSnorm = 52,

    [Description("@#bc6h-rgb-ufloat")]
    Bc6hRgbUfloat = 53,

    [Description("@#bc6h-rgb-float")]
    Bc6hRgbFloat = 54,

    [Description("@#bc7-rgba-unorm")]
    Bc7RgbaUnorm = 55,

    [Description("@#bc7-rgba-unorm-srgb")]
    Bc7RgbaUnormSrgb = 56,

    [Description("@#etc2-rgb8unorm")]
    Etc2Rgb8unorm = 57,

    [Description("@#etc2-rgb8unorm-srgb")]
    Etc2Rgb8unormSrgb = 58,

    [Description("@#etc2-rgb8a1unorm")]
    Etc2Rgb8a1unorm = 59,

    [Description("@#etc2-rgb8a1unorm-srgb")]
    Etc2Rgb8a1unormSrgb = 60,

    [Description("@#etc2-rgba8unorm")]
    Etc2Rgba8unorm = 61,

    [Description("@#etc2-rgba8unorm-srgb")]
    Etc2Rgba8unormSrgb = 62,

    [Description("@#eac-r11unorm")]
    EacR11unorm = 63,

    [Description("@#eac-r11snorm")]
    EacR11snorm = 64,

    [Description("@#eac-rg11unorm")]
    EacRg11unorm = 65,

    [Description("@#eac-rg11snorm")]
    EacRg11snorm = 66,

    [Description("@#astc-4x4-unorm")]
    Astc4x4Unorm = 67,

    [Description("@#astc-4x4-unorm-srgb")]
    Astc4x4UnormSrgb = 68,

    [Description("@#astc-5x4-unorm")]
    Astc5x4Unorm = 69,

    [Description("@#astc-5x4-unorm-srgb")]
    Astc5x4UnormSrgb = 70,

    [Description("@#astc-5x5-unorm")]
    Astc5x5Unorm = 71,

    [Description("@#astc-5x5-unorm-srgb")]
    Astc5x5UnormSrgb = 72,

    [Description("@#astc-6x5-unorm")]
    Astc6x5Unorm = 73,

    [Description("@#astc-6x5-unorm-srgb")]
    Astc6x5UnormSrgb = 74,

    [Description("@#astc-6x6-unorm")]
    Astc6x6Unorm = 75,

    [Description("@#astc-6x6-unorm-srgb")]
    Astc6x6UnormSrgb = 76,

    [Description("@#astc-8x5-unorm")]
    Astc8x5Unorm = 77,

    [Description("@#astc-8x5-unorm-srgb")]
    Astc8x5UnormSrgb = 78,

    [Description("@#astc-8x6-unorm")]
    Astc8x6Unorm = 79,

    [Description("@#astc-8x6-unorm-srgb")]
    Astc8x6UnormSrgb = 80,

    [Description("@#astc-8x8-unorm")]
    Astc8x8Unorm = 81,

    [Description("@#astc-8x8-unorm-srgb")]
    Astc8x8UnormSrgb = 82,

    [Description("@#astc-10x5-unorm")]
    Astc10x5Unorm = 83,

    [Description("@#astc-10x5-unorm-srgb")]
    Astc10x5UnormSrgb = 84,

    [Description("@#astc-10x6-unorm")]
    Astc10x6Unorm = 85,

    [Description("@#astc-10x6-unorm-srgb")]
    Astc10x6UnormSrgb = 86,

    [Description("@#astc-10x8-unorm")]
    Astc10x8Unorm = 87,

    [Description("@#astc-10x8-unorm-srgb")]
    Astc10x8UnormSrgb = 88,

    [Description("@#astc-10x10-unorm")]
    Astc10x10Unorm = 89,

    [Description("@#astc-10x10-unorm-srgb")]
    Astc10x10UnormSrgb = 90,

    [Description("@#astc-12x10-unorm")]
    Astc12x10Unorm = 91,

    [Description("@#astc-12x10-unorm-srgb")]
    Astc12x10UnormSrgb = 92,

    [Description("@#astc-12x12-unorm")]
    Astc12x12Unorm = 93,

    [Description("@#astc-12x12-unorm-srgb")]
    Astc12x12UnormSrgb = 94
}

/// <summary>
/// GPUTextureSampleType
/// </summary>
[Description("@#GPUTextureSampleType")]
[ECMAScript]
[String]
public enum GPUTextureSampleType
{
    [Description("@#float")]
    Float = 0,

    [Description("@#unfilterable-float")]
    UnfilterableFloat = 1,

    [Description("@#depth")]
    Depth = 2,

    [Description("@#sint")]
    Sint = 3,

    [Description("@#uint")]
    Uint = 4
}

/// <summary>
/// GPUTextureViewDimension
/// </summary>
[Description("@#GPUTextureViewDimension")]
[ECMAScript]
[String]
public enum GPUTextureViewDimension
{
    [Description("@#1d")]
    _1d = 0,

    [Description("@#2d")]
    _2d = 1,

    [Description("@#2d-array")]
    _2dArray = 2,

    [Description("@#cube")]
    Cube = 3,

    [Description("@#cube-array")]
    CubeArray = 4,

    [Description("@#3d")]
    _3d = 5
}

/// <summary>
/// GPUVertexFormat
/// </summary>
[Description("@#GPUVertexFormat")]
[ECMAScript]
[String]
public enum GPUVertexFormat
{
    [Description("@#uint8x2")]
    Uint8x2 = 0,

    [Description("@#uint8x4")]
    Uint8x4 = 1,

    [Description("@#sint8x2")]
    Sint8x2 = 2,

    [Description("@#sint8x4")]
    Sint8x4 = 3,

    [Description("@#unorm8x2")]
    Unorm8x2 = 4,

    [Description("@#unorm8x4")]
    Unorm8x4 = 5,

    [Description("@#snorm8x2")]
    Snorm8x2 = 6,

    [Description("@#snorm8x4")]
    Snorm8x4 = 7,

    [Description("@#uint16x2")]
    Uint16x2 = 8,

    [Description("@#uint16x4")]
    Uint16x4 = 9,

    [Description("@#sint16x2")]
    Sint16x2 = 10,

    [Description("@#sint16x4")]
    Sint16x4 = 11,

    [Description("@#unorm16x2")]
    Unorm16x2 = 12,

    [Description("@#unorm16x4")]
    Unorm16x4 = 13,

    [Description("@#snorm16x2")]
    Snorm16x2 = 14,

    [Description("@#snorm16x4")]
    Snorm16x4 = 15,

    [Description("@#float16x2")]
    Float16x2 = 16,

    [Description("@#float16x4")]
    Float16x4 = 17,

    [Description("@#float32")]
    Float32 = 18,

    [Description("@#float32x2")]
    Float32x2 = 19,

    [Description("@#float32x3")]
    Float32x3 = 20,

    [Description("@#float32x4")]
    Float32x4 = 21,

    [Description("@#uint32")]
    Uint32 = 22,

    [Description("@#uint32x2")]
    Uint32x2 = 23,

    [Description("@#uint32x3")]
    Uint32x3 = 24,

    [Description("@#uint32x4")]
    Uint32x4 = 25,

    [Description("@#sint32")]
    Sint32 = 26,

    [Description("@#sint32x2")]
    Sint32x2 = 27,

    [Description("@#sint32x3")]
    Sint32x3 = 28,

    [Description("@#sint32x4")]
    Sint32x4 = 29,

    [Description("@#unorm10-10-10-2")]
    Unorm1010102 = 30
}

/// <summary>
/// GPUVertexStepMode
/// </summary>
[Description("@#GPUVertexStepMode")]
[ECMAScript]
[String]
public enum GPUVertexStepMode
{
    [Description("@#vertex")]
    Vertex = 0,

    [Description("@#instance")]
    Instance = 1
}
