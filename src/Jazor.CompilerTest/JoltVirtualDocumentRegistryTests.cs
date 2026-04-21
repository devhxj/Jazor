using Jolt.VirtualDocuments.Mapping;
using Jolt.VirtualDocuments.Models;
using Jolt.VirtualDocuments.Registry;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JoltVirtualDocumentRegistryTests
{
    [TestMethod]
    public async Task VirtualDocumentRegistry_UpsertAsync_ReplacesStaleProjectedEntries()
    {
        const string sourcePath = "/workspace/Counter.jazor";
        const string projectedVuePath = "virtual:/workspace/Counter.jazor.g.vue";
        const string projectedCSharpPath = "virtual:/workspace/Counter.jazor.g.cs";
        var registry = new InMemoryVirtualDocumentRegistry();

        await registry.UpsertAsync(
        [
            CreateVirtualDocument(sourcePath, projectedVuePath, VirtualDocumentKind.Vue),
            CreateVirtualDocument(sourcePath, projectedCSharpPath, VirtualDocumentKind.CSharp)
        ],
            CancellationToken.None);

        await registry.UpsertAsync(
        [
            CreateVirtualDocument(sourcePath, projectedVuePath, VirtualDocumentKind.Vue, version: "2")
        ],
            CancellationToken.None);

        var bySource = await registry.GetBySourceDocumentAsync(sourcePath, CancellationToken.None);
        var removedDocument = await registry.GetByProjectedDocumentAsync(projectedCSharpPath, CancellationToken.None);

        Assert.AreEqual(1, bySource.Count);
        Assert.AreEqual(projectedVuePath, bySource[0].Identity.ProjectedDocumentPath);
        Assert.IsNull(removedDocument);
    }

    [TestMethod]
    public async Task VirtualDocumentRegistry_RemoveBySourceDocumentAsync_RemovesProjectedEntries()
    {
        const string sourcePath = "/workspace/Counter.jazor";
        const string projectedVuePath = "virtual:/workspace/Counter.jazor.g.vue";
        var registry = new InMemoryVirtualDocumentRegistry();

        await registry.UpsertAsync(
        [
            CreateVirtualDocument(sourcePath, projectedVuePath, VirtualDocumentKind.Vue)
        ],
            CancellationToken.None);

        await registry.RemoveBySourceDocumentAsync(sourcePath, CancellationToken.None);

        var bySource = await registry.GetBySourceDocumentAsync(sourcePath, CancellationToken.None);
        var byProjected = await registry.GetByProjectedDocumentAsync(projectedVuePath, CancellationToken.None);

        Assert.AreEqual(0, bySource.Count);
        Assert.IsNull(byProjected);
    }

    private static VirtualDocument CreateVirtualDocument(
        string sourcePath,
        string projectedPath,
        VirtualDocumentKind documentKind,
        string version = "1")
        => new(
            new VirtualDocumentIdentity(sourcePath, projectedPath, documentKind),
            $"// {documentKind}",
            ProjectionMap.CreateWholeDocument(sourcePath, projectedPath, sourceLength: 8, projectedLength: 8),
            version);
}
