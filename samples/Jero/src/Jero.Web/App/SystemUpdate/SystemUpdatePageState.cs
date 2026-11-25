using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using static ECMAScript.Global;

namespace Jero.Web.App.SystemUpdate;

[ECMAScriptModule("components/system-update-page-state.js")]
public static class SystemUpdatePageModule
{
    public static SystemUpdatePageState Create() => new();

    public sealed class SystemUpdatePageState : global::Jero.Web.App.IPageState<SystemUpdatePageProps>
    {
        private const int IdlePollingInterval = 1_500;
        private const int ActionPollingInterval = 500;

        private SystemBackupView[] backups = [];
        private string[] logs = [];
        private string selectedFileName = string.Empty;
        private string runningAction = string.Empty;
        private string webHookKey = string.Empty;
        private string lastLogRefreshTime = string.Empty;
        private bool backupLoading;
        private bool logLoading;
        private bool clearingLogs;
        private bool webHookLoading;
        private bool webHookDialogVisible;
        private bool pageActive;
        private int pollTimer;
        private string? backupError;
        private string? logError;
        private string? actionError;
        private string? clearError;
        private string? webHookError;

        private bool CanViewBackups => ApplicationState.HasPermission("sysUpdate:list");
        private bool CanViewLogs => ApplicationState.HasPermission("sysUpdate:logs");
        private bool CanUpdate => ApplicationState.HasPermission("sysUpdate:update");
        private bool CanRestore => ApplicationState.HasPermission("sysUpdate:restore");
        private bool CanClear => ApplicationState.HasPermission("sysUpdate:clear");
        private bool CanViewWebHookKey => ApplicationState.HasPermission("sysUpdate:webHookKey");
        private bool IsRunning => runningAction.Length > 0;
        private bool HasPageAccess => CanViewBackups || CanViewLogs;
        private bool CanRunRestore => CanRestore && selectedFileName.Length > 0 && !IsRunning;
        private string? Error => actionError ?? clearError ?? webHookError ?? backupError ?? logError;
        private string LogOutput => logs.Length == 0 ? string.Empty : string.Join("\n", logs);

        public void Initialize()
        {
            pageActive = true;

            // Unmount owns cleanup; late async results must not restart a page timer.
            OnBeforeUnmount(ReleasePage);
            RefreshAll();
            StartPolling(IdlePollingInterval);
        }

        private void ReleasePage()
        {
            pageActive = false;
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
                backups = [];
                selectedFileName = string.Empty;
                backupError = null;
                return;
            }
            if (backupLoading)
                return;

            backupLoading = true;
            SystemUpdateClient.Backups().Then(outcome =>
            {
                backupLoading = false;
                if (!outcome.Ok)
                {
                    backupError = outcome.Error ?? "加载备份列表失败。";
                    return;
                }

                backups = SystemUpdateClient.ReadBackups(outcome.Data);
                SyncSelectedBackup();
                backupError = null;
            }, _ =>
            {
                backupLoading = false;
                backupError = "加载备份列表失败，请检查网络后重试。";
            });
        }

        private void LoadLogs()
        {
            if (!CanViewLogs)
            {
                logs = [];
                logError = null;
                return;
            }
            if (logLoading)
                return;

            logLoading = true;
            SystemUpdateClient.Logs().Then(outcome =>
            {
                logLoading = false;
                if (!outcome.Ok)
                {
                    logError = outcome.Error ?? "加载更新日志失败。";
                    return;
                }

                logs = SystemUpdateClient.ReadLogs(outcome.Data);
                lastLogRefreshTime = new Date().ToLocaleString();
                logError = null;
            }, _ =>
            {
                logLoading = false;
                logError = "加载更新日志失败，请检查网络后重试。";
            });
        }

        private void SelectBackup(string fileName)
        {
            if (!IsRunning)
                selectedFileName = fileName;
        }

        private void SetWebHookDialogVisible(bool visible) => webHookDialogVisible = visible;

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
            var fileName = selectedFileName.Trim();
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
            runningAction = action;
            actionError = null;
            StartPolling(ActionPollingInterval);
        }

        private void FinishLongAction(string? error, string? successMessage)
        {
            runningAction = string.Empty;
            actionError = error;

            // Promise callbacks may complete after navigation. Never restart polling or emit
            // user-facing state once the route host has released this page.
            if (!pageActive)
                return;

            if (successMessage is not null)
                ElementPlus.ElMessageMembers.Success(successMessage);
            LoadBackups();
            LoadLogs();
            StartPolling(IdlePollingInterval);
        }

        private void ClearLogs()
        {
            if (!CanClear || IsRunning || clearingLogs)
                return;

            clearingLogs = true;
            clearError = null;
            SystemUpdateClient.ClearLogs().Then(outcome =>
            {
                clearingLogs = false;
                if (!outcome.Ok)
                {
                    clearError = outcome.Error ?? "清空更新日志失败。";
                    return;
                }

                logs = [];
                clearError = null;
                ElementPlus.ElMessageMembers.Success("更新日志已清空");
            }, _ =>
            {
                clearingLogs = false;
                clearError = "清空更新日志失败，请检查网络后重试。";
            });
        }

        private void LoadWebHookKey()
        {
            if (!CanViewWebHookKey || IsRunning || webHookLoading)
                return;

            webHookLoading = true;
            webHookError = null;
            SystemUpdateClient.WebHookKey().Then(outcome =>
            {
                webHookLoading = false;
                if (!outcome.Ok)
                {
                    webHookError = outcome.Error ?? "获取 WebHook 密钥失败。";
                    return;
                }

                webHookKey = SystemUpdateClient.ReadText(outcome.Data);
                webHookDialogVisible = true;
                webHookError = null;
            }, _ =>
            {
                webHookLoading = false;
                webHookError = "获取 WebHook 密钥失败，请检查网络后重试。";
            });
        }

        private void SyncSelectedBackup()
        {
            if (selectedFileName.Length > 0)
            {
                for (var index = 0; index < backups.Length; index++)
                    if (backups[index].fileName == selectedFileName)
                        return;
            }

            selectedFileName = backups.Length == 0 ? string.Empty : backups[0].fileName;
        }

        private void StartPolling(int interval)
        {
            StopPolling();
            if (!pageActive || !CanViewLogs)
                return;

            pollTimer = Window.SetInterval(
                (TimerHandler)(Delegate)(Action)LoadLogs,
                interval,
                []);
        }

        private void StopPolling()
        {
            if (pollTimer == 0)
                return;
            Window.ClearInterval(pollTimer);
            pollTimer = 0;
        }

        public SystemUpdatePageProps CreateProps() => new()
        {
            Backups = backups,
            SelectedFileName = selectedFileName,
            RunningAction = runningAction,
            WebHookKey = webHookKey,
            LastLogRefreshTime = lastLogRefreshTime,
            BackupLoading = backupLoading,
            LogLoading = logLoading,
            ClearingLogs = clearingLogs,
            WebHookLoading = webHookLoading,
            WebHookDialogVisible = webHookDialogVisible,
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
            HasLogs = logs.Length > 0,
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
