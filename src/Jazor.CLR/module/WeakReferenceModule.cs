namespace Jazor.CLR;

/// <summary>
/// 将 System.WeakReference 的有限支持面映射到 JavaScript WeakRef。
/// </summary>
	/// <remarks>
	/// JavaScript WeakRef 的可观察 API 与 CLR resurrection tracking 不完全相同；因此仅开放
	/// <c>trackResurrection == false</c> 的构造路径，并在请求 resurrection tracking 时明确失败。
	/// </remarks>
[ECMAScriptModule("System/WeakReferenceModule.js")]
[Jazor(Op.Alias, "System.WeakReference", "WeakRef")]
public static class WeakReferenceModule
{
	// WeakRef itself cannot be retargeted. Keep replacement references out-of-band so the CLR
	// carrier remains a native WeakRef while Target.set has the expected observable behavior.
	private static readonly WeakMap<WeakRef, WeakRef> ReplacementReferences = new();
	private static readonly WeakMap<WeakRef, object?> StrongTargets = new();

	private static bool CanUseWeakReference(object? value)
	{
		if (value == null)
			return false;

		var type = TypeOf(value);
		return type == "object" || type == "function";
	}

	private static WeakRef Create(object? target)
	{
		if (CanUseWeakReference(target))
			return new WeakRef(target!);

		// JavaScript WeakRef cannot reference primitives or null, while CLR permits any boxed
		// object and null. A side-table preserves those non-weakable values without changing the
		// public carrier; object targets still use native weak reachability.
		var instance = new WeakRef(new Error());
		StrongTargets.Set(instance, target);
		return instance;
	}

	private static object? GetTarget(WeakRef instance)
	{
		if (instance == null)
			throw new Error("NullReferenceException: instance is null.");

		if (StrongTargets.Has(instance))
			return StrongTargets.Get(instance);

		var reference = ReplacementReferences.Has(instance)
			? ReplacementReferences.Get(instance)!
			: instance;
		return reference.Deref() ?? null;
	}

	private static void SetTarget(WeakRef instance, object? value)
	{
		if (instance == null)
			throw new Error("NullReferenceException: instance is null.");

		if (CanUseWeakReference(value))
		{
			StrongTargets.Delete(instance);
			ReplacementReferences.Set(instance, new WeakRef(value!));
			return;
		}

		ReplacementReferences.Delete(instance);
		StrongTargets.Set(instance, value);
	}

	///<summary>Initializes a new instance of the <see cref="T:System.WeakReference" /> class, referencing the specified object.</summary>
	[Jazor(Op.Import ,"System.WeakReference.WeakReference(object)")]
	public static WeakRef _9a41b3fc95053633(object? target)
		=> Create(target);

	///<summary>Initializes a new instance of the <see cref="T:System.WeakReference" /> class, referencing the specified object and using the specified resurrection tracking.</summary>
	[Jazor(Op.Import ,"System.WeakReference.WeakReference(object, bool)")]
	public static WeakRef _bb3cf7219c9626be(object? target, bool trackResurrection)
	{
		if (trackResurrection)
			throw new Error("NotSupportedException: WeakReference resurrection tracking is not available in the JavaScript runtime.");

		return Create(target);
	}

	///<summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with all the data needed to serialize the current <see cref="T:System.WeakReference" /> object.</summary>
	[Jazor(Op.Discard ,"virtual System.WeakReference.GetObjectData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)")]
	public extern static void _5b37dc51e15031e2(WeakRef instance, object info, object context);

	[Jazor(Op.Inline ,"virtual System.WeakReference.TrackResurrection.get", "false")]
	public extern static bool _a2251c9f4ed1f026(WeakRef instance);

	[Jazor(Op.Import ,"virtual System.WeakReference.IsAlive.get")]
	public static bool _c3d16f7de644412a(WeakRef instance)
		=> GetTarget(instance) != null;

	[Jazor(Op.Import ,"virtual System.WeakReference.Target.get")]
	public static object? _ba77d80a1e80efa6(WeakRef instance)
		=> GetTarget(instance);

	[Jazor(Op.Import ,"virtual System.WeakReference.Target.set")]
	public static void _6576d2b2ae762786(WeakRef instance, object? value)
		=> SetTarget(instance, value);
}
