// WikiStyleSheet.cs - H 函数页面的 ECMAScript.Style 规则 / ECMAScript.Style rules for H-function pages

using ECMAScript;
using ECMAScript.Style;
using static ECMAScript.Style.css;

namespace Wiki;

/// <summary>
/// Registers the small component-scoped style surface owned by the H-function documentation page.
/// 通过 ECMAScript.Style 注册 H 函数文档页拥有的小型组件作用域样式。
/// </summary>
[ECMAScriptModule("components/wiki-styles.mjs")]
internal static class WikiStyleSheet
{
    // Keep this as a generated class rule rather than a site.css selector so the Wiki exercises
    // the deterministic CSS-in-JS path in its normal browser entry module.
    // 使用生成 class rule 而非 site.css 选择器，确保 Wiki 的正常浏览器入口真实覆盖确定性 CSS-in-JS 路径。
    public static readonly string HFunctionBadge = style(new CssRule
    {
        display = inline_flex,
        align_items = center,
        gap = px(6),
        padding = padding(px(4), px(8)),
        border_radius = px(4),
        background_color = hex("143d4f"),
        color = hex("d6f4ff"),
        font_size = px(12),
        font_weight = 600,
        line_height = px(16),
        children =
        [
            new(ChildKind.Selector, "&:focus-visible", new CssRule
            {
                outline = raw("2px solid #8bdfff"),
                outline_offset = px(2)
            })
        ]
    });

    /// <summary>Forces the generated style module to evaluate before Vue mounts the Wiki shell.</summary>
    public static void EnsureLoaded()
    {
        _ = HFunctionBadge;
    }
}
