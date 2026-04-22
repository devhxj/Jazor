using Jolt.Debug;

namespace Jolt.Test;

[TestClass]
public sealed class JoltDebugVariableMapperTests
{
    [TestMethod]
    public void VariableMapper_ToEvaluationResult_FormatsPrimitiveAndObjectValues()
    {
        var mapper = new VariableMapper();

        var stringResult = mapper.ToEvaluationResult(
            new CdpRemoteObject(
                Type: "string",
                SubType: null,
                Description: "\"hello\"",
                Value: "hello",
                UnserializableValue: null,
                ObjectId: null));
        Assert.AreEqual("hello", stringResult.Result);
        Assert.AreEqual("string", stringResult.Type);
        Assert.AreEqual(0, stringResult.VariablesReference);

        var objectResult = mapper.ToEvaluationResult(
            new CdpRemoteObject(
                Type: "object",
                SubType: "array",
                Description: "Array(2)",
                Value: null,
                UnserializableValue: null,
                ObjectId: "remote-1"),
            variablesReference: 11);
        Assert.AreEqual("Array(2)", objectResult.Result);
        Assert.AreEqual("object", objectResult.Type);
        Assert.AreEqual(11, objectResult.VariablesReference);
    }

    [TestMethod]
    public void VariableMapper_ToVariable_FormatsUnserializableNumbers()
    {
        var mapper = new VariableMapper();
        var variable = mapper.ToVariable(
            "value",
            new CdpRemoteObject(
                Type: "number",
                SubType: null,
                Description: null,
                Value: null,
                UnserializableValue: "NaN",
                ObjectId: null));

        Assert.AreEqual("value", variable.Name);
        Assert.AreEqual("NaN", variable.Value);
        Assert.AreEqual("number", variable.Type);
        Assert.AreEqual(0, variable.VariablesReference);
    }

    [TestMethod]
    public void VariableMapper_ToVariable_TruncatesOversizedValues()
    {
        var mapper = new VariableMapper();
        var variable = mapper.ToVariable(
            "secretLikePayload",
            new CdpRemoteObject(
                Type: "string",
                SubType: null,
                Description: null,
                Value: new string('x', 5000),
                UnserializableValue: null,
                ObjectId: null));

        Assert.IsTrue(variable.Value.Length < 5000);
        StringAssert.EndsWith(variable.Value, "...");
    }
}
