namespace Jero.Web.Application;

/// <summary>浏览器端的应用快照，由布局和使用按钮权限的页面共享。</summary>
[ECMAScriptModule("components/application-state.js")]
public static class ApplicationState
{
    private static Action<CurrentUser?>? _userChanged;
    private static Action<AppBrandState>? _brandChanged;

    public static CurrentUser? User { get; private set; }
    public static AppMenuItem[] Menus { get; private set; } = [];
    public static AppNavigationItem[] Navigation { get; private set; } = [];
    public static string[] Permissions { get; private set; } = [];
    public static AppBrandState Brand { get; private set; } = new();
    public static bool Ready { get; private set; }

    public static void BindUserChanged(Action<CurrentUser?> callback) => _userChanged = callback;

    public static void ClearUserChanged(Action<CurrentUser?> callback)
    {
        // 导航时新布局可能先绑定回调，旧布局卸载只清理自己注册的回调。
        if (_userChanged == callback)
            _userChanged = null;
    }

    public static void BindBrandChanged(Action<AppBrandState> callback) => _brandChanged = callback;

    public static void ClearBrandChanged(Action<AppBrandState> callback)
    {
        if (_brandChanged == callback)
            _brandChanged = null;
    }

    /// <summary>应用初始化快照，填充用户、菜单和权限后再按初始化结果设置 Ready。</summary>
    public static void Apply(AppBootstrapState state)
    {
        User = state.User;
        Menus = state.Menus;
        Navigation = state.Navigation;
        Permissions = state.Permissions;
        Brand = state.Brand;
        Ready = state.Ok;
    }

    public static void UpdateUser(CurrentUser currentUser)
    {
        User = currentUser;
        _userChanged?.Invoke(currentUser);
    }

    public static void UpdateRuntimeConfig(string title, string logo, string watermark, string homePath)
    {
        Brand = new AppBrandState
        {
            Title = string.IsNullOrWhiteSpace(title) ? "Jero 管理平台" : title,
            Logo = logo
        };
        _brandChanged?.Invoke(Brand);

        var current = User;
        if (current is null)
            return;

        // 品牌保存后只替换运行时配置字段；身份、租户和权限快照必须保持原值。
        UpdateUser(new CurrentUser
        {
            Id = current.Id,
            Account = current.Account,
            RealName = current.RealName,
            Avatar = current.Avatar,
            OrgName = current.OrgName,
            PosName = current.PosName,
            WatermarkText = watermark,
            LangCode = current.LangCode,
            HomePath = homePath,
            TenantId = current.TenantId,
            CurrentTenantId = current.CurrentTenantId,
            AccountType = current.AccountType,
            Buttons = current.Buttons,
            RoleIds = current.RoleIds
        });
    }

    public static void Clear()
    {
        User = null;
        Menus = [];
        Navigation = [];
        Permissions = [];
        Brand = new AppBrandState();
        Ready = false;
    }

    /// <summary>根据本地权限快照判断按钮是否可见。服务端接口独立执行授权。</summary>
    public static bool HasPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            return true;

        if (User?.AccountType == 999)
            return true;

        for (var index = 0; index < Permissions.Length; index++)
        {
            if (Permissions[index] == permission)
                return true;
        }

        return false;
    }
}
