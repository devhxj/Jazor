using System;
using System.ComponentModel;

namespace ECMAScript;

public static partial class PiniaTesting
{
	/// <summary>
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
		/// App instance created by <c>@pinia/testing</c> when <c>fakeApp</c> is enabled.
		/// This matches the official testing-root contract where plugins may wait for
		/// Pinia installation on an app boundary before executing.
		/// </summary>
		[Description("@#app")]
		public extern Vue3.VueApp App { get; }
	}

	/// <summary>
	/// Base state-seeding object used by <c>@pinia/testing</c> <c>initialState</c>.
	/// Keys are store ids and values are object-form patch payloads merged after
	/// each store instance is created.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract record TestingInitialState : Vue3.VueProps;

	/// <summary>
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

		private TestingStubActions(bool value)
		{
			_kind = 1;
			_boolean = value;
			_names = default;
			_predicate = default;
		}

		private TestingStubActions(string[] value)
		{
			_kind = 2;
			_boolean = default;
			_names = value;
			_predicate = default;
		}

		private TestingStubActions(PiniaTestingStubActionPredicate value)
		{
			_kind = 3;
			_boolean = default;
			_names = default;
			_predicate = value;
		}

		public bool? AsBoolean => _kind == 1 ? _boolean : default(bool?);

		public string[]? AsNames => _kind == 2 ? _names : default;

		public PiniaTestingStubActionPredicate? AsPredicate => _kind == 3 ? _predicate : default;

		public static implicit operator TestingStubActions(bool value)
			=> new(value);

		public static implicit operator TestingStubActions(string[] value)
			=> new(value);

		public static implicit operator TestingStubActions(PiniaTestingStubActionPredicate value)
			=> new(value);
	}

	/// <summary>
	/// Testing options accepted by <c>createTestingPinia()</c>.
	/// The contract stays close to Pinia's official testing package while preserving
	/// explicit C# host authoring types.
	/// </summary>
	public record TestingOptions : Vue3.VueProps
	{
		/// <summary>
		/// Pinia root state to seed into stores after creation.
		/// Each property key should match a store id and its value should be an
		/// object-form patch payload for that store.
		/// </summary>
		[Description("@#initialState")]
		public TestingInitialState? InitialState { get; init; }

		/// <summary>
		/// Pinia plugins to install before the testing plugin.
		/// </summary>
		[Description("@#plugins")]
		public PiniaPlugin[]? Plugins { get; init; }

		/// <summary>
		/// Controls whether store actions are replaced with spies by default.
		/// </summary>
		[Description("@#stubActions")]
		public TestingStubActions? StubActions { get; init; }

		/// <summary>
		/// Controls whether computed/getter values should stay writable in tests.
		/// </summary>
		[Description("@#writableComputed")]
		public bool? WritableComputed { get; init; }

		/// <summary>
		/// Controls whether <c>$patch()</c> is replaced with a spy and prevented from
		/// mutating state.
		/// </summary>
		[Description("@#stubPatch")]
		public bool? StubPatch { get; init; }

		/// <summary>
		/// Controls whether <c>$reset()</c> is replaced with a spy and prevented from
		/// mutating state.
		/// </summary>
		[Description("@#stubReset")]
		public bool? StubReset { get; init; }

		/// <summary>
		/// Installs the testing Pinia on an empty Vue app automatically so plugins
		/// depending on app-level installation can run in tests.
		/// </summary>
		[Description("@#fakeApp")]
		public bool? FakeApp { get; init; }

		/// <summary>
		/// Creates the spy implementation used for wrapped actions and store methods.
		/// </summary>
		[Description("@#createSpy")]
		public PiniaTestingSpyFactory? CreateSpy { get; init; }
	}

	/// <summary>
	/// Typed testing options accepted by <c>createTestingPinia()</c> when the caller
	/// wants <c>createSpy</c> authoring to preserve one concrete delegate shape.
	/// This keeps the same runtime <c>TestingOptions</c> object contract and only
	/// strengthens the compile-time type of the <c>createSpy</c> callback.
	/// </summary>
	/// <typeparam name="TDelegate">The concrete delegate shape expected by <c>createSpy</c>.</typeparam>
	public record TestingOptions<TDelegate> : TestingOptions
		where TDelegate : Delegate
	{
		/// <summary>
		/// Creates the spy implementation used for wrapped actions and store methods
		/// while preserving one explicit delegate shape at the C# authoring boundary.
		/// </summary>
		[Description("@#createSpy")]
		public new PiniaTestingSpyFactory<TDelegate>? CreateSpy { get; init; }
	}
}
