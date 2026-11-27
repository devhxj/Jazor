using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.ReceivedNotice;
using Jero.Web.Pages.Components;

namespace Jero.Web.Pages.ReceivedNotice;

[ECMAScriptModule("components/received-notice-page-state.js")]
public static class ReceivedNoticePageModule
{
    public static ReceivedNoticePageState Create() => new();

    public sealed class ReceivedNoticePageState : global::Jero.Web.Composition.IPageState<ReceivedNoticePageProps>
    {
        private ReceivedNoticeView[] _items = [];
        private ReceivedNoticeView[] _rows = [];
        private ReceivedNoticeOption[] _typeOptions = DefaultTypeOptions();
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _total;
        private string _queryTitle = string.Empty;
        private int _queryType;
        private bool _loading;
        private bool _optionsLoading;
        private bool _markingRead;
        private string? _error;

        private bool _detailDialogVisible;
        private string _detailTitle = "消息详情";
        private string _detailContent = string.Empty;

        public void Initialize()
        {
            LoadTypeOptions();
            LoadPage();
        }

        private void LoadTypeOptions()
        {
            _optionsLoading = true;
            ReceivedNoticeClient.Types().Then(outcome =>
            {
                _optionsLoading = false;
                if (!outcome.Ok)
                    return;

                var loaded = ReceivedNoticeClient.ReadTypeOptions(outcome.Data);
                if (loaded.Length > 0)
                    _typeOptions = loaded;
            }, _ => _optionsLoading = false);
        }

        private void LoadPage()
        {
            _loading = true;
            ReceivedNoticeClient.Page(new ReceivedNoticePageInput
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
                    ClearPage(outcome.Error ?? "加载站内信失败。");
                    return;
                }

                _items = ReceivedNoticeClient.ReadItems(outcome.Data);
                _rows = _items;
                _total = ReceivedNoticeClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                _loading = false;
                ClearPage("加载站内信失败，请检查网络后重试。");
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
            LoadTypeOptions();
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

        private void OpenDetail(string id)
        {
            var row = Find(id);
            if (row is null)
                return;

            _detailTitle = row.Title.Length == 0 ? "-" : row.Title;
            _detailContent = row.Content;
            _detailDialogVisible = true;

            if (row.ReadStatus == 1 || _markingRead)
                return;

            if (row.NoticeId.Length == 0)
            {
                _error = "通知 ID 缺失，无法标记为已读。";
                return;
            }

            _markingRead = true;
            ReceivedNoticeClient.SetRead(row.NoticeId).Then(outcome =>
            {
                _markingRead = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "标记站内信已读失败。";
                    return;
                }

                MarkRead(row.Id);
                _error = null;
            }, _ =>
            {
                _markingRead = false;
                _error = "标记站内信已读失败，请检查网络后重试。";
            });
        }

        private void MarkRead(string id)
        {
            for (var index = 0; index < _items.Length; index++)
            {
                if (_items[index].Id != id)
                    continue;

                _items[index].ReadStatus = 1;
                break;
            }
            _rows = _items;
        }

        private void ClearPage(string message)
        {
            _items = [];
            _rows = [];
            _total = 0;
            _error = message;
        }

        private ReceivedNoticeView? Find(string id)
        {
            for (var index = 0; index < _items.Length; index++)
                if (_items[index].Id == id)
                    return _items[index];
            return null;
        }

        private static ReceivedNoticeOption[] DefaultTypeOptions()
            =>
            [
                new ReceivedNoticeOption { Value = 1, Label = "通知", TagType = "primary" },
                new ReceivedNoticeOption { Value = 2, Label = "公告", TagType = "warning" }
            ];

        public ReceivedNoticePageProps CreateProps() => new()
        {
            Rows = _rows,
            TypeOptions = _typeOptions,
            QueryType = _queryType,
            OptionsLoading = _optionsLoading,
            MarkingRead = _markingRead,
            SetType = value => _queryType = (int)value,
            OpenDetail = OpenDetail,
            CurrentPage = _currentPage,
            PageSize = _pageSize,
            Total = _total,
            QueryTitle = _queryTitle,
            Loading = _loading,
            Error = _error,
            DetailDialogVisible = _detailDialogVisible,
            DetailTitle = _detailTitle,
            DetailContent = _detailContent,
            DismissError = () => _error = null,
            SetTitle = value => _queryTitle = value,
            Search = Search,
            ResetSearch = ResetSearch,
            Refresh = Refresh,
            ChangePage = ChangePage,
            ChangePageSize = ChangePageSize,
            SetDialogVisible = value => _detailDialogVisible = value,
        };
    }
}

public sealed record ReceivedNoticePageProps : VueProps
{
    public required ReceivedNoticeView[] Rows { get; init; }
    public required ReceivedNoticeOption[] TypeOptions { get; init; }
    public required int QueryType { get; init; }
    public required bool OptionsLoading { get; init; }
    public required bool MarkingRead { get; init; }
    public required Action<double> SetType { get; init; }
    public required Action<string> OpenDetail { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public required string QueryTitle { get; init; }
    public required bool Loading { get; init; }
    public required string? Error { get; init; }
    public required bool DetailDialogVisible { get; init; }
    public required string DetailTitle { get; init; }
    public required string DetailContent { get; init; }
    public required Action DismissError { get; init; }
    public required Action<string> SetTitle { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action Refresh { get; init; }
    public required Action<double> ChangePage { get; init; }
    public required Action<double> ChangePageSize { get; init; }
    public required Action<bool> SetDialogVisible { get; init; }
}
