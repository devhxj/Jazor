namespace ECMAScript.VueContract;

/// <summary>
/// Marks a concrete component as a compile-time implementation for a container
/// contract component.
/// </summary>
/// <typeparam name="TContainer">
/// The authored container contract component type implemented by this
/// component.
/// </typeparam>
public interface IVueContainerImplementation<TContainer>
    where TContainer : class, IVueContainerComponent
{
}
