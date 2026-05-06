namespace ECMAScript.VueContract;

/// <summary>
/// Marker for external Vue library component stubs that participate in
/// descriptor/registry flows without being treated as ordinary user components.
/// </summary>
public interface IVueLibraryComponent : IVueComponent
{
}
