using ECMAScript;
using ECMAScript.Style;
using static ECMAScript.Style.css;

namespace Todo.Library;

/// <summary>Registers every visual rule used by the TODOList through ECMAScript.Style.</summary>
[ECMAScriptModule("./components/todo-styles")]
public static class TodoStyleSheet
{
    public static readonly string Application = style(new CssRule
    {
        display = grid,
        gap = px(20),
        width = percent(100),
        max_width = px(760),
        margin = margin(px(40), auto),
        padding = px(24),
        color = hex("172033"),
        background_color = hex("f7fafc"),
        font_family = font_family(font("Arial"), generic_font("sans-serif"))
    });

    public static readonly string Header = style(new CssRule
    {
        display = flex,
        align_items = flex_start,
        justify_content = space_between,
        gap = px(16)
    });

    public static readonly string Eyebrow = style(new CssRule
    {
        margin = margin(px(0)),
        color = hex("087e8b"),
        font_size = px(13),
        font_weight = 700
    });

    public static readonly string Title = style(new CssRule
    {
        margin = margin(px(4), px(0)),
        color = hex("172033"),
        font_size = px(28),
        line_height = px(34)
    });

    public static readonly string Subtitle = style(new CssRule
    {
        margin = margin(px(0)),
        color = hex("52606d"),
        font_size = px(14)
    });

    public static readonly string LogicMarker = style(new CssRule
    {
        padding = padding(px(5), px(8)),
        color = hex("4a294f"),
        background_color = hex("f8e8f5"),
        border_radius = px(4),
        font_size = px(12),
        font_weight = 700
    });

    public static readonly string Composer = style(new CssRule
    {
        display = flex,
        align_items = end,
        gap = px(12),
        padding = px(16),
        background_color = hex("ffffff"),
        border = px(1) | solid | hex("d9e2ec"),
        border_radius = px(6)
    });

    public static readonly string Field = style(new CssRule
    {
        display = grid,
        // CSS flex:auto is the canonical shorthand for 1 1 auto.
        flex = auto,
        gap = px(6),
        color = hex("334e68"),
        font_size = px(13),
        font_weight = 700,
        children =
        [
            new(ChildKind.Selector, "& input", new CssRule
            {
                width = percent(100),
                min_height = px(38),
                padding = padding(px(8), px(10)),
                color = hex("172033"),
                background_color = hex("ffffff"),
                border = px(1) | solid | hex("bcccdc"),
                border_radius = px(4),
                font_size = px(15)
            }),
            new(ChildKind.Selector, "& input:focus", new CssRule
            {
                border = px(1) | solid | hex("087e8b"),
                outline = px(2) | solid | hex("b8f2e6")
            })
        ]
    });

    public static readonly string AddButton = style(new CssRule
    {
        min_height = px(38),
        padding = padding(px(8), px(14)),
        color = hex("ffffff"),
        background_color = hex("087e8b"),
        border = none,
        border_radius = px(4),
        cursor = pointer,
        font_size = px(14),
        font_weight = 700,
        children =
        [
            new(ChildKind.Selector, "&:hover", new CssRule { background_color = hex("05636d") }),
            new(ChildKind.Selector, "&:focus-visible", new CssRule { outline = px(2) | solid | hex("172033"), outline_offset = px(2) })
        ]
    });

    public static readonly string Summary = style(new CssRule
    {
        display = grid,
        grid_template_columns = tracks(repeat(3, min_max(px(0), fr(1)))),
        gap = px(12),
        children =
        [
            new(ChildKind.Selector, "& > div", new CssRule
            {
                display = grid,
                gap = px(3),
                padding = px(14),
                background_color = hex("eaf4f4"),
                border_radius = px(6)
            }),
            new(ChildKind.Selector, "& span", new CssRule { color = hex("52606d"), font_size = px(12), font_weight = 700 }),
            new(ChildKind.Selector, "& strong", new CssRule { color = hex("102a43"), font_size = px(22) })
        ]
    });

    public static readonly string ListPanel = style(new CssRule
    {
        padding = px(16),
        background_color = hex("ffffff"),
        border = px(1) | solid | hex("d9e2ec"),
        border_radius = px(6)
    });

    public static readonly string List = style(new CssRule
    {
        display = grid,
        gap = px(8),
        margin = margin(px(0)),
        padding = px(0),
        list_style = none
    });

    public static readonly string Task = style(new CssRule
    {
        display = flex,
        align_items = center,
        justify_content = space_between,
        gap = px(12),
        padding = padding(px(10), px(12)),
        background_color = hex("f7fafc"),
        border_radius = px(4)
    });

    public static readonly string TaskDone = style(new CssRule
    {
        display = flex,
        align_items = center,
        justify_content = space_between,
        gap = px(12),
        padding = padding(px(10), px(12)),
        color = hex("627d98"),
        background_color = hex("edf2f7"),
        border_radius = px(4),
        children =
        [
            new(ChildKind.Selector, "& span", new CssRule { text_decoration_line = keyword("line-through") })
        ]
    });

    public static readonly string TaskLabel = style(new CssRule
    {
        display = inline_flex,
        align_items = center,
        gap = px(10),
        cursor = pointer,
        font_size = px(15)
    });

    public static readonly string TaskState = style(new CssRule
    {
        padding = padding(px(3), px(7)),
        color = hex("334e68"),
        background_color = hex("d9e2ec"),
        border_radius = px(4),
        font_size = px(12),
        font_weight = 700
    });

    /// <summary>Evaluates the module before Vue mounts so all generated classes are registered.</summary>
    public static void EnsureLoaded() => _ = Application;
}
