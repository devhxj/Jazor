using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using static ECMAScript.Global;

namespace Jero.Web.App.PlatformConfig;

[ECMAScriptModule("components/platform-config-page-state.js")]
public static class PlatformConfigPageModule
{
    public static PlatformConfigPageState Create() => new();

    public sealed class PlatformConfigPageState : global::Jero.Web.App.IPageState<PlatformConfigPageProps>
    {
        private readonly double[] pageSizes = [10, 20, 50, 100];

        private PlatformConfigView[] items = [];
        private PlatformConfigTableRow[] tableRows = [];
        private PlatformConfigView[] selectedItems = [];
        private string[] groups = [];
        private int currentPage = 1;
        private int pageSize = 50;
        private int total;
        private string name = string.Empty;
        private string queryCode = string.Empty;
        private string groupCode = string.Empty;
        private string field = "orderNo";
        private string order = "ascending";
        private bool loading;
        private string? error;

        private bool editDialogVisible;
        private string editDialogTitle = "新增配置";
        private bool saving;
        private string editId = string.Empty;
        private string editName = string.Empty;
        private string editCode = string.Empty;
        private string editValue = string.Empty;
        private int editSysFlag = 2;
        private string editGroupCode = string.Empty;
        private int editOrderNo = 100;
        private string editRemark = string.Empty;
        private bool editBuiltinLocked;

        private bool auditDialogVisible;
        private string auditDialogTitle = "修改记录";
        private string auditCreateUser = "-";
        private string auditCreateTime = "-";
        private string auditUpdateUser = "-";
        private string auditUpdateTime = "-";
        private string auditRemark = "-";

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
            loading = true;
            PlatformConfigClient.Page(new PlatformConfigPageInput
            {
                page = currentPage,
                pageSize = pageSize,
                field = field,
                order = order,
                descStr = "descending",
                name = name.Trim(),
                code = queryCode.Trim(),
                groupCode = groupCode.Trim()
            }).Then(outcome =>
            {
                loading = false;
                selectedItems = [];
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载平台参数失败。";
                    items = [];
                    tableRows = [];
                    total = 0;
                    return;
                }

                items = PlatformConfigClient.ReadItems(outcome.Data);
                // Element Plus clears selection when the data reference changes.
                tableRows = TableRows();
                total = PlatformConfigClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                loading = false;
                error = "加载平台参数失败，请检查网络后重试。";
            });
        }

        private void LoadGroups()
        {
            PlatformConfigClient.Groups().Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载参数分组失败。";
                    groups = [];
                    return;
                }

                groups = PlatformConfigClient.ReadGroups(outcome.Data);
            }, _ => error = "加载参数分组失败，请检查网络后重试。");
        }

        private void Search()
        {
            currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            name = string.Empty;
            queryCode = string.Empty;
            groupCode = string.Empty;
            currentPage = 1;
            LoadPage();
        }

        private void Refresh()
        {
            LoadGroups();
            LoadPage();
        }

        private void GroupChanged(string? value)
            => groupCode = value ?? string.Empty;

        private void ChangePage(Number value)
        {
            currentPage = (int)value;
            LoadPage();
        }

        private void ChangePageSize(Number value)
        {
            pageSize = (int)value;
            currentPage = 1;
            LoadPage();
        }

        private void Sort(string prop, string direction)
        {
            field = string.IsNullOrWhiteSpace(prop) ? "orderNo" : prop;
            order = direction == "descending" ? "descending" : "ascending";
            LoadPage();
        }

        private void SelectionChanged(string[] selected)
        {
            var selectableCount = 0;
            for (var index = 0; index < selected.Length; index++)
            {
                var item = Find(selected[index]);
                if (item is not null && item.sysFlag != 1)
                    selectableCount++;
            }

            var result = new PlatformConfigView[selectableCount];
            var targetIndex = 0;
            for (var index = 0; index < selected.Length; index++)
            {
                var item = Find(selected[index]);
                if (item is null || item.sysFlag == 1)
                    continue;
                result[targetIndex] = item;
                targetIndex++;
            }
            selectedItems = result;
        }

        private void OpenCreate()
        {
            editDialogTitle = "新增配置";
            editId = string.Empty;
            editName = string.Empty;
            editCode = string.Empty;
            editValue = string.Empty;
            editSysFlag = 2;
            editGroupCode = string.Empty;
            editOrderNo = 100;
            editRemark = string.Empty;
            editBuiltinLocked = false;
            editDialogVisible = true;
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEditItem(row))
                return;

            editDialogTitle = "编辑配置";
            editId = row.id;
            editName = row.name;
            editCode = row.code;
            editValue = row.value;
            editSysFlag = row.sysFlag == 1 ? 1 : 2;
            editGroupCode = row.groupCode;
            editOrderNo = row.orderNo;
            editRemark = row.remark;
            editBuiltinLocked = row.sysFlag == 1;
            editDialogVisible = true;
        }

        private void Save()
        {
            if (saving)
                return;

            var normalizedName = editName.Trim();
            var normalizedCode = editCode.Trim();
            var normalizedValue = editValue.Trim();
            var normalizedGroup = editGroupCode.Trim();
            var normalizedRemark = editRemark.Trim();
            if (normalizedName.Length == 0)
            {
                error = "配置名称不能为空。";
                return;
            }
            if (normalizedName.Length > 64)
            {
                error = "配置名称不能超过64个字符。";
                return;
            }
            if (normalizedCode.Length == 0)
            {
                error = "配置编码不能为空。";
                return;
            }
            if (normalizedCode.Length > 64)
            {
                error = "配置编码不能超过64个字符。";
                return;
            }
            if (normalizedValue.Length == 0)
            {
                error = "属性值不能为空。";
                return;
            }
            if (normalizedValue.Length > 512)
            {
                error = "属性值不能超过512个字符。";
                return;
            }
            if (normalizedGroup.Length > 64)
            {
                error = "分组编码不能超过64个字符。";
                return;
            }
            if (normalizedRemark.Length > 256)
            {
                error = "备注不能超过256个字符。";
                return;
            }

            var input = new PlatformConfigSaveInput
            {
                id = editId,
                name = normalizedName,
                code = normalizedCode,
                value = normalizedValue,
                sysFlag = IsSuperAdmin && editSysFlag == 1 ? 1 : 2,
                groupCode = normalizedGroup,
                orderNo = editOrderNo < 0 ? 0 : editOrderNo,
                remark = normalizedRemark
            };

            saving = true;
            var request = editId.Length == 0 ? PlatformConfigClient.Add(input) : PlatformConfigClient.Update(input);
            request.Then(outcome =>
            {
                saving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存平台参数失败。";
                    return;
                }

                editDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success(editId.Length == 0 ? "配置已新增" : "配置已更新");
                LoadGroups();
                LoadPage();
            }, _ =>
            {
                saving = false;
                error = "保存平台参数失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || row.sysFlag == 1 || !CanDelete)
                return;

            PlatformConfigClient.Delete(row.id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除平台参数失败。";
                    return;
                }

                error = null;
                ElementPlus.ElMessageMembers.Success("配置已删除");
                LoadGroups();
                LoadPage();
            }, _ => error = "删除平台参数失败，请检查网络后重试。");
        }

        private void BatchDelete()
        {
            if (!CanBatchDelete || selectedItems.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请选择可删除的配置");
                return;
            }

            var ids = new string[selectedItems.Length];
            for (var index = 0; index < selectedItems.Length; index++)
                ids[index] = selectedItems[index].id;

            PlatformConfigClient.BatchDelete(ids).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "批量删除平台参数失败。";
                    return;
                }

                selectedItems = [];
                currentPage = 1;
                error = null;
                ElementPlus.ElMessageMembers.Success("配置已批量删除");
                LoadGroups();
                LoadPage();
            }, _ => error = "批量删除平台参数失败，请检查网络后重试。");
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
            auditRemark = DisplayValue(row.remark);
            auditDialogVisible = true;
        }

        private bool CanEditItem(PlatformConfigView? row)
            => CanEdit && row is not null && (row.sysFlag != 1 || IsSuperAdmin);

        private PlatformConfigView? Find(string id)
        {
            for (var index = 0; index < items.Length; index++)
                if (items[index].id == id)
                    return items[index];
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
            var result = new PlatformConfigTableRow[items.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var row = items[index];
                result[index] = new PlatformConfigTableRow(
                    row.id,
                    row.name,
                    row.code,
                    row.value,
                    row.groupCode,
                    row.orderNo,
                    row.sysFlag,
                    CanEditItem(row),
                    row.sysFlag == 1 && !IsSuperAdmin ? "仅超级管理员可编辑内置参数" : string.Empty,
                    CanDelete && row.sysFlag != 1,
                    row.sysFlag == 1 ? "内置参数不可删除" : string.Empty);
            }
            return result;
        }

        private ConfigurationOption[] GroupOptions()
        {
            var result = new ConfigurationOption[groups.Length];
            for (var index = 0; index < result.Length; index++)
                result[index] = new ConfigurationOption(groups[index], groups[index]);
            return result;
        }

        public PlatformConfigPageProps CreateProps() => new()
        {
            Rows = tableRows,
            CanBatchDelete = CanBatchDelete,
            CanEdit = CanEdit,
            CanDelete = CanDelete,
            OpenAudit = OpenAudit,
            OpenEdit = OpenEdit,
            Delete = Delete,
            Sort = Sort,
            SelectionChanged = SelectionChanged,
            GroupCode = groupCode,
            GroupOptions = GroupOptions(),
            GroupChanged = GroupChanged,
            SelectedCount = selectedItems.Length,
            EditSysFlag = editSysFlag,
            SetEditSysFlag = value => editSysFlag = (int)value,
            EditOrderNo = editOrderNo,
            SetEditOrderNo = value => editOrderNo = (int)(value ?? 100),
            SetName = value => name = value ?? "",
            SetQueryCode = value => queryCode = value ?? "",
            SetEditName = value => editName = value ?? "",
            SetEditCode = value => editCode = value ?? "",
            SetEditValue = value => editValue = value ?? "",
            SetEditGroupCode = value => editGroupCode = value ?? "",
            SetEditRemark = value => editRemark = value ?? "",
            SetEditDialogVisible = value => editDialogVisible = value,
            SetAuditDialogVisible = value => auditDialogVisible = value,
            SetTrue = () => editValue = "True",
            SetFalse = () => editValue = "False",
            CloseEditDialogVisible = () => editDialogVisible = false,
            Search = Search,
            ResetSearch = ResetSearch,
            OpenCreate = OpenCreate,
            Refresh = Refresh,
            Save = Save,
            DismissError = () => error = null,
            BatchDelete = BatchDelete,
            ChangePage = value => ChangePage(value),
            ChangePageSize = value => ChangePageSize(value),
            IsSuperAdmin = IsSuperAdmin,
            Error = error,
            CurrentPage = currentPage,
            PageSize = pageSize,
            Total = total,
            PageSizes = pageSizes,
            Loading = loading,
            Saving = saving,
            EditBuiltinLocked = editBuiltinLocked,
            EditDialogVisible = editDialogVisible,
            AuditDialogVisible = auditDialogVisible,
            CanAdd = CanAdd,
            Name = name,
            QueryCode = queryCode,
            EditDialogTitle = editDialogTitle,
            EditName = editName,
            EditRemark = editRemark,
            EditCode = editCode,
            EditValue = editValue,
            EditGroupCode = editGroupCode,
            AuditDialogTitle = auditDialogTitle,
            AuditCreateUser = auditCreateUser,
            AuditCreateTime = auditCreateTime,
            AuditUpdateUser = auditUpdateUser,
            AuditUpdateTime = auditUpdateTime,
            AuditRemark = auditRemark,
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
