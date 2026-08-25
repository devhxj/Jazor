namespace ECMAScript.VueDataUi;

/// <summary>
/// 所有 dataset/config 图表组件的 Razor 参数基类。继承参数让 catalog component 只声明 import identity，
/// 同时让 Razor Source Generator 继续负责 required parameter/type diagnostics。
/// </summary>
/// <typeparam name="TDataset">该图表在 upstream contract 中的 dataset 形状。</typeparam>
/// <typeparam name="TConfig">该图表的 config 形状。</typeparam>
public abstract class VueDataUiChartComponent<TDataset, TConfig> : ComponentBase, ECMAScript.Vue.IVueComponent
    where TConfig : VueDataUiConfig
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
public abstract class VueDataUiRequiredConfigChartComponent<TDataset, TConfig> : ComponentBase, ECMAScript.Vue.IVueComponent
    where TConfig : VueDataUiConfig
{
    [Parameter]
    [EditorRequired]
    [ECMAScriptName("dataset")]
    public TDataset Dataset { get; set; } = default!;

    [Parameter]
    [EditorRequired]
    [ECMAScriptName("config")]
    public TConfig Config { get; set; } = default!;
}

/// <summary>只有 config 的 vue-data-ui visual component 参数基类。</summary>
/// <typeparam name="TConfig">组件 config 形状。</typeparam>
public abstract class VueDataUiConfigComponent<TConfig> : ComponentBase, ECMAScript.Vue.IVueComponent
    where TConfig : VueDataUiConfig
{
    [Parameter]
    [ECMAScriptName("config")]
    public TConfig? Config { get; set; }
}

/// <summary>
/// 仅接受 dataset 的 visual component 参数基类。Digits、Gizmo 一类组件没有独立 config 时，
/// 仍通过这个基类保留 Razor required-parameter contract。
/// </summary>
/// <typeparam name="TDataset">该组件的输入数据形状。</typeparam>
public abstract class VueDataUiDatasetComponent<TDataset> : ComponentBase, ECMAScript.Vue.IVueComponent
{
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
public abstract class VueDataUiOptionalDatasetChartComponent<TDataset, TConfig> : ComponentBase, ECMAScript.Vue.IVueComponent
    where TDataset : class
    where TConfig : VueDataUiConfig
{
    [Parameter]
    [ECMAScriptName("dataset")]
    public TDataset? Dataset { get; set; }

    [Parameter]
    [ECMAScriptName("config")]
    public TConfig? Config { get; set; }
}
