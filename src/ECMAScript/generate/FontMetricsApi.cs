namespace ECMAScript;

/// <summary>
/// FontMetrics
/// </summary>
[ECMAScript]
[Description("@#FontMetrics")]
public class FontMetrics
{
    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern double Width { get; }

    /// <summary>
    /// advances
    /// </summary>
    [Description("@#advances")]
    public extern FrozenSet<double> Advances { get; }

    /// <summary>
    /// boundingBoxLeft
    /// </summary>
    [Description("@#boundingBoxLeft")]
    public extern double BoundingBoxLeft { get; }

    /// <summary>
    /// boundingBoxRight
    /// </summary>
    [Description("@#boundingBoxRight")]
    public extern double BoundingBoxRight { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern double Height { get; }

    /// <summary>
    /// emHeightAscent
    /// </summary>
    [Description("@#emHeightAscent")]
    public extern double EmHeightAscent { get; }

    /// <summary>
    /// emHeightDescent
    /// </summary>
    [Description("@#emHeightDescent")]
    public extern double EmHeightDescent { get; }

    /// <summary>
    /// boundingBoxAscent
    /// </summary>
    [Description("@#boundingBoxAscent")]
    public extern double BoundingBoxAscent { get; }

    /// <summary>
    /// boundingBoxDescent
    /// </summary>
    [Description("@#boundingBoxDescent")]
    public extern double BoundingBoxDescent { get; }

    /// <summary>
    /// fontBoundingBoxAscent
    /// </summary>
    [Description("@#fontBoundingBoxAscent")]
    public extern double FontBoundingBoxAscent { get; }

    /// <summary>
    /// fontBoundingBoxDescent
    /// </summary>
    [Description("@#fontBoundingBoxDescent")]
    public extern double FontBoundingBoxDescent { get; }

    /// <summary>
    /// dominantBaseline
    /// </summary>
    [Description("@#dominantBaseline")]
    public extern Baseline DominantBaseline { get; }

    /// <summary>
    /// baselines
    /// </summary>
    [Description("@#baselines")]
    public extern FrozenSet<Baseline> Baselines { get; }

    /// <summary>
    /// fonts
    /// </summary>
    [Description("@#fonts")]
    public extern FrozenSet<Font> Fonts { get; }
}

/// <summary>
/// Baseline
/// </summary>
[ECMAScript]
[Description("@#Baseline")]
public class Baseline
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern double Value { get; }
}

/// <summary>
/// Font
/// </summary>
[ECMAScript]
[Description("@#Font")]
public class Font
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// glyphsRendered
    /// </summary>
    [Description("@#glyphsRendered")]
    public extern uint GlyphsRendered { get; }
}