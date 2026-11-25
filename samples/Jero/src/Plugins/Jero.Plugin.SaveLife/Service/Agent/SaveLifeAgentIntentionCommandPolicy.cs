using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Jero.Plugin.SaveLife.Service;

public sealed record SaveLifeAgentIntentionCommandPayload(
    long PatientProfileId,
    long PatientContextVersion,
    long ProjectId,
    long StateRevision,
    string Intention,
    string? ContactName = null,
    string? ContactPhone = null);

public sealed record SaveLifeAgentPayloadHash(string Algorithm, string Value);

public static class SaveLifeAgentIntentionCommandPolicy
{
    public const string CommandType = "intention";
    public const string PayloadHashAlgorithm = "sha256-v1";
    private static readonly HashSet<string> SupportedChoices =
    [
        "明确有意向",
        "犹豫考虑",
        "暂不参与"
    ];

    public static string NormalizeChoice(string? value)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || !SupportedChoices.Contains(normalized))
            throw Oops.Oh("参与意向选择无效，请重新选择").StatusCode(400);

        return normalized;
    }

    public static SaveLifeAgentPayloadHash ComputePayloadHash(
        SaveLifeAgentIntentionCommandPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        var canonical = string.Join('\n',
            payload.PatientProfileId.ToString(CultureInfo.InvariantCulture),
            payload.PatientContextVersion.ToString(CultureInfo.InvariantCulture),
            payload.ProjectId.ToString(CultureInfo.InvariantCulture),
            payload.StateRevision.ToString(CultureInfo.InvariantCulture),
            payload.Intention.Normalize(NormalizationForm.FormC),
            CanonicalOptional(payload.ContactName),
            CanonicalOptional(payload.ContactPhone));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return new SaveLifeAgentPayloadHash(PayloadHashAlgorithm, Convert.ToHexString(hash));
    }

    private static string CanonicalOptional(string? value)
    {
        var normalized = value?.Trim().Normalize(NormalizationForm.FormC);
        return string.IsNullOrWhiteSpace(normalized)
            ? "null"
            : $"value:{normalized.Length.ToString(CultureInfo.InvariantCulture)}:{normalized}";
    }
}
