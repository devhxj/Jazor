namespace Jazor.Emit;

/// <summary>
/// Compiler-owned RazorVue HMR metadata materialized beside a module entry.
/// Emit preserves this record verbatim; classification remains in the compiler/host contract.
/// </summary>
internal sealed record HmrMetadata(
    string ComponentId,
    string ModuleId,
    string DescriptorHash,
    string TemplateHash,
    string LogicHash,
    string BoundaryKind)
{
    public static HmrMetadata Create(
        string componentId,
        string moduleId,
        string descriptorHash,
        string templateHash,
        string logicHash,
        string boundaryKind)
    {
        RequireValue(componentId, nameof(componentId));
        RequireValue(moduleId, nameof(moduleId));
        RequireValue(descriptorHash, nameof(descriptorHash));
        RequireValue(templateHash, nameof(templateHash));
        RequireValue(logicHash, nameof(logicHash));
        RequireValue(boundaryKind, nameof(boundaryKind));
        if (!IsKnownBoundaryKind(boundaryKind))
            throw new InvalidOperationException($"Unsupported RazorVue HMR boundary kind '{boundaryKind}'.");

        return new HmrMetadata(
            componentId,
            moduleId,
            descriptorHash,
            templateHash,
            logicHash,
            boundaryKind);
    }

    public static bool IsKnownBoundaryKind(string value)
        => value is "unknown" or "template-only" or "logic-safe" or "full-reload-required";

    private static void RequireValue(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"RazorVue HMR metadata field '{parameterName}' is required.");
    }
}
