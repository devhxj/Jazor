using Jero.Web.App.Auth;
using static ECMAScript.Global;

namespace Jero.Web.App.Realtime;

/// <summary>
/// Single online-user Hub connection for the authenticated browser session. UI effects and
/// logout navigation remain application-shell responsibilities; this boundary emits typed data.
/// </summary>
[ECMAScriptModule("components/realtime-client.js")]
public static class RealtimeClient
{
    private const string DisconnectedState = "Disconnected";
    private const string HubPath = "/hubs/onlineUser";
    private const string OnlineUserListEvent = "OnlineUserList";
    private const string PublicNoticeEvent = "PublicNotice";
    private const string ForceOfflineEvent = "ForceOffline";

    private static readonly Action<object?> OnlineUserListHandler = ReceiveOnlineUserList;
    private static readonly Action<object?> PublicNoticeHandler = ReceivePublicNotice;
    private static readonly Action<object?> ForceOfflineHandler = ReceiveForceOffline;
    private static readonly Action<Error?> ReconnectingHandler = _
        => RealtimeState.SetConnectionStatus(RealtimeConnectionStatus.Reconnecting);
    private static readonly Action<string?> ReconnectedHandler = _
        => RealtimeState.SetConnectionStatus(RealtimeConnectionStatus.Connected);
    private static readonly Action<Error?> ClosedHandler = _
        => RealtimeState.SetConnectionStatus(RealtimeConnectionStatus.Disconnected);

    private static RealtimeHubConnection? connection;
    private static IPromise? startPromise;
    private static bool forceOfflineHandling;

    public static IPromise Start()
    {
        var token = BrowserSessionStore.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
            return Stop();

        if (startPromise is not null)
            return startPromise;

        if (connection is not null && connection.State != DisconnectedState)
            return Promise.Resolve();

        if (connection is null)
            connection = CreateConnection(token!);

        var activeConnection = connection;
        RealtimeState.SetConnectionStatus(RealtimeConnectionStatus.Connecting);

        // One shared promise prevents layout rerenders or repeated bootstrap calls from racing
        // HubConnection.start(), which SignalR rejects while the same connection is starting.
        startPromise = activeConnection.Start()
            .Then(() =>
            {
                if (connection == activeConnection)
                    RealtimeState.SetConnectionStatus(RealtimeConnectionStatus.Connected);
            })
            .Catch(error =>
            {
                if (connection == activeConnection)
                {
                    UnbindServerEvents(activeConnection);
                    connection = null;
                    RealtimeState.SetConnectionStatus(RealtimeConnectionStatus.Disconnected);
                }

                // Preserve start() rejection so the application shell can decide whether to
                // surface it while the singleton still returns to a retryable state.
                throw error;
            })
            .Finally(() => startPromise = null);

        return startPromise;
    }

    public static IPromise Stop()
    {
        var activeConnection = connection;

        // Clear the singleton before awaiting SignalR. A concurrent caller must observe the
        // session as stopped even while the transport is finishing its close handshake.
        connection = null;
        startPromise = null;
        RealtimeState.SetConnectionStatus(RealtimeConnectionStatus.Disconnected);

        if (activeConnection is null)
            return Promise.Resolve();

        UnbindServerEvents(activeConnection);
        return activeConnection.State == DisconnectedState
            ? Promise.Resolve()
            : activeConnection.Stop();
    }

    private static RealtimeHubConnection CreateConnection(string token)
    {
        forceOfflineHandling = false;
        var nextConnection = new RealtimeHubConnectionBuilder()
            .ConfigureLogging(RealtimeLogLevel.Warning)
            .WithUrl(
                HubPath + "?token=" + EncodeURIComponent(token),
                new RealtimeHttpConnectionOptions
                {
                    skipNegotiation = true,
                    transport = RealtimeHttpTransportType.WebSockets
                })
            .WithAutomaticReconnect(new RealtimeRetryPolicy
            {
                nextRetryDelayInMilliseconds = _ => 5_000
            })
            .Build();

        nextConnection.KeepAliveIntervalInMilliseconds = 15_000;
        nextConnection.ServerTimeoutInMilliseconds = 30_000;
        nextConnection.On(OnlineUserListEvent, OnlineUserListHandler);
        nextConnection.On(PublicNoticeEvent, PublicNoticeHandler);
        nextConnection.On(ForceOfflineEvent, ForceOfflineHandler);
        nextConnection.OnReconnecting(ReconnectingHandler);
        nextConnection.OnReconnected(ReconnectedHandler);
        nextConnection.OnClose(ClosedHandler);
        return nextConnection;
    }

    private static void UnbindServerEvents(RealtimeHubConnection activeConnection)
    {
        // SignalR off(method, handler) requires the exact function instance passed to on().
        activeConnection.Off(OnlineUserListEvent, OnlineUserListHandler);
        activeConnection.Off(PublicNoticeEvent, PublicNoticeHandler);
        activeConnection.Off(ForceOfflineEvent, ForceOfflineHandler);
    }

    private static void ReceiveForceOffline(object? payload)
    {
        if (forceOfflineHandling)
            return;

        forceOfflineHandling = true;
        var message = payload is null ? string.Empty : StringValue(payload);
        RealtimeState.EmitForceOffline(
            string.IsNullOrWhiteSpace(message) ? "您已被强制下线" : message);
    }

    private static void ReceivePublicNotice(object? payload)
    {
        if (payload is null)
            return;

        RealtimeState.EmitPublicNotice(new RealtimePublicNotice
        {
            id = ReadString(payload, "id"),
            title = ReadString(payload, "title"),
            content = ReadString(payload, "content"),
            type = ReadInt(payload, "type"),
            status = ReadInt(payload, "status"),
            publicUserId = ReadString(payload, "publicUserId"),
            publicUserName = ReadString(payload, "publicUserName"),
            publicOrgId = ReadString(payload, "publicOrgId"),
            publicOrgName = ReadString(payload, "publicOrgName"),
            publicTime = ReadString(payload, "publicTime"),
            cancelTime = ReadString(payload, "cancelTime"),
            createTime = ReadString(payload, "createTime"),
            createUserId = ReadString(payload, "createUserId"),
            createUserName = ReadString(payload, "createUserName"),
            updateTime = ReadString(payload, "updateTime"),
            updateUserId = ReadString(payload, "updateUserId"),
            updateUserName = ReadString(payload, "updateUserName")
        });
    }

    private static void ReceiveOnlineUserList(object? payload)
    {
        RealtimeState.EmitOnlineUserList();
    }

    private static string ReadString(object payload, string key)
    {
        var value = Reflect.Get(payload, key);
        if (value is null)
            return string.Empty;
        var text = StringValue(value);
        return text == "null" || text == "undefined" ? string.Empty : text;
    }

    private static int ReadInt(object payload, string key)
    {
        var value = Reflect.Get(payload, key);
        return value is null ? 0 : (int)NumberValue(value);
    }

}
