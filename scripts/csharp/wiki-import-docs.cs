// wiki-import-docs.cs - docs/ markdown → Wiki 页面目录生成器 / docs/ markdown → Wiki page catalog generator
//
// 把仓库 docs/ 目录的 markdown 文档编译为 samples/Wiki/obj/wiki/WikiDocsContent.g.cs，
// 使 docs/ 成为官网内容的单一事实来源。生成文件只包含 Jazor 可编译的纯数据载体，
// MSBuild 在 docs 变化时自动重新生成。
//
// Usage:
//   dotnet run --file scripts/csharp/wiki-import-docs.cs
//   dotnet run --file scripts/csharp/wiki-import-docs.cs -- --check     // 校验已提交的生成文件是否最新（漂移门禁）
//   dotnet run --file scripts/csharp/wiki-import-docs.cs -- --output <file>
//
// 生成契约（与手写渲染层 WikiHomeModule.DocsPage.cs 对齐）：
//   DocsRun.Kind:   0=text 1=strong 2=em 3=code 4=link 5=inline-anchor(id 存于 Href)
//   DocsBlock.Kind: 0=heading 1=paragraph 2=code 3=list 4=quote 5=rule 6=anchor 7=table
//   DocsBlock.Rows: list → 每元素为条目 runs；table → 每元素为一行、行内每 cell 恰一个 run（text 或 link）

#:package Markdig@*

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Markdig.Extensions.Tables;

// ── 定位仓库根 / Locate repository root ──
// file-based app 的 AppContext.BaseDirectory 位于临时缓存目录，改从当前目录向上找 Jazor.slnx
var repoRootPath = LocateRepositoryRoot(Environment.CurrentDirectory)
    ?? throw new InvalidOperationException(
        "Cannot locate repository root (Jazor.slnx) above '" + Environment.CurrentDirectory +
        "'. Run this script from the repository: dotnet run --file scripts/csharp/wiki-import-docs.cs");

static string? LocateRepositoryRoot(string startDirectory)
{
    var directory = new DirectoryInfo(startDirectory);
    while (directory != null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            return directory.FullName;
        directory = directory.Parent;
    }
    return null;
}

var docsDir = Path.Combine(repoRootPath, "docs");
var outputPath = Path.Combine(repoRootPath, "samples", "Wiki", "obj", "wiki", "WikiDocsContent.g.cs");
var checkOnly = false;

for (var i = 0; i < args.Length; i++)
{
    if (args[i] == "--check")
        checkOnly = true;
    else if (args[i] == "--output" && i + 1 < args.Length)
        outputPath = Path.GetFullPath(args[++i]);
    else if (args[i] == "--docs" && i + 1 < args.Length)
        docsDir = Path.GetFullPath(args[++i]);
    else
        throw new ArgumentException("Unknown argument: " + args[i]);
}

// ── 分组配置：docs 子目录 → 路由分组 / Group config: docs subdir → route group ──
var groups = new (string Dir, string Id, string Label, string Route)[]
{
    ("01-overview", "Overview", "概览", "/overview"),
    ("02-architecture", "Architecture", "架构", "/architecture"),
    ("03-guides", "Guides", "指南", "/guides"),
    ("04-roadmap", "Roadmap", "路线图", "/roadmap"),
    ("05-history", "History", "历史", "/history"),
};

var pipeline = new MarkdownPipelineBuilder()
    .UsePipeTables()
    .Build();

// ── 收集页面：/（docs/README.md）+ /search（工具页）+ 各分组落地页与内容页 ──
var pages = new List<PageModel>
{
    Importer.Import(repoRootPath, Path.Combine(docsDir, "README.md"), "/", "Overview", pipeline, "docs/README.md"),
    PageModel.CreateSearchPlaceholder(),
};

foreach (var group in groups)
{
    var groupDir = Path.Combine(docsDir, group.Dir);
    if (!Directory.Exists(groupDir))
        throw new InvalidOperationException("Missing docs group directory: " + groupDir);

    pages.Add(Importer.Import(
        repoRootPath,
        Path.Combine(groupDir, "README.md"), group.Route, group.Id, pipeline, "docs/" + group.Dir + "/README.md"));

    var contentFiles = Directory.EnumerateFiles(groupDir, "*.md")
        .Where(file => !string.Equals(Path.GetFileName(file), "README.md", StringComparison.OrdinalIgnoreCase))
        .OrderBy(file => Path.GetFileName(file), StringComparer.Ordinal);

    foreach (var file in contentFiles)
    {
        var route = group.Route + "/" + Path.GetFileNameWithoutExtension(file);
        pages.Add(Importer.Import(repoRootPath, file, route, group.Id, pipeline, "docs/" + group.Dir + "/" + Path.GetFileName(file)));
    }
}

// ── 链接目标映射：docs 相对路径 → 站内路由 / Link target map: docs relative path → site route ──
var routeByDocsPath = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
foreach (var page in pages)
{
    if (page.Blocks != null)
        routeByDocsPath[page.SourceFile!] = page.Path;
}

// 第二遍：重写页面内链接 / Second pass: rewrite in-page links
foreach (var page in pages)
{
    if (page.Blocks != null)
        LinkRewriter.Rewrite(page, routeByDocsPath, repoRootPath);
}

// ── 相关页面：严格取同组相邻页 / Related pages: same-group neighbors only ──
for (var index = 0; index < pages.Count; index++)
{
    var page = pages[index];
    if (page.Blocks == null)
    {
        page.RelatedPaths = [];
        continue;
    }

    var siblings = pages
        .Where(candidate => candidate.GroupId == page.GroupId && candidate.Blocks != null)
        .ToList();
    var position = siblings.FindIndex(candidate => candidate.Path == page.Path);
    var related = new List<string>();
    if (position > 0)
        related.Add(siblings[position - 1].Path);
    if (position >= 0 && position < siblings.Count - 1)
        related.Add(siblings[position + 1].Path);

    page.RelatedPaths = related.ToArray();
}

var generated = NormalizeNewlines(Emitter.Emit(pages, groups));

if (checkOnly)
{
    var existing = File.Exists(outputPath) ? File.ReadAllText(outputPath) : "";
    if (NormalizeNewlines(existing) != generated)
    {
        Console.Error.WriteLine("wiki-import-docs: generated catalog is out of date. Run: dotnet run --file scripts/csharp/wiki-import-docs.cs");
        return 2;
    }
    Console.WriteLine("wiki-import-docs: catalog up to date (" + pages.Count + " pages).");
    return 0;
}

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
var previousContent = File.Exists(outputPath) ? File.ReadAllText(outputPath) : "";
if (NormalizeNewlines(previousContent) != generated)
{
    File.WriteAllText(outputPath, generated, new UTF8Encoding(false));
    Console.WriteLine("wiki-import-docs: wrote " + outputPath + " (" + pages.Count + " pages).");
}
else
{
    Console.WriteLine("wiki-import-docs: catalog unchanged (" + pages.Count + " pages).");
}

return 0;

static string NormalizeNewlines(string value)
    => value.Replace("\r\n", "\n", StringComparison.Ordinal);

// ══════════════════════════════ 数据模型 / Models ══════════════════════════════

internal sealed class RunModel
{
    public int Kind;         // 0 text 1 strong 2 em 3 code 4 link 5 inline-anchor
    public string Text = "";
    public string Href = "";
}

internal sealed class BlockModel
{
    public int Kind;         // 0 heading 1 paragraph 2 code 3 list 4 quote 5 rule 6 anchor 7 table
    public int Level;
    public string Text = "";
    public string Code = "";
    public bool Ordered;
    public string AnchorId = "";
    public List<RunModel> Runs = [];
    public List<List<RunModel>> Rows = [];
}

internal sealed class PageModel
{
    public string Path = "";
    public string GroupId = "";
    public string? Title;
    public string? Summary;
    public string? SourceFile;
    public string? LastUpdated;
    public int ReadingMinutes = 1;
    public string SearchBody = "";
    public string[] Tags = [];
    public List<(string Id, string Title)>? Sections;
    public List<BlockModel>? Blocks;
    public string[] RelatedPaths = [];

    public static PageModel CreateSearchPlaceholder()
        => new()
        {
            Path = "/search",
            GroupId = "Overview",
            Title = "搜索",
            Summary = "基于 URL 的全文搜索，覆盖页面元数据、标签和正文内容。",
            LastUpdated = "2000-01-01",
            ReadingMinutes = 1,
            SearchBody = "搜索 全文 查询 query",
            Tags = ["search", "discovery"],
            Sections = [("full-text", "全文搜索"), ("section-hits", "章节匹配"), ("topic-entry", "主题入口"), ("query-sharing", "可分享查询")],
            Blocks = null,
        };
}

// ══════════════════════════════ Markdown 解析 / Markdown parsing ══════════════════════════════

internal static partial class Importer
{
    [GeneratedRegex(@"<a\s+id\s*=\s*[""'](?<id>[A-Za-z0-9_-]+)[""']\s*>")]
    private static partial Regex AnchorIdRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    public static PageModel Import(string repoRootPath, string file, string route, string groupId, MarkdownPipeline pipeline, string repoRelativeSource)
    {
        var markdown = File.ReadAllText(file);
        var document = Markdown.Parse(markdown, pipeline);

        var page = new PageModel
        {
            Path = route,
            GroupId = groupId,
            SourceFile = repoRelativeSource,
            LastUpdated = GitDates.LastUpdated(repoRootPath, repoRelativeSource),
            Tags = [groupId.ToLowerInvariant(), "jazor", "docs"],
        };

        var plainText = new StringBuilder();
        var summaryText = "";
        var sectionIndex = 0;
        var sections = new List<(string, string)>();
        var blocks = new List<BlockModel>();
        var anchorIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var block in document)
            WalkBlock(block, page, blocks, sections, plainText, ref summaryText, ref sectionIndex, anchorIds);

        page.Blocks = blocks;
        // 右侧目录和锚点交互要求每个 docs 页面至少有一个稳定章节。
        // 文档没有 h2 时补一个概览分节，避免在渲染层猜测内容边界。
        if (sections.Count == 0)
        {
            const string defaultSectionId = "section-1";
            sections.Add((defaultSectionId, "概览"));
            blocks.Insert(0, new BlockModel
            {
                Kind = 0,
                Level = 2,
                Text = "概览",
                AnchorId = defaultSectionId,
            });
        }
        page.Sections = sections;
        page.Title ??= Path.GetFileNameWithoutExtension(file);
        page.Summary = summaryText.Length > 0 ? summaryText : BuildSummary(plainText.ToString());
        page.ReadingMinutes = EstimateReadingMinutes(plainText.ToString());
        page.SearchBody = BuildSearchBody(plainText.ToString());

        return page;
    }

    private static void WalkBlock(
        Block block,
        PageModel page,
        List<BlockModel> blocks,
        List<(string, string)> sections,
        StringBuilder plainText,
        ref string summaryText,
        ref int sectionIndex,
        HashSet<string> anchorIds)
    {
        switch (block)
        {
            case HeadingBlock heading:
            {
                var text = InlinePlainText(heading.Inline);
                plainText.Append(text).Append('\n');
                if (heading.Level <= 1)
                {
                    // h1 是页面标题，由 hero 渲染，不进入正文块 / h1 is the hero title
                    if (page.Title == null)
                        page.Title = text;
                    return;
                }

                var blockModel = new BlockModel { Kind = 0, Level = heading.Level, Text = text };
                if (heading.Level == 2)
                {
                    sectionIndex++;
                    blockModel.AnchorId = "section-" + sectionIndex;
                    sections.Add((blockModel.AnchorId, text));
                }
                blocks.Add(blockModel);
                return;
            }
            case ParagraphBlock paragraph:
            {
                var runs = InlineRuns(paragraph.Inline, plainText, anchorIds).ToList();
                // 摘要取首个段落（跳过 h1），避免摘要退化为页面标题
                if (summaryText.Length == 0)
                    summaryText = BuildSummary(string.Concat(runs.Select(run => run.Text)));
                AppendTrimmedParagraph(blocks, runs);
                plainText.Append('\n');
                return;
            }
            case FencedCodeBlock fenced:
            {
                var code = ExtractCode(fenced);
                blocks.Add(new BlockModel { Kind = 2, Text = (fenced.Info ?? "").Trim(), Code = code });
                plainText.Append(code).Append('\n');
                return;
            }
            case CodeBlock simpleCode:
            {
                var code = ExtractCode(simpleCode);
                blocks.Add(new BlockModel { Kind = 2, Text = "", Code = code });
                plainText.Append(code).Append('\n');
                return;
            }
            case ListBlock list:
            {
                var blockModel = new BlockModel { Kind = 3, Ordered = list.IsOrdered };
                CollectListItems(list, blockModel.Rows, plainText, anchorIds);
                blocks.Add(blockModel);
                return;
            }
            case QuoteBlock quote:
            {
                var runs = new List<RunModel>();
                foreach (var inner in quote)
                {
                    if (inner is ParagraphBlock paragraph)
                        runs.AddRange(InlineRuns(paragraph.Inline, plainText, anchorIds));
                }

                // 页面开头的引用块常是“适用范围”说明，作为摘要候选 / Leading quotes are scope notes, good summary candidates
                if (summaryText.Length == 0 && runs.Count > 0)
                    summaryText = BuildSummary(string.Concat(runs.Select(run => run.Text)));

                blocks.Add(new BlockModel { Kind = 4, Runs = runs });
                plainText.Append('\n');
                return;
            }
            case ThematicBreakBlock:
                blocks.Add(new BlockModel { Kind = 5 });
                return;
            case Table table:
            {
                // Markdig 1.x：Table/TableRow/TableCell 均以 Block 子级枚举，需显式类型过滤
                var blockModel = new BlockModel { Kind = 7 };
                foreach (var row in table.OfType<TableRow>())
                {
                    var cells = new List<RunModel>();
                    foreach (var cell in row.OfType<TableCell>())
                    {
                        // 每个 cell 压缩为单个 run：普通文本或链接，其余行内样式降级为文本
                        var cellRuns = new List<RunModel>();
                        foreach (var inner in cell)
                            if (inner is ParagraphBlock paragraph)
                                cellRuns.AddRange(InlineRuns(paragraph.Inline, plainText, anchorIds));

                        var link = cellRuns.FirstOrDefault(run => run.Kind == 4);
                        if (link != null && cellRuns.All(run => run.Kind is 0 or 4))
                        {
                            var label = string.Concat(cellRuns.Where(run => run.Kind == 0 || run == link).Select(run => run.Text)).Trim();
                            cells.Add(new RunModel { Kind = 4, Text = link.Text.Length > 0 ? link.Text : label, Href = link.Href });
                        }
                        else
                        {
                            cells.Add(new RunModel { Kind = 0, Text = string.Concat(cellRuns.Select(run => run.Text)).Trim() });
                        }
                    }
                    blockModel.Rows.Add(cells);
                }
                blocks.Add(blockModel);
                plainText.Append('\n');
                return;
            }
            case HtmlBlock html:
            {
                // Markdig 1.x：HTML 原文位于 Lines 载体 / Raw HTML text lives on the Lines carrier
                var anchor = AnchorIdRegex().Match(ExtractCode(html));
                if (anchor.Success)
                {
                    var id = anchor.Groups["id"].Value;
                    anchorIds.Add(id);
                    blocks.Add(new BlockModel { Kind = 6, AnchorId = id });
                }
                return;
            }
            default:
                return;
        }
    }

    private static void AppendTrimmedParagraph(List<BlockModel> blocks, List<RunModel> runs)
    {
        // 合并首尾用于换行的空白 run，避免正文渲染出多余空格 / Drop leading/trailing whitespace-only runs
        while (runs.Count > 0 && runs[0].Kind == 0 && runs[0].Text.Trim().Length == 0)
            runs.RemoveAt(0);
        while (runs.Count > 0 && runs[^1].Kind == 0 && runs[^1].Text.Trim().Length == 0)
            runs.RemoveAt(runs.Count - 1);

        if (runs.Count > 0)
            blocks.Add(new BlockModel { Kind = 1, Runs = runs });
    }

    private static void CollectListItems(ListBlock list, List<List<RunModel>> rows, StringBuilder plainText, HashSet<string> anchorIds)
    {
        foreach (var item in list)
        {
            if (item is not ListItemBlock listItem)
                continue;

            var itemRuns = new List<RunModel>();
            rows.Add(itemRuns);
            foreach (var inner in listItem)
            {
                switch (inner)
                {
                    case ParagraphBlock paragraph:
                        itemRuns.AddRange(InlineRuns(paragraph.Inline, plainText, anchorIds));
                        break;
                    case ListBlock nested:
                        // 嵌套列表摊平为同级条目，内容不丢失 / Flatten nested items as siblings
                        CollectListItems(nested, rows, plainText, anchorIds);
                        break;
                    case LeafBlock leaf when leaf.Inline != null:
                        itemRuns.AddRange(InlineRuns(leaf.Inline, plainText, anchorIds));
                        break;
                }
            }
            plainText.Append('\n');
        }
    }

    private static List<RunModel> InlineRuns(ContainerInline? container, StringBuilder plainText, HashSet<string> anchorIds)
    {
        var runs = new List<RunModel>();
        if (container == null)
            return runs;

        foreach (var inline in container)
            WalkInline(inline, runs, plainText, anchorIds, kindOverride: 0);

        return runs;
    }

    private static void WalkInline(Inline inline, List<RunModel> runs, StringBuilder plainText, HashSet<string> anchorIds, int kindOverride)
    {
        switch (inline)
        {
            case LiteralInline literal:
            {
                var text = literal.Content.ToString();
                if (text.Length > 0)
                {
                    runs.Add(new RunModel { Kind = kindOverride, Text = text });
                    plainText.Append(text);
                }
                return;
            }
            case CodeInline code:
                runs.Add(new RunModel { Kind = 3, Text = code.Content });
                plainText.Append(code.Content);
                return;
            case EmphasisInline emphasis:
            {
                var kind = emphasis.DelimiterCount == 2 ? 1 : 2;
                // 嵌套强调取外层样式 / Nested emphasis keeps the outer style
                foreach (var child in emphasis)
                    WalkInline(child, runs, plainText, anchorIds, kind);
                return;
            }
            case LinkInline link:
            {
                if (link.IsImage)
                    return;

                var label = new StringBuilder();
                CollectInlineText(link, label);
                plainText.Append(label);
                runs.Add(new RunModel { Kind = 4, Text = label.ToString(), Href = link.Url ?? "" });
                return;
            }
            case AutolinkInline autolink:
                runs.Add(new RunModel { Kind = 4, Text = autolink.Url, Href = autolink.Url });
                plainText.Append(autolink.Url);
                return;
            case HtmlInline html:
            {
                var anchor = AnchorIdRegex().Match(html.Tag ?? "");
                if (anchor.Success)
                {
                    var id = anchor.Groups["id"].Value;
                    anchorIds.Add(id);
                    // 行内锚点在渲染层转为带 id 的空 span / Inline anchor renders as an empty id-carrying span
                    runs.Add(new RunModel { Kind = 5, Text = "", Href = id });
                }
                return;
            }
            case LineBreakInline:
                plainText.Append(' ');
                runs.Add(new RunModel { Kind = 0, Text = " " });
                return;
            case ContainerInline container:
                foreach (var child in container)
                    WalkInline(child, runs, plainText, anchorIds, kindOverride);
                return;
            default:
                return;
        }
    }

    private static void CollectInlineText(ContainerInline container, StringBuilder builder)
    {
        foreach (var inline in container)
        {
            switch (inline)
            {
                case LiteralInline literal:
                    builder.Append(literal.Content.ToString());
                    break;
                case CodeInline code:
                    builder.Append(code.Content);
                    break;
                case ContainerInline childContainer:
                    CollectInlineText(childContainer, builder);
                    break;
            }
        }
    }

    private static string InlinePlainText(ContainerInline? container)
    {
        var builder = new StringBuilder();
        if (container != null)
            CollectInlineText(container, builder);
        return builder.ToString();
    }

    private static string ExtractCode(LeafBlock codeBlock)
    {
        var builder = new StringBuilder();
        var lines = codeBlock.Lines;
        for (var index = 0; index < lines.Count; index++)
        {
            builder.Append(lines.Lines[index].Slice.ToString());
            if (index < lines.Count - 1)
                builder.Append('\n');
        }

        return builder.ToString();
    }

    private static string BuildSummary(string plainText)
    {
        var firstParagraph = plainText.Trim();
        var newline = firstParagraph.IndexOf('\n');
        if (newline > 0)
            firstParagraph = firstParagraph[..newline];

        firstParagraph = WhitespaceRegex().Replace(firstParagraph, " ").Trim();
        if (firstParagraph.Length <= 110)
            return firstParagraph;

        return firstParagraph[..110].TrimEnd() + "…";
    }

    private static string BuildSearchBody(string plainText)
    {
        var normalized = WhitespaceRegex().Replace(plainText, " ").Trim();
        return normalized.Length <= 4000 ? normalized : normalized[..4000];
    }

    private static int EstimateReadingMinutes(string plainText)
    {
        var cjk = 0;
        var wordStart = -1;
        var words = 0;
        for (var index = 0; index < plainText.Length; index++)
        {
            var c = plainText[index];
            if (c >= 0x4E00 && c <= 0x9FFF)
            {
                cjk++;
                if (wordStart >= 0)
                {
                    words++;
                    wordStart = -1;
                }
            }
            else if (char.IsAsciiLetterOrDigit(c))
            {
                if (wordStart < 0)
                    wordStart = index;
            }
            else if (wordStart >= 0)
            {
                words++;
                wordStart = -1;
            }
        }
        if (wordStart >= 0)
            words++;

        var minutes = Math.Ceiling(cjk / 300.0 + words / 160.0);
        return Math.Max(1, (int)minutes);
    }
}

// ══════════════════════════════ 链接重写 / Link rewriting ══════════════════════════════

internal static class LinkRewriter
{
    private const string RepositoryBlobBaseUrl = "https://github.com/devhxj/Jazor/blob/main/";

    public static void Rewrite(PageModel page, Dictionary<string, string> routeByDocsPath, string repoRoot)
    {
        foreach (var block in page.Blocks!)
        {
            RewriteRuns(block.Runs, page, routeByDocsPath, repoRoot);
            foreach (var row in block.Rows)
                RewriteRuns(row, page, routeByDocsPath, repoRoot);
        }
    }

    private static void RewriteRuns(List<RunModel> runs, PageModel page, Dictionary<string, string> routeByDocsPath, string repoRoot)
    {
        foreach (var run in runs)
        {
            if (run.Kind == 4 && run.Href.Length > 0)
                run.Href = ResolveHref(run.Href, page, routeByDocsPath, repoRoot);
        }
    }

    private static string ResolveHref(string href, PageModel page, Dictionary<string, string> routeByDocsPath, string repoRoot)
    {
        if (href.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            href.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
            return href;

        var fragment = "";
        var target = href;
        var hashIndex = target.IndexOf('#');
        if (hashIndex >= 0)
        {
            fragment = target[hashIndex..];
            target = target[..hashIndex];
        }

        // 页内锚点（对应 <a id> 显式锚或渲染层生成的章节锚） / Same-page anchor
        if (target.Length == 0)
            return fragment;

        // 相对路径基于当前 docs 文件目录解析为仓库相对路径，不依赖进程 CWD / Pure segment normalization, CWD-independent
        var repoRelative = ResolveRepoRelative(page.SourceFile!, target);
        if (repoRelative == null)
        {
            Console.WriteLine("wiki-import-docs: warning: link escapes repository root '" + href + "' in " + page.SourceFile);
            return href;
        }

        if (routeByDocsPath.TryGetValue(repoRelative, out var route))
            return route + fragment;

        if (File.Exists(Path.Combine(repoRoot, repoRelative.Replace('/', Path.DirectorySeparatorChar))))
            return RepositoryBlobBaseUrl + repoRelative + fragment;

        Console.WriteLine("wiki-import-docs: warning: unresolved link '" + href + "' in " + page.SourceFile);
        return href;
    }

    private static string? ResolveRepoRelative(string pageSourceFile, string target)
    {
        // 纯 '/' 分段归一化：Windows 的 Path.GetDirectoryName 会把 '/' 规范为 '\'，破坏 ".." 弹段
        var normalizedSource = pageSourceFile.Replace('\\', '/');
        var lastSlash = normalizedSource.LastIndexOf('/');
        var baseDir = lastSlash < 0 ? "" : normalizedSource[..lastSlash];
        var segments = (baseDir.Length == 0 ? target : baseDir + "/" + target).Split('/');

        var stack = new List<string>();
        foreach (var segment in segments)
        {
            if (segment.Length == 0 || segment == ".")
                continue;
            if (segment == "..")
            {
                // 弹到仓库根之上的 ".." 视为抵达根（docs -> 仓库根的合法跳转）
                if (stack.Count > 0)
                    stack.RemoveAt(stack.Count - 1);
                continue;
            }
            stack.Add(segment);
        }

        return string.Join("/", stack);
    }
}

// ══════════════════════════════ 生成发射 / C# emission ══════════════════════════════

internal static class Emitter
{
    public static string Emit(IReadOnlyList<PageModel> pages, (string Dir, string Id, string Label, string Route)[] groups)
    {
        var builder = new StringBuilder(96 * 1024);
        builder.AppendLine("// WikiDocsContent.g.cs - 由 scripts/csharp/wiki-import-docs.cs 生成，请勿手动编辑");
        builder.AppendLine("// AUTO-GENERATED from docs/ by wiki-import-docs.cs. Do not edit by hand; edit docs/*.md and rerun the importer.");
        builder.AppendLine("// 内容块与行内 run 的 Kind 常量见 WikiHomeModule.DocsPage.cs 的 DocsBlock*/DocsRun* 常量。");
        builder.AppendLine("namespace Wiki;");
        builder.AppendLine();
        // Jazor 只会翻译模块类型及其嵌套类型；数据载体必须留在 WikiHomeModule 内。
        builder.AppendLine("public static partial class WikiHomeModule");
        builder.AppendLine("{");
        builder.AppendLine("    internal sealed class DocsRun");
        builder.AppendLine("    {");
        builder.AppendLine("        public int Kind;");
        builder.AppendLine("        public string Text = \"\";");
        builder.AppendLine("        public string Href = \"\";");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    internal sealed class DocsBlock");
        builder.AppendLine("    {");
        builder.AppendLine("        public int Kind;");
        builder.AppendLine("        public int Level;");
        builder.AppendLine("        public string Text = \"\";");
        builder.AppendLine("        public string Code = \"\";");
        builder.AppendLine("        public bool Ordered;");
        builder.AppendLine("        public string AnchorId = \"\";");
        builder.AppendLine("        public DocsRun[] Runs = [];");
        builder.AppendLine("        public DocsRun[][] Rows = [];");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    internal sealed class WikiDocsContent");
        builder.AppendLine("{");

        // 逐页正文块（先于聚合数组声明，保证静态字段初始化顺序） / Per-page block arrays first for init order
        for (var pageIndex = 0; pageIndex < pages.Count; pageIndex++)
        {
            var page = pages[pageIndex];
            if (page.Blocks == null)
                continue;

            builder.AppendLine("    private static readonly DocsBlock[] DocsBlocks" + pageIndex + " =");
            builder.AppendLine("    [");
            for (var blockIndex = 0; blockIndex < page.Blocks.Count; blockIndex++)
            {
                builder.Append("        ");
                AppendBlock(builder, page.Blocks[blockIndex]);
                builder.AppendLine(blockIndex < page.Blocks.Count - 1 ? "," : "");
            }
            builder.AppendLine("    ];");
            builder.AppendLine();
        }

        // 正文分发：PageBlockSets[pageIndex] 供渲染层 RenderDocsPage 消费；搜索页为空数组 / Body data for RenderDocsPage; search page is empty
        builder.AppendLine("    internal static readonly DocsBlock[][] PageBlockSets =");
        builder.AppendLine("    [");
        for (var pageIndex = 0; pageIndex < pages.Count; pageIndex++)
        {
            var page = pages[pageIndex];
            builder.Append("        ");
            if (page.Blocks == null)
            {
                builder.AppendLine("[],");
            }
            else
            {
                builder.AppendLine("DocsBlocks" + pageIndex + ",");
            }
        }
        builder.AppendLine("    ];");
        builder.AppendLine();
        builder.AppendLine("    // 导航分组（顺序即侧边栏顺序） / Navigation groups in sidebar order");
        builder.AppendLine("    internal static readonly string[] NavGroupIds =");
        builder.AppendLine("    [");
        foreach (var group in groups)
            builder.AppendLine("        " + CsString(group.Id) + ",");
        builder.AppendLine("    ];");
        builder.AppendLine();
        builder.AppendLine("    internal static readonly string[] NavGroupLabels =");
        builder.AppendLine("    [");
        foreach (var group in groups)
            builder.AppendLine("        " + CsString(group.Label) + ",");
        builder.AppendLine("    ];");
        builder.AppendLine();
        builder.AppendLine("    internal static readonly string[] NavGroupLandingPaths =");
        builder.AppendLine("    [");
        foreach (var group in groups)
            builder.AppendLine("        " + CsString(group.Route) + ",");
        builder.AppendLine("    ];");
        builder.AppendLine();

        AppendParallelArray(builder, "PagePaths", pages, page => CsString(page.Path));
        AppendParallelArray(builder, "PageGroups", pages, page => CsString(page.GroupId));
        AppendParallelArray(builder, "PageTitles", pages, page => CsString(page.Title!));
        AppendParallelArray(builder, "PageSummaries", pages, page => CsString(page.Summary!));
        AppendParallelArray(builder, "PageStatuses", pages, page => CsString(GroupLabel(groups, page.GroupId)));
        AppendParallelArray(builder, "PageOwners", pages, page => page.Blocks == null ? CsString("Jazor") : CsString("Jazor 文档"));
        AppendParallelArray(builder, "PageAudiences", pages, page => CsString(AudienceOf(page.GroupId)));
        AppendParallelArray(builder, "PageSourceFiles", pages, page => page.SourceFile == null ? CsString("samples/Wiki/WikiHomeModule.Search.cs") : CsString(page.SourceFile));
        AppendParallelArray(builder, "PageLastUpdatedDates", pages, page => CsString(page.LastUpdated!));
        AppendParallelArray(builder, "PageReadingMinutes", pages, page => page.ReadingMinutes.ToString());
        AppendParallelArray(builder, "PageSearchBodies", pages, page => CsString(page.SearchBody));

        builder.AppendLine("    internal static readonly string[][] PageTagSets =");
        builder.AppendLine("    [");
        foreach (var page in pages)
            builder.AppendLine("        [" + string.Join(", ", page.Tags.Select(CsString)) + "],");
        builder.AppendLine("    ];");
        builder.AppendLine();

        builder.AppendLine("    internal static readonly string[][] PageSectionIdSets =");
        builder.AppendLine("    [");
        foreach (var page in pages)
            builder.AppendLine("        [" + string.Join(", ", page.Sections!.Select(section => CsString(section.Id))) + "],");
        builder.AppendLine("    ];");
        builder.AppendLine();

        builder.AppendLine("    internal static readonly string[][] PageSectionTitleSets =");
        builder.AppendLine("    [");
        foreach (var page in pages)
            builder.AppendLine("        [" + string.Join(", ", page.Sections!.Select(section => CsString(section.Title))) + "],");
        builder.AppendLine("    ];");
        builder.AppendLine();

        builder.AppendLine("    internal static readonly string[][] PageRelatedPathSets =");
        builder.AppendLine("    [");
        foreach (var page in pages)
            builder.AppendLine("        [" + string.Join(", ", page.RelatedPaths.Select(CsString)) + "],");
        builder.AppendLine("    ];");
        builder.AppendLine();

        builder.AppendLine("    internal static string GetGroupLabel(string groupId)");
        builder.AppendLine("    {");
        for (var index = 0; index < groups.Length; index++)
        {
            builder.AppendLine("        if (groupId == " + CsString(groups[index].Id) + ")");
            builder.AppendLine("            return " + CsString(groups[index].Label) + ";");
        }
        builder.AppendLine("        return groupId;");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static string GroupLabel((string Dir, string Id, string Label, string Route)[] groups, string groupId)
        => groups.FirstOrDefault(group => group.Id == groupId).Label;

    private static string AudienceOf(string groupId)
        => groupId switch
        {
            "Overview" => "所有读者",
            "Architecture" => "贡献者与高级用户",
            "Guides" => "所有用户",
            "Roadmap" => "维护者与贡献者",
            "History" => "维护者",
            _ => "所有读者",
        };

    private static void AppendParallelArray(StringBuilder builder, string name, IReadOnlyList<PageModel> pages, Func<PageModel, string> render)
    {
        builder.AppendLine("    internal static readonly " + (name == "PageReadingMinutes" ? "int[]" : "string[]") + " " + name + " =");
        builder.AppendLine("    [");
        foreach (var page in pages)
            builder.AppendLine("        " + render(page) + ",");
        builder.AppendLine("    ];");
        builder.AppendLine();
    }

    private static string CsString(string value)
    {
        // 控制字符清洗，保证生成文件始终是可读文本 / Sanitize control characters for readable output
        var builder = new StringBuilder(value.Length + 8);
        foreach (var c in value)
        {
            switch (c)
            {
                case '\n':
                    builder.Append("\\n");
                    break;
                case '\t':
                    builder.Append("\\t");
                    break;
                case '"':
                    builder.Append("\\\"");
                    break;
                case '\\':
                    builder.Append("\\\\");
                    break;
                case < ' ':
                    builder.Append(' ');
                    break;
                default:
                    builder.Append(c);
                    break;
            }
        }
        return "\"" + builder + "\"";
    }

    private static void AppendBlock(StringBuilder builder, BlockModel block)
    {
        builder.Append("new DocsBlock { Kind = ").Append(block.Kind);
        builder.Append(", Level = ").Append(block.Level);
        builder.Append(", Text = ").Append(CsString(block.Text));
        builder.Append(", Code = ").Append(CsString(block.Code));
        builder.Append(", Ordered = ").Append(block.Ordered ? "true" : "false");
        builder.Append(", AnchorId = ").Append(CsString(block.AnchorId));
        builder.Append(", Runs = [");
        AppendRuns(builder, block.Runs);
        builder.Append("], Rows = [");
        for (var rowIndex = 0; rowIndex < block.Rows.Count; rowIndex++)
        {
            if (rowIndex > 0)
                builder.Append(", ");
            builder.Append("[");
            AppendRuns(builder, block.Rows[rowIndex]);
            builder.Append("]");
        }
        builder.Append("] }");
    }

    private static void AppendRuns(StringBuilder builder, List<RunModel> runs)
    {
        for (var index = 0; index < runs.Count; index++)
        {
            if (index > 0)
                builder.Append(", ");
            var run = runs[index];
            builder.Append("new DocsRun { Kind = ").Append(run.Kind);
            builder.Append(", Text = ").Append(CsString(run.Text));
            builder.Append(", Href = ").Append(CsString(run.Href));
            builder.Append(" }");
        }
    }
}

// ══════════════════════════════ Git 日期 / Git dates ══════════════════════════════

internal static class GitDates
{
    public static string LastUpdated(string repoRootPath, string repoRelativeSource)
    {
        // 逐文件提交日期；浅克隆无命中时回退 HEAD 日期，再回退固定日期保证守卫可解析
        if (TryQuery(repoRootPath, ["log", "-1", "--format=%as", "--", repoRelativeSource], out var fileDate))
            return fileDate;
        if (TryQuery(repoRootPath, ["log", "-1", "--format=%as"], out var headDate))
            return headDate;
        return "2000-01-01";
    }

    private static bool TryQuery(string repoRootPath, string[] arguments, out string value)
    {
        value = "";
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                WorkingDirectory = repoRootPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            foreach (var argument in arguments)
                startInfo.ArgumentList.Add(argument);

            using var process = Process.Start(startInfo);
            if (process == null)
                return false;

            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit(5000);
            if (process.ExitCode != 0 || output.Length != 10 || !DateOnly.TryParse(output, out var parsed))
                return false;

            value = parsed.ToString("yyyy-MM-dd");
            return true;
        }
        catch
        {
            return false;
        }
    }
}
