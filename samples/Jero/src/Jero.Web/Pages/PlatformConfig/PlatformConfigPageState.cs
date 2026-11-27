using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.PlatformConfig;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;

namespace Jero.Web.Pages.PlatformConfig;

[ECMAScriptModule("components/platform-config-page-state.js")]
public static class PlatformConfigPageModule
{
    public static PlatformConfigPageState Create() => new();

    public sealed class PlatformConfigPageState : global::Jero.Web.Composition.IPageState<PlatformConfigPageProps>
    {
        private readonly double[] _pageSizes = [10, 20, 50, 100];

        private PlatformConfigView[] _items = [];
        private PlatformConfigTableRow[] _tableRows = [];
        private PlatformConfigView[] _selectedItems = [];
        private string[] _groups = [];
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _total;
        private string _name = string.Empty;
        private string _queryCode = string.Empty;
        private string _groupCode = string.Empty;
        private string _field = "orderNo";
        private string _order = "ascending";
        private bool _loading;
        private string? _error;

        private bool _editDialogVisible;
        private string _editDialogTitle = "新增配置";
        private bool _saving;
        private string _editId = string.Empty;
        private string _editName = string.Empty;
        private string _editCode = string.Empty;
        private string _editValue = string.Empty;
        private int _editSysFlag = 2;
        private string _editGroupCode = string.Empty;
        private int _editOrderNo = 100;
        private string _editRemark = string.Empty;
        private bool _editBuiltinLocked;

        private bool _auditDialogVisible;
        private string _auditDialogTitle = "修改记录";
        private string _auditCreateUser = "-";
        private string _auditCreateTime = "-";
        private string _auditUpdateUser = "-";
        private string _auditUpdateTime = "-";
        private string _auditRemark = "-";

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool CanAdd => ApplicationState.HasPermission("sysConfig:add");
        private bool CanEdit => ApplicationState.HasPermission("sysConfig:update");
        private bool CanDelete => ApplicationState.HasPermission("sysConfig:delete");
        private bool CanBatchDelete => ApplicationState.HasPermission("sysConfig:batchDelete");

        public void Initialize()
        {
            LoadGroups();
            LoadPage();
        }

        private void LoadPage()
        {
            _loading = true;
            PlatformConfigClient.Page(new PlatformConfigPageInput
            {
                Page = _currentPage,
                PageSize = _pageSize,
                Field = _field,
                Order = _order,
                DescStr = "descending",
                Name = _name.Trim(),
                Code = _queryCode.Trim(),
                GroupCode = _groupCode.Trim()
            }).Then(outcome =>
            {
                _loading = false;
                _selectedItems = [];
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载平台参数失败。";
                    _items = [];
                    _tableRows = [];
                    _total = 0;
                    return;
                }

                _items = PlatformConfigClient.ReadItems(outcome.Data);
                // 数据引用变化时，Element Plus 会清空选中项。
                _tableRows = TableRows();
                _total = PlatformConfigClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                _loading = false;
                _error = "加载平台参数失败，请检查网络后重试。";
            });
        }

        private void LoadGroups()
        {
            PlatformConfigClient.Groups().Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载参数分组失败。";
                    _groups = [];
                    return;
                }

                _groups = PlatformConfigClient.ReadGroups(outcome.Data);
            }, _ => _error = "加载参数分组失败，请检查网络后重试。");
        }

        private void Search()
        {
            _currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            _name = string.Empty;
            _queryCode = string.Empty;
            _groupCode = string.Empty;
            _currentPage = 1;
            LoadPage();
        }

        private void Refresh()
        {
            LoadGroups();
            LoadPage();
        }

        private void GroupChanged(string? value)
            => _groupCode = value ?? string.Empty;

        private void ChangePage(Number value)
        {
            _currentPage = (int)value;
            LoadPage();
        }

        private void ChangePageSize(Number value)
        {
            _pageSize = (int)value;
            _currentPage = 1;
            LoadPage();
        }

        private void Sort(string prop, string direction)
        {
            _field = string.IsNullOrWhiteSpace(prop) ? "orderNo" : prop;
            _order = direction == "descending" ? "descending" : "ascending";
            LoadPage();
        }

        private void SelectionChanged(string[] selected)
        {
            var selectableCount = 0;
            for (var index = 0; index < selected.Length; index++)
            {
                var item = Find(selected[index]);
                if (item is not null && item.SysFlag != 1)
                    selectableCount++;
            }

            var result = new PlatformConfigView[selectableCount];
            var targetIndex = 0;
            for (var index = 0; index < selected.Length; index++)
            {
                var item = Find(selected[index]);
                if (item is null || item.SysFlag == 1)
                    continue;
                result[targetIndex] = item;
                targetIndex++;
            }
            _selectedItems = result;
        }

        private void OpenCreate()
        {
            _editDialogTitle = "新增配置";
            _editId = string.Empty;
            _editName = string.Empty;
            _editCode = string.Empty;
            _editValue = string.Empty;
            _editSysFlag = 2;
            _editGroupCode = string.Empty;
            _editOrderNo = 100;
            _editRemark = string.Empty;
            _editBuiltinLocked = false;
            _editDialogVisible = true;
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEditItem(row))
                return;

            _editDialogTitle = "编辑配置";
            _editId = row.Id;
            _editName = row.Name;
            _editCode = row.Code;
            _editValue = row.Value;
            _editSysFlag = row.SysFlag == 1 ? 1 : 2;
            _editGroupCode = row.GroupCode;
            _editOrderNo = row.OrderNo;
            _editRemark = row.Remark;
            _editBuiltinLocked = row.SysFlag == 1;
            _editDialogVisible = true;
        }

        private void Save()
        {
            if (_saving)
                return;

            var normalizedName = _editName.Trim();
            var normalizedCode = _editCode.Trim();
            var normalizedValue = _editValue.Trim();
            var normalizedGroup = _editGroupCode.Trim();
            var normalizedRemark = _editRemark.Trim();
            if (normalizedName.Length == 0)
            {
                _error = "配置名称不能为空。";
                return;
            }
            if (normalizedName.Length > 64)
            {
                _error = "配置名称不能超过64个字符。";
                return;
            }
            if (normalizedCode.Length == 0)
            {
                _error = "配置编码不能为空。";
                return;
            }
            if (normalizedCode.Length > 64)
            {
                _error = "配置编码不能超过64个字符。";
                return;
            }
            if (normalizedValue.Length == 0)
            {
                _error = "属性值不能为空。";
                return;
            }
            if (normalizedValue.Length > 512)
            {
                _error = "属性值不能超过512个字符。";
                return;
            }
            if (normalizedGroup.Length > 64)
            {
                _error = "分组编码不能超过64个字符。";
                return;
            }
            if (normalizedRemark.Length > 256)
            {
                _error = "备注不能超过256个字符。";
                return;
            }

            var input = new PlatformConfigSaveInput
            {
                Id = _editId,
                Name = normalizedName,
                Code = normalizedCode,
                Value = normalizedValue,
                SysFlag = IsSuperAdmin && _editSysFlag == 1 ? 1 : 2,
                GroupCode = normalizedGroup,
                OrderNo = _editOrderNo < 0 ? 0 : _editOrderNo,
                Remark = normalizedRemark
            };

            _saving = true;
            var request = _editId.Length == 0 ? PlatformConfigClient.Add(input) : PlatformConfigClient.Update(input);
            request.Then(outcome =>
            {
                _saving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存平台参数失败。";
                    return;
                }

                _editDialogVisible = false;
                _error = null;
                ElementPlus.ElMessageMembers.Success(_editId.Length == 0 ? "配置已新增" : "配置已更新");
                LoadGroups();
                LoadPage();
            }, _ =>
            {
                _saving = false;
                _error = "保存平台参数失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || row.SysFlag == 1 || !CanDelete)
                return;

            PlatformConfigClient.Delete(row.Id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除平台参数失败。";
                    return;
                }

                _error = null;
                ElementPlus.ElMessageMembers.Success("配置已删除");
                LoadGroups();
                LoadPage();
            }, _ => _error = "删除平台参数失败，请检查网络后重试。");
        }

        private void BatchDelete()
        {
            if (!CanBatchDelete || _selectedItems.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请选择可删除的配置");
                return;
            }

            var ids = new string[_selectedItems.Length];
            for (var index = 0; index < _selectedItems.Length; index++)
                ids[index] = _selectedItems[index].Id;

            PlatformConfigClient.BatchDelete(ids).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "批量删除平台参数失败。";
                    return;
                }

                _selectedItems = [];
                _currentPage = 1;
                _error = null;
                ElementPlus.ElMessageMembers.Success("配置已批量删除");
                LoadGroups();
                LoadPage();
            }, _ => _error = "批量删除平台参数失败，请检查网络后重试。");
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
            _auditRemark = DisplayValue(row.Remark);
            _auditDialogVisible = true;
        }

        private bool CanEditItem(PlatformConfigView? row)
            => CanEdit && row is not null && (row.SysFlag != 1 || IsSuperAdmin);

        private PlatformConfigView? Find(string id)
        {
            for (var index = 0; index < _items.Length; index++)
                if (_items[index].Id == id)
                    return _items[index];
            return null;
        }

        private static string DisplayUser(string name, string id)
        {
            if (name.Length > 0 && id.Length > 0)
                return name + " (" + id + ")";
            if (name.Length > 0)
                return name;
            return DisplayValue(id);
        }

        private static string DisplayValue(string value)
            => value.Length == 0 ? "-" : value;
        private PlatformConfigTableRow[] TableRows()
        {
            var result = new PlatformConfigTableRow[_items.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var row = _items[index];
                result[index] = new PlatformConfigTableRow(
                    row.Id,
                    row.Name,
                    row.Code,
                    row.Value,
                    row.GroupCode,
                    row.OrderNo,
                    row.SysFlag,
                    CanEditItem(row),
                    row.SysFlag == 1 && !IsSuperAdmin ? "仅超级管理员可编辑内置参数" : string.Empty,
                    CanDelete && row.SysFlag != 1,
                    row.SysFlag == 1 ? "内置参数不可删除" : string.Empty);
            }
            return result;
        }

        private ConfigurationOption[] GroupOptions()
        {
            var result = new ConfigurationOption[_groups.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new ConfigurationOption(_groups[index], _groups[index]);
            return result;
        }

        public PlatformConfigPageProps CreateProps() => new()
        {
            Rows = _tableRows,
            CanBatchDelete = CanBatchDelete,
            CanEdit = CanEdit,
            CanDelete = CanDelete,
            OpenAudit = OpenAudit,
            OpenEdit = OpenEdit,
            Delete = Delete,
            Sort = Sort,
            SelectionChanged = SelectionChanged,
            GroupCode = _groupCode,
            GroupOptions = GroupOptions(),
            GroupChanged = GroupChanged,
            SelectedCount = _selectedItems.Length,
            EditSysFlag = _editSysFlag,
            SetEditSysFlag = value => _editSysFlag = (int)value,
            EditOrderNo = _editOrderNo,
            SetEditOrderNo = value => _editOrderNo = (int)(value ?? 100),
            SetName = value => _name = value ?? "",
            SetQueryCode = value => _queryCode = value ?? "",
            SetEditName = value => _editName = value ?? "",
            SetEditCode = value => _editCode = value ?? "",
            SetEditValue = value => _editValue = value ?? "",
            SetEditGroupCode = value => _editGroupCode = value ?? "",
            SetEditRemark = value => _editRemark = value ?? "",
            SetEditDialogVisible = value => _editDialogVisible = value,
            SetAuditDialogVisible = value => _auditDialogVisible = value,
            SetTrue = () => _editValue = "True",
            SetFalse = () => _editValue = "False",
            CloseEditDialogVisible = () => _editDialogVisible = false,
            Search = Search,
            ResetSearch = ResetSearch,
            OpenCreate = OpenCreate,
            Refresh = Refresh,
            Save = Save,
            DismissError = () => _error = null,
            BatchDelete = BatchDelete,
            ChangePage = value => ChangePage(value),
            ChangePageSize = value => ChangePageSize(value),
            IsSuperAdmin = IsSuperAdmin,
            Error = _error,
            CurrentPage = _currentPage,
            PageSize = _pageSize,
            Total = _total,
            PageSizes = _pageSizes,
            Loading = _loading,
            Saving = _saving,
            EditBuiltinLocked = _editBuiltinLocked,
            EditDialogVisible = _editDialogVisible,
            AuditDialogVisible = _auditDialogVisible,
            CanAdd = CanAdd,
            Name = _name,
            QueryCode = _queryCode,
            EditDialogTitle = _editDialogTitle,
            EditName = _editName,
            EditRemark = _editRemark,
            EditCode = _editCode,
            EditValue = _editValue,
            EditGroupCode = _editGroupCode,
            AuditDialogTitle = _auditDialogTitle,
            AuditCreateUser = _auditCreateUser,
            AuditCreateTime = _auditCreateTime,
            AuditUpdateUser = _auditUpdateUser,
            AuditUpdateTime = _auditUpdateTime,
            AuditRemark = _auditRemark,
        };
    }
}

public sealed record PlatformConfigPageProps : VueProps
{
    public required PlatformConfigTableRow[] Rows { get; init; }
    public required bool CanBatchDelete { get; init; }
    public required bool CanEdit { get; init; }
    public required bool CanDelete { get; init; }
    public required Action<string> OpenAudit { get; init; }
    public required Action<string> OpenEdit { get; init; }
    public required Action<string> Delete { get; init; }
    public required Action<string, string> Sort { get; init; }
    public required Action<string[]> SelectionChanged { get; init; }
    public required string GroupCode { get; init; }
    public required ConfigurationOption[] GroupOptions { get; init; }
    public required Action<string> GroupChanged { get; init; }
    public required int SelectedCount { get; init; }
    public required int EditSysFlag { get; init; }
    public required Action<double> SetEditSysFlag { get; init; }
    public required int EditOrderNo { get; init; }
    public required Action<double?> SetEditOrderNo { get; init; }
    public required Action<string?> SetName { get; init; }
    public required Action<string?> SetQueryCode { get; init; }
    public required Action<string?> SetEditName { get; init; }
    public required Action<string?> SetEditCode { get; init; }
    public required Action<string?> SetEditValue { get; init; }
    public required Action<string?> SetEditGroupCode { get; init; }
    public required Action<string?> SetEditRemark { get; init; }
    public required Action<bool> SetEditDialogVisible { get; init; }
    public required Action<bool> SetAuditDialogVisible { get; init; }
    public required Action SetTrue { get; init; }
    public required Action SetFalse { get; init; }
    public required Action CloseEditDialogVisible { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action OpenCreate { get; init; }
    public required Action Refresh { get; init; }
    public required Action Save { get; init; }
    public required Action DismissError { get; init; }
    public required Action BatchDelete { get; init; }
    public required Action<double> ChangePage { get; init; }
    public required Action<double> ChangePageSize { get; init; }
    public required bool IsSuperAdmin { get; init; }
    public required string? Error { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public required double[] PageSizes { get; init; }
    public required bool Loading { get; init; }
    public required bool Saving { get; init; }
    public required bool EditBuiltinLocked { get; init; }
    public required bool EditDialogVisible { get; init; }
    public required bool AuditDialogVisible { get; init; }
    public required bool CanAdd { get; init; }
    public required string Name { get; init; }
    public required string QueryCode { get; init; }
    public required string EditDialogTitle { get; init; }
    public required string EditName { get; init; }
    public required string EditRemark { get; init; }
    public required string EditCode { get; init; }
    public required string EditValue { get; init; }
    public required string EditGroupCode { get; init; }
    public required string AuditDialogTitle { get; init; }
    public required string AuditCreateUser { get; init; }
    public required string AuditCreateTime { get; init; }
    public required string AuditUpdateUser { get; init; }
    public required string AuditUpdateTime { get; init; }
    public required string AuditRemark { get; init; }
}
