using System;
using System.ComponentModel;

namespace ECMAScript;

/// <summary>
/// Delegate used by <c>@pinia/testing</c> to create spies for actions, patch/reset,
/// and other instrumented callbacks.
/// </summary>
/// <param name="callback">The original callback being wrapped, if one exists.</param>
/// <returns>A spy/wrapper callback that should replace the original callable.</returns>
public delegate Delegate PiniaTestingSpyFactory(Delegate? callback);

/// <summary>
/// Generic delegate used by <c>@pinia/testing</c> when callers want the spy factory
/// to preserve a concrete delegate shape.
/// </summary>
/// <typeparam name="TDelegate">The concrete delegate shape being wrapped.</typeparam>
/// <param name="callback">The original callback being wrapped, if one exists.</param>
/// <returns>A spy/wrapper callback that should replace the original callable.</returns>
public delegate TDelegate PiniaTestingSpyFactory<TDelegate>(TDelegate? callback)
	where TDelegate : Delegate;

/// <summary>
/// Predicate used by <c>@pinia/testing</c> to decide whether a given action on a
/// given store should be stubbed.
/// </summary>
/// <param name="actionName">The action name currently being configured.</param>
/// <param name="store">The concrete store instance owning the action.</param>
/// <returns><c>true</c> when the action should be replaced with a stub.</returns>
public delegate bool PiniaTestingStubActionPredicate(string actionName, Pinia.StoreGeneric store);

/// <summary>
/// Typed predicate used by <c>@pinia/testing</c> when callers want the stub-action
/// decision callback to receive one explicit store projection.
/// </summary>
/// <typeparam name="TStore">The concrete store projection expected by the predicate.</typeparam>
/// <param name="actionName">The action name currently being configured.</param>
/// <param name="store">The concrete store instance owning the action, projected to <typeparamref name="TStore"/>.</param>
/// <returns><c>true</c> when the action should be replaced with a stub.</returns>
public delegate bool PiniaTestingStubActionPredicate<TStore>(string actionName, TStore store)
	where TStore : class;

[ECMAScript("@pinia/testing")]
[Description("@#")]
public static partial class PiniaTesting
{
}
