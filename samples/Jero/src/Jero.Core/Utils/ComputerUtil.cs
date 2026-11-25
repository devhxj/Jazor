namespace Jero.Core;

public static class ComputerUtil
{
    /// <summary>
    /// 内存信息
    /// </summary>
    /// <returns></returns>
    public static MemoryMetrics GetComputerInfo()
    {
        MemoryMetrics memoryMetrics;
        if (IsMacOS())
        {
            memoryMetrics = MemoryMetricsClient.GetMacOSMetrics();
        }
        else if (IsUnix())
        {
            memoryMetrics = MemoryMetricsClient.GetUnixMetrics();
        }
        else
        {
            memoryMetrics = MemoryMetricsClient.GetWindowsMetrics();
        }
        memoryMetrics.FreeRam = Math.Round(memoryMetrics.Free / 1024, 2) + "GB";
        memoryMetrics.UsedRam = Math.Round(memoryMetrics.Used / 1024, 2) + "GB";
        memoryMetrics.TotalRam = Math.Round(memoryMetrics.Total / 1024, 2) + "GB";
        memoryMetrics.RamRate = Math.Ceiling(100 * memoryMetrics.Used / memoryMetrics.Total) + "%";
        var cpuRates = GetCPURates();
        if (cpuRates != null)
        {
            memoryMetrics.CpuRates = cpuRates.Select(u => Math.Ceiling(u.ParseToDouble()) + "%").ToList();
        }
        memoryMetrics.CpuRate = memoryMetrics.CpuRates[0];
        return memoryMetrics;
    }

    /// <summary>
    /// 获取正确的操作系统版本（Linux获取发行版本）
    /// </summary>
    /// <returns></returns>
    public static String GetOSInfo()
    {
        string operation = string.Empty;
        if (IsMacOS())
        {
            var output = ShellUtil.Bash("sw_vers | awk 'NR<=2{printf \"%s \", $NF}'");
            if (output != null)
            {
                operation = output.Replace("%", string.Empty);
            }
        }
        else if (IsUnix())
        {
            var output = ShellUtil.Bash("awk -F= '/^VERSION_ID/ {print $2}' /etc/os-release | tr -d '\"'");
            operation = output ?? string.Empty;
        }
        else
        {
            operation = RuntimeInformation.OSDescription;
        }
        return operation;
    }

    /// <summary>
    /// 磁盘信息
    /// </summary>
    /// <returns></returns>
    public static List<DiskInfo> GetDiskInfos()
    {
        var diskInfos = new List<DiskInfo>();
        if (IsMacOS())
        {
            var output = ShellUtil.Bash(@"df -m | awk '/^\/dev\/disk/ {print $1,$2,$3,$4,$5}'");
            var disks = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (disks.Length < 1) return diskInfos;
            foreach (var item in disks)
            {
                var disk = item.Split(' ', (char)StringSplitOptions.RemoveEmptyEntries);
                if (disk.Length < 5) continue;

                var diskInfo = new DiskInfo()
                {
                    DiskName = disk[0],
                    TypeName = ShellUtil.Bash("diskutil info " + disk[0] + " | awk '/File System Personality/ {print $4}'").Replace("\n", string.Empty),
                    TotalSize = Math.Round(long.Parse(disk[1]) / 1024.0m, 2, MidpointRounding.AwayFromZero),
                    Used = Math.Round(long.Parse(disk[2]) / 1024.0m, 2, MidpointRounding.AwayFromZero),
                    AvailableFreeSpace = Math.Round(long.Parse(disk[3]) / 1024.0m, 2, MidpointRounding.AwayFromZero),
                    AvailablePercent = decimal.Parse(disk[4].Replace("%", ""))
                };
                diskInfos.Add(diskInfo);
            }
        }
        else if (IsUnix())
        {
            var output = ShellUtil.Bash(@"df -mT | awk '/^\/dev\/(sd|vd|xvd|nvme|sda|vda|mapper)/ {print $1,$2,$3,$4,$5,$6}'");
            var disks = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (disks.Length < 1) return diskInfos;

            //var rootDisk = disks[1].Split(' ', (char)StringSplitOptions.RemoveEmptyEntries);
            //if (rootDisk == null || rootDisk.Length < 1)
            //    return diskInfos;

            foreach (var item in disks)
            {
                var disk = item.Split(' ', (char)StringSplitOptions.RemoveEmptyEntries);
                if (disk.Length < 6) continue;

                var diskInfo = new DiskInfo()
                {
                    DiskName = disk[0],
                    TypeName = disk[1],
                    TotalSize = Math.Round(long.Parse(disk[2]) / 1024.0m, 2, MidpointRounding.AwayFromZero),
                    Used = Math.Round(long.Parse(disk[3]) / 1024.0m, 2, MidpointRounding.AwayFromZero),
                    AvailableFreeSpace = Math.Round(long.Parse(disk[4]) / 1024.0m, 2, MidpointRounding.AwayFromZero),
                    AvailablePercent = decimal.Parse(disk[5].Replace("%", ""))
                };
                diskInfos.Add(diskInfo);
            }
        }
        else
        {
            var driveList = DriveInfo.GetDrives().Where(u => u.IsReady);
            foreach (var item in driveList)
            {
                if (item.DriveType == DriveType.CDRom) continue;
                var diskInfo = new DiskInfo()
                {
                    DiskName = item.Name,
                    TypeName = item.DriveType.ToString(),
                    TotalSize = Math.Round(item.TotalSize / 1024 / 1024 / 1024.0m, 2, MidpointRounding.AwayFromZero),
                    AvailableFreeSpace = Math.Round(item.AvailableFreeSpace / 1024 / 1024 / 1024.0m, 2, MidpointRounding.AwayFromZero),
                };
                diskInfo.Used = diskInfo.TotalSize - diskInfo.AvailableFreeSpace;
                diskInfo.AvailablePercent = decimal.Ceiling(diskInfo.Used / (decimal)diskInfo.TotalSize * 100);
                diskInfos.Add(diskInfo);
            }
        }
        return diskInfos;
    }

    /// <summary>
    /// 获取外网IP地址
    /// </summary>
    /// <returns></returns>
    public static string GetIpFromOnline()
    {
        try
        {
            var url = "https://4.ipw.cn";
            var httpRemoteService = App.GetRequiredService<IHttpRemoteService>();
            var ip = httpRemoteService.GetAsString(url);
            if (string.IsNullOrWhiteSpace(ip)) return "unknow";
            var (ipLocation, _, _) = CommonUtil.GetIpAddress(ip);
            return ip + " " + ipLocation;
        }
        catch
        {
            return "unknow";
        }
    }

    public static bool IsUnix()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }

    public static bool IsMacOS()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    }

    public static List<string> GetCPURates()
    {
        var cpuRates = new List<string>();
        string output = "";
        if (IsMacOS())
        {
            output = ShellUtil.Bash("top -l 1 | grep \"CPU usage\" | awk '{print $3 + $5}'");
            cpuRates.Add(output.Trim());
        }
        else if (IsUnix())
        {
            output = ShellUtil.Bash("awk '{u=$2+$4; t=$2+$4+$5; if (NR==1){u1=u; t1=t;} else print ($2+$4-u1) * 100 / (t-t1); }' <(grep 'cpu ' /proc/stat) <(sleep 1;grep 'cpu ' /proc/stat)");
            cpuRates.Add(output.Trim());
        }
        else
        {
            try
            {
                output = ShellUtil.Cmd("wmic", "cpu get LoadPercentage");
            }
            catch (Exception)
            {
                output = ShellUtil.PowerShell("Get-CimInstance -ClassName Win32_Processor | Select-Object LoadPercentage");
                output = output.Replace("@", string.Empty).Replace("{", string.Empty).Replace("}", string.Empty).Replace("=", string.Empty).Trim();
            }
            cpuRates.AddRange(output.Replace("LoadPercentage", string.Empty).Trim().Split("\r\r\n"));
        }
        return cpuRates;
    }

    /// <summary>
    /// 获取系统运行时间
    /// </summary>
    /// <returns></returns>
    public static string GetRunTime()
    {
        string runTime = string.Empty;
        string output = "";
        if (IsMacOS())
        {
            // macOS 获取系统启动时间：
            // sysctl -n kern.boottime | awk '{print $4}' | tr -d ','
            // 返回：1705379131
            // 使用date格式化即可
            output = ShellUtil.Bash("date -r $(sysctl -n kern.boottime | awk '{print $4}' | tr -d ',') +\"%Y-%m-%d %H:%M:%S\"").Trim();
            runTime = DateTimeUtil.FormatTime((DateTime.Now - output.ParseToDateTime()).TotalMilliseconds.ToString().Split('.')[0].ParseToLong());
        }
        else if (IsUnix())
        {
            output = ShellUtil.Bash("date -d \"$(awk -F. '{print $1}' /proc/uptime) second ago\" +\"%Y-%m-%d %H:%M:%S\"").Trim();
            runTime = DateTimeUtil.FormatTime((DateTime.Now - output.ParseToDateTime()).TotalMilliseconds.ToString().Split('.')[0].ParseToLong());
        }
        else
        {
            try
            {
                output = ShellUtil.Cmd("wmic", "OS get LastBootUpTime/Value");
                string[] outputArr = output.Split('=', (char)StringSplitOptions.RemoveEmptyEntries);
                if (outputArr.Length == 2)
                    runTime = DateTimeUtil.FormatTime((DateTime.Now - outputArr[1].Split('.')[0].ParseToDateTime()).TotalMilliseconds.ToString().Split('.')[0].ParseToLong());
            }
            catch (Exception)
            {
                output = ShellUtil.PowerShell("Get-CimInstance -ClassName Win32_OperatingSystem | Select-Object LastBootUpTime");
                output = output.Replace("LastBootUpTime", string.Empty).Replace("@", string.Empty).Replace("{", string.Empty).Replace("}", string.Empty).Replace("=", string.Empty).Trim();
                runTime = DateTimeUtil.FormatTime((DateTime.Now - output.ParseToDateTime()).TotalMilliseconds.ToString().Split('.')[0].ParseToLong());
            }
        }
        return runTime;
    }
}

/// <summary>
/// 内存信息
/// </summary>
public class MemoryMetrics
{
    [System.Text.Json.Serialization.JsonIgnore]
    public double Total { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public double Used { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public double Free { get; set; }

    /// <summary>
    /// 已用内存
    /// </summary>
    public string UsedRam { get; set; } = string.Empty;
    /// <summary>
    /// CPU使用率%
    /// </summary>
    public List<string> CpuRates { get; set; } = [];
    public string CpuRate { get; set; } = string.Empty;
    /// <summary>
    /// 总内存 GB
    /// </summary>
    public string TotalRam { get; set; } = string.Empty;
    /// <summary>
    /// 内存使用率 %
    /// </summary>
    public string RamRate { get; set; } = string.Empty;
    /// <summary>
    /// 空闲内存
    /// </summary>
    public string FreeRam { get; set; } = string.Empty;
}

/// <summary>
/// 磁盘信息
/// </summary>
public class DiskInfo
{
    /// <summary>
    /// 磁盘名
    /// </summary>
    public string DiskName { get; set; } = string.Empty;
    /// <summary>
    /// 类型名
    /// </summary>
    public string TypeName { get; set; } = string.Empty;
    /// <summary>
    /// 总剩余
    /// </summary>
    public decimal TotalFree { get; set; }

    /// <summary>
    /// 总量
    /// </summary>
    public decimal TotalSize { get; set; }

    /// <summary>
    /// 已使用
    /// </summary>
    public decimal Used { get; set; }

    /// <summary>
    /// 可使用
    /// </summary>
    public decimal AvailableFreeSpace { get; set; }

    /// <summary>
    /// 使用百分比
    /// </summary>
    public decimal AvailablePercent { get; set; }
}

public class MemoryMetricsClient
{
    /// <summary>
    /// windows系统获取内存信息
    /// </summary>
    /// <returns></returns>
    public static MemoryMetrics GetWindowsMetrics()
    {
        string output = "";
        var metrics = new MemoryMetrics();
        try
        {
            output = ShellUtil.Cmd("wmic", "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value");
            var lines = output.Trim().Split('\n', (char)StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length <= 1) return metrics;

            var freeMemoryParts = lines[0].Split('=', (char)StringSplitOptions.RemoveEmptyEntries);
            var totalMemoryParts = lines[1].Split('=', (char)StringSplitOptions.RemoveEmptyEntries);
            metrics.Total = Math.Round(double.Parse(totalMemoryParts[1]) / 1024, 0);
            metrics.Free = Math.Round(double.Parse(freeMemoryParts[1]) / 1024, 0);//m
        }
        catch (Exception)
        {
            output = ShellUtil.PowerShell("Get-CimInstance -ClassName Win32_OperatingSystem | Select-Object FreePhysicalMemory, TotalVisibleMemorySize");
            output = output.Replace("@", string.Empty).Replace("{", string.Empty).Replace("}", string.Empty).Trim();
            var lines = output.Trim().Split(';', (char)StringSplitOptions.RemoveEmptyEntries);

            // 跳过表头与分隔线（通常为前两行）
            if (lines.Length >= 2)
            {
                // 解析并转换为MB（原单位为KB）
                metrics.Free = Math.Round(double.Parse(lines[0].Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1]) / 1024, 0);
                metrics.Total = Math.Round(double.Parse(lines[1].Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1]) / 1024, 0);
            }
        }
        metrics.Used = metrics.Total - metrics.Free;

        return metrics;
    }

    /// <summary>
    /// Unix系统获取
    /// </summary>
    /// <returns></returns>
    public static MemoryMetrics GetUnixMetrics()
    {
        string output = ShellUtil.Bash("awk '/MemTotal/ {total=$2} /MemAvailable/ {available=$2} END {print total,available}' /proc/meminfo");
        var metrics = new MemoryMetrics();
        var memory = output.Split(' ', (char)StringSplitOptions.RemoveEmptyEntries);
        if (memory.Length != 2) return metrics;

        metrics.Total = double.Parse(memory[0]) / 1024;
        metrics.Free = double.Parse(memory[1]) / 1024;
        metrics.Used = metrics.Total - metrics.Free;
        return metrics;
    }

    /// <summary>
    /// macOS系统获取
    /// </summary>
    /// <returns></returns>
    public static MemoryMetrics GetMacOSMetrics()
    {
        var metrics = new MemoryMetrics();
        //物理内存大小
        var total = ShellUtil.Bash("sysctl -n hw.memsize | awk '{printf \"%.2f\", $1/1024/1024}'");
        metrics.Total = float.Parse(total.Replace("%", string.Empty));
        //TODO:占用内存，检查效率
        var free = ShellUtil.Bash("top -l 1 -s 0 | awk '/PhysMem/ {print $6+$8}'");
        metrics.Free = float.Parse(free);
        metrics.Used = metrics.Total - metrics.Free;
        return metrics;
    }
}

public class ShellUtil
{
    /// <summary>
    /// linux 系统命令
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public static string Bash(string command)
    {
        var escapedArgs = command.Replace("\"", "\\\"");
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{escapedArgs}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
        process.Start();
        string result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        process.Dispose();
        return result;
    }

    /// <summary>
    /// windows CMD 系统命令
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string Cmd(string fileName, string args)
    {
        var info = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = args,
            RedirectStandardOutput = true
        };

        var output = string.Empty;
        using var process = Process.Start(info) ?? throw new InvalidOperationException($"启动进程失败：{fileName}");
        output = process.StandardOutput.ReadToEnd();
        return output;
    }

    /// <summary>
    /// Windows POWERSHELL 系统命令
    /// </summary>
    /// <param name="script"></param>
    /// <returns></returns>
    public static string PowerShell(string script)
    {
        using var PowerShellInstance = System.Management.Automation.PowerShell.Create();
        PowerShellInstance.AddScript(script);
        var PSOutput = PowerShellInstance.Invoke();

        var output = new StringBuilder();
        foreach (var outputItem in PSOutput)
        {
            output.AppendLine(outputItem.ToString());
        }
        return output.ToString();
    }
}

public class ShellHelper
{
    /// <summary>
    /// Linux 系统命令
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public static string Bash(string command)
    {
        var escapedArgs = command.Replace("\"", "\\\"");
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{escapedArgs}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
        process.Start();
        string result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        process.Dispose();
        return result;
    }

    /// <summary>
    /// Windows CMD 系统命令
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string Cmd(string fileName, string args)
    {
        var info = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = args,
            RedirectStandardOutput = true
        };

        var output = string.Empty;
        using var process = Process.Start(info) ?? throw new InvalidOperationException($"启动进程失败：{fileName}");
        output = process.StandardOutput.ReadToEnd();
        return output;
    }

    /// <summary>
    /// Windows POWERSHELL 系统命令
    /// </summary>
    /// <param name="script"></param>
    /// <returns></returns>
    public static string PowerShell(string script)
    {
        using var PowerShellInstance = System.Management.Automation.PowerShell.Create();
        PowerShellInstance.AddScript(script);
        var PSOutput = PowerShellInstance.Invoke();

        var output = new StringBuilder();
        foreach (var outputItem in PSOutput)
        {
            output.AppendLine(outputItem.BaseObject.ToString());
        }
        return output.ToString();
    }
}
