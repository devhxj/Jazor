namespace Jero.Web.Application;

/// <summary>当前登录用户的身份、租户和按钮权限，供页面在会话期间共享。</summary>
public sealed record CurrentUser
{
    public string Id { get; init; } = string.Empty;
    public string Account { get; init; } = string.Empty;
    public string RealName { get; init; } = string.Empty;
    public string Avatar { get; init; } = string.Empty;
    public string OrgName { get; init; } = string.Empty;
    public string PosName { get; init; } = string.Empty;
    public string WatermarkText { get; init; } = string.Empty;
    public string LangCode { get; init; } = string.Empty;
    public string HomePath { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string CurrentTenantId { get; init; } = string.Empty;
    public int AccountType { get; init; }
    public string[] Buttons { get; init; } = [];
    public string[] RoleIds { get; init; } = [];
}

public sealed record AppBrandState
{
    public string Title { get; init; } = "Jero 管理平台";
    public string Logo { get; init; } = string.Empty;
}

public sealed record AppMenuMeta
{
    public string Title { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public bool IsIframe { get; init; }
    public string Link { get; init; } = string.Empty;
    public bool IsHidden { get; init; }
    public bool IsKeepAlive { get; init; }
    public bool IsAffix { get; init; }
}

/// <summary>登录菜单树的节点，保留服务端的父子关系与路由配置。</summary>
public sealed record AppMenuItem
{
    public string Id { get; init; } = string.Empty;
    public string ParentId { get; init; } = string.Empty;
    public int Type { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public string Component { get; init; } = string.Empty;
    public string Permission { get; init; } = string.Empty;
    public string Redirect { get; init; } = string.Empty;
    public int OrderNo { get; init; }
    public AppMenuMeta Meta { get; init; } = new();
    public AppMenuItem[] Children { get; init; } = [];
}

/// <summary>从菜单树提取的可见菜单项，包含侧栏分组、应用路由和外部链接。</summary>
public sealed record AppNavigationItem
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string SectionTitle { get; init; } = string.Empty;
    public bool StartsSection { get; init; }
    public string BackendPath { get; init; } = string.Empty;
    public string Route { get; init; } = string.Empty;
    public bool IsSupported { get; init; }
    public bool IsExternal { get; init; }
    public bool IsIframe { get; init; }
    public bool IsAffix { get; init; }
    public string ExternalUrl { get; init; } = string.Empty;
}

public sealed record AppNavigationTab
{
    public string Title { get; init; } = string.Empty;
    public string Route { get; init; } = string.Empty;
    public bool IsAffix { get; init; }
}

/// <summary>登录后加载的应用快照，同时保存原始菜单树和侧栏使用的导航项。</summary>
public sealed record AppBootstrapState
{
    public bool Ok { get; init; }
    public bool Unauthorized { get; init; }
    public string? Error { get; init; }
    public CurrentUser? User { get; init; }
    public AppMenuItem[] Menus { get; init; } = [];
    public AppNavigationItem[] Navigation { get; init; } = [];
    public string[] Permissions { get; init; } = [];
    public AppBrandState Brand { get; init; } = new();
}
