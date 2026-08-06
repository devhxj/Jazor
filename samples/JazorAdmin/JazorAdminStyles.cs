using ECMAScript;
using ECMAScript.Style;
using static ECMAScript.Style.css;

namespace JazorAdmin;

/// <summary>
/// Registers JazorAdmin's application and page rules through ECMAScript.Style.
/// 通过 ECMAScript.Style 注册 JazorAdmin 的应用与页面样式，保持现有选择器契约。
/// </summary>
[ECMAScriptModule("./components/styles")]
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
            new("from", new CssDeclarations
            {
                Opacity = 0,
                Transform = translateY(px(8))
            }),
            new("to", new CssDeclarations
            {
                Opacity = 1,
                Transform = translateY(px(0))
            })
        ]);

        var sessionSpin = keyframes(
        [
            new("to", new CssDeclarations
            {
                Transform = rotate(deg(360))
            })
        ]);

        global(":root",
            new CssRule
            {
                ColorScheme = raw("light"),
                FontFamily = raw("Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, \"Segoe UI\", sans-serif"),
                FontSynthesis = raw("none"),
                TextRendering = raw("optimizeLegibility")
            });

        global("*",
            new CssRule
            {
                BoxSizing = raw("border-box")
            });

        global("html, body, #app",
            new CssRule
            {
                MinWidth = raw("320px"),
                MinHeight = raw("100%"),
                Margin = raw("0")
            });

        global("body",
            new CssRule
            {
                MinHeight = raw("100vh"),
                Background = raw("#f4f6f5")
            });

        global("button, input, select, textarea",
            new CssRule
            {
                Font = raw("inherit")
            });

        global("button, select, input[type=\"checkbox\"]",
            new CssRule
            {
                Cursor = raw("pointer")
            });

        global("button:disabled, input:disabled, select:disabled",
            new CssRule
            {
                Cursor = raw("not-allowed"),
                Opacity = raw("0.56")
            });

        global(".ja-application",
            new CssRule
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
                ["--success"] = raw("#22875a"),
                ["--success-soft"] = raw("#e9f7ef"),
                ["--warning"] = raw("#d18a19"),
                ["--warning-soft"] = raw("#fff7e8"),
                ["--info"] = raw("#2f6fed"),
                ["--info-soft"] = raw("#e9f1ff"),
                ["--shadow"] = shadows(new CssShadow(px(0), px(4), Blur: px(14), Color: rgba(31, 52, 78, 0.05))),
                MinHeight = raw("100vh"),
                Background = raw("var(--app-bg)"),
                Color = raw("var(--text)")
            });

        global(".ja-application--dark",
            new CssRule
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
                ["--success"] = raw("#62d39d"),
                ["--success-soft"] = raw("#1d4935"),
                ["--warning"] = raw("#f1c35d"),
                ["--warning-soft"] = raw("#45391f"),
                ["--info"] = raw("#8bbaf0"),
                ["--info-soft"] = raw("#24364c"),
                ["--shadow"] = shadows(
                    new CssShadow(px(0), px(1), Blur: px(2), Color: rgba(0, 0, 0, 0.28)),
                    new CssShadow(px(0), px(10), Blur: px(28), Color: rgba(0, 0, 0, 0.2)))
            });

        Media(".ja-application--system", "(prefers-color-scheme: dark)",
            new CssRule
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
                ["--success"] = raw("#62d39d"),
                ["--success-soft"] = raw("#1d4935"),
                ["--warning"] = raw("#f1c35d"),
                ["--warning-soft"] = raw("#45391f"),
                ["--info"] = raw("#8bbaf0"),
                ["--info-soft"] = raw("#24364c"),
                ["--shadow"] = shadows(
                    new CssShadow(px(0), px(1), Blur: px(2), Color: rgba(0, 0, 0, 0.28)),
                    new CssShadow(px(0), px(10), Blur: px(28), Color: rgba(0, 0, 0, 0.2)))
            });

        global(".ja-application--grayscale",
            new CssRule
            {
                Filter = raw("grayscale(1)")
            });

        global(".ja-shell",
            new CssRule
            {
                Display = raw("grid"),
                GridTemplateColumns = raw("232px minmax(0, 1fr)"),
                MinHeight = raw("100vh")
            });

        global(".ja-shell--top",
            new CssRule
            {
                Display = raw("block")
            });

        global(".ja-shell__sidebar",
            new CssRule
            {
                Position = raw("sticky"),
                Top = raw("0"),
                ZIndex = raw("20"),
                Height = raw("100vh"),
                Overflow = raw("auto"),
                Background = raw("#17241f"),
                Color = raw("#eef7f3"),
                BorderRight = raw("1px solid #293a33")
            });

        global(".ja-shell--collapsed",
            new CssRule
            {
                GridTemplateColumns = raw("0 minmax(0, 1fr)")
            });

        global(".ja-shell--collapsed .ja-shell__sidebar",
            new CssRule
            {
                Display = raw("none"),
                Width = raw("0"),
                BorderRight = raw("0")
            });

        global(".ja-shell__main",
            new CssRule
            {
                MinWidth = raw("0")
            });

        global(".ja-shell__header",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center"),
                Position = raw("sticky"),
                Top = raw("0"),
                ZIndex = raw("15"),
                MinHeight = raw("64px"),
                Background = raw("color-mix(in srgb, var(--surface) 94%, transparent)"),
                BorderBottom = raw("1px solid var(--border)"),
                BackdropFilter = raw("blur(12px)")
            });

        global(".ja-shell__sidebar-toggle",
            new CssRule
            {
                Position = raw("relative"),
                Flex = raw("0 0 36px"),
                Width = raw("36px"),
                Height = raw("36px"),
                Padding = raw("0"),
                MarginLeft = raw("14px"),
                Color = raw("var(--text)"),
                Background = raw("transparent"),
                Border = raw("1px solid var(--border)"),
                BorderRadius = raw("5px")
            });

        global(".ja-shell__sidebar-toggle::before",
            new CssRule
            {
                FontSize = raw("20px"),
                LineHeight = raw("1"),
                Content = raw("\"\\2630\"")
            });

        global(".ja-shell__sidebar-toggle:hover",
            new CssRule
            {
                Background = raw("var(--surface-strong)")
            });

        global(".ja-shell__content",
            new CssRule
            {
                Width = raw("100%")
            });

        global(".ja-tdesign-layout",
            new CssRule
            {
                MinWidth = raw("0"),
                MinHeight = raw("100vh"),
                Background = raw("var(--background)")
            });

        global(".ja-tdesign-layout > [data-shell-region=\"sidebar\"]",
            new CssRule
            {
                Position = raw("sticky"),
                Top = raw("0"),
                ZIndex = raw("20"),
                Height = raw("100vh"),
                Overflow = raw("auto"),
                BorderRight = raw("1px solid var(--border)")
            });

        global(".ja-tdesign-layout[data-shell-collapsed=\"false\"] > [data-shell-region=\"sidebar\"]",
            new CssRule
            {
                Width = raw("240px !important"),
                Flex = raw("0 0 240px !important")
            });

        global(".ja-tdesign-layout[data-shell-collapsed=\"true\"] > [data-shell-region=\"sidebar\"]",
            new CssRule
            {
                Width = raw("64px !important"),
                Flex = raw("0 0 64px !important")
            });

        global(".ja-tdesign-layout > [data-shell-region=\"main\"]",
            new CssRule
            {
                MinWidth = raw("0")
            });

        global(".ja-tdesign-layout [data-shell-region=\"header\"]",
            new CssRule
            {
                Position = raw("sticky"),
                Top = raw("0"),
                ZIndex = raw("15"),
                BorderBottom = raw("1px solid var(--border)")
            });

        global(".ja-tdesign-layout__header",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center"),
                Width = raw("100%"),
                MinWidth = raw("0"),
                MinHeight = raw("64px"),
                Gap = raw("14px")
            });

        global(".ja-tdesign-layout [data-shell-region=\"content\"]",
            new CssRule
            {
                Width = raw("100%"),
                MinWidth = raw("0")
            });

        global(".ja-tdesign-sidebar-shell",
            new CssRule
            {
                Display = raw("flex"),
                MinHeight = raw("100%"),
                Background = raw("var(--surface)")
            });

        global(".ja-iconbar",
            new CssRule
            {
                Display = raw("flex"),
                Flex = raw("0 0 64px"),
                FlexDirection = raw("column"),
                AlignItems = raw("center"),
                Width = raw("64px"),
                MinHeight = raw("100vh"),
                Padding = raw("14px 8px"),
                Background = raw("#20415d")
            });

        global(".ja-iconbar__items",
            new CssRule
            {
                Display = raw("flex"),
                FlexDirection = raw("column"),
                Gap = raw("10px")
            });

        global(".ja-iconbar__link",
            new CssRule
            {
                Display = raw("grid"),
                Width = raw("42px"),
                Height = raw("42px"),
                PlaceItems = raw("center"),
                Color = raw("#c8d8e6"),
                TextDecoration = raw("none"),
                BorderRadius = raw("6px")
            });

        global(".ja-iconbar__link:hover, .ja-iconbar__link.is-selected",
            new CssRule
            {
                Color = raw("#ffffff"),
                Background = raw("#2f6fed")
            });

        global(".ja-iconbar__icon",
            new CssRule
            {
                Position = raw("relative"),
                Display = raw("block"),
                Width = raw("18px"),
                Height = raw("18px")
            });

        global(".ja-iconbar__icon::before, .ja-iconbar__icon::after",
            new CssRule
            {
                Position = raw("absolute"),
                Display = raw("block"),
                BoxSizing = raw("border-box"),
                Content = raw("\"\"")
            });

        global(".ja-iconbar__icon[data-iconbar-icon=\"dashboard\"]::before",
            new CssRule
            {
                Width = raw("6px"),
                Height = raw("6px"),
                Background = raw("currentcolor"),
                BoxShadow = shadows(
                    new CssShadow(px(10), px(0), Color: currentColor),
                    new CssShadow(px(0), px(10), Color: currentColor),
                    new CssShadow(px(10), px(10), Color: currentColor))
            });

        global(".ja-iconbar__icon[data-iconbar-icon=\"organizations\"]::before", new CssRule
        {
            Top = px(1),
            Left = px(5),
            Width = px(8),
            Height = px(8),
            BorderWidth = px(2),
            BorderStyle = solid,
            BorderColor = currentColor,
            BorderRadius = percent(50)
        });

        global(".ja-iconbar__icon[data-iconbar-icon=\"organizations\"]::after", new CssRule
        {
            Right = px(1),
            Bottom = px(1),
            Left = px(1),
            Height = px(7),
            BorderWidth = px(2),
            BorderStyle = solid,
            BorderColor = currentColor,
            BorderRadius = raw("8px 8px 3px 3px")
        });

        global(".ja-iconbar__icon[data-iconbar-icon=\"authorization\"]::before", new CssRule
        {
            Top = px(1),
            Left = px(3),
            Width = px(12),
            Height = px(14),
            BorderWidth = px(2),
            BorderStyle = solid,
            BorderColor = currentColor,
            BorderRadius = raw("3px 3px 7px 7px")
        });

        global(".ja-iconbar__icon[data-iconbar-icon=\"authorization\"]::after", new CssRule
        {
            Top = px(5),
            Left = px(7),
            Width = px(4),
            Height = px(7),
            BorderRightWidth = px(2),
            BorderRightStyle = solid,
            BorderRightColor = currentColor,
            BorderBottomWidth = px(2),
            BorderBottomStyle = solid,
            BorderBottomColor = currentColor,
            Transform = rotate(deg(45))
        });

        global(".ja-iconbar__icon[data-iconbar-icon=\"accounts\"]::before", new CssRule
        {
            Top = px(1),
            Left = px(5),
            Width = px(8),
            Height = px(8),
            BorderWidth = px(2),
            BorderStyle = solid,
            BorderColor = currentColor,
            BorderRadius = percent(50)
        });

        global(".ja-iconbar__icon[data-iconbar-icon=\"accounts\"]::after", new CssRule
        {
            Right = px(1),
            Bottom = px(1),
            Left = px(1),
            Height = px(7),
            BorderWidth = px(2),
            BorderStyle = solid,
            BorderColor = currentColor,
            BorderRadius = raw("6px 6px 2px 2px")
        });

        global(".ja-iconbar__icon[data-iconbar-icon=\"sso\"]::before", new CssRule
        {
            Top = px(2),
            Left = px(1),
            Width = px(16),
            Height = px(2),
            Background = currentColor,
            BoxShadow = shadows(
                new CssShadow(px(0), px(6), Color: currentColor),
                new CssShadow(px(0), px(12), Color: currentColor))
        });

        global(".ja-iconbar__icon[data-iconbar-icon=\"sso\"]::after", new CssRule
        {
            Top = px(0),
            Left = px(4),
            Width = px(4),
            Height = px(5),
            Background = raw("var(--surface)"),
            BorderWidth = px(2),
            BorderStyle = solid,
            BorderColor = currentColor,
            BorderRadius = percent(50),
            BoxShadow = shadows(
                new CssShadow(px(7), px(6), Blur: px(0), Spread: px(-1), Color: var("--surface")),
                new CssShadow(px(7), px(6), Blur: px(0), Spread: px(1), Color: currentColor),
                new CssShadow(px(2), px(12), Blur: px(0), Spread: px(-1), Color: var("--surface")),
                new CssShadow(px(2), px(12), Blur: px(0), Spread: px(1), Color: currentColor))
        });

        global(".ja-iconbar__icon[data-iconbar-icon=\"settings\"]::before", new CssRule
        {
            Top = px(2),
            Left = px(1),
            Width = px(16),
            Height = px(2),
            Background = currentColor,
            BoxShadow = shadows(
                new CssShadow(px(0), px(6), Color: currentColor),
                new CssShadow(px(0), px(12), Color: currentColor))
        });

        global(".ja-iconbar__icon[data-iconbar-icon=\"settings\"]::after", new CssRule
        {
            Top = px(0),
            Left = px(4),
            Width = px(4),
            Height = px(5),
            Background = raw("var(--surface)"),
            BorderWidth = px(2),
            BorderStyle = solid,
            BorderColor = currentColor,
            BorderRadius = percent(50),
            BoxShadow = shadows(
                new CssShadow(px(7), px(6), Blur: px(0), Spread: px(-1), Color: var("--surface")),
                new CssShadow(px(7), px(6), Blur: px(0), Spread: px(1), Color: currentColor),
                new CssShadow(px(2), px(12), Blur: px(0), Spread: px(-1), Color: var("--surface")),
                new CssShadow(px(2), px(12), Blur: px(0), Spread: px(1), Color: currentColor))
        });

        global(".ja-iconbar__icon[data-iconbar-icon=\"schedules\"]::before", new CssRule
        {
            Top = px(1),
            Left = px(1),
            Width = px(16),
            Height = px(16),
            BorderWidth = px(2),
            BorderStyle = solid,
            BorderColor = currentColor,
            BorderRadius = raw("3px")
        });

        global(".ja-iconbar__icon[data-iconbar-icon=\"schedules\"]::after", new CssRule
        {
            Top = px(7),
            Left = px(8),
            Width = px(5),
            Height = px(5),
            BorderTopWidth = px(2),
            BorderTopStyle = solid,
            BorderTopColor = currentColor,
            BorderRightWidth = px(2),
            BorderRightStyle = solid,
            BorderRightColor = currentColor
        });

        global(".ja-tdesign-sidebar-shell__menu",
            new CssRule
            {
                Flex = raw("1 1 auto"),
                Width = raw("176px"),
                MinWidth = raw("0"),
                Padding = raw("14px 10px"),
                Overflow = raw("auto"),
                BorderLeft = raw("1px solid var(--border)")
            });

        global(".ja-tdesign-sidebar-shell__brand",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center"),
                MinHeight = raw("36px"),
                Margin = raw("0 8px 16px"),
                Gap = raw("8px"),
                Color = raw("var(--text)"),
                FontSize = raw("16px"),
                FontWeight = raw("700"),
                WhiteSpace = raw("nowrap")
            });

        global(".ja-tdesign-sidebar-shell__brand-mark",
            new CssRule
            {
                Display = raw("block"),
                Width = raw("18px"),
                Height = raw("18px"),
                Background = raw("#2f6fed"),
                BorderRadius = raw("4px"),
                BoxShadow = shadows(new CssShadow(px(0), px(0), Blur: px(0), Spread: px(4), Color: hex("dce8ff"), Inset: true))
            });

        global(".ja-tdesign-sidebar-shell__menu [data-navigation-orientation=\"vertical\"]",
            new CssRule
            {
                Width = raw("100%")
            });

        global(".ja-sidebar",
            new CssRule
            {
                MinHeight = raw("100%"),
                Padding = raw("20px 14px")
            });

        global(".ja-sidebar__logo",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center"),
                MinHeight = raw("42px"),
                Margin = raw("0 8px 22px"),
                Color = raw("#ffffff"),
                FontSize = raw("18px"),
                FontWeight = raw("750")
            });

        global(".ja-sidebar__list, .ja-sidebar__children",
            new CssRule
            {
                Padding = raw("0"),
                Margin = raw("0"),
                ListStyle = raw("none")
            });

        global(".ja-sidebar__item",
            new CssRule
            {
                Margin = raw("3px 0")
            });

        global(".ja-sidebar__item-content",
            new CssRule
            {
                Position = raw("relative")
            });

        global(".ja-sidebar__link, .ja-sidebar__button",
            new CssRule
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

        global(".ja-sidebar__link:hover, .ja-sidebar__button:hover, .ja-sidebar__item.is-ancestor-selected > .ja-sidebar__item-content > .ja-sidebar__button",
            new CssRule
            {
                Color = raw("#ffffff"),
                Background = raw("#243a31")
            });

        global(".ja-sidebar__item.is-selected > .ja-sidebar__item-content > .ja-sidebar__link, .ja-sidebar__item.is-selected > .ja-sidebar__item-content > .ja-sidebar__button",
            new CssRule
            {
                Color = raw("#ffffff"),
                Background = raw("#087f5b")
            });

        global(".ja-sidebar__toggle",
            new CssRule
            {
                MarginLeft = raw("auto"),
                FontSize = raw("12px")
            });

        global(".ja-sidebar__children",
            new CssRule
            {
                Padding = raw("4px 0 4px 12px")
            });

        global(".ja-sidebar__children .ja-sidebar__link, .ja-sidebar__children .ja-sidebar__button",
            new CssRule
            {
                MinHeight = raw("36px"),
                PaddingLeft = raw("16px"),
                FontSize = raw("14px")
            });

        global(".ja-header",
            new CssRule
            {
                Flex = raw("1 1 auto"),
                MinWidth = raw("0"),
                Display = raw("flex"),
                AlignItems = raw("center"),
                JustifyContent = raw("space-between"),
                MinHeight = raw("64px"),
                Padding = raw("8px 24px"),
                Gap = raw("20px")
            });

        global(".ja-header__main, .ja-header__actions, .ja-header__toolbar, .ja-header__user-region, .ja-preferences, .ja-user-region",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center")
            });

        global(".ja-header__main",
            new CssRule
            {
                MinWidth = raw("0"),
                Gap = raw("12px")
            });

        global(".ja-header__logo",
            new CssRule
            {
                Color = raw("var(--accent)"),
                FontWeight = raw("750")
            });

        global(".ja-header__titles",
            new CssRule
            {
                MinWidth = raw("0")
            });

        global(".ja-header__title",
            new CssRule
            {
                Overflow = raw("hidden"),
                FontSize = raw("16px"),
                FontWeight = raw("700"),
                TextOverflow = raw("ellipsis"),
                WhiteSpace = raw("nowrap")
            });

        global(".ja-header__subtitle",
            new CssRule
            {
                MarginTop = raw("2px"),
                Color = raw("var(--text-muted)"),
                FontSize = raw("12px")
            });

        global(".ja-header__actions",
            new CssRule
            {
                JustifyContent = raw("flex-end"),
                MinWidth = raw("0"),
                Gap = raw("16px")
            });

        global(".ja-header__navigation",
            new CssRule
            {
                Flex = raw("1 1 auto"),
                MinWidth = raw("0")
            });

        global(".ja-tdesign-header",
            new CssRule
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

        global(".ja-tdesign-header__main, .ja-tdesign-header__actions",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center")
            });

        global(".ja-tdesign-header__main",
            new CssRule
            {
                MinWidth = raw("0"),
                Gap = raw("12px")
            });

        global(".ja-tdesign-header__titles",
            new CssRule
            {
                MinWidth = raw("0")
            });

        global(".ja-tdesign-header__title",
            new CssRule
            {
                Overflow = raw("hidden"),
                FontSize = raw("16px"),
                FontWeight = raw("700"),
                TextOverflow = raw("ellipsis"),
                WhiteSpace = raw("nowrap")
            });

        global(".ja-tdesign-header__subtitle",
            new CssRule
            {
                MarginTop = raw("2px"),
                Color = raw("var(--text-muted)"),
                FontSize = raw("12px")
            });

        global(".ja-tdesign-header__navigation",
            new CssRule
            {
                Flex = raw("1 1 auto"),
                MinWidth = raw("0")
            });

        global(".ja-tdesign-header__actions",
            new CssRule
            {
                JustifyContent = raw("flex-end"),
                MinWidth = raw("0"),
                Gap = raw("16px")
            });

        global(".ja-sidebar--horizontal",
            new CssRule
            {
                MinHeight = raw("0"),
                Padding = raw("0"),
                Color = raw("var(--text)")
            });

        global(".ja-sidebar--horizontal > .ja-sidebar__list, .ja-sidebar--horizontal .ja-sidebar__children",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center"),
                Gap = raw("4px")
            });

        global(".ja-sidebar--horizontal > .ja-sidebar__list",
            new CssRule
            {
                OverflowX = raw("auto"),
                OverscrollBehaviorInline = raw("contain")
            });

        global(".ja-sidebar--horizontal .ja-sidebar__item",
            new CssRule
            {
                Display = raw("flex"),
                Flex = raw("0 0 auto"),
                AlignItems = raw("center"),
                Margin = raw("0")
            });

        global(".ja-sidebar--horizontal .ja-sidebar__link, .ja-sidebar--horizontal .ja-sidebar__button",
            new CssRule
            {
                Width = raw("auto"),
                MinHeight = raw("34px"),
                Padding = raw("6px 10px"),
                Color = raw("var(--text-muted)"),
                WhiteSpace = raw("nowrap")
            });

        global(".ja-sidebar--horizontal .ja-sidebar__link:hover, .ja-sidebar--horizontal .ja-sidebar__button:hover, .ja-sidebar--horizontal .ja-sidebar__item.is-ancestor-selected > .ja-sidebar__item-content > .ja-sidebar__button",
            new CssRule
            {
                Color = raw("var(--text)"),
                Background = raw("var(--surface-strong)")
            });

        global(".ja-sidebar--horizontal .ja-sidebar__item.is-selected > .ja-sidebar__item-content > .ja-sidebar__link, .ja-sidebar--horizontal .ja-sidebar__item.is-selected > .ja-sidebar__item-content > .ja-sidebar__button",
            new CssRule
            {
                Color = raw("var(--accent-strong)"),
                Background = raw("var(--accent-soft)")
            });

        global(".ja-sidebar--horizontal .ja-sidebar__children",
            new CssRule
            {
                Padding = raw("0 0 0 4px")
            });

        global(".ja-header__toolbar, .ja-preferences, .ja-user-region",
            new CssRule
            {
                Gap = raw("8px")
            });

        global(".ja-preference, .ja-preference-toggle",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center"),
                Gap = raw("6px"),
                Color = raw("var(--text-muted)"),
                FontSize = raw("12px"),
                WhiteSpace = raw("nowrap")
            });

        global(".ja-preference select, .ja-access input",
            new CssRule
            {
                MinHeight = raw("36px"),
                Padding = raw("7px 10px"),
                Color = raw("var(--text)"),
                Background = raw("var(--surface)"),
                Border = raw("1px solid var(--border-strong)"),
                BorderRadius = raw("5px")
            });

        global(".ja-preference select",
            new CssRule
            {
                MinHeight = raw("32px"),
                Padding = raw("4px 24px 4px 8px")
            });

        global(".ja-user",
            new CssRule
            {
                MaxWidth = raw("180px"),
                Overflow = raw("hidden"),
                FontSize = raw("13px"),
                FontWeight = raw("650"),
                TextOverflow = raw("ellipsis"),
                WhiteSpace = raw("nowrap")
            });

        global(".ja-access-command, .ja-error__action, .ja-access button",
            new CssRule
            {
                MinHeight = raw("34px"),
                Padding = raw("7px 12px"),
                Color = raw("var(--text)"),
                FontWeight = raw("650"),
                Background = raw("var(--surface)"),
                Border = raw("1px solid var(--border-strong)"),
                BorderRadius = raw("5px")
            });

        global(".ja-access-command:hover",
            new CssRule
            {
                Background = raw("var(--surface-strong)")
            });

        global(".ja-page",
            new CssRule
            {
                Width = raw("min(100%, 1480px)"),
                Margin = raw("0 auto"),
                Padding = raw("24px")
            });

        global(".ja-page__header",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("flex-end"),
                JustifyContent = raw("space-between"),
                MarginBottom = raw("20px"),
                Gap = raw("20px")
            });

        global(".ja-page__titles",
            new CssRule
            {
                MinWidth = raw("0")
            });

        global(".ja-page__breadcrumb",
            new CssRule
            {
                Display = raw("flex"),
                FlexWrap = raw("wrap"),
                Gap = raw("6px"),
                MarginBottom = raw("7px"),
                Color = raw("var(--text-muted)"),
                FontSize = raw("13px")
            });

        global(".ja-page__breadcrumb-item + .ja-page__breadcrumb-item::before",
            new CssRule
            {
                MarginRight = raw("6px"),
                Color = raw("var(--border-strong)"),
                Content = raw("\"/\"")
            });

        global(".ja-page__title",
            new CssRule
            {
                Margin = raw("0"),
                FontSize = raw("26px"),
                LineHeight = raw("1.25")
            });

        global(".ja-page__subtitle",
            new CssRule
            {
                MaxWidth = raw("760px"),
                Margin = raw("7px 0 0"),
                Color = raw("var(--text-muted)"),
                LineHeight = raw("1.5")
            });

        global(".ja-page__actions",
            new CssRule
            {
                Display = raw("flex"),
                FlexWrap = raw("wrap"),
                JustifyContent = raw("flex-end"),
                Gap = raw("8px")
            });

        global(".ja-error__action, .ja-access button",
            new CssRule
            {
                Color = raw("#ffffff"),
                Background = raw("var(--accent)"),
                BorderColor = raw("var(--accent)")
            });

        global(".ja-error__action:hover, .ja-access button:hover",
            new CssRule
            {
                Background = raw("var(--accent-strong)"),
                BorderColor = raw("var(--accent-strong)")
            });

        global(".ja-page__action--danger",
            new CssRule
            {
                Color = raw("var(--danger)"),
                BorderColor = raw("var(--danger)")
            });

        global(".ja-page__body > * + *",
            new CssRule
            {
                MarginTop = raw("20px")
            });

        global(".ja-tdesign-page-container",
            new CssRule
            {
                Width = raw("min(calc(100% - 48px), 1480px)"),
                MinWidth = raw("0"),
                Margin = raw("24px auto")
            });

        global(".ja-tdesign-page-container__header",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("flex-end"),
                JustifyContent = raw("space-between"),
                MarginBottom = raw("20px"),
                Gap = raw("20px")
            });

        global(".ja-tdesign-page-container__titles",
            new CssRule
            {
                MinWidth = raw("0")
            });

        global(".ja-tdesign-page-container__title",
            new CssRule
            {
                MarginTop = raw("7px"),
                FontSize = raw("26px"),
                FontWeight = raw("700"),
                LineHeight = raw("1.25")
            });

        global(".ja-tdesign-page-container__subtitle",
            new CssRule
            {
                MaxWidth = raw("760px"),
                MarginTop = raw("7px"),
                Color = raw("var(--text-muted)"),
                LineHeight = raw("1.5")
            });

        global(".ja-tdesign-page-container__body > * + *",
            new CssRule
            {
                MarginTop = raw("20px")
            });

        global(".ja-access, .ja-error",
            new CssRule
            {
                Display = raw("grid"),
                MinHeight = raw("100vh"),
                Padding = raw("32px"),
                PlaceItems = raw("center"),
                Background = raw("var(--app-bg)")
            });

        global(".ja-access__panel, .ja-error__content",
            new CssRule
            {
                Width = raw("min(100%, 420px)")
            });

        global(".ja-access__panel",
            new CssRule
            {
                Padding = raw("30px"),
                Background = raw("var(--surface)"),
                Border = raw("1px solid var(--border)"),
                BorderTop = raw("4px solid var(--accent)"),
                BorderRadius = raw("7px"),
                BoxShadow = var("--shadow")
            });

        global(".ja-access__brand",
            new CssRule
            {
                Color = raw("var(--accent)"),
                FontSize = raw("17px")
            });

        global(".ja-access h1",
            new CssRule
            {
                Margin = raw("22px 0 8px"),
                FontSize = raw("26px")
            });

        global(".ja-access p",
            new CssRule
            {
                Margin = raw("0"),
                Color = raw("var(--text-muted)"),
                LineHeight = raw("1.5")
            });

        global(".ja-access form",
            new CssRule
            {
                Display = raw("grid"),
                MarginTop = raw("24px"),
                Gap = raw("16px")
            });

        global(".ja-access label",
            new CssRule
            {
                Display = raw("grid"),
                Gap = raw("7px"),
                FontSize = raw("13px"),
                FontWeight = raw("650")
            });

        global(".ja-access__error",
            new CssRule
            {
                Color = raw("var(--danger) !important"),
                FontSize = raw("13px")
            });

        global(".ja-error__content",
            new CssRule
            {
                TextAlign = raw("center")
            });

        global(".ja-error__code",
            new CssRule
            {
                Display = raw("block"),
                Color = raw("var(--accent)"),
                FontSize = raw("72px"),
                FontWeight = raw("800"),
                LineHeight = raw("1")
            });

        global(".ja-error--internal-server-error .ja-error__code",
            new CssRule
            {
                Color = raw("var(--danger)")
            });

        global(".ja-error h1",
            new CssRule
            {
                Margin = raw("18px 0 8px"),
                FontSize = raw("28px")
            });

        global(".ja-error p",
            new CssRule
            {
                Margin = raw("0"),
                Color = raw("var(--text-muted)"),
                LineHeight = raw("1.6")
            });

        global(".ja-error__action",
            new CssRule
            {
                MarginTop = raw("24px")
            });

        global(":where(a, button, input, select):focus-visible",
            new CssRule
            {
                Outline = raw("3px solid color-mix(in srgb, var(--accent) 35%, transparent)"),
                OutlineOffset = raw("2px")
            });

        Media(".ja-header", "(max-width: 1080px)",
            new CssRule
            {
                AlignItems = raw("flex-start")
            });

        Media(".ja-header__actions", "(max-width: 1080px)",
            new CssRule
            {
                AlignItems = raw("flex-end"),
                FlexDirection = raw("column-reverse"),
                Gap = raw("6px")
            });

        Media(".ja-header__navigation", "(max-width: 1080px)",
            new CssRule
            {
                Width = raw("100%")
            });

        Media(".ja-preferences", "(max-width: 1080px)",
            new CssRule
            {
                FlexWrap = raw("wrap"),
                JustifyContent = raw("flex-end")
            });

        Media(".ja-tdesign-header", "(max-width: 1080px)",
            new CssRule
            {
                AlignItems = raw("flex-start")
            });

        Media(".ja-tdesign-header__actions", "(max-width: 1080px)",
            new CssRule
            {
                AlignItems = raw("flex-end"),
                FlexDirection = raw("column-reverse"),
                Gap = raw("6px")
            });

        Media(".ja-tdesign-header__navigation", "(max-width: 1080px)",
            new CssRule
            {
                Width = raw("100%")
            });

        Media(".ja-shell", "(max-width: 760px)",
            new CssRule
            {
                Display = raw("block"),
                MinWidth = raw("0")
            });

        Media(".ja-shell__sidebar", "(max-width: 760px)",
            new CssRule
            {
                Position = raw("static"),
                Height = raw("auto"),
                Overflow = raw("visible"),
                BorderRight = raw("0"),
                BorderBottom = raw("1px solid #293a33")
            });

        Media(".ja-tdesign-layout", "(max-width: 760px)",
            new CssRule
            {
                FlexDirection = raw("column !important"),
                MinWidth = raw("0")
            });

        Media(".ja-tdesign-layout[data-shell-collapsed] > [data-shell-region=\"sidebar\"]", "(max-width: 760px)",
            new CssRule
            {
                Position = raw("static"),
                Width = raw("100% !important"),
                MaxWidth = raw("100%"),
                Height = raw("auto"),
                Overflow = raw("visible"),
                BorderRight = raw("0"),
                BorderBottom = raw("1px solid var(--border)"),
                Flex = raw("0 0 auto !important")
            });

        Media(".ja-tdesign-layout > [data-shell-region=\"main\"]", "(max-width: 760px)",
            new CssRule
            {
                Width = raw("100%")
            });

        Media(".ja-tdesign-layout [data-shell-region=\"header\"]", "(max-width: 760px)",
            new CssRule
            {
                Position = raw("static"),
                Height = raw("auto !important"),
                MinHeight = raw("64px")
            });

        Media(".ja-tdesign-layout__header", "(max-width: 760px)",
            new CssRule
            {
                AlignItems = raw("flex-start"),
                FlexWrap = raw("wrap")
            });

        Media(".ja-tdesign-sidebar-shell", "(max-width: 760px)",
            new CssRule
            {
                FlexDirection = raw("column"),
                Width = raw("100%"),
                MinHeight = raw("0")
            });

        Media(".ja-iconbar", "(max-width: 760px)",
            new CssRule
            {
                Flex = raw("0 0 auto"),
                FlexDirection = raw("row"),
                Width = raw("100%"),
                MinHeight = raw("58px"),
                Padding = raw("8px 12px"),
                OverflowX = raw("auto"),
                OverscrollBehaviorInline = raw("contain")
            });

        Media(".ja-iconbar__items", "(max-width: 760px)",
            new CssRule
            {
                FlexDirection = raw("row"),
                Gap = raw("6px")
            });

        Media(".ja-tdesign-sidebar-shell__menu", "(max-width: 760px)",
            new CssRule
            {
                Width = raw("100%"),
                Padding = raw("10px 12px"),
                BorderTop = raw("1px solid var(--border)"),
                BorderLeft = raw("0")
            });

        Media(".ja-tdesign-sidebar-shell__brand", "(max-width: 760px)",
            new CssRule
            {
                MinHeight = raw("28px"),
                Margin = raw("0 4px 8px")
            });

        Media("[data-navigation-orientation=\"vertical\"]", "(max-width: 760px)",
            new CssRule
            {
                Display = raw("flex !important"),
                AlignItems = raw("flex-start"),
                Width = raw("100%"),
                OverflowX = raw("auto"),
                OverscrollBehaviorInline = raw("contain")
            });

        Media(".ja-sidebar", "(max-width: 760px)",
            new CssRule
            {
                MinHeight = raw("0"),
                Padding = raw("10px 12px")
            });

        Media(".ja-sidebar__logo", "(max-width: 760px)",
            new CssRule
            {
                MinHeight = raw("34px"),
                Margin = raw("0 6px 8px"),
                FontSize = raw("16px")
            });

        Media(".ja-sidebar__list", "(max-width: 760px)",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("flex-start"),
                Width = raw("100%"),
                PaddingBottom = raw("3px"),
                Gap = raw("4px"),
                OverflowX = raw("auto"),
                OverscrollBehaviorInline = raw("contain")
            });

        Media(".ja-sidebar__item", "(max-width: 760px)",
            new CssRule
            {
                Flex = raw("0 0 auto"),
                Margin = raw("0")
            });

        Media(".ja-sidebar__link,   .ja-sidebar__button", "(max-width: 760px)",
            new CssRule
            {
                MinHeight = raw("36px"),
                Width = raw("auto"),
                WhiteSpace = raw("nowrap")
            });

        Media(".ja-sidebar__children", "(max-width: 760px)",
            new CssRule
            {
                Display = raw("flex"),
                Padding = raw("4px 0 0 8px"),
                Gap = raw("4px")
            });

        Media(".ja-sidebar__children .ja-sidebar__item", "(max-width: 760px)",
            new CssRule
            {
                Flex = raw("0 0 auto")
            });

        Media(".ja-sidebar__children .ja-sidebar__link,   .ja-sidebar__children .ja-sidebar__button", "(max-width: 760px)",
            new CssRule
            {
                MinHeight = raw("32px"),
                Padding = raw("6px 10px")
            });

        Media(".ja-shell__header", "(max-width: 760px)",
            new CssRule
            {
                Position = raw("static")
            });

        Media(".ja-header", "(max-width: 760px)",
            new CssRule
            {
                Display = raw("block"),
                MinHeight = raw("0"),
                Padding = raw("12px 16px")
            });

        Media(".ja-shell__sidebar-toggle", "(max-width: 760px)",
            new CssRule
            {
                AlignSelf = raw("flex-start"),
                Margin = raw("12px 0 0 12px")
            });

        Media(".ja-header__subtitle", "(max-width: 760px)",
            new CssRule
            {
                Display = raw("none")
            });

        Media(".ja-header__actions", "(max-width: 760px)",
            new CssRule
            {
                AlignItems = raw("stretch"),
                MarginTop = raw("10px")
            });

        Media(".ja-tdesign-header", "(max-width: 760px)",
            new CssRule
            {
                Display = raw("block"),
                MinHeight = raw("0"),
                Padding = raw("12px 16px")
            });

        Media(".ja-tdesign-header__subtitle", "(max-width: 760px)",
            new CssRule
            {
                Display = raw("none")
            });

        Media(".ja-tdesign-header__actions", "(max-width: 760px)",
            new CssRule
            {
                AlignItems = raw("stretch"),
                MarginTop = raw("10px")
            });

        Media(".ja-tdesign-header__navigation", "(max-width: 760px)",
            new CssRule
            {
                MarginTop = raw("10px")
            });

        Media(".ja-header__navigation", "(max-width: 760px)",
            new CssRule
            {
                MarginTop = raw("10px")
            });

        Media(".ja-preferences,   .ja-user-region", "(max-width: 760px)",
            new CssRule
            {
                JustifyContent = raw("flex-start"),
                OverflowX = raw("auto")
            });

        Media(".ja-page", "(max-width: 760px)",
            new CssRule
            {
                Padding = raw("18px 14px 28px")
            });

        Media(".ja-page__header", "(max-width: 760px)",
            new CssRule
            {
                Display = raw("block"),
                MarginBottom = raw("16px")
            });

        Media(".ja-page__title", "(max-width: 760px)",
            new CssRule
            {
                FontSize = raw("23px")
            });

        Media(".ja-page__actions", "(max-width: 760px)",
            new CssRule
            {
                JustifyContent = raw("flex-start"),
                MarginTop = raw("14px")
            });

        Media(".ja-tdesign-page-container", "(max-width: 760px)",
            new CssRule
            {
                Width = raw("calc(100% - 28px)"),
                Margin = raw("14px")
            });

        Media(".ja-tdesign-page-container__header", "(max-width: 760px)",
            new CssRule
            {
                Display = raw("block"),
                MarginBottom = raw("16px")
            });

        Media(".ja-tdesign-page-container__title", "(max-width: 760px)",
            new CssRule
            {
                FontSize = raw("23px")
            });

        Media(".ja-access,   .ja-error", "(max-width: 760px)",
            new CssRule
            {
                Padding = raw("20px")
            });

        Media(".ja-access__panel", "(max-width: 760px)",
            new CssRule
            {
                Padding = raw("24px 20px")
            });

        Media(".ja-error__code", "(max-width: 760px)",
            new CssRule
            {
                FontSize = raw("58px")
            });

        Media(".ja-preference-toggle", "(max-width: 430px)",
            new CssRule
            {
                Display = raw("none")
            });

        Media(".ja-user", "(max-width: 430px)",
            new CssRule
            {
                MaxWidth = raw("120px")
            });

        global(".ja-tdesign-page-container--dashboard",
            new CssRule
            {
                Width = raw("min(calc(100% - 36px), 1480px)"),
                Margin = raw("18px auto 28px"),
                Background = raw("transparent !important"),
                Border = raw("0 !important"),
                Additional = [new("box-shadow", none, Important: true)]
            });

        global(".ja-tdesign-page-container--dashboard .ja-tdesign-page-container__header",
            new CssRule
            {
                AlignItems = raw("center"),
                MarginBottom = raw("14px")
            });

        global(".ja-tdesign-page-container--dashboard .ja-tdesign-page-container__title",
            new CssRule
            {
                MarginTop = raw("5px"),
                FontSize = raw("21px")
            });

        global(".ja-tdesign-page-container--dashboard .ja-tdesign-page-container__subtitle",
            new CssRule
            {
                MarginTop = raw("4px"),
                FontSize = raw("13px")
            });

        // The production administration pages share a compact TDesign Starter-like work surface.
        // 生产管理页共用紧凑的 TDesign Starter 风格工作台，状态层次由数据与选择关系表达。
        global(".ja-session-state",
            new CssRule
            {
                Display = raw("grid"),
                MinHeight = raw("100vh"),
                PlaceItems = raw("center"),
                Gap = raw("10px"),
                Color = raw("var(--text-muted)"),
                FontSize = raw("14px"),
                Background = raw("var(--app-bg)")
            });

        global(".ja-session-state__spinner",
            new CssRule
            {
                Width = raw("24px"),
                Height = raw("24px"),
                Border = raw("2px solid var(--border-strong)"),
                BorderTopColor = raw("var(--accent)"),
                BorderRadius = raw("50%"),
                Animation = raw(sessionSpin + " 720ms linear infinite")
            });

        global(".ja-header-context",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center"),
                MinWidth = raw("0"),
                Gap = raw("8px")
            });

        global(".ja-organization-picker",
            new CssRule
            {
                Display = raw("grid"),
                GridTemplateColumns = raw("auto minmax(112px, 180px)"),
                AlignItems = raw("center"),
                MinWidth = raw("0"),
                PaddingRight = raw("8px"),
                Gap = raw("7px"),
                Color = raw("var(--text-muted)"),
                FontSize = raw("12px"),
                BorderRight = raw("1px solid var(--border)")
            });

        global(".ja-organization-picker select",
            new CssRule
            {
                MinWidth = raw("0"),
                MinHeight = raw("32px"),
                Padding = raw("4px 26px 4px 8px"),
                Overflow = raw("hidden"),
                Color = raw("var(--text)"),
                FontSize = raw("12px"),
                FontWeight = raw("600"),
                TextOverflow = raw("ellipsis"),
                WhiteSpace = raw("nowrap"),
                Background = raw("var(--surface-subtle)"),
                Border = raw("1px solid var(--border)"),
                BorderRadius = raw("4px")
            });

        global(".ja-user-region",
            new CssRule
            {
                MinWidth = raw("0"),
                PaddingLeft = raw("8px"),
                BorderLeft = raw("1px solid var(--border)")
            });

        global(".ja-access-command",
            new CssRule
            {
                MinHeight = raw("30px"),
                Padding = raw("4px 8px"),
                FontSize = raw("12px"),
                Background = raw("transparent"),
                Border = raw("0")
            });

        global(".ja-access-command[data-access-command=\"sign-out\"]",
            new CssRule
            {
                Color = raw("var(--danger)")
            });

        global(".ja-overview",
            new CssRule
            {
                Display = raw("grid"),
                MinWidth = raw("0"),
                Gap = raw("16px")
            });

        global(".ja-overview__metrics",
            new CssRule
            {
                Display = raw("grid"),
                GridTemplateColumns = raw("repeat(4, minmax(0, 1fr))"),
                Gap = raw("16px")
            });

        global(".ja-overview__metric",
            new CssRule
            {
                MinWidth = raw("0"),
                MinHeight = raw("132px"),
                Padding = raw("18px 20px"),
                Background = raw("var(--surface)"),
                Border = raw("1px solid var(--border)"),
                BorderTop = raw("3px solid var(--accent)"),
                BorderRadius = raw("6px"),
                BoxShadow = var("--shadow"),
                Animation = raw(dashboardEnter + " 260ms ease both")
            });

        global(".ja-overview__metric:nth-child(2)",
            new CssRule
            {
                AnimationDelay = raw("35ms")
            });

        global(".ja-overview__metric:nth-child(3)",
            new CssRule
            {
                AnimationDelay = raw("70ms")
            });

        global(".ja-overview__metric:nth-child(4)",
            new CssRule
            {
                AnimationDelay = raw("105ms")
            });

        global(".ja-overview__metric span, .ja-overview__metric small",
            new CssRule
            {
                Display = raw("block"),
                Overflow = raw("hidden"),
                Color = raw("var(--text-muted)"),
                TextOverflow = raw("ellipsis"),
                WhiteSpace = raw("nowrap")
            });

        global(".ja-overview__metric span",
            new CssRule
            {
                FontSize = raw("13px"),
                FontWeight = raw("600")
            });

        global(".ja-overview__metric strong",
            new CssRule
            {
                Display = raw("block"),
                Margin = raw("16px 0 8px"),
                Overflow = raw("hidden"),
                FontSize = raw("24px"),
                LineHeight = raw("1"),
                TextOverflow = raw("ellipsis"),
                WhiteSpace = raw("nowrap")
            });

        global(".ja-overview__metric small",
            new CssRule
            {
                FontSize = raw("12px")
            });

        global(".ja-overview__grid",
            new CssRule
            {
                Display = raw("grid"),
                GridTemplateColumns = raw("repeat(2, minmax(0, 1fr))"),
                MinWidth = raw("0"),
                Gap = raw("16px")
            });

        global(".ja-overview__panel, .ja-management__panel",
            new CssRule
            {
                MinWidth = raw("0"),
                Background = raw("var(--surface)"),
                Border = raw("1px solid var(--border)"),
                BorderRadius = raw("6px"),
                BoxShadow = var("--shadow")
            });

        global(".ja-overview__panel-header, .ja-management__panel-header",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center"),
                JustifyContent = raw("space-between"),
                MinHeight = raw("68px"),
                Padding = raw("15px 20px"),
                Gap = raw("16px"),
                BorderBottom = raw("1px solid var(--border)")
            });

        global(".ja-overview__panel-header h2, .ja-overview__panel-header p, .ja-management__panel-header h2, .ja-management__panel-header p",
            new CssRule
            {
                Margin = raw("0")
            });

        global(".ja-overview__panel-header h2, .ja-management__panel-header h2",
            new CssRule
            {
                FontSize = raw("15px"),
                FontWeight = raw("650")
            });

        global(".ja-overview__panel-header p, .ja-management__panel-header p",
            new CssRule
            {
                MarginTop = raw("5px"),
                Color = raw("var(--text-muted)"),
                FontSize = raw("12px"),
                LineHeight = raw("1.4")
            });

        global(".ja-overview__organization-list, .ja-overview__role-list, .ja-management__item-list, .ja-management__role-list",
            new CssRule
            {
                Padding = raw("0"),
                Margin = raw("0"),
                ListStyle = raw("none")
            });

        global(".ja-overview__organization-list li, .ja-overview__role-list li, .ja-management__item-list li",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center"),
                MinHeight = raw("48px"),
                Padding = raw("10px 20px"),
                Gap = raw("10px"),
                BorderBottom = raw("1px solid var(--border)")
            });

        global(".ja-overview__organization-list li.is-current",
            new CssRule
            {
                Color = raw("var(--accent-strong)"),
                Background = raw("var(--accent-soft)")
            });

        global(".ja-overview__organization-code, .ja-management__code",
            new CssRule
            {
                Flex = raw("0 0 auto"),
                Padding = raw("3px 6px"),
                Color = raw("var(--accent-strong)"),
                FontSize = raw("11px"),
                FontWeight = raw("650"),
                Background = raw("var(--accent-soft)"),
                BorderRadius = raw("3px")
            });

        global(".ja-overview__role-list li",
            new CssRule
            {
                Color = raw("var(--text-muted)"),
                FontSize = raw("13px")
            });

        global(".ja-overview__empty, .ja-management__empty, .ja-management__loading, .ja-management__error",
            new CssRule
            {
                Margin = raw("0"),
                Padding = raw("20px"),
                Color = raw("var(--text-muted)"),
                FontSize = raw("13px"),
                LineHeight = raw("1.5")
            });

        global(".ja-management__error",
            new CssRule
            {
                Color = raw("var(--danger)"),
                Background = raw("var(--danger-soft)"),
                Border = raw("1px solid color-mix(in srgb, var(--danger) 30%, var(--border))"),
                BorderRadius = raw("6px")
            });

        global(".ja-management",
            new CssRule
            {
                Display = raw("grid"),
                MinWidth = raw("0"),
                Gap = raw("16px")
            });

        global(".ja-management__split",
            new CssRule
            {
                Display = raw("grid"),
                GridTemplateColumns = raw("minmax(0, 7fr) minmax(300px, 4fr)"),
                AlignItems = raw("start"),
                MinWidth = raw("0"),
                Gap = raw("16px")
            });

        global(".ja-management__split--authorization",
            new CssRule
            {
                GridTemplateColumns = raw("minmax(260px, 4fr) minmax(0, 7fr)")
            });

        global(".ja-management__split--members",
            new CssRule
            {
                GridTemplateColumns = raw("minmax(0, 7fr) minmax(320px, 4fr)")
            });

        global(".ja-management__panel > h3, .ja-management__role-editor h3",
            new CssRule
            {
                Margin = raw("20px 20px 10px"),
                FontSize = raw("14px")
            });

        global(".ja-management__details",
            new CssRule
            {
                Display = raw("grid"),
                GridTemplateColumns = raw("repeat(3, minmax(0, 1fr))"),
                Padding = raw("20px"),
                Margin = raw("0"),
                Gap = raw("12px")
            });

        global(".ja-management__details div",
            new CssRule
            {
                MinWidth = raw("0"),
                Padding = raw("12px"),
                Background = raw("var(--surface-subtle)"),
                Border = raw("1px solid var(--border)"),
                BorderRadius = raw("4px")
            });

        global(".ja-management__details dt, .ja-management__details dd",
            new CssRule
            {
                Margin = raw("0")
            });

        global(".ja-management__details dt",
            new CssRule
            {
                Color = raw("var(--text-muted)"),
                FontSize = raw("12px")
            });

        global(".ja-management__details dd",
            new CssRule
            {
                MarginTop = raw("6px"),
                Overflow = raw("hidden"),
                FontSize = raw("13px"),
                FontWeight = raw("600"),
                TextOverflow = raw("ellipsis"),
                WhiteSpace = raw("nowrap")
            });

        global(".ja-management__form",
            new CssRule
            {
                Display = raw("grid"),
                Padding = raw("20px"),
                Gap = raw("14px")
            });

        global(".ja-management__form--inline",
            new CssRule
            {
                GridTemplateColumns = raw("minmax(0, 1fr) minmax(0, 1fr) auto"),
                AlignItems = raw("end")
            });

        global(".ja-management__form label",
            new CssRule
            {
                Display = raw("grid"),
                Gap = raw("6px"),
                Color = raw("var(--text-muted)"),
                FontSize = raw("12px"),
                FontWeight = raw("600")
            });

        global(".ja-management input, .ja-management select, .ja-management textarea",
            new CssRule
            {
                Width = raw("100%"),
                MinHeight = raw("34px"),
                Padding = raw("6px 9px"),
                Color = raw("var(--text)"),
                Background = raw("var(--surface)"),
                Border = raw("1px solid var(--border-strong)"),
                BorderRadius = raw("4px")
            });

        global(".ja-management textarea",
            new CssRule
            {
                MinHeight = raw("76px"),
                Resize = raw("vertical"),
                LineHeight = raw("1.5")
            });

        global(".ja-management__field-grid",
            new CssRule
            {
                Display = raw("grid"),
                GridTemplateColumns = raw("repeat(2, minmax(0, 1fr))"),
                Gap = raw("14px")
            });

        global(".ja-management__options",
            new CssRule
            {
                Display = raw("grid"),
                GridTemplateColumns = raw("repeat(2, minmax(0, 1fr))"),
                Padding = raw("12px"),
                Margin = raw("0"),
                Gap = raw("10px 14px"),
                Border = raw("1px solid var(--border)"),
                BorderRadius = raw("4px")
            });

        global(".ja-management__options legend",
            new CssRule
            {
                Padding = raw("0 5px"),
                Color = raw("var(--text-muted)"),
                FontSize = raw("12px"),
                FontWeight = raw("650")
            });

        global(".ja-management__options label",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center"),
                MinWidth = raw("0"),
                Gap = raw("8px"),
                Color = raw("var(--text)"),
                FontSize = raw("13px"),
                FontWeight = raw("500")
            });

        global(".ja-management__options input",
            new CssRule
            {
                Flex = raw("0 0 16px"),
                Width = raw("16px"),
                MinHeight = raw("16px"),
                Padding = raw("0")
            });

        global(".ja-management__profiles",
            new CssRule
            {
                Display = raw("grid"),
                GridTemplateColumns = raw("repeat(3, minmax(0, 1fr))"),
                Margin = raw("20px 20px 0"),
                Border = raw("1px solid var(--border-strong)"),
                BorderRadius = raw("4px"),
                Overflow = raw("hidden")
            });

        global(".ja-management__profiles button",
            new CssRule
            {
                Color = raw("var(--text-muted)"),
                Background = raw("var(--surface)"),
                Border = raw("0"),
                BorderRight = raw("1px solid var(--border-strong)"),
                BorderRadius = raw("0")
            });

        global(".ja-management__profiles button:last-child",
            new CssRule
            {
                BorderRight = raw("0")
            });

        global(".ja-management__profiles button:hover, .ja-management__profiles button.is-selected",
            new CssRule
            {
                Color = raw("var(--accent-strong)"),
                Background = raw("var(--accent-soft)")
            });

        global(".ja-management__commands",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center"),
                FlexWrap = raw("wrap"),
                Gap = raw("8px")
            });

        global(".ja-management__secondary-button",
            new CssRule
            {
                Color = raw("var(--text) !important"),
                Background = raw("var(--surface) !important"),
                BorderColor = raw("var(--border-strong) !important")
            });

        global(".ja-management__danger-button",
            new CssRule
            {
                Color = raw("var(--danger) !important"),
                Background = raw("var(--danger-soft) !important"),
                BorderColor = raw("color-mix(in srgb, var(--danger) 45%, var(--border)) !important")
            });

        global(".ja-management__secret",
            new CssRule
            {
                Display = raw("grid"),
                Padding = raw("14px"),
                Margin = raw("20px 20px 0"),
                Gap = raw("6px"),
                Color = raw("var(--text)"),
                Background = raw("var(--warning-soft)"),
                Border = raw("1px solid color-mix(in srgb, var(--warning) 38%, var(--border))"),
                BorderRadius = raw("4px")
            });

        global(".ja-management__secret code",
            new CssRule
            {
                OverflowWrap = raw("anywhere"),
                FontSize = raw("13px")
            });

        global(".ja-management__secret span",
            new CssRule
            {
                Color = raw("var(--text-muted)"),
                FontSize = raw("12px")
            });

        global(".ja-management button",
            new CssRule
            {
                MinHeight = raw("34px"),
                Padding = raw("6px 12px"),
                Color = raw("#ffffff"),
                FontSize = raw("13px"),
                FontWeight = raw("600"),
                Background = raw("var(--accent)"),
                Border = raw("1px solid var(--accent)"),
                BorderRadius = raw("4px")
            });

        global(".ja-management button:hover",
            new CssRule
            {
                Background = raw("var(--accent-strong)")
            });

        global(".ja-management__table-wrap",
            new CssRule
            {
                Width = raw("100%"),
                OverflowX = raw("auto")
            });

        global(".ja-management__table",
            new CssRule
            {
                Width = raw("100%"),
                MinWidth = raw("560px"),
                BorderCollapse = raw("collapse")
            });

        global(".ja-management__table th, .ja-management__table td",
            new CssRule
            {
                Padding = raw("13px 20px"),
                TextAlign = raw("left"),
                VerticalAlign = raw("middle"),
                BorderBottom = raw("1px solid var(--border)")
            });

        global(".ja-management__table th",
            new CssRule
            {
                Color = raw("var(--text-muted)"),
                FontSize = raw("12px"),
                FontWeight = raw("600"),
                Background = raw("var(--surface-subtle)")
            });

        global(".ja-management__table td",
            new CssRule
            {
                FontSize = raw("13px")
            });

        global(".ja-management__table td strong, .ja-management__table td span",
            new CssRule
            {
                Display = raw("block")
            });

        global(".ja-management__table td span",
            new CssRule
            {
                MarginTop = raw("3px"),
                Color = raw("var(--text-muted)"),
                FontSize = raw("12px")
            });

        global(".ja-management__table tr.is-selected td, .ja-management__table tbody tr:hover td",
            new CssRule
            {
                Background = raw("var(--accent-soft)")
            });

        global(".ja-management__text-button",
            new CssRule
            {
                Padding = raw("4px 0 !important"),
                Color = raw("var(--accent) !important"),
                Background = raw("transparent !important"),
                Border = raw("0 !important")
            });

        global(".ja-management__role-list",
            new CssRule
            {
                Padding = raw("8px 0")
            });

        global(".ja-management__role-list button",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center"),
                JustifyContent = raw("space-between"),
                Width = raw("100%"),
                Padding = raw("11px 20px"),
                Color = raw("var(--text)"),
                TextAlign = raw("left"),
                Background = raw("transparent"),
                Border = raw("0"),
                BorderLeft = raw("3px solid transparent"),
                BorderRadius = raw("0")
            });

        global(".ja-management__role-list button:hover, .ja-management__role-list li.is-selected button",
            new CssRule
            {
                Color = raw("var(--accent-strong)"),
                Background = raw("var(--accent-soft)"),
                BorderLeftColor = raw("var(--accent)")
            });

        global(".ja-management__role-list small, .ja-management__check small",
            new CssRule
            {
                Color = raw("var(--text-muted)"),
                FontSize = raw("11px")
            });

        global(".ja-management__grant-list, .ja-management__role-editor",
            new CssRule
            {
                Display = raw("grid"),
                Padding = raw("12px 20px 20px"),
                Gap = raw("8px")
            });

        global(".ja-management__check",
            new CssRule
            {
                Display = raw("grid"),
                GridTemplateColumns = raw("16px minmax(0, 1fr) auto"),
                AlignItems = raw("center"),
                MinHeight = raw("40px"),
                Padding = raw("7px 9px"),
                Gap = raw("8px"),
                FontSize = raw("13px"),
                Background = raw("var(--surface-subtle)"),
                Border = raw("1px solid var(--border)"),
                BorderRadius = raw("4px")
            });

        global(".ja-management__check input",
            new CssRule
            {
                Width = raw("16px"),
                MinHeight = raw("16px"),
                Padding = raw("0")
            });

        global(".ja-management__grant-list + button, .ja-management__role-editor > button",
            new CssRule
            {
                Margin = raw("0 20px 20px")
            });

        Media(".ja-overview__metrics", "(max-width: 1100px)",
            new CssRule
            {
                GridTemplateColumns = raw("repeat(2, minmax(0, 1fr))")
            });

        Media(".ja-management__split, .ja-management__split--authorization, .ja-management__split--members", "(max-width: 980px)",
            new CssRule
            {
                GridTemplateColumns = raw("1fr")
            });

        Media(".ja-header-context", "(max-width: 1080px)",
            new CssRule
            {
                FlexWrap = raw("wrap"),
                JustifyContent = raw("flex-end")
            });

        Media(".ja-overview__grid", "(max-width: 760px)",
            new CssRule
            {
                GridTemplateColumns = raw("1fr")
            });

        Media(".ja-management__details", "(max-width: 620px)",
            new CssRule
            {
                GridTemplateColumns = raw("1fr")
            });

        Media(".ja-overview__metrics", "(max-width: 620px)",
            new CssRule
            {
                GridTemplateColumns = raw("1fr")
            });

        Media(".ja-management__form--inline", "(max-width: 620px)",
            new CssRule
            {
                GridTemplateColumns = raw("1fr")
            });

        Media(".ja-management__field-grid, .ja-management__options", "(max-width: 620px)",
            new CssRule
            {
                GridTemplateColumns = raw("1fr")
            });

        Media(".ja-management__commands button", "(max-width: 620px)",
            new CssRule
            {
                Flex = raw("1 1 100%"),
                Width = raw("100%")
            });

        Media(".ja-organization-picker", "(max-width: 620px)",
            new CssRule
            {
                GridTemplateColumns = raw("1fr"),
                PaddingRight = raw("0"),
                BorderRight = raw("0")
            });

        global(".ja-schedules__status",
            new CssRule
            {
                Display = raw("inline-block !important"),
                Padding = raw("3px 7px"),
                MarginTop = raw("0 !important"),
                FontSize = raw("11px !important"),
                FontWeight = raw("650"),
                BorderRadius = raw("3px")
            });

        global(".ja-schedules__status.is-enabled",
            new CssRule
            {
                Color = raw("var(--success) !important"),
                Background = raw("var(--success-soft)")
            });

        global(".ja-schedules__status.is-disabled",
            new CssRule
            {
                Color = raw("var(--text-muted) !important"),
                Background = raw("var(--surface-strong)")
            });

        global(".ja-schedules__summary",
            new CssRule
            {
                Display = raw("grid"),
                Padding = raw("12px"),
                Margin = raw("0"),
                Gap = raw("10px"),
                Background = raw("var(--surface-subtle)"),
                Border = raw("1px solid var(--border)"),
                BorderRadius = raw("4px")
            });

        global(".ja-schedules__summary div",
            new CssRule
            {
                Display = raw("grid"),
                GridTemplateColumns = raw("110px minmax(0, 1fr)"),
                Gap = raw("10px")
            });

        global(".ja-schedules__summary dt, .ja-schedules__summary dd",
            new CssRule
            {
                MinWidth = raw("0"),
                Margin = raw("0"),
                FontSize = raw("12px")
            });

        global(".ja-schedules__summary dt",
            new CssRule
            {
                Color = raw("var(--text-muted)"),
                FontWeight = raw("650")
            });

        global(".ja-schedules__summary dd",
            new CssRule
            {
                OverflowWrap = raw("anywhere"),
                Color = raw("var(--text)")
            });

        Media("*,   *::before,   *::after", "(prefers-reduced-motion: reduce)",
            new CssRule
            {
                AnimationDuration = raw("0.01ms !important"),
                ScrollBehavior = raw("auto !important"),
                TransitionDuration = raw("0.01ms !important")
            });

        return true;
    }

    private static void Media(string selector, string prelude, CssRule rule)
        => global(selector, new CssRule
        {
            Children = [new(CssChildKind.Media, prelude, rule)]
        });
}
