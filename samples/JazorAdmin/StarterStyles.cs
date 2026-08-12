using ECMAScript;
using ECMAScript.Style;
using static ECMAScript.Style.css;

namespace JazorAdmin;

/// <summary>
/// Starter-page rules live apart from the product baseline so template reproduction remains
/// reviewable. All selectors stay under the `ja-` product prefix.
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

        global(".tdesign-starter-footer",
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
                width = px(220),
                padding = padding(px(0), px(10)),
                background = hex("f3f3f3"),
                border_radius = px(3)
            });

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

        global(".ja-starter-select",
            new CssRule { width = px(84), padding = padding(px(0), px(4)) });

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

        global(".ja-starter-user__name",
            new CssRule
            {
                max_width = px(140),
                overflow = hidden,
                text_overflow = ellipsis,
                white_space = nowrap,
                font_size = px(14)
            });

        global(".ja-starter-user__command",
            new CssRule
            {
                display = inline_flex,
                align_items = center,
                gap = px(4),
                padding = padding(px(5), px(6)),
                color = hex("4b5b76"),
                background = transparent,
                border = none,
                border_radius = px(3),
                font_size = px(13)
            });

        global(".ja-starter-user__command:hover",
            new CssRule { color = hex("0052d9"), background = hex("f2f3ff") });

        global(".ja-starter-dashboard",
            new CssRule
            {
                display = grid,
                gap = px(16),
                animation = animation(ident("ja-dashboard-enter"), ms(220), ease_out)
            });

        global(".ja-starter-dashboard__metrics",
            new CssRule
            {
                display = grid,
                grid_template_columns = tracks(repeat(4, min_max(px(0), fr(1)))),
                gap = px(16)
            });

        global(".ja-starter-metric, .ja-starter-card",
            new CssRule
            {
                background = hex("fff"),
                border = important(none),
                border_radius = important(px(3)),
                box_shadow = important(none)
            });

        global(".ja-starter-metric .t-card__body, .ja-starter-card .t-card__body",
            new CssRule { padding = important(px(20)) });

        global(".ja-starter-metric__label, .ja-starter-card__header",
            new CssRule
            {
                display = flex,
                align_items = flex_start,
                justify_content = space_between,
                gap = px(12)
            });

        global(".ja-starter-metric__label",
            new CssRule { color = hex("4b5b76"), font_size = px(14) });

        global(".ja-starter-metric__label .t-icon",
            new CssRule { color = hex("0052d9") });

        global(".ja-starter-metric strong",
            new CssRule
            {
                display = block,
                margin_top = px(18),
                color = hex("1d2129"),
                font_size = px(32),
                font_weight = 600,
                line_height = 1
            });

        global(".ja-starter-metric strong.is-code",
            new CssRule { font_size = px(24), overflow_wrap = keyword("anywhere") });

        global(".ja-starter-metric__footer",
            new CssRule
            {
                display = flex,
                min_width = px(0),
                margin_top = px(16),
                gap = px(8),
                color = hex("86909c"),
                font_size = px(12)
            });

        global(".ja-starter-metric__footer span:last-child",
            new CssRule { overflow = hidden, text_overflow = ellipsis, white_space = nowrap });

        global(".ja-starter-metric__footer .is-up",
            new CssRule { color = hex("00a870"), font_weight = 600 });

        global(".ja-starter-metric__footer .is-stable",
            new CssRule { color = hex("86909c"), font_weight = 600 });

        global(".ja-starter-dashboard__analysis",
            new CssRule
            {
                display = grid,
                grid_template_columns = tracks(min_max(px(0), fr(1.65)), min_max(px(280), fr(0.75))),
                gap = px(16)
            });

        global(".ja-starter-card__header h2",
            new CssRule { margin = margin(px(0)), color = hex("1d2129"), font_size = px(18), font_weight = 500 });

        global(".ja-starter-card__header p",
            new CssRule { margin = margin(px(6), px(0), px(0)), color = hex("86909c"), font_size = px(13) });

        global(".ja-starter-segments",
            new CssRule { display = flex, padding = px(2), background = hex("f3f3f3"), border_radius = px(3) });

        global(".ja-starter-segments button, .ja-starter-output button",
            new CssRule { padding = padding(px(5), px(9)), color = hex("4b5b76"), background = transparent, border = none, border_radius = px(2), font_size = px(12) });

        global(".ja-starter-segments button.is-selected",
            new CssRule { color = hex("0052d9"), background = hex("fff"), box_shadow = shadows(new CssShadow(px(0), px(1), Blur: px(2), Color: rgba(0, 0, 0, 0.08))) });

        global(".ja-starter-chart",
            new CssRule { display = grid, grid_template_columns = tracks(px(30), min_max(px(0), fr(1))), height = px(226), margin_top = px(16) });

        global(".ja-starter-chart__axis",
            new CssRule { display = flex, flex_direction = column, justify_content = space_between, padding_bottom = px(22), color = hex("86909c"), font_size = px(11) });

        global(".ja-starter-chart__plot",
            new CssRule
            {
                display = grid,
                grid_template_columns = tracks(repeat(7, min_max(px(0), fr(1)))),
                align_items = end,
                column_gap = px(14),
                padding = padding(px(0), px(8), px(22)),
                background_image = linear_gradient(stop(hex("e7e7e7"), px(1)), stop(transparent, px(1))),
                background_size = background_size(percent(100), percent(25))
            });

        global(".ja-starter-chart__plot > div",
            new CssRule { position = relative, display = flex, align_items = end, justify_content = center, height = percent(100), gap = px(4) });

        global(".ja-starter-chart__plot i, .ja-starter-chart__plot b",
            new CssRule { display = block, width = px(11), border_radius = radius(px(2), px(2), px(0), px(0)) });

        global(".ja-starter-chart__plot i", new CssRule { background = hex("8ec5ff") });
        global(".ja-starter-chart__plot b", new CssRule { background = hex("0052d9") });
        global(".ja-starter-chart__plot span", new CssRule { position = absolute, bottom = px(-21), color = hex("86909c"), font_size = px(11) });

        global(".ja-starter-chart__legend",
            new CssRule { display = flex, margin_left = px(30), gap = px(20), color = hex("4b5b76"), font_size = px(12) });

        global(".ja-starter-chart__legend span, .ja-starter-distribution dt",
            new CssRule { display = inline_flex, align_items = center, gap = px(6) });

        global(".ja-starter-chart__legend i, .ja-starter-distribution dl i",
            new CssRule { display = block, width = px(8), height = px(8), border_radius = percent(50) });
        global(".ja-starter-chart__legend .is-primary, .ja-starter-distribution .is-brand", new CssRule { background = hex("0052d9") });
        global(".ja-starter-chart__legend .is-secondary, .ja-starter-distribution .is-cyan", new CssRule { background = hex("00a6a6") });
        global(".ja-starter-distribution .is-gray", new CssRule { background = hex("c9cdd4") });

        global(".ja-starter-distribution .t-card__body",
            new CssRule { display = flex, flex_direction = column });

        global(".ja-starter-donut",
            new CssRule { display = grid, width = px(164), height = px(164), margin = margin(px(20), auto), background = conic_gradient(stop(hex("0052d9"), px(0), percent(45)), stop(hex("00a6a6"), percent(45), percent(78)), stop(hex("c9cdd4"), percent(78), percent(100))), border_radius = percent(50), place_items = center });

        global(".ja-starter-donut::before",
            new CssRule { grid_area = grid_line(1, 1), width = px(118), height = px(118), background = hex("fff"), border_radius = percent(50), content = str(string.Empty) });

        global(".ja-starter-donut span",
            new CssRule { grid_area = grid_line(1, 1), position = relative, z_index = 1, color = hex("1d2129"), font_size = px(28), font_weight = 600, text_align = center });
        global(".ja-starter-donut small", new CssRule { display = block, margin_top = px(2), color = hex("86909c"), font_size = px(11), font_weight = 400 });

        global(".ja-starter-distribution dl", new CssRule { display = grid, margin = margin(auto, px(0), px(0)), gap = px(10) });
        global(".ja-starter-distribution dl div", new CssRule { display = flex, align_items = center, justify_content = space_between, color = hex("4b5b76"), font_size = px(13) });
        global(".ja-starter-distribution dd", new CssRule { margin = margin(px(0)), color = hex("1d2129"), font_weight = 600 });

        global(".ja-starter-dashboard__rankings", new CssRule { display = grid, grid_template_columns = tracks(repeat(2, min_max(px(0), fr(1)))), gap = px(16) });
        global(".ja-starter-rank table", new CssRule { width = percent(100), margin_top = px(18), border_collapse = keyword("collapse"), text_align = left });
        global(".ja-starter-rank th, .ja-starter-rank td", new CssRule { padding = padding(px(10), px(6)), border_bottom = px(1) | solid | hex("f0f0f0"), font_size = px(13) });
        global(".ja-starter-rank th", new CssRule { color = hex("86909c"), font_weight = 400 });
        global(".ja-starter-rank td", new CssRule { color = hex("4b5b76") });
        global(".ja-starter-rank code", new CssRule { color = hex("0052d9"), font_family = font_family(generic_font("ui-monospace"), generic_font("SFMono-Regular"), font("Consolas"), generic_font("monospace")), font_size = px(12) });
        global(".ja-starter-rank__index", new CssRule { display = grid, width = px(22), height = px(22), color = hex("4b5b76"), background = hex("f3f3f3"), border_radius = percent(50), place_items = center, font_size = px(12) });
        global(".ja-starter-status", new CssRule { display = inline_block, padding = padding(px(2), px(6)), border_radius = px(2), font_size = px(11) });
        global(".ja-starter-status.is-success", new CssRule { color = hex("00a870"), background = hex("e8ffea") });
        global(".ja-starter-empty", new CssRule { color = hex("86909c"), text_align = center });

        global(".ja-starter-output__grid", new CssRule { display = grid, grid_template_columns = tracks(repeat(4, min_max(px(0), fr(1)))), margin_top = px(20), gap = px(16) });
        global(".ja-starter-output__grid article", new CssRule { display = grid, grid_template_columns = tracks(px(24), min_max(px(0), fr(1))), align_items = center, padding = px(14), gap = px(10), background = hex("f7f8fa"), border_radius = px(3) });
        global(".ja-starter-output__grid .t-icon", new CssRule { color = hex("0052d9") });
        global(".ja-starter-output__grid strong, .ja-starter-output__grid span", new CssRule { display = block });
        global(".ja-starter-output__grid strong", new CssRule { color = hex("1d2129"), font_size = px(13) });
        global(".ja-starter-output__grid span", new CssRule { margin_top = px(3), color = hex("86909c"), font_size = px(11) });
        global(".ja-starter-output__grid em", new CssRule { grid_column = 2, color = hex("00a870"), font_size = px(11), font_style = normal });

        global(".ja-starter-page", new CssRule { min_width = px(0) });

        global(".ja-starter-page > section, .ja-starter-sheet, .ja-starter-user-page > aside > section",
            new CssRule { padding = px(20), background = hex("fff"), border_radius = px(3) });

        global(".ja-starter-page h2", new CssRule { margin = margin(px(0)), color = hex("1d2129"), font_size = px(18), font_weight = 500 });
        global(".ja-starter-page header", new CssRule { display = flex, align_items = center, justify_content = space_between, gap = px(12) });
        global(".ja-starter-page button, .ja-starter-primary, .ja-starter-filter select", new CssRule { padding = padding(px(7), px(12)), color = hex("4b5b76"), background = hex("fff"), border = px(1) | solid | hex("dcdcdc"), border_radius = px(3), font_size = px(13) });
        global(".ja-starter-primary", new CssRule
        {
            display = inline_flex,
            align_items = center,
            gap = px(4),
            color = important(hex("fff")),
            background = important(hex("0052d9")),
            border_color = important(hex("0052d9"))
        });
        global(".ja-starter-page button:hover", new CssRule { color = hex("0052d9"), border_color = hex("0052d9") });
        global(".ja-starter-primary:hover", new CssRule
        {
            color = important(hex("fff")),
            background = important(hex("003cab"))
        });
        global(".ja-starter-page button.is-plain", new CssRule { background = hex("fff") });
        global(".ja-starter-inline-message", new CssRule { margin = margin(px(12), px(0)), padding = padding(px(10), px(12)), color = hex("00a870"), background = hex("e8ffea"), border_radius = px(3), font_size = px(13) });

        global(".ja-starter-report", new CssRule { display = grid, gap = px(16) });
        global(".ja-starter-report__hero", new CssRule { display = flex, align_items = center, justify_content = space_between, padding = px(28), color = hex("fff"), background = hex("0052d9"), border_radius = px(3) });
        global(".ja-starter-report__hero h2", new CssRule { margin = margin(px(5), px(0)), color = hex("fff"), font_size = px(24) });
        global(".ja-starter-report__hero p", new CssRule { margin = margin(px(0)), color = rgba(255, 255, 255, 0.78), font_size = px(13) });
        global(".ja-starter-report__hero button", new CssRule { color = hex("0052d9"), border_color = hex("fff") });
        global(".ja-starter-report__metrics", new CssRule { display = grid, grid_template_columns = tracks(repeat(4, min_max(px(0), fr(1)))), gap = px(16) });
        global(".ja-starter-report__metrics article", new CssRule { position = relative, padding = px(18), background = hex("fff"), border_radius = px(3) });
        global(".ja-starter-report__metrics span", new CssRule { color = hex("86909c"), font_size = px(13) });
        global(".ja-starter-report__metrics strong", new CssRule { display = block, margin_top = px(12), color = hex("1d2129"), font_size = px(28), font_weight = 600 });
        global(".ja-starter-report__metrics em", new CssRule { position = absolute, right = px(18), bottom = px(18), color = hex("00a870"), font_size = px(12), font_style = normal });
        global(".ja-starter-report__metrics em.is-down", new CssRule { color = hex("d54941") });
        global(".ja-starter-report__charts", new CssRule { display = grid, grid_template_columns = tracks(repeat(2, min_max(px(0), fr(1)))), gap = px(16) });
        global(".ja-starter-report__charts > section", new CssRule { min_height = px(298) });
        global(".ja-starter-line-chart", new CssRule { display = grid, grid_template_columns = tracks(repeat(7, min_max(px(0), fr(1)))), align_items = end, height = px(194), margin_top = px(26), gap = px(10), background_image = linear_gradient(stop(hex("e7e7e7"), px(1)), stop(transparent, px(1))), background_size = background_size(percent(100), percent(25)) });
        global(".ja-starter-line-chart i", new CssRule { display = block, height = percent(45), background = hex("0052d9"), border_radius = radius(px(3), px(3), px(0), px(0)) });
        global(".ja-starter-line-chart i:nth-child(2n)", new CssRule { height = percent(72), background = hex("8ec5ff") });
        global(".ja-starter-line-chart i:nth-child(3n)", new CssRule { height = percent(84), background = hex("0052d9") });
        global(".ja-starter-chart-labels", new CssRule { display = flex, flex_wrap = wrap, margin = margin(px(14), px(0), px(0)), gap = px(14), color = hex("4b5b76"), font_size = px(12) });
        global(".ja-starter-chart-labels span", new CssRule { display = flex, align_items = center, gap = px(5) });
        global(".ja-starter-chart-labels i", new CssRule { width = px(8), height = px(8), background = hex("0052d9"), border_radius = percent(50) });
        global(".ja-starter-chart-labels span:nth-child(2) i", new CssRule { background = hex("00a6a6") });
        global(".ja-starter-chart-labels span:nth-child(3) i", new CssRule { background = hex("c9cdd4") });
        global(".ja-starter-sheet header", new CssRule { margin_bottom = px(14) });
        global(".ja-starter-sheet table, .ja-starter-table-wrap table", new CssRule { width = percent(100), border_collapse = keyword("collapse"), text_align = left });
        global(".ja-starter-sheet th, .ja-starter-sheet td, .ja-starter-table-wrap th, .ja-starter-table-wrap td", new CssRule { padding = padding(px(12), px(10)), border_bottom = px(1) | solid | hex("f0f0f0"), color = hex("4b5b76"), font_size = px(13) });
        global(".ja-starter-sheet th, .ja-starter-table-wrap th", new CssRule { color = hex("86909c"), background = hex("f7f8fa"), font_weight = 400 });

        global(".ja-starter-card-list", new CssRule { display = grid, gap = px(16) });
        global(".ja-starter-list-header", new CssRule { padding = px(20), background = hex("fff"), border_radius = px(3) });
        global(".ja-starter-list-header p", new CssRule { margin = margin(px(6), px(0), px(0)), color = hex("86909c"), font_size = px(13) });
        global(".ja-starter-filter", new CssRule { display = flex, align_items = center, flex_wrap = wrap, padding = padding(px(16), px(20)), background = hex("fff"), gap = px(10), border_radius = px(3) });
        global(".ja-starter-filter label, .ja-starter-tree label", new CssRule { display = flex, flex = flex_box(1, 1, px(260)), align_items = center, min_height = px(32), padding = padding(px(0), px(10)), gap = px(6), background = hex("f3f3f3"), border_radius = px(3) });
        global(".ja-starter-filter input, .ja-starter-filter select, .ja-starter-tree input", new CssRule { min_width = px(0), padding = padding(px(0)), background = transparent, border = none, outline = none });
        global(".ja-starter-card-list__grid", new CssRule { display = grid, grid_template_columns = tracks(repeat(3, min_max(px(0), fr(1)))), gap = px(16) });
        global(".ja-starter-card-list__grid article", new CssRule { padding = px(18), background = hex("fff"), border = px(1) | solid | hex("ececec"), border_radius = px(3) });
        global(".ja-starter-card-list__grid article:hover", new CssRule { border_color = hex("0052d9"), box_shadow = shadows(new CssShadow(px(0), px(4), Blur: px(16), Color: rgba(0, 0, 0, 0.06))) });
        global(".ja-starter-card-list__grid article header", new CssRule { padding = padding(px(0)) });
        global(".ja-starter-card-list__grid article header button", new CssRule { padding = padding(px(0)), border = none });
        global(".ja-starter-card-list__mark", new CssRule { display = grid, width = px(38), height = px(38), color = hex("0052d9"), background = hex("e8f3ff"), border_radius = px(3), place_items = center, font_size = px(12), font_weight = 600 });
        global(".ja-starter-card-list__grid h3", new CssRule { margin = margin(px(18), px(0), px(10)), color = hex("1d2129"), font_size = px(16), font_weight = 500 });
        global(".ja-starter-card-list__grid p", new CssRule { margin = margin(px(6), px(0)), color = hex("86909c"), font_size = px(12) });
        global(".ja-starter-card-list__grid footer", new CssRule { display = flex, align_items = center, justify_content = space_between, margin_top = px(18) });
        global(".ja-starter-card-list__grid footer > div", new CssRule { display = flex, align_items = center, gap = px(8) });
        global(".ja-starter-card-list__grid footer .ja-starter-text-button", new CssRule { margin_left = px(8) });

        global(".ja-starter-list", new CssRule { display = grid, background = hex("fff"), border_radius = px(3) });
        global(".ja-starter-list--tree", new CssRule { grid_template_columns = tracks(px(280), min_max(px(0), fr(1))) });
        global(".ja-starter-list--base .ja-starter-list__content", new CssRule { padding = px(20) });
        global(".ja-starter-list--base .ja-starter-list-header", new CssRule { padding = padding(px(0), px(0), px(18)), border_bottom = px(1) | solid | hex("e7e7e7"), border_radius = px(0) });
        global(".ja-starter-list__operations", new CssRule { display = flex, align_items = center, flex_wrap = wrap, gap = px(8) });
        global(".ja-starter-list__operations p", new CssRule { margin = margin(px(0), px(0), px(0), px(8)), color = hex("86909c"), font_size = px(13) });
        global(".ja-starter-search", new CssRule { display = flex, width = px(300), align_items = center, padding = padding(px(7), px(10)), gap = px(6), background = hex("f3f3f3"), border_radius = px(3) });
        global(".ja-starter-search input", new CssRule { width = percent(100), min_width = px(0), padding = px(0), background = transparent, border = none, outline = none });
        global(".ja-starter-list--filter", new CssRule { display = grid, gap = px(16), background = transparent });
        global(".ja-starter-filter-form", new CssRule { display = grid, grid_template_columns = tracks(repeat(4, min_max(px(0), fr(1)))), align_items = end, padding = px(20), gap = px(16), background = hex("fff"), border_radius = px(3) });
        global(".ja-starter-filter-form label", new CssRule { display = grid, gap = px(8), color = hex("4b5b76"), font_size = px(13) });
        global(".ja-starter-filter-form input, .ja-starter-filter-form select", new CssRule { width = percent(100), min_width = px(0), padding = padding(px(8), px(10)), color = hex("1d2129"), background = hex("fff"), border = px(1) | solid | hex("dcdcdc"), border_radius = px(3), outline = none });
        global(".ja-starter-filter-form__actions", new CssRule { display = flex, align_items = center, justify_content = end, gap = px(8) });
        global(".ja-starter-filter-table", new CssRule { background = hex("fff"), border_radius = px(3) });
        global(".ja-starter-list--tree .ja-starter-list-header", new CssRule { padding = px(20), border_bottom = px(1) | solid | hex("e7e7e7"), border_radius = px(0) });
        global(".ja-starter-list--tree .ja-starter-tree", new CssRule { min_width = px(0) });
        global(".ja-starter-text-button.is-danger", new CssRule { color = important(hex("d54941")) });
        global(".ja-starter-status.is-processing", new CssRule { color = hex("0052d9"), background = hex("e8f3ff") });
        global(".ja-starter-status.is-warning", new CssRule { color = hex("ed7b2f"), background = hex("fff1e9") });
        global(".ja-starter-tree", new CssRule { display = flex, flex_direction = column, padding = px(20), gap = px(8), border_right = px(1) | solid | hex("e7e7e7") });
        global(".ja-starter-tree strong", new CssRule { margin_top = px(16), color = hex("1d2129"), font_size = px(14) });
        global(".ja-starter-tree button", new CssRule { width = percent(100), text_align = left, border = none });
        global(".ja-starter-tree button.is-selected", new CssRule { color = hex("0052d9"), background = hex("e8f3ff") });
        global(".ja-starter-list__content", new CssRule { min_width = px(0) });
        global(".ja-starter-list__content > .ja-starter-list-header", new CssRule { border_bottom = px(1) | solid | hex("e7e7e7") });
        global(".ja-starter-list__content > .ja-starter-filter", new CssRule { border_bottom = px(1) | solid | hex("e7e7e7"), border_radius = px(0) });
        global(".ja-starter-table-wrap", new CssRule { overflow_x = auto, padding = padding(px(0), px(20)) });
        global(".ja-starter-table-wrap td strong, .ja-starter-table-wrap td small", new CssRule { display = block });
        global(".ja-starter-table-wrap td strong", new CssRule { color = hex("1d2129"), font_weight = 500 });
        global(".ja-starter-table-wrap td small", new CssRule { margin_top = px(3), color = hex("86909c") });
        global(".ja-starter-text-button", new CssRule
        {
            padding = important(px(0)),
            color = important(hex("0052d9")),
            border = important(none)
        });
        global(".ja-starter-pagination", new CssRule { display = flex, align_items = center, justify_content = space_between, padding = padding(px(16), px(20)), color = hex("86909c"), font_size = px(13) });
        global(".ja-starter-pagination div", new CssRule { display = flex, gap = px(4) });
        global(".ja-starter-pagination button", new CssRule { min_width = px(30), padding = px(5) });
        global(".ja-starter-pagination button.is-selected", new CssRule { color = hex("fff"), background = hex("0052d9"), border_color = hex("0052d9") });
        global(".ja-starter-dialog-backdrop", new CssRule { position = fixed_position, top = px(0), right = px(0), bottom = px(0), left = px(0), display = grid, place_items = center, z_index = 100, background = rgba(0, 0, 0, 0.42) });
        global(".ja-starter-dialog", new CssRule { width = min(percent(100) - px(32), px(420)), padding = px(24), background = hex("fff"), border_radius = px(4), box_shadow = shadows(new CssShadow(px(0), px(8), Blur: px(28), Color: rgba(0, 0, 0, 0.18))) });
        global(".ja-starter-dialog header", new CssRule { padding_bottom = px(14), border_bottom = px(1) | solid | hex("e7e7e7") });
        global(".ja-starter-dialog header button", new CssRule { padding = px(2), border = none, background = transparent });
        global(".ja-starter-dialog p", new CssRule { margin = margin(px(20), px(0)), color = hex("4b5b76"), line_height = 1.6 });
        global(".ja-starter-dialog footer", new CssRule { display = flex, justify_content = end, gap = px(8) });
        global(".ja-starter-danger", new CssRule { color = important(hex("fff")), background = important(hex("d54941")), border_color = important(hex("d54941")) });
        global(".ja-starter-card-dialog form", new CssRule { display = grid, margin_top = px(20), gap = px(16) });
        global(".ja-starter-card-dialog label", new CssRule { display = grid, gap = px(8), color = hex("4b5b76"), font_size = px(13) });
        global(".ja-starter-card-dialog input, .ja-starter-card-dialog select, .ja-starter-card-dialog textarea", new CssRule { width = percent(100), padding = padding(px(8), px(10)), color = hex("1d2129"), border = px(1) | solid | hex("dcdcdc"), border_radius = px(3), outline = none });

        global(".ja-starter-form", new CssRule { display = grid, gap = px(16) });
        global(".ja-starter-form form", new CssRule { display = grid, gap = px(16) });
        global(".ja-starter-form form > section", new CssRule { display = grid, gap = px(22) });
        global(".ja-starter-form--base form", new CssRule { gap = px(16) });
        global(".ja-starter-form--base form > section", new CssRule { padding = px(24), background = hex("fff"), border_radius = px(3) });
        global(".ja-starter-form--base form > section h2", new CssRule { padding_bottom = px(16), border_bottom = px(1) | solid | hex("e7e7e7") });
        global(".ja-starter-form__grid", new CssRule { display = grid, grid_template_columns = tracks(repeat(2, min_max(px(0), fr(1)))), gap = gap(px(20), px(32)) });
        global(".ja-starter-form label", new CssRule { display = grid, gap = px(8), color = hex("4b5b76"), font_size = px(13) });
        global(".ja-starter-form label.is-full", new CssRule { width = percent(100) });
        global(".ja-starter-form fieldset", new CssRule { display = grid, margin = margin(px(0)), padding = padding(px(0)), border = none, gap = px(8) });
        global(".ja-starter-form legend", new CssRule { color = hex("4b5b76"), font_size = px(13) });
        global(".ja-starter-radio-group", new CssRule { display = flex, align_items = center, flex_wrap = wrap, gap = px(14) });
        global(".ja-starter-radio-group label", new CssRule { display = inline_flex, width = auto, align_items = center, gap = px(5) });
        global(".ja-starter-radio-group input:last-child", new CssRule { width = px(180) });
        global(".ja-starter-avatar-group", new CssRule { display = flex, gap = px(6) });
        global(".ja-starter-avatar-group b", new CssRule { display = grid, width = px(30), height = px(30), place_items = center, color = hex("fff"), background = hex("0052d9"), border_radius = percent(50), font_size = px(12) });
        global(".ja-starter-avatar-group b:nth-child(2)", new CssRule { background = hex("00a870") });
        global(".ja-starter-avatar-group b:nth-child(3)", new CssRule { color = hex("0052d9"), background = hex("e8f3ff") });
        global(".ja-starter-form input, .ja-starter-form select, .ja-starter-form textarea", new CssRule { width = percent(100), padding = padding(px(8), px(10)), color = hex("1d2129"), background = hex("fff"), border = px(1) | solid | hex("dcdcdc"), border_radius = px(3), outline = none });
        global(".ja-starter-form input:focus, .ja-starter-form select:focus, .ja-starter-form textarea:focus", new CssRule { border_color = hex("0052d9"), box_shadow = shadows(new CssShadow(px(0), px(0), Blur: px(0), Spread: px(2), Color: rgba(0, 82, 217, 0.12))) });
        global(".ja-starter-form footer", new CssRule { display = flex, padding = padding(px(16), px(20)), background = hex("fff"), gap = px(10), border_radius = px(3) });
        global(".ja-starter-form--step", new CssRule { display = grid, gap = px(16) });
        global(".ja-starter-steps", new CssRule { display = grid, grid_template_columns = tracks(repeat(4, min_max(px(0), fr(1)))), padding = px(20), margin = margin(px(0)), background = hex("fff"), list_style = none, border_radius = px(3) });
        global(".ja-starter-steps li", new CssRule { display = flex, align_items = center, color = hex("86909c"), gap = px(10), font_size = px(14) });
        global(".ja-starter-steps b", new CssRule { display = grid, width = px(28), height = px(28), background = hex("e7e7e7"), border_radius = percent(50), place_items = center });
        global(".ja-starter-steps li.is-current", new CssRule { color = hex("0052d9") });
        global(".ja-starter-steps li.is-current b", new CssRule { color = hex("fff"), background = hex("0052d9") });
        global(".ja-starter-steps li span", new CssRule { display = grid, gap = px(2) });
        global(".ja-starter-steps li small", new CssRule { color = hex("86909c"), font_size = px(11) });
        global(".ja-starter-step-panel", new CssRule { display = grid, width = min(percent(100) - px(40), px(760)), padding = px(28), background = hex("fff"), border_radius = px(3), gap = px(18) });
        global(".ja-starter-step-panel__notice", new CssRule { padding = padding(px(12), px(16)), color = hex("4b5b76"), background = hex("f3f8ff"), border_left = px(3) | solid | hex("0052d9") });
        global(".ja-starter-step-panel__notice p", new CssRule { margin = margin(px(5), px(0), px(0)), color = hex("86909c"), font_size = px(13) });
        global(".ja-starter-step-panel label", new CssRule { display = grid, gap = px(8), color = hex("4b5b76"), font_size = px(13) });
        global(".ja-starter-step-amount", new CssRule { display = flex, align_items = center, justify_content = space_between, margin = margin(px(0)), padding = padding(px(14), px(0)), border_top = px(1) | solid | hex("e7e7e7"), border_bottom = px(1) | solid | hex("e7e7e7"), color = hex("86909c") });
        global(".ja-starter-step-amount strong", new CssRule { color = hex("1d2129"), font_size = px(20) });
        global(".ja-starter-step-complete", new CssRule { display = flex, flex_direction = column, align_items = center, justify_content = center, min_height = px(360), padding = px(28), background = hex("fff"), border_radius = px(3), text_align = center });
        global(".ja-starter-step-complete > .t-icon", new CssRule { color = hex("00a870") });
        global(".ja-starter-step-complete p", new CssRule { margin = margin(px(8), px(0), px(20)), color = hex("86909c") });
        global(".ja-starter-step-complete div", new CssRule { display = flex, gap = px(8) });

        global(".ja-starter-detail", new CssRule { display = grid, gap = px(16) });
        global(".ja-starter-detail__banner", new CssRule
        {
            display = important(block),
            padding = important(px(28)),
            background = important(hex("0052d9"))
        });
        global(".ja-starter-detail__banner h2", new CssRule { margin = margin(px(6), px(0)), color = hex("fff"), font_size = px(26) });
        global(".ja-starter-detail__banner p", new CssRule { margin = margin(px(0)), color = rgba(255, 255, 255, 0.8) });
        global(".ja-starter-detail__banner div", new CssRule { display = flex, margin_top = px(20), gap = px(8) });
        global(".ja-starter-detail--advanced > section", new CssRule { display = grid, gap = px(18) });
        global(".ja-starter-detail--advanced > section > header", new CssRule { padding_bottom = px(14), border_bottom = px(1) | solid | hex("e7e7e7") });
        global(".ja-starter-detail--advanced > section > header button", new CssRule { display = inline_flex, align_items = center, gap = px(4) });
        global(".ja-starter-descriptions", new CssRule { display = grid, grid_template_columns = tracks(repeat(3, min_max(px(0), fr(1)))), margin = margin(px(22), px(0), px(0)), gap = gap(px(22), px(32)) });
        global(".ja-starter-descriptions dt", new CssRule { color = hex("86909c"), font_size = px(13) });
        global(".ja-starter-descriptions dd", new CssRule { margin = margin(px(8), px(0), px(0)), color = hex("1d2129"), font_size = px(14) });
        global(".ja-starter-timeline", new CssRule { display = grid, margin = margin(px(24), px(0), px(0)), padding = padding(px(0)), gap = px(20), list_style = none });
        global(".ja-starter-timeline li", new CssRule { display = grid, grid_template_columns = tracks(px(18), min_max(px(0), fr(1))), gap = px(10) });
        global(".ja-starter-timeline b", new CssRule { width = px(10), height = px(10), margin_top = px(3), background = hex("0052d9"), border_radius = percent(50) });
        global(".ja-starter-timeline strong", new CssRule { color = hex("1d2129"), font_size = px(14) });
        global(".ja-starter-timeline p", new CssRule { margin = margin(px(4), px(0), px(0)), color = hex("86909c"), font_size = px(12) });
        global(".ja-starter-horizontal-steps", new CssRule { display = grid, grid_template_columns = tracks(repeat(4, min_max(px(0), fr(1)))), margin = margin(px(8), px(0), px(0)), padding = padding(px(0)), list_style = none, gap = px(8) });
        global(".ja-starter-horizontal-steps li", new CssRule { display = flex, align_items = center, position = relative, color = hex("86909c"), gap = px(8) });
        global(".ja-starter-horizontal-steps li:not(:last-child)::after", new CssRule { position = absolute, top = px(14), left = px(32), width = percent(60), border_top = px(1) | solid | hex("e7e7e7"), content = str("") });
        global(".ja-starter-horizontal-steps li.is-current", new CssRule { color = hex("0052d9") });
        global(".ja-starter-horizontal-steps b", new CssRule { display = grid, width = px(28), height = px(28), place_items = center, border = px(1) | solid | hex("dcdcdc"), border_radius = percent(50) });
        global(".ja-starter-horizontal-steps li.is-current b", new CssRule { color = hex("fff"), background = hex("0052d9"), border_color = hex("0052d9") });
        global(".ja-starter-horizontal-steps span", new CssRule { display = grid, gap = px(2), font_size = px(13) });
        global(".ja-starter-horizontal-steps small", new CssRule { color = hex("86909c"), font_size = px(11) });
        global(".ja-starter-product-grid", new CssRule { display = grid, grid_template_columns = tracks(repeat(4, min_max(px(0), fr(1)))), gap = px(12) });
        global(".ja-starter-product-card", new CssRule { display = grid, min_height = px(132), padding = px(16), background = hex("f7f8fa"), border = px(1) | solid | hex("e7e7e7"), border_radius = px(3), gap = px(6) });
        global(".ja-starter-product-card.is-add", new CssRule { align_content = center, place_items = center, color = hex("0052d9"), background = hex("f3f8ff"), border_style = dashed, cursor = pointer });
        global(".ja-starter-product-card > b", new CssRule { color = hex("0052d9"), font_size = px(11) });
        global(".ja-starter-product-card h3", new CssRule { margin = margin(px(4), px(0)), color = hex("1d2129"), font_size = px(14), font_weight = 500 });
        global(".ja-starter-product-card p", new CssRule { margin = margin(px(0)), color = hex("86909c"), font_size = px(12) });
        global(".ja-starter-product-card strong", new CssRule { margin_top = px(8), color = hex("1d2129"), font_size = px(14) });
        global(".ja-starter-dialog-descriptions", new CssRule { display = grid, grid_template_columns = tracks(repeat(2, min_max(px(0), fr(1)))), margin = margin(px(18), px(0)), gap = px(14) });
        global(".ja-starter-dialog-descriptions dt", new CssRule { color = hex("86909c"), font_size = px(12) });
        global(".ja-starter-dialog-descriptions dd", new CssRule { margin = margin(px(4), px(0), px(0)), color = hex("1d2129"), font_size = px(13) });
        global(".ja-starter-empty", new CssRule { display = flex, flex_direction = column, align_items = center, padding = px(52), color = hex("86909c"), text_align = center });
        global(".ja-starter-tabs", new CssRule { display = flex, margin_top = px(18), border_bottom = px(1) | solid | hex("e7e7e7"), gap = px(20) });
        global(".ja-starter-tabs button", new CssRule { padding = padding(px(0), px(0), px(10)), border = none, border_bottom = px(2) | solid | transparent, border_radius = px(0) });
        global(".ja-starter-tabs button.is-selected", new CssRule { color = hex("0052d9"), border_bottom_color = hex("0052d9") });
        global(".ja-starter-notices", new CssRule { display = grid, margin_top = px(4) });
        global(".ja-starter-notices article", new CssRule { display = grid, grid_template_columns = tracks(px(10), min_max(px(0), fr(1)), auto, auto, auto), align_items = center, padding = padding(px(16), px(0)), gap = px(12), border_bottom = px(1) | solid | hex("f0f0f0") });
        global(".ja-starter-notices article > i", new CssRule { width = px(8), height = px(8), background = hex("0052d9"), border_radius = percent(50) });
        global(".ja-starter-notices article > i.success", new CssRule { background = hex("00a870") });
        global(".ja-starter-notices article > i.warning", new CssRule { background = hex("ed7b2f") });
        global(".ja-starter-notices h3", new CssRule { margin = margin(px(0)), color = hex("4b5b76"), font_size = px(14), font_weight = 400 });
        global(".ja-starter-notices article.is-unread h3", new CssRule { color = hex("1d2129"), font_weight = 600 });
        global(".ja-starter-notices p", new CssRule { margin = margin(px(5), px(0), px(0)), color = hex("86909c"), font_size = px(12) });
        global(".ja-starter-notices time", new CssRule { color = hex("86909c"), font_size = px(12) });
        global(".ja-starter-notices button", new CssRule { padding = px(2), border = none });

        global(".ja-starter-result", new CssRule { display = flex, flex_direction = column, align_items = center, justify_content = center, min_height = vh(100) - px(220), text_align = center });
        global(".ja-starter-result > .t-icon", new CssRule { color = hex("00a870") });
        global(".ja-starter-result.is-fail > .t-icon", new CssRule { color = hex("d54941") });
        global(".ja-starter-result.is-warning > .t-icon", new CssRule { color = hex("ed7b2f") });
        global(".ja-starter-result__art", new CssRule { width = px(200), max_width = percent(100), height = px(160), object_fit = contain });
        global(".ja-starter-result h2", new CssRule { margin = margin(px(22), px(0), px(0)), color = hex("1d2129"), font_size = px(22), font_weight = 500 });
        global(".ja-starter-result p", new CssRule { max_width = px(480), margin = margin(px(10), px(0), px(24)), color = hex("86909c"), line_height = 1.6 });
        global(".ja-starter-result__actions", new CssRule { display = flex, gap = px(10) });
        global(".ja-starter-browser-recommendation", new CssRule { display = flex, align_items = center, margin_bottom = px(22), padding = padding(px(16), px(24)), gap = px(24), background = hex("fff"), border_radius = px(3), box_shadow = shadows(new CssShadow(px(0), px(1), Blur: px(2), Color: rgba(0, 0, 0, 0.1))) });
        global(".ja-starter-browser-recommendation > span", new CssRule { color = hex("86909c"), font_size = px(13) });
        global(".ja-starter-browser-recommendation strong", new CssRule { display = inline_flex, align_items = center, color = hex("4b5b76"), font_size = px(13), font_weight = 400, gap = px(6) });

        global(".ja-starter-user-page", new CssRule { display = grid, grid_template_columns = tracks(min_max(px(0), fr(1)), px(300)), gap = px(16) });
        global(".ja-starter-user-page__main", new CssRule { display = grid, gap = px(16) });
        global(".ja-starter-user-page__main > header", new CssRule { padding = px(24), color = hex("fff"), background = hex("0052d9"), border_radius = px(3) });
        global(".ja-starter-user-page__main > header h2", new CssRule { color = hex("fff") });
        global(".ja-starter-user-page__main > header p", new CssRule { margin = margin(px(6), px(0), px(0)), color = rgba(255, 255, 255, 0.8) });
        global(".ja-starter-user-page__main > header > span", new CssRule { display = grid, width = px(54), height = px(54), background = rgba(255, 255, 255, 0.2), border_radius = percent(50), place_items = center, font_size = px(22) });
        global(".ja-starter-user-page > aside", new CssRule { display = grid, align_content = start, gap = px(16) });
        global(".ja-starter-user-card", new CssRule { text_align = center });
        global(".ja-starter-user-card > span", new CssRule { display = grid, width = px(72), height = px(72), margin = margin(px(0), auto), color = hex("fff"), background = hex("0052d9"), border_radius = percent(50), place_items = center, font_size = px(26) });
        global(".ja-starter-user-card h2", new CssRule { margin_top = px(14) });
        global(".ja-starter-user-card p", new CssRule { margin = margin(px(6), px(0), px(0)), color = hex("86909c"), font_size = px(13) });
        global(".ja-starter-user-page aside ul", new CssRule { display = grid, margin = margin(px(18), px(0), px(0)), padding = padding(px(0)), gap = px(14), list_style = none });
        global(".ja-starter-user-page aside li", new CssRule { display = flex, align_items = center, gap = px(10) });
        global(".ja-starter-user-page aside li b", new CssRule { display = grid, width = px(30), height = px(30), color = hex("0052d9"), background = hex("e8f3ff"), border_radius = percent(50), place_items = center });
        global(".ja-starter-user-page aside li span, .ja-starter-user-page aside li small", new CssRule { display = block });
        global(".ja-starter-user-page aside li small", new CssRule { margin_top = px(2), color = hex("86909c") });
        global(".ja-starter-user-content", new CssRule { display = grid, min_height = px(300), gap = px(18) });
        global(".ja-starter-user-content > header", new CssRule { padding_top = px(8) });
        global(".ja-starter-user-placeholder", new CssRule { margin = margin(px(10), px(0)), color = hex("86909c"), font_size = px(13) });
        global(".ja-starter-user-activity", new CssRule { display = grid, margin = margin(px(0)), padding = padding(px(0)), list_style = none, gap = px(18) });
        global(".ja-starter-user-activity li", new CssRule { display = grid, grid_template_columns = tracks(px(14), min_max(px(0), fr(1))), gap = px(10) });
        global(".ja-starter-user-activity b", new CssRule { width = px(8), height = px(8), margin_top = px(4), background = hex("0052d9"), border_radius = percent(50) });
        global(".ja-starter-user-activity strong", new CssRule { color = hex("1d2129"), font_size = px(14) });
        global(".ja-starter-user-activity p", new CssRule { margin = margin(px(4), px(0), px(0)), color = hex("86909c"), font_size = px(12) });
        global(".ja-starter-user-products > div", new CssRule { display = grid, grid_template_columns = tracks(repeat(4, min_max(px(0), fr(1)))), margin_top = px(18), gap = px(8), color = hex("0052d9") });
        global(".ja-starter-user-products .t-icon", new CssRule { padding = px(8), background = hex("f3f8ff"), border_radius = px(3) });

        global(".ja-starter-login", new CssRule { min_height = vh(100) - px(112), background = hex("fff"), border_radius = px(3) });
        global(".ja-starter-login > header", new CssRule { padding = padding(px(18), px(28)), border_bottom = px(1) | solid | hex("eeeeee") });
        global(".ja-starter-login > header strong", new CssRule { display = flex, align_items = center, color = hex("1d2129"), gap = px(8) });
        global(".ja-starter-login > header img", new CssRule { width = px(28), height = px(28) });
        global(".ja-starter-login > header span", new CssRule { color = hex("86909c"), font_size = px(13) });
        global(".ja-starter-login main", new CssRule { display = grid, min_height = px(620), background = hex("f5f9ff"), place_items = center });
        global(".ja-starter-login main > section", new CssRule { width = min(percent(100) - px(40), px(380)), padding = px(34), background = hex("fff"), border_radius = px(3), box_shadow = shadows(new CssShadow(px(0), px(12), Blur: px(30), Color: rgba(0, 82, 217, 0.12))) });
        global(".ja-starter-login__mark", new CssRule { display = grid, width = px(48), height = px(48), color = hex("fff"), background = hex("0052d9"), border_radius = px(3), place_items = center, font_size = px(22) });
        global(".ja-starter-login h1", new CssRule { margin = margin(px(18), px(0), px(8)), color = hex("1d2129"), font_size = px(24) });
        global(".ja-starter-login main p", new CssRule { margin = margin(px(0)), color = hex("86909c") });
        global(".ja-starter-login form", new CssRule { display = grid, margin_top = px(24), gap = px(16) });
        global(".ja-starter-login form > label", new CssRule { display = grid, gap = px(8), color = hex("4b5b76"), font_size = px(13) });
        global(".ja-starter-login input", new CssRule { padding = px(10), border = px(1) | solid | hex("dcdcdc"), border_radius = px(3), outline = none });
        global(".ja-starter-login form > div", new CssRule { display = flex, align_items = center, justify_content = space_between, color = hex("86909c"), font_size = px(12) });
        global(".ja-starter-login form > div label", new CssRule { display = flex, align_items = center, gap = px(6) });
        global(".ja-starter-login form > div button", new CssRule { padding = padding(px(0)), color = hex("0052d9"), border = none });
        global(".ja-starter-login__switch", new CssRule { padding = important(px(0)), color = important(hex("0052d9")), background = important(transparent), border = important(none) });
        global(".ja-starter-login > footer", new CssRule { padding = px(16), color = hex("86909c"), font_size = px(12), text_align = center });

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
        global(".ja-application--dark .ja-starter-metric, .ja-application--dark .ja-starter-card, .ja-application--dark .ja-starter-page > section, .ja-application--dark .ja-starter-sheet, .ja-application--dark .ja-starter-user-page > aside > section",
            new CssRule { background = var("--td-bg-color-container"), color = var("--td-text-color-primary") });
        global(".ja-application--dark .ja-starter-metric strong, .ja-application--dark .ja-starter-user-activity strong, .ja-application--dark .ja-starter-login h1",
            new CssRule { color = var("--td-text-color-primary") });

        Media(".ja-starter-dashboard__metrics", "(max-width: 1180px)", new CssRule { grid_template_columns = tracks(repeat(2, min_max(px(0), fr(1)))) });
        Media(".ja-starter-output__grid", "(max-width: 1080px)", new CssRule { grid_template_columns = tracks(repeat(2, min_max(px(0), fr(1)))) });
        Media(".ja-starter-dashboard__analysis, .ja-starter-dashboard__rankings", "(max-width: 860px)", new CssRule { grid_template_columns = tracks(fr(1)) });
        Media(".ja-starter-search", "(max-width: 1120px)", new CssRule { width = px(150) });
        Media(".ja-starter-user__command span", "(max-width: 980px)", new CssRule { display = none });
        Media(".ja-starter-organization span, .ja-starter-organization", "(max-width: 900px)", new CssRule { display = none });
        Media(".ja-starter-dashboard__metrics, .ja-starter-output__grid", "(max-width: 620px)", new CssRule { grid_template_columns = tracks(fr(1)) });
        Media(".ja-starter-search", "(max-width: 620px)", new CssRule { display = none });
        Media(".ja-starter-user", "(max-width: 620px)", new CssRule { margin_left = px(2) });
        Media(".ja-starter-user__trigger .t-button__text, .ja-starter-user__trigger .t-button__suffix", "(max-width: 620px)", new CssRule { display = none });
        return true;
    }

    private static void Media(string selector, string prelude, CssRule rule)
        => global(selector, new CssRule { children = [new(ChildKind.Media, prelude, rule)] });
}
