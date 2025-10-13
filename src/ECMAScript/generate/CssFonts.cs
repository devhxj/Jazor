namespace ECMAScript;

/// <summary>
/// CSSFontFeatureValuesRule
/// </summary>
[ECMAScript]
[Description("@#CSSFontFeatureValuesRule")]
public class CSSFontFeatureValuesRule : CSSRule
{
    /// <summary>
    /// fontFamily
    /// </summary>
    [Description("@#fontFamily")]
    public extern string FontFamily { get; set; }

    /// <summary>
    /// annotation
    /// </summary>
    [Description("@#annotation")]
    public extern CSSFontFeatureValuesMap Annotation { get; }

    /// <summary>
    /// ornaments
    /// </summary>
    [Description("@#ornaments")]
    public extern CSSFontFeatureValuesMap Ornaments { get; }

    /// <summary>
    /// stylistic
    /// </summary>
    [Description("@#stylistic")]
    public extern CSSFontFeatureValuesMap Stylistic { get; }

    /// <summary>
    /// swash
    /// </summary>
    [Description("@#swash")]
    public extern CSSFontFeatureValuesMap Swash { get; }

    /// <summary>
    /// characterVariant
    /// </summary>
    [Description("@#characterVariant")]
    public extern CSSFontFeatureValuesMap CharacterVariant { get; }

    /// <summary>
    /// styleset
    /// </summary>
    [Description("@#styleset")]
    public extern CSSFontFeatureValuesMap Styleset { get; }

    /// <summary>
    /// historicalForms
    /// </summary>
    [Description("@#historicalForms")]
    public extern CSSFontFeatureValuesMap HistoricalForms { get; }
}

/// <summary>
/// CSSFontFeatureValuesMap
/// </summary>
[ECMAScript]
[Description("@#CSSFontFeatureValuesMap")]
public class CSSFontFeatureValuesMap : IDictionary<string, uint[]>
{
    #region Dictionary
    extern uint[] IDictionary<string, uint[]>.this[string key] { get; set; }
    extern ICollection<string> IDictionary<string, uint[]>.Keys { get; }
    extern ICollection<uint[]> IDictionary<string, uint[]>.Values { get; }
    extern int ICollection<KeyValuePair<string, uint[]>>.Count { get; }
    extern bool ICollection<KeyValuePair<string, uint[]>>.IsReadOnly { get; }
    extern void IDictionary<string, uint[]>.Add(string key, uint[] value);
    extern void ICollection<KeyValuePair<string, uint[]>>.Add(KeyValuePair<string, uint[]> item);
    extern void ICollection<KeyValuePair<string, uint[]>>.Clear();
    extern bool ICollection<KeyValuePair<string, uint[]>>.Contains(KeyValuePair<string, uint[]> item);
    extern bool IDictionary<string, uint[]>.ContainsKey(string key);
    extern void ICollection<KeyValuePair<string, uint[]>>.CopyTo(KeyValuePair<string, uint[]>[] array, int arrayIndex);
    extern IEnumerator<KeyValuePair<string, uint[]>> IEnumerable<KeyValuePair<string, uint[]>>.GetEnumerator();
    extern bool IDictionary<string, uint[]>.Remove(string key);
    extern bool ICollection<KeyValuePair<string, uint[]>>.Remove(KeyValuePair<string, uint[]> item);
    extern bool IDictionary<string, uint[]>.TryGetValue(string key, [MaybeNullWhen(false)] out uint[] value);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion

    /// <summary>
    /// set
    /// </summary>
    /// <param name="featureValueName">featureValueName</param>
    /// <param name="values">values</param>
    [Description("@#set")]
    public extern void Set(string featureValueName, Either<uint, uint[]> values);

    /// <summary>
    /// set
    /// </summary>
    /// <param name="featureValueName">featureValueName para</param>
    /// <param name="values">values</param>
    [Description("@#set")]
    public extern void Set(string featureValueName, uint values);

    /// <summary>
    /// set
    /// </summary>
    /// <param name="featureValueName">featureValueName para</param>
    /// <param name="values">values</param>
    [Description("@#set")]
    public extern void Set(string featureValueName, uint[] values);
}

/// <summary>
/// CSSFontPaletteValuesRule
/// </summary>
[ECMAScript]
[Description("@#CSSFontPaletteValuesRule")]
public class CSSFontPaletteValuesRule : CSSRule
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// fontFamily
    /// </summary>
    [Description("@#fontFamily")]
    public extern string FontFamily { get; }

    /// <summary>
    /// basePalette
    /// </summary>
    [Description("@#basePalette")]
    public extern string BasePalette { get; }

    /// <summary>
    /// overrideColors
    /// </summary>
    [Description("@#overrideColors")]
    public extern string OverrideColors { get; }
}