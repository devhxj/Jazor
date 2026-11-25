namespace Jero.Core.Service;

/// <summary>
/// 系统域登录信息配置输入参数
/// </summary>
public class SysLdapInput : BasePageInput
{
    /// <summary>
    /// 主机
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// 租户Id
    /// </summary>
    public long TenantId { get; set; }
}

public class AddSysLdapInput : SysLdap
{
}

public class UpdateSysLdapInput : SysLdap
{
}

public class DeleteSysLdapInput : BaseIdInput
{
}

public class DetailSysLdapInput : BaseIdInput
{
}

public class SyncSysLdapInput : BaseIdInput
{
}