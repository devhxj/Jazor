using System.Reflection;
using Acornima.Ast;
using Jazor.Compiler;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerInlineTemplateTest
{
    private static MethodInfo GetInlineMethod(string name, params Type[] parameterTypes)
        => typeof(SemanticWalker).GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static,
            binder: null,
            types: parameterTypes,
            modifiers: null)
           ?? throw new InvalidOperationException($"未找到方法: {name}");

    private static InvalidOperationException AssertInlineInvalidOperation(Action invoke)
    {
        try
        {
            invoke();
            throw new AssertFailedException("Expected InvalidOperationException but no exception was thrown.");
        }
        catch (TargetInvocationException tie) when (tie.InnerException is InvalidOperationException ioe)
        {
            return ioe;
        }
        catch (InvalidOperationException ioe)
        {
            return ioe;
        }
    }

    private static string NewSignature()
        => $"InlineTemplateTest/{Guid.NewGuid():N}";

    [TestMethod]
    public void ParseInlineTemplate_WithLegacyPlaceholder_Throws()
    {
        var parseMethod = GetInlineMethod("ParseInlineTemplate", typeof(string), typeof(string));
        var exception = AssertInlineInvalidOperation(() => parseMethod.Invoke(null, [NewSignature(), "@#{1} + 1"]));

        StringAssert.Contains(exception.Message, "legacy placeholder syntax");
        StringAssert.Contains(exception.Message, "__arg1");
    }

    [TestMethod]
    public void ParseInlineTemplate_WithReservedInternalPrefix_Throws()
    {
        var parseMethod = GetInlineMethod("ParseInlineTemplate", typeof(string), typeof(string));
        var exception = AssertInlineInvalidOperation(() => parseMethod.Invoke(null, [NewSignature(), "__jz_arg0 + 1"]));

        StringAssert.Contains(exception.Message, "reserved placeholder prefix");
        StringAssert.Contains(exception.Message, "__jz_arg");
    }

    [TestMethod]
    public void ParseInlineTemplate_WithZeroBasedPlaceholder_Throws()
    {
        var parseMethod = GetInlineMethod("ParseInlineTemplate", typeof(string), typeof(string));
        var exception = AssertInlineInvalidOperation(() => parseMethod.Invoke(null, [NewSignature(), "__arg01 + 1"]));

        StringAssert.Contains(exception.Message, "zero-based placeholder");
        StringAssert.Contains(exception.Message, "__arg1");
    }

    [TestMethod]
    public void ParseInlineTemplate_WithInvalidExpression_Throws()
    {
        var parseMethod = GetInlineMethod("ParseInlineTemplate", typeof(string), typeof(string));
        var exception = AssertInlineInvalidOperation(() => parseMethod.Invoke(null, [NewSignature(), "__arg1 +"]));

        StringAssert.Contains(exception.Message, "not a valid JavaScript expression");
        Assert.IsNotNull(exception.InnerException);
    }

    [TestMethod]
    public void InstantiateInlineTemplate_WithInsufficientArguments_Throws()
    {
        var instantiateMethod = GetInlineMethod(
            "InstantiateInlineTemplate",
            typeof(string),
            typeof(string),
            typeof(IReadOnlyList<Expression>),
            typeof(string),
            typeof(Identifier));
        var args = new List<Expression> { new Identifier("left") };
        var exception = AssertInlineInvalidOperation(() =>
            instantiateMethod.Invoke(null, [NewSignature(), "__arg1 + __arg2", args, null, null]));

        StringAssert.Contains(exception.Message, "expects at least 2 arguments");
        StringAssert.Contains(exception.Message, "received 1");
    }

    [TestMethod]
    public void InstantiateInlineTemplate_WithSparseHighPlaceholderAndInsufficientArguments_Throws()
    {
        var instantiateMethod = GetInlineMethod(
            "InstantiateInlineTemplate",
            typeof(string),
            typeof(string),
            typeof(IReadOnlyList<Expression>),
            typeof(string),
            typeof(Identifier));
        var args = new List<Expression>
        {
            new Identifier("left"),
            new Identifier("right")
        };
        var exception = AssertInlineInvalidOperation(() =>
            instantiateMethod.Invoke(null, [NewSignature(), "__arg3 + 1", args, null, null]));

        StringAssert.Contains(exception.Message, "expects at least 3 arguments");
        StringAssert.Contains(exception.Message, "received 2");
    }
}
