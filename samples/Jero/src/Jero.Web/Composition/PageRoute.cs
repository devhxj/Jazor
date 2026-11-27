using static ECMAScript.Vue;

namespace Jero.Web.Composition;

/// <summary>将页面视图、响应式状态和可选内容组件组合为路由组件。</summary>
[ECMAScriptModule("components/page-route.js")]
public static class PageRoute
{
    /// <summary>每次组件设置时创建一个页面状态，并用它生成视图属性和默认插槽。</summary>
    public static IVueComponent CreatePageRoute<TProps>(IVueComponent view, Func<IPageState<TProps>> create, IVueComponent? content = null) where TProps : VueProps
        => DefineComponent(new VueComponentOptions
        {
            Setup = () =>
            {
                // 初始化和渲染都通过 Vue 的响应式代理调用状态方法，确保页面能观察到变更。
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

/// <summary>路由组件需要的页面状态契约。</summary>
[ECMAScript]
public interface IPageState<out TProps> where TProps : VueProps
{
    /// <summary>在首次渲染前初始化页面状态。</summary>
    void Initialize();
    /// <summary>根据当前状态创建本次渲染所需的视图属性。</summary>
    TProps CreateProps();
}

/// <summary>用于向内容组件传递同一份页面状态。</summary>
public sealed record PageStateProps<TProps>(IPageState<TProps> State) : VueProps where TProps : VueProps;

public sealed record PageContentSlots : VueSlots
{
    [System.ComponentModel.Description("@#default")]
    public required Func<IVNode[]> Content { get; init; }
}
