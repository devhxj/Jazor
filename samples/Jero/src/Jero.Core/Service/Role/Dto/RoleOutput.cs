namespace Jero.Core.Service;

/// <summary>
/// 角色列表输出参数
/// </summary>
public class RoleOutput
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// 编码
    /// </summary>
    public string Code { get; set; } = string.Empty;
    /// <summary>
    /// 租户Id
    /// </summary>
    public long tenantId { get; set; }
}