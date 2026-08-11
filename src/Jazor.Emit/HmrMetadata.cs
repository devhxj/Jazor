namespace Jazor.Emit;

/// <summary>
/// Provider-owned HMR metadata materialized beside a module entry.
/// Emit validates only the transport envelope and never interprets provider payload semantics.
/// </summary>
internal sealed record HmrMetadata(
    string ProviderId,
    string ModuleId,
    string Payload)
{
    public static HmrMetadata Create(
        string providerId,
        string moduleId,
        string payload)
    {
        RequireValue(providerId, nameof(providerId));
        RequireValue(moduleId, nameof(moduleId));
        RequireValue(payload, nameof(payload));
        ValidatePayload(payload);

        return new HmrMetadata(
            providerId,
            moduleId,
            payload);
    }

    private static void RequireValue(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"HMR metadata field '{parameterName}' is required.");
    }

    private static void ValidatePayload(string payload)
    {
        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(payload);
            if (document.RootElement.ValueKind != System.Text.Json.JsonValueKind.Object)
                throw new InvalidOperationException("HMR metadata payload must be a JSON object.");
        }
        catch (System.Text.Json.JsonException ex)
        {
            throw new InvalidOperationException("HMR metadata payload must be valid JSON.", ex);
        }
    }
}
