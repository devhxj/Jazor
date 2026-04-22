using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue;

public sealed record RazorVueCompilationSymbols(
    INamedTypeSymbol ECMAScriptModuleAttribute,
    INamedTypeSymbol JazorComponent,
    INamedTypeSymbol VueComponent,
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
        // Prefer the final public runtime libraries but keep transitional
        // fallbacks so older test inputs and intermediate branches still load.
        var jazorComponent = GetTypeByMetadataName(
            compilation,
            "Jazor.Razor.JazorComponent",
            "Jazor.Compiler.Razor.JazorComponent");
        var vueComponent = GetTypeByMetadataName(
            compilation,
            "Jazor.RazorVue.VueComponent",
            "Jazor.Compiler.RazorVue.VueComponent");
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
        var iVueLibraryComponent = compilation.GetTypeByMetadataName("Jazor.RazorVue.IVueLibraryComponent");
        var vueLibraryComponentAttribute = compilation.GetTypeByMetadataName("Jazor.RazorVue.VueLibraryComponentAttribute");
        var vueLibraryStyleAttribute = compilation.GetTypeByMetadataName("Jazor.RazorVue.VueLibraryStyleAttribute");
        var vueLibraryPluginRequirementAttribute = compilation.GetTypeByMetadataName("Jazor.RazorVue.VueLibraryPluginRequirementAttribute");
        var vueLibraryPropAttribute = compilation.GetTypeByMetadataName("Jazor.RazorVue.VueLibraryPropAttribute");
        var vueLibraryEmitAttribute = compilation.GetTypeByMetadataName("Jazor.RazorVue.VueLibraryEmitAttribute");
        var vueLibrarySlotAttribute = compilation.GetTypeByMetadataName("Jazor.RazorVue.VueLibrarySlotAttribute");
        var vueLibraryComponentFlagsAttribute = compilation.GetTypeByMetadataName("Jazor.RazorVue.VueLibraryComponentFlagsAttribute");

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

    private static INamedTypeSymbol? GetTypeByMetadataName(Compilation compilation, params string[] metadataNames)
    {
        foreach (var metadataName in metadataNames)
        {
            var symbol = compilation.GetTypeByMetadataName(metadataName);
            if (symbol is not null)
                return symbol;
        }

        return null;
    }
}
