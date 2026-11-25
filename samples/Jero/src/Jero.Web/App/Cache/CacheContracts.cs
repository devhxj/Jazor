using System;

namespace Jero.Web.App.Cache;

public sealed record CacheValuePreview
{
    public string typeLabel { get; init; } = "unknown";
    public string sizeLabel { get; init; } = string.Empty;
    public string displayText { get; init; } = string.Empty;
}

public sealed record CacheTreeNode
{
    public string id { get; init; } = string.Empty;
    public string label { get; init; } = string.Empty;
    public string prefixKey { get; init; } = string.Empty;
    public string cacheKey { get; set; } = string.Empty;
    public int depth { get; init; }
    public int keyCount { get; set; }
}

public sealed record CachePageModel
{
    public required string? Error { get; init; }
    public required bool CanQuery { get; init; }
    public required bool CanClear { get; init; }
    public required bool CanDelete { get; init; }
    public required bool LoadingKeys { get; init; }
    public required bool LoadingValue { get; init; }
    public required bool DeletingKey { get; init; }
    public required bool DeletingPrefix { get; init; }
    public required bool Clearing { get; init; }
    public required CacheTreeNode[] VisibleNodes { get; init; }
    public required CacheTreeNode? SelectedNode { get; init; }
    public required string KeyFilter { get; init; }
    public required string[] SelectedKeys { get; init; }
    public required bool ValueLoaded { get; init; }
    public required string LoadedValueKey { get; init; }
    public required CacheValuePreview ValuePreview { get; init; }
    public required bool HasSelectedKey { get; init; }
    public required bool HasSelectedPrefixChildren { get; init; }
    public required string DetailTitle { get; init; }
    public required string PrefixDeleteTitle { get; init; }
    public required Action DismissError { get; init; }
    public required Action LoadKeys { get; init; }
    public required Action ClearAll { get; init; }
    public required Action<string> FilterChanged { get; init; }
    public required Action<string> SelectPrefix { get; init; }
    public required Action RefreshValue { get; init; }
    public required Action DeleteSelectedKey { get; init; }
    public required Action DeleteSelectedPrefix { get; init; }
}
