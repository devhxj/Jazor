using System.ComponentModel;

namespace ECMAScript;

/// <summary>
/// Vue component hooks 相关的 opaque handle 与 payload。Vue 内部实例不是业务可构造对象，
/// binding 只允许 Devtools callback/查询 API 在正确生命周期内传递和消费它。
/// </summary>
public static partial class VueDevtools
{
    /// <summary>Vue Devtools 使用的 component uid/key 值域。</summary>
    [ECMAScript]
    [Description("@#")]
    public readonly union ComponentIdentifier(string, Number)
    {
        /// <summary>从整数 uid 创建 identity projection。</summary>
        [ECMAScriptInline("__arg1")]
        public extern static ComponentIdentifier From(int value);
    }

    /// <summary>
    /// 由 Vue Devtools 提供的 component internal-instance handle。该类型刻意没有公开构造器或内部字段，
    /// 避免业务代码耦合 <c>ComponentInternalInstance</c> 这一非公共 Vue runtime shape。
    /// </summary>
    [ECMAScript]
    [Description("@#")]
    public abstract class ComponentInstance
    {
        protected ComponentInstance()
        {
        }
    }

    /// <summary>Devtools 中可编辑的 Vue component tree 节点。</summary>
    [ECMAScript]
    [Description("@#")]
    public abstract class ComponentTreeNode
    {
        protected ComponentTreeNode()
        {
        }

        /// <summary>Vue component uid。</summary>
        [Description("@#uid")]
        public extern ComponentIdentifier Uid { get; set; }

        /// <summary>Devtools component tree node id。</summary>
        [Description("@#id")]
        public extern string Id { get; set; }

        /// <summary>Devtools 中显示的 component 名称。</summary>
        [Description("@#name")]
        public extern string Name { get; set; }

        /// <summary>当前 render key。</summary>
        [Description("@#renderKey")]
        public extern ComponentIdentifier RenderKey { get; set; }

        /// <summary>该节点是否为 inactive/kept-alive component。</summary>
        [Description("@#inactive")]
        public extern bool Inactive { get; set; }

        /// <summary>该节点是否表示 Vue fragment。</summary>
        [Description("@#isFragment")]
        public extern bool IsFragment { get; set; }

        /// <summary>是否有 child component。</summary>
        [Description("@#hasChildren")]
        public extern bool HasChildren { get; set; }

        /// <summary>子 component 节点列表。</summary>
        [Description("@#children")]
        public extern Array<ComponentTreeNode> Children { get; set; }

        /// <summary>可选 DOM 排序位置。</summary>
        [Description("@#domOrder")]
        public extern Array<Number>? DomOrder { get; set; }

        /// <summary>可选 console id。</summary>
        [Description("@#consoleId")]
        public extern string? ConsoleId { get; set; }

        /// <summary>是否为 router-view component。</summary>
        [Description("@#isRouterView")]
        public extern bool? IsRouterView { get; set; }

        /// <summary>
        /// 当前匹配的 route segment。上游字段拼写为 <c>macthedRouteSegment</c>，
        /// C# 侧保留正确命名并通过显式 runtime name 保证 ABI 一致。
        /// </summary>
        [Description("@#macthedRouteSegment")]
        public extern string? MatchedRouteSegment { get; set; }

        /// <summary>附加到 component 的 Devtools tag。</summary>
        [Description("@#tags")]
        public extern Array<InspectorNodeTag> Tags { get; set; }

        /// <summary>是否建议 Devtools 自动展开此节点。</summary>
        [Description("@#autoOpen")]
        public extern bool AutoOpen { get; set; }

        /// <summary>可选的结构化元数据。</summary>
        [Description("@#meta")]
        public extern DevtoolsValue? Meta { get; set; }

        /// <summary>可选的 component 源文件路径。</summary>
        [Description("@#file")]
        public extern string? File { get; set; }
    }

    /// <summary>Devtools 中被检查 component 的数据快照。</summary>
    [ECMAScript]
    [Description("@#")]
    public abstract class InspectedComponentData
    {
        protected InspectedComponentData()
        {
        }

        /// <summary>Devtools component id。</summary>
        [Description("@#id")]
        public extern string Id { get; set; }

        /// <summary>当前显示的 component 名称。</summary>
        [Description("@#name")]
        public extern string Name { get; set; }

        /// <summary>component 源文件路径。</summary>
        [Description("@#file")]
        public extern string File { get; set; }

        /// <summary>
        /// Devtools component state 条目。可通过 <see cref="Array{T}.Push(T[])"/> 追加插件自定义状态，
        /// 以便显示在 component inspector 中。
        /// </summary>
        [Description("@#state")]
        public extern Array<ComponentStateEntry> State { get; set; }

        /// <summary>函数式 component 标志。</summary>
        [Description("@#functional")]
        public extern bool? Functional { get; set; }
    }

    /// <summary>component 对应 DOM bounds。</summary>
    [ECMAScript]
    [Description("@#")]
    public abstract class ComponentBounds
    {
        protected ComponentBounds()
        {
        }

        /// <summary>左侧坐标。Left viewport coordinate.</summary>
        [Description("@#left")]
        public extern double Left { get; }

        /// <summary>顶部坐标。Top viewport coordinate.</summary>
        [Description("@#top")]
        public extern double Top { get; }

        /// <summary>元素宽度。Element width.</summary>
        [Description("@#width")]
        public extern double Width { get; }

        /// <summary>元素高度。Element height.</summary>
        [Description("@#height")]
        public extern double Height { get; }
    }

    /// <summary>由 <c>visitComponentTree</c> hook 提供的 component tree payload。</summary>
    [ECMAScript]
    [Description("@#")]
    public abstract class VisitComponentTreePayload
    {
        protected VisitComponentTreePayload()
        {
        }

        /// <summary>所属 Vue app。</summary>
        [Description("@#app")]
        public extern Vue.VueApp App { get; }

        /// <summary>当前 component internal-instance handle。</summary>
        [Description("@#componentInstance")]
        public extern ComponentInstance ComponentInstance { get; }

        /// <summary>可由插件补充 tag/meta 的 Devtools tree node。</summary>
        [Description("@#treeNode")]
        public extern ComponentTreeNode TreeNode { get; }

        /// <summary>用户在 component tree 中输入的 filter 文本。</summary>
        [Description("@#filter")]
        public extern string Filter { get; }
    }

    /// <summary>由 <c>inspectComponent</c> hook 提供的 component state payload。</summary>
    [ECMAScript]
    [Description("@#")]
    public abstract class InspectComponentPayload
    {
        protected InspectComponentPayload()
        {
        }

        /// <summary>所属 Vue app。</summary>
        [Description("@#app")]
        public extern Vue.VueApp App { get; }

        /// <summary>当前 component internal-instance handle。</summary>
        [Description("@#componentInstance")]
        public extern ComponentInstance ComponentInstance { get; }

        /// <summary>可由插件追加 custom state 的 component data snapshot。</summary>
        [Description("@#instanceData")]
        public extern InspectedComponentData InstanceData { get; }
    }
}
