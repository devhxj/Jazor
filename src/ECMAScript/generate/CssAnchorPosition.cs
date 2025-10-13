namespace ECMAScript;

/// <summary>
/// CSSPositionTryRule
/// </summary>
[ECMAScript]
[Description("@#CSSPositionTryRule")]
public class CSSPositionTryRule : CSSRule
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// style
    /// </summary>
    [Description("@#style")]
    public extern CSSPositionTryDescriptors Style { get; }
}

/// <summary>
/// CSSPositionTryDescriptors
/// </summary>
[ECMAScript]
[Description("@#CSSPositionTryDescriptors")]
public class CSSPositionTryDescriptors : CSSStyleDeclaration
{
    /// <summary>
    /// margin-top
    /// </summary>
    [Description("@#margin-top")]
    public extern string Margin_Top { get; set; }

    /// <summary>
    /// margin-right
    /// </summary>
    [Description("@#margin-right")]
    public extern string Margin_Right { get; set; }

    /// <summary>
    /// margin-bottom
    /// </summary>
    [Description("@#margin-bottom")]
    public extern string Margin_Bottom { get; set; }

    /// <summary>
    /// margin-left
    /// </summary>
    [Description("@#margin-left")]
    public extern string Margin_Left { get; set; }

    /// <summary>
    /// margin-block
    /// </summary>
    [Description("@#margin-block")]
    public extern string Margin_Block { get; set; }

    /// <summary>
    /// margin-block-start
    /// </summary>
    [Description("@#margin-block-start")]
    public extern string Margin_Block_Start { get; set; }

    /// <summary>
    /// margin-block-end
    /// </summary>
    [Description("@#margin-block-end")]
    public extern string Margin_Block_End { get; set; }

    /// <summary>
    /// margin-inline
    /// </summary>
    [Description("@#margin-inline")]
    public extern string Margin_Inline { get; set; }

    /// <summary>
    /// margin-inline-start
    /// </summary>
    [Description("@#margin-inline-start")]
    public extern string Margin_Inline_Start { get; set; }

    /// <summary>
    /// margin-inline-end
    /// </summary>
    [Description("@#margin-inline-end")]
    public extern string Margin_Inline_End { get; set; }

    /// <summary>
    /// inset-block
    /// </summary>
    [Description("@#inset-block")]
    public extern string Inset_Block { get; set; }

    /// <summary>
    /// inset-block-start
    /// </summary>
    [Description("@#inset-block-start")]
    public extern string Inset_Block_Start { get; set; }

    /// <summary>
    /// inset-block-end
    /// </summary>
    [Description("@#inset-block-end")]
    public extern string Inset_Block_End { get; set; }

    /// <summary>
    /// inset-inline
    /// </summary>
    [Description("@#inset-inline")]
    public extern string Inset_Inline { get; set; }

    /// <summary>
    /// inset-inline-start
    /// </summary>
    [Description("@#inset-inline-start")]
    public extern string Inset_Inline_Start { get; set; }

    /// <summary>
    /// inset-inline-end
    /// </summary>
    [Description("@#inset-inline-end")]
    public extern string Inset_Inline_End { get; set; }

    /// <summary>
    /// min-width
    /// </summary>
    [Description("@#min-width")]
    public extern string Min_Width { get; set; }

    /// <summary>
    /// max-width
    /// </summary>
    [Description("@#max-width")]
    public extern string Max_Width { get; set; }

    /// <summary>
    /// min-height
    /// </summary>
    [Description("@#min-height")]
    public extern string Min_Height { get; set; }

    /// <summary>
    /// max-height
    /// </summary>
    [Description("@#max-height")]
    public extern string Max_Height { get; set; }

    /// <summary>
    /// block-size
    /// </summary>
    [Description("@#block-size")]
    public extern string Block_Size { get; set; }

    /// <summary>
    /// min-block-size
    /// </summary>
    [Description("@#min-block-size")]
    public extern string Min_Block_Size { get; set; }

    /// <summary>
    /// max-block-size
    /// </summary>
    [Description("@#max-block-size")]
    public extern string Max_Block_Size { get; set; }

    /// <summary>
    /// inline-size
    /// </summary>
    [Description("@#inline-size")]
    public extern string Inline_Size { get; set; }

    /// <summary>
    /// min-inline-size
    /// </summary>
    [Description("@#min-inline-size")]
    public extern string Min_Inline_Size { get; set; }

    /// <summary>
    /// max-inline-size
    /// </summary>
    [Description("@#max-inline-size")]
    public extern string Max_Inline_Size { get; set; }

    /// <summary>
    /// place-self
    /// </summary>
    [Description("@#place-self")]
    public extern string Place_Self { get; set; }

    /// <summary>
    /// align-self
    /// </summary>
    [Description("@#align-self")]
    public extern string Align_Self { get; set; }

    /// <summary>
    /// justify-self
    /// </summary>
    [Description("@#justify-self")]
    public extern string Justify_Self { get; set; }

    /// <summary>
    /// position-anchor
    /// </summary>
    [Description("@#position-anchor")]
    public extern string Position_Anchor { get; set; }

    /// <summary>
    /// position-area
    /// </summary>
    [Description("@#position-area")]
    public extern string Position_Area { get; set; }
}