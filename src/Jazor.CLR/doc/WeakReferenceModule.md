# WeakReferenceModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.WeakReference.WeakReference(object)</br>
**签名**：_9a41b3fc95053633</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.WeakReference" /> class, referencing the specified object.</summary>
<param name="target">The object to track or <see langword="null" />.</param>
```

**成员**：System.WeakReference.WeakReference(object, bool)</br>
**签名**：_bb3cf7219c9626be</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.WeakReference" /> class, referencing the specified object and using the specified resurrection tracking.</summary>
<param name="target">An object to track.</param>
<param name="trackResurrection">Indicates when to stop tracking the object. If <see langword="true" />, the object is tracked after finalization; if <see langword="false" />, the object is only tracked until finalization.</param>
```

**成员**：virtual System.WeakReference.GetObjectData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)</br>
**签名**：_5b37dc51e15031e2</br>
**注释**：

```xml
<summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with all the data needed to serialize the current <see cref="T:System.WeakReference" /> object.</summary>
<param name="info">An object that holds all the data needed to serialize or deserialize the current <see cref="T:System.WeakReference" /> object.</param>
<param name="context">(Reserved) The location where serialized data is stored and retrieved.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="info" /> is <see langword="null" />.</exception>
```

**成员**：virtual System.WeakReference.TrackResurrection.get</br>
**签名**：_a2251c9f4ed1f026</br>

**成员**：virtual System.WeakReference.IsAlive.get</br>
**签名**：_c3d16f7de644412a</br>

**成员**：virtual System.WeakReference.Target.get</br>
**签名**：_ba77d80a1e80efa6</br>

**成员**：virtual System.WeakReference.Target.set</br>
**签名**：_6576d2b2ae762786</br>

