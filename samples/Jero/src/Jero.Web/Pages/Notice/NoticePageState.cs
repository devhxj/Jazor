using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.Notice;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Pages.Notice;

[ECMAScriptModule("components/notice-page-state.js")]
public static class NoticePageModule
{
    public static NoticePageState Create() => new();

    public sealed class NoticePageState : global::Jero.Web.Composition.IPageState<NoticePageProps>
    {
        private const string RichEditorId = "jero-notice-rich-editor";

        private NoticeView[] _items = [];
        private NoticeOption[] _typeOptions = DefaultTypeOptions();
        private NoticeOption[] _statusOptions = DefaultStatusOptions();
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _total;
        private string _queryTitle = string.Empty;
        private int _queryType;
        private bool _loading;
        private bool _optionsLoading;
        private int _pendingOptionRequests;
        private string? _error;

        private bool _editDialogVisible;
        private string _editDialogTitle = "新增通知公告";
        private bool _saving;
        private NoticeView? _editOriginal;
        private string _editId = string.Empty;
        private string _editTitle = string.Empty;
        private int _editType = 1;
        private string _editContent = string.Empty;
        private string _richEditorHtml = string.Empty;

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
            _optionsLoading = true;
            _pendingOptionRequests = 2;

            NoticeClient.Types().Then(outcome =>
            {
                if (outcome.Ok)
                {
                    var loaded = NoticeClient.ReadTypeOptions(outcome.Data);
                    if (loaded.Length > 0)
                        _typeOptions = loaded;
                }
                FinishOptionRequest();
            }, _ => FinishOptionRequest());

            NoticeClient.Statuses().Then(outcome =>
            {
                if (outcome.Ok)
                {
                    var loaded = NoticeClient.ReadStatusOptions(outcome.Data);
                    if (loaded.Length > 0)
                        _statusOptions = loaded;
                }
                FinishOptionRequest();
            }, _ => FinishOptionRequest());
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
            NoticeClient.Page(new NoticePageInput
            {
                Page = _currentPage,
                PageSize = _pageSize,
                Title = _queryTitle.Trim(),
                Type = _queryType > 0 ? _queryType : null
            }).Then(outcome =>
            {
                _loading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载通知公告失败。";
                    _items = [];
                    _total = 0;
                    return;
                }

                _items = NoticeClient.ReadItems(outcome.Data);
                _total = NoticeClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                _loading = false;
                _error = "加载通知公告失败，请检查网络后重试。";
            });
        }

        private void Search()
        {
            _currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            _queryTitle = string.Empty;
            _queryType = 0;
            _currentPage = 1;
            LoadPage();
        }

        private void Refresh()
        {
            LoadOptions();
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
            _editDialogTitle = "新增通知公告";
            _editOriginal = null;
            _editId = string.Empty;
            _editTitle = string.Empty;
            _editType = _typeOptions.Length == 0 ? 1 : _typeOptions[0].Value;
            _editContent = string.Empty;
            _richEditorHtml = string.Empty;
            _error = null;
            _editDialogVisible = true;
        }

        private void OpenEdit(string id)
        {
            var row = Find(id);
            if (row is null || !CanEdit || !CanMutate(row))
                return;

            _editDialogTitle = "编辑通知公告";
            _editOriginal = row;
            _editId = row.Id;
            _editTitle = row.Title;
            _editType = row.Type;
            _editContent = row.Content;
            _richEditorHtml = row.Content;
            _error = null;
            _editDialogVisible = true;
        }

        private void Save()
        {
            if (_saving)
                return;

            SyncRichContent();
            var normalizedTitle = _editTitle.Trim();
            var normalizedContent = _editContent.Trim();
            if (normalizedTitle.Length == 0)
            {
                _error = "标题不能为空。";
                return;
            }
            if (normalizedTitle.Length > 32)
            {
                _error = "标题不能超过32个字符。";
                return;
            }
            if (_editType <= 0)
            {
                _error = "请选择通知公告类型。";
                return;
            }
            if (normalizedContent.Length == 0)
            {
                _error = "内容不能为空。";
                return;
            }

            var creating = _editId.Length == 0;
            IPromise<ApiOutcome> request;
            if (creating)
            {
                request = NoticeClient.Add(new NoticeAddInput
                {
                    Title = normalizedTitle,
                    Content = normalizedContent,
                    Type = _editType,
                    Status = 0
                });
            }
            else
            {
                var source = _editOriginal;
                if (source is null || !CanMutate(source))
                {
                    _error = "当前通知公告不可编辑。";
                    return;
                }

                request = NoticeClient.Update(new NoticeSaveInput
                {
                    Id = source.Id,
                    Title = normalizedTitle,
                    Content = normalizedContent,
                    Type = _editType,
                    Status = source.Status,
                    PublicUserId = RequiredId(source.PublicUserId),
                    PublicUserName = Optional(source.PublicUserName),
                    PublicOrgId = RequiredId(source.PublicOrgId),
                    PublicOrgName = Optional(source.PublicOrgName),
                    PublicTime = Optional(source.PublicTime),
                    CancelTime = Optional(source.CancelTime),
                    CreateTime = source.CreateTime,
                    CreateUserId = source.CreateUserId,
                    CreateUserName = Optional(source.CreateUserName),
                    UpdateTime = Optional(source.UpdateTime),
                    UpdateUserId = Optional(source.UpdateUserId),
                    UpdateUserName = Optional(source.UpdateUserName)
                });
            }

            _saving = true;
            request.Then(outcome =>
            {
                _saving = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "保存通知公告失败。";
                    return;
                }

                _editDialogVisible = false;
                _error = null;
                ElementPlus.ElMessageMembers.Success(creating ? "通知公告已新增" : "通知公告已更新");
                LoadPage();
            }, _ =>
            {
                _saving = false;
                _error = "保存通知公告失败，请检查网络后重试。";
            });
        }

        private void Delete(string id)
        {
            var row = Find(id);
            if (row is null || !CanDelete || !CanMutate(row))
                return;

            NoticeClient.Delete(row.Id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除通知公告失败。";
                    return;
                }

                _error = null;
                ElementPlus.ElMessageMembers.Success("通知公告已删除");
                LoadPage();
            }, _ => _error = "删除通知公告失败，请检查网络后重试。");
        }

        private void Publish(string id)
        {
            var row = Find(id);
            if (row is null || !CanPublish || !CanMutate(row))
                return;

            NoticeClient.Publish(row.Id).Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "发布通知公告失败。";
                    return;
                }

                _error = null;
                ElementPlus.ElMessageMembers.Success("通知公告已发布");
                LoadPage();
            }, _ => _error = "发布通知公告失败，请检查网络后重试。");
        }

        private void SyncRichContent()
            => _editContent = NoticeBrowserActions.ReadHtml(RichEditorId);

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
            => row is not null && row.Status != 1 && IsOwn(row);

        private bool IsOwn(NoticeView row)
            => CurrentUserId.Length > 0 && row.CreateUserId == CurrentUserId;

        private NoticeTableRow[] TableRows()
        {
            var result = new NoticeTableRow[_items.Length];
            for (var index = 0; index < _items.Length; index++)
            {
                var row = _items[index];
                var disabledReason = !IsOwn(row) ? "仅创建者可操作"
                    : row.Status == 1 ? "已发布通知公告不可修改" : string.Empty;
                result[index] = new NoticeTableRow(
                    row.Id, row.Title, row.ContentSummary, OptionLabel(_typeOptions, row.Type, "类型"),
                    OptionTag(_typeOptions, row.Type, "primary"), row.CreateUserName, row.CreateTime,
                    OptionLabel(_statusOptions, row.Status, "状态"), OptionTag(_statusOptions, row.Status, "info"),
                    row.PublicUserName, row.PublicTime, CanMutate(row), disabledReason);
            }
            return result;
        }

        private NoticeSelectOption[] TypeSelectOptions()
        {
            var result = new NoticeSelectOption[_typeOptions.Length];
            for (var index = 0; index < _typeOptions.Length; index++)
                result[index] = new NoticeSelectOption(_typeOptions[index].Value, _typeOptions[index].Label);
            return result;
        }

        private NoticeView? Find(string id)
        {
            for (var index = 0; index < _items.Length; index++)
                if (_items[index].Id == id)
                    return _items[index];
            return null;
        }

        private static string OptionLabel(NoticeOption[] options, int value, string fallback)
        {
            for (var index = 0; index < options.Length; index++)
                if (options[index].Value == value)
                    return options[index].Label;
            return fallback + value;
        }

        private static string OptionTag(NoticeOption[] options, int value, string fallback)
        {
            for (var index = 0; index < options.Length; index++)
            {
                if (options[index].Value != value)
                    continue;
                var tag = options[index].TagType;
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
                new NoticeOption { Value = 1, Label = "通知", TagType = "primary" },
                new NoticeOption { Value = 2, Label = "公告", TagType = "success" }
            ];

        private static NoticeOption[] DefaultStatusOptions()
            =>
            [
                new NoticeOption { Value = 0, Label = "草稿", TagType = "info" },
                new NoticeOption { Value = 1, Label = "发布", TagType = "primary" },
                new NoticeOption { Value = 2, Label = "撤回", TagType = "warning" },
                new NoticeOption { Value = 3, Label = "删除", TagType = "danger" }
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
            Loading = _loading,
            OptionsLoading = _optionsLoading,
            EditDialogVisible = _editDialogVisible,
            Saving = _saving,
            CurrentPage = _currentPage,
            PageSize = _pageSize,
            Total = _total,
            QueryType = _queryType,
            EditType = _editType,
            QueryTitle = _queryTitle,
            EditTitle = _editTitle,
            EditDialogTitle = _editDialogTitle,
            RichEditorHtml = _richEditorHtml,
            RichEditorId = RichEditorId,
            Error = _error,
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
            DismissError = () => _error = null,
            CancelEdit = () => _editDialogVisible = false,
            SetQueryTitle = value => _queryTitle = value ?? "",
            SetEditTitle = value => _editTitle = value ?? "",
            SetQueryType = value => _queryType = (int)value,
            SetEditType = value => _editType = (int)value,
            ChangePage = ChangePage,
            ChangePageSize = ChangePageSize,
            SetEditDialogVisible = value => _editDialogVisible = value,
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
