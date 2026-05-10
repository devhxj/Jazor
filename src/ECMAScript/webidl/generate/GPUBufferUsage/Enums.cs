namespace ECMAScript.GPUBufferUsage;

/// <summary>
/// GPUAddressMode
/// </summary>
[Description("@#GPUAddressMode")]
[ECMAScript]
[String]
public enum GPUAddressMode
{
    [Description("@#ClampToEdge")]
    ClampToEdge = 0,

    [Description("@#Repeat")]
    Repeat = 1,

    [Description("@#MirrorRepeat")]
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
    [Description("@#Auto")]
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
    [Description("@#Zero")]
    Zero = 0,

    [Description("@#One")]
    One = 1,

    [Description("@#Src")]
    Src = 2,

    [Description("@#OneMinusSrc")]
    OneMinusSrc = 3,

    [Description("@#SrcAlpha")]
    SrcAlpha = 4,

    [Description("@#OneMinusSrcAlpha")]
    OneMinusSrcAlpha = 5,

    [Description("@#Dst")]
    Dst = 6,

    [Description("@#OneMinusDst")]
    OneMinusDst = 7,

    [Description("@#DstAlpha")]
    DstAlpha = 8,

    [Description("@#OneMinusDstAlpha")]
    OneMinusDstAlpha = 9,

    [Description("@#SrcAlphaSaturated")]
    SrcAlphaSaturated = 10,

    [Description("@#Constant")]
    Constant = 11,

    [Description("@#OneMinusConstant")]
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
    [Description("@#Add")]
    Add = 0,

    [Description("@#Subtract")]
    Subtract = 1,

    [Description("@#ReverseSubtract")]
    ReverseSubtract = 2,

    [Description("@#Min")]
    Min = 3,

    [Description("@#Max")]
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
    [Description("@#Uniform")]
    Uniform = 0,

    [Description("@#Storage")]
    Storage = 1,

    [Description("@#ReadOnlyStorage")]
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
    [Description("@#Unmapped")]
    Unmapped = 0,

    [Description("@#Pending")]
    Pending = 1,

    [Description("@#Mapped")]
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
    [Description("@#Opaque")]
    Opaque = 0,

    [Description("@#Premultiplied")]
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
    [Description("@#Never")]
    Never = 0,

    [Description("@#Less")]
    Less = 1,

    [Description("@#Equal")]
    Equal = 2,

    [Description("@#LessEqual")]
    LessEqual = 3,

    [Description("@#Greater")]
    Greater = 4,

    [Description("@#NotEqual")]
    NotEqual = 5,

    [Description("@#GreaterEqual")]
    GreaterEqual = 6,

    [Description("@#Always")]
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
    [Description("@#Error")]
    Error = 0,

    [Description("@#Warning")]
    Warning = 1,

    [Description("@#Info")]
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
    [Description("@#None")]
    None = 0,

    [Description("@#Front")]
    Front = 1,

    [Description("@#Back")]
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
    [Description("@#Unknown")]
    Unknown = 0,

    [Description("@#Destroyed")]
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
    [Description("@#Validation")]
    Validation = 0,

    [Description("@#OutOfMemory")]
    OutOfMemory = 1,

    [Description("@#Internal")]
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
    [Description("@#DepthClipControl")]
    DepthClipControl = 0,

    [Description("@#Depth32floatStencil8")]
    Depth32floatStencil8 = 1,

    [Description("@#TextureCompressionBc")]
    TextureCompressionBc = 2,

    [Description("@#TextureCompressionEtc2")]
    TextureCompressionEtc2 = 3,

    [Description("@#TextureCompressionAstc")]
    TextureCompressionAstc = 4,

    [Description("@#TimestampQuery")]
    TimestampQuery = 5,

    [Description("@#IndirectFirstInstance")]
    IndirectFirstInstance = 6,

    [Description("@#ShaderF16")]
    ShaderF16 = 7,

    [Description("@#Rg11b10ufloatRenderable")]
    Rg11b10ufloatRenderable = 8,

    [Description("@#Bgra8unormStorage")]
    Bgra8unormStorage = 9,

    [Description("@#Float32Filterable")]
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
    [Description("@#Nearest")]
    Nearest = 0,

    [Description("@#Linear")]
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
    [Description("@#Ccw")]
    Ccw = 0,

    [Description("@#Cw")]
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
    [Description("@#Uint16")]
    Uint16 = 0,

    [Description("@#Uint32")]
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
    [Description("@#Load")]
    Load = 0,

    [Description("@#Clear")]
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
    [Description("@#Nearest")]
    Nearest = 0,

    [Description("@#Linear")]
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
    [Description("@#Validation")]
    Validation = 0,

    [Description("@#Internal")]
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
    [Description("@#LowPower")]
    LowPower = 0,

    [Description("@#HighPerformance")]
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
    [Description("@#PointList")]
    PointList = 0,

    [Description("@#LineList")]
    LineList = 1,

    [Description("@#LineStrip")]
    LineStrip = 2,

    [Description("@#TriangleList")]
    TriangleList = 3,

    [Description("@#TriangleStrip")]
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
    [Description("@#Occlusion")]
    Occlusion = 0,

    [Description("@#Timestamp")]
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
    [Description("@#Filtering")]
    Filtering = 0,

    [Description("@#NonFiltering")]
    NonFiltering = 1,

    [Description("@#Comparison")]
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
    [Description("@#Keep")]
    Keep = 0,

    [Description("@#Zero")]
    Zero = 1,

    [Description("@#Replace")]
    Replace = 2,

    [Description("@#Invert")]
    Invert = 3,

    [Description("@#IncrementClamp")]
    IncrementClamp = 4,

    [Description("@#DecrementClamp")]
    DecrementClamp = 5,

    [Description("@#IncrementWrap")]
    IncrementWrap = 6,

    [Description("@#DecrementWrap")]
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
    [Description("@#WriteOnly")]
    WriteOnly = 0,

    [Description("@#ReadOnly")]
    ReadOnly = 1,

    [Description("@#ReadWrite")]
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
    [Description("@#Store")]
    Store = 0,

    [Description("@#Discard")]
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
    [Description("@#All")]
    All = 0,

    [Description("@#StencilOnly")]
    StencilOnly = 1,

    [Description("@#DepthOnly")]
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
    [Description("@#_1d")]
    _1d = 0,

    [Description("@#_2d")]
    _2d = 1,

    [Description("@#_3d")]
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
    [Description("@#R8unorm")]
    R8unorm = 0,

    [Description("@#R8snorm")]
    R8snorm = 1,

    [Description("@#R8uint")]
    R8uint = 2,

    [Description("@#R8sint")]
    R8sint = 3,

    [Description("@#R16uint")]
    R16uint = 4,

    [Description("@#R16sint")]
    R16sint = 5,

    [Description("@#R16float")]
    R16float = 6,

    [Description("@#Rg8unorm")]
    Rg8unorm = 7,

    [Description("@#Rg8snorm")]
    Rg8snorm = 8,

    [Description("@#Rg8uint")]
    Rg8uint = 9,

    [Description("@#Rg8sint")]
    Rg8sint = 10,

    [Description("@#R32uint")]
    R32uint = 11,

    [Description("@#R32sint")]
    R32sint = 12,

    [Description("@#R32float")]
    R32float = 13,

    [Description("@#Rg16uint")]
    Rg16uint = 14,

    [Description("@#Rg16sint")]
    Rg16sint = 15,

    [Description("@#Rg16float")]
    Rg16float = 16,

    [Description("@#Rgba8unorm")]
    Rgba8unorm = 17,

    [Description("@#Rgba8unormSrgb")]
    Rgba8unormSrgb = 18,

    [Description("@#Rgba8snorm")]
    Rgba8snorm = 19,

    [Description("@#Rgba8uint")]
    Rgba8uint = 20,

    [Description("@#Rgba8sint")]
    Rgba8sint = 21,

    [Description("@#Bgra8unorm")]
    Bgra8unorm = 22,

    [Description("@#Bgra8unormSrgb")]
    Bgra8unormSrgb = 23,

    [Description("@#Rgb9e5ufloat")]
    Rgb9e5ufloat = 24,

    [Description("@#Rgb10a2uint")]
    Rgb10a2uint = 25,

    [Description("@#Rgb10a2unorm")]
    Rgb10a2unorm = 26,

    [Description("@#Rg11b10ufloat")]
    Rg11b10ufloat = 27,

    [Description("@#Rg32uint")]
    Rg32uint = 28,

    [Description("@#Rg32sint")]
    Rg32sint = 29,

    [Description("@#Rg32float")]
    Rg32float = 30,

    [Description("@#Rgba16uint")]
    Rgba16uint = 31,

    [Description("@#Rgba16sint")]
    Rgba16sint = 32,

    [Description("@#Rgba16float")]
    Rgba16float = 33,

    [Description("@#Rgba32uint")]
    Rgba32uint = 34,

    [Description("@#Rgba32sint")]
    Rgba32sint = 35,

    [Description("@#Rgba32float")]
    Rgba32float = 36,

    [Description("@#Stencil8")]
    Stencil8 = 37,

    [Description("@#Depth16unorm")]
    Depth16unorm = 38,

    [Description("@#Depth24plus")]
    Depth24plus = 39,

    [Description("@#Depth24plusStencil8")]
    Depth24plusStencil8 = 40,

    [Description("@#Depth32float")]
    Depth32float = 41,

    [Description("@#Depth32floatStencil8")]
    Depth32floatStencil8 = 42,

    [Description("@#Bc1RgbaUnorm")]
    Bc1RgbaUnorm = 43,

    [Description("@#Bc1RgbaUnormSrgb")]
    Bc1RgbaUnormSrgb = 44,

    [Description("@#Bc2RgbaUnorm")]
    Bc2RgbaUnorm = 45,

    [Description("@#Bc2RgbaUnormSrgb")]
    Bc2RgbaUnormSrgb = 46,

    [Description("@#Bc3RgbaUnorm")]
    Bc3RgbaUnorm = 47,

    [Description("@#Bc3RgbaUnormSrgb")]
    Bc3RgbaUnormSrgb = 48,

    [Description("@#Bc4RUnorm")]
    Bc4RUnorm = 49,

    [Description("@#Bc4RSnorm")]
    Bc4RSnorm = 50,

    [Description("@#Bc5RgUnorm")]
    Bc5RgUnorm = 51,

    [Description("@#Bc5RgSnorm")]
    Bc5RgSnorm = 52,

    [Description("@#Bc6hRgbUfloat")]
    Bc6hRgbUfloat = 53,

    [Description("@#Bc6hRgbFloat")]
    Bc6hRgbFloat = 54,

    [Description("@#Bc7RgbaUnorm")]
    Bc7RgbaUnorm = 55,

    [Description("@#Bc7RgbaUnormSrgb")]
    Bc7RgbaUnormSrgb = 56,

    [Description("@#Etc2Rgb8unorm")]
    Etc2Rgb8unorm = 57,

    [Description("@#Etc2Rgb8unormSrgb")]
    Etc2Rgb8unormSrgb = 58,

    [Description("@#Etc2Rgb8a1unorm")]
    Etc2Rgb8a1unorm = 59,

    [Description("@#Etc2Rgb8a1unormSrgb")]
    Etc2Rgb8a1unormSrgb = 60,

    [Description("@#Etc2Rgba8unorm")]
    Etc2Rgba8unorm = 61,

    [Description("@#Etc2Rgba8unormSrgb")]
    Etc2Rgba8unormSrgb = 62,

    [Description("@#EacR11unorm")]
    EacR11unorm = 63,

    [Description("@#EacR11snorm")]
    EacR11snorm = 64,

    [Description("@#EacRg11unorm")]
    EacRg11unorm = 65,

    [Description("@#EacRg11snorm")]
    EacRg11snorm = 66,

    [Description("@#Astc4x4Unorm")]
    Astc4x4Unorm = 67,

    [Description("@#Astc4x4UnormSrgb")]
    Astc4x4UnormSrgb = 68,

    [Description("@#Astc5x4Unorm")]
    Astc5x4Unorm = 69,

    [Description("@#Astc5x4UnormSrgb")]
    Astc5x4UnormSrgb = 70,

    [Description("@#Astc5x5Unorm")]
    Astc5x5Unorm = 71,

    [Description("@#Astc5x5UnormSrgb")]
    Astc5x5UnormSrgb = 72,

    [Description("@#Astc6x5Unorm")]
    Astc6x5Unorm = 73,

    [Description("@#Astc6x5UnormSrgb")]
    Astc6x5UnormSrgb = 74,

    [Description("@#Astc6x6Unorm")]
    Astc6x6Unorm = 75,

    [Description("@#Astc6x6UnormSrgb")]
    Astc6x6UnormSrgb = 76,

    [Description("@#Astc8x5Unorm")]
    Astc8x5Unorm = 77,

    [Description("@#Astc8x5UnormSrgb")]
    Astc8x5UnormSrgb = 78,

    [Description("@#Astc8x6Unorm")]
    Astc8x6Unorm = 79,

    [Description("@#Astc8x6UnormSrgb")]
    Astc8x6UnormSrgb = 80,

    [Description("@#Astc8x8Unorm")]
    Astc8x8Unorm = 81,

    [Description("@#Astc8x8UnormSrgb")]
    Astc8x8UnormSrgb = 82,

    [Description("@#Astc10x5Unorm")]
    Astc10x5Unorm = 83,

    [Description("@#Astc10x5UnormSrgb")]
    Astc10x5UnormSrgb = 84,

    [Description("@#Astc10x6Unorm")]
    Astc10x6Unorm = 85,

    [Description("@#Astc10x6UnormSrgb")]
    Astc10x6UnormSrgb = 86,

    [Description("@#Astc10x8Unorm")]
    Astc10x8Unorm = 87,

    [Description("@#Astc10x8UnormSrgb")]
    Astc10x8UnormSrgb = 88,

    [Description("@#Astc10x10Unorm")]
    Astc10x10Unorm = 89,

    [Description("@#Astc10x10UnormSrgb")]
    Astc10x10UnormSrgb = 90,

    [Description("@#Astc12x10Unorm")]
    Astc12x10Unorm = 91,

    [Description("@#Astc12x10UnormSrgb")]
    Astc12x10UnormSrgb = 92,

    [Description("@#Astc12x12Unorm")]
    Astc12x12Unorm = 93,

    [Description("@#Astc12x12UnormSrgb")]
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
    [Description("@#Float")]
    Float = 0,

    [Description("@#UnfilterableFloat")]
    UnfilterableFloat = 1,

    [Description("@#Depth")]
    Depth = 2,

    [Description("@#Sint")]
    Sint = 3,

    [Description("@#Uint")]
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
    [Description("@#_1d")]
    _1d = 0,

    [Description("@#_2d")]
    _2d = 1,

    [Description("@#_2dArray")]
    _2dArray = 2,

    [Description("@#Cube")]
    Cube = 3,

    [Description("@#CubeArray")]
    CubeArray = 4,

    [Description("@#_3d")]
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
    [Description("@#Uint8x2")]
    Uint8x2 = 0,

    [Description("@#Uint8x4")]
    Uint8x4 = 1,

    [Description("@#Sint8x2")]
    Sint8x2 = 2,

    [Description("@#Sint8x4")]
    Sint8x4 = 3,

    [Description("@#Unorm8x2")]
    Unorm8x2 = 4,

    [Description("@#Unorm8x4")]
    Unorm8x4 = 5,

    [Description("@#Snorm8x2")]
    Snorm8x2 = 6,

    [Description("@#Snorm8x4")]
    Snorm8x4 = 7,

    [Description("@#Uint16x2")]
    Uint16x2 = 8,

    [Description("@#Uint16x4")]
    Uint16x4 = 9,

    [Description("@#Sint16x2")]
    Sint16x2 = 10,

    [Description("@#Sint16x4")]
    Sint16x4 = 11,

    [Description("@#Unorm16x2")]
    Unorm16x2 = 12,

    [Description("@#Unorm16x4")]
    Unorm16x4 = 13,

    [Description("@#Snorm16x2")]
    Snorm16x2 = 14,

    [Description("@#Snorm16x4")]
    Snorm16x4 = 15,

    [Description("@#Float16x2")]
    Float16x2 = 16,

    [Description("@#Float16x4")]
    Float16x4 = 17,

    [Description("@#Float32")]
    Float32 = 18,

    [Description("@#Float32x2")]
    Float32x2 = 19,

    [Description("@#Float32x3")]
    Float32x3 = 20,

    [Description("@#Float32x4")]
    Float32x4 = 21,

    [Description("@#Uint32")]
    Uint32 = 22,

    [Description("@#Uint32x2")]
    Uint32x2 = 23,

    [Description("@#Uint32x3")]
    Uint32x3 = 24,

    [Description("@#Uint32x4")]
    Uint32x4 = 25,

    [Description("@#Sint32")]
    Sint32 = 26,

    [Description("@#Sint32x2")]
    Sint32x2 = 27,

    [Description("@#Sint32x3")]
    Sint32x3 = 28,

    [Description("@#Sint32x4")]
    Sint32x4 = 29,

    [Description("@#Unorm1010102")]
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
    [Description("@#Vertex")]
    Vertex = 0,

    [Description("@#Instance")]
    Instance = 1
}
