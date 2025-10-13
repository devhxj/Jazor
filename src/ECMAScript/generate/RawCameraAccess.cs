namespace ECMAScript;

/// <summary>
/// XRView
/// </summary>
[ECMAScript]
[Description("@#XRView")]
public partial class XRView
{
    /// <summary>
    /// camera
    /// </summary>
    [Description("@#camera")]
    public extern XRCamera? Camera { get; }

    /// <summary>
    /// isFirstPersonObserver
    /// </summary>
    [Description("@#isFirstPersonObserver")]
    public extern bool IsFirstPersonObserver { get; }

    /// <summary>
    /// eye
    /// </summary>
    [Description("@#eye")]
    public extern XREye Eye { get; }

    /// <summary>
    /// recommendedViewportScale
    /// </summary>
    [Description("@#recommendedViewportScale")]
    public extern double? RecommendedViewportScale { get; }

    /// <summary>
    /// requestViewportScale
    /// </summary>
    /// <param name="scale">scale</param>
    [Description("@#requestViewportScale")]
    public extern void RequestViewportScale(double? scale);

    #region mixin XRViewGeometry
    /// <summary>
    /// projectionMatrix
    /// </summary>
    [Description("@#projectionMatrix")]
    public extern Float32Array ProjectionMatrix { get; }

    /// <summary>
    /// transform
    /// </summary>
    [Description("@#transform")]
    public extern XRRigidTransform Transform { get; }
    #endregion
}

/// <summary>
/// XRCamera
/// </summary>
[ECMAScript]
[Description("@#XRCamera")]
public class XRCamera
{
    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern uint Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern uint Height { get; }
}

/// <summary>
/// XRWebGLBinding
/// </summary>
[ECMAScript]
[Description("@#XRWebGLBinding")]
public partial class XRWebGLBinding
{
    /// <summary>
    /// getCameraImage
    /// </summary>
    /// <param name="camera">camera</param>
    [Description("@#getCameraImage")]
    public extern WebGLTexture? GetCameraImage(XRCamera camera);

    /// <summary>
    /// getDepthInformation
    /// </summary>
    /// <param name="view">view</param>
    [Description("@#getDepthInformation")]
    public extern XRWebGLDepthInformation? GetDepthInformation(XRView view);

    /// <summary>
    /// getReflectionCubeMap
    /// </summary>
    /// <param name="lightProbe">lightProbe</param>
    [Description("@#getReflectionCubeMap")]
    public extern WebGLTexture? GetReflectionCubeMap(XRLightProbe lightProbe);

    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="session">session</param>
    /// <param name="context">context</param>
    public extern XRWebGLBinding(XRSession session, XRWebGLRenderingContext context);

    /// <summary>
    /// nativeProjectionScaleFactor
    /// </summary>
    [Description("@#nativeProjectionScaleFactor")]
    public extern double NativeProjectionScaleFactor { get; }

    /// <summary>
    /// usesDepthValues
    /// </summary>
    [Description("@#usesDepthValues")]
    public extern bool UsesDepthValues { get; }

    /// <summary>
    /// createProjectionLayer
    /// </summary>
    /// <param name="init">init</param>
    [Description("@#createProjectionLayer")]
    public extern XRProjectionLayer CreateProjectionLayer(XRProjectionLayerInit? init = default);

    /// <summary>
    /// createQuadLayer
    /// </summary>
    /// <param name="init">init</param>
    [Description("@#createQuadLayer")]
    public extern XRQuadLayer CreateQuadLayer(XRQuadLayerInit? init = default);

    /// <summary>
    /// createCylinderLayer
    /// </summary>
    /// <param name="init">init</param>
    [Description("@#createCylinderLayer")]
    public extern XRCylinderLayer CreateCylinderLayer(XRCylinderLayerInit? init = default);

    /// <summary>
    /// createEquirectLayer
    /// </summary>
    /// <param name="init">init</param>
    [Description("@#createEquirectLayer")]
    public extern XREquirectLayer CreateEquirectLayer(XREquirectLayerInit? init = default);

    /// <summary>
    /// createCubeLayer
    /// </summary>
    /// <param name="init">init</param>
    [Description("@#createCubeLayer")]
    public extern XRCubeLayer CreateCubeLayer(XRCubeLayerInit? init = default);

    /// <summary>
    /// getSubImage
    /// </summary>
    /// <param name="layer">layer</param>
    /// <param name="frame">frame</param>
    /// <param name="eye">eye</param>
    [Description("@#getSubImage")]
    public extern XRWebGLSubImage GetSubImage(XRCompositionLayer layer, XRFrame frame, XREye eye = XREye.None);

    /// <summary>
    /// getViewSubImage
    /// </summary>
    /// <param name="layer">layer</param>
    /// <param name="view">view</param>
    [Description("@#getViewSubImage")]
    public extern XRWebGLSubImage GetViewSubImage(XRProjectionLayer layer, XRView view);

    /// <summary>
    /// foveateBoundTexture
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="fixedFoveation">fixed_foveation</param>
    [Description("@#foveateBoundTexture")]
    public extern void FoveateBoundTexture(GLenum target, float fixedFoveation);
}