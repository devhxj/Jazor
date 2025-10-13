namespace ECMAScript;

/// <summary>
/// CSSFontFaceDescriptors
/// </summary>
[ECMAScript]
[Description("@#CSSFontFaceDescriptors")]
public class CSSFontFaceDescriptors : CSSStyleDeclaration
{
    /// <summary>
    /// src
    /// </summary>
    [Description("@#src")]
    public extern string Src { get; set; }

    /// <summary>
    /// font-family
    /// </summary>
    [Description("@#font-family")]
    public extern string Font_Family { get; set; }

    /// <summary>
    /// font-style
    /// </summary>
    [Description("@#font-style")]
    public extern string Font_Style { get; set; }

    /// <summary>
    /// font-weight
    /// </summary>
    [Description("@#font-weight")]
    public extern string Font_Weight { get; set; }

    /// <summary>
    /// font-stretch
    /// </summary>
    [Description("@#font-stretch")]
    public extern string Font_Stretch { get; set; }

    /// <summary>
    /// font-width
    /// </summary>
    [Description("@#font-width")]
    public extern string Font_Width { get; set; }

    /// <summary>
    /// font-size
    /// </summary>
    [Description("@#font-size")]
    public extern string Font_Size { get; set; }

    /// <summary>
    /// sizeAdjust
    /// </summary>
    [Description("@#sizeAdjust")]
    public extern string SizeAdjust { get; set; }

    /// <summary>
    /// size-adjust
    /// </summary>
    [Description("@#size-adjust")]
    public extern string Size_Adjust { get; set; }

    /// <summary>
    /// unicodeRange
    /// </summary>
    [Description("@#unicodeRange")]
    public extern string UnicodeRange { get; set; }

    /// <summary>
    /// unicode-range
    /// </summary>
    [Description("@#unicode-range")]
    public extern string Unicode_Range { get; set; }

    /// <summary>
    /// font-feature-settings
    /// </summary>
    [Description("@#font-feature-settings")]
    public extern string Font_Feature_Settings { get; set; }

    /// <summary>
    /// font-variation-settings
    /// </summary>
    [Description("@#font-variation-settings")]
    public extern string Font_Variation_Settings { get; set; }

    /// <summary>
    /// fontNamedInstance
    /// </summary>
    [Description("@#fontNamedInstance")]
    public extern string FontNamedInstance { get; set; }

    /// <summary>
    /// font-named-instance
    /// </summary>
    [Description("@#font-named-instance")]
    public extern string Font_Named_Instance { get; set; }

    /// <summary>
    /// fontDisplay
    /// </summary>
    [Description("@#fontDisplay")]
    public extern string FontDisplay { get; set; }

    /// <summary>
    /// font-display
    /// </summary>
    [Description("@#font-display")]
    public extern string Font_Display { get; set; }

    /// <summary>
    /// font-language-override
    /// </summary>
    [Description("@#font-language-override")]
    public extern string Font_Language_Override { get; set; }

    /// <summary>
    /// ascentOverride
    /// </summary>
    [Description("@#ascentOverride")]
    public extern string AscentOverride { get; set; }

    /// <summary>
    /// ascent-override
    /// </summary>
    [Description("@#ascent-override")]
    public extern string Ascent_Override { get; set; }

    /// <summary>
    /// descentOverride
    /// </summary>
    [Description("@#descentOverride")]
    public extern string DescentOverride { get; set; }

    /// <summary>
    /// descent-override
    /// </summary>
    [Description("@#descent-override")]
    public extern string Descent_Override { get; set; }

    /// <summary>
    /// lineGapOverride
    /// </summary>
    [Description("@#lineGapOverride")]
    public extern string LineGapOverride { get; set; }

    /// <summary>
    /// line-gap-override
    /// </summary>
    [Description("@#line-gap-override")]
    public extern string Line_Gap_Override { get; set; }

    /// <summary>
    /// superscriptPositionOverride
    /// </summary>
    [Description("@#superscriptPositionOverride")]
    public extern string SuperscriptPositionOverride { get; set; }

    /// <summary>
    /// superscript-position-override
    /// </summary>
    [Description("@#superscript-position-override")]
    public extern string Superscript_Position_Override { get; set; }

    /// <summary>
    /// subscriptPositionOverride
    /// </summary>
    [Description("@#subscriptPositionOverride")]
    public extern string SubscriptPositionOverride { get; set; }

    /// <summary>
    /// subscript-position-override
    /// </summary>
    [Description("@#subscript-position-override")]
    public extern string Subscript_Position_Override { get; set; }

    /// <summary>
    /// superscriptSizeOverride
    /// </summary>
    [Description("@#superscriptSizeOverride")]
    public extern string SuperscriptSizeOverride { get; set; }

    /// <summary>
    /// superscript-size-override
    /// </summary>
    [Description("@#superscript-size-override")]
    public extern string Superscript_Size_Override { get; set; }

    /// <summary>
    /// subscriptSizeOverride
    /// </summary>
    [Description("@#subscriptSizeOverride")]
    public extern string SubscriptSizeOverride { get; set; }

    /// <summary>
    /// subscript-size-override
    /// </summary>
    [Description("@#subscript-size-override")]
    public extern string Subscript_Size_Override { get; set; }
}

/// <summary>
/// CSSFontFaceRule
/// </summary>
[ECMAScript]
[Description("@#CSSFontFaceRule")]
public class CSSFontFaceRule : CSSRule
{
    /// <summary>
    /// style
    /// </summary>
    [Description("@#style")]
    public extern CSSFontFaceDescriptors Style { get; }
}