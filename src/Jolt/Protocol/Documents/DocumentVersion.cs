namespace Jolt.Protocol.Documents;

public readonly record struct DocumentVersion
{
    public DocumentVersion(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;

    public static DocumentVersion Create(int version)
    {
        if (!TryCreate(version, out var documentVersion))
        {
            throw new ArgumentOutOfRangeException(nameof(version), version, "Document version must be non-negative.");
        }

        return documentVersion;
    }

    public static bool TryCreate(string? value, out DocumentVersion documentVersion)
    {
        documentVersion = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        documentVersion = new DocumentVersion(value);
        return true;
    }

    public static bool TryCreate(int version, out DocumentVersion documentVersion)
    {
        documentVersion = default;
        if (version < 0)
        {
            return false;
        }

        documentVersion = new DocumentVersion(version.ToString(System.Globalization.CultureInfo.InvariantCulture));
        return true;
    }
}
