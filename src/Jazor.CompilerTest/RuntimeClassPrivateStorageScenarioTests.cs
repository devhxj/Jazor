using Jazor.Common;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class RuntimeClassPrivateStorageScenarioTests
{
    [TestMethod]
    public void PrivateStorageNames_KeepDefaultFieldsAndMangleProxySafeMembersDeterministically()
    {
        const string source = """
            public sealed class RuntimeHost
            {
                private int secret;

                private int Value { get; set; }
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "RuntimeClassPrivateStorage",
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var runtimeHost = compilation.GetTypeByMetadataName("RuntimeHost")!;
        var secret = runtimeHost.GetMembers("secret").OfType<IFieldSymbol>().Single();
        var value = runtimeHost.GetMembers("Value").OfType<IPropertySymbol>().Single();
        var backingField = runtimeHost.GetMembers("<Value>k__BackingField").OfType<IFieldSymbol>().Single();
        var valueHash = Format.HashName(value.OriginalDefinition.ToDisplayString(Format.NameFormat));

        Assert.AreEqual(
            "secret",
            RuntimeClassPrivateStorageNames.GetFieldStorageName(
                RuntimeClassPrivateStorage.JavaScriptPrivateFields,
                secret,
                "secret"));
        Assert.AreEqual(
            "$jazor$private$secret",
            RuntimeClassPrivateStorageNames.GetFieldStorageName(
                RuntimeClassPrivateStorage.ProxySafeMangledProperties,
                secret,
                "secret"));
        Assert.AreEqual(
            "$jazor$private$" + valueHash,
            RuntimeClassPrivateStorageNames.GetFieldStorageName(
                RuntimeClassPrivateStorage.ProxySafeMangledProperties,
                backingField,
                "ignored"));
        Assert.AreEqual(
            "primary",
            RuntimeClassPrivateStorageNames.GetSyntheticStorageName(
                RuntimeClassPrivateStorage.JavaScriptPrivateFields,
                "primary"));
        Assert.AreEqual(
            "$jazor$private$primary",
            RuntimeClassPrivateStorageNames.GetSyntheticStorageName(
                RuntimeClassPrivateStorage.ProxySafeMangledProperties,
                "primary"));
    }
}
