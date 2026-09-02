using ECMAScript;
using ECMAScript.Style;
using static ECMAScript.Style.css;

namespace RazorVue.Authoring;

[ECMAScriptModule("./components/authoring-styles")]
public static class AuthoringStyleSheet
{
    public static readonly string Page = style(new CssRule
    {
        display = grid,
        gap = px(24),
        width = percent(100),
        max_width = px(1120),
        margin = margin(px(32), auto),
        padding = px(24),
        color = hex("172033"),
        background_color = hex("f6f8fb"),
        font_family = font_family(font("Arial"), generic_font("sans-serif"))
    });

    public static readonly string Header = style(new CssRule
    {
        display = flex,
        align_items = flex_end,
        justify_content = space_between,
        gap = px(24),
        padding_bottom = px(8),
        border_bottom = px(1) | solid | hex("d8e0ea")
    });

    public static readonly string HeaderActions = style(new CssRule
    {
        display = flex,
        align_items = center,
        gap = px(12),
        color = hex("52606d"),
        font_size = px(13)
    });

    public static readonly string Eyebrow = style(new CssRule
    {
        margin = margin(px(0)),
        color = hex("087e8b"),
        font_size = px(12),
        font_weight = 700,
        text_transform = uppercase
    });

    public static readonly string Subtitle = style(new CssRule
    {
        margin = margin(px(6), px(0), px(0)),
        color = hex("52606d"),
        font_size = px(14)
    });

    public static readonly string Toolbar = style(new CssRule
    {
        display = flex,
        align_items = center,
        justify_content = space_between,
        gap = px(18),
        padding = px(16),
        background_color = hex("ffffff"),
        border = px(1) | solid | hex("d8e0ea"),
        border_radius = px(6)
    });

    public static readonly string Label = style(new CssRule
    {
        display = block,
        margin_bottom = px(4),
        color = hex("627d98"),
        font_size = px(12),
        font_weight = 700,
        text_transform = uppercase
    });

    public static readonly string TableSection = style(new CssRule
    {
        display = grid,
        gap = px(16),
        padding = px(20),
        background_color = hex("ffffff"),
        border = px(1) | solid | hex("d8e0ea"),
        border_radius = px(6)
    });

    public static readonly string SectionHeading = style(new CssRule
    {
        display = flex,
        align_items = flex_end,
        justify_content = space_between,
        gap = px(12)
    });

    public static readonly string Status = style(new CssRule
    {
        margin = margin(px(0)),
        color = hex("52606d"),
        font_size = px(13)
    });

    public static void EnsureLoaded() => _ = Page;
}
