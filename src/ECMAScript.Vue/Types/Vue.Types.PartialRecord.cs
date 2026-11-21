using System;

namespace ECMAScript;

public static partial class Vue
{
	/// <summary>A JavaScript record whose declared properties may be absent.</summary>
	[ECMAScript]
	public sealed class PartialRecord<TRecord> where TRecord : VueProps
	{
		private PartialRecord() { }

		/// <summary>Reads a selected property path and returns null when any segment is absent.</summary>
		public extern TValue? Get<TValue>(Func<TRecord, TValue?> property) where TValue : class;

		/// <summary>Reads a selected property path and returns null when any segment is absent.</summary>
		public extern TValue? GetValue<TValue>(Func<TRecord, TValue> property) where TValue : struct;

		/// <summary>Reads a selected nullable property and returns null when any segment is absent.</summary>
		public extern TValue? GetValue<TValue>(Func<TRecord, TValue?> property) where TValue : struct;
	}
}
