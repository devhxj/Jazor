using Acornima.Ast;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class GuidRuntimeArtifactTests
{
    private const string GuidModulePath = "System/GuidModule.js";

    [TestMethod]
    public void ToStringExport_UsesExplicitCharCodeConversionsForLowercaseSpecifier()
    {
        var function = GetExportedFunction("System.Guid.ToString(string)");
        var calls = DescendantsAndSelf(function).OfType<CallExpression>().ToArray();

        Assert.IsTrue(calls.Any(static call => IsMemberCall(call, "String", "fromCharCode")));
        Assert.IsTrue(calls.Any(static call => IsMemberCall(call, instanceName: null, "charCodeAt")));
    }

    [TestMethod]
    public void TryParseExport_UsesTwoElementOutParameterResultProtocol()
    {
        var function = GetExportedFunction("static System.Guid.TryParse(string, out System.Guid)");
        var arrays = DescendantsAndSelf(function)
            .OfType<ArrayExpression>()
            .Where(static array => array.Elements.Count == 2)
            .ToArray();

        Assert.HasCount(2, arrays);
        Assert.IsTrue(arrays.Any(static array => array.Elements[0] is BooleanLiteral { Value: true }));
        Assert.IsTrue(arrays.Any(static array => array.Elements[0] is BooleanLiteral { Value: false }));
    }

    private static FunctionDeclaration GetExportedFunction(string member)
    {
        var mapping = ClrRuntimeMappingCatalog.GetImport(member);
        Assert.AreEqual(GuidModulePath, mapping.ModulePath);
        return ClrRuntimeCatalog.Get(GuidModulePath).GetExportedFunction(mapping.ExportName);
    }

    private static bool IsMemberCall(CallExpression call, string? instanceName, string memberName)
        => call.Callee is MemberExpression
        {
            Object: var instance,
            Property: Identifier { Name: var actualMember }
        }
        && actualMember == memberName
        && (instanceName is null || instance is Identifier { Name: var actualInstance } && actualInstance == instanceName);

    private static IEnumerable<Node> DescendantsAndSelf(Node node)
    {
        yield return node;
        foreach (var child in node.ChildNodes)
        {
            foreach (var descendant in DescendantsAndSelf(child))
                yield return descendant;
        }
    }
}
