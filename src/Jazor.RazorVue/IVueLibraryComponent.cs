namespace Jazor.RazorVue;

/// <summary>
/// Base type for external Vue library stubs that participate in the RazorVue
/// descriptor/registry pipeline without becoming normal compiled RazorVue
/// component entries.
/// </summary>
public interface IVueLibraryComponent : IVueComponent
{
}
