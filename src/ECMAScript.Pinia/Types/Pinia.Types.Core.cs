using System;
using System.ComponentModel;

namespace ECMAScript;

public static partial class Pinia
{
	/// <summary>
	/// Base record for Pinia store-definition option bags.
	/// </summary>
	public abstract record DefineStoreOptionsBase : Vue3.VueProps;

	/// <summary>
	/// Option-style store definition bag.
	/// </summary>
	/// <typeparam name="TState">The typed state record returned by the store's <c>state()</c> factory.</typeparam>
	public record DefineStoreOptions<TState> : DefineStoreOptionsBase
		where TState : PiniaStateTree
	{
		/// <summary>
		/// Factory that returns one fresh state object per store instance.
		/// </summary>
		[Description("@#state")]
		public Func<TState> State { get; init; } = default!;

		/// <summary>
		/// Getter declarations bag. Use a typed <see cref="Vue3.VueProps"/> record when
		/// the store exposes heterogeneous getter signatures.
		/// </summary>
		[Description("@#getters")]
		public Vue3.VueProps? Getters { get; init; }

		/// <summary>
		/// Action declarations bag. Use a typed <see cref="Vue3.VueProps"/> record when
		/// the store exposes heterogeneous action signatures.
		/// </summary>
		[Description("@#actions")]
		public Vue3.VueProps? Actions { get; init; }

		/// <summary>
		/// Optional hydration hook for SSR/client hydration boundaries.
		/// </summary>
		[Description("@#hydrate")]
		public PiniaHydrateCallback<TState>? Hydrate { get; init; }
	}

	/// <summary>
	/// Setup-style store options bag. Pinia currently keeps this surface small; this
	/// wrapper preserves the third <c>defineStore(..., setup, options)</c> parameter
	/// shape without inventing extra C#-only semantics.
	/// </summary>
	public record DefineSetupStoreOptions : DefineStoreOptionsBase;

	/// <summary>
	/// Base record for strongly typed store-state projections.
	/// </summary>
	public abstract record PiniaStateTree : Vue3.VueProps;

	/// <summary>
	/// Unknown-like value bridge used by action-listener arguments and action results.
	/// This keeps the public surface free of raw <see cref="object"/> while still
	/// allowing typical JavaScript payload shapes.
	/// </summary>
	[ECMAScript]
	[Description("@#")]
	public sealed class PiniaValue
	{
		private PiniaValue()
		{
		}

		public extern static implicit operator PiniaValue(string value);

		public extern static implicit operator PiniaValue(bool value);

		public extern static implicit operator PiniaValue(Number value);

		public extern static implicit operator PiniaValue(BigInt value);

		public extern static implicit operator PiniaValue(char value);

		public extern static implicit operator PiniaValue(double value);

		public extern static implicit operator PiniaValue(float value);

		public extern static implicit operator PiniaValue(int value);

		public extern static implicit operator PiniaValue(long value);

		public extern static implicit operator PiniaValue(short value);

		public extern static implicit operator PiniaValue(ushort value);

		public extern static implicit operator PiniaValue(byte value);

		public extern static implicit operator PiniaValue(sbyte value);

		public extern static implicit operator PiniaValue(uint value);

		public extern static implicit operator PiniaValue(ulong value);

		public extern static implicit operator PiniaValue(decimal value);

		public extern static implicit operator PiniaValue(Error value);

		public extern static implicit operator PiniaValue(Vue3.VueProps value);

		public extern static implicit operator PiniaValue(PiniaValue[] value);
	}

	/// <summary>
	/// Mutation kind reported by Pinia subscriptions.
	/// </summary>
	public enum MutationType
	{
		/// <summary>
		/// Direct state assignment.
		/// </summary>
		[Description("@#direct")]
		Direct,

		/// <summary>
		/// <c>$patch({ ... })</c> object patch.
		/// </summary>
		[Description("@#patchObject")]
		PatchObject,

		/// <summary>
		/// <c>$patch((state) =&gt; ...)</c> function patch.
		/// </summary>
		[Description("@#patchFunction")]
		PatchFunction
	}

	/// <summary>
	/// Options for <c>$subscribe()</c>.
	/// </summary>
	public record SubscribeOptions
	{
		/// <summary>
		/// Keep the subscription alive even when no component is currently using the store.
		/// </summary>
		[Description("@#detached")]
		public bool? Detached { get; init; }

		/// <summary>
		/// Controls when the underlying watcher callback flushes relative to Vue updates.
		/// </summary>
		[Description("@#flush")]
		public Vue3.VueWatchFlush? Flush { get; init; }
	}

	/// <summary>
	/// Typed base for user-defined <c>storeToRefs()</c> projections.
	/// </summary>
	/// <typeparam name="TStore">The store contract being converted to refs.</typeparam>
	[ECMAScript]
	[Description("@#")]
	public abstract class StoreRefs<TStore> : Vue3.VueRefs<TStore>
		where TStore : class
	{
		protected StoreRefs()
		{
		}
	}
}
