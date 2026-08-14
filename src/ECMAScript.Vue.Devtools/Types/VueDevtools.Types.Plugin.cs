using System.ComponentModel;

namespace ECMAScript;

/// <summary>
/// Vue Devtools plugin descriptor、settings 和 API handle 的强类型 contract。
/// 这些 record 只形成最终 JavaScript object shape，不会生成额外的 CLR runtime protocol。
/// </summary>
public static partial class VueDevtools
{
    /// <summary>
    /// 注册 Devtools plugin 所需的身份与作用域描述。<see cref="App"/> 必须是同一应用实际使用的 Vue app，
    /// 否则 inspector/timeline 不会落入预期的 app scope。
    /// </summary>
    [ECMAScript]
    [Description("@#")]
    public record PluginDescriptor : Vue.VueProps
    {
        /// <summary>插件唯一标识。Plugin id must be stable across reloads.</summary>
        [Description("@#id")]
        public string Id { get; init; } = default!;

        /// <summary>显示在 Vue Devtools 内的插件标签。Human-readable plugin label.</summary>
        [Description("@#label")]
        public string Label { get; init; } = default!;

        /// <summary>插件所属的 Vue application instance。</summary>
        [Description("@#app")]
        public Vue.VueApp App { get; init; } = default!;

        /// <summary>可选的 NuGet/npm package 名称显示文本。Optional package name shown by Devtools.</summary>
        [Description("@#packageName")]
        public string? PackageName { get; init; }

        /// <summary>插件主页 URL。Optional plugin homepage URL.</summary>
        [Description("@#homepage")]
        public string? Homepage { get; init; }

        /// <summary>组件 state type 的显示分组。Optional component state type labels.</summary>
        [Description("@#componentStateTypes")]
        public string[]? ComponentStateTypes { get; init; }

        /// <summary>插件 logo URL 或 data URL。Plugin logo URL/data URL.</summary>
        [Description("@#logo")]
        public string? Logo { get; init; }

        /// <summary>禁用 app scope selector。Disable Devtools app-scope selector for this plugin.</summary>
        [Description("@#disableAppScope")]
        public bool? DisableAppScope { get; init; }

        /// <summary>禁用 plugin scope selector。Disable Devtools plugin-scope selector.</summary>
        [Description("@#disablePluginScope")]
        public bool? DisablePluginScope { get; init; }

        /// <summary>
        /// 允许在 Devtools UI 尚未打开时提前提供 proxy API。适合早期 timeline event；
        /// 这会改变 setup 时机，因此只在确实需要早期记录时启用。
        /// </summary>
        [Description("@#enableEarlyProxy")]
        public bool? EnableEarlyProxy { get; init; }

        /// <summary>以 setting key 为索引的插件设置定义。</summary>
        [Description("@#settings")]
        public PluginSettings? Settings { get; init; }
    }

    /// <summary>
    /// 带 settings 值类型标记的 descriptor。泛型在 JavaScript 发射时擦除，
    /// 仅让 <see cref="PluginApi{TSettings}.GetSettings"/> 和 settings hook 保持业务类型。
    /// </summary>
    /// <typeparam name="TSettings">应用声明的 settings 值 record。</typeparam>
    [ECMAScript]
    [Description("@#")]
    public record PluginDescriptor<TSettings> : PluginDescriptor
        where TSettings : Vue.VueProps;

    /// <summary>
    /// Devtools settings 定义的动态键字典。collection initializer 会直接 lowering 为普通 JS object，
    /// 不会在运行时创建 <c>Map</c>。
    /// </summary>
    [ECMAScript]
    [Description("@#")]
    public record PluginSettings : Vue.VueDictionary<PluginSetting>;

    /// <summary>
    /// Devtools 读取到的 settings 值字典。对于固定业务 schema，优先使用
    /// <see cref="PluginApi{TSettings}.GetSettings"/> 获取具体 record。
    /// </summary>
    [ECMAScript]
    [Description("@#")]
    public record PluginSettingsValues : Vue.VueDictionary<DevtoolsValue>;

    /// <summary>
    /// 所有 Devtools setting definition 的共同结果类型。具体 setting 必须通过各自的
    /// <c>Create(...)</c> factory 创建，factory 会写入不能由业务侧覆盖的官方 <c>type</c> literal。
    /// </summary>
    [ECMAScript]
    [Description("@#")]
    public abstract record PluginSetting : Vue.VueProps;

    /// <summary>Boolean setting factory 的输入形状。</summary>
    [ECMAScript]
    [Description("@#")]
    public record BooleanPluginSettingOptions : Vue.VueProps
    {
        /// <summary>setting 的用户可见标签。User-facing setting label.</summary>
        [Description("@#label")]
        public string Label { get; init; } = default!;

        /// <summary>可选的辅助说明。Optional setting description.</summary>
        [Description("@#description")]
        public string? Description { get; init; }

        /// <summary>Devtools 初始使用的布尔值。Default boolean value.</summary>
        [Description("@#defaultValue")]
        public bool DefaultValue { get; init; }
    }

    /// <summary>
    /// Boolean setting 的 opaque result。不能直接 <c>new</c>，以确保 emitted object 始终包含
    /// <c>type: "boolean"</c>，而不会依赖 C# property initializer 的 CLR-only 默认值。
    /// </summary>
    [ECMAScript]
    [Description("@#")]
    public sealed record BooleanPluginSetting : PluginSetting
    {
        private BooleanPluginSetting()
        {
        }

        /// <summary>创建带固定官方 boolean discriminator 的 setting definition。</summary>
        [ECMAScriptInline("Object.assign({ type: \"boolean\" }, __arg1)")]
        public extern static BooleanPluginSetting Create(BooleanPluginSettingOptions options);
    }

    /// <summary>Choice setting 的候选值，官方 API 只接受字符串或数字。</summary>
    [ECMAScript]
    [Description("@#")]
    public readonly union ChoiceValue(string, Number)
    {
        /// <summary>从整数创建 choice 值；helper 在发射时就是 identity expression。</summary>
        [ECMAScriptInline("__arg1")]
        public extern static ChoiceValue From(int value);

        /// <summary>从长整数创建 choice 值；helper 在发射时就是 identity expression。</summary>
        [ECMAScriptInline("__arg1")]
        public extern static ChoiceValue From(long value);

        /// <summary>从浮点数创建 choice 值；helper 在发射时就是 identity expression。</summary>
        [ECMAScriptInline("__arg1")]
        public extern static ChoiceValue From(double value);

        /// <summary>从 decimal 创建 choice 值；helper 在发射时就是 identity expression。</summary>
        [ECMAScriptInline("__arg1")]
        public extern static ChoiceValue From(decimal value);
    }

    /// <summary>Choice setting 的一个可选项。</summary>
    [ECMAScript]
    [Description("@#")]
    public record PluginSettingChoice : Vue.VueProps
    {
        /// <summary>写入 settings object 的实际值。The stored value.</summary>
        [Description("@#value")]
        public ChoiceValue Value { get; init; } = default!;

        /// <summary>显示给用户的候选项文本。The user-facing option label.</summary>
        [Description("@#label")]
        public string Label { get; init; } = default!;
    }

    /// <summary>Choice setting 的呈现形式。</summary>
    [String]
    public enum ChoiceSettingComponent
    {
        /// <summary>下拉选择框。Select input.</summary>
        [Description("@#select")]
        Select,

        /// <summary>按钮组。Button-group input.</summary>
        [Description("@#button-group")]
        ButtonGroup
    }

    /// <summary>Choice setting factory 的输入形状。</summary>
    [ECMAScript]
    [Description("@#")]
    public record ChoicePluginSettingOptions : Vue.VueProps
    {
        /// <summary>setting 的用户可见标签。User-facing setting label.</summary>
        [Description("@#label")]
        public string Label { get; init; } = default!;

        /// <summary>可选的辅助说明。Optional setting description.</summary>
        [Description("@#description")]
        public string? Description { get; init; }

        /// <summary>Devtools 初始选择的值。Default selected value.</summary>
        [Description("@#defaultValue")]
        public ChoiceValue DefaultValue { get; init; } = default!;

        /// <summary>候选项列表。Available choice options.</summary>
        [Description("@#options")]
        public PluginSettingChoice[] Options { get; init; } = default!;

        /// <summary>可选的 Devtools 输入组件提示。Optional Devtools input component hint.</summary>
        [Description("@#component")]
        public ChoiceSettingComponent? Component { get; init; }
    }

    /// <summary>Choice setting 的 opaque result；factory 固定发射 <c>type: "choice"</c>。</summary>
    [ECMAScript]
    [Description("@#")]
    public sealed record ChoicePluginSetting : PluginSetting
    {
        private ChoicePluginSetting()
        {
        }

        /// <summary>创建带固定官方 choice discriminator 的 setting definition。</summary>
        [ECMAScriptInline("Object.assign({ type: \"choice\" }, __arg1)")]
        public extern static ChoicePluginSetting Create(ChoicePluginSettingOptions options);
    }

    /// <summary>Text setting factory 的输入形状。</summary>
    [ECMAScript]
    [Description("@#")]
    public record TextPluginSettingOptions : Vue.VueProps
    {
        /// <summary>setting 的用户可见标签。User-facing setting label.</summary>
        [Description("@#label")]
        public string Label { get; init; } = default!;

        /// <summary>可选的辅助说明。Optional setting description.</summary>
        [Description("@#description")]
        public string? Description { get; init; }

        /// <summary>Devtools 初始显示的文本。Default text value.</summary>
        [Description("@#defaultValue")]
        public string DefaultValue { get; init; } = default!;
    }

    /// <summary>Text setting 的 opaque result；factory 固定发射 <c>type: "text"</c>。</summary>
    [ECMAScript]
    [Description("@#")]
    public sealed record TextPluginSetting : PluginSetting
    {
        private TextPluginSetting()
        {
        }

        /// <summary>创建带固定官方 text discriminator 的 setting definition。</summary>
        [ECMAScriptInline("Object.assign({ type: \"text\" }, __arg1)")]
        public extern static TextPluginSetting Create(TextPluginSettingOptions options);
    }

    /// <summary>
    /// 由 <c>setupDevToolsPlugin()</c> 注入的运行时 API。该 handle 是 Devtools bridge 所有，
    /// 只能在 setup callback 内保存和使用，不能使用 <c>new</c> 创建。
    /// </summary>
    [ECMAScript]
    [Description("@#")]
    public abstract class PluginApi
    {
        protected PluginApi()
        {
        }

        /// <summary>注册 lifecycle/inspector/timeline hooks 的入口。</summary>
        [Description("@#on")]
        public extern PluginHooks On { get; }

        /// <summary>通知 Devtools 某个组件状态已经更新，触发组件检查视图刷新。</summary>
        [Description("@#notifyComponentUpdate")]
        public extern void NotifyComponentUpdate();

        /// <summary>通知 Devtools 指定组件状态已经更新。</summary>
        [Description("@#notifyComponentUpdate")]
        public extern void NotifyComponentUpdate(ComponentInstance instance);

        /// <summary>注册一个 custom inspector。</summary>
        [Description("@#addInspector")]
        public extern void AddInspector(InspectorOptions options);

        /// <summary>请求 Devtools 重新读取指定 inspector 的 tree。</summary>
        [Description("@#sendInspectorTree")]
        public extern void SendInspectorTree(string inspectorId);

        /// <summary>请求 Devtools 重新读取指定 inspector 的 state。</summary>
        [Description("@#sendInspectorState")]
        public extern void SendInspectorState(string inspectorId);

        /// <summary>在 Devtools UI 中选择指定 inspector node。</summary>
        [Description("@#selectInspectorNode")]
        public extern void SelectInspectorNode(string inspectorId, string nodeId);

        /// <summary>
        /// 主动触发 component tree hook。通常由 Devtools 调用；仅在已有合法 payload 的集成场景中转发，
        /// 返回的 Promise 代表所有已注册 hook 完成，不能假定其承载业务返回值。
        /// </summary>
        [Description("@#visitComponentTree")]
        public extern IPromise VisitComponentTree(VisitComponentTreePayload payload);

        /// <summary>返回 Devtools 的高精度时间戳，用于与 timeline event 对齐。</summary>
        [Description("@#now")]
        public extern double Now();

        /// <summary>添加一个 timeline layer。</summary>
        [Description("@#addTimelineLayer")]
        public extern void AddTimelineLayer<TData, TMeta>(TimelineLayerOptions<TData, TMeta> options);

        /// <summary>向 timeline layer 写入一个 event。</summary>
        [Description("@#addTimelineEvent")]
        public extern void AddTimelineEvent<TData, TMeta>(TimelineEventOptions<TData, TMeta> options);

        /// <summary>读取动态 settings 字典。固定 schema 请使用泛型 overload。</summary>
        [Description("@#getSettings")]
        public extern PluginSettingsValues GetSettings();

        /// <summary>
        /// 读取指定 plugin 的动态 settings 字典。该 overload 直接对应官方可选 <c>pluginId</c> 参数，
        /// 不会改变当前 plugin 的 lifecycle 或默认 settings 作用域。
        /// </summary>
        [Description("@#getSettings")]
        public extern PluginSettingsValues GetSettings(string pluginId);

        /// <summary>将 Devtools settings object 投影为调用方声明的结构化 record。</summary>
        /// <typeparam name="TSettings">业务 settings record 类型。</typeparam>
        [Description("@#getSettings")]
        public extern TSettings GetSettings<TSettings>()
            where TSettings : Vue.VueProps;

        /// <summary>将指定 plugin 的 settings object 投影为调用方声明的结构化 record。</summary>
        /// <typeparam name="TSettings">业务 settings record 类型。</typeparam>
        [Description("@#getSettings")]
        public extern TSettings GetSettings<TSettings>(string pluginId)
            where TSettings : Vue.VueProps;

        /// <summary>获取 app 中由 Devtools 识别的 component instance 列表。</summary>
        [Description("@#getComponentInstances")]
        public extern IPromise<Array<ComponentInstance>> GetComponentInstances(Vue.VueApp app);

        /// <summary>异步读取 component 的 DOM bounds。</summary>
        [Description("@#getComponentBounds")]
        public extern IPromise<ComponentBounds> GetComponentBounds(ComponentInstance instance);

        /// <summary>异步读取 Devtools 推断的 component name。</summary>
        [Description("@#getComponentName")]
        public extern IPromise<string> GetComponentName(ComponentInstance instance);

        /// <summary>高亮指定 component 对应的 DOM element。</summary>
        [Description("@#highlightElement")]
        public extern IPromise HighlightElement(ComponentInstance instance);

        /// <summary>清除当前 component element highlight。</summary>
        [Description("@#unhighlightElement")]
        public extern IPromise UnhighlightElement();
    }

    /// <summary>带具体 settings record 投影的 Devtools API handle。</summary>
    /// <typeparam name="TSettings">业务 settings record 类型。</typeparam>
    [ECMAScript]
    [Description("@#")]
    public abstract class PluginApi<TSettings> : PluginApi
        where TSettings : Vue.VueProps
    {
        protected PluginApi()
        {
        }

        /// <summary>读取当前插件的强类型 settings 值。</summary>
        [Description("@#getSettings")]
        public new extern TSettings GetSettings();

        /// <summary>读取指定 plugin 的强类型 settings 值。</summary>
        [Description("@#getSettings")]
        public new extern TSettings GetSettings(string pluginId);
    }

    /// <summary>
    /// <see cref="PluginApi.On"/> 提供的 hook 注册对象。每个方法都只注册 handler，
    /// 不会像 CLR event 一样返回订阅句柄；plugin 生命周期由 Devtools 控制。
    /// </summary>
    [ECMAScript]
    [Description("@#")]
    public abstract class PluginHooks
    {
        protected PluginHooks()
        {
        }

        /// <summary>注册 component tree 访问 handler。</summary>
        [Description("@#visitComponentTree")]
        public extern void VisitComponentTree(DevtoolsVisitComponentTreeCallback handler);

        /// <summary>注册 component state 检查 handler。</summary>
        [Description("@#inspectComponent")]
        public extern void InspectComponent(DevtoolsInspectComponentCallback handler);

        /// <summary>注册 component state 编辑 handler。</summary>
        [Description("@#editComponentState")]
        public extern void EditComponentState(DevtoolsEditComponentStateCallback handler);

        /// <summary>注册 custom inspector tree handler。</summary>
        [Description("@#getInspectorTree")]
        public extern void GetInspectorTree(DevtoolsGetInspectorTreeCallback handler);

        /// <summary>注册 custom inspector state handler。</summary>
        [Description("@#getInspectorState")]
        public extern void GetInspectorState(DevtoolsGetInspectorStateCallback handler);

        /// <summary>注册 custom inspector state 编辑 handler。</summary>
        [Description("@#editInspectorState")]
        public extern void EditInspectorState(DevtoolsEditInspectorStateCallback handler);

        /// <summary>注册带业务数据投影的 timeline event 检查 handler。</summary>
        [Description("@#inspectTimelineEvent")]
        public extern void InspectTimelineEvent<TData, TMeta>(DevtoolsInspectTimelineEventCallback<TData, TMeta> handler);

        /// <summary>注册 timeline 清空通知 handler。</summary>
        [Description("@#timelineCleared")]
        public extern void TimelineCleared(DevtoolsTimelineClearedCallback handler);

        /// <summary>注册带强类型 settings 投影的 settings 修改 handler。</summary>
        [Description("@#setPluginSettings")]
        public extern void SetPluginSettings<TSettings>(DevtoolsSetPluginSettingsCallback<TSettings> handler)
            where TSettings : Vue.VueProps;
    }
}
