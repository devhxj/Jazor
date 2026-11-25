namespace Jero.Core.Service;

public class LogVisOutput
{
    /// <summary>
    /// 登录地点
    /// </summary>
    public string Location { get; set; } = string.Empty;
    /// <summary>
    /// 经度
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// 维度
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// 真实姓名
    /// </summary>
    public string RealName { get; set; } = string.Empty;
    /// <summary>
    /// 日志时间
    /// </summary>
    public DateTime? LogDateTime { get; set; }
}