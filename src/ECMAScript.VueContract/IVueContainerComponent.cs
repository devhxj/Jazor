using ECMAScript.Contract;

namespace ECMAScript.VueContract;

/// <summary>
/// Marks a component authoring surface as a compile-time container contract.
/// The authored component remains stable, while RazorVue may resolve it to a
/// configured implementation component during compilation.
/// </summary>
public interface IVueContainerComponent : IUIComponent
{
}
