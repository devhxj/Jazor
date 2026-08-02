namespace JazorAdmin;

[ECMAScriptModule("./components/jazor-admin-dashboard-page")]
public partial class DashboardPage : AppComponentBase, IVueContainerComponent
{
    [Parameter]
    public string? SelectedRowKey { get; set; }

    [Parameter]
    public EventCallback<string> SelectedRowKeyChanged { get; set; }

    [Parameter]
    public string[]? SelectedRowKeys { get; set; }

    [Parameter]
    public EventCallback<string[]> SelectedRowKeysChanged { get; set; }

    [Parameter]
    public string? SearchText { get; set; }

    [Parameter]
    public EventCallback<string> SearchTextChanged { get; set; }

    [Parameter]
    public int PageIndex { get; set; }

    [Parameter]
    public EventCallback<int> PageIndexChanged { get; set; }

    [Parameter]
    public string? SortColumnKey { get; set; }

    [Parameter]
    public EventCallback<string> SortColumnKeyChanged { get; set; }

    [Parameter]
    public bool SortDescending { get; set; }

    [Parameter]
    public EventCallback<bool> SortDescendingChanged { get; set; }

    [Parameter]
    public bool Loading { get; set; }
}
