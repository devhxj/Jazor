namespace ECMAScript.GPUBufferUsage;

/// <summary>
/// GPUPowerPreference
/// </summary>
[Description("@#GPUPowerPreference")]
[ECMAScript]
public enum GPUPowerPreference
{
    [Description("@#LowPower")]
    LowPower = 0,

    [Description("@#HighPerformance")]
    HighPerformance = 1
}

/// <summary>
/// GPUFeatureName
/// </summary>
[Description("@#GPUFeatureName")]
[ECMAScript]
public enum GPUFeatureName
{
    [Description("@#CoreFeaturesAndLimits")]
    CoreFeaturesAndLimits = 0,

    [Description("@#DepthClipControl")]
    DepthClipControl = 1,

    [Description("@#Depth32floatStencil8")]
    Depth32floatStencil8 = 2,

    [Description("@#TextureCompressionBc")]
    TextureCompressionBc = 3,

    [Description("@#TextureCompressionBcSliced3d")]
    TextureCompressionBcSliced3d = 4,

    [Description("@#TextureCompressionEtc2")]
    TextureCompressionEtc2 = 5,

    [Description("@#TextureCompressionAstc")]
    TextureCompressionAstc = 6,

    [Description("@#TextureCompressionAstcSliced3d")]
    TextureCompressionAstcSliced3d = 7,

    [Description("@#TimestampQuery")]
    TimestampQuery = 8,

    [Description("@#IndirectFirstInstance")]
    IndirectFirstInstance = 9,

    [Description("@#ShaderF16")]
    ShaderF16 = 10,

    [Description("@#Rg11b10ufloatRenderable")]
    Rg11b10ufloatRenderable = 11,

    [Description("@#Bgra8unormStorage")]
    Bgra8unormStorage = 12,

    [Description("@#Float32Filterable")]
    Float32Filterable = 13,

    [Description("@#Float32Blendable")]
    Float32Blendable = 14,

    [Description("@#ClipDistances")]
    ClipDistances = 15,

    [Description("@#DualSourceBlending")]
    DualSourceBlending = 16,

    [Description("@#Subgroups")]
    Subgroups = 17,

    [Description("@#TextureFormatsTier1")]
    TextureFormatsTier1 = 18
}

/// <summary>
/// GPUBufferMapState
/// </summary>
[Description("@#GPUBufferMapState")]
[ECMAScript]
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
/// GPUTextureDimension
/// </summary>
[Description("@#GPUTextureDimension")]
[ECMAScript]
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
/// GPUTextureViewDimension
/// </summary>
[Description("@#GPUTextureViewDimension")]
[ECMAScript]
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
/// GPUTextureAspect
/// </summary>
[Description("@#GPUTextureAspect")]
[ECMAScript]
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
/// GPUTextureFormat
/// </summary>
[Description("@#GPUTextureFormat")]
[ECMAScript]
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

    [Description("@#R16unorm")]
    R16unorm = 4,

    [Description("@#R16snorm")]
    R16snorm = 5,

    [Description("@#R16uint")]
    R16uint = 6,

    [Description("@#R16sint")]
    R16sint = 7,

    [Description("@#R16float")]
    R16float = 8,

    [Description("@#Rg8unorm")]
    Rg8unorm = 9,

    [Description("@#Rg8snorm")]
    Rg8snorm = 10,

    [Description("@#Rg8uint")]
    Rg8uint = 11,

    [Description("@#Rg8sint")]
    Rg8sint = 12,

    [Description("@#R32uint")]
    R32uint = 13,

    [Description("@#R32sint")]
    R32sint = 14,

    [Description("@#R32float")]
    R32float = 15,

    [Description("@#Rg16unorm")]
    Rg16unorm = 16,

    [Description("@#Rg16snorm")]
    Rg16snorm = 17,

    [Description("@#Rg16uint")]
    Rg16uint = 18,

    [Description("@#Rg16sint")]
    Rg16sint = 19,

    [Description("@#Rg16float")]
    Rg16float = 20,

    [Description("@#Rgba8unorm")]
    Rgba8unorm = 21,

    [Description("@#Rgba8unormSrgb")]
    Rgba8unormSrgb = 22,

    [Description("@#Rgba8snorm")]
    Rgba8snorm = 23,

    [Description("@#Rgba8uint")]
    Rgba8uint = 24,

    [Description("@#Rgba8sint")]
    Rgba8sint = 25,

    [Description("@#Bgra8unorm")]
    Bgra8unorm = 26,

    [Description("@#Bgra8unormSrgb")]
    Bgra8unormSrgb = 27,

    [Description("@#Rgb9e5ufloat")]
    Rgb9e5ufloat = 28,

    [Description("@#Rgb10a2uint")]
    Rgb10a2uint = 29,

    [Description("@#Rgb10a2unorm")]
    Rgb10a2unorm = 30,

    [Description("@#Rg11b10ufloat")]
    Rg11b10ufloat = 31,

    [Description("@#Rg32uint")]
    Rg32uint = 32,

    [Description("@#Rg32sint")]
    Rg32sint = 33,

    [Description("@#Rg32float")]
    Rg32float = 34,

    [Description("@#Rgba16unorm")]
    Rgba16unorm = 35,

    [Description("@#Rgba16snorm")]
    Rgba16snorm = 36,

    [Description("@#Rgba16uint")]
    Rgba16uint = 37,

    [Description("@#Rgba16sint")]
    Rgba16sint = 38,

    [Description("@#Rgba16float")]
    Rgba16float = 39,

    [Description("@#Rgba32uint")]
    Rgba32uint = 40,

    [Description("@#Rgba32sint")]
    Rgba32sint = 41,

    [Description("@#Rgba32float")]
    Rgba32float = 42,

    [Description("@#Stencil8")]
    Stencil8 = 43,

    [Description("@#Depth16unorm")]
    Depth16unorm = 44,

    [Description("@#Depth24plus")]
    Depth24plus = 45,

    [Description("@#Depth24plusStencil8")]
    Depth24plusStencil8 = 46,

    [Description("@#Depth32float")]
    Depth32float = 47,

    [Description("@#Depth32floatStencil8")]
    Depth32floatStencil8 = 48,

    [Description("@#Bc1RgbaUnorm")]
    Bc1RgbaUnorm = 49,

    [Description("@#Bc1RgbaUnormSrgb")]
    Bc1RgbaUnormSrgb = 50,

    [Description("@#Bc2RgbaUnorm")]
    Bc2RgbaUnorm = 51,

    [Description("@#Bc2RgbaUnormSrgb")]
    Bc2RgbaUnormSrgb = 52,

    [Description("@#Bc3RgbaUnorm")]
    Bc3RgbaUnorm = 53,

    [Description("@#Bc3RgbaUnormSrgb")]
    Bc3RgbaUnormSrgb = 54,

    [Description("@#Bc4RUnorm")]
    Bc4RUnorm = 55,

    [Description("@#Bc4RSnorm")]
    Bc4RSnorm = 56,

    [Description("@#Bc5RgUnorm")]
    Bc5RgUnorm = 57,

    [Description("@#Bc5RgSnorm")]
    Bc5RgSnorm = 58,

    [Description("@#Bc6hRgbUfloat")]
    Bc6hRgbUfloat = 59,

    [Description("@#Bc6hRgbFloat")]
    Bc6hRgbFloat = 60,

    [Description("@#Bc7RgbaUnorm")]
    Bc7RgbaUnorm = 61,

    [Description("@#Bc7RgbaUnormSrgb")]
    Bc7RgbaUnormSrgb = 62,

    [Description("@#Etc2Rgb8unorm")]
    Etc2Rgb8unorm = 63,

    [Description("@#Etc2Rgb8unormSrgb")]
    Etc2Rgb8unormSrgb = 64,

    [Description("@#Etc2Rgb8a1unorm")]
    Etc2Rgb8a1unorm = 65,

    [Description("@#Etc2Rgb8a1unormSrgb")]
    Etc2Rgb8a1unormSrgb = 66,

    [Description("@#Etc2Rgba8unorm")]
    Etc2Rgba8unorm = 67,

    [Description("@#Etc2Rgba8unormSrgb")]
    Etc2Rgba8unormSrgb = 68,

    [Description("@#EacR11unorm")]
    EacR11unorm = 69,

    [Description("@#EacR11snorm")]
    EacR11snorm = 70,

    [Description("@#EacRg11unorm")]
    EacRg11unorm = 71,

    [Description("@#EacRg11snorm")]
    EacRg11snorm = 72,

    [Description("@#Astc4x4Unorm")]
    Astc4x4Unorm = 73,

    [Description("@#Astc4x4UnormSrgb")]
    Astc4x4UnormSrgb = 74,

    [Description("@#Astc5x4Unorm")]
    Astc5x4Unorm = 75,

    [Description("@#Astc5x4UnormSrgb")]
    Astc5x4UnormSrgb = 76,

    [Description("@#Astc5x5Unorm")]
    Astc5x5Unorm = 77,

    [Description("@#Astc5x5UnormSrgb")]
    Astc5x5UnormSrgb = 78,

    [Description("@#Astc6x5Unorm")]
    Astc6x5Unorm = 79,

    [Description("@#Astc6x5UnormSrgb")]
    Astc6x5UnormSrgb = 80,

    [Description("@#Astc6x6Unorm")]
    Astc6x6Unorm = 81,

    [Description("@#Astc6x6UnormSrgb")]
    Astc6x6UnormSrgb = 82,

    [Description("@#Astc8x5Unorm")]
    Astc8x5Unorm = 83,

    [Description("@#Astc8x5UnormSrgb")]
    Astc8x5UnormSrgb = 84,

    [Description("@#Astc8x6Unorm")]
    Astc8x6Unorm = 85,

    [Description("@#Astc8x6UnormSrgb")]
    Astc8x6UnormSrgb = 86,

    [Description("@#Astc8x8Unorm")]
    Astc8x8Unorm = 87,

    [Description("@#Astc8x8UnormSrgb")]
    Astc8x8UnormSrgb = 88,

    [Description("@#Astc10x5Unorm")]
    Astc10x5Unorm = 89,

    [Description("@#Astc10x5UnormSrgb")]
    Astc10x5UnormSrgb = 90,

    [Description("@#Astc10x6Unorm")]
    Astc10x6Unorm = 91,

    [Description("@#Astc10x6UnormSrgb")]
    Astc10x6UnormSrgb = 92,

    [Description("@#Astc10x8Unorm")]
    Astc10x8Unorm = 93,

    [Description("@#Astc10x8UnormSrgb")]
    Astc10x8UnormSrgb = 94,

    [Description("@#Astc10x10Unorm")]
    Astc10x10Unorm = 95,

    [Description("@#Astc10x10UnormSrgb")]
    Astc10x10UnormSrgb = 96,

    [Description("@#Astc12x10Unorm")]
    Astc12x10Unorm = 97,

    [Description("@#Astc12x10UnormSrgb")]
    Astc12x10UnormSrgb = 98,

    [Description("@#Astc12x12Unorm")]
    Astc12x12Unorm = 99,

    [Description("@#Astc12x12UnormSrgb")]
    Astc12x12UnormSrgb = 100
}

/// <summary>
/// GPUAddressMode
/// </summary>
[Description("@#GPUAddressMode")]
[ECMAScript]
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
/// GPUFilterMode
/// </summary>
[Description("@#GPUFilterMode")]
[ECMAScript]
public enum GPUFilterMode
{
    [Description("@#Nearest")]
    Nearest = 0,

    [Description("@#Linear")]
    Linear = 1
}

/// <summary>
/// GPUMipmapFilterMode
/// </summary>
[Description("@#GPUMipmapFilterMode")]
[ECMAScript]
public enum GPUMipmapFilterMode
{
    [Description("@#Nearest")]
    Nearest = 0,

    [Description("@#Linear")]
    Linear = 1
}

/// <summary>
/// GPUCompareFunction
/// </summary>
[Description("@#GPUCompareFunction")]
[ECMAScript]
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
/// GPUBufferBindingType
/// </summary>
[Description("@#GPUBufferBindingType")]
[ECMAScript]
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
/// GPUSamplerBindingType
/// </summary>
[Description("@#GPUSamplerBindingType")]
[ECMAScript]
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
/// GPUTextureSampleType
/// </summary>
[Description("@#GPUTextureSampleType")]
[ECMAScript]
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
/// GPUStorageTextureAccess
/// </summary>
[Description("@#GPUStorageTextureAccess")]
[ECMAScript]
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
/// GPUCompilationMessageType
/// </summary>
[Description("@#GPUCompilationMessageType")]
[ECMAScript]
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
/// GPUPipelineErrorReason
/// </summary>
[Description("@#GPUPipelineErrorReason")]
[ECMAScript]
public enum GPUPipelineErrorReason
{
    [Description("@#Validation")]
    Validation = 0,

    [Description("@#Internal")]
    Internal = 1
}

/// <summary>
/// GPUAutoLayoutMode
/// </summary>
[Description("@#GPUAutoLayoutMode")]
[ECMAScript]
public enum GPUAutoLayoutMode
{
    [Description("@#Auto")]
    Auto = 0
}

/// <summary>
/// GPUPrimitiveTopology
/// </summary>
[Description("@#GPUPrimitiveTopology")]
[ECMAScript]
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
/// GPUFrontFace
/// </summary>
[Description("@#GPUFrontFace")]
[ECMAScript]
public enum GPUFrontFace
{
    [Description("@#Ccw")]
    Ccw = 0,

    [Description("@#Cw")]
    Cw = 1
}

/// <summary>
/// GPUCullMode
/// </summary>
[Description("@#GPUCullMode")]
[ECMAScript]
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
/// GPUBlendFactor
/// </summary>
[Description("@#GPUBlendFactor")]
[ECMAScript]
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
    OneMinusConstant = 12,

    [Description("@#Src1")]
    Src1 = 13,

    [Description("@#OneMinusSrc1")]
    OneMinusSrc1 = 14,

    [Description("@#Src1Alpha")]
    Src1Alpha = 15,

    [Description("@#OneMinusSrc1Alpha")]
    OneMinusSrc1Alpha = 16
}

/// <summary>
/// GPUBlendOperation
/// </summary>
[Description("@#GPUBlendOperation")]
[ECMAScript]
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
/// GPUStencilOperation
/// </summary>
[Description("@#GPUStencilOperation")]
[ECMAScript]
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
/// GPUIndexFormat
/// </summary>
[Description("@#GPUIndexFormat")]
[ECMAScript]
public enum GPUIndexFormat
{
    [Description("@#Uint16")]
    Uint16 = 0,

    [Description("@#Uint32")]
    Uint32 = 1
}

/// <summary>
/// GPUVertexFormat
/// </summary>
[Description("@#GPUVertexFormat")]
[ECMAScript]
public enum GPUVertexFormat
{
    [Description("@#Uint8")]
    Uint8 = 0,

    [Description("@#Uint8x2")]
    Uint8x2 = 1,

    [Description("@#Uint8x4")]
    Uint8x4 = 2,

    [Description("@#Sint8")]
    Sint8 = 3,

    [Description("@#Sint8x2")]
    Sint8x2 = 4,

    [Description("@#Sint8x4")]
    Sint8x4 = 5,

    [Description("@#Unorm8")]
    Unorm8 = 6,

    [Description("@#Unorm8x2")]
    Unorm8x2 = 7,

    [Description("@#Unorm8x4")]
    Unorm8x4 = 8,

    [Description("@#Snorm8")]
    Snorm8 = 9,

    [Description("@#Snorm8x2")]
    Snorm8x2 = 10,

    [Description("@#Snorm8x4")]
    Snorm8x4 = 11,

    [Description("@#Uint16")]
    Uint16 = 12,

    [Description("@#Uint16x2")]
    Uint16x2 = 13,

    [Description("@#Uint16x4")]
    Uint16x4 = 14,

    [Description("@#Sint16")]
    Sint16 = 15,

    [Description("@#Sint16x2")]
    Sint16x2 = 16,

    [Description("@#Sint16x4")]
    Sint16x4 = 17,

    [Description("@#Unorm16")]
    Unorm16 = 18,

    [Description("@#Unorm16x2")]
    Unorm16x2 = 19,

    [Description("@#Unorm16x4")]
    Unorm16x4 = 20,

    [Description("@#Snorm16")]
    Snorm16 = 21,

    [Description("@#Snorm16x2")]
    Snorm16x2 = 22,

    [Description("@#Snorm16x4")]
    Snorm16x4 = 23,

    [Description("@#Float16")]
    Float16 = 24,

    [Description("@#Float16x2")]
    Float16x2 = 25,

    [Description("@#Float16x4")]
    Float16x4 = 26,

    [Description("@#Float32")]
    Float32 = 27,

    [Description("@#Float32x2")]
    Float32x2 = 28,

    [Description("@#Float32x3")]
    Float32x3 = 29,

    [Description("@#Float32x4")]
    Float32x4 = 30,

    [Description("@#Uint32")]
    Uint32 = 31,

    [Description("@#Uint32x2")]
    Uint32x2 = 32,

    [Description("@#Uint32x3")]
    Uint32x3 = 33,

    [Description("@#Uint32x4")]
    Uint32x4 = 34,

    [Description("@#Sint32")]
    Sint32 = 35,

    [Description("@#Sint32x2")]
    Sint32x2 = 36,

    [Description("@#Sint32x3")]
    Sint32x3 = 37,

    [Description("@#Sint32x4")]
    Sint32x4 = 38,

    [Description("@#Unorm1010102")]
    Unorm1010102 = 39,

    [Description("@#Unorm8x4Bgra")]
    Unorm8x4Bgra = 40
}

/// <summary>
/// GPUVertexStepMode
/// </summary>
[Description("@#GPUVertexStepMode")]
[ECMAScript]
public enum GPUVertexStepMode
{
    [Description("@#Vertex")]
    Vertex = 0,

    [Description("@#Instance")]
    Instance = 1
}

/// <summary>
/// GPULoadOp
/// </summary>
[Description("@#GPULoadOp")]
[ECMAScript]
public enum GPULoadOp
{
    [Description("@#Load")]
    Load = 0,

    [Description("@#Clear")]
    Clear = 1
}

/// <summary>
/// GPUStoreOp
/// </summary>
[Description("@#GPUStoreOp")]
[ECMAScript]
public enum GPUStoreOp
{
    [Description("@#Store")]
    Store = 0,

    [Description("@#Discard")]
    Discard = 1
}

/// <summary>
/// GPUQueryType
/// </summary>
[Description("@#GPUQueryType")]
[ECMAScript]
public enum GPUQueryType
{
    [Description("@#Occlusion")]
    Occlusion = 0,

    [Description("@#Timestamp")]
    Timestamp = 1
}

/// <summary>
/// GPUCanvasAlphaMode
/// </summary>
[Description("@#GPUCanvasAlphaMode")]
[ECMAScript]
public enum GPUCanvasAlphaMode
{
    [Description("@#Opaque")]
    Opaque = 0,

    [Description("@#Premultiplied")]
    Premultiplied = 1
}

/// <summary>
/// GPUCanvasToneMappingMode
/// </summary>
[Description("@#GPUCanvasToneMappingMode")]
[ECMAScript]
public enum GPUCanvasToneMappingMode
{
    [Description("@#Standard")]
    Standard = 0,

    [Description("@#Extended")]
    Extended = 1
}

/// <summary>
/// GPUDeviceLostReason
/// </summary>
[Description("@#GPUDeviceLostReason")]
[ECMAScript]
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
public enum GPUErrorFilter
{
    [Description("@#Validation")]
    Validation = 0,

    [Description("@#OutOfMemory")]
    OutOfMemory = 1,

    [Description("@#Internal")]
    Internal = 2
}