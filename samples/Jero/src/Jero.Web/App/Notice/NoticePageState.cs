using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Notice;

[ECMAScriptModule("components/notice-page-state.js")]
public static class NoticePageModule
{
    public static NoticePageState Create() => new();

    public sealed class NoticePageState : global::Jero.Web.App.IPageState<NoticePageProps>
    {
        private const string RichEditorId = "jero-notice-rich-editor";

        private NoticeView[] items = [];
        private NoticeOption[] typeOptions = DefaultTypeOptions();
        private NoticeOption[] statusOptions = DefaultStatusOptions();
        private int currentPage = 1;
        private int pageSize = 50;
        private int total;
        private string queryTitle = string.Empty;
        private int queryType;
        private bool loading;
        private bool optionsLoading;
        private int pendingOptionRequests;
        private string? error;

        private bool editDialogVisible;
        private string editDialogTitle = "新增通知公告";
        private bool saving;
        private NoticeView? editOriginal;
        private string editId = string.Empty;
        private string editTitle = string.Empty;
        private int editType = 1;
        private string editContent = string.Empty;
        private string richEditorHtml = string.Empty;

        private bool CanAdd => ApplicationState.HasPermission("sysNotice:add");
        private bool CanEdit => ApplicationState.HasPermission("sysNotice:update");
        private bool CanDelete => ApplicationState.HasPermission("sysNotice:delete");
        private bool CanPublish => ApplicationState.HasPermission("sysNotice:public");
        private string CurrentUserId => ApplicationState.User?.Id ?? string.Empty;

        public void Initialize()
        {
            LoadOptions();
            LoadPage();
        }

        private void LoadOptions()
        {
            optionsLoading = true;
            pendingOptionRequests = 2;

            NoticeClient.Types().Then(outcome =>
            {
                if (outcome.Ok)
                {
                    var loaded = NoticeClient.ReadTypeOptions(outcome.Data);
                    if (loaded.Length > 0)
                        typeOptions = loaded;
                }
                FinishOptionRequest();
            }, _ => FinishOptionRequest());

            NoticeClient.Statuses().Then(outcome =>
            {
                if (outcome.Ok)
                {
                    var loaded = NoticeClient.ReadStatusOptions(outcome.Data);
                    if (loaded.Length > 0)
                        statusOptions = loaded;
                }
                FinishOptionRequest();
            }, _ => FinishOptionRequest());
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
            NoticeClient.Page(new NoticePageInput
            {
                page = currentPage,
                pageSize = pageSize,
                title = queryTitle.Trim(),
                type = queryType > 0 ? queryType : null
            }).Then(outcome =>
            {
                loading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载通知公告失败。";
                    items = [];
                    total = 0;
                    return;
                }

                items = NoticeClient.ReadItems(outcome.Data);
                total = NoticeClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                loading = false;
                error = "加载通知公告失败，请检查网络后重试。";
            });
        }

        private void Search()
        {
            currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            queryTitle = string.Empty;
            queryType = 0;
            currentPage = 1;
            LoadPage();
        }

        private void Refresh()
        {
            LoadOptions();
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
            editDialogTitle = "新增通知公告";
            editOriginal = null;
            editId = string.Empty;
            editTitle = string.Empty;
            editType = typeOptions.Length == 0 ? 1 : typeOptions[0].value;
            editContent = string.Empty;
            richEditorHtml = string.Empty;
            error = null;
            editDialogVisible = true;
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit || !CanMutate(row))
                return;

            editDialogTitle = "编辑通知公告";
            editOriginal = row;
            editId = row.id;
            editTitle = row.title;
            editType = row.type;
            editContent = row.content;
            richEditorHtml = row.content;
            error = null;
            editDialogVisible = true;
        }

        private void Save()
        {
            if (saving)
                return;

            SyncRichContent();
            var normalizedTitle = editTitle.Trim();
            var normalizedContent = editContent.Trim();
            if (normalizedTitle.Length == 0)
            {
                error = "标题不能为空。";
                return;
            }
            if (normalizedTitle.Length > 32)
            {
                error = "标题不能超过32个字符。";
                return;
            }
            if (editType <= 0)
            {
                error = "请选择通知公告类型。";
                return;
            }
            if (normalizedContent.Length == 0)
            {
                error = "内容不能为空。";
                return;
            }

            var creating = editId.Length == 0;
            IPromise<ApiOutcome> request;
            if (creating)
            {
                request = NoticeClient.Add(new NoticeAddInput
                {
                    title = normalizedTitle,
                    content = normalizedContent,
                    type = editType,
                    status = 0
                });
            }
            else
            {
                var source = editOriginal;
                if (source is null || !CanMutate(source))
                {
                    error = "当前通知公告不可编辑。";
                    return;
                }

                request = NoticeClient.Update(new NoticeSaveInput
                {
                    id = source.id,
                    title = normalizedTitle,
                    content = normalizedContent,
                    type = editType,
                    status = source.status,
                    publicUserId = RequiredId(source.publicUserId),
                    publicUserName = Optional(source.publicUserName),
                    publicOrgId = RequiredId(source.publicOrgId),
                    publicOrgName = Optional(source.publicOrgName),
                    publicTime = Optional(source.publicTime),
                    cancelTime = Optional(source.cancelTime),
                    createTime = source.createTime,
                    createUserId = source.createUserId,
                    createUserName = Optional(source.createUserName),
                    updateTime = Optional(source.updateTime),
                    updateUserId = Optional(source.updateUserId),
                    updateUserName = Optional(source.updateUserName)
                });
            }

            saving = true;
            request.Then(outcome =>
            {
                saving = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "保存通知公告失败。";
                    return;
                }

                editDialogVisible = false;
                error = null;
                ElementPlus.ElMessageMembers.Success(creating ? "通知公告已新增" : "通知公告已更新");
                LoadPage();
            }, _ =>
            {
                saving = false;
                error = "保存通知公告失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete || !CanMutate(row))
                return;

            NoticeClient.Delete(row.id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除通知公告失败。";
                    return;
                }

                error = null;
                ElementPlus.ElMessageMembers.Success("通知公告已删除");
                LoadPage();
            }, _ => error = "删除通知公告失败，请检查网络后重试。");
        }

        private void Publish(string id)
        {
            var row = Find(id);
            if (row is null || !CanPublish || !CanMutate(row))
                return;

            NoticeClient.Publish(row.id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "发布通知公告失败。";
                    return;
                }

                error = null;
                ElementPlus.ElMessageMembers.Success("通知公告已发布");
                LoadPage();
            }, _ => error = "发布通知公告失败，请检查网络后重试。");
        }

        private void SyncRichContent()
            => editContent = NoticeBrowserActions.ReadHtml(RichEditorId);

        private void RichCommand(string command)
        {
            NoticeBrowserActions.ExecuteEditorCommand(RichEditorId, command);
            SyncRichContent();
        }

        private void RichBlock(string tag)
        {
            NoticeBrowserActions.ExecuteEditorCommand(RichEditorId, "formatBlock", tag);
            SyncRichContent();
        }

        private bool CanMutate(NoticeView? row)
            => row is not null && row.status != 1 && IsOwn(row);

        private bool IsOwn(NoticeView row)
            => CurrentUserId.Length > 0 && row.createUserId == CurrentUserId;

        private NoticeTableRow[] TableRows()
        {
            var result = new NoticeTableRow[items.Length];
            for (var index = 0; index < items.Length; index++)
            {
                var row = items[index];
                var disabledReason = !IsOwn(row) ? "仅创建者可操作"
                    : row.status == 1 ? "已发布通知公告不可修改" : string.Empty;
                result[index] = new NoticeTableRow(
                    row.id, row.title, row.contentSummary, OptionLabel(typeOptions, row.type, "类型"),
                    OptionTag(typeOptions, row.type, "primary"), row.createUserName, row.createTime,
                    OptionLabel(statusOptions, row.status, "状态"), OptionTag(statusOptions, row.status, "info"),
                    row.publicUserName, row.publicTime, CanMutate(row), disabledReason);
            }
            return result;
        }

        private NoticeSelectOption[] TypeSelectOptions()
        {
            var result = new NoticeSelectOption[typeOptions.Length];
            for (var index = 0; index < typeOptions.Length; index++)
                result[index] = new NoticeSelectOption(typeOptions[index].value, typeOptions[index].label);
            return result;
        }

        private NoticeView? Find(string id)
        {
            for (var index = 0; index < items.Length; index++)
                if (items[index].id == id)
                    return items[index];
            return null;
        }

        private static string OptionLabel(NoticeOption[] options, int value, string fallback)
        {
            for (var index = 0; index < options.Length; index++)
                if (options[index].value == value)
                    return options[index].label;
            return fallback + value;
        }

        private static string OptionTag(NoticeOption[] options, int value, string fallback)
        {
            for (var index = 0; index < options.Length; index++)
            {
                if (options[index].value != value)
                    continue;
                var tag = options[index].tagType;
                if (tag == "success") return "success";
                if (tag == "warning") return "warning";
                if (tag == "danger") return "danger";
                if (tag == "info") return "info";
                return "primary";
            }
            return fallback;
        }

        private static NoticeOption[] DefaultTypeOptions()
            =>
            [
                new NoticeOption { value = 1, label = "通知", tagType = "primary" },
                new NoticeOption { value = 2, label = "公告", tagType = "success" }
            ];

        private static NoticeOption[] DefaultStatusOptions()
            =>
            [
                new NoticeOption { value = 0, label = "草稿", tagType = "info" },
                new NoticeOption { value = 1, label = "发布", tagType = "primary" },
                new NoticeOption { value = 2, label = "撤回", tagType = "warning" },
                new NoticeOption { value = 3, label = "删除", tagType = "danger" }
            ];

        private static string RequiredId(string value)
            => value.Length == 0 ? "0" : value;

        private static string? Optional(string value)
            => value.Length == 0 ? null : value;

        public NoticePageProps CreateProps() => new()
        {
            CanAdd = CanAdd,
            CanEdit = CanEdit,
            CanPublish = CanPublish,
            CanDelete = CanDelete,
            Loading = loading,
            OptionsLoading = optionsLoading,
            EditDialogVisible = editDialogVisible,
            Saving = saving,
            CurrentPage = currentPage,
            PageSize = pageSize,
            Total = total,
            QueryType = queryType,
            EditType = editType,
            QueryTitle = queryTitle,
            EditTitle = editTitle,
            EditDialogTitle = editDialogTitle,
            RichEditorHtml = richEditorHtml,
            RichEditorId = RichEditorId,
            Error = error,
            Rows = TableRows(),
            TypeOptions = TypeSelectOptions(),
            Search = Search,
            ResetSearch = ResetSearch,
            Refresh = Refresh,
            OpenCreate = OpenCreate,
            Save = Save,
            SyncRichContent = SyncRichContent,
            RichCommand = RichCommand,
            RichBlock = RichBlock,
            OpenEdit = OpenEdit,
            Publish = Publish,
            Delete = Delete,
            DismissError = () => error = null,
            CancelEdit = () => editDialogVisible = false,
            SetQueryTitle = value => queryTitle = value ?? "",
            SetEditTitle = value => editTitle = value ?? "",
            SetQueryType = value => queryType = (int)value,
            SetEditType = value => editType = (int)value,
            ChangePage = ChangePage,
            ChangePageSize = ChangePageSize,
            SetEditDialogVisible = value => editDialogVisible = value,
        };
    }
}

public sealed record NoticePageProps : VueProps
{
    public required bool CanAdd { get; init; }
    public required bool CanEdit { get; init; }
    public required bool CanPublish { get; init; }
    public required bool CanDelete { get; init; }
    public required bool Loading { get; init; }
    public required bool OptionsLoading { get; init; }
    public required bool EditDialogVisible { get; init; }
    public required bool Saving { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public required int QueryType { get; init; }
    public required int EditType { get; init; }
    public required string QueryTitle { get; init; }
    public required string EditTitle { get; init; }
    public required string EditDialogTitle { get; init; }
    public required string RichEditorHtml { get; init; }
    public required string RichEditorId { get; init; }
    public required string? Error { get; init; }
    public required NoticeTableRow[] Rows { get; init; }
    public required NoticeSelectOption[] TypeOptions { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action Refresh { get; init; }
    public required Action OpenCreate { get; init; }
    public required Action Save { get; init; }
    public required Action SyncRichContent { get; init; }
    public required Action<string> RichCommand { get; init; }
    public required Action<string> RichBlock { get; init; }
    public required Action<string> OpenEdit { get; init; }
    public required Action<string> Publish { get; init; }
    public required Action<string> Delete { get; init; }
    public required Action DismissError { get; init; }
    public required Action CancelEdit { get; init; }
    public required Action<string?> SetQueryTitle { get; init; }
    public required Action<string?> SetEditTitle { get; init; }
    public required Action<double> SetQueryType { get; init; }
    public required Action<double> SetEditType { get; init; }
    public required Action<double> ChangePage { get; init; }
    public required Action<double> ChangePageSize { get; init; }
    public required Action<bool> SetEditDialogVisible { get; init; }
}
