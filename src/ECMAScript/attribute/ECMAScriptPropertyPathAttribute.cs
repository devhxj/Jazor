namespace ECMAScript;

/// <summary>Emits a property selector as its JavaScript property path.</summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
public sealed class ECMAScriptPropertyPathAttribute : Attribute
{
    /// <summary>Emits the selected property path.</summary>
    public ECMAScriptPropertyPathAttribute() { }

    /// <summary>Sets the selector mode.</summary>
    /// <param name="singleKey">Selects exactly one property and emits its literal key.</param>
    public ECMAScriptPropertyPathAttribute(bool singleKey) => SingleKey = singleKey;

    /// <summary>Whether the selector emits one literal key.</summary>
    public bool SingleKey { get; }

    /// <summary>Uses vee-validate's bracket syntax for literal field paths.</summary>
    public bool VeeValidatePath { get; set; }

    /// <summary>Allows constant JavaScript array indices in a nested path.</summary>
    public bool ArrayIndices { get; set; }

    /// <summary>Whether a returned container reads the selected property indirectly.</summary>
    public bool IndirectRead { get; set; }

    /// <summary>Requires a structural record or array payload that preserves its data shape under Vue's deep proxy reads.</summary>
    public bool RequirePlainRecord { get; set; }

    /// <summary>Requires a non-null structural record as the root of a Vue unwrapped view.</summary>
    public bool RequireRecordView { get; set; }
}
