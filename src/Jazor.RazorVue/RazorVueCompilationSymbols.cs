using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue;

internal sealed record RazorVueCompilationSymbols(
    INamedTypeSymbol ECMAScriptModuleAttribute,
    INamedTypeSymbol JazorComponentMarker,
    INamedTypeSymbol VueComponentMarker,
    INamedTypeSymbol ComponentBase,
    INamedTypeSymbol? ParameterAttribute,
    INamedTypeSymbol? ParameterView,
    INamedTypeSymbol? EventCallback,
    INamedTypeSymbol? EventCallbackOfT,
    INamedTypeSymbol? RenderFragment,
    INamedTypeSymbol? RenderFragmentOfT,
    INamedTypeSymbol? VueLibraryComponent,
    INamedTypeSymbol? IVueLibraryComponent,
    INamedTypeSymbol? VueLibraryComponentAttribute,
    INamedTypeSymbol? VueLibraryStyleAttribute,
    INamedTypeSymbol? VueLibraryPluginRequirementAttribute,
    INamedTypeSymbol? VueLibraryPropAttribute,
    INamedTypeSymbol? VueLibraryEmitAttribute,
    INamedTypeSymbol? VueLibrarySlotAttribute,
    INamedTypeSymbol? VueLibraryComponentFlagsAttribute)
{
    public static RazorVueCompilationSymbols? TryCreate(Compilation compilation)
    {
        var ecmaScriptModuleAttribute = compilation.GetTypeByMetadataName("ECMAScript.ECMAScriptModuleAttribute");
        // 生产 authoring 面保持低 TFM：组件边界只由接口标记表达，不再依赖 VueComponent/JazorComponent 基类。
        var jazorComponent = compilation.GetTypeByMetadataName("ECMAScript.Contract.IUIComponent");
        var vueComponent = compilation.GetTypeByMetadataName("ECMAScript.Vue3+IVueComponent");
        var componentBase = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.ComponentBase");

        if (ecmaScriptModuleAttribute is null ||
            jazorComponent is null ||
            vueComponent is null ||
            componentBase is null)
        {
            return null;
        }

        var parameterAttribute = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.ParameterAttribute");
        var parameterView = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.ParameterView");
        var eventCallback = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.EventCallback");
        var eventCallbackOfT = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.EventCallback`1");
        var renderFragment = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.RenderFragment");
        var renderFragmentOfT = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.RenderFragment`1");
        var vueLibraryComponent = compilation.GetTypeByMetadataName("Jazor.RazorVue.VueLibraryComponent");
        var iVueLibraryComponent = compilation.GetTypeByMetadataName("ECMAScript.Vue3+IVueLibraryComponent");
        var vueLibraryComponentAttribute = compilation.GetTypeByMetadataName("ECMAScript.VueContract.VueLibraryComponentAttribute");
        var vueLibraryStyleAttribute = compilation.GetTypeByMetadataName("ECMAScript.VueContract.VueLibraryStyleAttribute");
        var vueLibraryPluginRequirementAttribute = compilation.GetTypeByMetadataName("ECMAScript.VueContract.VueLibraryPluginRequirementAttribute");
        var vueLibraryPropAttribute = compilation.GetTypeByMetadataName("ECMAScript.VueContract.VueLibraryPropAttribute");
        var vueLibraryEmitAttribute = compilation.GetTypeByMetadataName("ECMAScript.VueContract.VueLibraryEmitAttribute");
        var vueLibrarySlotAttribute = compilation.GetTypeByMetadataName("ECMAScript.VueContract.VueLibrarySlotAttribute");
        var vueLibraryComponentFlagsAttribute = compilation.GetTypeByMetadataName("ECMAScript.VueContract.VueLibraryComponentFlagsAttribute");

        return new RazorVueCompilationSymbols(
            ecmaScriptModuleAttribute,
            jazorComponent,
            vueComponent,
            componentBase,
            parameterAttribute,
            parameterView,
            eventCallback,
            eventCallbackOfT,
            renderFragment,
            renderFragmentOfT,
            vueLibraryComponent,
            iVueLibraryComponent,
            vueLibraryComponentAttribute,
            vueLibraryStyleAttribute,
            vueLibraryPluginRequirementAttribute,
            vueLibraryPropAttribute,
            vueLibraryEmitAttribute,
            vueLibrarySlotAttribute,
            vueLibraryComponentFlagsAttribute);
    }

}
