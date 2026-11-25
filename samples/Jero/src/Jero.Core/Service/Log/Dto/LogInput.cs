namespace Jero.Core.Service;

public class PageOpLogInput : PageVisLogInput
{
    /// <summary>
    /// 模块名称
    /// </summary>
    public string? ControllerName { get; set; }
}

public class PageExLogInput : PageOpLogInput
{
}

public class PageVisLogInput : PageLogInput
{
    /// <summary>
    /// 方法名称
    ///</summary>
    public string? ActionName { get; set; }
}

public class PageLogInput : BasePageTimeInput
{
    /// <summary>
    /// 账号
    /// </summary>
    public string? Account { get; set; }

    /// <summary>
    /// 操作用时
    /// </summary>
    public long? Elapsed { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>
    /// IP地址
    /// </summary>
    public string? RemoteIp { get; set; }

    /// <summary>
    /// 租户Id
    /// </summary>
    public long TenantId { get; set; }
}

public class LogInput
{
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }
}