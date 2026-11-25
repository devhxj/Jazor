namespace Jero.Web.App.StressTest;

public sealed record StressBrowseGroup(string Name, string Summary, StressBrowseNode[] Nodes);

public sealed record StressBrowseNode(string Id, string Method, string Path, string Summary, int GroupIndex, int NodeIndex);

public sealed record StressTestResult
{
    public string totalRequests { get; init; } = "0";
    public string successfulRequests { get; init; } = "0";
    public string failedRequests { get; init; } = "0";
    // Jazor runtime objects need explicit initializers; CLR default(T) is not replayed for
    // omitted reactive fields, which would otherwise render as JavaScript NaN before a run.
    public double totalTimeInSeconds { get; init; } = 0;
    public double queriesPerSecond { get; init; } = 0;
    public double minResponseTime { get; init; } = 0;
    public double maxResponseTime { get; init; } = 0;
    public double averageResponseTime { get; init; } = 0;
    public double percentile10ResponseTime { get; init; } = 0;
    public double percentile25ResponseTime { get; init; } = 0;
    public double percentile50ResponseTime { get; init; } = 0;
    public double percentile75ResponseTime { get; init; } = 0;
    public double percentile90ResponseTime { get; init; } = 0;
    public double percentile99ResponseTime { get; init; } = 0;
    public double percentile999ResponseTime { get; init; } = 0;
}

public sealed record StressPairView(string key, string value);
