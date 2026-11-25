using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using static ECMAScript.Global;

namespace Jero.Web.App.Server;

[ECMAScriptModule("components/server-page-state.js")]
public static class ServerPageModule
{
    public static ServerPageState Create() => new();

    public sealed class ServerPageState : global::Jero.Web.App.IPageState<ServerPageProps>
    {
        private const int PollingInterval = 10_000;

        private ServerBaseView baseInfo = new();
        private ServerUsedView usedInfo = new();
        private ServerDiskView[] disks = [];
        private ServerAssemblyView[] assemblies = [];
        private bool baseLoading;
        private bool usedLoading;
        private bool diskLoading;
        private bool assemblyLoading;
        private string lastRefreshTime = string.Empty;
        private string? baseError;
        private string? usedError;
        private string? diskError;
        private string? assemblyError;
        private int pollTimer;

        private bool IsRefreshing => baseLoading || usedLoading || diskLoading || assemblyLoading;
        private string? Error => baseError ?? usedError ?? diskError ?? assemblyError;
        private int MemoryPercentage => ServerClient.ParsePercent(usedInfo.ramRate);
        private int CpuPercentage => ServerClient.ParsePercent(usedInfo.cpuRate);

        public void Initialize()
        {
            // Unmount owns cleanup; late async results must not restart a page timer.
            OnBeforeUnmount(ReleasePage);
            StartPolling();
            RefreshAll();
        }

        private void ReleasePage()
        {
            StopPolling();
        }

        private void RefreshAll()
        {
            LoadBaseInfo();
            LoadUsedInfo();
            LoadDisks();
            LoadAssemblies();
        }

        private void LoadBaseInfo()
        {
            if (baseLoading)
                return;

            baseLoading = true;
            ServerClient.BaseInfo().Then(outcome =>
            {
                baseLoading = false;
                if (!outcome.Ok)
                {
                    baseError = outcome.Error ?? "加载系统信息失败。";
                    return;
                }

                baseInfo = ServerClient.ReadBaseInfo(outcome.Data);
                baseError = null;
            }, _ =>
            {
                baseLoading = false;
                baseError = "加载系统信息失败，请检查网络后重试。";
            });
        }

        private void LoadUsedInfo()
        {
            if (usedLoading)
                return;

            usedLoading = true;
            ServerClient.UsedInfo().Then(outcome =>
            {
                usedLoading = false;
                if (!outcome.Ok)
                {
                    usedError = outcome.Error ?? "加载资源使用信息失败。";
                    return;
                }

                usedInfo = ServerClient.ReadUsedInfo(outcome.Data);
                lastRefreshTime = new Date().ToLocaleString();
                usedError = null;
            }, _ =>
            {
                usedLoading = false;
                usedError = "加载资源使用信息失败，请检查网络后重试。";
            });
        }

        private void LoadDisks()
        {
            if (diskLoading)
                return;

            diskLoading = true;
            ServerClient.Disks().Then(outcome =>
            {
                diskLoading = false;
                if (!outcome.Ok)
                {
                    diskError = outcome.Error ?? "加载磁盘信息失败。";
                    return;
                }

                disks = ServerClient.ReadDisks(outcome.Data);
                diskError = null;
            }, _ =>
            {
                diskLoading = false;
                diskError = "加载磁盘信息失败，请检查网络后重试。";
            });
        }

        private void LoadAssemblies()
        {
            if (assemblyLoading)
                return;

            assemblyLoading = true;
            ServerClient.Assemblies().Then(outcome =>
            {
                assemblyLoading = false;
                if (!outcome.Ok)
                {
                    assemblyError = outcome.Error ?? "加载程序集信息失败。";
                    return;
                }

                assemblies = ServerClient.ReadAssemblies(outcome.Data);
                assemblyError = null;
            }, _ =>
            {
                assemblyLoading = false;
                assemblyError = "加载程序集信息失败，请检查网络后重试。";
            });
        }

        private void StartPolling()
        {
            StopPolling();
            pollTimer = Window.SetInterval(
                (TimerHandler)(Delegate)(Action)LoadUsedInfo,
                PollingInterval,
                []);
        }

        private void StopPolling()
        {
            if (pollTimer == 0)
                return;
            Window.ClearInterval(pollTimer);
            pollTimer = 0;
        }

        public ServerPageProps CreateProps() => new()
        {
            BaseInfo = baseInfo,
            UsedInfo = usedInfo,
            Disks = disks,
            Assemblies = assemblies,
            BaseLoading = baseLoading,
            UsedLoading = usedLoading,
            DiskLoading = diskLoading,
            AssemblyLoading = assemblyLoading,
            LastRefreshTime = lastRefreshTime,
            IsRefreshing = IsRefreshing,
            Error = Error,
            MemoryPercentage = MemoryPercentage,
            CpuPercentage = CpuPercentage,
            RefreshAll = RefreshAll,
        };
    }
}

public sealed record ServerPageProps : VueProps
{
    public required ServerBaseView BaseInfo { get; init; }
    public required ServerUsedView UsedInfo { get; init; }
    public required ServerDiskView[] Disks { get; init; }
    public required ServerAssemblyView[] Assemblies { get; init; }
    public required bool BaseLoading { get; init; }
    public required bool UsedLoading { get; init; }
    public required bool DiskLoading { get; init; }
    public required bool AssemblyLoading { get; init; }
    public required string LastRefreshTime { get; init; }
    public required bool IsRefreshing { get; init; }
    public required string? Error { get; init; }
    public required int MemoryPercentage { get; init; }
    public required int CpuPercentage { get; init; }
    public required Action RefreshAll { get; init; }
}
