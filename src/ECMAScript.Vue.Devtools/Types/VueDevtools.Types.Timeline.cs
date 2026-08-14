using System.ComponentModel;

namespace ECMAScript;

/// <summary>
/// Vue Devtools Timeline API 的 layer、event 和 inspection contract。
/// 泛型数据/元数据保持调用方声明的类型，让业务 telemetry 不必经过 untyped JSON/object 中转。
/// </summary>
public static partial class VueDevtools
{
    /// <summary>timeline event 的日志级别。</summary>
    [String]
    public enum TimelineLogType
    {
        /// <summary>普通 event。Default event.</summary>
        [Description("@#default")]
        Default,

        /// <summary>警告 event。Warning event.</summary>
        [Description("@#warning")]
        Warning,

        /// <summary>错误 event。Error event.</summary>
        [Description("@#error")]
        Error
    }

    /// <summary>timeline event group id，官方 API 支持字符串或 number。</summary>
    [ECMAScript]
    [Description("@#")]
    public readonly union TimelineGroupId(string, Number)
    {
        /// <summary>从整数 group id 创建 identity projection。</summary>
        [ECMAScriptInline("__arg1")]
        public extern static TimelineGroupId From(int value);

        /// <summary>从长整数 group id 创建 identity projection。</summary>
        [ECMAScriptInline("__arg1")]
        public extern static TimelineGroupId From(long value);

        /// <summary>从双精度 group id 创建 identity projection。</summary>
        [ECMAScriptInline("__arg1")]
        public extern static TimelineGroupId From(double value);
    }

    /// <summary>写入 Devtools timeline 的一个业务 event。</summary>
    /// <typeparam name="TData">event 的业务数据类型。</typeparam>
    /// <typeparam name="TMeta">event 的可选元数据类型。</typeparam>
    [ECMAScript]
    [Description("@#")]
    public record TimelineEvent<TData, TMeta> : Vue.VueProps
    {
        /// <summary>与 <see cref="PluginApi.Now"/> 对齐的时间戳。</summary>
        [Description("@#time")]
        public double Time { get; init; }

        /// <summary>Devtools 展开显示的业务数据。</summary>
        [Description("@#data")]
        public TData Data { get; init; } = default!;

        /// <summary>可选日志级别。</summary>
        [Description("@#logType")]
        public TimelineLogType? LogType { get; init; }

        /// <summary>可选元数据，保持与 data 独立的强类型投影。</summary>
        [Description("@#meta")]
        public TMeta? Meta { get; init; }

        /// <summary>可选 group id，用于归并关联 event。</summary>
        [Description("@#groupId")]
        public TimelineGroupId? GroupId { get; init; }

        /// <summary>可选 event 标题。</summary>
        [Description("@#title")]
        public string? Title { get; init; }

        /// <summary>可选 event 副标题。</summary>
        [Description("@#subtitle")]
        public string? Subtitle { get; init; }
    }

    /// <summary>添加 timeline layer 所需的基础选项。</summary>
    /// <typeparam name="TData">该 layer 事件的数据类型。</typeparam>
    /// <typeparam name="TMeta">该 layer 事件的元数据类型。</typeparam>
    [ECMAScript]
    [Description("@#")]
    public record TimelineLayerOptions<TData, TMeta> : Vue.VueProps
    {
        /// <summary>layer 唯一 id。Stable layer id.</summary>
        [Description("@#id")]
        public string Id { get; init; } = default!;

        /// <summary>Devtools 中显示的 layer 标签。</summary>
        [Description("@#label")]
        public string Label { get; init; } = default!;

        /// <summary>layer 颜色的 RGB number。</summary>
        [Description("@#color")]
        public Number Color { get; init; } = default!;

        /// <summary>是否跳过截图采集。</summary>
        [Description("@#skipScreenshots")]
        public bool? SkipScreenshots { get; init; }

        /// <summary>是否仅显示 event groups。</summary>
        [Description("@#groupsOnly")]
        public bool? GroupsOnly { get; init; }

        /// <summary>是否忽略没有 duration 的 group。</summary>
        [Description("@#ignoreNoDurationGroups")]
        public bool? IgnoreNoDurationGroups { get; init; }
    }

    /// <summary>包含同步 screenshot overlay renderer 的 timeline layer。</summary>
    /// <typeparam name="TData">该 layer 事件的数据类型。</typeparam>
    /// <typeparam name="TMeta">该 layer 事件的元数据类型。</typeparam>
    [ECMAScript]
    [Description("@#")]
    public record TimelineLayerOptionsWithOverlay<TData, TMeta> : TimelineLayerOptions<TData, TMeta>
    {
        /// <summary>Devtools 请求截图 overlay 时调用的同步 renderer。</summary>
        [Description("@#screenshotOverlayRender")]
        public DevtoolsScreenshotOverlayRenderCallback<TData, TMeta> ScreenshotOverlayRender { get; init; } = default!;
    }

    /// <summary>包含异步 screenshot overlay renderer 的 timeline layer。</summary>
    /// <typeparam name="TData">该 layer 事件的数据类型。</typeparam>
    /// <typeparam name="TMeta">该 layer 事件的元数据类型。</typeparam>
    [ECMAScript]
    [Description("@#")]
    public record AsyncTimelineLayerOptionsWithOverlay<TData, TMeta> : TimelineLayerOptions<TData, TMeta>
    {
        /// <summary>Devtools 请求截图 overlay 时调用的异步 renderer。</summary>
        [Description("@#screenshotOverlayRender")]
        public DevtoolsAsyncScreenshotOverlayRenderCallback<TData, TMeta> ScreenshotOverlayRender { get; init; } = default!;
    }

    /// <summary>向指定 layer 写 event 的包装对象。</summary>
    /// <typeparam name="TData">event 的业务数据类型。</typeparam>
    /// <typeparam name="TMeta">event 的可选元数据类型。</typeparam>
    [ECMAScript]
    [Description("@#")]
    public record TimelineEventOptions<TData, TMeta> : Vue.VueProps
    {
        /// <summary>目标 timeline layer id。</summary>
        [Description("@#layerId")]
        public string LayerId { get; init; } = default!;

        /// <summary>需要添加的 timeline event。</summary>
        [Description("@#event")]
        public TimelineEvent<TData, TMeta> Event { get; init; } = default!;

        /// <summary>为 true 时将 event 广播到所有 apps。</summary>
        [Description("@#all")]
        public bool? All { get; init; }
    }

    /// <summary>Devtools 截图时间点。</summary>
    [ECMAScript]
    [Description("@#")]
    public record ScreenshotData : Vue.VueProps
    {
        /// <summary>截图对应的时间戳。</summary>
        [Description("@#time")]
        public double Time { get; init; }
    }

    /// <summary>带 layer id 的 screenshot overlay event。</summary>
    /// <typeparam name="TData">event 的业务数据类型。</typeparam>
    /// <typeparam name="TMeta">event 的可选元数据类型。</typeparam>
    [ECMAScript]
    [Description("@#")]
    public record ScreenshotOverlayEvent<TData, TMeta> : TimelineEvent<TData, TMeta>
    {
        /// <summary>该 event 所属 timeline layer id。</summary>
        [Description("@#layerId")]
        public string LayerId { get; init; } = default!;

        /// <summary>
        /// Devtools 在截图采集时附带的渲染元数据。上游值域为 <c>any</c>；这里收敛到可跨 bridge
        /// 稳定传输的 <see cref="DevtoolsValue"/>，避免把未约束的 <c>object</c> 暴露到公共 API。
        /// </summary>
        [Description("@#renderMeta")]
        public DevtoolsValue? RenderMeta { get; init; }
    }

    /// <summary>截图 overlay renderer 接收的上下文。</summary>
    /// <typeparam name="TData">event 的业务数据类型。</typeparam>
    /// <typeparam name="TMeta">event 的可选元数据类型。</typeparam>
    [ECMAScript]
    [Description("@#")]
    public abstract class ScreenshotOverlayRenderContext<TData, TMeta>
    {
        protected ScreenshotOverlayRenderContext()
        {
        }

        /// <summary>当前截图元数据。</summary>
        [Description("@#screenshot")]
        public extern ScreenshotData Screenshot { get; }

        /// <summary>当前截图时间范围内的 event 列表。</summary>
        [Description("@#events")]
        public extern Array<ScreenshotOverlayEvent<TData, TMeta>> Events { get; }

        /// <summary>当前 event 在 <see cref="Events"/> 中的索引。</summary>
        [Description("@#index")]
        public extern int Index { get; }
    }

    /// <summary>
    /// screenshot overlay renderer 的返回值：HTML element、纯文本或 <c>false</c>。
    /// <c>true</c> 在上游 API 中没有意义，因此调用方应只使用 false 表示不渲染。
    /// </summary>
    [ECMAScript]
    [Description("@#")]
    public readonly union ScreenshotOverlayRenderResult(HTMLElement, string, bool)
    {
        /// <summary>
        /// 返回官方 <c>false</c> 哨兵以跳过当前 overlay。C# 没有 false literal type，
        /// 因此保留 bool 分支并提供这个明确的 authoring entry point。
        /// </summary>
        [ECMAScriptInline("false")]
        public extern static ScreenshotOverlayRenderResult None();
    }

    /// <summary>由 <c>inspectTimelineEvent</c> hook 提供的强类型 event payload。</summary>
    /// <typeparam name="TData">event 的业务数据类型。</typeparam>
    /// <typeparam name="TMeta">event 的可选元数据类型。</typeparam>
    [ECMAScript]
    [Description("@#")]
    public abstract class InspectTimelineEventPayload<TData, TMeta>
    {
        protected InspectTimelineEventPayload()
        {
        }

        /// <summary>所属 Vue app。</summary>
        [Description("@#app")]
        public extern Vue.VueApp App { get; }

        /// <summary>event 所属 layer id。</summary>
        [Description("@#layerId")]
        public extern string LayerId { get; }

        /// <summary>当前被检查的 timeline event。</summary>
        [Description("@#event")]
        public extern TimelineEvent<TData, TMeta> Event { get; }

        /// <summary>是否来自所有 app 的聚合检查。</summary>
        [Description("@#all")]
        public extern bool? All { get; }

        /// <summary>Devtools 暴露给 hook 的可修改 event 数据。</summary>
        [Description("@#data")]
        public extern TData Data { get; set; }
    }
}
