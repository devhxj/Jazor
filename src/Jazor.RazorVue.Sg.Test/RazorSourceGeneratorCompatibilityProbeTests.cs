using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class GeneratorDriverFinalCompilationContractTests
{
    [TestMethod]
    public void RunGeneratorsAndUpdateCompilation_ExposesStableFinalCompilationBoundary()
    {
        var method = typeof(GeneratorDriver).GetMethod(
            "RunGeneratorsAndUpdateCompilation",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public,
            binder: null,
            types:
            [
                typeof(Compilation),
                typeof(Compilation).MakeByRefType(),
                typeof(ImmutableArray<Diagnostic>).MakeByRefType(),
                typeof(CancellationToken)
            ],
            modifiers: null);

        Assert.IsNotNull(method);
        Assert.IsFalse(method!.IsVirtual);
        Assert.AreEqual(typeof(GeneratorDriver), method.DeclaringType);
    }
}
