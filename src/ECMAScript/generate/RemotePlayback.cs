namespace ECMAScript;

/// <summary>
/// RemotePlayback
/// </summary>
[ECMAScript]
[Description("@#RemotePlayback")]
public class RemotePlayback : EventTarget
{
    /// <summary>
    /// watchAvailability
    /// </summary>
    /// <param name="callback">callback</param>
    [Description("@#watchAvailability")]
    public extern PromiseResult<int> WatchAvailability(RemotePlaybackAvailabilityCallback callback);

    /// <summary>
    /// cancelWatchAvailability
    /// </summary>
    /// <param name="id">id</param>
    [Description("@#cancelWatchAvailability")]
    public extern PromiseResult<object> CancelWatchAvailability(int id);

    /// <summary>
    /// state
    /// </summary>
    [Description("@#state")]
    public extern RemotePlaybackState State { get; }

    /// <summary>
    /// onconnecting
    /// </summary>
    [Description("@#onconnecting")]
    public extern EventHandler Onconnecting { get; set; }

    /// <summary>
    /// onconnect
    /// </summary>
    [Description("@#onconnect")]
    public extern EventHandler Onconnect { get; set; }

    /// <summary>
    /// ondisconnect
    /// </summary>
    [Description("@#ondisconnect")]
    public extern EventHandler Ondisconnect { get; set; }

    /// <summary>
    /// prompt
    /// </summary>
    [Description("@#prompt")]
    public extern PromiseResult<object> Prompt();
}