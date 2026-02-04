using ECMAScript.Common;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.WeakReference", WhiteListOp.Allowed, null,"System/WeakReferenceModule.js")]
public static class WeakReferenceModule
{
	///<summary>Initializes a new instance of the <see cref="T:System.WeakReference" /> class, referencing the specified object.</summary>
	[WhiteList("System.WeakReference.WeakReference(object)", WhiteListOp.Discard)]
	public extern static WeakRef _9a41b3fc95053633(Object? target);

	///<summary>Initializes a new instance of the <see cref="T:System.WeakReference" /> class, referencing the specified object and using the specified resurrection tracking.</summary>
	[WhiteList("System.WeakReference.WeakReference(object, bool)", WhiteListOp.Discard)]
	public extern static WeakRef _bb3cf7219c9626be(Object? target, object trackResurrection);

	///<summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with all the data needed to serialize the current <see cref="T:System.WeakReference" /> object.</summary>
	[WhiteList("virtual System.WeakReference.GetObjectData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)", WhiteListOp.Discard)]
	public extern static void _5b37dc51e15031e2(WeakRef instance, object info, object context);

	[WhiteList("virtual System.WeakReference.TrackResurrection.get", WhiteListOp.Discard)]
	public extern static bool _a2251c9f4ed1f026(WeakRef instance);

	[WhiteList("virtual System.WeakReference.IsAlive.get", WhiteListOp.Discard)]
	public extern static bool _c3d16f7de644412a(WeakRef instance);

	[WhiteList("virtual System.WeakReference.Target.get", WhiteListOp.Discard)]
	public extern static Object? _ba77d80a1e80efa6(WeakRef instance);

	[WhiteList("virtual System.WeakReference.Target.set", WhiteListOp.Discard)]
	public extern static void _6576d2b2ae762786(WeakRef instance, Object? value);
}
