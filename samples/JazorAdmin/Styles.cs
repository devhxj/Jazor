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
                opacity = 0,
                transform = translate_y(px(8))
            }),
            new("to", new CssDeclarations
            {
                opacity = 1,
                transform = translate_y(px(0))
            })
        ]);

        var sessionSpin = keyframes(
        [
            new("to", new CssDeclarations
            {
                transform = rotate(deg(360))
            })
        ]);

        var quickPulse = keyframes(
        [
            new("0%, 100%", new CssDeclarations
            {
                box_shadow = raw("0 0 0 0 rgba(47, 111, 237, 0.18)")
            }),
            new("50%", new CssDeclarations
            {
                box_shadow = raw("0 0 0 8px rgba(47, 111, 237, 0)")
            })
        ]);

        global(":root",
            new CssRule
            {
                color_scheme = raw("light"),
                font_family = raw("Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, \"Segoe UI\", sans-serif"),
                font_synthesis = raw("none"),
                text_rendering = raw("optimizeLegibility"),
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
                box_sizing = raw("border-box")
            });

        global("html, body, #app",
            new CssRule
            {
                min_width = raw("320px"),
                min_height = raw("100%"),
                margin = raw("0")
            });

        global("body",
            new CssRule
            {
                min_height = raw("100vh"),
                background = raw("#f3f3f3")
            });

        global("button, input, select, textarea",
            new CssRule
            {
                font = raw("inherit")
            });

        global("button, select, input[type=\"checkbox\"]",
            new CssRule
            {
                cursor = raw("pointer")
            });

        global("button:disabled, input:disabled, select:disabled",
            new CssRule
            {
                cursor = raw("not-allowed"),
                opacity = raw("0.56")
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
                min_height = raw("100vh"),
                background = raw("var(--app-bg)"),
                color = raw("var(--text)")
            });

        global(".ja-application--dark",
            new CssRule
            {
                color_scheme = raw("dark"),
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
                color_scheme = raw("dark"),
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
                filter = grayscale(1)
            });

        global(".ja-shell",
            new CssRule
            {
                display = raw("grid"),
                grid_template_columns = raw("232px minmax(0, 1fr)"),
                min_height = raw("100vh")
            });

        global(".ja-shell--top",
            new CssRule
            {
                display = raw("block")
            });

        global(".ja-shell__sidebar",
            new CssRule
            {
                position = sticky,
                top = px(0),
                z_index = raw("20"),
                height = vh(100),
                overflow = raw("auto"),
                background = raw("#17241f"),
                color = raw("#eef7f3"),
                border_right = px(1) | solid | hex("293a33")
            });

        global(".ja-shell--collapsed",
            new CssRule
            {
                grid_template_columns = raw("0 minmax(0, 1fr)")
            });

        global(".ja-shell--collapsed .ja-shell__sidebar",
            new CssRule
            {
                display = raw("none"),
                width = raw("0"),
                border_right = raw("0")
            });

        global(".ja-shell__main",
            new CssRule
            {
                min_width = raw("0")
            });

        global(".ja-shell__header",
            new CssRule
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

        global(".ja-shell__sidebar-toggle",
            new CssRule
            {
                position = raw("relative"),
                flex = raw("0 0 36px"),
                width = raw("36px"),
                height = raw("36px"),
                padding = padding(px(0)),
                margin_left = raw("14px"),
                color = raw("var(--text)"),
                background = raw("transparent"),
                border = px(1) | solid | var("--border"),
                border_radius = raw("5px")
            });

        global(".ja-shell__sidebar-toggle::before",
            new CssRule
            {
                font_size = raw("20px"),
                line_height = raw("1"),
                content = raw("\"\\2630\"")
            });

        global(".ja-shell__sidebar-toggle:hover",
            new CssRule
            {
                background = raw("var(--surface-strong)")
            });

        global(".ja-shell__content",
            new CssRule
            {
                width = raw("100%")
            });

        global(".ja-tdesign-layout",
            new CssRule
            {
                min_width = raw("0"),
                min_height = raw("100vh"),
                background = raw("var(--background)")
            });

        global(".ja-tdesign-layout > [data-shell-region=\"sidebar\"]",
            new CssRule
            {
                position = raw("sticky"),
                top = raw("0"),
                z_index = raw("20"),
                height = raw("100vh"),
                min_width = px(0),
                // The secondary menu owns vertical scrolling. The Aside itself must never
                // create a second scrollbar or expose the menu's box-model overflow.
                overflow = hidden,
                border_right = px(1) | solid | var("--border")
            });

        global(".ja-tdesign-layout--mixed[data-shell-collapsed=\"false\"] > [data-shell-region=\"sidebar\"]",
            new CssRule
            {
                width = raw("296px !important"),
                flex = raw("0 0 296px !important")
            });

        global(".ja-tdesign-layout--sidebar[data-shell-collapsed=\"false\"] > [data-shell-region=\"sidebar\"]",
            new CssRule
            {
                width = raw("232px !important"),
                flex = raw("0 0 232px !important")
            });

        global(".ja-tdesign-layout[data-shell-collapsed=\"true\"] > [data-shell-region=\"sidebar\"]",
            new CssRule
            {
                width = raw("64px !important"),
                flex = raw("0 0 64px !important")
            });

        global(".ja-tdesign-layout[data-shell-sidebar-fixed=\"false\"] > [data-shell-region=\"sidebar\"]",
            new CssRule
            {
                position = relative,
                height = raw("auto"),
                min_height = raw("100vh")
            });

        global(".ja-tdesign-layout > [data-shell-region=\"main\"]",
            new CssRule
            {
                min_width = raw("0")
            });

        global(".ja-tdesign-layout [data-shell-region=\"header\"]",
            new CssRule
            {
                position = raw("sticky"),
                top = raw("0"),
                z_index = raw("15"),
                border_bottom = px(1) | solid | var("--border")
            });

        global(".ja-tdesign-layout__header",
            new CssRule
            {
                display = raw("flex"),
                align_items = raw("center"),
                width = raw("100%"),
                min_width = raw("0"),
                min_height = raw("64px"),
                gap = raw("14px")
            });

        global(".ja-tdesign-layout [data-shell-region=\"content\"]",
            new CssRule
            {
                width = raw("100%"),
                min_width = raw("0"),
                // Contain intentionally wide data tables inside their own scroll region on small screens.
                // 小屏幕下宽表格必须由表格容器滚动，不能把宿主页面整体撑出横向滚动条。
                overflow_x = hidden
            });

        global(".ja-tdesign-layout__tabs",
            new CssRule
            {
                position = sticky,
                top = px(64),
                z_index = 14,
                background = var("--td-bg-color-container"),
                border_bottom = px(1) | solid | var("--td-component-stroke")
            });

        global(".ja-tdesign-layout[data-shell-show-header=\"false\"] .ja-tdesign-layout__tabs",
            new CssRule { top = px(0) });

        global(".ja-route-tabs",
            new CssRule
            {
                width = percent(100),
                background = var("--td-bg-color-container")
            });

        global(".ja-route-tabs .t-tabs__nav-container",
            new CssRule
            {
                padding_left = px(0),
                padding_right = px(0)
            });

        global(".ja-route-tabs .t-tabs__nav--card.t-tabs__nav-item",
            new CssRule
            {
                padding_left = px(12),
                padding_right = px(12)
            });

        // TDesign 1.20 maps t-size-m tabs to 48px. Starter keeps the same
        // size class but uses a denser 40px route-tab rail.
        global(".ja-route-tabs .t-tabs__nav-item.t-size-m, .ja-route-tabs .t-tabs__btn.t-size-m, .ja-route-tabs .t-tabs__scroll-btn.t-size-m",
            new CssRule
            {
                height = px(40),
                line_height = px(40)
            });

        global(".ja-route-tabs__label, .ja-route-tabs__home",
            new CssRule
            {
                display = inline_flex,
                align_items = center,
                min_width = px(0),
                gap = px(6)
            });

        global(".ja-route-tabs__label",
            new CssRule
            {
                max_width = px(168),
                overflow = hidden,
                text_overflow = ellipsis,
                white_space = nowrap
            });

        global(".ja-tdesign-layout__content",
            new CssRule
            {
                width = percent(100),
                max_width = raw("none"),
                min_width = px(0),
                margin = margin(px(8), px(12), px(16))
            });

        global(".ja-route-breadcrumb",
            new CssRule
            {
                // TDesign breadcrumb items are flex children. Block layout makes every
                // crumb full width and turns a route hierarchy into multiple lines.
                display = flex,
                align_items = center,
                flex_wrap = no_wrap,
                margin_bottom = px(16)
            });

        global(".ja-tdesign-sidebar-shell",
            new CssRule
            {
                display = flex,
                width = percent(100),
                height = percent(100),
                min_width = px(0),
                min_height = px(0),
                overflow = hidden,
                background = raw("var(--surface)")
            });

        global(".ja-iconbar",
            new CssRule
            {
                border_right = px(1) | solid | var("--border")
            });

        global(".ja-iconbar .t-menu__logo",
            new CssRule
            {
                position = relative,
                display = flex,
                align_items = center,
                justify_content = center,
                flex = flex_box(0, 0, px(64)),
                width = raw("64px !important"),
                min_width = raw("64px !important"),
                max_width = raw("64px !important"),
                height = px(64),
                margin = margin(px(0)),
                padding = raw("0 !important"),
                box_sizing = border_box
            });

        global(".ja-iconbar__brand",
            new CssRule
            {
                position = absolute,
                // The slot remains 64px wide in every rail state. Center against that fixed
                // geometry instead of the logo's intrinsic SVG box.
                left = percent(50),
                top = px(13),
                display = inline_flex,
                align_items = center,
                justify_content = center,
                width = px(38),
                height = px(38),
                // TDesign styles the logo anchor after this generated stylesheet.
                // This must win so its visual center stays on the 64px IconBar rail.
                margin_left = raw("0 !important"),
                border_radius = radius(px(19)),
                transform = raw("translateX(-50%)")
            });

        global(".ja-iconbar__brand img",
            new CssRule
            {
                display = block,
                width = px(30),
                height = px(30)
            });

        global(".ja-iconbar .t-menu__operations",
            new CssRule
            {
                display = flex,
                align_items = center,
                justify_content = center,
                border_top = px(1) | solid | var("--border"),
                padding = padding(px(12), px(0))
            });

        global(".ja-iconbar__operations",
            new CssRule
            {
                display = flex,
                align_items = center,
                justify_content = center,
                width = percent(100)
            });

        global(".ja-iconbar .ja-iconbar__quick-trigger",
            new CssRule
            {
                position = relative,
                width = px(18),
                height = px(18),
                border = px(2) | solid | var("--accent"),
                border_radius = radius(px(999)),
                background = var("--surface"),
                color = var("--accent"),
                box_shadow = raw("0 0 0 8px rgba(47, 111, 237, 0.12), 0 0 0 18px rgba(47, 111, 237, 0.08)"),
                animation = raw(quickPulse + " 2.4s ease-in-out infinite"),
                overflow = visible
            });

        global(".ja-iconbar .ja-iconbar__quick-trigger .t-icon",
            new CssRule
            {
                display = none
            });

        global(".ja-iconbar__quick-popup .t-popup__content",
            new CssRule
            {
                padding = padding(px(0)),
                border = raw("0"),
                border_radius = raw("0"),
                background = raw("transparent"),
                box_shadow = raw("none"),
                // RightBottom places the content after the trigger. Move its local polar
                // origin back onto the small breathing circle (18px, 114px).
                transform = raw("translate(-36px, -11px)")
            });

        global(".ja-iconbar__quick-actions",
            new CssRule
            {
                position = relative,
                width = px(128),
                height = px(112),
                padding = padding(px(0)),
                isolation = isolate
            });

        global(".ja-iconbar__quick-actions .ja-iconbar__quick-action",
            new CssRule
            {
                position = absolute,
                display = flex,
                align_items = center,
                justify_content = center,
                width = px(36),
                height = px(36)
            });

        global(".ja-iconbar__quick-actions .ja-iconbar__quick-action:nth-child(1)",
            new CssRule
            {
                left = px(0),
                top = px(0)
            });

        global(".ja-iconbar__quick-actions .ja-iconbar__quick-action:nth-child(2)",
            new CssRule
            {
                // 96px * sin/cos(22.5deg), measured from the trigger's center.
                // Keep these as the polar values rather than hand-tuned stair steps.
                left = raw("36.738px"),
                top = raw("7.307px")
            });

        global(".ja-iconbar__quick-actions .ja-iconbar__quick-action:nth-child(3)",
            new CssRule
            {
                // 96px * sin/cos(45deg).
                left = raw("67.882px"),
                top = raw("28.118px")
            });

        global(".ja-iconbar__quick-actions .ja-iconbar__quick-action:nth-child(4)",
            new CssRule
            {
                // 96px * sin/cos(67.5deg): four equal 22.5deg intervals on a 90deg arc.
                left = raw("88.693px"),
                top = raw("59.261px")
            });

        global(".ja-iconbar__quick-actions .ja-iconbar__quick-action",
            new CssRule
            {
                width = px(36),
                height = px(36),
                border_radius = radius(px(4)),
                background = var("--surface"),
                color = var("--text"),
                border = px(1) | solid | var("--border"),
                box_shadow = shadows(new CssShadow(px(0), px(8), Blur: px(18), Color: rgba(31, 52, 78, 0.1)))
            });

        global(".ja-iconbar__quick-actions .ja-iconbar__quick-action--danger",
            new CssRule
            {
                background = var("--danger-soft"),
                color = var("--danger")
            });

        // IconBar is intentionally a TDesign menu instance, not a parallel navigation widget.
        // TDesign owns item spacing, selected state, tooltip and icon treatment.
        global(".ja-iconbar--rail",
            new CssRule
            {
                display = flex,
                flex = flex_box(0, 0, px(64)),
                flex_direction = column,
                width = px(64),
                min_height = percent(100)
            });

        global(".ja-iconbar--head",
            new CssRule
            {
                display = none
            });

        global(".ja-tdesign-sidebar-shell__menu",
            new CssRule
            {
                display = flex,
                flex = flex_box(0, 0, px(232)),
                flex_direction = column,
                width = px(232),
                min_width = px(0),
                max_width = percent(100),
                height = percent(100),
                min_height = px(0),
                padding = padding(px(0)),
                box_sizing = border_box,
                // TDesign's .t-menu--scroll owns the menu's overflow. Keeping this wrapper
                // non-scrollable prevents a duplicate scrollbar beside the native one.
                overflow = hidden,
                border_left = px(1) | solid | var("--border")
            });

        global(".ja-tdesign-sidebar-shell__menu-title",
            new CssRule
            {
                display = flex,
                align_items = center,
                flex = flex_box(0, 0, px(64)),
                height = px(64),
                min_width = px(0),
                padding = padding(px(0), px(20)),
                overflow = hidden,
                color = var("--td-text-color-primary"),
                font_size = px(16),
                font_weight = raw("600"),
                text_overflow = ellipsis,
                white_space = nowrap,
                border_bottom = px(1) | solid | var("--border")
            });

        global(".ja-tdesign-sidebar-shell__menu-body",
            new CssRule
            {
                flex = flex_box(1, 1, px(0)),
                min_height = px(0),
                overflow = hidden
            });

        global(".ja-tdesign-sidebar-shell__mobile-menu",
            new CssRule
            {
                display = none
            });

        global(".ja-tdesign-sidebar-shell__mobile-brand",
            new CssRule
            {
                display = none
            });

        global(".ja-tdesign-sidebar-shell__menu [data-navigation-orientation=\"vertical\"]",
            new CssRule
            {
                width = percent(100),
                height = percent(100),
                min_width = px(0),
                max_width = percent(100),
                overflow_x = hidden
            });

        global(".ja-sidebar",
            new CssRule
            {
                min_height = raw("100%"),
                padding = padding(px(20), px(14))
            });

        global(".ja-sidebar__logo",
            new CssRule
            {
                display = raw("flex"),
                align_items = raw("center"),
                min_height = raw("42px"),
                margin = raw("0 8px 22px"),
                color = raw("#ffffff"),
                font_size = raw("18px"),
                font_weight = raw("750")
            });

        global(".ja-sidebar__list, .ja-sidebar__children",
            new CssRule
            {
                padding = padding(px(0)),
                margin = raw("0"),
                list_style = raw("none")
            });

        global(".ja-sidebar__item",
            new CssRule
            {
                margin = raw("3px 0")
            });

        global(".ja-sidebar__item-content",
            new CssRule
            {
                position = raw("relative")
            });

        global(".ja-sidebar__link, .ja-sidebar__button",
            new CssRule
            {
                display = raw("flex"),
                align_items = raw("center"),
                width = raw("100%"),
                min_height = raw("40px"),
                padding = padding(px(8), px(12)),
                color = raw("#b8c8c1"),
                text_align = raw("left"),
                text_decoration = raw("none"),
                background = raw("transparent"),
                border = raw("0"),
                border_radius = raw("6px")
            });

        global(".ja-sidebar__link:hover, .ja-sidebar__button:hover, .ja-sidebar__item.is-ancestor-selected > .ja-sidebar__item-content > .ja-sidebar__button",
            new CssRule
            {
                color = raw("#ffffff"),
                background = raw("#243a31")
            });

        global(".ja-sidebar__item.is-selected > .ja-sidebar__item-content > .ja-sidebar__link, .ja-sidebar__item.is-selected > .ja-sidebar__item-content > .ja-sidebar__button",
            new CssRule
            {
                color = raw("#ffffff"),
                background = raw("#087f5b")
            });

        global(".ja-sidebar__toggle",
            new CssRule
            {
                margin_left = raw("auto"),
                font_size = raw("12px")
            });

        global(".ja-sidebar__children",
            new CssRule
            {
                padding = padding(px(4), px(0), px(4), px(12))
            });

        global(".ja-sidebar__children .ja-sidebar__link, .ja-sidebar__children .ja-sidebar__button",
            new CssRule
            {
                min_height = raw("36px"),
                padding_left = raw("16px"),
                font_size = raw("14px")
            });

        global(".ja-header",
            new CssRule
            {
                flex = raw("1 1 auto"),
                min_width = raw("0"),
                display = raw("flex"),
                align_items = raw("center"),
                justify_content = raw("space-between"),
                min_height = raw("64px"),
                padding = padding(px(8), px(24)),
                gap = raw("20px")
            });

        global(".ja-header__main, .ja-header__actions, .ja-header__toolbar, .ja-header__user-region, .ja-preferences, .ja-user-region",
            new CssRule
            {
                display = raw("flex"),
                align_items = raw("center")
            });

        global(".ja-header__main",
            new CssRule
            {
                min_width = raw("0"),
                gap = raw("12px")
            });

        global(".ja-header__logo",
            new CssRule
            {
                color = raw("var(--accent)"),
                font_weight = raw("750")
            });

        global(".ja-header__titles",
            new CssRule
            {
                min_width = raw("0")
            });

        global(".ja-header__title",
            new CssRule
            {
                overflow = raw("hidden"),
                font_size = raw("16px"),
                font_weight = raw("700"),
                text_overflow = raw("ellipsis"),
                white_space = raw("nowrap")
            });

        global(".ja-header__subtitle",
            new CssRule
            {
                margin_top = raw("2px"),
                color = raw("var(--text-muted)"),
                font_size = raw("12px")
            });

        global(".ja-header__actions",
            new CssRule
            {
                justify_content = raw("flex-end"),
                min_width = raw("0"),
                gap = raw("16px")
            });

        global(".ja-header__navigation",
            new CssRule
            {
                flex = raw("1 1 auto"),
                min_width = raw("0")
            });

        global(".ja-tdesign-header",
            new CssRule
            {
                width = percent(100),
                height = px(64),
                min_width = px(0),
                min_height = px(64),
                padding = padding(px(0)),
                overflow = hidden
            });

        global(".ja-tdesign-header .t-head-menu__inner",
            new CssRule
            {
                display = flex,
                align_items = center,
                height = px(64),
                min_height = px(64),
                min_width = px(0),
                padding_left = raw("0 !important"),
                border_bottom = px(1) | solid | var("--border"),
                overflow = hidden
            });

        global(".ja-tdesign-header__main, .ja-tdesign-header__actions",
            new CssRule
            {
                display = raw("flex"),
                align_items = raw("center")
            });

        global(".ja-tdesign-header__main",
            new CssRule
            {
                min_width = raw("0"),
                gap = raw("12px")
            });

        global(".ja-tdesign-header__titles",
            new CssRule
            {
                min_width = raw("0")
            });

        global(".ja-tdesign-header__title",
            new CssRule
            {
                overflow = raw("hidden"),
                font_size = raw("16px"),
                font_weight = raw("700"),
                text_overflow = raw("ellipsis"),
                white_space = raw("nowrap")
            });

        global(".ja-tdesign-header__subtitle",
            new CssRule
            {
                margin_top = raw("2px"),
                color = raw("var(--text-muted)"),
                font_size = raw("12px")
            });

        global(".ja-tdesign-header__navigation",
            new CssRule
            {
                flex = raw("1 1 auto"),
                min_width = raw("0"),
                overflow = hidden
            });

        global(".ja-tdesign-header__navigation .t-menu__item",
            new CssRule { min_width = raw("unset") });

        global(".ja-tdesign-header__actions",
            new CssRule
            {
                justify_content = raw("flex-end"),
                min_width = raw("0"),
                gap = raw("16px")
            });

        global(".ja-tdesign-header__logo, .ja-tdesign-header__operations",
            new CssRule
            {
                display = flex,
                align_items = center,
                min_width = px(0),
                gap = px(10)
            });

        global(".ja-tdesign-header__logo",
            new CssRule
            {
                flex = raw("0 0 64px"),
                width = px(64),
                margin_left = px(0),
                justify_content = center,
                gap = px(0)
            });

        global(".ja-tdesign-header__operations",
            new CssRule
            {
                margin_left = auto
            });

        global(".ja-tdesign-header .t-menu__logo",
            new CssRule
            {
                // The header starts at the main-content edge. TDesign's default logo slot is
                // too wide for a navigation toggle, so reserve one 64px geometry rail.
                flex = raw("0 0 64px !important"),
                width = raw("64px !important"),
                margin_left = raw("0 !important"),
                padding = raw("0 !important")
            });

        global(".ja-tdesign-layout--top .ja-tdesign-header__logo",
            new CssRule
            {
                flex = raw("0 0 208px"),
                width = px(208),
                justify_content = raw("flex-start")
            });

        global(".ja-tdesign-layout--top .ja-tdesign-header .t-menu__logo",
            new CssRule
            {
                flex = raw("0 0 208px !important"),
                width = raw("208px !important"),
                padding_left = raw("24px !important")
            });

        global(".ja-tdesign-header [data-shell-command=\"toggle-sidebar\"]",
            new CssRule
            {
                display = grid,
                width = px(32),
                height = px(32),
                min_width = px(32),
                padding = raw("0 !important"),
                place_items = center
            });

        global(".ja-tdesign-header .t-menu__operations",
            new CssRule
            {
                margin_right = px(16)
            });

        global(".ja-sidebar--horizontal",
            new CssRule
            {
                min_height = raw("0"),
                padding = padding(px(0)),
                color = raw("var(--text)")
            });

        global(".ja-sidebar--horizontal > .ja-sidebar__list, .ja-sidebar--horizontal .ja-sidebar__children",
            new CssRule
            {
                display = raw("flex"),
                align_items = raw("center"),
                gap = raw("4px")
            });

        global(".ja-sidebar--horizontal > .ja-sidebar__list",
            new CssRule
            {
                overflow_x = raw("auto"),
                overscroll_behavior_inline = raw("contain")
            });

        global(".ja-sidebar--horizontal .ja-sidebar__item",
            new CssRule
            {
                display = raw("flex"),
                flex = raw("0 0 auto"),
                align_items = raw("center"),
                margin = raw("0")
            });

        global(".ja-sidebar--horizontal .ja-sidebar__link, .ja-sidebar--horizontal .ja-sidebar__button",
            new CssRule
            {
                width = raw("auto"),
                min_height = raw("34px"),
                padding = padding(px(6), px(10)),
                color = raw("var(--text-muted)"),
                white_space = raw("nowrap")
            });

        global(".ja-sidebar--horizontal .ja-sidebar__link:hover, .ja-sidebar--horizontal .ja-sidebar__button:hover, .ja-sidebar--horizontal .ja-sidebar__item.is-ancestor-selected > .ja-sidebar__item-content > .ja-sidebar__button",
            new CssRule
            {
                color = raw("var(--text)"),
                background = raw("var(--surface-strong)")
            });

        global(".ja-sidebar--horizontal .ja-sidebar__item.is-selected > .ja-sidebar__item-content > .ja-sidebar__link, .ja-sidebar--horizontal .ja-sidebar__item.is-selected > .ja-sidebar__item-content > .ja-sidebar__button",
            new CssRule
            {
                color = raw("var(--accent-strong)"),
                background = raw("var(--accent-soft)")
            });

        global(".ja-sidebar--horizontal .ja-sidebar__children",
            new CssRule
            {
                padding = padding(px(0), px(0), px(0), px(4))
            });

        global(".ja-header__toolbar, .ja-preferences, .ja-user-region",
            new CssRule
            {
                gap = raw("8px")
            });

        global(".ja-preference, .ja-preference-toggle",
            new CssRule
            {
                display = raw("flex"),
                align_items = raw("center"),
                gap = raw("6px"),
                color = raw("var(--text-muted)"),
                font_size = raw("12px"),
                white_space = raw("nowrap")
            });

        global(".ja-preference select, .ja-access input",
            new CssRule
            {
                min_height = raw("36px"),
                padding = padding(px(7), px(10)),
                color = raw("var(--text)"),
                background = raw("var(--surface)"),
                border = px(1) | solid | var("--border-strong"),
                border_radius = raw("5px")
            });

        global(".ja-preference select",
            new CssRule
            {
                min_height = raw("32px"),
                padding = padding(px(4), px(24), px(4), px(8))
            });

        global(".ja-user",
            new CssRule
            {
                max_width = raw("180px"),
                overflow = raw("hidden"),
                font_size = raw("13px"),
                font_weight = raw("650"),
                text_overflow = raw("ellipsis"),
                white_space = raw("nowrap")
            });

        global(".ja-access-command, .ja-error__action, .ja-access button",
            new CssRule
            {
                min_height = raw("34px"),
                padding = padding(px(7), px(12)),
                color = raw("var(--text)"),
                font_weight = raw("650"),
                background = raw("var(--surface)"),
                border = px(1) | solid | var("--border-strong"),
                border_radius = raw("5px")
            });

        global(".ja-access-command:hover",
            new CssRule
            {
                background = raw("var(--surface-strong)")
            });

        global(".ja-page",
            new CssRule
            {
                width = raw("min(100%, 1480px)"),
                margin = raw("0 auto"),
                padding = padding(px(16))
            });

        global(".ja-page__header",
            new CssRule
            {
                display = raw("flex"),
                align_items = raw("flex-end"),
                justify_content = raw("space-between"),
                margin_bottom = raw("16px"),
                gap = raw("16px")
            });

        global(".ja-page__titles",
            new CssRule
            {
                min_width = raw("0")
            });

        global(".ja-breadcrumb",
            new CssRule
            {
                display = raw("flex"),
                flex_wrap = raw("wrap"),
                gap = raw("6px"),
                margin_bottom = raw("7px"),
                color = raw("var(--text-muted)"),
                font_size = raw("13px")
            });

        global(".ja-breadcrumb__item + .ja-breadcrumb__item::before",
            new CssRule
            {
                margin_right = raw("6px"),
                color = raw("var(--border-strong)"),
                content = raw("\"/\"")
            });

        global(".ja-page__title",
            new CssRule
            {
                margin = raw("0"),
                font_size = raw("26px"),
                line_height = raw("1.25")
            });

        global(".ja-page__subtitle",
            new CssRule
            {
                max_width = raw("760px"),
                margin = raw("7px 0 0"),
                color = raw("var(--text-muted)"),
                line_height = raw("1.5")
            });

        global(".ja-page__actions",
            new CssRule
            {
                display = raw("flex"),
                flex_wrap = raw("wrap"),
                justify_content = raw("flex-end"),
                gap = raw("8px")
            });

        global(".ja-error__action, .ja-access button",
            new CssRule
            {
                color = raw("#ffffff"),
                background = raw("var(--accent)"),
                border_color = raw("var(--accent)")
            });

        global(".ja-error__action:hover, .ja-access button:hover",
            new CssRule
            {
                background = raw("var(--accent-strong)"),
                border_color = raw("var(--accent-strong)")
            });

        global(".ja-page__action--danger",
            new CssRule
            {
                color = raw("var(--danger)"),
                border_color = raw("var(--danger)")
            });

        global(".ja-page__body > * + *",
            new CssRule
            {
                margin_top = raw("16px")
            });

        global(".ja-tdesign-page-container",
            new CssRule
            {
                width = percent(100),
                min_width = px(0),
                margin = margin(px(0))
            });

        global(".ja-tdesign-page-container__header",
            new CssRule
            {
                display = raw("flex"),
                align_items = raw("flex-end"),
                justify_content = raw("space-between"),
                margin_bottom = raw("16px"),
                gap = raw("16px")
            });

        global(".ja-tdesign-page-container__titles",
            new CssRule
            {
                min_width = raw("0")
            });

        global(".ja-tdesign-page-container__title",
            new CssRule
            {
                margin_top = raw("7px"),
                font_size = raw("26px"),
                font_weight = raw("700"),
                line_height = raw("1.25")
            });

        global(".ja-tdesign-page-container__subtitle",
            new CssRule
            {
                max_width = raw("760px"),
                margin_top = raw("7px"),
                color = raw("var(--text-muted)"),
                line_height = raw("1.5")
            });

        global(".ja-tdesign-page-container__body > * + *",
            new CssRule
            {
                margin_top = raw("16px")
            });

        global(".ja-error",
            new CssRule
            {
                display = raw("grid"),
                min_height = raw("100vh"),
                padding = padding(px(32)),
                place_items = raw("center")
            });

        global(".ja-access",
            new CssRule
            {
                display = raw("grid"),
                position = raw("relative"),
                isolation = isolate,
                min_height = raw("100vh"),
                grid_template_columns = raw("minmax(0, 1fr) minmax(0, 1fr)"),
                align_items = stretch,
                justify_items = stretch,
                padding = padding(px(0)),
                overflow = hidden,
                background = hex("edf3ed")
            });

        global(".ja-error",
            new CssRule
            {
                background = raw("var(--app-bg)")
            });

        global(".ja-access__panel, .ja-error__content",
            new CssRule
            {
                width = raw("min(100%, 448px)")
            });

        global(".ja-access__visual",
            new CssRule
            {
                position = relative,
                display = flex,
                flex_direction = column,
                justify_content = space_between,
                min_width = px(0),
                min_height = vh(100),
                padding = padding(px(40), px(56), px(48)),
                overflow = hidden,
                color = hex("ffffff"),
                background_color = hex("0b3f42"),
                background_image = raw("linear-gradient(90deg, rgba(7, 47, 49, 0.76), rgba(7, 47, 49, 0.22) 54%, rgba(237, 243, 237, 0.62) 100%), url('/brand/login-art.webp')"),
                background_position = raw("center right"),
                background_size = cover
            });

        global(".ja-access__visual::after",
            new CssRule
            {
                position = absolute,
                inset = raw("0"),
                content = raw("\"\""),
                background = raw("linear-gradient(180deg, rgba(255, 255, 255, 0.08), rgba(0, 0, 0, 0.14))"),
                pointer_events = none
            });

        global(".ja-access__visual > *",
            new CssRule
            {
                position = relative,
                z_index = raw("1")
            });

        global(".ja-access__visual-copy",
            new CssRule
            {
                max_width = px(520)
            });

        global(".ja-access__visual-copy h2",
            new CssRule
            {
                margin = margin(px(0), px(0), px(16)),
                color = hex("ffffff"),
                font_size = raw("40px"),
                font_weight = raw("700"),
                line_height = raw("1.18")
            });

        global(".ja-access__visual-copy p",
            new CssRule
            {
                max_width = px(440),
                color = raw("rgba(255, 255, 255, 0.76)"),
                font_size = px(16),
                line_height = raw("1.7")
            });

        global(".ja-access__panel",
            new CssRule
            {
                position = relative,
                z_index = raw("1"),
                grid_column = 2,
                align_self = center,
                justify_self = center,
                padding = padding(px(56), px(56)),
                overflow = hidden,
                color = hex("1d2129"),
                background = raw("transparent"),
                border = raw("0"),
                border_radius = px(0),
                box_shadow = raw("none")
            });

        global(".ja-access__panel > .ja-access__brand",
            new CssRule
            {
                display = none
            });

        global(".ja-access__brand",
            new CssRule
            {
                display = raw("inline-flex"),
                align_items = raw("center"),
                gap = raw("12px"),
                color = raw("currentColor"),
                font_size = raw("18px"),
                line_height = raw("1")
            });

        global(".ja-access__brand-mark",
            new CssRule
            {
                display = raw("block"),
                flex = raw("0 0 auto"),
                width = raw("44px"),
                height = raw("44px"),
                filter = raw("drop-shadow(0 8px 18px rgba(4, 24, 33, 0.28))")
            });

        global(".ja-access__brand-copy",
            new CssRule
            {
                display = raw("grid"),
                gap = raw("5px")
            });

        global(".ja-access__brand-copy small",
            new CssRule
            {
                color = raw("rgba(255, 255, 255, 0.68)"),
                font_size = raw("11px"),
                font_weight = raw("600"),
                text_transform = raw("uppercase")
            });

        global(".ja-brand-logo",
            new CssRule
            {
                display = raw("inline-flex"),
                align_items = raw("center"),
                gap = raw("8px"),
                color = raw("var(--text)"),
                font_size = raw("16px"),
                font_weight = raw("700"),
                line_height = raw("1")
            });

        global(".ja-brand-mark",
            new CssRule
            {
                display = raw("block"),
                flex = raw("0 0 auto"),
                width = raw("24px"),
                height = raw("24px")
            });

        global(".ja-access__intro",
            new CssRule
            {
                margin_top = raw("0")
            });

        global(".ja-access h1",
            new CssRule
            {
                margin = raw("0 0 10px"),
                color = raw("#1d2129"),
                font_size = raw("28px"),
                font_weight = raw("700"),
                text_shadow = raw("none")
            });

        global(".ja-access p",
            new CssRule
            {
                margin = raw("0"),
                color = raw("#86909c"),
                line_height = raw("1.5")
            });

        global(".ja-access form",
            new CssRule
            {
                display = raw("grid"),
                margin_top = raw("32px"),
                gap = raw("18px")
            });

        global(".ja-access label",
            new CssRule
            {
                display = raw("grid"),
                gap = raw("8px"),
                color = raw("#4e5969"),
                font_size = raw("12px"),
                font_weight = raw("650"),
                text_shadow = raw("none")
            });

        global(".ja-access input",
            new CssRule
            {
                width = raw("100%"),
                min_height = raw("40px"),
                padding = padding(px(8), px(12)),
                color = raw("#1d2129"),
                background = raw("#ffffff"),
                border = px(1) | solid | raw("#dcdcdc"),
                border_radius = raw("3px"),
                box_shadow = raw("none")
            });

        global(".ja-access input:focus",
            new CssRule
            {
                background = raw("#ffffff"),
                border_color = raw("#0052d9"),
                box_shadow = raw("0 0 0 2px rgba(0, 82, 217, 0.12)")
            });

        global(".ja-access button",
            new CssRule
            {
                min_height = raw("40px"),
                color = raw("#ffffff"),
                background = raw("#0052d9"),
                border = px(1) | solid | raw("#0052d9"),
                border_radius = raw("3px"),
                box_shadow = raw("none")
            });

        global(".ja-access button:hover",
            new CssRule
            {
                background = raw("#266fe8"),
                border_color = raw("#266fe8")
            });

        global(".ja-access__captcha-control",
            new CssRule
            {
                display = raw("grid"),
                grid_template_columns = raw("minmax(0, 1fr) 112px 40px"),
                align_items = raw("center"),
                gap = raw("8px")
            });

        global(".ja-access__captcha-control input",
            new CssRule
            {
                min_width = raw("0"),
                text_transform = raw("uppercase"),
                letter_spacing = raw("0.08em")
            });

        global(".ja-access__captcha-image",
            new CssRule
            {
                display = raw("block"),
                width = raw("112px"),
                height = raw("40px"),
                background = raw("#f3f3f3"),
                border = px(1) | solid | raw("#dcdcdc"),
                border_radius = raw("3px")
            });

        global(".ja-access__captcha-refresh",
            new CssRule
            {
                display = raw("grid"),
                width = raw("40px"),
                height = raw("40px"),
                min_height = raw("40px"),
                padding = padding(px(0)),
                place_items = raw("center"),
                color = raw("#0052d9 !important"),
                font_size = raw("18px"),
                line_height = raw("1"),
                background = raw("#ffffff !important"),
                border_color = raw("#dcdcdc !important"),
                border_radius = raw("3px"),
                box_shadow = raw("none")
            });

        global(".ja-access__captcha-refresh:hover",
            new CssRule
            {
                background = raw("#f2f3ff !important"),
                border_color = raw("#0052d9 !important")
            });

        global(".ja-access__captcha-refresh:focus-visible",
            new CssRule
            {
                outline = px(2) | solid | rgba(0, 82, 217, 0.28),
                outline_offset = raw("2px")
            });

        global(".ja-access__error",
            new CssRule
            {
                padding = padding(px(10), px(12)),
                color = raw("#d54941 !important"),
                font_size = raw("13px"),
                background = raw("#fff0ed"),
                border = px(1) | solid | raw("#f9d7d2"),
                border_radius = raw("3px")
            });

        global(".ja-error__content",
            new CssRule
            {
                text_align = raw("center")
            });

        global(".ja-error__code",
            new CssRule
            {
                display = raw("block"),
                color = raw("var(--accent)"),
                font_size = raw("72px"),
                font_weight = raw("800"),
                line_height = raw("1")
            });

        global(".ja-error--internal-server-error .ja-error__code",
            new CssRule
            {
                color = raw("var(--danger)")
            });

        global(".ja-error h1",
            new CssRule
            {
                margin = raw("18px 0 8px"),
                font_size = raw("28px")
            });

        global(".ja-error p",
            new CssRule
            {
                margin = raw("0"),
                color = raw("var(--text-muted)"),
                line_height = raw("1.6")
            });

        global(".ja-error__action",
            new CssRule
            {
                margin_top = raw("24px")
            });

        global(":where(a, button, input, select):focus-visible",
            new CssRule
            {
                outline = px(3) | solid | raw("color-mix(in srgb, var(--accent) 35%, transparent)"),
                outline_offset = raw("2px")
            });

        Media(".ja-header", "(max-width: 1080px)",
            new CssRule
            {
                align_items = raw("flex-start")
            });

        Media(".ja-header__actions", "(max-width: 1080px)",
            new CssRule
            {
                align_items = raw("flex-end"),
                flex_direction = raw("column-reverse"),
                gap = raw("6px")
            });

        Media(".ja-header__navigation", "(max-width: 1080px)",
            new CssRule
            {
                width = raw("100%")
            });

        Media(".ja-preferences", "(max-width: 1080px)",
            new CssRule
            {
                flex_wrap = raw("wrap"),
                justify_content = raw("flex-end")
            });

        Media(".ja-tdesign-header", "(max-width: 1080px)",
            new CssRule
            {
                align_items = center
            });

        Media(".ja-tdesign-header__actions", "(max-width: 1080px)",
            new CssRule
            {
                align_items = raw("flex-end"),
                flex_direction = raw("column-reverse"),
                gap = raw("6px")
            });

        Media(".ja-tdesign-header__navigation", "(max-width: 1080px)",
            new CssRule
            {
                width = raw("100%")
            });

        Media(".ja-shell", "(max-width: 760px)",
            new CssRule
            {
                display = raw("block"),
                min_width = raw("0")
            });

        Media(".ja-shell__sidebar", "(max-width: 760px)",
            new CssRule
            {
                position = raw("static"),
                height = raw("auto"),
                overflow = raw("visible"),
                border_right = raw("0"),
                border_bottom = px(1) | solid | hex("293a33")
            });

        Media(".ja-tdesign-layout", "(max-width: 760px)",
            new CssRule
            {
                flex_direction = raw("column !important"),
                min_width = raw("0")
            });

        Media(".ja-tdesign-layout[data-shell-collapsed] > [data-shell-region=\"sidebar\"]", "(max-width: 760px)",
            new CssRule
            {
                position = raw("static"),
                width = raw("100% !important"),
                max_width = raw("100%"),
                height = raw("auto"),
                overflow = raw("visible"),
                border_right = raw("0"),
                border_bottom = px(1) | solid | var("--border"),
                flex = raw("0 0 auto !important")
            });

        Media(".ja-tdesign-layout > [data-shell-region=\"main\"]", "(max-width: 760px)",
            new CssRule
            {
                width = raw("100%")
            });

        Media(".ja-tdesign-layout [data-shell-region=\"header\"]", "(max-width: 760px)",
            new CssRule
            {
                position = static_position,
                height = important(px(64)),
                min_height = px(64)
            });

        Media(".ja-tdesign-layout__header", "(max-width: 760px)",
            new CssRule
            {
                align_items = center,
                flex_wrap = no_wrap,
                height = px(64)
            });

        Media(".ja-tdesign-sidebar-shell", "(max-width: 760px)",
            new CssRule
            {
                flex_direction = column,
                width = percent(100),
                height = auto,
                min_height = px(0),
                overflow = visible
            });

        Media(".ja-iconbar--rail", "(max-width: 760px)",
            new CssRule
            {
                display = none
            });

        Media(".ja-iconbar--head", "(max-width: 760px)",
            new CssRule
            {
                display = none
            });

        Media(".ja-tdesign-sidebar-shell__menu", "(max-width: 760px)",
            new CssRule
            {
                display = none
            });

        Media(".ja-tdesign-sidebar-shell__mobile-menu", "(max-width: 760px)",
            new CssRule
            {
                display = flex,
                align_items = center,
                width = percent(100),
                height = px(64),
                min_width = px(0),
                border_bottom = px(1) | solid | var("--border")
            });

        Media(".ja-tdesign-sidebar-shell__mobile-brand", "(max-width: 760px)",
            new CssRule
            {
                display = inline_flex,
                align_items = center,
                justify_content = center,
                flex = flex_box(0, 0, px(64)),
                width = px(64),
                height = px(64)
            });

        Media(".ja-tdesign-sidebar-shell__mobile-brand img", "(max-width: 760px)",
            new CssRule
            {
                display = block,
                width = px(30),
                height = px(30)
            });

        Media(".ja-tdesign-sidebar-shell__mobile-navigation", "(max-width: 760px)",
            new CssRule
            {
                flex = flex_box(1, 1, px(0)),
                min_width = px(0),
                overflow_x = auto
            });

        Media(".ja-tdesign-header [data-shell-command=\"toggle-sidebar\"]", "(max-width: 760px)",
            new CssRule
            {
                display = none
            });

        Media("[data-navigation-orientation=\"vertical\"]", "(max-width: 760px)",
            new CssRule
            {
                display = important(flex),
                align_items = flex_start,
                width = percent(100),
                overflow_x = hidden,
                overscroll_behavior_inline = keyword("contain")
            });

        Media(".ja-sidebar", "(max-width: 760px)",
            new CssRule
            {
                min_height = raw("0"),
                padding = padding(px(10), px(12))
            });

        Media(".ja-sidebar__logo", "(max-width: 760px)",
            new CssRule
            {
                min_height = raw("34px"),
                margin = raw("0 6px 8px"),
                font_size = raw("16px")
            });

        Media(".ja-sidebar__list", "(max-width: 760px)",
            new CssRule
            {
                display = raw("flex"),
                align_items = raw("flex-start"),
                width = raw("100%"),
                padding_bottom = raw("3px"),
                gap = raw("4px"),
                overflow_x = raw("auto"),
                overscroll_behavior_inline = raw("contain")
            });

        Media(".ja-sidebar__item", "(max-width: 760px)",
            new CssRule
            {
                flex = raw("0 0 auto"),
                margin = raw("0")
            });

        Media(".ja-sidebar__link,   .ja-sidebar__button", "(max-width: 760px)",
            new CssRule
            {
                min_height = raw("36px"),
                width = raw("auto"),
                white_space = raw("nowrap")
            });

        Media(".ja-sidebar__children", "(max-width: 760px)",
            new CssRule
            {
                display = raw("flex"),
                padding = padding(px(4), px(0), px(0), px(8)),
                gap = raw("4px")
            });

        Media(".ja-sidebar__children .ja-sidebar__item", "(max-width: 760px)",
            new CssRule
            {
                flex = raw("0 0 auto")
            });

        Media(".ja-sidebar__children .ja-sidebar__link,   .ja-sidebar__children .ja-sidebar__button", "(max-width: 760px)",
            new CssRule
            {
                min_height = raw("32px"),
                padding = padding(px(6), px(10))
            });

        Media(".ja-shell__header", "(max-width: 760px)",
            new CssRule
            {
                position = raw("static")
            });

        Media(".ja-header", "(max-width: 760px)",
            new CssRule
            {
                display = raw("block"),
                min_height = raw("0"),
                padding = padding(px(12), px(16))
            });

        Media(".ja-shell__sidebar-toggle", "(max-width: 760px)",
            new CssRule
            {
                align_self = raw("flex-start"),
                margin = raw("12px 0 0 12px")
            });

        Media(".ja-header__subtitle", "(max-width: 760px)",
            new CssRule
            {
                display = raw("none")
            });

        Media(".ja-header__actions", "(max-width: 760px)",
            new CssRule
            {
                align_items = raw("stretch"),
                margin_top = raw("10px")
            });

        Media(".ja-tdesign-header", "(max-width: 760px)",
            new CssRule
            {
                width = percent(100),
                height = px(64),
                min_height = px(64),
                padding = padding(px(0))
            });

        Media(".ja-tdesign-header__subtitle", "(max-width: 760px)",
            new CssRule
            {
                display = raw("none")
            });

        Media(".ja-tdesign-header__actions", "(max-width: 760px)",
            new CssRule
            {
                align_items = raw("stretch"),
                margin_top = raw("10px")
            });

        Media(".ja-tdesign-header__navigation", "(max-width: 760px)",
            new CssRule
            {
                margin_top = raw("10px")
            });

        Media(".ja-header__navigation", "(max-width: 760px)",
            new CssRule
            {
                margin_top = raw("10px")
            });

        Media(".ja-preferences,   .ja-user-region", "(max-width: 760px)",
            new CssRule
            {
                justify_content = raw("flex-start"),
                overflow_x = raw("auto")
            });

        Media(".ja-page", "(max-width: 760px)",
            new CssRule
            {
                padding = padding(px(18), px(14), px(28))
            });

        Media(".ja-page__header", "(max-width: 760px)",
            new CssRule
            {
                display = raw("block"),
                margin_bottom = raw("16px")
            });

        Media(".ja-page__title", "(max-width: 760px)",
            new CssRule
            {
                font_size = raw("23px")
            });

        Media(".ja-page__actions", "(max-width: 760px)",
            new CssRule
            {
                justify_content = raw("flex-start"),
                margin_top = raw("14px")
            });

        Media(".ja-tdesign-layout__content", "(max-width: 760px)",
            new CssRule
            {
                margin = margin(px(8), px(8), px(12))
            });

        Media(".ja-route-breadcrumb", "(max-width: 760px)",
            new CssRule
            {
                margin_bottom = px(16)
            });

        Media(".ja-tdesign-page-container__header", "(max-width: 760px)",
            new CssRule
            {
                display = raw("block"),
                margin_bottom = raw("16px")
            });

        Media(".ja-tdesign-page-container__title", "(max-width: 760px)",
            new CssRule
            {
                font_size = raw("23px")
            });

        Media(".ja-access,   .ja-error", "(max-width: 760px)",
            new CssRule
            {
                padding = padding(px(0))
            });

        Media(".ja-access", "(max-width: 760px)",
            new CssRule
            {
                grid_template_columns = raw("minmax(0, 1fr)"),
                min_height = raw("100svh")
            });

        Media(".ja-access__visual", "(max-width: 760px)",
            new CssRule
            {
                display = none
            });

        Media(".ja-access__panel", "(max-width: 760px)",
            new CssRule
            {
                grid_column = raw("1"),
                width = raw("min(100%, 448px)"),
                padding = padding(px(32), px(24)),
                border_radius = px(0)
            });

        Media(".ja-access__panel > .ja-access__brand", "(max-width: 760px)",
            new CssRule
            {
                display = inline_flex,
                margin_bottom = px(40),
                color = raw("#1d2129")
            });

        Media(".ja-access__panel > .ja-access__brand .ja-access__brand-copy small", "(max-width: 760px)",
            new CssRule
            {
                color = raw("#86909c")
            });

        Media(".ja-access__captcha-control", "(max-width: 430px)",
            new CssRule
            {
                grid_template_columns = raw("minmax(0, 1fr) 48px")
            });

        Media(".ja-access__captcha-image", "(max-width: 430px)",
            new CssRule
            {
                grid_column = raw("1 / -1"),
                grid_row = raw("2")
            });

        Media(".ja-error__code", "(max-width: 760px)",
            new CssRule
            {
                font_size = raw("58px")
            });

        Media(".ja-preference-toggle", "(max-width: 430px)",
            new CssRule
            {
                display = raw("none")
            });

        Media(".ja-user", "(max-width: 430px)",
            new CssRule
            {
                max_width = raw("120px")
            });

        // The production administration pages share a compact TDesign Starter-like work surface.
        // 生产管理页共用紧凑的 TDesign Starter 风格工作台，状态层次由数据与选择关系表达。
        global(".ja-session-state",
            new CssRule
            {
                display = raw("grid"),
                min_height = raw("100vh"),
                place_items = raw("center"),
                gap = raw("10px"),
                color = raw("var(--text-muted)"),
                font_size = raw("14px"),
                background = raw("var(--app-bg)")
            });

        global(".ja-session-state__spinner",
            new CssRule
            {
                width = raw("24px"),
                height = raw("24px"),
                border = px(2) | solid | var("--border-strong"),
                border_top_color = raw("var(--accent)"),
                border_radius = raw("50%"),
                animation = raw(sessionSpin + " 720ms linear infinite")
            });

        global(".ja-header-context",
            new CssRule
            {
                display = raw("flex"),
                align_items = raw("center"),
                min_width = raw("0"),
                gap = raw("8px")
            });

        global(".ja-organization-picker",
            new CssRule
            {
                display = raw("grid"),
                grid_template_columns = raw("auto minmax(112px, 180px)"),
                align_items = raw("center"),
                min_width = raw("0"),
                padding_right = raw("8px"),
                gap = raw("7px"),
                color = raw("var(--text-muted)"),
                font_size = raw("12px"),
                border_right = px(1) | solid | var("--border")
            });

        global(".ja-organization-picker select",
            new CssRule
            {
                min_width = raw("0"),
                min_height = raw("32px"),
                padding = padding(px(4), px(26), px(4), px(8)),
                overflow = raw("hidden"),
                color = raw("var(--text)"),
                font_size = raw("12px"),
                font_weight = raw("600"),
                text_overflow = raw("ellipsis"),
                white_space = raw("nowrap"),
                background = raw("var(--surface-subtle)"),
                border = px(1) | solid | var("--border"),
                border_radius = raw("4px")
            });

        global(".ja-user-region",
            new CssRule
            {
                min_width = raw("0"),
                padding_left = raw("8px"),
                border_left = px(1) | solid | var("--border")
            });

        global(".ja-access-command",
            new CssRule
            {
                min_height = raw("30px"),
                padding = padding(px(4), px(8)),
                font_size = raw("12px"),
                background = raw("transparent"),
                border = raw("0")
            });

        global(".ja-access-command[data-access-command=\"sign-out\"]",
            new CssRule
            {
                color = raw("var(--danger)")
            });

        global(".ja-overview",
            new CssRule
            {
                display = raw("grid"),
                min_width = raw("0"),
                gap = raw("16px")
            });

        global(".ja-overview__metrics",
            new CssRule
            {
                display = raw("grid"),
                grid_template_columns = raw("repeat(4, minmax(0, 1fr))"),
                gap = raw("16px")
            });

        global(".ja-overview__metric",
            new CssRule
            {
                min_width = raw("0"),
                min_height = raw("116px"),
                padding = padding(px(14), px(16)),
                background = raw("var(--surface)"),
                border = px(1) | solid | var("--border"),
                border_top = px(3) | solid | var("--accent"),
                border_radius = raw("6px"),
                box_shadow = var("--shadow"),
                animation = raw(dashboardEnter + " 260ms ease both")
            });

        global(".ja-overview__metric:nth-child(2)",
            new CssRule
            {
                animation_delay = raw("35ms")
            });

        global(".ja-overview__metric:nth-child(3)",
            new CssRule
            {
                animation_delay = raw("70ms")
            });

        global(".ja-overview__metric:nth-child(4)",
            new CssRule
            {
                animation_delay = raw("105ms")
            });

        global(".ja-overview__metric span, .ja-overview__metric small",
            new CssRule
            {
                display = raw("block"),
                overflow = raw("hidden"),
                color = raw("var(--text-muted)"),
                text_overflow = raw("ellipsis"),
                white_space = raw("nowrap")
            });

        global(".ja-overview__metric span",
            new CssRule
            {
                font_size = raw("13px"),
                font_weight = raw("600")
            });

        global(".ja-overview__metric strong",
            new CssRule
            {
                display = raw("block"),
                margin = raw("12px 0 6px"),
                overflow = raw("hidden"),
                font_size = raw("24px"),
                line_height = raw("1"),
                text_overflow = raw("ellipsis"),
                white_space = raw("nowrap")
            });

        global(".ja-overview__metric small",
            new CssRule
            {
                font_size = raw("12px")
            });

        global(".ja-overview__grid",
            new CssRule
            {
                display = raw("grid"),
                grid_template_columns = raw("repeat(2, minmax(0, 1fr))"),
                min_width = raw("0"),
                gap = raw("16px")
            });

        global(".ja-overview__panel, .ja-management__panel",
            new CssRule
            {
                min_width = raw("0"),
                background = raw("var(--surface)"),
                border = px(1) | solid | var("--border"),
                border_radius = raw("6px"),
                box_shadow = var("--shadow")
            });

        global(".ja-overview__panel-header, .ja-management__panel-header",
            new CssRule
            {
                display = raw("flex"),
                align_items = raw("center"),
                justify_content = raw("space-between"),
                min_height = raw("56px"),
                padding = padding(px(12), px(16)),
                gap = raw("12px"),
                border_bottom = px(1) | solid | var("--border")
            });

        global(".ja-overview__panel-header h2, .ja-overview__panel-header p, .ja-management__panel-header h2, .ja-management__panel-header p",
            new CssRule
            {
                margin = raw("0")
            });

        global(".ja-overview__panel-header h2, .ja-management__panel-header h2",
            new CssRule
            {
                font_size = raw("15px"),
                font_weight = raw("650")
            });

        global(".ja-overview__panel-header p, .ja-management__panel-header p",
            new CssRule
            {
                margin_top = raw("5px"),
                color = raw("var(--text-muted)"),
                font_size = raw("12px"),
                line_height = raw("1.4")
            });

        global(".ja-overview__organization-list, .ja-overview__role-list, .ja-management__item-list, .ja-management__role-list",
            new CssRule
            {
                padding = padding(px(0)),
                margin = raw("0"),
                list_style = raw("none")
            });

        global(".ja-overview__organization-list li, .ja-overview__role-list li, .ja-management__item-list li",
            new CssRule
            {
                display = raw("flex"),
                align_items = raw("center"),
                min_height = raw("48px"),
                padding = padding(px(10), px(20)),
                gap = raw("10px"),
                border_bottom = px(1) | solid | var("--border")
            });

        global(".ja-overview__organization-list li.is-current",
            new CssRule
            {
                color = raw("var(--accent-strong)"),
                background = raw("var(--accent-soft)")
            });

        global(".ja-overview__organization-code, .ja-management__code",
            new CssRule
            {
                flex = raw("0 0 auto"),
                padding = padding(px(3), px(6)),
                color = raw("var(--accent-strong)"),
                font_size = raw("11px"),
                font_weight = raw("650"),
                background = raw("var(--accent-soft)"),
                border_radius = raw("3px")
            });

        global(".ja-overview__role-list li",
            new CssRule
            {
                color = raw("var(--text-muted)"),
                font_size = raw("13px")
            });

        global(".ja-overview__empty, .ja-management__empty, .ja-management__loading, .ja-management__error",
            new CssRule
            {
                margin = raw("0"),
                padding = padding(px(20)),
                color = raw("var(--text-muted)"),
                font_size = raw("13px"),
                line_height = raw("1.5")
            });

        global(".ja-management__error",
            new CssRule
            {
                color = raw("var(--danger)"),
                background = raw("var(--danger-soft)"),
                border = px(1) | solid | raw("color-mix(in srgb, var(--danger) 30%, var(--border))"),
                border_radius = raw("6px")
            });

        global(".ja-management",
            new CssRule
            {
                display = raw("grid"),
                min_width = raw("0"),
                gap = raw("16px")
            });

        global(".ja-management__split",
            new CssRule
            {
                display = raw("grid"),
                grid_template_columns = raw("minmax(0, 7fr) minmax(300px, 4fr)"),
                align_items = raw("start"),
                min_width = raw("0"),
                gap = raw("16px")
            });

        global(".ja-management__split--authorization",
            new CssRule
            {
                grid_template_columns = raw("minmax(260px, 4fr) minmax(0, 7fr)")
            });

        global(".ja-management__split--members",
            new CssRule
            {
                grid_template_columns = raw("minmax(0, 7fr) minmax(320px, 4fr)")
            });

        global(".ja-management__panel > h3, .ja-management__role-editor h3",
            new CssRule
            {
                margin = raw("20px 20px 10px"),
                font_size = raw("14px")
            });

        global(".ja-management__details",
            new CssRule
            {
                display = raw("grid"),
                grid_template_columns = raw("repeat(3, minmax(0, 1fr))"),
                padding = padding(px(20)),
                margin = raw("0"),
                gap = raw("12px")
            });

        global(".ja-management__details div",
            new CssRule
            {
                min_width = raw("0"),
                padding = padding(px(12)),
                background = raw("var(--surface-subtle)"),
                border = px(1) | solid | var("--border"),
                border_radius = raw("4px")
            });

        global(".ja-management__details dt, .ja-management__details dd",
            new CssRule
            {
                margin = raw("0")
            });

        global(".ja-management__details dt",
            new CssRule
            {
                color = raw("var(--text-muted)"),
                font_size = raw("12px")
            });

        global(".ja-management__details dd",
            new CssRule
            {
                margin_top = raw("6px"),
                overflow = raw("hidden"),
                font_size = raw("13px"),
                font_weight = raw("600"),
                text_overflow = raw("ellipsis"),
                white_space = raw("nowrap")
            });

        global(".ja-management__form",
            new CssRule
            {
                display = raw("grid"),
                padding = padding(px(20)),
                gap = raw("14px")
            });

        global(".ja-management__form--inline",
            new CssRule
            {
                grid_template_columns = raw("minmax(0, 1fr) minmax(0, 1fr) auto"),
                align_items = raw("end")
            });

        global(".ja-management__form label",
            new CssRule
            {
                display = raw("grid"),
                gap = raw("6px"),
                color = raw("var(--text-muted)"),
                font_size = raw("12px"),
                font_weight = raw("600")
            });

        global(".ja-management input, .ja-management select, .ja-management textarea",
            new CssRule
            {
                width = raw("100%"),
                min_height = raw("34px"),
                padding = padding(px(6), px(9)),
                color = raw("var(--text)"),
                background = raw("var(--surface)"),
                border = px(1) | solid | var("--border-strong"),
                border_radius = raw("4px")
            });

        global(".ja-management textarea",
            new CssRule
            {
                min_height = raw("76px"),
                resize = raw("vertical"),
                line_height = raw("1.5")
            });

        global(".ja-management__field-grid",
            new CssRule
            {
                display = raw("grid"),
                grid_template_columns = raw("repeat(2, minmax(0, 1fr))"),
                gap = raw("14px")
            });

        global(".ja-management__options",
            new CssRule
            {
                display = raw("grid"),
                grid_template_columns = raw("repeat(2, minmax(0, 1fr))"),
                padding = padding(px(12)),
                margin = raw("0"),
                gap = raw("10px 14px"),
                border = px(1) | solid | var("--border"),
                border_radius = raw("4px")
            });

        global(".ja-management__options legend",
            new CssRule
            {
                padding = padding(px(0), px(5)),
                color = raw("var(--text-muted)"),
                font_size = raw("12px"),
                font_weight = raw("650")
            });

        global(".ja-management__options label",
            new CssRule
            {
                display = raw("flex"),
                align_items = raw("center"),
                min_width = raw("0"),
                gap = raw("8px"),
                color = raw("var(--text)"),
                font_size = raw("13px"),
                font_weight = raw("500")
            });

        global(".ja-management__options input",
            new CssRule
            {
                flex = raw("0 0 16px"),
                width = raw("16px"),
                min_height = raw("16px"),
                padding = padding(px(0))
            });

        global(".ja-management__profiles",
            new CssRule
            {
                display = raw("grid"),
                grid_template_columns = raw("repeat(3, minmax(0, 1fr))"),
                margin = raw("20px 20px 0"),
                border = px(1) | solid | var("--border-strong"),
                border_radius = raw("4px"),
                overflow = raw("hidden")
            });

        global(".ja-management__profiles button",
            new CssRule
            {
                color = raw("var(--text-muted)"),
                background = raw("var(--surface)"),
                border = raw("0"),
                border_right = px(1) | solid | var("--border-strong"),
                border_radius = raw("0")
            });

        global(".ja-management__profiles button:last-child",
            new CssRule
            {
                border_right = raw("0")
            });

        global(".ja-management__profiles button:hover, .ja-management__profiles button.is-selected",
            new CssRule
            {
                color = raw("var(--accent-strong)"),
                background = raw("var(--accent-soft)")
            });

        global(".ja-management__commands",
            new CssRule
            {
                display = raw("flex"),
                align_items = raw("center"),
                flex_wrap = raw("wrap"),
                gap = raw("8px")
            });

        global(".ja-management__secondary-button",
            new CssRule
            {
                color = raw("var(--text) !important"),
                background = raw("var(--surface) !important"),
                border_color = raw("var(--border-strong) !important")
            });

        global(".ja-management__danger-button",
            new CssRule
            {
                color = raw("var(--danger) !important"),
                background = raw("var(--danger-soft) !important"),
                border_color = raw("color-mix(in srgb, var(--danger) 45%, var(--border)) !important")
            });

        global(".ja-management__secret",
            new CssRule
            {
                display = raw("grid"),
                padding = padding(px(14)),
                margin = raw("20px 20px 0"),
                gap = raw("6px"),
                color = raw("var(--text)"),
                background = raw("var(--warning-soft)"),
                border = px(1) | solid | raw("color-mix(in srgb, var(--warning) 38%, var(--border))"),
                border_radius = raw("4px")
            });

        global(".ja-management__secret code",
            new CssRule
            {
                overflow_wrap = raw("anywhere"),
                font_size = raw("13px")
            });

        global(".ja-management__secret span",
            new CssRule
            {
                color = raw("var(--text-muted)"),
                font_size = raw("12px")
            });

        global(".ja-management button",
            new CssRule
            {
                min_height = raw("34px"),
                padding = padding(px(6), px(12)),
                color = raw("#ffffff"),
                font_size = raw("13px"),
                font_weight = raw("600"),
                background = raw("var(--accent)"),
                border = px(1) | solid | var("--accent"),
                border_radius = raw("4px")
            });

        global(".ja-management button:hover",
            new CssRule
            {
                background = raw("var(--accent-strong)")
            });

        global(".ja-management__table-wrap",
            new CssRule
            {
                width = raw("100%"),
                overflow_x = raw("auto")
            });

        global(".ja-management__table",
            new CssRule
            {
                width = raw("100%"),
                min_width = raw("560px"),
                border_collapse = raw("collapse")
            });

        global(".ja-management__table th, .ja-management__table td",
            new CssRule
            {
                padding = padding(px(13), px(20)),
                text_align = raw("left"),
                vertical_align = raw("middle"),
                border_bottom = px(1) | solid | var("--border")
            });

        global(".ja-management__table th",
            new CssRule
            {
                color = raw("var(--text-muted)"),
                font_size = raw("12px"),
                font_weight = raw("600"),
                background = raw("var(--surface-subtle)")
            });

        global(".ja-management__table td",
            new CssRule
            {
                font_size = raw("13px")
            });

        global(".ja-management__table td strong, .ja-management__table td span",
            new CssRule
            {
                display = raw("block")
            });

        global(".ja-management__table td span",
            new CssRule
            {
                margin_top = raw("3px"),
                color = raw("var(--text-muted)"),
                font_size = raw("12px")
            });

        global(".ja-management__table tr.is-selected td, .ja-management__table tbody tr:hover td",
            new CssRule
            {
                background = raw("var(--accent-soft)")
            });

        global(".ja-management__text-button",
            new CssRule
            {
                padding = important(padding(px(4), px(0))),
                color = raw("var(--accent) !important"),
                background = raw("transparent !important"),
                border = raw("0 !important")
            });

        global(".ja-management__role-list",
            new CssRule
            {
                padding = padding(px(8), px(0))
            });

        global(".ja-management__role-list button",
            new CssRule
            {
                display = raw("flex"),
                align_items = raw("center"),
                justify_content = raw("space-between"),
                width = raw("100%"),
                padding = padding(px(11), px(20)),
                color = raw("var(--text)"),
                text_align = raw("left"),
                background = raw("transparent"),
                border = raw("0"),
                border_left = px(3) | solid | transparent,
                border_radius = raw("0")
            });

        global(".ja-management__role-list button:hover, .ja-management__role-list li.is-selected button",
            new CssRule
            {
                color = raw("var(--accent-strong)"),
                background = raw("var(--accent-soft)"),
                border_left_color = raw("var(--accent)")
            });

        global(".ja-management__role-list small, .ja-management__check small",
            new CssRule
            {
                color = raw("var(--text-muted)"),
                font_size = raw("11px")
            });

        global(".ja-management__grant-list, .ja-management__role-editor",
            new CssRule
            {
                display = raw("grid"),
                padding = padding(px(12), px(20), px(20)),
                gap = raw("8px")
            });

        global(".ja-management__check",
            new CssRule
            {
                display = raw("grid"),
                grid_template_columns = raw("16px minmax(0, 1fr) auto"),
                align_items = raw("center"),
                min_height = raw("40px"),
                padding = padding(px(7), px(9)),
                gap = raw("8px"),
                font_size = raw("13px"),
                background = raw("var(--surface-subtle)"),
                border = px(1) | solid | var("--border"),
                border_radius = raw("4px")
            });

        global(".ja-management__check input",
            new CssRule
            {
                width = raw("16px"),
                min_height = raw("16px"),
                padding = padding(px(0))
            });

        global(".ja-management__grant-list + button, .ja-management__role-editor > button",
            new CssRule
            {
                margin = raw("0 20px 20px")
            });

        Media(".ja-overview__metrics", "(max-width: 1100px)",
            new CssRule
            {
                grid_template_columns = raw("repeat(2, minmax(0, 1fr))")
            });

        Media(".ja-management__split, .ja-management__split--authorization, .ja-management__split--members", "(max-width: 980px)",
            new CssRule
            {
                grid_template_columns = raw("1fr")
            });

        Media(".ja-header-context", "(max-width: 1080px)",
            new CssRule
            {
                flex_wrap = raw("wrap"),
                justify_content = raw("flex-end")
            });

        Media(".ja-overview__grid", "(max-width: 760px)",
            new CssRule
            {
                grid_template_columns = raw("1fr")
            });

        Media(".ja-management__details", "(max-width: 620px)",
            new CssRule
            {
                grid_template_columns = raw("1fr")
            });

        Media(".ja-overview__metrics", "(max-width: 620px)",
            new CssRule
            {
                grid_template_columns = raw("1fr")
            });

        Media(".ja-management__form--inline", "(max-width: 620px)",
            new CssRule
            {
                grid_template_columns = raw("1fr")
            });

        Media(".ja-management__field-grid, .ja-management__options", "(max-width: 620px)",
            new CssRule
            {
                grid_template_columns = raw("1fr")
            });

        Media(".ja-management__commands button", "(max-width: 620px)",
            new CssRule
            {
                flex = raw("1 1 100%"),
                width = raw("100%")
            });

        Media(".ja-organization-picker", "(max-width: 620px)",
            new CssRule
            {
                grid_template_columns = raw("1fr"),
                padding_right = raw("0"),
                border_right = raw("0")
            });

        global(".ja-schedules__status",
            new CssRule
            {
                display = raw("inline-block !important"),
                padding = padding(px(3), px(7)),
                margin_top = raw("0 !important"),
                font_size = raw("11px !important"),
                font_weight = raw("650"),
                border_radius = raw("3px")
            });

        global(".ja-schedules__status.is-enabled",
            new CssRule
            {
                color = raw("var(--success) !important"),
                background = raw("var(--success-soft)")
            });

        global(".ja-schedules__status.is-disabled",
            new CssRule
            {
                color = raw("var(--text-muted) !important"),
                background = raw("var(--surface-strong)")
            });

        global(".ja-schedules__summary",
            new CssRule
            {
                display = raw("grid"),
                padding = padding(px(12)),
                margin = raw("0"),
                gap = raw("10px"),
                background = raw("var(--surface-subtle)"),
                border = px(1) | solid | var("--border"),
                border_radius = raw("4px")
            });

        global(".ja-schedules__summary div",
            new CssRule
            {
                display = raw("grid"),
                grid_template_columns = raw("110px minmax(0, 1fr)"),
                gap = raw("10px")
            });

        global(".ja-schedules__summary dt, .ja-schedules__summary dd",
            new CssRule
            {
                min_width = raw("0"),
                margin = raw("0"),
                font_size = raw("12px")
            });

        global(".ja-schedules__summary dt",
            new CssRule
            {
                color = raw("var(--text-muted)"),
                font_weight = raw("650")
            });

        global(".ja-schedules__summary dd",
            new CssRule
            {
                overflow_wrap = raw("anywhere"),
                color = raw("var(--text)")
            });

        Media("*,   *::before,   *::after", "(prefers-reduced-motion: reduce)",
            new CssRule
            {
                animation_duration = raw("0.01ms !important"),
                scroll_behavior = raw("auto !important"),
                transition_duration = raw("0.01ms !important")
            });

        return true;
    }

    private static void Media(string selector, string prelude, CssRule rule)
        => global(selector, new CssRule
        {
            children = [new(ChildKind.Media, prelude, rule)]
        });
}
