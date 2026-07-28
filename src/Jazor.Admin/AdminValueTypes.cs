namespace Jazor.Admin;

[String]
public enum AdminLayoutMode
{
    [Description("@#sidebar")]
    Sidebar,

    [Description("@#top")]
    Top,

    [Description("@#mixed")]
    Mixed
}

[String]
public enum AdminThemeMode
{
    [Description("@#system")]
    System,

    [Description("@#light")]
    Light,

    [Description("@#dark")]
    Dark
}

[String]
public enum AdminPageActionKind
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
public sealed record AdminPageAction : VueProps
{
    [Description("@#key")]
    public string Key { get; init; } = string.Empty;

    [Description("@#text")]
    public string? Text { get; init; }

    [Description("@#kind")]
    public AdminPageActionKind? Kind { get; init; }

    [Description("@#disabled")]
    public bool? Disabled { get; init; }

    [Description("@#href")]
    public string? Href { get; init; }

    [Description("@#routeTarget")]
    public RouteLocationRaw? RouteTarget { get; init; }

    [Description("@#click")]
    public EventCallback Click { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record AdminBreadcrumbItem : VueProps
{
    [Description("@#key")]
    public string Key { get; init; } = string.Empty;

    [Description("@#title")]
    public string? Title { get; init; }

    [Description("@#href")]
    public string? Href { get; init; }

    [Description("@#routeTarget")]
    public RouteLocationRaw? RouteTarget { get; init; }

    [Description("@#disabled")]
    public bool? Disabled { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record AdminNavItem : VueProps
{
    [Description("@#key")]
    public string Key { get; init; } = string.Empty;

    [Description("@#title")]
    public string? Title { get; init; }

    [Description("@#icon")]
    public string? Icon { get; init; }

    [Description("@#href")]
    public string? Href { get; init; }

    [Description("@#routeTarget")]
    public RouteLocationRaw? RouteTarget { get; init; }

    [Description("@#disabled")]
    public bool? Disabled { get; init; }

    [Description("@#children")]
    public AdminNavItems? Children { get; init; }
}

[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(AdminNavItemsCollectionBuilder), nameof(AdminNavItemsCollectionBuilder.Create))]
public readonly union AdminNavItems(AdminNavItem[]) : IEnumerable<AdminNavItem>
{
    public AdminNavItem[]? AsArray => Value as AdminNavItem[];

    IEnumerator<AdminNavItem> IEnumerable<AdminNavItem>.GetEnumerator()
        => ((IEnumerable<AdminNavItem>)(AsArray ?? Array.Empty<AdminNavItem>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<AdminNavItem>)this).GetEnumerator();
}

public static class AdminNavItemsCollectionBuilder
{
    public static AdminNavItems Create(ReadOnlySpan<AdminNavItem> values)
        => values.ToArray();
}
