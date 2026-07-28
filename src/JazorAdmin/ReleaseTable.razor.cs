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

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var columns = GetEffectiveColumns();
        var filteredRows = FilterRows(GetEffectiveRows(), columns, SearchText);
        var sortedRows = SortRows(filteredRows, columns, SortColumnKey, SortDescending);
        var pageSize = NormalizePageSize(PageSize);
        var pageIndex = NormalizePageIndex(PageIndex, sortedRows.Length, pageSize);
        var pageRows = SliceRows(sortedRows, pageIndex, pageSize);
        var pageCount = GetPageCount(sortedRows.Length, pageSize);
        var hasPagination = pageCount > 1;

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", RootCssClass);
        builder.AddAttribute(2, "style", CssStyle);
        builder.AddAttribute(3, "aria-busy", Loading);
        builder.OpenElement(4, "div");
        builder.AddAttribute(5, "class", "jazor-admin-release-table__toolbar");

        builder.OpenElement(6, "input");
        builder.AddAttribute(7, "class", "jazor-admin-release-table__search");
        builder.AddAttribute(8, "type", "search");
        builder.AddAttribute(9, "value", SearchText);
        builder.AddAttribute(10, "placeholder", Text.Normalize(SearchPlaceholder) ?? "Search");
        builder.AddAttribute(11, "oninput", EventCallback.Factory.Create<string>(this, OnSearchTextChanged));
        builder.AddAttribute(12, "disabled", Loading);
        builder.SetUpdatesAttributeName("value");
        builder.CloseElement();

        builder.OpenElement(12, "span");
        builder.AddAttribute(13, "class", "jazor-admin-release-table__summary");
        builder.AddContent(14, filteredRows.Length);
        builder.AddContent(15, " rows");
        builder.CloseElement();

        builder.CloseElement();

        if (columns.Length > 0)
        {
            builder.OpenElement(10, "table");
            builder.AddAttribute(11, "class", "jazor-admin-release-table__table");

            builder.OpenElement(20, "thead");
            builder.AddAttribute(21, "class", "jazor-admin-release-table__head");
            builder.OpenElement(22, "tr");
            if (MultiSelectable)
            {
                builder.OpenElement(23, "th");
                builder.AddAttribute(24, "class", "jazor-admin-release-table__heading jazor-admin-release-table__selection-heading");

                builder.OpenElement(25, "input");
                builder.AddAttribute(26, "class", "jazor-admin-release-table__select-all");
                builder.AddAttribute(27, "type", "checkbox");
                builder.AddAttribute(28, "checked", PageSelectionIsComplete(pageRows));
                builder.AddAttribute(29, "aria-label", "Select visible rows");
                builder.AddAttribute(30, "onclick", EventCallback.Factory.Create(this, () => OnVisibleSelectionToggled(pageRows)));
                builder.AddAttribute(31, "disabled", Loading);
                builder.CloseElement();

                builder.CloseElement();
            }

            foreach (var column in columns)
            {
                var columnIsSorted = ColumnIsSorted(column);

                builder.OpenElement(23, "th");
                builder.AddAttribute(24, "class", BuildHeadingCssClass(column, columnIsSorted));
                builder.AddAttribute(25, "data-column-key", column.Key);
                builder.AddAttribute(26, "style", column.Width);
                builder.AddAttribute(27, "aria-sort", columnIsSorted ? (SortDescending ? "descending" : "ascending") : "none");
                if (column.Sortable)
                {
                    builder.OpenElement(28, "button");
                    builder.AddAttribute(29, "type", "button");
                    builder.AddAttribute(30, "class", "jazor-admin-release-table__sort-button");
                    builder.AddAttribute(31, "onclick", EventCallback.Factory.Create(this, () => OnSortRequested(column)));
                    builder.AddAttribute(32, "disabled", Loading);
                    builder.AddContent(32, column.Title);

                    if (columnIsSorted)
                    {
                        builder.OpenElement(33, "span");
                        builder.AddAttribute(34, "class", "jazor-admin-release-table__sort-indicator");
                        builder.AddAttribute(35, "aria-hidden", "true");
                        builder.AddContent(36, SortDescending ? "desc" : "asc");
                        builder.CloseElement();
                    }

                    builder.CloseElement();
                }
                else
                {
                    builder.AddContent(37, column.Title);
                }
                builder.CloseElement();
            }
            builder.CloseElement();
            builder.CloseElement();

            builder.OpenElement(30, "tbody");
            builder.AddAttribute(31, "class", "jazor-admin-release-table__body");
            if (Loading)
            {
                builder.OpenElement(32, "tr");
                builder.AddAttribute(33, "class", "jazor-admin-release-table__loading-row");

                builder.OpenElement(34, "td");
                builder.AddAttribute(35, "class", "jazor-admin-release-table__loading");
                builder.AddAttribute(36, "colspan", MultiSelectable ? columns.Length + 1 : columns.Length);
                builder.AddContent(37, NormalizedLoadingText);
                builder.CloseElement();

                builder.CloseElement();
            }
            else if (pageRows.Length > 0)
            {
                foreach (var row in pageRows)
                {
                    builder.OpenElement(32, "tr");
                    builder.AddAttribute(33, "class", BuildRowCssClass(RowIsSelected(row) || RowIsMultiSelected(row), row.Source.Disabled ?? false));
                    builder.AddAttribute(34, "data-row-key", row.Key);
                    builder.AddAttribute(35, "tabindex", 0);
                    builder.AddAttribute(36, "onclick", EventCallback.Factory.Create(this, () => OnRowSelected(row)));

                    if (MultiSelectable)
                    {
                        builder.OpenElement(37, "td");
                        builder.AddAttribute(38, "class", "jazor-admin-release-table__cell jazor-admin-release-table__selection-cell");

                        builder.OpenElement(39, "input");
                        builder.AddAttribute(40, "class", "jazor-admin-release-table__row-select");
                        builder.AddAttribute(41, "type", "checkbox");
                        builder.AddAttribute(42, "checked", RowIsMultiSelected(row));
                        builder.AddAttribute(43, "disabled", row.Source.Disabled ?? false);
                        builder.AddAttribute(44, "data-row-select-key", row.Key);
                        builder.AddAttribute(45, "aria-label", "Select row " + row.Key);
                        builder.AddAttribute(46, "onclick", EventCallback.Factory.Create(this, () => OnRowSelectionToggled(row)));
                        builder.CloseElement();

                        builder.CloseElement();
                    }

                    foreach (var column in columns)
                    {
                        builder.OpenElement(47, "td");
                        builder.AddAttribute(48, "class", "jazor-admin-release-table__cell");
                        builder.AddAttribute(49, "data-column-key", column.Key);
                        builder.AddContent(50, ResolveCellText(row, column));
                        builder.CloseElement();
                    }

                    builder.CloseElement();
                }
            }
            else
            {
                builder.OpenElement(50, "tr");
                builder.AddAttribute(51, "class", "jazor-admin-release-table__empty-row");

                builder.OpenElement(52, "td");
                builder.AddAttribute(53, "class", "jazor-admin-release-table__empty");
                builder.AddAttribute(54, "colspan", MultiSelectable ? columns.Length + 1 : columns.Length);
                builder.AddContent(55, NormalizedEmptyText);
                builder.CloseElement();

                builder.CloseElement();
            }
            builder.CloseElement();

            builder.CloseElement();
        }
        else
        {
            builder.OpenElement(40, "div");
            builder.AddAttribute(41, "class", Loading ? "jazor-admin-release-table__loading" : "jazor-admin-release-table__empty");
            builder.AddContent(42, Loading ? NormalizedLoadingText : NormalizedEmptyText);
            builder.CloseElement();
        }

        if (hasPagination)
        {
            builder.OpenElement(60, "div");
            builder.AddAttribute(61, "class", "jazor-admin-release-table__pagination");

            builder.OpenElement(62, "button");
            builder.AddAttribute(63, "type", "button");
            builder.AddAttribute(64, "class", "jazor-admin-release-table__page-button");
            builder.AddAttribute(65, "disabled", Loading || pageIndex == 0);
            builder.AddAttribute(66, "onclick", EventCallback.Factory.Create(this, OnPreviousPage));
            builder.AddContent(67, "Previous");
            builder.CloseElement();

            builder.OpenElement(68, "span");
            builder.AddAttribute(69, "class", "jazor-admin-release-table__page-status");
            builder.AddContent(70, pageIndex + 1);
            builder.AddContent(71, " / ");
            builder.AddContent(72, pageCount);
            builder.CloseElement();

            builder.OpenElement(73, "button");
            builder.AddAttribute(74, "type", "button");
            builder.AddAttribute(75, "class", "jazor-admin-release-table__page-button");
            builder.AddAttribute(76, "disabled", Loading || pageIndex >= pageCount - 1);
            builder.AddAttribute(77, "onclick", EventCallback.Factory.Create(this, OnNextPage));
            builder.AddContent(78, "Next");
            builder.CloseElement();

            builder.CloseElement();
        }

        builder.CloseElement();
    }

    private async Task OnSearchTextChanged(string value)
    {
        if (Loading)
        {
            return;
        }

        await SearchTextChanged.InvokeAsync(value);
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
