using System.Reflection;
using ECMAScript.Contract;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class IndexRangeModuleWhitelistTests
{
    [TestMethod]
    public void IndexMappings_UseObjectCarrierAndIndexRuntimeModule()
    {
        AssertTypeAlias(typeof(Jazor.CLR.IndexModule), "System.Index", "Object");
        AssertImports(typeof(Jazor.CLR.IndexModule), "System/IndexModule.js",
        [
            "System.Index.Index()",
            "System.Index.Index(int, bool)",
            "static System.Index.Start.get",
            "static System.Index.End.get",
            "static System.Index.FromStart(int)",
            "static System.Index.FromEnd(int)",
            "System.Index.Value.get",
            "System.Index.IsFromEnd.get",
            "System.Index.GetOffset(int)",
            "static System.Index.implicit operator System.Index(int)",
            "override System.Index.Equals(object)",
            "System.Index.Equals(System.Index)",
            "override System.Index.GetHashCode()",
            "override System.Index.ToString()"
        ]);
    }

    [TestMethod]
    public void RangeMappings_UseObjectCarrierAndRangeRuntimeModule()
    {
        AssertTypeAlias(typeof(Jazor.CLR.RangeModule), "System.Range", "Object");
        AssertImports(typeof(Jazor.CLR.RangeModule), "System/RangeModule.js",
        [
            "System.Range.Range()",
            "System.Range.Start.get",
            "System.Range.End.get",
            "System.Range.Range(System.Index, System.Index)",
            "override System.Range.Equals(object)",
            "System.Range.Equals(System.Range)",
            "override System.Range.GetHashCode()",
            "override System.Range.ToString()",
            "static System.Range.StartAt(System.Index)",
            "static System.Range.EndAt(System.Index)",
            "static System.Range.All.get",
            "System.Range.GetOffsetAndLength(int)"
        ]);
    }

    private static void AssertTypeAlias(Type moduleType, string member, string alias)
    {
        var attribute = moduleType.GetCustomAttribute<JazorAttribute>();

        Assert.IsNotNull(attribute);
        Assert.AreEqual(Op.Alias, attribute.Op);
        Assert.AreEqual(member, attribute.Member);
        Assert.AreEqual(alias, attribute.Value);
    }

    private static void AssertImports(Type moduleType, string modulePath, IReadOnlyList<string> expectedMembers)
    {
        var attributes = moduleType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(static method => method.GetCustomAttribute<JazorAttribute>())
            .OfType<JazorAttribute>()
            .Where(static attribute => attribute.Op == Op.Import)
            .OrderBy(static attribute => attribute.Member, StringComparer.Ordinal)
            .ToArray();

        Assert.HasCount(expectedMembers.Count, attributes);
        CollectionAssert.AreEquivalent(expectedMembers.ToArray(), attributes.Select(static attribute => attribute.Member).ToArray());
        foreach (var member in expectedMembers)
        {
            var mapping = ClrRuntimeMappingCatalog.GetImport(member);
            Assert.AreEqual(modulePath, mapping.ModulePath, member);
        }
    }
}
