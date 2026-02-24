namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlyCollection","System/Collections/ObjectModel/ReadOnlyCollectionModule.js")]
public static class ReadOnlyCollectionModule
{
	[Jazor(Op.Discard ,"static System.Collections.ObjectModel.ReadOnlyCollection.CreateCollection<T>(params System.ReadOnlySpan<T>)")]
	public extern static Array<T> _a0cccd63a3a3eee1<T>( object values);

	[Jazor(Op.Discard ,"static System.Collections.ObjectModel.ReadOnlyCollection.CreateSet<T>(params System.ReadOnlySpan<T>)")]
	public extern static System.Collections.ObjectModel.ReadOnlySet<T> _b80678a096dde585<T>( object values);
}
