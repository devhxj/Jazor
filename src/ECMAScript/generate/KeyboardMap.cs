namespace ECMAScript;

/// <summary>
/// KeyboardLayoutMap
/// </summary>
[ECMAScript]
[Description("@#KeyboardLayoutMap")]
public class KeyboardLayoutMap : IDictionary<string, string>
{
    #region Dictionary
    extern string IDictionary<string, string>.this[string key] { get; set; }
    extern ICollection<string> IDictionary<string, string>.Keys { get; }
    extern ICollection<string> IDictionary<string, string>.Values { get; }
    extern int ICollection<KeyValuePair<string, string>>.Count { get; }
    extern bool ICollection<KeyValuePair<string, string>>.IsReadOnly { get; }
    extern void IDictionary<string, string>.Add(string key, string value);
    extern void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item);
    extern void ICollection<KeyValuePair<string, string>>.Clear();
    extern bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item);
    extern bool IDictionary<string, string>.ContainsKey(string key);
    extern void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex);
    extern IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator();
    extern bool IDictionary<string, string>.Remove(string key);
    extern bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item);
    extern bool IDictionary<string, string>.TryGetValue(string key, [MaybeNullWhen(false)] out string value);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion
}