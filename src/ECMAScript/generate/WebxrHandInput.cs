namespace ECMAScript;

/// <summary>
/// XRHand
/// </summary>
[ECMAScript]
[Description("@#XRHand")]
public class XRHand : IEnumerable<(XRHandJoint, XRJointSpace)>
{
    extern IEnumerator<(XRHandJoint, XRJointSpace)> IEnumerable<(XRHandJoint, XRJointSpace)>.GetEnumerator();
    extern IEnumerator IEnumerable.GetEnumerator();

    /// <summary>
    /// size
    /// </summary>
    [Description("@#size")]
    public extern uint Size { get; }

    /// <summary>
    /// get
    /// </summary>
    /// <param name="key">key</param>
    [Description("@#get")]
    public extern XRJointSpace Get(XRHandJoint key);
}

/// <summary>
/// XRJointSpace
/// </summary>
[ECMAScript]
[Description("@#XRJointSpace")]
public class XRJointSpace : XRSpace
{
    /// <summary>
    /// jointName
    /// </summary>
    [Description("@#jointName")]
    public extern XRHandJoint JointName { get; }
}

/// <summary>
/// XRJointPose
/// </summary>
[ECMAScript]
[Description("@#XRJointPose")]
public class XRJointPose : XRPose
{
    /// <summary>
    /// radius
    /// </summary>
    [Description("@#radius")]
    public extern float Radius { get; }
}