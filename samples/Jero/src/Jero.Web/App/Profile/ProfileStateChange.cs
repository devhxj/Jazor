namespace Jero.Web.App.Profile;

/// <summary>
/// 个人中心发布变更的用户字段，由应用壳合并到共享状态。
/// </summary>
[ECMAScriptModule("components/profile-state-change.js")]
public static class ProfileStateChange
{
    private static Action<ProfileShellChange>? changed;

    public static void Bind(Action<ProfileShellChange> callback) => changed = callback;

    public static void Clear(Action<ProfileShellChange> callback)
    {
        if (changed == callback)
            changed = null;
    }

    public static void Notify(ProfileShellChange change) => changed?.Invoke(change);
}

public sealed record ProfileShellChange
{
    /// <summary>null 表示本次没有修改该字段；空字符串是有效的新值。</summary>
    public string? RealName { get; init; }

    /// <summary>null 表示本次没有修改该字段。</summary>
    public string? Avatar { get; init; }
}
