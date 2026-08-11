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
            MinHeight = raw("100vh"),
            Background = raw("var(--app-bg)"),
            Color = raw("var(--text)")
        });

        var darkTheme = new CssRule
        {
            ColorScheme = raw("dark"),
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
            Filter = raw("grayscale(1)")
        });

        global(".ja-shell", new CssRule
        {
            Display = raw("grid"),
            GridTemplateColumns = raw("232px minmax(0, 1fr)"),
            MinHeight = raw("100vh")
        });
        global(".ja-shell--top", new CssRule
        {
            Display = raw("block")
        });
        global(".ja-shell__sidebar", new CssRule
        {
            Position = raw("sticky"),
            Top = raw("0"),
            ZIndex = raw("20"),
            Height = raw("100vh"),
            Overflow = raw("auto"),
            Background = raw("#17241f"),
            Color = raw("#eef7f3"),
            BorderRight = px(1) | solid | hex("293a33")
        });
        global(".ja-shell--collapsed", new CssRule
        {
            GridTemplateColumns = raw("0 minmax(0, 1fr)")
        });
        global(".ja-shell--collapsed .ja-shell__sidebar", new CssRule
        {
            Display = raw("none"),
            Width = raw("0"),
            BorderRight = raw("0")
        });
        global(".ja-shell__main", new CssRule
        {
            MinWidth = raw("0")
        });
        global(".ja-shell__header", new CssRule
        {
            Display = raw("flex"),
            AlignItems = raw("center"),
            Position = raw("sticky"),
            Top = raw("0"),
            ZIndex = raw("15"),
            MinHeight = raw("64px"),
            Background = raw("color-mix(in srgb, var(--surface) 94%, transparent)"),
            BorderBottom = px(1) | solid | var("--border"),
            BackdropFilter = raw("blur(12px)")
        });
        global(".ja-shell__sidebar-toggle", new CssRule
        {
            Position = raw("relative"),
            Flex = raw("0 0 36px"),
            Width = raw("36px"),
            Height = raw("36px"),
            Padding = raw("0"),
            MarginLeft = raw("14px"),
            Color = raw("var(--text)"),
            Background = raw("transparent"),
            Border = px(1) | solid | var("--border"),
            BorderRadius = raw("5px")
        });
        global(".ja-shell__sidebar-toggle::before", new CssRule
        {
            FontSize = raw("20px"),
            LineHeight = raw("1"),
            Content = raw("\"\\2630\"")
        });
        global(".ja-shell__sidebar-toggle:hover", new CssRule
        {
            Background = raw("var(--surface-strong)")
        });
        global(".ja-shell__content", new CssRule
        {
            Width = raw("100%")
        });

        global(".ja-sidebar", new CssRule
        {
            MinHeight = raw("100%"),
            Padding = raw("20px 14px")
        });
        global(".ja-sidebar__logo", new CssRule
        {
            Display = raw("flex"),
            AlignItems = raw("center"),
            MinHeight = raw("42px"),
            Margin = raw("0 8px 22px"),
            Color = raw("#ffffff"),
            FontSize = raw("18px"),
            FontWeight = raw("750")
        });
        global(".ja-sidebar__list, .ja-sidebar__children", new CssRule
        {
            Padding = raw("0"),
            Margin = raw("0"),
            ListStyle = raw("none")
        });
        global(".ja-sidebar__item", new CssRule
        {
            Margin = raw("3px 0")
        });
        global(".ja-sidebar__item-content", new CssRule
        {
            Position = raw("relative")
        });
        global(".ja-sidebar__link, .ja-sidebar__button", new CssRule
        {
            Display = raw("flex"),
            AlignItems = raw("center"),
            Width = raw("100%"),
            MinHeight = raw("40px"),
            Padding = raw("8px 12px"),
            Color = raw("#b8c8c1"),
            TextAlign = raw("left"),
            TextDecoration = raw("none"),
            Background = raw("transparent"),
            Border = raw("0"),
            BorderRadius = raw("6px")
        });
        global(".ja-sidebar__link:hover, .ja-sidebar__button:hover, .ja-sidebar__item.is-ancestor-selected > .ja-sidebar__item-content > .ja-sidebar__button", new CssRule
        {
            Color = raw("#ffffff"),
            Background = raw("#243a31")
        });
        global(".ja-sidebar__item.is-selected > .ja-sidebar__item-content > .ja-sidebar__link, .ja-sidebar__item.is-selected > .ja-sidebar__item-content > .ja-sidebar__button", new CssRule
        {
            Color = raw("#ffffff"),
            Background = raw("#087f5b")
        });
        global(".ja-sidebar__toggle", new CssRule
        {
            MarginLeft = raw("auto"),
            FontSize = raw("12px")
        });
        global(".ja-sidebar__children", new CssRule
        {
            Padding = raw("4px 0 4px 12px")
        });

        global(".ja-header", new CssRule
        {
            Display = raw("flex"),
            Flex = raw("1 1 auto"),
            AlignItems = raw("center"),
            JustifyContent = raw("space-between"),
            MinWidth = raw("0"),
            MinHeight = raw("64px"),
            Padding = raw("8px 24px"),
            Gap = raw("20px")
        });
        global(".ja-header__main, .ja-header__actions, .ja-header__toolbar, .ja-header__user-region", new CssRule
        {
            Display = raw("flex"),
            AlignItems = raw("center")
        });
        global(".ja-header__main", new CssRule
        {
            MinWidth = raw("0"),
            Gap = raw("12px")
        });
        global(".ja-header__logo", new CssRule
        {
            Color = raw("var(--accent)"),
            FontWeight = raw("750")
        });
        global(".ja-header__titles", new CssRule
        {
            MinWidth = raw("0")
        });
        global(".ja-header__title", new CssRule
        {
            Overflow = raw("hidden"),
            FontSize = raw("16px"),
            FontWeight = raw("700"),
            TextOverflow = raw("ellipsis"),
            WhiteSpace = raw("nowrap")
        });
        global(".ja-header__subtitle", new CssRule
        {
            MarginTop = raw("2px"),
            Color = raw("var(--text-muted)"),
            FontSize = raw("12px")
        });
        global(".ja-header__actions", new CssRule
        {
            JustifyContent = raw("flex-end"),
            MinWidth = raw("0"),
            Gap = raw("16px")
        });
        global(".ja-header__navigation", new CssRule
        {
            Flex = raw("1 1 auto"),
            MinWidth = raw("0")
        });

        global(".ja-page", new CssRule
        {
            Width = raw("min(100%, 1480px)"),
            Margin = raw("0 auto"),
            Padding = raw("24px")
        });
        global(".ja-page__header", new CssRule
        {
            Display = raw("flex"),
            AlignItems = raw("flex-end"),
            JustifyContent = raw("space-between"),
            MarginBottom = raw("20px"),
            Gap = raw("20px")
        });
        global(".ja-page__titles", new CssRule
        {
            MinWidth = raw("0")
        });
        global(".ja-page__title", new CssRule
        {
            Margin = raw("0"),
            FontSize = raw("26px"),
            LineHeight = raw("1.25")
        });
        global(".ja-page__subtitle", new CssRule
        {
            MaxWidth = raw("760px"),
            Margin = raw("7px 0 0"),
            Color = raw("var(--text-muted)"),
            LineHeight = raw("1.5")
        });
        global(".ja-page__actions", new CssRule
        {
            Display = raw("flex"),
            FlexWrap = raw("wrap"),
            JustifyContent = raw("flex-end"),
            Gap = raw("8px")
        });
        global(".ja-page__body > * + *", new CssRule
        {
            MarginTop = raw("20px")
        });

        Media(".ja-shell", "(max-width: 760px)", new CssRule
        {
            Display = raw("block"),
            MinWidth = raw("0")
        });
        Media(".ja-shell__sidebar", "(max-width: 760px)", new CssRule
        {
            Position = raw("static"),
            Height = raw("auto"),
            Overflow = raw("visible"),
            BorderRight = raw("0"),
            BorderBottom = px(1) | solid | hex("293a33")
        });
        Media(".ja-sidebar", "(max-width: 760px)", new CssRule
        {
            MinHeight = raw("0"),
            Padding = raw("10px 12px")
        });
        Media(".ja-page", "(max-width: 760px)", new CssRule
        {
            Padding = raw("18px 14px 28px")
        });
        return true;
    }

    private static void Media(string selector, string prelude, CssRule rule)
        => global(selector, new CssRule { Children = [new(ChildKind.Media, prelude, rule)] });
}
