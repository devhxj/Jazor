using System.Collections;
using static ECMAScript.CLRModule;
using static ECMAScript.Jazor;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.WeakReference")]
public static class WeakReferenceModule
{
    ///<summary>Initializes a new instance of the <see cref="T:System.WeakReference" /> class, referencing the specified object.</summary>
    ///<param name="target">The object to track or <see langword="null" />.</param>    [WhiteList("System.WeakReference.WeakReference(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static WeakRef WeakReferenceNew(Object? target);

    ///<summary>Initializes a new instance of the <see cref="T:System.WeakReference" /> class, referencing the specified object and using the specified resurrection tracking.</summary>
    ///<param name="target">An object to track.</param>
    ///<param name="trackResurrection">Indicates when to stop tracking the object. If <see langword="true" />, the object is tracked after finalization; if <see langword="false" />, the object is only tracked until finalization.</param>    [WhiteList("System.WeakReference.WeakReference(object, bool)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static WeakRef WeakReferenceNew2(Object? target, bool trackResurrection);

    ///<summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with all the data needed to serialize the current <see cref="T:System.WeakReference" /> object.</summary>
    ///<param name="info">An object that holds all the data needed to serialize or deserialize the current <see cref="T:System.WeakReference" /> object.</param>
    ///<param name="context">(Reserved) The location where serialized data is stored and retrieved.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="info" /> is <see langword="null" />.</exception>    [WhiteList("virtual System.WeakReference.GetObjectData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void WeakReferenceGetObjectData(WeakRef instance, System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context);

    [WhiteList("virtual System.WeakReference.TrackResurrection.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool WeakReferenceGetTrackResurrection(WeakRef instance);

    [WhiteList("virtual System.WeakReference.IsAlive.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool WeakReferenceGetIsAlive(WeakRef instance);

    [WhiteList("virtual System.WeakReference.Target.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Object? WeakReferenceGetTarget(WeakRef instance);

    [WhiteList("virtual System.WeakReference.Target.set")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void WeakReferenceSetTarget(WeakRef instance, Object? value);
}
