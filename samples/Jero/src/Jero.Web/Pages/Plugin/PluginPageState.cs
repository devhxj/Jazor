using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.Plugin;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;
using static ECMAScript.Vue;

namespace Jero.Web.Pages.Plugin;

[ECMAScriptModule("components/plugin-page-state.js")]
public static class PluginPageModule
{
    public static PluginPageState Create() => new();

    public sealed class PluginPageState : global::Jero.Web.Composition.IPageState<PluginPageProps>
    {
        private const string EditorContainerId = "jero-plugin-monaco";

        private EditorSession? _editorSession;

        private PluginView[] _items = [];
        private PluginTenantOption[] _tenantItems = [];
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _total;
        private string _tenantId = string.Empty;
        private string _name = string.Empty;
        private bool _loading;
        private bool _tenantLoading;
        private string? _error;

        private bool _editDialogVisible;
        private string _editDialogTitle = "新增并安装动态插件";
        private bool _saving;
        private PluginView? _editing;
        private string _editTenantId = string.Empty;
        private string _editName = string.Empty;
        private string _editAssemblyName = string.Empty;
        private int _editOrderNo = 100;
        private int _editStatus = 1;
        private string _editRemark = string.Empty;
        private string _editCsharpCode = string.Empty;
        private string _editorTab = "basic";
        private bool _editorDialogOpened;
        private bool _editorLoading;
        private string? _editorLoadError;
        private int _monacoMountVersion;
        private string _busyActionId = string.Empty;

        private bool _auditDialogVisible;
        private string _auditDialogTitle = "修改记录";
        private string _auditCreateUser = "-";
        private string _auditCreateTime = "-";
        private string _auditUpdateUser = "-";
        private string _auditUpdateTime = "-";

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool CanAdd => ApplicationState.HasPermission("sysPlugin:add");
        private bool CanEdit => ApplicationState.HasPermission("sysPlugin:update");
        private bool CanDelete => ApplicationState.HasPermission("sysPlugin:delete");
        private string DefaultTenantId => IsSuperAdmin
            ? ApplicationState.User?.CurrentTenantId ?? string.Empty
            : ApplicationState.User?.TenantId ?? string.Empty;

        public void Initialize()
        {
            // Vue 移除页面 DOM 之前，先释放这个页面持有的浏览器资源。
            OnBeforeUnmount(Dispose);
            _tenantId = DefaultTenantId;
            if (IsSuperAdmin)
                LoadTenants();
            LoadPage();
        }

        private void EditorDialogOpened()
        {
            _editorDialogOpened = true;
            ScheduleMonacoReady();
        }

        private void ScheduleMonacoReady()
        {
            if (!_editDialogVisible || !_editorDialogOpened || _editorTab != "code")
                return;

            var version = ++_monacoMountVersion;
            // 弹窗入场动画结束后，还要等 Vue 把代码标签页的编辑器容器插入 DOM。
            NextTick(() =>
            {
                if (version != _monacoMountVersion || !_editDialogVisible || !_editorDialogOpened || _editorTab != "code")
                    return;

                _editorLoading = true;
                _editorLoadError = null;
                PluginMonacoLoader.Load().Then(api =>
                {
                    // 加载 Monaco 期间用户可能已关闭或重开弹窗。只有发起加载的那次打开可以挂载编辑器。
                    if (version != _monacoMountVersion || !_editDialogVisible || !_editorDialogOpened || _editorTab != "code")
                        return;
                    if (Document.GetElementById(EditorContainerId) is HTMLElement container)
                    {
                        // 保留会话中的原生句柄和回调，避免 Vue 将其包装为代理。
                        _editorSession ??= MarkRaw(api.Mount(container, _editCsharpCode, value => _editCsharpCode = value));
                        _editorSession.Focus();
                    }
                    _editorLoading = false;
                }).Catch(loadError =>
                {
                    if (version != _monacoMountVersion || !_editDialogVisible || !_editorDialogOpened || _editorTab != "code")
                        return;
                    _editorLoading = false;
                    _editorLoadError = "代码编辑器加载失败：" + loadError.Message;
                });
            });
        }

        public void Dispose()
        {
            CleanupEditor();
        }

        private void LoadTenants()
        {
            _tenantLoading = true;
            PluginClient.Tenants().Then(outcome =>
            {
                _tenantLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载租户失败。";
                    _tenantItems = [];
                    return;
                }
                _tenantItems = PluginClient.ReadTenants(outcome.Data);
            }, _ =>
            {
                _tenantLoading = false;
                _error = "加载租户失败，请检查网络后重试。";
            });
        }

        private void LoadPage()
        {
            _loading = true;
            PluginClient.Page(
                _currentPage,
                _pageSize,
                _name.Trim(),
                IsSuperAdmin ? _tenantId : string.Empty).Then(outcome =>
            {
                _loading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载动态插件失败。";
                    _items = [];
                    _total = 0;
                    return;
                }
                _items = PluginClient.ReadItems(outcome.Data);
                _total = PluginClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                _loading = false;
                _error = "加载动态插件失败，请检查网络后重试。";
            });
        }

        private void Search()
        {
            _currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            _tenantId = DefaultTenantId;
            _name = string.Empty;
            _currentPage = 1;
            LoadPage();
        }

        private void TenantChanged(string? value)
        {
            _tenantId = value ?? string.Empty;
            _currentPage = 1;
            LoadPage();
        }

        private void ChangePage(double value)
        {
            _currentPage = (int)value;
            LoadPage();
        }

        private void ChangePageSize(double value)
        {
            _pageSize = (int)value;
            _currentPage = 1;
            LoadPage();
        }

        private void OpenCreate()
        {
            var targetTenantId = IsSuperAdmin ? _tenantId : DefaultTenantId;
            if (IsSuperAdmin && targetTenantId.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择租户后再新增动态插件");
                return;
            }

            CleanupEditor();
            _editing = null;
            _editDialogTitle = "新增并安装动态插件";
            _editTenantId = targetTenantId;
            _editName = string.Empty;
            _editAssemblyName = string.Empty;
            _editOrderNo = 100;
            _editStatus = 1;
            _editRemark = string.Empty;
            _editCsharpCode = PluginShared.CreateDefaultCode(string.Empty);
            _editorTab = "basic";
            _error = null;
            _editDialogVisible = true;
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit)
                return;

            CleanupEditor();
            _editing = row;
            _editDialogTitle = "编辑动态插件";
            _editTenantId = row.TenantId;
            _editName = row.Name;
            _editAssemblyName = row.AssemblyName;
            _editOrderNo = row.OrderNo;
            _editStatus = row.Status == 2 ? 2 : 1;
            _editRemark = row.Remark;
            _editCsharpCode = row.CsharpCode;
            _editorTab = "basic";
            _error = null;
            _editDialogVisible = true;
        }

        private void EditorTabChanged(string value)
        {
            _editorTab = value == "code" ? "code" : "basic";
            if (_editorTab == "code")
                ScheduleMonacoReady();
            else
            {
                ++_monacoMountVersion;
                _editorLoading = false;
            }
        }

        private void RegenerateExampleCode()
        {
            _editCsharpCode = PluginShared.CreateDefaultCode(_editName);
            EditorTabChanged("code");
            _editorSession?.Write(_editCsharpCode);
        }

        private void Save()
        {
            if (_saving)
                return;

            _editCsharpCode = _editorSession?.Read() ?? _editCsharpCode;
            var normalizedName = _editName.Trim();
            var normalizedCode = _editCsharpCode.Trim();
            if (normalizedName.Length == 0)
            {
                _error = "功能名称不能为空。";
                return;
            }
            if (normalizedName.Length > 64)
            {
                _error = "功能名称不能超过64个字符。";
                return;
            }
            if (_editAssemblyName.Trim().Length > 512)
            {
                _error = "程序集名称不能超过512个字符。";
                return;
            }
            if (_editRemark.Trim().Length > 128)
            {
                _error = "备注不能超过128个字符。";
                return;
            }
            if (normalizedCode.Length == 0)
            {
                _error = "C# 代码不能为空。";
                EditorTabChanged("code");
                return;
            }
            if (IsSuperAdmin && _editTenantId.Length == 0)
            {
                _error = "请选择租户。";
                return;
            }

            _saving = true;
            var request = _editing is null
                ? PluginClient.Add(new PluginAddInput
                {
                    TenantId = PluginShared.Optional(_editTenantId),
                    Name = normalizedName,
                    CsharpCode = normalizedCode,
                    AssemblyName = PluginShared.Optional(_editAssemblyName),
                    OrderNo = _editOrderNo < 0 ? 0 : _editOrderNo,
                    Status = _editStatus == 2 ? 2 : 1,
                    Remark = PluginShared.Optional(_editRemark)
                })
                : PluginClient.Update(new PluginUpdateInput
                {
                    Id = _editing.Id,
                    TenantId = PluginShared.Optional(_editTenantId),
                    Name = normalizedName,
                    CsharpCode = normalizedCode,
                    AssemblyName = PluginShared.Optional(_editAssemblyName),
                    OrderNo = _editOrderNo < 0 ? 0 : _editOrderNo,
                    Status = _editStatus == 2 ? 2 : 1,
                    Remark = PluginShared.Optional(_editRemark),
                    CreateTime = _editing.CreateTime,
                    CreateUserId = PluginShared.Optional(_editing.CreateUserId),
                    CreateUserName = PluginShared.Optional(_editing.CreateUserName),
                    UpdateTime = PluginShared.Optional(_editing.UpdateTime),
                    UpdateUserId = PluginShared.Optional(_editing.UpdateUserId),
                    UpdateUserName = PluginShared.Optional(_editing.UpdateUserName)
                });

            var wasCreate = _editing is null;
            request.Then(outcome =>
            {
                _saving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存动态插件失败。";
                    return;
                }
                CloseEditor();
                _error = null;
                ElementPlus.ElMessageMembers.Success(wasCreate
                    ? "动态插件已新增、编译并加载"
                    : "动态插件已更新、重新编译并加载");
                LoadPage();
            }, _ =>
            {
                _saving = false;
                _error = "保存动态插件失败，请检查网络后重试。";
            });
        }

        private void CompileAndLoad(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit || _busyActionId.Length > 0)
                return;
            if (row.CsharpCode.Trim().Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("当前插件没有可编译的 C# 代码");
                return;
            }

            _busyActionId = row.Id;
            PluginClient.CompileAssembly(row.CsharpCode, row.AssemblyName).Then(outcome =>
            {
                _busyActionId = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "编译并加载动态插件失败。";
                    return;
                }
                var compiledName = PluginClient.ReadAssemblyName(outcome.Data);
                _error = null;
                ElementPlus.ElMessageMembers.Success(compiledName.Length == 0
                    ? "动态程序集已编译并加载"
                    : "动态程序集已编译并加载：" + compiledName);
            }, _ =>
            {
                _busyActionId = string.Empty;
                _error = "编译并加载动态插件失败，请检查网络后重试。";
            });
        }

        private void RemoveAssembly(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit || _busyActionId.Length > 0)
                return;
            if (row.AssemblyName.Trim().Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("当前插件没有可卸载的程序集名称");
                return;
            }

            _busyActionId = row.Id;
            PluginClient.RemoveAssembly(row.AssemblyName).Then(outcome =>
            {
                _busyActionId = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "卸载动态程序集失败。";
                    return;
                }
                _error = null;
                ElementPlus.ElMessageMembers.Success("动态程序集已从当前运行时卸载");
            }, _ =>
            {
                _busyActionId = string.Empty;
                _error = "卸载动态程序集失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete || _busyActionId.Length > 0)
                return;

            _busyActionId = row.Id;
            PluginClient.Delete(row.Id).Then(outcome =>
            {
                _busyActionId = string.Empty;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除动态插件失败。";
                    return;
                }
                _error = null;
                ElementPlus.ElMessageMembers.Success("动态插件及当前运行时程序集已删除");
                LoadPage();
            }, _ =>
            {
                _busyActionId = string.Empty;
                _error = "删除动态插件失败，请检查网络后重试。";
            });
        }

        private void OpenAudit(string id)
        {
            var row = Find(id);
            if (row is null)
                return;
            _auditDialogTitle = "修改记录 · " + row.Name;
            _auditCreateUser = DisplayUser(row.CreateUserName, row.CreateUserId);
            _auditCreateTime = DisplayValue(row.CreateTime);
            _auditUpdateUser = DisplayUser(row.UpdateUserName, row.UpdateUserId);
            _auditUpdateTime = DisplayValue(row.UpdateTime);
            _auditDialogVisible = true;
        }

        private void CloseEditor()
        {
            _editDialogVisible = false;
            CleanupEditor();
        }

        private void CleanupEditor()
        {
            _editorDialogOpened = false;
            ++_monacoMountVersion;
            _editorLoading = false;
            _editorLoadError = null;
            _editorSession?.Close();
            _editorSession = null;
        }

        private PluginView? Find(string id)
        {
            for (var index = 0; index < _items.Length; index++)
                if (_items[index].Id == id)
                    return _items[index];
            return null;
        }

        private PluginTableRow[] TableRows()
        {
            var result = new PluginTableRow[_items.Length];
            for (var index = 0; index < _items.Length; index++)
            {
                var item = _items[index];
                result[index] = new PluginTableRow(item.Id, item.Name, item.AssemblyName, item.Status, item.OrderNo, item.Remark);
            }
            return result;
        }

        private ConfigurationOption[] TenantOptions()
        {
            var result = new ConfigurationOption[_tenantItems.Length];
            for (var index = 0; index < _tenantItems.Length; index++)
                result[index] = new ConfigurationOption(_tenantItems[index].Value, _tenantItems[index].Label);
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
            Error = _error,
            Name = _name,
            EditDialogTitle = _editDialogTitle,
            EditTenantId = _editTenantId,
            EditName = _editName,
            EditAssemblyName = _editAssemblyName,
            EditRemark = _editRemark,
            EditorTab = _editorTab,
            EditorLoadError = _editorLoadError,
            AuditDialogTitle = _auditDialogTitle,
            AuditCreateUser = _auditCreateUser,
            AuditCreateTime = _auditCreateTime,
            AuditUpdateUser = _auditUpdateUser,
            AuditUpdateTime = _auditUpdateTime,
            BusyActionId = _busyActionId,
            TenantId = _tenantId,
            IsSuperAdmin = IsSuperAdmin,
            CanAdd = CanAdd,
            CanEdit = CanEdit,
            CanDelete = CanDelete,
            TenantLoading = _tenantLoading,
            Loading = _loading,
            EditDialogVisible = _editDialogVisible,
            EditorLoading = _editorLoading,
            Saving = _saving,
            AuditDialogVisible = _auditDialogVisible,
            Editing = _editing is not null,
            CurrentPage = _currentPage,
            PageSize = _pageSize,
            Total = _total,
            EditOrderNo = _editOrderNo,
            EditStatus = _editStatus,
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
            DismissError = () => _error = null,
            OpenAudit = OpenAudit,
            OpenEdit = OpenEdit,
            CompileAndLoad = CompileAndLoad,
            RemoveAssembly = RemoveAssembly,
            Delete = Delete,
            TenantChanged = TenantChanged,
            EditorTabChanged = EditorTabChanged,
            SetName = value => _name = value ?? "",
            SetEditName = value => _editName = value ?? "",
            SetEditAssemblyName = value => _editAssemblyName = value ?? "",
            SetEditRemark = value => _editRemark = value ?? "",
            SetEditTenantId = value => _editTenantId = value ?? "",
            SetEditOrderNo = value => _editOrderNo = (int)(value ?? 100),
            SetEditStatus = value => _editStatus = (int)value,
            ChangePage = ChangePage,
            ChangePageSize = ChangePageSize,
            SetEditDialogVisible = SetEditDialogVisible,
            SetAuditDialogVisible = value => _auditDialogVisible = value,
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
