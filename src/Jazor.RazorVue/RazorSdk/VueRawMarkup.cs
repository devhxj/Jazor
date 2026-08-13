using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>
/// Analyzes raw Razor markup for Vue Static vnode framing and exposes the shared runtime ABI.
/// 用于计算静态 HTML 顶层节点数，并把动态 MarkupString 接到按需加载的 Vue runtime helper。
/// </summary>
/// <remarks>
/// This type never lowers C# expressions. It only owns HTML-fragment facts that Vue needs for
/// hydration; expression evaluation and CLR semantics remain in <c>SemanticWalker</c>.
/// </remarks>
internal static class VueRawMarkup
{
    public const string CreateRawMarkupName = "__jazor$createRawMarkup";

    public const string RuntimeExportName = "createRawMarkup";

    public const string RuntimeModuleSpecifier = "@jazor/vue-runtime/raw-markup.mjs";

    /// <summary>
    /// Parses one static fragment with an HTML5 parser so <c>staticCount</c> matches DOM siblings.
    /// 字符串扫描无法正确处理 raw-text、template 和隐式节点，因此这里只接受 parser 结果。
    /// </summary>
    public static StaticRawMarkupAnalysis AnalyzeStatic(string markup)
    {
        if (markup.Length == 0)
            return new StaticRawMarkupAnalysis(0, CanHydrateAsStaticVNode: false);

        var parser = new HtmlParser();
        var document = parser.ParseDocument("<!doctype html><html><body></body></html>");
        var context = document.CreateElement("template");
        var nodes = parser.ParseFragment(markup, context);
        if (nodes.Length == 0)
            return new StaticRawMarkupAnalysis(0, CanHydrateAsStaticVNode: false);

        // Vue's hydration branch accepts a Static vnode only when its first hydrated node is
        // an element or text node. A leading comment needs explicit comment/Fragment framing.
        // Vue hydrate 对 Static 首节点只接受 element/text；leading comment 必须走共享 helper。
        var firstNodeType = nodes[0].NodeType;
        return new StaticRawMarkupAnalysis(
            nodes.Length,
            firstNodeType is NodeType.Element or NodeType.Text);
    }
}

/// <summary>
/// Carries the two HTML facts needed to choose the direct Static vnode fast path.
/// 只承载 cardinality 与 hydration 可用性，不保存或重写原始 C# expression。
/// </summary>
internal readonly record struct StaticRawMarkupAnalysis(
    int NodeCount,
    bool CanHydrateAsStaticVNode);
