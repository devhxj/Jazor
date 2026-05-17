namespace ECMAScript.Vben;

internal static class VbenNavItemRenderHelper
{
    public static bool HasRenderableItems(VbenNavItems? items)
        => HasRenderableItems(items?.AsArray);

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
           && !string.IsNullOrWhiteSpace(item.Title);
}
