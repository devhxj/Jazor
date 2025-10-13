namespace ECMAScript;

/// <summary>
/// XRProjectionLayerInit
/// </summary>
[ECMAScript]
[Description("@#XRProjectionLayerInit")]
public record XRProjectionLayerInit(
    [property: Description("@#textureType")]XRTextureType TextureType = XRTextureType.Texture,
	[property: Description("@#colorFormat")]GLenum? ColorFormat = default,
	[property: Description("@#depthFormat")]GLenum? DepthFormat = default,
	[property: Description("@#scaleFactor")]double ScaleFactor = 1.0d,
	[property: Description("@#clearOnAccess")]bool ClearOnAccess = true);

/// <summary>
/// XRLayerInit
/// </summary>
[ECMAScript]
[Description("@#XRLayerInit")]
public record XRLayerInit(
    [property: Description("@#space")]XRSpace? Space = default,
	[property: Description("@#colorFormat")]GLenum? ColorFormat = default,
	[property: Description("@#depthFormat")]GLenum? DepthFormat = default,
	[property: Description("@#mipLevels")]uint MipLevels = 1,
	[property: Description("@#viewPixelWidth")]uint ViewPixelWidth = default,
	[property: Description("@#viewPixelHeight")]uint ViewPixelHeight = default,
	[property: Description("@#layout")]XRLayerLayout Layout = XRLayerLayout.Mono,
	[property: Description("@#isStatic")]bool IsStatic = false,
	[property: Description("@#clearOnAccess")]bool ClearOnAccess = true);

/// <summary>
/// XRQuadLayerInit
/// </summary>
[ECMAScript]
[Description("@#XRQuadLayerInit")]
public record XRQuadLayerInit(
    [property: Description("@#textureType")]XRTextureType TextureType = XRTextureType.Texture,
	[property: Description("@#transform")]XRRigidTransform? Transform = default,
	[property: Description("@#width")]float Width = 1.0f,
	[property: Description("@#height")]float Height = 1.0f): XRLayerInit;

/// <summary>
/// XRCylinderLayerInit
/// </summary>
[ECMAScript]
[Description("@#XRCylinderLayerInit")]
public record XRCylinderLayerInit(
    [property: Description("@#textureType")]XRTextureType TextureType = XRTextureType.Texture,
	[property: Description("@#transform")]XRRigidTransform? Transform = default,
	[property: Description("@#radius")]float Radius = 2.0f,
	[property: Description("@#centralAngle")]float CentralAngle = 0.78539f,
	[property: Description("@#aspectRatio")]float AspectRatio = 2.0f): XRLayerInit;

/// <summary>
/// XREquirectLayerInit
/// </summary>
[ECMAScript]
[Description("@#XREquirectLayerInit")]
public record XREquirectLayerInit(
    [property: Description("@#textureType")]XRTextureType TextureType = XRTextureType.Texture,
	[property: Description("@#transform")]XRRigidTransform? Transform = default,
	[property: Description("@#radius")]float Radius = 0f,
	[property: Description("@#centralHorizontalAngle")]float CentralHorizontalAngle = 6.28318f,
	[property: Description("@#upperVerticalAngle")]float UpperVerticalAngle = 1.570795f,
	[property: Description("@#lowerVerticalAngle")]float LowerVerticalAngle = -1.570795f): XRLayerInit;

/// <summary>
/// XRCubeLayerInit
/// </summary>
[ECMAScript]
[Description("@#XRCubeLayerInit")]
public record XRCubeLayerInit(
    [property: Description("@#orientation")]DOMPointReadOnly? Orientation = default): XRLayerInit;

/// <summary>
/// XRMediaLayerInit
/// </summary>
[ECMAScript]
[Description("@#XRMediaLayerInit")]
public record XRMediaLayerInit(
    [property: Description("@#space")]XRSpace? Space = default,
	[property: Description("@#layout")]XRLayerLayout Layout = XRLayerLayout.Mono,
	[property: Description("@#invertStereo")]bool InvertStereo = false);

/// <summary>
/// XRMediaQuadLayerInit
/// </summary>
[ECMAScript]
[Description("@#XRMediaQuadLayerInit")]
public record XRMediaQuadLayerInit(
    [property: Description("@#transform")]XRRigidTransform? Transform = default,
	[property: Description("@#width")]float Width = default,
	[property: Description("@#height")]float Height = default): XRMediaLayerInit;

/// <summary>
/// XRMediaCylinderLayerInit
/// </summary>
[ECMAScript]
[Description("@#XRMediaCylinderLayerInit")]
public record XRMediaCylinderLayerInit(
    [property: Description("@#transform")]XRRigidTransform? Transform = default,
	[property: Description("@#radius")]float Radius = 2.0f,
	[property: Description("@#centralAngle")]float CentralAngle = 0.78539f,
	[property: Description("@#aspectRatio")]float AspectRatio = default): XRMediaLayerInit;

/// <summary>
/// XRMediaEquirectLayerInit
/// </summary>
[ECMAScript]
[Description("@#XRMediaEquirectLayerInit")]
public record XRMediaEquirectLayerInit(
    [property: Description("@#transform")]XRRigidTransform? Transform = default,
	[property: Description("@#radius")]float Radius = 0.0f,
	[property: Description("@#centralHorizontalAngle")]float CentralHorizontalAngle = 6.28318f,
	[property: Description("@#upperVerticalAngle")]float UpperVerticalAngle = 1.570795f,
	[property: Description("@#lowerVerticalAngle")]float LowerVerticalAngle = -1.570795f): XRMediaLayerInit;

/// <summary>
/// XRLayerEventInit
/// </summary>
[ECMAScript]
[Description("@#XRLayerEventInit")]
public record XRLayerEventInit(
    [property: Description("@#layer")]XRLayer? Layer = default): EventInit;

/// <summary>
/// XRCompositionLayer
/// </summary>
[ECMAScript]
[Description("@#XRCompositionLayer")]
public class XRCompositionLayer : XRLayer
{
    /// <summary>
    /// layout
    /// </summary>
    [Description("@#layout")]
    public extern XRLayerLayout Layout { get; }

    /// <summary>
    /// blendTextureSourceAlpha
    /// </summary>
    [Description("@#blendTextureSourceAlpha")]
    public extern bool BlendTextureSourceAlpha { get; set; }

    /// <summary>
    /// forceMonoPresentation
    /// </summary>
    [Description("@#forceMonoPresentation")]
    public extern bool ForceMonoPresentation { get; set; }

    /// <summary>
    /// opacity
    /// </summary>
    [Description("@#opacity")]
    public extern float Opacity { get; set; }

    /// <summary>
    /// mipLevels
    /// </summary>
    [Description("@#mipLevels")]
    public extern uint MipLevels { get; }

    /// <summary>
    /// quality
    /// </summary>
    [Description("@#quality")]
    public extern XRLayerQuality Quality { get; set; }

    /// <summary>
    /// needsRedraw
    /// </summary>
    [Description("@#needsRedraw")]
    public extern bool NeedsRedraw { get; }

    /// <summary>
    /// destroy
    /// </summary>
    [Description("@#destroy")]
    public extern void Destroy();
}

/// <summary>
/// XRProjectionLayer
/// </summary>
[ECMAScript]
[Description("@#XRProjectionLayer")]
public class XRProjectionLayer : XRCompositionLayer
{
    /// <summary>
    /// textureWidth
    /// </summary>
    [Description("@#textureWidth")]
    public extern uint TextureWidth { get; }

    /// <summary>
    /// textureHeight
    /// </summary>
    [Description("@#textureHeight")]
    public extern uint TextureHeight { get; }

    /// <summary>
    /// textureArrayLength
    /// </summary>
    [Description("@#textureArrayLength")]
    public extern uint TextureArrayLength { get; }

    /// <summary>
    /// ignoreDepthValues
    /// </summary>
    [Description("@#ignoreDepthValues")]
    public extern bool IgnoreDepthValues { get; }

    /// <summary>
    /// fixedFoveation
    /// </summary>
    [Description("@#fixedFoveation")]
    public extern float? FixedFoveation { get; set; }

    /// <summary>
    /// deltaPose
    /// </summary>
    [Description("@#deltaPose")]
    public extern XRRigidTransform? DeltaPose { get; set; }
}

/// <summary>
/// XRQuadLayer
/// </summary>
[ECMAScript]
[Description("@#XRQuadLayer")]
public class XRQuadLayer : XRCompositionLayer
{
    /// <summary>
    /// space
    /// </summary>
    [Description("@#space")]
    public extern XRSpace Space { get; set; }

    /// <summary>
    /// transform
    /// </summary>
    [Description("@#transform")]
    public extern XRRigidTransform Transform { get; set; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern float Width { get; set; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern float Height { get; set; }

    /// <summary>
    /// onredraw
    /// </summary>
    [Description("@#onredraw")]
    public extern EventHandler Onredraw { get; set; }
}

/// <summary>
/// XRCylinderLayer
/// </summary>
[ECMAScript]
[Description("@#XRCylinderLayer")]
public class XRCylinderLayer : XRCompositionLayer
{
    /// <summary>
    /// space
    /// </summary>
    [Description("@#space")]
    public extern XRSpace Space { get; set; }

    /// <summary>
    /// transform
    /// </summary>
    [Description("@#transform")]
    public extern XRRigidTransform Transform { get; set; }

    /// <summary>
    /// radius
    /// </summary>
    [Description("@#radius")]
    public extern float Radius { get; set; }

    /// <summary>
    /// centralAngle
    /// </summary>
    [Description("@#centralAngle")]
    public extern float CentralAngle { get; set; }

    /// <summary>
    /// aspectRatio
    /// </summary>
    [Description("@#aspectRatio")]
    public extern float AspectRatio { get; set; }

    /// <summary>
    /// onredraw
    /// </summary>
    [Description("@#onredraw")]
    public extern EventHandler Onredraw { get; set; }
}

/// <summary>
/// XREquirectLayer
/// </summary>
[ECMAScript]
[Description("@#XREquirectLayer")]
public class XREquirectLayer : XRCompositionLayer
{
    /// <summary>
    /// space
    /// </summary>
    [Description("@#space")]
    public extern XRSpace Space { get; set; }

    /// <summary>
    /// transform
    /// </summary>
    [Description("@#transform")]
    public extern XRRigidTransform Transform { get; set; }

    /// <summary>
    /// radius
    /// </summary>
    [Description("@#radius")]
    public extern float Radius { get; set; }

    /// <summary>
    /// centralHorizontalAngle
    /// </summary>
    [Description("@#centralHorizontalAngle")]
    public extern float CentralHorizontalAngle { get; set; }

    /// <summary>
    /// upperVerticalAngle
    /// </summary>
    [Description("@#upperVerticalAngle")]
    public extern float UpperVerticalAngle { get; set; }

    /// <summary>
    /// lowerVerticalAngle
    /// </summary>
    [Description("@#lowerVerticalAngle")]
    public extern float LowerVerticalAngle { get; set; }

    /// <summary>
    /// onredraw
    /// </summary>
    [Description("@#onredraw")]
    public extern EventHandler Onredraw { get; set; }
}

/// <summary>
/// XRCubeLayer
/// </summary>
[ECMAScript]
[Description("@#XRCubeLayer")]
public class XRCubeLayer : XRCompositionLayer
{
    /// <summary>
    /// space
    /// </summary>
    [Description("@#space")]
    public extern XRSpace Space { get; set; }

    /// <summary>
    /// orientation
    /// </summary>
    [Description("@#orientation")]
    public extern DOMPointReadOnly Orientation { get; set; }

    /// <summary>
    /// onredraw
    /// </summary>
    [Description("@#onredraw")]
    public extern EventHandler Onredraw { get; set; }
}

/// <summary>
/// XRSubImage
/// </summary>
[ECMAScript]
[Description("@#XRSubImage")]
public class XRSubImage
{
    /// <summary>
    /// viewport
    /// </summary>
    [Description("@#viewport")]
    public extern XRViewport Viewport { get; }
}

/// <summary>
/// XRWebGLSubImage
/// </summary>
[ECMAScript]
[Description("@#XRWebGLSubImage")]
public class XRWebGLSubImage : XRSubImage
{
    /// <summary>
    /// colorTexture
    /// </summary>
    [Description("@#colorTexture")]
    public extern WebGLTexture ColorTexture { get; }

    /// <summary>
    /// depthStencilTexture
    /// </summary>
    [Description("@#depthStencilTexture")]
    public extern WebGLTexture? DepthStencilTexture { get; }

    /// <summary>
    /// motionVectorTexture
    /// </summary>
    [Description("@#motionVectorTexture")]
    public extern WebGLTexture? MotionVectorTexture { get; }

    /// <summary>
    /// imageIndex
    /// </summary>
    [Description("@#imageIndex")]
    public extern uint? ImageIndex { get; }

    /// <summary>
    /// colorTextureWidth
    /// </summary>
    [Description("@#colorTextureWidth")]
    public extern uint ColorTextureWidth { get; }

    /// <summary>
    /// colorTextureHeight
    /// </summary>
    [Description("@#colorTextureHeight")]
    public extern uint ColorTextureHeight { get; }

    /// <summary>
    /// depthStencilTextureWidth
    /// </summary>
    [Description("@#depthStencilTextureWidth")]
    public extern uint? DepthStencilTextureWidth { get; }

    /// <summary>
    /// depthStencilTextureHeight
    /// </summary>
    [Description("@#depthStencilTextureHeight")]
    public extern uint? DepthStencilTextureHeight { get; }

    /// <summary>
    /// motionVectorTextureWidth
    /// </summary>
    [Description("@#motionVectorTextureWidth")]
    public extern uint? MotionVectorTextureWidth { get; }

    /// <summary>
    /// motionVectorTextureHeight
    /// </summary>
    [Description("@#motionVectorTextureHeight")]
    public extern uint? MotionVectorTextureHeight { get; }
}

/// <summary>
/// XRMediaBinding
/// </summary>
[ECMAScript]
[Description("@#XRMediaBinding")]
public class XRMediaBinding
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="session">session</param>
    public extern XRMediaBinding(XRSession session);

    /// <summary>
    /// createQuadLayer
    /// </summary>
    /// <param name="video">video</param>
    /// <param name="init">init</param>
    [Description("@#createQuadLayer")]
    public extern XRQuadLayer CreateQuadLayer(HTMLVideoElement video, XRMediaQuadLayerInit? init = default);

    /// <summary>
    /// createCylinderLayer
    /// </summary>
    /// <param name="video">video</param>
    /// <param name="init">init</param>
    [Description("@#createCylinderLayer")]
    public extern XRCylinderLayer CreateCylinderLayer(HTMLVideoElement video, XRMediaCylinderLayerInit? init = default);

    /// <summary>
    /// createEquirectLayer
    /// </summary>
    /// <param name="video">video</param>
    /// <param name="init">init</param>
    [Description("@#createEquirectLayer")]
    public extern XREquirectLayer CreateEquirectLayer(HTMLVideoElement video, XRMediaEquirectLayerInit? init = default);
}

/// <summary>
/// XRLayerEvent
/// </summary>
[ECMAScript]
[Description("@#XRLayerEvent")]
public class XRLayerEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern XRLayerEvent(string type, XRLayerEventInit eventInitDict);

    /// <summary>
    /// layer
    /// </summary>
    [Description("@#layer")]
    public extern XRLayer Layer { get; }
}