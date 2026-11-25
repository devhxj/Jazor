namespace Jero.Web.App.Realtime;

/// <summary>
/// Application-facing SignalR state. Consumers receive normalized payloads and own their
/// subscription lifetime through the returned disposer callbacks.
/// </summary>
[ECMAScriptModule("components/realtime-state.js")]
public static class RealtimeState
{
    private static Action<RealtimeConnectionStatus>[] connectionStatusListeners = [];
    private static Action<RealtimePublicNotice>[] publicNoticeListeners = [];
    private static Action<string>[] forceOfflineListeners = [];
    private static Action[] onlineUserListListeners = [];

    public static RealtimeConnectionStatus ConnectionStatus { get; private set; }
        = RealtimeConnectionStatus.Disconnected;

    public static bool IsConnected => ConnectionStatus == RealtimeConnectionStatus.Connected;

    public static Action SubscribeConnectionStatus(Action<RealtimeConnectionStatus> listener)
    {
        connectionStatusListeners = Add(connectionStatusListeners, listener);
        return () => connectionStatusListeners = Remove(connectionStatusListeners, listener);
    }

    public static Action SubscribePublicNotice(Action<RealtimePublicNotice> listener)
    {
        publicNoticeListeners = Add(publicNoticeListeners, listener);
        return () => publicNoticeListeners = Remove(publicNoticeListeners, listener);
    }

    public static Action SubscribeForceOffline(Action<string> listener)
    {
        forceOfflineListeners = Add(forceOfflineListeners, listener);
        return () => forceOfflineListeners = Remove(forceOfflineListeners, listener);
    }

    /// <summary>
    /// Subscribes to the online-user change signal. The page reloads its authoritative HTTP
    /// result instead of treating the Hub's ten-row broadcast snapshot as a page response.
    /// </summary>
    public static Action SubscribeOnlineUserList(Action listener)
    {
        for (var index = 0; index < onlineUserListListeners.Length; index++)
            if (onlineUserListListeners[index] == listener)
                return () => RemoveOnlineUserListListener(listener);

        var next = new Action[onlineUserListListeners.Length + 1];
        for (var index = 0; index < onlineUserListListeners.Length; index++)
            next[index] = onlineUserListListeners[index];
        next[onlineUserListListeners.Length] = listener;
        onlineUserListListeners = next;
        return () => RemoveOnlineUserListListener(listener);
    }

    internal static void SetConnectionStatus(RealtimeConnectionStatus status)
    {
        if (ConnectionStatus == status)
            return;

        ConnectionStatus = status;
        for (var index = 0; index < connectionStatusListeners.Length; index++)
            connectionStatusListeners[index](status);
    }

    internal static void EmitPublicNotice(RealtimePublicNotice notice)
    {
        for (var index = 0; index < publicNoticeListeners.Length; index++)
            publicNoticeListeners[index](notice);
    }

    internal static void EmitForceOffline(string message)
    {
        for (var index = 0; index < forceOfflineListeners.Length; index++)
            forceOfflineListeners[index](message);
    }

    internal static void EmitOnlineUserList()
    {
        for (var index = 0; index < onlineUserListListeners.Length; index++)
            onlineUserListListeners[index]();
    }

    private static void RemoveOnlineUserListListener(Action listener)
    {
        var removeIndex = -1;
        for (var index = 0; index < onlineUserListListeners.Length; index++)
        {
            if (onlineUserListListeners[index] == listener)
            {
                removeIndex = index;
                break;
            }
        }

        if (removeIndex < 0)
            return;

        var next = new Action[onlineUserListListeners.Length - 1];
        var target = 0;
        for (var index = 0; index < onlineUserListListeners.Length; index++)
        {
            if (index == removeIndex)
                continue;
            next[target++] = onlineUserListListeners[index];
        }
        onlineUserListListeners = next;
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

/// <summary>Normalized SysNotice payload; every database identifier remains decimal text.</summary>
public sealed record RealtimePublicNotice : VueProps
{
    public string id { get; init; } = string.Empty;
    public string title { get; init; } = string.Empty;
    public string content { get; init; } = string.Empty;
    public int type { get; init; }
    public int status { get; init; }
    public string publicUserId { get; init; } = string.Empty;
    public string publicUserName { get; init; } = string.Empty;
    public string publicOrgId { get; init; } = string.Empty;
    public string publicOrgName { get; init; } = string.Empty;
    public string publicTime { get; init; } = string.Empty;
    public string cancelTime { get; init; } = string.Empty;
    public string createTime { get; init; } = string.Empty;
    public string createUserId { get; init; } = string.Empty;
    public string createUserName { get; init; } = string.Empty;
    public string updateTime { get; init; } = string.Empty;
    public string updateUserId { get; init; } = string.Empty;
    public string updateUserName { get; init; } = string.Empty;
}
