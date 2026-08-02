namespace JazorAdmin;

[ECMAScriptModule("./components/jazor-admin-release-table")]
public partial class ReleaseTable : AppComponentBase, IVueContainerComponent
{
    [Parameter]
    public ReleaseTableColumns? Columns { get; set; }

    [Parameter]
    public ReleaseTableRows? Rows { get; set; }

    [Parameter]
    public string? SelectedRowKey { get; set; }

    [Parameter]
    public EventCallback<string> SelectedRowKeyChanged { get; set; }

    [Parameter]
    public string[]? SelectedRowKeys { get; set; }

    [Parameter]
    public EventCallback<string[]> SelectedRowKeysChanged { get; set; }

    [Parameter]
    public bool MultiSelectable { get; set; }

    [Parameter]
    public string? EmptyText { get; set; }

    [Parameter]
    public bool Loading { get; set; }

    [Parameter]
    public string? LoadingText { get; set; }

    [Parameter]
    public string? SearchText { get; set; }

    [Parameter]
    public EventCallback<string> SearchTextChanged { get; set; }

    [Parameter]
    public string? SearchPlaceholder { get; set; }

    [Parameter]
    public int PageIndex { get; set; }

    [Parameter]
    public EventCallback<int> PageIndexChanged { get; set; }

    [Parameter]
    public int PageSize { get; set; } = 10;

    [Parameter]
    public string? SortColumnKey { get; set; }

    [Parameter]
    public EventCallback<string> SortColumnKeyChanged { get; set; }

    [Parameter]
    public bool SortDescending { get; set; }

    [Parameter]
    public EventCallback<bool> SortDescendingChanged { get; set; }

    private EffectiveColumn[] GetEffectiveColumns()
        => BuildEffectiveColumns(Columns?.AsArray);

    private EffectiveRow[] GetEffectiveRows()
        => BuildEffectiveRows(Rows?.AsArray);

    private string NormalizedEmptyText
        => Text.Normalize(EmptyText) ?? "No data";

    private string NormalizedLoadingText
        => Text.Normalize(LoadingText) ?? "Loading";

    private VueClassValue RootCssClass
        => BuildCssClass("jazor-admin-release-table");

    private async Task OnSearchTextChanged(string? value)
    {
        if (Loading)
        {
            return;
        }

        await SearchTextChanged.InvokeAsync(value ?? string.Empty);
        await PageIndexChanged.InvokeAsync(0);
    }

    private async Task OnPreviousPage()
    {
        if (Loading)
        {
            return;
        }

        var columns = GetEffectiveColumns();
        var filteredRows = FilterRows(GetEffectiveRows(), columns, SearchText);
        var pageSize = NormalizePageSize(PageSize);
        var pageIndex = NormalizePageIndex(PageIndex, filteredRows.Length, pageSize);
        if (pageIndex > 0)
        {
            await PageIndexChanged.InvokeAsync(pageIndex - 1);
        }
    }

    private async Task OnNextPage()
    {
        if (Loading)
        {
            return;
        }

        var columns = GetEffectiveColumns();
        var filteredRows = FilterRows(GetEffectiveRows(), columns, SearchText);
        var sortedRows = SortRows(filteredRows, columns, SortColumnKey, SortDescending);
        var pageSize = NormalizePageSize(PageSize);
        var pageIndex = NormalizePageIndex(PageIndex, sortedRows.Length, pageSize);
        var pageCount = GetPageCount(sortedRows.Length, pageSize);
        if (pageIndex < pageCount - 1)
        {
            await PageIndexChanged.InvokeAsync(pageIndex + 1);
        }
    }

    private async Task OnSortRequested(EffectiveColumn column)
    {
        if (Loading || !column.Sortable)
        {
            return;
        }

        var sortColumnKey = Text.Normalize(SortColumnKey);
        var descending = sortColumnKey == column.Key && !SortDescending;
        await SortColumnKeyChanged.InvokeAsync(column.Key);
        await SortDescendingChanged.InvokeAsync(descending);
        await PageIndexChanged.InvokeAsync(0);
    }

    private async Task OnRowSelected(EffectiveRow row)
    {
        if (Loading || (row.Source.Disabled ?? false))
        {
            return;
        }

        await SelectedRowKeyChanged.InvokeAsync(row.Key);
    }

    private async Task OnRowSelectionToggled(EffectiveRow row)
    {
        if (Loading || (row.Source.Disabled ?? false) || !MultiSelectable)
        {
            return;
        }

        var selectedKeys = NormalizeSelectedRowKeys(SelectedRowKeys);
        if (!selectedKeys.Add(row.Key))
        {
            selectedKeys.Remove(row.Key);
        }

        await SelectedRowKeysChanged.InvokeAsync(ToOrderedArray(selectedKeys));
    }

    private async Task OnVisibleSelectionToggled(EffectiveRow[] visibleRows)
    {
        if (Loading || !MultiSelectable || visibleRows.Length == 0)
        {
            return;
        }

        var selectedKeys = NormalizeSelectedRowKeys(SelectedRowKeys);
        var allSelected = true;
        foreach (var row in visibleRows)
        {
            if (row.Source.Disabled ?? false)
            {
                continue;
            }

            if (!selectedKeys.Contains(row.Key))
            {
                allSelected = false;
                break;
            }
        }

        foreach (var row in visibleRows)
        {
            if (row.Source.Disabled ?? false)
            {
                continue;
            }

            if (allSelected)
            {
                selectedKeys.Remove(row.Key);
            }
            else
            {
                selectedKeys.Add(row.Key);
            }
        }

        await SelectedRowKeysChanged.InvokeAsync(ToOrderedArray(selectedKeys));
    }

    private bool RowIsSelected(EffectiveRow row)
    {
        var selectedRowKey = Text.Normalize(SelectedRowKey);
        return selectedRowKey is not null
               && !(row.Source.Disabled ?? false)
               && row.Key == selectedRowKey;
    }

    private bool RowIsMultiSelected(EffectiveRow row)
        => !(row.Source.Disabled ?? false) &&
           NormalizeSelectedRowKeys(SelectedRowKeys).Contains(row.Key);

    private bool PageSelectionIsComplete(EffectiveRow[] visibleRows)
    {
        var hasSelectableRow = false;
        var selectedKeys = NormalizeSelectedRowKeys(SelectedRowKeys);
        foreach (var row in visibleRows)
        {
            if (row.Source.Disabled ?? false)
            {
                continue;
            }

            hasSelectableRow = true;
            if (!selectedKeys.Contains(row.Key))
            {
                return false;
            }
        }

        return hasSelectableRow;
    }

    private bool ColumnIsSorted(EffectiveColumn column)
    {
        var sortColumnKey = Text.Normalize(SortColumnKey);
        return sortColumnKey is not null && column.Key == sortColumnKey;
    }

    private static string ResolveCellText(EffectiveRow row, EffectiveColumn column)
    {
        foreach (var cell in row.Cells)
        {
            if (cell.ColumnKey == column.Key)
            {
                return cell.Text ?? string.Empty;
            }
        }

        return string.Empty;
    }

    private static string BuildRowCssClass(bool isSelected, bool isDisabled)
    {
        if (isSelected && isDisabled)
        {
            return "jazor-admin-release-table__row is-selected is-disabled";
        }

        if (isDisabled)
        {
            return "jazor-admin-release-table__row is-disabled";
        }

        return isSelected
            ? "jazor-admin-release-table__row is-selected"
            : "jazor-admin-release-table__row";
    }

    private static HashSet<string> NormalizeSelectedRowKeys(string[]? selectedRowKeys)
    {
        var normalized = new HashSet<string>();
        if (selectedRowKeys is null)
        {
            return normalized;
        }

        foreach (var selectedRowKey in selectedRowKeys)
        {
            var normalizedKey = Text.Normalize(selectedRowKey);
            if (normalizedKey is not null)
            {
                normalized.Add(normalizedKey);
            }
        }

        return normalized;
    }

    private static string[] ToOrderedArray(HashSet<string> keys)
    {
        if (keys.Count == 0)
        {
            return Array.Empty<string>();
        }

        var orderedKeyList = new List<string>(keys.Count);
        foreach (var key in keys)
        {
            orderedKeyList.Add(key);
        }

        var orderedKeys = orderedKeyList.ToArray();
        Array.Sort(orderedKeys);
        return orderedKeys;
    }

    private static string BuildHeadingCssClass(EffectiveColumn column, bool isSorted)
    {
        if (column.Sortable && isSorted)
        {
            return "jazor-admin-release-table__heading is-sortable is-sorted";
        }

        if (isSorted)
        {
            return "jazor-admin-release-table__heading is-sorted";
        }

        return column.Sortable
            ? "jazor-admin-release-table__heading is-sortable"
            : "jazor-admin-release-table__heading";
    }

    private static EffectiveRow[] FilterRows(
        EffectiveRow[] rows,
        EffectiveColumn[] columns,
        string? searchText)
    {
        var normalizedSearchText = Text.Normalize(searchText);
        if (normalizedSearchText is null)
        {
            return rows;
        }

        var filteredRows = new List<EffectiveRow>(rows.Length);
        foreach (var row in rows)
        {
            if (RowMatchesSearch(row, columns, normalizedSearchText))
            {
                filteredRows.Add(row);
            }
        }

        return filteredRows.ToArray();
    }

    private static bool RowMatchesSearch(
        EffectiveRow row,
        EffectiveColumn[] columns,
        string searchText)
    {
        foreach (var column in columns)
        {
            if (ResolveCellText(row, column).Contains(searchText, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static EffectiveRow[] SortRows(
        EffectiveRow[] rows,
        EffectiveColumn[] columns,
        string? sortColumnKey,
        bool descending)
    {
        var normalizedSortColumnKey = Text.Normalize(sortColumnKey);
        if (normalizedSortColumnKey is null || !ColumnCanSort(columns, normalizedSortColumnKey) || rows.Length < 2)
        {
            return rows;
        }

        var sortedRows = new List<EffectiveRow>(rows.Length);
        foreach (var row in rows)
        {
            sortedRows.Add(row);
        }

        for (var index = 1; index < sortedRows.Count; index++)
        {
            var current = sortedRows[index];
            var currentText = ResolveCellText(current, normalizedSortColumnKey);
            var cursor = index - 1;
            while (cursor >= 0)
            {
                var candidate = sortedRows[cursor];
                var comparison = CompareCellText(ResolveCellText(candidate, normalizedSortColumnKey), currentText, descending);
                if (comparison <= 0)
                {
                    break;
                }

                sortedRows[cursor + 1] = candidate;
                cursor--;
            }

            sortedRows[cursor + 1] = current;
        }

        return sortedRows.ToArray();
    }

    private static bool ColumnCanSort(EffectiveColumn[] columns, string columnKey)
    {
        foreach (var column in columns)
        {
            if (column.Key == columnKey)
            {
                return column.Sortable;
            }
        }

        return false;
    }

    private static int CompareCellText(string left, string right, bool descending)
    {
        var comparison = string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
        return descending ? -comparison : comparison;
    }

    private static string ResolveCellText(EffectiveRow row, string columnKey)
    {
        foreach (var cell in row.Cells)
        {
            if (cell.ColumnKey == columnKey)
            {
                return cell.Text ?? string.Empty;
            }
        }

        return string.Empty;
    }

    private static int NormalizePageSize(int pageSize)
        => pageSize <= 0 ? 10 : pageSize;

    private static int NormalizePageIndex(
        int pageIndex,
        int rowCount,
        int pageSize)
    {
        if (pageIndex <= 0)
        {
            return 0;
        }

        var pageCount = GetPageCount(rowCount, pageSize);
        if (pageIndex >= pageCount)
        {
            return pageCount - 1;
        }

        return pageIndex;
    }

    private static int GetPageCount(int rowCount, int pageSize)
        => rowCount <= 0 ? 1 : (rowCount + pageSize - 1) / pageSize;

    private static EffectiveRow[] SliceRows(
        EffectiveRow[] rows,
        int pageIndex,
        int pageSize)
    {
        var start = pageIndex * pageSize;
        var end = start + pageSize;
        var slicedRows = new List<EffectiveRow>(pageSize);
        for (var index = start; index < end && index < rows.Length; index++)
        {
            slicedRows.Add(rows[index]);
        }

        return slicedRows.ToArray();
    }

    private static EffectiveColumn[] BuildEffectiveColumns(ReleaseTableColumn[]? columns)
    {
        if (columns is not { Length: > 0 })
        {
            return Array.Empty<EffectiveColumn>();
        }

        var effectiveColumns = new List<EffectiveColumn>(columns.Length);
        var usedKeys = new HashSet<string>();
        foreach (var column in columns)
        {
            var key = Text.Normalize(column.Key);
            var title = Text.Normalize(column.Title);
            if (key is null || title is null || !usedKeys.Add(key))
            {
                continue;
            }

            effectiveColumns.Add(new EffectiveColumn(column, key, title, Text.Normalize(column.Width), column.Sortable ?? false));
        }

        return effectiveColumns.ToArray();
    }

    private static EffectiveRow[] BuildEffectiveRows(ReleaseTableRow[]? rows)
    {
        if (rows is not { Length: > 0 })
        {
            return Array.Empty<EffectiveRow>();
        }

        var effectiveRows = new List<EffectiveRow>(rows.Length);
        var usedKeys = new HashSet<string>();
        foreach (var row in rows)
        {
            var key = Text.Normalize(row.Key);
            if (key is null || !usedKeys.Add(key))
            {
                continue;
            }

            effectiveRows.Add(new EffectiveRow(row, key, BuildEffectiveCells(row.Cells?.AsArray)));
        }

        return effectiveRows.ToArray();
    }

    private static EffectiveCell[] BuildEffectiveCells(ReleaseTableCell[]? cells)
    {
        if (cells is not { Length: > 0 })
        {
            return Array.Empty<EffectiveCell>();
        }

        var effectiveCells = new List<EffectiveCell>(cells.Length);
        var usedKeys = new HashSet<string>();
        foreach (var cell in cells)
        {
            var columnKey = Text.Normalize(cell.ColumnKey);
            if (columnKey is null || !usedKeys.Add(columnKey))
            {
                continue;
            }

            effectiveCells.Add(new EffectiveCell(columnKey, Text.Normalize(cell.Text)));
        }

        return effectiveCells.ToArray();
    }

    private sealed class EffectiveColumn
    {
        public EffectiveColumn(ReleaseTableColumn source, string key, string title, string? width, bool sortable)
        {
            Source = source;
            Key = key;
            Title = title;
            Width = width;
            Sortable = sortable;
        }

        public ReleaseTableColumn Source { get; }

        public string Key { get; }

        public string Title { get; }

        public string? Width { get; }

        public bool Sortable { get; }
    }

    private sealed class EffectiveRow
    {
        public EffectiveRow(ReleaseTableRow source, string key, EffectiveCell[] cells)
        {
            Source = source;
            Key = key;
            Cells = cells;
        }

        public ReleaseTableRow Source { get; }

        public string Key { get; }

        public EffectiveCell[] Cells { get; }
    }

    private sealed class EffectiveCell
    {
        public EffectiveCell(string columnKey, string? text)
        {
            ColumnKey = columnKey;
            Text = text;
        }

        public string ColumnKey { get; }

        public string? Text { get; }
    }
}
