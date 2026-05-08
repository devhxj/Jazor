using System.ComponentModel;

namespace ECMAScript;

public static partial class PiniaTesting
{
	private const string IdentityInlineTemplate = "__arg1";

	/// <summary>
	/// 创建一个为组件/单元测试配置的 Pinia 根实例。
	/// 此方法镜像了 <c>@pinia/testing</c> 中的 <c>createTestingPinia(options?)</c>。
	/// <para/>
	/// Creates a Pinia root instance configured for component/unit testing.
	/// This mirrors <c>createTestingPinia(options?)</c> from <c>@pinia/testing</c>.
	/// </summary>
	/// <param name="options">可选的测试配置。 / Optional testing configuration.</param>
	/// <returns>适用于测试时 store 解析的 Pinia 实例。 / A Pinia instance suitable for test-time store resolution.</returns>
	[Description("@#createTestingPinia")]
	public extern static TestingPinia CreateTestingPinia();

	/// <summary>
	/// 创建一个为组件/单元测试配置的 Pinia 根实例。
	/// 此方法镜像了 <c>@pinia/testing</c> 中的 <c>createTestingPinia(options?)</c>。
	/// <para/>
	/// Creates a Pinia root instance configured for component/unit testing.
	/// This mirrors <c>createTestingPinia(options?)</c> from <c>@pinia/testing</c>.
	/// </summary>
	/// <param name="options">可选的测试配置。 / Optional testing configuration.</param>
	/// <returns>适用于测试时 store 解析的 Pinia 实例。 / A Pinia instance suitable for test-time store resolution.</returns>
	[Description("@#createTestingPinia")]
	public extern static TestingPinia CreateTestingPinia(TestingOptions options);

	/// <summary>
	/// 将带类型的 Pinia 插件回调投影到 <see cref="TestingOptions.Plugins"/> 所需的无类型运行时插件形态。
	/// 此方法仅进行编译期投影，不会创建新的运行时函数对象。
	/// <para/>
	/// Projects a typed Pinia plugin callback to the untyped runtime plugin shape
	/// required by <see cref="TestingOptions.Plugins"/>.
	/// This is a compile-time projection only and does not create a new runtime
	/// function object.
	/// </summary>
	/// <typeparam name="TStore">插件上下文提供的带类型的 store 投影。 / The typed store projection supplied by the plugin context.</typeparam>
	/// <param name="plugin">要投影的带类型插件回调。 / The typed plugin callback to project.</param>
	/// <returns>投影到无类型测试选项表面的同一运行时插件回调。 / The same runtime plugin callback projected to the untyped testing-options surface.</returns>
	[ECMAScriptInline(IdentityInlineTemplate)]
	public extern static PiniaPlugin ProjectPlugin<TStore>(PiniaPlugin<TStore> plugin)
		where TStore : class;

	/// <summary>
	/// 将带类型的 <c>stubActions</c> 谓词投影到 <see cref="TestingStubActions"/> 所需的无类型运行时谓词形态。
	/// 此方法仅进行编译期投影，不会创建新的运行时函数对象。
	/// <para/>
	/// Projects a typed <c>stubActions</c> predicate to the untyped runtime predicate
	/// shape required by <see cref="TestingStubActions"/>.
	/// This is a compile-time projection only and does not create a new runtime
	/// function object.
	/// </summary>
	/// <typeparam name="TStore">提供给谓词的带类型的 store 投影。 / The typed store projection supplied to the predicate.</typeparam>
	/// <param name="predicate">要投影的带类型 stub-action 谓词。 / The typed stub-action predicate to project.</param>
	/// <returns>投影到无类型测试选项表面的同一运行时谓词。 / The same runtime predicate projected to the untyped testing-options surface.</returns>
	[ECMAScriptInline(IdentityInlineTemplate)]
	public extern static PiniaTestingStubActionPredicate ProjectStubActionPredicate<TStore>(PiniaTestingStubActionPredicate<TStore> predicate)
		where TStore : class;

	/// <summary>
	/// 将带类型的 <c>stubActions</c> 谓词直接投影到 <see cref="TestingOptions{TDelegate,TStore}.StubActions"/> 所使用的带类型测试联合表面。
	/// 此方法仅进行编译期投影，不会创建新的运行时谓词或包装对象。
	/// <para/>
	/// Projects a typed <c>stubActions</c> predicate directly to the typed testing
	/// union surface used by <see cref="TestingOptions{TDelegate,TStore}.StubActions"/>.
	/// This is a compile-time projection only and does not create a new runtime
	/// predicate or wrapper object.
	/// </summary>
	/// <typeparam name="TStore">提供给谓词的带类型的 store 投影。 / The typed store projection supplied to the predicate.</typeparam>
	/// <param name="predicate">要投影的带类型 stub-action 谓词。 / The typed stub-action predicate to project.</param>
	/// <returns>投影到带类型 <c>stubActions</c> 联合表面的同一运行时谓词。 / The same runtime predicate projected to the typed <c>stubActions</c> union surface.</returns>
	[ECMAScriptInline(IdentityInlineTemplate)]
	public extern static TestingStubActions<TStore> ProjectStubActions<TStore>(PiniaTestingStubActionPredicate<TStore> predicate)
		where TStore : class;

	/// <summary>
	/// 将带有带类型 store 定义选项的带类型 Pinia 插件回调投影到 <see cref="TestingOptions.Plugins"/> 所需的无类型运行时插件形态。
	/// 此方法仅进行编译期投影，不会创建新的运行时函数对象。
	/// <para/>
	/// Projects a typed Pinia plugin callback with typed store-definition options to
	/// the untyped runtime plugin shape required by <see cref="TestingOptions.Plugins"/>.
	/// This is a compile-time projection only and does not create a new runtime
	/// function object.
	/// </summary>
	/// <typeparam name="TStore">插件上下文提供的带类型的 store 投影。 / The typed store projection supplied by the plugin context.</typeparam>
	/// <typeparam name="TOptions">带类型的插件可见选项投影。 / The typed plugin-visible options projection.</typeparam>
	/// <param name="plugin">要投影的带类型插件回调。 / The typed plugin callback to project.</param>
	/// <returns>投影到无类型测试选项表面的同一运行时插件回调。 / The same runtime plugin callback projected to the untyped testing-options surface.</returns>
	[ECMAScriptInline(IdentityInlineTemplate)]
	public extern static PiniaPlugin ProjectPlugin<TStore, TOptions>(PiniaPlugin<TStore, TOptions> plugin)
		where TStore : class
		where TOptions : Pinia.DefineStoreOptionsInPlugin;

	/// <summary>
	/// 将合并扩展对象同样带类型的完全带类型 Pinia 插件回调投影到 <see cref="TestingOptions.Plugins"/> 所需的无类型运行时插件形态。
	/// 此方法仅进行编译期投影，不会创建新的运行时函数对象。
	/// <para/>
	/// Projects a fully typed Pinia plugin callback whose merged extension object is
	/// also typed to the untyped runtime plugin shape required by
	/// <see cref="TestingOptions.Plugins"/>.
	/// This is a compile-time projection only and does not create a new runtime
	/// function object.
	/// </summary>
	/// <typeparam name="TStore">插件上下文提供的带类型的 store 投影。 / The typed store projection supplied by the plugin context.</typeparam>
	/// <typeparam name="TOptions">带类型的插件可见选项投影。 / The typed plugin-visible options projection.</typeparam>
	/// <typeparam name="TExtension">插件返回的带类型扩展对象。 / The typed extension object returned by the plugin.</typeparam>
	/// <param name="plugin">要投影的带类型插件回调。 / The typed plugin callback to project.</param>
	/// <returns>投影到无类型测试选项表面的同一运行时插件回调。 / The same runtime plugin callback projected to the untyped testing-options surface.</returns>
	[ECMAScriptInline(IdentityInlineTemplate)]
	public extern static PiniaPlugin ProjectPlugin<TStore, TOptions, TExtension>(PiniaPlugin<TStore, TOptions, TExtension> plugin)
		where TStore : class
		where TOptions : Pinia.DefineStoreOptionsInPlugin
		where TExtension : Vue3.VueProps;

	/// <summary>
	/// 将上下文同时暴露显式插件添加的自定义 store 属性的带类型 Pinia 插件回调投影到 <see cref="TestingOptions.Plugins"/> 所需的无类型运行时插件形态。
	/// 此方法仅进行编译期投影，不会创建新的运行时函数对象。
	/// <para/>
	/// Projects a typed Pinia plugin callback whose context also exposes explicit
	/// plugin-added custom store properties to the untyped runtime plugin shape
	/// required by <see cref="TestingOptions.Plugins"/>.
	/// This is a compile-time projection only and does not create a new runtime
	/// function object.
	/// </summary>
	/// <typeparam name="TStore">插件上下文提供的基础带类型 store 投影。 / The base typed store projection supplied by the plugin context.</typeparam>
	/// <typeparam name="TOptions">带类型的插件可见选项投影。 / The typed plugin-visible options projection.</typeparam>
	/// <typeparam name="TCustomProperties">当前 store 上已可见的插件添加的自定义 store 属性。 / The plugin-added custom store properties already visible on the current store.</typeparam>
	/// <typeparam name="TExtension">插件返回的带类型扩展对象。 / The typed extension object returned by the plugin.</typeparam>
	/// <param name="plugin">要投影的带类型插件回调。 / The typed plugin callback to project.</param>
	/// <returns>投影到无类型测试选项表面的同一运行时插件回调。 / The same runtime plugin callback projected to the untyped testing-options surface.</returns>
	[ECMAScriptInline(IdentityInlineTemplate)]
	public extern static PiniaPlugin ProjectPlugin<TStore, TOptions, TCustomProperties, TExtension>(PiniaPlugin<TStore, TOptions, TCustomProperties, TExtension> plugin)
		where TStore : class
		where TOptions : Pinia.DefineStoreOptionsInPlugin
		where TCustomProperties : Vue3.VueProps
		where TExtension : Vue3.VueProps;

	/// <summary>
	/// 将上下文同时暴露显式插件添加的自定义 store 属性和自定义状态的带类型 Pinia 插件回调投影到 <see cref="TestingOptions.Plugins"/> 所需的无类型运行时插件形态。
	/// 此方法仅进行编译期投影，不会创建新的运行时函数对象。
	/// <para/>
	/// Projects a typed Pinia plugin callback whose context also exposes explicit
	/// plugin-added custom store properties and custom state to the untyped runtime
	/// plugin shape required by <see cref="TestingOptions.Plugins"/>.
	/// This is a compile-time projection only and does not create a new runtime
	/// function object.
	/// </summary>
	/// <typeparam name="TStore">插件上下文提供的基础带类型 store 投影。 / The base typed store projection supplied by the plugin context.</typeparam>
	/// <typeparam name="TOptions">带类型的插件可见选项投影。 / The typed plugin-visible options projection.</typeparam>
	/// <typeparam name="TCustomProperties">当前 store 上已可见的插件添加的自定义 store 属性。 / The plugin-added custom store properties already visible on the current store.</typeparam>
	/// <typeparam name="TCustomState"><c>store.$state</c> 上已可见的插件添加的自定义状态。 / The plugin-added custom state already visible on <c>store.$state</c>.</typeparam>
	/// <typeparam name="TExtension">插件返回的带类型扩展对象。 / The typed extension object returned by the plugin.</typeparam>
	/// <param name="plugin">要投影的带类型插件回调。 / The typed plugin callback to project.</param>
	/// <returns>投影到无类型测试选项表面的同一运行时插件回调。 / The same runtime plugin callback projected to the untyped testing-options surface.</returns>
	[ECMAScriptInline(IdentityInlineTemplate)]
	public extern static PiniaPlugin ProjectPlugin<TStore, TOptions, TCustomProperties, TCustomState, TExtension>(PiniaPlugin<TStore, TOptions, TCustomProperties, TCustomState, TExtension> plugin)
		where TStore : class
		where TOptions : Pinia.DefineStoreOptionsInPlugin
		where TCustomProperties : Vue3.VueProps
		where TCustomState : Pinia.PiniaStateTree
		where TExtension : Vue3.VueProps;
}
