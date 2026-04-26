using System.Reflection;
using Jazor.Compiler;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class WhiteListLookupCompatibilityTests
{
    [TestMethod]
    public void WhiteListLookup_GenericParameterNormalization_DoesNotRewriteQualifiedConcreteTypeNames()
    {
        const string candidateKey = "LookupTests.Host<T>.Use(LookupTests.Types.T)";
        const string lookupKey = "LookupTests.Host<U>.Use(LookupTests.Types.U)";
        var mappings = new Dictionary<string, string>
        {
            [candidateKey] = "allowed"
        };

        var result = InvokeStringLookup(
            typeof(SemanticWalker).Assembly.GetType("Jazor.Compiler.WhiteListLookup")
            ?? throw new InvalidOperationException("Cannot locate Jazor.Compiler.WhiteListLookup."),
            "TryGetValue",
            mappings,
            lookupKey,
            out var matchedKey,
            out var matchedValue);

        Assert.IsFalse(result, $"Unexpected whitelist match: key={matchedKey}, value={matchedValue}");
    }

    [TestMethod]
    public void SemanticWalkerLookup_GenericParameterNormalization_DoesNotRewriteQualifiedConcreteTypeNames()
    {
        const string candidateKey = "LookupTests.Host<T>.Use(LookupTests.Types.T)";
        const string lookupKey = "LookupTests.Host<U>.Use(LookupTests.Types.U)";
        var mappings = new Dictionary<string, string>
        {
            [candidateKey] = "allowed"
        };

        var result = InvokeStringLookup(
            typeof(SemanticWalker),
            "TryGetWhiteListValue",
            mappings,
            lookupKey,
            out var matchedKey,
            out var matchedValue);

        Assert.IsFalse(result, $"Unexpected whitelist match: key={matchedKey}, value={matchedValue}");
    }

    private static bool InvokeStringLookup(
        Type lookupType,
        string methodName,
        Dictionary<string, string> mappings,
        string lookupKey,
        out string? matchedKey,
        out string? matchedValue)
    {
        var method = lookupType
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
            .Single(candidate =>
            {
                if (candidate.Name != methodName || !candidate.IsGenericMethodDefinition)
                    return false;

                var parameters = candidate.GetParameters();
                return parameters.Length == 4 &&
                    parameters[0].ParameterType.IsGenericType &&
                    parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(Dictionary<,>) &&
                    parameters[1].ParameterType == typeof(string);
            })
            .MakeGenericMethod(typeof(string));

        var args = new object?[] { mappings, lookupKey, null, null };
        var result = (bool)(method.Invoke(null, args)
            ?? throw new InvalidOperationException($"Lookup method {lookupType.FullName}.{methodName} returned null."));
        matchedKey = args[2] as string;
        matchedValue = args[3] as string;
        return result;
    }
}
