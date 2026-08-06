using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 多行文本输入框组件的编写代理。
/// Vuetify textarea authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VTextarea", StyleUrls = [VuetifyLibraryAssets.StyleUrl])]
public sealed class VTextarea : VInputComponentBase
{
    /// <summary>
    /// 自动增长。
    /// Auto-grows the textarea height.
    /// </summary>
    [Parameter]
    public bool AutoGrow { get; set; }

    /// <summary>
    /// 行数。
    /// Number of visible rows.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Rows { get; set; }

    /// <summary>
    /// 最大行数。
    /// Maximum number of visible rows.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxRows { get; set; }

    /// <summary>
    /// 禁止调整大小。
    /// Disables textarea resizing.
    /// </summary>
    [Parameter]
    public bool NoResize { get; set; }

    /// <summary>
    /// 自动聚焦。
    /// Autofocuses the textarea.
    /// </summary>
    [Parameter]
    public bool Autofocus { get; set; }

    /// <summary>
    /// 额外属性。
    /// Additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
