using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue;

internal sealed record RazorVueCompilationSymbols(
    INamedTypeSymbol ECMAScriptModuleAttribute,
    INamedTypeSymbol JazorComponentMarker,
    INamedTypeSymbol VueComponentMarker,
    INamedTypeSymbol ComponentBase,
    INamedTypeSymbol? RouteAttribute,
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
    INamedTypeSymbol? VuePropAttribute,
    INamedTypeSymbol? VueLibraryEmitAttribute,
    INamedTypeSymbol? VueSlotAttribute,
    INamedTypeSymbol? VueLibraryComponentFlagsAttribute,
    INamedTypeSymbol? IVueContainerComponent,
    INamedTypeSymbol? IVueContainerImplementation,
    INamedTypeSymbol? VueInjectAttribute,
    INamedTypeSymbol? EditorRequiredAttribute,
    INamedTypeSymbol? CascadingParameterAttribute)
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

        var routeAttribute = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.RouteAttribute");
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
        var vuePropAttribute = compilation.GetTypeByMetadataName("ECMAScript.VueContract.VuePropAttribute");
        var vueLibraryEmitAttribute = compilation.GetTypeByMetadataName("ECMAScript.VueContract.VueLibraryEmitAttribute");
        var vueSlotAttribute = compilation.GetTypeByMetadataName("ECMAScript.VueContract.VueSlotAttribute");
        var vueLibraryComponentFlagsAttribute = compilation.GetTypeByMetadataName("ECMAScript.VueContract.VueLibraryComponentFlagsAttribute");
        var iVueContainerComponent = compilation.GetTypeByMetadataName("ECMAScript.VueContract.IVueContainerComponent");
        var iVueContainerImplementation = compilation.GetTypeByMetadataName("ECMAScript.VueContract.IVueContainerImplementation`1");
        var vueInjectAttribute = compilation.GetTypeByMetadataName("ECMAScript.VueContract.VueInjectAttribute");
        var editorRequiredAttribute = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.EditorRequiredAttribute");
        var cascadingParameterAttribute = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.CascadingParameterAttribute");

        return new RazorVueCompilationSymbols(
            ecmaScriptModuleAttribute,
            jazorComponent,
            vueComponent,
            componentBase,
            routeAttribute,
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
            vuePropAttribute,
            vueLibraryEmitAttribute,
            vueSlotAttribute,
            vueLibraryComponentFlagsAttribute,
            iVueContainerComponent,
            iVueContainerImplementation,
            vueInjectAttribute,
            editorRequiredAttribute,
            cascadingParameterAttribute);
    }

}
