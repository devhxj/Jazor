using ECMAScript;
using ECMAScript.Style;
using static ECMAScript.Style.css;

namespace JazorAdmin;

/// <summary>
/// Registers JazorAdmin's application and page rules through ECMAScript.Style.
/// 通过 ECMAScript.Style 注册 JazorAdmin 的应用与页面样式，保持现有选择器契约。
/// </summary>
[ECMAScriptModule("./components/jazor-admin-styles")]
internal static class JazorAdminStyles
{
    private static readonly bool IsRegistered = Register();

    public static void EnsureLoaded()
    {
        _ = IsRegistered;
    }

    private static bool Register()
    {
        var dashboardEnter = keyframes(
        [
            new("from",
                Rule(
                    D("opacity", "0"),
                    D("transform", "translateY(8px)")
                )
            ),
            new("to",
                Rule(
                    D("opacity", "1"),
                    D("transform", "translateY(0)")
                )
            )
        ]);

        global(":root",
            Rule(
                D("color-scheme", "light"),
                D("font-family", "Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, \"Segoe UI\", sans-serif"),
                D("font-synthesis", "none"),
                D("text-rendering", "optimizeLegibility")
            ));

        global("*",
            Rule(
                D("box-sizing", "border-box")
            ));

        global("html,\nbody,\n#app",
            Rule(
                D("min-width", "320px"),
                D("min-height", "100%"),
                D("margin", "0")
            ));

        global("body",
            Rule(
                D("min-height", "100vh"),
                D("background", "#f4f6f5")
            ));

        global("button,\ninput,\nselect",
            Rule(
                D("font", "inherit")
            ));

        global("button,\nselect,\ninput[type=\"checkbox\"]",
            Rule(
                D("cursor", "pointer")
            ));

        global("button:disabled,\ninput:disabled,\nselect:disabled",
            Rule(
                D("cursor", "not-allowed"),
                D("opacity", "0.56")
            ));

        global(".jazor-admin-application",
            Rule(
                D("--app-bg", "#f5f7fa"),
                D("--surface", "#ffffff"),
                D("--surface-subtle", "#f8fafc"),
                D("--surface-strong", "#edf2f7"),
                D("--text", "#1f2937"),
                D("--text-muted", "#738092"),
                D("--border", "#e5eaf1"),
                D("--border-strong", "#ccd6e2"),
                D("--accent", "#2f6fed"),
                D("--accent-strong", "#1f5bd2"),
                D("--accent-soft", "#e9f1ff"),
                D("--danger", "#d84a4a"),
                D("--danger-soft", "#fff0f0"),
                D("--warning", "#d18a19"),
                D("--warning-soft", "#fff7e8"),
                D("--info", "#2f6fed"),
                D("--info-soft", "#e9f1ff"),
                D("--shadow", "0 4px 14px rgb(31 52 78 / 5%)"),
                D("min-height", "100vh"),
                D("background", "var(--app-bg)"),
                D("color", "var(--text)")
            ));

        global(".jazor-admin-application--dark",
            Rule(
                D("color-scheme", "dark"),
                D("--app-bg", "#151a18"),
                D("--surface", "#1e2522"),
                D("--surface-subtle", "#242c29"),
                D("--surface-strong", "#2c3732"),
                D("--text", "#edf3f0"),
                D("--text-muted", "#aab8b1"),
                D("--border", "#39453f"),
                D("--border-strong", "#526159"),
                D("--accent", "#50c99a"),
                D("--accent-strong", "#78dab4"),
                D("--accent-soft", "#193e31"),
                D("--danger", "#ff8c86"),
                D("--danger-soft", "#492827"),
                D("--warning", "#f1c35d"),
                D("--warning-soft", "#45391f"),
                D("--info", "#8bbaf0"),
                D("--info-soft", "#24364c"),
                D("--shadow", "0 1px 2px rgb(0 0 0 / 28%), 0 10px 28px rgb(0 0 0 / 20%)")
            ));

        Media(".jazor-admin-application--system", "(prefers-color-scheme: dark)",
            Rule(
                D("color-scheme", "dark"),
                D("--app-bg", "#151a18"),
                D("--surface", "#1e2522"),
                D("--surface-subtle", "#242c29"),
                D("--surface-strong", "#2c3732"),
                D("--text", "#edf3f0"),
                D("--text-muted", "#aab8b1"),
                D("--border", "#39453f"),
                D("--border-strong", "#526159"),
                D("--accent", "#50c99a"),
                D("--accent-strong", "#78dab4"),
                D("--accent-soft", "#193e31"),
                D("--danger", "#ff8c86"),
                D("--danger-soft", "#492827"),
                D("--warning", "#f1c35d"),
                D("--warning-soft", "#45391f"),
                D("--info", "#8bbaf0"),
                D("--info-soft", "#24364c"),
                D("--shadow", "0 1px 2px rgb(0 0 0 / 28%), 0 10px 28px rgb(0 0 0 / 20%)")
            ));

        global(".jazor-admin-application--grayscale",
            Rule(
                D("filter", "grayscale(1)")
            ));

        global(".jazor-admin-shell",
            Rule(
                D("display", "grid"),
                D("grid-template-columns", "232px minmax(0, 1fr)"),
                D("min-height", "100vh")
            ));

        global(".jazor-admin-shell--top",
            Rule(
                D("display", "block")
            ));

        global(".jazor-admin-shell__sidebar",
            Rule(
                D("position", "sticky"),
                D("top", "0"),
                D("z-index", "20"),
                D("height", "100vh"),
                D("overflow", "auto"),
                D("background", "#17241f"),
                D("color", "#eef7f3"),
                D("border-right", "1px solid #293a33")
            ));

        global(".jazor-admin-shell--collapsed",
            Rule(
                D("grid-template-columns", "0 minmax(0, 1fr)")
            ));

        global(".jazor-admin-shell--collapsed .jazor-admin-shell__sidebar",
            Rule(
                D("display", "none"),
                D("width", "0"),
                D("border-right", "0")
            ));

        global(".jazor-admin-shell__main",
            Rule(
                D("min-width", "0")
            ));

        global(".jazor-admin-shell__header",
            Rule(
                D("display", "flex"),
                D("align-items", "center"),
                D("position", "sticky"),
                D("top", "0"),
                D("z-index", "15"),
                D("min-height", "64px"),
                D("background", "color-mix(in srgb, var(--surface) 94%, transparent)"),
                D("border-bottom", "1px solid var(--border)"),
                D("backdrop-filter", "blur(12px)")
            ));

        global(".jazor-admin-shell__sidebar-toggle",
            Rule(
                D("position", "relative"),
                D("flex", "0 0 36px"),
                D("width", "36px"),
                D("height", "36px"),
                D("padding", "0"),
                D("margin-left", "14px"),
                D("color", "var(--text)"),
                D("background", "transparent"),
                D("border", "1px solid var(--border)"),
                D("border-radius", "5px")
            ));

        global(".jazor-admin-shell__sidebar-toggle::before",
            Rule(
                D("font-size", "20px"),
                D("line-height", "1"),
                D("content", "\"\\2630\"")
            ));

        global(".jazor-admin-shell__sidebar-toggle:hover",
            Rule(
                D("background", "var(--surface-strong)")
            ));

        global(".jazor-admin-shell__content",
            Rule(
                D("width", "100%")
            ));

        global(".jazor-admin-tdesign-layout",
            Rule(
                D("min-width", "0"),
                D("min-height", "100vh"),
                D("background", "var(--background)")
            ));

        global(".jazor-admin-tdesign-layout > [data-shell-region=\"sidebar\"]",
            Rule(
                D("position", "sticky"),
                D("top", "0"),
                D("z-index", "20"),
                D("height", "100vh"),
                D("overflow", "auto"),
                D("border-right", "1px solid var(--border)")
            ));

        global(".jazor-admin-tdesign-layout[data-shell-collapsed=\"false\"] > [data-shell-region=\"sidebar\"]",
            Rule(
                D("width", "240px !important"),
                D("flex", "0 0 240px !important")
            ));

        global(".jazor-admin-tdesign-layout[data-shell-collapsed=\"true\"] > [data-shell-region=\"sidebar\"]",
            Rule(
                D("width", "64px !important"),
                D("flex", "0 0 64px !important")
            ));

        global(".jazor-admin-tdesign-layout > [data-shell-region=\"main\"]",
            Rule(
                D("min-width", "0")
            ));

        global(".jazor-admin-tdesign-layout [data-shell-region=\"header\"]",
            Rule(
                D("position", "sticky"),
                D("top", "0"),
                D("z-index", "15"),
                D("border-bottom", "1px solid var(--border)")
            ));

        global(".jazor-admin-tdesign-layout__header",
            Rule(
                D("display", "flex"),
                D("align-items", "center"),
                D("width", "100%"),
                D("min-width", "0"),
                D("min-height", "64px"),
                D("gap", "14px")
            ));

        global(".jazor-admin-tdesign-layout [data-shell-region=\"content\"]",
            Rule(
                D("width", "100%"),
                D("min-width", "0")
            ));

        global(".jazor-admin-tdesign-sidebar-shell",
            Rule(
                D("display", "flex"),
                D("min-height", "100%"),
                D("background", "var(--surface)")
            ));

        global(".jazor-admin-tdesign-sidebar-rail",
            Rule(
                D("display", "flex"),
                D("flex", "0 0 64px"),
                D("flex-direction", "column"),
                D("align-items", "center"),
                D("width", "64px"),
                D("min-height", "100vh"),
                D("padding", "14px 8px"),
                D("gap", "10px"),
                D("background", "#20415d")
            ));

        global(".jazor-admin-tdesign-sidebar-rail__link",
            Rule(
                D("display", "grid"),
                D("width", "42px"),
                D("height", "42px"),
                D("place-items", "center"),
                D("color", "#c8d8e6"),
                D("text-decoration", "none"),
                D("border-radius", "6px")
            ));

        global(".jazor-admin-tdesign-sidebar-rail__link:hover,\n.jazor-admin-tdesign-sidebar-rail__link.is-selected",
            Rule(
                D("color", "#ffffff"),
                D("background", "#2f6fed")
            ));

        global(".jazor-admin-tdesign-sidebar-rail__icon",
            Rule(
                D("position", "relative"),
                D("display", "block"),
                D("width", "18px"),
                D("height", "18px")
            ));

        global(".jazor-admin-tdesign-sidebar-rail__icon::before,\n.jazor-admin-tdesign-sidebar-rail__icon::after",
            Rule(
                D("position", "absolute"),
                D("display", "block"),
                D("box-sizing", "border-box"),
                D("content", "\"\"")
            ));

        global(".jazor-admin-tdesign-sidebar-rail__icon[data-rail-icon=\"dashboard\"]::before",
            Rule(
                D("width", "6px"),
                D("height", "6px"),
                D("background", "currentcolor"),
                D("box-shadow", "10px 0 0 currentcolor, 0 10px 0 currentcolor, 10px 10px 0 currentcolor")
            ));

        global(".jazor-admin-tdesign-sidebar-rail__icon[data-rail-icon=\"operations\"]::before",
            Rule(
                D("top", "2px"),
                D("left", "1px"),
                D("width", "16px"),
                D("height", "3px"),
                D("background", "currentcolor"),
                D("box-shadow", "0 6px 0 currentcolor, 0 12px 0 currentcolor")
            ));

        global(".jazor-admin-tdesign-sidebar-rail__icon[data-rail-icon=\"operations\"]::after",
            Rule(
                D("top", "0"),
                D("left", "3px"),
                D("width", "3px"),
                D("height", "16px"),
                D("background", "#20415d"),
                D("box-shadow", "5px 0 0 #20415d, 5px 6px 0 #20415d, 10px 6px 0 #20415d, 10px 12px 0 #20415d")
            ));

        global(".jazor-admin-tdesign-sidebar-rail__link.is-selected .jazor-admin-tdesign-sidebar-rail__icon[data-rail-icon=\"operations\"]::after,\n.jazor-admin-tdesign-sidebar-rail__link:hover .jazor-admin-tdesign-sidebar-rail__icon[data-rail-icon=\"operations\"]::after",
            Rule(
                D("background", "#2f6fed"),
                D("box-shadow", "5px 0 0 #2f6fed, 5px 6px 0 #2f6fed, 10px 6px 0 #2f6fed, 10px 12px 0 #2f6fed")
            ));

        global(".jazor-admin-tdesign-sidebar-rail__icon[data-rail-icon=\"workspace\"]::before",
            Rule(
                D("width", "18px"),
                D("height", "18px"),
                D("border", "2px solid currentcolor")
            ));

        global(".jazor-admin-tdesign-sidebar-rail__icon[data-rail-icon=\"workspace\"]::after",
            Rule(
                D("top", "2px"),
                D("left", "7px"),
                D("width", "2px"),
                D("height", "14px"),
                D("background", "currentcolor"),
                D("box-shadow", "5px 0 0 currentcolor")
            ));

        global(".jazor-admin-tdesign-sidebar-rail__icon[data-rail-icon=\"settings\"]::before",
            Rule(
                D("top", "1px"),
                D("left", "8px"),
                D("width", "3px"),
                D("height", "16px"),
                D("background", "currentcolor"),
                D("box-shadow", "-7px 0 0 currentcolor, 7px 0 0 currentcolor")
            ));

        global(".jazor-admin-tdesign-sidebar-rail__icon[data-rail-icon=\"settings\"]::after",
            Rule(
                D("top", "4px"),
                D("left", "5px"),
                D("width", "6px"),
                D("height", "6px"),
                D("border", "2px solid currentcolor"),
                D("border-radius", "50%"),
                D("box-shadow", "7px 5px 0 -2px #20415d, -7px 7px 0 -2px #20415d")
            ));

        global(".jazor-admin-tdesign-sidebar-rail__link.is-selected .jazor-admin-tdesign-sidebar-rail__icon[data-rail-icon=\"settings\"]::after,\n.jazor-admin-tdesign-sidebar-rail__link:hover .jazor-admin-tdesign-sidebar-rail__icon[data-rail-icon=\"settings\"]::after",
            Rule(
                D("box-shadow", "7px 5px 0 -2px #2f6fed, -7px 7px 0 -2px #2f6fed")
            ));

        global(".jazor-admin-tdesign-sidebar-shell__menu",
            Rule(
                D("flex", "1 1 auto"),
                D("width", "176px"),
                D("min-width", "0"),
                D("padding", "14px 10px"),
                D("overflow", "auto"),
                D("border-left", "1px solid var(--border)")
            ));

        global(".jazor-admin-tdesign-sidebar-shell__brand",
            Rule(
                D("display", "flex"),
                D("align-items", "center"),
                D("min-height", "36px"),
                D("margin", "0 8px 16px"),
                D("gap", "8px"),
                D("color", "var(--text)"),
                D("font-size", "16px"),
                D("font-weight", "700"),
                D("white-space", "nowrap")
            ));

        global(".jazor-admin-tdesign-sidebar-shell__brand-mark",
            Rule(
                D("display", "block"),
                D("width", "18px"),
                D("height", "18px"),
                D("background", "#2f6fed"),
                D("border-radius", "4px"),
                D("box-shadow", "inset 0 0 0 4px #dce8ff")
            ));

        global(".jazor-admin-tdesign-sidebar-shell__menu [data-navigation-orientation=\"vertical\"]",
            Rule(
                D("width", "100%")
            ));

        global(".jazor-admin-sidebar",
            Rule(
                D("min-height", "100%"),
                D("padding", "20px 14px")
            ));

        global(".jazor-admin-sidebar__logo",
            Rule(
                D("display", "flex"),
                D("align-items", "center"),
                D("min-height", "42px"),
                D("margin", "0 8px 22px"),
                D("color", "#ffffff"),
                D("font-size", "18px"),
                D("font-weight", "750")
            ));

        global(".jazor-admin-sidebar__list,\n.jazor-admin-sidebar__children",
            Rule(
                D("padding", "0"),
                D("margin", "0"),
                D("list-style", "none")
            ));

        global(".jazor-admin-sidebar__item",
            Rule(
                D("margin", "3px 0")
            ));

        global(".jazor-admin-sidebar__item-content",
            Rule(
                D("position", "relative")
            ));

        global(".jazor-admin-sidebar__link,\n.jazor-admin-sidebar__button",
            Rule(
                D("display", "flex"),
                D("align-items", "center"),
                D("width", "100%"),
                D("min-height", "40px"),
                D("padding", "8px 12px"),
                D("color", "#b8c8c1"),
                D("text-align", "left"),
                D("text-decoration", "none"),
                D("background", "transparent"),
                D("border", "0"),
                D("border-radius", "6px")
            ));

        global(".jazor-admin-sidebar__link:hover,\n.jazor-admin-sidebar__button:hover,\n.jazor-admin-sidebar__item.is-ancestor-selected > .jazor-admin-sidebar__item-content > .jazor-admin-sidebar__button",
            Rule(
                D("color", "#ffffff"),
                D("background", "#243a31")
            ));

        global(".jazor-admin-sidebar__item.is-selected > .jazor-admin-sidebar__item-content > .jazor-admin-sidebar__link,\n.jazor-admin-sidebar__item.is-selected > .jazor-admin-sidebar__item-content > .jazor-admin-sidebar__button",
            Rule(
                D("color", "#ffffff"),
                D("background", "#087f5b")
            ));

        global(".jazor-admin-sidebar__toggle",
            Rule(
                D("margin-left", "auto"),
                D("font-size", "12px")
            ));

        global(".jazor-admin-sidebar__children",
            Rule(
                D("padding", "4px 0 4px 12px")
            ));

        global(".jazor-admin-sidebar__children .jazor-admin-sidebar__link,\n.jazor-admin-sidebar__children .jazor-admin-sidebar__button",
            Rule(
                D("min-height", "36px"),
                D("padding-left", "16px"),
                D("font-size", "14px")
            ));

        global(".jazor-admin-header",
            Rule(
                D("flex", "1 1 auto"),
                D("min-width", "0"),
                D("display", "flex"),
                D("align-items", "center"),
                D("justify-content", "space-between"),
                D("min-height", "64px"),
                D("padding", "8px 24px"),
                D("gap", "20px")
            ));

        global(".jazor-admin-header__main,\n.jazor-admin-header__actions,\n.jazor-admin-header__toolbar,\n.jazor-admin-header__user-region,\n.jazor-admin__preferences,\n.jazor-admin__user-region",
            Rule(
                D("display", "flex"),
                D("align-items", "center")
            ));

        global(".jazor-admin-header__main",
            Rule(
                D("min-width", "0"),
                D("gap", "12px")
            ));

        global(".jazor-admin-header__logo",
            Rule(
                D("color", "var(--accent)"),
                D("font-weight", "750")
            ));

        global(".jazor-admin-header__titles",
            Rule(
                D("min-width", "0")
            ));

        global(".jazor-admin-header__title",
            Rule(
                D("overflow", "hidden"),
                D("font-size", "16px"),
                D("font-weight", "700"),
                D("text-overflow", "ellipsis"),
                D("white-space", "nowrap")
            ));

        global(".jazor-admin-header__subtitle",
            Rule(
                D("margin-top", "2px"),
                D("color", "var(--text-muted)"),
                D("font-size", "12px")
            ));

        global(".jazor-admin-header__actions",
            Rule(
                D("justify-content", "flex-end"),
                D("min-width", "0"),
                D("gap", "16px")
            ));

        global(".jazor-admin-header__navigation",
            Rule(
                D("flex", "1 1 auto"),
                D("min-width", "0")
            ));

        global(".jazor-admin-tdesign-header",
            Rule(
                D("display", "flex"),
                D("flex", "1 1 auto"),
                D("align-items", "center"),
                D("justify-content", "space-between"),
                D("min-width", "0"),
                D("min-height", "64px"),
                D("padding", "8px 24px"),
                D("gap", "20px")
            ));

        global(".jazor-admin-tdesign-header__main,\n.jazor-admin-tdesign-header__actions",
            Rule(
                D("display", "flex"),
                D("align-items", "center")
            ));

        global(".jazor-admin-tdesign-header__main",
            Rule(
                D("min-width", "0"),
                D("gap", "12px")
            ));

        global(".jazor-admin-tdesign-header__titles",
            Rule(
                D("min-width", "0")
            ));

        global(".jazor-admin-tdesign-header__title",
            Rule(
                D("overflow", "hidden"),
                D("font-size", "16px"),
                D("font-weight", "700"),
                D("text-overflow", "ellipsis"),
                D("white-space", "nowrap")
            ));

        global(".jazor-admin-tdesign-header__subtitle",
            Rule(
                D("margin-top", "2px"),
                D("color", "var(--text-muted)"),
                D("font-size", "12px")
            ));

        global(".jazor-admin-tdesign-header__navigation",
            Rule(
                D("flex", "1 1 auto"),
                D("min-width", "0")
            ));

        global(".jazor-admin-tdesign-header__actions",
            Rule(
                D("justify-content", "flex-end"),
                D("min-width", "0"),
                D("gap", "16px")
            ));

        global(".jazor-admin-sidebar--horizontal",
            Rule(
                D("min-height", "0"),
                D("padding", "0"),
                D("color", "var(--text)")
            ));

        global(".jazor-admin-sidebar--horizontal > .jazor-admin-sidebar__list,\n.jazor-admin-sidebar--horizontal .jazor-admin-sidebar__children",
            Rule(
                D("display", "flex"),
                D("align-items", "center"),
                D("gap", "4px")
            ));

        global(".jazor-admin-sidebar--horizontal > .jazor-admin-sidebar__list",
            Rule(
                D("overflow-x", "auto"),
                D("overscroll-behavior-inline", "contain")
            ));

        global(".jazor-admin-sidebar--horizontal .jazor-admin-sidebar__item",
            Rule(
                D("display", "flex"),
                D("flex", "0 0 auto"),
                D("align-items", "center"),
                D("margin", "0")
            ));

        global(".jazor-admin-sidebar--horizontal .jazor-admin-sidebar__link,\n.jazor-admin-sidebar--horizontal .jazor-admin-sidebar__button",
            Rule(
                D("width", "auto"),
                D("min-height", "34px"),
                D("padding", "6px 10px"),
                D("color", "var(--text-muted)"),
                D("white-space", "nowrap")
            ));

        global(".jazor-admin-sidebar--horizontal .jazor-admin-sidebar__link:hover,\n.jazor-admin-sidebar--horizontal .jazor-admin-sidebar__button:hover,\n.jazor-admin-sidebar--horizontal .jazor-admin-sidebar__item.is-ancestor-selected > .jazor-admin-sidebar__item-content > .jazor-admin-sidebar__button",
            Rule(
                D("color", "var(--text)"),
                D("background", "var(--surface-strong)")
            ));

        global(".jazor-admin-sidebar--horizontal .jazor-admin-sidebar__item.is-selected > .jazor-admin-sidebar__item-content > .jazor-admin-sidebar__link,\n.jazor-admin-sidebar--horizontal .jazor-admin-sidebar__item.is-selected > .jazor-admin-sidebar__item-content > .jazor-admin-sidebar__button",
            Rule(
                D("color", "var(--accent-strong)"),
                D("background", "var(--accent-soft)")
            ));

        global(".jazor-admin-sidebar--horizontal .jazor-admin-sidebar__children",
            Rule(
                D("padding", "0 0 0 4px")
            ));

        global(".jazor-admin-header__toolbar,\n.jazor-admin__preferences,\n.jazor-admin__user-region",
            Rule(
                D("gap", "8px")
            ));

        global(".jazor-admin__preference,\n.jazor-admin__preference-toggle",
            Rule(
                D("display", "flex"),
                D("align-items", "center"),
                D("gap", "6px"),
                D("color", "var(--text-muted)"),
                D("font-size", "12px"),
                D("white-space", "nowrap")
            ));

        global(".jazor-admin__preference select,\n.jazor-admin-release-table__search,\n.jazor-admin-settings-form__input,\n.jazor-admin-settings-form__select,\n.jazor-admin-access input",
            Rule(
                D("min-height", "36px"),
                D("padding", "7px 10px"),
                D("color", "var(--text)"),
                D("background", "var(--surface)"),
                D("border", "1px solid var(--border-strong)"),
                D("border-radius", "5px")
            ));

        global(".jazor-admin__preference select",
            Rule(
                D("min-height", "32px"),
                D("padding", "4px 24px 4px 8px")
            ));

        global(".jazor-admin__user",
            Rule(
                D("max-width", "180px"),
                D("overflow", "hidden"),
                D("font-size", "13px"),
                D("font-weight", "650"),
                D("text-overflow", "ellipsis"),
                D("white-space", "nowrap")
            ));

        global(".jazor-admin__access-command,\n.jazor-admin-page__action,\n.jazor-admin-release-table__page-button,\n.jazor-admin-settings-form__submit,\n.jazor-admin-action-notice__dismiss,\n.jazor-admin-error__action,\n.jazor-admin-access button",
            Rule(
                D("min-height", "34px"),
                D("padding", "7px 12px"),
                D("color", "var(--text)"),
                D("font-weight", "650"),
                D("background", "var(--surface)"),
                D("border", "1px solid var(--border-strong)"),
                D("border-radius", "5px")
            ));

        global(".jazor-admin__access-command:hover,\n.jazor-admin-page__action:hover,\n.jazor-admin-release-table__page-button:hover,\n.jazor-admin-action-notice__dismiss:hover",
            Rule(
                D("background", "var(--surface-strong)")
            ));

        global(".jazor-admin-page",
            Rule(
                D("width", "min(100%, 1480px)"),
                D("margin", "0 auto"),
                D("padding", "24px")
            ));

        global(".jazor-admin-page__header",
            Rule(
                D("display", "flex"),
                D("align-items", "flex-end"),
                D("justify-content", "space-between"),
                D("margin-bottom", "20px"),
                D("gap", "20px")
            ));

        global(".jazor-admin-page__titles",
            Rule(
                D("min-width", "0")
            ));

        global(".jazor-admin-page__breadcrumb",
            Rule(
                D("display", "flex"),
                D("flex-wrap", "wrap"),
                D("gap", "6px"),
                D("margin-bottom", "7px"),
                D("color", "var(--text-muted)"),
                D("font-size", "13px")
            ));

        global(".jazor-admin-page__breadcrumb-item + .jazor-admin-page__breadcrumb-item::before",
            Rule(
                D("margin-right", "6px"),
                D("color", "var(--border-strong)"),
                D("content", "\"/\"")
            ));

        global(".jazor-admin-page__title",
            Rule(
                D("margin", "0"),
                D("font-size", "26px"),
                D("line-height", "1.25")
            ));

        global(".jazor-admin-page__subtitle",
            Rule(
                D("max-width", "760px"),
                D("margin", "7px 0 0"),
                D("color", "var(--text-muted)"),
                D("line-height", "1.5")
            ));

        global(".jazor-admin-page__actions",
            Rule(
                D("display", "flex"),
                D("flex-wrap", "wrap"),
                D("justify-content", "flex-end"),
                D("gap", "8px")
            ));

        global(".jazor-admin-page__action--primary,\n.jazor-admin-settings-form__submit,\n.jazor-admin-error__action,\n.jazor-admin-access button",
            Rule(
                D("color", "#ffffff"),
                D("background", "var(--accent)"),
                D("border-color", "var(--accent)")
            ));

        global(".jazor-admin-page__action--primary:hover,\n.jazor-admin-settings-form__submit:hover,\n.jazor-admin-error__action:hover,\n.jazor-admin-access button:hover",
            Rule(
                D("background", "var(--accent-strong)"),
                D("border-color", "var(--accent-strong)")
            ));

        global(".jazor-admin-page__action--danger",
            Rule(
                D("color", "var(--danger)"),
                D("border-color", "var(--danger)")
            ));

        global(".jazor-admin-page__body > * + *",
            Rule(
                D("margin-top", "20px")
            ));

        global(".jazor-admin-tdesign-page-container",
            Rule(
                D("width", "min(calc(100% - 48px), 1480px)"),
                D("min-width", "0"),
                D("margin", "24px auto")
            ));

        global(".jazor-admin-tdesign-page-container__header",
            Rule(
                D("display", "flex"),
                D("align-items", "flex-end"),
                D("justify-content", "space-between"),
                D("margin-bottom", "20px"),
                D("gap", "20px")
            ));

        global(".jazor-admin-tdesign-page-container__titles",
            Rule(
                D("min-width", "0")
            ));

        global(".jazor-admin-tdesign-page-container__title",
            Rule(
                D("margin-top", "7px"),
                D("font-size", "26px"),
                D("font-weight", "700"),
                D("line-height", "1.25")
            ));

        global(".jazor-admin-tdesign-page-container__subtitle",
            Rule(
                D("max-width", "760px"),
                D("margin-top", "7px"),
                D("color", "var(--text-muted)"),
                D("line-height", "1.5")
            ));

        global(".jazor-admin-tdesign-page-container__body > * + *",
            Rule(
                D("margin-top", "20px")
            ));

        global(".jazor-admin__metrics",
            Rule(
                D("display", "grid"),
                D("grid-template-columns", "repeat(3, minmax(0, 1fr))"),
                D("gap", "14px")
            ));

        global(".jazor-admin__metric,\n.jazor-admin__release-section,\n.jazor-admin__audit,\n.jazor-admin__settings,\n.jazor-admin__workspace",
            Rule(
                D("background", "var(--surface)"),
                D("border", "1px solid var(--border)"),
                D("border-radius", "7px"),
                D("box-shadow", "var(--shadow)")
            ));

        global(".jazor-admin__metric",
            Rule(
                D("min-width", "0"),
                D("padding", "18px")
            ));

        global(".jazor-admin__metric h2,\n.jazor-admin__release-section h2,\n.jazor-admin__audit h2,\n.jazor-admin__settings h2,\n.jazor-admin__workspace h2",
            Rule(
                D("margin", "0"),
                D("font-size", "15px")
            ));

        global(".jazor-admin__metric strong",
            Rule(
                D("display", "block"),
                D("margin-top", "12px"),
                D("color", "var(--accent)"),
                D("font-size", "22px")
            ));

        global(".jazor-admin__metric p,\n.jazor-admin__selection",
            Rule(
                D("margin", "7px 0 0"),
                D("color", "var(--text-muted)"),
                D("font-size", "13px"),
                D("line-height", "1.45")
            ));

        global(".jazor-admin__release-section,\n.jazor-admin__audit,\n.jazor-admin__settings,\n.jazor-admin__workspace",
            Rule(
                D("padding", "20px")
            ));

        global(".jazor-admin__release-section--focused",
            Rule(
                D("border-top", "3px solid var(--accent)")
            ));

        global(".jazor-admin-release-table",
            Rule(
                D("margin-top", "16px")
            ));

        global(".jazor-admin-release-table__toolbar,\n.jazor-admin-release-table__pagination",
            Rule(
                D("display", "flex"),
                D("align-items", "center"),
                D("justify-content", "space-between"),
                D("gap", "12px")
            ));

        global(".jazor-admin-release-table__search",
            Rule(
                D("width", "min(100%, 320px)")
            ));

        global(".jazor-admin-release-table__summary,\n.jazor-admin-release-table__page-status",
            Rule(
                D("color", "var(--text-muted)"),
                D("font-size", "13px"),
                D("white-space", "nowrap")
            ));

        global(".jazor-admin-release-table__table",
            Rule(
                D("display", "block"),
                D("width", "100%"),
                D("margin-top", "12px"),
                D("overflow-x", "auto"),
                D("border", "1px solid var(--border)"),
                D("border-radius", "6px"),
                D("border-spacing", "0")
            ));

        global(".jazor-admin-release-table__head,\n.jazor-admin-release-table__body",
            Rule(
                D("display", "table"),
                D("width", "100%"),
                D("min-width", "680px"),
                D("table-layout", "fixed")
            ));

        global(".jazor-admin-release-table__heading,\n.jazor-admin-release-table__cell",
            Rule(
                D("padding", "11px 14px"),
                D("text-align", "left"),
                D("border-bottom", "1px solid var(--border)")
            ));

        global(".jazor-admin-release-table__heading",
            Rule(
                D("color", "var(--text-muted)"),
                D("font-size", "12px"),
                D("font-weight", "700"),
                D("text-transform", "uppercase")
            ));

        global(".jazor-admin-release-table__selection-heading,\n.jazor-admin-release-table__selection-cell",
            Rule(
                D("width", "48px"),
                D("padding-right", "6px"),
                D("padding-left", "14px")
            ));

        global(".jazor-admin-release-table__sort-button",
            Rule(
                D("width", "100%"),
                D("padding", "0"),
                D("color", "inherit"),
                D("font", "inherit"),
                D("text-align", "left"),
                D("text-transform", "inherit"),
                D("background", "transparent"),
                D("border", "0")
            ));

        global(".jazor-admin-release-table__sort-indicator",
            Rule(
                D("margin-left", "5px"),
                D("color", "var(--accent)")
            ));

        global(".jazor-admin-release-table__row:last-child .jazor-admin-release-table__cell",
            Rule(
                D("border-bottom", "0")
            ));

        global(".jazor-admin-release-table__row:hover",
            Rule(
                D("background", "var(--surface-subtle)")
            ));

        global(".jazor-admin-release-table__row.is-selected",
            Rule(
                D("background", "var(--accent-soft)")
            ));

        global(".jazor-admin-release-table__row.is-disabled",
            Rule(
                D("color", "var(--text-muted)"),
                D("opacity", "0.64")
            ));

        global(".jazor-admin-release-table__loading,\n.jazor-admin-release-table__empty",
            Rule(
                D("padding", "30px 14px"),
                D("color", "var(--text-muted)"),
                D("text-align", "center")
            ));

        global(".jazor-admin-release-table__pagination",
            Rule(
                D("justify-content", "flex-end"),
                D("margin-top", "12px")
            ));

        global(".jazor-admin-settings-form",
            Rule(
                D("display", "grid"),
                D("grid-template-columns", "repeat(2, minmax(0, 1fr))"),
                D("max-width", "720px"),
                D("margin-top", "18px"),
                D("gap", "18px")
            ));

        global(".jazor-admin-settings-form__field",
            Rule(
                D("display", "flex"),
                D("flex-direction", "column"),
                D("min-width", "0"),
                D("gap", "7px")
            ));

        global(".jazor-admin-settings-form__checkbox-field",
            Rule(
                D("display", "grid"),
                D("grid-template-columns", "auto minmax(0, 1fr)"),
                D("align-content", "start"),
                D("align-items", "center"),
                D("min-width", "0"),
                D("gap", "7px 8px")
            ));

        global(".jazor-admin-settings-form__checkbox-field .jazor-admin-settings-form__help",
            Rule(
                D("grid-column", "2")
            ));

        global(".jazor-admin-settings-form__label,\n.jazor-admin-settings-form__checkbox-field .jazor-admin-settings-form__label",
            Rule(
                D("font-size", "13px"),
                D("font-weight", "650")
            ));

        global(".jazor-admin-settings-form__help,\n.jazor-admin-settings-form__status",
            Rule(
                D("margin", "0"),
                D("color", "var(--text-muted)"),
                D("font-size", "12px"),
                D("line-height", "1.45")
            ));

        global(".jazor-admin-settings-form__submit,\n.jazor-admin-settings-form__status",
            Rule(
                D("grid-column", "1 / -1"),
                D("justify-self", "start")
            ));

        global(".jazor-admin__status-line",
            Rule(
                D("display", "grid"),
                D("grid-template-columns", "minmax(150px, 0.3fr) minmax(0, 1fr)"),
                D("padding", "14px 0"),
                D("gap", "16px"),
                D("border-bottom", "1px solid var(--border)")
            ));

        global(".jazor-admin__status-line:last-child",
            Rule(
                D("border-bottom", "0")
            ));

        global(".jazor-admin__status-line span",
            Rule(
                D("color", "var(--text-muted)")
            ));

        global(".jazor-admin-action-notice",
            Rule(
                D("display", "flex"),
                D("align-items", "flex-start"),
                D("justify-content", "space-between"),
                D("padding", "13px 14px"),
                D("gap", "16px"),
                D("background", "var(--info-soft)"),
                D("border", "1px solid var(--info)"),
                D("border-radius", "6px")
            ));

        global(".jazor-admin-action-notice--success",
            Rule(
                D("background", "var(--accent-soft)"),
                D("border-color", "var(--accent)")
            ));

        global(".jazor-admin-action-notice--warning",
            Rule(
                D("background", "var(--warning-soft)"),
                D("border-color", "var(--warning)")
            ));

        global(".jazor-admin-action-notice--error",
            Rule(
                D("background", "var(--danger-soft)"),
                D("border-color", "var(--danger)")
            ));

        global(".jazor-admin-action-notice__title,\n.jazor-admin-action-notice__description",
            Rule(
                D("margin", "0")
            ));

        global(".jazor-admin-action-notice__description",
            Rule(
                D("margin-top", "3px"),
                D("color", "var(--text-muted)"),
                D("line-height", "1.45")
            ));

        global(".jazor-admin-access,\n.jazor-admin-error",
            Rule(
                D("display", "grid"),
                D("min-height", "100vh"),
                D("padding", "32px"),
                D("place-items", "center"),
                D("background", "var(--app-bg)")
            ));

        global(".jazor-admin-access__panel,\n.jazor-admin-error__content",
            Rule(
                D("width", "min(100%, 420px)")
            ));

        global(".jazor-admin-access__panel",
            Rule(
                D("padding", "30px"),
                D("background", "var(--surface)"),
                D("border", "1px solid var(--border)"),
                D("border-top", "4px solid var(--accent)"),
                D("border-radius", "7px"),
                D("box-shadow", "var(--shadow)")
            ));

        global(".jazor-admin-access__brand",
            Rule(
                D("color", "var(--accent)"),
                D("font-size", "17px")
            ));

        global(".jazor-admin-access h1",
            Rule(
                D("margin", "22px 0 8px"),
                D("font-size", "26px")
            ));

        global(".jazor-admin-access p",
            Rule(
                D("margin", "0"),
                D("color", "var(--text-muted)"),
                D("line-height", "1.5")
            ));

        global(".jazor-admin-access form",
            Rule(
                D("display", "grid"),
                D("margin-top", "24px"),
                D("gap", "16px")
            ));

        global(".jazor-admin-access label",
            Rule(
                D("display", "grid"),
                D("gap", "7px"),
                D("font-size", "13px"),
                D("font-weight", "650")
            ));

        global(".jazor-admin-access__error",
            Rule(
                D("color", "var(--danger) !important"),
                D("font-size", "13px")
            ));

        global(".jazor-admin-error__content",
            Rule(
                D("text-align", "center")
            ));

        global(".jazor-admin-error__code",
            Rule(
                D("display", "block"),
                D("color", "var(--accent)"),
                D("font-size", "72px"),
                D("font-weight", "800"),
                D("line-height", "1")
            ));

        global(".jazor-admin-error--internal-server-error .jazor-admin-error__code",
            Rule(
                D("color", "var(--danger)")
            ));

        global(".jazor-admin-error h1",
            Rule(
                D("margin", "18px 0 8px"),
                D("font-size", "28px")
            ));

        global(".jazor-admin-error p",
            Rule(
                D("margin", "0"),
                D("color", "var(--text-muted)"),
                D("line-height", "1.6")
            ));

        global(".jazor-admin-error__action",
            Rule(
                D("margin-top", "24px")
            ));

        global(":where(a, button, input, select):focus-visible",
            Rule(
                D("outline", "3px solid color-mix(in srgb, var(--accent) 35%, transparent)"),
                D("outline-offset", "2px")
            ));

        Media(".jazor-admin-header", "(max-width: 1080px)",
            Rule(
                D("align-items", "flex-start")
            ));

        Media(".jazor-admin-header__actions", "(max-width: 1080px)",
            Rule(
                D("align-items", "flex-end"),
                D("flex-direction", "column-reverse"),
                D("gap", "6px")
            ));

        Media(".jazor-admin-header__navigation", "(max-width: 1080px)",
            Rule(
                D("width", "100%")
            ));

        Media(".jazor-admin__preferences", "(max-width: 1080px)",
            Rule(
                D("flex-wrap", "wrap"),
                D("justify-content", "flex-end")
            ));

        Media(".jazor-admin-tdesign-header", "(max-width: 1080px)",
            Rule(
                D("align-items", "flex-start")
            ));

        Media(".jazor-admin-tdesign-header__actions", "(max-width: 1080px)",
            Rule(
                D("align-items", "flex-end"),
                D("flex-direction", "column-reverse"),
                D("gap", "6px")
            ));

        Media(".jazor-admin-tdesign-header__navigation", "(max-width: 1080px)",
            Rule(
                D("width", "100%")
            ));

        Media(".jazor-admin-shell", "(max-width: 760px)",
            Rule(
                D("display", "block"),
                D("min-width", "0")
            ));

        Media(".jazor-admin-shell__sidebar", "(max-width: 760px)",
            Rule(
                D("position", "static"),
                D("height", "auto"),
                D("overflow", "visible"),
                D("border-right", "0"),
                D("border-bottom", "1px solid #293a33")
            ));

        Media(".jazor-admin-tdesign-layout", "(max-width: 760px)",
            Rule(
                D("flex-direction", "column !important"),
                D("min-width", "0")
            ));

        Media(".jazor-admin-tdesign-layout[data-shell-collapsed] > [data-shell-region=\"sidebar\"]", "(max-width: 760px)",
            Rule(
                D("position", "static"),
                D("width", "100% !important"),
                D("max-width", "100%"),
                D("height", "auto"),
                D("overflow", "visible"),
                D("border-right", "0"),
                D("border-bottom", "1px solid var(--border)"),
                D("flex", "0 0 auto !important")
            ));

        Media(".jazor-admin-tdesign-layout > [data-shell-region=\"main\"]", "(max-width: 760px)",
            Rule(
                D("width", "100%")
            ));

        Media(".jazor-admin-tdesign-layout [data-shell-region=\"header\"]", "(max-width: 760px)",
            Rule(
                D("position", "static"),
                D("height", "auto !important"),
                D("min-height", "64px")
            ));

        Media(".jazor-admin-tdesign-layout__header", "(max-width: 760px)",
            Rule(
                D("align-items", "flex-start"),
                D("flex-wrap", "wrap")
            ));

        Media("[data-navigation-orientation=\"vertical\"]", "(max-width: 760px)",
            Rule(
                D("display", "flex !important"),
                D("align-items", "flex-start"),
                D("width", "100%"),
                D("overflow-x", "auto"),
                D("overscroll-behavior-inline", "contain")
            ));

        Media(".jazor-admin-sidebar", "(max-width: 760px)",
            Rule(
                D("min-height", "0"),
                D("padding", "10px 12px")
            ));

        Media(".jazor-admin-sidebar__logo", "(max-width: 760px)",
            Rule(
                D("min-height", "34px"),
                D("margin", "0 6px 8px"),
                D("font-size", "16px")
            ));

        Media(".jazor-admin-sidebar__list", "(max-width: 760px)",
            Rule(
                D("display", "flex"),
                D("align-items", "flex-start"),
                D("width", "100%"),
                D("padding-bottom", "3px"),
                D("gap", "4px"),
                D("overflow-x", "auto"),
                D("overscroll-behavior-inline", "contain")
            ));

        Media(".jazor-admin-sidebar__item", "(max-width: 760px)",
            Rule(
                D("flex", "0 0 auto"),
                D("margin", "0")
            ));

        Media(".jazor-admin-sidebar__link,\n  .jazor-admin-sidebar__button", "(max-width: 760px)",
            Rule(
                D("min-height", "36px"),
                D("width", "auto"),
                D("white-space", "nowrap")
            ));

        Media(".jazor-admin-sidebar__children", "(max-width: 760px)",
            Rule(
                D("display", "flex"),
                D("padding", "4px 0 0 8px"),
                D("gap", "4px")
            ));

        Media(".jazor-admin-sidebar__children .jazor-admin-sidebar__item", "(max-width: 760px)",
            Rule(
                D("flex", "0 0 auto")
            ));

        Media(".jazor-admin-sidebar__children .jazor-admin-sidebar__link,\n  .jazor-admin-sidebar__children .jazor-admin-sidebar__button", "(max-width: 760px)",
            Rule(
                D("min-height", "32px"),
                D("padding", "6px 10px")
            ));

        Media(".jazor-admin-shell__header", "(max-width: 760px)",
            Rule(
                D("position", "static")
            ));

        Media(".jazor-admin-header", "(max-width: 760px)",
            Rule(
                D("display", "block"),
                D("min-height", "0"),
                D("padding", "12px 16px")
            ));

        Media(".jazor-admin-shell__sidebar-toggle", "(max-width: 760px)",
            Rule(
                D("align-self", "flex-start"),
                D("margin", "12px 0 0 12px")
            ));

        Media(".jazor-admin-header__subtitle", "(max-width: 760px)",
            Rule(
                D("display", "none")
            ));

        Media(".jazor-admin-header__actions", "(max-width: 760px)",
            Rule(
                D("align-items", "stretch"),
                D("margin-top", "10px")
            ));

        Media(".jazor-admin-tdesign-header", "(max-width: 760px)",
            Rule(
                D("display", "block"),
                D("min-height", "0"),
                D("padding", "12px 16px")
            ));

        Media(".jazor-admin-tdesign-header__subtitle", "(max-width: 760px)",
            Rule(
                D("display", "none")
            ));

        Media(".jazor-admin-tdesign-header__actions", "(max-width: 760px)",
            Rule(
                D("align-items", "stretch"),
                D("margin-top", "10px")
            ));

        Media(".jazor-admin-tdesign-header__navigation", "(max-width: 760px)",
            Rule(
                D("margin-top", "10px")
            ));

        Media(".jazor-admin-header__navigation", "(max-width: 760px)",
            Rule(
                D("margin-top", "10px")
            ));

        Media(".jazor-admin__preferences,\n  .jazor-admin__user-region", "(max-width: 760px)",
            Rule(
                D("justify-content", "flex-start"),
                D("overflow-x", "auto")
            ));

        Media(".jazor-admin-page", "(max-width: 760px)",
            Rule(
                D("padding", "18px 14px 28px")
            ));

        Media(".jazor-admin-page__header", "(max-width: 760px)",
            Rule(
                D("display", "block"),
                D("margin-bottom", "16px")
            ));

        Media(".jazor-admin-page__title", "(max-width: 760px)",
            Rule(
                D("font-size", "23px")
            ));

        Media(".jazor-admin-page__actions", "(max-width: 760px)",
            Rule(
                D("justify-content", "flex-start"),
                D("margin-top", "14px")
            ));

        Media(".jazor-admin-tdesign-page-container", "(max-width: 760px)",
            Rule(
                D("width", "calc(100% - 28px)"),
                D("margin", "14px")
            ));

        Media(".jazor-admin-tdesign-page-container__header", "(max-width: 760px)",
            Rule(
                D("display", "block"),
                D("margin-bottom", "16px")
            ));

        Media(".jazor-admin-tdesign-page-container__title", "(max-width: 760px)",
            Rule(
                D("font-size", "23px")
            ));

        Media(".jazor-admin__metrics,\n  .jazor-admin-settings-form", "(max-width: 760px)",
            Rule(
                D("grid-template-columns", "1fr")
            ));

        Media(".jazor-admin__release-section,\n  .jazor-admin__audit,\n  .jazor-admin__settings,\n  .jazor-admin__workspace", "(max-width: 760px)",
            Rule(
                D("padding", "16px")
            ));

        Media(".jazor-admin-release-table__toolbar", "(max-width: 760px)",
            Rule(
                D("align-items", "stretch"),
                D("flex-direction", "column")
            ));

        Media(".jazor-admin-release-table__search", "(max-width: 760px)",
            Rule(
                D("width", "100%")
            ));

        Media(".jazor-admin__status-line", "(max-width: 760px)",
            Rule(
                D("grid-template-columns", "1fr"),
                D("gap", "5px")
            ));

        Media(".jazor-admin-access,\n  .jazor-admin-error", "(max-width: 760px)",
            Rule(
                D("padding", "20px")
            ));

        Media(".jazor-admin-access__panel", "(max-width: 760px)",
            Rule(
                D("padding", "24px 20px")
            ));

        Media(".jazor-admin-error__code", "(max-width: 760px)",
            Rule(
                D("font-size", "58px")
            ));

        Media(".jazor-admin__preference-toggle", "(max-width: 430px)",
            Rule(
                D("display", "none")
            ));

        Media(".jazor-admin__user", "(max-width: 430px)",
            Rule(
                D("max-width", "120px")
            ));

        Media(".jazor-admin-action-notice", "(max-width: 430px)",
            Rule(
                D("align-items", "stretch"),
                D("flex-direction", "column")
            ));

        global(".jazor-admin-tdesign-page-container--dashboard",
            Rule(
                D("width", "min(calc(100% - 36px), 1480px)"),
                D("margin", "18px auto 28px"),
                D("background", "transparent !important"),
                D("border", "0 !important"),
                D("box-shadow", "none !important")
            ));

        global(".jazor-admin-tdesign-page-container--dashboard .jazor-admin-tdesign-page-container__header",
            Rule(
                D("align-items", "center"),
                D("margin-bottom", "14px")
            ));

        global(".jazor-admin-tdesign-page-container--dashboard .jazor-admin-tdesign-page-container__title",
            Rule(
                D("margin-top", "5px"),
                D("font-size", "21px")
            ));

        global(".jazor-admin-tdesign-page-container--dashboard .jazor-admin-tdesign-page-container__subtitle",
            Rule(
                D("margin-top", "4px"),
                D("font-size", "13px")
            ));

        global(".jazor-admin-medical-dashboard",
            Rule(
                D("display", "grid"),
                D("min-width", "0"),
                D("gap", "14px")
            ));

        global(".jazor-admin-medical-dashboard__metrics",
            Rule(
                D("display", "grid"),
                D("grid-template-columns", "repeat(4, minmax(0, 1fr))"),
                D("gap", "12px")
            ));

        global(".jazor-admin-medical-dashboard__metric",
            Rule(
                D("position", "relative"),
                D("min-height", "104px"),
                D("padding", "15px 17px 14px 20px"),
                D("overflow", "hidden"),
                D("background", "var(--surface)"),
                D("border", "1px solid var(--border)"),
                D("border-radius", "7px"),
                D("box-shadow", "var(--shadow)"),
                D("animation", raw(dashboardEnter + " 320ms ease both"))
            ));

        global(".jazor-admin-medical-dashboard__metric::before",
            Rule(
                D("position", "absolute"),
                D("top", "16px"),
                D("right", "16px"),
                D("width", "30px"),
                D("height", "30px"),
                D("background", "var(--accent-soft)"),
                D("border-radius", "6px"),
                D("content", "\"\"")
            ));

        global(".jazor-admin-medical-dashboard__metric::after",
            Rule(
                D("position", "absolute"),
                D("top", "25px"),
                D("right", "25px"),
                D("width", "12px"),
                D("height", "12px"),
                D("border", "2px solid var(--accent)"),
                D("border-radius", "3px"),
                D("content", "\"\"")
            ));

        global(".jazor-admin-medical-dashboard__metric:nth-child(2)",
            Rule(
                D("animation-delay", "45ms")
            ));

        global(".jazor-admin-medical-dashboard__metric:nth-child(3)",
            Rule(
                D("animation-delay", "90ms")
            ));

        global(".jazor-admin-medical-dashboard__metric:nth-child(4)",
            Rule(
                D("animation-delay", "135ms")
            ));

        global(".jazor-admin-medical-dashboard__metric--recruiting::before",
            Rule(
                D("background", "#ecfbf3")
            ));

        global(".jazor-admin-medical-dashboard__metric--recruiting::after",
            Rule(
                D("border-color", "#24a76a"),
                D("border-radius", "50%")
            ));

        global(".jazor-admin-medical-dashboard__metric--applications::before",
            Rule(
                D("background", "#f2f4f7")
            ));

        global(".jazor-admin-medical-dashboard__metric--applications::after",
            Rule(
                D("border-color", "#7b8798"),
                D("border-radius", "50%")
            ));

        global(".jazor-admin-medical-dashboard__metric--review::before",
            Rule(
                D("background", "#fff7e7")
            ));

        global(".jazor-admin-medical-dashboard__metric--review::after",
            Rule(
                D("border-color", "#dfa22f"),
                D("border-radius", "50%")
            ));

        global(".jazor-admin-medical-dashboard__metric-label,\n.jazor-admin-medical-dashboard__metric-detail",
            Rule(
                D("display", "block")
            ));

        global(".jazor-admin-medical-dashboard__metric-label",
            Rule(
                D("color", "#5f6b7a"),
                D("font-size", "13px"),
                D("font-weight", "650")
            ));

        global(".jazor-admin-medical-dashboard__metric strong",
            Rule(
                D("display", "block"),
                D("margin-top", "8px"),
                D("color", "var(--text)"),
                D("font-size", "26px"),
                D("line-height", "1")
            ));

        global(".jazor-admin-medical-dashboard__metric-detail",
            Rule(
                D("margin-top", "8px"),
                D("color", "var(--text-muted)"),
                D("font-size", "11px")
            ));

        global(".jazor-admin-medical-dashboard__grid",
            Rule(
                D("display", "grid"),
                D("grid-template-columns", "minmax(0, 1.08fr) minmax(0, 0.92fr)"),
                D("min-width", "0"),
                D("gap", "12px")
            ));

        global(".jazor-admin-medical-dashboard__panel",
            Rule(
                D("min-width", "0"),
                D("min-height", "430px"),
                D("overflow", "hidden"),
                D("background", "var(--surface)"),
                D("border", "1px solid var(--border)"),
                D("border-radius", "7px"),
                D("box-shadow", "var(--shadow)"),
                D("animation", raw(dashboardEnter + " 360ms 120ms ease both"))
            ));

        global(".jazor-admin-medical-dashboard__panel--applications",
            Rule(
                D("animation-delay", "180ms")
            ));

        global(".jazor-admin-medical-dashboard__panel-header",
            Rule(
                D("display", "flex"),
                D("align-items", "center"),
                D("justify-content", "space-between"),
                D("min-height", "62px"),
                D("padding", "13px 16px"),
                D("gap", "16px"),
                D("border-bottom", "1px solid var(--border)")
            ));

        global(".jazor-admin-medical-dashboard__panel-header h2,\n.jazor-admin-medical-dashboard__panel-header p",
            Rule(
                D("margin", "0")
            ));

        global(".jazor-admin-medical-dashboard__panel-header h2",
            Rule(
                D("font-size", "14px"),
                D("font-weight", "700")
            ));

        global(".jazor-admin-medical-dashboard__panel-header p",
            Rule(
                D("max-width", "440px"),
                D("margin-top", "4px"),
                D("overflow", "hidden"),
                D("color", "var(--text-muted)"),
                D("font-size", "11px"),
                D("text-overflow", "ellipsis"),
                D("white-space", "nowrap")
            ));

        global(".jazor-admin-medical-dashboard__text-action",
            Rule(
                D("flex", "0 0 auto"),
                D("min-height", "30px"),
                D("padding", "4px 9px"),
                D("color", "var(--accent)"),
                D("font-size", "12px"),
                D("font-weight", "650"),
                D("background", "transparent"),
                D("border", "1px solid transparent"),
                D("border-radius", "5px")
            ));

        global(".jazor-admin-medical-dashboard__text-action:hover",
            Rule(
                D("background", "var(--accent-soft)"),
                D("border-color", "#cfe0ff")
            ));

        global(".jazor-admin-medical-dashboard__study-list",
            Rule(
                D("padding", "4px 16px 16px"),
                D("margin", "0"),
                D("list-style", "none")
            ));

        global(".jazor-admin-medical-dashboard__study-list li + li",
            Rule(
                D("border-top", "1px solid var(--border)")
            ));

        global(".jazor-admin-medical-dashboard__study",
            Rule(
                D("display", "grid"),
                D("grid-template-columns", "minmax(0, 1fr) auto"),
                D("align-items", "center"),
                D("width", "100%"),
                D("min-height", "56px"),
                D("padding", "9px 6px 9px 9px"),
                D("gap", "12px"),
                D("color", "var(--text)"),
                D("text-align", "left"),
                D("background", "transparent"),
                D("border", "0"),
                D("border-left", "3px solid transparent"),
                D("transition", "background-color 150ms ease, border-color 150ms ease, transform 150ms ease")
            ));

        global(".jazor-admin-medical-dashboard__study span",
            Rule(
                D("overflow", "hidden"),
                D("font-size", "12px"),
                D("font-weight", "550"),
                D("line-height", "1.45"),
                D("text-overflow", "ellipsis"),
                D("white-space", "nowrap")
            ));

        global(".jazor-admin-medical-dashboard__study strong",
            Rule(
                D("color", "var(--text-muted)"),
                D("font-size", "11px"),
                D("font-weight", "600")
            ));

        global(".jazor-admin-medical-dashboard__study:hover,\n.jazor-admin-medical-dashboard__study.is-selected",
            Rule(
                D("background", "#f5f8ff"),
                D("border-left-color", "var(--accent)")
            ));

        global(".jazor-admin-medical-dashboard__study:hover",
            Rule(
                D("transform", "translateX(2px)")
            ));

        global(".jazor-admin-medical-dashboard__table-wrap",
            Rule(
                D("width", "100%"),
                D("overflow-x", "auto")
            ));

        global(".jazor-admin-medical-dashboard__table",
            Rule(
                D("width", "100%"),
                D("min-width", "560px"),
                D("border-collapse", "collapse"),
                D("table-layout", "fixed")
            ));

        global(".jazor-admin-medical-dashboard__table th,\n.jazor-admin-medical-dashboard__table td",
            Rule(
                D("padding", "12px 14px"),
                D("text-align", "left"),
                D("vertical-align", "middle"),
                D("border-bottom", "1px solid var(--border)")
            ));

        global(".jazor-admin-medical-dashboard__table th",
            Rule(
                D("color", "var(--text-muted)"),
                D("font-size", "11px"),
                D("font-weight", "650"),
                D("background", "#fbfcfe")
            ));

        global(".jazor-admin-medical-dashboard__table th:first-child,\n.jazor-admin-medical-dashboard__table td:first-child",
            Rule(
                D("width", "54%")
            ));

        global(".jazor-admin-medical-dashboard__table th:nth-child(2),\n.jazor-admin-medical-dashboard__table td:nth-child(2)",
            Rule(
                D("width", "18%")
            ));

        global(".jazor-admin-medical-dashboard__table td",
            Rule(
                D("overflow", "hidden"),
                D("color", "#536174"),
                D("font-size", "11px"),
                D("text-overflow", "ellipsis"),
                D("white-space", "nowrap"),
                D("transition", "background-color 150ms ease")
            ));

        global(".jazor-admin-medical-dashboard__table tr:hover td",
            Rule(
                D("background", "#f8faff")
            ));

        global(".jazor-admin-medical-dashboard__status",
            Rule(
                D("display", "inline-flex"),
                D("align-items", "center"),
                D("min-height", "22px"),
                D("padding", "2px 7px"),
                D("font-size", "10px"),
                D("border-radius", "4px")
            ));

        global(".jazor-admin-medical-dashboard__status--cancelled",
            Rule(
                D("color", "#b77713"),
                D("background", "var(--warning-soft)")
            ));

        global(".jazor-admin-medical-dashboard__status--registered",
            Rule(
                D("color", "#238458"),
                D("background", "#eaf8f1")
            ));

        global(".jazor-admin__release-section--supporting",
            Rule(
                D("margin-top", "28px")
            ));

        Media(".jazor-admin-medical-dashboard__metrics", "(max-width: 980px)",
            Rule(
                D("grid-template-columns", "repeat(2, minmax(0, 1fr))")
            ));

        Media(".jazor-admin-medical-dashboard__grid", "(max-width: 980px)",
            Rule(
                D("grid-template-columns", "1fr")
            ));

        Media(".jazor-admin-medical-dashboard__panel", "(max-width: 980px)",
            Rule(
                D("min-height", "0")
            ));

        Media(".jazor-admin-tdesign-page-container--dashboard", "(max-width: 620px)",
            Rule(
                D("width", "calc(100% - 24px)"),
                D("margin", "12px")
            ));

        Media(".jazor-admin-medical-dashboard__metrics", "(max-width: 620px)",
            Rule(
                D("grid-template-columns", "1fr")
            ));

        Media(".jazor-admin-medical-dashboard__metric", "(max-width: 620px)",
            Rule(
                D("min-height", "96px")
            ));

        Media(".jazor-admin-medical-dashboard__panel-header", "(max-width: 620px)",
            Rule(
                D("align-items", "flex-start")
            ));

        Media("*,\n  *::before,\n  *::after", "(prefers-reduced-motion: reduce)",
            Rule(
                D("animation-duration", "0.01ms !important"),
                D("scroll-behavior", "auto !important"),
                D("transition-duration", "0.01ms !important")
            ));

        return true;
    }

    private static CssRule Rule(params CssDeclaration[] declarations)
        => new() { Additional = declarations };

    private static CssDeclaration D(string name, string value)
        => new(name, raw(value));

    private static CssDeclaration D(string name, CssValue value)
        => new(name, value);

    private static void Media(string selector, string prelude, CssRule rule)
        => global(selector, new CssRule
        {
            Children = [new(CssChildKind.Media, prelude, rule)]
        });
}
