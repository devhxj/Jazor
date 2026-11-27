using System.ComponentModel;

namespace Jero.Web.Infrastructure.Realtime;

/// <summary>在线用户 Hub 使用的 SignalR 浏览器客户端构建器。</summary>
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

/// <summary>应用实时连接使用的 SignalR HubConnection 接口。</summary>
[ECMAScript("@microsoft/signalr")]
[Description("@#HubConnection")]
internal sealed class RealtimeHubConnection
{
#pragma warning disable CS0626 // 由 JavaScript 提供这些属性的实现。
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
    [ECMAScriptName("skipNegotiation")]
    public bool SkipNegotiation { get; init; }
    [ECMAScriptName("transport")]
    public RealtimeHttpTransportType Transport { get; init; }
}

internal sealed record RealtimeRetryPolicy
{
    [ECMAScriptName("nextRetryDelayInMilliseconds")]
    public Func<RealtimeRetryContext, int> NextRetryDelayInMilliseconds { get; init; } = default!;
}

internal sealed record RealtimeRetryContext
{
    [ECMAScriptName("previousRetryCount")]
    public int PreviousRetryCount { get; }
    [ECMAScriptName("elapsedMilliseconds")]
    public int ElapsedMilliseconds { get; }
    [ECMAScriptName("retryReason")]
    public Error RetryReason { get; } = default!;
}

/// <summary>在线用户连接使用的 SignalR WebSocket 传输标志。</summary>
internal enum RealtimeHttpTransportType
{
    WebSockets = 1
}

/// <summary>连接构建器使用的 SignalR 日志级别。</summary>
internal enum RealtimeLogLevel
{
    Warning = 3
}
