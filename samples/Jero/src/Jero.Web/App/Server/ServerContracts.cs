namespace Jero.Web.App.Server;

public sealed record ServerBaseView
{
    public string hostName { get; init; } = string.Empty;
    public string systemOs { get; init; } = string.Empty;
    public string osArchitecture { get; init; } = string.Empty;
    public string processorCount { get; init; } = string.Empty;
    public string sysRunTime { get; init; } = string.Empty;
    public string remoteIp { get; init; } = string.Empty;
    public string localIp { get; init; } = string.Empty;
    public string frameworkDescription { get; init; } = string.Empty;
    public string environment { get; init; } = string.Empty;
    public string wwwroot { get; init; } = string.Empty;
    public string stage { get; init; } = string.Empty;
}

public sealed record ServerUsedView
{
    public string freeRam { get; init; } = string.Empty;
    public string usedRam { get; init; } = string.Empty;
    public string totalRam { get; init; } = string.Empty;
    public string ramRate { get; init; } = string.Empty;
    public string cpuRates { get; init; } = string.Empty;
    public string cpuRate { get; init; } = string.Empty;
    public string startTime { get; init; } = string.Empty;
    public string runTime { get; init; } = string.Empty;
}

public sealed record ServerDiskView
{
    public string diskName { get; init; } = string.Empty;
    public string typeName { get; init; } = string.Empty;
    public string totalSize { get; init; } = string.Empty;
    public string used { get; init; } = string.Empty;
    public string availableFreeSpace { get; init; } = string.Empty;
    public string availablePercentText { get; init; } = string.Empty;
    public int availablePercent { get; init; }
}

public sealed record ServerAssemblyView
{
    public string name { get; init; } = string.Empty;
    public string version { get; init; } = string.Empty;
}
