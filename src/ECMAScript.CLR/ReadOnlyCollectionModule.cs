using System.Collections;
using static ECMAScript.CLRModule;
using static ECMAScript.Jazor;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.Collections.ObjectModel.ReadOnlyCollection")]
public static class ReadOnlyCollectionModule
{
    [WhiteList("static System.Collections.ObjectModel.ReadOnlyCollection.CreateCollection<T>(params System.ReadOnlySpan<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Array<T> ReadOnlyCollectionCreateCollection<T>(params System.ReadOnlySpan<T> values);

    [WhiteList("static System.Collections.ObjectModel.ReadOnlyCollection.CreateSet<T>(params System.ReadOnlySpan<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Collections.ObjectModel.ReadOnlySet<T> ReadOnlyCollectionCreateSet<T>(params System.ReadOnlySpan<T> values);
}
