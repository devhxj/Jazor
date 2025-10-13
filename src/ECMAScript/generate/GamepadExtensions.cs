namespace ECMAScript;

/// <summary>
/// GamepadPose
/// </summary>
[ECMAScript]
[Description("@#GamepadPose")]
public class GamepadPose
{
    /// <summary>
    /// hasOrientation
    /// </summary>
    [Description("@#hasOrientation")]
    public extern bool HasOrientation { get; }

    /// <summary>
    /// hasPosition
    /// </summary>
    [Description("@#hasPosition")]
    public extern bool HasPosition { get; }

    /// <summary>
    /// position
    /// </summary>
    [Description("@#position")]
    public extern Float32Array? Position { get; }

    /// <summary>
    /// linearVelocity
    /// </summary>
    [Description("@#linearVelocity")]
    public extern Float32Array? LinearVelocity { get; }

    /// <summary>
    /// linearAcceleration
    /// </summary>
    [Description("@#linearAcceleration")]
    public extern Float32Array? LinearAcceleration { get; }

    /// <summary>
    /// orientation
    /// </summary>
    [Description("@#orientation")]
    public extern Float32Array? Orientation { get; }

    /// <summary>
    /// angularVelocity
    /// </summary>
    [Description("@#angularVelocity")]
    public extern Float32Array? AngularVelocity { get; }

    /// <summary>
    /// angularAcceleration
    /// </summary>
    [Description("@#angularAcceleration")]
    public extern Float32Array? AngularAcceleration { get; }
}

/// <summary>
/// Gamepad
/// </summary>
[ECMAScript]
[Description("@#Gamepad")]
public partial class Gamepad
{
    /// <summary>
    /// hand
    /// </summary>
    [Description("@#hand")]
    public extern GamepadHand Hand { get; }

    /// <summary>
    /// hapticActuators
    /// </summary>
    [Description("@#hapticActuators")]
    public extern FrozenSet<GamepadHapticActuator> HapticActuators { get; }

    /// <summary>
    /// pose
    /// </summary>
    [Description("@#pose")]
    public extern GamepadPose? Pose { get; }

    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern string Id { get; }

    /// <summary>
    /// index
    /// </summary>
    [Description("@#index")]
    public extern int Index { get; }

    /// <summary>
    /// connected
    /// </summary>
    [Description("@#connected")]
    public extern bool Connected { get; }

    /// <summary>
    /// timestamp
    /// </summary>
    [Description("@#timestamp")]
    public extern double Timestamp { get; }

    /// <summary>
    /// mapping
    /// </summary>
    [Description("@#mapping")]
    public extern GamepadMappingType Mapping { get; }

    /// <summary>
    /// axes
    /// </summary>
    [Description("@#axes")]
    public extern FrozenSet<double> Axes { get; }

    /// <summary>
    /// buttons
    /// </summary>
    [Description("@#buttons")]
    public extern FrozenSet<GamepadButton> Buttons { get; }

    /// <summary>
    /// touches
    /// </summary>
    [Description("@#touches")]
    public extern FrozenSet<GamepadTouch> Touches { get; }

    /// <summary>
    /// vibrationActuator
    /// </summary>
    [Description("@#vibrationActuator")]
    public extern GamepadHapticActuator VibrationActuator { get; }
}

/// <summary>
/// GamepadHapticActuator
/// </summary>
[ECMAScript]
[Description("@#GamepadHapticActuator")]
public partial class GamepadHapticActuator
{
    /// <summary>
    /// pulse
    /// </summary>
    /// <param name="value">value</param>
    /// <param name="duration">duration</param>
    [Description("@#pulse")]
    public extern PromiseResult<bool> Pulse(double value, double duration);

    /// <summary>
    /// effects
    /// </summary>
    [Description("@#effects")]
    public extern FrozenSet<GamepadHapticEffectType> Effects { get; }

    /// <summary>
    /// playEffect
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="params">params</param>
    [Description("@#playEffect")]
    public extern PromiseResult<GamepadHapticsResult> PlayEffect(GamepadHapticEffectType type, GamepadEffectParameters? @params = default);

    /// <summary>
    /// reset
    /// </summary>
    [Description("@#reset")]
    public extern PromiseResult<GamepadHapticsResult> Reset();
}