namespace Jero.Core.Service;

/// <summary>
/// 接口压测输出参数
/// </summary>
public class StressTestOutput
{
    /// <summary>
    /// 总请求次数
    /// </summary>
    public long TotalRequests { get; set; }

    /// <summary>
    /// 总用时（秒）
    /// </summary>
    public double TotalTimeInSeconds { get; set; }

    /// <summary>
    /// 成功请求次数
    /// </summary>
    public long SuccessfulRequests { get; set; }

    /// <summary>
    /// 失败请求次数
    /// </summary>
    public long FailedRequests { get; set; }

    /// <summary>
    /// 每秒查询率（QPS）
    /// </summary>
    public double QueriesPerSecond { get; set; }

    /// <summary>
    /// 最小响应时间（毫秒）
    /// </summary>
    public double MinResponseTime { get; set; }

    /// <summary>
    /// 最大响应时间（毫秒）
    /// </summary>
    public double MaxResponseTime { get; set; }

    /// <summary>
    /// 平均响应时间（毫秒）
    /// </summary>
    public double AverageResponseTime { get; set; }

    /// <summary>
    /// P10 响应时间（毫秒）
    /// </summary>
    public double Percentile10ResponseTime { get; set; }

    /// <summary>
    /// P25 响应时间（毫秒）
    /// </summary>
    public double Percentile25ResponseTime { get; set; }

    /// <summary>
    /// P50 响应时间（毫秒）
    /// </summary>
    public double Percentile50ResponseTime { get; set; }

    /// <summary>
    /// P75 响应时间（毫秒）
    /// </summary>
    public double Percentile75ResponseTime { get; set; }

    /// <summary>
    /// P90 响应时间（毫秒）
    /// </summary>
    public double Percentile90ResponseTime { get; set; }

    /// <summary>
    /// P99 响应时间（毫秒）
    /// </summary>
    public double Percentile99ResponseTime { get; set; }

    /// <summary>
    /// P999 响应时间（毫秒）
    /// </summary>
    public double Percentile999ResponseTime { get; set; }
}