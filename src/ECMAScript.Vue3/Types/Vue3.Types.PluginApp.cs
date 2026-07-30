using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>Vue app、plugin、provide/inject 相关的结构化类型 contract。</summary>
/// <remarks>该分片只描述 app.use 等 host surface，不负责插件安装或全局注册的运行时实现。</remarks>
public static partial class Vue3
{
	/// <summary>
	/// 直接对象形式的 Vue 插件编写表面。映射到带有 <c>install(app)</c> 函数的普通 JavaScript 对象，可以直接传递给 <see cref="VueApp.Use(VuePlugin)"/> 或 <see cref="VueApp.Use(VuePlugin, VuePluginOptions)"/>。对于类型化安装选项，使用 <see cref="VuePlugin{TOptions}"/>。
	/// Direct object-form Vue plugin authoring surface. This maps to a plain JavaScript
	/// object with an <c>install(app)</c> function and can be passed directly to
	/// <see cref="VueApp.Use(VuePlugin)"/> or <see cref="VueApp.Use(VuePlugin, VuePluginOptions)"/>.
	/// For typed install options, use <see cref="VuePlugin{TOptions}"/>.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public record VuePlugin : IVueOptionsBag
	{
		/// <summary>
		/// 插件安装入口点。当 <c>app.use(plugin)</c> 运行时 Vue 调用此函数。
		/// Plugin installation entrypoint. Vue calls this when <c>app.use(plugin)</c> runs.
		/// </summary>
		[Description("@#install")]
		public VuePluginInstallCallback? Install { get; init; }
	}

	/// <summary>
	/// 类型化对象形式的 Vue 插件编写表面。映射到带有 <c>install(app, options)</c> 函数的普通 JavaScript 对象，其中选项值在编写时保持声明的 <typeparamref name="TOptions"/> 契约。
	/// Typed object-form Vue plugin authoring surface. This maps to a plain JavaScript
	/// object with an <c>install(app, options)</c> function, where the options value
	/// keeps the declared <typeparamref name="TOptions"/> contract at authoring time.
	/// </summary>
	/// <typeparam name="TOptions">类型化的安装选项契约。The typed install options contract.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VuePlugin<TOptions> : IVueOptionsBag
		where TOptions : VuePluginOptions
	{
		/// <summary>
		/// 插件安装入口点。当为当前插件实例运行 <c>app.use(plugin, options)</c> 时 Vue 调用此函数。
		/// Plugin installation entrypoint. Vue calls this when
		/// <c>app.use(plugin, options)</c> runs for the current plugin instance.
		/// </summary>
		[Description("@#install")]
		public VuePluginInstallCallback<TOptions>? Install { get; init; }
	}

	/// <summary>
	/// 由 <c>createApp()</c> 创建的 Vue 应用实例。提供挂载、配置以及全局注册组件、指令和插件的方法。
	/// A Vue application instance created by <c>createApp()</c>. Provides methods for
	/// mounting, configuration, and global registration of components, directives,
	/// and plugins.
	/// </summary>
	public abstract class VueApp
	{
		/// <summary>
		/// 创建此应用实例的 Vue 版本。
		/// The version of Vue that created this application instance.
		/// </summary>
		[Description("@#version")]
		public extern string Version { get; }

		/// <summary>
		/// 应用作用域的 Vue 配置对象。在挂载应用之前修改此对象以配置错误处理、运行时编译器行为、全局选项和自定义选项合并策略。
		/// Application-scoped Vue configuration object. Mutate this before mounting the
		/// app to configure error handling, runtime compiler behavior, globals, and
		/// custom option merge strategies.
		/// </summary>
		[Description("@#config")]
		public extern VueAppConfig Config { get; }

		/// <summary>
		/// 将应用挂载到匹配给定 CSS 选择器的第一个 DOM 元素。挂载的组件成为应用组件树的根。
		/// Mount the application to the first DOM element matching the given CSS selector.
		/// The mounted component becomes the root of the application's component tree.
		/// </summary>
		/// <param name="selector">标识挂载点的 CSS 选择器字符串（例如 <c>"#app"</c>）。A CSS selector string (e.g. <c>"#app"</c>) identifying the mount point.</param>
		/// <returns>挂载的根组件的公开实例。The public instance of the mounted root component.</returns>
		[Description("@#mount")]
		public extern VueComponentPublicInstance Mount(string selector);

		/// <summary>
		/// 将应用直接挂载到特定的 DOM 元素。
		/// Mount the application directly to a specific DOM element.
		/// </summary>
		/// <param name="container">要挂载到的 DOM 元素。元素的现有内容将被替换。The DOM element to mount into. The element's existing content is replaced.</param>
		/// <returns>挂载的根组件的公开实例。The public instance of the mounted root component.</returns>
		[Description("@#mount")]
		public extern VueComponentPublicInstance Mount(Element container);

		/// <summary>
		/// 卸载应用，销毁组件树并清理所有响应式效果、watcher 和事件监听器。
		/// Unmount the application, destroying the component tree and cleaning up all
		/// reactive effects, watchers, and event listeners.
		/// </summary>
		[Description("@#unmount")]
		public extern void Unmount();

		/// <summary>
		/// 注册在应用卸载时运行的回调。
		/// Register a callback to run when the application is unmounted.
		/// </summary>
		/// <param name="callback">在应用卸载期间运行的回调。The callback to run during application unmount.</param>
		/// <returns>应用实例，用于链式调用进一步的配置。The app instance, for chaining further configuration calls.</returns>
		[Description("@#onUnmount")]
		public extern VueApp OnUnmount(Action callback);

		/// <summary>
		/// 安装不带配置选项的 Vue 插件。插件的 <c>install()</c> 方法接收应用实例。
		/// Install a Vue plugin with no configuration options. The plugin's <c>install()</c>
		/// method receives the app instance.
		/// </summary>
		/// <param name="plugin">要安装的插件。必须继承自 <see cref="VuePlugin"/>。The plugin to install. Must inherit from <see cref="VuePlugin"/>.</param>
		/// <returns>应用实例，用于链式调用进一步的配置。The app instance, for chaining further configuration calls.</returns>
		[Description("@#use")]
		public extern VueApp Use(VuePlugin plugin);

		/// <summary>
		/// 安装带有配置选项的 Vue 插件。插件的 <c>install()</c> 方法接收应用实例和选项对象。
		/// Install a Vue plugin with configuration options. The plugin's <c>install()</c>
		/// method receives the app instance and the options object.
		/// </summary>
		/// <param name="plugin">要安装的插件。必须继承自 <see cref="VuePlugin"/>。The plugin to install. Must inherit from <see cref="VuePlugin"/>.</param>
		/// <param name="options">插件特定的配置。必须继承自 <see cref="VuePluginOptions"/>。Plugin-specific configuration. Must inherit from <see cref="VuePluginOptions"/>.</param>
		/// <returns>应用实例，用于链式调用进一步的配置。The app instance, for chaining further configuration calls.</returns>
		[Description("@#use")]
		public extern VueApp Use(VuePlugin plugin, VuePluginOptions options);

		/// <summary>
		/// 安装不带配置选项的函数形式 Vue 插件。回调本身作为插件安装入口点。
		/// Install a function-form Vue plugin with no configuration options. The callback
		/// itself acts as the plugin installation entrypoint.
		/// </summary>
		/// <param name="plugin">函数形式的插件安装回调。The function-form plugin install callback.</param>
		/// <returns>应用实例，用于链式调用进一步的配置。The app instance, for chaining further configuration calls.</returns>
		[Description("@#use")]
		public extern VueApp Use(VuePluginInstallCallback plugin);

		/// <summary>
		/// 安装带有配置选项的函数形式 Vue 插件。Vue 在调用插件回调时将提供的选项作为第二个参数传递。
		/// Install a function-form Vue plugin with configuration options. Vue passes the
		/// supplied options as the second argument when invoking the plugin callback.
		/// </summary>
		/// <param name="plugin">函数形式的插件安装回调。The function-form plugin install callback.</param>
		/// <param name="options">插件特定的配置。Plugin-specific configuration.</param>
		/// <returns>应用实例，用于链式调用进一步的配置。The app instance, for chaining further configuration calls.</returns>
		[Description("@#use")]
		public extern VueApp Use(VuePluginInstallCallback plugin, VuePluginOptions options);

		/// <summary>
		/// 安装带有强类型配置选项的类型化对象形式 Vue 插件。
		/// Install a typed object-form Vue plugin with strongly typed configuration options.
		/// </summary>
		/// <typeparam name="TOptions">类型化的插件选项契约。The typed plugin options contract.</typeparam>
		/// <param name="plugin">要安装的类型化对象形式插件。The typed object-form plugin to install.</param>
		/// <param name="options">传递给插件的强类型选项值。The strongly typed options value passed to the plugin.</param>
		/// <returns>应用实例，用于链式调用进一步的配置。The app instance, for chaining further configuration calls.</returns>
		[Description("@#use")]
		public extern VueApp Use<TOptions>(VuePlugin<TOptions> plugin, TOptions options)
			where TOptions : VuePluginOptions;

		/// <summary>
		/// 安装带有强类型配置选项的函数形式 Vue 插件。
		/// Install a function-form Vue plugin with strongly typed configuration options.
		/// </summary>
		/// <typeparam name="TOptions">类型化的插件选项契约。The typed plugin options contract.</typeparam>
		/// <param name="plugin">类型化的函数形式插件安装回调。The typed function-form plugin install callback.</param>
		/// <param name="options">传递给插件的强类型选项值。The strongly typed options value passed to the plugin.</param>
		/// <returns>应用实例，用于链式调用进一步的配置。The app instance, for chaining further configuration calls.</returns>
		[Description("@#use")]
		public extern VueApp Use<TOptions>(VuePluginInstallCallback<TOptions> plugin, TOptions options)
			where TOptions : VuePluginOptions;

		/// <summary>
		/// 将全局混入应用到由此应用创建的每个组件实例。Vue 文档不推荐在应用代码中使用全局混入；除非库集成特别需要此钩子，否则优先使用显式组合。
		/// Apply a global mixin to every component instance created in this app. Vue's
		/// documentation does not recommend global mixins for application code; prefer
		/// explicit composition unless a library integration specifically needs this hook.
		/// </summary>
		/// <param name="mixin">要合并到每个组件的组件选项对象。The component options object to merge into every component.</param>
		/// <returns>应用实例，用于链式调用进一步的配置。The app instance, for chaining further configuration calls.</returns>
		[Description("@#mixin")]
		public extern VueApp Mixin(VueComponentDefinition mixin);

		/// <summary>
		/// 按名称注册全局组件，使其在所有组件模板中无需显式导入即可使用。
		/// Register a global component by name, making it available in all component
		/// templates without explicit import.
		/// </summary>
		/// <param name="name">要注册的组件名称（例如 <c>"MyButton"</c>）。The component name to register (e.g. <c>"MyButton"</c>).</param>
		/// <param name="component">由 <c>defineComponent()</c> 生成的组件定义。The component definition, produced by <c>defineComponent()</c>.</param>
		/// <returns>应用实例，用于链式调用进一步的配置。The app instance, for chaining further configuration calls.</returns>
		[Description("@#component")]
		public extern VueApp Component(string name, IVueComponent component);

		/// <summary>
		/// 按名称检索先前注册的全局组件。
		/// Retrieve a previously registered global component by name.
		/// </summary>
		/// <param name="name">要查找的已注册组件名称。The registered component name to look up.</param>
		/// <returns>以给定名称注册的组件定义。The component definition registered under the given name.</returns>
		[Description("@#component")]
		public extern IVueComponent Component(string name);

		/// <summary>
		/// 按名称注册全局自定义指令，使其在所有组件模板中以 <c>v-name</c> 形式可用。
		/// Register a global custom directive by name, making it available in all component
		/// templates as <c>v-name</c>.
		/// </summary>
		/// <param name="name">要注册的指令名称（不带 <c>v-</c> 前缀，例如 <c>"focus"</c>）。The directive name to register (without the <c>v-</c> prefix, e.g. <c>"focus"</c>).</param>
		/// <param name="directive">指令定义。必须继承自 <see cref="VueDirective"/>。The directive definition. Must inherit from <see cref="VueDirective"/>.</param>
		/// <returns>应用实例，用于链式调用进一步的配置。The app instance, for chaining further configuration calls.</returns>
		[Description("@#directive")]
		public extern VueApp Directive(string name, VueDirective directive);

		/// <summary>
		/// 按名称注册带有强类型绑定值契约的全局自定义指令。
		/// Register a global custom directive by name with a strongly typed binding value contract.
		/// </summary>
		/// <typeparam name="TValue">指令绑定值的类型化契约。The typed contract of the directive's binding value.</typeparam>
		/// <param name="name">要注册的指令名称（不带 <c>v-</c> 前缀）。The directive name to register (without the <c>v-</c> prefix).</param>
		/// <param name="directive">要注册的类型化指令定义。The typed directive definition to register.</param>
		/// <returns>应用实例，用于链式调用进一步的配置。The app instance, for chaining further configuration calls.</returns>
		[Description("@#directive")]
		public extern VueApp Directive<TValue>(string name, VueDirective<TValue> directive);

		/// <summary>
		/// 使用 Vue 函数简写形式按名称注册全局自定义指令。Vue 在 <c>mounted</c> 和 <c>updated</c> 阶段调用相同的回调。
		/// Register a global custom directive by name using Vue's function shorthand.
		/// Vue invokes the same callback for both the <c>mounted</c> and <c>updated</c> phases.
		/// </summary>
		/// <param name="name">要注册的指令名称（不带 <c>v-</c> 前缀）。The directive name to register (without the <c>v-</c> prefix).</param>
		/// <param name="directive">函数简写回调。The function shorthand callback.</param>
		/// <returns>应用实例，用于链式调用进一步的配置。The app instance, for chaining further configuration calls.</returns>
		[Description("@#directive")]
		public extern VueApp Directive(string name, VueDirectiveFunction directive);

		/// <summary>
		/// 使用 Vue 函数简写形式按名称注册带有强类型绑定值契约的全局自定义指令。Vue 在 <c>mounted</c> 和 <c>updated</c> 阶段调用相同的回调。
		/// Register a global custom directive by name using Vue's function shorthand with a strongly typed binding value contract.
		/// Vue invokes the same callback for both the <c>mounted</c> and <c>updated</c> phases.
		/// </summary>
		/// <typeparam name="TValue">指令绑定值的类型化契约。The typed contract of the directive's binding value.</typeparam>
		/// <param name="name">要注册的指令名称（不带 <c>v-</c> 前缀）。The directive name to register (without the <c>v-</c> prefix).</param>
		/// <param name="directive">类型化函数简写回调。The typed function shorthand callback.</param>
		/// <returns>应用实例，用于链式调用进一步的配置。The app instance, for chaining further configuration calls.</returns>
		[Description("@#directive")]
		public extern VueApp Directive<TValue>(string name, VueDirectiveFunction<TValue> directive);

		/// <summary>
		/// 按名称检索先前注册的全局指令。
		/// Retrieve a previously registered global directive by name.
		/// </summary>
		/// <param name="name">要查找的已注册指令名称（不带 <c>v-</c> 前缀）。The registered directive name to look up (without the <c>v-</c> prefix).</param>
		/// <returns>以给定名称注册的指令定义。The directive definition registered under the given name.</returns>
		[Description("@#directive")]
		public extern VueDirectiveValue Directive(string name);

		/// <summary>
		/// 在应用层面提供值，任何后代组件都可以通过 <c>inject()</c> 注入。应用层面的 provide 对树中的所有组件可用，无论嵌套深度如何。
		/// Provide a value at the application level, injectable by any descendant component
		/// via <c>inject()</c>. Application-level provides are available to all components
		/// in the tree, regardless of nesting depth.
		/// </summary>
		/// <typeparam name="TValue">所提供值的类型。The type of the provided value.</typeparam>
		/// <param name="key"><c>inject()</c> 用于检索值的注入键字符串。The injection key string used by <c>inject()</c> to retrieve the value.</param>
		/// <param name="value">要提供的值。可以是任何类型：原始值、对象、函数等。The value to provide. Can be any type: primitives, objects, functions, etc.</param>
		/// <returns>应用实例，用于链式调用进一步的配置。The app instance, for chaining further configuration calls.</returns>
		[Description("@#provide")]
		public extern VueApp Provide<TValue>(string key, TValue value);

		/// <summary>
		/// 使用强类型注入键在应用层面提供值。
		/// Provide a value at the application level using a strongly typed injection key.
		/// </summary>
		/// <typeparam name="TValue">与注入键关联的值类型。The value type associated with the injection key.</typeparam>
		/// <param name="key"><c>inject()</c> 使用的类型化注入键符号。The typed injection key symbol used by <c>inject()</c>.</param>
		/// <param name="value">要提供的值。The value to provide.</param>
		/// <returns>应用实例，用于链式调用进一步的配置。The app instance, for chaining further configuration calls.</returns>
		[Description("@#provide")]
		public extern VueApp Provide<TValue>(VueInjectionKey<TValue> key, TValue value);

		/// <summary>
		/// 以此应用作为活跃注入上下文运行回调。
		/// Run a callback with this app as the active injection context.
		/// </summary>
		/// <typeparam name="TResult">回调的返回类型。The callback return type.</typeparam>
		/// <param name="callback">在此应用上下文中执行的回调。The callback to execute in this app context.</param>
		/// <returns>回调的结果。The callback result.</returns>
		[Description("@#runWithContext")]
		public extern TResult RunWithContext<TResult>(Func<TResult> callback);
	}

}
