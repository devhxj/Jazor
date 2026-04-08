namespace ECMAScript.CSS;

/// <summary>
/// CSS
/// </summary>
[ECMAScript]
[Description("@#CSS")]
public static partial class CSS
{
    /// <summary>
    /// animationWorklet
    /// </summary>
    [Description("@#animationWorklet")]
    public static extern Worklet AnimationWorklet { get; }

    /// <summary>
    /// supports
    /// </summary>
    /// <param name="property">property</param>
    /// <param name="value">value</param>
    [Description("@#supports")]
    public static extern bool Supports(string property, string value);

    /// <summary>
    /// supports
    /// </summary>
    /// <param name="conditionText">conditionText</param>
    [Description("@#supports")]
    public static extern bool Supports(string conditionText);

    /// <summary>
    /// highlights
    /// </summary>
    [Description("@#highlights")]
    public static extern HighlightRegistry Highlights { get; }

    /// <summary>
    /// elementSources
    /// </summary>
    [Description("@#elementSources")]
    public static extern object ElementSources { get; }

    /// <summary>
    /// layoutWorklet
    /// </summary>
    [Description("@#layoutWorklet")]
    public static extern Worklet LayoutWorklet { get; }

    /// <summary>
    /// paintWorklet
    /// </summary>
    [Description("@#paintWorklet")]
    public static extern Worklet PaintWorklet { get; }

    /// <summary>
    /// parseStylesheet
    /// </summary>
    /// <param name="css">css</param>
    /// <param name="options">options</param>
    [Description("@#parseStylesheet")]
    public static extern PromiseResult<CSSParserRule[]> ParseStylesheet(CSSStringSource css, CSSParserOptions? options = default);

    /// <summary>
    /// parseRuleList
    /// </summary>
    /// <param name="css">css</param>
    /// <param name="options">options</param>
    [Description("@#parseRuleList")]
    public static extern PromiseResult<CSSParserRule[]> ParseRuleList(CSSStringSource css, CSSParserOptions? options = default);

    /// <summary>
    /// parseRule
    /// </summary>
    /// <param name="css">css</param>
    /// <param name="options">options</param>
    [Description("@#parseRule")]
    public static extern PromiseResult<CSSParserRule> ParseRule(CSSStringSource css, CSSParserOptions? options = default);

    /// <summary>
    /// parseDeclarationList
    /// </summary>
    /// <param name="css">css</param>
    /// <param name="options">options</param>
    [Description("@#parseDeclarationList")]
    public static extern PromiseResult<CSSParserRule[]> ParseDeclarationList(CSSStringSource css, CSSParserOptions? options = default);

    /// <summary>
    /// parseDeclaration
    /// </summary>
    /// <param name="css">css</param>
    /// <param name="options">options</param>
    [Description("@#parseDeclaration")]
    public static extern CSSParserDeclaration ParseDeclaration(string css, CSSParserOptions? options = default);

    /// <summary>
    /// parseValue
    /// </summary>
    /// <param name="css">css</param>
    [Description("@#parseValue")]
    public static extern CSSToken ParseValue(string css);

    /// <summary>
    /// parseValueList
    /// </summary>
    /// <param name="css">css</param>
    [Description("@#parseValueList")]
    public static extern CSSToken[] ParseValueList(string css);

    /// <summary>
    /// parseCommaValueList
    /// </summary>
    /// <param name="css">css</param>
    [Description("@#parseCommaValueList")]
    public static extern CSSToken[][] ParseCommaValueList(string css);

    /// <summary>
    /// registerProperty
    /// </summary>
    /// <param name="definition">definition</param>
    [Description("@#registerProperty")]
    public static extern void RegisterProperty(PropertyDefinition definition);

    /// <summary>
    /// number
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#number")]
    public static extern CSSUnitValue Number(double value);

    /// <summary>
    /// percent
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#percent")]
    public static extern CSSUnitValue Percent(double value);

    /// <summary>
    /// cap
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#cap")]
    public static extern CSSUnitValue Cap(double value);

    /// <summary>
    /// ch
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#ch")]
    public static extern CSSUnitValue Ch(double value);

    /// <summary>
    /// em
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#em")]
    public static extern CSSUnitValue Em(double value);

    /// <summary>
    /// ex
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#ex")]
    public static extern CSSUnitValue Ex(double value);

    /// <summary>
    /// ic
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#ic")]
    public static extern CSSUnitValue Ic(double value);

    /// <summary>
    /// lh
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#lh")]
    public static extern CSSUnitValue Lh(double value);

    /// <summary>
    /// rcap
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#rcap")]
    public static extern CSSUnitValue Rcap(double value);

    /// <summary>
    /// rch
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#rch")]
    public static extern CSSUnitValue Rch(double value);

    /// <summary>
    /// rem
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#rem")]
    public static extern CSSUnitValue Rem(double value);

    /// <summary>
    /// rex
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#rex")]
    public static extern CSSUnitValue Rex(double value);

    /// <summary>
    /// ric
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#ric")]
    public static extern CSSUnitValue Ric(double value);

    /// <summary>
    /// rlh
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#rlh")]
    public static extern CSSUnitValue Rlh(double value);

    /// <summary>
    /// vw
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#vw")]
    public static extern CSSUnitValue Vw(double value);

    /// <summary>
    /// vh
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#vh")]
    public static extern CSSUnitValue Vh(double value);

    /// <summary>
    /// vi
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#vi")]
    public static extern CSSUnitValue Vi(double value);

    /// <summary>
    /// vb
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#vb")]
    public static extern CSSUnitValue Vb(double value);

    /// <summary>
    /// vmin
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#vmin")]
    public static extern CSSUnitValue Vmin(double value);

    /// <summary>
    /// vmax
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#vmax")]
    public static extern CSSUnitValue Vmax(double value);

    /// <summary>
    /// svw
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#svw")]
    public static extern CSSUnitValue Svw(double value);

    /// <summary>
    /// svh
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#svh")]
    public static extern CSSUnitValue Svh(double value);

    /// <summary>
    /// svi
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#svi")]
    public static extern CSSUnitValue Svi(double value);

    /// <summary>
    /// svb
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#svb")]
    public static extern CSSUnitValue Svb(double value);

    /// <summary>
    /// svmin
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#svmin")]
    public static extern CSSUnitValue Svmin(double value);

    /// <summary>
    /// svmax
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#svmax")]
    public static extern CSSUnitValue Svmax(double value);

    /// <summary>
    /// lvw
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#lvw")]
    public static extern CSSUnitValue Lvw(double value);

    /// <summary>
    /// lvh
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#lvh")]
    public static extern CSSUnitValue Lvh(double value);

    /// <summary>
    /// lvi
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#lvi")]
    public static extern CSSUnitValue Lvi(double value);

    /// <summary>
    /// lvb
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#lvb")]
    public static extern CSSUnitValue Lvb(double value);

    /// <summary>
    /// lvmin
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#lvmin")]
    public static extern CSSUnitValue Lvmin(double value);

    /// <summary>
    /// lvmax
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#lvmax")]
    public static extern CSSUnitValue Lvmax(double value);

    /// <summary>
    /// dvw
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#dvw")]
    public static extern CSSUnitValue Dvw(double value);

    /// <summary>
    /// dvh
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#dvh")]
    public static extern CSSUnitValue Dvh(double value);

    /// <summary>
    /// dvi
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#dvi")]
    public static extern CSSUnitValue Dvi(double value);

    /// <summary>
    /// dvb
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#dvb")]
    public static extern CSSUnitValue Dvb(double value);

    /// <summary>
    /// dvmin
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#dvmin")]
    public static extern CSSUnitValue Dvmin(double value);

    /// <summary>
    /// dvmax
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#dvmax")]
    public static extern CSSUnitValue Dvmax(double value);

    /// <summary>
    /// cqw
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#cqw")]
    public static extern CSSUnitValue Cqw(double value);

    /// <summary>
    /// cqh
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#cqh")]
    public static extern CSSUnitValue Cqh(double value);

    /// <summary>
    /// cqi
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#cqi")]
    public static extern CSSUnitValue Cqi(double value);

    /// <summary>
    /// cqb
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#cqb")]
    public static extern CSSUnitValue Cqb(double value);

    /// <summary>
    /// cqmin
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#cqmin")]
    public static extern CSSUnitValue Cqmin(double value);

    /// <summary>
    /// cqmax
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#cqmax")]
    public static extern CSSUnitValue Cqmax(double value);

    /// <summary>
    /// cm
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#cm")]
    public static extern CSSUnitValue Cm(double value);

    /// <summary>
    /// mm
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#mm")]
    public static extern CSSUnitValue Mm(double value);

    /// <summary>
    /// Q
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#Q")]
    public static extern CSSUnitValue Q(double value);

    /// <summary>
    /// in
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#in")]
    public static extern CSSUnitValue In(double value);

    /// <summary>
    /// pt
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#pt")]
    public static extern CSSUnitValue Pt(double value);

    /// <summary>
    /// pc
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#pc")]
    public static extern CSSUnitValue Pc(double value);

    /// <summary>
    /// px
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#px")]
    public static extern CSSUnitValue Px(double value);

    /// <summary>
    /// deg
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#deg")]
    public static extern CSSUnitValue Deg(double value);

    /// <summary>
    /// grad
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#grad")]
    public static extern CSSUnitValue Grad(double value);

    /// <summary>
    /// rad
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#rad")]
    public static extern CSSUnitValue Rad(double value);

    /// <summary>
    /// turn
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#turn")]
    public static extern CSSUnitValue Turn(double value);

    /// <summary>
    /// s
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#s")]
    public static extern CSSUnitValue S(double value);

    /// <summary>
    /// ms
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#ms")]
    public static extern CSSUnitValue Ms(double value);

    /// <summary>
    /// Hz
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#Hz")]
    public static extern CSSUnitValue Hz(double value);

    /// <summary>
    /// kHz
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#kHz")]
    public static extern CSSUnitValue KHz(double value);

    /// <summary>
    /// dpi
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#dpi")]
    public static extern CSSUnitValue Dpi(double value);

    /// <summary>
    /// dpcm
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#dpcm")]
    public static extern CSSUnitValue Dpcm(double value);

    /// <summary>
    /// dppx
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#dppx")]
    public static extern CSSUnitValue Dppx(double value);

    /// <summary>
    /// fr
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#fr")]
    public static extern CSSUnitValue Fr(double value);

    /// <summary>
    /// escape
    /// </summary>
    /// <param name="ident">ident</param>
    [Description("@#escape")]
    public static extern string Escape(string ident);
}
