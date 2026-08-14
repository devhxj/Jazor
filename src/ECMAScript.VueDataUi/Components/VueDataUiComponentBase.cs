namespace ECMAScript.VueDataUi;

/// <summary>
/// 所有 dataset/config 图表组件的 Razor 参数基类。继承参数让 catalog component 只声明 import identity，
/// 同时让 Razor Source Generator 继续负责 required parameter/type diagnostics。
/// </summary>
/// <typeparam name="TDataset">该图表在 upstream contract 中的 dataset 形状。</typeparam>
/// <typeparam name="TConfig">该图表的 config 形状。</typeparam>
public abstract class VueDataUiChartComponent<TDataset, TConfig> : ComponentBase
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
public abstract class VueDataUiRequiredConfigChartComponent<TDataset, TConfig> : ComponentBase
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
public abstract class VueDataUiConfigComponent<TConfig> : ComponentBase
    where TConfig : VueDataUiConfig
{
    [Parameter]
    [ECMAScriptName("config")]
    public TConfig? Config { get; set; }
}
