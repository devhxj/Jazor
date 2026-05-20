using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorIrMetadataReferenceCacheTests
{
    [TestMethod]
    public void CreateMetadataReferences_ReusesCachedPortableExecutableReferences()
    {
        var first = RazorIrTestHost.CreateMetadataReferences();
        var second = RazorIrTestHost.CreateMetadataReferences();

        var expectedPath = Path.GetFullPath(typeof(ComponentBase).Assembly.Location);
        var firstReference = first
            .OfType<PortableExecutableReference>()
            .Single(static reference => string.Equals(
                Path.GetFullPath(reference.FilePath!),
                Path.GetFullPath(typeof(ComponentBase).Assembly.Location),
                StringComparison.OrdinalIgnoreCase));
        var secondReference = second
            .OfType<PortableExecutableReference>()
            .Single(static reference => string.Equals(
                Path.GetFullPath(reference.FilePath!),
                Path.GetFullPath(typeof(ComponentBase).Assembly.Location),
                StringComparison.OrdinalIgnoreCase));

        Assert.AreEqual(expectedPath, Path.GetFullPath(firstReference.FilePath!));
        Assert.AreSame(firstReference, secondReference);
    }
}
