using System.Reflection;
using ECMAScript.Contract;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class NullableModuleWhitelistTests
{
    [TestMethod]
    public void Value_UsesCompilerOwnedNullishThrowSemantics()
    {
        var attribute = typeof(Jazor.CLR.NullableT1Module<>)
            .GetMethod("_value", BindingFlags.Public | BindingFlags.Static)!
            .GetCustomAttribute<JazorAttribute>();

        Assert.IsNotNull(attribute);
        Assert.AreEqual(Op.Compile, attribute.Op);
        Assert.AreEqual("System.Nullable<T>.Value.get", attribute.Member);
        Assert.AreEqual("NullableValue", attribute.Value);
    }

    [TestMethod]
    public void GetValueOrDefault_UsesCompilerOwnedUnderlyingTypeDefault()
    {
        var attribute = typeof(Jazor.CLR.NullableT1Module<>)
            .GetMethod("_getValueOrDefault", BindingFlags.Public | BindingFlags.Static)!
            .GetCustomAttribute<JazorAttribute>();

        Assert.IsNotNull(attribute);
        Assert.AreEqual(Op.Compile, attribute.Op);
        Assert.AreEqual("System.Nullable<T>.GetValueOrDefault()", attribute.Member);
        Assert.AreEqual("NullableGetValueOrDefault", attribute.Value);
    }

    [TestMethod]
    public void GetValueOrDefault_WithExplicitDefault_UsesCompilerOwnedEagerEvaluation()
    {
        var attribute = typeof(Jazor.CLR.NullableT1Module<>)
            .GetMethod("_getValueOrDefaultWithDefault", BindingFlags.Public | BindingFlags.Static)!
            .GetCustomAttribute<JazorAttribute>();

        Assert.IsNotNull(attribute);
        Assert.AreEqual(Op.Compile, attribute.Op);
        Assert.AreEqual("System.Nullable<T>.GetValueOrDefault(T)", attribute.Member);
        Assert.AreEqual("NullableGetValueOrDefaultWithDefault", attribute.Value);
    }
}
