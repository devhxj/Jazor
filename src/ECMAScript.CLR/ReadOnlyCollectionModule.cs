using ECMAScript.Common;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.Collections.ObjectModel.ReadOnlyCollection", WhiteListOp.Allowed, null,"System/Collections/ObjectModel/ReadOnlyCollectionModule.js")]
public static class ReadOnlyCollectionModule
{
	[WhiteList("static System.Collections.ObjectModel.ReadOnlyCollection.CreateCollection<T>(params System.ReadOnlySpan<T>)", WhiteListOp.Discard)]
	public extern static Array<T> _a0cccd63a3a3eee1<T>( object values);

	[WhiteList("static System.Collections.ObjectModel.ReadOnlyCollection.CreateSet<T>(params System.ReadOnlySpan<T>)", WhiteListOp.Discard)]
	public extern static System.Collections.ObjectModel.ReadOnlySet<T> _b80678a096dde585<T>( object values);
}
