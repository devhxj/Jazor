using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class RazorVueMetadataReferenceCacheTests
{
    [TestMethod]
    public void Create_ReusesCachedPortableExecutableReferencesForKnownMarkerAssemblies()
    {
        var first = RazorVueMetadataReferences.Create();
        var second = RazorVueMetadataReferences.Create();

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

    [TestMethod]
    public void Create_CanonicalizesExtraPortableExecutableReferencesByFilePath()
    {
        var extraPath = typeof(RazorVueMetadataReferenceCacheTests).Assembly.Location;
        var first = RazorVueMetadataReferences.Create(MetadataReference.CreateFromFile(extraPath));
        var second = RazorVueMetadataReferences.Create(MetadataReference.CreateFromFile(extraPath));

        var expectedPath = Path.GetFullPath(extraPath);
        var firstReference = first
            .OfType<PortableExecutableReference>()
            .Single(static reference => string.Equals(
                Path.GetFullPath(reference.FilePath!),
                Path.GetFullPath(typeof(RazorVueMetadataReferenceCacheTests).Assembly.Location),
                StringComparison.OrdinalIgnoreCase));
        var secondReference = second
            .OfType<PortableExecutableReference>()
            .Single(static reference => string.Equals(
                Path.GetFullPath(reference.FilePath!),
                Path.GetFullPath(typeof(RazorVueMetadataReferenceCacheTests).Assembly.Location),
                StringComparison.OrdinalIgnoreCase));

        Assert.AreEqual(expectedPath, Path.GetFullPath(firstReference.FilePath!));
        Assert.AreSame(firstReference, secondReference);
    }
}
