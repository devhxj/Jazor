namespace Jero.Web.Pages.Profile;

/// <summary>
/// 个人中心发布变更的用户字段，由应用壳合并到共享状态。
/// </summary>
[ECMAScriptModule("components/profile-state-change.js")]
public static class ProfileStateChange
{
    private static Action<ProfileShellChange>? _changed;

    public static void Bind(Action<ProfileShellChange> callback) => _changed = callback;

    public static void Clear(Action<ProfileShellChange> callback)
    {
        if (_changed == callback)
            _changed = null;
    }

    public static void Notify(ProfileShellChange change) => _changed?.Invoke(change);
}

public sealed record ProfileShellChange
{
    /// <summary>null 表示本次没有修改该字段；空字符串是有效的新值。</summary>
    public string? RealName { get; init; }

    /// <summary>null 表示本次没有修改该字段。</summary>
    public string? Avatar { get; init; }
}
