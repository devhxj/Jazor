using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.TenantConfig;
using System.Text;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;

namespace Jero.Web.Pages.TenantConfig;

[ECMAScriptModule("components/tenant-config-page-state.js")]
public static class TenantConfigPageModule
{
    public static TenantConfigPageState Create() => new();

    public sealed class TenantConfigPageState : global::Jero.Web.Composition.IPageState<TenantConfigPageProps>
    {
        private readonly double[] _pageSizes = [10, 20, 50, 100];

        private TenantConfigView[] _items = [];
        private TenantConfigTableRow[] _rows = [];
        private TenantConfigView[] _selectedItems = [];
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
        private bool _exporting;
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
                Page = page,
                PageSize = size,
                Field = _field,
                Order = _order,
                DescStr = "descending",
                Name = _name.Trim(),
                Code = _queryCode.Trim(),
                GroupCode = _groupCode.Trim()
            };

        private void LoadPage()
        {
            _loading = true;
            TenantConfigClient.Page(BuildPageInput(_currentPage, _pageSize)).Then(outcome =>
            {
                _loading = false;
                _selectedItems = [];
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载系统参数失败。";
                    _items = [];
                    _rows = [];
                    _total = 0;
                    return;
                }

                _items = TenantConfigClient.ReadItems(outcome.Data);
                // Element Plus 在重新渲染时按行对象的引用识别选中项。
                _rows = TableRows();
                _total = TenantConfigClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                _loading = false;
                _error = "加载系统参数失败，请检查网络后重试。";
            });
        }

        private void LoadGroups()
        {
            TenantConfigClient.Groups().Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载参数分组失败。";
                    _groups = [];
                    return;
                }

                _groups = TenantConfigClient.ReadGroups(outcome.Data);
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

        private bool CanSelectRow(VueProps row, Number index)
            => RowInt(row, "sysFlag", 2) != 1;

        private void SelectionChanged(string[] selected)
        {
            var selectableCount = 0;
            for (var index = 0; index < selected.Length; index++)
            {
                var item = Find(selected[index]);
                if (item is not null && item.SysFlag != 1)
                    selectableCount++;
            }

            var result = new TenantConfigView[selectableCount];
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
            if (row is null || !CanEdit)
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

            var input = new TenantConfigSaveInput
            {
                Id = _editId,
                Name = normalizedName,
                Code = normalizedCode,
                Value = normalizedValue,
                SysFlag = _editSysFlag == 1 ? 1 : 2,
                GroupCode = normalizedGroup,
                OrderNo = _editOrderNo < 0 ? 0 : _editOrderNo,
                Remark = normalizedRemark
            };

            _saving = true;
            var request = _editId.Length == 0 ? TenantConfigClient.Add(input) : TenantConfigClient.Update(input);
            request.Then(outcome =>
            {
                _saving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存系统参数失败。";
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
                _error = "保存系统参数失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || row.SysFlag == 1 || !CanDelete)
                return;

            TenantConfigClient.Delete(row.Id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除系统参数失败。";
                    return;
                }

                _error = null;
                ElementPlus.ElMessageMembers.Success("配置已删除");
                LoadGroups();
                LoadPage();
            }, _ => _error = "删除系统参数失败，请检查网络后重试。");
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

            TenantConfigClient.BatchDelete(ids).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "批量删除系统参数失败。";
                    return;
                }

                _selectedItems = [];
                _currentPage = 1;
                _error = null;
                ElementPlus.ElMessageMembers.Success("配置已批量删除");
                LoadGroups();
                LoadPage();
            }, _ => _error = "批量删除系统参数失败，请检查网络后重试。");
        }

        private void ExportPage()
            => DownloadCsv(_items, "本页数据");

        private void ExportCommand(string command)
        {
            if (command == "all")
                ExportAll();
            else
                ExportPage();
        }

        private void ExportAll()
        {
            if (_exporting)
                return;

            _exporting = true;
            TenantConfigClient.Page(BuildPageInput(1, 999999)).Then(outcome =>
            {
                _exporting = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "导出系统参数失败。";
                    return;
                }

                _error = null;
                DownloadCsv(TenantConfigClient.ReadItems(outcome.Data), "全部数据");
            }, _ =>
            {
                _exporting = false;
                _error = "导出系统参数失败，请检查网络后重试。";
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
            if (_items.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("没有数据可以打印");
                return;
            }

            if (!TenantConfigBrowserActions.PrintHtml(BuildPrintHtml(_items)))
            {
                ElementPlus.ElMessageMembers.Error("无法打开打印窗口，请检查浏览器弹窗拦截设置");
            }
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

        private TenantConfigView? Find(string id)
        {
            for (var index = 0; index < _items.Length; index++)
                if (_items[index].Id == id)
                    return _items[index];
            return null;
        }

        private static string BuildCsv(TenantConfigView[] source)
        {
            var builder = new StringBuilder();
            builder.Append("\"配置名称\",\"配置编码\",\"属性值\",\"内置参数\",\"分组编码\",\"排序\",\"创建者\",\"创建时间\",\"修改者\",\"修改时间\",\"备注\"\r\n");
            for (var index = 0; index < source.Length; index++)
            {
                var row = source[index];
                builder.Append(CsvCell(row.Name));
                builder.Append(",");
                builder.Append(CsvCell(row.Code));
                builder.Append(",");
                builder.Append(CsvCell(row.Value));
                builder.Append(",");
                builder.Append(CsvCell(row.SysFlag == 1 ? "是" : "否"));
                builder.Append(",");
                builder.Append(CsvCell(row.GroupCode));
                builder.Append(",");
                builder.Append(CsvCell(row.OrderNo.ToString()));
                builder.Append(",");
                builder.Append(CsvCell(row.CreateUserName));
                builder.Append(",");
                builder.Append(CsvCell(row.CreateTime));
                builder.Append(",");
                builder.Append(CsvCell(row.UpdateUserName));
                builder.Append(",");
                builder.Append(CsvCell(row.UpdateTime));
                builder.Append(",");
                builder.Append(CsvCell(row.Remark));
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
                builder.Append(EscapeHtml(row.Name));
                builder.Append("</td><td>");
                builder.Append(EscapeHtml(row.Code));
                builder.Append("</td><td>");
                builder.Append(EscapeHtml(row.Value));
                builder.Append("</td><td>");
                builder.Append(row.SysFlag == 1 ? "是" : "否");
                builder.Append("</td><td>");
                builder.Append(EscapeHtml(row.GroupCode));
                builder.Append("</td><td>");
                builder.Append(row.OrderNo.ToString());
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
            var result = new TenantConfigTableRow[_items.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var row = _items[index];
                result[index] = new TenantConfigTableRow(
                    row.Id,
                    row.Name,
                    row.Code,
                    row.Value,
                    row.GroupCode,
                    row.OrderNo,
                    row.SysFlag,
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

        public TenantConfigPageProps CreateProps() => new()
        {
            Rows = _rows,
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
            Exporting = _exporting,
            ExportPage = ExportPage,
            ExportCommand = ExportCommand,
            SelectedCount = _selectedItems.Length,
            ItemCount = _items.Length,
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
            PrintPage = PrintPage,
            Save = Save,
            DismissError = () => _error = null,
            BatchDelete = BatchDelete,
            ChangePage = value => ChangePage(value),
            ChangePageSize = value => ChangePageSize(value),
            Editing = _editId.Length > 0,
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
