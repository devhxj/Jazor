using System.Text;
using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using static ECMAScript.Global;

namespace Jero.Web.App.TenantConfig;

[ECMAScriptModule("components/tenant-config-page-state.js")]
public static class TenantConfigPageModule
{
    public static TenantConfigPageState Create() => new();

    public sealed class TenantConfigPageState : global::Jero.Web.App.IPageState<TenantConfigPageProps>
    {
        private readonly double[] pageSizes = [10, 20, 50, 100];

        private TenantConfigView[] items = [];
        private TenantConfigTableRow[] rows = [];
        private TenantConfigView[] selectedItems = [];
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
        private bool exporting;
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

        private bool CanAdd => ApplicationState.HasPermission("sysTenantConfig:add");
        private bool CanEdit => ApplicationState.HasPermission("sysTenantConfig:update");
        private bool CanDelete => ApplicationState.HasPermission("sysTenantConfig:delete");
        private bool CanBatchDelete => ApplicationState.HasPermission("sysTenantConfig:batchDelete");

        public void Initialize()
        {
            LoadGroups();
            LoadPage();
        }

        private TenantConfigPageInput BuildPageInput(int page, int size)
            => new()
            {
                page = page,
                pageSize = size,
                field = field,
                order = order,
                descStr = "descending",
                name = name.Trim(),
                code = queryCode.Trim(),
                groupCode = groupCode.Trim()
            };

        private void LoadPage()
        {
            loading = true;
            TenantConfigClient.Page(BuildPageInput(currentPage, pageSize)).Then(outcome =>
            {
                loading = false;
                selectedItems = [];
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载系统参数失败。";
                    items = [];
                    rows = [];
                    total = 0;
                    return;
                }

                items = TenantConfigClient.ReadItems(outcome.Data);
                // Element Plus tracks selected rows by identity between renders.
                rows = TableRows();
                total = TenantConfigClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                loading = false;
                error = "加载系统参数失败，请检查网络后重试。";
            });
        }

        private void LoadGroups()
        {
            TenantConfigClient.Groups().Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载参数分组失败。";
                    groups = [];
                    return;
                }

                groups = TenantConfigClient.ReadGroups(outcome.Data);
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

        private bool CanSelectRow(VueProps row, Number index)
            => RowInt(row, "sysFlag", 2) != 1;

        private void SelectionChanged(string[] selected)
        {
            var selectableCount = 0;
            for (var index = 0; index < selected.Length; index++)
            {
                var item = Find(selected[index]);
                if (item is not null && item.sysFlag != 1)
                    selectableCount++;
            }

            var result = new TenantConfigView[selectableCount];
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
            if (row is null || !CanEdit)
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

            var input = new TenantConfigSaveInput
            {
                id = editId,
                name = normalizedName,
                code = normalizedCode,
                value = normalizedValue,
                sysFlag = editSysFlag == 1 ? 1 : 2,
                groupCode = normalizedGroup,
                orderNo = editOrderNo < 0 ? 0 : editOrderNo,
                remark = normalizedRemark
            };

            saving = true;
            var request = editId.Length == 0 ? TenantConfigClient.Add(input) : TenantConfigClient.Update(input);
            request.Then(outcome =>
            {
                saving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存系统参数失败。";
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
                error = "保存系统参数失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || row.sysFlag == 1 || !CanDelete)
                return;

            TenantConfigClient.Delete(row.id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除系统参数失败。";
                    return;
                }

                error = null;
                ElementPlus.ElMessageMembers.Success("配置已删除");
                LoadGroups();
                LoadPage();
            }, _ => error = "删除系统参数失败，请检查网络后重试。");
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

            TenantConfigClient.BatchDelete(ids).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "批量删除系统参数失败。";
                    return;
                }

                selectedItems = [];
                currentPage = 1;
                error = null;
                ElementPlus.ElMessageMembers.Success("配置已批量删除");
                LoadGroups();
                LoadPage();
            }, _ => error = "批量删除系统参数失败，请检查网络后重试。");
        }

        private void ExportPage()
            => DownloadCsv(items, "本页数据");

        private void ExportCommand(string command)
        {
            if (command == "all")
                ExportAll();
            else
                ExportPage();
        }

        private void ExportAll()
        {
            if (exporting)
                return;

            exporting = true;
            TenantConfigClient.Page(BuildPageInput(1, 999999)).Then(outcome =>
            {
                exporting = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "导出系统参数失败。";
                    return;
                }

                error = null;
                DownloadCsv(TenantConfigClient.ReadItems(outcome.Data), "全部数据");
            }, _ =>
            {
                exporting = false;
                error = "导出系统参数失败，请检查网络后重试。";
            });
        }

        private static void DownloadCsv(TenantConfigView[] source, string scopeName)
        {
            if (source.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("没有数据可以导出");
                return;
            }

            TenantConfigBrowserActions.DownloadCsv(BuildCsv(source), "系统参数_" + scopeName + ".csv");
        }

        private void PrintPage()
        {
            if (items.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("没有数据可以打印");
                return;
            }

            if (!TenantConfigBrowserActions.PrintHtml(BuildPrintHtml(items)))
            {
                ElementPlus.ElMessageMembers.Error("无法打开打印窗口，请检查浏览器弹窗拦截设置");
            }
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

        private TenantConfigView? Find(string id)
        {
            for (var index = 0; index < items.Length; index++)
                if (items[index].id == id)
                    return items[index];
            return null;
        }

        private static string BuildCsv(TenantConfigView[] source)
        {
            var builder = new StringBuilder();
            builder.Append("\"配置名称\",\"配置编码\",\"属性值\",\"内置参数\",\"分组编码\",\"排序\",\"创建者\",\"创建时间\",\"修改者\",\"修改时间\",\"备注\"\r\n");
            for (var index = 0; index < source.Length; index++)
            {
                var row = source[index];
                builder.Append(CsvCell(row.name));
                builder.Append(",");
                builder.Append(CsvCell(row.code));
                builder.Append(",");
                builder.Append(CsvCell(row.value));
                builder.Append(",");
                builder.Append(CsvCell(row.sysFlag == 1 ? "是" : "否"));
                builder.Append(",");
                builder.Append(CsvCell(row.groupCode));
                builder.Append(",");
                builder.Append(CsvCell(row.orderNo.ToString()));
                builder.Append(",");
                builder.Append(CsvCell(row.createUserName));
                builder.Append(",");
                builder.Append(CsvCell(row.createTime));
                builder.Append(",");
                builder.Append(CsvCell(row.updateUserName));
                builder.Append(",");
                builder.Append(CsvCell(row.updateTime));
                builder.Append(",");
                builder.Append(CsvCell(row.remark));
                builder.Append("\r\n");
            }
            return builder.ToString();
        }

        private static string BuildPrintHtml(TenantConfigView[] source)
        {
            var builder = new StringBuilder();
            builder.Append("<!doctype html><html><head><meta charset=\"utf-8\"><title>系统参数</title>");
            builder.Append("<style>body{font-family:Arial,sans-serif;padding:24px;color:#111827}h1{font-size:20px;margin:0 0 16px}table{border-collapse:collapse;width:100%;font-size:12px}th,td{border:1px solid #d1d5db;padding:8px;text-align:left}th{background:#f3f4f6}</style>");
            builder.Append("</head><body><h1>系统参数</h1><table><thead><tr><th>配置名称</th><th>配置编码</th><th>属性值</th><th>内置参数</th><th>分组编码</th><th>排序</th></tr></thead><tbody>");
            for (var index = 0; index < source.Length; index++)
            {
                var row = source[index];
                builder.Append("<tr><td>");
                builder.Append(EscapeHtml(row.name));
                builder.Append("</td><td>");
                builder.Append(EscapeHtml(row.code));
                builder.Append("</td><td>");
                builder.Append(EscapeHtml(row.value));
                builder.Append("</td><td>");
                builder.Append(row.sysFlag == 1 ? "是" : "否");
                builder.Append("</td><td>");
                builder.Append(EscapeHtml(row.groupCode));
                builder.Append("</td><td>");
                builder.Append(row.orderNo.ToString());
                builder.Append("</td></tr>");
            }
            builder.Append("</tbody></table></body></html>");
            return builder.ToString();
        }

        private static string CsvCell(string value)
            => "\"" + value.Replace("\"", "\"\"") + "\"";

        private static string EscapeHtml(string value)
            => value.Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");

        private static string RowString(VueProps? row, string key)
        {
            if (row is null)
                return string.Empty;
            var value = Reflect.Get(row, key);
            return value is null ? string.Empty : StringValue(value);
        }

        private static int RowInt(VueProps? row, string key, int fallback)
        {
            if (row is null)
                return fallback;
            var value = Reflect.Get(row, key);
            return value is null ? fallback : (int)NumberValue(value);
        }

        private static string YesNoText(VueProps? row)
            => RowInt(row, "sysFlag", 2) == 1 ? "是" : "否";

        private static string YesNoTagType(VueProps? row)
            => RowInt(row, "sysFlag", 2) == 1 ? "success" : "info";

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


        private TenantConfigTableRow[] TableRows()
        {
            var result = new TenantConfigTableRow[items.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var row = items[index];
                result[index] = new TenantConfigTableRow(
                    row.id,
                    row.name,
                    row.code,
                    row.value,
                    row.groupCode,
                    row.orderNo,
                    row.sysFlag,
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

        public TenantConfigPageProps CreateProps() => new()
        {
            Rows = rows,
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
            Exporting = exporting,
            ExportPage = ExportPage,
            ExportCommand = ExportCommand,
            SelectedCount = selectedItems.Length,
            ItemCount = items.Length,
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
            PrintPage = PrintPage,
            Save = Save,
            DismissError = () => error = null,
            BatchDelete = BatchDelete,
            ChangePage = value => ChangePage(value),
            ChangePageSize = value => ChangePageSize(value),
            Editing = editId.Length > 0,
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

public sealed record TenantConfigPageProps : VueProps
{
    public required TenantConfigTableRow[] Rows { get; init; }
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
    public required bool Exporting { get; init; }
    public required Action ExportPage { get; init; }
    public required Action<string> ExportCommand { get; init; }
    public required int SelectedCount { get; init; }
    public required int ItemCount { get; init; }
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
    public required Action PrintPage { get; init; }
    public required Action Save { get; init; }
    public required Action DismissError { get; init; }
    public required Action BatchDelete { get; init; }
    public required Action<double> ChangePage { get; init; }
    public required Action<double> ChangePageSize { get; init; }
    public required bool Editing { get; init; }
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
