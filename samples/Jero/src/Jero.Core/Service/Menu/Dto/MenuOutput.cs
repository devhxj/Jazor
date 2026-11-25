namespace Jero.Core.Service;

/// <summary>
/// 系统菜单返回结果
/// </summary>
public class MenuOutput
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 父Id
    /// </summary>
    public long Pid { get; set; }

    /// <summary>
    /// 菜单类型（0目录 1菜单 2按钮）
    /// </summary>
    public MenuTypeEnum Type { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// 路由地址
    /// </summary>
    public string Path { get; set; } = string.Empty;
    /// <summary>
    /// 组件路径
    /// </summary>
    public string Component { get; set; } = string.Empty;
    /// <summary>
    /// 权限标识
    /// </summary>
    public string Permission { get; set; } = string.Empty;
    /// <summary>
    /// 重定向
    /// </summary>
    public string Redirect { get; set; } = string.Empty;
    /// <summary>
    /// 排序
    /// </summary>
    public int OrderNo { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public StatusEnum Status { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; } = string.Empty;
    /// <summary>
    /// 创建时间
    /// </summary>
    public virtual DateTime CreateTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public virtual DateTime UpdateTime { get; set; }

    /// <summary>
    /// 创建者姓名
    /// </summary>
    public virtual string CreateUserName { get; set; } = string.Empty;
    /// <summary>
    /// 修改者姓名
    /// </summary>
    public virtual string UpdateUserName { get; set; } = string.Empty;
    /// <summary>
    /// 菜单Meta
    /// </summary>
    public SysMenuMeta Meta { get; set; } = new();

    /// <summary>
    /// 菜单子项
    /// </summary>
    public List<MenuOutput> Children { get; set; } = [];
}

/// <summary>
/// 菜单Meta配置
/// </summary>
public class SysMenuMeta
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; } = string.Empty;
    /// <summary>
    /// 是否内嵌
    /// </summary>
    public bool IsIframe { get; set; }

    /// <summary>
    /// 外链链接
    /// </summary>
    public string IsLink { get; set; } = string.Empty;
    /// <summary>
    /// 是否隐藏
    /// </summary>
    public bool IsHide { get; set; }

    /// <summary>
    /// 是否缓存
    /// </summary>
    public bool IsKeepAlive { get; set; }

    /// <summary>
    /// 是否固定
    /// </summary>
    public bool IsAffix { get; set; }
}

/// <summary>
/// 配置菜单对象映射
/// </summary>
public class SysMenuMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<SysMenu, MenuOutput>()
            .Map(t => t.Meta.Title, o => o.Title)
            .Map(t => t.Meta.Icon, o => o.Icon)
            .Map(t => t.Meta.IsIframe, o => o.IsIframe)
            .Map(t => t.Meta.IsLink, o => o.OutLink)
            .Map(t => t.Meta.IsHide, o => o.IsHide)
            .Map(t => t.Meta.IsKeepAlive, o => o.IsKeepAlive)
            .Map(t => t.Meta.IsAffix, o => o.IsAffix);
    }
}
