namespace Jero.Web.Application;

public sealed record StressTestResult
{
    public string TotalRequests { get; init; } = "0";
    public string SuccessfulRequests { get; init; } = "0";
    public string FailedRequests { get; init; } = "0";
    // 页面在第一次压测前就会计算统计值。显式写入 0，避免缺省字段参与 JavaScript 运算时得到 NaN。
    public double TotalTimeInSeconds { get; init; } = 0;
    public double QueriesPerSecond { get; init; } = 0;
    public double MinResponseTime { get; init; } = 0;
    public double MaxResponseTime { get; init; } = 0;
    public double AverageResponseTime { get; init; } = 0;
    public double Percentile10ResponseTime { get; init; } = 0;
    public double Percentile25ResponseTime { get; init; } = 0;
    public double Percentile50ResponseTime { get; init; } = 0;
    public double Percentile75ResponseTime { get; init; } = 0;
    public double Percentile90ResponseTime { get; init; } = 0;
    public double Percentile99ResponseTime { get; init; } = 0;
    public double Percentile999ResponseTime { get; init; } = 0;
}
