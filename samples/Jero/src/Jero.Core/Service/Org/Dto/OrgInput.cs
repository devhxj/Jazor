namespace Jero.Core.Service;

public class OrgInput : BaseIdInput
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// 编码
    /// </summary>
    public string Code { get; set; } = string.Empty;
    /// <summary>
    /// 机构类型
    /// </summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>
    /// 租户Id
    /// </summary>
    public long TenantId { get; set; }
}

public class AddOrgInput : SysOrg
{
    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "机构名称不能为空")]
    public override string Name { get; set; } = string.Empty;
    /// <summary>
    /// 机构类型
    /// </summary>
    [Dict("org_type", ErrorMessage = "机构类型不能合法", AllowNullValue = true, AllowEmptyStrings = true)]
    public override string? Type { get; set; }
}

public class UpdateOrgInput : AddOrgInput
{
}

public class DeleteOrgInput : BaseIdInput
{
}