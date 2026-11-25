namespace Jero.Web.App.Shell;

/// <summary>Process-local reactive snapshot shared by layouts and permission-aware pages.</summary>
[ECMAScriptModule("components/application-state.js")]
public static class ApplicationState
{
    private static Action<CurrentUser?>? userChanged;
    private static Action<AppBrandState>? brandChanged;

    public static CurrentUser? User { get; private set; }
    public static AppMenuItem[] Menus { get; private set; } = [];
    public static AppNavigationItem[] Navigation { get; private set; } = [];
    public static string[] Permissions { get; private set; } = [];
    public static AppBrandState Brand { get; private set; } = new();
    public static bool Ready { get; private set; }

    public static void BindUserChanged(Action<CurrentUser?> callback) => userChanged = callback;

    public static void ClearUserChanged(Action<CurrentUser?> callback)
    {
        // During navigation the new shell may bind its callback before the old shell unmounts.
        if (userChanged == callback)
            userChanged = null;
    }

    public static void BindBrandChanged(Action<AppBrandState> callback) => brandChanged = callback;

    public static void ClearBrandChanged(Action<AppBrandState> callback)
    {
        if (brandChanged == callback)
            brandChanged = null;
    }

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
        userChanged?.Invoke(currentUser);
    }

    public static void UpdateRuntimeConfig(string title, string logo, string watermark, string homePath)
    {
        Brand = new AppBrandState
        {
            Title = string.IsNullOrWhiteSpace(title) ? "Jero 管理平台" : title,
            Logo = logo
        };
        brandChanged?.Invoke(Brand);

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
