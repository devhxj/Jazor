namespace Jero.Plugin.SaveLife.Service;

public class SaveLifeAgentVectorIndexInput : BasePageInput
{
    public string? ResourceType { get; set; }

    public string? SyncStatus { get; set; }
}

public class RetrySaveLifeAgentVectorIndexInput : BaseIdInput
{
}
