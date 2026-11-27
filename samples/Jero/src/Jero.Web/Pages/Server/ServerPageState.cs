using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.Server;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;

namespace Jero.Web.Pages.Server;

[ECMAScriptModule("components/server-page-state.js")]
public static class ServerPageModule
{
    public static ServerPageState Create() => new();

    public sealed class ServerPageState : global::Jero.Web.Composition.IPageState<ServerPageProps>
    {
        private const int PollingInterval = 10_000;

        private ServerBaseView _baseInfo = new();
        private ServerUsedView _usedInfo = new();
        private ServerDiskView[] _disks = [];
        private ServerAssemblyView[] _assemblies = [];
        private bool _baseLoading;
        private bool _usedLoading;
        private bool _diskLoading;
        private bool _assemblyLoading;
        private string _lastRefreshTime = string.Empty;
        private string? _baseError;
        private string? _usedError;
        private string? _diskError;
        private string? _assemblyError;
        private int _pollTimer;

        private bool IsRefreshing => _baseLoading || _usedLoading || _diskLoading || _assemblyLoading;
        private string? Error => _baseError ?? _usedError ?? _diskError ?? _assemblyError;
        private int MemoryPercentage => ServerClient.ParsePercent(_usedInfo.RamRate);
        private int CpuPercentage => ServerClient.ParsePercent(_usedInfo.CpuRate);

        public void Initialize()
        {
            // 卸载时会清理计时器，迟到的异步响应不能重新启动它。
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
            if (_baseLoading)
                return;

            _baseLoading = true;
            ServerClient.BaseInfo().Then(outcome =>
            {
                _baseLoading = false;
                if (!outcome.Ok)
                {
                    _baseError = outcome.Error ?? "加载系统信息失败。";
                    return;
                }

                _baseInfo = ServerClient.ReadBaseInfo(outcome.Data);
                _baseError = null;
            }, _ =>
            {
                _baseLoading = false;
                _baseError = "加载系统信息失败，请检查网络后重试。";
            });
        }

        private void LoadUsedInfo()
        {
            if (_usedLoading)
                return;

            _usedLoading = true;
            ServerClient.UsedInfo().Then(outcome =>
            {
                _usedLoading = false;
                if (!outcome.Ok)
                {
                    _usedError = outcome.Error ?? "加载资源使用信息失败。";
                    return;
                }

                _usedInfo = ServerClient.ReadUsedInfo(outcome.Data);
                _lastRefreshTime = new Date().ToLocaleString();
                _usedError = null;
            }, _ =>
            {
                _usedLoading = false;
                _usedError = "加载资源使用信息失败，请检查网络后重试。";
            });
        }

        private void LoadDisks()
        {
            if (_diskLoading)
                return;

            _diskLoading = true;
            ServerClient.Disks().Then(outcome =>
            {
                _diskLoading = false;
                if (!outcome.Ok)
                {
                    _diskError = outcome.Error ?? "加载磁盘信息失败。";
                    return;
                }

                _disks = ServerClient.ReadDisks(outcome.Data);
                _diskError = null;
            }, _ =>
            {
                _diskLoading = false;
                _diskError = "加载磁盘信息失败，请检查网络后重试。";
            });
        }

        private void LoadAssemblies()
        {
            if (_assemblyLoading)
                return;

            _assemblyLoading = true;
            ServerClient.Assemblies().Then(outcome =>
            {
                _assemblyLoading = false;
                if (!outcome.Ok)
                {
                    _assemblyError = outcome.Error ?? "加载程序集信息失败。";
                    return;
                }

                _assemblies = ServerClient.ReadAssemblies(outcome.Data);
                _assemblyError = null;
            }, _ =>
            {
                _assemblyLoading = false;
                _assemblyError = "加载程序集信息失败，请检查网络后重试。";
            });
        }

        private void StartPolling()
        {
            StopPolling();
            _pollTimer = Window.SetInterval(
                (TimerHandler)(Delegate)(Action)LoadUsedInfo,
                PollingInterval,
                []);
        }

        private void StopPolling()
        {
            if (_pollTimer == 0)
                return;
            Window.ClearInterval(_pollTimer);
            _pollTimer = 0;
        }

        public ServerPageProps CreateProps() => new()
        {
            BaseInfo = _baseInfo,
            UsedInfo = _usedInfo,
            Disks = _disks,
            Assemblies = _assemblies,
            BaseLoading = _baseLoading,
            UsedLoading = _usedLoading,
            DiskLoading = _diskLoading,
            AssemblyLoading = _assemblyLoading,
            LastRefreshTime = _lastRefreshTime,
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
