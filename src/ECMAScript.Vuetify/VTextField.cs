using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 文本输入框组件的编写代理。
/// Vuetify text-field authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VTextField")]
public sealed class VTextField : VInputComponentBase
{
    /// <summary>
    /// 输入类型。
    /// Input type attribute.
    /// </summary>
    [Parameter]
    public string? Type { get; set; }

    /// <summary>
    /// 自动聚焦。
    /// Autofocuses the input.
    /// </summary>
    [Parameter]
    public bool Autofocus { get; set; }

    /// <summary>
    /// 反转。
    /// Reverses the input direction.
    /// </summary>
    [Parameter]
    public bool Reverse { get; set; }

    /// <summary>
    /// 额外属性。
    /// Additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
