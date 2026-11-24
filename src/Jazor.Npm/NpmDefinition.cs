namespace Jazor.Npm;

public sealed record NpmDefinition(string Path, int Start, int Length)
{
    public NpmDocumentation? Documentation { get; init; }
    [System.Text.Json.Serialization.JsonInclude]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    internal bool DocumentationFromCallableOwner { get; init; }

    public bool Equals(NpmDefinition? other) =>
        other is not null && Path == other.Path && Start == other.Start && Length == other.Length;

    public override int GetHashCode() => HashCode.Combine(Path, Start, Length);
}

public sealed record NpmDocumentation(
    string? Summary = null,
    IReadOnlyDictionary<string, string>? Parameters = null,
    IReadOnlyDictionary<string, string>? TypeParameters = null,
    string? Returns = null,
    string? Remarks = null,
    IReadOnlyList<string>? Examples = null,
    string? Deprecated = null);
