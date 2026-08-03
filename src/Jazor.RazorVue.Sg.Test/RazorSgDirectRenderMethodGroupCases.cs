namespace Jazor.RazorVue.Sg.Test;

internal static partial class DirectRenderCaseCatalog
{
    private static void AddRenderFragmentMethodGroupCase(List<DirectRenderCase> cases)
    {
        const string marker = "method-group-header";
        Add(
            cases,
            "render_fragment_method_group_component_slot",
            "builder.OpenComponent<MatrixChild>(0); builder.AddComponentParameter(1, \"Header\", (RenderFragment)RenderHeader); builder.CloseComponent();",
            marker,
            "header",
            usesFragment: false,
            usesStaticVNode: false,
            group: DirectRenderCaseGroup.Advanced,
            members:
            """
            private void RenderHeader(RenderTreeBuilder builder)
            {
                builder.OpenElement(0, "strong");
                builder.AddAttribute(1, "data-render-source", "method-group");
                builder.AddContent(2, "method-group-header");
                builder.CloseElement();
            }
            """,
            importCount: 1);
    }
}
