namespace Jero.Web.App.Plugin;

public sealed record PluginTableRow(string id, string name, string assemblyName, int status, int orderNo, string remark);
