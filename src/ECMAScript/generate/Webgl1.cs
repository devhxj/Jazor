namespace ECMAScript;

/// <summary>
/// WebGLContextAttributes
/// </summary>
[ECMAScript]
[Description("@#WebGLContextAttributes")]
public record WebGLContextAttributes(
    [property: Description("@#alpha")]bool Alpha = true,
	[property: Description("@#depth")]bool Depth = true,
	[property: Description("@#stencil")]bool Stencil = false,
	[property: Description("@#antialias")]bool Antialias = true,
	[property: Description("@#premultipliedAlpha")]bool PremultipliedAlpha = true,
	[property: Description("@#preserveDrawingBuffer")]bool PreserveDrawingBuffer = false,
	[property: Description("@#powerPreference")]WebGLPowerPreference PowerPreference = WebGLPowerPreference.Default,
	[property: Description("@#failIfMajorPerformanceCaveat")]bool FailIfMajorPerformanceCaveat = false,
	[property: Description("@#desynchronized")]bool Desynchronized = false,
	[property: Description("@#xrCompatible")]bool XrCompatible = false)
{
    [Category("optional")]
    public extern static WebGLContextAttributes OptionalAlphaDepthStencil9(
        [Description("@#alpha")]bool alpha = true,
        [Description("@#depth")]bool depth = true,
        [Description("@#stencil")]bool stencil = false,
        [Description("@#antialias")]bool antialias = true,
        [Description("@#premultipliedAlpha")]bool premultipliedAlpha = true,
        [Description("@#preserveDrawingBuffer")]bool preserveDrawingBuffer = false,
        [Description("@#powerPreference")]WebGLPowerPreference powerPreference = WebGLPowerPreference.Default,
        [Description("@#failIfMajorPerformanceCaveat")]bool failIfMajorPerformanceCaveat = false,
        [Description("@#desynchronized")]bool desynchronized = false);

    [Category("optional")]
    public extern static WebGLContextAttributes OptionalXrCompatible(
        [Description("@#xrCompatible")]bool xrCompatible = false);
}

/// <summary>
/// WebGLContextEventInit
/// </summary>
[ECMAScript]
[Description("@#WebGLContextEventInit")]
public record WebGLContextEventInit(
    [property: Description("@#statusMessage")]string? StatusMessage = default): EventInit;

/// <summary>
/// WebGLObject
/// </summary>
[ECMAScript]
[Description("@#WebGLObject")]
public class WebGLObject
{
    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
}

/// <summary>
/// WebGLBuffer
/// </summary>
[ECMAScript]
[Description("@#WebGLBuffer")]
public class WebGLBuffer : WebGLObject
{

}

/// <summary>
/// WebGLFramebuffer
/// </summary>
[ECMAScript]
[Description("@#WebGLFramebuffer")]
public class WebGLFramebuffer : WebGLObject
{

}

/// <summary>
/// WebGLProgram
/// </summary>
[ECMAScript]
[Description("@#WebGLProgram")]
public class WebGLProgram : WebGLObject
{

}

/// <summary>
/// WebGLRenderbuffer
/// </summary>
[ECMAScript]
[Description("@#WebGLRenderbuffer")]
public class WebGLRenderbuffer : WebGLObject
{

}

/// <summary>
/// WebGLShader
/// </summary>
[ECMAScript]
[Description("@#WebGLShader")]
public class WebGLShader : WebGLObject
{

}

/// <summary>
/// WebGLTexture
/// </summary>
[ECMAScript]
[Description("@#WebGLTexture")]
public class WebGLTexture : WebGLObject
{

}

/// <summary>
/// WebGLUniformLocation
/// </summary>
[ECMAScript]
[Description("@#WebGLUniformLocation")]
public class WebGLUniformLocation
{

}

/// <summary>
/// WebGLActiveInfo
/// </summary>
[ECMAScript]
[Description("@#WebGLActiveInfo")]
public class WebGLActiveInfo
{
    /// <summary>
    /// size
    /// </summary>
    [Description("@#size")]
    public extern GLint Size { get; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern GLenum Type { get; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }
}

/// <summary>
/// WebGLShaderPrecisionFormat
/// </summary>
[ECMAScript]
[Description("@#WebGLShaderPrecisionFormat")]
public class WebGLShaderPrecisionFormat
{
    /// <summary>
    /// rangeMin
    /// </summary>
    [Description("@#rangeMin")]
    public extern GLint RangeMin { get; }

    /// <summary>
    /// rangeMax
    /// </summary>
    [Description("@#rangeMax")]
    public extern GLint RangeMax { get; }

    /// <summary>
    /// precision
    /// </summary>
    [Description("@#precision")]
    public extern GLint Precision { get; }
}

/// <summary>
/// WebGLRenderingContext
/// </summary>
[ECMAScript]
[Description("@#WebGLRenderingContext")]
public class WebGLRenderingContext
{


    #region mixin WebGLRenderingContextBase
    /// <summary>
    /// DEPTH_BUFFER_BIT
    /// </summary>
    [Description("@#DEPTH_BUFFER_BIT")]
    public const GLenum DEPTH_BUFFER_BIT = 0x00000100;

    /// <summary>
    /// STENCIL_BUFFER_BIT
    /// </summary>
    [Description("@#STENCIL_BUFFER_BIT")]
    public const GLenum STENCIL_BUFFER_BIT = 0x00000400;

    /// <summary>
    /// COLOR_BUFFER_BIT
    /// </summary>
    [Description("@#COLOR_BUFFER_BIT")]
    public const GLenum COLOR_BUFFER_BIT = 0x00004000;

    /// <summary>
    /// POINTS
    /// </summary>
    [Description("@#POINTS")]
    public const GLenum POINTS = 0x0000;

    /// <summary>
    /// LINES
    /// </summary>
    [Description("@#LINES")]
    public const GLenum LINES = 0x0001;

    /// <summary>
    /// LINE_LOOP
    /// </summary>
    [Description("@#LINE_LOOP")]
    public const GLenum LINE_LOOP = 0x0002;

    /// <summary>
    /// LINE_STRIP
    /// </summary>
    [Description("@#LINE_STRIP")]
    public const GLenum LINE_STRIP = 0x0003;

    /// <summary>
    /// TRIANGLES
    /// </summary>
    [Description("@#TRIANGLES")]
    public const GLenum TRIANGLES = 0x0004;

    /// <summary>
    /// TRIANGLE_STRIP
    /// </summary>
    [Description("@#TRIANGLE_STRIP")]
    public const GLenum TRIANGLE_STRIP = 0x0005;

    /// <summary>
    /// TRIANGLE_FAN
    /// </summary>
    [Description("@#TRIANGLE_FAN")]
    public const GLenum TRIANGLE_FAN = 0x0006;

    /// <summary>
    /// ZERO
    /// </summary>
    [Description("@#ZERO")]
    public const GLenum ZERO = 0;

    /// <summary>
    /// ONE
    /// </summary>
    [Description("@#ONE")]
    public const GLenum ONE = 1;

    /// <summary>
    /// SRC_COLOR
    /// </summary>
    [Description("@#SRC_COLOR")]
    public const GLenum SRC_COLOR = 0x0300;

    /// <summary>
    /// ONE_MINUS_SRC_COLOR
    /// </summary>
    [Description("@#ONE_MINUS_SRC_COLOR")]
    public const GLenum ONE_MINUS_SRC_COLOR = 0x0301;

    /// <summary>
    /// SRC_ALPHA
    /// </summary>
    [Description("@#SRC_ALPHA")]
    public const GLenum SRC_ALPHA = 0x0302;

    /// <summary>
    /// ONE_MINUS_SRC_ALPHA
    /// </summary>
    [Description("@#ONE_MINUS_SRC_ALPHA")]
    public const GLenum ONE_MINUS_SRC_ALPHA = 0x0303;

    /// <summary>
    /// DST_ALPHA
    /// </summary>
    [Description("@#DST_ALPHA")]
    public const GLenum DST_ALPHA = 0x0304;

    /// <summary>
    /// ONE_MINUS_DST_ALPHA
    /// </summary>
    [Description("@#ONE_MINUS_DST_ALPHA")]
    public const GLenum ONE_MINUS_DST_ALPHA = 0x0305;

    /// <summary>
    /// DST_COLOR
    /// </summary>
    [Description("@#DST_COLOR")]
    public const GLenum DST_COLOR = 0x0306;

    /// <summary>
    /// ONE_MINUS_DST_COLOR
    /// </summary>
    [Description("@#ONE_MINUS_DST_COLOR")]
    public const GLenum ONE_MINUS_DST_COLOR = 0x0307;

    /// <summary>
    /// SRC_ALPHA_SATURATE
    /// </summary>
    [Description("@#SRC_ALPHA_SATURATE")]
    public const GLenum SRC_ALPHA_SATURATE = 0x0308;

    /// <summary>
    /// FUNC_ADD
    /// </summary>
    [Description("@#FUNC_ADD")]
    public const GLenum FUNC_ADD = 0x8006;

    /// <summary>
    /// BLEND_EQUATION
    /// </summary>
    [Description("@#BLEND_EQUATION")]
    public const GLenum BLEND_EQUATION = 0x8009;

    /// <summary>
    /// BLEND_EQUATION_RGB
    /// </summary>
    [Description("@#BLEND_EQUATION_RGB")]
    public const GLenum BLEND_EQUATION_RGB = 0x8009;

    /// <summary>
    /// BLEND_EQUATION_ALPHA
    /// </summary>
    [Description("@#BLEND_EQUATION_ALPHA")]
    public const GLenum BLEND_EQUATION_ALPHA = 0x883D;

    /// <summary>
    /// FUNC_SUBTRACT
    /// </summary>
    [Description("@#FUNC_SUBTRACT")]
    public const GLenum FUNC_SUBTRACT = 0x800A;

    /// <summary>
    /// FUNC_REVERSE_SUBTRACT
    /// </summary>
    [Description("@#FUNC_REVERSE_SUBTRACT")]
    public const GLenum FUNC_REVERSE_SUBTRACT = 0x800B;

    /// <summary>
    /// BLEND_DST_RGB
    /// </summary>
    [Description("@#BLEND_DST_RGB")]
    public const GLenum BLEND_DST_RGB = 0x80C8;

    /// <summary>
    /// BLEND_SRC_RGB
    /// </summary>
    [Description("@#BLEND_SRC_RGB")]
    public const GLenum BLEND_SRC_RGB = 0x80C9;

    /// <summary>
    /// BLEND_DST_ALPHA
    /// </summary>
    [Description("@#BLEND_DST_ALPHA")]
    public const GLenum BLEND_DST_ALPHA = 0x80CA;

    /// <summary>
    /// BLEND_SRC_ALPHA
    /// </summary>
    [Description("@#BLEND_SRC_ALPHA")]
    public const GLenum BLEND_SRC_ALPHA = 0x80CB;

    /// <summary>
    /// CONSTANT_COLOR
    /// </summary>
    [Description("@#CONSTANT_COLOR")]
    public const GLenum CONSTANT_COLOR = 0x8001;

    /// <summary>
    /// ONE_MINUS_CONSTANT_COLOR
    /// </summary>
    [Description("@#ONE_MINUS_CONSTANT_COLOR")]
    public const GLenum ONE_MINUS_CONSTANT_COLOR = 0x8002;

    /// <summary>
    /// CONSTANT_ALPHA
    /// </summary>
    [Description("@#CONSTANT_ALPHA")]
    public const GLenum CONSTANT_ALPHA = 0x8003;

    /// <summary>
    /// ONE_MINUS_CONSTANT_ALPHA
    /// </summary>
    [Description("@#ONE_MINUS_CONSTANT_ALPHA")]
    public const GLenum ONE_MINUS_CONSTANT_ALPHA = 0x8004;

    /// <summary>
    /// BLEND_COLOR
    /// </summary>
    [Description("@#BLEND_COLOR")]
    public const GLenum BLEND_COLOR = 0x8005;

    /// <summary>
    /// ARRAY_BUFFER
    /// </summary>
    [Description("@#ARRAY_BUFFER")]
    public const GLenum ARRAY_BUFFER = 0x8892;

    /// <summary>
    /// ELEMENT_ARRAY_BUFFER
    /// </summary>
    [Description("@#ELEMENT_ARRAY_BUFFER")]
    public const GLenum ELEMENT_ARRAY_BUFFER = 0x8893;

    /// <summary>
    /// ARRAY_BUFFER_BINDING
    /// </summary>
    [Description("@#ARRAY_BUFFER_BINDING")]
    public const GLenum ARRAY_BUFFER_BINDING = 0x8894;

    /// <summary>
    /// ELEMENT_ARRAY_BUFFER_BINDING
    /// </summary>
    [Description("@#ELEMENT_ARRAY_BUFFER_BINDING")]
    public const GLenum ELEMENT_ARRAY_BUFFER_BINDING = 0x8895;

    /// <summary>
    /// STREAM_DRAW
    /// </summary>
    [Description("@#STREAM_DRAW")]
    public const GLenum STREAM_DRAW = 0x88E0;

    /// <summary>
    /// STATIC_DRAW
    /// </summary>
    [Description("@#STATIC_DRAW")]
    public const GLenum STATIC_DRAW = 0x88E4;

    /// <summary>
    /// DYNAMIC_DRAW
    /// </summary>
    [Description("@#DYNAMIC_DRAW")]
    public const GLenum DYNAMIC_DRAW = 0x88E8;

    /// <summary>
    /// BUFFER_SIZE
    /// </summary>
    [Description("@#BUFFER_SIZE")]
    public const GLenum BUFFER_SIZE = 0x8764;

    /// <summary>
    /// BUFFER_USAGE
    /// </summary>
    [Description("@#BUFFER_USAGE")]
    public const GLenum BUFFER_USAGE = 0x8765;

    /// <summary>
    /// CURRENT_VERTEX_ATTRIB
    /// </summary>
    [Description("@#CURRENT_VERTEX_ATTRIB")]
    public const GLenum CURRENT_VERTEX_ATTRIB = 0x8626;

    /// <summary>
    /// FRONT
    /// </summary>
    [Description("@#FRONT")]
    public const GLenum FRONT = 0x0404;

    /// <summary>
    /// BACK
    /// </summary>
    [Description("@#BACK")]
    public const GLenum BACK = 0x0405;

    /// <summary>
    /// FRONT_AND_BACK
    /// </summary>
    [Description("@#FRONT_AND_BACK")]
    public const GLenum FRONT_AND_BACK = 0x0408;

    /// <summary>
    /// CULL_FACE
    /// </summary>
    [Description("@#CULL_FACE")]
    public const GLenum CULL_FACE = 0x0B44;

    /// <summary>
    /// BLEND
    /// </summary>
    [Description("@#BLEND")]
    public const GLenum BLEND = 0x0BE2;

    /// <summary>
    /// DITHER
    /// </summary>
    [Description("@#DITHER")]
    public const GLenum DITHER = 0x0BD0;

    /// <summary>
    /// STENCIL_TEST
    /// </summary>
    [Description("@#STENCIL_TEST")]
    public const GLenum STENCIL_TEST = 0x0B90;

    /// <summary>
    /// DEPTH_TEST
    /// </summary>
    [Description("@#DEPTH_TEST")]
    public const GLenum DEPTH_TEST = 0x0B71;

    /// <summary>
    /// SCISSOR_TEST
    /// </summary>
    [Description("@#SCISSOR_TEST")]
    public const GLenum SCISSOR_TEST = 0x0C11;

    /// <summary>
    /// POLYGON_OFFSET_FILL
    /// </summary>
    [Description("@#POLYGON_OFFSET_FILL")]
    public const GLenum POLYGON_OFFSET_FILL = 0x8037;

    /// <summary>
    /// SAMPLE_ALPHA_TO_COVERAGE
    /// </summary>
    [Description("@#SAMPLE_ALPHA_TO_COVERAGE")]
    public const GLenum SAMPLE_ALPHA_TO_COVERAGE = 0x809E;

    /// <summary>
    /// SAMPLE_COVERAGE
    /// </summary>
    [Description("@#SAMPLE_COVERAGE")]
    public const GLenum SAMPLE_COVERAGE = 0x80A0;

    /// <summary>
    /// NO_ERROR
    /// </summary>
    [Description("@#NO_ERROR")]
    public const GLenum NO_ERROR = 0;

    /// <summary>
    /// INVALID_ENUM
    /// </summary>
    [Description("@#INVALID_ENUM")]
    public const GLenum INVALID_ENUM = 0x0500;

    /// <summary>
    /// INVALID_VALUE
    /// </summary>
    [Description("@#INVALID_VALUE")]
    public const GLenum INVALID_VALUE = 0x0501;

    /// <summary>
    /// INVALID_OPERATION
    /// </summary>
    [Description("@#INVALID_OPERATION")]
    public const GLenum INVALID_OPERATION = 0x0502;

    /// <summary>
    /// OUT_OF_MEMORY
    /// </summary>
    [Description("@#OUT_OF_MEMORY")]
    public const GLenum OUT_OF_MEMORY = 0x0505;

    /// <summary>
    /// CW
    /// </summary>
    [Description("@#CW")]
    public const GLenum CW = 0x0900;

    /// <summary>
    /// CCW
    /// </summary>
    [Description("@#CCW")]
    public const GLenum CCW = 0x0901;

    /// <summary>
    /// LINE_WIDTH
    /// </summary>
    [Description("@#LINE_WIDTH")]
    public const GLenum LINE_WIDTH = 0x0B21;

    /// <summary>
    /// ALIASED_POINT_SIZE_RANGE
    /// </summary>
    [Description("@#ALIASED_POINT_SIZE_RANGE")]
    public const GLenum ALIASED_POINT_SIZE_RANGE = 0x846D;

    /// <summary>
    /// ALIASED_LINE_WIDTH_RANGE
    /// </summary>
    [Description("@#ALIASED_LINE_WIDTH_RANGE")]
    public const GLenum ALIASED_LINE_WIDTH_RANGE = 0x846E;

    /// <summary>
    /// CULL_FACE_MODE
    /// </summary>
    [Description("@#CULL_FACE_MODE")]
    public const GLenum CULL_FACE_MODE = 0x0B45;

    /// <summary>
    /// FRONT_FACE
    /// </summary>
    [Description("@#FRONT_FACE")]
    public const GLenum FRONT_FACE = 0x0B46;

    /// <summary>
    /// DEPTH_RANGE
    /// </summary>
    [Description("@#DEPTH_RANGE")]
    public const GLenum DEPTH_RANGE = 0x0B70;

    /// <summary>
    /// DEPTH_WRITEMASK
    /// </summary>
    [Description("@#DEPTH_WRITEMASK")]
    public const GLenum DEPTH_WRITEMASK = 0x0B72;

    /// <summary>
    /// DEPTH_CLEAR_VALUE
    /// </summary>
    [Description("@#DEPTH_CLEAR_VALUE")]
    public const GLenum DEPTH_CLEAR_VALUE = 0x0B73;

    /// <summary>
    /// DEPTH_FUNC
    /// </summary>
    [Description("@#DEPTH_FUNC")]
    public const GLenum DEPTH_FUNC = 0x0B74;

    /// <summary>
    /// STENCIL_CLEAR_VALUE
    /// </summary>
    [Description("@#STENCIL_CLEAR_VALUE")]
    public const GLenum STENCIL_CLEAR_VALUE = 0x0B91;

    /// <summary>
    /// STENCIL_FUNC
    /// </summary>
    [Description("@#STENCIL_FUNC")]
    public const GLenum STENCIL_FUNC = 0x0B92;

    /// <summary>
    /// STENCIL_FAIL
    /// </summary>
    [Description("@#STENCIL_FAIL")]
    public const GLenum STENCIL_FAIL = 0x0B94;

    /// <summary>
    /// STENCIL_PASS_DEPTH_FAIL
    /// </summary>
    [Description("@#STENCIL_PASS_DEPTH_FAIL")]
    public const GLenum STENCIL_PASS_DEPTH_FAIL = 0x0B95;

    /// <summary>
    /// STENCIL_PASS_DEPTH_PASS
    /// </summary>
    [Description("@#STENCIL_PASS_DEPTH_PASS")]
    public const GLenum STENCIL_PASS_DEPTH_PASS = 0x0B96;

    /// <summary>
    /// STENCIL_REF
    /// </summary>
    [Description("@#STENCIL_REF")]
    public const GLenum STENCIL_REF = 0x0B97;

    /// <summary>
    /// STENCIL_VALUE_MASK
    /// </summary>
    [Description("@#STENCIL_VALUE_MASK")]
    public const GLenum STENCIL_VALUE_MASK = 0x0B93;

    /// <summary>
    /// STENCIL_WRITEMASK
    /// </summary>
    [Description("@#STENCIL_WRITEMASK")]
    public const GLenum STENCIL_WRITEMASK = 0x0B98;

    /// <summary>
    /// STENCIL_BACK_FUNC
    /// </summary>
    [Description("@#STENCIL_BACK_FUNC")]
    public const GLenum STENCIL_BACK_FUNC = 0x8800;

    /// <summary>
    /// STENCIL_BACK_FAIL
    /// </summary>
    [Description("@#STENCIL_BACK_FAIL")]
    public const GLenum STENCIL_BACK_FAIL = 0x8801;

    /// <summary>
    /// STENCIL_BACK_PASS_DEPTH_FAIL
    /// </summary>
    [Description("@#STENCIL_BACK_PASS_DEPTH_FAIL")]
    public const GLenum STENCIL_BACK_PASS_DEPTH_FAIL = 0x8802;

    /// <summary>
    /// STENCIL_BACK_PASS_DEPTH_PASS
    /// </summary>
    [Description("@#STENCIL_BACK_PASS_DEPTH_PASS")]
    public const GLenum STENCIL_BACK_PASS_DEPTH_PASS = 0x8803;

    /// <summary>
    /// STENCIL_BACK_REF
    /// </summary>
    [Description("@#STENCIL_BACK_REF")]
    public const GLenum STENCIL_BACK_REF = 0x8CA3;

    /// <summary>
    /// STENCIL_BACK_VALUE_MASK
    /// </summary>
    [Description("@#STENCIL_BACK_VALUE_MASK")]
    public const GLenum STENCIL_BACK_VALUE_MASK = 0x8CA4;

    /// <summary>
    /// STENCIL_BACK_WRITEMASK
    /// </summary>
    [Description("@#STENCIL_BACK_WRITEMASK")]
    public const GLenum STENCIL_BACK_WRITEMASK = 0x8CA5;

    /// <summary>
    /// VIEWPORT
    /// </summary>
    [Description("@#VIEWPORT")]
    public const GLenum VIEWPORT = 0x0BA2;

    /// <summary>
    /// SCISSOR_BOX
    /// </summary>
    [Description("@#SCISSOR_BOX")]
    public const GLenum SCISSOR_BOX = 0x0C10;

    /// <summary>
    /// COLOR_CLEAR_VALUE
    /// </summary>
    [Description("@#COLOR_CLEAR_VALUE")]
    public const GLenum COLOR_CLEAR_VALUE = 0x0C22;

    /// <summary>
    /// COLOR_WRITEMASK
    /// </summary>
    [Description("@#COLOR_WRITEMASK")]
    public const GLenum COLOR_WRITEMASK = 0x0C23;

    /// <summary>
    /// UNPACK_ALIGNMENT
    /// </summary>
    [Description("@#UNPACK_ALIGNMENT")]
    public const GLenum UNPACK_ALIGNMENT = 0x0CF5;

    /// <summary>
    /// PACK_ALIGNMENT
    /// </summary>
    [Description("@#PACK_ALIGNMENT")]
    public const GLenum PACK_ALIGNMENT = 0x0D05;

    /// <summary>
    /// MAX_TEXTURE_SIZE
    /// </summary>
    [Description("@#MAX_TEXTURE_SIZE")]
    public const GLenum MAX_TEXTURE_SIZE = 0x0D33;

    /// <summary>
    /// MAX_VIEWPORT_DIMS
    /// </summary>
    [Description("@#MAX_VIEWPORT_DIMS")]
    public const GLenum MAX_VIEWPORT_DIMS = 0x0D3A;

    /// <summary>
    /// SUBPIXEL_BITS
    /// </summary>
    [Description("@#SUBPIXEL_BITS")]
    public const GLenum SUBPIXEL_BITS = 0x0D50;

    /// <summary>
    /// RED_BITS
    /// </summary>
    [Description("@#RED_BITS")]
    public const GLenum RED_BITS = 0x0D52;

    /// <summary>
    /// GREEN_BITS
    /// </summary>
    [Description("@#GREEN_BITS")]
    public const GLenum GREEN_BITS = 0x0D53;

    /// <summary>
    /// BLUE_BITS
    /// </summary>
    [Description("@#BLUE_BITS")]
    public const GLenum BLUE_BITS = 0x0D54;

    /// <summary>
    /// ALPHA_BITS
    /// </summary>
    [Description("@#ALPHA_BITS")]
    public const GLenum ALPHA_BITS = 0x0D55;

    /// <summary>
    /// DEPTH_BITS
    /// </summary>
    [Description("@#DEPTH_BITS")]
    public const GLenum DEPTH_BITS = 0x0D56;

    /// <summary>
    /// STENCIL_BITS
    /// </summary>
    [Description("@#STENCIL_BITS")]
    public const GLenum STENCIL_BITS = 0x0D57;

    /// <summary>
    /// POLYGON_OFFSET_UNITS
    /// </summary>
    [Description("@#POLYGON_OFFSET_UNITS")]
    public const GLenum POLYGON_OFFSET_UNITS = 0x2A00;

    /// <summary>
    /// POLYGON_OFFSET_FACTOR
    /// </summary>
    [Description("@#POLYGON_OFFSET_FACTOR")]
    public const GLenum POLYGON_OFFSET_FACTOR = 0x8038;

    /// <summary>
    /// TEXTURE_BINDING_2D
    /// </summary>
    [Description("@#TEXTURE_BINDING_2D")]
    public const GLenum TEXTUREBINDING2D = 0x8069;

    /// <summary>
    /// SAMPLE_BUFFERS
    /// </summary>
    [Description("@#SAMPLE_BUFFERS")]
    public const GLenum SAMPLE_BUFFERS = 0x80A8;

    /// <summary>
    /// SAMPLES
    /// </summary>
    [Description("@#SAMPLES")]
    public const GLenum SAMPLES = 0x80A9;

    /// <summary>
    /// SAMPLE_COVERAGE_VALUE
    /// </summary>
    [Description("@#SAMPLE_COVERAGE_VALUE")]
    public const GLenum SAMPLE_COVERAGE_VALUE = 0x80AA;

    /// <summary>
    /// SAMPLE_COVERAGE_INVERT
    /// </summary>
    [Description("@#SAMPLE_COVERAGE_INVERT")]
    public const GLenum SAMPLE_COVERAGE_INVERT = 0x80AB;

    /// <summary>
    /// COMPRESSED_TEXTURE_FORMATS
    /// </summary>
    [Description("@#COMPRESSED_TEXTURE_FORMATS")]
    public const GLenum COMPRESSED_TEXTURE_FORMATS = 0x86A3;

    /// <summary>
    /// DONT_CARE
    /// </summary>
    [Description("@#DONT_CARE")]
    public const GLenum DONT_CARE = 0x1100;

    /// <summary>
    /// FASTEST
    /// </summary>
    [Description("@#FASTEST")]
    public const GLenum FASTEST = 0x1101;

    /// <summary>
    /// NICEST
    /// </summary>
    [Description("@#NICEST")]
    public const GLenum NICEST = 0x1102;

    /// <summary>
    /// GENERATE_MIPMAP_HINT
    /// </summary>
    [Description("@#GENERATE_MIPMAP_HINT")]
    public const GLenum GENERATE_MIPMAP_HINT = 0x8192;

    /// <summary>
    /// BYTE
    /// </summary>
    [Description("@#BYTE")]
    public const GLenum BYTE = 0x1400;

    /// <summary>
    /// UNSIGNED_BYTE
    /// </summary>
    [Description("@#UNSIGNED_BYTE")]
    public const GLenum UNSIGNED_BYTE = 0x1401;

    /// <summary>
    /// SHORT
    /// </summary>
    [Description("@#SHORT")]
    public const GLenum SHORT = 0x1402;

    /// <summary>
    /// UNSIGNED_SHORT
    /// </summary>
    [Description("@#UNSIGNED_SHORT")]
    public const GLenum UNSIGNED_SHORT = 0x1403;

    /// <summary>
    /// INT
    /// </summary>
    [Description("@#INT")]
    public const GLenum INT = 0x1404;

    /// <summary>
    /// UNSIGNED_INT
    /// </summary>
    [Description("@#UNSIGNED_INT")]
    public const GLenum UNSIGNED_INT = 0x1405;

    /// <summary>
    /// FLOAT
    /// </summary>
    [Description("@#FLOAT")]
    public const GLenum FLOAT = 0x1406;

    /// <summary>
    /// DEPTH_COMPONENT
    /// </summary>
    [Description("@#DEPTH_COMPONENT")]
    public const GLenum DEPTH_COMPONENT = 0x1902;

    /// <summary>
    /// ALPHA
    /// </summary>
    [Description("@#ALPHA")]
    public const GLenum ALPHA = 0x1906;

    /// <summary>
    /// RGB
    /// </summary>
    [Description("@#RGB")]
    public const GLenum RGB = 0x1907;

    /// <summary>
    /// RGBA
    /// </summary>
    [Description("@#RGBA")]
    public const GLenum RGBA = 0x1908;

    /// <summary>
    /// LUMINANCE
    /// </summary>
    [Description("@#LUMINANCE")]
    public const GLenum LUMINANCE = 0x1909;

    /// <summary>
    /// LUMINANCE_ALPHA
    /// </summary>
    [Description("@#LUMINANCE_ALPHA")]
    public const GLenum LUMINANCE_ALPHA = 0x190A;

    /// <summary>
    /// UNSIGNED_SHORT_4_4_4_4
    /// </summary>
    [Description("@#UNSIGNED_SHORT_4_4_4_4")]
    public const GLenum UNSIGNEDSHORT4444 = 0x8033;

    /// <summary>
    /// UNSIGNED_SHORT_5_5_5_1
    /// </summary>
    [Description("@#UNSIGNED_SHORT_5_5_5_1")]
    public const GLenum UNSIGNEDSHORT5551 = 0x8034;

    /// <summary>
    /// UNSIGNED_SHORT_5_6_5
    /// </summary>
    [Description("@#UNSIGNED_SHORT_5_6_5")]
    public const GLenum UNSIGNEDSHORT565 = 0x8363;

    /// <summary>
    /// FRAGMENT_SHADER
    /// </summary>
    [Description("@#FRAGMENT_SHADER")]
    public const GLenum FRAGMENT_SHADER = 0x8B30;

    /// <summary>
    /// VERTEX_SHADER
    /// </summary>
    [Description("@#VERTEX_SHADER")]
    public const GLenum VERTEX_SHADER = 0x8B31;

    /// <summary>
    /// MAX_VERTEX_ATTRIBS
    /// </summary>
    [Description("@#MAX_VERTEX_ATTRIBS")]
    public const GLenum MAX_VERTEX_ATTRIBS = 0x8869;

    /// <summary>
    /// MAX_VERTEX_UNIFORM_VECTORS
    /// </summary>
    [Description("@#MAX_VERTEX_UNIFORM_VECTORS")]
    public const GLenum MAX_VERTEX_UNIFORM_VECTORS = 0x8DFB;

    /// <summary>
    /// MAX_VARYING_VECTORS
    /// </summary>
    [Description("@#MAX_VARYING_VECTORS")]
    public const GLenum MAX_VARYING_VECTORS = 0x8DFC;

    /// <summary>
    /// MAX_COMBINED_TEXTURE_IMAGE_UNITS
    /// </summary>
    [Description("@#MAX_COMBINED_TEXTURE_IMAGE_UNITS")]
    public const GLenum MAX_COMBINED_TEXTURE_IMAGE_UNITS = 0x8B4D;

    /// <summary>
    /// MAX_VERTEX_TEXTURE_IMAGE_UNITS
    /// </summary>
    [Description("@#MAX_VERTEX_TEXTURE_IMAGE_UNITS")]
    public const GLenum MAX_VERTEX_TEXTURE_IMAGE_UNITS = 0x8B4C;

    /// <summary>
    /// MAX_TEXTURE_IMAGE_UNITS
    /// </summary>
    [Description("@#MAX_TEXTURE_IMAGE_UNITS")]
    public const GLenum MAX_TEXTURE_IMAGE_UNITS = 0x8872;

    /// <summary>
    /// MAX_FRAGMENT_UNIFORM_VECTORS
    /// </summary>
    [Description("@#MAX_FRAGMENT_UNIFORM_VECTORS")]
    public const GLenum MAX_FRAGMENT_UNIFORM_VECTORS = 0x8DFD;

    /// <summary>
    /// SHADER_TYPE
    /// </summary>
    [Description("@#SHADER_TYPE")]
    public const GLenum SHADER_TYPE = 0x8B4F;

    /// <summary>
    /// DELETE_STATUS
    /// </summary>
    [Description("@#DELETE_STATUS")]
    public const GLenum DELETE_STATUS = 0x8B80;

    /// <summary>
    /// LINK_STATUS
    /// </summary>
    [Description("@#LINK_STATUS")]
    public const GLenum LINK_STATUS = 0x8B82;

    /// <summary>
    /// VALIDATE_STATUS
    /// </summary>
    [Description("@#VALIDATE_STATUS")]
    public const GLenum VALIDATE_STATUS = 0x8B83;

    /// <summary>
    /// ATTACHED_SHADERS
    /// </summary>
    [Description("@#ATTACHED_SHADERS")]
    public const GLenum ATTACHED_SHADERS = 0x8B85;

    /// <summary>
    /// ACTIVE_UNIFORMS
    /// </summary>
    [Description("@#ACTIVE_UNIFORMS")]
    public const GLenum ACTIVE_UNIFORMS = 0x8B86;

    /// <summary>
    /// ACTIVE_ATTRIBUTES
    /// </summary>
    [Description("@#ACTIVE_ATTRIBUTES")]
    public const GLenum ACTIVE_ATTRIBUTES = 0x8B89;

    /// <summary>
    /// SHADING_LANGUAGE_VERSION
    /// </summary>
    [Description("@#SHADING_LANGUAGE_VERSION")]
    public const GLenum SHADING_LANGUAGE_VERSION = 0x8B8C;

    /// <summary>
    /// CURRENT_PROGRAM
    /// </summary>
    [Description("@#CURRENT_PROGRAM")]
    public const GLenum CURRENT_PROGRAM = 0x8B8D;

    /// <summary>
    /// NEVER
    /// </summary>
    [Description("@#NEVER")]
    public const GLenum NEVER = 0x0200;

    /// <summary>
    /// LESS
    /// </summary>
    [Description("@#LESS")]
    public const GLenum LESS = 0x0201;

    /// <summary>
    /// EQUAL
    /// </summary>
    [Description("@#EQUAL")]
    public const GLenum EQUAL = 0x0202;

    /// <summary>
    /// LEQUAL
    /// </summary>
    [Description("@#LEQUAL")]
    public const GLenum LEQUAL = 0x0203;

    /// <summary>
    /// GREATER
    /// </summary>
    [Description("@#GREATER")]
    public const GLenum GREATER = 0x0204;

    /// <summary>
    /// NOTEQUAL
    /// </summary>
    [Description("@#NOTEQUAL")]
    public const GLenum NOTEQUAL = 0x0205;

    /// <summary>
    /// GEQUAL
    /// </summary>
    [Description("@#GEQUAL")]
    public const GLenum GEQUAL = 0x0206;

    /// <summary>
    /// ALWAYS
    /// </summary>
    [Description("@#ALWAYS")]
    public const GLenum ALWAYS = 0x0207;

    /// <summary>
    /// KEEP
    /// </summary>
    [Description("@#KEEP")]
    public const GLenum KEEP = 0x1E00;

    /// <summary>
    /// REPLACE
    /// </summary>
    [Description("@#REPLACE")]
    public const GLenum REPLACE = 0x1E01;

    /// <summary>
    /// INCR
    /// </summary>
    [Description("@#INCR")]
    public const GLenum INCR = 0x1E02;

    /// <summary>
    /// DECR
    /// </summary>
    [Description("@#DECR")]
    public const GLenum DECR = 0x1E03;

    /// <summary>
    /// INVERT
    /// </summary>
    [Description("@#INVERT")]
    public const GLenum INVERT = 0x150A;

    /// <summary>
    /// INCR_WRAP
    /// </summary>
    [Description("@#INCR_WRAP")]
    public const GLenum INCR_WRAP = 0x8507;

    /// <summary>
    /// DECR_WRAP
    /// </summary>
    [Description("@#DECR_WRAP")]
    public const GLenum DECR_WRAP = 0x8508;

    /// <summary>
    /// VENDOR
    /// </summary>
    [Description("@#VENDOR")]
    public const GLenum VENDOR = 0x1F00;

    /// <summary>
    /// RENDERER
    /// </summary>
    [Description("@#RENDERER")]
    public const GLenum RENDERER = 0x1F01;

    /// <summary>
    /// VERSION
    /// </summary>
    [Description("@#VERSION")]
    public const GLenum VERSION = 0x1F02;

    /// <summary>
    /// NEAREST
    /// </summary>
    [Description("@#NEAREST")]
    public const GLenum NEAREST = 0x2600;

    /// <summary>
    /// LINEAR
    /// </summary>
    [Description("@#LINEAR")]
    public const GLenum LINEAR = 0x2601;

    /// <summary>
    /// NEAREST_MIPMAP_NEAREST
    /// </summary>
    [Description("@#NEAREST_MIPMAP_NEAREST")]
    public const GLenum NEAREST_MIPMAP_NEAREST = 0x2700;

    /// <summary>
    /// LINEAR_MIPMAP_NEAREST
    /// </summary>
    [Description("@#LINEAR_MIPMAP_NEAREST")]
    public const GLenum LINEAR_MIPMAP_NEAREST = 0x2701;

    /// <summary>
    /// NEAREST_MIPMAP_LINEAR
    /// </summary>
    [Description("@#NEAREST_MIPMAP_LINEAR")]
    public const GLenum NEAREST_MIPMAP_LINEAR = 0x2702;

    /// <summary>
    /// LINEAR_MIPMAP_LINEAR
    /// </summary>
    [Description("@#LINEAR_MIPMAP_LINEAR")]
    public const GLenum LINEAR_MIPMAP_LINEAR = 0x2703;

    /// <summary>
    /// TEXTURE_MAG_FILTER
    /// </summary>
    [Description("@#TEXTURE_MAG_FILTER")]
    public const GLenum TEXTURE_MAG_FILTER = 0x2800;

    /// <summary>
    /// TEXTURE_MIN_FILTER
    /// </summary>
    [Description("@#TEXTURE_MIN_FILTER")]
    public const GLenum TEXTURE_MIN_FILTER = 0x2801;

    /// <summary>
    /// TEXTURE_WRAP_S
    /// </summary>
    [Description("@#TEXTURE_WRAP_S")]
    public const GLenum TEXTURE_WRAP_S = 0x2802;

    /// <summary>
    /// TEXTURE_WRAP_T
    /// </summary>
    [Description("@#TEXTURE_WRAP_T")]
    public const GLenum TEXTURE_WRAP_T = 0x2803;

    /// <summary>
    /// TEXTURE_2D
    /// </summary>
    [Description("@#TEXTURE_2D")]
    public const GLenum TEXTURE2D = 0x0DE1;

    /// <summary>
    /// TEXTURE
    /// </summary>
    [Description("@#TEXTURE")]
    public const GLenum TEXTURE = 0x1702;

    /// <summary>
    /// TEXTURE_CUBE_MAP
    /// </summary>
    [Description("@#TEXTURE_CUBE_MAP")]
    public const GLenum TEXTURE_CUBE_MAP = 0x8513;

    /// <summary>
    /// TEXTURE_BINDING_CUBE_MAP
    /// </summary>
    [Description("@#TEXTURE_BINDING_CUBE_MAP")]
    public const GLenum TEXTURE_BINDING_CUBE_MAP = 0x8514;

    /// <summary>
    /// TEXTURE_CUBE_MAP_POSITIVE_X
    /// </summary>
    [Description("@#TEXTURE_CUBE_MAP_POSITIVE_X")]
    public const GLenum TEXTURE_CUBE_MAP_POSITIVE_X = 0x8515;

    /// <summary>
    /// TEXTURE_CUBE_MAP_NEGATIVE_X
    /// </summary>
    [Description("@#TEXTURE_CUBE_MAP_NEGATIVE_X")]
    public const GLenum TEXTURE_CUBE_MAP_NEGATIVE_X = 0x8516;

    /// <summary>
    /// TEXTURE_CUBE_MAP_POSITIVE_Y
    /// </summary>
    [Description("@#TEXTURE_CUBE_MAP_POSITIVE_Y")]
    public const GLenum TEXTURE_CUBE_MAP_POSITIVE_Y = 0x8517;

    /// <summary>
    /// TEXTURE_CUBE_MAP_NEGATIVE_Y
    /// </summary>
    [Description("@#TEXTURE_CUBE_MAP_NEGATIVE_Y")]
    public const GLenum TEXTURE_CUBE_MAP_NEGATIVE_Y = 0x8518;

    /// <summary>
    /// TEXTURE_CUBE_MAP_POSITIVE_Z
    /// </summary>
    [Description("@#TEXTURE_CUBE_MAP_POSITIVE_Z")]
    public const GLenum TEXTURE_CUBE_MAP_POSITIVE_Z = 0x8519;

    /// <summary>
    /// TEXTURE_CUBE_MAP_NEGATIVE_Z
    /// </summary>
    [Description("@#TEXTURE_CUBE_MAP_NEGATIVE_Z")]
    public const GLenum TEXTURE_CUBE_MAP_NEGATIVE_Z = 0x851A;

    /// <summary>
    /// MAX_CUBE_MAP_TEXTURE_SIZE
    /// </summary>
    [Description("@#MAX_CUBE_MAP_TEXTURE_SIZE")]
    public const GLenum MAX_CUBE_MAP_TEXTURE_SIZE = 0x851C;

    /// <summary>
    /// TEXTURE0
    /// </summary>
    [Description("@#TEXTURE0")]
    public const GLenum TEXTURE0 = 0x84C0;

    /// <summary>
    /// TEXTURE1
    /// </summary>
    [Description("@#TEXTURE1")]
    public const GLenum TEXTURE1 = 0x84C1;

    /// <summary>
    /// TEXTURE2
    /// </summary>
    [Description("@#TEXTURE2")]
    public const GLenum TEXTURE2 = 0x84C2;

    /// <summary>
    /// TEXTURE3
    /// </summary>
    [Description("@#TEXTURE3")]
    public const GLenum TEXTURE3 = 0x84C3;

    /// <summary>
    /// TEXTURE4
    /// </summary>
    [Description("@#TEXTURE4")]
    public const GLenum TEXTURE4 = 0x84C4;

    /// <summary>
    /// TEXTURE5
    /// </summary>
    [Description("@#TEXTURE5")]
    public const GLenum TEXTURE5 = 0x84C5;

    /// <summary>
    /// TEXTURE6
    /// </summary>
    [Description("@#TEXTURE6")]
    public const GLenum TEXTURE6 = 0x84C6;

    /// <summary>
    /// TEXTURE7
    /// </summary>
    [Description("@#TEXTURE7")]
    public const GLenum TEXTURE7 = 0x84C7;

    /// <summary>
    /// TEXTURE8
    /// </summary>
    [Description("@#TEXTURE8")]
    public const GLenum TEXTURE8 = 0x84C8;

    /// <summary>
    /// TEXTURE9
    /// </summary>
    [Description("@#TEXTURE9")]
    public const GLenum TEXTURE9 = 0x84C9;

    /// <summary>
    /// TEXTURE10
    /// </summary>
    [Description("@#TEXTURE10")]
    public const GLenum TEXTURE10 = 0x84CA;

    /// <summary>
    /// TEXTURE11
    /// </summary>
    [Description("@#TEXTURE11")]
    public const GLenum TEXTURE11 = 0x84CB;

    /// <summary>
    /// TEXTURE12
    /// </summary>
    [Description("@#TEXTURE12")]
    public const GLenum TEXTURE12 = 0x84CC;

    /// <summary>
    /// TEXTURE13
    /// </summary>
    [Description("@#TEXTURE13")]
    public const GLenum TEXTURE13 = 0x84CD;

    /// <summary>
    /// TEXTURE14
    /// </summary>
    [Description("@#TEXTURE14")]
    public const GLenum TEXTURE14 = 0x84CE;

    /// <summary>
    /// TEXTURE15
    /// </summary>
    [Description("@#TEXTURE15")]
    public const GLenum TEXTURE15 = 0x84CF;

    /// <summary>
    /// TEXTURE16
    /// </summary>
    [Description("@#TEXTURE16")]
    public const GLenum TEXTURE16 = 0x84D0;

    /// <summary>
    /// TEXTURE17
    /// </summary>
    [Description("@#TEXTURE17")]
    public const GLenum TEXTURE17 = 0x84D1;

    /// <summary>
    /// TEXTURE18
    /// </summary>
    [Description("@#TEXTURE18")]
    public const GLenum TEXTURE18 = 0x84D2;

    /// <summary>
    /// TEXTURE19
    /// </summary>
    [Description("@#TEXTURE19")]
    public const GLenum TEXTURE19 = 0x84D3;

    /// <summary>
    /// TEXTURE20
    /// </summary>
    [Description("@#TEXTURE20")]
    public const GLenum TEXTURE20 = 0x84D4;

    /// <summary>
    /// TEXTURE21
    /// </summary>
    [Description("@#TEXTURE21")]
    public const GLenum TEXTURE21 = 0x84D5;

    /// <summary>
    /// TEXTURE22
    /// </summary>
    [Description("@#TEXTURE22")]
    public const GLenum TEXTURE22 = 0x84D6;

    /// <summary>
    /// TEXTURE23
    /// </summary>
    [Description("@#TEXTURE23")]
    public const GLenum TEXTURE23 = 0x84D7;

    /// <summary>
    /// TEXTURE24
    /// </summary>
    [Description("@#TEXTURE24")]
    public const GLenum TEXTURE24 = 0x84D8;

    /// <summary>
    /// TEXTURE25
    /// </summary>
    [Description("@#TEXTURE25")]
    public const GLenum TEXTURE25 = 0x84D9;

    /// <summary>
    /// TEXTURE26
    /// </summary>
    [Description("@#TEXTURE26")]
    public const GLenum TEXTURE26 = 0x84DA;

    /// <summary>
    /// TEXTURE27
    /// </summary>
    [Description("@#TEXTURE27")]
    public const GLenum TEXTURE27 = 0x84DB;

    /// <summary>
    /// TEXTURE28
    /// </summary>
    [Description("@#TEXTURE28")]
    public const GLenum TEXTURE28 = 0x84DC;

    /// <summary>
    /// TEXTURE29
    /// </summary>
    [Description("@#TEXTURE29")]
    public const GLenum TEXTURE29 = 0x84DD;

    /// <summary>
    /// TEXTURE30
    /// </summary>
    [Description("@#TEXTURE30")]
    public const GLenum TEXTURE30 = 0x84DE;

    /// <summary>
    /// TEXTURE31
    /// </summary>
    [Description("@#TEXTURE31")]
    public const GLenum TEXTURE31 = 0x84DF;

    /// <summary>
    /// ACTIVE_TEXTURE
    /// </summary>
    [Description("@#ACTIVE_TEXTURE")]
    public const GLenum ACTIVE_TEXTURE = 0x84E0;

    /// <summary>
    /// REPEAT
    /// </summary>
    [Description("@#REPEAT")]
    public const GLenum REPEAT = 0x2901;

    /// <summary>
    /// CLAMP_TO_EDGE
    /// </summary>
    [Description("@#CLAMP_TO_EDGE")]
    public const GLenum CLAMP_TO_EDGE = 0x812F;

    /// <summary>
    /// MIRRORED_REPEAT
    /// </summary>
    [Description("@#MIRRORED_REPEAT")]
    public const GLenum MIRRORED_REPEAT = 0x8370;

    /// <summary>
    /// FLOAT_VEC2
    /// </summary>
    [Description("@#FLOAT_VEC2")]
    public const GLenum FLOATVEC2 = 0x8B50;

    /// <summary>
    /// FLOAT_VEC3
    /// </summary>
    [Description("@#FLOAT_VEC3")]
    public const GLenum FLOATVEC3 = 0x8B51;

    /// <summary>
    /// FLOAT_VEC4
    /// </summary>
    [Description("@#FLOAT_VEC4")]
    public const GLenum FLOATVEC4 = 0x8B52;

    /// <summary>
    /// INT_VEC2
    /// </summary>
    [Description("@#INT_VEC2")]
    public const GLenum INTVEC2 = 0x8B53;

    /// <summary>
    /// INT_VEC3
    /// </summary>
    [Description("@#INT_VEC3")]
    public const GLenum INTVEC3 = 0x8B54;

    /// <summary>
    /// INT_VEC4
    /// </summary>
    [Description("@#INT_VEC4")]
    public const GLenum INTVEC4 = 0x8B55;

    /// <summary>
    /// BOOL
    /// </summary>
    [Description("@#BOOL")]
    public const GLenum BOOL = 0x8B56;

    /// <summary>
    /// BOOL_VEC2
    /// </summary>
    [Description("@#BOOL_VEC2")]
    public const GLenum BOOLVEC2 = 0x8B57;

    /// <summary>
    /// BOOL_VEC3
    /// </summary>
    [Description("@#BOOL_VEC3")]
    public const GLenum BOOLVEC3 = 0x8B58;

    /// <summary>
    /// BOOL_VEC4
    /// </summary>
    [Description("@#BOOL_VEC4")]
    public const GLenum BOOLVEC4 = 0x8B59;

    /// <summary>
    /// FLOAT_MAT2
    /// </summary>
    [Description("@#FLOAT_MAT2")]
    public const GLenum FLOATMAT2 = 0x8B5A;

    /// <summary>
    /// FLOAT_MAT3
    /// </summary>
    [Description("@#FLOAT_MAT3")]
    public const GLenum FLOATMAT3 = 0x8B5B;

    /// <summary>
    /// FLOAT_MAT4
    /// </summary>
    [Description("@#FLOAT_MAT4")]
    public const GLenum FLOATMAT4 = 0x8B5C;

    /// <summary>
    /// SAMPLER_2D
    /// </summary>
    [Description("@#SAMPLER_2D")]
    public const GLenum SAMPLER2D = 0x8B5E;

    /// <summary>
    /// SAMPLER_CUBE
    /// </summary>
    [Description("@#SAMPLER_CUBE")]
    public const GLenum SAMPLER_CUBE = 0x8B60;

    /// <summary>
    /// VERTEX_ATTRIB_ARRAY_ENABLED
    /// </summary>
    [Description("@#VERTEX_ATTRIB_ARRAY_ENABLED")]
    public const GLenum VERTEX_ATTRIB_ARRAY_ENABLED = 0x8622;

    /// <summary>
    /// VERTEX_ATTRIB_ARRAY_SIZE
    /// </summary>
    [Description("@#VERTEX_ATTRIB_ARRAY_SIZE")]
    public const GLenum VERTEX_ATTRIB_ARRAY_SIZE = 0x8623;

    /// <summary>
    /// VERTEX_ATTRIB_ARRAY_STRIDE
    /// </summary>
    [Description("@#VERTEX_ATTRIB_ARRAY_STRIDE")]
    public const GLenum VERTEX_ATTRIB_ARRAY_STRIDE = 0x8624;

    /// <summary>
    /// VERTEX_ATTRIB_ARRAY_TYPE
    /// </summary>
    [Description("@#VERTEX_ATTRIB_ARRAY_TYPE")]
    public const GLenum VERTEX_ATTRIB_ARRAY_TYPE = 0x8625;

    /// <summary>
    /// VERTEX_ATTRIB_ARRAY_NORMALIZED
    /// </summary>
    [Description("@#VERTEX_ATTRIB_ARRAY_NORMALIZED")]
    public const GLenum VERTEX_ATTRIB_ARRAY_NORMALIZED = 0x886A;

    /// <summary>
    /// VERTEX_ATTRIB_ARRAY_POINTER
    /// </summary>
    [Description("@#VERTEX_ATTRIB_ARRAY_POINTER")]
    public const GLenum VERTEX_ATTRIB_ARRAY_POINTER = 0x8645;

    /// <summary>
    /// VERTEX_ATTRIB_ARRAY_BUFFER_BINDING
    /// </summary>
    [Description("@#VERTEX_ATTRIB_ARRAY_BUFFER_BINDING")]
    public const GLenum VERTEX_ATTRIB_ARRAY_BUFFER_BINDING = 0x889F;

    /// <summary>
    /// IMPLEMENTATION_COLOR_READ_TYPE
    /// </summary>
    [Description("@#IMPLEMENTATION_COLOR_READ_TYPE")]
    public const GLenum IMPLEMENTATION_COLOR_READ_TYPE = 0x8B9A;

    /// <summary>
    /// IMPLEMENTATION_COLOR_READ_FORMAT
    /// </summary>
    [Description("@#IMPLEMENTATION_COLOR_READ_FORMAT")]
    public const GLenum IMPLEMENTATION_COLOR_READ_FORMAT = 0x8B9B;

    /// <summary>
    /// COMPILE_STATUS
    /// </summary>
    [Description("@#COMPILE_STATUS")]
    public const GLenum COMPILE_STATUS = 0x8B81;

    /// <summary>
    /// LOW_FLOAT
    /// </summary>
    [Description("@#LOW_FLOAT")]
    public const GLenum LOW_FLOAT = 0x8DF0;

    /// <summary>
    /// MEDIUM_FLOAT
    /// </summary>
    [Description("@#MEDIUM_FLOAT")]
    public const GLenum MEDIUM_FLOAT = 0x8DF1;

    /// <summary>
    /// HIGH_FLOAT
    /// </summary>
    [Description("@#HIGH_FLOAT")]
    public const GLenum HIGH_FLOAT = 0x8DF2;

    /// <summary>
    /// LOW_INT
    /// </summary>
    [Description("@#LOW_INT")]
    public const GLenum LOW_INT = 0x8DF3;

    /// <summary>
    /// MEDIUM_INT
    /// </summary>
    [Description("@#MEDIUM_INT")]
    public const GLenum MEDIUM_INT = 0x8DF4;

    /// <summary>
    /// HIGH_INT
    /// </summary>
    [Description("@#HIGH_INT")]
    public const GLenum HIGH_INT = 0x8DF5;

    /// <summary>
    /// FRAMEBUFFER
    /// </summary>
    [Description("@#FRAMEBUFFER")]
    public const GLenum FRAMEBUFFER = 0x8D40;

    /// <summary>
    /// RENDERBUFFER
    /// </summary>
    [Description("@#RENDERBUFFER")]
    public const GLenum RENDERBUFFER = 0x8D41;

    /// <summary>
    /// RGBA4
    /// </summary>
    [Description("@#RGBA4")]
    public const GLenum RGBA4 = 0x8056;

    /// <summary>
    /// RGB5_A1
    /// </summary>
    [Description("@#RGB5_A1")]
    public const GLenum RGB5A1 = 0x8057;

    /// <summary>
    /// RGBA8
    /// </summary>
    [Description("@#RGBA8")]
    public const GLenum RGBA8 = 0x8058;

    /// <summary>
    /// RGB565
    /// </summary>
    [Description("@#RGB565")]
    public const GLenum RGB565 = 0x8D62;

    /// <summary>
    /// DEPTH_COMPONENT16
    /// </summary>
    [Description("@#DEPTH_COMPONENT16")]
    public const GLenum DEPTHCOMPONENT16 = 0x81A5;

    /// <summary>
    /// STENCIL_INDEX8
    /// </summary>
    [Description("@#STENCIL_INDEX8")]
    public const GLenum STENCILINDEX8 = 0x8D48;

    /// <summary>
    /// DEPTH_STENCIL
    /// </summary>
    [Description("@#DEPTH_STENCIL")]
    public const GLenum DEPTH_STENCIL = 0x84F9;

    /// <summary>
    /// RENDERBUFFER_WIDTH
    /// </summary>
    [Description("@#RENDERBUFFER_WIDTH")]
    public const GLenum RENDERBUFFER_WIDTH = 0x8D42;

    /// <summary>
    /// RENDERBUFFER_HEIGHT
    /// </summary>
    [Description("@#RENDERBUFFER_HEIGHT")]
    public const GLenum RENDERBUFFER_HEIGHT = 0x8D43;

    /// <summary>
    /// RENDERBUFFER_INTERNAL_FORMAT
    /// </summary>
    [Description("@#RENDERBUFFER_INTERNAL_FORMAT")]
    public const GLenum RENDERBUFFER_INTERNAL_FORMAT = 0x8D44;

    /// <summary>
    /// RENDERBUFFER_RED_SIZE
    /// </summary>
    [Description("@#RENDERBUFFER_RED_SIZE")]
    public const GLenum RENDERBUFFER_RED_SIZE = 0x8D50;

    /// <summary>
    /// RENDERBUFFER_GREEN_SIZE
    /// </summary>
    [Description("@#RENDERBUFFER_GREEN_SIZE")]
    public const GLenum RENDERBUFFER_GREEN_SIZE = 0x8D51;

    /// <summary>
    /// RENDERBUFFER_BLUE_SIZE
    /// </summary>
    [Description("@#RENDERBUFFER_BLUE_SIZE")]
    public const GLenum RENDERBUFFER_BLUE_SIZE = 0x8D52;

    /// <summary>
    /// RENDERBUFFER_ALPHA_SIZE
    /// </summary>
    [Description("@#RENDERBUFFER_ALPHA_SIZE")]
    public const GLenum RENDERBUFFER_ALPHA_SIZE = 0x8D53;

    /// <summary>
    /// RENDERBUFFER_DEPTH_SIZE
    /// </summary>
    [Description("@#RENDERBUFFER_DEPTH_SIZE")]
    public const GLenum RENDERBUFFER_DEPTH_SIZE = 0x8D54;

    /// <summary>
    /// RENDERBUFFER_STENCIL_SIZE
    /// </summary>
    [Description("@#RENDERBUFFER_STENCIL_SIZE")]
    public const GLenum RENDERBUFFER_STENCIL_SIZE = 0x8D55;

    /// <summary>
    /// FRAMEBUFFER_ATTACHMENT_OBJECT_TYPE
    /// </summary>
    [Description("@#FRAMEBUFFER_ATTACHMENT_OBJECT_TYPE")]
    public const GLenum FRAMEBUFFER_ATTACHMENT_OBJECT_TYPE = 0x8CD0;

    /// <summary>
    /// FRAMEBUFFER_ATTACHMENT_OBJECT_NAME
    /// </summary>
    [Description("@#FRAMEBUFFER_ATTACHMENT_OBJECT_NAME")]
    public const GLenum FRAMEBUFFER_ATTACHMENT_OBJECT_NAME = 0x8CD1;

    /// <summary>
    /// FRAMEBUFFER_ATTACHMENT_TEXTURE_LEVEL
    /// </summary>
    [Description("@#FRAMEBUFFER_ATTACHMENT_TEXTURE_LEVEL")]
    public const GLenum FRAMEBUFFER_ATTACHMENT_TEXTURE_LEVEL = 0x8CD2;

    /// <summary>
    /// FRAMEBUFFER_ATTACHMENT_TEXTURE_CUBE_MAP_FACE
    /// </summary>
    [Description("@#FRAMEBUFFER_ATTACHMENT_TEXTURE_CUBE_MAP_FACE")]
    public const GLenum FRAMEBUFFER_ATTACHMENT_TEXTURE_CUBE_MAP_FACE = 0x8CD3;

    /// <summary>
    /// COLOR_ATTACHMENT0
    /// </summary>
    [Description("@#COLOR_ATTACHMENT0")]
    public const GLenum COLORATTACHMENT0 = 0x8CE0;

    /// <summary>
    /// DEPTH_ATTACHMENT
    /// </summary>
    [Description("@#DEPTH_ATTACHMENT")]
    public const GLenum DEPTH_ATTACHMENT = 0x8D00;

    /// <summary>
    /// STENCIL_ATTACHMENT
    /// </summary>
    [Description("@#STENCIL_ATTACHMENT")]
    public const GLenum STENCIL_ATTACHMENT = 0x8D20;

    /// <summary>
    /// DEPTH_STENCIL_ATTACHMENT
    /// </summary>
    [Description("@#DEPTH_STENCIL_ATTACHMENT")]
    public const GLenum DEPTH_STENCIL_ATTACHMENT = 0x821A;

    /// <summary>
    /// NONE
    /// </summary>
    [Description("@#NONE")]
    public const GLenum NONE = 0;

    /// <summary>
    /// FRAMEBUFFER_COMPLETE
    /// </summary>
    [Description("@#FRAMEBUFFER_COMPLETE")]
    public const GLenum FRAMEBUFFER_COMPLETE = 0x8CD5;

    /// <summary>
    /// FRAMEBUFFER_INCOMPLETE_ATTACHMENT
    /// </summary>
    [Description("@#FRAMEBUFFER_INCOMPLETE_ATTACHMENT")]
    public const GLenum FRAMEBUFFER_INCOMPLETE_ATTACHMENT = 0x8CD6;

    /// <summary>
    /// FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT
    /// </summary>
    [Description("@#FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT")]
    public const GLenum FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT = 0x8CD7;

    /// <summary>
    /// FRAMEBUFFER_INCOMPLETE_DIMENSIONS
    /// </summary>
    [Description("@#FRAMEBUFFER_INCOMPLETE_DIMENSIONS")]
    public const GLenum FRAMEBUFFER_INCOMPLETE_DIMENSIONS = 0x8CD9;

    /// <summary>
    /// FRAMEBUFFER_UNSUPPORTED
    /// </summary>
    [Description("@#FRAMEBUFFER_UNSUPPORTED")]
    public const GLenum FRAMEBUFFER_UNSUPPORTED = 0x8CDD;

    /// <summary>
    /// FRAMEBUFFER_BINDING
    /// </summary>
    [Description("@#FRAMEBUFFER_BINDING")]
    public const GLenum FRAMEBUFFER_BINDING = 0x8CA6;

    /// <summary>
    /// RENDERBUFFER_BINDING
    /// </summary>
    [Description("@#RENDERBUFFER_BINDING")]
    public const GLenum RENDERBUFFER_BINDING = 0x8CA7;

    /// <summary>
    /// MAX_RENDERBUFFER_SIZE
    /// </summary>
    [Description("@#MAX_RENDERBUFFER_SIZE")]
    public const GLenum MAX_RENDERBUFFER_SIZE = 0x84E8;

    /// <summary>
    /// INVALID_FRAMEBUFFER_OPERATION
    /// </summary>
    [Description("@#INVALID_FRAMEBUFFER_OPERATION")]
    public const GLenum INVALID_FRAMEBUFFER_OPERATION = 0x0506;

    /// <summary>
    /// UNPACK_FLIP_Y_WEBGL
    /// </summary>
    [Description("@#UNPACK_FLIP_Y_WEBGL")]
    public const GLenum UNPACK_FLIP_Y_WEBGL = 0x9240;

    /// <summary>
    /// UNPACK_PREMULTIPLY_ALPHA_WEBGL
    /// </summary>
    [Description("@#UNPACK_PREMULTIPLY_ALPHA_WEBGL")]
    public const GLenum UNPACK_PREMULTIPLY_ALPHA_WEBGL = 0x9241;

    /// <summary>
    /// CONTEXT_LOST_WEBGL
    /// </summary>
    [Description("@#CONTEXT_LOST_WEBGL")]
    public const GLenum CONTEXT_LOST_WEBGL = 0x9242;

    /// <summary>
    /// UNPACK_COLORSPACE_CONVERSION_WEBGL
    /// </summary>
    [Description("@#UNPACK_COLORSPACE_CONVERSION_WEBGL")]
    public const GLenum UNPACK_COLORSPACE_CONVERSION_WEBGL = 0x9243;

    /// <summary>
    /// BROWSER_DEFAULT_WEBGL
    /// </summary>
    [Description("@#BROWSER_DEFAULT_WEBGL")]
    public const GLenum BROWSER_DEFAULT_WEBGL = 0x9244;

    /// <summary>
    /// canvas
    /// </summary>
    [Description("@#canvas")]
    public extern Either<HTMLCanvasElement, OffscreenCanvas> Canvas { get; }

    /// <summary>
    /// drawingBufferWidth
    /// </summary>
    [Description("@#drawingBufferWidth")]
    public extern GLsizei DrawingBufferWidth { get; }

    /// <summary>
    /// drawingBufferHeight
    /// </summary>
    [Description("@#drawingBufferHeight")]
    public extern GLsizei DrawingBufferHeight { get; }

    /// <summary>
    /// drawingBufferFormat
    /// </summary>
    [Description("@#drawingBufferFormat")]
    public extern GLenum DrawingBufferFormat { get; }

    /// <summary>
    /// drawingBufferColorSpace
    /// </summary>
    [Description("@#drawingBufferColorSpace")]
    public extern PredefinedColorSpace DrawingBufferColorSpace { get; set; }

    /// <summary>
    /// unpackColorSpace
    /// </summary>
    [Description("@#unpackColorSpace")]
    public extern PredefinedColorSpace UnpackColorSpace { get; set; }

    /// <summary>
    /// getContextAttributes
    /// </summary>
    [Description("@#getContextAttributes")]
    public extern WebGLContextAttributes? GetContextAttributes();

    /// <summary>
    /// isContextLost
    /// </summary>
    [Description("@#isContextLost")]
    public extern bool IsContextLost();

    /// <summary>
    /// getSupportedExtensions
    /// </summary>
    [Description("@#getSupportedExtensions")]
    public extern string[]? GetSupportedExtensions();

    /// <summary>
    /// getExtension
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#getExtension")]
    public extern object? GetExtension(string name);

    /// <summary>
    /// drawingBufferStorage
    /// </summary>
    /// <param name="sizedFormat">sizedFormat</param>
    /// <param name="width">width</param>
    /// <param name="height">height</param>
    [Description("@#drawingBufferStorage")]
    public extern void DrawingBufferStorage(GLenum sizedFormat, uint width, uint height);

    /// <summary>
    /// activeTexture
    /// </summary>
    /// <param name="texture">texture</param>
    [Description("@#activeTexture")]
    public extern void ActiveTexture(GLenum texture);

    /// <summary>
    /// attachShader
    /// </summary>
    /// <param name="program">program</param>
    /// <param name="shader">shader</param>
    [Description("@#attachShader")]
    public extern void AttachShader(WebGLProgram program, WebGLShader shader);

    /// <summary>
    /// bindAttribLocation
    /// </summary>
    /// <param name="program">program</param>
    /// <param name="index">index</param>
    /// <param name="name">name</param>
    [Description("@#bindAttribLocation")]
    public extern void BindAttribLocation(WebGLProgram program, GLuint index, string name);

    /// <summary>
    /// bindBuffer
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="buffer">buffer</param>
    [Description("@#bindBuffer")]
    public extern void BindBuffer(GLenum target, WebGLBuffer? buffer);

    /// <summary>
    /// bindFramebuffer
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="framebuffer">framebuffer</param>
    [Description("@#bindFramebuffer")]
    public extern void BindFramebuffer(GLenum target, WebGLFramebuffer? framebuffer);

    /// <summary>
    /// bindRenderbuffer
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="renderbuffer">renderbuffer</param>
    [Description("@#bindRenderbuffer")]
    public extern void BindRenderbuffer(GLenum target, WebGLRenderbuffer? renderbuffer);

    /// <summary>
    /// bindTexture
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="texture">texture</param>
    [Description("@#bindTexture")]
    public extern void BindTexture(GLenum target, WebGLTexture? texture);

    /// <summary>
    /// blendColor
    /// </summary>
    /// <param name="red">red</param>
    /// <param name="green">green</param>
    /// <param name="blue">blue</param>
    /// <param name="alpha">alpha</param>
    [Description("@#blendColor")]
    public extern void BlendColor(GLclampf red, GLclampf green, GLclampf blue, GLclampf alpha);

    /// <summary>
    /// blendEquation
    /// </summary>
    /// <param name="mode">mode</param>
    [Description("@#blendEquation")]
    public extern void BlendEquation(GLenum mode);

    /// <summary>
    /// blendEquationSeparate
    /// </summary>
    /// <param name="modeRgb">modeRGB</param>
    /// <param name="modeAlpha">modeAlpha</param>
    [Description("@#blendEquationSeparate")]
    public extern void BlendEquationSeparate(GLenum modeRgb, GLenum modeAlpha);

    /// <summary>
    /// blendFunc
    /// </summary>
    /// <param name="sfactor">sfactor</param>
    /// <param name="dfactor">dfactor</param>
    [Description("@#blendFunc")]
    public extern void BlendFunc(GLenum sfactor, GLenum dfactor);

    /// <summary>
    /// blendFuncSeparate
    /// </summary>
    /// <param name="srcRgb">srcRGB</param>
    /// <param name="dstRgb">dstRGB</param>
    /// <param name="srcAlpha">srcAlpha</param>
    /// <param name="dstAlpha">dstAlpha</param>
    [Description("@#blendFuncSeparate")]
    public extern void BlendFuncSeparate(GLenum srcRgb, GLenum dstRgb, GLenum srcAlpha, GLenum dstAlpha);

    /// <summary>
    /// checkFramebufferStatus
    /// </summary>
    /// <param name="target">target</param>
    [Description("@#checkFramebufferStatus")]
    public extern GLenum CheckFramebufferStatus(GLenum target);

    /// <summary>
    /// clear
    /// </summary>
    /// <param name="mask">mask</param>
    [Description("@#clear")]
    public extern void Clear(GLbitfield mask);

    /// <summary>
    /// clearColor
    /// </summary>
    /// <param name="red">red</param>
    /// <param name="green">green</param>
    /// <param name="blue">blue</param>
    /// <param name="alpha">alpha</param>
    [Description("@#clearColor")]
    public extern void ClearColor(GLclampf red, GLclampf green, GLclampf blue, GLclampf alpha);

    /// <summary>
    /// clearDepth
    /// </summary>
    /// <param name="depth">depth</param>
    [Description("@#clearDepth")]
    public extern void ClearDepth(GLclampf depth);

    /// <summary>
    /// clearStencil
    /// </summary>
    /// <param name="s">s</param>
    [Description("@#clearStencil")]
    public extern void ClearStencil(GLint s);

    /// <summary>
    /// colorMask
    /// </summary>
    /// <param name="red">red</param>
    /// <param name="green">green</param>
    /// <param name="blue">blue</param>
    /// <param name="alpha">alpha</param>
    [Description("@#colorMask")]
    public extern void ColorMask(GLboolean red, GLboolean green, GLboolean blue, GLboolean alpha);

    /// <summary>
    /// compileShader
    /// </summary>
    /// <param name="shader">shader</param>
    [Description("@#compileShader")]
    public extern void CompileShader(WebGLShader shader);

    /// <summary>
    /// copyTexImage2D
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="level">level</param>
    /// <param name="internalformat">internalformat</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="width">width</param>
    /// <param name="height">height</param>
    /// <param name="border">border</param>
    [Description("@#copyTexImage2D")]
    public extern void CopyTexImage2D(GLenum target, GLint level, GLenum internalformat, GLint x, GLint y, GLsizei width, GLsizei height, GLint border);

    /// <summary>
    /// copyTexSubImage2D
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="level">level</param>
    /// <param name="xoffset">xoffset</param>
    /// <param name="yoffset">yoffset</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="width">width</param>
    /// <param name="height">height</param>
    [Description("@#copyTexSubImage2D")]
    public extern void CopyTexSubImage2D(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint x, GLint y, GLsizei width, GLsizei height);

    /// <summary>
    /// createBuffer
    /// </summary>
    [Description("@#createBuffer")]
    public extern WebGLBuffer CreateBuffer();

    /// <summary>
    /// createFramebuffer
    /// </summary>
    [Description("@#createFramebuffer")]
    public extern WebGLFramebuffer CreateFramebuffer();

    /// <summary>
    /// createProgram
    /// </summary>
    [Description("@#createProgram")]
    public extern WebGLProgram CreateProgram();

    /// <summary>
    /// createRenderbuffer
    /// </summary>
    [Description("@#createRenderbuffer")]
    public extern WebGLRenderbuffer CreateRenderbuffer();

    /// <summary>
    /// createShader
    /// </summary>
    /// <param name="type">type</param>
    [Description("@#createShader")]
    public extern WebGLShader? CreateShader(GLenum type);

    /// <summary>
    /// createTexture
    /// </summary>
    [Description("@#createTexture")]
    public extern WebGLTexture CreateTexture();

    /// <summary>
    /// cullFace
    /// </summary>
    /// <param name="mode">mode</param>
    [Description("@#cullFace")]
    public extern void CullFace(GLenum mode);

    /// <summary>
    /// deleteBuffer
    /// </summary>
    /// <param name="buffer">buffer</param>
    [Description("@#deleteBuffer")]
    public extern void DeleteBuffer(WebGLBuffer? buffer);

    /// <summary>
    /// deleteFramebuffer
    /// </summary>
    /// <param name="framebuffer">framebuffer</param>
    [Description("@#deleteFramebuffer")]
    public extern void DeleteFramebuffer(WebGLFramebuffer? framebuffer);

    /// <summary>
    /// deleteProgram
    /// </summary>
    /// <param name="program">program</param>
    [Description("@#deleteProgram")]
    public extern void DeleteProgram(WebGLProgram? program);

    /// <summary>
    /// deleteRenderbuffer
    /// </summary>
    /// <param name="renderbuffer">renderbuffer</param>
    [Description("@#deleteRenderbuffer")]
    public extern void DeleteRenderbuffer(WebGLRenderbuffer? renderbuffer);

    /// <summary>
    /// deleteShader
    /// </summary>
    /// <param name="shader">shader</param>
    [Description("@#deleteShader")]
    public extern void DeleteShader(WebGLShader? shader);

    /// <summary>
    /// deleteTexture
    /// </summary>
    /// <param name="texture">texture</param>
    [Description("@#deleteTexture")]
    public extern void DeleteTexture(WebGLTexture? texture);

    /// <summary>
    /// depthFunc
    /// </summary>
    /// <param name="func">func</param>
    [Description("@#depthFunc")]
    public extern void DepthFunc(GLenum func);

    /// <summary>
    /// depthMask
    /// </summary>
    /// <param name="flag">flag</param>
    [Description("@#depthMask")]
    public extern void DepthMask(GLboolean flag);

    /// <summary>
    /// depthRange
    /// </summary>
    /// <param name="zNear">zNear</param>
    /// <param name="zFar">zFar</param>
    [Description("@#depthRange")]
    public extern void DepthRange(GLclampf zNear, GLclampf zFar);

    /// <summary>
    /// detachShader
    /// </summary>
    /// <param name="program">program</param>
    /// <param name="shader">shader</param>
    [Description("@#detachShader")]
    public extern void DetachShader(WebGLProgram program, WebGLShader shader);

    /// <summary>
    /// disable
    /// </summary>
    /// <param name="cap">cap</param>
    [Description("@#disable")]
    public extern void Disable(GLenum cap);

    /// <summary>
    /// disableVertexAttribArray
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#disableVertexAttribArray")]
    public extern void DisableVertexAttribArray(GLuint index);

    /// <summary>
    /// drawArrays
    /// </summary>
    /// <param name="mode">mode</param>
    /// <param name="first">first</param>
    /// <param name="count">count</param>
    [Description("@#drawArrays")]
    public extern void DrawArrays(GLenum mode, GLint first, GLsizei count);

    /// <summary>
    /// drawElements
    /// </summary>
    /// <param name="mode">mode</param>
    /// <param name="count">count</param>
    /// <param name="type">type</param>
    /// <param name="offset">offset</param>
    [Description("@#drawElements")]
    public extern void DrawElements(GLenum mode, GLsizei count, GLenum type, GLintptr offset);

    /// <summary>
    /// enable
    /// </summary>
    /// <param name="cap">cap</param>
    [Description("@#enable")]
    public extern void Enable(GLenum cap);

    /// <summary>
    /// enableVertexAttribArray
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#enableVertexAttribArray")]
    public extern void EnableVertexAttribArray(GLuint index);

    /// <summary>
    /// finish
    /// </summary>
    [Description("@#finish")]
    public extern void Finish();

    /// <summary>
    /// flush
    /// </summary>
    [Description("@#flush")]
    public extern void Flush();

    /// <summary>
    /// framebufferRenderbuffer
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="attachment">attachment</param>
    /// <param name="renderbuffertarget">renderbuffertarget</param>
    /// <param name="renderbuffer">renderbuffer</param>
    [Description("@#framebufferRenderbuffer")]
    public extern void FramebufferRenderbuffer(GLenum target, GLenum attachment, GLenum renderbuffertarget, WebGLRenderbuffer? renderbuffer);

    /// <summary>
    /// framebufferTexture2D
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="attachment">attachment</param>
    /// <param name="textarget">textarget</param>
    /// <param name="texture">texture</param>
    /// <param name="level">level</param>
    [Description("@#framebufferTexture2D")]
    public extern void FramebufferTexture2D(GLenum target, GLenum attachment, GLenum textarget, WebGLTexture? texture, GLint level);

    /// <summary>
    /// frontFace
    /// </summary>
    /// <param name="mode">mode</param>
    [Description("@#frontFace")]
    public extern void FrontFace(GLenum mode);

    /// <summary>
    /// generateMipmap
    /// </summary>
    /// <param name="target">target</param>
    [Description("@#generateMipmap")]
    public extern void GenerateMipmap(GLenum target);

    /// <summary>
    /// getActiveAttrib
    /// </summary>
    /// <param name="program">program</param>
    /// <param name="index">index</param>
    [Description("@#getActiveAttrib")]
    public extern WebGLActiveInfo? GetActiveAttrib(WebGLProgram program, GLuint index);

    /// <summary>
    /// getActiveUniform
    /// </summary>
    /// <param name="program">program</param>
    /// <param name="index">index</param>
    [Description("@#getActiveUniform")]
    public extern WebGLActiveInfo? GetActiveUniform(WebGLProgram program, GLuint index);

    /// <summary>
    /// getAttachedShaders
    /// </summary>
    /// <param name="program">program</param>
    [Description("@#getAttachedShaders")]
    public extern WebGLShader[]? GetAttachedShaders(WebGLProgram program);

    /// <summary>
    /// getAttribLocation
    /// </summary>
    /// <param name="program">program</param>
    /// <param name="name">name</param>
    [Description("@#getAttribLocation")]
    public extern GLint GetAttribLocation(WebGLProgram program, string name);

    /// <summary>
    /// getBufferParameter
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="pname">pname</param>
    [Description("@#getBufferParameter")]
    public extern object GetBufferParameter(GLenum target, GLenum pname);

    /// <summary>
    /// getParameter
    /// </summary>
    /// <param name="pname">pname</param>
    [Description("@#getParameter")]
    public extern object GetParameter(GLenum pname);

    /// <summary>
    /// getError
    /// </summary>
    [Description("@#getError")]
    public extern GLenum GetError();

    /// <summary>
    /// getFramebufferAttachmentParameter
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="attachment">attachment</param>
    /// <param name="pname">pname</param>
    [Description("@#getFramebufferAttachmentParameter")]
    public extern object GetFramebufferAttachmentParameter(GLenum target, GLenum attachment, GLenum pname);

    /// <summary>
    /// getProgramParameter
    /// </summary>
    /// <param name="program">program</param>
    /// <param name="pname">pname</param>
    [Description("@#getProgramParameter")]
    public extern object GetProgramParameter(WebGLProgram program, GLenum pname);

    /// <summary>
    /// getProgramInfoLog
    /// </summary>
    /// <param name="program">program</param>
    [Description("@#getProgramInfoLog")]
    public extern string? GetProgramInfoLog(WebGLProgram program);

    /// <summary>
    /// getRenderbufferParameter
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="pname">pname</param>
    [Description("@#getRenderbufferParameter")]
    public extern object GetRenderbufferParameter(GLenum target, GLenum pname);

    /// <summary>
    /// getShaderParameter
    /// </summary>
    /// <param name="shader">shader</param>
    /// <param name="pname">pname</param>
    [Description("@#getShaderParameter")]
    public extern object GetShaderParameter(WebGLShader shader, GLenum pname);

    /// <summary>
    /// getShaderPrecisionFormat
    /// </summary>
    /// <param name="shadertype">shadertype</param>
    /// <param name="precisiontype">precisiontype</param>
    [Description("@#getShaderPrecisionFormat")]
    public extern WebGLShaderPrecisionFormat? GetShaderPrecisionFormat(GLenum shadertype, GLenum precisiontype);

    /// <summary>
    /// getShaderInfoLog
    /// </summary>
    /// <param name="shader">shader</param>
    [Description("@#getShaderInfoLog")]
    public extern string? GetShaderInfoLog(WebGLShader shader);

    /// <summary>
    /// getShaderSource
    /// </summary>
    /// <param name="shader">shader</param>
    [Description("@#getShaderSource")]
    public extern string? GetShaderSource(WebGLShader shader);

    /// <summary>
    /// getTexParameter
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="pname">pname</param>
    [Description("@#getTexParameter")]
    public extern object GetTexParameter(GLenum target, GLenum pname);

    /// <summary>
    /// getUniform
    /// </summary>
    /// <param name="program">program</param>
    /// <param name="location">location</param>
    [Description("@#getUniform")]
    public extern object GetUniform(WebGLProgram program, WebGLUniformLocation location);

    /// <summary>
    /// getUniformLocation
    /// </summary>
    /// <param name="program">program</param>
    /// <param name="name">name</param>
    [Description("@#getUniformLocation")]
    public extern WebGLUniformLocation? GetUniformLocation(WebGLProgram program, string name);

    /// <summary>
    /// getVertexAttrib
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="pname">pname</param>
    [Description("@#getVertexAttrib")]
    public extern object GetVertexAttrib(GLuint index, GLenum pname);

    /// <summary>
    /// getVertexAttribOffset
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="pname">pname</param>
    [Description("@#getVertexAttribOffset")]
    public extern GLintptr GetVertexAttribOffset(GLuint index, GLenum pname);

    /// <summary>
    /// hint
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="mode">mode</param>
    [Description("@#hint")]
    public extern void Hint(GLenum target, GLenum mode);

    /// <summary>
    /// isBuffer
    /// </summary>
    /// <param name="buffer">buffer</param>
    [Description("@#isBuffer")]
    public extern GLboolean IsBuffer(WebGLBuffer? buffer);

    /// <summary>
    /// isEnabled
    /// </summary>
    /// <param name="cap">cap</param>
    [Description("@#isEnabled")]
    public extern GLboolean IsEnabled(GLenum cap);

    /// <summary>
    /// isFramebuffer
    /// </summary>
    /// <param name="framebuffer">framebuffer</param>
    [Description("@#isFramebuffer")]
    public extern GLboolean IsFramebuffer(WebGLFramebuffer? framebuffer);

    /// <summary>
    /// isProgram
    /// </summary>
    /// <param name="program">program</param>
    [Description("@#isProgram")]
    public extern GLboolean IsProgram(WebGLProgram? program);

    /// <summary>
    /// isRenderbuffer
    /// </summary>
    /// <param name="renderbuffer">renderbuffer</param>
    [Description("@#isRenderbuffer")]
    public extern GLboolean IsRenderbuffer(WebGLRenderbuffer? renderbuffer);

    /// <summary>
    /// isShader
    /// </summary>
    /// <param name="shader">shader</param>
    [Description("@#isShader")]
    public extern GLboolean IsShader(WebGLShader? shader);

    /// <summary>
    /// isTexture
    /// </summary>
    /// <param name="texture">texture</param>
    [Description("@#isTexture")]
    public extern GLboolean IsTexture(WebGLTexture? texture);

    /// <summary>
    /// lineWidth
    /// </summary>
    /// <param name="width">width</param>
    [Description("@#lineWidth")]
    public extern void LineWidth(GLfloat width);

    /// <summary>
    /// linkProgram
    /// </summary>
    /// <param name="program">program</param>
    [Description("@#linkProgram")]
    public extern void LinkProgram(WebGLProgram program);

    /// <summary>
    /// pixelStorei
    /// </summary>
    /// <param name="pname">pname</param>
    /// <param name="param">param</param>
    [Description("@#pixelStorei")]
    public extern void PixelStorei(GLenum pname, GLint param);

    /// <summary>
    /// polygonOffset
    /// </summary>
    /// <param name="factor">factor</param>
    /// <param name="units">units</param>
    [Description("@#polygonOffset")]
    public extern void PolygonOffset(GLfloat factor, GLfloat units);

    /// <summary>
    /// renderbufferStorage
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="internalformat">internalformat</param>
    /// <param name="width">width</param>
    /// <param name="height">height</param>
    [Description("@#renderbufferStorage")]
    public extern void RenderbufferStorage(GLenum target, GLenum internalformat, GLsizei width, GLsizei height);

    /// <summary>
    /// sampleCoverage
    /// </summary>
    /// <param name="value">value</param>
    /// <param name="invert">invert</param>
    [Description("@#sampleCoverage")]
    public extern void SampleCoverage(GLclampf value, GLboolean invert);

    /// <summary>
    /// scissor
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="width">width</param>
    /// <param name="height">height</param>
    [Description("@#scissor")]
    public extern void Scissor(GLint x, GLint y, GLsizei width, GLsizei height);

    /// <summary>
    /// shaderSource
    /// </summary>
    /// <param name="shader">shader</param>
    /// <param name="source">source</param>
    [Description("@#shaderSource")]
    public extern void ShaderSource(WebGLShader shader, string source);

    /// <summary>
    /// stencilFunc
    /// </summary>
    /// <param name="func">func</param>
    /// <param name="ref">ref</param>
    /// <param name="mask">mask</param>
    [Description("@#stencilFunc")]
    public extern void StencilFunc(GLenum func, GLint @ref, GLuint mask);

    /// <summary>
    /// stencilFuncSeparate
    /// </summary>
    /// <param name="face">face</param>
    /// <param name="func">func</param>
    /// <param name="ref">ref</param>
    /// <param name="mask">mask</param>
    [Description("@#stencilFuncSeparate")]
    public extern void StencilFuncSeparate(GLenum face, GLenum func, GLint @ref, GLuint mask);

    /// <summary>
    /// stencilMask
    /// </summary>
    /// <param name="mask">mask</param>
    [Description("@#stencilMask")]
    public extern void StencilMask(GLuint mask);

    /// <summary>
    /// stencilMaskSeparate
    /// </summary>
    /// <param name="face">face</param>
    /// <param name="mask">mask</param>
    [Description("@#stencilMaskSeparate")]
    public extern void StencilMaskSeparate(GLenum face, GLuint mask);

    /// <summary>
    /// stencilOp
    /// </summary>
    /// <param name="fail">fail</param>
    /// <param name="zfail">zfail</param>
    /// <param name="zpass">zpass</param>
    [Description("@#stencilOp")]
    public extern void StencilOp(GLenum fail, GLenum zfail, GLenum zpass);

    /// <summary>
    /// stencilOpSeparate
    /// </summary>
    /// <param name="face">face</param>
    /// <param name="fail">fail</param>
    /// <param name="zfail">zfail</param>
    /// <param name="zpass">zpass</param>
    [Description("@#stencilOpSeparate")]
    public extern void StencilOpSeparate(GLenum face, GLenum fail, GLenum zfail, GLenum zpass);

    /// <summary>
    /// texParameterf
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="pname">pname</param>
    /// <param name="param">param</param>
    [Description("@#texParameterf")]
    public extern void TexParameterf(GLenum target, GLenum pname, GLfloat param);

    /// <summary>
    /// texParameteri
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="pname">pname</param>
    /// <param name="param">param</param>
    [Description("@#texParameteri")]
    public extern void TexParameteri(GLenum target, GLenum pname, GLint param);

    /// <summary>
    /// uniform1f
    /// </summary>
    /// <param name="location">location</param>
    /// <param name="x">x</param>
    [Description("@#uniform1f")]
    public extern void Uniform1f(WebGLUniformLocation? location, GLfloat x);

    /// <summary>
    /// uniform2f
    /// </summary>
    /// <param name="location">location</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#uniform2f")]
    public extern void Uniform2f(WebGLUniformLocation? location, GLfloat x, GLfloat y);

    /// <summary>
    /// uniform3f
    /// </summary>
    /// <param name="location">location</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="z">z</param>
    [Description("@#uniform3f")]
    public extern void Uniform3f(WebGLUniformLocation? location, GLfloat x, GLfloat y, GLfloat z);

    /// <summary>
    /// uniform4f
    /// </summary>
    /// <param name="location">location</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="z">z</param>
    /// <param name="w">w</param>
    [Description("@#uniform4f")]
    public extern void Uniform4f(WebGLUniformLocation? location, GLfloat x, GLfloat y, GLfloat z, GLfloat w);

    /// <summary>
    /// uniform1i
    /// </summary>
    /// <param name="location">location</param>
    /// <param name="x">x</param>
    [Description("@#uniform1i")]
    public extern void Uniform1i(WebGLUniformLocation? location, GLint x);

    /// <summary>
    /// uniform2i
    /// </summary>
    /// <param name="location">location</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#uniform2i")]
    public extern void Uniform2i(WebGLUniformLocation? location, GLint x, GLint y);

    /// <summary>
    /// uniform3i
    /// </summary>
    /// <param name="location">location</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="z">z</param>
    [Description("@#uniform3i")]
    public extern void Uniform3i(WebGLUniformLocation? location, GLint x, GLint y, GLint z);

    /// <summary>
    /// uniform4i
    /// </summary>
    /// <param name="location">location</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="z">z</param>
    /// <param name="w">w</param>
    [Description("@#uniform4i")]
    public extern void Uniform4i(WebGLUniformLocation? location, GLint x, GLint y, GLint z, GLint w);

    /// <summary>
    /// useProgram
    /// </summary>
    /// <param name="program">program</param>
    [Description("@#useProgram")]
    public extern void UseProgram(WebGLProgram? program);

    /// <summary>
    /// validateProgram
    /// </summary>
    /// <param name="program">program</param>
    [Description("@#validateProgram")]
    public extern void ValidateProgram(WebGLProgram program);

    /// <summary>
    /// vertexAttrib1f
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="x">x</param>
    [Description("@#vertexAttrib1f")]
    public extern void VertexAttrib1f(GLuint index, GLfloat x);

    /// <summary>
    /// vertexAttrib2f
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#vertexAttrib2f")]
    public extern void VertexAttrib2f(GLuint index, GLfloat x, GLfloat y);

    /// <summary>
    /// vertexAttrib3f
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="z">z</param>
    [Description("@#vertexAttrib3f")]
    public extern void VertexAttrib3f(GLuint index, GLfloat x, GLfloat y, GLfloat z);

    /// <summary>
    /// vertexAttrib4f
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="z">z</param>
    /// <param name="w">w</param>
    [Description("@#vertexAttrib4f")]
    public extern void VertexAttrib4f(GLuint index, GLfloat x, GLfloat y, GLfloat z, GLfloat w);

    /// <summary>
    /// vertexAttrib1fv
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="values">values</param>
    [Description("@#vertexAttrib1fv")]
    public extern void VertexAttrib1fv(GLuint index, Float32List values);

    /// <summary>
    /// vertexAttrib2fv
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="values">values</param>
    [Description("@#vertexAttrib2fv")]
    public extern void VertexAttrib2fv(GLuint index, Float32List values);

    /// <summary>
    /// vertexAttrib3fv
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="values">values</param>
    [Description("@#vertexAttrib3fv")]
    public extern void VertexAttrib3fv(GLuint index, Float32List values);

    /// <summary>
    /// vertexAttrib4fv
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="values">values</param>
    [Description("@#vertexAttrib4fv")]
    public extern void VertexAttrib4fv(GLuint index, Float32List values);

    /// <summary>
    /// vertexAttribPointer
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="size">size</param>
    /// <param name="type">type</param>
    /// <param name="normalized">normalized</param>
    /// <param name="stride">stride</param>
    /// <param name="offset">offset</param>
    [Description("@#vertexAttribPointer")]
    public extern void VertexAttribPointer(GLuint index, GLint size, GLenum type, GLboolean normalized, GLsizei stride, GLintptr offset);

    /// <summary>
    /// viewport
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="width">width</param>
    /// <param name="height">height</param>
    [Description("@#viewport")]
    public extern void Viewport(GLint x, GLint y, GLsizei width, GLsizei height);

    /// <summary>
    /// makeXRCompatible
    /// </summary>
    [Description("@#makeXRCompatible")]
    public extern PromiseResult<object> MakeXRCompatible();
    #endregion

    #region mixin WebGLRenderingContextOverloads
    /// <summary>
    /// bufferData
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="size">size</param>
    /// <param name="usage">usage</param>
    [Description("@#bufferData")]
    public extern void BufferData(GLenum target, GLsizeiptr size, GLenum usage);

    /// <summary>
    /// bufferData
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="data">data</param>
    /// <param name="usage">usage</param>
    [Description("@#bufferData")]
    public extern void BufferData(GLenum target, IAllowSharedBufferSource? data, GLenum usage);

    /// <summary>
    /// bufferSubData
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="offset">offset</param>
    /// <param name="data">data</param>
    [Description("@#bufferSubData")]
    public extern void BufferSubData(GLenum target, GLintptr offset, IAllowSharedBufferSource data);

    /// <summary>
    /// compressedTexImage2D
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="level">level</param>
    /// <param name="internalformat">internalformat</param>
    /// <param name="width">width</param>
    /// <param name="height">height</param>
    /// <param name="border">border</param>
    /// <param name="data">data</param>
    [Description("@#compressedTexImage2D")]
    public extern void CompressedTexImage2D(GLenum target, GLint level, GLenum internalformat, GLsizei width, GLsizei height, GLint border, IArrayBufferView data);

    /// <summary>
    /// compressedTexSubImage2D
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="level">level</param>
    /// <param name="xoffset">xoffset</param>
    /// <param name="yoffset">yoffset</param>
    /// <param name="width">width</param>
    /// <param name="height">height</param>
    /// <param name="format">format</param>
    /// <param name="data">data</param>
    [Description("@#compressedTexSubImage2D")]
    public extern void CompressedTexSubImage2D(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height, GLenum format, IArrayBufferView data);

    /// <summary>
    /// readPixels
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="width">width</param>
    /// <param name="height">height</param>
    /// <param name="format">format</param>
    /// <param name="type">type</param>
    /// <param name="pixels">pixels</param>
    [Description("@#readPixels")]
    public extern void ReadPixels(GLint x, GLint y, GLsizei width, GLsizei height, GLenum format, GLenum type, IArrayBufferView? pixels);

    /// <summary>
    /// texImage2D
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="level">level</param>
    /// <param name="internalformat">internalformat</param>
    /// <param name="width">width</param>
    /// <param name="height">height</param>
    /// <param name="border">border</param>
    /// <param name="format">format</param>
    /// <param name="type">type</param>
    /// <param name="pixels">pixels</param>
    [Description("@#texImage2D")]
    public extern void TexImage2D(GLenum target, GLint level, GLint internalformat, GLsizei width, GLsizei height, GLint border, GLenum format, GLenum type, IArrayBufferView? pixels);

    /// <summary>
    /// texImage2D
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="level">level</param>
    /// <param name="internalformat">internalformat</param>
    /// <param name="format">format</param>
    /// <param name="type">type</param>
    /// <param name="source">source</param>
    [Description("@#texImage2D")]
    public extern void TexImage2D(GLenum target, GLint level, GLint internalformat, GLenum format, GLenum type, TexImageSource source);

    /// <summary>
    /// texSubImage2D
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="level">level</param>
    /// <param name="xoffset">xoffset</param>
    /// <param name="yoffset">yoffset</param>
    /// <param name="width">width</param>
    /// <param name="height">height</param>
    /// <param name="format">format</param>
    /// <param name="type">type</param>
    /// <param name="pixels">pixels</param>
    [Description("@#texSubImage2D")]
    public extern void TexSubImage2D(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height, GLenum format, GLenum type, IArrayBufferView? pixels);

    /// <summary>
    /// texSubImage2D
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="level">level</param>
    /// <param name="xoffset">xoffset</param>
    /// <param name="yoffset">yoffset</param>
    /// <param name="format">format</param>
    /// <param name="type">type</param>
    /// <param name="source">source</param>
    [Description("@#texSubImage2D")]
    public extern void TexSubImage2D(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLenum format, GLenum type, TexImageSource source);

    /// <summary>
    /// uniform1fv
    /// </summary>
    /// <param name="location">location</param>
    /// <param name="v">v</param>
    [Description("@#uniform1fv")]
    public extern void Uniform1fv(WebGLUniformLocation? location, Float32List v);

    /// <summary>
    /// uniform2fv
    /// </summary>
    /// <param name="location">location</param>
    /// <param name="v">v</param>
    [Description("@#uniform2fv")]
    public extern void Uniform2fv(WebGLUniformLocation? location, Float32List v);

    /// <summary>
    /// uniform3fv
    /// </summary>
    /// <param name="location">location</param>
    /// <param name="v">v</param>
    [Description("@#uniform3fv")]
    public extern void Uniform3fv(WebGLUniformLocation? location, Float32List v);

    /// <summary>
    /// uniform4fv
    /// </summary>
    /// <param name="location">location</param>
    /// <param name="v">v</param>
    [Description("@#uniform4fv")]
    public extern void Uniform4fv(WebGLUniformLocation? location, Float32List v);

    /// <summary>
    /// uniform1iv
    /// </summary>
    /// <param name="location">location</param>
    /// <param name="v">v</param>
    [Description("@#uniform1iv")]
    public extern void Uniform1iv(WebGLUniformLocation? location, Int32List v);

    /// <summary>
    /// uniform2iv
    /// </summary>
    /// <param name="location">location</param>
    /// <param name="v">v</param>
    [Description("@#uniform2iv")]
    public extern void Uniform2iv(WebGLUniformLocation? location, Int32List v);

    /// <summary>
    /// uniform3iv
    /// </summary>
    /// <param name="location">location</param>
    /// <param name="v">v</param>
    [Description("@#uniform3iv")]
    public extern void Uniform3iv(WebGLUniformLocation? location, Int32List v);

    /// <summary>
    /// uniform4iv
    /// </summary>
    /// <param name="location">location</param>
    /// <param name="v">v</param>
    [Description("@#uniform4iv")]
    public extern void Uniform4iv(WebGLUniformLocation? location, Int32List v);

    /// <summary>
    /// uniformMatrix2fv
    /// </summary>
    /// <param name="location">location</param>
    /// <param name="transpose">transpose</param>
    /// <param name="value">value</param>
    [Description("@#uniformMatrix2fv")]
    public extern void UniformMatrix2fv(WebGLUniformLocation? location, GLboolean transpose, Float32List value);

    /// <summary>
    /// uniformMatrix3fv
    /// </summary>
    /// <param name="location">location</param>
    /// <param name="transpose">transpose</param>
    /// <param name="value">value</param>
    [Description("@#uniformMatrix3fv")]
    public extern void UniformMatrix3fv(WebGLUniformLocation? location, GLboolean transpose, Float32List value);

    /// <summary>
    /// uniformMatrix4fv
    /// </summary>
    /// <param name="location">location</param>
    /// <param name="transpose">transpose</param>
    /// <param name="value">value</param>
    [Description("@#uniformMatrix4fv")]
    public extern void UniformMatrix4fv(WebGLUniformLocation? location, GLboolean transpose, Float32List value);
    #endregion
}

/// <summary>
/// WebGLContextEvent
/// </summary>
[ECMAScript]
[Description("@#WebGLContextEvent")]
public class WebGLContextEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInit">eventInit</param>
    public extern WebGLContextEvent(string type, WebGLContextEventInit eventInit);

    /// <summary>
    /// statusMessage
    /// </summary>
    [Description("@#statusMessage")]
    public extern string StatusMessage { get; }
}