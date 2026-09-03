using ECMAScript;
using ECMAScript.Style;
using static ECMAScript.Style.css;

namespace JazorAdmin;

/// <summary>
/// Shell chrome, appearance drawer and Starter page layout rules. TDesign owns control
/// visuals; this module only supplies the page composition that the templates need.
/// 壳层、外观抽屉与 Starter 页面布局规则；控件视觉由 TDesign 负责，本模块只补页面编排。
/// </summary>
[ECMAScriptModule("./components/starter-styles")]
internal static class StarterStyles
{
    private static readonly bool IsRegistered = Register();

    public static void EnsureLoaded() => _ = IsRegistered;

    private static bool Register()
    {
        global(".ja-tdesign-layout [data-shell-region=\"content\"]",
            new CssRule { background = hex("f3f3f3") });

        global(".ja-tdesign-layout__header",
            new CssRule { background = hex("fff"), gap = px(8) });

        global(".ja-tdesign-sidebar-only",
            new CssRule
            {
                display = flex,
                flex_direction = column,
                width = percent(100),
                height = percent(100),
                background = var("--surface")
            });

        global(".ja-tdesign-sidebar-only__brand",
            new CssRule
            {
                display = flex,
                flex = raw("0 0 64px"),
                align_items = center,
                height = px(64),
                padding = padding(px(0), px(20)),
                gap = px(10),
                color = raw("var(--td-text-color-primary)"),
                font_size = px(16),
                font_weight = 600,
                text_decoration = none
            });

        global(".ja-tdesign-sidebar-only__brand img",
            new CssRule { width = px(28), height = px(28) });

        global(".ja-tdesign-sidebar-only__menu",
            new CssRule { min_height = px(0), overflow_y = auto });

        global(".ja-footer",
            new CssRule
            {
                padding = padding(px(0), px(24), px(80)),
                color = raw("var(--td-text-color-placeholder)"),
                font_size = px(12),
                line_height = px(20),
                text_align = center
            });

        global(".setting-container",
            new CssRule
            {
                padding_bottom = px(100)
            });

        global(".setting-group-title",
            new CssRule
            {
                margin = margin(px(32), px(0), px(24)),
                color = raw("var(--td-text-color-primary)"),
                font_family = raw("'PingFang SC', var(--td-font-family)"),
                font_size = px(14),
                font_weight = 500,
                line_height = px(22)
            });

        global(".setting-layout-color-group",
            new CssRule
            {
                display = inline_flex,
                align_items = center,
                justify_content = center,
                padding = important(px(6)),
                border = important(px(2) | solid | transparent),
                border_radius = important(percent(50))
            });

        global(".setting-layout-color-group > .t-radio-button__label",
            new CssRule { display = inline_flex });

        global(".setting-color-preview",
            new CssRule { display = block, width = px(16), height = px(16), border_radius = percent(50) });

        global(".setting-drawer-container .setting-container",
            new CssRule { padding_bottom = px(100) });

        global(".setting-drawer-container .t-radio-group.t-size-m",
            new CssRule
            {
                display = flex,
                align_items = center,
                justify_content = space_between,
                width = percent(100),
                min_height = px(32)
            });

        global(".setting-drawer-container .t-radio-group.t-size-m.side-mode-radio",
            new CssRule { justify_content = end });

        global(".setting-drawer-container .t-radio-group.t-size-m .t-radio-button",
            new CssRule { height = raw("auto") });

        global(".setting-drawer-container .setting-layout-drawer",
            new CssRule
            {
                display = flex,
                flex_direction = column,
                align_items = center,
                margin_bottom = px(16)
            });

        global(".setting-drawer-container .setting-layout-drawer .t-radio-button",
            new CssRule
            {
                display = inline_flex,
                max_height = px(78),
                padding = px(8),
                border = px(2) | solid | raw("var(--td-component-border)"),
                border_radius = raw("var(--td-radius-default)")
            });

        global(".setting-drawer-container .setting-layout-drawer .t-radio-button > .t-radio-button__label",
            new CssRule { display = inline_flex });

        global(".setting-drawer-container .setting-layout-drawer p",
            new CssRule { margin_top = px(8), text_align = center });

        global(".setting-drawer-container .thumbnail-layout",
            new CssRule { display = inline_block, width = px(88), height = px(48) });

        global(".setting-drawer-container .setting-layout-drawer .t-is-checked",
            new CssRule { border = important(px(2) | solid | raw("var(--td-brand-color)")) });

        global(".setting-drawer-container .t-form__controls-content",
            new CssRule { justify_content = end });

        global(".setting-info",
            new CssRule
            {
                position = absolute,
                bottom = px(0),
                left = px(0),
                width = percent(100),
                padding = px(24),
                color = raw("var(--td-text-color-placeholder)"),
                background = raw("var(--td-bg-color-container)"),
                font_size = px(12),
                line_height = px(20),
                text_align = center
            });

        global(".setting-info p",
            new CssRule { margin = px(0) });

        global(".ja-starter-setting-trigger",
            new CssRule
            {
                position = fixed_position,
                left = percent(50),
                bottom = px(24),
                z_index = 40,
                width = px(40),
                height = px(40),
                min_width = px(40),
                padding = px(0),
                transform = raw("translateX(-50%)"),
                box_shadow = shadows(new CssShadow(px(0), px(6), Blur: px(16), Color: rgba(0, 0, 0, 0.16)))
            });

        global(".ja-starter-operations",
            new CssRule
            {
                display = flex,
                align_items = center,
                gap = px(4)
            });

        global(".ja-starter-search, .ja-starter-organization, .ja-starter-select",
            new CssRule
            {
                display = flex,
                align_items = center,
                min_height = px(32),
                gap = px(6),
                color = hex("4b5b76")
            });

        global(".ja-starter-search",
            new CssRule
            {
                position = relative,
                width = px(220),
                padding = padding(px(0), px(10)),
                background = hex("f3f3f3"),
                border_radius = px(3)
            });

        // Navigation-search dropdown. TDesign 色彩变量随主题自动切换，无需单独的 dark 覆盖。
        global(".ja-starter-search__panel",
            new CssRule
            {
                position = absolute,
                top = raw("calc(100% + 4px)"),
                left = px(0),
                right = px(0),
                z_index = 1000,
                display = flex,
                flex_direction = column,
                gap = px(2),
                max_height = px(320),
                overflow_y = auto,
                padding = px(4),
                text_align = left,
                background = raw("var(--td-bg-color-container)"),
                border = px(1) | solid | raw("var(--td-component-border)"),
                border_radius = raw("var(--td-radius-default)"),
                box_shadow = raw("var(--td-shadow-1)")
            });
        global(".ja-starter-search__empty",
            new CssRule
            {
                padding = padding(px(8), px(10)),
                color = raw("var(--td-text-color-placeholder)"),
                font_size = px(12),
                line_height = px(20)
            });
        global(".ja-starter-search__item",
            new CssRule
            {
                display = flex,
                align_items = center,
                justify_content = space_between,
                gap = px(8),
                padding = padding(px(6), px(10)),
                color = raw("var(--td-text-color-primary)"),
                text_align = left,
                background = transparent,
                border = none,
                border_radius = px(3)
            });
        global(".ja-starter-search__item:hover",
            new CssRule { background = raw("var(--td-bg-color-container-hover)") });
        global(".ja-starter-search__item small",
            new CssRule
            {
                overflow = hidden,
                color = raw("var(--td-text-color-placeholder)"),
                font_size = px(12),
                text_overflow = ellipsis,
                white_space = nowrap
            });

        // Header icon anchor that matches TDesign text square buttons for external links.
        global(".ja-starter-link-button",
            new CssRule
            {
                display = inline_flex,
                align_items = center,
                justify_content = center,
                width = px(32),
                height = px(32),
                color = raw("var(--td-text-color-primary)"),
                text_decoration = none,
                border_radius = px(3)
            });
        global(".ja-starter-link-button:hover",
            new CssRule { background = raw("var(--td-bg-color-container-hover)") });

        // Notification bell panel. 与搜索面板一致：TDesign 变量随主题切换。
        global(".ja-starter-notifications",
            new CssRule { position = relative });
        global(".ja-starter-notifications__badge",
            new CssRule
            {
                position = absolute,
                top = px(1),
                right = px(1),
                min_width = px(16),
                height = px(16),
                padding = padding(px(0), px(4)),
                color = hex("ffffff"),
                font_size = px(11),
                line_height = px(16),
                text_align = center,
                background = raw("var(--td-error-color)"),
                border_radius = raw("8px")
            });
        global(".ja-starter-notifications__panel",
            new CssRule
            {
                position = absolute,
                top = raw("calc(100% + 8px)"),
                right = px(0),
                z_index = 1000,
                width = px(320),
                overflow = hidden,
                background = raw("var(--td-bg-color-container)"),
                border = px(1) | solid | raw("var(--td-component-border)"),
                border_radius = raw("var(--td-radius-default)"),
                box_shadow = raw("var(--td-shadow-1)")
            });
        global(".ja-starter-notifications__panel header",
            new CssRule
            {
                padding = padding(px(10), px(12)),
                color = raw("var(--td-text-color-primary)"),
                font_weight = 600,
                border_bottom = px(1) | solid | raw("var(--td-component-stroke)")
            });
        global(".ja-starter-notifications__empty",
            new CssRule
            {
                margin = px(0),
                padding = padding(px(14), px(12)),
                color = raw("var(--td-text-color-placeholder)"),
                font_size = px(12),
                line_height = px(20)
            });
        global(".ja-starter-notifications__panel ul",
            new CssRule
            {
                max_height = px(300),
                overflow_y = auto,
                margin = px(0),
                padding = px(4),
                list_style = none
            });
        global(".ja-starter-notifications__panel li",
            new CssRule
            {
                display = flex,
                flex_direction = column,
                gap = px(2),
                padding = padding(px(8), px(10)),
                border_radius = px(3)
            });
        global(".ja-starter-notifications__panel li:hover",
            new CssRule { background = raw("var(--td-bg-color-container-hover)") });
        global(".ja-starter-notifications__panel li strong",
            new CssRule
            {
                color = raw("var(--td-text-color-primary)"),
                font_size = px(13),
                font_weight = 600
            });
        global(".ja-starter-notifications__panel li small",
            new CssRule
            {
                overflow = hidden,
                color = raw("var(--td-text-color-placeholder)"),
                font_size = px(12),
                line_height = px(18),
                text_overflow = ellipsis,
                white_space = nowrap
            });

        // Help dialog content.
        global(".ja-help section + section",
            new CssRule { margin_top = px(18) });
        global(".ja-help h3",
            new CssRule
            {
                margin = px(0),
                color = raw("var(--td-text-color-primary)"),
                font_size = px(14)
            });
        global(".ja-help ul",
            new CssRule
            {
                margin = margin(px(8), px(0), px(0)),
                padding_left = px(18),
                color = raw("var(--td-text-color-secondary)"),
                font_size = px(13),
                line_height = px(22)
            });
        global(".ja-help a",
            new CssRule { color = raw("var(--td-brand-color)") });

        global(".ja-starter-search input, .ja-starter-organization select, .ja-starter-select select",
            new CssRule
            {
                width = percent(100),
                min_width = px(0),
                color = hex("1d2129"),
                background = transparent,
                border = none,
                outline = none
            });

        global(".ja-starter-organization",
            new CssRule
            {
                max_width = px(220),
                padding = padding(px(0), px(8)),
                border_right = px(1) | solid | hex("e7e7e7")
            });

        global(".ja-starter-organization span",
            new CssRule { font_size = px(13), white_space = nowrap });

        global(".ja-starter-user",
            new CssRule
            {
                display = flex,
                align_items = center,
                min_width = px(0),
                gap = px(8),
                margin_left = px(10)
            });

        global(".ja-starter-user__avatar",
            new CssRule
            {
                display = grid,
                flex = flex_box(0, 0, px(32)),
                width = px(32),
                height = px(32),
                color = hex("fff"),
                background = hex("0052d9"),
                border_radius = percent(50),
                place_items = center,
                font_size = px(13),
                font_weight = 600
            });

        // Starter templates share a compact, responsive work surface. Keep these rules here
        // so the page component stays focused on typed TDesign composition and state.
        // Starter 模板共享紧凑且响应式的工作台；布局规则集中在样式模块，页面只负责组件组合。
        global(".ja-starter-page",
            new CssRule
            {
                display = grid,
                gap = px(16),
                min_width = px(0)
            });

        global(".ja-starter-page > .t-card",
            new CssRule { min_width = px(0) });

        global(".ja-starter-metrics",
            new CssRule
            {
                display = grid,
                grid_template_columns = tracks(repeat(4, min_max(px(0), fr(1)))),
                gap = px(16)
            });

        global(".ja-starter-metric-value",
            new CssRule
            {
                display = block,
                margin_top = px(12),
                color = raw("var(--td-text-color-primary)"),
                font_size = px(28),
                font_weight = 600,
                line_height = raw("1.1")
            });

        global(".ja-starter-bars",
            new CssRule
            {
                display = grid,
                align_content = end,
                gap = px(12),
                min_height = px(180),
                padding = padding(px(12), px(0))
            });

        global(".ja-starter-bars > .t-progress",
            new CssRule { width = percent(100) });

        global(".ja-starter-distribution",
            new CssRule
            {
                display = flex,
                flex_wrap = wrap,
                align_items = center,
                gap = px(8),
                min_height = px(180),
                align_content = center
            });

        global(".ja-starter-donut",
            new CssRule
            {
                display = grid,
                grid_template_columns = tracks(min_max(px(120), px(160)), min_max(px(0), fr(1))),
                align_items = center,
                gap = px(14),
                min_height = px(180)
            });

        global(".ja-starter-donut > strong",
            new CssRule
            {
                display = grid,
                width = px(126),
                height = px(126),
                place_items = center,
                color = raw("var(--td-brand-color)"),
                font_size = px(28),
                border = px(14) | solid | raw("var(--td-brand-color-light)"),
                border_radius = percent(50)
            });

        global(".ja-starter-donut > strong small",
            new CssRule { font_size = px(13), font_weight = 400 });

        global(".ja-starter-toolbar",
            new CssRule
            {
                display = flex,
                flex_wrap = wrap,
                align_items = center,
                justify_content = space_between,
                gap = px(8)
            });

        global(".ja-starter-toolbar > .t-input",
            new CssRule { width = px(280), max_width = percent(100) });

        global(".ja-starter-row-actions",
            new CssRule
            {
                display = flex,
                justify_content = flex_end,
                gap = px(8),
                margin_top = px(10)
            });

        global(".ja-starter-card-grid",
            new CssRule
            {
                display = grid,
                grid_template_columns = tracks(repeat(3, min_max(px(0), fr(1)))),
                gap = px(16)
            });

        global(".ja-starter-card-grid .t-card",
            new CssRule { min_width = px(0), height = percent(100) });

        global(".ja-starter-card-grid .t-card__body",
            new CssRule { display = grid, align_content = start, gap = px(8) });

        global(".ja-starter-card-grid p",
            new CssRule { margin = px(0), color = raw("var(--td-text-color-secondary)"), font_size = px(13) });

        global(".ja-starter-tree-page",
            new CssRule
            {
                grid_template_columns = tracks(min_max(px(220), px(260)), min_max(px(0), fr(1))),
                align_items = start
            });

        global(".ja-starter-tree-page .ja-panel",
            new CssRule { min_width = px(0) });

        global(".ja-starter-notices",
            new CssRule { display = grid, gap = px(0) });

        global(".ja-starter-notice",
            new CssRule
            {
                display = grid,
                grid_template_columns = tracks(px(78), min_max(px(0), fr(1)), auto, auto),
                align_items = center,
                gap = px(10),
                padding = padding(px(14), px(0)),
                border_bottom = px(1) | solid | raw("var(--td-component-stroke)")
            });

        global(".ja-starter-notice p",
            new CssRule { margin = margin(px(4), px(0), px(0)), color = raw("var(--td-text-color-secondary)"), font_size = px(12) });

        global(".ja-starter-notice small",
            new CssRule { color = raw("var(--td-text-color-placeholder)"), font_size = px(12) });

        global(".ja-starter-result",
            new CssRule
            {
                display = flex,
                flex_direction = column,
                align_items = center,
                justify_content = center,
                min_height = raw("min(620px, calc(100vh - 240px))"),
                padding = px(32),
                text_align = center
            });

        global(".ja-starter-result-art",
            new CssRule { width = px(200), max_width = percent(100), height = px(140), object_fit = contain });

        global(".ja-starter-result > p",
            new CssRule { max_width = px(520), margin = margin(px(12), px(0), px(20)), color = raw("var(--td-text-color-secondary)"), line_height = raw("1.6") });

        global(".ja-starter-login",
            new CssRule
            {
                display = grid,
                place_items = center,
                min_height = raw("min(680px, calc(100vh - 240px))")
            });

        global(".ja-starter-login > .t-card",
            new CssRule { width = min(percent(100), px(440)) });

        global(".ja-starter-login .t-form",
            new CssRule { margin_top = px(16) });

        global(".ja-starter-team",
            new CssRule
            {
                display = grid,
                gap = px(12),
                margin = px(0),
                padding = px(0),
                list_style = none
            });

        global(".ja-starter-page .t-descriptions, .ja-starter-page .t-table",
            new CssRule { min_width = px(0) });

        Media(".ja-starter-metrics", "(max-width: 980px)",
            new CssRule { grid_template_columns = tracks(repeat(2, min_max(px(0), fr(1)))) });
        Media(".ja-starter-card-grid", "(max-width: 900px)",
            new CssRule { grid_template_columns = tracks(repeat(2, min_max(px(0), fr(1)))) });
        Media(".ja-starter-tree-page", "(max-width: 760px)",
            new CssRule { grid_template_columns = tracks(min_max(px(0), fr(1))) });
        Media(".ja-starter-notice", "(max-width: 620px)",
            new CssRule { grid_template_columns = tracks(min_max(px(0), fr(1)), auto), align_items = start });
        Media(".ja-starter-notice > .t-tag", "(max-width: 620px)",
            new CssRule { grid_column = raw("1 / -1") });
        Media(".ja-starter-notice > .t-button", "(max-width: 620px)",
            new CssRule { grid_row = raw("2") });
        Media(".ja-starter-donut", "(max-width: 520px)",
            new CssRule { grid_template_columns = tracks(min_max(px(0), fr(1))), justify_items = center });
        Media(".ja-starter-toolbar > .t-input", "(max-width: 520px)",
            new CssRule { width = percent(100) });
        Media(".ja-starter-metrics, .ja-starter-card-grid", "(max-width: 520px)",
            new CssRule { grid_template_columns = tracks(min_max(px(0), fr(1))) });

        // Dashboard (M2): KPI cards, chart hosts and service tiles. Charts need a definite
        // height because Responsive only resolves width upstream.
        // 仪表盘：KPI 卡、图表容器与服务块。图表容器必须给确定高度。
        global(".ja-dash__metrics",
            new CssRule
            {
                display = grid,
                grid_template_columns = tracks(repeat(4, min_max(px(0), fr(1)))),
                gap = px(16)
            });

        global(".ja-metric",
            new CssRule
            {
                display = grid,
                align_content = start,
                padding = px(12),
                background = hex("fff"),
                border_radius = px(3)
            });

        global(".ja-metric .vue-data-ui-kpi",
            new CssRule { min_height = px(140) });

        global(".ja-metric__label",
            new CssRule { display = flex, align_items = center, justify_content = space_between, color = hex("4b5b76"), font_size = px(14) });

        global(".ja-metric__code",
            new CssRule { margin_top = px(18), color = hex("1d2129"), font_size = px(24), font_weight = px(600), overflow_wrap = keyword("anywhere") });

        global(".ja-metric__footer",
            new CssRule
            {
                display = flex,
                min_width = px(0),
                gap = px(8),
                color = hex("86909c"),
                font_size = px(12)
            });

        global(".ja-metric__footer span:last-child",
            new CssRule { overflow = hidden, text_overflow = ellipsis, white_space = nowrap });

        global(".ja-dash__analysis",
            new CssRule
            {
                display = grid,
                grid_template_columns = tracks(min_max(px(0), fr(1.65)), min_max(px(280), fr(0.75))),
                gap = px(16)
            });

        global(".ja-chart-host",
            new CssRule { display = grid, height = px(260), margin_top = px(8) });

        // Portal is an unframed section; only each repeated application is a real TDesign card.
        // 门户本身不做浮层卡片，只有重复的应用入口使用真实 TDesign 卡片。
        global(".ja-portal",
            new CssRule
            {
                display = grid,
                gap = px(16),
                padding = padding(px(4), px(0))
            });

        global(".ja-portal__grid",
            new CssRule
            {
                display = grid,
                grid_template_columns = tracks(repeat(3, min_max(px(0), fr(1)))),
                gap = px(16)
            });

        global(".ja-portal__card",
            new CssRule { min_width = px(0) });

        global(".ja-portal__body",
            new CssRule
            {
                display = grid,
                grid_template_columns = tracks(px(24), min_max(px(0), fr(1))),
                align_items = center,
                gap = px(10),
                color = raw("var(--td-text-color-secondary)")
            });

        global(".ja-portal__body > .vu-icon, .ja-portal__body > svg",
            new CssRule { color = raw("var(--td-brand-color)") });

        global(".ja-portal__body > span",
            new CssRule { min_width = px(0), font_size = px(12), line_height = px(20) });

        global(".ja-portal__launch",
            new CssRule
            {
                grid_column = raw("1 / -1"),
                display = inline_flex,
                align_items = center,
                justify_content = center,
                width = raw("fit-content"),
                gap = px(6),
                padding = padding(px(4), px(0)),
                color = raw("var(--td-brand-color)"),
                font_size = px(13),
                font_weight = 600,
                text_decoration = none
            });

        global(".ja-portal__launch:hover",
            new CssRule { text_decoration = raw("underline") });

        global(".ja-portal__launch:focus-visible",
            new CssRule { outline = px(2) | solid | raw("var(--td-brand-color)"), outline_offset = px(2) });

        global(".ja-dash__rankings",
            new CssRule { display = grid, grid_template_columns = tracks(repeat(2, min_max(px(0), fr(1)))), gap = px(16) });

        global(".ja-dash__output",
            new CssRule { display = grid, grid_template_columns = tracks(repeat(4, min_max(px(0), fr(1)))), gap = px(16) });

        global(".ja-dash__output article",
            new CssRule { display = grid, grid_template_columns = tracks(px(24), min_max(px(0), fr(1))), align_items = center, padding = px(14), gap = px(10), background = hex("f7f8fa"), border_radius = px(3) });

        global(".ja-dash__output .vu-icon, .ja-dash__output svg",
            new CssRule { color = hex("0052d9") });

        global(".ja-dash__output strong, .ja-dash__output span",
            new CssRule { display = block });

        global(".ja-dash__output strong",
            new CssRule { color = hex("1d2129"), font_size = px(13) });

        global(".ja-dash__output span",
            new CssRule { margin_top = px(3), color = hex("86909c"), font_size = px(11) });

        global(".ja-dash__output em",
            new CssRule { grid_column = grid_line(2), color = hex("00a870"), font_size = px(11), font_style = normal });

        // TDesign consumes inherited design tokens. Product page shells use the same tokens so
        // switching the header control changes both library controls and checked-in templates.
        global(".ja-application--dark .ja-tdesign-layout [data-shell-region=\"content\"]",
            new CssRule { background = var("--td-bg-color-page") });
        global(".ja-application--dark .ja-tdesign-layout__header",
            new CssRule { background = var("--td-bg-color-container") });
        global(".ja-application--dark .ja-starter-search",
            new CssRule { background = var("--td-bg-color-secondarycontainer") });
        global(".ja-application--dark .ja-starter-search input, .ja-application--dark .ja-starter-organization select, .ja-application--dark .ja-starter-select select",
            new CssRule { color = var("--td-text-color-primary") });
        Media(".ja-starter-search", "(max-width: 1120px)", new CssRule { width = px(150) });
        Media(".ja-starter-organization span, .ja-starter-organization", "(max-width: 900px)", new CssRule { display = none });
        Media(".ja-starter-search", "(max-width: 620px)", new CssRule { display = none });
        Media(".ja-starter-user", "(max-width: 620px)", new CssRule { margin_left = px(2) });
        Media(".ja-starter-user__trigger .t-button__text, .ja-starter-user__trigger .t-button__suffix", "(max-width: 620px)", new CssRule { display = none });
        Media(".ja-portal__grid", "(max-width: 1080px)", new CssRule { grid_template_columns = tracks(repeat(2, min_max(px(0), fr(1)))) });
        Media(".ja-portal__grid", "(max-width: 620px)", new CssRule { grid_template_columns = tracks(min_max(px(0), fr(1))) });
        return true;
    }

    private static void Media(string selector, string prelude, CssRule rule)
        => global(selector, new CssRule { children = [new(ChildKind.Media, prelude, rule)] });
}
