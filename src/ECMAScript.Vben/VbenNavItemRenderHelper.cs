namespace ECMAScript.Vben;

internal static class VbenNavItemRenderHelper
{
    public static bool HasRenderableItems(VbenNavItems? items)
        => BuildEffectiveItems(items).Length > 0;

    public static VbenEffectiveNavItem[] BuildEffectiveItems(VbenNavItems? items)
        => BuildEffectiveItems(items?.AsArray);

    public static VbenEffectiveNavItem[] BuildEffectiveItems(VbenNavItem[]? items)
    {
        if (items is not { Length: > 0 })
        {
            return Array.Empty<VbenEffectiveNavItem>();
        }

        var seenKeys = new HashSet<string>(StringComparer.Ordinal);
        var effectiveItems = new List<VbenEffectiveNavItem>(items.Length);
        foreach (var item in items)
        {
            if (TryBuildEffectiveItem(item, seenKeys, out var effectiveItem))
            {
                effectiveItems.Add(effectiveItem);
            }
        }

        return effectiveItems.Count == 0
            ? Array.Empty<VbenEffectiveNavItem>()
            : effectiveItems.ToArray();
    }

    public static VbenNavItem[] FilterRenderableItems(VbenNavItems? items)
        => FilterRenderableItems(items?.AsArray);

    public static VbenNavItem[] FilterRenderableItems(VbenNavItem[]? items)
    {
        if (items is not { Length: > 0 })
        {
            return Array.Empty<VbenNavItem>();
        }

        List<VbenNavItem>? filtered = null;
        for (var index = 0; index < items.Length; index++)
        {
            var item = items[index];
            if (!IsRenderable(item))
            {
                if (filtered is null)
                {
                    filtered = new List<VbenNavItem>(items.Length - 1);
                    for (var copyIndex = 0; copyIndex < index; copyIndex++)
                    {
                        if (IsRenderable(items[copyIndex]))
                        {
                            filtered.Add(items[copyIndex]!);
                        }
                    }
                }

                continue;
            }

            filtered?.Add(item);
        }

        if (filtered is null)
        {
            return items;
        }

        return filtered.Count == 0
            ? Array.Empty<VbenNavItem>()
            : filtered.ToArray();
    }

    private static bool HasRenderableItems(VbenNavItem[]? items)
    {
        if (items is not { Length: > 0 })
        {
            return false;
        }

        foreach (var item in items)
        {
            if (IsRenderable(item))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsRenderable(VbenNavItem? item)
        => item is not null
           && !string.IsNullOrWhiteSpace(item.Title)
           && VbenNavigationKeyHelper.Normalize(item.Key) is not null;

    private static bool TryBuildEffectiveItem(
        VbenNavItem? item,
        HashSet<string> seenKeys,
        out VbenEffectiveNavItem effectiveItem)
    {
        effectiveItem = null!;
        if (!IsRenderable(item))
        {
            return false;
        }

        var source = item!;
        var key = VbenNavigationKeyHelper.Normalize(source.Key);
        var title = VbenDisplayTextHelper.Normalize(source.Title);
        if (key is null || title is null || !seenKeys.Add(key))
        {
            return false;
        }

        effectiveItem = new(
            source,
            key,
            title,
            BuildEffectiveItems(source.Children?.AsArray, seenKeys));
        return true;
    }

    private static VbenEffectiveNavItem[] BuildEffectiveItems(
        VbenNavItem[]? items,
        HashSet<string> seenKeys)
    {
        if (items is not { Length: > 0 })
        {
            return Array.Empty<VbenEffectiveNavItem>();
        }

        var effectiveItems = new List<VbenEffectiveNavItem>(items.Length);
        foreach (var item in items)
        {
            if (TryBuildEffectiveItem(item, seenKeys, out var effectiveItem))
            {
                effectiveItems.Add(effectiveItem);
            }
        }

        return effectiveItems.Count == 0
            ? Array.Empty<VbenEffectiveNavItem>()
            : effectiveItems.ToArray();
    }
}

internal sealed class VbenEffectiveNavItem(
    VbenNavItem source,
    string key,
    string title,
    VbenEffectiveNavItem[] children)
{
    public VbenNavItem Source { get; } = source;

    public string Key { get; } = key;

    public string Title { get; } = title;

    public VbenEffectiveNavItem[] Children { get; } = children;
}
