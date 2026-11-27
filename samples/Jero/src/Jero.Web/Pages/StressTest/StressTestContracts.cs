namespace Jero.Web.Pages.StressTest;

public sealed record StressBrowseGroup(string Name, string Summary, StressBrowseNode[] Nodes);

public sealed record StressBrowseNode(string Id, string Method, string Path, string Summary, int GroupIndex, int NodeIndex);

public sealed record StressPairView(string Key, string Value);
