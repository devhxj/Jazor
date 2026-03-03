namespace Jazor.CLR;

[ECMAScriptModule("System/WeakReferenceModule.js")]
[Jazor(Op.Alias, "System.WeakReference", "WeakRef")]
public static class WeakReferenceModule
{
	///<summary>Initializes a new instance of the <see cref="T:System.WeakReference" /> class, referencing the specified object.</summary>
	[Jazor(Op.Inline ,"System.WeakReference.WeakReference(object)", "new WeakRef(@#{0})")]
	public extern static WeakRef _9a41b3fc95053633(object? target);

	///<summary>Initializes a new instance of the <see cref="T:System.WeakReference" /> class, referencing the specified object and using the specified resurrection tracking.</summary>
	[Jazor(Op.Discard ,"System.WeakReference.WeakReference(object, bool)")]
	public extern static WeakRef _bb3cf7219c9626be(object? target, bool trackResurrection);

	///<summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with all the data needed to serialize the current <see cref="T:System.WeakReference" /> object.</summary>
	[Jazor(Op.Discard ,"virtual System.WeakReference.GetObjectData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)")]
	public extern static void _5b37dc51e15031e2(WeakRef instance, object info, object context);

	[Jazor(Op.Discard ,"virtual System.WeakReference.TrackResurrection.get")]
	public extern static bool _a2251c9f4ed1f026(WeakRef instance);

	[Jazor(Op.Inline ,"virtual System.WeakReference.IsAlive.get", "(@#{0}.deref() !== undefined)")]
	public extern static bool _c3d16f7de644412a(WeakRef instance);

	[Jazor(Op.Inline ,"virtual System.WeakReference.Target.get", "@#{0}.deref()")]
	public extern static object? _ba77d80a1e80efa6(WeakRef instance);

	[Jazor(Op.Discard ,"virtual System.WeakReference.Target.set")]
	public extern static void _6576d2b2ae762786(WeakRef instance, object? value);
}
