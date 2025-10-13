namespace ECMAScript;

/// <summary>
/// FontFaceDescriptors
/// </summary>
[ECMAScript]
[Description("@#FontFaceDescriptors")]
public record FontFaceDescriptors(
    [property: Description("@#style")]string? Style = default,
	[property: Description("@#weight")]string? Weight = default,
	[property: Description("@#stretch")]string? Stretch = default,
	[property: Description("@#unicodeRange")]string? UnicodeRange = default,
	[property: Description("@#featureSettings")]string? FeatureSettings = default,
	[property: Description("@#variationSettings")]string? VariationSettings = default,
	[property: Description("@#display")]string? Display = default,
	[property: Description("@#ascentOverride")]string? AscentOverride = default,
	[property: Description("@#descentOverride")]string? DescentOverride = default,
	[property: Description("@#lineGapOverride")]string? LineGapOverride = default);

/// <summary>
/// FontFaceSetLoadEventInit
/// </summary>
[ECMAScript]
[Description("@#FontFaceSetLoadEventInit")]
public record FontFaceSetLoadEventInit(
    [property: Description("@#fontfaces")]FontFace[]? Fontfaces = default): EventInit;

/// <summary>
/// FontFace
/// </summary>
[ECMAScript]
[Description("@#FontFace")]
public partial class FontFace
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="family">family</param>
    /// <param name="source">source</param>
    /// <param name="descriptors">descriptors</param>
    public extern FontFace(string family, Either<string, IBufferSource> source, FontFaceDescriptors descriptors);

    /// <summary>
    /// family
    /// </summary>
    [Description("@#family")]
    public extern string Family { get; set; }

    /// <summary>
    /// style
    /// </summary>
    [Description("@#style")]
    public extern string Style { get; set; }

    /// <summary>
    /// weight
    /// </summary>
    [Description("@#weight")]
    public extern string Weight { get; set; }

    /// <summary>
    /// stretch
    /// </summary>
    [Description("@#stretch")]
    public extern string Stretch { get; set; }

    /// <summary>
    /// unicodeRange
    /// </summary>
    [Description("@#unicodeRange")]
    public extern string UnicodeRange { get; set; }

    /// <summary>
    /// featureSettings
    /// </summary>
    [Description("@#featureSettings")]
    public extern string FeatureSettings { get; set; }

    /// <summary>
    /// variationSettings
    /// </summary>
    [Description("@#variationSettings")]
    public extern string VariationSettings { get; set; }

    /// <summary>
    /// display
    /// </summary>
    [Description("@#display")]
    public extern string Display { get; set; }

    /// <summary>
    /// ascentOverride
    /// </summary>
    [Description("@#ascentOverride")]
    public extern string AscentOverride { get; set; }

    /// <summary>
    /// descentOverride
    /// </summary>
    [Description("@#descentOverride")]
    public extern string DescentOverride { get; set; }

    /// <summary>
    /// lineGapOverride
    /// </summary>
    [Description("@#lineGapOverride")]
    public extern string LineGapOverride { get; set; }

    /// <summary>
    /// status
    /// </summary>
    [Description("@#status")]
    public extern FontFaceLoadStatus Status { get; }

    /// <summary>
    /// load
    /// </summary>
    [Description("@#load")]
    public extern PromiseResult<FontFace> Load();

    /// <summary>
    /// loaded
    /// </summary>
    [Description("@#loaded")]
    public extern PromiseResult<FontFace> Loaded { get; }

    /// <summary>
    /// features
    /// </summary>
    [Description("@#features")]
    public extern FontFaceFeatures Features { get; }

    /// <summary>
    /// variations
    /// </summary>
    [Description("@#variations")]
    public extern FontFaceVariations Variations { get; }

    /// <summary>
    /// palettes
    /// </summary>
    [Description("@#palettes")]
    public extern FontFacePalettes Palettes { get; }
}

/// <summary>
/// FontFaceFeatures
/// </summary>
[ECMAScript]
[Description("@#FontFaceFeatures")]
public class FontFaceFeatures
{

}

/// <summary>
/// FontFaceVariationAxis
/// </summary>
[ECMAScript]
[Description("@#FontFaceVariationAxis")]
public class FontFaceVariationAxis
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// axisTag
    /// </summary>
    [Description("@#axisTag")]
    public extern string AxisTag { get; }

    /// <summary>
    /// minimumValue
    /// </summary>
    [Description("@#minimumValue")]
    public extern double MinimumValue { get; }

    /// <summary>
    /// maximumValue
    /// </summary>
    [Description("@#maximumValue")]
    public extern double MaximumValue { get; }

    /// <summary>
    /// defaultValue
    /// </summary>
    [Description("@#defaultValue")]
    public extern double DefaultValue { get; }
}

/// <summary>
/// FontFaceVariations
/// </summary>
[ECMAScript]
[Description("@#FontFaceVariations")]
public class FontFaceVariations : ISet<FontFaceVariationAxis>
{
    #region Set
    extern int ICollection<FontFaceVariationAxis>.Count { get; }
    extern bool ICollection<FontFaceVariationAxis>.IsReadOnly { get; }
    extern bool ISet<FontFaceVariationAxis>.Add(FontFaceVariationAxis item);
    extern void ICollection<FontFaceVariationAxis>.Clear();
    extern bool ICollection<FontFaceVariationAxis>.Contains(FontFaceVariationAxis item);
    extern void ICollection<FontFaceVariationAxis>.CopyTo(FontFaceVariationAxis[] array, int arrayIndex);
    extern void ISet<FontFaceVariationAxis>.ExceptWith(IEnumerable<FontFaceVariationAxis> other);
    extern IEnumerator<FontFaceVariationAxis> IEnumerable<FontFaceVariationAxis>.GetEnumerator();
    extern void ISet<FontFaceVariationAxis>.IntersectWith(IEnumerable<FontFaceVariationAxis> other);
    extern bool ISet<FontFaceVariationAxis>.IsProperSubsetOf(IEnumerable<FontFaceVariationAxis> other);
    extern bool ISet<FontFaceVariationAxis>.IsProperSupersetOf(IEnumerable<FontFaceVariationAxis> other);
    extern bool ISet<FontFaceVariationAxis>.IsSubsetOf(IEnumerable<FontFaceVariationAxis> other);
    extern bool ISet<FontFaceVariationAxis>.IsSupersetOf(IEnumerable<FontFaceVariationAxis> other);
    extern bool ISet<FontFaceVariationAxis>.Overlaps(IEnumerable<FontFaceVariationAxis> other);
    extern bool ICollection<FontFaceVariationAxis>.Remove(FontFaceVariationAxis item);
    extern bool ISet<FontFaceVariationAxis>.SetEquals(IEnumerable<FontFaceVariationAxis> other);
    extern void ISet<FontFaceVariationAxis>.SymmetricExceptWith(IEnumerable<FontFaceVariationAxis> other);
    extern void ISet<FontFaceVariationAxis>.UnionWith(IEnumerable<FontFaceVariationAxis> other);
    extern void ICollection<FontFaceVariationAxis>.Add(FontFaceVariationAxis item);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion
}

/// <summary>
/// FontFacePalette
/// </summary>
[ECMAScript]
[Description("@#FontFacePalette")]
public class FontFacePalette : IEnumerable<string>
{
    extern IEnumerator<string> IEnumerable<string>.GetEnumerator();
    extern IEnumerator IEnumerable.GetEnumerator();

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    [Description("@#")]
    public extern string this[uint index] { get; }

    /// <summary>
    /// usableWithLightBackground
    /// </summary>
    [Description("@#usableWithLightBackground")]
    public extern bool UsableWithLightBackground { get; }

    /// <summary>
    /// usableWithDarkBackground
    /// </summary>
    [Description("@#usableWithDarkBackground")]
    public extern bool UsableWithDarkBackground { get; }
}

/// <summary>
/// FontFacePalettes
/// </summary>
[ECMAScript]
[Description("@#FontFacePalettes")]
public class FontFacePalettes : IEnumerable<FontFacePalette>
{
    extern IEnumerator<FontFacePalette> IEnumerable<FontFacePalette>.GetEnumerator();
    extern IEnumerator IEnumerable.GetEnumerator();

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    [Description("@#")]
    public extern FontFacePalette this[uint index] { get; }
}

/// <summary>
/// FontFaceSetLoadEvent
/// </summary>
[ECMAScript]
[Description("@#FontFaceSetLoadEvent")]
public class FontFaceSetLoadEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern FontFaceSetLoadEvent(string type, FontFaceSetLoadEventInit eventInitDict);

    /// <summary>
    /// fontfaces
    /// </summary>
    [Description("@#fontfaces")]
    public extern FrozenSet<FontFace> Fontfaces { get; }
}

/// <summary>
/// FontFaceSet
/// </summary>
[ECMAScript]
[Description("@#FontFaceSet")]
public class FontFaceSet : EventTarget, ISet<FontFace>
{
    #region Set
    extern int ICollection<FontFace>.Count { get; }
    extern bool ICollection<FontFace>.IsReadOnly { get; }
    extern bool ISet<FontFace>.Add(FontFace item);
    extern void ICollection<FontFace>.Clear();
    extern bool ICollection<FontFace>.Contains(FontFace item);
    extern void ICollection<FontFace>.CopyTo(FontFace[] array, int arrayIndex);
    extern void ISet<FontFace>.ExceptWith(IEnumerable<FontFace> other);
    extern IEnumerator<FontFace> IEnumerable<FontFace>.GetEnumerator();
    extern void ISet<FontFace>.IntersectWith(IEnumerable<FontFace> other);
    extern bool ISet<FontFace>.IsProperSubsetOf(IEnumerable<FontFace> other);
    extern bool ISet<FontFace>.IsProperSupersetOf(IEnumerable<FontFace> other);
    extern bool ISet<FontFace>.IsSubsetOf(IEnumerable<FontFace> other);
    extern bool ISet<FontFace>.IsSupersetOf(IEnumerable<FontFace> other);
    extern bool ISet<FontFace>.Overlaps(IEnumerable<FontFace> other);
    extern bool ICollection<FontFace>.Remove(FontFace item);
    extern bool ISet<FontFace>.SetEquals(IEnumerable<FontFace> other);
    extern void ISet<FontFace>.SymmetricExceptWith(IEnumerable<FontFace> other);
    extern void ISet<FontFace>.UnionWith(IEnumerable<FontFace> other);
    extern void ICollection<FontFace>.Add(FontFace item);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion

    /// <summary>
    /// add
    /// </summary>
    /// <param name="font">font</param>
    [Description("@#add")]
    public extern FontFaceSet Add(FontFace font);

    /// <summary>
    /// delete
    /// </summary>
    /// <param name="font">font</param>
    [Description("@#delete")]
    public extern bool Delete(FontFace font);

    /// <summary>
    /// clear
    /// </summary>
    [Description("@#clear")]
    public extern void Clear();

    /// <summary>
    /// onloading
    /// </summary>
    [Description("@#onloading")]
    public extern EventHandler Onloading { get; set; }

    /// <summary>
    /// onloadingdone
    /// </summary>
    [Description("@#onloadingdone")]
    public extern EventHandler Onloadingdone { get; set; }

    /// <summary>
    /// onloadingerror
    /// </summary>
    [Description("@#onloadingerror")]
    public extern EventHandler Onloadingerror { get; set; }

    /// <summary>
    /// load
    /// </summary>
    /// <param name="font">font</param>
    /// <param name="text">text</param>
    [Description("@#load")]
    public extern PromiseResult<FontFace[]> Load(string font, string? text = default);

    /// <summary>
    /// check
    /// </summary>
    /// <param name="font">font</param>
    /// <param name="text">text</param>
    [Description("@#check")]
    public extern bool Check(string font, string? text = default);

    /// <summary>
    /// ready
    /// </summary>
    [Description("@#ready")]
    public extern PromiseResult<FontFaceSet> Ready { get; }

    /// <summary>
    /// status
    /// </summary>
    [Description("@#status")]
    public extern FontFaceSetLoadStatus Status { get; }
}