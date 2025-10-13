namespace ECMAScript;

/// <summary>
/// CaptureHandleConfig
/// </summary>
[ECMAScript]
[Description("@#CaptureHandleConfig")]
public record CaptureHandleConfig(
    [property: Description("@#exposeOrigin")]bool ExposeOrigin = false,
	[property: Description("@#handle")]string? Handle = default,
	[property: Description("@#permittedOrigins")]string[]? PermittedOrigins = default);

/// <summary>
/// CaptureHandle
/// </summary>
[ECMAScript]
[Description("@#CaptureHandle")]
public record CaptureHandle(
    [property: Description("@#origin")]string? Origin = default,
	[property: Description("@#handle")]string? Handle = default);

/// <summary>
/// MediaStreamTrack
/// </summary>
[ECMAScript]
[Description("@#MediaStreamTrack")]
public partial class MediaStreamTrack : EventTarget
{
    /// <summary>
    /// getCaptureHandle
    /// </summary>
    [Description("@#getCaptureHandle")]
    public extern CaptureHandle? GetCaptureHandle();

    /// <summary>
    /// oncapturehandlechange
    /// </summary>
    [Description("@#oncapturehandlechange")]
    public extern EventHandler Oncapturehandlechange { get; set; }

    /// <summary>
    /// getSupportedCaptureActions
    /// </summary>
    [Description("@#getSupportedCaptureActions")]
    public extern string[] GetSupportedCaptureActions();

    /// <summary>
    /// sendCaptureAction
    /// </summary>
    /// <param name="action">action</param>
    [Description("@#sendCaptureAction")]
    public extern PromiseResult<object> SendCaptureAction(CaptureAction action);

    /// <summary>
    /// kind
    /// </summary>
    [Description("@#kind")]
    public extern string Kind { get; }

    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern string Id { get; }

    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; }

    /// <summary>
    /// enabled
    /// </summary>
    [Description("@#enabled")]
    public extern bool Enabled { get; set; }

    /// <summary>
    /// muted
    /// </summary>
    [Description("@#muted")]
    public extern bool Muted { get; }

    /// <summary>
    /// onmute
    /// </summary>
    [Description("@#onmute")]
    public extern EventHandler Onmute { get; set; }

    /// <summary>
    /// onunmute
    /// </summary>
    [Description("@#onunmute")]
    public extern EventHandler Onunmute { get; set; }

    /// <summary>
    /// readyState
    /// </summary>
    [Description("@#readyState")]
    public extern MediaStreamTrackState ReadyState { get; }

    /// <summary>
    /// onended
    /// </summary>
    [Description("@#onended")]
    public extern EventHandler Onended { get; set; }

    /// <summary>
    /// clone
    /// </summary>
    [Description("@#clone")]
    public extern MediaStreamTrack Clone();

    /// <summary>
    /// stop
    /// </summary>
    [Description("@#stop")]
    public extern void Stop();

    /// <summary>
    /// getCapabilities
    /// </summary>
    [Description("@#getCapabilities")]
    public extern MediaTrackCapabilities GetCapabilities();

    /// <summary>
    /// getConstraints
    /// </summary>
    [Description("@#getConstraints")]
    public extern MediaTrackConstraints GetConstraints();

    /// <summary>
    /// getSettings
    /// </summary>
    [Description("@#getSettings")]
    public extern MediaTrackSettings GetSettings();

    /// <summary>
    /// applyConstraints
    /// </summary>
    /// <param name="constraints">constraints</param>
    [Description("@#applyConstraints")]
    public extern PromiseResult<object> ApplyConstraints(MediaTrackConstraints? constraints = default);

    /// <summary>
    /// contentHint
    /// </summary>
    [Description("@#contentHint")]
    public extern string ContentHint { get; set; }

    /// <summary>
    /// isolated
    /// </summary>
    [Description("@#isolated")]
    public extern bool Isolated { get; }

    /// <summary>
    /// onisolationchange
    /// </summary>
    [Description("@#onisolationchange")]
    public extern EventHandler Onisolationchange { get; set; }
}