using System;
using System.ComponentModel;
using ECMAScript;

namespace Jero.Web.App.FormDesign;

[ECMAScript("/FormDesign/designer-actions.mjs")]
public static class FormDesignerSdk
{
    [Description("@#loadDesigner")]
    public static extern IPromise<FormDesignerRuntime> Load();
}

[ECMAScript]
public sealed class FormDesignerRuntime
{
    [Description("@#mount")]
    public extern Action Mount();
}
