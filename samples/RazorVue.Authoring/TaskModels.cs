namespace RazorVue.Authoring;

public sealed record TaskDraft
{
    public string Title { get; set; } = string.Empty;

    public string Owner { get; set; } = string.Empty;
}

public sealed record TaskRow(int Id, string Title, string Owner, string Status);
