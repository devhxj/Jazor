using static ECMAScript.Vue;

namespace Jero.Web.App;

[ECMAScriptModule("components/page-route.js")]
public static class PageRoute
{
    public static IVueComponent CreatePageRoute<TProps>(IVueComponent view, Func<IPageState<TProps>> create, IVueComponent? content = null) where TProps : VueProps
        => DefineComponent(new VueComponentOptions
        {
            Setup = () =>
            {
                // Call state methods through Vue's proxy so their mutations update the page.
                var state = Reactive(create());
                state.Initialize();
                if (content is null)
                    return () => H(view, state.CreateProps());

                var slots = new PageContentSlots
                {
                    Content = () => [H(content, new PageStateProps<TProps>(state))]
                };
                return () => H(view, state.CreateProps(), slots);
            }
        });
}

[ECMAScript]
public interface IPageState<out TProps> where TProps : VueProps
{
    void Initialize();
    TProps CreateProps();
}

public sealed record PageStateProps<TProps>(IPageState<TProps> State) : VueProps where TProps : VueProps;

public sealed record PageContentSlots : VueSlots
{
    [System.ComponentModel.Description("@#default")]
    public required Func<IVNode[]> Content { get; init; }
}
