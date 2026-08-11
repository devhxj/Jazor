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
            new CssRule { Background = hex("f3f3f3") });

        global(".ja-tdesign-layout__header",
            new CssRule { Background = hex("fff"), Gap = px(8) });

        global(".ja-tdesign-sidebar-only",
            new CssRule
            {
                Display = flex,
                FlexDirection = column,
                Width = percent(100),
                Height = percent(100),
                Background = var("--surface")
            });

        global(".ja-tdesign-sidebar-only__brand",
            new CssRule
            {
                Display = flex,
                Flex = raw("0 0 64px"),
                AlignItems = center,
                Height = px(64),
                Padding = padding(px(0), px(20)),
                Gap = px(10),
                Color = raw("var(--td-text-color-primary)"),
                FontSize = px(16),
                FontWeight = 600,
                TextDecoration = none
            });

        global(".ja-tdesign-sidebar-only__brand img",
            new CssRule { Width = px(28), Height = px(28) });

        global(".ja-tdesign-sidebar-only__menu",
            new CssRule { MinHeight = px(0), OverflowY = auto });

        global(".tdesign-starter-footer",
            new CssRule
            {
                Padding = padding(px(0), px(24), px(80)),
                Color = raw("var(--td-text-color-placeholder)"),
                FontSize = px(12),
                LineHeight = px(20),
                TextAlign = center
            });

        global(".setting-container",
            new CssRule
            {
                PaddingBottom = px(100)
            });

        global(".setting-group-title",
            new CssRule
            {
                Margin = margin(px(32), px(0), px(24)),
                Color = raw("var(--td-text-color-primary)"),
                FontFamily = raw("'PingFang SC', var(--td-font-family)"),
                FontSize = px(14),
                FontWeight = 500,
                LineHeight = px(22)
            });

        global(".setting-layout-color-group",
            new CssRule
            {
                Display = inlineFlex,
                AlignItems = center,
                JustifyContent = center,
                Padding = important(px(6)),
                Border = important(px(2) | solid | transparent),
                BorderRadius = important(percent(50))
            });

        global(".setting-layout-color-group > .t-radio-button__label",
            new CssRule { Display = inlineFlex });

        global(".setting-color-preview",
            new CssRule { Display = block, Width = px(16), Height = px(16), BorderRadius = percent(50) });

        global(".setting-drawer-container .setting-container",
            new CssRule { PaddingBottom = px(100) });

        global(".setting-drawer-container .t-radio-group.t-size-m",
            new CssRule
            {
                Display = flex,
                AlignItems = center,
                JustifyContent = spaceBetween,
                Width = percent(100),
                MinHeight = px(32)
            });

        global(".setting-drawer-container .t-radio-group.t-size-m.side-mode-radio",
            new CssRule { JustifyContent = end });

        global(".setting-drawer-container .t-radio-group.t-size-m .t-radio-button",
            new CssRule { Height = raw("auto") });

        global(".setting-drawer-container .setting-layout-drawer",
            new CssRule
            {
                Display = flex,
                FlexDirection = column,
                AlignItems = center,
                MarginBottom = px(16)
            });

        global(".setting-drawer-container .setting-layout-drawer .t-radio-button",
            new CssRule
            {
                Display = inlineFlex,
                MaxHeight = px(78),
                Padding = px(8),
                Border = px(2) | solid | raw("var(--td-component-border)"),
                BorderRadius = raw("var(--td-radius-default)")
            });

        global(".setting-drawer-container .setting-layout-drawer .t-radio-button > .t-radio-button__label",
            new CssRule { Display = inlineFlex });

        global(".setting-drawer-container .setting-layout-drawer p",
            new CssRule { MarginTop = px(8), TextAlign = center });

        global(".setting-drawer-container .thumbnail-layout",
            new CssRule { Display = inlineBlock, Width = px(88), Height = px(48) });

        global(".setting-drawer-container .setting-layout-drawer .t-is-checked",
            new CssRule { Border = important(px(2) | solid | raw("var(--td-brand-color)")) });

        global(".setting-drawer-container .t-form__controls-content",
            new CssRule { JustifyContent = end });

        global(".setting-info",
            new CssRule
            {
                Position = absolute,
                Bottom = px(0),
                Left = px(0),
                Width = percent(100),
                Padding = px(24),
                Color = raw("var(--td-text-color-placeholder)"),
                Background = raw("var(--td-bg-color-container)"),
                FontSize = px(12),
                LineHeight = px(20),
                TextAlign = center
            });

        global(".setting-info p",
            new CssRule { Margin = px(0) });

        global(".ja-starter-setting-trigger",
            new CssRule
            {
                Position = fixedPosition,
                Left = percent(50),
                Bottom = px(24),
                ZIndex = 40,
                Width = px(40),
                Height = px(40),
                MinWidth = px(40),
                Padding = px(0),
                Transform = raw("translateX(-50%)"),
                BoxShadow = shadows(new CssShadow(px(0), px(6), Blur: px(16), Color: rgba(0, 0, 0, 0.16)))
            });

        global(".ja-starter-operations",
            new CssRule
            {
                Display = flex,
                AlignItems = center,
                Gap = px(4)
            });

        global(".ja-starter-search, .ja-starter-organization, .ja-starter-select",
            new CssRule
            {
                Display = flex,
                AlignItems = center,
                MinHeight = px(32),
                Gap = px(6),
                Color = hex("4b5b76")
            });

        global(".ja-starter-search",
            new CssRule
            {
                Width = px(220),
                Padding = padding(px(0), px(10)),
                Background = hex("f3f3f3"),
                BorderRadius = px(3)
            });

        global(".ja-starter-search input, .ja-starter-organization select, .ja-starter-select select",
            new CssRule
            {
                Width = percent(100),
                MinWidth = px(0),
                Color = hex("1d2129"),
                Background = transparent,
                Border = none,
                Outline = none
            });

        global(".ja-starter-organization",
            new CssRule
            {
                MaxWidth = px(220),
                Padding = padding(px(0), px(8)),
                BorderRight = px(1) | solid | hex("e7e7e7")
            });

        global(".ja-starter-organization span",
            new CssRule { FontSize = px(13), WhiteSpace = nowrap });

        global(".ja-starter-select",
            new CssRule { Width = px(84), Padding = padding(px(0), px(4)) });

        global(".ja-starter-user",
            new CssRule
            {
                Display = flex,
                AlignItems = center,
                MinWidth = px(0),
                Gap = px(8),
                MarginLeft = px(10)
            });

        global(".ja-starter-user__avatar",
            new CssRule
            {
                Display = grid,
                Flex = flexBox(0, 0, px(32)),
                Width = px(32),
                Height = px(32),
                Color = hex("fff"),
                Background = hex("0052d9"),
                BorderRadius = percent(50),
                PlaceItems = center,
                FontSize = px(13),
                FontWeight = 600
            });

        global(".ja-starter-user__name",
            new CssRule
            {
                MaxWidth = px(140),
                Overflow = hidden,
                TextOverflow = ellipsis,
                WhiteSpace = nowrap,
                FontSize = px(14)
            });

        global(".ja-starter-user__command",
            new CssRule
            {
                Display = inlineFlex,
                AlignItems = center,
                Gap = px(4),
                Padding = padding(px(5), px(6)),
                Color = hex("4b5b76"),
                Background = transparent,
                Border = none,
                BorderRadius = px(3),
                FontSize = px(13)
            });

        global(".ja-starter-user__command:hover",
            new CssRule { Color = hex("0052d9"), Background = hex("f2f3ff") });

        global(".ja-starter-dashboard",
            new CssRule
            {
                Display = grid,
                Gap = px(16),
                Animation = animation(ident("ja-dashboard-enter"), ms(220), easeOut)
            });

        global(".ja-starter-dashboard__metrics",
            new CssRule
            {
                Display = grid,
                GridTemplateColumns = tracks(repeat(4, minMax(px(0), fr(1)))),
                Gap = px(16)
            });

        global(".ja-starter-metric, .ja-starter-card",
            new CssRule
            {
                Background = hex("fff"),
                Border = important(none),
                BorderRadius = important(px(3)),
                BoxShadow = important(none)
            });

        global(".ja-starter-metric .t-card__body, .ja-starter-card .t-card__body",
            new CssRule { Padding = important(px(20)) });

        global(".ja-starter-metric__label, .ja-starter-card__header",
            new CssRule
            {
                Display = flex,
                AlignItems = flexStart,
                JustifyContent = spaceBetween,
                Gap = px(12)
            });

        global(".ja-starter-metric__label",
            new CssRule { Color = hex("4b5b76"), FontSize = px(14) });

        global(".ja-starter-metric__label .t-icon",
            new CssRule { Color = hex("0052d9") });

        global(".ja-starter-metric strong",
            new CssRule
            {
                Display = block,
                MarginTop = px(18),
                Color = hex("1d2129"),
                FontSize = px(32),
                FontWeight = 600,
                LineHeight = 1
            });

        global(".ja-starter-metric strong.is-code",
            new CssRule { FontSize = px(24), OverflowWrap = keyword("anywhere") });

        global(".ja-starter-metric__footer",
            new CssRule
            {
                Display = flex,
                MinWidth = px(0),
                MarginTop = px(16),
                Gap = px(8),
                Color = hex("86909c"),
                FontSize = px(12)
            });

        global(".ja-starter-metric__footer span:last-child",
            new CssRule { Overflow = hidden, TextOverflow = ellipsis, WhiteSpace = nowrap });

        global(".ja-starter-metric__footer .is-up",
            new CssRule { Color = hex("00a870"), FontWeight = 600 });

        global(".ja-starter-metric__footer .is-stable",
            new CssRule { Color = hex("86909c"), FontWeight = 600 });

        global(".ja-starter-dashboard__analysis",
            new CssRule
            {
                Display = grid,
                GridTemplateColumns = tracks(minMax(px(0), fr(1.65)), minMax(px(280), fr(0.75))),
                Gap = px(16)
            });

        global(".ja-starter-card__header h2",
            new CssRule { Margin = margin(px(0)), Color = hex("1d2129"), FontSize = px(18), FontWeight = 500 });

        global(".ja-starter-card__header p",
            new CssRule { Margin = margin(px(6), px(0), px(0)), Color = hex("86909c"), FontSize = px(13) });

        global(".ja-starter-segments",
            new CssRule { Display = flex, Padding = px(2), Background = hex("f3f3f3"), BorderRadius = px(3) });

        global(".ja-starter-segments button, .ja-starter-output button",
            new CssRule { Padding = padding(px(5), px(9)), Color = hex("4b5b76"), Background = transparent, Border = none, BorderRadius = px(2), FontSize = px(12) });

        global(".ja-starter-segments button.is-selected",
            new CssRule { Color = hex("0052d9"), Background = hex("fff"), BoxShadow = shadows(new CssShadow(px(0), px(1), Blur: px(2), Color: rgba(0, 0, 0, 0.08))) });

        global(".ja-starter-chart",
            new CssRule { Display = grid, GridTemplateColumns = tracks(px(30), minMax(px(0), fr(1))), Height = px(226), MarginTop = px(16) });

        global(".ja-starter-chart__axis",
            new CssRule { Display = flex, FlexDirection = column, JustifyContent = spaceBetween, PaddingBottom = px(22), Color = hex("86909c"), FontSize = px(11) });

        global(".ja-starter-chart__plot",
            new CssRule
            {
                Display = grid,
                GridTemplateColumns = tracks(repeat(7, minMax(px(0), fr(1)))),
                AlignItems = end,
                ColumnGap = px(14),
                Padding = padding(px(0), px(8), px(22)),
                BackgroundImage = linearGradient(stop(hex("e7e7e7"), px(1)), stop(transparent, px(1))),
                BackgroundSize = backgroundSize(percent(100), percent(25))
            });

        global(".ja-starter-chart__plot > div",
            new CssRule { Position = relative, Display = flex, AlignItems = end, JustifyContent = center, Height = percent(100), Gap = px(4) });

        global(".ja-starter-chart__plot i, .ja-starter-chart__plot b",
            new CssRule { Display = block, Width = px(11), BorderRadius = radius(px(2), px(2), px(0), px(0)) });

        global(".ja-starter-chart__plot i", new CssRule { Background = hex("8ec5ff") });
        global(".ja-starter-chart__plot b", new CssRule { Background = hex("0052d9") });
        global(".ja-starter-chart__plot span", new CssRule { Position = absolute, Bottom = px(-21), Color = hex("86909c"), FontSize = px(11) });

        global(".ja-starter-chart__legend",
            new CssRule { Display = flex, MarginLeft = px(30), Gap = px(20), Color = hex("4b5b76"), FontSize = px(12) });

        global(".ja-starter-chart__legend span, .ja-starter-distribution dt",
            new CssRule { Display = inlineFlex, AlignItems = center, Gap = px(6) });

        global(".ja-starter-chart__legend i, .ja-starter-distribution dl i",
            new CssRule { Display = block, Width = px(8), Height = px(8), BorderRadius = percent(50) });
        global(".ja-starter-chart__legend .is-primary, .ja-starter-distribution .is-brand", new CssRule { Background = hex("0052d9") });
        global(".ja-starter-chart__legend .is-secondary, .ja-starter-distribution .is-cyan", new CssRule { Background = hex("00a6a6") });
        global(".ja-starter-distribution .is-gray", new CssRule { Background = hex("c9cdd4") });

        global(".ja-starter-distribution .t-card__body",
            new CssRule { Display = flex, FlexDirection = column });

        global(".ja-starter-donut",
            new CssRule { Display = grid, Width = px(164), Height = px(164), Margin = margin(px(20), auto), Background = conicGradient(stop(hex("0052d9"), px(0), percent(45)), stop(hex("00a6a6"), percent(45), percent(78)), stop(hex("c9cdd4"), percent(78), percent(100))), BorderRadius = percent(50), PlaceItems = center });

        global(".ja-starter-donut::before",
            new CssRule { GridArea = gridLine(1, 1), Width = px(118), Height = px(118), Background = hex("fff"), BorderRadius = percent(50), Content = str(string.Empty) });

        global(".ja-starter-donut span",
            new CssRule { GridArea = gridLine(1, 1), Position = relative, ZIndex = 1, Color = hex("1d2129"), FontSize = px(28), FontWeight = 600, TextAlign = center });
        global(".ja-starter-donut small", new CssRule { Display = block, MarginTop = px(2), Color = hex("86909c"), FontSize = px(11), FontWeight = 400 });

        global(".ja-starter-distribution dl", new CssRule { Display = grid, Margin = margin(auto, px(0), px(0)), Gap = px(10) });
        global(".ja-starter-distribution dl div", new CssRule { Display = flex, AlignItems = center, JustifyContent = spaceBetween, Color = hex("4b5b76"), FontSize = px(13) });
        global(".ja-starter-distribution dd", new CssRule { Margin = margin(px(0)), Color = hex("1d2129"), FontWeight = 600 });

        global(".ja-starter-dashboard__rankings", new CssRule { Display = grid, GridTemplateColumns = tracks(repeat(2, minMax(px(0), fr(1)))), Gap = px(16) });
        global(".ja-starter-rank table", new CssRule { Width = percent(100), MarginTop = px(18), BorderCollapse = keyword("collapse"), TextAlign = left });
        global(".ja-starter-rank th, .ja-starter-rank td", new CssRule { Padding = padding(px(10), px(6)), BorderBottom = px(1) | solid | hex("f0f0f0"), FontSize = px(13) });
        global(".ja-starter-rank th", new CssRule { Color = hex("86909c"), FontWeight = 400 });
        global(".ja-starter-rank td", new CssRule { Color = hex("4b5b76") });
        global(".ja-starter-rank code", new CssRule { Color = hex("0052d9"), FontFamily = fontFamily(genericFont("ui-monospace"), genericFont("SFMono-Regular"), font("Consolas"), genericFont("monospace")), FontSize = px(12) });
        global(".ja-starter-rank__index", new CssRule { Display = grid, Width = px(22), Height = px(22), Color = hex("4b5b76"), Background = hex("f3f3f3"), BorderRadius = percent(50), PlaceItems = center, FontSize = px(12) });
        global(".ja-starter-status", new CssRule { Display = inlineBlock, Padding = padding(px(2), px(6)), BorderRadius = px(2), FontSize = px(11) });
        global(".ja-starter-status.is-success", new CssRule { Color = hex("00a870"), Background = hex("e8ffea") });
        global(".ja-starter-empty", new CssRule { Color = hex("86909c"), TextAlign = center });

        global(".ja-starter-output__grid", new CssRule { Display = grid, GridTemplateColumns = tracks(repeat(4, minMax(px(0), fr(1)))), MarginTop = px(20), Gap = px(16) });
        global(".ja-starter-output__grid article", new CssRule { Display = grid, GridTemplateColumns = tracks(px(24), minMax(px(0), fr(1))), AlignItems = center, Padding = px(14), Gap = px(10), Background = hex("f7f8fa"), BorderRadius = px(3) });
        global(".ja-starter-output__grid .t-icon", new CssRule { Color = hex("0052d9") });
        global(".ja-starter-output__grid strong, .ja-starter-output__grid span", new CssRule { Display = block });
        global(".ja-starter-output__grid strong", new CssRule { Color = hex("1d2129"), FontSize = px(13) });
        global(".ja-starter-output__grid span", new CssRule { MarginTop = px(3), Color = hex("86909c"), FontSize = px(11) });
        global(".ja-starter-output__grid em", new CssRule { GridColumn = 2, Color = hex("00a870"), FontSize = px(11), FontStyle = normal });

        global(".ja-starter-page", new CssRule { MinWidth = px(0) });

        global(".ja-starter-page > section, .ja-starter-sheet, .ja-starter-user-page > aside > section",
            new CssRule { Padding = px(20), Background = hex("fff"), BorderRadius = px(3) });

        global(".ja-starter-page h2", new CssRule { Margin = margin(px(0)), Color = hex("1d2129"), FontSize = px(18), FontWeight = 500 });
        global(".ja-starter-page header", new CssRule { Display = flex, AlignItems = center, JustifyContent = spaceBetween, Gap = px(12) });
        global(".ja-starter-page button, .ja-starter-primary, .ja-starter-filter select", new CssRule { Padding = padding(px(7), px(12)), Color = hex("4b5b76"), Background = hex("fff"), Border = px(1) | solid | hex("dcdcdc"), BorderRadius = px(3), FontSize = px(13) });
        global(".ja-starter-primary", new CssRule
        {
            Display = inlineFlex,
            AlignItems = center,
            Gap = px(4),
            Color = important(hex("fff")),
            Background = important(hex("0052d9")),
            BorderColor = important(hex("0052d9"))
        });
        global(".ja-starter-page button:hover", new CssRule { Color = hex("0052d9"), BorderColor = hex("0052d9") });
        global(".ja-starter-primary:hover", new CssRule
        {
            Color = important(hex("fff")),
            Background = important(hex("003cab"))
        });
        global(".ja-starter-page button.is-plain", new CssRule { Background = hex("fff") });
        global(".ja-starter-inline-message", new CssRule { Margin = margin(px(12), px(0)), Padding = padding(px(10), px(12)), Color = hex("00a870"), Background = hex("e8ffea"), BorderRadius = px(3), FontSize = px(13) });

        global(".ja-starter-report", new CssRule { Display = grid, Gap = px(16) });
        global(".ja-starter-report__hero", new CssRule { Display = flex, AlignItems = center, JustifyContent = spaceBetween, Padding = px(28), Color = hex("fff"), Background = hex("0052d9"), BorderRadius = px(3) });
        global(".ja-starter-report__hero h2", new CssRule { Margin = margin(px(5), px(0)), Color = hex("fff"), FontSize = px(24) });
        global(".ja-starter-report__hero p", new CssRule { Margin = margin(px(0)), Color = rgba(255, 255, 255, 0.78), FontSize = px(13) });
        global(".ja-starter-report__hero button", new CssRule { Color = hex("0052d9"), BorderColor = hex("fff") });
        global(".ja-starter-report__metrics", new CssRule { Display = grid, GridTemplateColumns = tracks(repeat(4, minMax(px(0), fr(1)))), Gap = px(16) });
        global(".ja-starter-report__metrics article", new CssRule { Position = relative, Padding = px(18), Background = hex("fff"), BorderRadius = px(3) });
        global(".ja-starter-report__metrics span", new CssRule { Color = hex("86909c"), FontSize = px(13) });
        global(".ja-starter-report__metrics strong", new CssRule { Display = block, MarginTop = px(12), Color = hex("1d2129"), FontSize = px(28), FontWeight = 600 });
        global(".ja-starter-report__metrics em", new CssRule { Position = absolute, Right = px(18), Bottom = px(18), Color = hex("00a870"), FontSize = px(12), FontStyle = normal });
        global(".ja-starter-report__metrics em.is-down", new CssRule { Color = hex("d54941") });
        global(".ja-starter-report__charts", new CssRule { Display = grid, GridTemplateColumns = tracks(repeat(2, minMax(px(0), fr(1)))), Gap = px(16) });
        global(".ja-starter-report__charts > section", new CssRule { MinHeight = px(298) });
        global(".ja-starter-line-chart", new CssRule { Display = grid, GridTemplateColumns = tracks(repeat(7, minMax(px(0), fr(1)))), AlignItems = end, Height = px(194), MarginTop = px(26), Gap = px(10), BackgroundImage = linearGradient(stop(hex("e7e7e7"), px(1)), stop(transparent, px(1))), BackgroundSize = backgroundSize(percent(100), percent(25)) });
        global(".ja-starter-line-chart i", new CssRule { Display = block, Height = percent(45), Background = hex("0052d9"), BorderRadius = radius(px(3), px(3), px(0), px(0)) });
        global(".ja-starter-line-chart i:nth-child(2n)", new CssRule { Height = percent(72), Background = hex("8ec5ff") });
        global(".ja-starter-line-chart i:nth-child(3n)", new CssRule { Height = percent(84), Background = hex("0052d9") });
        global(".ja-starter-chart-labels", new CssRule { Display = flex, FlexWrap = wrap, Margin = margin(px(14), px(0), px(0)), Gap = px(14), Color = hex("4b5b76"), FontSize = px(12) });
        global(".ja-starter-chart-labels span", new CssRule { Display = flex, AlignItems = center, Gap = px(5) });
        global(".ja-starter-chart-labels i", new CssRule { Width = px(8), Height = px(8), Background = hex("0052d9"), BorderRadius = percent(50) });
        global(".ja-starter-chart-labels span:nth-child(2) i", new CssRule { Background = hex("00a6a6") });
        global(".ja-starter-chart-labels span:nth-child(3) i", new CssRule { Background = hex("c9cdd4") });
        global(".ja-starter-sheet header", new CssRule { MarginBottom = px(14) });
        global(".ja-starter-sheet table, .ja-starter-table-wrap table", new CssRule { Width = percent(100), BorderCollapse = keyword("collapse"), TextAlign = left });
        global(".ja-starter-sheet th, .ja-starter-sheet td, .ja-starter-table-wrap th, .ja-starter-table-wrap td", new CssRule { Padding = padding(px(12), px(10)), BorderBottom = px(1) | solid | hex("f0f0f0"), Color = hex("4b5b76"), FontSize = px(13) });
        global(".ja-starter-sheet th, .ja-starter-table-wrap th", new CssRule { Color = hex("86909c"), Background = hex("f7f8fa"), FontWeight = 400 });

        global(".ja-starter-card-list", new CssRule { Display = grid, Gap = px(16) });
        global(".ja-starter-list-header", new CssRule { Padding = px(20), Background = hex("fff"), BorderRadius = px(3) });
        global(".ja-starter-list-header p", new CssRule { Margin = margin(px(6), px(0), px(0)), Color = hex("86909c"), FontSize = px(13) });
        global(".ja-starter-filter", new CssRule { Display = flex, AlignItems = center, FlexWrap = wrap, Padding = padding(px(16), px(20)), Background = hex("fff"), Gap = px(10), BorderRadius = px(3) });
        global(".ja-starter-filter label, .ja-starter-tree label", new CssRule { Display = flex, Flex = flexBox(1, 1, px(260)), AlignItems = center, MinHeight = px(32), Padding = padding(px(0), px(10)), Gap = px(6), Background = hex("f3f3f3"), BorderRadius = px(3) });
        global(".ja-starter-filter input, .ja-starter-filter select, .ja-starter-tree input", new CssRule { MinWidth = px(0), Padding = padding(px(0)), Background = transparent, Border = none, Outline = none });
        global(".ja-starter-card-list__grid", new CssRule { Display = grid, GridTemplateColumns = tracks(repeat(3, minMax(px(0), fr(1)))), Gap = px(16) });
        global(".ja-starter-card-list__grid article", new CssRule { Padding = px(18), Background = hex("fff"), Border = px(1) | solid | hex("ececec"), BorderRadius = px(3) });
        global(".ja-starter-card-list__grid article:hover", new CssRule { BorderColor = hex("0052d9"), BoxShadow = shadows(new CssShadow(px(0), px(4), Blur: px(16), Color: rgba(0, 0, 0, 0.06))) });
        global(".ja-starter-card-list__grid article header", new CssRule { Padding = padding(px(0)) });
        global(".ja-starter-card-list__grid article header button", new CssRule { Padding = padding(px(0)), Border = none });
        global(".ja-starter-card-list__mark", new CssRule { Display = grid, Width = px(38), Height = px(38), Color = hex("0052d9"), Background = hex("e8f3ff"), BorderRadius = px(3), PlaceItems = center, FontSize = px(12), FontWeight = 600 });
        global(".ja-starter-card-list__grid h3", new CssRule { Margin = margin(px(18), px(0), px(10)), Color = hex("1d2129"), FontSize = px(16), FontWeight = 500 });
        global(".ja-starter-card-list__grid p", new CssRule { Margin = margin(px(6), px(0)), Color = hex("86909c"), FontSize = px(12) });
        global(".ja-starter-card-list__grid footer", new CssRule { Display = flex, AlignItems = center, JustifyContent = spaceBetween, MarginTop = px(18) });
        global(".ja-starter-card-list__grid footer > div", new CssRule { Display = flex, AlignItems = center, Gap = px(8) });
        global(".ja-starter-card-list__grid footer .ja-starter-text-button", new CssRule { MarginLeft = px(8) });

        global(".ja-starter-list", new CssRule { Display = grid, Background = hex("fff"), BorderRadius = px(3) });
        global(".ja-starter-list--tree", new CssRule { GridTemplateColumns = tracks(px(280), minMax(px(0), fr(1))) });
        global(".ja-starter-list--base .ja-starter-list__content", new CssRule { Padding = px(20) });
        global(".ja-starter-list--base .ja-starter-list-header", new CssRule { Padding = padding(px(0), px(0), px(18)), BorderBottom = px(1) | solid | hex("e7e7e7"), BorderRadius = px(0) });
        global(".ja-starter-list__operations", new CssRule { Display = flex, AlignItems = center, FlexWrap = wrap, Gap = px(8) });
        global(".ja-starter-list__operations p", new CssRule { Margin = margin(px(0), px(0), px(0), px(8)), Color = hex("86909c"), FontSize = px(13) });
        global(".ja-starter-search", new CssRule { Display = flex, Width = px(300), AlignItems = center, Padding = padding(px(7), px(10)), Gap = px(6), Background = hex("f3f3f3"), BorderRadius = px(3) });
        global(".ja-starter-search input", new CssRule { Width = percent(100), MinWidth = px(0), Padding = px(0), Background = transparent, Border = none, Outline = none });
        global(".ja-starter-list--filter", new CssRule { Display = grid, Gap = px(16), Background = transparent });
        global(".ja-starter-filter-form", new CssRule { Display = grid, GridTemplateColumns = tracks(repeat(4, minMax(px(0), fr(1)))), AlignItems = end, Padding = px(20), Gap = px(16), Background = hex("fff"), BorderRadius = px(3) });
        global(".ja-starter-filter-form label", new CssRule { Display = grid, Gap = px(8), Color = hex("4b5b76"), FontSize = px(13) });
        global(".ja-starter-filter-form input, .ja-starter-filter-form select", new CssRule { Width = percent(100), MinWidth = px(0), Padding = padding(px(8), px(10)), Color = hex("1d2129"), Background = hex("fff"), Border = px(1) | solid | hex("dcdcdc"), BorderRadius = px(3), Outline = none });
        global(".ja-starter-filter-form__actions", new CssRule { Display = flex, AlignItems = center, JustifyContent = end, Gap = px(8) });
        global(".ja-starter-filter-table", new CssRule { Background = hex("fff"), BorderRadius = px(3) });
        global(".ja-starter-list--tree .ja-starter-list-header", new CssRule { Padding = px(20), BorderBottom = px(1) | solid | hex("e7e7e7"), BorderRadius = px(0) });
        global(".ja-starter-list--tree .ja-starter-tree", new CssRule { MinWidth = px(0) });
        global(".ja-starter-text-button.is-danger", new CssRule { Color = important(hex("d54941")) });
        global(".ja-starter-status.is-processing", new CssRule { Color = hex("0052d9"), Background = hex("e8f3ff") });
        global(".ja-starter-status.is-warning", new CssRule { Color = hex("ed7b2f"), Background = hex("fff1e9") });
        global(".ja-starter-tree", new CssRule { Display = flex, FlexDirection = column, Padding = px(20), Gap = px(8), BorderRight = px(1) | solid | hex("e7e7e7") });
        global(".ja-starter-tree strong", new CssRule { MarginTop = px(16), Color = hex("1d2129"), FontSize = px(14) });
        global(".ja-starter-tree button", new CssRule { Width = percent(100), TextAlign = left, Border = none });
        global(".ja-starter-tree button.is-selected", new CssRule { Color = hex("0052d9"), Background = hex("e8f3ff") });
        global(".ja-starter-list__content", new CssRule { MinWidth = px(0) });
        global(".ja-starter-list__content > .ja-starter-list-header", new CssRule { BorderBottom = px(1) | solid | hex("e7e7e7") });
        global(".ja-starter-list__content > .ja-starter-filter", new CssRule { BorderBottom = px(1) | solid | hex("e7e7e7"), BorderRadius = px(0) });
        global(".ja-starter-table-wrap", new CssRule { OverflowX = auto, Padding = padding(px(0), px(20)) });
        global(".ja-starter-table-wrap td strong, .ja-starter-table-wrap td small", new CssRule { Display = block });
        global(".ja-starter-table-wrap td strong", new CssRule { Color = hex("1d2129"), FontWeight = 500 });
        global(".ja-starter-table-wrap td small", new CssRule { MarginTop = px(3), Color = hex("86909c") });
        global(".ja-starter-text-button", new CssRule
        {
            Padding = important(px(0)),
            Color = important(hex("0052d9")),
            Border = important(none)
        });
        global(".ja-starter-pagination", new CssRule { Display = flex, AlignItems = center, JustifyContent = spaceBetween, Padding = padding(px(16), px(20)), Color = hex("86909c"), FontSize = px(13) });
        global(".ja-starter-pagination div", new CssRule { Display = flex, Gap = px(4) });
        global(".ja-starter-pagination button", new CssRule { MinWidth = px(30), Padding = px(5) });
        global(".ja-starter-pagination button.is-selected", new CssRule { Color = hex("fff"), Background = hex("0052d9"), BorderColor = hex("0052d9") });
        global(".ja-starter-dialog-backdrop", new CssRule { Position = fixedPosition, Top = px(0), Right = px(0), Bottom = px(0), Left = px(0), Display = grid, PlaceItems = center, ZIndex = 100, Background = rgba(0, 0, 0, 0.42) });
        global(".ja-starter-dialog", new CssRule { Width = min(percent(100) - px(32), px(420)), Padding = px(24), Background = hex("fff"), BorderRadius = px(4), BoxShadow = shadows(new CssShadow(px(0), px(8), Blur: px(28), Color: rgba(0, 0, 0, 0.18))) });
        global(".ja-starter-dialog header", new CssRule { PaddingBottom = px(14), BorderBottom = px(1) | solid | hex("e7e7e7") });
        global(".ja-starter-dialog header button", new CssRule { Padding = px(2), Border = none, Background = transparent });
        global(".ja-starter-dialog p", new CssRule { Margin = margin(px(20), px(0)), Color = hex("4b5b76"), LineHeight = 1.6 });
        global(".ja-starter-dialog footer", new CssRule { Display = flex, JustifyContent = end, Gap = px(8) });
        global(".ja-starter-danger", new CssRule { Color = important(hex("fff")), Background = important(hex("d54941")), BorderColor = important(hex("d54941")) });
        global(".ja-starter-card-dialog form", new CssRule { Display = grid, MarginTop = px(20), Gap = px(16) });
        global(".ja-starter-card-dialog label", new CssRule { Display = grid, Gap = px(8), Color = hex("4b5b76"), FontSize = px(13) });
        global(".ja-starter-card-dialog input, .ja-starter-card-dialog select, .ja-starter-card-dialog textarea", new CssRule { Width = percent(100), Padding = padding(px(8), px(10)), Color = hex("1d2129"), Border = px(1) | solid | hex("dcdcdc"), BorderRadius = px(3), Outline = none });

        global(".ja-starter-form", new CssRule { Display = grid, Gap = px(16) });
        global(".ja-starter-form form", new CssRule { Display = grid, Gap = px(16) });
        global(".ja-starter-form form > section", new CssRule { Display = grid, Gap = px(22) });
        global(".ja-starter-form--base form", new CssRule { Gap = px(16) });
        global(".ja-starter-form--base form > section", new CssRule { Padding = px(24), Background = hex("fff"), BorderRadius = px(3) });
        global(".ja-starter-form--base form > section h2", new CssRule { PaddingBottom = px(16), BorderBottom = px(1) | solid | hex("e7e7e7") });
        global(".ja-starter-form__grid", new CssRule { Display = grid, GridTemplateColumns = tracks(repeat(2, minMax(px(0), fr(1)))), Gap = gap(px(20), px(32)) });
        global(".ja-starter-form label", new CssRule { Display = grid, Gap = px(8), Color = hex("4b5b76"), FontSize = px(13) });
        global(".ja-starter-form label.is-full", new CssRule { Width = percent(100) });
        global(".ja-starter-form fieldset", new CssRule { Display = grid, Margin = margin(px(0)), Padding = padding(px(0)), Border = none, Gap = px(8) });
        global(".ja-starter-form legend", new CssRule { Color = hex("4b5b76"), FontSize = px(13) });
        global(".ja-starter-radio-group", new CssRule { Display = flex, AlignItems = center, FlexWrap = wrap, Gap = px(14) });
        global(".ja-starter-radio-group label", new CssRule { Display = inlineFlex, Width = auto, AlignItems = center, Gap = px(5) });
        global(".ja-starter-radio-group input:last-child", new CssRule { Width = px(180) });
        global(".ja-starter-avatar-group", new CssRule { Display = flex, Gap = px(6) });
        global(".ja-starter-avatar-group b", new CssRule { Display = grid, Width = px(30), Height = px(30), PlaceItems = center, Color = hex("fff"), Background = hex("0052d9"), BorderRadius = percent(50), FontSize = px(12) });
        global(".ja-starter-avatar-group b:nth-child(2)", new CssRule { Background = hex("00a870") });
        global(".ja-starter-avatar-group b:nth-child(3)", new CssRule { Color = hex("0052d9"), Background = hex("e8f3ff") });
        global(".ja-starter-form input, .ja-starter-form select, .ja-starter-form textarea", new CssRule { Width = percent(100), Padding = padding(px(8), px(10)), Color = hex("1d2129"), Background = hex("fff"), Border = px(1) | solid | hex("dcdcdc"), BorderRadius = px(3), Outline = none });
        global(".ja-starter-form input:focus, .ja-starter-form select:focus, .ja-starter-form textarea:focus", new CssRule { BorderColor = hex("0052d9"), BoxShadow = shadows(new CssShadow(px(0), px(0), Blur: px(0), Spread: px(2), Color: rgba(0, 82, 217, 0.12))) });
        global(".ja-starter-form footer", new CssRule { Display = flex, Padding = padding(px(16), px(20)), Background = hex("fff"), Gap = px(10), BorderRadius = px(3) });
        global(".ja-starter-form--step", new CssRule { Display = grid, Gap = px(16) });
        global(".ja-starter-steps", new CssRule { Display = grid, GridTemplateColumns = tracks(repeat(4, minMax(px(0), fr(1)))), Padding = px(20), Margin = margin(px(0)), Background = hex("fff"), ListStyle = none, BorderRadius = px(3) });
        global(".ja-starter-steps li", new CssRule { Display = flex, AlignItems = center, Color = hex("86909c"), Gap = px(10), FontSize = px(14) });
        global(".ja-starter-steps b", new CssRule { Display = grid, Width = px(28), Height = px(28), Background = hex("e7e7e7"), BorderRadius = percent(50), PlaceItems = center });
        global(".ja-starter-steps li.is-current", new CssRule { Color = hex("0052d9") });
        global(".ja-starter-steps li.is-current b", new CssRule { Color = hex("fff"), Background = hex("0052d9") });
        global(".ja-starter-steps li span", new CssRule { Display = grid, Gap = px(2) });
        global(".ja-starter-steps li small", new CssRule { Color = hex("86909c"), FontSize = px(11) });
        global(".ja-starter-step-panel", new CssRule { Display = grid, Width = min(percent(100) - px(40), px(760)), Padding = px(28), Background = hex("fff"), BorderRadius = px(3), Gap = px(18) });
        global(".ja-starter-step-panel__notice", new CssRule { Padding = padding(px(12), px(16)), Color = hex("4b5b76"), Background = hex("f3f8ff"), BorderLeft = px(3) | solid | hex("0052d9") });
        global(".ja-starter-step-panel__notice p", new CssRule { Margin = margin(px(5), px(0), px(0)), Color = hex("86909c"), FontSize = px(13) });
        global(".ja-starter-step-panel label", new CssRule { Display = grid, Gap = px(8), Color = hex("4b5b76"), FontSize = px(13) });
        global(".ja-starter-step-amount", new CssRule { Display = flex, AlignItems = center, JustifyContent = spaceBetween, Margin = margin(px(0)), Padding = padding(px(14), px(0)), BorderTop = px(1) | solid | hex("e7e7e7"), BorderBottom = px(1) | solid | hex("e7e7e7"), Color = hex("86909c") });
        global(".ja-starter-step-amount strong", new CssRule { Color = hex("1d2129"), FontSize = px(20) });
        global(".ja-starter-step-complete", new CssRule { Display = flex, FlexDirection = column, AlignItems = center, JustifyContent = center, MinHeight = px(360), Padding = px(28), Background = hex("fff"), BorderRadius = px(3), TextAlign = center });
        global(".ja-starter-step-complete > .t-icon", new CssRule { Color = hex("00a870") });
        global(".ja-starter-step-complete p", new CssRule { Margin = margin(px(8), px(0), px(20)), Color = hex("86909c") });
        global(".ja-starter-step-complete div", new CssRule { Display = flex, Gap = px(8) });

        global(".ja-starter-detail", new CssRule { Display = grid, Gap = px(16) });
        global(".ja-starter-detail__banner", new CssRule
        {
            Display = important(block),
            Padding = important(px(28)),
            Background = important(hex("0052d9"))
        });
        global(".ja-starter-detail__banner h2", new CssRule { Margin = margin(px(6), px(0)), Color = hex("fff"), FontSize = px(26) });
        global(".ja-starter-detail__banner p", new CssRule { Margin = margin(px(0)), Color = rgba(255, 255, 255, 0.8) });
        global(".ja-starter-detail__banner div", new CssRule { Display = flex, MarginTop = px(20), Gap = px(8) });
        global(".ja-starter-detail--advanced > section", new CssRule { Display = grid, Gap = px(18) });
        global(".ja-starter-detail--advanced > section > header", new CssRule { PaddingBottom = px(14), BorderBottom = px(1) | solid | hex("e7e7e7") });
        global(".ja-starter-detail--advanced > section > header button", new CssRule { Display = inlineFlex, AlignItems = center, Gap = px(4) });
        global(".ja-starter-descriptions", new CssRule { Display = grid, GridTemplateColumns = tracks(repeat(3, minMax(px(0), fr(1)))), Margin = margin(px(22), px(0), px(0)), Gap = gap(px(22), px(32)) });
        global(".ja-starter-descriptions dt", new CssRule { Color = hex("86909c"), FontSize = px(13) });
        global(".ja-starter-descriptions dd", new CssRule { Margin = margin(px(8), px(0), px(0)), Color = hex("1d2129"), FontSize = px(14) });
        global(".ja-starter-timeline", new CssRule { Display = grid, Margin = margin(px(24), px(0), px(0)), Padding = padding(px(0)), Gap = px(20), ListStyle = none });
        global(".ja-starter-timeline li", new CssRule { Display = grid, GridTemplateColumns = tracks(px(18), minMax(px(0), fr(1))), Gap = px(10) });
        global(".ja-starter-timeline b", new CssRule { Width = px(10), Height = px(10), MarginTop = px(3), Background = hex("0052d9"), BorderRadius = percent(50) });
        global(".ja-starter-timeline strong", new CssRule { Color = hex("1d2129"), FontSize = px(14) });
        global(".ja-starter-timeline p", new CssRule { Margin = margin(px(4), px(0), px(0)), Color = hex("86909c"), FontSize = px(12) });
        global(".ja-starter-horizontal-steps", new CssRule { Display = grid, GridTemplateColumns = tracks(repeat(4, minMax(px(0), fr(1)))), Margin = margin(px(8), px(0), px(0)), Padding = padding(px(0)), ListStyle = none, Gap = px(8) });
        global(".ja-starter-horizontal-steps li", new CssRule { Display = flex, AlignItems = center, Position = relative, Color = hex("86909c"), Gap = px(8) });
        global(".ja-starter-horizontal-steps li:not(:last-child)::after", new CssRule { Position = absolute, Top = px(14), Left = px(32), Width = percent(60), BorderTop = px(1) | solid | hex("e7e7e7"), Content = str("") });
        global(".ja-starter-horizontal-steps li.is-current", new CssRule { Color = hex("0052d9") });
        global(".ja-starter-horizontal-steps b", new CssRule { Display = grid, Width = px(28), Height = px(28), PlaceItems = center, Border = px(1) | solid | hex("dcdcdc"), BorderRadius = percent(50) });
        global(".ja-starter-horizontal-steps li.is-current b", new CssRule { Color = hex("fff"), Background = hex("0052d9"), BorderColor = hex("0052d9") });
        global(".ja-starter-horizontal-steps span", new CssRule { Display = grid, Gap = px(2), FontSize = px(13) });
        global(".ja-starter-horizontal-steps small", new CssRule { Color = hex("86909c"), FontSize = px(11) });
        global(".ja-starter-product-grid", new CssRule { Display = grid, GridTemplateColumns = tracks(repeat(4, minMax(px(0), fr(1)))), Gap = px(12) });
        global(".ja-starter-product-card", new CssRule { Display = grid, MinHeight = px(132), Padding = px(16), Background = hex("f7f8fa"), Border = px(1) | solid | hex("e7e7e7"), BorderRadius = px(3), Gap = px(6) });
        global(".ja-starter-product-card.is-add", new CssRule { AlignContent = center, PlaceItems = center, Color = hex("0052d9"), Background = hex("f3f8ff"), BorderStyle = dashed, Cursor = pointer });
        global(".ja-starter-product-card > b", new CssRule { Color = hex("0052d9"), FontSize = px(11) });
        global(".ja-starter-product-card h3", new CssRule { Margin = margin(px(4), px(0)), Color = hex("1d2129"), FontSize = px(14), FontWeight = 500 });
        global(".ja-starter-product-card p", new CssRule { Margin = margin(px(0)), Color = hex("86909c"), FontSize = px(12) });
        global(".ja-starter-product-card strong", new CssRule { MarginTop = px(8), Color = hex("1d2129"), FontSize = px(14) });
        global(".ja-starter-dialog-descriptions", new CssRule { Display = grid, GridTemplateColumns = tracks(repeat(2, minMax(px(0), fr(1)))), Margin = margin(px(18), px(0)), Gap = px(14) });
        global(".ja-starter-dialog-descriptions dt", new CssRule { Color = hex("86909c"), FontSize = px(12) });
        global(".ja-starter-dialog-descriptions dd", new CssRule { Margin = margin(px(4), px(0), px(0)), Color = hex("1d2129"), FontSize = px(13) });
        global(".ja-starter-empty", new CssRule { Display = flex, FlexDirection = column, AlignItems = center, Padding = px(52), Color = hex("86909c"), TextAlign = center });
        global(".ja-starter-tabs", new CssRule { Display = flex, MarginTop = px(18), BorderBottom = px(1) | solid | hex("e7e7e7"), Gap = px(20) });
        global(".ja-starter-tabs button", new CssRule { Padding = padding(px(0), px(0), px(10)), Border = none, BorderBottom = px(2) | solid | transparent, BorderRadius = px(0) });
        global(".ja-starter-tabs button.is-selected", new CssRule { Color = hex("0052d9"), BorderBottomColor = hex("0052d9") });
        global(".ja-starter-notices", new CssRule { Display = grid, MarginTop = px(4) });
        global(".ja-starter-notices article", new CssRule { Display = grid, GridTemplateColumns = tracks(px(10), minMax(px(0), fr(1)), auto, auto, auto), AlignItems = center, Padding = padding(px(16), px(0)), Gap = px(12), BorderBottom = px(1) | solid | hex("f0f0f0") });
        global(".ja-starter-notices article > i", new CssRule { Width = px(8), Height = px(8), Background = hex("0052d9"), BorderRadius = percent(50) });
        global(".ja-starter-notices article > i.success", new CssRule { Background = hex("00a870") });
        global(".ja-starter-notices article > i.warning", new CssRule { Background = hex("ed7b2f") });
        global(".ja-starter-notices h3", new CssRule { Margin = margin(px(0)), Color = hex("4b5b76"), FontSize = px(14), FontWeight = 400 });
        global(".ja-starter-notices article.is-unread h3", new CssRule { Color = hex("1d2129"), FontWeight = 600 });
        global(".ja-starter-notices p", new CssRule { Margin = margin(px(5), px(0), px(0)), Color = hex("86909c"), FontSize = px(12) });
        global(".ja-starter-notices time", new CssRule { Color = hex("86909c"), FontSize = px(12) });
        global(".ja-starter-notices button", new CssRule { Padding = px(2), Border = none });

        global(".ja-starter-result", new CssRule { Display = flex, FlexDirection = column, AlignItems = center, JustifyContent = center, MinHeight = vh(100) - px(220), TextAlign = center });
        global(".ja-starter-result > .t-icon", new CssRule { Color = hex("00a870") });
        global(".ja-starter-result.is-fail > .t-icon", new CssRule { Color = hex("d54941") });
        global(".ja-starter-result.is-warning > .t-icon", new CssRule { Color = hex("ed7b2f") });
        global(".ja-starter-result__art", new CssRule { Width = px(200), MaxWidth = percent(100), Height = px(160), ObjectFit = contain });
        global(".ja-starter-result h2", new CssRule { Margin = margin(px(22), px(0), px(0)), Color = hex("1d2129"), FontSize = px(22), FontWeight = 500 });
        global(".ja-starter-result p", new CssRule { MaxWidth = px(480), Margin = margin(px(10), px(0), px(24)), Color = hex("86909c"), LineHeight = 1.6 });
        global(".ja-starter-result__actions", new CssRule { Display = flex, Gap = px(10) });
        global(".ja-starter-browser-recommendation", new CssRule { Display = flex, AlignItems = center, MarginBottom = px(22), Padding = padding(px(16), px(24)), Gap = px(24), Background = hex("fff"), BorderRadius = px(3), BoxShadow = shadows(new CssShadow(px(0), px(1), Blur: px(2), Color: rgba(0, 0, 0, 0.1))) });
        global(".ja-starter-browser-recommendation > span", new CssRule { Color = hex("86909c"), FontSize = px(13) });
        global(".ja-starter-browser-recommendation strong", new CssRule { Display = inlineFlex, AlignItems = center, Color = hex("4b5b76"), FontSize = px(13), FontWeight = 400, Gap = px(6) });

        global(".ja-starter-user-page", new CssRule { Display = grid, GridTemplateColumns = tracks(minMax(px(0), fr(1)), px(300)), Gap = px(16) });
        global(".ja-starter-user-page__main", new CssRule { Display = grid, Gap = px(16) });
        global(".ja-starter-user-page__main > header", new CssRule { Padding = px(24), Color = hex("fff"), Background = hex("0052d9"), BorderRadius = px(3) });
        global(".ja-starter-user-page__main > header h2", new CssRule { Color = hex("fff") });
        global(".ja-starter-user-page__main > header p", new CssRule { Margin = margin(px(6), px(0), px(0)), Color = rgba(255, 255, 255, 0.8) });
        global(".ja-starter-user-page__main > header > span", new CssRule { Display = grid, Width = px(54), Height = px(54), Background = rgba(255, 255, 255, 0.2), BorderRadius = percent(50), PlaceItems = center, FontSize = px(22) });
        global(".ja-starter-user-page > aside", new CssRule { Display = grid, AlignContent = start, Gap = px(16) });
        global(".ja-starter-user-card", new CssRule { TextAlign = center });
        global(".ja-starter-user-card > span", new CssRule { Display = grid, Width = px(72), Height = px(72), Margin = margin(px(0), auto), Color = hex("fff"), Background = hex("0052d9"), BorderRadius = percent(50), PlaceItems = center, FontSize = px(26) });
        global(".ja-starter-user-card h2", new CssRule { MarginTop = px(14) });
        global(".ja-starter-user-card p", new CssRule { Margin = margin(px(6), px(0), px(0)), Color = hex("86909c"), FontSize = px(13) });
        global(".ja-starter-user-page aside ul", new CssRule { Display = grid, Margin = margin(px(18), px(0), px(0)), Padding = padding(px(0)), Gap = px(14), ListStyle = none });
        global(".ja-starter-user-page aside li", new CssRule { Display = flex, AlignItems = center, Gap = px(10) });
        global(".ja-starter-user-page aside li b", new CssRule { Display = grid, Width = px(30), Height = px(30), Color = hex("0052d9"), Background = hex("e8f3ff"), BorderRadius = percent(50), PlaceItems = center });
        global(".ja-starter-user-page aside li span, .ja-starter-user-page aside li small", new CssRule { Display = block });
        global(".ja-starter-user-page aside li small", new CssRule { MarginTop = px(2), Color = hex("86909c") });
        global(".ja-starter-user-content", new CssRule { Display = grid, MinHeight = px(300), Gap = px(18) });
        global(".ja-starter-user-content > header", new CssRule { PaddingTop = px(8) });
        global(".ja-starter-user-placeholder", new CssRule { Margin = margin(px(10), px(0)), Color = hex("86909c"), FontSize = px(13) });
        global(".ja-starter-user-activity", new CssRule { Display = grid, Margin = margin(px(0)), Padding = padding(px(0)), ListStyle = none, Gap = px(18) });
        global(".ja-starter-user-activity li", new CssRule { Display = grid, GridTemplateColumns = tracks(px(14), minMax(px(0), fr(1))), Gap = px(10) });
        global(".ja-starter-user-activity b", new CssRule { Width = px(8), Height = px(8), MarginTop = px(4), Background = hex("0052d9"), BorderRadius = percent(50) });
        global(".ja-starter-user-activity strong", new CssRule { Color = hex("1d2129"), FontSize = px(14) });
        global(".ja-starter-user-activity p", new CssRule { Margin = margin(px(4), px(0), px(0)), Color = hex("86909c"), FontSize = px(12) });
        global(".ja-starter-user-products > div", new CssRule { Display = grid, GridTemplateColumns = tracks(repeat(4, minMax(px(0), fr(1)))), MarginTop = px(18), Gap = px(8), Color = hex("0052d9") });
        global(".ja-starter-user-products .t-icon", new CssRule { Padding = px(8), Background = hex("f3f8ff"), BorderRadius = px(3) });

        global(".ja-starter-login", new CssRule { MinHeight = vh(100) - px(112), Background = hex("fff"), BorderRadius = px(3) });
        global(".ja-starter-login > header", new CssRule { Padding = padding(px(18), px(28)), BorderBottom = px(1) | solid | hex("eeeeee") });
        global(".ja-starter-login > header strong", new CssRule { Display = flex, AlignItems = center, Color = hex("1d2129"), Gap = px(8) });
        global(".ja-starter-login > header img", new CssRule { Width = px(28), Height = px(28) });
        global(".ja-starter-login > header span", new CssRule { Color = hex("86909c"), FontSize = px(13) });
        global(".ja-starter-login main", new CssRule { Display = grid, MinHeight = px(620), Background = hex("f5f9ff"), PlaceItems = center });
        global(".ja-starter-login main > section", new CssRule { Width = min(percent(100) - px(40), px(380)), Padding = px(34), Background = hex("fff"), BorderRadius = px(3), BoxShadow = shadows(new CssShadow(px(0), px(12), Blur: px(30), Color: rgba(0, 82, 217, 0.12))) });
        global(".ja-starter-login__mark", new CssRule { Display = grid, Width = px(48), Height = px(48), Color = hex("fff"), Background = hex("0052d9"), BorderRadius = px(3), PlaceItems = center, FontSize = px(22) });
        global(".ja-starter-login h1", new CssRule { Margin = margin(px(18), px(0), px(8)), Color = hex("1d2129"), FontSize = px(24) });
        global(".ja-starter-login main p", new CssRule { Margin = margin(px(0)), Color = hex("86909c") });
        global(".ja-starter-login form", new CssRule { Display = grid, MarginTop = px(24), Gap = px(16) });
        global(".ja-starter-login form > label", new CssRule { Display = grid, Gap = px(8), Color = hex("4b5b76"), FontSize = px(13) });
        global(".ja-starter-login input", new CssRule { Padding = px(10), Border = px(1) | solid | hex("dcdcdc"), BorderRadius = px(3), Outline = none });
        global(".ja-starter-login form > div", new CssRule { Display = flex, AlignItems = center, JustifyContent = spaceBetween, Color = hex("86909c"), FontSize = px(12) });
        global(".ja-starter-login form > div label", new CssRule { Display = flex, AlignItems = center, Gap = px(6) });
        global(".ja-starter-login form > div button", new CssRule { Padding = padding(px(0)), Color = hex("0052d9"), Border = none });
        global(".ja-starter-login__switch", new CssRule { Padding = important(px(0)), Color = important(hex("0052d9")), Background = important(transparent), Border = important(none) });
        global(".ja-starter-login > footer", new CssRule { Padding = px(16), Color = hex("86909c"), FontSize = px(12), TextAlign = center });

        // TDesign consumes inherited design tokens. Product page shells use the same tokens so
        // switching the header control changes both library controls and checked-in templates.
        global(".ja-application--dark .ja-tdesign-layout [data-shell-region=\"content\"]",
            new CssRule { Background = var("--td-bg-color-page") });
        global(".ja-application--dark .ja-tdesign-layout__header",
            new CssRule { Background = var("--td-bg-color-container") });
        global(".ja-application--dark .ja-starter-search",
            new CssRule { Background = var("--td-bg-color-secondarycontainer") });
        global(".ja-application--dark .ja-starter-search input, .ja-application--dark .ja-starter-organization select, .ja-application--dark .ja-starter-select select",
            new CssRule { Color = var("--td-text-color-primary") });
        global(".ja-application--dark .ja-starter-metric, .ja-application--dark .ja-starter-card, .ja-application--dark .ja-starter-page > section, .ja-application--dark .ja-starter-sheet, .ja-application--dark .ja-starter-user-page > aside > section",
            new CssRule { Background = var("--td-bg-color-container"), Color = var("--td-text-color-primary") });
        global(".ja-application--dark .ja-starter-metric strong, .ja-application--dark .ja-starter-user-activity strong, .ja-application--dark .ja-starter-login h1",
            new CssRule { Color = var("--td-text-color-primary") });

        Media(".ja-starter-dashboard__metrics", "(max-width: 1180px)", new CssRule { GridTemplateColumns = tracks(repeat(2, minMax(px(0), fr(1)))) });
        Media(".ja-starter-output__grid", "(max-width: 1080px)", new CssRule { GridTemplateColumns = tracks(repeat(2, minMax(px(0), fr(1)))) });
        Media(".ja-starter-dashboard__analysis, .ja-starter-dashboard__rankings", "(max-width: 860px)", new CssRule { GridTemplateColumns = tracks(fr(1)) });
        Media(".ja-starter-search", "(max-width: 1120px)", new CssRule { Width = px(150) });
        Media(".ja-starter-user__command span", "(max-width: 980px)", new CssRule { Display = none });
        Media(".ja-starter-organization span, .ja-starter-organization", "(max-width: 900px)", new CssRule { Display = none });
        Media(".ja-starter-dashboard__metrics, .ja-starter-output__grid", "(max-width: 620px)", new CssRule { GridTemplateColumns = tracks(fr(1)) });
        Media(".ja-starter-search", "(max-width: 620px)", new CssRule { Display = none });
        Media(".ja-starter-user", "(max-width: 620px)", new CssRule { MarginLeft = px(2) });
        Media(".ja-starter-user__trigger .t-button__text, .ja-starter-user__trigger .t-button__suffix", "(max-width: 620px)", new CssRule { Display = none });
        return true;
    }

    private static void Media(string selector, string prelude, CssRule rule)
        => global(selector, new CssRule { Children = [new(ChildKind.Media, prelude, rule)] });
}
