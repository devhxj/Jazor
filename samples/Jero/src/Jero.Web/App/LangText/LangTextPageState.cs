using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using static ECMAScript.Global;

namespace Jero.Web.App.LangText;

[ECMAScriptModule("components/lang-text-page-state.js")]
public static class LangTextPageModule
{
    public static LangTextPageState Create() => new();

    public sealed class LangTextPageState : global::Jero.Web.App.IPageState<LangTextPageProps>
    {
        private FileRef? importFile;

        private const string ImportFileInputId = "lang-text-import-file";

        private LangTextView[] items = [];
        private LangTextView[] selectedItems = [];
        private int currentPage = 1;
        private int pageSize = 20;
        private int total;
        private string keyword = string.Empty;
        private string entityName = string.Empty;
        private string entityId = string.Empty;
        private string fieldName = string.Empty;
        private string langCode = string.Empty;
        private string content = string.Empty;
        private string field = "createTime";
        private string order = "descending";
        private bool showAdvanced;
        private bool loading;
        private bool exporting;
        private string? error;

        private bool editDialogVisible;
        private string editDialogTitle = "新增翻译";
        private bool formLoading;
        private bool saving;
        private string editId = string.Empty;
        private string editEntityName = string.Empty;
        private string editEntityId = string.Empty;
        private string editFieldName = string.Empty;
        private string editLangCode = string.Empty;
        private string editContent = string.Empty;

        private bool importDialogVisible;
        private bool importing;
        private bool templateDownloading;
        private bool importCompleted;
        private bool importHasErrorFile;
        private string importResultFileName = string.Empty;

        private bool auditDialogVisible;
        private string auditDialogTitle = "修改记录";
        private string auditCreateUser = "-";
        private string auditCreateTime = "-";
        private string auditUpdateUser = "-";
        private string auditUpdateTime = "-";

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
                page = page,
                pageSize = size,
                field = field,
                order = order,
                descStr = "descending",
                keyword = keyword.Trim(),
                entityName = entityName.Trim(),
                entityId = entityId.Trim().Length == 0 ? null : entityId.Trim(),
                fieldName = fieldName.Trim(),
                langCode = langCode.Trim(),
                content = content.Trim(),
                selectKeyList = selectedIds
            };

        private void LoadPage()
        {
            loading = true;
            LangTextClient.Page(BuildPageInput(currentPage, pageSize)).Then(outcome =>
            {
                loading = false;
                selectedItems = [];
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载翻译文本失败。";
                    items = [];
                    total = 0;
                    return;
                }

                items = LangTextClient.ReadItems(outcome.Data);
                total = LangTextClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                loading = false;
                error = "加载翻译文本失败，请检查网络后重试。";
            });
        }

        private void Search()
        {
            var queryEntityId = entityId.Trim();
            if (queryEntityId.Length > 0 && !IsPositiveInteger(queryEntityId))
            {
                error = "所属实体 ID 必须为正整数。";
                return;
            }

            currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            keyword = string.Empty;
            entityName = string.Empty;
            entityId = string.Empty;
            fieldName = string.Empty;
            langCode = string.Empty;
            content = string.Empty;
            currentPage = 1;
            LoadPage();
        }

        private void Refresh()
            => LoadPage();

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
            field = string.IsNullOrWhiteSpace(property) ? "createTime" : property;
            order = direction == "descending" ? "descending" : "ascending";
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
                selectedItems = result;
                return;
            }

            var compact = new LangTextView[targetIndex];
            for (var index = 0; index < targetIndex; index++)
                compact[index] = result[index];
            selectedItems = compact;
        }

        private void OpenCreate()
        {
            editDialogTitle = "新增翻译";
            editId = string.Empty;
            editEntityName = string.Empty;
            editEntityId = string.Empty;
            editFieldName = string.Empty;
            editLangCode = string.Empty;
            editContent = string.Empty;
            formLoading = false;
            editDialogVisible = true;
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit)
                return;

            editDialogTitle = "编辑翻译";
            editId = row.id;
            editEntityName = row.entityName;
            editEntityId = row.entityId;
            editFieldName = row.fieldName;
            editLangCode = row.langCode;
            editContent = row.content;
            editDialogVisible = true;
            formLoading = true;

            LangTextClient.Detail(row.id).Then(outcome =>
            {
                formLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载翻译详情失败。";
                    return;
                }

                var detail = LangTextClient.ReadDetail(outcome.Data);
                if (detail is null)
                {
                    error = "翻译详情不存在。";
                    return;
                }

                editId = detail.id;
                editEntityName = detail.entityName;
                editEntityId = detail.entityId;
                editFieldName = detail.fieldName;
                editLangCode = detail.langCode;
                editContent = detail.content;
                error = null;
            }, _ =>
            {
                formLoading = false;
                error = "加载翻译详情失败，请检查网络后重试。";
            });
        }

        private void Save()
        {
            if (saving || formLoading)
                return;

            var normalizedEntityName = editEntityName.Trim();
            var normalizedEntityId = editEntityId.Trim();
            var normalizedFieldName = editFieldName.Trim();
            var normalizedLangCode = editLangCode.Trim();
            var normalizedContent = editContent.Trim();

            if (!ValidateRequired(normalizedEntityName, "所属实体名", 255) ||
                !ValidateRequired(normalizedFieldName, "字段名", 255) ||
                !ValidateRequired(normalizedLangCode, "语言代码", 255) ||
                !ValidateRequired(normalizedContent, "翻译内容", 255))
            {
                return;
            }
            if (!IsPositiveInteger(normalizedEntityId))
            {
                error = "所属实体 ID 必须为正整数。";
                return;
            }

            var input = new LangTextSaveInput
            {
                id = editId,
                entityName = normalizedEntityName,
                entityId = normalizedEntityId,
                fieldName = normalizedFieldName,
                langCode = normalizedLangCode,
                content = normalizedContent
            };

            saving = true;
            var request = editId.Length == 0 ? LangTextClient.Add(input) : LangTextClient.Update(input);
            request.Then(outcome =>
            {
                saving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存翻译失败。";
                    return;
                }

                editDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success(editId.Length == 0 ? "翻译已新增" : "翻译已更新");
                LoadPage();
            }, _ =>
            {
                saving = false;
                error = "保存翻译失败，请检查网络后重试。";
            });
        }

        private bool ValidateRequired(string value, string label, int maxLength)
        {
            if (value.Length == 0)
            {
                error = label + "不能为空。";
                return false;
            }
            if (value.Length > maxLength)
            {
                error = label + "不能超过" + maxLength + "个字符。";
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

            LangTextClient.Delete(row.id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除翻译失败。";
                    return;
                }

                error = null;
                ElementPlus.ElMessageMembers.Success("翻译已删除");
                LoadPage();
            }, _ => error = "删除翻译失败，请检查网络后重试。");
        }

        private void BatchDelete()
        {
            if (!CanBatchDelete || selectedItems.Length == 0)
            {
                ElementPlus.ElMessageMembers.Warning("请选择要删除的翻译记录");
                return;
            }

            var ids = SelectedIds();
            LangTextClient.BatchDelete(ids).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "批量删除翻译失败。";
                    return;
                }

                selectedItems = [];
                currentPage = 1;
                error = null;
                ElementPlus.ElMessageMembers.Success("翻译已批量删除");
                LoadPage();
            }, _ => error = "批量删除翻译失败，请检查网络后重试。");
        }

        private void ExportPage()
            => BeginExport(BuildPageInput(currentPage, pageSize), "翻译表_本页数据.xlsx");

        private void ExportCommand(string command)
        {
            if (command == "select")
            {
                if (selectedItems.Length == 0)
                {
                    ElementPlus.ElMessageMembers.Warning("请先选择要导出的翻译记录");
                    return;
                }
                BeginExport(BuildPageInput(currentPage, pageSize, SelectedIds()), "翻译表_选中记录.xlsx");
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
            if (exporting)
                return;

            exporting = true;
            LangTextBrowserActions.DownloadExport(input, fallbackFileName).Then(outcome =>
            {
                exporting = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "导出翻译失败。";
                    return;
                }
                error = null;
            }, _ =>
            {
                exporting = false;
                error = "导出翻译失败，请检查网络后重试。";
            });
        }

        private void OpenImport()
        {
            ResetImportState();
            importDialogVisible = true;
        }

        private void ImportDialogChanged(bool value)
        {
            importDialogVisible = value == true;
            if (!importDialogVisible)
                ResetImportState();
        }

        private void CloseImport()
        {
            importDialogVisible = false;
            ResetImportState();
        }

        private void ResetImportState()
        {
            importing = false;
            importCompleted = false;
            importHasErrorFile = false;
            importResultFileName = string.Empty;
            importFile = null;
            LangTextBrowserActions.ClearSelectedFile(ImportFileInputId);
            LangTextBrowserActions.ResetImportResult();
        }

        private void DownloadTemplate()
        {
            if (templateDownloading)
                return;

            templateDownloading = true;
            LangTextBrowserActions.DownloadTemplate("翻译表导入模板.xlsx").Then(outcome =>
            {
                templateDownloading = false;
                if (!outcome.Ok)
                    error = outcome.Error ?? "下载导入模板失败。";
                else
                    error = null;
            }, _ =>
            {
                templateDownloading = false;
                error = "下载导入模板失败，请检查网络后重试。";
            });
        }

        private void ImportPrimaryAction()
        {
            if (importCompleted)
            {
                CloseImport();
                return;
            }

            SubmitImport();
        }

        private void SelectImportFile()
        {
            // Snapshot the native File during the callback, before any async work.
            importFile = LangTextBrowserActions.GetSelectedFile(ImportFileInputId);
        }

        private void SubmitImport()
        {
            if (importing)
                return;
            if (importFile is null)
            {
                ElementPlus.ElMessageMembers.Warning("请先选择要导入的文件");
                return;
            }

            importing = true;
            LangTextBrowserActions.ImportSelectedFile(importFile).Then(outcome =>
            {
                importing = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "导入翻译失败。";
                    return;
                }

                error = null;
                LoadPage();
                if (!outcome.HasErrorFile)
                {
                    ElementPlus.ElMessageMembers.Success(outcome.Message.Length == 0 ? "导入成功" : outcome.Message);
                    CloseImport();
                    return;
                }

                importCompleted = true;
                importHasErrorFile = true;
                importResultFileName = outcome.FileName;
                ElementPlus.ElMessageMembers.Warning("导入完成，但存在错误记录，请下载导入结果查看");
            }, _ =>
            {
                importing = false;
                error = "导入翻译失败，请检查网络后重试。";
            });
        }

        private static void DownloadImportResult()
            => LangTextBrowserActions.DownloadImportResult();

        private void OpenAudit(string id)
        {
            var row = Find(id);
            if (row is null)
                return;

            auditDialogTitle = "修改记录 · " + row.entityName + "/" + row.fieldName;
            auditCreateUser = DisplayUser(row.createUserName, row.createUserId);
            auditCreateTime = DisplayValue(row.createTime);
            auditUpdateUser = DisplayUser(row.updateUserName, row.updateUserId);
            auditUpdateTime = DisplayValue(row.updateTime);
            auditDialogVisible = true;
        }

        private string[] SelectedIds()
        {
            var result = new string[selectedItems.Length];
            for (var index = 0; index < selectedItems.Length; index++)
                result[index] = selectedItems[index].id;
            return result;
        }

        private LangTextView? Find(string id)
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

        public LangTextPageProps CreateProps() => new()
        {
            CanAdd = CanAdd,
            CanEdit = CanEdit,
            CanDelete = CanDelete,
            CanExport = CanExport,
            CanImport = CanImport,
            CanBatchDelete = CanBatchDelete,
            ShowAdvanced = showAdvanced,
            Loading = loading,
            Exporting = exporting,
            EditDialogVisible = editDialogVisible,
            AuditDialogVisible = auditDialogVisible,
            Saving = saving,
            FormLoading = formLoading,
            ImportDialogVisible = importDialogVisible,
            Importing = importing,
            ImportCompleted = importCompleted,
            TemplateDownloading = templateDownloading,
            ImportHasErrorFile = importHasErrorFile,
            CurrentPage = currentPage,
            PageSize = pageSize,
            Total = total,
            Keyword = keyword,
            EntityName = entityName,
            EntityId = entityId,
            FieldName = fieldName,
            LangCode = langCode,
            Content = content,
            EditDialogTitle = editDialogTitle,
            EditEntityName = editEntityName,
            EditEntityId = editEntityId,
            EditFieldName = editFieldName,
            EditLangCode = editLangCode,
            EditContent = editContent,
            AuditDialogTitle = auditDialogTitle,
            AuditCreateUser = auditCreateUser,
            AuditCreateTime = auditCreateTime,
            AuditUpdateUser = auditUpdateUser,
            AuditUpdateTime = auditUpdateTime,
            ImportResultFileName = importResultFileName,
            ImportFileInputId = ImportFileInputId,
            Error = error,
            SetEditDialogVisible = value => editDialogVisible = value,
            SetAuditDialogVisible = value => auditDialogVisible = value,
            SetKeyword = value => keyword = value ?? "",
            SetEntityName = value => entityName = value ?? "",
            SetEntityId = value => entityId = value ?? "",
            SetFieldName = value => fieldName = value ?? "",
            SetLangCode = value => langCode = value ?? "",
            SetContent = value => content = value ?? "",
            SetEditEntityName = value => editEntityName = value ?? "",
            SetEditEntityId = value => editEntityId = value ?? "",
            SetEditFieldName = value => editFieldName = value ?? "",
            SetEditLangCode = value => editLangCode = value ?? "",
            SetEditContent = value => editContent = value ?? "",
            Rows = items,
            SelectedCount = selectedItems.Length,
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
            DismissError = () => error = null,
            CancelEdit = () => editDialogVisible = false,
            ToggleAdvanced = () => showAdvanced = !showAdvanced,
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
