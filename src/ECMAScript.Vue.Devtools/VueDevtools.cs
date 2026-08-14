using System;
using System.ComponentModel;

namespace ECMAScript;

/// <summary>
/// Vue Devtools plugin setup callback. 插件注册后由 Devtools 在可用时调用，
/// <paramref name="api"/> 的生命周期由 Vue Devtools 管理，不能在 C# 端自行构造。
/// </summary>
/// <param name="api">当前插件专属的 Devtools API handle。</param>
public delegate void DevtoolsPluginSetupCallback(VueDevtools.PluginApi api);

/// <summary>
/// 带强类型 settings 投影的 plugin setup callback。泛型仅约束 authoring surface，
/// JavaScript 端仍直接使用 Vue Devtools 的 settings object。
/// </summary>
/// <typeparam name="TSettings">应用声明的 settings 值形状。</typeparam>
/// <param name="api">带有 <typeparamref name="TSettings"/> 投影的 API handle。</param>
public delegate void DevtoolsPluginSetupCallback<TSettings>(VueDevtools.PluginApi<TSettings> api)
    where TSettings : Vue.VueProps;

/// <summary>
/// Devtools 或 Devtools client 连接完成时运行的回调。
/// </summary>
public delegate void DevtoolsConnectionCallback();

/// <summary>
/// 访问 Vue component tree 时的 hook callback。
/// </summary>
/// <param name="payload">由 Devtools 提供且可修改的树节点 payload。</param>
public delegate void DevtoolsVisitComponentTreeCallback(VueDevtools.VisitComponentTreePayload payload);

/// <summary>
/// 检查 Vue component state 时的 hook callback。
/// </summary>
/// <param name="payload">由 Devtools 提供的组件检查 payload。</param>
public delegate void DevtoolsInspectComponentCallback(VueDevtools.InspectComponentPayload payload);

/// <summary>
/// 编辑 component state 时的 hook callback。
/// </summary>
/// <param name="payload">包含目标路径、变更值和官方 setter 的编辑 payload。</param>
public delegate void DevtoolsEditComponentStateCallback(VueDevtools.EditComponentStatePayload payload);

/// <summary>
/// Devtools 请求 custom inspector tree 时的 hook callback。
/// </summary>
/// <param name="payload">可向 <c>RootNodes</c> 写入节点的 inspector tree payload。</param>
public delegate void DevtoolsGetInspectorTreeCallback(VueDevtools.GetInspectorTreePayload payload);

/// <summary>
/// Devtools 请求 custom inspector state 时的 hook callback。
/// </summary>
/// <param name="payload">可向 <c>State</c> 写入分组状态的 inspector state payload。</param>
public delegate void DevtoolsGetInspectorStateCallback(VueDevtools.GetInspectorStatePayload payload);

/// <summary>
/// 编辑 custom inspector state 时的 hook callback。
/// </summary>
/// <param name="payload">包含状态变更和官方 setter 的 inspector 编辑 payload。</param>
public delegate void DevtoolsEditInspectorStateCallback(VueDevtools.EditInspectorStatePayload payload);

/// <summary>
/// 检查 timeline event 的强类型 hook callback。
/// </summary>
/// <typeparam name="TData">timeline event 的业务数据类型。</typeparam>
/// <typeparam name="TMeta">timeline event 的可选元数据类型。</typeparam>
/// <param name="payload">由 Devtools 提供的 timeline event payload。</param>
public delegate void DevtoolsInspectTimelineEventCallback<TData, TMeta>(
    VueDevtools.InspectTimelineEventPayload<TData, TMeta> payload);

/// <summary>
/// Devtools 清空 timeline 时的 hook callback。
/// </summary>
public delegate void DevtoolsTimelineClearedCallback();

/// <summary>
/// settings 改变时的 hook callback；泛型投影避免业务代码重新回退到 untyped object。
/// </summary>
/// <typeparam name="TSettings">应用声明的 settings 值形状。</typeparam>
/// <param name="payload">包含新旧值与完整 settings 的 payload。</param>
public delegate void DevtoolsSetPluginSettingsCallback<TSettings>(
    VueDevtools.SetPluginSettingsPayload<TSettings> payload)
    where TSettings : Vue.VueProps;

/// <summary>
/// 同步 custom inspector toolbar action。
/// </summary>
public delegate void DevtoolsInspectorActionCallback();

/// <summary>
/// 异步 custom inspector toolbar action。返回值必须是 JavaScript Promise shape，
/// Devtools 据此等待 action 完成。
/// </summary>
public delegate IPromise DevtoolsAsyncInspectorActionCallback();

/// <summary>
/// 同步 custom inspector node action。
/// </summary>
/// <param name="nodeId">当前选中的 inspector node id。</param>
public delegate void DevtoolsInspectorNodeActionCallback(string nodeId);

/// <summary>
/// 异步 custom inspector node action。
/// </summary>
/// <param name="nodeId">当前选中的 inspector node id。</param>
/// <returns>完成 action 的 JavaScript Promise。</returns>
public delegate IPromise DevtoolsAsyncInspectorNodeActionCallback(string nodeId);

/// <summary>
/// Devtools state setter 在实际写入字段后调用的通知回调。
/// </summary>
/// <typeparam name="TTarget">被编辑的状态对象类型。</typeparam>
/// <typeparam name="TValue">写入值的类型。</typeparam>
/// <param name="target">被修改的对象。</param>
/// <param name="field">实际写入的最终字段名。</param>
/// <param name="value">已经写入的值。</param>
public delegate void DevtoolsStateSetCallback<TTarget, TValue>(TTarget target, string field, TValue value);

/// <summary>
/// 同步 screenshot overlay renderer。
/// </summary>
/// <typeparam name="TData">timeline event 的业务数据类型。</typeparam>
/// <typeparam name="TMeta">timeline event 的可选元数据类型。</typeparam>
/// <param name="@event">当前 overlay 对应的 event。</param>
/// <param name="context">当前截图与 event 序列上下文。</param>
/// <returns>HTML element、文本或 <c>false</c> 哨兵。</returns>
public delegate VueDevtools.ScreenshotOverlayRenderResult DevtoolsScreenshotOverlayRenderCallback<TData, TMeta>(
    VueDevtools.ScreenshotOverlayEvent<TData, TMeta> @event,
    VueDevtools.ScreenshotOverlayRenderContext<TData, TMeta> context);

/// <summary>
/// 异步 screenshot overlay renderer。
/// </summary>
/// <typeparam name="TData">timeline event 的业务数据类型。</typeparam>
/// <typeparam name="TMeta">timeline event 的可选元数据类型。</typeparam>
/// <param name="@event">当前 overlay 对应的 event。</param>
/// <param name="context">当前截图与 event 序列上下文。</param>
/// <returns>兑现为 overlay 结果的 JavaScript Promise。</returns>
public delegate IPromise<VueDevtools.ScreenshotOverlayRenderResult> DevtoolsAsyncScreenshotOverlayRenderCallback<TData, TMeta>(
    VueDevtools.ScreenshotOverlayEvent<TData, TMeta> @event,
    VueDevtools.ScreenshotOverlayRenderContext<TData, TMeta> context);

/// <summary>
/// Vue Devtools Plugin API 的模块入口。该 binding 只封装官方 plugin authoring surface，
/// 不暴露浏览器扩展、Vite/Electron RPC 或 <c>@vue/devtools-kit</c> 内部对象。
/// </summary>
[ECMAScript("@vue/devtools-api")]
[Description("@#")]
public static partial class VueDevtools
{
}
