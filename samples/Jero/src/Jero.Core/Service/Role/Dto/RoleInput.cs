namespace Jero.Core.Service;

public class RoleInput : BaseIdInput
{
    /// <summary>
    /// 状态
    /// </summary>
    public virtual StatusEnum Status { get; set; }
}

public class PageRoleInput : BasePageInput
{
    /// <summary>
    /// 租户Id
    /// </summary>
    public long TenantId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public virtual string Name { get; set; } = string.Empty;
    /// <summary>
    /// 编码
    /// </summary>
    public virtual string Code { get; set; } = string.Empty;
}

public class AddRoleInput : SysRole
{
    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "角色名称不能为空")]
    public override string Name { get; set; } = string.Empty;
    /// <summary>
    /// 菜单Id集合
    /// </summary>
    public List<long> MenuIdList { get; set; } = [];
}

public class UpdateRoleInput : AddRoleInput
{
}

public class DeleteRoleInput : BaseIdInput
{
}