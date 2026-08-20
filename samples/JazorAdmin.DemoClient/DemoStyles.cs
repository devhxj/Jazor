using ECMAScript;
using ECMAScript.Style;
using static ECMAScript.Style.css;

namespace JazorAdmin.DemoClient;

[ECMAScriptModule("components/demo-styles")]
internal static class DemoStyles
{
    public static readonly string Application = style(new CssRule
    {
        display = grid,
        gap = px(24),
        min_height = vh(100),
        padding = padding(px(32), px(40)),
        color = hex("1d2129"),
        background_color = hex("f5f7fa"),
        font_family = raw("Inter, ui-sans-serif, system-ui, sans-serif")
    });

    public static readonly string Header = style(new CssRule
    {
        display = flex,
        align_items = center,
        justify_content = space_between,
        gap = px(24),
        max_width = px(1180),
        width = percent(100),
        margin = margin(auto),
        children =
        [
            new(ChildKind.Selector, "& h1", new CssRule { margin = margin(px(4), px(0)), font_size = px(32) }),
            new(ChildKind.Selector, "& p", new CssRule { margin = margin(px(0)) })
        ]
    });

    public static readonly string Eyebrow = style(new CssRule
    {
        color = hex("0052d9"),
        font_size = px(12),
        font_weight = 700,
        text_transform = uppercase,
        letter_spacing = px(1)
    });

    public static readonly string Subtitle = style(new CssRule { color = hex("5f6875"), font_size = px(14) });

    public static readonly string Action = style(new CssRule
    {
        display = inline_flex,
        align_items = center,
        justify_content = center,
        gap = px(8),
        min_height = px(36),
        padding = padding(px(8), px(14)),
        color = hex("0052d9"),
        border = px(1) | solid | hex("0052d9"),
        border_radius = px(3),
        text_decoration = none,
        font_weight = 600,
        children =
        [
            new(ChildKind.Selector, "&:hover", new CssRule { background_color = hex("e8f3ff") }),
            new(ChildKind.Selector, "&:focus-visible", new CssRule { outline = px(2) | solid | hex("0052d9"), outline_offset = px(2) })
        ]
    });

    public static readonly string PrimaryAction = style(new CssRule
    {
        color = hex("ffffff"),
        background_color = hex("0052d9"),
        children = [new(ChildKind.Selector, "&:hover", new CssRule { background_color = hex("003cab") })]
    });

    public static readonly string Anonymous = style(new CssRule
    {
        display = flex,
        align_items = center,
        gap = px(18),
        max_width = px(1180),
        width = percent(100),
        margin = margin(auto),
        padding = px(28),
        background_color = hex("ffffff"),
        border = px(1) | solid | hex("e7e7e7"),
        border_radius = px(4),
        children =
        [
            new(ChildKind.Selector, "& > div", new CssRule { flex = raw("1") }),
            new(ChildKind.Selector, "& p", new CssRule { color = hex("5f6875") })
        ]
    });

    public static readonly string IdentityStrip = style(new CssRule
    {
        display = flex,
        align_items = center,
        gap = px(12),
        max_width = px(1180),
        width = percent(100),
        margin = margin(auto),
        padding = padding(px(12), px(16)),
        background_color = hex("ffffff"),
        border = px(1) | solid | hex("e7e7e7"),
        border_radius = px(4),
        children =
        [
            new(ChildKind.Selector, "& > div:nth-child(2)", new CssRule { display = grid, flex = raw("1") }),
            new(ChildKind.Selector, "& span", new CssRule { color = hex("5f6875"), font_size = px(12) })
        ]
    });

    public static readonly string IdentityIcon = style(new CssRule { color = hex("0052d9") });

    public static readonly string Cards = style(new CssRule
    {
        display = grid,
        grid_template_columns = tracks(repeat(3, min_max(px(0), fr(1)))),
        gap = px(16),
        max_width = px(1180),
        width = percent(100),
        margin = margin(auto)
    });

    public static readonly string CardBody = style(new CssRule
    {
        display = grid,
        gap = px(8),
        color = hex("5f6875"),
        children = [new(ChildKind.Selector, "& strong", new CssRule { color = hex("1d2129"), font_size = px(20) })]
    });

    public static readonly string Metrics = style(new CssRule
    {
        display = grid,
        grid_template_columns = tracks(repeat(3, min_max(px(0), fr(1)))),
        gap = px(10),
        children =
        [
            new(ChildKind.Selector, "& span", new CssRule { display = grid, gap = px(4), padding = px(8), background_color = hex("f5f7fa") }),
            new(ChildKind.Selector, "& small", new CssRule { color = hex("5f6875") }),
            new(ChildKind.Selector, "& strong", new CssRule { font_size = px(22) })
        ]
    });

    public static void EnsureLoaded() => _ = _registered;

    private static readonly bool _registered = Register();

    private static bool Register()
    {
        global("html, body, #app", new CssRule { min_height = percent(100), margin = px(0) });
        global("a", new CssRule { color = inherit });
        return true;
    }
}
