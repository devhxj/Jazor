namespace Jero.Web.Application;

/// <summary>
/// 向页面分发统一格式的 SignalR 事件。订阅返回解除函数，页面卸载时调用以释放回调。
/// </summary>
[ECMAScriptModule("components/realtime-state.js")]
public static class RealtimeState
{
    private static Action<RealtimeConnectionStatus>[] _connectionStatusListeners = [];
    private static Action<RealtimePublicNotice>[] _publicNoticeListeners = [];
    private static Action<string>[] _forceOfflineListeners = [];
    private static Action[] _onlineUserListListeners = [];

    public static RealtimeConnectionStatus ConnectionStatus { get; private set; }
        = RealtimeConnectionStatus.Disconnected;

    public static bool IsConnected => ConnectionStatus == RealtimeConnectionStatus.Connected;

    public static Action SubscribeConnectionStatus(Action<RealtimeConnectionStatus> listener)
    {
        _connectionStatusListeners = Add(_connectionStatusListeners, listener);
        return () => _connectionStatusListeners = Remove(_connectionStatusListeners, listener);
    }

    public static Action SubscribePublicNotice(Action<RealtimePublicNotice> listener)
    {
        _publicNoticeListeners = Add(_publicNoticeListeners, listener);
        return () => _publicNoticeListeners = Remove(_publicNoticeListeners, listener);
    }

    public static Action SubscribeForceOffline(Action<string> listener)
    {
        _forceOfflineListeners = Add(_forceOfflineListeners, listener);
        return () => _forceOfflineListeners = Remove(_forceOfflineListeners, listener);
    }

    /// <summary>
    /// Hub 广播的在线用户快照最多十条。页面收到通知后按当前查询条件重新发起 HTTP 分页请求。
    /// </summary>
    public static Action SubscribeOnlineUserList(Action listener)
    {
        for (var index = 0; index < _onlineUserListListeners.Length; index++)
            if (_onlineUserListListeners[index] == listener)
                return () => RemoveOnlineUserListListener(listener);

        var next = new Action[_onlineUserListListeners.Length + 1];
        for (var index = 0; index < _onlineUserListListeners.Length; index++)
            next[index] = _onlineUserListListeners[index];
        next[_onlineUserListListeners.Length] = listener;
        _onlineUserListListeners = next;
        return () => RemoveOnlineUserListListener(listener);
    }

    internal static void SetConnectionStatus(RealtimeConnectionStatus status)
    {
        if (ConnectionStatus == status)
            return;

        ConnectionStatus = status;
        for (var index = 0; index < _connectionStatusListeners.Length; index++)
            _connectionStatusListeners[index](status);
    }

    internal static void EmitPublicNotice(RealtimePublicNotice notice)
    {
        for (var index = 0; index < _publicNoticeListeners.Length; index++)
            _publicNoticeListeners[index](notice);
    }

    internal static void EmitForceOffline(string message)
    {
        for (var index = 0; index < _forceOfflineListeners.Length; index++)
            _forceOfflineListeners[index](message);
    }

    internal static void EmitOnlineUserList()
    {
        for (var index = 0; index < _onlineUserListListeners.Length; index++)
            _onlineUserListListeners[index]();
    }

    private static void RemoveOnlineUserListListener(Action listener)
    {
        var removeIndex = -1;
        for (var index = 0; index < _onlineUserListListeners.Length; index++)
        {
            if (_onlineUserListListeners[index] == listener)
            {
                removeIndex = index;
                break;
            }
        }

        if (removeIndex < 0)
            return;

        var next = new Action[_onlineUserListListeners.Length - 1];
        var target = 0;
        for (var index = 0; index < _onlineUserListListeners.Length; index++)
        {
            if (index == removeIndex)
                continue;
            next[target++] = _onlineUserListListeners[index];
        }
        _onlineUserListListeners = next;
    }

    private static Action<T>[] Add<T>(Action<T>[] listeners, Action<T> listener)
    {
        for (var index = 0; index < listeners.Length; index++)
            if (listeners[index] == listener)
                return listeners;

        var next = new Action<T>[listeners.Length + 1];
        for (var index = 0; index < listeners.Length; index++)
            next[index] = listeners[index];
        next[listeners.Length] = listener;
        return next;
    }

    private static Action<T>[] Remove<T>(Action<T>[] listeners, Action<T> listener)
    {
        var removeIndex = -1;
        for (var index = 0; index < listeners.Length; index++)
        {
            if (listeners[index] == listener)
            {
                removeIndex = index;
                break;
            }
        }

        if (removeIndex < 0)
            return listeners;

        var next = new Action<T>[listeners.Length - 1];
        var target = 0;
        for (var index = 0; index < listeners.Length; index++)
        {
            if (index == removeIndex)
                continue;
            next[target++] = listeners[index];
        }
        return next;
    }
}

public enum RealtimeConnectionStatus
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting
}

/// <summary>标准化的系统通知，所有数据库标识均保留为十进制文本。</summary>
public sealed record RealtimePublicNotice : VueProps
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public int Type { get; init; }
    public int Status { get; init; }
    public string PublicUserId { get; init; } = string.Empty;
    public string PublicUserName { get; init; } = string.Empty;
    public string PublicOrgId { get; init; } = string.Empty;
    public string PublicOrgName { get; init; } = string.Empty;
    public string PublicTime { get; init; } = string.Empty;
    public string CancelTime { get; init; } = string.Empty;
    public string CreateTime { get; init; } = string.Empty;
    public string CreateUserId { get; init; } = string.Empty;
    public string CreateUserName { get; init; } = string.Empty;
    public string UpdateTime { get; init; } = string.Empty;
    public string UpdateUserId { get; init; } = string.Empty;
    public string UpdateUserName { get; init; } = string.Empty;
}
