using System;
using System.ComponentModel;

namespace ECMAScript;

/// <summary>
/// <c>@pinia/testing</c> 使用的委托，用于为 action、patch/reset 以及其他被检测的回调创建间谍函数。
/// Delegate used by <c>@pinia/testing</c> to create spies for actions, patch/reset,
/// and other instrumented callbacks.
/// </summary>
/// <param name="callback">被包装的原始回调（如果存在）。The original callback being wrapped, if one exists.</param>
/// <returns>应替换原始可调用对象的间谍/包装回调。A spy/wrapper callback that should replace the original callable.</returns>
public delegate Delegate PiniaTestingSpyFactory(Delegate? callback);

/// <summary>
/// <c>@pinia/testing</c> 使用的泛型委托，当调用方希望间谍工厂保留具体的委托形状时使用。
/// Generic delegate used by <c>@pinia/testing</c> when callers want the spy factory
/// to preserve a concrete delegate shape.
/// </summary>
/// <typeparam name="TDelegate">被包装的具体委托形状。The concrete delegate shape being wrapped.</typeparam>
/// <param name="callback">被包装的原始回调（如果存在）。The original callback being wrapped, if one exists.</param>
/// <returns>应替换原始可调用对象的间谍/包装回调。A spy/wrapper callback that should replace the original callable.</returns>
public delegate TDelegate PiniaTestingSpyFactory<TDelegate>(TDelegate? callback)
	where TDelegate : Delegate;

/// <summary>
/// <c>@pinia/testing</c> 使用的谓词，用于决定给定 store 上的某个 action 是否应被存根替换。
/// Predicate used by <c>@pinia/testing</c> to decide whether a given action on a
/// given store should be stubbed.
/// </summary>
/// <param name="actionName">当前正在配置的 action 名称。The action name currently being configured.</param>
/// <param name="store">拥有该 action 的具体 store 实例。The concrete store instance owning the action.</param>
/// <returns>当 action 应被存根替换时返回 <c>true</c>。<c>true</c> when the action should be replaced with a stub.</returns>
public delegate bool PiniaTestingStubActionPredicate(string actionName, Pinia.StoreGeneric store);

/// <summary>
/// <c>@pinia/testing</c> 使用的类型化谓词，当调用方希望存根-action 决策回调接收一个显式的 store 投影时使用。
/// Typed predicate used by <c>@pinia/testing</c> when callers want the stub-action
/// decision callback to receive one explicit store projection.
/// </summary>
/// <typeparam name="TStore">谓词所期望的具体 store 投影类型。The concrete store projection expected by the predicate.</typeparam>
/// <param name="actionName">当前正在配置的 action 名称。The action name currently being configured.</param>
/// <param name="store">拥有该 action 的具体 store 实例，已投影为 <typeparamref name="TStore"/>。The concrete store instance owning the action, projected to <typeparamref name="TStore"/>.</param>
/// <returns>当 action 应被存根替换时返回 <c>true</c>。<c>true</c> when the action should be replaced with a stub.</returns>
public delegate bool PiniaTestingStubActionPredicate<TStore>(string actionName, TStore store)
	where TStore : class;

[ECMAScript("@pinia/testing")]
[Description("@#")]
public static partial class PiniaTesting
{
}
