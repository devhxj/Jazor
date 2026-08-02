namespace JazorAdmin;

[ECMAScriptModule("./components/jazor-admin-release-queue")]
public partial class ReleaseQueue : AppComponentBase, IVueContainerComponent
{
    [Parameter]
    public string? SectionClass { get; set; }

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

    private ReleaseTableColumns Columns { get; } =
        new ReleaseTableColumn[]
        {
            new()
            {
                Key = "name",
                Title = "Service",
                Width = "36%",
                Sortable = true
            },
            new()
            {
                Key = "version",
                Title = "Version",
                Width = "22%"
            },
            new()
            {
                Key = "status",
                Title = "Status",
                Width = "22%"
            },
            new()
            {
                Key = "owner",
                Title = "Owner",
                Width = "20%"
            }
        };

    private ReleaseTableRows Rows { get; } =
        new ReleaseTableRow[]
        {
            new()
            {
                Key = "release.api",
                Cells = new ReleaseTableCell[]
                {
                    new() { ColumnKey = "name", Text = "Admin API" },
                    new() { ColumnKey = "version", Text = "2026.07.28-alpha" },
                    new() { ColumnKey = "status", Text = "Ready" },
                    new() { ColumnKey = "owner", Text = "Platform" }
                }
            },
            new()
            {
                Key = "release.web",
                Cells = new ReleaseTableCell[]
                {
                    new() { ColumnKey = "name", Text = "Admin Web" },
                    new() { ColumnKey = "version", Text = "2026.07.28-alpha" },
                    new() { ColumnKey = "status", Text = "Verifying" },
                    new() { ColumnKey = "owner", Text = "Frontend" }
                }
            },
            new()
            {
                Key = "release.worker",
                Cells = new ReleaseTableCell[]
                {
                    new() { ColumnKey = "name", Text = "Audit Worker" },
                    new() { ColumnKey = "version", Text = "2026.07.27" },
                    new() { ColumnKey = "status", Text = "Queued" },
                    new() { ColumnKey = "owner", Text = "Operations" }
                }
            }
        };
}
