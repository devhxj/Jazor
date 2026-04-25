using Basic.Reference.Assemblies;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class UniqueNameAllocatorTests
{
    private static readonly Regex GeneratedNameRegex = new(@"__[a-z0-9]+\$[0-9a-f]+", RegexOptions.Compiled);

    private static readonly Assembly CompilerAssembly = typeof(SemanticWalker).Assembly;
    private static readonly Type UniqueNameSessionType = CompilerAssembly.GetType("Jazor.Compiler.UniqueNameSession", throwOnError: true)!;
    private static readonly Type EmissionScopeContextType = CompilerAssembly.GetType("Jazor.Compiler.EmissionScopeContext", throwOnError: true)!;
    private static readonly Type ScopeSiteType = CompilerAssembly.GetType("Jazor.Compiler.ScopeSite", throwOnError: true)!;
    private static readonly Type LoweringSiteType = CompilerAssembly.GetType("Jazor.Compiler.LoweringSite", throwOnError: true)!;
    private static readonly Type LoweringNameOwnerType = CompilerAssembly.GetType("Jazor.Compiler.LoweringNameOwner", throwOnError: true)!;

    private static IBlockOperation GetBlockOperation(string code)
    {
        const string usings = """
            global using System;
            global using System.Collections.Generic;
            global using System.Linq;
            global using System.Numerics;
            global using ECMAScript;
            global using static ECMAScript.Global;
            """;

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(usings),
                CSharpSyntaxTree.ParseText(code)
            ],
            references: Net100.References.All
                .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var diagnostics = compilation.GetDiagnostics();
        var errors = diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).ToList();
        if (errors.Count > 0)
        {
            var errorMessages = string.Join("\n", errors.Select(static e => $"{e.Id}: {e.GetMessage()}"));
            throw new InvalidOperationException(errorMessages);
        }

        var syntaxTree = compilation.SyntaxTrees[^1];
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var methodDeclaration = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        return (semanticModel.GetOperation(methodDeclaration.Body!) as IBlockOperation)
            ?? throw new InvalidOperationException("未找到可分析的操作");
    }

    [TestMethod]
    public void Allocate_AllowsStableFallbackBeyondSixtyFourReservedCandidates()
    {
        var block = GetBlockOperation("""
            class TestClass
            {
                void TestMethod()
                {
                    int value = 1;
                }
            }
            """);

        var rootSite = InvokeStaticFactory(ScopeSiteType, "RootFragment");
        var loweringSite = InvokeStaticFactory(LoweringSiteType, "CreationTemp");
        var session = Activator.CreateInstance(
            UniqueNameSessionType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args: [block, rootSite],
            culture: null) ?? throw new InvalidOperationException("无法创建 UniqueNameSession。");
        var rootScope = UniqueNameSessionType
            .GetProperty("RootScope", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .GetValue(session)
            ?? throw new InvalidOperationException("无法读取 RootScope。");
        var scopeKey = EmissionScopeContextType
            .GetProperty("ScopeKey", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .GetValue(rootScope) as string
            ?? throw new InvalidOperationException("无法读取 ScopeKey。");
        var getOperationIdentity = UniqueNameSessionType.GetMethod(
            "GetOperationIdentity",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(IOperation)],
            modifiers: null)
            ?? throw new InvalidOperationException("无法调用 GetOperationIdentity。");
        var ownerIdentity = getOperationIdentity.Invoke(session, [block]) as string
            ?? throw new InvalidOperationException("无法获取 operation identity。");
        var owner = Activator.CreateInstance(
            LoweringNameOwnerType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args: [ownerIdentity, ownerIdentity],
            culture: null) ?? throw new InvalidOperationException("无法创建 LoweringNameOwner。");
        var reservedNames = EmissionScopeContextType
            .GetField("_localReservedNames", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(rootScope)
            ?? throw new InvalidOperationException("无法读取保留名集合。");
        var addReservedName = reservedNames.GetType().GetMethod("Add", [typeof(string)])
            ?? throw new InvalidOperationException("无法向保留名集合添加名称。");
        var createName = UniqueNameSessionType.GetMethod(
            "CreateName",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: [LoweringSiteType, typeof(string), LoweringNameOwnerType, typeof(string)],
            modifiers: null)
            ?? throw new InvalidOperationException("无法调用 CreateName。");

        for (var index = 0; index < 80; index++)
        {
            var salt = index == 0 ? "p" : "f" + index.ToString(CultureInfo.InvariantCulture);
            var candidate = createName.Invoke(session, [loweringSite, scopeKey, owner, salt]) as string
                ?? throw new InvalidOperationException("无法生成预留名称。");
            addReservedName.Invoke(reservedNames, [candidate]);
        }

        var expected = createName.Invoke(session, [loweringSite, scopeKey, owner, "f80"]) as string
            ?? throw new InvalidOperationException("无法生成期望名称。");
        var allocate = EmissionScopeContextType.GetMethod(
            "Allocate",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: [LoweringNameOwnerType, LoweringSiteType],
            modifiers: null)
            ?? throw new InvalidOperationException("无法调用 Allocate。");
        var actual = allocate.Invoke(rootScope, [owner, loweringSite]) as string
            ?? throw new InvalidOperationException("无法分配稳定名称。");

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void Allocate_ReusesSameNameForSameOwnerAndFallsBackForDistinctOwnerWithSameStableKey()
    {
        var block = GetBlockOperation("""
            class TestClass
            {
                void TestMethod()
                {
                    int value = 1;
                }
            }
            """);

        var rootSite = InvokeStaticFactory(ScopeSiteType, "RootFragment");
        var loweringSite = InvokeStaticFactory(LoweringSiteType, "ReferenceTemp");
        var session = Activator.CreateInstance(
            UniqueNameSessionType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args: [block, rootSite],
            culture: null) ?? throw new InvalidOperationException("无法创建 UniqueNameSession。");
        var rootScope = UniqueNameSessionType
            .GetProperty("RootScope", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .GetValue(session)
            ?? throw new InvalidOperationException("无法读取 RootScope。");
        var scopeKey = EmissionScopeContextType
            .GetProperty("ScopeKey", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .GetValue(rootScope) as string
            ?? throw new InvalidOperationException("无法读取 ScopeKey。");
        var owner1 = Activator.CreateInstance(
            LoweringNameOwnerType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args: ["shared-owner", "id-1"],
            culture: null) ?? throw new InvalidOperationException("无法创建 LoweringNameOwner(id-1)。");
        var owner2 = Activator.CreateInstance(
            LoweringNameOwnerType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args: ["shared-owner", "id-2"],
            culture: null) ?? throw new InvalidOperationException("无法创建 LoweringNameOwner(id-2)。");

        var allocate = EmissionScopeContextType.GetMethod(
            "Allocate",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: [LoweringNameOwnerType, LoweringSiteType],
            modifiers: null)
            ?? throw new InvalidOperationException("无法调用 Allocate(LoweringNameOwner, LoweringSite)。");
        var createName = UniqueNameSessionType.GetMethod(
            "CreateName",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: [LoweringSiteType, typeof(string), LoweringNameOwnerType, typeof(string)],
            modifiers: null)
            ?? throw new InvalidOperationException("无法调用 CreateName(LoweringSite, string, LoweringNameOwner, string)。");

        var first = allocate.Invoke(rootScope, [owner1, loweringSite]) as string
            ?? throw new InvalidOperationException("无法为 owner1 分配名称。");
        var firstAgain = allocate.Invoke(rootScope, [owner1, loweringSite]) as string
            ?? throw new InvalidOperationException("无法为 owner1 重新分配名称。");
        var second = allocate.Invoke(rootScope, [owner2, loweringSite]) as string
            ?? throw new InvalidOperationException("无法为 owner2 分配名称。");

        var expectedPrimary = createName.Invoke(session, [loweringSite, scopeKey, owner1, "p"]) as string
            ?? throw new InvalidOperationException("无法生成 owner1 主名称。");
        var expectedFallback = createName.Invoke(session, [loweringSite, scopeKey, owner2, "f1"]) as string
            ?? throw new InvalidOperationException("无法生成 owner2 回退名称。");

        Assert.AreEqual(expectedPrimary, first);
        Assert.AreEqual(first, firstAgain);
        Assert.AreEqual(expectedFallback, second);
        Assert.AreNotEqual(first, second);
    }

    [TestMethod]
    public void Allocate_DistinguishesExactTupleSlotPathsEvenWhenLegacyHashedIndicesCollide()
    {
        var block = GetBlockOperation("""
            class TestClass
            {
                void TestMethod()
                {
                    int value = 1;
                }
            }
            """);

        var rootSite = InvokeStaticFactory(ScopeSiteType, "RootFragment");
        var legacyCollisionLeft = "16.8.20.31.15";
        var legacyCollisionRight = "29.29.7.10.25.9.3";
        var loweringSiteLeft = InvokeStaticFactory(LoweringSiteType, "TupleFieldCache", legacyCollisionLeft);
        var loweringSiteRight = InvokeStaticFactory(LoweringSiteType, "TupleFieldCache", legacyCollisionRight);
        var session = Activator.CreateInstance(
            UniqueNameSessionType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args: [block, rootSite],
            culture: null) ?? throw new InvalidOperationException("无法创建 UniqueNameSession。");
        var rootScope = UniqueNameSessionType
            .GetProperty("RootScope", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .GetValue(session)
            ?? throw new InvalidOperationException("无法读取 RootScope。");
        var scopeKey = EmissionScopeContextType
            .GetProperty("ScopeKey", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .GetValue(rootScope) as string
            ?? throw new InvalidOperationException("无法读取 ScopeKey。");
        var owner = Activator.CreateInstance(
            LoweringNameOwnerType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args: ["shared-slot-owner", "slot-owner"],
            culture: null) ?? throw new InvalidOperationException("无法创建 LoweringNameOwner(slot-owner)。");

        var allocate = EmissionScopeContextType.GetMethod(
            "Allocate",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: [LoweringNameOwnerType, LoweringSiteType],
            modifiers: null)
            ?? throw new InvalidOperationException("无法调用 Allocate(LoweringNameOwner, LoweringSite)。");
        var createName = UniqueNameSessionType.GetMethod(
            "CreateName",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: [LoweringSiteType, typeof(string), LoweringNameOwnerType, typeof(string)],
            modifiers: null)
            ?? throw new InvalidOperationException("无法调用 CreateName(LoweringSite, string, LoweringNameOwner, string)。");

        var left = allocate.Invoke(rootScope, [owner, loweringSiteLeft]) as string
            ?? throw new InvalidOperationException("无法为左侧 slot path 分配名称。");
        var right = allocate.Invoke(rootScope, [owner, loweringSiteRight]) as string
            ?? throw new InvalidOperationException("无法为右侧 slot path 分配名称。");

        var expectedLeft = createName.Invoke(session, [loweringSiteLeft, scopeKey, owner, "p"]) as string
            ?? throw new InvalidOperationException("无法生成左侧 slot path 名称。");
        var expectedRight = createName.Invoke(session, [loweringSiteRight, scopeKey, owner, "p"]) as string
            ?? throw new InvalidOperationException("无法生成右侧 slot path 名称。");

        Assert.AreEqual(expectedLeft, left);
        Assert.AreEqual(expectedRight, right);
        Assert.AreNotEqual(left, right);
    }

    [TestMethod]
    public void Visit_ReusedTestWalker_ResetsAliasMapPerTopLevelVisit()
    {
        var first = GetBlockOperation("""
            class TestClass
            {
                void TestMethod()
                {
                    int value = 1;
                    var result = value switch
                    {
                        1 => 10,
                        _ => 0
                    };
                }
            }
            """);
        var second = GetBlockOperation("""
            class TestClass
            {
                void TestMethod()
                {
                    int other = 2;
                    var result = other switch
                    {
                        1 => 10,
                        _ => 0
                    };
                }
            }
            """);

        var walker = new SemanticWalker(true);
        var firstScript = walker.Visit(first, new())?.ToKnRECMAScript();
        var secondScript = walker.Visit(second, new())?.ToKnRECMAScript();

        StringAssert.Contains(firstScript, "const v$0 = value;");
        StringAssert.Contains(secondScript, "const v$0 = other;");
    }

    [TestMethod]
    public void Visit_ObjectInitializerExpressionStatement_StableAcrossEarlierSameCtorExpressionStatement()
    {
        var baseBlock = GetBlockOperation("""
            class Box
            {
                public int Value { get; set; }
            }

            class TestClass
            {
                void TestMethod(int value)
                {
                    _ = value;
                    new Box { Value = value };
                }
            }
            """);
        var insertedBlock = GetBlockOperation("""
            class Box
            {
                public int Value { get; set; }
            }

            class TestClass
            {
                void TestMethod(int value)
                {
                    _ = value;
                    new Box();
                    new Box { Value = value };
                }
            }
            """);

        var walker = new SemanticWalker();
        var baseScript = walker.Visit(baseBlock, new())?.ToKnRECMAScript();
        var insertedScript = walker.Visit(insertedBlock, new())?.ToKnRECMAScript();

        Assert.AreEqual(ExtractSingleGeneratedName(baseScript), ExtractSingleGeneratedName(insertedScript));
    }

    [TestMethod]
    public void Visit_OutInvocationExpressionStatement_StableAcrossEarlierSiblingInvocationWithSameMethod()
    {
        var baseBlock = GetBlockOperation("""
            class TestClass
            {
                void TestMethod(string a, string b)
                {
                    int.TryParse(a, out int x);
                }
            }
            """);
        var insertedBlock = GetBlockOperation("""
            class TestClass
            {
                void TestMethod(string a, string b)
                {
                    int.TryParse(b, out int y);
                    int.TryParse(a, out int x);
                }
            }
            """);

        var walker = new SemanticWalker();
        var baseScript = walker.Visit(baseBlock, new())?.ToKnRECMAScript();
        var insertedScript = walker.Visit(insertedBlock, new())?.ToKnRECMAScript();

        Assert.AreEqual(ExtractSingleGeneratedName(baseScript), ExtractLastGeneratedName(insertedScript));
    }

    [TestMethod]
    public void Visit_ObjectInitializerExpressionStatement_StableAcrossEarlierSiblingInitializerWithDifferentValueSource()
    {
        var baseBlock = GetBlockOperation("""
            class Box
            {
                public int Value { get; set; }
            }

            class TestClass
            {
                void TestMethod(int value, int seed)
                {
                    _ = seed;
                    new Box { Value = value };
                }
            }
            """);
        var insertedBlock = GetBlockOperation("""
            class Box
            {
                public int Value { get; set; }
            }

            class TestClass
            {
                void TestMethod(int value, int seed)
                {
                    new Box { Value = seed };
                    new Box { Value = value };
                }
            }
            """);

        var walker = new SemanticWalker();
        var baseScript = walker.Visit(baseBlock, new())?.ToKnRECMAScript();
        var insertedScript = walker.Visit(insertedBlock, new())?.ToKnRECMAScript();

        Assert.AreEqual(ExtractSingleGeneratedName(baseScript), ExtractLastGeneratedName(insertedScript));
    }

    [TestMethod]
    public void Visit_TupleBinaryExpressionStatement_StableAcrossEarlierSiblingTupleBinaryWithDifferentOperand()
    {
        var baseBlock = GetBlockOperation("""
            class TestClass
            {
                void TestMethod(int a, int b)
                {
                    _ = Make(a) == (0, 1);
                }

                (int, int) Make(int x) => (x, 1);
            }
            """);
        var insertedBlock = GetBlockOperation("""
            class TestClass
            {
                void TestMethod(int a, int b)
                {
                    _ = Make(b) == (0, 1);
                    _ = Make(a) == (0, 1);
                }

                (int, int) Make(int x) => (x, 1);
            }
            """);

        var walker = new SemanticWalker();
        var baseScript = walker.Visit(baseBlock, new())?.ToKnRECMAScript();
        var insertedScript = walker.Visit(insertedBlock, new())?.ToKnRECMAScript();

        Assert.AreEqual(ExtractSingleGeneratedName(baseScript), ExtractLastGeneratedName(insertedScript));
    }

    [TestMethod]
    public void Visit_CustomDeconstructAssignment_StableAcrossEarlierSiblingInvocationWithDifferentSource()
    {
        var baseBlock = GetBlockOperation("""
            class TestClass
            {
                void TestMethod(int a, int b)
                {
                    int x = 0;
                    int y = 0;
                    (x, y) = Make(a);
                }

                PairLike Make(int value) => new PairLike(value);
            }

            class PairLike
            {
                private readonly int _value;

                public PairLike(int value)
                {
                    _value = value;
                }

                public void Deconstruct(out int left, out int right)
                {
                    left = _value;
                    right = _value + 1;
                }
            }
            """);
        var insertedBlock = GetBlockOperation("""
            class TestClass
            {
                void TestMethod(int a, int b)
                {
                    int x = 0;
                    int y = 0;
                    (x, y) = Make(b);
                    (x, y) = Make(a);
                }

                PairLike Make(int value) => new PairLike(value);
            }

            class PairLike
            {
                private readonly int _value;

                public PairLike(int value)
                {
                    _value = value;
                }

                public void Deconstruct(out int left, out int right)
                {
                    left = _value;
                    right = _value + 1;
                }
            }
            """);

        var walker = new SemanticWalker();
        var baseScript = walker.Visit(baseBlock, new())?.ToKnRECMAScript();
        var insertedScript = walker.Visit(insertedBlock, new())?.ToKnRECMAScript();

        Assert.AreEqual(ExtractSingleGeneratedName(baseScript), ExtractLastGeneratedName(insertedScript));
    }

    [TestMethod]
    public void Visit_TupleFieldCache_StableAcrossEarlierSiblingDeconstructionWithSameTarget()
    {
        var baseBlock = GetBlockOperation("""
            class TestClass
            {
                void TestMethod(int a, int b, int seed)
                {
                    int x = a;
                    int y = b;
                    (x, y) = (y, x);
                }
            }
            """);
        var insertedBlock = GetBlockOperation("""
            class TestClass
            {
                void TestMethod(int a, int b, int seed)
                {
                    int x = a;
                    int y = b;
                    (x, y) = (seed, x);
                    (x, y) = (y, x);
                }
            }
            """);

        var walker = new SemanticWalker();
        var baseScript = walker.Visit(baseBlock, new())?.ToKnRECMAScript();
        var insertedScript = walker.Visit(insertedBlock, new())?.ToKnRECMAScript();

        var baseNames = ExtractGeneratedNames(baseScript);
        var insertedNames = ExtractGeneratedNames(insertedScript);
        var actualNames = insertedNames.Skip(insertedNames.Length - baseNames.Length).ToArray();

        Assert.AreEqual(string.Join(",", baseNames), string.Join(",", actualNames));
    }

    [TestMethod]
    public void Visit_TupleDeconstructionSource_StableAcrossEarlierSiblingSwitchExpressionWithDifferentRelationalPattern()
    {
        var baseBlock = GetBlockOperation("""
            class TestClass
            {
                void TestMethod(int value)
                {
                    int left = 0;
                    int right = 0;
                    (left, right) = value switch
                    {
                        > 0 => (1, 2),
                        _ => (3, 4)
                    };
                }
            }
            """);
        var insertedBlock = GetBlockOperation("""
            class TestClass
            {
                void TestMethod(int value)
                {
                    int left = 0;
                    int right = 0;
                    (left, right) = value switch
                    {
                        < 0 => (1, 2),
                        _ => (3, 4)
                    };
                    (left, right) = value switch
                    {
                        > 0 => (1, 2),
                        _ => (3, 4)
                    };
                }
            }
            """);

        var walker = new SemanticWalker();
        var baseScript = walker.Visit(baseBlock, new())?.ToKnRECMAScript();
        var insertedScript = walker.Visit(insertedBlock, new())?.ToKnRECMAScript();

        Assert.AreEqual(
            ExtractSingleGeneratedName(baseScript, "__tdecon$"),
            ExtractLastGeneratedName(insertedScript, "__tdecon$"));
    }

    [TestMethod]
    public void Visit_TupleDeconstructionSource_StableAcrossEarlierSiblingIndexerTupleSourceWithDifferentIndexArgument()
    {
        var baseBlock = GetBlockOperation("""
            class TestClass
            {
                void TestMethod(System.Collections.Generic.Dictionary<int, (int left, int right)> map, int a, int b)
                {
                    int left = 0;
                    int right = 0;
                    (left, right) = map[a];
                }
            }
            """);
        var insertedBlock = GetBlockOperation("""
            class TestClass
            {
                void TestMethod(System.Collections.Generic.Dictionary<int, (int left, int right)> map, int a, int b)
                {
                    int left = 0;
                    int right = 0;
                    (left, right) = map[b];
                    (left, right) = map[a];
                }
            }
            """);

        var walker = new SemanticWalker();
        var baseScript = walker.Visit(baseBlock, new())?.ToKnRECMAScript();
        var insertedScript = walker.Visit(insertedBlock, new())?.ToKnRECMAScript();

        Assert.AreEqual(
            ExtractSingleGeneratedName(baseScript, "__tdecon$"),
            ExtractLastGeneratedName(insertedScript, "__tdecon$"));
    }

    [TestMethod]
    public void Visit_TupleDeconstructionSource_StableAcrossEarlierSiblingIndexerTupleSourceWithDifferentIncrementShape()
    {
        var baseBlock = GetBlockOperation("""
            class TestClass
            {
                void TestMethod(System.Collections.Generic.Dictionary<int, (int left, int right)> map, int i)
                {
                    int left = 0;
                    int right = 0;
                    (left, right) = map[i++];
                }
            }
            """);
        var insertedBlock = GetBlockOperation("""
            class TestClass
            {
                void TestMethod(System.Collections.Generic.Dictionary<int, (int left, int right)> map, int i)
                {
                    int left = 0;
                    int right = 0;
                    (left, right) = map[++i];
                    (left, right) = map[i++];
                }
            }
            """);

        var walker = new SemanticWalker();
        var baseScript = walker.Visit(baseBlock, new())?.ToKnRECMAScript();
        var insertedScript = walker.Visit(insertedBlock, new())?.ToKnRECMAScript();

        Assert.AreEqual(
            ExtractSingleGeneratedName(baseScript, "__tdecon$"),
            ExtractLastGeneratedName(insertedScript, "__tdecon$"));
    }

    [TestMethod]
    public void Visit_SwitchExpressionInput_StableAcrossEarlierSiblingCompoundAssignmentWithDifferentOperator()
    {
        var baseBlock = GetBlockOperation("""
            class TestClass
            {
                int TestMethod(int value)
                {
                    return (value += 1) switch
                    {
                        2 => 10,
                        _ => 0
                    };
                }
            }
            """);
        var insertedBlock = GetBlockOperation("""
            class TestClass
            {
                int TestMethod(int value)
                {
                    _ = (value -= 1) switch
                    {
                        0 => 10,
                        _ => 0
                    };

                    return (value += 1) switch
                    {
                        2 => 10,
                        _ => 0
                    };
                }
            }
            """);

        var walker = new SemanticWalker();
        var baseScript = walker.Visit(baseBlock, new())?.ToKnRECMAScript();
        var insertedScript = walker.Visit(insertedBlock, new())?.ToKnRECMAScript();

        Assert.AreEqual(
            ExtractSingleGeneratedName(baseScript, "__swexpr$"),
            ExtractLastGeneratedName(insertedScript, "__swexpr$"));
    }

    [TestMethod]
    public void Visit_TupleDeconstructionSource_StableAcrossEarlierSiblingInvocationWithDifferentLambdaArgument()
    {
        var baseBlock = GetBlockOperation("""
            class TestClass
            {
                void TestMethod(int value)
                {
                    int left = 0;
                    int right = 0;
                    (left, right) = Make(value, x => x + 1);
                }

                (int left, int right) Make(int value, System.Func<int, int> map)
                    => (map(value), value);
            }
            """);
        var insertedBlock = GetBlockOperation("""
            class TestClass
            {
                void TestMethod(int value)
                {
                    int left = 0;
                    int right = 0;
                    _ = Make(value, x => x - 1);
                    (left, right) = Make(value, x => x + 1);
                }

                (int left, int right) Make(int value, System.Func<int, int> map)
                    => (map(value), value);
            }
            """);

        var walker = new SemanticWalker();
        var baseScript = walker.Visit(baseBlock, new())?.ToKnRECMAScript();
        var insertedScript = walker.Visit(insertedBlock, new())?.ToKnRECMAScript();

        Assert.AreEqual(
            ExtractSingleGeneratedName(baseScript, "__tdecon$"),
            ExtractLastGeneratedName(insertedScript, "__tdecon$"));
    }

    [TestMethod]
    public void Visit_MethodReferenceProxy_StableAcrossEarlierSiblingMethodReferenceWithSameDelegateType()
    {
        var baseBlock = GetBlockOperation("""
            class TestClass
            {
                void TestMethod()
                {
                    Func<object, string> typeOf = ECMAScript.Global.TypeOf;
                }

                string Describe(object value) => value?.ToString() ?? string.Empty;
            }
            """);
        var insertedBlock = GetBlockOperation("""
            class TestClass
            {
                void TestMethod()
                {
                    Func<object, string> describe = Describe;
                    Func<object, string> typeOf = ECMAScript.Global.TypeOf;
                }

                string Describe(object value) => value?.ToString() ?? string.Empty;
            }
            """);

        var walker = new SemanticWalker();
        var baseScript = walker.Visit(baseBlock, new())?.ToKnRECMAScript();
        var insertedScript = walker.Visit(insertedBlock, new())?.ToKnRECMAScript();

        Assert.AreEqual(ExtractSingleGeneratedName(baseScript), ExtractSingleGeneratedName(insertedScript));
    }

    [TestMethod]
    public void Visit_MethodReferenceProxy_StableAcrossEarlierSiblingStaticMethodReferenceWithSameDelegateType()
    {
        var baseBlock = GetBlockOperation("""
            class TestClass
            {
                void TestMethod()
                {
                    Func<object, string> typeOf = ECMAScript.Global.TypeOf;
                }

                static string Describe(object value) => value?.ToString() ?? string.Empty;
            }
            """);
        var insertedBlock = GetBlockOperation("""
            class TestClass
            {
                void TestMethod()
                {
                    Func<object, string> describe = Describe;
                    Func<object, string> typeOf = ECMAScript.Global.TypeOf;
                }

                static string Describe(object value) => value?.ToString() ?? string.Empty;
            }
            """);

        var walker = new SemanticWalker();
        var baseScript = walker.Visit(baseBlock, new())?.ToKnRECMAScript();
        var insertedScript = walker.Visit(insertedBlock, new())?.ToKnRECMAScript();

        Assert.AreEqual(ExtractSingleGeneratedName(baseScript), ExtractSingleGeneratedName(insertedScript));
    }

    private static object InvokeStaticFactory(Type type, string methodName, params object[] args)
    {
        var parameterTypes = args.Select(static arg => arg.GetType()).ToArray();
        var method = type.GetMethod(
            methodName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
            binder: null,
            types: parameterTypes,
            modifiers: null);
        return method?.Invoke(null, args)
            ?? throw new InvalidOperationException($"无法调用 {type.FullName}.{methodName}。");
    }

    private static string[] ExtractGeneratedNames(string? script)
    {
        Assert.IsNotNull(script);
        return GeneratedNameRegex.Matches(script)
            .Select(static match => match.Value)
            .Distinct()
            .ToArray();
    }

    private static string[] ExtractGeneratedNames(string? script, string prefix)
        => ExtractGeneratedNames(script)
            .Where(name => name.StartsWith(prefix, StringComparison.Ordinal))
            .ToArray();

    private static string ExtractLastGeneratedName(string? script)
    {
        var matches = ExtractGeneratedNames(script);
        Assert.IsTrue(matches.Length > 0, $"Expected at least one generated unique name. Script:{Environment.NewLine}{script}");
        return matches[^1];
    }

    private static string ExtractLastGeneratedName(string? script, string prefix)
    {
        var matches = ExtractGeneratedNames(script, prefix);
        Assert.IsTrue(matches.Length > 0, $"Expected at least one generated unique name with prefix '{prefix}'. Script:{Environment.NewLine}{script}");
        return matches[^1];
    }

    private static string ExtractSingleGeneratedName(string? script)
    {
        var matches = ExtractGeneratedNames(script);
        Assert.AreEqual(1, matches.Length, $"Expected exactly one generated unique name, got {matches.Length}. Script:{Environment.NewLine}{script}");
        return matches[0];
    }

    private static string ExtractSingleGeneratedName(string? script, string prefix)
    {
        var matches = ExtractGeneratedNames(script, prefix);
        Assert.AreEqual(1, matches.Length, $"Expected exactly one generated unique name with prefix '{prefix}', got {matches.Length}. Script:{Environment.NewLine}{script}");
        return matches[0];
    }
}
