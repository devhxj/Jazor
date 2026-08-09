using ECMAScript;
using ECMAScript.Style;
using static ECMAScript.Style.css;

namespace JazorAdmin;

/// <summary>
/// Registers JazorAdmin's application and page rules through ECMAScript.Style.
/// 通过 ECMAScript.Style 注册 JazorAdmin 的应用与页面样式，保持现有选择器契约。
/// </summary>
[ECMAScriptModule("./components/styles")]
internal static class Styles
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

        var quickPulse = keyframes(
        [
            new("0%, 100%", new CssDeclarations
            {
                BoxShadow = raw("0 0 0 0 rgba(47, 111, 237, 0.18)")
            }),
            new("50%", new CssDeclarations
            {
                BoxShadow = raw("0 0 0 8px rgba(47, 111, 237, 0)")
            })
        ]);

        global(":root",
            new CssRule
            {
                ColorScheme = raw("light"),
                FontFamily = raw("Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, \"Segoe UI\", sans-serif"),
                FontSynthesis = raw("none"),
                TextRendering = raw("optimizeLegibility"),
                // Match Starter's dynamic brand token entry point. The setting drawer
                // changes --ja-brand-color and every TDesign control follows it.
                ["--td-brand-color"] = raw("var(--ja-brand-color, #0052D9)"),
                ["--td-brand-color-hover"] = raw("color-mix(in srgb, var(--ja-brand-color, #0052D9) 86%, #ffffff)"),
                ["--td-brand-color-active"] = raw("color-mix(in srgb, var(--ja-brand-color, #0052D9) 86%, #000000)"),
                ["--td-brand-color-light"] = raw("color-mix(in srgb, var(--ja-brand-color, #0052D9) 10%, #ffffff)"),
                ["--td-brand-color-light-hover"] = raw("color-mix(in srgb, var(--ja-brand-color, #0052D9) 16%, #ffffff)")
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
                Background = raw("#f3f3f3")
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
                ["--app-bg"] = raw("#f3f3f3"),
                ["--surface"] = raw("#ffffff"),
                ["--surface-subtle"] = raw("#f3f3f3"),
                ["--surface-strong"] = raw("#f3f3f3"),
                ["--text"] = raw("#1d2129"),
                ["--text-muted"] = raw("#86909c"),
                ["--border"] = raw("#e7e7e7"),
                ["--border-strong"] = raw("#dcdcdc"),
                ["--accent"] = raw("var(--td-brand-color)"),
                ["--accent-strong"] = raw("var(--td-brand-color-active)"),
                ["--accent-soft"] = raw("var(--td-brand-color-light)"),
                ["--danger"] = raw("#e34d59"),
                ["--danger-soft"] = raw("#fcecec"),
                ["--success"] = raw("#00a870"),
                ["--success-soft"] = raw("#e8f8f2"),
                ["--warning"] = raw("#ed7b2f"),
                ["--warning-soft"] = raw("#fff1e8"),
                ["--info"] = raw("var(--td-brand-color)"),
                ["--info-soft"] = raw("var(--td-brand-color-light)"),
                ["--background"] = var("--td-bg-color-page"),
                ["--shadow"] = shadows(new CssShadow(px(0), px(4), Blur: px(14), Color: rgba(31, 52, 78, 0.05))),
                MinHeight = raw("100vh"),
                Background = raw("var(--app-bg)"),
                Color = raw("var(--text)")
            });

        global(".ja-application--dark",
            new CssRule
            {
                ColorScheme = raw("dark"),
                ["--app-bg"] = raw("#181818"),
                ["--surface"] = raw("#242424"),
                ["--surface-subtle"] = raw("#2c2c2c"),
                ["--surface-strong"] = raw("#393939"),
                ["--text"] = raw("rgba(255, 255, 255, 0.9)"),
                ["--text-muted"] = raw("rgba(255, 255, 255, 0.55)"),
                ["--border"] = raw("#393939"),
                ["--border-strong"] = raw("#5e5e5e"),
                ["--accent"] = raw("var(--td-brand-color)"),
                ["--accent-strong"] = raw("var(--td-brand-color-hover)"),
                ["--accent-soft"] = raw("color-mix(in srgb, var(--td-brand-color) 24%, #181818)"),
                ["--danger"] = raw("#e34d59"),
                ["--danger-soft"] = raw("#492827"),
                ["--success"] = raw("#00a870"),
                ["--success-soft"] = raw("#1d4935"),
                ["--warning"] = raw("#ed7b2f"),
                ["--warning-soft"] = raw("#45391f"),
                ["--info"] = raw("var(--td-brand-color)"),
                ["--info-soft"] = raw("color-mix(in srgb, var(--td-brand-color) 24%, #181818)"),
                ["--background"] = var("--td-bg-color-page"),
                ["--td-bg-color-page"] = hex("181818"),
                ["--td-bg-color-container"] = hex("242424"),
                ["--td-bg-color-container-hover"] = hex("2c2c2c"),
                ["--td-bg-color-container-active"] = hex("4b4b4b"),
                ["--td-bg-color-secondarycontainer"] = hex("2c2c2c"),
                ["--td-bg-color-secondarycontainer-hover"] = hex("393939"),
                ["--td-bg-color-secondarycontainer-active"] = hex("5e5e5e"),
                ["--td-bg-color-component"] = hex("393939"),
                ["--td-bg-color-component-hover"] = hex("4b4b4b"),
                ["--td-bg-color-component-active"] = hex("5e5e5e"),
                ["--td-text-color-primary"] = rgba(255, 255, 255, 0.9),
                ["--td-text-color-secondary"] = rgba(255, 255, 255, 0.55),
                ["--td-text-color-placeholder"] = rgba(255, 255, 255, 0.35),
                ["--td-text-color-disabled"] = rgba(255, 255, 255, 0.22),
                ["--td-component-stroke"] = hex("393939"),
                ["--td-border-level-1-color"] = hex("393939"),
                ["--td-border-level-2-color"] = hex("5e5e5e"),
                ["--shadow"] = shadows(
                    new CssShadow(px(0), px(1), Blur: px(2), Color: rgba(0, 0, 0, 0.28)),
                    new CssShadow(px(0), px(10), Blur: px(28), Color: rgba(0, 0, 0, 0.2)))
            });

        Media(".ja-application--system", "(prefers-color-scheme: dark)",
            new CssRule
            {
                ColorScheme = raw("dark"),
                ["--app-bg"] = raw("#181818"),
                ["--surface"] = raw("#242424"),
                ["--surface-subtle"] = raw("#2c2c2c"),
                ["--surface-strong"] = raw("#393939"),
                ["--text"] = raw("rgba(255, 255, 255, 0.9)"),
                ["--text-muted"] = raw("rgba(255, 255, 255, 0.55)"),
                ["--border"] = raw("#393939"),
                ["--border-strong"] = raw("#5e5e5e"),
                ["--accent"] = raw("var(--td-brand-color)"),
                ["--accent-strong"] = raw("var(--td-brand-color-hover)"),
                ["--accent-soft"] = raw("color-mix(in srgb, var(--td-brand-color) 24%, #181818)"),
                ["--danger"] = raw("#e34d59"),
                ["--danger-soft"] = raw("#492827"),
                ["--success"] = raw("#00a870"),
                ["--success-soft"] = raw("#1d4935"),
                ["--warning"] = raw("#ed7b2f"),
                ["--warning-soft"] = raw("#45391f"),
                ["--info"] = raw("var(--td-brand-color)"),
                ["--info-soft"] = raw("color-mix(in srgb, var(--td-brand-color) 24%, #181818)"),
                ["--td-bg-color-page"] = hex("181818"),
                ["--td-bg-color-container"] = hex("242424"),
                ["--td-bg-color-container-hover"] = hex("2c2c2c"),
                ["--td-bg-color-container-active"] = hex("4b4b4b"),
                ["--td-bg-color-secondarycontainer"] = hex("2c2c2c"),
                ["--td-bg-color-secondarycontainer-hover"] = hex("393939"),
                ["--td-bg-color-secondarycontainer-active"] = hex("5e5e5e"),
                ["--td-bg-color-component"] = hex("393939"),
                ["--td-bg-color-component-hover"] = hex("4b4b4b"),
                ["--td-bg-color-component-active"] = hex("5e5e5e"),
                ["--td-text-color-primary"] = rgba(255, 255, 255, 0.9),
                ["--td-text-color-secondary"] = rgba(255, 255, 255, 0.55),
                ["--td-text-color-placeholder"] = rgba(255, 255, 255, 0.35),
                ["--td-text-color-disabled"] = rgba(255, 255, 255, 0.22),
                ["--td-component-stroke"] = hex("393939"),
                ["--td-border-level-1-color"] = hex("393939"),
                ["--td-border-level-2-color"] = hex("5e5e5e"),
                ["--shadow"] = shadows(
                    new CssShadow(px(0), px(1), Blur: px(2), Color: rgba(0, 0, 0, 0.28)),
                    new CssShadow(px(0), px(10), Blur: px(28), Color: rgba(0, 0, 0, 0.2)))
            });

        global(".ja-application--grayscale",
            new CssRule
            {
                Filter = grayscale(1)
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
                Position = sticky,
                Top = px(0),
                ZIndex = raw("20"),
                Height = vh(100),
                Overflow = raw("auto"),
                Background = raw("#17241f"),
                Color = raw("#eef7f3"),
                BorderRight = px(1) | solid | hex("293a33")
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
                BorderBottom = px(1) | solid | var("--border"),
                BackdropFilter = raw("blur(12px)")
            });

        global(".ja-shell__sidebar-toggle",
            new CssRule
            {
                Position = raw("relative"),
                Flex = raw("0 0 36px"),
                Width = raw("36px"),
                Height = raw("36px"),
                Padding = padding(px(0)),
                MarginLeft = raw("14px"),
                Color = raw("var(--text)"),
                Background = raw("transparent"),
                Border = px(1) | solid | var("--border"),
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
                MinWidth = px(0),
                // The secondary menu owns vertical scrolling. The Aside itself must never
                // create a second scrollbar or expose the menu's box-model overflow.
                Overflow = hidden,
                BorderRight = px(1) | solid | var("--border")
            });

        global(".ja-tdesign-layout--mixed[data-shell-collapsed=\"false\"] > [data-shell-region=\"sidebar\"]",
            new CssRule
            {
                Width = raw("296px !important"),
                Flex = raw("0 0 296px !important")
            });

        global(".ja-tdesign-layout--sidebar[data-shell-collapsed=\"false\"] > [data-shell-region=\"sidebar\"]",
            new CssRule
            {
                Width = raw("232px !important"),
                Flex = raw("0 0 232px !important")
            });

        global(".ja-tdesign-layout[data-shell-collapsed=\"true\"] > [data-shell-region=\"sidebar\"]",
            new CssRule
            {
                Width = raw("64px !important"),
                Flex = raw("0 0 64px !important")
            });

        global(".ja-tdesign-layout[data-shell-sidebar-fixed=\"false\"] > [data-shell-region=\"sidebar\"]",
            new CssRule
            {
                Position = relative,
                Height = raw("auto"),
                MinHeight = raw("100vh")
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
                BorderBottom = px(1) | solid | var("--border")
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
                MinWidth = raw("0"),
                // Contain intentionally wide data tables inside their own scroll region on small screens.
                // 小屏幕下宽表格必须由表格容器滚动，不能把宿主页面整体撑出横向滚动条。
                OverflowX = hidden
            });

        global(".ja-tdesign-layout__tabs",
            new CssRule
            {
                Position = sticky,
                Top = px(64),
                ZIndex = 14,
                Background = var("--td-bg-color-container"),
                BorderBottom = px(1) | solid | var("--td-component-stroke")
            });

        global(".ja-tdesign-layout[data-shell-show-header=\"false\"] .ja-tdesign-layout__tabs",
            new CssRule { Top = px(0) });

        global(".ja-route-tabs",
            new CssRule
            {
                Width = percent(100),
                Background = var("--td-bg-color-container")
            });

        global(".ja-route-tabs .t-tabs__nav-container",
            new CssRule
            {
                PaddingLeft = px(0),
                PaddingRight = px(0)
            });

        global(".ja-route-tabs .t-tabs__nav--card.t-tabs__nav-item",
            new CssRule
            {
                PaddingLeft = px(12),
                PaddingRight = px(12)
            });

        // TDesign 1.20 maps t-size-m tabs to 48px. Starter keeps the same
        // size class but uses a denser 40px route-tab rail.
        global(".ja-route-tabs .t-tabs__nav-item.t-size-m, .ja-route-tabs .t-tabs__btn.t-size-m, .ja-route-tabs .t-tabs__scroll-btn.t-size-m",
            new CssRule
            {
                Height = px(40),
                LineHeight = px(40)
            });

        global(".ja-route-tabs__label, .ja-route-tabs__home",
            new CssRule
            {
                Display = inlineFlex,
                AlignItems = center,
                MinWidth = px(0),
                Gap = px(6)
            });

        global(".ja-route-tabs__label",
            new CssRule
            {
                MaxWidth = px(168),
                Overflow = hidden,
                TextOverflow = ellipsis,
                WhiteSpace = nowrap
            });

        global(".ja-tdesign-layout__content",
            new CssRule
            {
                Width = percent(100),
                MaxWidth = raw("none"),
                MinWidth = px(0),
                Margin = margin(px(8), px(12), px(16))
            });

        global(".ja-route-breadcrumb",
            new CssRule
            {
                // TDesign breadcrumb items are flex children. Block layout makes every
                // crumb full width and turns a route hierarchy into multiple lines.
                Display = flex,
                AlignItems = center,
                FlexWrap = noWrap,
                MarginBottom = px(16)
            });

        global(".ja-tdesign-sidebar-shell",
            new CssRule
            {
                Display = flex,
                Width = percent(100),
                Height = percent(100),
                MinWidth = px(0),
                MinHeight = px(0),
                Overflow = hidden,
                Background = raw("var(--surface)")
            });

        global(".ja-iconbar",
            new CssRule
            {
                BorderRight = px(1) | solid | var("--border")
            });

        global(".ja-iconbar .t-menu__logo",
            new CssRule
            {
                Position = relative,
                Display = flex,
                AlignItems = center,
                JustifyContent = center,
                Flex = flexBox(0, 0, px(64)),
                Width = raw("64px !important"),
                MinWidth = raw("64px !important"),
                MaxWidth = raw("64px !important"),
                Height = px(64),
                Margin = margin(px(0)),
                Padding = raw("0 !important"),
                BoxSizing = borderBox
            });

        global(".ja-iconbar__brand",
            new CssRule
            {
                Position = absolute,
                // The slot remains 64px wide in every rail state. Center against that fixed
                // geometry instead of the logo's intrinsic SVG box.
                Left = percent(50),
                Top = px(13),
                Display = inlineFlex,
                AlignItems = center,
                JustifyContent = center,
                Width = px(38),
                Height = px(38),
                // TDesign styles the logo anchor after this generated stylesheet.
                // This must win so its visual center stays on the 64px IconBar rail.
                MarginLeft = raw("0 !important"),
                BorderRadius = radius(px(19)),
                Transform = raw("translateX(-50%)")
            });

        global(".ja-iconbar__brand img",
            new CssRule
            {
                Display = block,
                Width = px(30),
                Height = px(30)
            });

        global(".ja-iconbar .t-menu__operations",
            new CssRule
            {
                Display = flex,
                AlignItems = center,
                JustifyContent = center,
                BorderTop = px(1) | solid | var("--border"),
                Padding = padding(px(12), px(0))
            });

        global(".ja-iconbar__operations",
            new CssRule
            {
                Display = flex,
                AlignItems = center,
                JustifyContent = center,
                Width = percent(100)
            });

        global(".ja-iconbar .ja-iconbar__quick-trigger",
            new CssRule
            {
                Position = relative,
                Width = px(18),
                Height = px(18),
                Border = px(2) | solid | var("--accent"),
                BorderRadius = radius(px(999)),
                Background = var("--surface"),
                Color = var("--accent"),
                BoxShadow = raw("0 0 0 8px rgba(47, 111, 237, 0.12), 0 0 0 18px rgba(47, 111, 237, 0.08)"),
                Animation = raw(quickPulse + " 2.4s ease-in-out infinite"),
                Overflow = visible
            });

        global(".ja-iconbar .ja-iconbar__quick-trigger .t-icon",
            new CssRule
            {
                Display = none
            });

        global(".ja-iconbar__quick-popup .t-popup__content",
            new CssRule
            {
                Padding = padding(px(0)),
                Border = raw("0"),
                BorderRadius = raw("0"),
                Background = raw("transparent"),
                BoxShadow = raw("none"),
                // RightBottom places the content after the trigger. Move its local polar
                // origin back onto the small breathing circle (18px, 114px).
                Transform = raw("translate(-36px, -11px)")
            });

        global(".ja-iconbar__quick-actions",
            new CssRule
            {
                Position = relative,
                Width = px(128),
                Height = px(112),
                Padding = padding(px(0)),
                Isolation = isolate
            });

        global(".ja-iconbar__quick-actions .ja-iconbar__quick-action",
            new CssRule
            {
                Position = absolute,
                Display = flex,
                AlignItems = center,
                JustifyContent = center,
                Width = px(36),
                Height = px(36)
            });

        global(".ja-iconbar__quick-actions .ja-iconbar__quick-action:nth-child(1)",
            new CssRule
            {
                Left = px(0),
                Top = px(0)
            });

        global(".ja-iconbar__quick-actions .ja-iconbar__quick-action:nth-child(2)",
            new CssRule
            {
                // 96px * sin/cos(22.5deg), measured from the trigger's center.
                // Keep these as the polar values rather than hand-tuned stair steps.
                Left = raw("36.738px"),
                Top = raw("7.307px")
            });

        global(".ja-iconbar__quick-actions .ja-iconbar__quick-action:nth-child(3)",
            new CssRule
            {
                // 96px * sin/cos(45deg).
                Left = raw("67.882px"),
                Top = raw("28.118px")
            });

        global(".ja-iconbar__quick-actions .ja-iconbar__quick-action:nth-child(4)",
            new CssRule
            {
                // 96px * sin/cos(67.5deg): four equal 22.5deg intervals on a 90deg arc.
                Left = raw("88.693px"),
                Top = raw("59.261px")
            });

        global(".ja-iconbar__quick-actions .ja-iconbar__quick-action",
            new CssRule
            {
                Width = px(36),
                Height = px(36),
                BorderRadius = radius(px(4)),
                Background = var("--surface"),
                Color = var("--text"),
                Border = px(1) | solid | var("--border"),
                BoxShadow = shadows(new CssShadow(px(0), px(8), Blur: px(18), Color: rgba(31, 52, 78, 0.1)))
            });

        global(".ja-iconbar__quick-actions .ja-iconbar__quick-action--danger",
            new CssRule
            {
                Background = var("--danger-soft"),
                Color = var("--danger")
            });

        // IconBar is intentionally a TDesign menu instance, not a parallel navigation widget.
        // TDesign owns item spacing, selected state, tooltip and icon treatment.
        global(".ja-iconbar--rail",
            new CssRule
            {
                Display = flex,
                Flex = flexBox(0, 0, px(64)),
                FlexDirection = column,
                Width = px(64),
                MinHeight = percent(100)
            });

        global(".ja-iconbar--head",
            new CssRule
            {
                Display = none
            });

        global(".ja-tdesign-sidebar-shell__menu",
            new CssRule
            {
                Display = flex,
                Flex = flexBox(0, 0, px(232)),
                FlexDirection = column,
                Width = px(232),
                MinWidth = px(0),
                MaxWidth = percent(100),
                Height = percent(100),
                MinHeight = px(0),
                Padding = padding(px(0)),
                BoxSizing = borderBox,
                // TDesign's .t-menu--scroll owns the menu's overflow. Keeping this wrapper
                // non-scrollable prevents a duplicate scrollbar beside the native one.
                Overflow = hidden,
                BorderLeft = px(1) | solid | var("--border")
            });

        global(".ja-tdesign-sidebar-shell__menu-title",
            new CssRule
            {
                Display = flex,
                AlignItems = center,
                Flex = flexBox(0, 0, px(64)),
                Height = px(64),
                MinWidth = px(0),
                Padding = padding(px(0), px(20)),
                Overflow = hidden,
                Color = var("--td-text-color-primary"),
                FontSize = px(16),
                FontWeight = raw("600"),
                TextOverflow = ellipsis,
                WhiteSpace = nowrap,
                BorderBottom = px(1) | solid | var("--border")
            });

        global(".ja-tdesign-sidebar-shell__menu-body",
            new CssRule
            {
                Flex = flexBox(1, 1, px(0)),
                MinHeight = px(0),
                Overflow = hidden
            });

        global(".ja-tdesign-sidebar-shell__mobile-menu",
            new CssRule
            {
                Display = none
            });

        global(".ja-tdesign-sidebar-shell__mobile-brand",
            new CssRule
            {
                Display = none
            });

        global(".ja-tdesign-sidebar-shell__menu [data-navigation-orientation=\"vertical\"]",
            new CssRule
            {
                Width = percent(100),
                Height = percent(100),
                MinWidth = px(0),
                MaxWidth = percent(100),
                OverflowX = hidden
            });

        global(".ja-sidebar",
            new CssRule
            {
                MinHeight = raw("100%"),
                Padding = padding(px(20), px(14))
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
                Padding = padding(px(0)),
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
                Padding = padding(px(8), px(12)),
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
                Padding = padding(px(4), px(0), px(4), px(12))
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
                Padding = padding(px(8), px(24)),
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
                Width = percent(100),
                Height = px(64),
                MinWidth = px(0),
                MinHeight = px(64),
                Padding = padding(px(0)),
                Overflow = hidden
            });

        global(".ja-tdesign-header .t-head-menu__inner",
            new CssRule
            {
                Display = flex,
                AlignItems = center,
                Height = px(64),
                MinHeight = px(64),
                MinWidth = px(0),
                PaddingLeft = raw("0 !important"),
                BorderBottom = px(1) | solid | var("--border"),
                Overflow = hidden
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
                MinWidth = raw("0"),
                Overflow = hidden
            });

        global(".ja-tdesign-header__navigation .t-menu__item",
            new CssRule { MinWidth = raw("unset") });

        global(".ja-tdesign-header__actions",
            new CssRule
            {
                JustifyContent = raw("flex-end"),
                MinWidth = raw("0"),
                Gap = raw("16px")
            });

        global(".ja-tdesign-header__logo, .ja-tdesign-header__operations",
            new CssRule
            {
                Display = flex,
                AlignItems = center,
                MinWidth = px(0),
                Gap = px(10)
            });

        global(".ja-tdesign-header__logo",
            new CssRule
            {
                Flex = raw("0 0 64px"),
                Width = px(64),
                MarginLeft = px(0),
                JustifyContent = center,
                Gap = px(0)
            });

        global(".ja-tdesign-header__operations",
            new CssRule
            {
                MarginLeft = auto
            });

        global(".ja-tdesign-header .t-menu__logo",
            new CssRule
            {
                // The header starts at the main-content edge. TDesign's default logo slot is
                // too wide for a navigation toggle, so reserve one 64px geometry rail.
                Flex = raw("0 0 64px !important"),
                Width = raw("64px !important"),
                MarginLeft = raw("0 !important"),
                Padding = raw("0 !important")
            });

        global(".ja-tdesign-layout--top .ja-tdesign-header__logo",
            new CssRule
            {
                Flex = raw("0 0 208px"),
                Width = px(208),
                JustifyContent = raw("flex-start")
            });

        global(".ja-tdesign-layout--top .ja-tdesign-header .t-menu__logo",
            new CssRule
            {
                Flex = raw("0 0 208px !important"),
                Width = raw("208px !important"),
                PaddingLeft = raw("24px !important")
            });

        global(".ja-tdesign-header [data-shell-command=\"toggle-sidebar\"]",
            new CssRule
            {
                Display = grid,
                Width = px(32),
                Height = px(32),
                MinWidth = px(32),
                Padding = raw("0 !important"),
                PlaceItems = center
            });

        global(".ja-tdesign-header .t-menu__operations",
            new CssRule
            {
                MarginRight = px(16)
            });

        global(".ja-sidebar--horizontal",
            new CssRule
            {
                MinHeight = raw("0"),
                Padding = padding(px(0)),
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
                Padding = padding(px(6), px(10)),
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
                Padding = padding(px(0), px(0), px(0), px(4))
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
                Padding = padding(px(7), px(10)),
                Color = raw("var(--text)"),
                Background = raw("var(--surface)"),
                Border = px(1) | solid | var("--border-strong"),
                BorderRadius = raw("5px")
            });

        global(".ja-preference select",
            new CssRule
            {
                MinHeight = raw("32px"),
                Padding = padding(px(4), px(24), px(4), px(8))
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
                Padding = padding(px(7), px(12)),
                Color = raw("var(--text)"),
                FontWeight = raw("650"),
                Background = raw("var(--surface)"),
                Border = px(1) | solid | var("--border-strong"),
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
                Padding = padding(px(16))
            });

        global(".ja-page__header",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("flex-end"),
                JustifyContent = raw("space-between"),
                MarginBottom = raw("16px"),
                Gap = raw("16px")
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
                MarginTop = raw("16px")
            });

        global(".ja-tdesign-page-container",
            new CssRule
            {
                Width = percent(100),
                MinWidth = px(0),
                Margin = margin(px(0))
            });

        global(".ja-tdesign-page-container__header",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("flex-end"),
                JustifyContent = raw("space-between"),
                MarginBottom = raw("16px"),
                Gap = raw("16px")
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
                MarginTop = raw("16px")
            });

        global(".ja-error",
            new CssRule
            {
                Display = raw("grid"),
                MinHeight = raw("100vh"),
                Padding = padding(px(32)),
                PlaceItems = raw("center")
            });

        global(".ja-access",
            new CssRule
            {
                Display = raw("grid"),
                Position = raw("relative"),
                Isolation = isolate,
                MinHeight = raw("100vh"),
                GridTemplateColumns = raw("minmax(0, 1fr) minmax(0, 1fr)"),
                AlignItems = stretch,
                JustifyItems = stretch,
                Padding = padding(px(0)),
                Overflow = hidden,
                Background = hex("edf3ed")
            });

        global(".ja-error",
            new CssRule
            {
                Background = raw("var(--app-bg)")
            });

        global(".ja-access__panel, .ja-error__content",
            new CssRule
            {
                Width = raw("min(100%, 448px)")
            });

        global(".ja-access__visual",
            new CssRule
            {
                Position = relative,
                Display = flex,
                FlexDirection = column,
                JustifyContent = spaceBetween,
                MinWidth = px(0),
                MinHeight = vh(100),
                Padding = padding(px(40), px(56), px(48)),
                Overflow = hidden,
                Color = hex("ffffff"),
                BackgroundColor = hex("0b3f42"),
                BackgroundImage = raw("linear-gradient(90deg, rgba(7, 47, 49, 0.76), rgba(7, 47, 49, 0.22) 54%, rgba(237, 243, 237, 0.62) 100%), url('/brand/login-art.webp')"),
                BackgroundPosition = raw("center right"),
                BackgroundSize = cover
            });

        global(".ja-access__visual::after",
            new CssRule
            {
                Position = absolute,
                Inset = raw("0"),
                Content = raw("\"\""),
                Background = raw("linear-gradient(180deg, rgba(255, 255, 255, 0.08), rgba(0, 0, 0, 0.14))"),
                PointerEvents = none
            });

        global(".ja-access__visual > *",
            new CssRule
            {
                Position = relative,
                ZIndex = raw("1")
            });

        global(".ja-access__visual-copy",
            new CssRule
            {
                MaxWidth = px(520)
            });

        global(".ja-access__visual-copy h2",
            new CssRule
            {
                Margin = margin(px(0), px(0), px(16)),
                Color = hex("ffffff"),
                FontSize = raw("40px"),
                FontWeight = raw("700"),
                LineHeight = raw("1.18")
            });

        global(".ja-access__visual-copy p",
            new CssRule
            {
                MaxWidth = px(440),
                Color = raw("rgba(255, 255, 255, 0.76)"),
                FontSize = px(16),
                LineHeight = raw("1.7")
            });

        global(".ja-access__panel",
            new CssRule
            {
                Position = relative,
                ZIndex = raw("1"),
                GridColumn = 2,
                AlignSelf = center,
                JustifySelf = center,
                Padding = padding(px(56), px(56)),
                Overflow = hidden,
                Color = hex("1d2129"),
                Background = raw("transparent"),
                Border = raw("0"),
                BorderRadius = px(0),
                BoxShadow = raw("none")
            });

        global(".ja-access__panel > .ja-access__brand",
            new CssRule
            {
                Display = none
            });

        global(".ja-access__brand",
            new CssRule
            {
                Display = raw("inline-flex"),
                AlignItems = raw("center"),
                Gap = raw("12px"),
                Color = raw("currentColor"),
                FontSize = raw("18px"),
                LineHeight = raw("1")
            });

        global(".ja-access__brand-mark",
            new CssRule
            {
                Display = raw("block"),
                Flex = raw("0 0 auto"),
                Width = raw("44px"),
                Height = raw("44px"),
                Filter = raw("drop-shadow(0 8px 18px rgba(4, 24, 33, 0.28))")
            });

        global(".ja-access__brand-copy",
            new CssRule
            {
                Display = raw("grid"),
                Gap = raw("5px")
            });

        global(".ja-access__brand-copy small",
            new CssRule
            {
                Color = raw("rgba(255, 255, 255, 0.68)"),
                FontSize = raw("11px"),
                FontWeight = raw("600"),
                TextTransform = raw("uppercase")
            });

        global(".ja-brand-logo",
            new CssRule
            {
                Display = raw("inline-flex"),
                AlignItems = raw("center"),
                Gap = raw("8px"),
                Color = raw("var(--text)"),
                FontSize = raw("16px"),
                FontWeight = raw("700"),
                LineHeight = raw("1")
            });

        global(".ja-brand-mark",
            new CssRule
            {
                Display = raw("block"),
                Flex = raw("0 0 auto"),
                Width = raw("24px"),
                Height = raw("24px")
            });

        global(".ja-access__intro",
            new CssRule
            {
                MarginTop = raw("0")
            });

        global(".ja-access h1",
            new CssRule
            {
                Margin = raw("0 0 10px"),
                Color = raw("#1d2129"),
                FontSize = raw("28px"),
                FontWeight = raw("700"),
                TextShadow = raw("none")
            });

        global(".ja-access p",
            new CssRule
            {
                Margin = raw("0"),
                Color = raw("#86909c"),
                LineHeight = raw("1.5")
            });

        global(".ja-access form",
            new CssRule
            {
                Display = raw("grid"),
                MarginTop = raw("32px"),
                Gap = raw("18px")
            });

        global(".ja-access label",
            new CssRule
            {
                Display = raw("grid"),
                Gap = raw("8px"),
                Color = raw("#4e5969"),
                FontSize = raw("12px"),
                FontWeight = raw("650"),
                TextShadow = raw("none")
            });

        global(".ja-access input",
            new CssRule
            {
                Width = raw("100%"),
                MinHeight = raw("40px"),
                Padding = padding(px(8), px(12)),
                Color = raw("#1d2129"),
                Background = raw("#ffffff"),
                Border = px(1) | solid | raw("#dcdcdc"),
                BorderRadius = raw("3px"),
                BoxShadow = raw("none")
            });

        global(".ja-access input:focus",
            new CssRule
            {
                Background = raw("#ffffff"),
                BorderColor = raw("#0052d9"),
                BoxShadow = raw("0 0 0 2px rgba(0, 82, 217, 0.12)")
            });

        global(".ja-access button",
            new CssRule
            {
                MinHeight = raw("40px"),
                Color = raw("#ffffff"),
                Background = raw("#0052d9"),
                Border = px(1) | solid | raw("#0052d9"),
                BorderRadius = raw("3px"),
                BoxShadow = raw("none")
            });

        global(".ja-access button:hover",
            new CssRule
            {
                Background = raw("#266fe8"),
                BorderColor = raw("#266fe8")
            });

        global(".ja-access__captcha-control",
            new CssRule
            {
                Display = raw("grid"),
                GridTemplateColumns = raw("minmax(0, 1fr) 112px 40px"),
                AlignItems = raw("center"),
                Gap = raw("8px")
            });

        global(".ja-access__captcha-control input",
            new CssRule
            {
                MinWidth = raw("0"),
                TextTransform = raw("uppercase"),
                LetterSpacing = raw("0.08em")
            });

        global(".ja-access__captcha-image",
            new CssRule
            {
                Display = raw("block"),
                Width = raw("112px"),
                Height = raw("40px"),
                Background = raw("#f3f3f3"),
                Border = px(1) | solid | raw("#dcdcdc"),
                BorderRadius = raw("3px")
            });

        global(".ja-access__captcha-refresh",
            new CssRule
            {
                Display = raw("grid"),
                Width = raw("40px"),
                Height = raw("40px"),
                MinHeight = raw("40px"),
                Padding = padding(px(0)),
                PlaceItems = raw("center"),
                Color = raw("#0052d9 !important"),
                FontSize = raw("18px"),
                LineHeight = raw("1"),
                Background = raw("#ffffff !important"),
                BorderColor = raw("#dcdcdc !important"),
                BorderRadius = raw("3px"),
                BoxShadow = raw("none")
            });

        global(".ja-access__captcha-refresh:hover",
            new CssRule
            {
                Background = raw("#f2f3ff !important"),
                BorderColor = raw("#0052d9 !important")
            });

        global(".ja-access__captcha-refresh:focus-visible",
            new CssRule
            {
                Outline = px(2) | solid | rgba(0, 82, 217, 0.28),
                OutlineOffset = raw("2px")
            });

        global(".ja-access__error",
            new CssRule
            {
                Padding = padding(px(10), px(12)),
                Color = raw("#d54941 !important"),
                FontSize = raw("13px"),
                Background = raw("#fff0ed"),
                Border = px(1) | solid | raw("#f9d7d2"),
                BorderRadius = raw("3px")
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
                Outline = px(3) | solid | raw("color-mix(in srgb, var(--accent) 35%, transparent)"),
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
                AlignItems = center
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
                BorderBottom = px(1) | solid | hex("293a33")
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
                BorderBottom = px(1) | solid | var("--border"),
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
                Position = staticPosition,
                Height = important(px(64)),
                MinHeight = px(64)
            });

        Media(".ja-tdesign-layout__header", "(max-width: 760px)",
            new CssRule
            {
                AlignItems = center,
                FlexWrap = noWrap,
                Height = px(64)
            });

        Media(".ja-tdesign-sidebar-shell", "(max-width: 760px)",
            new CssRule
            {
                FlexDirection = column,
                Width = percent(100),
                Height = auto,
                MinHeight = px(0),
                Overflow = visible
            });

        Media(".ja-iconbar--rail", "(max-width: 760px)",
            new CssRule
            {
                Display = none
            });

        Media(".ja-iconbar--head", "(max-width: 760px)",
            new CssRule
            {
                Display = none
            });

        Media(".ja-tdesign-sidebar-shell__menu", "(max-width: 760px)",
            new CssRule
            {
                Display = none
            });

        Media(".ja-tdesign-sidebar-shell__mobile-menu", "(max-width: 760px)",
            new CssRule
            {
                Display = flex,
                AlignItems = center,
                Width = percent(100),
                Height = px(64),
                MinWidth = px(0),
                BorderBottom = px(1) | solid | var("--border")
            });

        Media(".ja-tdesign-sidebar-shell__mobile-brand", "(max-width: 760px)",
            new CssRule
            {
                Display = inlineFlex,
                AlignItems = center,
                JustifyContent = center,
                Flex = flexBox(0, 0, px(64)),
                Width = px(64),
                Height = px(64)
            });

        Media(".ja-tdesign-sidebar-shell__mobile-brand img", "(max-width: 760px)",
            new CssRule
            {
                Display = block,
                Width = px(30),
                Height = px(30)
            });

        Media(".ja-tdesign-sidebar-shell__mobile-navigation", "(max-width: 760px)",
            new CssRule
            {
                Flex = flexBox(1, 1, px(0)),
                MinWidth = px(0),
                OverflowX = auto
            });

        Media(".ja-tdesign-header [data-shell-command=\"toggle-sidebar\"]", "(max-width: 760px)",
            new CssRule
            {
                Display = none
            });

        Media("[data-navigation-orientation=\"vertical\"]", "(max-width: 760px)",
            new CssRule
            {
                Display = important(flex),
                AlignItems = flexStart,
                Width = percent(100),
                OverflowX = hidden,
                OverscrollBehaviorInline = keyword("contain")
            });

        Media(".ja-sidebar", "(max-width: 760px)",
            new CssRule
            {
                MinHeight = raw("0"),
                Padding = padding(px(10), px(12))
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
                Padding = padding(px(4), px(0), px(0), px(8)),
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
                Padding = padding(px(6), px(10))
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
                Padding = padding(px(12), px(16))
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
                Width = percent(100),
                Height = px(64),
                MinHeight = px(64),
                Padding = padding(px(0))
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
                Padding = padding(px(18), px(14), px(28))
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

        Media(".ja-tdesign-layout__content", "(max-width: 760px)",
            new CssRule
            {
                Margin = margin(px(8), px(8), px(12))
            });

        Media(".ja-route-breadcrumb", "(max-width: 760px)",
            new CssRule
            {
                MarginBottom = px(16)
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
                Padding = padding(px(0))
            });

        Media(".ja-access", "(max-width: 760px)",
            new CssRule
            {
                GridTemplateColumns = raw("minmax(0, 1fr)"),
                MinHeight = raw("100svh")
            });

        Media(".ja-access__visual", "(max-width: 760px)",
            new CssRule
            {
                Display = none
            });

        Media(".ja-access__panel", "(max-width: 760px)",
            new CssRule
            {
                GridColumn = raw("1"),
                Width = raw("min(100%, 448px)"),
                Padding = padding(px(32), px(24)),
                BorderRadius = px(0)
            });

        Media(".ja-access__panel > .ja-access__brand", "(max-width: 760px)",
            new CssRule
            {
                Display = inlineFlex,
                MarginBottom = px(40),
                Color = raw("#1d2129")
            });

        Media(".ja-access__panel > .ja-access__brand .ja-access__brand-copy small", "(max-width: 760px)",
            new CssRule
            {
                Color = raw("#86909c")
            });

        Media(".ja-access__captcha-control", "(max-width: 430px)",
            new CssRule
            {
                GridTemplateColumns = raw("minmax(0, 1fr) 48px")
            });

        Media(".ja-access__captcha-image", "(max-width: 430px)",
            new CssRule
            {
                GridColumn = raw("1 / -1"),
                GridRow = raw("2")
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
                Border = px(2) | solid | var("--border-strong"),
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
                BorderRight = px(1) | solid | var("--border")
            });

        global(".ja-organization-picker select",
            new CssRule
            {
                MinWidth = raw("0"),
                MinHeight = raw("32px"),
                Padding = padding(px(4), px(26), px(4), px(8)),
                Overflow = raw("hidden"),
                Color = raw("var(--text)"),
                FontSize = raw("12px"),
                FontWeight = raw("600"),
                TextOverflow = raw("ellipsis"),
                WhiteSpace = raw("nowrap"),
                Background = raw("var(--surface-subtle)"),
                Border = px(1) | solid | var("--border"),
                BorderRadius = raw("4px")
            });

        global(".ja-user-region",
            new CssRule
            {
                MinWidth = raw("0"),
                PaddingLeft = raw("8px"),
                BorderLeft = px(1) | solid | var("--border")
            });

        global(".ja-access-command",
            new CssRule
            {
                MinHeight = raw("30px"),
                Padding = padding(px(4), px(8)),
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
                MinHeight = raw("116px"),
                Padding = padding(px(14), px(16)),
                Background = raw("var(--surface)"),
                Border = px(1) | solid | var("--border"),
                BorderTop = px(3) | solid | var("--accent"),
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
                Margin = raw("12px 0 6px"),
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
                Border = px(1) | solid | var("--border"),
                BorderRadius = raw("6px"),
                BoxShadow = var("--shadow")
            });

        global(".ja-overview__panel-header, .ja-management__panel-header",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center"),
                JustifyContent = raw("space-between"),
                MinHeight = raw("56px"),
                Padding = padding(px(12), px(16)),
                Gap = raw("12px"),
                BorderBottom = px(1) | solid | var("--border")
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
                Padding = padding(px(0)),
                Margin = raw("0"),
                ListStyle = raw("none")
            });

        global(".ja-overview__organization-list li, .ja-overview__role-list li, .ja-management__item-list li",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center"),
                MinHeight = raw("48px"),
                Padding = padding(px(10), px(20)),
                Gap = raw("10px"),
                BorderBottom = px(1) | solid | var("--border")
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
                Padding = padding(px(3), px(6)),
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
                Padding = padding(px(20)),
                Color = raw("var(--text-muted)"),
                FontSize = raw("13px"),
                LineHeight = raw("1.5")
            });

        global(".ja-management__error",
            new CssRule
            {
                Color = raw("var(--danger)"),
                Background = raw("var(--danger-soft)"),
                Border = px(1) | solid | raw("color-mix(in srgb, var(--danger) 30%, var(--border))"),
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
                Padding = padding(px(20)),
                Margin = raw("0"),
                Gap = raw("12px")
            });

        global(".ja-management__details div",
            new CssRule
            {
                MinWidth = raw("0"),
                Padding = padding(px(12)),
                Background = raw("var(--surface-subtle)"),
                Border = px(1) | solid | var("--border"),
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
                Padding = padding(px(20)),
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
                Padding = padding(px(6), px(9)),
                Color = raw("var(--text)"),
                Background = raw("var(--surface)"),
                Border = px(1) | solid | var("--border-strong"),
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
                Padding = padding(px(12)),
                Margin = raw("0"),
                Gap = raw("10px 14px"),
                Border = px(1) | solid | var("--border"),
                BorderRadius = raw("4px")
            });

        global(".ja-management__options legend",
            new CssRule
            {
                Padding = padding(px(0), px(5)),
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
                Padding = padding(px(0))
            });

        global(".ja-management__profiles",
            new CssRule
            {
                Display = raw("grid"),
                GridTemplateColumns = raw("repeat(3, minmax(0, 1fr))"),
                Margin = raw("20px 20px 0"),
                Border = px(1) | solid | var("--border-strong"),
                BorderRadius = raw("4px"),
                Overflow = raw("hidden")
            });

        global(".ja-management__profiles button",
            new CssRule
            {
                Color = raw("var(--text-muted)"),
                Background = raw("var(--surface)"),
                Border = raw("0"),
                BorderRight = px(1) | solid | var("--border-strong"),
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
                Padding = padding(px(14)),
                Margin = raw("20px 20px 0"),
                Gap = raw("6px"),
                Color = raw("var(--text)"),
                Background = raw("var(--warning-soft)"),
                Border = px(1) | solid | raw("color-mix(in srgb, var(--warning) 38%, var(--border))"),
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
                Padding = padding(px(6), px(12)),
                Color = raw("#ffffff"),
                FontSize = raw("13px"),
                FontWeight = raw("600"),
                Background = raw("var(--accent)"),
                Border = px(1) | solid | var("--accent"),
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
                Padding = padding(px(13), px(20)),
                TextAlign = raw("left"),
                VerticalAlign = raw("middle"),
                BorderBottom = px(1) | solid | var("--border")
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
                Padding = important(padding(px(4), px(0))),
                Color = raw("var(--accent) !important"),
                Background = raw("transparent !important"),
                Border = raw("0 !important")
            });

        global(".ja-management__role-list",
            new CssRule
            {
                Padding = padding(px(8), px(0))
            });

        global(".ja-management__role-list button",
            new CssRule
            {
                Display = raw("flex"),
                AlignItems = raw("center"),
                JustifyContent = raw("space-between"),
                Width = raw("100%"),
                Padding = padding(px(11), px(20)),
                Color = raw("var(--text)"),
                TextAlign = raw("left"),
                Background = raw("transparent"),
                Border = raw("0"),
                BorderLeft = px(3) | solid | transparent,
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
                Padding = padding(px(12), px(20), px(20)),
                Gap = raw("8px")
            });

        global(".ja-management__check",
            new CssRule
            {
                Display = raw("grid"),
                GridTemplateColumns = raw("16px minmax(0, 1fr) auto"),
                AlignItems = raw("center"),
                MinHeight = raw("40px"),
                Padding = padding(px(7), px(9)),
                Gap = raw("8px"),
                FontSize = raw("13px"),
                Background = raw("var(--surface-subtle)"),
                Border = px(1) | solid | var("--border"),
                BorderRadius = raw("4px")
            });

        global(".ja-management__check input",
            new CssRule
            {
                Width = raw("16px"),
                MinHeight = raw("16px"),
                Padding = padding(px(0))
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
                Padding = padding(px(3), px(7)),
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
                Padding = padding(px(12)),
                Margin = raw("0"),
                Gap = raw("10px"),
                Background = raw("var(--surface-subtle)"),
                Border = px(1) | solid | var("--border"),
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
