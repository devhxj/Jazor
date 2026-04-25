namespace Jazor.Compiler;

internal enum LoweringSiteKind
{
    CreationTemp,
    ConditionalAccessInput,
    MethodReferenceProxy,
    ReferenceTemp,
    SwitchExpressionInput,
    SwitchPatternInput,
    MultiCatchParameter,
    SyntheticCatchParameter,
    TupleProjectionSource,
    TupleDeconstructionSource,
    TupleFieldCache,
    TupleNestedArgument,
    DeconstructResult,
    TupleBinaryOperandCache
}

internal readonly record struct LoweringSite(LoweringSiteKind Kind, string Slot = "")
{
    public string Tag
        => Kind switch
        {
            LoweringSiteKind.CreationTemp => "creation",
            LoweringSiteKind.ConditionalAccessInput => "cacc",
            LoweringSiteKind.MethodReferenceProxy => "mref",
            LoweringSiteKind.ReferenceTemp => "ref",
            LoweringSiteKind.SwitchExpressionInput => "swexpr",
            LoweringSiteKind.SwitchPatternInput => "swpat",
            LoweringSiteKind.MultiCatchParameter => "mcatch",
            LoweringSiteKind.SyntheticCatchParameter => "scatch",
            LoweringSiteKind.TupleProjectionSource => "tproj",
            LoweringSiteKind.TupleDeconstructionSource => "tdecon",
            LoweringSiteKind.TupleFieldCache => "tfield",
            LoweringSiteKind.TupleNestedArgument => "tnest",
            LoweringSiteKind.DeconstructResult => "decon",
            LoweringSiteKind.TupleBinaryOperandCache => "tbin",
            _ => "temp"
        };

    public static LoweringSite CreationTemp()
        => new(LoweringSiteKind.CreationTemp);

    public static LoweringSite ConditionalAccessInput()
        => new(LoweringSiteKind.ConditionalAccessInput);

    public static LoweringSite MethodReferenceProxy()
        => new(LoweringSiteKind.MethodReferenceProxy);

    public static LoweringSite ReferenceTemp()
        => new(LoweringSiteKind.ReferenceTemp);

    public static LoweringSite SwitchExpressionInput()
        => new(LoweringSiteKind.SwitchExpressionInput);

    public static LoweringSite SwitchPatternInput()
        => new(LoweringSiteKind.SwitchPatternInput);

    public static LoweringSite MultiCatchParameter()
        => new(LoweringSiteKind.MultiCatchParameter);

    public static LoweringSite SyntheticCatchParameter()
        => new(LoweringSiteKind.SyntheticCatchParameter);

    public static LoweringSite TupleProjectionSource()
        => new(LoweringSiteKind.TupleProjectionSource);

    public static LoweringSite TupleDeconstructionSource()
        => new(LoweringSiteKind.TupleDeconstructionSource);

    public static LoweringSite TupleFieldCache(string slot)
        => new(LoweringSiteKind.TupleFieldCache, slot);

    public static LoweringSite TupleFieldCache(int index)
        => TupleFieldCache(index.ToString(System.Globalization.CultureInfo.InvariantCulture));

    public static LoweringSite TupleNestedArgument(string slot)
        => new(LoweringSiteKind.TupleNestedArgument, slot);

    public static LoweringSite TupleNestedArgument(int index)
        => TupleNestedArgument(index.ToString(System.Globalization.CultureInfo.InvariantCulture));

    public static LoweringSite DeconstructResult()
        => new(LoweringSiteKind.DeconstructResult);

    public static LoweringSite TupleBinaryOperandCache()
        => new(LoweringSiteKind.TupleBinaryOperandCache);
}
