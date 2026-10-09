namespace ECMAScript;

/// <summary>
/// Typed extension projections for authored C# compatibility types. WebIDL-owned browser interfaces
/// remain in the generated declarations; these members only add an extra mapping to an authored shape.
/// </summary>
public static partial class Global
{
    extension(IWindow window)
    {
        /// <summary>
        /// Gets the native Location object for an authored window contract.
        /// </summary>
        [Description("@#location")]
        public extern LocationRef LiveLocation { get; }
    }
}
