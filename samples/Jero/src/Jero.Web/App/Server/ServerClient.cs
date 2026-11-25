using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Server;

/// <summary>Typed browser boundary for the four server-monitoring endpoints.</summary>
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
            hostName = ReadString(data, "hostName"),
            systemOs = ReadString(data, "systemOs"),
            osArchitecture = ReadString(data, "osArchitecture"),
            processorCount = ReadString(data, "processorCount"),
            sysRunTime = ReadString(data, "sysRunTime"),
            remoteIp = ReadString(data, "remoteIp"),
            localIp = ReadString(data, "localIp"),
            frameworkDescription = ReadString(data, "frameworkDescription"),
            environment = ReadString(data, "environment"),
            wwwroot = ReadString(data, "wwwroot"),
            stage = ReadString(data, "stage")
        };
    }

    public static ServerUsedView ReadUsedInfo(object? data)
    {
        if (data is null)
            return new ServerUsedView();

        return new ServerUsedView
        {
            freeRam = ReadString(data, "freeRam"),
            usedRam = ReadString(data, "usedRam"),
            totalRam = ReadString(data, "totalRam"),
            ramRate = ReadString(data, "ramRate"),
            cpuRates = ReadString(data, "cpuRates"),
            cpuRate = ReadString(data, "cpuRate"),
            startTime = ReadString(data, "startTime"),
            runTime = ReadString(data, "runTime")
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
                diskName = ReadString(value, "diskName"),
                typeName = ReadString(value, "typeName"),
                totalSize = ReadString(value, "totalSize"),
                used = ReadString(value, "used"),
                availableFreeSpace = ReadString(value, "availableFreeSpace"),
                availablePercentText = percentText,
                availablePercent = ParsePercent(percentText)
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
                name = ReadString(value, "name"),
                version = ReadString(value, "version")
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
