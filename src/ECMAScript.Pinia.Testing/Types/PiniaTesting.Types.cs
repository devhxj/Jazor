using System;
using System.ComponentModel;

namespace ECMAScript;

public static partial class PiniaTesting
{
	/// <summary>
	/// 由 <c>createTestingPinia()</c> 返回的 Pinia 根实例。
	/// 保持标准 Pinia 根/运行时契约，同时将该实例标记为来自测试包。
	/// Pinia root returned by <c>createTestingPinia()</c>.
	/// It keeps the normal Pinia root/runtime contract while marking the instance as
	/// originating from the testing package.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract record TestingPinia : Pinia.PiniaInstance
	{
		protected TestingPinia()
		{
		}

		/// <summary>
		/// 当启用 <c>fakeApp</c> 时，由 <c>@pinia/testing</c> 创建的应用实例。
		/// 符合官方测试根契约，插件可等待 Pinia 在应用边界上安装完毕后再执行。
		/// App instance created by <c>@pinia/testing</c> when <c>fakeApp</c> is enabled.
		/// This matches the official testing-root contract where plugins may wait for
		/// Pinia installation on an app boundary before executing.
		/// </summary>
		[Description("@#app")]
		public extern Vue3.VueApp App { get; }
	}

	/// <summary>
	/// <c>@pinia/testing</c> <c>initialState</c> 使用的基础状态种子对象。
	/// 键为 store id，值为对象形式的 patch 负载，在每个 store 实例创建后合并。
	/// Base state-seeding object used by <c>@pinia/testing</c> <c>initialState</c>.
	/// Keys are store ids and values are object-form patch payloads merged after
	/// each store instance is created.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract record TestingInitialState : Vue3.VueProps;

	/// <summary>
	/// <c>@pinia/testing</c> 接受的 <c>stubActions</c> 配置。
	/// Pinia 接受一个全局布尔值、一个显式 action 名称列表、或一个谓词函数，
	/// 用于按 action/store 粒度决定是否对 action 进行 stub 处理。
	/// <c>stubActions</c> configuration accepted by <c>@pinia/testing</c>.
	/// Pinia accepts either one global boolean, one explicit action-name list, or a
	/// predicate that decides on a per-action/per-store basis whether the action
	/// should be stubbed.
	/// </summary>
	[ECMAScript]
	[ECMAScriptUnion]
	[Description("@#")]
	public readonly struct TestingStubActions
	{
		private readonly byte _kind;
		private readonly bool _boolean;
		private readonly string[]? _names;
		private readonly PiniaTestingStubActionPredicate? _predicate;

		/// <summary>
		/// 从布尔值初始化 TestingStubActions 实例。
		/// Initializes a TestingStubActions instance from a boolean value.
		/// </summary>
		/// <param name="value">是否 stub 所有 action。Whether to stub all actions.</param>
		private TestingStubActions(bool value)
		{
			_kind = 1;
			_boolean = value;
			_names = default;
			_predicate = default;
		}

		/// <summary>
		/// 从 action 名称数组初始化 TestingStubActions 实例。
		/// Initializes a TestingStubActions instance from an action name array.
		/// </summary>
		/// <param name="value">要 stub 的 action 名称列表。The list of action names to stub.</param>
		private TestingStubActions(string[] value)
		{
			_kind = 2;
			_boolean = default;
			_names = value;
			_predicate = default;
		}

		/// <summary>
		/// 从谓词函数初始化 TestingStubActions 实例。
		/// Initializes a TestingStubActions instance from a predicate function.
		/// </summary>
		/// <param name="value">用于按 action/store 粒度决定是否 stub 的谓词。The predicate that decides whether to stub on a per-action/per-store basis.</param>
		private TestingStubActions(PiniaTestingStubActionPredicate value)
		{
			_kind = 3;
			_boolean = default;
			_names = default;
			_predicate = value;
		}

		/// <summary>
		/// 以布尔值形式返回，如果不是布尔变体则返回 default。
		/// Returns the value as a boolean, or default if not a boolean variant.
		/// </summary>
		public bool? AsBoolean => _kind == 1 ? _boolean : default(bool?);

		/// <summary>
		/// 以 action 名称数组形式返回，如果不是名称列表变体则返回 default。
		/// Returns the value as an action name array, or default if not a names variant.
		/// </summary>
		public string[]? AsNames => _kind == 2 ? _names : default;

		/// <summary>
		/// 以谓词函数形式返回，如果不是谓词变体则返回 default。
		/// Returns the value as a predicate, or default if not a predicate variant.
		/// </summary>
		public PiniaTestingStubActionPredicate? AsPredicate => _kind == 3 ? _predicate : default;

		/// <summary>
		/// 从布尔值创建 TestingStubActions。
		/// Creates a TestingStubActions from a boolean value.
		/// </summary>
		/// <param name="value">是否 stub 所有 action。Whether to stub all actions.</param>
		/// <returns>布尔变体的 TestingStubActions。A boolean variant of TestingStubActions.</returns>
		[ECMAScriptInline("__arg1")]
		public extern static TestingStubActions From(bool value);

		/// <summary>
		/// 从 action 名称数组创建 TestingStubActions。
		/// Creates a TestingStubActions from an action name array.
		/// </summary>
		/// <param name="value">要 stub 的 action 名称列表。The list of action names to stub.</param>
		/// <returns>名称列表变体的 TestingStubActions。A names variant of TestingStubActions.</returns>
		[ECMAScriptInline("__arg1")]
		public extern static TestingStubActions From(string[] value);

		/// <summary>
		/// 从谓词函数创建 TestingStubActions。
		/// Creates a TestingStubActions from a predicate function.
		/// </summary>
		/// <param name="value">用于按 action/store 粒度决定是否 stub 的谓词。The predicate that decides whether to stub on a per-action/per-store basis.</param>
		/// <returns>谓词变体的 TestingStubActions。A predicate variant of TestingStubActions.</returns>
		[ECMAScriptInline("__arg1")]
		public extern static TestingStubActions From(PiniaTestingStubActionPredicate value);

		/// <summary>
		/// 从布尔值隐式转换为 TestingStubActions。
		/// Implicitly converts a boolean to TestingStubActions.
		/// </summary>
		/// <param name="value">是否 stub 所有 action。Whether to stub all actions.</param>
		public static implicit operator TestingStubActions(bool value)
			=> new(value);

		/// <summary>
		/// 从 action 名称数组隐式转换为 TestingStubActions。
		/// Implicitly converts an action name array to TestingStubActions.
		/// </summary>
		/// <param name="value">要 stub 的 action 名称列表。The list of action names to stub.</param>
		public static implicit operator TestingStubActions(string[] value)
			=> new(value);

		/// <summary>
		/// 从谓词函数隐式转换为 TestingStubActions。
		/// Implicitly converts a predicate to TestingStubActions.
		/// </summary>
		/// <param name="value">用于按 action/store 粒度决定是否 stub 的谓词。The predicate that decides whether to stub on a per-action/per-store basis.</param>
		public static implicit operator TestingStubActions(PiniaTestingStubActionPredicate value)
			=> new(value);
	}

	/// <summary>
	/// <c>@pinia/testing</c> 接受的强类型 <c>stubActions</c> 配置，
	/// 当调用方希望谓词编写接收一个显式 store 投影时使用。
	/// 运行时形态保持与官方 Pinia 联合类型一致：全局布尔值、显式 action 名称列表或谓词函数。
	/// Strongly typed <c>stubActions</c> configuration accepted by
	/// <c>@pinia/testing</c> when the caller wants predicate authoring to receive one
	/// explicit store projection.
	/// Runtime shape remains the same official Pinia union:
	/// global boolean, explicit action-name list, or predicate function.
	/// </summary>
	/// <typeparam name="TStore">谓词分支所期望的具体 store 投影类型。The concrete store projection expected by the predicate branch.</typeparam>
	[ECMAScript]
	[ECMAScriptUnion]
	[Description("@#")]
	public readonly struct TestingStubActions<TStore>
		where TStore : class
	{
		private readonly byte _kind;
		private readonly bool _boolean;
		private readonly string[]? _names;
		private readonly PiniaTestingStubActionPredicate<TStore>? _predicate;

		/// <summary>
		/// 从布尔值初始化 TestingStubActions&lt;TStore&gt; 实例。
		/// Initializes a TestingStubActions&lt;TStore&gt; instance from a boolean value.
		/// </summary>
		/// <param name="value">是否 stub 所有 action。Whether to stub all actions.</param>
		private TestingStubActions(bool value)
		{
			_kind = 1;
			_boolean = value;
			_names = default;
			_predicate = default;
		}

		/// <summary>
		/// 从 action 名称数组初始化 TestingStubActions&lt;TStore&gt; 实例。
		/// Initializes a TestingStubActions&lt;TStore&gt; instance from an action name array.
		/// </summary>
		/// <param name="value">要 stub 的 action 名称列表。The list of action names to stub.</param>
		private TestingStubActions(string[] value)
		{
			_kind = 2;
			_boolean = default;
			_names = value;
			_predicate = default;
		}

		/// <summary>
		/// 从谓词函数初始化 TestingStubActions&lt;TStore&gt; 实例。
		/// Initializes a TestingStubActions&lt;TStore&gt; instance from a predicate function.
		/// </summary>
		/// <param name="value">用于按 action/store 粒度决定是否 stub 的强类型谓词。The strongly typed predicate that decides whether to stub on a per-action/per-store basis.</param>
		private TestingStubActions(PiniaTestingStubActionPredicate<TStore> value)
		{
			_kind = 3;
			_boolean = default;
			_names = default;
			_predicate = value;
		}

		/// <summary>
		/// 以布尔值形式返回，如果不是布尔变体则返回 default。
		/// Returns the value as a boolean, or default if not a boolean variant.
		/// </summary>
		public bool? AsBoolean => _kind == 1 ? _boolean : default(bool?);

		/// <summary>
		/// 以 action 名称数组形式返回，如果不是名称列表变体则返回 default。
		/// Returns the value as an action name array, or default if not a names variant.
		/// </summary>
		public string[]? AsNames => _kind == 2 ? _names : default;

		/// <summary>
		/// 以强类型谓词形式返回，如果不是谓词变体则返回 default。
		/// Returns the value as a strongly typed predicate, or default if not a predicate variant.
		/// </summary>
		public PiniaTestingStubActionPredicate<TStore>? AsPredicate => _kind == 3 ? _predicate : default;

		/// <summary>
		/// 从布尔值创建 TestingStubActions&lt;TStore&gt;。
		/// Creates a TestingStubActions&lt;TStore&gt; from a boolean value.
		/// </summary>
		/// <param name="value">是否 stub 所有 action。Whether to stub all actions.</param>
		/// <returns>布尔变体的 TestingStubActions&lt;TStore&gt;。A boolean variant of TestingStubActions&lt;TStore&gt;.</returns>
		[ECMAScriptInline("__arg1")]
		public extern static TestingStubActions<TStore> From(bool value);

		/// <summary>
		/// 从 action 名称数组创建 TestingStubActions&lt;TStore&gt;。
		/// Creates a TestingStubActions&lt;TStore&gt; from an action name array.
		/// </summary>
		/// <param name="value">要 stub 的 action 名称列表。The list of action names to stub.</param>
		/// <returns>名称列表变体的 TestingStubActions&lt;TStore&gt;。A names variant of TestingStubActions&lt;TStore&gt;.</returns>
		[ECMAScriptInline("__arg1")]
		public extern static TestingStubActions<TStore> From(string[] value);

		/// <summary>
		/// 从强类型谓词函数创建 TestingStubActions&lt;TStore&gt;。
		/// Creates a TestingStubActions&lt;TStore&gt; from a strongly typed predicate function.
		/// </summary>
		/// <param name="value">用于按 action/store 粒度决定是否 stub 的强类型谓词。The strongly typed predicate that decides whether to stub on a per-action/per-store basis.</param>
		/// <returns>谓词变体的 TestingStubActions&lt;TStore&gt;。A predicate variant of TestingStubActions&lt;TStore&gt;.</returns>
		[ECMAScriptInline("__arg1")]
		public extern static TestingStubActions<TStore> From(PiniaTestingStubActionPredicate<TStore> value);

		/// <summary>
		/// 从布尔值隐式转换为 TestingStubActions&lt;TStore&gt;。
		/// Implicitly converts a boolean to TestingStubActions&lt;TStore&gt;.
		/// </summary>
		/// <param name="value">是否 stub 所有 action。Whether to stub all actions.</param>
		public static implicit operator TestingStubActions<TStore>(bool value)
			=> new(value);

		/// <summary>
		/// 从 action 名称数组隐式转换为 TestingStubActions&lt;TStore&gt;。
		/// Implicitly converts an action name array to TestingStubActions&lt;TStore&gt;.
		/// </summary>
		/// <param name="value">要 stub 的 action 名称列表。The list of action names to stub.</param>
		public static implicit operator TestingStubActions<TStore>(string[] value)
			=> new(value);

		/// <summary>
		/// 从强类型谓词隐式转换为 TestingStubActions&lt;TStore&gt;。
		/// Implicitly converts a strongly typed predicate to TestingStubActions&lt;TStore&gt;.
		/// </summary>
		/// <param name="value">用于按 action/store 粒度决定是否 stub 的强类型谓词。The strongly typed predicate that decides whether to stub on a per-action/per-store basis.</param>
		public static implicit operator TestingStubActions<TStore>(PiniaTestingStubActionPredicate<TStore> value)
			=> new(value);
	}

	/// <summary>
	/// <c>createTestingPinia()</c> 接受的测试选项。
	/// 契约保持与 Pinia 官方测试包一致，同时保留显式的 C# 宿主编写类型。
	/// Testing options accepted by <c>createTestingPinia()</c>.
	/// The contract stays close to Pinia's official testing package while preserving
	/// explicit C# host authoring types.
	/// </summary>
	public record TestingOptions : Vue3.VueProps
	{
		/// <summary>
		/// 创建后要种子到 store 中的 Pinia 根状态。
		/// 每个属性键应对应一个 store id，其值为该 store 的对象形式 patch 负载。
		/// Pinia root state to seed into stores after creation.
		/// Each property key should match a store id and its value should be an
		/// object-form patch payload for that store.
		/// </summary>
		[Description("@#initialState")]
		public TestingInitialState? InitialState { get; init; }

		/// <summary>
		/// 在测试插件之前安装的 Pinia 插件。
		/// Pinia plugins to install before the testing plugin.
		/// </summary>
		[Description("@#plugins")]
		public PiniaPlugin[]? Plugins { get; init; }

		/// <summary>
		/// 控制是否默认用 spy 替换 store action。
		/// Controls whether store actions are replaced with spies by default.
		/// </summary>
		[Description("@#stubActions")]
		public TestingStubActions? StubActions { get; init; }

		/// <summary>
		/// 控制测试中 computed/getter 值是否应保持可写。
		/// Controls whether computed/getter values should stay writable in tests.
		/// </summary>
		[Description("@#writableComputed")]
		public bool? WritableComputed { get; init; }

		/// <summary>
		/// 控制是否用 spy 替换 <c>$patch()</c> 并阻止其修改状态。
		/// Controls whether <c>$patch()</c> is replaced with a spy and prevented from
		/// mutating state.
		/// </summary>
		[Description("@#stubPatch")]
		public bool? StubPatch { get; init; }

		/// <summary>
		/// 控制是否用 spy 替换 <c>$reset()</c> 并阻止其修改状态。
		/// Controls whether <c>$reset()</c> is replaced with a spy and prevented from
		/// mutating state.
		/// </summary>
		[Description("@#stubReset")]
		public bool? StubReset { get; init; }

		/// <summary>
		/// 自动在空 Vue 应用上安装测试 Pinia，使依赖应用级安装的插件能在测试中运行。
		/// Installs the testing Pinia on an empty Vue app automatically so plugins
		/// depending on app-level installation can run in tests.
		/// </summary>
		[Description("@#fakeApp")]
		public bool? FakeApp { get; init; }

		/// <summary>
		/// 创建用于包装 action 和 store 方法的 spy 实现。
		/// Creates the spy implementation used for wrapped actions and store methods.
		/// </summary>
		[Description("@#createSpy")]
		public PiniaTestingSpyFactory? CreateSpy { get; init; }
	}

	/// <summary>
	/// <c>createTestingPinia()</c> 接受的类型化测试选项，
	/// 当调用方希望 <c>createSpy</c> 编写保留一个具体委托形态时使用。
	/// 保持相同的运行时 <c>TestingOptions</c> 对象契约，仅增强 <c>createSpy</c> 回调的编译时类型。
	/// Typed testing options accepted by <c>createTestingPinia()</c> when the caller
	/// wants <c>createSpy</c> authoring to preserve one concrete delegate shape.
	/// This keeps the same runtime <c>TestingOptions</c> object contract and only
	/// strengthens the compile-time type of the <c>createSpy</c> callback.
	/// </summary>
	/// <typeparam name="TDelegate"><c>createSpy</c> 所期望的具体委托形态。The concrete delegate shape expected by <c>createSpy</c>.</typeparam>
	public record TestingOptions<TDelegate> : TestingOptions
		where TDelegate : Delegate
	{
		/// <summary>
		/// 创建用于包装 action 和 store 方法的 spy 实现，
		/// 同时在 C# 宿主编写边界保留一个显式委托形态。
		/// Creates the spy implementation used for wrapped actions and store methods
		/// while preserving one explicit delegate shape at the C# authoring boundary.
		/// </summary>
		[Description("@#createSpy")]
		public new PiniaTestingSpyFactory<TDelegate>? CreateSpy { get; init; }
	}

	/// <summary>
	/// <c>createTestingPinia()</c> 接受的类型化测试选项，
	/// 当调用方希望为 <c>createSpy</c> 保留一个显式委托形态，
	/// 并为谓词风格的 <c>stubActions</c> 编写保留一个显式 store 投影时使用。
	/// 运行时对象形态与 <see cref="TestingOptions"/> 相同。
	/// Typed testing options accepted by <c>createTestingPinia()</c> when the caller
	/// wants to preserve one explicit delegate shape for <c>createSpy</c> and one
	/// explicit store projection for predicate-style <c>stubActions</c> authoring.
	/// Runtime object shape remains the same as <see cref="TestingOptions"/>.
	/// </summary>
	/// <typeparam name="TDelegate"><c>createSpy</c> 所期望的具体委托形态。The concrete delegate shape expected by <c>createSpy</c>.</typeparam>
	/// <typeparam name="TStore">谓词风格 <c>stubActions</c> 编写所期望的具体 store 投影类型。The concrete store projection expected by predicate-style <c>stubActions</c>.</typeparam>
	public record TestingOptions<TDelegate, TStore> : TestingOptions<TDelegate>
		where TDelegate : Delegate
		where TStore : class
	{
		/// <summary>
		/// 控制是否默认用 spy 替换 store action，
		/// 同时为谓词风格编写保留一个显式 store 投影。
		/// Controls whether store actions are replaced with spies by default while
		/// preserving one explicit store projection for predicate-style authoring.
		/// </summary>
		[Description("@#stubActions")]
		public new TestingStubActions<TStore>? StubActions { get; init; }
	}
}
