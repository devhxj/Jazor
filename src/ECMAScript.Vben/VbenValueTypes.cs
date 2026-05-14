namespace ECMAScript.Vben;

[String]
public enum VbenLayoutMode
{
    [Description("@#sidebar")]
    Sidebar,

    [Description("@#top")]
    Top,

    [Description("@#mixed")]
    Mixed
}

[String]
public enum VbenPageActionKind
{
    [Description("@#default")]
    Default,

    [Description("@#primary")]
    Primary,

    [Description("@#secondary")]
    Secondary,

    [Description("@#link")]
    Link,

    [Description("@#danger")]
    Danger
}

[ECMAScript]
[Description("@#")]
public sealed record VbenRouteLocation : VueProps
{
    [Description("@#path")]
    public string? Path { get; init; }

    [Description("@#name")]
    public string? Name { get; init; }

    [Description("@#hash")]
    public string? Hash { get; init; }
}

[ECMAScript]
[Union]
[Description("@#")]
public readonly struct VbenNavTarget : IUnion
{
    private readonly byte _kind;
    private readonly string? _href;
    private readonly VbenRouteLocation? _route;

    private VbenNavTarget(string value)
    {
        _kind = 1;
        _href = value;
        _route = default;
    }

    private VbenNavTarget(VbenRouteLocation value)
    {
        _kind = 2;
        _href = default;
        _route = value;
    }

    public string? AsHref => _kind == 1 ? _href : default;

    public VbenRouteLocation? AsRoute => _kind == 2 ? _route : default;

    public object? Value => _kind switch
    {
        1 => AsHref,
        2 => AsRoute,
        _ => default
    };

    public extern static VbenNavTarget From(string value);

    public extern static VbenNavTarget From(VbenRouteLocation value);

    public static implicit operator VbenNavTarget(string value)
        => new(value);

    public static implicit operator VbenNavTarget(VbenRouteLocation value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public sealed record VbenPageAction : VueProps
{
    [Description("@#key")]
    public string Key { get; init; } = string.Empty;

    [Description("@#text")]
    public string? Text { get; init; }

    [Description("@#kind")]
    public VbenPageActionKind? Kind { get; init; }

    [Description("@#disabled")]
    public bool? Disabled { get; init; }

    [Description("@#href")]
    public VbenNavTarget? Target { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record VbenBreadcrumbItem : VueProps
{
    [Description("@#key")]
    public string Key { get; init; } = string.Empty;

    [Description("@#title")]
    public string? Title { get; init; }

    [Description("@#target")]
    public VbenNavTarget? Target { get; init; }

    [Description("@#disabled")]
    public bool? Disabled { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record VbenNavItem : VueProps
{
    [Description("@#key")]
    public string Key { get; init; } = string.Empty;

    [Description("@#title")]
    public string? Title { get; init; }

    [Description("@#icon")]
    public string? Icon { get; init; }

    [Description("@#target")]
    public VbenNavTarget? Target { get; init; }

    [Description("@#disabled")]
    public bool? Disabled { get; init; }

    [Description("@#children")]
    public VbenNavItems? Children { get; init; }
}

[CollectionBuilder(typeof(VbenNavItemsCollectionBuilder), nameof(VbenNavItemsCollectionBuilder.Create))]
public readonly union VbenNavItems(VbenNavItem[]) : IEnumerable<VbenNavItem>
{
    public VbenNavItem[]? AsArray => Value as VbenNavItem[];

    IEnumerator<VbenNavItem> IEnumerable<VbenNavItem>.GetEnumerator()
        => ((IEnumerable<VbenNavItem>)(AsArray ?? Array.Empty<VbenNavItem>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VbenNavItem>)this).GetEnumerator();
}

public static class VbenNavItemsCollectionBuilder
{
    public static VbenNavItems Create(ReadOnlySpan<VbenNavItem> values)
        => values.ToArray();
}
