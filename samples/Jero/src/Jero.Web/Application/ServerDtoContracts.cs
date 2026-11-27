namespace Jero.Web.Application;

public sealed record ServerBaseView
{
    public string HostName { get; init; } = string.Empty;
    public string SystemOs { get; init; } = string.Empty;
    public string OsArchitecture { get; init; } = string.Empty;
    public string ProcessorCount { get; init; } = string.Empty;
    public string SysRunTime { get; init; } = string.Empty;
    public string RemoteIp { get; init; } = string.Empty;
    public string LocalIp { get; init; } = string.Empty;
    public string FrameworkDescription { get; init; } = string.Empty;
    public string Environment { get; init; } = string.Empty;
    public string Wwwroot { get; init; } = string.Empty;
    public string Stage { get; init; } = string.Empty;
}

public sealed record ServerUsedView
{
    public string FreeRam { get; init; } = string.Empty;
    public string UsedRam { get; init; } = string.Empty;
    public string TotalRam { get; init; } = string.Empty;
    public string RamRate { get; init; } = string.Empty;
    public string CpuRates { get; init; } = string.Empty;
    public string CpuRate { get; init; } = string.Empty;
    public string StartTime { get; init; } = string.Empty;
    public string RunTime { get; init; } = string.Empty;
}

public sealed record ServerDiskView
{
    public string DiskName { get; init; } = string.Empty;
    public string TypeName { get; init; } = string.Empty;
    public string TotalSize { get; init; } = string.Empty;
    public string Used { get; init; } = string.Empty;
    public string AvailableFreeSpace { get; init; } = string.Empty;
    public string AvailablePercentText { get; init; } = string.Empty;
    public int AvailablePercent { get; init; }
}

public sealed record ServerAssemblyView
{
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
}
