namespace ECMAScript;

/// <summary>
/// Stops JavaScript host-name qualification at this binding container.
/// The emitted path starts with its child and omits the container and its enclosing types.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false)]
public sealed class BoundaryAttribute : Attribute;
