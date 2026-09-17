using System.Collections.Generic;
using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue;

namespace ECMAScript;

/// <summary>
/// Razor authoring proxy for the <c>vue-filepond</c> adapter component.
/// 上游适配器是默认导出的工厂（按插件参数动态生成 props），因此代理按默认导出绑定；
/// 常用选项与回调通过 <see cref="Options"/> 传入，其余 HTML 属性按透传处理。
/// The upstream adapter is a default-export factory whose props are generated per plugin, so the
/// proxy binds the default export; common options and callbacks travel through <see cref="Options"/>
/// and remaining HTML attributes flow through the unmatched-values parameter.
/// </summary>
[ECMAScript("vue-filepond", Transform.Component, "default")]
public sealed class VueFilePond : ComponentBase, IVueComponent
{
    /// <summary>
    /// FilePond 实例选项（服务端、多文件、回调等）。
    /// The FilePond instance options (server, multiple files, callbacks, and so on).
    /// </summary>
    [Parameter]
    public FilePondOptions? Options { get; set; }

    /// <summary>
    /// 传给 FilePond 的其他宿主属性；键使用 Vue/HTML 实际属性名。
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
