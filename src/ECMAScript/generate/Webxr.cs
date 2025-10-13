namespace ECMAScript;

/// <summary>
/// XRRenderStateInit
/// </summary>
[ECMAScript]
[Description("@#XRRenderStateInit")]
public record XRRenderStateInit(
    [property: Description("@#depthNear")]double DepthNear = default,
	[property: Description("@#depthFar")]double DepthFar = default,
	[property: Description("@#passthroughFullyObscured")]bool PassthroughFullyObscured = default,
	[property: Description("@#inlineVerticalFieldOfView")]double InlineVerticalFieldOfView = default,
	[property: Description("@#baseLayer")]XRWebGLLayer? BaseLayer = default,
	[property: Description("@#layers")]XRLayer[]? Layers = default);

/// <summary>
/// XRWebGLLayerInit
/// </summary>
[ECMAScript]
[Description("@#XRWebGLLayerInit")]
public record XRWebGLLayerInit(
    [property: Description("@#antialias")]bool Antialias = true,
	[property: Description("@#depth")]bool Depth = true,
	[property: Description("@#stencil")]bool Stencil = false,
	[property: Description("@#alpha")]bool Alpha = true,
	[property: Description("@#ignoreDepthValues")]bool IgnoreDepthValues = false,
	[property: Description("@#framebufferScaleFactor")]double FramebufferScaleFactor = 1.0d);

/// <summary>
/// XRSessionEventInit
/// </summary>
[ECMAScript]
[Description("@#XRSessionEventInit")]
public record XRSessionEventInit(
    [property: Description("@#session")]XRSession? Session = default): EventInit;

/// <summary>
/// XRInputSourceEventInit
/// </summary>
[ECMAScript]
[Description("@#XRInputSourceEventInit")]
public record XRInputSourceEventInit(
    [property: Description("@#frame")]XRFrame? Frame = default,
	[property: Description("@#inputSource")]XRInputSource? InputSource = default): EventInit;

/// <summary>
/// XRInputSourcesChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#XRInputSourcesChangeEventInit")]
public record XRInputSourcesChangeEventInit(
    [property: Description("@#session")]XRSession? Session = default,
	[property: Description("@#added")]XRInputSource[]? Added = default,
	[property: Description("@#removed")]XRInputSource[]? Removed = default): EventInit;

/// <summary>
/// XRReferenceSpaceEventInit
/// </summary>
[ECMAScript]
[Description("@#XRReferenceSpaceEventInit")]
public record XRReferenceSpaceEventInit(
    [property: Description("@#referenceSpace")]XRReferenceSpace? ReferenceSpace = default,
	[property: Description("@#transform")]XRRigidTransform? Transform = null): EventInit;

/// <summary>
/// XRSessionSupportedPermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#XRSessionSupportedPermissionDescriptor")]
public record XRSessionSupportedPermissionDescriptor(
    [property: Description("@#mode")]XRSessionMode? Mode = default): PermissionDescriptor;

/// <summary>
/// XRPermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#XRPermissionDescriptor")]
public record XRPermissionDescriptor(
    [property: Description("@#mode")]XRSessionMode? Mode = default,
	[property: Description("@#requiredFeatures")]string[]? RequiredFeatures = default,
	[property: Description("@#optionalFeatures")]string[]? OptionalFeatures = default): PermissionDescriptor;

/// <summary>
/// XRSystem
/// </summary>
[ECMAScript]
[Description("@#XRSystem")]
public class XRSystem : EventTarget
{
    /// <summary>
    /// isSessionSupported
    /// </summary>
    /// <param name="mode">mode</param>
    [Description("@#isSessionSupported")]
    public extern PromiseResult<bool> IsSessionSupported(XRSessionMode mode);

    /// <summary>
    /// requestSession
    /// </summary>
    /// <param name="mode">mode</param>
    /// <param name="options">options</param>
    [Description("@#requestSession")]
    public extern PromiseResult<XRSession> RequestSession(XRSessionMode mode, XRSessionInit? options = default);

    /// <summary>
    /// ondevicechange
    /// </summary>
    [Description("@#ondevicechange")]
    public extern EventHandler Ondevicechange { get; set; }
}

/// <summary>
/// XRRenderState
/// </summary>
[ECMAScript]
[Description("@#XRRenderState")]
public partial class XRRenderState
{
    /// <summary>
    /// depthNear
    /// </summary>
    [Description("@#depthNear")]
    public extern double DepthNear { get; }

    /// <summary>
    /// depthFar
    /// </summary>
    [Description("@#depthFar")]
    public extern double DepthFar { get; }

    /// <summary>
    /// passthroughFullyObscured
    /// </summary>
    [Description("@#passthroughFullyObscured")]
    public extern bool? PassthroughFullyObscured { get; }

    /// <summary>
    /// inlineVerticalFieldOfView
    /// </summary>
    [Description("@#inlineVerticalFieldOfView")]
    public extern double? InlineVerticalFieldOfView { get; }

    /// <summary>
    /// baseLayer
    /// </summary>
    [Description("@#baseLayer")]
    public extern XRWebGLLayer? BaseLayer { get; }

    /// <summary>
    /// layers
    /// </summary>
    [Description("@#layers")]
    public extern FrozenSet<XRLayer> Layers { get; }
}

/// <summary>
/// XRSpace
/// </summary>
[ECMAScript]
[Description("@#XRSpace")]
public class XRSpace : EventTarget
{

}

/// <summary>
/// XRReferenceSpace
/// </summary>
[ECMAScript]
[Description("@#XRReferenceSpace")]
public class XRReferenceSpace : XRSpace
{
    /// <summary>
    /// getOffsetReferenceSpace
    /// </summary>
    /// <param name="originOffset">originOffset</param>
    [Description("@#getOffsetReferenceSpace")]
    public extern XRReferenceSpace GetOffsetReferenceSpace(XRRigidTransform originOffset);

    /// <summary>
    /// onreset
    /// </summary>
    [Description("@#onreset")]
    public extern EventHandler Onreset { get; set; }
}

/// <summary>
/// XRBoundedReferenceSpace
/// </summary>
[ECMAScript]
[Description("@#XRBoundedReferenceSpace")]
public class XRBoundedReferenceSpace : XRReferenceSpace
{
    /// <summary>
    /// boundsGeometry
    /// </summary>
    [Description("@#boundsGeometry")]
    public extern FrozenSet<DOMPointReadOnly> BoundsGeometry { get; }
}

/// <summary>
/// XRViewport
/// </summary>
[ECMAScript]
[Description("@#XRViewport")]
public class XRViewport
{
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern int X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern int Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern int Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern int Height { get; }
}

/// <summary>
/// XRRigidTransform
/// </summary>
[ECMAScript]
[Description("@#XRRigidTransform")]
public class XRRigidTransform
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="position">position</param>
    /// <param name="orientation">orientation</param>
    public extern XRRigidTransform(DOMPointInit position, DOMPointInit orientation);

    /// <summary>
    /// position
    /// </summary>
    [Description("@#position")]
    public extern DOMPointReadOnly Position { get; }

    /// <summary>
    /// orientation
    /// </summary>
    [Description("@#orientation")]
    public extern DOMPointReadOnly Orientation { get; }

    /// <summary>
    /// matrix
    /// </summary>
    [Description("@#matrix")]
    public extern Float32Array Matrix { get; }

    /// <summary>
    /// inverse
    /// </summary>
    [Description("@#inverse")]
    public extern XRRigidTransform Inverse { get; }
}

/// <summary>
/// XRPose
/// </summary>
[ECMAScript]
[Description("@#XRPose")]
public class XRPose
{
    /// <summary>
    /// transform
    /// </summary>
    [Description("@#transform")]
    public extern XRRigidTransform Transform { get; }

    /// <summary>
    /// linearVelocity
    /// </summary>
    [Description("@#linearVelocity")]
    public extern DOMPointReadOnly? LinearVelocity { get; }

    /// <summary>
    /// angularVelocity
    /// </summary>
    [Description("@#angularVelocity")]
    public extern DOMPointReadOnly? AngularVelocity { get; }

    /// <summary>
    /// emulatedPosition
    /// </summary>
    [Description("@#emulatedPosition")]
    public extern bool EmulatedPosition { get; }
}

/// <summary>
/// XRViewerPose
/// </summary>
[ECMAScript]
[Description("@#XRViewerPose")]
public class XRViewerPose : XRPose
{
    /// <summary>
    /// views
    /// </summary>
    [Description("@#views")]
    public extern FrozenSet<XRView> Views { get; }
}

/// <summary>
/// XRInputSourceArray
/// </summary>
[ECMAScript]
[Description("@#XRInputSourceArray")]
public class XRInputSourceArray : IEnumerable<XRInputSource>
{
    extern IEnumerator<XRInputSource> IEnumerable<XRInputSource>.GetEnumerator();
    extern IEnumerator IEnumerable.GetEnumerator();

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    [Description("@#")]
    public extern XRInputSource this[uint index] { get; }
}

/// <summary>
/// XRLayer
/// </summary>
[ECMAScript]
[Description("@#XRLayer")]
public class XRLayer : EventTarget
{

}

/// <summary>
/// XRWebGLLayer
/// </summary>
[ECMAScript]
[Description("@#XRWebGLLayer")]
public class XRWebGLLayer : XRLayer
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="session">session</param>
    /// <param name="context">context</param>
    /// <param name="layerInit">layerInit</param>
    public extern XRWebGLLayer(XRSession session, XRWebGLRenderingContext context, XRWebGLLayerInit layerInit);

    /// <summary>
    /// antialias
    /// </summary>
    [Description("@#antialias")]
    public extern bool Antialias { get; }

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
    /// framebuffer
    /// </summary>
    [Description("@#framebuffer")]
    public extern WebGLFramebuffer? Framebuffer { get; }

    /// <summary>
    /// framebufferWidth
    /// </summary>
    [Description("@#framebufferWidth")]
    public extern uint FramebufferWidth { get; }

    /// <summary>
    /// framebufferHeight
    /// </summary>
    [Description("@#framebufferHeight")]
    public extern uint FramebufferHeight { get; }

    /// <summary>
    /// getViewport
    /// </summary>
    /// <param name="view">view</param>
    [Description("@#getViewport")]
    public extern XRViewport? GetViewport(XRView view);

    /// <summary>
    /// getNativeFramebufferScaleFactor
    /// </summary>
    /// <param name="session">session</param>
    [Description("@#getNativeFramebufferScaleFactor")]
    public extern static double GetNativeFramebufferScaleFactor(XRSession session);
}

/// <summary>
/// XRSessionEvent
/// </summary>
[ECMAScript]
[Description("@#XRSessionEvent")]
public class XRSessionEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern XRSessionEvent(string type, XRSessionEventInit eventInitDict);

    /// <summary>
    /// session
    /// </summary>
    [Description("@#session")]
    public extern XRSession Session { get; }
}

/// <summary>
/// XRInputSourceEvent
/// </summary>
[ECMAScript]
[Description("@#XRInputSourceEvent")]
public class XRInputSourceEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern XRInputSourceEvent(string type, XRInputSourceEventInit eventInitDict);

    /// <summary>
    /// frame
    /// </summary>
    [Description("@#frame")]
    public extern XRFrame Frame { get; }

    /// <summary>
    /// inputSource
    /// </summary>
    [Description("@#inputSource")]
    public extern XRInputSource InputSource { get; }
}

/// <summary>
/// XRInputSourcesChangeEvent
/// </summary>
[ECMAScript]
[Description("@#XRInputSourcesChangeEvent")]
public class XRInputSourcesChangeEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern XRInputSourcesChangeEvent(string type, XRInputSourcesChangeEventInit eventInitDict);

    /// <summary>
    /// session
    /// </summary>
    [Description("@#session")]
    public extern XRSession Session { get; }

    /// <summary>
    /// added
    /// </summary>
    [Description("@#added")]
    public extern FrozenSet<XRInputSource> Added { get; }

    /// <summary>
    /// removed
    /// </summary>
    [Description("@#removed")]
    public extern FrozenSet<XRInputSource> Removed { get; }
}

/// <summary>
/// XRReferenceSpaceEvent
/// </summary>
[ECMAScript]
[Description("@#XRReferenceSpaceEvent")]
public class XRReferenceSpaceEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern XRReferenceSpaceEvent(string type, XRReferenceSpaceEventInit eventInitDict);

    /// <summary>
    /// referenceSpace
    /// </summary>
    [Description("@#referenceSpace")]
    public extern XRReferenceSpace ReferenceSpace { get; }

    /// <summary>
    /// transform
    /// </summary>
    [Description("@#transform")]
    public extern XRRigidTransform? Transform { get; }
}

/// <summary>
/// XRPermissionStatus
/// </summary>
[ECMAScript]
[Description("@#XRPermissionStatus")]
public class XRPermissionStatus : PermissionStatus
{
    /// <summary>
    /// granted
    /// </summary>
    [Description("@#granted")]
    public extern FrozenSet<string> Granted { get; set; }
}