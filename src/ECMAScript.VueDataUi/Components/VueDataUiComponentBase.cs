namespace ECMAScript.VueDataUi;

/// <summary>
/// 所有 dataset/config 图表组件的 Razor 参数基类。继承参数让 catalog component 只声明 import identity，
/// 同时让 Razor Source Generator 继续负责 required parameter/type diagnostics。
/// </summary>
/// <typeparam name="TDataset">该图表在 upstream contract 中的 dataset 形状。</typeparam>
/// <typeparam name="TConfig">该图表的 config 形状。</typeparam>
public abstract class VdChartComponent<TDataset, TConfig> : ComponentBase, ECMAScript.Vue.IVueComponent
    where TConfig : VdConfig
{
    /// <summary>图表输入数据。The chart input dataset.</summary>
    [Parameter]
    [EditorRequired]
    [ECMAScriptName("dataset")]
    public TDataset Dataset { get; set; } = default!;

    /// <summary>可选 chart configuration。Optional chart configuration.</summary>
    [Parameter]
    [ECMAScriptName("config")]
    public TConfig? Config { get; set; }
}

/// <summary>
/// 仅用于 upstream 要求同时提供 dataset/config 的图表。保留独立 base，避免把 optional config
/// 错误升级为整个 catalog 的 Razor required parameter。
/// </summary>
/// <typeparam name="TDataset">该图表在 upstream contract 中的 dataset 形状。</typeparam>
/// <typeparam name="TConfig">该图表的必填 config 形状。</typeparam>
public abstract class VdRequiredConfigChartComponent<TDataset, TConfig> : ComponentBase, ECMAScript.Vue.IVueComponent
    where TConfig : VdConfig
{
    /// <summary>
    /// 传入组件的数据；数据形状由当前图表的 Dataset 类型决定。
    /// </summary>
    [Parameter]
    [EditorRequired]
    [ECMAScriptName("dataset")]
    public TDataset Dataset { get; set; } = default!;

    /// <summary>
    /// 组件的显示和交互配置；具体选项由当前图表的强类型配置定义。
    /// </summary>
    [Parameter]
    [EditorRequired]
    [ECMAScriptName("config")]
    public TConfig Config { get; set; } = default!;
}

/// <summary>只有 config 的 vue-data-ui visual component 参数基类。</summary>
/// <typeparam name="TConfig">组件 config 形状。</typeparam>
public abstract class VdConfigComponent<TConfig> : ComponentBase, ECMAScript.Vue.IVueComponent
    where TConfig : VdConfig
{
    /// <summary>
    /// 组件的显示和交互配置；具体选项由当前图表的强类型配置定义。
    /// </summary>
    [Parameter]
    [ECMAScriptName("config")]
    public TConfig? Config { get; set; }
}

/// <summary>
/// 仅接受 dataset 的 visual component 参数基类。Digits、Gizmo 一类组件没有独立 config 时，
/// 仍通过这个基类保留 Razor required-parameter contract。
/// </summary>
/// <typeparam name="TDataset">该组件的输入数据形状。</typeparam>
public abstract class VdDatasetComponent<TDataset> : ComponentBase, ECMAScript.Vue.IVueComponent
{
    /// <summary>
    /// 传入组件的数据；数据形状由当前图表的 Dataset 类型决定。
    /// </summary>
    [Parameter]
    [EditorRequired]
    [ECMAScriptName("dataset")]
    public TDataset Dataset { get; set; } = default!;
}

/// <summary>
/// 上游允许省略 dataset 的 chart 参数基类。Only the three upstream components whose props
/// explicitly mark dataset optional use this base; other charts keep the stricter required contract.
/// </summary>
/// <typeparam name="TDataset">可选输入数据的形状。</typeparam>
/// <typeparam name="TConfig">组件配置形状。</typeparam>
public abstract class VdOptionalDatasetChartComponent<TDataset, TConfig> : ComponentBase, ECMAScript.Vue.IVueComponent
    where TDataset : class
    where TConfig : VdConfig
{
    /// <summary>
    /// 传入组件的数据；数据形状由当前图表的 Dataset 类型决定。
    /// </summary>
    [Parameter]
    [ECMAScriptName("dataset")]
    public TDataset? Dataset { get; set; }

    /// <summary>
    /// 组件的显示和交互配置；具体选项由当前图表的强类型配置定义。
    /// </summary>
    [Parameter]
    [ECMAScriptName("config")]
    public TConfig? Config { get; set; }
}