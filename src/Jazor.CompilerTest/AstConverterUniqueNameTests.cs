using System.Text.RegularExpressions;
using Basic.Reference.Assemblies;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class AstConverterUniqueNameTests
{
    private static readonly Regex GeneratedNameRegex = new(@"__[a-z0-9]+\$[0-9a-f]+", RegexOptions.Compiled);

    private static async Task<string?> ConvertModuleAsync(string code, string path = "/src/TestClass.cs")
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code, path: path);
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            Net100.References.All,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var classDeclaration = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

        Assert.IsNotNull(classSymbol);
        var converter = new AstConverter(classSymbol, semanticModel);
        var module = await converter.Convert();
        return module?.ToKnRECMAScript();
    }

    private static string[] ExtractGeneratedNames(string? script)
    {
        Assert.IsNotNull(script);
        return GeneratedNameRegex.Matches(script)
            .Select(static match => match.Value)
            .Distinct()
            .ToArray();
    }

    private static string ExtractSingleGeneratedName(string? script)
    {
        var matches = ExtractGeneratedNames(script);
        Assert.AreEqual(1, matches.Length, $"Expected exactly one generated unique name, got {matches.Length}. Script:{Environment.NewLine}{script}");
        return matches[0];
    }

    private static string ExtractLastGeneratedName(string? script)
    {
        var matches = ExtractGeneratedNames(script);
        Assert.IsTrue(matches.Length > 0, $"Expected at least one generated unique name. Script:{Environment.NewLine}{script}");
        return matches[^1];
    }

    [TestMethod]
    public async Task Convert_ClassWithSwitchExpression_StableUniqueNameAcrossTriviaOnlyChanges()
    {
        var compact = """
            public static class TestClass
            {
                public static int Get(int value)
                    => value switch
                    {
                        1 => 10,
                        _ => 0
                    };
            }
            """;

        var triviaHeavy = """
            public static class TestClass
            {
                // comment before method
                public static int Get(
                    int value
                )
                    => value switch
                    {
                        1 => 10, // inline comment

                        _ => 0
                    };
            }
            """;

        var compactScript = await ConvertModuleAsync(compact);
        var triviaScript = await ConvertModuleAsync(triviaHeavy);

        Assert.AreEqual(ExtractSingleGeneratedName(compactScript), ExtractSingleGeneratedName(triviaScript));
    }

    [TestMethod]
    public async Task Convert_ClassWithSwitchExpression_StableAcrossUnrelatedStatementInsertion()
    {
        var baseCode = """
            public static class TestClass
            {
                public static int Get(int value)
                {
                    int seed = 0;
                    return value switch
                    {
                        1 => 10,
                        _ => 0
                    };
                }
            }
            """;

        var insertedCode = """
            public static class TestClass
            {
                public static int Get(int value)
                {
                    int seed = 0;
                    int extra = seed + 1;
                    return value switch
                    {
                        1 => 10,
                        _ => extra
                    };
                }
            }
            """;

        var baseScript = await ConvertModuleAsync(baseCode);
        var insertedScript = await ConvertModuleAsync(insertedCode);

        Assert.AreEqual(ExtractSingleGeneratedName(baseScript), ExtractSingleGeneratedName(insertedScript));
    }

    [TestMethod]
    public async Task Convert_ClassWithSwitchExpressionInsideLocalDeclaration_StableAcrossEarlierLocalDeclarationInsertion()
    {
        var baseCode = """
            public static class TestClass
            {
                public static int Get(int value)
                {
                    var result = value switch
                    {
                        1 => 10,
                        _ => 0
                    };
                    return result;
                }
            }
            """;

        var insertedCode = """
            public static class TestClass
            {
                public static int Get(int value)
                {
                    int seed = 0;
                    var result = value switch
                    {
                        1 => 10,
                        _ => seed
                    };
                    return result;
                }
            }
            """;

        var baseScript = await ConvertModuleAsync(baseCode);
        var insertedScript = await ConvertModuleAsync(insertedCode);

        Assert.AreEqual(ExtractSingleGeneratedName(baseScript), ExtractSingleGeneratedName(insertedScript));
    }

    [TestMethod]
    public async Task Convert_ClassWithSwitchExpressionInsideIfBlock_StableAcrossEarlierSiblingIfWithSameCondition()
    {
        var baseCode = """
            public static class TestClass
            {
                public static int Get(bool flag, int value)
                {
                    if (flag)
                    {
                        return value switch
                        {
                            1 => 10,
                            _ => 0
                        };
                    }

                    return 0;
                }
            }
            """;

        var insertedCode = """
            public static class TestClass
            {
                public static int Get(bool flag, int value)
                {
                    if (flag)
                    {
                        _ = value;
                    }

                    if (flag)
                    {
                        return value switch
                        {
                            1 => 10,
                            _ => 0
                        };
                    }

                    return 0;
                }
            }
            """;

        var baseScript = await ConvertModuleAsync(baseCode);
        var insertedScript = await ConvertModuleAsync(insertedCode);

        Assert.AreEqual(ExtractSingleGeneratedName(baseScript), ExtractSingleGeneratedName(insertedScript));
    }

    [TestMethod]
    public async Task Convert_ClassWithSwitchExpressionInsideWhileLoop_StableAcrossEarlierSiblingWhileWithSameCondition()
    {
        var baseCode = """
            public static class TestClass
            {
                public static int Get(bool flag, int value)
                {
                    while (flag)
                    {
                        return value switch
                        {
                            1 => 10,
                            _ => 0
                        };
                    }

                    return 0;
                }
            }
            """;

        var insertedCode = """
            public static class TestClass
            {
                public static int Get(bool flag, int value)
                {
                    while (flag)
                    {
                        _ = value;
                        break;
                    }

                    while (flag)
                    {
                        return value switch
                        {
                            1 => 10,
                            _ => 0
                        };
                    }

                    return 0;
                }
            }
            """;

        var baseScript = await ConvertModuleAsync(baseCode);
        var insertedScript = await ConvertModuleAsync(insertedCode);

        Assert.AreEqual(ExtractSingleGeneratedName(baseScript), ExtractSingleGeneratedName(insertedScript));
    }

    [TestMethod]
    public async Task Convert_ClassWithSwitchExpressionInsideTry_StableAcrossEarlierSiblingTryWithSameCatchShape()
    {
        var baseCode = """
            public static class TestClass
            {
                public static int Get(int value)
                {
                    try
                    {
                        return value switch
                        {
                            1 => 10,
                            _ => 0
                        };
                    }
                    catch (System.Exception)
                    {
                        return 0;
                    }
                }
            }
            """;

        var insertedCode = """
            public static class TestClass
            {
                public static int Get(int value)
                {
                    try
                    {
                        _ = value;
                    }
                    catch (System.Exception)
                    {
                        return -1;
                    }

                    try
                    {
                        return value switch
                        {
                            1 => 10,
                            _ => 0
                        };
                    }
                    catch (System.Exception)
                    {
                        return 0;
                    }
                }
            }
            """;

        var baseScript = await ConvertModuleAsync(baseCode);
        var insertedScript = await ConvertModuleAsync(insertedCode);

        Assert.AreEqual(ExtractSingleGeneratedName(baseScript), ExtractSingleGeneratedName(insertedScript));
    }

    [TestMethod]
    public async Task Convert_ClassWithSwitchExpressionInsideSwitchStatement_StableAcrossEarlierSiblingSwitchWithSameCaseCount()
    {
        var baseCode = """
            public static class TestClass
            {
                public static int Get(int kind, int value)
                {
                    switch (kind)
                    {
                        case 0:
                            return value switch
                            {
                                1 => 10,
                                _ => 0
                            };
                        default:
                            return 0;
                    }
                }
            }
            """;

        var insertedCode = """
            public static class TestClass
            {
                public static int Get(int kind, int value)
                {
                    switch (kind)
                    {
                        case 0:
                            _ = value;
                            break;
                        default:
                            break;
                    }

                    switch (kind)
                    {
                        case 0:
                            return value switch
                            {
                                1 => 10,
                                _ => 0
                            };
                        default:
                            return 0;
                    }
                }
            }
            """;

        var baseScript = await ConvertModuleAsync(baseCode);
        var insertedScript = await ConvertModuleAsync(insertedCode);

        Assert.AreEqual(ExtractSingleGeneratedName(baseScript), ExtractSingleGeneratedName(insertedScript));
    }

    [TestMethod]
    public async Task Convert_ClassWithTargetSwitchExpression_StableAcrossEarlierSiblingSwitchExpressionWithSameArmCount()
    {
        var baseCode = """
            public static class TestClass
            {
                public static int Get(bool flag, int value)
                {
                    if (flag)
                    {
                        return value switch
                        {
                            1 => 10,
                            _ => 0
                        };
                    }

                    return 0;
                }
            }
            """;

        var insertedCode = """
            public static class TestClass
            {
                public static int Get(bool flag, int value)
                {
                    if (flag)
                    {
                        return value switch
                        {
                            2 => 20,
                            _ => 0
                        };
                    }

                    if (flag)
                    {
                        return value switch
                        {
                            1 => 10,
                            _ => 0
                        };
                    }

                    return 0;
                }
            }
            """;

        var baseScript = await ConvertModuleAsync(baseCode);
        var insertedScript = await ConvertModuleAsync(insertedCode);

        Assert.AreEqual(ExtractSingleGeneratedName(baseScript), ExtractLastGeneratedName(insertedScript));
    }

    [TestMethod]
    public async Task Convert_ClassWithRelationalPatternSwitchExpression_StableAcrossEarlierSiblingSwitchExpressionWithDifferentThreshold()
    {
        var baseCode = """
            public static class TestClass
            {
                public static int Get(bool flag, int value)
                {
                    if (flag)
                    {
                        return value switch
                        {
                            > 0 => 1,
                            _ => 0
                        };
                    }

                    return 0;
                }
            }
            """;

        var insertedCode = """
            public static class TestClass
            {
                public static int Get(bool flag, int value)
                {
                    if (flag)
                    {
                        return value switch
                        {
                            > 1 => 1,
                            _ => 0
                        };
                    }

                    if (flag)
                    {
                        return value switch
                        {
                            > 0 => 1,
                            _ => 0
                        };
                    }

                    return 0;
                }
            }
            """;

        var baseScript = await ConvertModuleAsync(baseCode);
        var insertedScript = await ConvertModuleAsync(insertedCode);

        Assert.AreEqual(ExtractSingleGeneratedName(baseScript), ExtractLastGeneratedName(insertedScript));
    }

    [TestMethod]
    public async Task Convert_ClassWithAwaitWrappedSwitchExpression_StableAcrossEarlierSiblingAwaitWrappedSwitchExpression()
    {
        var baseCode = """
            public static class TestClass
            {
                public static async System.Threading.Tasks.Task<int> Get(int value, int other)
                {
                    return Use((await MapAsync(value)) switch
                    {
                        1 => 10,
                        _ => 0
                    });
                }

                private static System.Threading.Tasks.Task<int> MapAsync(int value)
                    => System.Threading.Tasks.Task.FromResult(value);

                private static int Use(params int[] values)
                    => values.Length;
            }
            """;

        var insertedCode = """
            public static class TestClass
            {
                public static async System.Threading.Tasks.Task<int> Get(int value, int other)
                {
                    return Use(
                        (await MapAsync(other)) switch
                        {
                            1 => 10,
                            _ => 0
                        },
                        (await MapAsync(value)) switch
                        {
                            1 => 10,
                            _ => 0
                        });
                }

                private static System.Threading.Tasks.Task<int> MapAsync(int value)
                    => System.Threading.Tasks.Task.FromResult(value);

                private static int Use(params int[] values)
                    => values.Length;
            }
            """;

        var baseScript = await ConvertModuleAsync(baseCode);
        var insertedScript = await ConvertModuleAsync(insertedCode);

        Assert.AreEqual(ExtractSingleGeneratedName(baseScript), ExtractLastGeneratedName(insertedScript));
    }

    [TestMethod]
    public async Task Convert_ClassWithCoalesceWrappedSwitchExpression_StableAcrossEarlierSiblingCoalesceWrappedSwitchExpression()
    {
        var baseCode = """
            public static class TestClass
            {
                public static int Get(int? value, int? other, int fallback)
                {
                    return Use((value ?? fallback) switch
                    {
                        1 => 10,
                        _ => 0
                    });
                }

                private static int Use(params int[] values)
                    => values.Length;
            }
            """;

        var insertedCode = """
            public static class TestClass
            {
                public static int Get(int? value, int? other, int fallback)
                {
                    return Use(
                        (other ?? fallback) switch
                        {
                            1 => 10,
                            _ => 0
                        },
                        (value ?? fallback) switch
                        {
                            1 => 10,
                            _ => 0
                        });
                }

                private static int Use(params int[] values)
                    => values.Length;
            }
            """;

        var baseScript = await ConvertModuleAsync(baseCode);
        var insertedScript = await ConvertModuleAsync(insertedCode);

        Assert.AreEqual(ExtractSingleGeneratedName(baseScript), ExtractLastGeneratedName(insertedScript));
    }

    [TestMethod]
    public async Task Convert_ClassWithConditionalAccessWrappedSwitchExpression_StableAcrossEarlierSiblingConditionalAccessWrappedSwitchExpression()
    {
        var baseCode = """
            public static class TestClass
            {
                public sealed class Box
                {
                    public int Value { get; set; }
                }

                public static int Get(Box first, Box second)
                {
                    return Use(first?.Value switch
                    {
                        1 => 10,
                        _ => 0
                    });
                }

                private static int Use(params int?[] values)
                    => values.Length;
            }
            """;

        var insertedCode = """
            public static class TestClass
            {
                public sealed class Box
                {
                    public int Value { get; set; }
                }

                public static int Get(Box first, Box second)
                {
                    return Use(
                        second?.Value switch
                        {
                            1 => 10,
                            _ => 0
                        },
                        first?.Value switch
                        {
                            1 => 10,
                            _ => 0
                        });
                }

                private static int Use(params int?[] values)
                    => values.Length;
            }
            """;

        var baseScript = await ConvertModuleAsync(baseCode);
        var insertedScript = await ConvertModuleAsync(insertedCode);

        Assert.AreEqual(ExtractSingleGeneratedName(baseScript), ExtractLastGeneratedName(insertedScript));
    }

    [TestMethod]
    public async Task Convert_ClassWithArrayInitializerContainedSwitchExpression_StableAcrossEarlierSiblingArrayInitializer()
    {
        var baseCode = """
            public static class TestClass
            {
                public static int Get(int value, int seed)
                {
                    return Use(new[]
                    {
                        value switch
                        {
                            1 => 10,
                            _ => 0
                        }
                    }[0]);
                }

                private static int Use(params int[] values)
                    => values.Length;
            }
            """;

        var insertedCode = """
            public static class TestClass
            {
                public static int Get(int value, int seed)
                {
                    return Use(
                        new[]
                        {
                            seed switch
                            {
                                1 => 10,
                                _ => 0
                            }
                        }[0],
                        new[]
                        {
                            value switch
                            {
                                1 => 10,
                                _ => 0
                            }
                        }[0]);
                }

                private static int Use(params int[] values)
                    => values.Length;
            }
            """;

        var baseScript = await ConvertModuleAsync(baseCode);
        var insertedScript = await ConvertModuleAsync(insertedCode);

        Assert.AreEqual(ExtractSingleGeneratedName(baseScript), ExtractLastGeneratedName(insertedScript));
    }

    [TestMethod]
    public async Task Convert_ClassWithPatternSwitchStatement_StableAcrossEarlierSiblingCoalesceWrappedPatternSwitch()
    {
        var baseCode = """
            public static class TestClass
            {
                public static int Get(int? value, int? other, int fallback)
                {
                    switch (value ?? fallback)
                    {
                        case > 0:
                            return 1;
                        default:
                            return 0;
                    }
                }
            }
            """;

        var insertedCode = """
            public static class TestClass
            {
                public static int Get(int? value, int? other, int fallback)
                {
                    switch (other ?? fallback)
                    {
                        case > 0:
                            break;
                        default:
                            break;
                    }

                    switch (value ?? fallback)
                    {
                        case > 0:
                            return 1;
                        default:
                            return 0;
                    }
                }
            }
            """;

        var baseScript = await ConvertModuleAsync(baseCode);
        var insertedScript = await ConvertModuleAsync(insertedCode);

        var baseName = ExtractSingleGeneratedName(baseScript);
        var insertedName = ExtractLastGeneratedName(insertedScript);

        Assert.AreEqual(baseName, insertedName);
        StringAssert.StartsWith(baseName, "__swpat$");
    }

    [TestMethod]
    public async Task Convert_ClassWithAnonymousFunctionWrappedSwitchExpression_StableAcrossEarlierSiblingLambda()
    {
        var baseCode = """
            public static class TestClass
            {
                public static int Get(int value, int other)
                {
                    System.Func<int, int> map = item => item switch
                    {
                        1 => 10,
                        _ => 0
                    };

                    return map(value);
                }
            }
            """;

        var insertedCode = """
            public static class TestClass
            {
                public static int Get(int value, int other)
                {
                    System.Func<int, int> before = item => item switch
                    {
                        2 => 20,
                        _ => 0
                    };

                    System.Func<int, int> map = item => item switch
                    {
                        1 => 10,
                        _ => 0
                    };

                    return before(other) + map(value);
                }
            }
            """;

        var baseScript = await ConvertModuleAsync(baseCode);
        var insertedScript = await ConvertModuleAsync(insertedCode);

        Assert.AreEqual(ExtractSingleGeneratedName(baseScript), ExtractLastGeneratedName(insertedScript));
    }

    [TestMethod]
    public async Task Convert_ClassWithTupleSwap_UsesDistinctStableNamesForMultipleLoweringSlots()
    {
        var code = """
            public static class TestClass
            {
                public static (int left, int right) Swap()
                {
                    int left = 1;
                    int right = 2;
                    (left, right) = (right, left);
                    return (left, right);
                }
            }
            """;

        var script = await ConvertModuleAsync(code);
        var names = ExtractGeneratedNames(script);

        Assert.AreEqual(2, names.Length, $"Expected two generated names, got {names.Length}. Script:{Environment.NewLine}{script}");
        Assert.AreNotEqual(names[0], names[1]);
    }

    [TestMethod]
    public async Task Convert_ClassWithMultipleCatch_AllocatesSingleStableCatchName()
    {
        var code = """
            public static class TestClass
            {
                public static int Get()
                {
                    try
                    {
                        throw new System.Exception();
                    }
                    catch (System.ArgumentNullException)
                    {
                        return 1;
                    }
                    catch (System.Exception)
                    {
                        return 2;
                    }
                }
            }
            """;

        var script = await ConvertModuleAsync(code);
        var names = ExtractGeneratedNames(script);

        Assert.AreEqual(1, names.Length, $"Expected one generated name, got {names.Length}. Script:{Environment.NewLine}{script}");
    }

    [TestMethod]
    public async Task Convert_ClassWithMultipleUniqueNameSites_UsesDeterministicTagNames()
    {
        var code = """
            public static class TestClass
            {
                public static (int left, int right) Get(int input)
                {
                    int left = input switch
                    {
                        > 0 => 1,
                        < 0 => -1,
                        _ => 0
                    };
                    int right = 2;
                    (left, right) = (right, left);
                    return (left, right);
                }
            }
            """;

        var script = await ConvertModuleAsync(code);
        var names = ExtractGeneratedNames(script);

        CollectionAssert.AreEquivalent(
            new[]
            {
                "__swexpr$",
                "__tfield$"
            },
            names.Select(static name => name[..(name.LastIndexOf('$') + 1)]).Distinct().ToArray());
    }

    [TestMethod]
    public async Task Convert_ClassWithIsTypeWrappedSwitchExpression_StableAcrossEarlierSiblingDifferentIsTypeSwitch()
    {
        var baseCode = """
            public static class TestClass
            {
                public static int Get(object value)
                {
                    return (value is int) switch
                    {
                        true => 1,
                        _ => 0
                    };
                }
            }
            """;

        var insertedCode = """
            public static class TestClass
            {
                public static int Get(object value)
                {
                    _ = (value is string) switch
                    {
                        true => -1,
                        _ => 2
                    };

                    return (value is int) switch
                    {
                        true => 1,
                        _ => 0
                    };
                }
            }
            """;

        var baseScript = await ConvertModuleAsync(baseCode);
        var insertedScript = await ConvertModuleAsync(insertedCode);

        Assert.AreEqual(ExtractSingleGeneratedName(baseScript), ExtractLastGeneratedName(insertedScript));
    }

    [TestMethod]
    public async Task Convert_ClassWithInterpolatedStringWrappedSwitchExpression_StableAcrossEarlierSiblingDifferentInterpolatedStringSwitch()
    {
        var baseCode = """
            public static class TestClass
            {
                public static int Get(int value, int other)
                {
                    return ($"{value}") switch
                    {
                        "1" => 1,
                        _ => 0
                    };
                }
            }
            """;

        var insertedCode = """
            public static class TestClass
            {
                public static int Get(int value, int other)
                {
                    _ = ($"{other}") switch
                    {
                        "2" => -1,
                        _ => 2
                    };

                    return ($"{value}") switch
                    {
                        "1" => 1,
                        _ => 0
                    };
                }
            }
            """;

        var baseScript = await ConvertModuleAsync(baseCode);
        var insertedScript = await ConvertModuleAsync(insertedCode);

        Assert.AreEqual(ExtractSingleGeneratedName(baseScript), ExtractLastGeneratedName(insertedScript));
    }

    [TestMethod]
    public async Task Convert_ClassWithInterpolatedStringTextWrappedSwitchExpression_StableAcrossEarlierSiblingDifferentInterpolatedStringTextSwitch()
    {
        var baseCode = """
            public static class TestClass
            {
                public static int Get(int value)
                {
                    return ($"a{value}") switch
                    {
                        "a1" => 1,
                        _ => 0
                    };
                }
            }
            """;

        var insertedCode = """
            public static class TestClass
            {
                public static int Get(int value)
                {
                    _ = ($"b{value}") switch
                    {
                        "b1" => -1,
                        _ => 2
                    };

                    return ($"a{value}") switch
                    {
                        "a1" => 1,
                        _ => 0
                    };
                }
            }
            """;

        var baseScript = await ConvertModuleAsync(baseCode);
        var insertedScript = await ConvertModuleAsync(insertedCode);

        Assert.AreEqual(ExtractSingleGeneratedName(baseScript), ExtractLastGeneratedName(insertedScript));
    }

}
