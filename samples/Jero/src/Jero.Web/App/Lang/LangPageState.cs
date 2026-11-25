using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using static ECMAScript.Global;

namespace Jero.Web.App.Lang;

[ECMAScriptModule("components/lang-page-state.js")]
public static class LangPageModule
{
    public static LangPageState Create() => new();

    public sealed class LangPageState : global::Jero.Web.App.IPageState<LangPageProps>
    {
        private LangView[] items = [];
        private LangEnumOption[] directionOptions = [];
        private LangEnumOption[] weekOptions = [];
        private int currentPage = 1;
        private int pageSize = 20;
        private int total;
        private string keyword = string.Empty;
        private string name = string.Empty;
        private string queryCode = string.Empty;
        private string isoCode = string.Empty;
        private string urlCode = string.Empty;
        private string activeFilter = string.Empty;
        private string field = "active";
        private string order = "descending";
        private bool showAdvanced;
        private bool loading;
        private bool optionsLoading;
        private int pendingOptionRequests;
        private string? error;

        private bool editDialogVisible;
        private string editDialogTitle = "新增语言";
        private bool saving;
        private string editId = string.Empty;
        private string editName = string.Empty;
        private string editCode = string.Empty;
        private string editIsoCode = string.Empty;
        private string editUrlCode = string.Empty;
        private int editDirection = 1;
        private string editDateFormat = string.Empty;
        private string editTimeFormat = string.Empty;
        private int editWeekStart = 7;
        private string editGrouping = string.Empty;
        private string editDecimalPoint = string.Empty;
        private string editThousandsSep = string.Empty;
        private int editActive = 2;

        private bool auditDialogVisible;
        private string auditDialogTitle = "修改记录";
        private string auditCreateUser = "-";
        private string auditCreateTime = "-";
        private string auditUpdateUser = "-";
        private string auditUpdateTime = "-";

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
            optionsLoading = true;
            pendingOptionRequests = 2;

            LangClient.DirectionOptions().Then(outcome =>
            {
                if (outcome.Ok)
                    directionOptions = LangClient.ReadEnumOptions(outcome.Data);
                else
                    error = outcome.Error ?? "加载书写方向字典失败。";
                FinishOptionRequest();
            }, _ =>
            {
                error = "加载书写方向字典失败，请检查网络后重试。";
                FinishOptionRequest();
            });

            LangClient.WeekOptions().Then(outcome =>
            {
                if (outcome.Ok)
                    weekOptions = LangClient.ReadEnumOptions(outcome.Data);
                else
                    error = outcome.Error ?? "加载每周起始日字典失败。";
                FinishOptionRequest();
            }, _ =>
            {
                error = "加载每周起始日字典失败，请检查网络后重试。";
                FinishOptionRequest();
            });
        }

        private void FinishOptionRequest()
        {
            pendingOptionRequests--;
            if (pendingOptionRequests <= 0)
                optionsLoading = false;
        }

        private void LoadPage()
        {
            loading = true;
            LangClient.Page(new LangPageInput
            {
                page = currentPage,
                pageSize = pageSize,
                field = field,
                order = order,
                descStr = "descending",
                keyword = keyword.Trim(),
                name = name.Trim(),
                code = queryCode.Trim(),
                isoCode = isoCode.Trim(),
                urlCode = urlCode.Trim(),
                active = ActiveFilterValue()
            }).Then(outcome =>
            {
                loading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载语言失败。";
                    items = [];
                    total = 0;
                    return;
                }

                items = LangClient.ReadItems(outcome.Data);
                total = LangClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                loading = false;
                error = "加载语言失败，请检查网络后重试。";
            });
        }

        private bool? ActiveFilterValue()
        {
            if (activeFilter == "true")
                return true;
            if (activeFilter == "false")
                return false;
            return null;
        }

        private void Search()
        {
            currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            keyword = string.Empty;
            name = string.Empty;
            queryCode = string.Empty;
            isoCode = string.Empty;
            urlCode = string.Empty;
            activeFilter = string.Empty;
            currentPage = 1;
            LoadPage();
        }

        private void Refresh()
        {
            LoadOptions();
            LoadPage();
        }

        private void ActiveFilterChanged(string? value)
            => activeFilter = value ?? string.Empty;

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

        private void Sort(string property, string direction)
        {
            field = string.IsNullOrWhiteSpace(property) ? "active" : property;
            order = direction == "descending" ? "descending" : "ascending";
            LoadPage();
        }

        private void OpenCreate()
        {
            editDialogTitle = "新增语言";
            editId = string.Empty;
            editName = string.Empty;
            editCode = string.Empty;
            editIsoCode = string.Empty;
            editUrlCode = string.Empty;
            editDirection = 1;
            editDateFormat = string.Empty;
            editTimeFormat = string.Empty;
            editWeekStart = 7;
            editGrouping = string.Empty;
            editDecimalPoint = string.Empty;
            editThousandsSep = string.Empty;
            editActive = 2;
            editDialogVisible = true;
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit)
                return;

            editDialogTitle = "编辑语言";
            editId = row.id;
            editName = row.name;
            editCode = row.code;
            editIsoCode = row.isoCode;
            editUrlCode = row.urlCode;
            editDirection = row.direction;
            editDateFormat = row.dateFormat;
            editTimeFormat = row.timeFormat;
            editWeekStart = row.weekStart;
            editGrouping = row.grouping;
            editDecimalPoint = row.decimalPoint;
            editThousandsSep = row.thousandsSep;
            editActive = row.active ? 1 : 2;
            editDialogVisible = true;
        }

        private void Save()
        {
            if (saving)
                return;

            var normalizedName = editName.Trim();
            var normalizedCode = editCode.Trim();
            var normalizedIsoCode = editIsoCode.Trim();
            var normalizedUrlCode = editUrlCode.Trim();
            var normalizedDateFormat = editDateFormat.Trim();
            var normalizedTimeFormat = editTimeFormat.Trim();
            var normalizedGrouping = editGrouping.Trim();
            var normalizedDecimalPoint = editDecimalPoint.Trim();
            var normalizedThousandsSep = editThousandsSep.Trim();

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
                error = "千分位分隔符不能超过255个字符。";
                return;
            }
            if (editDirection != 1 && editDirection != 2)
            {
                error = "请选择书写方向。";
                return;
            }
            if (editWeekStart < 1 || editWeekStart > 7)
            {
                error = "请选择每周起始日。";
                return;
            }

            var input = new LangSaveInput
            {
                id = editId,
                name = normalizedName,
                code = normalizedCode,
                isoCode = normalizedIsoCode,
                urlCode = normalizedUrlCode,
                direction = editDirection,
                dateFormat = normalizedDateFormat,
                timeFormat = normalizedTimeFormat,
                weekStart = editWeekStart,
                grouping = normalizedGrouping,
                decimalPoint = normalizedDecimalPoint,
                thousandsSep = normalizedThousandsSep.Length == 0 ? null : normalizedThousandsSep,
                active = editActive == 1
            };

            saving = true;
            var request = editId.Length == 0 ? LangClient.Add(input) : LangClient.Update(input);
            request.Then(outcome =>
            {
                saving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存语言失败。";
                    return;
                }

                editDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success(editId.Length == 0 ? "语言已新增" : "语言已更新");
                LoadPage();
            }, _ =>
            {
                saving = false;
                error = "保存语言失败，请检查网络后重试。";
            });
        }

        private bool ValidateRequired(string value, string label)
        {
            if (value.Length == 0)
            {
                error = label + "不能为空。";
                return false;
            }
            if (value.Length > 255)
            {
                error = label + "不能超过255个字符。";
                return false;
            }
            return true;
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete)
                return;

            LangClient.Delete(row.id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除语言失败。";
                    return;
                }

                error = null;
                ElementPlus.ElMessageMembers.Success("语言已删除");
                LoadPage();
            }, _ => error = "删除语言失败，请检查网络后重试。");
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

        private LangView? Find(string id)
        {
            for (var index = 0; index < items.Length; index++)
                if (items[index].id == id)
                    return items[index];
            return null;
        }

        private static readonly ConfigurationOption[] ActiveOptions = [new("true", "启用"), new("false", "停用")];

        private LangTableRow[] TableRows()
        {
            var result = new LangTableRow[items.Length];
            for (var index = 0; index < items.Length; index++)
            {
                var row = items[index];
                var directionLabel = OptionLabel(directionOptions, row.direction, row.direction == 2 ? "从右到左" : "从左到右");
                var weekLabel = OptionLabel(weekOptions, row.weekStart, row.weekStart == 7 ? "周日" : "周" + row.weekStart);
                result[index] = new LangTableRow(
                    row.id, row.name, row.code, row.isoCode, row.urlCode, row.direction, directionLabel,
                    row.dateFormat, row.timeFormat, weekLabel, row.grouping, row.decimalPoint,
                    row.thousandsSepDisplay, row.active);
            }
            return result;
        }

        private static LangNumberOption[] NumberOptions(LangEnumOption[] options)
        {
            var result = new LangNumberOption[options.Length];
            for (var index = 0; index < options.Length; index++)
                result[index] = new LangNumberOption(options[index].value, options[index].label);
            return result;
        }

        private static string OptionLabel(LangEnumOption[] options, int value, string fallback)
        {
            for (var index = 0; index < options.Length; index++)
                if (options[index].value == value)
                    return options[index].label;
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
            ShowAdvanced = showAdvanced,
            Loading = loading,
            OptionsLoading = optionsLoading,
            EditDialogVisible = editDialogVisible,
            AuditDialogVisible = auditDialogVisible,
            Saving = saving,
            CurrentPage = currentPage,
            PageSize = pageSize,
            Total = total,
            EditDirection = editDirection,
            EditWeekStart = editWeekStart,
            EditActive = editActive,
            Keyword = keyword,
            Name = name,
            QueryCode = queryCode,
            IsoCode = isoCode,
            UrlCode = urlCode,
            ActiveFilter = activeFilter,
            EditDialogTitle = editDialogTitle,
            EditName = editName,
            EditCode = editCode,
            EditIsoCode = editIsoCode,
            EditUrlCode = editUrlCode,
            EditDateFormat = editDateFormat,
            EditTimeFormat = editTimeFormat,
            EditGrouping = editGrouping,
            EditDecimalPoint = editDecimalPoint,
            EditThousandsSep = editThousandsSep,
            AuditDialogTitle = auditDialogTitle,
            AuditCreateUser = auditCreateUser,
            AuditCreateTime = auditCreateTime,
            AuditUpdateUser = auditUpdateUser,
            AuditUpdateTime = auditUpdateTime,
            Error = error,
            SetEditDialogVisible = value => editDialogVisible = value,
            SetAuditDialogVisible = value => auditDialogVisible = value,
            SetKeyword = value => keyword = value ?? "",
            SetName = value => name = value ?? "",
            SetQueryCode = value => queryCode = value ?? "",
            SetIsoCode = value => isoCode = value ?? "",
            SetUrlCode = value => urlCode = value ?? "",
            SetEditName = value => editName = value ?? "",
            SetEditCode = value => editCode = value ?? "",
            SetEditIsoCode = value => editIsoCode = value ?? "",
            SetEditUrlCode = value => editUrlCode = value ?? "",
            SetEditDateFormat = value => editDateFormat = value ?? "",
            SetEditTimeFormat = value => editTimeFormat = value ?? "",
            SetEditGrouping = value => editGrouping = value ?? "",
            SetEditDecimalPoint = value => editDecimalPoint = value ?? "",
            SetEditThousandsSep = value => editThousandsSep = value ?? "",
            Rows = TableRows(),
            ActiveOptions = ActiveOptions,
            DirectionOptions = NumberOptions(directionOptions),
            WeekOptions = NumberOptions(weekOptions),
            Search = Search,
            ResetSearch = ResetSearch,
            Refresh = Refresh,
            OpenCreate = OpenCreate,
            Save = Save,
            OpenAudit = OpenAudit,
            OpenEdit = OpenEdit,
            Delete = Delete,
            ActiveFilterChanged = ActiveFilterChanged,
            DismissError = () => error = null,
            CancelEdit = () => editDialogVisible = false,
            ToggleAdvanced = () => showAdvanced = !showAdvanced,
            SetEditActive = value => editActive = (int)value,
            SetEditDirection = value => editDirection = (int)value,
            SetEditWeekStart = value => editWeekStart = (int)value,
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
