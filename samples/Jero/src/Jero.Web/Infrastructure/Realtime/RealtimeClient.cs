using Jero.Web.Application;
using Jero.Web.Infrastructure.Storage;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Realtime;

/// <summary>
/// 为已登录会话维护一条在线用户 SignalR 连接，并向应用外壳发送类型化事件。
/// </summary>
[ECMAScriptModule("components/realtime-client.js")]
public static class RealtimeClient
{
    private const string DisconnectedState = "Disconnected";
    private const string HubPath = "/hubs/onlineUser";
    private const string OnlineUserListEvent = "OnlineUserList";
    private const string PublicNoticeEvent = "PublicNotice";
    private const string ForceOfflineEvent = "ForceOffline";

    private static readonly Action<object?> _onlineUserListHandler = ReceiveOnlineUserList;
    private static readonly Action<object?> _publicNoticeHandler = ReceivePublicNotice;
    private static readonly Action<object?> _forceOfflineHandler = ReceiveForceOffline;

    private static RealtimeHubConnection? _connection;
    private static IPromise? _startPromise;
    private static bool _forceOfflineHandling;

    /// <summary>启动或复用当前连接。并发启动调用共享同一个 Promise。</summary>
    public static IPromise Start()
    {
        var token = BrowserSessionStore.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
            return Stop();

        if (_startPromise is not null)
            return _startPromise;

        if (_connection is not null && _connection.State != DisconnectedState)
            return Promise.Resolve();

        if (_connection is null)
            _connection = CreateConnection(token!);

        var activeConnection = _connection;
        RealtimeState.SetConnectionStatus(RealtimeConnectionStatus.Connecting);

        // SignalR 会拒绝在同一连接尚在启动时再次调用 start，复用 Promise 可避免重复启动。
        IPromise? pending = null;
        pending = activeConnection.Start()
            .Then(() =>
            {
                if (_connection == activeConnection)
                    RealtimeState.SetConnectionStatus(RealtimeConnectionStatus.Connected);
            })
            .Catch(error =>
            {
                if (_connection == activeConnection)
                {
                    UnbindServerEvents(activeConnection);
                    _connection = null;
                    RealtimeState.SetConnectionStatus(RealtimeConnectionStatus.Disconnected);
                }

                // 恢复到可重试状态，同时把启动错误交给外壳决定是否提示用户。
                throw error;
            })
            .Finally(() =>
            {
                // Stop 之后可能已经开始新一次启动，旧 Promise 只能清理自己。
                if (_startPromise == pending)
                    _startPromise = null;
            });

        _startPromise = pending;
        return pending;
    }

    /// <summary>解除当前连接的事件订阅并启动关闭流程。</summary>
    public static IPromise Stop()
    {
        var activeConnection = _connection;

        // 先清除当前连接引用。随后调用 Start 时，即使旧连接仍在关闭，也会建立新连接。
        _connection = null;
        _startPromise = null;
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
        _forceOfflineHandling = false;
        var nextConnection = new RealtimeHubConnectionBuilder()
            .ConfigureLogging(RealtimeLogLevel.Warning)
            .WithUrl(
                HubPath + "?token=" + EncodeURIComponent(token),
                new RealtimeHttpConnectionOptions
                {
                    SkipNegotiation = true,
                    Transport = RealtimeHttpTransportType.WebSockets
                })
            .WithAutomaticReconnect(new RealtimeRetryPolicy
            {
                NextRetryDelayInMilliseconds = _ => 5_000
            })
            .Build();

        nextConnection.KeepAliveIntervalInMilliseconds = 15_000;
        nextConnection.ServerTimeoutInMilliseconds = 30_000;
        nextConnection.On(OnlineUserListEvent, _onlineUserListHandler);
        nextConnection.On(PublicNoticeEvent, _publicNoticeHandler);
        nextConnection.On(ForceOfflineEvent, _forceOfflineHandler);
        // SignalR 的生命周期回调可能在连接已被替换后才抵达。
        nextConnection.OnReconnecting(_ => SetConnectionStatus(nextConnection, RealtimeConnectionStatus.Reconnecting));
        nextConnection.OnReconnected(_ => SetConnectionStatus(nextConnection, RealtimeConnectionStatus.Connected));
        nextConnection.OnClose(_ => SetConnectionStatus(nextConnection, RealtimeConnectionStatus.Disconnected));
        return nextConnection;
    }

    private static void SetConnectionStatus(RealtimeHubConnection connection, RealtimeConnectionStatus status)
    {
        if (_connection == connection)
            RealtimeState.SetConnectionStatus(status);
    }

    private static void UnbindServerEvents(RealtimeHubConnection activeConnection)
    {
        // SignalR 取消订阅时必须传入注册时使用的同一个委托实例。
        activeConnection.Off(OnlineUserListEvent, _onlineUserListHandler);
        activeConnection.Off(PublicNoticeEvent, _publicNoticeHandler);
        activeConnection.Off(ForceOfflineEvent, _forceOfflineHandler);
    }

    private static void ReceiveForceOffline(object? payload)
    {
        if (_forceOfflineHandling)
            return;

        _forceOfflineHandling = true;
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
            Id = ReadString(payload, "id"),
            Title = ReadString(payload, "title"),
            Content = ReadString(payload, "content"),
            Type = ReadInt(payload, "type"),
            Status = ReadInt(payload, "status"),
            PublicUserId = ReadString(payload, "publicUserId"),
            PublicUserName = ReadString(payload, "publicUserName"),
            PublicOrgId = ReadString(payload, "publicOrgId"),
            PublicOrgName = ReadString(payload, "publicOrgName"),
            PublicTime = ReadString(payload, "publicTime"),
            CancelTime = ReadString(payload, "cancelTime"),
            CreateTime = ReadString(payload, "createTime"),
            CreateUserId = ReadString(payload, "createUserId"),
            CreateUserName = ReadString(payload, "createUserName"),
            UpdateTime = ReadString(payload, "updateTime"),
            UpdateUserId = ReadString(payload, "updateUserId"),
            UpdateUserName = ReadString(payload, "updateUserName")
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
