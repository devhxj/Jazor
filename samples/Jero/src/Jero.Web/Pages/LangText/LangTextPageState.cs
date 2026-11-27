using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.LangText;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;

namespace Jero.Web.Pages.LangText;

[ECMAScriptModule("components/lang-text-page-state.js")]
public static class LangTextPageModule
{
    public static LangTextPageState Create() => new();

    public sealed class LangTextPageState : global::Jero.Web.Composition.IPageState<LangTextPageProps>
    {
        private FileRef? _importFile;

        private const string ImportFileInputId = "lang-text-import-file";

        private LangTextView[] _items = [];
        private LangTextView[] _selectedItems = [];
        private int _currentPage = 1;
        private int _pageSize = 20;
        private int _total;
        private string _keyword = string.Empty;
        private string _entityName = string.Empty;
        private string _entityId = string.Empty;
        private string _fieldName = string.Empty;
        private string _langCode = string.Empty;
        private string _content = string.Empty;
        private string _field = "createTime";
        private string _order = "descending";
        private bool _showAdvanced;
        private bool _loading;
        private bool _exporting;
        private string? _error;

        private bool _editDialogVisible;
        private string _editDialogTitle = "新增翻译";
        private bool _formLoading;
        private bool _saving;
        private string _editId = string.Empty;
        private string _editEntityName = string.Empty;
        private string _editEntityId = string.Empty;
        private string _editFieldName = string.Empty;
        private string _editLangCode = string.Empty;
        private string _editContent = string.Empty;

        private bool _importDialogVisible;
        private bool _importing;
        private bool _templateDownloading;
        private bool _importCompleted;
        private bool _importHasErrorFile;
        private string _importResultFileName = string.Empty;

        private bool _auditDialogVisible;
        private string _auditDialogTitle = "修改记录";
        private string _auditCreateUser = "-";
        private string _auditCreateTime = "-";
        private string _auditUpdateUser = "-";
        private string _auditUpdateTime = "-";

        private bool CanAdd => ApplicationState.HasPermission("sysLangText:add");
        private bool CanEdit => ApplicationState.HasPermission("sysLangText:update");
        private bool CanDelete => ApplicationState.HasPermission("sysLangText:delete");
        private bool CanBatchDelete => ApplicationState.HasPermission("sysLangText:batchDelete");
        private bool CanExport => ApplicationState.HasPermission("sysLangText:export");
        private bool CanImport => ApplicationState.HasPermission("sysLangText:import");

        public void Initialize()
            => LoadPage();

        private LangTextPageInput BuildPageInput(int page, int size)
            => BuildPageInput(page, size, []);

        private LangTextPageInput BuildPageInput(int page, int size, string[] selectedIds)
            => new()
            {
                Page = page,
                PageSize = size,
                Field = _field,
                Order = _order,
                DescStr = "descending",
                Keyword = _keyword.Trim(),
                EntityName = _entityName.Trim(),
                EntityId = _entityId.Trim().Length == 0 ? null : _entityId.Trim(),
                FieldName = _fieldName.Trim(),
                LangCode = _langCode.Trim(),
                Content = _content.Trim(),
                SelectKeyList = selectedIds
            };

        private void LoadPage()
        {
            _loading = true;
            LangTextClient.Page(BuildPageInput(_currentPage, _pageSize)).Then(outcome =>
            {
                _loading = false;
                _selectedItems = [];
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载翻译文本失败。";
                    _items = [];
                    _total = 0;
                    return;
                }

                _items = LangTextClient.ReadItems(outcome.Data);
                _total = LangTextClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                _loading = false;
                _error = "加载翻译文本失败，请检查网络后重试。";
            });
        }

        private void Search()
        {
            var queryEntityId = _entityId.Trim();
            if (queryEntityId.Length > 0 && !IsPositiveInteger(queryEntityId))
            {
                _error = "所属实体 ID 必须为正整数。";
                return;
            }

            _currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            _keyword = string.Empty;
            _entityName = string.Empty;
            _entityId = string.Empty;
            _fieldName = string.Empty;
            _langCode = string.Empty;
            _content = string.Empty;
            _currentPage = 1;
            LoadPage();
        }

        private void Refresh()
            => LoadPage();

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
            _field = string.IsNullOrWhiteSpace(property) ? "createTime" : property;
            _order = direction == "descending" ? "descending" : "ascending";
            LoadPage();
        }

        private void SelectionChanged(string[] selected)
        {
            var result = new LangTextView[selected.Length];
            var targetIndex = 0;
            for (var index = 0; index < selected.Length; index++)
            {
                var item = Find(selected[index]);
                if (item is null)
                    continue;
                result[targetIndex] = item;
                targetIndex++;
            }

            if (targetIndex == selected.Length)
            {
                _selectedItems = result;
                return;
            }

            var compact = new LangTextView[targetIndex];
            for (var index = 0; index < targetIndex; index++)
                compact[index] = result[index];
            _selectedItems = compact;
        }

        private void OpenCreate()
        {
            _editDialogTitle = "新增翻译";
            _editId = string.Empty;
            _editEntityName = string.Empty;
            _editEntityId = string.Empty;
            _editFieldName = string.Empty;
            _editLangCode = string.Empty;
            _editContent = string.Empty;
            _formLoading = false;
            _editDialogVisible = true;
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit)
                return;

            _editDialogTitle = "编辑翻译";
            _editId = row.Id;
            _editEntityName = row.EntityName;
            _editEntityId = row.EntityId;
            _editFieldName = row.FieldName;
            _editLangCode = row.LangCode;
            _editContent = row.Content;
            _editDialogVisible = true;
            _formLoading = true;

            LangTextClient.Detail(row.Id).Then(outcome =>
            {
                _formLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载翻译详情失败。";
                    return;
                }

                var detail = LangTextClient.ReadDetail(outcome.Data);
                if (detail is null)
                {
                    _error = "翻译详情不存在。";
                    return;
                }

                _editId = detail.Id;
                _editEntityName = detail.EntityName;
                _editEntityId = detail.EntityId;
                _editFieldName = detail.FieldName;
                _editLangCode = detail.LangCode;
                _editContent = detail.Content;
                _error = null;
            }, _ =>
            {
                _formLoading = false;
                _error = "加载翻译详情失败，请检查网络后重试。";
            });
        }

        private void Save()
        {
            if (_saving || _formLoading)
                return;

            var normalizedEntityName = _editEntityName.Trim();
            var normalizedEntityId = _editEntityId.Trim();
            var normalizedFieldName = _editFieldName.Trim();
            var normalizedLangCode = _editLangCode.Trim();
            var normalizedContent = _editContent.Trim();

            if (!ValidateRequired(normalizedEntityName, "所属实体名", 255) ||
                !ValidateRequired(normalizedFieldName, "字段名", 255) ||
                !ValidateRequired(normalizedLangCode, "语言代码", 255) ||
                !ValidateRequired(normalizedContent, "翻译内容", 255))
            {
                return;
            }
            if (!IsPositiveInteger(normalizedEntityId))
            {
                _error = "所属实体 ID 必须为正整数。";
                return;
            }

            var input = new LangTextSaveInput
            {
                Id = _editId,
                EntityName = normalizedEntityName,
                EntityId = normalizedEntityId,
                FieldName = normalizedFieldName,
                LangCode = normalizedLangCode,
                Content = normalizedContent
            };

            _saving = true;
            var request = _editId.Length == 0 ? LangTextClient.Add(input) : LangTextClient.Update(input);
            request.Then(outcome =>
            {
                _saving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存翻译失败。";
                    return;
                }

                _editDialogVisible = false;
                _error = null;
                ElementPlus.ElMessageMembers.Success(_editId.Length == 0 ? "翻译已新增" : "翻译已更新");
                LoadPage();
            }, _ =>
            {
                _saving = false;
                _error = "保存翻译失败，请检查网络后重试。";
            });
        }

        private bool ValidateRequired(string value, string label, int maxLength)
        {
            if (value.Length == 0)
            {
                _error = label + "不能为空。";
                return false;
            }
            if (value.Length > maxLength)
            {
                _error = label + "不能超过" + maxLength + "个字符。";
                return false;
            }
            return true;
        }

        private static bool IsPositiveInteger(string value)
            => new RegExp("^[1-9][0-9]*$").Test(value);

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete)
                return;

            LangTextClient.Delete(row.Id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除翻译失败。";
                    return;
                }

                _error = null;
                ElementPlus.ElMessageMembers.Success("翻译已删除");
                LoadPage();
            }, _ => _error = "删除翻译失败，请检查网络后重试。");
        }

        private void BatchDelete()
        {
            if (!CanBatchDelete || _selectedItems.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请选择要删除的翻译记录");
                return;
            }

            var ids = SelectedIds();
            LangTextClient.BatchDelete(ids).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "批量删除翻译失败。";
                    return;
                }

                _selectedItems = [];
                _currentPage = 1;
                _error = null;
                ElementPlus.ElMessageMembers.Success("翻译已批量删除");
                LoadPage();
            }, _ => _error = "批量删除翻译失败，请检查网络后重试。");
        }

        private void ExportPage()
            => BeginExport(BuildPageInput(_currentPage, _pageSize), "翻译表_本页数据.xlsx");

        private void ExportCommand(string command)
        {
            if (command == "select")
            {
                if (_selectedItems.Length == 0)
                {
                    ElementPlus.ElMessageMembers.Warning("请先选择要导出的翻译记录");
                    return;
                }
                BeginExport(BuildPageInput(_currentPage, _pageSize, SelectedIds()), "翻译表_选中记录.xlsx");
                return;
            }

            if (command == "all")
            {
                BeginExport(BuildPageInput(1, 999999), "翻译表_全部数据.xlsx");
                return;
            }

            ExportPage();
        }

        private void BeginExport(LangTextPageInput input, string fallbackFileName)
        {
            if (_exporting)
                return;

            _exporting = true;
            LangTextBrowserActions.DownloadExport(input, fallbackFileName).Then(outcome =>
            {
                _exporting = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "导出翻译失败。";
                    return;
                }
                _error = null;
            }, _ =>
            {
                _exporting = false;
                _error = "导出翻译失败，请检查网络后重试。";
            });
        }

        private void OpenImport()
        {
            ResetImportState();
            _importDialogVisible = true;
        }

        private void ImportDialogChanged(bool value)
        {
            _importDialogVisible = value == true;
            if (!_importDialogVisible)
                ResetImportState();
        }

        private void CloseImport()
        {
            _importDialogVisible = false;
            ResetImportState();
        }

        private void ResetImportState()
        {
            _importing = false;
            _importCompleted = false;
            _importHasErrorFile = false;
            _importResultFileName = string.Empty;
            _importFile = null;
            LangTextBrowserActions.ClearSelectedFile(ImportFileInputId);
            LangTextBrowserActions.ResetImportResult();
        }

        private void DownloadTemplate()
        {
            if (_templateDownloading)
                return;

            _templateDownloading = true;
            LangTextBrowserActions.DownloadTemplate("翻译表导入模板.xlsx").Then(outcome =>
            {
                _templateDownloading = false;
                if (!outcome.Ok)
                    _error = outcome.Error ?? "下载导入模板失败。";
                else
                    _error = null;
            }, _ =>
            {
                _templateDownloading = false;
                _error = "下载导入模板失败，请检查网络后重试。";
            });
        }

        private void ImportPrimaryAction()
        {
            if (_importCompleted)
            {
                CloseImport();
                return;
            }

            SubmitImport();
        }

        private void SelectImportFile()
        {
            // 在事件回调中取出原生 File，再开始异步操作。
            _importFile = LangTextBrowserActions.GetSelectedFile(ImportFileInputId);
        }

        private void SubmitImport()
        {
            if (_importing)
                return;
            if (_importFile is null)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择要导入的文件");
                return;
            }

            _importing = true;
            LangTextBrowserActions.ImportSelectedFile(_importFile).Then(outcome =>
            {
                _importing = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "导入翻译失败。";
                    return;
                }

                _error = null;
                LoadPage();
                if (!outcome.HasErrorFile)
                {
                    ElementPlus.ElMessageMembers.Success(outcome.Message.Length == 0 ? "导入成功" : outcome.Message);
                    CloseImport();
                    return;
                }

                _importCompleted = true;
                _importHasErrorFile = true;
                _importResultFileName = outcome.FileName;
                ElementPlus.ElMessageMembers.Warning("导入完成，但存在错误记录，请下载导入结果查看");
            }, _ =>
            {
                _importing = false;
                _error = "导入翻译失败，请检查网络后重试。";
            });
        }

        private static void DownloadImportResult()
            => LangTextBrowserActions.DownloadImportResult();

        private void OpenAudit(string id)
        {
            var row = Find(id);
            if (row is null)
                return;

            _auditDialogTitle = "修改记录 · " + row.EntityName + "/" + row.FieldName;
            _auditCreateUser = DisplayUser(row.CreateUserName, row.CreateUserId);
            _auditCreateTime = DisplayValue(row.CreateTime);
            _auditUpdateUser = DisplayUser(row.UpdateUserName, row.UpdateUserId);
            _auditUpdateTime = DisplayValue(row.UpdateTime);
            _auditDialogVisible = true;
        }

        private string[] SelectedIds()
        {
            var result = new string[_selectedItems.Length];
            for (var index = 0; index < _selectedItems.Length; index++)
                result[index] = _selectedItems[index].Id;
            return result;
        }

        private LangTextView? Find(string id)
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

        public LangTextPageProps CreateProps() => new()
        {
            CanAdd = CanAdd,
            CanEdit = CanEdit,
            CanDelete = CanDelete,
            CanExport = CanExport,
            CanImport = CanImport,
            CanBatchDelete = CanBatchDelete,
            ShowAdvanced = _showAdvanced,
            Loading = _loading,
            Exporting = _exporting,
            EditDialogVisible = _editDialogVisible,
            AuditDialogVisible = _auditDialogVisible,
            Saving = _saving,
            FormLoading = _formLoading,
            ImportDialogVisible = _importDialogVisible,
            Importing = _importing,
            ImportCompleted = _importCompleted,
            TemplateDownloading = _templateDownloading,
            ImportHasErrorFile = _importHasErrorFile,
            CurrentPage = _currentPage,
            PageSize = _pageSize,
            Total = _total,
            Keyword = _keyword,
            EntityName = _entityName,
            EntityId = _entityId,
            FieldName = _fieldName,
            LangCode = _langCode,
            Content = _content,
            EditDialogTitle = _editDialogTitle,
            EditEntityName = _editEntityName,
            EditEntityId = _editEntityId,
            EditFieldName = _editFieldName,
            EditLangCode = _editLangCode,
            EditContent = _editContent,
            AuditDialogTitle = _auditDialogTitle,
            AuditCreateUser = _auditCreateUser,
            AuditCreateTime = _auditCreateTime,
            AuditUpdateUser = _auditUpdateUser,
            AuditUpdateTime = _auditUpdateTime,
            ImportResultFileName = _importResultFileName,
            ImportFileInputId = ImportFileInputId,
            Error = _error,
            SetEditDialogVisible = value => _editDialogVisible = value,
            SetAuditDialogVisible = value => _auditDialogVisible = value,
            SetKeyword = value => _keyword = value ?? "",
            SetEntityName = value => _entityName = value ?? "",
            SetEntityId = value => _entityId = value ?? "",
            SetFieldName = value => _fieldName = value ?? "",
            SetLangCode = value => _langCode = value ?? "",
            SetContent = value => _content = value ?? "",
            SetEditEntityName = value => _editEntityName = value ?? "",
            SetEditEntityId = value => _editEntityId = value ?? "",
            SetEditFieldName = value => _editFieldName = value ?? "",
            SetEditLangCode = value => _editLangCode = value ?? "",
            SetEditContent = value => _editContent = value ?? "",
            Rows = _items,
            SelectedCount = _selectedItems.Length,
            Search = Search,
            ResetSearch = ResetSearch,
            Refresh = Refresh,
            OpenCreate = OpenCreate,
            Save = Save,
            BatchDelete = BatchDelete,
            OpenImport = OpenImport,
            ExportPage = ExportPage,
            DownloadTemplate = DownloadTemplate,
            DownloadImportResult = DownloadImportResult,
            CloseImport = CloseImport,
            ImportPrimaryAction = ImportPrimaryAction,
            SelectImportFile = SelectImportFile,
            OpenAudit = OpenAudit,
            OpenEdit = OpenEdit,
            Delete = Delete,
            ExportCommand = ExportCommand,
            DismissError = () => _error = null,
            CancelEdit = () => _editDialogVisible = false,
            ToggleAdvanced = () => _showAdvanced = !_showAdvanced,
            ChangePage = ChangePage,
            ChangePageSize = ChangePageSize,
            Sort = Sort,
            SelectionChanged = SelectionChanged,
            ImportDialogChanged = ImportDialogChanged,
        };
    }
}

public sealed record LangTextPageProps : VueProps
{
    public required bool CanAdd { get; init; }
    public required bool CanEdit { get; init; }
    public required bool CanDelete { get; init; }
    public required bool CanExport { get; init; }
    public required bool CanImport { get; init; }
    public required bool CanBatchDelete { get; init; }
    public required bool ShowAdvanced { get; init; }
    public required bool Loading { get; init; }
    public required bool Exporting { get; init; }
    public required bool EditDialogVisible { get; init; }
    public required bool AuditDialogVisible { get; init; }
    public required bool Saving { get; init; }
    public required bool FormLoading { get; init; }
    public required bool ImportDialogVisible { get; init; }
    public required bool Importing { get; init; }
    public required bool ImportCompleted { get; init; }
    public required bool TemplateDownloading { get; init; }
    public required bool ImportHasErrorFile { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public required string Keyword { get; init; }
    public required string EntityName { get; init; }
    public required string EntityId { get; init; }
    public required string FieldName { get; init; }
    public required string LangCode { get; init; }
    public required string Content { get; init; }
    public required string EditDialogTitle { get; init; }
    public required string EditEntityName { get; init; }
    public required string EditEntityId { get; init; }
    public required string EditFieldName { get; init; }
    public required string EditLangCode { get; init; }
    public required string EditContent { get; init; }
    public required string AuditDialogTitle { get; init; }
    public required string AuditCreateUser { get; init; }
    public required string AuditCreateTime { get; init; }
    public required string AuditUpdateUser { get; init; }
    public required string AuditUpdateTime { get; init; }
    public required string ImportResultFileName { get; init; }
    public required string ImportFileInputId { get; init; }
    public required string? Error { get; init; }
    public required Action<bool> SetEditDialogVisible { get; init; }
    public required Action<bool> SetAuditDialogVisible { get; init; }
    public required Action<string?> SetKeyword { get; init; }
    public required Action<string?> SetEntityName { get; init; }
    public required Action<string?> SetEntityId { get; init; }
    public required Action<string?> SetFieldName { get; init; }
    public required Action<string?> SetLangCode { get; init; }
    public required Action<string?> SetContent { get; init; }
    public required Action<string?> SetEditEntityName { get; init; }
    public required Action<string?> SetEditEntityId { get; init; }
    public required Action<string?> SetEditFieldName { get; init; }
    public required Action<string?> SetEditLangCode { get; init; }
    public required Action<string?> SetEditContent { get; init; }
    public required LangTextView[] Rows { get; init; }
    public required int SelectedCount { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action Refresh { get; init; }
    public required Action OpenCreate { get; init; }
    public required Action Save { get; init; }
    public required Action BatchDelete { get; init; }
    public required Action OpenImport { get; init; }
    public required Action ExportPage { get; init; }
    public required Action DownloadTemplate { get; init; }
    public required Action DownloadImportResult { get; init; }
    public required Action CloseImport { get; init; }
    public required Action ImportPrimaryAction { get; init; }
    public required Action SelectImportFile { get; init; }
    public required Action<string> OpenAudit { get; init; }
    public required Action<string> OpenEdit { get; init; }
    public required Action<string> Delete { get; init; }
    public required Action<string> ExportCommand { get; init; }
    public required Action DismissError { get; init; }
    public required Action CancelEdit { get; init; }
    public required Action ToggleAdvanced { get; init; }
    public required Action<double> ChangePage { get; init; }
    public required Action<double> ChangePageSize { get; init; }
    public required Action<string, string> Sort { get; init; }
    public required Action<string[]> SelectionChanged { get; init; }
    public required Action<bool> ImportDialogChanged { get; init; }
}
