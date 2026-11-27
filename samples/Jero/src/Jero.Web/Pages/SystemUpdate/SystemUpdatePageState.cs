using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.SystemUpdate;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;

namespace Jero.Web.Pages.SystemUpdate;

[ECMAScriptModule("components/system-update-page-state.js")]
public static class SystemUpdatePageModule
{
    public static SystemUpdatePageState Create() => new();

    public sealed class SystemUpdatePageState : global::Jero.Web.Composition.IPageState<SystemUpdatePageProps>
    {
        private const int IdlePollingInterval = 1_500;
        private const int ActionPollingInterval = 500;

        private SystemBackupView[] _backups = [];
        private string[] _logs = [];
        private string _selectedFileName = string.Empty;
        private string _runningAction = string.Empty;
        private string _webHookKey = string.Empty;
        private string _lastLogRefreshTime = string.Empty;
        private bool _backupLoading;
        private bool _logLoading;
        private bool _clearingLogs;
        private bool _webHookLoading;
        private bool _webHookDialogVisible;
        private bool _pageActive;
        private int _pollTimer;
        private string? _backupError;
        private string? _logError;
        private string? _actionError;
        private string? _clearError;
        private string? _webHookError;

        private bool CanViewBackups => ApplicationState.HasPermission("sysUpdate:list");
        private bool CanViewLogs => ApplicationState.HasPermission("sysUpdate:logs");
        private bool CanUpdate => ApplicationState.HasPermission("sysUpdate:update");
        private bool CanRestore => ApplicationState.HasPermission("sysUpdate:restore");
        private bool CanClear => ApplicationState.HasPermission("sysUpdate:clear");
        private bool CanViewWebHookKey => ApplicationState.HasPermission("sysUpdate:webHookKey");
        private bool IsRunning => _runningAction.Length > 0;
        private bool HasPageAccess => CanViewBackups || CanViewLogs;
        private bool CanRunRestore => CanRestore && _selectedFileName.Length > 0 && !IsRunning;
        private string? Error => _actionError ?? _clearError ?? _webHookError ?? _backupError ?? _logError;
        private string LogOutput => _logs.Length == 0 ? string.Empty : string.Join("\n", _logs);

        public void Initialize()
        {
            _pageActive = true;

            // 卸载时会清理计时器，迟到的异步响应不能重新启动它。
            OnBeforeUnmount(ReleasePage);
            RefreshAll();
            StartPolling(IdlePollingInterval);
        }

        private void ReleasePage()
        {
            _pageActive = false;
            StopPolling();
        }

        private void RefreshAll()
        {
            LoadBackups();
            LoadLogs();
        }

        private void LoadBackups()
        {
            if (!CanViewBackups)
            {
                _backups = [];
                _selectedFileName = string.Empty;
                _backupError = null;
                return;
            }
            if (_backupLoading)
                return;

            _backupLoading = true;
            SystemUpdateClient.Backups().Then(outcome =>
            {
                _backupLoading = false;
                if (!outcome.Ok)
                {
                    _backupError = outcome.Error ?? "加载备份列表失败。";
                    return;
                }

                _backups = SystemUpdateClient.ReadBackups(outcome.Data);
                SyncSelectedBackup();
                _backupError = null;
            }, _ =>
            {
                _backupLoading = false;
                _backupError = "加载备份列表失败，请检查网络后重试。";
            });
        }

        private void LoadLogs()
        {
            if (!CanViewLogs)
            {
                _logs = [];
                _logError = null;
                return;
            }
            if (_logLoading)
                return;

            _logLoading = true;
            SystemUpdateClient.Logs().Then(outcome =>
            {
                _logLoading = false;
                if (!outcome.Ok)
                {
                    _logError = outcome.Error ?? "加载更新日志失败。";
                    return;
                }

                _logs = SystemUpdateClient.ReadLogs(outcome.Data);
                _lastLogRefreshTime = new Date().ToLocaleString();
                _logError = null;
            }, _ =>
            {
                _logLoading = false;
                _logError = "加载更新日志失败，请检查网络后重试。";
            });
        }

        private void SelectBackup(string fileName)
        {
            if (!IsRunning)
                _selectedFileName = fileName;
        }

        private void SetWebHookDialogVisible(bool visible) => _webHookDialogVisible = visible;

        private void RunUpdate()
        {
            if (!CanUpdate || IsRunning)
                return;

            BeginLongAction("update");
            SystemUpdateClient.Update().Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    FinishLongAction(outcome.Error ?? "系统更新失败。", null);
                    return;
                }

                FinishLongAction(null, "系统更新已完成");
            }, _ => FinishLongAction("系统更新失败，请检查网络后重试。", null));
        }

        private void RunRestore()
        {
            var fileName = _selectedFileName.Trim();
            if (!CanRestore || IsRunning || fileName.Length == 0)
                return;

            BeginLongAction("restore");
            SystemUpdateClient.Restore(fileName).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    FinishLongAction(outcome.Error ?? "系统还原失败。", null);
                    return;
                }

                FinishLongAction(null, "系统还原已完成");
            }, _ => FinishLongAction("系统还原失败，请检查网络后重试。", null));
        }

        private void BeginLongAction(string action)
        {
            _runningAction = action;
            _actionError = null;
            StartPolling(ActionPollingInterval);
        }

        private void FinishLongAction(string? error, string? successMessage)
        {
            _runningAction = string.Empty;
            _actionError = error;

            // Promise 回调可能在导航完成后才执行。页面卸载后，
            // 回调不得重启轮询或更新界面状态。
            if (!_pageActive)
                return;

            if (successMessage is not null)
                ElementPlus.ElMessageMembers.Success(successMessage);
            LoadBackups();
            LoadLogs();
            StartPolling(IdlePollingInterval);
        }

        private void ClearLogs()
        {
            if (!CanClear || IsRunning || _clearingLogs)
                return;

            _clearingLogs = true;
            _clearError = null;
            SystemUpdateClient.ClearLogs().Then(outcome =>
            {
                _clearingLogs = false;
                if (!outcome.Ok)
                {
                    _clearError = outcome.Error ?? "清空更新日志失败。";
                    return;
                }

                _logs = [];
                _clearError = null;
                ElementPlus.ElMessageMembers.Success("更新日志已清空");
            }, _ =>
            {
                _clearingLogs = false;
                _clearError = "清空更新日志失败，请检查网络后重试。";
            });
        }

        private void LoadWebHookKey()
        {
            if (!CanViewWebHookKey || IsRunning || _webHookLoading)
                return;

            _webHookLoading = true;
            _webHookError = null;
            SystemUpdateClient.WebHookKey().Then(outcome =>
            {
                _webHookLoading = false;
                if (!outcome.Ok)
                {
                    _webHookError = outcome.Error ?? "获取 WebHook 密钥失败。";
                    return;
                }

                _webHookKey = SystemUpdateClient.ReadText(outcome.Data);
                _webHookDialogVisible = true;
                _webHookError = null;
            }, _ =>
            {
                _webHookLoading = false;
                _webHookError = "获取 WebHook 密钥失败，请检查网络后重试。";
            });
        }

        private void SyncSelectedBackup()
        {
            if (_selectedFileName.Length > 0)
            {
                for (var index = 0; index < _backups.Length; index++)
                    if (_backups[index].FileName == _selectedFileName)
                        return;
            }

            _selectedFileName = _backups.Length == 0 ? string.Empty : _backups[0].FileName;
        }

        private void StartPolling(int interval)
        {
            StopPolling();
            if (!_pageActive || !CanViewLogs)
                return;

            _pollTimer = Window.SetInterval(
                (TimerHandler)(Delegate)(Action)LoadLogs,
                interval,
                []);
        }

        private void StopPolling()
        {
            if (_pollTimer == 0)
                return;
            Window.ClearInterval(_pollTimer);
            _pollTimer = 0;
        }

        public SystemUpdatePageProps CreateProps() => new()
        {
            Backups = _backups,
            SelectedFileName = _selectedFileName,
            RunningAction = _runningAction,
            WebHookKey = _webHookKey,
            LastLogRefreshTime = _lastLogRefreshTime,
            BackupLoading = _backupLoading,
            LogLoading = _logLoading,
            ClearingLogs = _clearingLogs,
            WebHookLoading = _webHookLoading,
            WebHookDialogVisible = _webHookDialogVisible,
            CanViewBackups = CanViewBackups,
            CanViewLogs = CanViewLogs,
            CanUpdate = CanUpdate,
            CanRestore = CanRestore,
            CanClear = CanClear,
            CanViewWebHookKey = CanViewWebHookKey,
            IsRunning = IsRunning,
            HasPageAccess = HasPageAccess,
            CanRunRestore = CanRunRestore,
            Error = Error,
            LogOutput = LogOutput,
            RefreshAll = RefreshAll,
            LoadBackups = LoadBackups,
            LoadLogs = LoadLogs,
            SelectBackup = SelectBackup,
            RunUpdate = RunUpdate,
            RunRestore = RunRestore,
            ClearLogs = ClearLogs,
            LoadWebHookKey = LoadWebHookKey,
            SetWebHookDialogVisible = SetWebHookDialogVisible,
            HasLogs = _logs.Length > 0,
        };
    }
}

public sealed record SystemUpdatePageProps : VueProps
{
    public required SystemBackupView[] Backups { get; init; }
    public required string SelectedFileName { get; init; }
    public required string RunningAction { get; init; }
    public required string WebHookKey { get; init; }
    public required string LastLogRefreshTime { get; init; }
    public required bool BackupLoading { get; init; }
    public required bool LogLoading { get; init; }
    public required bool ClearingLogs { get; init; }
    public required bool WebHookLoading { get; init; }
    public required bool WebHookDialogVisible { get; init; }
    public required bool CanViewBackups { get; init; }
    public required bool CanViewLogs { get; init; }
    public required bool CanUpdate { get; init; }
    public required bool CanRestore { get; init; }
    public required bool CanClear { get; init; }
    public required bool CanViewWebHookKey { get; init; }
    public required bool IsRunning { get; init; }
    public required bool HasPageAccess { get; init; }
    public required bool CanRunRestore { get; init; }
    public required string? Error { get; init; }
    public required string LogOutput { get; init; }
    public required Action RefreshAll { get; init; }
    public required Action LoadBackups { get; init; }
    public required Action LoadLogs { get; init; }
    public required Action<string> SelectBackup { get; init; }
    public required Action RunUpdate { get; init; }
    public required Action RunRestore { get; init; }
    public required Action ClearLogs { get; init; }
    public required Action LoadWebHookKey { get; init; }
    public required Action<bool> SetWebHookDialogVisible { get; init; }
    public required bool HasLogs { get; init; }
}
