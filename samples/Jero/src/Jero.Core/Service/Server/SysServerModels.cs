#if NET10_0_OR_GREATER

using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Jero.Core.Service;

/// <summary>
/// 服务器硬件与运行时信息采集器。
/// </summary>
public static class SysServerInfoProvider
{
    private static readonly TimeSpan HardwareInfoTimeout = TimeSpan.FromSeconds(3);

    /// <summary>
    /// 获取完整的系统硬件信息。
    /// </summary>
    public static SystemInfo GetSystemInfo()
    {
        var hardwareInfo = new Hardware.Info.HardwareInfo(HardwareInfoTimeout);

        return new SystemInfo
        {
            CpuInfo = GetCpuInfo(hardwareInfo),
            RamInfo = GetRamInfo(hardwareInfo),
            DiskInfos = GetDiskInfos(hardwareInfo),
            NetworkInfos = GetNetworkInfos(),
            GpuInfos = GetGpuInfos(hardwareInfo),
            BoardInfo = GetBoardInfo(hardwareInfo),
            RuntimeInfo = GetRuntimeInfo()
        };
    }

    /// <summary>
    /// 获取服务器运行时信息。
    /// </summary>
    public static RuntimeInfo GetRuntimeInfo()
    {
        try
        {
            using var currentProcess = Process.GetCurrentProcess();
            var now = DateTime.Now;

            return new RuntimeInfo
            {
                OsName = GetOperatingSystemName(),
                OsDescription = RuntimeInformation.OSDescription,
                OsVersion = Environment.OSVersion.Version.ToString(),
                OsArchitecture = RuntimeInformation.OSArchitecture.ToString(),
                ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
                FrameworkDescription = RuntimeInformation.FrameworkDescription,
                RuntimeVersion = Environment.Version.ToString(),
                Is64BitOperatingSystem = Environment.Is64BitOperatingSystem,
                Is64BitProcess = Environment.Is64BitProcess,
                IsInteractive = Environment.UserInteractive,
                ProcessorCount = Environment.ProcessorCount,
                SystemDirectory = Environment.SystemDirectory,
                CurrentDirectory = Environment.CurrentDirectory,
                MachineName = Environment.MachineName,
                UserName = Environment.UserName,
                UserDomainName = Environment.UserDomainName,
                WorkingSet = Environment.WorkingSet,
                SystemStartTime = now - TimeSpan.FromMilliseconds(Environment.TickCount64),
                SystemUptime = TimeSpan.FromMilliseconds(Environment.TickCount64),
                ProcessStartTime = currentProcess.StartTime,
                ProcessUptime = now - currentProcess.StartTime,
                ProcessId = currentProcess.Id,
                ProcessName = currentProcess.ProcessName,
                ClrVersion = Environment.Version.ToString(),
                EnvironmentVariableCount = Environment.GetEnvironmentVariables().Count,
                CommandLineArgs = Environment.GetCommandLineArgs()
            };
        }
        catch (Exception ex)
        {
            return new RuntimeInfo
            {
                OsName = "Unknown",
                OsDescription = $"Error collecting info: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// 获取指定前缀程序集引用的主要 NuGet 包。
    /// </summary>
    public static List<NuGetPackage> GetNuGetPackages(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return [];
        }

        var nugetPackages = new Dictionary<string, NuGetPackage>(StringComparer.OrdinalIgnoreCase);

        foreach (var assembly in GetEffectiveAssemblies(prefix))
        {
            AssemblyName[] referencedAssemblies;
            try
            {
                referencedAssemblies = assembly.GetReferencedAssemblies();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading assembly references {assembly.FullName}: {ex.Message}");
                continue;
            }

            foreach (var referencedAssembly in referencedAssemblies)
            {
                if (IsIgnoredReference(referencedAssembly, prefix) || referencedAssembly.Version is null)
                {
                    continue;
                }

                var packageName = referencedAssembly.Name!;
                var packageVersion = referencedAssembly.Version.ToString();

                if (!nugetPackages.TryGetValue(packageName, out var existingPackage) ||
                    IsVersionNewer(packageVersion, existingPackage.PackageVersion))
                {
                    nugetPackages[packageName] = new NuGetPackage
                    {
                        PackageName = packageName,
                        PackageVersion = packageVersion
                    };
                }
            }
        }

        return [.. nugetPackages.Values.OrderBy(package => package.PackageName, StringComparer.OrdinalIgnoreCase)];
    }

    private static CpuInfo GetCpuInfo(Hardware.Info.HardwareInfo hardwareInfo)
    {
        var cpuInfo = new CpuInfo
        {
            LogicalCoreCount = Environment.ProcessorCount,
            ProcessorArchitecture = RuntimeInformation.ProcessArchitecture.ToString()
        };

        if (!TryRefresh(() => hardwareInfo.RefreshCPUList(false, 0, false)))
        {
            return cpuInfo;
        }

        var cpu = hardwareInfo.CpuList.FirstOrDefault();
        if (cpu is null)
        {
            return cpuInfo;
        }

        cpuInfo.ProcessorName = FirstNotEmpty(cpu.Name, cpu.Description, Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"));
        cpuInfo.PhysicalCoreCount = ToInt32(cpu.NumberOfCores);
        cpuInfo.LogicalCoreCount = ToInt32(cpu.NumberOfLogicalProcessors, Environment.ProcessorCount);
        cpuInfo.BaseClockSpeed = ClockMhzToGhz(cpu.MaxClockSpeed > 0 ? cpu.MaxClockSpeed : cpu.CurrentClockSpeed);
        cpuInfo.CacheBytes = CacheKilobytesToBytes(cpu.L1DataCacheSize, cpu.L1InstructionCacheSize, cpu.L2CacheSize, cpu.L3CacheSize);
        cpuInfo.UsagePercentage = Math.Round((double)cpu.PercentProcessorTime, 2);

        return cpuInfo;
    }

    private static RamInfo GetRamInfo(Hardware.Info.HardwareInfo hardwareInfo)
    {
        var ramInfo = new RamInfo();

        if (TryRefresh(hardwareInfo.RefreshMemoryStatus))
        {
            ramInfo.TotalBytes = ToInt64(hardwareInfo.MemoryStatus.TotalPhysical);
            ramInfo.AvailableBytes = ToInt64(hardwareInfo.MemoryStatus.AvailablePhysical);
            ramInfo.FreeBytes = ramInfo.AvailableBytes;
            ramInfo.UsedBytes = Math.Max(0, ramInfo.TotalBytes - ramInfo.AvailableBytes);
        }

        if (ramInfo.TotalBytes <= 0 && TryRefresh(hardwareInfo.RefreshMemoryList))
        {
            ramInfo.TotalBytes = ToInt64(hardwareInfo.MemoryList.Aggregate(0UL, (total, memory) => total + memory.Capacity));
            ramInfo.AvailableBytes = ramInfo.FreeBytes;
            ramInfo.UsedBytes = Math.Max(0, ramInfo.TotalBytes - ramInfo.AvailableBytes);
        }

        if (ramInfo.TotalBytes > 0)
        {
            ramInfo.UsagePercentage = Math.Round((double)ramInfo.UsedBytes / ramInfo.TotalBytes * 100, 2);
            ramInfo.AvailablePercentage = Math.Round((double)ramInfo.AvailableBytes / ramInfo.TotalBytes * 100, 2);
        }

        return ramInfo;
    }

    private static List<DiskInfo> GetDiskInfos(Hardware.Info.HardwareInfo hardwareInfo)
    {
        if (TryRefresh(hardwareInfo.RefreshDriveList) && hardwareInfo.DriveList.Count > 0)
        {
            return [.. hardwareInfo.DriveList.Select(ToDiskInfo).Where(disk => !string.IsNullOrWhiteSpace(disk.DiskName))];
        }

        return [.. DriveInfo.GetDrives()
            .Where(drive => drive.IsReady && drive.DriveType != DriveType.CDRom)
            .Select(drive =>
            {
                var totalSpace = drive.TotalSize;
                var freeSpace = drive.AvailableFreeSpace;
                return new DiskInfo
                {
                    DiskName = drive.Name,
                    TypeName = drive.DriveType.ToString(),
                    TotalSpace = totalSpace,
                    FreeSpace = freeSpace,
                    UsedSpace = Math.Max(0, totalSpace - freeSpace),
                    AvailableRate = totalSpace > 0 ? Math.Round((double)freeSpace / totalSpace * 100, 2) : 0
                };
            })];
    }

    private static DiskInfo ToDiskInfo(Hardware.Info.Drive drive)
    {
        var volumeList = drive.PartitionList.SelectMany(partition => partition.VolumeList).ToList();
        var totalSpace = drive.Size > 0 ? ToInt64(drive.Size) : ToInt64(volumeList.Aggregate(0UL, (total, volume) => total + volume.Size));
        var freeSpace = ToInt64(volumeList.Aggregate(0UL, (total, volume) => total + volume.FreeSpace));

        return new DiskInfo
        {
            DiskName = FirstNotEmpty(drive.Name, drive.Caption, drive.Model),
            TypeName = FirstNotEmpty(drive.MediaType, drive.Description),
            TotalSpace = totalSpace,
            FreeSpace = freeSpace,
            UsedSpace = Math.Max(0, totalSpace - freeSpace),
            AvailableRate = totalSpace > 0 ? Math.Round((double)freeSpace / totalSpace * 100, 2) : 0
        };
    }

    private static List<NetworkInfo> GetNetworkInfos()
    {
        var networkInfos = new List<NetworkInfo>();

        try
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                var networkInfo = new NetworkInfo
                {
                    Name = networkInterface.Name,
                    Description = networkInterface.Description,
                    Type = networkInterface.NetworkInterfaceType.ToString(),
                    OperationalStatus = networkInterface.OperationalStatus.ToString(),
                    Speed = networkInterface.Speed > 0 ? $"{networkInterface.Speed:#,##0} bps" : "Unknown",
                    PhysicalAddress = BitConverter.ToString(networkInterface.GetPhysicalAddress().GetAddressBytes()),
                    SupportsMulticast = networkInterface.SupportsMulticast,
                    IsReceiveOnly = networkInterface.IsReceiveOnly
                };

                FillNetworkAddressInfo(networkInterface, networkInfo);

                if (networkInfo.DhcpServerAddresses.Count == 0 &&
                    networkInfo.DnsAddresses.Count == 0 &&
                    networkInfo.GatewayAddresses.Count == 0 &&
                    networkInfo.IPv4Addresses.Count == 0 &&
                    networkInfo.IPv6Addresses.Count == 0)
                {
                    continue;
                }

                networkInfos.Add(networkInfo);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error collecting network information: {ex.Message}");
            networkInfos.Add(new NetworkInfo
            {
                Name = "Error",
                Description = "Failed to retrieve network information"
            });
        }

        return networkInfos;
    }

    private static void FillNetworkAddressInfo(NetworkInterface networkInterface, NetworkInfo networkInfo)
    {
        try
        {
            var properties = networkInterface.GetIPProperties();
            networkInfo.DnsAddresses = [.. properties.DnsAddresses.Select(ip => ip.ToString())];
            networkInfo.GatewayAddresses = [.. properties.GatewayAddresses.Select(gateway => gateway.Address.ToString())];

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                networkInfo.DhcpServerAddresses = [.. properties.DhcpServerAddresses.Select(ip => ip.ToString())];
            }

            var unicastAddresses = properties.UnicastAddresses.ToList();
            networkInfo.IPv4Addresses = [.. unicastAddresses
                .Where(address => address.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(address => new IpAddressInfo
                {
                    Address = address.Address.ToString(),
                    SubnetMask = address.IPv4Mask?.ToString() ?? string.Empty,
                    PrefixLength = address.PrefixLength
                })];

            networkInfo.IPv6Addresses = [.. unicastAddresses
                .Where(address => address.Address.AddressFamily == AddressFamily.InterNetworkV6)
                .Select(address => new IpAddressInfo
                {
                    Address = address.Address.ToString(),
                    PrefixLength = address.PrefixLength
                })];

            if (networkInterface.OperationalStatus == OperationalStatus.Up)
            {
                var stats = networkInterface.GetIPv4Statistics();
                networkInfo.Statistics = new NetworkInterfaceStatistics
                {
                    BytesReceived = stats.BytesReceived,
                    BytesSent = stats.BytesSent,
                    PacketsReceived = stats.UnicastPacketsReceived + stats.NonUnicastPacketsReceived,
                    PacketsSent = stats.UnicastPacketsSent + stats.NonUnicastPacketsSent,
                    IncomingPacketsDiscarded = stats.IncomingPacketsDiscarded,
                    IncomingPacketsWithErrors = stats.IncomingPacketsWithErrors,
                    OutgoingPacketsWithErrors = stats.OutgoingPacketsWithErrors
                };

                if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    networkInfo.Statistics.OutgoingPacketsDiscarded = stats.OutgoingPacketsDiscarded;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error collecting network interface {networkInterface.Name}: {ex.Message}");
        }
    }

    private static List<GpuInfo> GetGpuInfos(Hardware.Info.HardwareInfo hardwareInfo)
    {
        if (!TryRefresh(() => hardwareInfo.RefreshVideoControllerList(false)))
        {
            return [];
        }

        return [.. hardwareInfo.VideoControllerList.Select(videoController => new GpuInfo
        {
            Name = videoController.Name,
            Description = videoController.Description,
            Vendor = videoController.Manufacturer,
            DeviceId = videoController.Caption,
            DriverVersion = videoController.DriverVersion,
            MemoryBytes = ToInt64(videoController.AdapterRAM),
            VideoModeDescription = videoController.VideoModeDescription,
            Status = string.Empty
        })];
    }

    private static BoardInfo GetBoardInfo(Hardware.Info.HardwareInfo hardwareInfo)
    {
        if (!TryRefresh(hardwareInfo.RefreshMotherboardList))
        {
            return new BoardInfo();
        }

        var motherboard = hardwareInfo.MotherboardList.FirstOrDefault();
        if (motherboard is null)
        {
            return new BoardInfo();
        }

        return new BoardInfo
        {
            Product = motherboard.Product,
            Manufacturer = motherboard.Manufacturer,
            SerialNumber = motherboard.SerialNumber,
            Version = string.Empty
        };
    }

    private static IEnumerable<Assembly> GetEffectiveAssemblies(string prefix)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic)
            .Where(assembly =>
            {
                try
                {
                    return assembly.ManifestModule.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) &&
                        assembly.ManifestModule.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    return false;
                }
            })
            .Distinct();
    }

    private static bool IsIgnoredReference(AssemblyName assemblyName, string prefix)
    {
        if (string.IsNullOrWhiteSpace(assemblyName.Name))
        {
            return true;
        }

        var fullName = assemblyName.FullName;
        return fullName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
            fullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) ||
            fullName.StartsWith("System", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsVersionNewer(string currentVersion, string existingVersion)
    {
        if (Version.TryParse(currentVersion, out var current) &&
            Version.TryParse(existingVersion, out var existing))
        {
            return current > existing;
        }

        return string.Compare(currentVersion, existingVersion, StringComparison.OrdinalIgnoreCase) > 0;
    }

    private static bool TryRefresh(Action refreshAction)
    {
        try
        {
            refreshAction();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Hardware information refresh failed: {ex.Message}");
            return false;
        }
    }

    private static string GetOperatingSystemName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return OSPlatform.Windows.ToString();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return OSPlatform.Linux.ToString();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return OSPlatform.OSX.ToString();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            return OSPlatform.FreeBSD.ToString();
        }

        return "Unknown";
    }

    private static int ToInt32(uint value, int defaultValue = 0)
    {
        return value > int.MaxValue ? defaultValue : (int)value;
    }

    private static long ToInt64(ulong value)
    {
        return value > long.MaxValue ? long.MaxValue : (long)value;
    }

    private static double ClockMhzToGhz(uint mhz)
    {
        return mhz > 0 ? Math.Round(mhz / 1000d, 2) : 0;
    }

    private static long CacheKilobytesToBytes(params uint[] cacheSizes)
    {
        var totalKilobytes = cacheSizes.Aggregate(0UL, (total, size) => total + size);
        return ToInt64(totalKilobytes * 1024);
    }

    private static string FirstNotEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
    }
}

/// <summary>
/// 系统运行信息。
/// </summary>
public record SystemInfo
{
    /// <summary>
    /// CPU信息。
    /// </summary>
    public CpuInfo CpuInfo { get; set; } = new();

    /// <summary>
    /// 内存信息。
    /// </summary>
    public RamInfo RamInfo { get; set; } = new();

    /// <summary>
    /// 磁盘信息列表。
    /// </summary>
    public List<DiskInfo> DiskInfos { get; set; } = [];

    /// <summary>
    /// 网络接口信息列表。
    /// </summary>
    public List<NetworkInfo> NetworkInfos { get; set; } = [];

    /// <summary>
    /// GPU信息列表。
    /// </summary>
    public List<GpuInfo> GpuInfos { get; set; } = [];

    /// <summary>
    /// 主板信息。
    /// </summary>
    public BoardInfo BoardInfo { get; set; } = new();

    /// <summary>
    /// 运行时信息。
    /// </summary>
    public RuntimeInfo RuntimeInfo { get; set; } = new();
}

/// <summary>
/// CPU信息。
/// </summary>
public record CpuInfo
{
    /// <summary>
    /// 处理器名称。
    /// </summary>
    public string ProcessorName { get; set; } = string.Empty;

    /// <summary>
    /// 处理器架构。
    /// </summary>
    public string ProcessorArchitecture { get; set; } = string.Empty;

    /// <summary>
    /// 物理核心数。
    /// </summary>
    public int PhysicalCoreCount { get; set; }

    /// <summary>
    /// 逻辑核心数(超线程)。
    /// </summary>
    public int LogicalCoreCount { get; set; }

    /// <summary>
    /// 基础时钟频率(GHz)。
    /// </summary>
    public double BaseClockSpeed { get; set; }

    /// <summary>
    /// 缓存大小。
    /// </summary>
    public long CacheBytes { get; set; }

    /// <summary>
    /// CPU使用率(%)。
    /// </summary>
    public double UsagePercentage { get; set; }
}

/// <summary>
/// 内存信息。
/// </summary>
public record RamInfo
{
    /// <summary>
    /// 总内存大小（字节）。
    /// </summary>
    public long TotalBytes { get; set; }

    /// <summary>
    /// 已用内存大小（字节）。
    /// </summary>
    public long UsedBytes { get; set; }

    /// <summary>
    /// 空闲内存大小（字节）。
    /// </summary>
    public long FreeBytes { get; set; }

    /// <summary>
    /// 可用内存大小（字节）。
    /// </summary>
    public long AvailableBytes { get; set; }

    /// <summary>
    /// 缓冲区和缓存大小（字节）。
    /// </summary>
    public long BuffersCachedBytes { get; set; }

    /// <summary>
    /// 内存使用率（%）。
    /// </summary>
    public double UsagePercentage { get; set; }

    /// <summary>
    /// 可用内存占比（%）。
    /// </summary>
    public double AvailablePercentage { get; set; }
}

/// <summary>
/// 磁盘信息。
/// </summary>
public record DiskInfo
{
    /// <summary>
    /// 磁盘名称。
    /// </summary>
    public string DiskName { get; set; } = string.Empty;

    /// <summary>
    /// 磁盘类型。
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// 总大小。
    /// </summary>
    public long TotalSpace { get; set; }

    /// <summary>
    /// 空闲大小。
    /// </summary>
    public long FreeSpace { get; set; }

    /// <summary>
    /// 已用大小。
    /// </summary>
    public long UsedSpace { get; set; }

    /// <summary>
    /// 可用占比。
    /// </summary>
    public double AvailableRate { get; set; }
}

/// <summary>
/// 网络接口信息。
/// </summary>
public record NetworkInfo
{
    /// <summary>
    /// 名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 描述。
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 类型。
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 操作状态。
    /// </summary>
    public string OperationalStatus { get; set; } = string.Empty;

    /// <summary>
    /// 速度。
    /// </summary>
    public string Speed { get; set; } = string.Empty;

    /// <summary>
    /// 物理地址(mac 地址)。
    /// </summary>
    public string PhysicalAddress { get; set; } = string.Empty;

    /// <summary>
    /// 是否支持多播。
    /// </summary>
    public bool SupportsMulticast { get; set; }

    /// <summary>
    /// 是否只接收。
    /// </summary>
    public bool IsReceiveOnly { get; set; }

    /// <summary>
    /// DNS 地址。
    /// </summary>
    public List<string> DnsAddresses { get; set; } = [];

    /// <summary>
    /// 网关地址。
    /// </summary>
    public List<string> GatewayAddresses { get; set; } = [];

    /// <summary>
    /// DHCP服务器地址。
    /// </summary>
    public List<string> DhcpServerAddresses { get; set; } = [];

    /// <summary>
    /// IPv4 地址详细信息。
    /// </summary>
    public List<IpAddressInfo> IPv4Addresses { get; set; } = [];

    /// <summary>
    /// IPv6 地址详细信息。
    /// </summary>
    public List<IpAddressInfo> IPv6Addresses { get; set; } = [];

    /// <summary>
    /// 网络接口统计信息。
    /// </summary>
    public NetworkInterfaceStatistics? Statistics { get; set; }
}

/// <summary>
/// IP地址信息。
/// </summary>
public record IpAddressInfo
{
    /// <summary>
    /// IP地址。
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// 子网掩码。
    /// </summary>
    public string SubnetMask { get; set; } = string.Empty;

    /// <summary>
    /// 前缀长度。
    /// </summary>
    public int PrefixLength { get; set; }
}

/// <summary>
/// 网络接口统计信息。
/// </summary>
public record NetworkInterfaceStatistics
{
    /// <summary>
    /// 接收字节数。
    /// </summary>
    public long BytesReceived { get; set; }

    /// <summary>
    /// 发送字节数。
    /// </summary>
    public long BytesSent { get; set; }

    /// <summary>
    /// 接收数据包数。
    /// </summary>
    public long PacketsReceived { get; set; }

    /// <summary>
    /// 发送数据包数。
    /// </summary>
    public long PacketsSent { get; set; }

    /// <summary>
    /// 丢弃的传入数据包数。
    /// </summary>
    public long IncomingPacketsDiscarded { get; set; }

    /// <summary>
    /// 丢弃的传出数据包数。
    /// </summary>
    public long OutgoingPacketsDiscarded { get; set; }

    /// <summary>
    /// 传入错误数据包数。
    /// </summary>
    public long IncomingPacketsWithErrors { get; set; }

    /// <summary>
    /// 传出错误数据包数。
    /// </summary>
    public long OutgoingPacketsWithErrors { get; set; }
}

/// <summary>
/// GPU信息。
/// </summary>
public record GpuInfo
{
    /// <summary>
    /// GPU名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 描述。
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 厂商。
    /// </summary>
    public string Vendor { get; set; } = string.Empty;

    /// <summary>
    /// 设备唯一标识。
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// 总线信息。
    /// </summary>
    public string BusInfo { get; set; } = string.Empty;

    /// <summary>
    /// 驱动版本。
    /// </summary>
    public string DriverVersion { get; set; } = string.Empty;

    /// <summary>
    /// 显存大小（字节）。
    /// </summary>
    public long MemoryBytes { get; set; }

    /// <summary>
    /// GPU温度（°C）。
    /// </summary>
    public double? Temperature { get; set; }

    /// <summary>
    /// 视频模式描述。
    /// </summary>
    public string VideoModeDescription { get; set; } = string.Empty;

    /// <summary>
    /// 状态。
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// GPU使用率（%）。
    /// </summary>
    public double? UtilizationPercentage { get; set; }

    /// <summary>
    /// 显存使用率（%）。
    /// </summary>
    public double? MemoryUtilizationPercentage { get; set; }
}

/// <summary>
/// 主板信息。
/// </summary>
public record BoardInfo
{
    /// <summary>
    /// 型号。
    /// </summary>
    public string Product { get; set; } = string.Empty;

    /// <summary>
    /// 制造商。
    /// </summary>
    public string Manufacturer { get; set; } = string.Empty;

    /// <summary>
    /// 序列号。
    /// </summary>
    public string SerialNumber { get; set; } = string.Empty;

    /// <summary>
    /// 版本号。
    /// </summary>
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// 系统运行时信息。
/// </summary>
public record RuntimeInfo
{
    /// <summary>
    /// 操作系统名称。
    /// </summary>
    public string OsName { get; set; } = string.Empty;

    /// <summary>
    /// 操作系统描述。
    /// </summary>
    public string OsDescription { get; set; } = string.Empty;

    /// <summary>
    /// 操作系统版本。
    /// </summary>
    public string OsVersion { get; set; } = string.Empty;

    /// <summary>
    /// 操作系统架构。
    /// </summary>
    public string OsArchitecture { get; set; } = string.Empty;

    /// <summary>
    /// 进程架构。
    /// </summary>
    public string ProcessArchitecture { get; set; } = string.Empty;

    /// <summary>
    /// 运行时框架描述。
    /// </summary>
    public string FrameworkDescription { get; set; } = string.Empty;

    /// <summary>
    /// 运行时版本。
    /// </summary>
    public string RuntimeVersion { get; set; } = string.Empty;

    /// <summary>
    /// 是否64位操作系统。
    /// </summary>
    public bool Is64BitOperatingSystem { get; set; }

    /// <summary>
    /// 是否64位进程。
    /// </summary>
    public bool Is64BitProcess { get; set; }

    /// <summary>
    /// 是否交互模式。
    /// </summary>
    public bool IsInteractive { get; set; }

    /// <summary>
    /// 交互模式描述。
    /// </summary>
    public string InteractiveMode => IsInteractive ? "交互运行" : "非交互运行";

    /// <summary>
    /// 处理器数量。
    /// </summary>
    public int ProcessorCount { get; set; }

    /// <summary>
    /// 系统目录。
    /// </summary>
    public string SystemDirectory { get; set; } = string.Empty;

    /// <summary>
    /// 当前目录。
    /// </summary>
    public string CurrentDirectory { get; set; } = string.Empty;

    /// <summary>
    /// 机器名称。
    /// </summary>
    public string MachineName { get; set; } = string.Empty;

    /// <summary>
    /// 用户名。
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 用户域名。
    /// </summary>
    public string UserDomainName { get; set; } = string.Empty;

    /// <summary>
    /// 工作集大小（字节）。
    /// </summary>
    public long WorkingSet { get; set; }

    /// <summary>
    /// 系统启动时间。
    /// </summary>
    public DateTime SystemStartTime { get; set; }

    /// <summary>
    /// 系统运行时间。
    /// </summary>
    public TimeSpan SystemUptime { get; set; }

    /// <summary>
    /// 进程启动时间。
    /// </summary>
    public DateTime ProcessStartTime { get; set; }

    /// <summary>
    /// 进程运行时间。
    /// </summary>
    public TimeSpan ProcessUptime { get; set; }

    /// <summary>
    /// 进程唯一标识。
    /// </summary>
    public int ProcessId { get; set; }

    /// <summary>
    /// 进程名称。
    /// </summary>
    public string ProcessName { get; set; } = string.Empty;

    /// <summary>
    /// CLR版本。
    /// </summary>
    public string ClrVersion { get; set; } = string.Empty;

    /// <summary>
    /// 环境变量数量。
    /// </summary>
    public int EnvironmentVariableCount { get; set; }

    /// <summary>
    /// 命令行参数。
    /// </summary>
    public string[] CommandLineArgs { get; set; } = [];
}

/// <summary>
/// NuGet 程序包信息记录。
/// </summary>
public record NuGetPackage
{
    /// <summary>
    /// NuGet 包名称。
    /// </summary>
    public string PackageName { get; init; } = string.Empty;

    /// <summary>
    /// NuGet 包版本号。
    /// </summary>
    public string PackageVersion { get; init; } = string.Empty;

    /// <summary>
    /// 获取包的完整标识符。
    /// </summary>
    public override string ToString()
    {
        return $"{PackageName}@{PackageVersion}";
    }
}

#endif // NET10_0_OR_GREATER
