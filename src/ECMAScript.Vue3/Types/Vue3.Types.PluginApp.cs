using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

public static partial class Vue3
{
	/// <summary>
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
		/// Plugin installation entrypoint. Vue calls this when <c>app.use(plugin)</c> runs.
		/// </summary>
		[Description("@#install")]
		public VuePluginInstallCallback? Install { get; init; }
	}

	/// <summary>
	/// Typed object-form Vue plugin authoring surface. This maps to a plain JavaScript
	/// object with an <c>install(app, options)</c> function, where the options value
	/// keeps the declared <typeparamref name="TOptions"/> contract at authoring time.
	/// </summary>
	/// <typeparam name="TOptions">The typed install options contract.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public record VuePlugin<TOptions> : IVueOptionsBag
		where TOptions : VuePluginOptions
	{
		/// <summary>
		/// Plugin installation entrypoint. Vue calls this when
		/// <c>app.use(plugin, options)</c> runs for the current plugin instance.
		/// </summary>
		[Description("@#install")]
		public VuePluginInstallCallback<TOptions>? Install { get; init; }
	}

	/// <summary>
	/// A Vue application instance created by <c>createApp()</c>. Provides methods for
	/// mounting, configuration, and global registration of components, directives,
	/// and plugins.
	/// </summary>
	public abstract class VueApp
	{
		/// <summary>
		/// The version of Vue that created this application instance.
		/// </summary>
		[Description("@#version")]
		public extern string Version { get; }

		/// <summary>
		/// Application-scoped Vue configuration object. Mutate this before mounting the
		/// app to configure error handling, runtime compiler behavior, globals, and
		/// custom option merge strategies.
		/// </summary>
		[Description("@#config")]
		public extern VueAppConfig Config { get; }

		/// <summary>
		/// Mount the application to the first DOM element matching the given CSS selector.
		/// The mounted component becomes the root of the application's component tree.
		/// </summary>
		/// <param name="selector">A CSS selector string (e.g. <c>"#app"</c>) identifying the mount point.</param>
		/// <returns>The public instance of the mounted root component.</returns>
		[Description("@#mount")]
		public extern VueComponentPublicInstance Mount(string selector);

		/// <summary>
		/// Mount the application directly to a specific DOM element.
		/// </summary>
		/// <param name="container">The DOM element to mount into. The element's existing content is replaced.</param>
		/// <returns>The public instance of the mounted root component.</returns>
		[Description("@#mount")]
		public extern VueComponentPublicInstance Mount(Element container);

		/// <summary>
		/// Unmount the application, destroying the component tree and cleaning up all
		/// reactive effects, watchers, and event listeners.
		/// </summary>
		[Description("@#unmount")]
		public extern void Unmount();

		/// <summary>
		/// Register a callback to run when the application is unmounted.
		/// </summary>
		/// <param name="callback">The callback to run during application unmount.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#onUnmount")]
		public extern VueApp OnUnmount(Action callback);

		/// <summary>
		/// Install a Vue plugin with no configuration options. The plugin's <c>install()</c>
		/// method receives the app instance.
		/// </summary>
		/// <param name="plugin">The plugin to install. Must inherit from <see cref="VuePlugin"/>.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#use")]
		public extern VueApp Use(VuePlugin plugin);

		/// <summary>
		/// Install a Vue plugin with configuration options. The plugin's <c>install()</c>
		/// method receives the app instance and the options object.
		/// </summary>
		/// <param name="plugin">The plugin to install. Must inherit from <see cref="VuePlugin"/>.</param>
		/// <param name="options">Plugin-specific configuration. Must inherit from <see cref="VuePluginOptions"/>.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#use")]
		public extern VueApp Use(VuePlugin plugin, VuePluginOptions options);

		/// <summary>
		/// Install a function-form Vue plugin with no configuration options. The callback
		/// itself acts as the plugin installation entrypoint.
		/// </summary>
		/// <param name="plugin">The function-form plugin install callback.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#use")]
		public extern VueApp Use(VuePluginInstallCallback plugin);

		/// <summary>
		/// Install a function-form Vue plugin with configuration options. Vue passes the
		/// supplied options as the second argument when invoking the plugin callback.
		/// </summary>
		/// <param name="plugin">The function-form plugin install callback.</param>
		/// <param name="options">Plugin-specific configuration.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#use")]
		public extern VueApp Use(VuePluginInstallCallback plugin, VuePluginOptions options);

		/// <summary>
		/// Install a typed object-form Vue plugin with strongly typed configuration options.
		/// </summary>
		/// <typeparam name="TOptions">The typed plugin options contract.</typeparam>
		/// <param name="plugin">The typed object-form plugin to install.</param>
		/// <param name="options">The strongly typed options value passed to the plugin.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#use")]
		public extern VueApp Use<TOptions>(VuePlugin<TOptions> plugin, TOptions options)
			where TOptions : VuePluginOptions;

		/// <summary>
		/// Install a function-form Vue plugin with strongly typed configuration options.
		/// </summary>
		/// <typeparam name="TOptions">The typed plugin options contract.</typeparam>
		/// <param name="plugin">The typed function-form plugin install callback.</param>
		/// <param name="options">The strongly typed options value passed to the plugin.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#use")]
		public extern VueApp Use<TOptions>(VuePluginInstallCallback<TOptions> plugin, TOptions options)
			where TOptions : VuePluginOptions;

		/// <summary>
		/// Apply a global mixin to every component instance created in this app. Vue's
		/// documentation does not recommend global mixins for application code; prefer
		/// explicit composition unless a library integration specifically needs this hook.
		/// </summary>
		/// <param name="mixin">The component options object to merge into every component.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#mixin")]
		public extern VueApp Mixin(VueComponentDefinition mixin);

		/// <summary>
		/// Register a global component by name, making it available in all component
		/// templates without explicit import.
		/// </summary>
		/// <param name="name">The component name to register (e.g. <c>"MyButton"</c>).</param>
		/// <param name="component">The component definition, produced by <c>defineComponent()</c>.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#component")]
		public extern VueApp Component(string name, IVueComponent component);

		/// <summary>
		/// Retrieve a previously registered global component by name.
		/// </summary>
		/// <param name="name">The registered component name to look up.</param>
		/// <returns>The component definition registered under the given name.</returns>
		[Description("@#component")]
		public extern IVueComponent Component(string name);

		/// <summary>
		/// Register a global custom directive by name, making it available in all component
		/// templates as <c>v-name</c>.
		/// </summary>
		/// <param name="name">The directive name to register (without the <c>v-</c> prefix, e.g. <c>"focus"</c>).</param>
		/// <param name="directive">The directive definition. Must inherit from <see cref="VueDirective"/>.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#directive")]
		public extern VueApp Directive(string name, VueDirective directive);

		/// <summary>
		/// Register a global custom directive by name with a strongly typed binding value contract.
		/// </summary>
		/// <typeparam name="TValue">The typed contract of the directive's binding value.</typeparam>
		/// <param name="name">The directive name to register (without the <c>v-</c> prefix).</param>
		/// <param name="directive">The typed directive definition to register.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#directive")]
		public extern VueApp Directive<TValue>(string name, VueDirective<TValue> directive);

		/// <summary>
		/// Register a global custom directive by name using Vue's function shorthand.
		/// Vue invokes the same callback for both the <c>mounted</c> and <c>updated</c> phases.
		/// </summary>
		/// <param name="name">The directive name to register (without the <c>v-</c> prefix).</param>
		/// <param name="directive">The function shorthand callback.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#directive")]
		public extern VueApp Directive(string name, VueDirectiveFunction directive);

		/// <summary>
		/// Register a global custom directive by name using Vue's function shorthand with a strongly typed binding value contract.
		/// Vue invokes the same callback for both the <c>mounted</c> and <c>updated</c> phases.
		/// </summary>
		/// <typeparam name="TValue">The typed contract of the directive's binding value.</typeparam>
		/// <param name="name">The directive name to register (without the <c>v-</c> prefix).</param>
		/// <param name="directive">The typed function shorthand callback.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#directive")]
		public extern VueApp Directive<TValue>(string name, VueDirectiveFunction<TValue> directive);

		/// <summary>
		/// Retrieve a previously registered global directive by name.
		/// </summary>
		/// <param name="name">The registered directive name to look up (without the <c>v-</c> prefix).</param>
		/// <returns>The directive definition registered under the given name.</returns>
		[Description("@#directive")]
		public extern VueDirectiveValue Directive(string name);

		/// <summary>
		/// Provide a value at the application level, injectable by any descendant component
		/// via <c>inject()</c>. Application-level provides are available to all components
		/// in the tree, regardless of nesting depth.
		/// </summary>
		/// <typeparam name="TValue">The type of the provided value.</typeparam>
		/// <param name="key">The injection key string used by <c>inject()</c> to retrieve the value.</param>
		/// <param name="value">The value to provide. Can be any type: primitives, objects, functions, etc.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#provide")]
		public extern VueApp Provide<TValue>(string key, TValue value);

		/// <summary>
		/// Provide a value at the application level using a strongly typed injection key.
		/// </summary>
		/// <typeparam name="TValue">The value type associated with the injection key.</typeparam>
		/// <param name="key">The typed injection key symbol used by <c>inject()</c>.</param>
		/// <param name="value">The value to provide.</param>
		/// <returns>The app instance, for chaining further configuration calls.</returns>
		[Description("@#provide")]
		public extern VueApp Provide<TValue>(VueInjectionKey<TValue> key, TValue value);

		/// <summary>
		/// Run a callback with this app as the active injection context.
		/// </summary>
		/// <typeparam name="TResult">The callback return type.</typeparam>
		/// <param name="callback">The callback to execute in this app context.</param>
		/// <returns>The callback result.</returns>
		[Description("@#runWithContext")]
		public extern TResult RunWithContext<TResult>(Func<TResult> callback);
	}

}
