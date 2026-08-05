using ECMAScript.Style;
using static ECMAScript.Style.css;

namespace Jazor.Admin;

/// <summary>
/// Registers the native admin shell styles through ECMAScript.Style.
/// 通过 ECMAScript.Style 注册原生管理端框架样式，不再要求应用提供静态 CSS 文件。
/// </summary>
[ECMAScriptModule("./components/jazor-admin-style-sheet")]
internal static class AdminStyleSheet
{
    private static readonly bool IsRegistered = Register();

    public static void EnsureLoaded()
    {
        _ = IsRegistered;
    }

    private static bool Register()
    {
        G(".jazor-admin-application",
            D("--app-bg", "#f5f7fa"), D("--surface", "#ffffff"), D("--surface-subtle", "#f8fafc"),
            D("--surface-strong", "#edf2f7"), D("--text", "#1f2937"), D("--text-muted", "#738092"),
            D("--border", "#e5eaf1"), D("--border-strong", "#ccd6e2"), D("--accent", "#2f6fed"),
            D("--accent-strong", "#1f5bd2"), D("--accent-soft", "#e9f1ff"), D("--danger", "#d84a4a"),
            D("--danger-soft", "#fff0f0"), D("--warning", "#d18a19"), D("--warning-soft", "#fff7e8"),
            D("--info", "#2f6fed"), D("--info-soft", "#e9f1ff"),
            D("--shadow", "0 4px 14px rgb(31 52 78 / 5%)"),
            D("min-height", "100vh"), D("background", "var(--app-bg)"), D("color", "var(--text)"));

        var darkTheme = Rule(
            D("color-scheme", "dark"), D("--app-bg", "#151a18"), D("--surface", "#1e2522"),
            D("--surface-subtle", "#242c29"), D("--surface-strong", "#2c3732"), D("--text", "#edf3f0"),
            D("--text-muted", "#aab8b1"), D("--border", "#39453f"), D("--border-strong", "#526159"),
            D("--accent", "#50c99a"), D("--accent-strong", "#78dab4"), D("--accent-soft", "#193e31"),
            D("--danger", "#ff8c86"), D("--danger-soft", "#492827"), D("--warning", "#f1c35d"),
            D("--warning-soft", "#45391f"), D("--info", "#8bbaf0"), D("--info-soft", "#24364c"),
            D("--shadow", "0 1px 2px rgb(0 0 0 / 28%), 0 10px 28px rgb(0 0 0 / 20%)"));
        global(".jazor-admin-application--dark", darkTheme);
        Media(".jazor-admin-application--system", "(prefers-color-scheme: dark)", darkTheme);
        G(".jazor-admin-application--grayscale", D("filter", "grayscale(1)"));

        G(".jazor-admin-shell", D("display", "grid"), D("grid-template-columns", "232px minmax(0, 1fr)"), D("min-height", "100vh"));
        G(".jazor-admin-shell--top", D("display", "block"));
        G(".jazor-admin-shell__sidebar",
            D("position", "sticky"), D("top", "0"), D("z-index", "20"), D("height", "100vh"),
            D("overflow", "auto"), D("background", "#17241f"), D("color", "#eef7f3"),
            D("border-right", "1px solid #293a33"));
        G(".jazor-admin-shell--collapsed", D("grid-template-columns", "0 minmax(0, 1fr)"));
        G(".jazor-admin-shell--collapsed .jazor-admin-shell__sidebar", D("display", "none"), D("width", "0"), D("border-right", "0"));
        G(".jazor-admin-shell__main", D("min-width", "0"));
        G(".jazor-admin-shell__header",
            D("display", "flex"), D("align-items", "center"), D("position", "sticky"), D("top", "0"),
            D("z-index", "15"), D("min-height", "64px"),
            D("background", "color-mix(in srgb, var(--surface) 94%, transparent)"),
            D("border-bottom", "1px solid var(--border)"), D("backdrop-filter", "blur(12px)"));
        G(".jazor-admin-shell__sidebar-toggle",
            D("position", "relative"), D("flex", "0 0 36px"), D("width", "36px"), D("height", "36px"),
            D("padding", "0"), D("margin-left", "14px"), D("color", "var(--text)"),
            D("background", "transparent"), D("border", "1px solid var(--border)"), D("border-radius", "5px"));
        G(".jazor-admin-shell__sidebar-toggle::before", D("font-size", "20px"), D("line-height", "1"), D("content", "\"\\2630\""));
        G(".jazor-admin-shell__sidebar-toggle:hover", D("background", "var(--surface-strong)"));
        G(".jazor-admin-shell__content", D("width", "100%"));

        G(".jazor-admin-sidebar", D("min-height", "100%"), D("padding", "20px 14px"));
        G(".jazor-admin-sidebar__logo",
            D("display", "flex"), D("align-items", "center"), D("min-height", "42px"),
            D("margin", "0 8px 22px"), D("color", "#ffffff"), D("font-size", "18px"), D("font-weight", "750"));
        G(".jazor-admin-sidebar__list, .jazor-admin-sidebar__children", D("padding", "0"), D("margin", "0"), D("list-style", "none"));
        G(".jazor-admin-sidebar__item", D("margin", "3px 0"));
        G(".jazor-admin-sidebar__item-content", D("position", "relative"));
        G(".jazor-admin-sidebar__link, .jazor-admin-sidebar__button",
            D("display", "flex"), D("align-items", "center"), D("width", "100%"), D("min-height", "40px"),
            D("padding", "8px 12px"), D("color", "#b8c8c1"), D("text-align", "left"),
            D("text-decoration", "none"), D("background", "transparent"), D("border", "0"), D("border-radius", "6px"));
        G(".jazor-admin-sidebar__link:hover, .jazor-admin-sidebar__button:hover, .jazor-admin-sidebar__item.is-ancestor-selected > .jazor-admin-sidebar__item-content > .jazor-admin-sidebar__button",
            D("color", "#ffffff"), D("background", "#243a31"));
        G(".jazor-admin-sidebar__item.is-selected > .jazor-admin-sidebar__item-content > .jazor-admin-sidebar__link, .jazor-admin-sidebar__item.is-selected > .jazor-admin-sidebar__item-content > .jazor-admin-sidebar__button",
            D("color", "#ffffff"), D("background", "#087f5b"));
        G(".jazor-admin-sidebar__toggle", D("margin-left", "auto"), D("font-size", "12px"));
        G(".jazor-admin-sidebar__children", D("padding", "4px 0 4px 12px"));

        G(".jazor-admin-header",
            D("display", "flex"), D("flex", "1 1 auto"), D("align-items", "center"),
            D("justify-content", "space-between"), D("min-width", "0"), D("min-height", "64px"),
            D("padding", "8px 24px"), D("gap", "20px"));
        G(".jazor-admin-header__main, .jazor-admin-header__actions, .jazor-admin-header__toolbar, .jazor-admin-header__user-region",
            D("display", "flex"), D("align-items", "center"));
        G(".jazor-admin-header__main", D("min-width", "0"), D("gap", "12px"));
        G(".jazor-admin-header__logo", D("color", "var(--accent)"), D("font-weight", "750"));
        G(".jazor-admin-header__titles", D("min-width", "0"));
        G(".jazor-admin-header__title", D("overflow", "hidden"), D("font-size", "16px"), D("font-weight", "700"), D("text-overflow", "ellipsis"), D("white-space", "nowrap"));
        G(".jazor-admin-header__subtitle", D("margin-top", "2px"), D("color", "var(--text-muted)"), D("font-size", "12px"));
        G(".jazor-admin-header__actions", D("justify-content", "flex-end"), D("min-width", "0"), D("gap", "16px"));
        G(".jazor-admin-header__navigation", D("flex", "1 1 auto"), D("min-width", "0"));

        G(".jazor-admin-page", D("width", "min(100%, 1480px)"), D("margin", "0 auto"), D("padding", "24px"));
        G(".jazor-admin-page__header", D("display", "flex"), D("align-items", "flex-end"), D("justify-content", "space-between"), D("margin-bottom", "20px"), D("gap", "20px"));
        G(".jazor-admin-page__titles", D("min-width", "0"));
        G(".jazor-admin-page__title", D("margin", "0"), D("font-size", "26px"), D("line-height", "1.25"));
        G(".jazor-admin-page__subtitle", D("max-width", "760px"), D("margin", "7px 0 0"), D("color", "var(--text-muted)"), D("line-height", "1.5"));
        G(".jazor-admin-page__actions", D("display", "flex"), D("flex-wrap", "wrap"), D("justify-content", "flex-end"), D("gap", "8px"));
        G(".jazor-admin-page__body > * + *", D("margin-top", "20px"));

        Media(".jazor-admin-shell", "(max-width: 760px)", Rule(D("display", "block"), D("min-width", "0")));
        Media(".jazor-admin-shell__sidebar", "(max-width: 760px)", Rule(
            D("position", "static"), D("height", "auto"), D("overflow", "visible"), D("border-right", "0"), D("border-bottom", "1px solid #293a33")));
        Media(".jazor-admin-sidebar", "(max-width: 760px)", Rule(D("min-height", "0"), D("padding", "10px 12px")));
        Media(".jazor-admin-page", "(max-width: 760px)", Rule(D("padding", "18px 14px 28px")));
        return true;
    }

    private static void G(string selector, params CssDeclaration[] declarations)
        => global(selector, Rule(declarations));

    private static CssRule Rule(params CssDeclaration[] declarations)
        => new() { Additional = declarations };

    private static CssDeclaration D(string name, string value)
        => new(name, raw(value));

    private static void Media(string selector, string prelude, CssRule rule)
        => global(selector, new CssRule { Children = [new(CssChildKind.Media, prelude, rule)] });
}
