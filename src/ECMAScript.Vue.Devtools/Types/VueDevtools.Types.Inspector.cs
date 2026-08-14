using System.ComponentModel;

namespace ECMAScript;

/// <summary>
/// Inspector、component hook 和 settings 编辑使用的结构化值 contract。
/// <c>DevtoolsValue</c> 有意只覆盖可稳定跨 Devtools bridge 传输的常见值域；
/// 业务 timeline payload 则通过泛型保持其原始强类型，而不是回退为 <c>object</c>。
/// </summary>
public static partial class VueDevtools
{
    /// <summary>
    /// Devtools inspector/state 中的通用可序列化值域。对于数值使用 <see cref="Number"/> 分支，
    /// 标量便捷构造通过 <c>From(...)</c> identity helper 完成，避免多重 implicit conversion 链。
    /// </summary>
    [ECMAScript]
    [Description("@#")]
    public readonly union DevtoolsValue(string, bool, Number, BigInt, Vue.VueProps, Array<DevtoolsValue?>)
    {
        /// <summary>从 CLR 整数创建 Devtools value；发射时保留原始 JavaScript number。</summary>
        [ECMAScriptInline("__arg1")]
        public extern static DevtoolsValue From(int value);

        /// <summary>从 CLR 长整数创建 Devtools value；发射时保留原始 JavaScript number。</summary>
        [ECMAScriptInline("__arg1")]
        public extern static DevtoolsValue From(long value);

        /// <summary>从双精度数创建 Devtools value。</summary>
        [ECMAScriptInline("__arg1")]
        public extern static DevtoolsValue From(double value);

        /// <summary>从 decimal 创建 Devtools value。</summary>
        [ECMAScriptInline("__arg1")]
        public extern static DevtoolsValue From(decimal value);
    }

    /// <summary>custom inspector 节点 tag 的显示样式。</summary>
    [ECMAScript]
    [Description("@#")]
    public record InspectorNodeTag : Vue.VueProps
    {
        /// <summary>tag 中显示的文本。The visible tag label.</summary>
        [Description("@#label")]
        public string Label { get; init; } = default!;

        /// <summary>文字颜色的 RGB 数值。Text color encoded as Devtools RGB number.</summary>
        [Description("@#textColor")]
        public Number TextColor { get; init; } = default!;

        /// <summary>背景颜色的 RGB 数值。Background color encoded as Devtools RGB number.</summary>
        [Description("@#backgroundColor")]
        public Number BackgroundColor { get; init; } = default!;

        /// <summary>悬停时显示的辅助说明。Optional tooltip.</summary>
        [Description("@#tooltip")]
        public string? Tooltip { get; init; }
    }

    /// <summary>custom inspector toolbar action 的共有显示元数据。</summary>
    [ECMAScript]
    [Description("@#")]
    public abstract record InspectorAction : Vue.VueProps
    {
        /// <summary>Iconify icon 名称或图像 URL。Iconify icon name or image URL.</summary>
        [Description("@#icon")]
        public string Icon { get; init; } = default!;

        /// <summary>可选悬浮提示。Optional toolbar tooltip.</summary>
        [Description("@#tooltip")]
        public string? Tooltip { get; init; }
    }

    /// <summary>同步 custom inspector toolbar action。</summary>
    [ECMAScript]
    [Description("@#")]
    public record SyncInspectorAction : InspectorAction
    {
        /// <summary>点击 action 时执行的同步回调。</summary>
        [Description("@#action")]
        public DevtoolsInspectorActionCallback Action { get; init; } = default!;
    }

    /// <summary>异步 custom inspector toolbar action。</summary>
    [ECMAScript]
    [Description("@#")]
    public record AsyncInspectorAction : InspectorAction
    {
        /// <summary>点击 action 时执行并返回 Promise 的回调。</summary>
        [Description("@#action")]
        public DevtoolsAsyncInspectorActionCallback Action { get; init; } = default!;
    }

    /// <summary>custom inspector node action 的共有显示元数据。</summary>
    [ECMAScript]
    [Description("@#")]
    public abstract record InspectorNodeAction : Vue.VueProps
    {
        /// <summary>Iconify icon 名称或图像 URL。Iconify icon name or image URL.</summary>
        [Description("@#icon")]
        public string Icon { get; init; } = default!;

        /// <summary>可选悬浮提示。Optional node-action tooltip.</summary>
        [Description("@#tooltip")]
        public string? Tooltip { get; init; }
    }

    /// <summary>同步 custom inspector node action。</summary>
    [ECMAScript]
    [Description("@#")]
    public record SyncInspectorNodeAction : InspectorNodeAction
    {
        /// <summary>接收当前 node id 的同步回调。</summary>
        [Description("@#action")]
        public DevtoolsInspectorNodeActionCallback Action { get; init; } = default!;
    }

    /// <summary>异步 custom inspector node action。</summary>
    [ECMAScript]
    [Description("@#")]
    public record AsyncInspectorNodeAction : InspectorNodeAction
    {
        /// <summary>接收当前 node id 并返回 Promise 的回调。</summary>
        [Description("@#action")]
        public DevtoolsAsyncInspectorNodeActionCallback Action { get; init; } = default!;
    }

    /// <summary>注册 custom inspector 时使用的选项。</summary>
    [ECMAScript]
    [Description("@#")]
    public record InspectorOptions : Vue.VueProps
    {
        /// <summary>inspector 唯一 id。Stable inspector id.</summary>
        [Description("@#id")]
        public string Id { get; init; } = default!;

        /// <summary>Devtools 中显示的 inspector 标签。Visible inspector label.</summary>
        [Description("@#label")]
        public string Label { get; init; } = default!;

        /// <summary>可选 icon。Optional inspector icon.</summary>
        [Description("@#icon")]
        public string? Icon { get; init; }

        /// <summary>tree filter 输入框的 placeholder。</summary>
        [Description("@#treeFilterPlaceholder")]
        public string? TreeFilterPlaceholder { get; init; }

        /// <summary>state filter 输入框的 placeholder。</summary>
        [Description("@#stateFilterPlaceholder")]
        public string? StateFilterPlaceholder { get; init; }

        /// <summary>没有选中 tree node 时的说明文本。</summary>
        [Description("@#noSelectionText")]
        public string? NoSelectionText { get; init; }

        /// <summary>inspector toolbar 的 action 列表。</summary>
        [Description("@#actions")]
        public InspectorAction[]? Actions { get; init; }

        /// <summary>选中节点后显示的 node action 列表。</summary>
        [Description("@#nodeActions")]
        public InspectorNodeAction[]? NodeActions { get; init; }
    }

    /// <summary>custom inspector tree 的一个节点。</summary>
    [ECMAScript]
    [Description("@#")]
    public record InspectorNode : Vue.VueProps
    {
        /// <summary>节点唯一 id。Stable inspector node id.</summary>
        [Description("@#id")]
        public string Id { get; init; } = default!;

        /// <summary>树中显示的节点标签。Visible tree label.</summary>
        [Description("@#label")]
        public string Label { get; init; } = default!;

        /// <summary>可选子节点。Optional child nodes.</summary>
        [Description("@#children")]
        public InspectorNode[]? Children { get; init; }

        /// <summary>附着在节点上的 Devtools tag。</summary>
        [Description("@#tags")]
        public InspectorNodeTag[]? Tags { get; init; }

        /// <summary>可选名称，供 Devtools 检索/显示。Optional searchable name.</summary>
        [Description("@#name")]
        public string? Name { get; init; }

        /// <summary>可选源码文件路径。Optional source file path.</summary>
        [Description("@#file")]
        public string? File { get; init; }
    }

    /// <summary>state entry 的对象来源提示。</summary>
    [String]
    public enum StateObjectType
    {
        /// <summary>Vue ref。Vue ref.</summary>
        [Description("@#ref")]
        Ref,

        /// <summary>Vue reactive object。Vue reactive object.</summary>
        [Description("@#reactive")]
        Reactive,

        /// <summary>Vue computed ref。Vue computed ref.</summary>
        [Description("@#computed")]
        Computed,

        /// <summary>其他普通值。Other regular value.</summary>
        [Description("@#other")]
        Other
    }

    /// <summary>
    /// custom inspector state 的基础条目。value 使用 <see cref="DevtoolsValue"/> 让常用可传输值保持封闭域；
    /// 复杂业务对象应实现为 <see cref="Vue.VueProps"/> record 后直接传入该分支。
    /// </summary>
    [ECMAScript]
    [Description("@#")]
    public record InspectorStateEntry : Vue.VueProps
    {
        /// <summary>state entry 的显示键。The visible state key.</summary>
        [Description("@#key")]
        public string Key { get; init; } = default!;

        /// <summary>显示/编辑的结构化值。The displayed and editable structured value.</summary>
        [Description("@#value")]
        public DevtoolsValue Value { get; init; } = default!;

        /// <summary>Devtools 是否允许用户编辑此值。</summary>
        [Description("@#editable")]
        public bool? Editable { get; init; }

        /// <summary>可选的 Vue object-kind 提示。</summary>
        [Description("@#objectType")]
        public StateObjectType? ObjectType { get; init; }

        /// <summary>可选原始展示文本。Optional raw display string.</summary>
        [Description("@#raw")]
        public string? Raw { get; init; }
    }

    /// <summary>包含 Devtools component state type 的 state entry。</summary>
    [ECMAScript]
    [Description("@#")]
    public record ComponentStateEntry : InspectorStateEntry
    {
        /// <summary>组件 state 的来源类别，例如 <c>data</c>、<c>props</c> 或插件自定义分类。</summary>
        [Description("@#type")]
        public string Type { get; init; } = default!;

        /// <summary>可选的 prop 元信息。Optional prop metadata.</summary>
        [Description("@#meta")]
        public ComponentPropMetadata? Meta { get; init; }
    }

    /// <summary>component prop state 的 Devtools 元信息。</summary>
    [ECMAScript]
    [Description("@#")]
    public record ComponentPropMetadata : Vue.VueProps
    {
        /// <summary>prop 的 runtime type 显示文本。</summary>
        [Description("@#type")]
        public string Type { get; init; } = default!;

        /// <summary>是否为 required prop。</summary>
        [Description("@#required")]
        public bool Required { get; init; }

        /// <summary>可选的 Vue prop mode。</summary>
        [Description("@#mode")]
        public ComponentPropMode? Mode { get; init; }
    }

    /// <summary>Vue Devtools 识别的 prop mode。</summary>
    [String]
    public enum ComponentPropMode
    {
        /// <summary>默认单向 prop。Default prop.</summary>
        [Description("@#default")]
        Default,

        /// <summary>同步 prop。Synchronized prop.</summary>
        [Description("@#sync")]
        Sync,

        /// <summary>一次性 prop。One-time prop.</summary>
        [Description("@#once")]
        Once
    }

    /// <summary>Devtools custom state wrapper；将其赋给 <see cref="InspectorStateEntry.Value"/> 可显示自定义 UI。</summary>
    [ECMAScript]
    [Description("@#")]
    public record CustomState : Vue.VueProps
    {
        /// <summary>Devtools 使用的保留 custom-state descriptor。</summary>
        [Description("@#_custom")]
        public CustomStateDescriptor Custom { get; init; } = default!;
    }

    /// <summary>custom state 的显示、identity 和 action 元数据。</summary>
    [ECMAScript]
    [Description("@#")]
    public record CustomStateDescriptor : Vue.VueProps
    {
        /// <summary>custom state 类型名称或内置类型。</summary>
        [Description("@#type")]
        public string Type { get; init; } = default!;

        /// <summary>可选 object-kind 说明。Optional object type hint.</summary>
        [Description("@#objectType")]
        public string? ObjectType { get; init; }

        /// <summary>UI 中的主显示文本。Primary display text.</summary>
        [Description("@#display")]
        public string? Display { get; init; }

        /// <summary>悬浮提示。Optional tooltip.</summary>
        [Description("@#tooltip")]
        public string? Tooltip { get; init; }

        /// <summary>可选的原始值投影。Optional raw value projection.</summary>
        [Description("@#value")]
        public DevtoolsValue? Value { get; init; }

        /// <summary>是否将内部字段作为抽象值处理。</summary>
        [Description("@#abstract")]
        public bool? Abstract { get; init; }

        /// <summary>可选的源码文件路径。</summary>
        [Description("@#file")]
        public string? File { get; init; }

        /// <summary>可选 component uid。</summary>
        [Description("@#uid")]
        public int? Uid { get; init; }

        /// <summary>是否阻止编辑直属子字段。</summary>
        [Description("@#readOnly")]
        public bool? ReadOnly { get; init; }

        /// <summary>对子字段的附加显示约束。</summary>
        [Description("@#fields")]
        public CustomStateFields? Fields { get; init; }

        /// <summary>可选的稳定 identity 值。</summary>
        [Description("@#id")]
        public DevtoolsValue? Id { get; init; }

        /// <summary>custom state 自带 action。</summary>
        [Description("@#actions")]
        public InspectorAction[]? Actions { get; init; }
    }

    /// <summary>custom state 子字段的显示约束。</summary>
    [ECMAScript]
    [Description("@#")]
    public record CustomStateFields : Vue.VueProps
    {
        /// <summary>是否将子字段作为抽象值展示。</summary>
        [Description("@#abstract")]
        public bool? Abstract { get; init; }
    }

    /// <summary>按 group key 组织的 custom inspector state。</summary>
    [ECMAScript]
    [Description("@#")]
    public record InspectorState : Vue.VueDictionary<InspectorStateEntry[]>;

    /// <summary>普通编辑 state payload：写入值和可选重命名。</summary>
    [ECMAScript]
    [Description("@#")]
    public record EditStateValue : Vue.VueProps
    {
        /// <summary>需要写入的新值。New value to write.</summary>
        [Description("@#value")]
        public DevtoolsValue Value { get; init; } = default!;

        /// <summary>可选的新键名。Optional replacement key.</summary>
        [Description("@#newKey")]
        public string? NewKey { get; init; }

        /// <summary>显式 false 时保持条目。Explicit false keeps the entry.</summary>
        [Description("@#remove")]
        public bool? Remove { get; init; }
    }

    /// <summary>删除 state entry 的编辑 payload。</summary>
    [ECMAScript]
    [Description("@#")]
    public record RemoveState : Vue.VueProps
    {
        /// <summary>必须为 true，表示删除目标 state entry。</summary>
        [Description("@#remove")]
        public bool Remove { get; init; } = true;
    }

    /// <summary>Devtools state 编辑操作：写入/重命名或删除。</summary>
    [ECMAScript]
    [Description("@#")]
    public readonly union StateChange(EditStateValue, RemoveState)
    {
    }

    /// <summary>
    /// component/inspector state 编辑 payload 的共享基类。<c>Set(...)</c> 直接映射官方 payload 的
    /// <c>set</c> callback，因此使用方不需要自行重建 JavaScript 路径写入逻辑。
    /// </summary>
    [ECMAScript]
    [Description("@#")]
    public abstract class EditStatePayload
    {
        protected EditStatePayload()
        {
        }

        /// <summary>所属 Vue app。</summary>
        [Description("@#app")]
        public extern Vue.VueApp App { get; }

        /// <summary>当前 inspector id。</summary>
        [Description("@#inspectorId")]
        public extern string InspectorId { get; }

        /// <summary>当前 node id。</summary>
        [Description("@#nodeId")]
        public extern string NodeId { get; }

        /// <summary>被编辑的字段路径。</summary>
        [Description("@#path")]
        public extern string[] Path { get; }

        /// <summary>Devtools state 分类类型。</summary>
        [Description("@#type")]
        public extern string Type { get; }

        /// <summary>本次编辑的写入或删除描述。</summary>
        [Description("@#state")]
        public extern StateChange State { get; }

        /// <summary>按单一路径字段写入强类型值。</summary>
        [Description("@#set")]
        public extern void Set<TTarget, TValue>(TTarget target, string path, TValue value);

        /// <summary>按嵌套路径写入强类型值。</summary>
        [Description("@#set")]
        public extern void Set<TTarget, TValue>(TTarget target, string[] path, TValue value);

        /// <summary>按单一路径字段写入并在完成后接收目标/字段/值通知。</summary>
        [Description("@#set")]
        public extern void Set<TTarget, TValue>(
            TTarget target,
            string path,
            TValue value,
            DevtoolsStateSetCallback<TTarget, TValue> callback);

        /// <summary>按嵌套路径写入并在完成后接收目标/字段/值通知。</summary>
        [Description("@#set")]
        public extern void Set<TTarget, TValue>(
            TTarget target,
            string[] path,
            TValue value,
            DevtoolsStateSetCallback<TTarget, TValue> callback);
    }

    /// <summary>由 <c>editComponentState</c> hook 提供的编辑 payload。</summary>
    [ECMAScript]
    [Description("@#")]
    public abstract class EditComponentStatePayload : EditStatePayload
    {
        protected EditComponentStatePayload()
        {
        }
    }

    /// <summary>由 <c>editInspectorState</c> hook 提供的编辑 payload。</summary>
    [ECMAScript]
    [Description("@#")]
    public abstract class EditInspectorStatePayload : EditStatePayload
    {
        protected EditInspectorStatePayload()
        {
        }
    }

    /// <summary>由 <c>getInspectorTree</c> hook 提供的 payload。</summary>
    [ECMAScript]
    [Description("@#")]
    public abstract class GetInspectorTreePayload
    {
        protected GetInspectorTreePayload()
        {
        }

        /// <summary>所属 Vue app。</summary>
        [Description("@#app")]
        public extern Vue.VueApp App { get; }

        /// <summary>请求的 inspector id。</summary>
        [Description("@#inspectorId")]
        public extern string InspectorId { get; }

        /// <summary>当前 Devtools filter 文本。</summary>
        [Description("@#filter")]
        public extern string Filter { get; }

        /// <summary>可替换或通过 <see cref="Array{T}.Push(T[])"/> 追加的根节点集合。</summary>
        [Description("@#rootNodes")]
        public extern Array<InspectorNode> RootNodes { get; set; }
    }

    /// <summary>由 <c>getInspectorState</c> hook 提供的 payload。</summary>
    [ECMAScript]
    [Description("@#")]
    public abstract class GetInspectorStatePayload
    {
        protected GetInspectorStatePayload()
        {
        }

        /// <summary>所属 Vue app。</summary>
        [Description("@#app")]
        public extern Vue.VueApp App { get; }

        /// <summary>请求的 inspector id。</summary>
        [Description("@#inspectorId")]
        public extern string InspectorId { get; }

        /// <summary>当前选中的 inspector node id。</summary>
        [Description("@#nodeId")]
        public extern string NodeId { get; }

        /// <summary>按 group key 填充的 inspector state。</summary>
        [Description("@#state")]
        public extern InspectorState State { get; set; }
    }

    /// <summary>由 <c>setPluginSettings</c> hook 提供的动态 settings payload。</summary>
    [ECMAScript]
    [Description("@#")]
    public abstract class SetPluginSettingsPayload
    {
        protected SetPluginSettingsPayload()
        {
        }

        /// <summary>所属 Vue app。</summary>
        [Description("@#app")]
        public extern Vue.VueApp App { get; }

        /// <summary>发生变更的 plugin id。</summary>
        [Description("@#pluginId")]
        public extern string PluginId { get; }

        /// <summary>发生变更的 setting key。</summary>
        [Description("@#key")]
        public extern string Key { get; }

        /// <summary>新设置值。</summary>
        [Description("@#newValue")]
        public extern DevtoolsValue NewValue { get; }

        /// <summary>旧设置值。</summary>
        [Description("@#oldValue")]
        public extern DevtoolsValue OldValue { get; }

        /// <summary>完整动态 settings 字典。</summary>
        [Description("@#settings")]
        public extern PluginSettingsValues Settings { get; }
    }

    /// <summary>带业务 settings record 投影的修改 payload。</summary>
    /// <typeparam name="TSettings">应用声明的 settings record 类型。</typeparam>
    [ECMAScript]
    [Description("@#")]
    public abstract class SetPluginSettingsPayload<TSettings> : SetPluginSettingsPayload
        where TSettings : Vue.VueProps
    {
        protected SetPluginSettingsPayload()
        {
        }

        /// <summary>完整的强类型 settings record。</summary>
        [Description("@#settings")]
        public new extern TSettings Settings { get; }
    }
}
