// WikiHomeModule.DocsPage.cs - docs/ markdown 内容渲染层 / docs/ markdown content renderer
// 把 obj/wiki/WikiDocsContent.g.cs 生成的 DocsBlock 数据渲染为 H() VNode 树。
// h2 起聚合为带锚点的 PageSection（与右侧目录联动），首个 h2 之前的内容进入 intro 容器。
using System.Collections.Generic;
using ECMAScript;
using static ECMAScript.Vue;

namespace Wiki;

public static partial class WikiHomeModule
{
    // ── 生成数据 Kind 常量（与 wiki-import-docs.cs 的生成契约对齐，勿单独改动） ──
    private const int DocsRunText = 0;
    private const int DocsRunStrong = 1;
    private const int DocsRunEm = 2;
    private const int DocsRunCode = 3;
    private const int DocsRunLink = 4;
    private const int DocsRunInlineAnchor = 5;

    private const int DocsBlockHeading = 0;
    private const int DocsBlockParagraph = 1;
    private const int DocsBlockCode = 2;
    private const int DocsBlockList = 3;
    private const int DocsBlockQuote = 4;
    private const int DocsBlockRule = 5;
    private const int DocsBlockAnchor = 6;
    private const int DocsBlockTable = 7;

    // 渲染 docs 页面正文：h2 分节 + intro 引导块 / Render docs page body: h2 sections + intro blocks
    internal static IVNode RenderDocsPage(int pageIndex)
    {
        var blocks = PageBlockSets[pageIndex];
        var children = new List<IVNode>();
        var introChildren = new List<IVNode>();
        var sectionChildren = new List<IVNode>();
        var currentTarget = introChildren;
        var sectionId = "";
        var sectionTitle = "";
        var inSection = false;

        for (var blockIndex = 0; blockIndex < blocks.Length; blockIndex++)
        {
            var block = blocks[blockIndex];
            // 首页的首个引用由 Hero 摘要承载；正文再次渲染会形成重复的首屏陈述。
            if (PagePaths[pageIndex] == OverviewPath && blockIndex == 0 && block.Kind == DocsBlockQuote)
                continue;

            if (block.Kind == DocsBlockHeading && block.Level == 2)
            {
                FlushDocsSection(children, introChildren, sectionChildren, sectionId, sectionTitle, ref inSection);
                sectionId = block.AnchorId;
                sectionTitle = block.Text;
                inSection = true;
                currentTarget = sectionChildren;
                continue;
            }

            var rendered = RenderDocsBlock(block);
            if (rendered != null)
                currentTarget.Add(rendered);
        }

        FlushDocsSection(children, introChildren, sectionChildren, sectionId, sectionTitle, ref inSection);

        return H("div", new VueObject { Class = "doc-body" }, children.ToArray());
    }

    // 首页首屏从 docs/README.md 的前三个一级章节提取引导语，避免页面文案出现第二个事实来源。
    internal static IVNode RenderHomeHeroBrief(int pageIndex)
    {
        var blocks = PageBlockSets[pageIndex];
        var items = new List<IVNode>();

        for (var blockIndex = 0; blockIndex < blocks.Length && items.Count < 3; blockIndex++)
        {
            var block = blocks[blockIndex];
            if (block.Kind != DocsBlockHeading || block.Level != 2)
                continue;

            var leadRuns = FindSectionLeadRuns(blocks, blockIndex + 1, block.Text);

            items.Add(H("a", new VueObject
            {
                Class = "home-brief-item",
                Href = BuildBrowserUrl(OverviewPath, block.AnchorId, ""),
                Events = CreateTocClickEvents()
            },
            [
                H("span", new VueObject { Class = "home-brief-title" }, block.Text),
                H("span", new VueObject { Class = "home-brief-copy" }, RenderDocsRuns(leadRuns))
            ]));
        }

        if (items.Count != 3)
            throw new Error("The homepage brief requires the first three docs/README.md sections.");

        return H("div", new VueObject { Class = "home-brief" }, items.ToArray());
    }

    private static DocsRun[] FindSectionLeadRuns(DocsBlock[] blocks, int startIndex, string sectionTitle)
    {
        for (var blockIndex = startIndex; blockIndex < blocks.Length; blockIndex++)
        {
            var block = blocks[blockIndex];
            if (block.Kind == DocsBlockHeading && block.Level == 2)
                break;

            if (block.Kind == DocsBlockParagraph && block.Runs.Length > 0)
                return block.Runs;
        }

        throw new Error(
            "Homepage section '" + sectionTitle + "' must begin with a Markdown paragraph for the homepage brief.");
    }

    // 收束当前分节（或首个 h2 前的 intro 容器） / Flush current section or the pre-h2 intro container
    private static void FlushDocsSection(
        List<IVNode> children,
        List<IVNode> introChildren,
        List<IVNode> sectionChildren,
        string sectionId,
        string sectionTitle,
        ref bool inSection)
    {
        if (!inSection)
        {
            if (introChildren.Count > 0)
            {
                children.Add(H("div", new VueObject { Class = "doc-section doc-section-intro" }, introChildren.ToArray()));
                introChildren.Clear();
            }
            return;
        }

        if (sectionChildren.Count > 0)
        {
            children.Add(PageSection(sectionId, sectionTitle, sectionChildren.ToArray()));
            sectionChildren.Clear();
        }

        inSection = false;
    }

    private static IVNode? RenderDocsBlock(DocsBlock block)
    {
        switch (block.Kind)
        {
            case DocsBlockHeading:
            {
                var headingTag = block.Level >= 4 ? "h4" : "h3";
                return H(headingTag, new VueObject { Class = "md-heading md-heading-" + block.Level }, block.Text);
            }
            case DocsBlockParagraph:
                return H("p", new VueObject { Class = "md-paragraph" }, RenderDocsRuns(block.Runs));
            case DocsBlockCode:
            {
                // mermaid 图示降级为带说明的代码块 / Mermaid diagrams degrade to labeled code blocks
                var label = block.Text.Length == 0 ? "代码" : block.Text;
                if (block.Text == "mermaid")
                    label = "mermaid 图（源码）";
                return CodeBlock(label, block.Code);
            }
            case DocsBlockList:
            {
                var listTag = block.Ordered ? "ol" : "ul";
                var listClassName = block.Ordered ? "md-list md-list-ordered" : "md-list";
                var items = new List<IVNode>();
                for (var itemIndex = 0; itemIndex < block.Rows.Length; itemIndex++)
                    items.Add(H("li", new VueObject { Class = "md-list-item" }, RenderDocsRuns(block.Rows[itemIndex])));

                return H(listTag, new VueObject { Class = listClassName }, items.ToArray());
            }
            case DocsBlockQuote:
                return H("blockquote", new VueObject { Class = "md-quote" },
                [
                    H("p", new VueObject { Class = "md-quote-text" }, RenderDocsRuns(block.Runs))
                ]);
            case DocsBlockRule:
                return H("hr", new VueObject { Class = "md-rule" }, "");
            case DocsBlockAnchor:
                return H("span", new VueObject { Id = block.AnchorId, Class = "md-anchor" }, "");
            case DocsBlockTable:
                return RenderDocsTable(block);
            default:
                return null;
        }
    }

    private static IVNode RenderDocsTable(DocsBlock block)
    {
        var rows = block.Rows;
        if (rows.Length == 0)
            return H("table", new VueObject { Class = "md-table" }, "");

        var headerCells = new List<IVNode>();
        for (var cellIndex = 0; cellIndex < rows[0].Length; cellIndex++)
            headerCells.Add(H("th", new VueObject { Class = "md-table-head" }, RenderDocsSingleRun(rows[0][cellIndex])));

        var bodyRows = new List<IVNode>();
        for (var rowIndex = 1; rowIndex < rows.Length; rowIndex++)
        {
            var cells = new List<IVNode>();
            for (var cellIndex = 0; cellIndex < rows[rowIndex].Length; cellIndex++)
                cells.Add(H("td", new VueObject { Class = "md-table-cell" }, RenderDocsSingleRun(rows[rowIndex][cellIndex])));

            bodyRows.Add(H("tr", new VueObject { Class = "md-table-row" }, cells.ToArray()));
        }

        return H("div", new VueObject { Class = "md-table-shell" },
        [
            H("table", new VueObject { Class = "md-table" },
            [
                H("thead", [H("tr", headerCells.ToArray())]),
                H("tbody", bodyRows.ToArray())
            ])
        ]);
    }

    // 表格 cell 在生成契约中压缩为单个 run / Table cells are single runs by generator contract
    private static IVNode[] RenderDocsSingleRun(DocsRun run)
        => run.Kind == DocsRunText && run.Text.Length > 0
            ? [H("span", run.Text)]
            : RenderDocsRuns([run]);

    private static IVNode[] RenderDocsRuns(DocsRun[] runs)
    {
        var nodes = new List<IVNode>();
        for (var runIndex = 0; runIndex < runs.Length; runIndex++)
        {
            var run = runs[runIndex];
            switch (run.Kind)
            {
                case DocsRunStrong:
                    nodes.Add(H("strong", new VueObject { Class = "md-strong" }, run.Text));
                    break;
                case DocsRunEm:
                    nodes.Add(H("em", new VueObject { Class = "md-em" }, run.Text));
                    break;
                case DocsRunCode:
                    nodes.Add(H("code", new VueObject { Class = "md-inline-code" }, run.Text));
                    break;
                case DocsRunLink:
                {
                    var link = RenderDocsLink(run);
                    if (link != null)
                        nodes.Add(link);
                    break;
                }
                case DocsRunInlineAnchor:
                    nodes.Add(H("span", new VueObject { Id = run.Href, Class = "md-anchor" }, ""));
                    break;
                default:
                    if (run.Text.Length > 0)
                        nodes.Add(H("span", new VueObject { Class = "md-text" }, run.Text));
                    break;
            }
        }

        return nodes.ToArray();
    }

    private static IVNode? RenderDocsLink(DocsRun run)
    {
        if (run.Href.Length == 0)
            return H("span", new VueObject { Class = "md-text" }, run.Text);

        // 站内路由走 SPA 导航；带 #frag 时使用 TOC 点击语义（滚动到锚点）
        if (run.Href.StartsWith("/", StringComparison.Ordinal))
        {
            var hashIndex = run.Href.IndexOf('#');
            var targetPath = hashIndex < 0 ? run.Href : run.Href.Substring(0, hashIndex);
            var targetHash = hashIndex < 0 ? "" : run.Href.Substring(hashIndex + 1);
            var normalizedPath = NormalizePath(targetPath);
            var isKnown = normalizedPath == SearchPath || IsKnownPage(normalizedPath);
            var className = isKnown ? "md-link" : "md-link md-link-external";
            if (!isKnown)
                return H("a", new VueObject
                {
                    Class = className,
                    Href = run.Href,
                    Target = "_blank",
                    Rel = "noreferrer"
                }, run.Text);

            return H("a", new VueObject
            {
                Class = className,
                Href = BuildBrowserUrl(normalizedPath, targetHash, ""),
                Events = targetHash.Length > 0 ? CreateTocClickEvents() : CreateRouteClickEvents()
            }, run.Text);
        }

        // 锚点链接（页内显式 <a id>）/ Fragment-only link to explicit in-page anchors
        if (run.Href.StartsWith("#", StringComparison.Ordinal))
        {
            var anchorId = NormalizeHash(run.Href);
            return H("a", new VueObject
            {
                Class = "md-link",
                Href = BuildBrowserUrl(GetCurrentPathRef()?.Value ?? "/", anchorId, ""),
                Events = CreateTocClickEvents()
            }, run.Text);
        }

        return H("a", new VueObject
        {
            Class = "md-link md-link-external",
            Href = run.Href,
            Target = "_blank",
            Rel = "noreferrer"
        }, run.Text);
    }
}
