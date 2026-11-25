using Jero.Web.App.Components;

namespace Jero.Web.App.ReceivedNotice;

[ECMAScriptModule("components/received-notice-page-state.js")]
public static class ReceivedNoticePageModule
{
    public static ReceivedNoticePageState Create() => new();

    public sealed class ReceivedNoticePageState : global::Jero.Web.App.IPageState<ReceivedNoticePageProps>
    {
        private ReceivedNoticeView[] items = [];
        private ReceivedNoticeView[] rows = [];
        private ReceivedNoticeOption[] typeOptions = DefaultTypeOptions();
        private int currentPage = 1;
        private int pageSize = 50;
        private int total;
        private string queryTitle = string.Empty;
        private int queryType;
        private bool loading;
        private bool optionsLoading;
        private bool markingRead;
        private string? error;

        private bool detailDialogVisible;
        private string detailTitle = "消息详情";
        private string detailContent = string.Empty;

        public void Initialize()
        {
            LoadTypeOptions();
            LoadPage();
        }

        private void LoadTypeOptions()
        {
            optionsLoading = true;
            ReceivedNoticeClient.Types().Then(outcome =>
            {
                optionsLoading = false;
                if (!outcome.Ok)
                    return;

                var loaded = ReceivedNoticeClient.ReadTypeOptions(outcome.Data);
                if (loaded.Length > 0)
                    typeOptions = loaded;
            }, _ => optionsLoading = false);
        }

        private void LoadPage()
        {
            loading = true;
            ReceivedNoticeClient.Page(new ReceivedNoticePageInput
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
                    ClearPage(outcome.Error ?? "加载站内信失败。");
                    return;
                }

                items = ReceivedNoticeClient.ReadItems(outcome.Data);
                rows = items;
                total = ReceivedNoticeClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                loading = false;
                ClearPage("加载站内信失败，请检查网络后重试。");
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
            LoadTypeOptions();
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

        private void OpenDetail(string id)
        {
            var row = Find(id);
            if (row is null)
                return;

            detailTitle = row.title.Length == 0 ? "-" : row.title;
            detailContent = row.content;
            detailDialogVisible = true;

            if (row.readStatus == 1 || markingRead)
                return;

            if (row.noticeId.Length == 0)
            {
                error = "通知 ID 缺失，无法标记为已读。";
                return;
            }

            markingRead = true;
            ReceivedNoticeClient.SetRead(row.noticeId).Then(outcome =>
            {
                markingRead = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "标记站内信已读失败。";
                    return;
                }

                MarkRead(row.id);
                error = null;
            }, _ =>
            {
                markingRead = false;
                error = "标记站内信已读失败，请检查网络后重试。";
            });
        }

        private void MarkRead(string id)
        {
            for (var index = 0; index < items.Length; index++)
            {
                if (items[index].id != id)
                    continue;

                items[index].readStatus = 1;
                break;
            }
            rows = items;
        }

        private void ClearPage(string message)
        {
            items = [];
            rows = [];
            total = 0;
            error = message;
        }

        private ReceivedNoticeView? Find(string id)
        {
            for (var index = 0; index < items.Length; index++)
                if (items[index].id == id)
                    return items[index];
            return null;
        }

        private static ReceivedNoticeOption[] DefaultTypeOptions()
            =>
            [
                new ReceivedNoticeOption { value = 1, label = "通知", tagType = "primary" },
                new ReceivedNoticeOption { value = 2, label = "公告", tagType = "warning" }
            ];

        public ReceivedNoticePageProps CreateProps() => new()
        {
            Rows = rows,
            TypeOptions = typeOptions,
            QueryType = queryType,
            OptionsLoading = optionsLoading,
            MarkingRead = markingRead,
            SetType = value => queryType = (int)value,
            OpenDetail = OpenDetail,
            CurrentPage = currentPage,
            PageSize = pageSize,
            Total = total,
            QueryTitle = queryTitle,
            Loading = loading,
            Error = error,
            DetailDialogVisible = detailDialogVisible,
            DetailTitle = detailTitle,
            DetailContent = detailContent,
            DismissError = () => error = null,
            SetTitle = value => queryTitle = value,
            Search = Search,
            ResetSearch = ResetSearch,
            Refresh = Refresh,
            ChangePage = ChangePage,
            ChangePageSize = ChangePageSize,
            SetDialogVisible = value => detailDialogVisible = value,
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
