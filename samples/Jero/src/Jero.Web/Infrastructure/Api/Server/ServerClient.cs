using Jero.Web.Application;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.Server;

/// <summary>读取服务器基本信息、资源使用情况、磁盘和程序集列表。</summary>
[ECMAScriptModule("components/server-client.js")]
public static class ServerClient
{
    public static IPromise<ApiOutcome> BaseInfo()
        => BrowserApiClient.Get("/api/sysServer/serverBase");

    public static IPromise<ApiOutcome> UsedInfo()
        => BrowserApiClient.Get("/api/sysServer/serverUsed");

    public static IPromise<ApiOutcome> Disks()
        => BrowserApiClient.Get("/api/sysServer/serverDisk");

    public static IPromise<ApiOutcome> Assemblies()
        => BrowserApiClient.Get("/api/sysServer/assemblyList");

    public static ServerBaseView ReadBaseInfo(object? data)
    {
        if (data is null)
            return new ServerBaseView();

        return new ServerBaseView
        {
            HostName = ReadString(data, "hostName"),
            SystemOs = ReadString(data, "systemOs"),
            OsArchitecture = ReadString(data, "osArchitecture"),
            ProcessorCount = ReadString(data, "processorCount"),
            SysRunTime = ReadString(data, "sysRunTime"),
            RemoteIp = ReadString(data, "remoteIp"),
            LocalIp = ReadString(data, "localIp"),
            FrameworkDescription = ReadString(data, "frameworkDescription"),
            Environment = ReadString(data, "environment"),
            Wwwroot = ReadString(data, "wwwroot"),
            Stage = ReadString(data, "stage")
        };
    }

    public static ServerUsedView ReadUsedInfo(object? data)
    {
        if (data is null)
            return new ServerUsedView();

        return new ServerUsedView
        {
            FreeRam = ReadString(data, "freeRam"),
            UsedRam = ReadString(data, "usedRam"),
            TotalRam = ReadString(data, "totalRam"),
            RamRate = ReadString(data, "ramRate"),
            CpuRates = ReadString(data, "cpuRates"),
            CpuRate = ReadString(data, "cpuRate"),
            StartTime = ReadString(data, "startTime"),
            RunTime = ReadString(data, "runTime")
        };
    }

    public static ServerDiskView[] ReadDisks(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new ServerDiskView[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            var percentText = ReadString(value, "availablePercent");
            result[index] = new ServerDiskView
            {
                DiskName = ReadString(value, "diskName"),
                TypeName = ReadString(value, "typeName"),
                TotalSize = ReadString(value, "totalSize"),
                Used = ReadString(value, "used"),
                AvailableFreeSpace = ReadString(value, "availableFreeSpace"),
                AvailablePercentText = percentText,
                AvailablePercent = ParsePercent(percentText)
            };
        }
        return result;
    }

    public static ServerAssemblyView[] ReadAssemblies(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new ServerAssemblyView[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new ServerAssemblyView
            {
                Name = ReadString(value, "name"),
                Version = ReadString(value, "version")
            };
        }
        return result;
    }

    public static int ParsePercent(string value)
    {
        var normalized = value.Trim().Replace("%", string.Empty);
        if (normalized.Length == 0)
            return 0;

        var number = NumberValue(normalized);
        if (!Number.IsFinite(number))
            return 0;
        if (number < 0)
            return 0;
        if (number > 100)
            return 100;
        return (int)Math.Round((double)number);
    }

    private static string ReadString(object data, string key)
    {
        var value = Reflect.Get(data, key);
        if (value is null)
            return string.Empty;
        var result = StringValue(value).Trim();
        return result == "null" || result == "undefined" ? string.Empty : result;
    }
}
