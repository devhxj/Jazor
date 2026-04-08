namespace Jazor.VueContracts.Protocol;

public static class VueHostRpcMethodNames
{
    public const string Ping = "vuehost/ping";
    public const string GetHostInfo = "vuehost/getHostInfo";
    public const string OpenDocument = "vuehost/openDocument";
    public const string UpdateDocument = "vuehost/updateDocument";
    public const string CloseDocument = "vuehost/closeDocument";
    public const string GetOpenDocuments = "vuehost/getOpenDocuments";
    public const string GetFrontendContext = "vuehost/getFrontendContext";
    public const string AnalyzeJazor = "vuehost/analyzeJazor";
    public const string GetVirtualArtifact = "vuehost/getVirtualArtifact";
    public const string GetHotUpdatePlan = "vuehost/getHotUpdatePlan";
}
