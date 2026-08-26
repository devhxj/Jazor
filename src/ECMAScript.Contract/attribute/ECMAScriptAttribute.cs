namespace ECMAScript;

/// <summary>
/// Describes how a declaration is supplied to the ECMAScript host.
/// 描述声明如何由 ECMAScript 宿主提供。
/// </summary>
public enum Transform
{
    /// <summary>Ambient or compiler-allowed host contract without an import.</summary>
    Allow,

    /// <summary>Ordinary external ESM binding.</summary>
    Import,

    /// <summary>External component ESM binding.</summary>
    Component
}

/// <summary>
/// Marks a declaration as an ECMAScript host contract that Jazor can validate and lower.
/// 标记声明为可由 Jazor 校验和 lowering 的 ECMAScript 宿主契约。
/// </summary>
/// <remarks>
/// The parameterless form is an ambient <see cref="Transform.Allow"/> declaration. The
/// one-string form preserves the existing ordinary import contract. Component declarations
/// may provide an optional export name; an omitted name means the module default export.
/// </remarks>
[AttributeUsage(AttributeTargets.All, Inherited = false)]
public class ECMAScriptAttribute : Attribute
{
    /// <summary>Gets the ESM import specifier, or <see langword="null"/> for <see cref="Transform.Allow"/>.</summary>
    public string? Import { get; }

    /// <summary>Gets the intended lowering category.</summary>
    public Transform Transform { get; }

    /// <summary>Gets the component export name, or <see langword="null"/> for a default export.</summary>
    public virtual string? ExportName { get; }

    /// <summary>Creates an ambient <see cref="Transform.Allow"/> declaration.</summary>
    public ECMAScriptAttribute()
    {
        Transform = Transform.Allow;
    }

    /// <summary>Creates an ordinary external <see cref="Transform.Import"/> declaration.</summary>
    /// <param name="import">The ESM import specifier preserved in generated JavaScript.</param>
    public ECMAScriptAttribute(string import)
        : this(import, Transform.Import)
    {
    }

    /// <summary>Creates a declaration with an explicit transform category.</summary>
    /// <param name="import">The ESM import specifier.</param>
    /// <param name="transform">The intended host transform.</param>
    public ECMAScriptAttribute(string import, Transform transform)
        : this(import, transform, null)
    {
    }

    /// <summary>Creates a declaration with an explicit category and optional component export.</summary>
    /// <param name="import">The ESM import specifier.</param>
    /// <param name="transform">The intended host transform.</param>
    /// <param name="exportName">The component named export; <see langword="null"/> means default.</param>
    public ECMAScriptAttribute(string import, Transform transform, string? exportName)
    {
        Validate(import, transform, exportName);
        Import = import;
        Transform = transform;
        ExportName = exportName;
    }

    private static void Validate(string? import, Transform transform, string? exportName)
    {
        if (transform is not (Transform.Allow or Transform.Import or Transform.Component))
            throw new ArgumentOutOfRangeException(nameof(transform), transform, "Unknown ECMAScript transform.");

        if (transform == Transform.Allow)
        {
            if (!string.IsNullOrWhiteSpace(import) || exportName is not null)
                throw new ArgumentException("Allow declarations cannot specify an import or export name.");
            return;
        }

        if (string.IsNullOrWhiteSpace(import))
            throw new ArgumentException("Import and Component declarations require an import specifier.", nameof(import));

        if (transform == Transform.Import && exportName is not null)
            throw new ArgumentException("Import declarations cannot specify a component export name.", nameof(exportName));
    }
}
