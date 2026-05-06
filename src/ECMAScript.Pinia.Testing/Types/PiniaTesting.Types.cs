using System;
using System.ComponentModel;

namespace ECMAScript;

public static partial class PiniaTesting
{
	/// <summary>
	/// Base state-seeding object used by <c>@pinia/testing</c> <c>initialState</c>.
	/// Keys are store ids and values are object-form patch payloads merged after
	/// each store instance is created.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public abstract record TestingInitialState : Vue3.VueProps;

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
		public bool? StubActions { get; init; }

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
}
