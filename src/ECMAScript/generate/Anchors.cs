namespace ECMAScript;

/// <summary>
/// XRAnchor
/// </summary>
[ECMAScript]
[Description("@#XRAnchor")]
public class XRAnchor
{
    /// <summary>
    /// anchorSpace
    /// </summary>
    [Description("@#anchorSpace")]
    public extern XRSpace AnchorSpace { get; }

    /// <summary>
    /// requestPersistentHandle
    /// </summary>
    [Description("@#requestPersistentHandle")]
    public extern PromiseResult<string> RequestPersistentHandle();

    /// <summary>
    /// delete
    /// </summary>
    [Description("@#delete")]
    public extern void Delete();
}

/// <summary>
/// XRFrame
/// </summary>
[ECMAScript]
[Description("@#XRFrame")]
public partial class XRFrame
{
    /// <summary>
    /// createAnchor
    /// </summary>
    /// <param name="pose">pose</param>
    /// <param name="space">space</param>
    [Description("@#createAnchor")]
    public extern PromiseResult<XRAnchor> CreateAnchor(XRRigidTransform pose, XRSpace space);

    /// <summary>
    /// trackedAnchors
    /// </summary>
    [Description("@#trackedAnchors")]
    public extern XRAnchorSet TrackedAnchors { get; }

    /// <summary>
    /// detectedMeshes
    /// </summary>
    [Description("@#detectedMeshes")]
    public extern XRMeshSet DetectedMeshes { get; }

    /// <summary>
    /// getDepthInformation
    /// </summary>
    /// <param name="view">view</param>
    [Description("@#getDepthInformation")]
    public extern XRCPUDepthInformation? GetDepthInformation(XRView view);

    /// <summary>
    /// getJointPose
    /// </summary>
    /// <param name="joint">joint</param>
    /// <param name="baseSpace">baseSpace</param>
    [Description("@#getJointPose")]
    public extern XRJointPose? GetJointPose(XRJointSpace joint, XRSpace baseSpace);

    /// <summary>
    /// fillJointRadii
    /// </summary>
    /// <param name="jointSpaces">jointSpaces</param>
    /// <param name="radii">radii</param>
    [Description("@#fillJointRadii")]
    public extern bool FillJointRadii(XRJointSpace[] jointSpaces, Float32Array radii);

    /// <summary>
    /// fillPoses
    /// </summary>
    /// <param name="spaces">spaces</param>
    /// <param name="baseSpace">baseSpace</param>
    /// <param name="transforms">transforms</param>
    [Description("@#fillPoses")]
    public extern bool FillPoses(XRSpace[] spaces, XRSpace baseSpace, Float32Array transforms);

    /// <summary>
    /// getHitTestResults
    /// </summary>
    /// <param name="hitTestSource">hitTestSource</param>
    [Description("@#getHitTestResults")]
    public extern XRHitTestResult[] GetHitTestResults(XRHitTestSource hitTestSource);

    /// <summary>
    /// getHitTestResultsForTransientInput
    /// </summary>
    /// <param name="hitTestSource">hitTestSource</param>
    [Description("@#getHitTestResultsForTransientInput")]
    public extern XRTransientInputHitTestResult[] GetHitTestResultsForTransientInput(XRTransientInputHitTestSource hitTestSource);

    /// <summary>
    /// getLightEstimate
    /// </summary>
    /// <param name="lightProbe">lightProbe</param>
    [Description("@#getLightEstimate")]
    public extern XRLightEstimate? GetLightEstimate(XRLightProbe lightProbe);

    /// <summary>
    /// detectedPlanes
    /// </summary>
    [Description("@#detectedPlanes")]
    public extern XRPlaneSet DetectedPlanes { get; }

    /// <summary>
    /// session
    /// </summary>
    [Description("@#session")]
    public extern XRSession Session { get; }

    /// <summary>
    /// predictedDisplayTime
    /// </summary>
    [Description("@#predictedDisplayTime")]
    public extern double PredictedDisplayTime { get; }

    /// <summary>
    /// getViewerPose
    /// </summary>
    /// <param name="referenceSpace">referenceSpace</param>
    [Description("@#getViewerPose")]
    public extern XRViewerPose? GetViewerPose(XRReferenceSpace referenceSpace);

    /// <summary>
    /// getPose
    /// </summary>
    /// <param name="space">space</param>
    /// <param name="baseSpace">baseSpace</param>
    [Description("@#getPose")]
    public extern XRPose? GetPose(XRSpace space, XRSpace baseSpace);
}

/// <summary>
/// XRSession
/// </summary>
[ECMAScript]
[Description("@#XRSession")]
public partial class XRSession : EventTarget
{
    /// <summary>
    /// persistentAnchors
    /// </summary>
    [Description("@#persistentAnchors")]
    public extern FrozenSet<string> PersistentAnchors { get; }

    /// <summary>
    /// restorePersistentAnchor
    /// </summary>
    /// <param name="uuid">uuid</param>
    [Description("@#restorePersistentAnchor")]
    public extern PromiseResult<XRAnchor> RestorePersistentAnchor(string uuid);

    /// <summary>
    /// deletePersistentAnchor
    /// </summary>
    /// <param name="uuid">uuid</param>
    [Description("@#deletePersistentAnchor")]
    public extern PromiseResult<object> DeletePersistentAnchor(string uuid);

    /// <summary>
    /// environmentBlendMode
    /// </summary>
    [Description("@#environmentBlendMode")]
    public extern XREnvironmentBlendMode EnvironmentBlendMode { get; }

    /// <summary>
    /// interactionMode
    /// </summary>
    [Description("@#interactionMode")]
    public extern XRInteractionMode InteractionMode { get; }

    /// <summary>
    /// depthUsage
    /// </summary>
    [Description("@#depthUsage")]
    public extern XRDepthUsage DepthUsage { get; }

    /// <summary>
    /// depthDataFormat
    /// </summary>
    [Description("@#depthDataFormat")]
    public extern XRDepthDataFormat DepthDataFormat { get; }

    /// <summary>
    /// depthType
    /// </summary>
    [Description("@#depthType")]
    public extern XRDepthType? DepthType { get; }

    /// <summary>
    /// depthActive
    /// </summary>
    [Description("@#depthActive")]
    public extern bool? DepthActive { get; }

    /// <summary>
    /// pauseDepthSensing
    /// </summary>
    [Description("@#pauseDepthSensing")]
    public extern void PauseDepthSensing();

    /// <summary>
    /// resumeDepthSensing
    /// </summary>
    [Description("@#resumeDepthSensing")]
    public extern void ResumeDepthSensing();

    /// <summary>
    /// domOverlayState
    /// </summary>
    [Description("@#domOverlayState")]
    public extern XRDOMOverlayState? DomOverlayState { get; }

    /// <summary>
    /// requestHitTestSource
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#requestHitTestSource")]
    public extern PromiseResult<XRHitTestSource> RequestHitTestSource(XRHitTestOptionsInit options);

    /// <summary>
    /// requestHitTestSourceForTransientInput
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#requestHitTestSourceForTransientInput")]
    public extern PromiseResult<XRTransientInputHitTestSource> RequestHitTestSourceForTransientInput(XRTransientInputHitTestOptionsInit options);

    /// <summary>
    /// requestLightProbe
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#requestLightProbe")]
    public extern PromiseResult<XRLightProbe> RequestLightProbe(XRLightProbeInit? options = default);

    /// <summary>
    /// preferredReflectionFormat
    /// </summary>
    [Description("@#preferredReflectionFormat")]
    public extern XRReflectionFormat PreferredReflectionFormat { get; }

    /// <summary>
    /// initiateRoomCapture
    /// </summary>
    [Description("@#initiateRoomCapture")]
    public extern PromiseResult<object> InitiateRoomCapture();

    /// <summary>
    /// visibilityState
    /// </summary>
    [Description("@#visibilityState")]
    public extern XRVisibilityState VisibilityState { get; }

    /// <summary>
    /// frameRate
    /// </summary>
    [Description("@#frameRate")]
    public extern float? FrameRate { get; }

    /// <summary>
    /// supportedFrameRates
    /// </summary>
    [Description("@#supportedFrameRates")]
    public extern Float32Array? SupportedFrameRates { get; }

    /// <summary>
    /// renderState
    /// </summary>
    [Description("@#renderState")]
    public extern XRRenderState RenderState { get; }

    /// <summary>
    /// inputSources
    /// </summary>
    [Description("@#inputSources")]
    public extern XRInputSourceArray InputSources { get; }

    /// <summary>
    /// trackedSources
    /// </summary>
    [Description("@#trackedSources")]
    public extern XRInputSourceArray TrackedSources { get; }

    /// <summary>
    /// enabledFeatures
    /// </summary>
    [Description("@#enabledFeatures")]
    public extern FrozenSet<string> EnabledFeatures { get; }

    /// <summary>
    /// isSystemKeyboardSupported
    /// </summary>
    [Description("@#isSystemKeyboardSupported")]
    public extern bool IsSystemKeyboardSupported { get; }

    /// <summary>
    /// updateRenderState
    /// </summary>
    /// <param name="state">state</param>
    [Description("@#updateRenderState")]
    public extern void UpdateRenderState(XRRenderStateInit? state = default);

    /// <summary>
    /// updateTargetFrameRate
    /// </summary>
    /// <param name="rate">rate</param>
    [Description("@#updateTargetFrameRate")]
    public extern PromiseResult<object> UpdateTargetFrameRate(float rate);

    /// <summary>
    /// requestReferenceSpace
    /// </summary>
    /// <param name="type">type</param>
    [Description("@#requestReferenceSpace")]
    public extern PromiseResult<XRReferenceSpace> RequestReferenceSpace(XRReferenceSpaceType type);

    /// <summary>
    /// requestAnimationFrame
    /// </summary>
    /// <param name="callback">callback</param>
    [Description("@#requestAnimationFrame")]
    public extern uint RequestAnimationFrame(XRFrameRequestCallback callback);

    /// <summary>
    /// cancelAnimationFrame
    /// </summary>
    /// <param name="handle">handle</param>
    [Description("@#cancelAnimationFrame")]
    public extern void CancelAnimationFrame(uint handle);

    /// <summary>
    /// end
    /// </summary>
    [Description("@#end")]
    public extern PromiseResult<object> End();

    /// <summary>
    /// onend
    /// </summary>
    [Description("@#onend")]
    public extern EventHandler Onend { get; set; }

    /// <summary>
    /// oninputsourceschange
    /// </summary>
    [Description("@#oninputsourceschange")]
    public extern EventHandler Oninputsourceschange { get; set; }

    /// <summary>
    /// onselect
    /// </summary>
    [Description("@#onselect")]
    public extern EventHandler Onselect { get; set; }

    /// <summary>
    /// onselectstart
    /// </summary>
    [Description("@#onselectstart")]
    public extern EventHandler Onselectstart { get; set; }

    /// <summary>
    /// onselectend
    /// </summary>
    [Description("@#onselectend")]
    public extern EventHandler Onselectend { get; set; }

    /// <summary>
    /// onsqueeze
    /// </summary>
    [Description("@#onsqueeze")]
    public extern EventHandler Onsqueeze { get; set; }

    /// <summary>
    /// onsqueezestart
    /// </summary>
    [Description("@#onsqueezestart")]
    public extern EventHandler Onsqueezestart { get; set; }

    /// <summary>
    /// onsqueezeend
    /// </summary>
    [Description("@#onsqueezeend")]
    public extern EventHandler Onsqueezeend { get; set; }

    /// <summary>
    /// onvisibilitychange
    /// </summary>
    [Description("@#onvisibilitychange")]
    public extern EventHandler Onvisibilitychange { get; set; }

    /// <summary>
    /// onframeratechange
    /// </summary>
    [Description("@#onframeratechange")]
    public extern EventHandler Onframeratechange { get; set; }
}

/// <summary>
/// XRHitTestResult
/// </summary>
[ECMAScript]
[Description("@#XRHitTestResult")]
public partial class XRHitTestResult
{
    /// <summary>
    /// createAnchor
    /// </summary>
    [Description("@#createAnchor")]
    public extern PromiseResult<XRAnchor> CreateAnchor();

    /// <summary>
    /// getPose
    /// </summary>
    /// <param name="baseSpace">baseSpace</param>
    [Description("@#getPose")]
    public extern XRPose? GetPose(XRSpace baseSpace);
}

/// <summary>
/// XRAnchorSet
/// </summary>
[ECMAScript]
[Description("@#XRAnchorSet")]
public class XRAnchorSet : ISet<XRAnchor>
{
    #region Set
    extern int ICollection<XRAnchor>.Count { get; }
    extern bool ICollection<XRAnchor>.IsReadOnly { get; }
    extern bool ISet<XRAnchor>.Add(XRAnchor item);
    extern void ICollection<XRAnchor>.Clear();
    extern bool ICollection<XRAnchor>.Contains(XRAnchor item);
    extern void ICollection<XRAnchor>.CopyTo(XRAnchor[] array, int arrayIndex);
    extern void ISet<XRAnchor>.ExceptWith(IEnumerable<XRAnchor> other);
    extern IEnumerator<XRAnchor> IEnumerable<XRAnchor>.GetEnumerator();
    extern void ISet<XRAnchor>.IntersectWith(IEnumerable<XRAnchor> other);
    extern bool ISet<XRAnchor>.IsProperSubsetOf(IEnumerable<XRAnchor> other);
    extern bool ISet<XRAnchor>.IsProperSupersetOf(IEnumerable<XRAnchor> other);
    extern bool ISet<XRAnchor>.IsSubsetOf(IEnumerable<XRAnchor> other);
    extern bool ISet<XRAnchor>.IsSupersetOf(IEnumerable<XRAnchor> other);
    extern bool ISet<XRAnchor>.Overlaps(IEnumerable<XRAnchor> other);
    extern bool ICollection<XRAnchor>.Remove(XRAnchor item);
    extern bool ISet<XRAnchor>.SetEquals(IEnumerable<XRAnchor> other);
    extern void ISet<XRAnchor>.SymmetricExceptWith(IEnumerable<XRAnchor> other);
    extern void ISet<XRAnchor>.UnionWith(IEnumerable<XRAnchor> other);
    extern void ICollection<XRAnchor>.Add(XRAnchor item);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion
}