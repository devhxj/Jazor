using ECMAScript.Contract;

namespace ECMAScript.VueContract;

/// <summary>
/// RazorVue/Vue3 authoring component contract.
/// Components are expected to inherit <c>ComponentBase</c> and implement this marker.
/// </summary>
public interface IVueComponent : IUIComponent
{
}
