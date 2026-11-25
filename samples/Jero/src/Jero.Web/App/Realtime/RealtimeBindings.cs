using System.ComponentModel;

namespace Jero.Web.App.Realtime;

/// <summary>Typed binding for the SignalR browser client used by Jero's online-user hub.</summary>
[ECMAScript("@microsoft/signalr")]
[Description("@#HubConnectionBuilder")]
internal sealed class RealtimeHubConnectionBuilder
{
    public RealtimeHubConnectionBuilder()
    {
    }

    [Description("@#configureLogging")]
    public extern RealtimeHubConnectionBuilder ConfigureLogging(RealtimeLogLevel level);

    [Description("@#withUrl")]
    public extern RealtimeHubConnectionBuilder WithUrl(string url, RealtimeHttpConnectionOptions options);

    [Description("@#withAutomaticReconnect")]
    public extern RealtimeHubConnectionBuilder WithAutomaticReconnect(RealtimeRetryPolicy policy);

    [Description("@#build")]
    public extern RealtimeHubConnection Build();
}

/// <summary>Runtime HubConnection surface required by the application singleton.</summary>
[ECMAScript("@microsoft/signalr")]
[Description("@#HubConnection")]
internal sealed class RealtimeHubConnection
{
#pragma warning disable CS0626 // JavaScript owns these accessors.
    [Description("@#state")]
    public extern string State { get; }

    [Description("@#keepAliveIntervalInMilliseconds")]
    public extern int KeepAliveIntervalInMilliseconds { get; set; }

    [Description("@#serverTimeoutInMilliseconds")]
    public extern int ServerTimeoutInMilliseconds { get; set; }
#pragma warning restore CS0626

    [Description("@#start")]
    public extern IPromise Start();

    [Description("@#stop")]
    public extern IPromise Stop();

    [Description("@#on")]
    public extern void On(string methodName, Action<object?> handler);

    [Description("@#off")]
    public extern void Off(string methodName, Action<object?> handler);

    [Description("@#onreconnecting")]
    public extern void OnReconnecting(Action<Error?> handler);

    [Description("@#onreconnected")]
    public extern void OnReconnected(Action<string?> handler);

    [Description("@#onclose")]
    public extern void OnClose(Action<Error?> handler);
}

internal sealed record RealtimeHttpConnectionOptions
{
    public bool skipNegotiation { get; init; }
    public RealtimeHttpTransportType transport { get; init; }
}

internal sealed record RealtimeRetryPolicy
{
    public Func<RealtimeRetryContext, int> nextRetryDelayInMilliseconds { get; init; } = default!;
}

internal sealed record RealtimeRetryContext
{
    public int previousRetryCount { get; }
    public int elapsedMilliseconds { get; }
    public Error retryReason { get; } = default!;
}

/// <summary>SignalR's transport bit flags; this slice intentionally exposes WebSockets only.</summary>
internal enum RealtimeHttpTransportType
{
    WebSockets = 1
}

/// <summary>SignalR logging levels used by the application connection builder.</summary>
internal enum RealtimeLogLevel
{
    Warning = 3
}
