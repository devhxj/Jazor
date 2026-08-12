using ECMAScript.Style;
using static ECMAScript.Style.css;

namespace Jazor.Admin;

/// <summary>
/// Registers the native admin shell styles through ECMAScript.Style.
/// 通过 ECMAScript.Style 注册原生管理端框架样式，不再要求应用提供静态 CSS 文件。
/// </summary>
[ECMAScriptModule("./components/admin/styles")]
internal static class AdminStyleSheet
{
    private static readonly bool IsRegistered = Register();

    public static void EnsureLoaded()
    {
        _ = IsRegistered;
    }

    private static bool Register()
    {
        global(".ja-application", new CssRule
        {
            ["--app-bg"] = raw("#f5f7fa"),
            ["--surface"] = raw("#ffffff"),
            ["--surface-subtle"] = raw("#f8fafc"),
            ["--surface-strong"] = raw("#edf2f7"),
            ["--text"] = raw("#1f2937"),
            ["--text-muted"] = raw("#738092"),
            ["--border"] = raw("#e5eaf1"),
            ["--border-strong"] = raw("#ccd6e2"),
            ["--accent"] = raw("#2f6fed"),
            ["--accent-strong"] = raw("#1f5bd2"),
            ["--accent-soft"] = raw("#e9f1ff"),
            ["--danger"] = raw("#d84a4a"),
            ["--danger-soft"] = raw("#fff0f0"),
            ["--warning"] = raw("#d18a19"),
            ["--warning-soft"] = raw("#fff7e8"),
            ["--info"] = raw("#2f6fed"),
            ["--info-soft"] = raw("#e9f1ff"),
            ["--shadow"] = raw("0 4px 14px rgb(31 52 78 / 5%)"),
            min_height = raw("100vh"),
            background = raw("var(--app-bg)"),
            color = raw("var(--text)")
        });

        var darkTheme = new CssRule
        {
            color_scheme = raw("dark"),
            ["--app-bg"] = raw("#151a18"),
            ["--surface"] = raw("#1e2522"),
            ["--surface-subtle"] = raw("#242c29"),
            ["--surface-strong"] = raw("#2c3732"),
            ["--text"] = raw("#edf3f0"),
            ["--text-muted"] = raw("#aab8b1"),
            ["--border"] = raw("#39453f"),
            ["--border-strong"] = raw("#526159"),
            ["--accent"] = raw("#50c99a"),
            ["--accent-strong"] = raw("#78dab4"),
            ["--accent-soft"] = raw("#193e31"),
            ["--danger"] = raw("#ff8c86"),
            ["--danger-soft"] = raw("#492827"),
            ["--warning"] = raw("#f1c35d"),
            ["--warning-soft"] = raw("#45391f"),
            ["--info"] = raw("#8bbaf0"),
            ["--info-soft"] = raw("#24364c"),
            ["--shadow"] = raw("0 1px 2px rgb(0 0 0 / 28%), 0 10px 28px rgb(0 0 0 / 20%)")
        };
        global(".ja-application--dark", darkTheme);
        Media(".ja-application--system", "(prefers-color-scheme: dark)", darkTheme);
        global(".ja-application--grayscale", new CssRule
        {
            filter = raw("grayscale(1)")
        });

        global(".ja-shell", new CssRule
        {
            display = raw("grid"),
            grid_template_columns = raw("232px minmax(0, 1fr)"),
            min_height = raw("100vh")
        });
        global(".ja-shell--top", new CssRule
        {
            display = raw("block")
        });
        global(".ja-shell__sidebar", new CssRule
        {
            position = raw("sticky"),
            top = raw("0"),
            z_index = raw("20"),
            height = raw("100vh"),
            overflow = raw("auto"),
            background = raw("#17241f"),
            color = raw("#eef7f3"),
            border_right = px(1) | solid | hex("293a33")
        });
        global(".ja-shell--collapsed", new CssRule
        {
            grid_template_columns = raw("0 minmax(0, 1fr)")
        });
        global(".ja-shell--collapsed .ja-shell__sidebar", new CssRule
        {
            display = raw("none"),
            width = raw("0"),
            border_right = raw("0")
        });
        global(".ja-shell__main", new CssRule
        {
            min_width = raw("0")
        });
        global(".ja-shell__header", new CssRule
        {
            display = raw("flex"),
            align_items = raw("center"),
            position = raw("sticky"),
            top = raw("0"),
            z_index = raw("15"),
            min_height = raw("64px"),
            background = raw("color-mix(in srgb, var(--surface) 94%, transparent)"),
            border_bottom = px(1) | solid | var("--border"),
            backdrop_filter = raw("blur(12px)")
        });
        global(".ja-shell__sidebar-toggle", new CssRule
        {
            position = raw("relative"),
            flex = raw("0 0 36px"),
            width = raw("36px"),
            height = raw("36px"),
            padding = raw("0"),
            margin_left = raw("14px"),
            color = raw("var(--text)"),
            background = raw("transparent"),
            border = px(1) | solid | var("--border"),
            border_radius = raw("5px")
        });
        global(".ja-shell__sidebar-toggle::before", new CssRule
        {
            font_size = raw("20px"),
            line_height = raw("1"),
            content = raw("\"\\2630\"")
        });
        global(".ja-shell__sidebar-toggle:hover", new CssRule
        {
            background = raw("var(--surface-strong)")
        });
        global(".ja-shell__content", new CssRule
        {
            width = raw("100%")
        });

        global(".ja-sidebar", new CssRule
        {
            min_height = raw("100%"),
            padding = raw("20px 14px")
        });
        global(".ja-sidebar__logo", new CssRule
        {
            display = raw("flex"),
            align_items = raw("center"),
            min_height = raw("42px"),
            margin = raw("0 8px 22px"),
            color = raw("#ffffff"),
            font_size = raw("18px"),
            font_weight = raw("750")
        });
        global(".ja-sidebar__list, .ja-sidebar__children", new CssRule
        {
            padding = raw("0"),
            margin = raw("0"),
            list_style = raw("none")
        });
        global(".ja-sidebar__item", new CssRule
        {
            margin = raw("3px 0")
        });
        global(".ja-sidebar__item-content", new CssRule
        {
            position = raw("relative")
        });
        global(".ja-sidebar__link, .ja-sidebar__button", new CssRule
        {
            display = raw("flex"),
            align_items = raw("center"),
            width = raw("100%"),
            min_height = raw("40px"),
            padding = raw("8px 12px"),
            color = raw("#b8c8c1"),
            text_align = raw("left"),
            text_decoration = raw("none"),
            background = raw("transparent"),
            border = raw("0"),
            border_radius = raw("6px")
        });
        global(".ja-sidebar__link:hover, .ja-sidebar__button:hover, .ja-sidebar__item.is-ancestor-selected > .ja-sidebar__item-content > .ja-sidebar__button", new CssRule
        {
            color = raw("#ffffff"),
            background = raw("#243a31")
        });
        global(".ja-sidebar__item.is-selected > .ja-sidebar__item-content > .ja-sidebar__link, .ja-sidebar__item.is-selected > .ja-sidebar__item-content > .ja-sidebar__button", new CssRule
        {
            color = raw("#ffffff"),
            background = raw("#087f5b")
        });
        global(".ja-sidebar__toggle", new CssRule
        {
            margin_left = raw("auto"),
            font_size = raw("12px")
        });
        global(".ja-sidebar__children", new CssRule
        {
            padding = raw("4px 0 4px 12px")
        });

        global(".ja-header", new CssRule
        {
            display = raw("flex"),
            flex = raw("1 1 auto"),
            align_items = raw("center"),
            justify_content = raw("space-between"),
            min_width = raw("0"),
            min_height = raw("64px"),
            padding = raw("8px 24px"),
            gap = raw("20px")
        });
        global(".ja-header__main, .ja-header__actions, .ja-header__toolbar, .ja-header__user-region", new CssRule
        {
            display = raw("flex"),
            align_items = raw("center")
        });
        global(".ja-header__main", new CssRule
        {
            min_width = raw("0"),
            gap = raw("12px")
        });
        global(".ja-header__logo", new CssRule
        {
            color = raw("var(--accent)"),
            font_weight = raw("750")
        });
        global(".ja-header__titles", new CssRule
        {
            min_width = raw("0")
        });
        global(".ja-header__title", new CssRule
        {
            overflow = raw("hidden"),
            font_size = raw("16px"),
            font_weight = raw("700"),
            text_overflow = raw("ellipsis"),
            white_space = raw("nowrap")
        });
        global(".ja-header__subtitle", new CssRule
        {
            margin_top = raw("2px"),
            color = raw("var(--text-muted)"),
            font_size = raw("12px")
        });
        global(".ja-header__actions", new CssRule
        {
            justify_content = raw("flex-end"),
            min_width = raw("0"),
            gap = raw("16px")
        });
        global(".ja-header__navigation", new CssRule
        {
            flex = raw("1 1 auto"),
            min_width = raw("0")
        });

        global(".ja-page", new CssRule
        {
            width = raw("min(100%, 1480px)"),
            margin = raw("0 auto"),
            padding = raw("24px")
        });
        global(".ja-page__header", new CssRule
        {
            display = raw("flex"),
            align_items = raw("flex-end"),
            justify_content = raw("space-between"),
            margin_bottom = raw("20px"),
            gap = raw("20px")
        });
        global(".ja-page__titles", new CssRule
        {
            min_width = raw("0")
        });
        global(".ja-page__title", new CssRule
        {
            margin = raw("0"),
            font_size = raw("26px"),
            line_height = raw("1.25")
        });
        global(".ja-page__subtitle", new CssRule
        {
            max_width = raw("760px"),
            margin = raw("7px 0 0"),
            color = raw("var(--text-muted)"),
            line_height = raw("1.5")
        });
        global(".ja-page__actions", new CssRule
        {
            display = raw("flex"),
            flex_wrap = raw("wrap"),
            justify_content = raw("flex-end"),
            gap = raw("8px")
        });
        global(".ja-page__body > * + *", new CssRule
        {
            margin_top = raw("20px")
        });

        Media(".ja-shell", "(max-width: 760px)", new CssRule
        {
            display = raw("block"),
            min_width = raw("0")
        });
        Media(".ja-shell__sidebar", "(max-width: 760px)", new CssRule
        {
            position = raw("static"),
            height = raw("auto"),
            overflow = raw("visible"),
            border_right = raw("0"),
            border_bottom = px(1) | solid | hex("293a33")
        });
        Media(".ja-sidebar", "(max-width: 760px)", new CssRule
        {
            min_height = raw("0"),
            padding = raw("10px 12px")
        });
        Media(".ja-page", "(max-width: 760px)", new CssRule
        {
            padding = raw("18px 14px 28px")
        });
        return true;
    }

    private static void Media(string selector, string prelude, CssRule rule)
        => global(selector, new CssRule { children = [new(ChildKind.Media, prelude, rule)] });
}
