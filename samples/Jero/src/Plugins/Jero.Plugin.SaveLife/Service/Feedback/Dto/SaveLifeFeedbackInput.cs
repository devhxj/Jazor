namespace Jero.Plugin.SaveLife.Service;

public class SaveLifeFeedbackInput : BasePageInput
{
    public string? ExternalUserId { get; set; }

    public string? FeedbackType { get; set; }

    public string? Status { get; set; }
}

public class SubmitSaveLifeClientDiagnosticInput
{
    public string? Route { get; set; }

    public int DurationMs { get; set; }

    public int StatusCode { get; set; }

    public string? Version { get; set; }

    public string? Environment { get; set; }

    public string? NetworkType { get; set; }
}
