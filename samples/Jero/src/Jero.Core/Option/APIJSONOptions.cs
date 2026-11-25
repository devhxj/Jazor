namespace Jero.Core;

/// <summary>
/// APIJSON配置选项
/// </summary>
public sealed class APIJSONOptions : IConfigurableOptions
{
    /// <summary>
    /// 角色集合
    /// </summary>
    public List<APIJSON_Role> Roles { get; set; } = [];
}

/// <summary>
/// APIJSON角色权限
/// </summary>
public class APIJSON_Role
{
    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleName { get; set; } = string.Empty;
    /// <summary>
    /// 查询
    /// </summary>
    public APIJSON_RoleItem Select { get; set; } = new();
    /// <summary>
    /// 增加
    /// </summary>
    public APIJSON_RoleItem Insert { get; set; } = new();
    /// <summary>
    /// 更新
    /// </summary>
    public APIJSON_RoleItem Update { get; set; } = new();
    /// <summary>
    /// 删除
    /// </summary>
    public APIJSON_RoleItem Delete { get; set; } = new();
}

/// <summary>
/// APIJSON角色权限内容
/// </summary>
public class APIJSON_RoleItem
{
    /// <summary>
    /// 表集合
    /// </summary>
    public string[] Table { get; set; } = [];
    /// <summary>
    /// 列集合
    /// </summary>
    public string[] Column { get; set; } = [];
    /// <summary>
    /// 过滤器
    /// </summary>
    public string[] Filter { get; set; } = [];
}