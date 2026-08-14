using System.ComponentModel;

namespace ECMAScript;

/// <summary>
/// Vue Devtools 顶层 custom tab 与 command palette 的公开结构化 contract。
/// 它们属于官方 <c>@vue/devtools-api</c>，不依赖 Devtools browser-extension 内部 RPC。
/// </summary>
public static partial class VueDevtools
{
    /// <summary>custom tab 在 Devtools 内的分类。</summary>
    [String]
    public enum TabCategory
    {
        /// <summary>固定在侧栏前部。Pinned area.</summary>
        [Description("@#pinned")]
        Pinned,

        /// <summary>当前 app 的 tab 区。Application area.</summary>
        [Description("@#app")]
        App,

        /// <summary>模块 tab 区。Modules area.</summary>
        [Description("@#modules")]
        Modules,

        /// <summary>高级功能 tab 区。Advanced area.</summary>
        [Description("@#advanced")]
        Advanced
    }

    /// <summary>custom tab 可使用的 Devtools view 类型。</summary>
    [ECMAScript]
    [Description("@#")]
    public readonly union ModuleView(IframeView, VNodeView, SfcView)
    {
    }

    /// <summary>iframe custom tab view 的输入形状。</summary>
    [ECMAScript]
    [Description("@#")]
    public record IframeViewOptions : Vue.VueProps
    {
        /// <summary>iframe URL。</summary>
        [Description("@#src")]
        public string Src { get; init; } = default!;

        /// <summary>tab 非活动时是否保持 iframe 实例。</summary>
        [Description("@#persistent")]
        public bool? Persistent { get; init; }
    }

    /// <summary>以 iframe 承载 custom tab 的 opaque view result。</summary>
    [ECMAScript]
    [Description("@#")]
    public sealed record IframeView : Vue.VueProps
    {
        private IframeView()
        {
        }

        /// <summary>创建带固定 <c>type: "iframe"</c> literal 的 custom tab view。</summary>
        [ECMAScriptInline("Object.assign({ type: \"iframe\" }, __arg1)")]
        public extern static IframeView Create(IframeViewOptions options);
    }

    /// <summary>VNode custom tab view 的输入形状。</summary>
    [ECMAScript]
    [Description("@#")]
    public record VNodeViewOptions : Vue.VueProps
    {
        /// <summary>需要可序列化且静态的 Vue VNode。</summary>
        [Description("@#vnode")]
        public Vue.IVNode VNode { get; init; } = default!;
    }

    /// <summary>以 Vue VNode 承载 custom tab 的 opaque view result。</summary>
    [ECMAScript]
    [Description("@#")]
    public sealed record VNodeView : Vue.VueProps
    {
        private VNodeView()
        {
        }

        /// <summary>创建带固定 <c>type: "vnode"</c> literal 的 custom tab view。</summary>
        [ECMAScriptInline("Object.assign({ type: \"vnode\" }, __arg1)")]
        public extern static VNodeView Create(VNodeViewOptions options);
    }

    /// <summary>SFC custom tab view 的输入形状。</summary>
    [ECMAScript]
    [Description("@#")]
    public record SfcViewOptions : Vue.VueProps
    {
        /// <summary>Devtools client 负责解析的 SFC 内容。</summary>
        [Description("@#sfc")]
        public string Sfc { get; init; } = default!;
    }

    /// <summary>以 SFC 源码字符串承载 custom tab 的 opaque view result。</summary>
    [ECMAScript]
    [Description("@#")]
    public sealed record SfcView : Vue.VueProps
    {
        private SfcView()
        {
        }

        /// <summary>创建带固定 <c>type: "sfc"</c> literal 的 custom tab view。</summary>
        [ECMAScriptInline("Object.assign({ type: \"sfc\" }, __arg1)")]
        public extern static SfcView Create(SfcViewOptions options);
    }

    /// <summary>使用 <see cref="AddCustomTab"/> 注册的 custom tab。</summary>
    [ECMAScript]
    [Description("@#")]
    public record CustomTab : Vue.VueProps
    {
        /// <summary>tab 唯一名称。Stable unique tab name.</summary>
        [Description("@#name")]
        public string Name { get; init; } = default!;

        /// <summary>可选 Iconify icon 或图像 URL。</summary>
        [Description("@#icon")]
        public string? Icon { get; init; }

        /// <summary>显示在 tab 上的标题。</summary>
        [Description("@#title")]
        public string Title { get; init; } = default!;

        /// <summary>tab 的主 view。</summary>
        [Description("@#view")]
        public ModuleView View { get; init; } = default!;

        /// <summary>可选的 Devtools tab 分类。</summary>
        [Description("@#category")]
        public TabCategory? Category { get; init; }
    }

    /// <summary>URL custom command action 的输入形状。</summary>
    [ECMAScript]
    [Description("@#")]
    public record CustomCommandUrlActionOptions : Vue.VueProps
    {
        /// <summary>执行 command 时打开的 URL。</summary>
        [Description("@#src")]
        public string Src { get; init; } = default!;
    }

    /// <summary>当前官方 public API 支持的 URL custom command action opaque result。</summary>
    [ECMAScript]
    [Description("@#")]
    public sealed record CustomCommandUrlAction : Vue.VueProps
    {
        private CustomCommandUrlAction()
        {
        }

        /// <summary>创建带固定 <c>type: "url"</c> literal 的 command action。</summary>
        [ECMAScriptInline("Object.assign({ type: \"url\" }, __arg1)")]
        public extern static CustomCommandUrlAction Create(CustomCommandUrlActionOptions options);
    }

    /// <summary>
    /// custom command 的一级子项。官方 API 明确排除嵌套 <c>children</c>，
    /// 所以它不是 <see cref="CustomCommand"/> 的递归别名。
    /// </summary>
    [ECMAScript]
    [Description("@#")]
    public record CustomCommandChild : Vue.VueProps
    {
        /// <summary>子 command 的唯一 id。Stable unique child command id.</summary>
        [Description("@#id")]
        public string Id { get; init; } = default!;

        /// <summary>子 command 显示标题。</summary>
        [Description("@#title")]
        public string Title { get; init; } = default!;

        /// <summary>可选 command 描述。</summary>
        [Description("@#description")]
        public string? Description { get; init; }

        /// <summary>排序权重，值越大越靠前。</summary>
        [Description("@#order")]
        public int? Order { get; init; }

        /// <summary>可选 Iconify icon 或图像 URL。</summary>
        [Description("@#icon")]
        public string? Icon { get; init; }

        /// <summary>执行子 command 的 URL action。</summary>
        [Description("@#action")]
        public CustomCommandUrlAction? Action { get; init; }
    }

    /// <summary>
    /// Devtools command palette 中的 command。<see cref="Children"/> 非空时，
    /// 上游会忽略 <see cref="Action"/>，因此二者不应同时用于同一业务命令。
    /// </summary>
    [ECMAScript]
    [Description("@#")]
    public record CustomCommand : Vue.VueProps
    {
        /// <summary>command 唯一 id。Stable unique command id.</summary>
        [Description("@#id")]
        public string Id { get; init; } = default!;

        /// <summary>command 显示标题。</summary>
        [Description("@#title")]
        public string Title { get; init; } = default!;

        /// <summary>可选 command 描述。</summary>
        [Description("@#description")]
        public string? Description { get; init; }

        /// <summary>排序权重，值越大越靠前。</summary>
        [Description("@#order")]
        public int? Order { get; init; }

        /// <summary>可选 Iconify icon 或图像 URL。</summary>
        [Description("@#icon")]
        public string? Icon { get; init; }

        /// <summary>没有 children 时执行的 URL action。</summary>
        [Description("@#action")]
        public CustomCommandUrlAction? Action { get; init; }

        /// <summary>可选的一层子命令列表，不能继续嵌套 children。</summary>
        [Description("@#children")]
        public CustomCommandChild[]? Children { get; init; }
    }
}
