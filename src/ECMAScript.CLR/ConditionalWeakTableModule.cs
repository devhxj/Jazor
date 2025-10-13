using System.Collections;
using static ECMAScript.CLRModule;
using static ECMAScript.Jazor;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>")]
public static class ConditionalWeakTableModule
{
    ///<summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.ConditionalWeakTable`2" /> class.</summary>    [WhiteList("System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.ConditionalWeakTable()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static WeakMap<TKey,TValue> ConditionalWeakTableNew<TKey, TValue>();

    ///<summary>Gets the value of the specified key.</summary>
    ///<param name="key">The key that represents an object with an attached property.</param>
    ///<param name="value">When this method returns, contains the attached property value. If <paramref name="key" /> is not found, <paramref name="value" /> contains the default value.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.</exception>
    ///<returns>  <see langword="true" /> if <paramref name="key" /> is found; otherwise, <see langword="false" />.</returns>    [WhiteList("System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.TryGetValue(TKey, out TValue)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ConditionalWeakTableTryGetValue<TKey, TValue>(WeakMap<TKey,TValue> instance, TKey key, OutValue<TValue> value);

    ///<summary>Adds a key to the table.</summary>
    ///<param name="key">The key to add. <paramref name="key" /> represents the object to which the property is attached.</param>
    ///<param name="value">The key's property value.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="key" /> already exists.</exception>    [WhiteList("System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Add(TKey, TValue)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ConditionalWeakTableAdd<TKey, TValue>(WeakMap<TKey,TValue> instance, TKey key, TValue value);

    ///<summary>Adds a key to the table if it doesn't already exist.</summary>
    ///<param name="key">The key to add.</param>
    ///<param name="value">The key's property value.</param>
    ///<returns>  <see langword="true" /> if the key/value pair was added; <see langword="false" /> if the table already contained the key.</returns>    [WhiteList("System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.TryAdd(TKey, TValue)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ConditionalWeakTableTryAdd<TKey, TValue>(WeakMap<TKey,TValue> instance, TKey key, TValue value);

    ///<summary>Adds the key and value if the key doesn't exist, or updates the existing key's value if it does exist.</summary>
    ///<param name="key">The key to add or update. May not be <see langword="null" />.</param>
    ///<param name="value">The value to associate with <paramref name="key" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.</exception>    [WhiteList("System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.AddOrUpdate(TKey, TValue)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ConditionalWeakTableAddOrUpdate<TKey, TValue>(WeakMap<TKey,TValue> instance, TKey key, TValue value);

    ///<summary>Removes a key and its value from the table.</summary>
    ///<param name="key">The key to remove.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.</exception>
    ///<returns>  <see langword="true" /> if the key is found and removed; otherwise, <see langword="false" />.</returns>    [WhiteList("System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Remove(TKey)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ConditionalWeakTableRemove<TKey, TValue>(WeakMap<TKey,TValue> instance, TKey key);

    [WhiteList("System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Remove(TKey, out TValue)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ConditionalWeakTableRemove2<TKey, TValue>(WeakMap<TKey,TValue> instance, TKey key, OutValue<TValue> value);

    ///<summary>Clears all the key/value pairs.</summary>    [WhiteList("System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Clear()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ConditionalWeakTableClear<TKey, TValue>(WeakMap<TKey,TValue> instance);

    [WhiteList("System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetOrAdd(TKey, TValue)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static TValue ConditionalWeakTableGetOrAdd<TKey, TValue>(WeakMap<TKey,TValue> instance, TKey key, TValue value);

    [WhiteList("System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetOrAdd(TKey, System.Func<TKey, TValue>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static TValue ConditionalWeakTableGetOrAdd2<TKey, TValue>(WeakMap<TKey,TValue> instance, TKey key, System.Func<TKey, TValue> valueFactory);

    [WhiteList("System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetOrAdd<TArg>(TKey, System.Func<TKey, TArg, TValue>, TArg)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static TValue ConditionalWeakTableGetOrAdd3<TKey, TValue, TArg>(WeakMap<TKey,TValue> instance, TKey key, System.Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument);

    ///<summary>Atomically searches for a specified key in the table and returns the corresponding value. If the key does not exist in the table, the method invokes a callback method to create a value that is bound to the specified key.</summary>
    ///<param name="key">The key to search for. <paramref name="key" /> represents the object to which the property is attached.</param>
    ///<param name="createValueCallback">A delegate to a method that can create a value for the given <paramref name="key" />. It has a single parameter of type <c>TKey</c>, and returns a value of type <c>TValue</c>.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> or <paramref name="createValueCallback" /> is <see langword="null" />.</exception>
    ///<returns>The value attached to <paramref name="key" />, if <paramref name="key" /> already exists in the table; otherwise, the new value returned by the <paramref name="createValueCallback" /> delegate.</returns>    [WhiteList("System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetValue(TKey, System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.CreateValueCallback)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static TValue ConditionalWeakTableGetValue<TKey, TValue>(WeakMap<TKey,TValue> instance, TKey key, System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.CreateValueCallback createValueCallback);

    ///<summary>Atomically searches for a specified key in the table and returns the corresponding value. If the key does not exist in the table, the method invokes the parameterless constructor of the class that represents the table's value to create a value that is bound to the specified key.</summary>
    ///<param name="key">The key to search for. <paramref name="key" /> represents the object to which the property is attached.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="key" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.MissingMethodException">The class that represents the table's value does not define a parameterless constructor.Note: In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MissingMemberException" />, instead.</exception>
    ///<returns>The value that corresponds to <paramref name="key" />, if <paramref name="key" /> already exists in the table; otherwise, a new value created by the parameterless constructor of the class defined by the <paramref name="TValue" /> generic type parameter.</returns>    [WhiteList("System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetOrCreateValue(TKey)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static TValue ConditionalWeakTableGetOrCreateValue<TKey, TValue>(WeakMap<TKey,TValue> instance, TKey key);
}
