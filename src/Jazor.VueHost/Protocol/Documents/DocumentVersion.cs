namespace Jazor.VueHost.Protocol.Documents;

public readonly record struct DocumentVersion(string Value)
{
    public override string ToString() => Value;

    public static DocumentVersion Create(int version)
        => new(version.ToString(System.Globalization.CultureInfo.InvariantCulture));
}
