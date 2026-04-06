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
    INamedTypeSymbol? RenderFragmentOfT)
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
            renderFragmentOfT);
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
