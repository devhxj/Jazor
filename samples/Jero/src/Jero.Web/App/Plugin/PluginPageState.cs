using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using static ECMAScript.Global;

namespace Jero.Web.App.Plugin;

[ECMAScriptModule("components/plugin-page-state.js")]
public static class PluginPageModule
{
    public static PluginPageState Create(PluginMonacoActions monaco) => new(monaco);

    public sealed class PluginPageState : global::Jero.Web.App.IPageState<PluginPageProps>
    {
        private const string EditorContainerId = "jero-plugin-monaco";

        private readonly PluginMonacoActions monaco;

        public PluginPageState(PluginMonacoActions monaco) => this.monaco = monaco;

        private PluginView[] items = [];
        private PluginTenantOption[] tenantItems = [];
        private int currentPage = 1;
        private int pageSize = 50;
        private int total;
        private string tenantId = string.Empty;
        private string name = string.Empty;
        private bool loading;
        private bool tenantLoading;
        private string? error;

        private bool editDialogVisible;
        private string editDialogTitle = "新增并安装动态插件";
        private bool saving;
        private PluginView? editing;
        private string editTenantId = string.Empty;
        private string editName = string.Empty;
        private string editAssemblyName = string.Empty;
        private int editOrderNo = 100;
        private int editStatus = 1;
        private string editRemark = string.Empty;
        private string editCsharpCode = string.Empty;
        private string editorTab = "basic";
        private bool editorDialogOpened;
        private bool editorLoading;
        private string? editorLoadError;
        private int monacoMountVersion;
        private string busyActionId = string.Empty;

        private bool auditDialogVisible;
        private string auditDialogTitle = "修改记录";
        private string auditCreateUser = "-";
        private string auditCreateTime = "-";
        private string auditUpdateUser = "-";
        private string auditUpdateTime = "-";

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool CanAdd => ApplicationState.HasPermission("sysPlugin:add");
        private bool CanEdit => ApplicationState.HasPermission("sysPlugin:update");
        private bool CanDelete => ApplicationState.HasPermission("sysPlugin:delete");
        private string DefaultTenantId => IsSuperAdmin
            ? ApplicationState.User?.CurrentTenantId ?? string.Empty
            : ApplicationState.User?.TenantId ?? string.Empty;

        public void Initialize()
        {
            // Release owned browser resources before Vue removes this page DOM.
            OnBeforeUnmount(Dispose);
            tenantId = DefaultTenantId;
            if (IsSuperAdmin)
                LoadTenants();
            LoadPage();
        }

        private void EditorDialogOpened()
        {
            editorDialogOpened = true;
            ScheduleMonacoReady();
        }

        private void ScheduleMonacoReady()
        {
            if (!editDialogVisible || !editorDialogOpened || editorTab != "code")
                return;

            var version = ++monacoMountVersion;
            // OnOpened waits for the dialog transition; NextTick then waits for the tab DOM.
            // 关闭/重开会更新 version，旧 tick 不能把 editor 挂到新弹窗或已卸载的页面。
            NextTick(() =>
            {
                if (version != monacoMountVersion || !editDialogVisible || !editorDialogOpened || editorTab != "code")
                    return;

                editorLoading = true;
                editorLoadError = null;
                monaco.Load().Then(api =>
                {
                    // The response owns no DOM. Check this dialog instance before creating an editor;
                    // 关闭、离页或重新进入后的旧完成不能挂到当前 host。
                    if (version != monacoMountVersion || !editDialogVisible || !editorDialogOpened || editorTab != "code")
                        return;
                    if (monaco.Create(api, EditorContainerId, editCsharpCode, value => editCsharpCode = value))
                        monaco.LayoutAndFocus();
                    editorLoading = false;
                }).Catch(loadError =>
                {
                    if (version != monacoMountVersion || !editDialogVisible || !editorDialogOpened || editorTab != "code")
                        return;
                    editorLoading = false;
                    editorLoadError = "代码编辑器加载失败：" + loadError.Message;
                });
            });
        }

        public void Dispose()
        {
            CleanupEditor();
        }

        private void LoadTenants()
        {
            tenantLoading = true;
            PluginClient.Tenants().Then(outcome =>
            {
                tenantLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载租户失败。";
                    tenantItems = [];
                    return;
                }
                tenantItems = PluginClient.ReadTenants(outcome.Data);
            }, _ =>
            {
                tenantLoading = false;
                error = "加载租户失败，请检查网络后重试。";
            });
        }

        private void LoadPage()
        {
            loading = true;
            PluginClient.Page(
                currentPage,
                pageSize,
                name.Trim(),
                IsSuperAdmin ? tenantId : string.Empty).Then(outcome =>
            {
                loading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载动态插件失败。";
                    items = [];
                    total = 0;
                    return;
                }
                items = PluginClient.ReadItems(outcome.Data);
                total = PluginClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                loading = false;
                error = "加载动态插件失败，请检查网络后重试。";
            });
        }

        private void Search()
        {
            currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            tenantId = DefaultTenantId;
            name = string.Empty;
            currentPage = 1;
            LoadPage();
        }

        private void TenantChanged(string? value)
        {
            tenantId = value ?? string.Empty;
            currentPage = 1;
            LoadPage();
        }

        private void ChangePage(double value)
        {
            currentPage = (int)value;
            LoadPage();
        }

        private void ChangePageSize(double value)
        {
            pageSize = (int)value;
            currentPage = 1;
            LoadPage();
        }

        private void OpenCreate()
        {
            var targetTenantId = IsSuperAdmin ? tenantId : DefaultTenantId;
            if (IsSuperAdmin && targetTenantId.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择租户后再新增动态插件");
                return;
            }

            CleanupEditor();
            editing = null;
            editDialogTitle = "新增并安装动态插件";
            editTenantId = targetTenantId;
            editName = string.Empty;
            editAssemblyName = string.Empty;
            editOrderNo = 100;
            editStatus = 1;
            editRemark = string.Empty;
            editCsharpCode = PluginShared.CreateDefaultCode(string.Empty);
            editorTab = "basic";
            error = null;
            editDialogVisible = true;
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit)
                return;

            CleanupEditor();
            editing = row;
            editDialogTitle = "编辑动态插件";
            editTenantId = row.tenantId;
            editName = row.name;
            editAssemblyName = row.assemblyName;
            editOrderNo = row.orderNo;
            editStatus = row.status == 2 ? 2 : 1;
            editRemark = row.remark;
            editCsharpCode = row.csharpCode;
            editorTab = "basic";
            error = null;
            editDialogVisible = true;
        }

        private void EditorTabChanged(string value)
        {
            editorTab = value == "code" ? "code" : "basic";
            if (editorTab == "code")
                ScheduleMonacoReady();
            else
            {
                ++monacoMountVersion;
                editorLoading = false;
            }
        }

        private void RegenerateExampleCode()
        {
            editCsharpCode = PluginShared.CreateDefaultCode(editName);
            EditorTabChanged("code");
            monaco.SetValue(editCsharpCode);
        }

        private void Save()
        {
            if (saving)
                return;

            editCsharpCode = monaco.GetValue(editCsharpCode);
            var normalizedName = editName.Trim();
            var normalizedCode = editCsharpCode.Trim();
            if (normalizedName.Length == 0)
            {
                error = "功能名称不能为空。";
                return;
            }
            if (normalizedName.Length > 64)
            {
                error = "功能名称不能超过64个字符。";
                return;
            }
            if (editAssemblyName.Trim().Length > 512)
            {
                error = "程序集名称不能超过512个字符。";
                return;
            }
            if (editRemark.Trim().Length > 128)
            {
                error = "备注不能超过128个字符。";
                return;
            }
            if (normalizedCode.Length == 0)
            {
                error = "C# 代码不能为空。";
                EditorTabChanged("code");
                return;
            }
            if (IsSuperAdmin && editTenantId.Length == 0)
            {
                error = "请选择租户。";
                return;
            }

            saving = true;
            var request = editing is null
                ? PluginClient.Add(new PluginAddInput
                {
                    tenantId = PluginShared.Optional(editTenantId),
                    name = normalizedName,
                    csharpCode = normalizedCode,
                    assemblyName = PluginShared.Optional(editAssemblyName),
                    orderNo = editOrderNo < 0 ? 0 : editOrderNo,
                    status = editStatus == 2 ? 2 : 1,
                    remark = PluginShared.Optional(editRemark)
                })
                : PluginClient.Update(new PluginUpdateInput
                {
                    id = editing.id,
                    tenantId = PluginShared.Optional(editTenantId),
                    name = normalizedName,
                    csharpCode = normalizedCode,
                    assemblyName = PluginShared.Optional(editAssemblyName),
                    orderNo = editOrderNo < 0 ? 0 : editOrderNo,
                    status = editStatus == 2 ? 2 : 1,
                    remark = PluginShared.Optional(editRemark),
                    createTime = editing.createTime,
                    createUserId = PluginShared.Optional(editing.createUserId),
                    createUserName = PluginShared.Optional(editing.createUserName),
                    updateTime = PluginShared.Optional(editing.updateTime),
                    updateUserId = PluginShared.Optional(editing.updateUserId),
                    updateUserName = PluginShared.Optional(editing.updateUserName)
                });

            var wasCreate = editing is null;
            request.Then(outcome =>
            {
                saving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存动态插件失败。";
                    return;
                }
                CloseEditor();
                error = null;
                ElementPlus.ElMessageMembers.Success(wasCreate
                    ? "动态插件已新增、编译并加载"
                    : "动态插件已更新、重新编译并加载");
                LoadPage();
            }, _ =>
            {
                saving = false;
                error = "保存动态插件失败，请检查网络后重试。";
            });
        }

        private void CompileAndLoad(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit || busyActionId.Length > 0)
                return;
            if (row.csharpCode.Trim().Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("当前插件没有可编译的 C# 代码");
                return;
            }

            busyActionId = row.id;
            PluginClient.CompileAssembly(row.csharpCode, row.assemblyName).Then(outcome =>
            {
                busyActionId = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "编译并加载动态插件失败。";
                    return;
                }
                var compiledName = PluginClient.ReadAssemblyName(outcome.Data);
                error = null;
                ElementPlus.ElMessageMembers.Success(compiledName.Length == 0
                    ? "动态程序集已编译并加载"
                    : "动态程序集已编译并加载：" + compiledName);
            }, _ =>
            {
                busyActionId = string.Empty;
                error = "编译并加载动态插件失败，请检查网络后重试。";
            });
        }

        private void RemoveAssembly(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit || busyActionId.Length > 0)
                return;
            if (row.assemblyName.Trim().Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("当前插件没有可卸载的程序集名称");
                return;
            }

            busyActionId = row.id;
            PluginClient.RemoveAssembly(row.assemblyName).Then(outcome =>
            {
                busyActionId = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "卸载动态程序集失败。";
                    return;
                }
                error = null;
                ElementPlus.ElMessageMembers.Success("动态程序集已从当前运行时卸载");
            }, _ =>
            {
                busyActionId = string.Empty;
                error = "卸载动态程序集失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete || busyActionId.Length > 0)
                return;

            busyActionId = row.id;
            PluginClient.Delete(row.id).Then(outcome =>
            {
                busyActionId = string.Empty;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除动态插件失败。";
                    return;
                }
                error = null;
                ElementPlus.ElMessageMembers.Success("动态插件及当前运行时程序集已删除");
                LoadPage();
            }, _ =>
            {
                busyActionId = string.Empty;
                error = "删除动态插件失败，请检查网络后重试。";
            });
        }

        private void OpenAudit(string id)
        {
            var row = Find(id);
            if (row is null)
                return;
            auditDialogTitle = "修改记录 · " + row.name;
            auditCreateUser = DisplayUser(row.createUserName, row.createUserId);
            auditCreateTime = DisplayValue(row.createTime);
            auditUpdateUser = DisplayUser(row.updateUserName, row.updateUserId);
            auditUpdateTime = DisplayValue(row.updateTime);
            auditDialogVisible = true;
        }

        private void CloseEditor()
        {
            editDialogVisible = false;
            CleanupEditor();
        }

        private void CleanupEditor()
        {
            editorDialogOpened = false;
            ++monacoMountVersion;
            editorLoading = false;
            editorLoadError = null;
            monaco.Dispose();
        }

        private PluginView? Find(string id)
        {
            for (var index = 0; index < items.Length; index++)
                if (items[index].id == id)
                    return items[index];
            return null;
        }

        private PluginTableRow[] TableRows()
        {
            var result = new PluginTableRow[items.Length];
            for (var index = 0; index < items.Length; index++)
            {
                var item = items[index];
                result[index] = new PluginTableRow(item.id, item.name, item.assemblyName, item.status, item.orderNo, item.remark);
            }
            return result;
        }

        private ConfigurationOption[] TenantOptions()
        {
            var result = new ConfigurationOption[tenantItems.Length];
            for (var index = 0; index < tenantItems.Length; index++)
                result[index] = new ConfigurationOption(tenantItems[index].value, tenantItems[index].label);
            return result;
        }

        private void SetEditDialogVisible(bool value)
        {
            if (!value)
                CloseEditor();
        }

        private static string DisplayUser(string name, string id)
        {
            if (name.Length > 0 && id.Length > 0)
                return name + " (" + id + ")";
            if (name.Length > 0)
                return name;
            return DisplayValue(id);
        }

        private static string DisplayValue(string value) => value.Length == 0 ? "-" : value;

        public PluginPageProps CreateProps() => new()
        {
            Error = error,
            Name = name,
            EditDialogTitle = editDialogTitle,
            EditTenantId = editTenantId,
            EditName = editName,
            EditAssemblyName = editAssemblyName,
            EditRemark = editRemark,
            EditorTab = editorTab,
            EditorLoadError = editorLoadError,
            AuditDialogTitle = auditDialogTitle,
            AuditCreateUser = auditCreateUser,
            AuditCreateTime = auditCreateTime,
            AuditUpdateUser = auditUpdateUser,
            AuditUpdateTime = auditUpdateTime,
            BusyActionId = busyActionId,
            TenantId = tenantId,
            IsSuperAdmin = IsSuperAdmin,
            CanAdd = CanAdd,
            CanEdit = CanEdit,
            CanDelete = CanDelete,
            TenantLoading = tenantLoading,
            Loading = loading,
            EditDialogVisible = editDialogVisible,
            EditorLoading = editorLoading,
            Saving = saving,
            AuditDialogVisible = auditDialogVisible,
            Editing = editing is not null,
            CurrentPage = currentPage,
            PageSize = pageSize,
            Total = total,
            EditOrderNo = editOrderNo,
            EditStatus = editStatus,
            TenantOptions = TenantOptions(),
            Rows = TableRows(),
            Search = Search,
            ResetSearch = ResetSearch,
            OpenCreate = OpenCreate,
            LoadPage = LoadPage,
            EditorDialogOpened = EditorDialogOpened,
            CleanupEditor = CleanupEditor,
            RegenerateExampleCode = RegenerateExampleCode,
            CloseEditor = CloseEditor,
            Save = Save,
            DismissError = () => error = null,
            OpenAudit = OpenAudit,
            OpenEdit = OpenEdit,
            CompileAndLoad = CompileAndLoad,
            RemoveAssembly = RemoveAssembly,
            Delete = Delete,
            TenantChanged = TenantChanged,
            EditorTabChanged = EditorTabChanged,
            SetName = value => name = value ?? "",
            SetEditName = value => editName = value ?? "",
            SetEditAssemblyName = value => editAssemblyName = value ?? "",
            SetEditRemark = value => editRemark = value ?? "",
            SetEditTenantId = value => editTenantId = value ?? "",
            SetEditOrderNo = value => editOrderNo = (int)(value ?? 100),
            SetEditStatus = value => editStatus = (int)value,
            ChangePage = ChangePage,
            ChangePageSize = ChangePageSize,
            SetEditDialogVisible = SetEditDialogVisible,
            SetAuditDialogVisible = value => auditDialogVisible = value,
        };
    }
}

public sealed record PluginPageProps : VueProps
{
    public required string? Error { get; init; }
    public required string Name { get; init; }
    public required string EditDialogTitle { get; init; }
    public required string EditTenantId { get; init; }
    public required string EditName { get; init; }
    public required string EditAssemblyName { get; init; }
    public required string EditRemark { get; init; }
    public required string EditorTab { get; init; }
    public required string? EditorLoadError { get; init; }
    public required string AuditDialogTitle { get; init; }
    public required string AuditCreateUser { get; init; }
    public required string AuditCreateTime { get; init; }
    public required string AuditUpdateUser { get; init; }
    public required string AuditUpdateTime { get; init; }
    public required string BusyActionId { get; init; }
    public required string TenantId { get; init; }
    public required bool IsSuperAdmin { get; init; }
    public required bool CanAdd { get; init; }
    public required bool CanEdit { get; init; }
    public required bool CanDelete { get; init; }
    public required bool TenantLoading { get; init; }
    public required bool Loading { get; init; }
    public required bool EditDialogVisible { get; init; }
    public required bool EditorLoading { get; init; }
    public required bool Saving { get; init; }
    public required bool AuditDialogVisible { get; init; }
    public required bool Editing { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public required int EditOrderNo { get; init; }
    public required int EditStatus { get; init; }
    public required ConfigurationOption[] TenantOptions { get; init; }
    public required PluginTableRow[] Rows { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action OpenCreate { get; init; }
    public required Action LoadPage { get; init; }
    public required Action EditorDialogOpened { get; init; }
    public required Action CleanupEditor { get; init; }
    public required Action RegenerateExampleCode { get; init; }
    public required Action CloseEditor { get; init; }
    public required Action Save { get; init; }
    public required Action DismissError { get; init; }
    public required Action<string> OpenAudit { get; init; }
    public required Action<string> OpenEdit { get; init; }
    public required Action<string> CompileAndLoad { get; init; }
    public required Action<string> RemoveAssembly { get; init; }
    public required Action<string> Delete { get; init; }
    public required Action<string?> TenantChanged { get; init; }
    public required Action<string> EditorTabChanged { get; init; }
    public required Action<string?> SetName { get; init; }
    public required Action<string?> SetEditName { get; init; }
    public required Action<string?> SetEditAssemblyName { get; init; }
    public required Action<string?> SetEditRemark { get; init; }
    public required Action<string?> SetEditTenantId { get; init; }
    public required Action<double?> SetEditOrderNo { get; init; }
    public required Action<double> SetEditStatus { get; init; }
    public required Action<double> ChangePage { get; init; }
    public required Action<double> ChangePageSize { get; init; }
    public required Action<bool> SetEditDialogVisible { get; init; }
    public required Action<bool> SetAuditDialogVisible { get; init; }
}
