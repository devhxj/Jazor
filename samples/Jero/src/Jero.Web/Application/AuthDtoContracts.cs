namespace Jero.Web.Application;

public sealed record AuthTenantOption
{
    public required string Value { get; init; }

    public required string Label { get; init; }

    public string Host { get; init; } = string.Empty;
}

public sealed record AuthRegistrationWayOption
{
    public required string Value { get; init; }

    public required string Label { get; init; }
}
