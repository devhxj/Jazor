namespace Jazor.Compiler;

/// <summary>
/// 描述临时变量被引入的 lowering 场景及其局部槽位。
/// </summary>
/// <remarks>
/// 该值不代表 C# 语法节点，而是编译器为保持求值顺序、副作用次数或回写协议而引入的
/// 合成位置。Tag 只用于生成可读且稳定的 JavaScript 临时名前缀。
/// </remarks>
internal enum LoweringSiteKind
{
    CreationTemp,
    ConditionalAccessInput,
    TryCastInput,
    LockValueTemp,
    UsingResourceTemp,
    MethodReferenceReceiver,
    PropertyMutationTemp,
    MethodReferenceProxy,
    ReferenceTemp,
    SwitchExpressionInput,
    SwitchPatternInput,
    PatternInputCache,
    MultiCatchParameter,
    SyntheticCatchParameter,
    TupleProjectionSource,
    TupleDeconstructionSource,
    TupleFieldCache,
    TupleNestedArgument,
    DeconstructResult,
    TupleBinaryOperandCache,
    BoundArgumentTemp,
    InterpolationValue
}

/// <summary>
/// 一个具体的临时变量分配位置。
/// </summary>
/// <remarks>
/// Slot 用于区分同一 lowering 场景中的多个缓存槽位；它必须是稳定的逻辑名称，不能使用
/// 当前遍历次数或集合索引等偶然信息。
/// </remarks>
internal readonly record struct LoweringSite(LoweringSiteKind Kind, string Slot = "")
{
    public string Tag
        => Kind switch
        {
            LoweringSiteKind.CreationTemp => "creation",
            LoweringSiteKind.ConditionalAccessInput => "cacc",
            LoweringSiteKind.TryCastInput => "trycast",
            LoweringSiteKind.LockValueTemp => "lock",
            LoweringSiteKind.UsingResourceTemp => "using",
            LoweringSiteKind.MethodReferenceReceiver => "mrecv",
            LoweringSiteKind.PropertyMutationTemp => "pmut",
            LoweringSiteKind.MethodReferenceProxy => "mref",
            LoweringSiteKind.ReferenceTemp => "ref",
            LoweringSiteKind.SwitchExpressionInput => "swexpr",
            LoweringSiteKind.SwitchPatternInput => "swpat",
            LoweringSiteKind.PatternInputCache => "patin",
            LoweringSiteKind.MultiCatchParameter => "mcatch",
            LoweringSiteKind.SyntheticCatchParameter => "scatch",
            LoweringSiteKind.TupleProjectionSource => "tproj",
            LoweringSiteKind.TupleDeconstructionSource => "tdecon",
            LoweringSiteKind.TupleFieldCache => "tfield",
            LoweringSiteKind.TupleNestedArgument => "tnest",
            LoweringSiteKind.DeconstructResult => "decon",
            LoweringSiteKind.TupleBinaryOperandCache => "tbin",
            LoweringSiteKind.BoundArgumentTemp => "arg",
            LoweringSiteKind.InterpolationValue => "interp",
            _ => "temp"
        };

    public static LoweringSite CreationTemp()
        => new(LoweringSiteKind.CreationTemp);

    public static LoweringSite ConditionalAccessInput()
        => new(LoweringSiteKind.ConditionalAccessInput);

    public static LoweringSite TryCastInput()
        => new(LoweringSiteKind.TryCastInput);

    public static LoweringSite LockValueTemp(string slot)
        => new(LoweringSiteKind.LockValueTemp, slot);

    public static LoweringSite UsingResourceTemp(string slot)
        => new(LoweringSiteKind.UsingResourceTemp, slot);

    public static LoweringSite MethodReferenceReceiver()
        => new(LoweringSiteKind.MethodReferenceReceiver);

    public static LoweringSite PropertyMutationTemp(string slot)
        => new(LoweringSiteKind.PropertyMutationTemp, slot);

    public static LoweringSite MethodReferenceProxy()
        => new(LoweringSiteKind.MethodReferenceProxy);

    public static LoweringSite ReferenceTemp()
        => new(LoweringSiteKind.ReferenceTemp);

    public static LoweringSite SwitchExpressionInput()
        => new(LoweringSiteKind.SwitchExpressionInput);

    public static LoweringSite SwitchPatternInput()
        => new(LoweringSiteKind.SwitchPatternInput);

    public static LoweringSite PatternInputCache(string slot)
        => new(LoweringSiteKind.PatternInputCache, slot);

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

    public static LoweringSite BoundArgumentTemp(int parameterOrdinal, int sourceIndex)
        => new(
            LoweringSiteKind.BoundArgumentTemp,
            parameterOrdinal.ToString(System.Globalization.CultureInfo.InvariantCulture) + "-" +
            sourceIndex.ToString(System.Globalization.CultureInfo.InvariantCulture));

    public static LoweringSite InterpolationValue()
        => new(LoweringSiteKind.InterpolationValue);
}
