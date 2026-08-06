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

        global(".jazor-admin-application",
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
                ["--warning"] = raw("#d18a19"),
                ["--warning-soft"] = raw("#fff7e8"),
                ["--info"] = raw("#2f6fed"),
                ["--info-soft"] = raw("#e9f1ff"),
                ["--shadow"] = shadows(new CssShadow(px(0), px(4), Blur: px(14), Color: rgba(31, 52, 78, 0.05))),
                MinHeight = raw("100vh"),
                Background = raw("var(--app-bg)"),
                Color = raw("var(--text)")
            });

        global(".jazor-admin-application--dark",
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
                ["--warning"] = raw("#f1c35d"),
                ["--warning-soft"] = raw("#45391f"),
                ["--info"] = raw("#8bbaf0"),
                ["--info-soft"] = raw("#24364c"),
                ["--shadow"] = shadows(
                    new CssShadow(px(0), px(1), Blur: px(2), Color: rgba(0, 0, 0, 0.28)),
                    new CssShadow(px(0), px(10), Blur: px(28), Color: rgba(0, 0, 0, 0.2)))
            });

        Media(".jazor-admin-application--system", "(prefers-color-scheme: dark)",
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
                ["--warning"] = raw("#f1c35d"),
                ["--warning-soft"] = raw("#45391f"),
                ["--info"] = raw("#8bbaf0"),
                ["--info-soft"] = raw("#24364c"),
                ["--shadow"] = shadows(
                    new CssShadow(px(0), px(1), Blur: px(2), Color: rgba(0, 0, 0, 0.28)),
                    new CssShadow(px(0), px(10), Blur: px(28), Color: rgba(0, 0, 0, 0.2)))
            });

        global(".jazor-admin-application--grayscale",
            new CssRule
            {
                Filter = raw("grayscale(1)")
            });

        global(".jazor-admin-shell",
            new CssRule
            {
                Display = raw("grid"),
                GridTemplateColumns = raw("232px minmax(0, 1fr)"),
                MinHeight = raw("100vh")
            });

        global(".jazor-admin-shell--top",
            new CssRule
            {
                Display = raw("block")
            });

        global(".jazor-admin-shell__sidebar",
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

        global(".jazor-admin-shell--collapsed",
            new CssRule
            {
                GridTemplateColumns = raw("0 minmax(0, 1fr)")
            });

        global(".jazor-admin-shell--collapsed .jazor-admin-shell__sidebar",
            new CssRule
            {
                Display = raw("none"),
                Width = raw("0"),
                BorderRight = raw("0")
            });

        global(".jazor-admin-shell__main",
            new CssRule
            {
                MinWidth = raw("0")
            });

        global(".jazor-admin-shell__header",
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

        global(".jazor-admin-shell__sidebar-toggle",
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

        global(".jazor-admin-shell__sidebar-toggle::before",
            new CssRule
            {
                FontSize = raw("20px"),
                LineHeight = raw("1"),
                Content = raw("\"\\2630\"")
            });

        global(".jazor-admin-shell__sidebar-toggle:hover",
            new CssRule
            {
                Background = raw("var(--surface-strong)")
            });

        global(".jazor-admin-shell__content",
            new CssRule
            {
                Width = raw("100%")
            });

        global(".jazor-admin-tdesign-layout",
            new CssRule
            {
                MinWidth = raw("0"),
                MinHeight = raw("100vh"),
                Background = raw("var(--background)")
            });

        global(".jazor-admin-tdesign-layout > [data-shell-region=\"sidebar\"]",
            new CssRule
            {
                Position = raw("sticky"),
                Top = raw("0"),
                ZIndex = raw("20"),
                Height = raw("100vh"),
                Overflow = raw("auto"),
                BorderRight = raw("1px solid var(--border)")
            });

        global(".jazor-admin-tdesign-layout[data-shell-collapsed=\"false\"] > [data-shell-region=\"sidebar\"]",
            new CssRule
            {
                Width = raw("240px !important"),
                Flex = raw("0 0 240px !important")
            });

        global(".jazor-admin-tdesign-layout[data-shell-collapsed=\"true\"] > [data-shell-region=\"sidebar\"]",
            new CssRule
            {
                Width = raw("64px !important"),
                Flex = raw("0 0 64px !important")
            });

        global(".jazor-admin-tdesign-layout > [data-shell-region=\"main\"]",
            new CssRule
            {
                MinWidth = raw("0")
            });

        global(".jazor-admin-tdesign-layout [data-shell-region=\"header\"]",
            new CssRule
            {
                Position = raw("sticky"),
                Top = raw("0"),
                ZIndex = raw("15"),
                BorderBottom = raw("1px solid var(--border)")
            });

        global(".jazor-admin-tdesign-layout__header",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center"),
                Width = raw("100%"),
                MinWidth = raw("0"),
                MinHeight = raw("64px"),
                Gap = raw("14px")
            });

        global(".jazor-admin-tdesign-layout [data-shell-region=\"content\"]",
            new CssRule
            {
                Width = raw("100%"),
                MinWidth = raw("0")
            });

        global(".jazor-admin-tdesign-sidebar-shell",
            new CssRule
            {
                Display = raw("flex"),
                MinHeight = raw("100%"),
                Background = raw("var(--surface)")
            });

        global(".jazor-admin-iconbar",
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

        global(".jazor-admin-iconbar__items",
            new CssRule
            {
                Display = raw("flex"),
                FlexDirection = raw("column"),
                Gap = raw("10px")
            });

        global(".jazor-admin-iconbar__link",
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

        global(".jazor-admin-iconbar__link:hover, .jazor-admin-iconbar__link.is-selected",
            new CssRule
            {
                Color = raw("#ffffff"),
                Background = raw("#2f6fed")
            });

        global(".jazor-admin-iconbar__icon",
            new CssRule
            {
                Position = raw("relative"),
                Display = raw("block"),
                Width = raw("18px"),
                Height = raw("18px")
            });

        global(".jazor-admin-iconbar__icon::before, .jazor-admin-iconbar__icon::after",
            new CssRule
            {
                Position = raw("absolute"),
                Display = raw("block"),
                BoxSizing = raw("border-box"),
                Content = raw("\"\"")
            });

        global(".jazor-admin-iconbar__icon[data-iconbar-icon=\"dashboard\"]::before",
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

        global(".jazor-admin-iconbar__icon[data-iconbar-icon=\"organizations\"]::before", new CssRule
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

        global(".jazor-admin-iconbar__icon[data-iconbar-icon=\"organizations\"]::after", new CssRule
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

        global(".jazor-admin-iconbar__icon[data-iconbar-icon=\"authorization\"]::before", new CssRule
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

        global(".jazor-admin-iconbar__icon[data-iconbar-icon=\"authorization\"]::after", new CssRule
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

        global(".jazor-admin-iconbar__icon[data-iconbar-icon=\"accounts\"]::before", new CssRule
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

        global(".jazor-admin-iconbar__icon[data-iconbar-icon=\"accounts\"]::after", new CssRule
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

        global(".jazor-admin-iconbar__icon[data-iconbar-icon=\"configuration\"]::before", new CssRule
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

        global(".jazor-admin-iconbar__icon[data-iconbar-icon=\"configuration\"]::after", new CssRule
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

        global(".jazor-admin-tdesign-sidebar-shell__menu",
            new CssRule
            {
                Flex = raw("1 1 auto"),
                Width = raw("176px"),
                MinWidth = raw("0"),
                Padding = raw("14px 10px"),
                Overflow = raw("auto"),
                BorderLeft = raw("1px solid var(--border)")
            });

        global(".jazor-admin-tdesign-sidebar-shell__brand",
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

        global(".jazor-admin-tdesign-sidebar-shell__brand-mark",
            new CssRule
            {
                Display = raw("block"),
                Width = raw("18px"),
                Height = raw("18px"),
                Background = raw("#2f6fed"),
                BorderRadius = raw("4px"),
                BoxShadow = shadows(new CssShadow(px(0), px(0), Blur: px(0), Spread: px(4), Color: hex("dce8ff"), Inset: true))
            });

        global(".jazor-admin-tdesign-sidebar-shell__menu [data-navigation-orientation=\"vertical\"]",
            new CssRule
            {
                Width = raw("100%")
            });

        global(".jazor-admin-sidebar",
            new CssRule
            {
                MinHeight = raw("100%"),
                Padding = raw("20px 14px")
            });

        global(".jazor-admin-sidebar__logo",
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

        global(".jazor-admin-sidebar__list, .jazor-admin-sidebar__children",
            new CssRule
            {
                Padding = raw("0"),
                Margin = raw("0"),
                ListStyle = raw("none")
            });

        global(".jazor-admin-sidebar__item",
            new CssRule
            {
                Margin = raw("3px 0")
            });

        global(".jazor-admin-sidebar__item-content",
            new CssRule
            {
                Position = raw("relative")
            });

        global(".jazor-admin-sidebar__link, .jazor-admin-sidebar__button",
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

        global(".jazor-admin-sidebar__link:hover, .jazor-admin-sidebar__button:hover, .jazor-admin-sidebar__item.is-ancestor-selected > .jazor-admin-sidebar__item-content > .jazor-admin-sidebar__button",
            new CssRule
            {
                Color = raw("#ffffff"),
                Background = raw("#243a31")
            });

        global(".jazor-admin-sidebar__item.is-selected > .jazor-admin-sidebar__item-content > .jazor-admin-sidebar__link, .jazor-admin-sidebar__item.is-selected > .jazor-admin-sidebar__item-content > .jazor-admin-sidebar__button",
            new CssRule
            {
                Color = raw("#ffffff"),
                Background = raw("#087f5b")
            });

        global(".jazor-admin-sidebar__toggle",
            new CssRule
            {
                MarginLeft = raw("auto"),
                FontSize = raw("12px")
            });

        global(".jazor-admin-sidebar__children",
            new CssRule
            {
                Padding = raw("4px 0 4px 12px")
            });

        global(".jazor-admin-sidebar__children .jazor-admin-sidebar__link, .jazor-admin-sidebar__children .jazor-admin-sidebar__button",
            new CssRule
            {
                MinHeight = raw("36px"),
                PaddingLeft = raw("16px"),
                FontSize = raw("14px")
            });

        global(".jazor-admin-header",
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

        global(".jazor-admin-header__main, .jazor-admin-header__actions, .jazor-admin-header__toolbar, .jazor-admin-header__user-region, .jazor-admin__preferences, .jazor-admin__user-region",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center")
            });

        global(".jazor-admin-header__main",
            new CssRule
            {
                MinWidth = raw("0"),
                Gap = raw("12px")
            });

        global(".jazor-admin-header__logo",
            new CssRule
            {
                Color = raw("var(--accent)"),
                FontWeight = raw("750")
            });

        global(".jazor-admin-header__titles",
            new CssRule
            {
                MinWidth = raw("0")
            });

        global(".jazor-admin-header__title",
            new CssRule
            {
                Overflow = raw("hidden"),
                FontSize = raw("16px"),
                FontWeight = raw("700"),
                TextOverflow = raw("ellipsis"),
                WhiteSpace = raw("nowrap")
            });

        global(".jazor-admin-header__subtitle",
            new CssRule
            {
                MarginTop = raw("2px"),
                Color = raw("var(--text-muted)"),
                FontSize = raw("12px")
            });

        global(".jazor-admin-header__actions",
            new CssRule
            {
                JustifyContent = raw("flex-end"),
                MinWidth = raw("0"),
                Gap = raw("16px")
            });

        global(".jazor-admin-header__navigation",
            new CssRule
            {
                Flex = raw("1 1 auto"),
                MinWidth = raw("0")
            });

        global(".jazor-admin-tdesign-header",
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

        global(".jazor-admin-tdesign-header__main, .jazor-admin-tdesign-header__actions",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center")
            });

        global(".jazor-admin-tdesign-header__main",
            new CssRule
            {
                MinWidth = raw("0"),
                Gap = raw("12px")
            });

        global(".jazor-admin-tdesign-header__titles",
            new CssRule
            {
                MinWidth = raw("0")
            });

        global(".jazor-admin-tdesign-header__title",
            new CssRule
            {
                Overflow = raw("hidden"),
                FontSize = raw("16px"),
                FontWeight = raw("700"),
                TextOverflow = raw("ellipsis"),
                WhiteSpace = raw("nowrap")
            });

        global(".jazor-admin-tdesign-header__subtitle",
            new CssRule
            {
                MarginTop = raw("2px"),
                Color = raw("var(--text-muted)"),
                FontSize = raw("12px")
            });

        global(".jazor-admin-tdesign-header__navigation",
            new CssRule
            {
                Flex = raw("1 1 auto"),
                MinWidth = raw("0")
            });

        global(".jazor-admin-tdesign-header__actions",
            new CssRule
            {
                JustifyContent = raw("flex-end"),
                MinWidth = raw("0"),
                Gap = raw("16px")
            });

        global(".jazor-admin-sidebar--horizontal",
            new CssRule
            {
                MinHeight = raw("0"),
                Padding = raw("0"),
                Color = raw("var(--text)")
            });

        global(".jazor-admin-sidebar--horizontal > .jazor-admin-sidebar__list, .jazor-admin-sidebar--horizontal .jazor-admin-sidebar__children",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center"),
                Gap = raw("4px")
            });

        global(".jazor-admin-sidebar--horizontal > .jazor-admin-sidebar__list",
            new CssRule
            {
                OverflowX = raw("auto"),
                OverscrollBehaviorInline = raw("contain")
            });

        global(".jazor-admin-sidebar--horizontal .jazor-admin-sidebar__item",
            new CssRule
            {
                Display = raw("flex"),
                Flex = raw("0 0 auto"),
                AlignItems = raw("center"),
                Margin = raw("0")
            });

        global(".jazor-admin-sidebar--horizontal .jazor-admin-sidebar__link, .jazor-admin-sidebar--horizontal .jazor-admin-sidebar__button",
            new CssRule
            {
                Width = raw("auto"),
                MinHeight = raw("34px"),
                Padding = raw("6px 10px"),
                Color = raw("var(--text-muted)"),
                WhiteSpace = raw("nowrap")
            });

        global(".jazor-admin-sidebar--horizontal .jazor-admin-sidebar__link:hover, .jazor-admin-sidebar--horizontal .jazor-admin-sidebar__button:hover, .jazor-admin-sidebar--horizontal .jazor-admin-sidebar__item.is-ancestor-selected > .jazor-admin-sidebar__item-content > .jazor-admin-sidebar__button",
            new CssRule
            {
                Color = raw("var(--text)"),
                Background = raw("var(--surface-strong)")
            });

        global(".jazor-admin-sidebar--horizontal .jazor-admin-sidebar__item.is-selected > .jazor-admin-sidebar__item-content > .jazor-admin-sidebar__link, .jazor-admin-sidebar--horizontal .jazor-admin-sidebar__item.is-selected > .jazor-admin-sidebar__item-content > .jazor-admin-sidebar__button",
            new CssRule
            {
                Color = raw("var(--accent-strong)"),
                Background = raw("var(--accent-soft)")
            });

        global(".jazor-admin-sidebar--horizontal .jazor-admin-sidebar__children",
            new CssRule
            {
                Padding = raw("0 0 0 4px")
            });

        global(".jazor-admin-header__toolbar, .jazor-admin__preferences, .jazor-admin__user-region",
            new CssRule
            {
                Gap = raw("8px")
            });

        global(".jazor-admin__preference, .jazor-admin__preference-toggle",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center"),
                Gap = raw("6px"),
                Color = raw("var(--text-muted)"),
                FontSize = raw("12px"),
                WhiteSpace = raw("nowrap")
            });

        global(".jazor-admin__preference select, .jazor-admin-access input",
            new CssRule
            {
                MinHeight = raw("36px"),
                Padding = raw("7px 10px"),
                Color = raw("var(--text)"),
                Background = raw("var(--surface)"),
                Border = raw("1px solid var(--border-strong)"),
                BorderRadius = raw("5px")
            });

        global(".jazor-admin__preference select",
            new CssRule
            {
                MinHeight = raw("32px"),
                Padding = raw("4px 24px 4px 8px")
            });

        global(".jazor-admin__user",
            new CssRule
            {
                MaxWidth = raw("180px"),
                Overflow = raw("hidden"),
                FontSize = raw("13px"),
                FontWeight = raw("650"),
                TextOverflow = raw("ellipsis"),
                WhiteSpace = raw("nowrap")
            });

        global(".jazor-admin__access-command, .jazor-admin-error__action, .jazor-admin-access button",
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

        global(".jazor-admin__access-command:hover",
            new CssRule
            {
                Background = raw("var(--surface-strong)")
            });

        global(".jazor-admin-page",
            new CssRule
            {
                Width = raw("min(100%, 1480px)"),
                Margin = raw("0 auto"),
                Padding = raw("24px")
            });

        global(".jazor-admin-page__header",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("flex-end"),
                JustifyContent = raw("space-between"),
                MarginBottom = raw("20px"),
                Gap = raw("20px")
            });

        global(".jazor-admin-page__titles",
            new CssRule
            {
                MinWidth = raw("0")
            });

        global(".jazor-admin-page__breadcrumb",
            new CssRule
            {
                Display = raw("flex"),
                FlexWrap = raw("wrap"),
                Gap = raw("6px"),
                MarginBottom = raw("7px"),
                Color = raw("var(--text-muted)"),
                FontSize = raw("13px")
            });

        global(".jazor-admin-page__breadcrumb-item + .jazor-admin-page__breadcrumb-item::before",
            new CssRule
            {
                MarginRight = raw("6px"),
                Color = raw("var(--border-strong)"),
                Content = raw("\"/\"")
            });

        global(".jazor-admin-page__title",
            new CssRule
            {
                Margin = raw("0"),
                FontSize = raw("26px"),
                LineHeight = raw("1.25")
            });

        global(".jazor-admin-page__subtitle",
            new CssRule
            {
                MaxWidth = raw("760px"),
                Margin = raw("7px 0 0"),
                Color = raw("var(--text-muted)"),
                LineHeight = raw("1.5")
            });

        global(".jazor-admin-page__actions",
            new CssRule
            {
                Display = raw("flex"),
                FlexWrap = raw("wrap"),
                JustifyContent = raw("flex-end"),
                Gap = raw("8px")
            });

        global(".jazor-admin-error__action, .jazor-admin-access button",
            new CssRule
            {
                Color = raw("#ffffff"),
                Background = raw("var(--accent)"),
                BorderColor = raw("var(--accent)")
            });

        global(".jazor-admin-error__action:hover, .jazor-admin-access button:hover",
            new CssRule
            {
                Background = raw("var(--accent-strong)"),
                BorderColor = raw("var(--accent-strong)")
            });

        global(".jazor-admin-page__action--danger",
            new CssRule
            {
                Color = raw("var(--danger)"),
                BorderColor = raw("var(--danger)")
            });

        global(".jazor-admin-page__body > * + *",
            new CssRule
            {
                MarginTop = raw("20px")
            });

        global(".jazor-admin-tdesign-page-container",
            new CssRule
            {
                Width = raw("min(calc(100% - 48px), 1480px)"),
                MinWidth = raw("0"),
                Margin = raw("24px auto")
            });

        global(".jazor-admin-tdesign-page-container__header",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("flex-end"),
                JustifyContent = raw("space-between"),
                MarginBottom = raw("20px"),
                Gap = raw("20px")
            });

        global(".jazor-admin-tdesign-page-container__titles",
            new CssRule
            {
                MinWidth = raw("0")
            });

        global(".jazor-admin-tdesign-page-container__title",
            new CssRule
            {
                MarginTop = raw("7px"),
                FontSize = raw("26px"),
                FontWeight = raw("700"),
                LineHeight = raw("1.25")
            });

        global(".jazor-admin-tdesign-page-container__subtitle",
            new CssRule
            {
                MaxWidth = raw("760px"),
                MarginTop = raw("7px"),
                Color = raw("var(--text-muted)"),
                LineHeight = raw("1.5")
            });

        global(".jazor-admin-tdesign-page-container__body > * + *",
            new CssRule
            {
                MarginTop = raw("20px")
            });

        global(".jazor-admin-access, .jazor-admin-error",
            new CssRule
            {
                Display = raw("grid"),
                MinHeight = raw("100vh"),
                Padding = raw("32px"),
                PlaceItems = raw("center"),
                Background = raw("var(--app-bg)")
            });

        global(".jazor-admin-access__panel, .jazor-admin-error__content",
            new CssRule
            {
                Width = raw("min(100%, 420px)")
            });

        global(".jazor-admin-access__panel",
            new CssRule
            {
                Padding = raw("30px"),
                Background = raw("var(--surface)"),
                Border = raw("1px solid var(--border)"),
                BorderTop = raw("4px solid var(--accent)"),
                BorderRadius = raw("7px"),
                BoxShadow = var("--shadow")
            });

        global(".jazor-admin-access__brand",
            new CssRule
            {
                Color = raw("var(--accent)"),
                FontSize = raw("17px")
            });

        global(".jazor-admin-access h1",
            new CssRule
            {
                Margin = raw("22px 0 8px"),
                FontSize = raw("26px")
            });

        global(".jazor-admin-access p",
            new CssRule
            {
                Margin = raw("0"),
                Color = raw("var(--text-muted)"),
                LineHeight = raw("1.5")
            });

        global(".jazor-admin-access form",
            new CssRule
            {
                Display = raw("grid"),
                MarginTop = raw("24px"),
                Gap = raw("16px")
            });

        global(".jazor-admin-access label",
            new CssRule
            {
                Display = raw("grid"),
                Gap = raw("7px"),
                FontSize = raw("13px"),
                FontWeight = raw("650")
            });

        global(".jazor-admin-access__error",
            new CssRule
            {
                Color = raw("var(--danger) !important"),
                FontSize = raw("13px")
            });

        global(".jazor-admin-error__content",
            new CssRule
            {
                TextAlign = raw("center")
            });

        global(".jazor-admin-error__code",
            new CssRule
            {
                Display = raw("block"),
                Color = raw("var(--accent)"),
                FontSize = raw("72px"),
                FontWeight = raw("800"),
                LineHeight = raw("1")
            });

        global(".jazor-admin-error--internal-server-error .jazor-admin-error__code",
            new CssRule
            {
                Color = raw("var(--danger)")
            });

        global(".jazor-admin-error h1",
            new CssRule
            {
                Margin = raw("18px 0 8px"),
                FontSize = raw("28px")
            });

        global(".jazor-admin-error p",
            new CssRule
            {
                Margin = raw("0"),
                Color = raw("var(--text-muted)"),
                LineHeight = raw("1.6")
            });

        global(".jazor-admin-error__action",
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

        Media(".jazor-admin-header", "(max-width: 1080px)",
            new CssRule
            {
                AlignItems = raw("flex-start")
            });

        Media(".jazor-admin-header__actions", "(max-width: 1080px)",
            new CssRule
            {
                AlignItems = raw("flex-end"),
                FlexDirection = raw("column-reverse"),
                Gap = raw("6px")
            });

        Media(".jazor-admin-header__navigation", "(max-width: 1080px)",
            new CssRule
            {
                Width = raw("100%")
            });

        Media(".jazor-admin__preferences", "(max-width: 1080px)",
            new CssRule
            {
                FlexWrap = raw("wrap"),
                JustifyContent = raw("flex-end")
            });

        Media(".jazor-admin-tdesign-header", "(max-width: 1080px)",
            new CssRule
            {
                AlignItems = raw("flex-start")
            });

        Media(".jazor-admin-tdesign-header__actions", "(max-width: 1080px)",
            new CssRule
            {
                AlignItems = raw("flex-end"),
                FlexDirection = raw("column-reverse"),
                Gap = raw("6px")
            });

        Media(".jazor-admin-tdesign-header__navigation", "(max-width: 1080px)",
            new CssRule
            {
                Width = raw("100%")
            });

        Media(".jazor-admin-shell", "(max-width: 760px)",
            new CssRule
            {
                Display = raw("block"),
                MinWidth = raw("0")
            });

        Media(".jazor-admin-shell__sidebar", "(max-width: 760px)",
            new CssRule
            {
                Position = raw("static"),
                Height = raw("auto"),
                Overflow = raw("visible"),
                BorderRight = raw("0"),
                BorderBottom = raw("1px solid #293a33")
            });

        Media(".jazor-admin-tdesign-layout", "(max-width: 760px)",
            new CssRule
            {
                FlexDirection = raw("column !important"),
                MinWidth = raw("0")
            });

        Media(".jazor-admin-tdesign-layout[data-shell-collapsed] > [data-shell-region=\"sidebar\"]", "(max-width: 760px)",
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

        Media(".jazor-admin-tdesign-layout > [data-shell-region=\"main\"]", "(max-width: 760px)",
            new CssRule
            {
                Width = raw("100%")
            });

        Media(".jazor-admin-tdesign-layout [data-shell-region=\"header\"]", "(max-width: 760px)",
            new CssRule
            {
                Position = raw("static"),
                Height = raw("auto !important"),
                MinHeight = raw("64px")
            });

        Media(".jazor-admin-tdesign-layout__header", "(max-width: 760px)",
            new CssRule
            {
                AlignItems = raw("flex-start"),
                FlexWrap = raw("wrap")
            });

        Media(".jazor-admin-tdesign-sidebar-shell", "(max-width: 760px)",
            new CssRule
            {
                FlexDirection = raw("column"),
                Width = raw("100%"),
                MinHeight = raw("0")
            });

        Media(".jazor-admin-iconbar", "(max-width: 760px)",
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

        Media(".jazor-admin-iconbar__items", "(max-width: 760px)",
            new CssRule
            {
                FlexDirection = raw("row"),
                Gap = raw("6px")
            });

        Media(".jazor-admin-tdesign-sidebar-shell__menu", "(max-width: 760px)",
            new CssRule
            {
                Width = raw("100%"),
                Padding = raw("10px 12px"),
                BorderTop = raw("1px solid var(--border)"),
                BorderLeft = raw("0")
            });

        Media(".jazor-admin-tdesign-sidebar-shell__brand", "(max-width: 760px)",
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

        Media(".jazor-admin-sidebar", "(max-width: 760px)",
            new CssRule
            {
                MinHeight = raw("0"),
                Padding = raw("10px 12px")
            });

        Media(".jazor-admin-sidebar__logo", "(max-width: 760px)",
            new CssRule
            {
                MinHeight = raw("34px"),
                Margin = raw("0 6px 8px"),
                FontSize = raw("16px")
            });

        Media(".jazor-admin-sidebar__list", "(max-width: 760px)",
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

        Media(".jazor-admin-sidebar__item", "(max-width: 760px)",
            new CssRule
            {
                Flex = raw("0 0 auto"),
                Margin = raw("0")
            });

        Media(".jazor-admin-sidebar__link,   .jazor-admin-sidebar__button", "(max-width: 760px)",
            new CssRule
            {
                MinHeight = raw("36px"),
                Width = raw("auto"),
                WhiteSpace = raw("nowrap")
            });

        Media(".jazor-admin-sidebar__children", "(max-width: 760px)",
            new CssRule
            {
                Display = raw("flex"),
                Padding = raw("4px 0 0 8px"),
                Gap = raw("4px")
            });

        Media(".jazor-admin-sidebar__children .jazor-admin-sidebar__item", "(max-width: 760px)",
            new CssRule
            {
                Flex = raw("0 0 auto")
            });

        Media(".jazor-admin-sidebar__children .jazor-admin-sidebar__link,   .jazor-admin-sidebar__children .jazor-admin-sidebar__button", "(max-width: 760px)",
            new CssRule
            {
                MinHeight = raw("32px"),
                Padding = raw("6px 10px")
            });

        Media(".jazor-admin-shell__header", "(max-width: 760px)",
            new CssRule
            {
                Position = raw("static")
            });

        Media(".jazor-admin-header", "(max-width: 760px)",
            new CssRule
            {
                Display = raw("block"),
                MinHeight = raw("0"),
                Padding = raw("12px 16px")
            });

        Media(".jazor-admin-shell__sidebar-toggle", "(max-width: 760px)",
            new CssRule
            {
                AlignSelf = raw("flex-start"),
                Margin = raw("12px 0 0 12px")
            });

        Media(".jazor-admin-header__subtitle", "(max-width: 760px)",
            new CssRule
            {
                Display = raw("none")
            });

        Media(".jazor-admin-header__actions", "(max-width: 760px)",
            new CssRule
            {
                AlignItems = raw("stretch"),
                MarginTop = raw("10px")
            });

        Media(".jazor-admin-tdesign-header", "(max-width: 760px)",
            new CssRule
            {
                Display = raw("block"),
                MinHeight = raw("0"),
                Padding = raw("12px 16px")
            });

        Media(".jazor-admin-tdesign-header__subtitle", "(max-width: 760px)",
            new CssRule
            {
                Display = raw("none")
            });

        Media(".jazor-admin-tdesign-header__actions", "(max-width: 760px)",
            new CssRule
            {
                AlignItems = raw("stretch"),
                MarginTop = raw("10px")
            });

        Media(".jazor-admin-tdesign-header__navigation", "(max-width: 760px)",
            new CssRule
            {
                MarginTop = raw("10px")
            });

        Media(".jazor-admin-header__navigation", "(max-width: 760px)",
            new CssRule
            {
                MarginTop = raw("10px")
            });

        Media(".jazor-admin__preferences,   .jazor-admin__user-region", "(max-width: 760px)",
            new CssRule
            {
                JustifyContent = raw("flex-start"),
                OverflowX = raw("auto")
            });

        Media(".jazor-admin-page", "(max-width: 760px)",
            new CssRule
            {
                Padding = raw("18px 14px 28px")
            });

        Media(".jazor-admin-page__header", "(max-width: 760px)",
            new CssRule
            {
                Display = raw("block"),
                MarginBottom = raw("16px")
            });

        Media(".jazor-admin-page__title", "(max-width: 760px)",
            new CssRule
            {
                FontSize = raw("23px")
            });

        Media(".jazor-admin-page__actions", "(max-width: 760px)",
            new CssRule
            {
                JustifyContent = raw("flex-start"),
                MarginTop = raw("14px")
            });

        Media(".jazor-admin-tdesign-page-container", "(max-width: 760px)",
            new CssRule
            {
                Width = raw("calc(100% - 28px)"),
                Margin = raw("14px")
            });

        Media(".jazor-admin-tdesign-page-container__header", "(max-width: 760px)",
            new CssRule
            {
                Display = raw("block"),
                MarginBottom = raw("16px")
            });

        Media(".jazor-admin-tdesign-page-container__title", "(max-width: 760px)",
            new CssRule
            {
                FontSize = raw("23px")
            });

        Media(".jazor-admin-access,   .jazor-admin-error", "(max-width: 760px)",
            new CssRule
            {
                Padding = raw("20px")
            });

        Media(".jazor-admin-access__panel", "(max-width: 760px)",
            new CssRule
            {
                Padding = raw("24px 20px")
            });

        Media(".jazor-admin-error__code", "(max-width: 760px)",
            new CssRule
            {
                FontSize = raw("58px")
            });

        Media(".jazor-admin__preference-toggle", "(max-width: 430px)",
            new CssRule
            {
                Display = raw("none")
            });

        Media(".jazor-admin__user", "(max-width: 430px)",
            new CssRule
            {
                MaxWidth = raw("120px")
            });

        global(".jazor-admin-tdesign-page-container--dashboard",
            new CssRule
            {
                Width = raw("min(calc(100% - 36px), 1480px)"),
                Margin = raw("18px auto 28px"),
                Background = raw("transparent !important"),
                Border = raw("0 !important"),
                Additional = [new("box-shadow", none, Important: true)]
            });

        global(".jazor-admin-tdesign-page-container--dashboard .jazor-admin-tdesign-page-container__header",
            new CssRule
            {
                AlignItems = raw("center"),
                MarginBottom = raw("14px")
            });

        global(".jazor-admin-tdesign-page-container--dashboard .jazor-admin-tdesign-page-container__title",
            new CssRule
            {
                MarginTop = raw("5px"),
                FontSize = raw("21px")
            });

        global(".jazor-admin-tdesign-page-container--dashboard .jazor-admin-tdesign-page-container__subtitle",
            new CssRule
            {
                MarginTop = raw("4px"),
                FontSize = raw("13px")
            });

        // The production administration pages share a compact TDesign Starter-like work surface.
        // 生产管理页共用紧凑的 TDesign Starter 风格工作台，状态层次由数据与选择关系表达。
        global(".jazor-admin-session-state",
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

        global(".jazor-admin-session-state__spinner",
            new CssRule
            {
                Width = raw("24px"),
                Height = raw("24px"),
                Border = raw("2px solid var(--border-strong)"),
                BorderTopColor = raw("var(--accent)"),
                BorderRadius = raw("50%"),
                Animation = raw(sessionSpin + " 720ms linear infinite")
            });

        global(".jazor-admin__header-context",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center"),
                MinWidth = raw("0"),
                Gap = raw("8px")
            });

        global(".jazor-admin__organization-picker",
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

        global(".jazor-admin__organization-picker select",
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

        global(".jazor-admin__user-region",
            new CssRule
            {
                MinWidth = raw("0"),
                PaddingLeft = raw("8px"),
                BorderLeft = raw("1px solid var(--border)")
            });

        global(".jazor-admin__access-command",
            new CssRule
            {
                MinHeight = raw("30px"),
                Padding = raw("4px 8px"),
                FontSize = raw("12px"),
                Background = raw("transparent"),
                Border = raw("0")
            });

        global(".jazor-admin__access-command[data-access-command=\"sign-out\"]",
            new CssRule
            {
                Color = raw("var(--danger)")
            });

        global(".jazor-admin-overview",
            new CssRule
            {
                Display = raw("grid"),
                MinWidth = raw("0"),
                Gap = raw("16px")
            });

        global(".jazor-admin-overview__metrics",
            new CssRule
            {
                Display = raw("grid"),
                GridTemplateColumns = raw("repeat(4, minmax(0, 1fr))"),
                Gap = raw("16px")
            });

        global(".jazor-admin-overview__metric",
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

        global(".jazor-admin-overview__metric:nth-child(2)",
            new CssRule
            {
                AnimationDelay = raw("35ms")
            });

        global(".jazor-admin-overview__metric:nth-child(3)",
            new CssRule
            {
                AnimationDelay = raw("70ms")
            });

        global(".jazor-admin-overview__metric:nth-child(4)",
            new CssRule
            {
                AnimationDelay = raw("105ms")
            });

        global(".jazor-admin-overview__metric span, .jazor-admin-overview__metric small",
            new CssRule
            {
                Display = raw("block"),
                Overflow = raw("hidden"),
                Color = raw("var(--text-muted)"),
                TextOverflow = raw("ellipsis"),
                WhiteSpace = raw("nowrap")
            });

        global(".jazor-admin-overview__metric span",
            new CssRule
            {
                FontSize = raw("13px"),
                FontWeight = raw("600")
            });

        global(".jazor-admin-overview__metric strong",
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

        global(".jazor-admin-overview__metric small",
            new CssRule
            {
                FontSize = raw("12px")
            });

        global(".jazor-admin-overview__grid",
            new CssRule
            {
                Display = raw("grid"),
                GridTemplateColumns = raw("repeat(2, minmax(0, 1fr))"),
                MinWidth = raw("0"),
                Gap = raw("16px")
            });

        global(".jazor-admin-overview__panel, .jazor-admin-management__panel",
            new CssRule
            {
                MinWidth = raw("0"),
                Background = raw("var(--surface)"),
                Border = raw("1px solid var(--border)"),
                BorderRadius = raw("6px"),
                BoxShadow = var("--shadow")
            });

        global(".jazor-admin-overview__panel-header, .jazor-admin-management__panel-header",
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

        global(".jazor-admin-overview__panel-header h2, .jazor-admin-overview__panel-header p, .jazor-admin-management__panel-header h2, .jazor-admin-management__panel-header p",
            new CssRule
            {
                Margin = raw("0")
            });

        global(".jazor-admin-overview__panel-header h2, .jazor-admin-management__panel-header h2",
            new CssRule
            {
                FontSize = raw("15px"),
                FontWeight = raw("650")
            });

        global(".jazor-admin-overview__panel-header p, .jazor-admin-management__panel-header p",
            new CssRule
            {
                MarginTop = raw("5px"),
                Color = raw("var(--text-muted)"),
                FontSize = raw("12px"),
                LineHeight = raw("1.4")
            });

        global(".jazor-admin-overview__organization-list, .jazor-admin-overview__role-list, .jazor-admin-management__item-list, .jazor-admin-management__role-list",
            new CssRule
            {
                Padding = raw("0"),
                Margin = raw("0"),
                ListStyle = raw("none")
            });

        global(".jazor-admin-overview__organization-list li, .jazor-admin-overview__role-list li, .jazor-admin-management__item-list li",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center"),
                MinHeight = raw("48px"),
                Padding = raw("10px 20px"),
                Gap = raw("10px"),
                BorderBottom = raw("1px solid var(--border)")
            });

        global(".jazor-admin-overview__organization-list li.is-current",
            new CssRule
            {
                Color = raw("var(--accent-strong)"),
                Background = raw("var(--accent-soft)")
            });

        global(".jazor-admin-overview__organization-code, .jazor-admin-management__code",
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

        global(".jazor-admin-overview__role-list li",
            new CssRule
            {
                Color = raw("var(--text-muted)"),
                FontSize = raw("13px")
            });

        global(".jazor-admin-overview__empty, .jazor-admin-management__empty, .jazor-admin-management__loading, .jazor-admin-management__error",
            new CssRule
            {
                Margin = raw("0"),
                Padding = raw("20px"),
                Color = raw("var(--text-muted)"),
                FontSize = raw("13px"),
                LineHeight = raw("1.5")
            });

        global(".jazor-admin-management__error",
            new CssRule
            {
                Color = raw("var(--danger)"),
                Background = raw("var(--danger-soft)"),
                Border = raw("1px solid color-mix(in srgb, var(--danger) 30%, var(--border))"),
                BorderRadius = raw("6px")
            });

        global(".jazor-admin-management",
            new CssRule
            {
                Display = raw("grid"),
                MinWidth = raw("0"),
                Gap = raw("16px")
            });

        global(".jazor-admin-management__split",
            new CssRule
            {
                Display = raw("grid"),
                GridTemplateColumns = raw("minmax(0, 7fr) minmax(300px, 4fr)"),
                AlignItems = raw("start"),
                MinWidth = raw("0"),
                Gap = raw("16px")
            });

        global(".jazor-admin-management__split--authorization",
            new CssRule
            {
                GridTemplateColumns = raw("minmax(260px, 4fr) minmax(0, 7fr)")
            });

        global(".jazor-admin-management__split--members",
            new CssRule
            {
                GridTemplateColumns = raw("minmax(0, 7fr) minmax(320px, 4fr)")
            });

        global(".jazor-admin-management__panel > h3, .jazor-admin-management__role-editor h3",
            new CssRule
            {
                Margin = raw("20px 20px 10px"),
                FontSize = raw("14px")
            });

        global(".jazor-admin-management__details",
            new CssRule
            {
                Display = raw("grid"),
                GridTemplateColumns = raw("repeat(3, minmax(0, 1fr))"),
                Padding = raw("20px"),
                Margin = raw("0"),
                Gap = raw("12px")
            });

        global(".jazor-admin-management__details div",
            new CssRule
            {
                MinWidth = raw("0"),
                Padding = raw("12px"),
                Background = raw("var(--surface-subtle)"),
                Border = raw("1px solid var(--border)"),
                BorderRadius = raw("4px")
            });

        global(".jazor-admin-management__details dt, .jazor-admin-management__details dd",
            new CssRule
            {
                Margin = raw("0")
            });

        global(".jazor-admin-management__details dt",
            new CssRule
            {
                Color = raw("var(--text-muted)"),
                FontSize = raw("12px")
            });

        global(".jazor-admin-management__details dd",
            new CssRule
            {
                MarginTop = raw("6px"),
                Overflow = raw("hidden"),
                FontSize = raw("13px"),
                FontWeight = raw("600"),
                TextOverflow = raw("ellipsis"),
                WhiteSpace = raw("nowrap")
            });

        global(".jazor-admin-management__form",
            new CssRule
            {
                Display = raw("grid"),
                Padding = raw("20px"),
                Gap = raw("14px")
            });

        global(".jazor-admin-management__form--inline",
            new CssRule
            {
                GridTemplateColumns = raw("minmax(0, 1fr) minmax(0, 1fr) auto"),
                AlignItems = raw("end")
            });

        global(".jazor-admin-management__form label",
            new CssRule
            {
                Display = raw("grid"),
                Gap = raw("6px"),
                Color = raw("var(--text-muted)"),
                FontSize = raw("12px"),
                FontWeight = raw("600")
            });

        global(".jazor-admin-management input, .jazor-admin-management select, .jazor-admin-management textarea",
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

        global(".jazor-admin-management textarea",
            new CssRule
            {
                MinHeight = raw("76px"),
                Resize = raw("vertical"),
                LineHeight = raw("1.5")
            });

        global(".jazor-admin-management__field-grid",
            new CssRule
            {
                Display = raw("grid"),
                GridTemplateColumns = raw("repeat(2, minmax(0, 1fr))"),
                Gap = raw("14px")
            });

        global(".jazor-admin-management__options",
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

        global(".jazor-admin-management__options legend",
            new CssRule
            {
                Padding = raw("0 5px"),
                Color = raw("var(--text-muted)"),
                FontSize = raw("12px"),
                FontWeight = raw("650")
            });

        global(".jazor-admin-management__options label",
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

        global(".jazor-admin-management__options input",
            new CssRule
            {
                Flex = raw("0 0 16px"),
                Width = raw("16px"),
                MinHeight = raw("16px"),
                Padding = raw("0")
            });

        global(".jazor-admin-management__profiles",
            new CssRule
            {
                Display = raw("grid"),
                GridTemplateColumns = raw("repeat(3, minmax(0, 1fr))"),
                Margin = raw("20px 20px 0"),
                Border = raw("1px solid var(--border-strong)"),
                BorderRadius = raw("4px"),
                Overflow = raw("hidden")
            });

        global(".jazor-admin-management__profiles button",
            new CssRule
            {
                Color = raw("var(--text-muted)"),
                Background = raw("var(--surface)"),
                Border = raw("0"),
                BorderRight = raw("1px solid var(--border-strong)"),
                BorderRadius = raw("0")
            });

        global(".jazor-admin-management__profiles button:last-child",
            new CssRule
            {
                BorderRight = raw("0")
            });

        global(".jazor-admin-management__profiles button:hover, .jazor-admin-management__profiles button.is-selected",
            new CssRule
            {
                Color = raw("var(--accent-strong)"),
                Background = raw("var(--accent-soft)")
            });

        global(".jazor-admin-management__commands",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center"),
                FlexWrap = raw("wrap"),
                Gap = raw("8px")
            });

        global(".jazor-admin-management__secondary-button",
            new CssRule
            {
                Color = raw("var(--text) !important"),
                Background = raw("var(--surface) !important"),
                BorderColor = raw("var(--border-strong) !important")
            });

        global(".jazor-admin-management__danger-button",
            new CssRule
            {
                Color = raw("var(--danger) !important"),
                Background = raw("var(--danger-soft) !important"),
                BorderColor = raw("color-mix(in srgb, var(--danger) 45%, var(--border)) !important")
            });

        global(".jazor-admin-management__secret",
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

        global(".jazor-admin-management__secret code",
            new CssRule
            {
                OverflowWrap = raw("anywhere"),
                FontSize = raw("13px")
            });

        global(".jazor-admin-management__secret span",
            new CssRule
            {
                Color = raw("var(--text-muted)"),
                FontSize = raw("12px")
            });

        global(".jazor-admin-management button",
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

        global(".jazor-admin-management button:hover",
            new CssRule
            {
                Background = raw("var(--accent-strong)")
            });

        global(".jazor-admin-management__table-wrap",
            new CssRule
            {
                Width = raw("100%"),
                OverflowX = raw("auto")
            });

        global(".jazor-admin-management__table",
            new CssRule
            {
                Width = raw("100%"),
                MinWidth = raw("560px"),
                BorderCollapse = raw("collapse")
            });

        global(".jazor-admin-management__table th, .jazor-admin-management__table td",
            new CssRule
            {
                Padding = raw("13px 20px"),
                TextAlign = raw("left"),
                VerticalAlign = raw("middle"),
                BorderBottom = raw("1px solid var(--border)")
            });

        global(".jazor-admin-management__table th",
            new CssRule
            {
                Color = raw("var(--text-muted)"),
                FontSize = raw("12px"),
                FontWeight = raw("600"),
                Background = raw("var(--surface-subtle)")
            });

        global(".jazor-admin-management__table td",
            new CssRule
            {
                FontSize = raw("13px")
            });

        global(".jazor-admin-management__table td strong, .jazor-admin-management__table td span",
            new CssRule
            {
                Display = raw("block")
            });

        global(".jazor-admin-management__table td span",
            new CssRule
            {
                MarginTop = raw("3px"),
                Color = raw("var(--text-muted)"),
                FontSize = raw("12px")
            });

        global(".jazor-admin-management__table tr.is-selected td, .jazor-admin-management__table tbody tr:hover td",
            new CssRule
            {
                Background = raw("var(--accent-soft)")
            });

        global(".jazor-admin-management__text-button",
            new CssRule
            {
                Padding = raw("4px 0 !important"),
                Color = raw("var(--accent) !important"),
                Background = raw("transparent !important"),
                Border = raw("0 !important")
            });

        global(".jazor-admin-management__role-list",
            new CssRule
            {
                Padding = raw("8px 0")
            });

        global(".jazor-admin-management__role-list button",
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

        global(".jazor-admin-management__role-list button:hover, .jazor-admin-management__role-list li.is-selected button",
            new CssRule
            {
                Color = raw("var(--accent-strong)"),
                Background = raw("var(--accent-soft)"),
                BorderLeftColor = raw("var(--accent)")
            });

        global(".jazor-admin-management__role-list small, .jazor-admin-management__check small",
            new CssRule
            {
                Color = raw("var(--text-muted)"),
                FontSize = raw("11px")
            });

        global(".jazor-admin-management__grant-list, .jazor-admin-management__role-editor",
            new CssRule
            {
                Display = raw("grid"),
                Padding = raw("12px 20px 20px"),
                Gap = raw("8px")
            });

        global(".jazor-admin-management__check",
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

        global(".jazor-admin-management__check input",
            new CssRule
            {
                Width = raw("16px"),
                MinHeight = raw("16px"),
                Padding = raw("0")
            });

        global(".jazor-admin-management__grant-list + button, .jazor-admin-management__role-editor > button",
            new CssRule
            {
                Margin = raw("0 20px 20px")
            });

        Media(".jazor-admin-overview__metrics", "(max-width: 1100px)",
            new CssRule
            {
                GridTemplateColumns = raw("repeat(2, minmax(0, 1fr))")
            });

        Media(".jazor-admin-management__split, .jazor-admin-management__split--authorization, .jazor-admin-management__split--members", "(max-width: 980px)",
            new CssRule
            {
                GridTemplateColumns = raw("1fr")
            });

        Media(".jazor-admin__header-context", "(max-width: 1080px)",
            new CssRule
            {
                FlexWrap = raw("wrap"),
                JustifyContent = raw("flex-end")
            });

        Media(".jazor-admin-overview__grid", "(max-width: 760px)",
            new CssRule
            {
                GridTemplateColumns = raw("1fr")
            });

        Media(".jazor-admin-management__details", "(max-width: 620px)",
            new CssRule
            {
                GridTemplateColumns = raw("1fr")
            });

        Media(".jazor-admin-overview__metrics", "(max-width: 620px)",
            new CssRule
            {
                GridTemplateColumns = raw("1fr")
            });

        Media(".jazor-admin-management__form--inline", "(max-width: 620px)",
            new CssRule
            {
                GridTemplateColumns = raw("1fr")
            });

        Media(".jazor-admin-management__field-grid, .jazor-admin-management__options", "(max-width: 620px)",
            new CssRule
            {
                GridTemplateColumns = raw("1fr")
            });

        Media(".jazor-admin-management__commands button", "(max-width: 620px)",
            new CssRule
            {
                Flex = raw("1 1 100%"),
                Width = raw("100%")
            });

        Media(".jazor-admin__organization-picker", "(max-width: 620px)",
            new CssRule
            {
                GridTemplateColumns = raw("1fr"),
                PaddingRight = raw("0"),
                BorderRight = raw("0")
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
