namespace Jazor.Admin;

[ECMAScriptModule("components/jazor-admin-nav-item-render-helper.mjs")]
internal static class AdminNavItemRenderHelper
{
    public static EffectiveNavItem[] BuildEffectiveItems(AdminNavItem[]? items)
    {
        if (items is not { Length: > 0 })
        {
            return Array.Empty<EffectiveNavItem>();
        }

        var seenKeys = new HashSet<string>();
        var effectiveItems = new List<EffectiveNavItem>(items.Length);
        foreach (var item in items)
        {
            if (TryBuildEffectiveItem(item, seenKeys, out var effectiveItem))
            {
                effectiveItems.Add(effectiveItem);
            }
        }

        return effectiveItems.Count == 0
            ? Array.Empty<EffectiveNavItem>()
            : effectiveItems.ToArray();
    }

    public static AdminNavItem[] FilterRenderableItems(AdminNavItem[]? items)
    {
        if (items is not { Length: > 0 })
        {
            return Array.Empty<AdminNavItem>();
        }

        List<AdminNavItem>? filtered = null;
        for (var index = 0; index < items.Length; index++)
        {
            var item = items[index];
            if (!IsRenderable(item))
            {
                if (filtered is null)
                {
                    filtered = new List<AdminNavItem>(items.Length - 1);
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
            ? Array.Empty<AdminNavItem>()
            : filtered.ToArray();
    }

    private static bool HasRenderableItems(AdminNavItem[]? items)
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

    private static bool IsRenderable(AdminNavItem? item)
        => item is not null
           && !string.IsNullOrWhiteSpace(item.Title)
           && AdminNavigationKeyHelper.Normalize(item.Key) is not null;

    private static bool TryBuildEffectiveItem(
        AdminNavItem? item,
        HashSet<string> seenKeys,
        out EffectiveNavItem effectiveItem)
    {
        effectiveItem = null!;
        if (!IsRenderable(item))
        {
            return false;
        }

        var source = item!;
        var key = AdminNavigationKeyHelper.Normalize(source.Key);
        var title = AdminDisplayTextHelper.Normalize(source.Title);
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

    private static EffectiveNavItem[] BuildEffectiveItems(
        AdminNavItem[]? items,
        HashSet<string> seenKeys)
    {
        if (items is not { Length: > 0 })
        {
            return Array.Empty<EffectiveNavItem>();
        }

        var effectiveItems = new List<EffectiveNavItem>(items.Length);
        foreach (var item in items)
        {
            if (TryBuildEffectiveItem(item, seenKeys, out var effectiveItem))
            {
                effectiveItems.Add(effectiveItem);
            }
        }

        return effectiveItems.Count == 0
            ? Array.Empty<EffectiveNavItem>()
            : effectiveItems.ToArray();
    }

    [ECMAScript]
    internal sealed class EffectiveNavItem
    {
        public EffectiveNavItem(
            AdminNavItem source,
            string key,
            string title,
            EffectiveNavItem[] children)
        {
            Source = source;
            Key = key;
            Title = title;
            Children = children;
        }

        public AdminNavItem Source { get; }

        public string Key { get; }

        public string Title { get; }

        public EffectiveNavItem[] Children { get; }
    }
}
