using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.Lang;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;

namespace Jero.Web.Pages.Lang;

[ECMAScriptModule("components/lang-page-state.js")]
public static class LangPageModule
{
    public static LangPageState Create() => new();

    public sealed class LangPageState : global::Jero.Web.Composition.IPageState<LangPageProps>
    {
        private LangView[] _items = [];
        private LangEnumOption[] _directionOptions = [];
        private LangEnumOption[] _weekOptions = [];
        private int _currentPage = 1;
        private int _pageSize = 20;
        private int _total;
        private string _keyword = string.Empty;
        private string _name = string.Empty;
        private string _queryCode = string.Empty;
        private string _isoCode = string.Empty;
        private string _urlCode = string.Empty;
        private string _activeFilter = string.Empty;
        private string _field = "active";
        private string _order = "descending";
        private bool _showAdvanced;
        private bool _loading;
        private bool _optionsLoading;
        private int _pendingOptionRequests;
        private string? _error;

        private bool _editDialogVisible;
        private string _editDialogTitle = "新增语言";
        private bool _saving;
        private string _editId = string.Empty;
        private string _editName = string.Empty;
        private string _editCode = string.Empty;
        private string _editIsoCode = string.Empty;
        private string _editUrlCode = string.Empty;
        private int _editDirection = 1;
        private string _editDateFormat = string.Empty;
        private string _editTimeFormat = string.Empty;
        private int _editWeekStart = 7;
        private string _editGrouping = string.Empty;
        private string _editDecimalPoint = string.Empty;
        private string _editThousandsSep = string.Empty;
        private int _editActive = 2;

        private bool _auditDialogVisible;
        private string _auditDialogTitle = "修改记录";
        private string _auditCreateUser = "-";
        private string _auditCreateTime = "-";
        private string _auditUpdateUser = "-";
        private string _auditUpdateTime = "-";

        private bool CanAdd => ApplicationState.HasPermission("sysLang:add");
        private bool CanEdit => ApplicationState.HasPermission("sysLang:update");
        private bool CanDelete => ApplicationState.HasPermission("sysLang:delete");

        public void Initialize()
        {
            LoadOptions();
            LoadPage();
        }

        private void LoadOptions()
        {
            _optionsLoading = true;
            _pendingOptionRequests = 2;

            LangClient.DirectionOptions().Then(outcome =>
            {
                if (outcome.Ok)
                    _directionOptions = LangClient.ReadEnumOptions(outcome.Data);
                else
                    _error = outcome.Error ?? "加载书写方向字典失败。";
                FinishOptionRequest();
            }, _ =>
            {
                _error = "加载书写方向字典失败，请检查网络后重试。";
                FinishOptionRequest();
            });

            LangClient.WeekOptions().Then(outcome =>
            {
                if (outcome.Ok)
                    _weekOptions = LangClient.ReadEnumOptions(outcome.Data);
                else
                    _error = outcome.Error ?? "加载每周起始日字典失败。";
                FinishOptionRequest();
            }, _ =>
            {
                _error = "加载每周起始日字典失败，请检查网络后重试。";
                FinishOptionRequest();
            });
        }

        private void FinishOptionRequest()
        {
            _pendingOptionRequests--;
            if (_pendingOptionRequests <= 0)
                _optionsLoading = false;
        }

        private void LoadPage()
        {
            _loading = true;
            LangClient.Page(new LangPageInput
            {
                Page = _currentPage,
                PageSize = _pageSize,
                Field = _field,
                Order = _order,
                DescStr = "descending",
                Keyword = _keyword.Trim(),
                Name = _name.Trim(),
                Code = _queryCode.Trim(),
                IsoCode = _isoCode.Trim(),
                UrlCode = _urlCode.Trim(),
                Active = ActiveFilterValue()
            }).Then(outcome =>
            {
                _loading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载语言失败。";
                    _items = [];
                    _total = 0;
                    return;
                }

                _items = LangClient.ReadItems(outcome.Data);
                _total = LangClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                _loading = false;
                _error = "加载语言失败，请检查网络后重试。";
            });
        }

        private bool? ActiveFilterValue()
        {
            if (_activeFilter == "true")
                return true;
            if (_activeFilter == "false")
                return false;
            return null;
        }

        private void Search()
        {
            _currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            _keyword = string.Empty;
            _name = string.Empty;
            _queryCode = string.Empty;
            _isoCode = string.Empty;
            _urlCode = string.Empty;
            _activeFilter = string.Empty;
            _currentPage = 1;
            LoadPage();
        }

        private void Refresh()
        {
            LoadOptions();
            LoadPage();
        }

        private void ActiveFilterChanged(string? value)
            => _activeFilter = value ?? string.Empty;

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

        private void Sort(string property, string direction)
        {
            _field = string.IsNullOrWhiteSpace(property) ? "active" : property;
            _order = direction == "descending" ? "descending" : "ascending";
            LoadPage();
        }

        private void OpenCreate()
        {
            _editDialogTitle = "新增语言";
            _editId = string.Empty;
            _editName = string.Empty;
            _editCode = string.Empty;
            _editIsoCode = string.Empty;
            _editUrlCode = string.Empty;
            _editDirection = 1;
            _editDateFormat = string.Empty;
            _editTimeFormat = string.Empty;
            _editWeekStart = 7;
            _editGrouping = string.Empty;
            _editDecimalPoint = string.Empty;
            _editThousandsSep = string.Empty;
            _editActive = 2;
            _editDialogVisible = true;
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit)
                return;

            _editDialogTitle = "编辑语言";
            _editId = row.Id;
            _editName = row.Name;
            _editCode = row.Code;
            _editIsoCode = row.IsoCode;
            _editUrlCode = row.UrlCode;
            _editDirection = row.Direction;
            _editDateFormat = row.DateFormat;
            _editTimeFormat = row.TimeFormat;
            _editWeekStart = row.WeekStart;
            _editGrouping = row.Grouping;
            _editDecimalPoint = row.DecimalPoint;
            _editThousandsSep = row.ThousandsSep;
            _editActive = row.Active ? 1 : 2;
            _editDialogVisible = true;
        }

        private void Save()
        {
            if (_saving)
                return;

            var normalizedName = _editName.Trim();
            var normalizedCode = _editCode.Trim();
            var normalizedIsoCode = _editIsoCode.Trim();
            var normalizedUrlCode = _editUrlCode.Trim();
            var normalizedDateFormat = _editDateFormat.Trim();
            var normalizedTimeFormat = _editTimeFormat.Trim();
            var normalizedGrouping = _editGrouping.Trim();
            var normalizedDecimalPoint = _editDecimalPoint.Trim();
            var normalizedThousandsSep = _editThousandsSep.Trim();

            if (!ValidateRequired(normalizedName, "语言名称") ||
                !ValidateRequired(normalizedCode, "语言代码") ||
                !ValidateRequired(normalizedIsoCode, "ISO 语言代码") ||
                !ValidateRequired(normalizedUrlCode, "URL 语言代码") ||
                !ValidateRequired(normalizedDateFormat, "日期格式") ||
                !ValidateRequired(normalizedTimeFormat, "时间格式") ||
                !ValidateRequired(normalizedGrouping, "分组符号") ||
                !ValidateRequired(normalizedDecimalPoint, "小数点符号"))
            {
                return;
            }

            if (normalizedThousandsSep.Length > 255)
            {
                _error = "千分位分隔符不能超过255个字符。";
                return;
            }
            if (_editDirection != 1 && _editDirection != 2)
            {
                _error = "请选择书写方向。";
                return;
            }
            if (_editWeekStart < 1 || _editWeekStart > 7)
            {
                _error = "请选择每周起始日。";
                return;
            }

            var input = new LangSaveInput
            {
                Id = _editId,
                Name = normalizedName,
                Code = normalizedCode,
                IsoCode = normalizedIsoCode,
                UrlCode = normalizedUrlCode,
                Direction = _editDirection,
                DateFormat = normalizedDateFormat,
                TimeFormat = normalizedTimeFormat,
                WeekStart = _editWeekStart,
                Grouping = normalizedGrouping,
                DecimalPoint = normalizedDecimalPoint,
                ThousandsSep = normalizedThousandsSep.Length == 0 ? null : normalizedThousandsSep,
                Active = _editActive == 1
            };

            _saving = true;
            var request = _editId.Length == 0 ? LangClient.Add(input) : LangClient.Update(input);
            request.Then(outcome =>
            {
                _saving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存语言失败。";
                    return;
                }

                _editDialogVisible = false;
                _error = null;
                ElementPlus.ElMessageMembers.Success(_editId.Length == 0 ? "语言已新增" : "语言已更新");
                LoadPage();
            }, _ =>
            {
                _saving = false;
                _error = "保存语言失败，请检查网络后重试。";
            });
        }

        private bool ValidateRequired(string value, string label)
        {
            if (value.Length == 0)
            {
                _error = label + "不能为空。";
                return false;
            }
            if (value.Length > 255)
            {
                _error = label + "不能超过255个字符。";
                return false;
            }
            return true;
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete)
                return;

            LangClient.Delete(row.Id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除语言失败。";
                    return;
                }

                _error = null;
                ElementPlus.ElMessageMembers.Success("语言已删除");
                LoadPage();
            }, _ => _error = "删除语言失败，请检查网络后重试。");
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

        private LangView? Find(string id)
        {
            for (var index = 0; index < _items.Length; index++)
                if (_items[index].Id == id)
                    return _items[index];
            return null;
        }

        private static readonly ConfigurationOption[] _activeOptions = [new("true", "启用"), new("false", "停用")];

        private LangTableRow[] TableRows()
        {
            var result = new LangTableRow[_items.Length];
            for (var index = 0; index < _items.Length; index++)
            {
                var row = _items[index];
                var directionLabel = OptionLabel(_directionOptions, row.Direction, row.Direction == 2 ? "从右到左" : "从左到右");
                var weekLabel = OptionLabel(_weekOptions, row.WeekStart, row.WeekStart == 7 ? "周日" : "周" + row.WeekStart);
                result[index] = new LangTableRow(
                    row.Id, row.Name, row.Code, row.IsoCode, row.UrlCode, row.Direction, directionLabel,
                    row.DateFormat, row.TimeFormat, weekLabel, row.Grouping, row.DecimalPoint,
                    row.ThousandsSepDisplay, row.Active);
            }
            return result;
        }

        private static LangNumberOption[] NumberOptions(LangEnumOption[] options)
        {
            var result = new LangNumberOption[options.Length];
            for (var index = 0; index < options.Length; index++)
                result[index] = new LangNumberOption(options[index].Value, options[index].Label);
            return result;
        }

        private static string OptionLabel(LangEnumOption[] options, int value, string fallback)
        {
            for (var index = 0; index < options.Length; index++)
                if (options[index].Value == value)
                    return options[index].Label;
            return fallback;
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

        public LangPageProps CreateProps() => new()
        {
            CanAdd = CanAdd,
            CanEdit = CanEdit,
            CanDelete = CanDelete,
            ShowAdvanced = _showAdvanced,
            Loading = _loading,
            OptionsLoading = _optionsLoading,
            EditDialogVisible = _editDialogVisible,
            AuditDialogVisible = _auditDialogVisible,
            Saving = _saving,
            CurrentPage = _currentPage,
            PageSize = _pageSize,
            Total = _total,
            EditDirection = _editDirection,
            EditWeekStart = _editWeekStart,
            EditActive = _editActive,
            Keyword = _keyword,
            Name = _name,
            QueryCode = _queryCode,
            IsoCode = _isoCode,
            UrlCode = _urlCode,
            ActiveFilter = _activeFilter,
            EditDialogTitle = _editDialogTitle,
            EditName = _editName,
            EditCode = _editCode,
            EditIsoCode = _editIsoCode,
            EditUrlCode = _editUrlCode,
            EditDateFormat = _editDateFormat,
            EditTimeFormat = _editTimeFormat,
            EditGrouping = _editGrouping,
            EditDecimalPoint = _editDecimalPoint,
            EditThousandsSep = _editThousandsSep,
            AuditDialogTitle = _auditDialogTitle,
            AuditCreateUser = _auditCreateUser,
            AuditCreateTime = _auditCreateTime,
            AuditUpdateUser = _auditUpdateUser,
            AuditUpdateTime = _auditUpdateTime,
            Error = _error,
            SetEditDialogVisible = value => _editDialogVisible = value,
            SetAuditDialogVisible = value => _auditDialogVisible = value,
            SetKeyword = value => _keyword = value ?? "",
            SetName = value => _name = value ?? "",
            SetQueryCode = value => _queryCode = value ?? "",
            SetIsoCode = value => _isoCode = value ?? "",
            SetUrlCode = value => _urlCode = value ?? "",
            SetEditName = value => _editName = value ?? "",
            SetEditCode = value => _editCode = value ?? "",
            SetEditIsoCode = value => _editIsoCode = value ?? "",
            SetEditUrlCode = value => _editUrlCode = value ?? "",
            SetEditDateFormat = value => _editDateFormat = value ?? "",
            SetEditTimeFormat = value => _editTimeFormat = value ?? "",
            SetEditGrouping = value => _editGrouping = value ?? "",
            SetEditDecimalPoint = value => _editDecimalPoint = value ?? "",
            SetEditThousandsSep = value => _editThousandsSep = value ?? "",
            Rows = TableRows(),
            ActiveOptions = _activeOptions,
            DirectionOptions = NumberOptions(_directionOptions),
            WeekOptions = NumberOptions(_weekOptions),
            Search = Search,
            ResetSearch = ResetSearch,
            Refresh = Refresh,
            OpenCreate = OpenCreate,
            Save = Save,
            OpenAudit = OpenAudit,
            OpenEdit = OpenEdit,
            Delete = Delete,
            ActiveFilterChanged = ActiveFilterChanged,
            DismissError = () => _error = null,
            CancelEdit = () => _editDialogVisible = false,
            ToggleAdvanced = () => _showAdvanced = !_showAdvanced,
            SetEditActive = value => _editActive = (int)value,
            SetEditDirection = value => _editDirection = (int)value,
            SetEditWeekStart = value => _editWeekStart = (int)value,
            ChangePage = ChangePage,
            ChangePageSize = ChangePageSize,
            Sort = Sort,
        };
    }
}

public sealed record LangPageProps : VueProps
{
    public required bool CanAdd { get; init; }
    public required bool CanEdit { get; init; }
    public required bool CanDelete { get; init; }
    public required bool ShowAdvanced { get; init; }
    public required bool Loading { get; init; }
    public required bool OptionsLoading { get; init; }
    public required bool EditDialogVisible { get; init; }
    public required bool AuditDialogVisible { get; init; }
    public required bool Saving { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public required int EditDirection { get; init; }
    public required int EditWeekStart { get; init; }
    public required int EditActive { get; init; }
    public required string Keyword { get; init; }
    public required string Name { get; init; }
    public required string QueryCode { get; init; }
    public required string IsoCode { get; init; }
    public required string UrlCode { get; init; }
    public required string ActiveFilter { get; init; }
    public required string EditDialogTitle { get; init; }
    public required string EditName { get; init; }
    public required string EditCode { get; init; }
    public required string EditIsoCode { get; init; }
    public required string EditUrlCode { get; init; }
    public required string EditDateFormat { get; init; }
    public required string EditTimeFormat { get; init; }
    public required string EditGrouping { get; init; }
    public required string EditDecimalPoint { get; init; }
    public required string EditThousandsSep { get; init; }
    public required string AuditDialogTitle { get; init; }
    public required string AuditCreateUser { get; init; }
    public required string AuditCreateTime { get; init; }
    public required string AuditUpdateUser { get; init; }
    public required string AuditUpdateTime { get; init; }
    public required string? Error { get; init; }
    public required Action<bool> SetEditDialogVisible { get; init; }
    public required Action<bool> SetAuditDialogVisible { get; init; }
    public required Action<string?> SetKeyword { get; init; }
    public required Action<string?> SetName { get; init; }
    public required Action<string?> SetQueryCode { get; init; }
    public required Action<string?> SetIsoCode { get; init; }
    public required Action<string?> SetUrlCode { get; init; }
    public required Action<string?> SetEditName { get; init; }
    public required Action<string?> SetEditCode { get; init; }
    public required Action<string?> SetEditIsoCode { get; init; }
    public required Action<string?> SetEditUrlCode { get; init; }
    public required Action<string?> SetEditDateFormat { get; init; }
    public required Action<string?> SetEditTimeFormat { get; init; }
    public required Action<string?> SetEditGrouping { get; init; }
    public required Action<string?> SetEditDecimalPoint { get; init; }
    public required Action<string?> SetEditThousandsSep { get; init; }
    public required LangTableRow[] Rows { get; init; }
    public required ConfigurationOption[] ActiveOptions { get; init; }
    public required LangNumberOption[] DirectionOptions { get; init; }
    public required LangNumberOption[] WeekOptions { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action Refresh { get; init; }
    public required Action OpenCreate { get; init; }
    public required Action Save { get; init; }
    public required Action<string> OpenAudit { get; init; }
    public required Action<string> OpenEdit { get; init; }
    public required Action<string> Delete { get; init; }
    public required Action<string> ActiveFilterChanged { get; init; }
    public required Action DismissError { get; init; }
    public required Action CancelEdit { get; init; }
    public required Action ToggleAdvanced { get; init; }
    public required Action<double> SetEditActive { get; init; }
    public required Action<double> SetEditDirection { get; init; }
    public required Action<double> SetEditWeekStart { get; init; }
    public required Action<double> ChangePage { get; init; }
    public required Action<double> ChangePageSize { get; init; }
    public required Action<string, string> Sort { get; init; }
}
