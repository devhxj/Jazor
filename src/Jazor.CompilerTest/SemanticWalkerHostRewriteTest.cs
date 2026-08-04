using Acornima.Ast;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerHostRewriteTest
{
    [TestMethod]
    public void RewriteLocalDeclarationIdentifier_RewritesDeclarationPatternDeclarationAndReferences()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod()
                {
                    object value = 42;
                    if (value is int props)
                    {
                        Console.WriteLine(props);
                    }
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new LocalAliasHost("props", "__alias")
        };
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "let __alias;", StringComparison.Ordinal);
        StringAssert.Contains(script!, "(__alias = value, true)", StringComparison.Ordinal);
        StringAssert.Contains(script!, "console.log(__alias);", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("let props", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("console.log(props)", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteLocalDeclarationIdentifier_RewritesRecursivePatternDeclarationAndReferences()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod()
                {
                    object value = 42;
                    if (value is int { } props)
                    {
                        Console.WriteLine(props);
                    }
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new LocalAliasHost("props", "__alias")
        };
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "let __alias;", StringComparison.Ordinal);
        StringAssert.Contains(script!, "(__alias = value, true)", StringComparison.Ordinal);
        StringAssert.Contains(script!, "console.log(__alias);", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("let props", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("console.log(props)", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteLocalDeclarationIdentifier_RewritesListPatternDeclarationAndReferences()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod()
                {
                    int[] value = [1, 2, 3];
                    if (value is [1, ..] props)
                    {
                        Console.WriteLine(props.Length);
                    }
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new LocalAliasHost("props", "__alias")
        };
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "let __alias;", StringComparison.Ordinal);
        StringAssert.Contains(script!, "(__alias = value, true)", StringComparison.Ordinal);
        StringAssert.Contains(script!, "console.log(__alias.length);", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("let props", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("console.log(props", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteLocalDeclarationIdentifier_RewritesOutDeclarationDeclarationAndReferences()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod()
                {
                    if (int.TryParse("42", out var props))
                    {
                        Console.WriteLine(props);
                    }
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new LocalAliasHost("props", "__alias")
        };
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "let __alias", StringComparison.Ordinal);
        StringAssert.Contains(script!, "__alias = ", StringComparison.Ordinal);
        StringAssert.Contains(script!, "console.log(__alias);", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("let props", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("console.log(props)", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteLocalDeclarationIdentifier_RewritesOutDeclarationExpressionWithoutReferenceRewrite()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod()
                {
                    if (int.TryParse("42", out var props))
                    {
                        Console.WriteLine("ok");
                    }
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new DeclarationOnlyAliasHost("props", "__alias")
        };
        var node = walker.Visit(block, new());
        var script = node?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "let __alias", StringComparison.Ordinal);
        StringAssert.Contains(script!, "(\"42\", undefined)", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("let props", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("\"42\", props", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteObjectCreation_WhenSelectedHostDeclines_UsesStandardCreationLowering()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                sealed class NestedValue;

                void TestMethod()
                {
                    var value = new NestedValue();
                }
            }
            """);
        var host = new DecliningObjectCreationHost();
        var walker = new SemanticWalker(true) { Host = host };

        var script = walker.Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        Assert.AreEqual(1, host.RewriteCount);
        StringAssert.Contains(script, "let value = new NestedValue;", StringComparison.Ordinal);
    }

    [TestMethod]
    public void CompositeHost_RewriteHooksUseFirstHandlerThatClaimsOperation()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                int TestMethod()
                {
                    var value = 1;
                    return value;
                }
            }
            """);
        var first = new LocalReferenceAliasHost("value", "firstValue");
        var second = new LocalReferenceAliasHost("value", "secondValue");
        var walker = new SemanticWalker(true)
        {
            Host = new CompositeSemanticWalkerHost(first, second)
        };

        var script = walker.Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "return firstValue;", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("secondValue", StringComparison.Ordinal), script);
        Assert.AreEqual(1, first.RewriteCount);
        Assert.AreEqual(0, second.RewriteCount);
    }

    [TestMethod]
    public void CompositeHost_TypeObservationFansOutToEveryHost()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod()
                {
                    string value = "ready";
                }
            }
            """);
        var first = new TypeObservationHost();
        var second = new TypeObservationHost();
        var host = new CompositeSemanticWalkerHost(new PassiveSemanticWalkerHost(), first, second);

        host.ObserveTypeReference(block.Locals.Single().Type, new SenseArgument());

        Assert.AreEqual(1, first.ObservationCount);
        Assert.AreEqual(1, second.ObservationCount);
    }

    [TestMethod]
    public void CompositeHost_SkipHooksClaimWhenAnyHostRequestsSkip()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod()
                {
                    void Nested() { }
                    Nested();
                }
            }
            """);
        var localFunction = block.Operations.OfType<ILocalFunctionOperation>().Single();
        var first = new LocalFunctionSkipHost(false);
        var second = new LocalFunctionSkipHost(true);
        var host = new CompositeSemanticWalkerHost(new PassiveSemanticWalkerHost(), first, second);

        var shouldSkip = host.ShouldSkipLocalFunctionDeclaration(localFunction, new SenseArgument());

        Assert.IsTrue(shouldSkip);
        Assert.AreEqual(1, first.InvocationCount);
        Assert.AreEqual(1, second.InvocationCount);
    }

    [TestMethod]
    public void ShouldSkipVariableDeclarator_BlockSequenceOmitsClaimedTypeCarrierAndPreservesNeighbors()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod()
                {
                    Type childType = typeof(string);
                    int count = 2, next = count + 1;
                    Console.WriteLine(next);
                }
            }
            """);
        var skipHost = new VariableDeclaratorSkipHost("childType");
        var walker = new SemanticWalker(true)
        {
            Host = new CompositeSemanticWalkerHost(new PassiveSemanticWalkerHost(), skipHost)
        };

        var script = walker.Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        _ = new Acornima.Parser().ParseModule($"function test() {script}");
        Assert.IsFalse(script.Contains("childType", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("typeof", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "let count = 2, next = count + 1;", StringComparison.Ordinal);
        StringAssert.Contains(script, "console.log(next);", StringComparison.Ordinal);
        Assert.Contains("childType", skipHost.ClaimedSymbols);
    }

    [TestMethod]
    public void CompositeHost_UnclaimedHooksFanOutAcrossRepresentativeOperationPipeline()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                private int _field;

                int Value { get; set; }

                int TestMethod(object boxed, int input)
                {
                    var created = new NestedValue();
                    var converted = (int)boxed;
                    this._field = input;
                    Value = this._field;
                    Action<int> callback = Touch;
                    callback(Value);

                    try
                    {
                        Touch(input);
                    }
                    catch (Exception error)
                    {
                        input = error.Message.Length;
                    }

                    return converted + input;
                }

                void Touch(int value) { }

                sealed class NestedValue;
            }
            """);
        var first = new RecordingPassThroughHost(shouldRewriteObjectCreation: false);
        var second = new RecordingPassThroughHost(shouldRewriteObjectCreation: true);
        var walker = new SemanticWalker(true)
        {
            Host = new CompositeSemanticWalkerHost(new PassiveSemanticWalkerHost(), first, second)
        };

        var script = walker.Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        _ = new Acornima.Parser().ParseModule($"function test() {script}");
        HostHook[] expectedHooks =
        [
            HostHook.ConversionPreorder,
            HostHook.ObjectCreationPreorder,
            HostHook.ShouldRewriteObjectCreation,
            HostHook.RewriteObjectCreation,
            HostHook.CatchClauseParameterIdentifier,
            HostHook.SimpleAssignmentPreorder,
            HostHook.SimpleAssignmentPostorder,
            HostHook.ParameterReference,
            HostHook.InvocationPreorder,
            HostHook.InvocationArgumentPreorder,
            HostHook.FieldReference,
            HostHook.PropertyReference,
            HostHook.MethodReferencePreorder,
            HostHook.MethodReference,
            HostHook.Invocation,
            HostHook.InvocationIntrinsic,
            HostHook.InstanceReference
        ];
        foreach (var hook in expectedHooks)
        {
            Assert.Contains(hook, first.Calls, $"First host did not observe {hook}.");
            Assert.Contains(hook, second.Calls, $"Second host did not observe {hook}.");
        }
    }

    [TestMethod]
    public void RewriteSimpleAssignmentPostorder_ReceivesLoweredValueAndClaimsStorageTarget()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                private int _value;

                void TestMethod()
                {
                    _value = Next();
                }

                int Next() => 1;
            }
            """);
        var host = new PostorderAssignmentHost("_value");
        var walker = new SemanticWalker(true) { Host = host };

        var script = walker.Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        _ = new Acornima.Parser().ParseModule($"function test() {script}");
        Assert.AreEqual(1, host.RewriteCount);
        Assert.IsInstanceOfType<CallExpression>(host.LoweredValue);
        StringAssert.Contains(script, "hostValue = ", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("this._value =", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteMethodReference_PostorderHostReceivesTranslatedReceiverAndClaimsCallback()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod()
                {
                    Action callback = Touch;
                }

                void Touch() { }
            }
            """);
        var host = new MethodReferenceCallbackHost("Touch");
        var walker = new SemanticWalker(true) { Host = host };

        var script = walker.Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "let callback = hostCallback;", StringComparison.Ordinal);
        Assert.IsInstanceOfType<ThisExpression>(host.Receiver);
    }

    [TestMethod]
    public void RewriteInvocationIntrinsic_HostPrecedesCompilerOwnedInvocationLowering()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod()
                {
                    var value = int.Parse("42");
                }
            }
            """);
        var host = new ParseInvocationHost();
        var walker = new SemanticWalker(true) { Host = host };

        var script = walker.Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "let value = hostValue;", StringComparison.Ordinal);
        Assert.AreEqual(1, host.RewriteCount);
        Assert.IsNotNull(host.Argument);
        Assert.AreEqual(TypeMapper.Number, host.TypeMapping.Mapper);
        Assert.AreEqual("Number", host.TypeMapping.TypeName);
        Assert.AreEqual(TypeMapper.Boolean, host.BooleanTypeMapping.Mapper);
        Assert.AreEqual("Boolean", host.BooleanTypeMapping.TypeName);
        Assert.IsNull(host.ModuleImportPath);
        Assert.IsFalse(host.ImportedMemberResolved);
        Assert.IsNull(host.ImportedMember);
        CollectionAssert.Contains(host.TypeHierarchyNames, "Int32");
        Assert.IsNotNull(host.Diagnostic);
        Assert.AreEqual(OperationKind.Invocation, host.Diagnostic.Kind);
        Assert.AreEqual("host intrinsic probe", host.Diagnostic.Message);
        Assert.AreEqual("<unknown>", host.Diagnostic.Data["location.path"]);
    }

    [TestMethod]
    public void RewriteReferenceAndInvocation_PostorderHostComposesTranslatedInputs()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                private int _count;

                int Count { get; }

                int TestMethod(int input)
                {
                    var field = _count;
                    var property = Count;
                    return Add(input, property);
                }

                int Add(int left, int right) => left + right;
            }
            """);
        var host = new ReferenceAndInvocationHost();
        var walker = new SemanticWalker(true) { Host = host };

        var script = walker.Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "let field = hostField;", StringComparison.Ordinal);
        StringAssert.Contains(script, "let property = hostProperty;", StringComparison.Ordinal);
        StringAssert.Contains(script, "return hostCall;", StringComparison.Ordinal);
        Assert.IsInstanceOfType<ThisExpression>(host.FieldReceiver);
        Assert.IsInstanceOfType<ThisExpression>(host.PropertyReceiver);
        Assert.HasCount(2, host.InvocationArguments);
        Assert.IsInstanceOfType<Identifier>(host.InvocationArguments[0]);
        Assert.AreEqual("input", ((Identifier)host.InvocationArguments[0]).Name);
        Assert.IsInstanceOfType<Identifier>(host.InvocationArguments[1]);
        Assert.AreEqual("property", ((Identifier)host.InvocationArguments[1]).Name);
    }

    [TestMethod]
    public void RewriteParameterReference_HostProjectsClaimedParameterAndPreservesOtherParameters()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                int TestMethod(int count, int offset)
                {
                    return count + offset;
                }
            }
            """);
        var host = new ParameterReferenceProjectionHost("count");
        var walker = new SemanticWalker(true) { Host = host };

        var script = walker.Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        _ = new Acornima.Parser().ParseModule($"function test(count, offset) {script}");
        StringAssert.Contains(script, "return props.count + offset;", StringComparison.Ordinal);
        Assert.AreEqual(1, host.RewriteCount);
    }

    [TestMethod]
    public void RewriteMethodReferencePreorder_HostPrecedesCompilerOwnedCallbackLowering()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod()
                {
                    Func<string, int> parser = int.Parse;
                }
            }
            """);
        var host = new ParseMethodGroupHost();
        var walker = new SemanticWalker(true) { Host = host };

        var script = walker.Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "let parser = hostParser;", StringComparison.Ordinal);
        Assert.AreEqual(1, host.RewriteCount);
    }

    [TestMethod]
    public void RewriteInvocationArgumentPreorder_HostClaimsRawArgumentBeforeCoreTranslation()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void TestMethod()
                {
                    Consume(DateTime.UtcNow);
                }

                void Consume(DateTime timestamp) { }
            }
            """);
        var host = new InvocationArgumentHost();
        var walker = new SemanticWalker(true) { Host = host };

        var script = walker.Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "this.consume(hostTimestamp);", StringComparison.Ordinal);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(host.ArgumentValue);
        Assert.AreEqual("UtcNow", ((IPropertyReferenceOperation)host.ArgumentValue!).Property.Name);
    }

    [TestMethod]
    public void RewritePreorder_HostClaimsConversionAssignmentAndObjectCreationBeforeCoreLowering()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                private int _value;

                void TestMethod(object source)
                {
                    int converted = (int)source;
                    _value = converted;
                    var created = new NestedValue();
                }

                sealed class NestedValue;
            }
            """);
        var host = new PreorderClaimHost();
        var walker = new SemanticWalker(true) { Host = host };

        var script = walker.Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        _ = new Acornima.Parser().ParseModule($"function test(source) {script}");
        StringAssert.Contains(script, "let converted = hostConversion;", StringComparison.Ordinal);
        StringAssert.Contains(script, "hostAssignment;", StringComparison.Ordinal);
        StringAssert.Contains(script, "let created = hostCreation;", StringComparison.Ordinal);
        Assert.AreEqual(1, host.ConversionCount);
        Assert.AreEqual(1, host.AssignmentCount);
        Assert.AreEqual(1, host.CreationCount);
    }

    private static IBlockOperation GetBlockOperation(string code)
    {
        var usings =
            """
            global using System;
            global using ECMAScript;
            global using static ECMAScript.Global;
            """;

        var references = TestMetadataReferences.Net11
            .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location));
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(usings),
                CSharpSyntaxTree.ParseText(code)
            ],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        if (errors.Length > 0)
            throw new InvalidOperationException(string.Join("\n", errors.Select(static error => $"{error.Id}: {error.GetMessage()}")));

        var syntaxTree = compilation.SyntaxTrees.Last();
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var methodDeclaration = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(static method => method.Identifier.ValueText == "TestMethod");

        return semanticModel.GetOperation(methodDeclaration.Body!) as IBlockOperation
            ?? throw new InvalidOperationException("Method body operation was not available.");
    }

    private sealed class LocalAliasHost : SemanticWalkerHost
    {
        private readonly string _sourceName;
        private readonly string _alias;

        public LocalAliasHost(string sourceName, string alias)
        {
            _sourceName = sourceName;
            _alias = alias;
        }

        public override Identifier? RewriteLocalDeclarationIdentifier(ILocalSymbol local, IOperation operation, SenseArgument argument)
            => string.Equals(local.Name, _sourceName, StringComparison.Ordinal)
                ? new Identifier(_alias)
                : null;

        public override Expression? RewriteLocalReference(ILocalReferenceOperation operation, SenseArgument argument)
            => string.Equals(operation.Local.Name, _sourceName, StringComparison.Ordinal)
                ? new Identifier(_alias)
                : null;
    }

    private sealed class DeclarationOnlyAliasHost : SemanticWalkerHost
    {
        private readonly string _sourceName;
        private readonly string _alias;

        public DeclarationOnlyAliasHost(string sourceName, string alias)
        {
            _sourceName = sourceName;
            _alias = alias;
        }

        public override Identifier? RewriteLocalDeclarationIdentifier(ILocalSymbol local, IOperation operation, SenseArgument argument)
            => string.Equals(local.Name, _sourceName, StringComparison.Ordinal)
                ? new Identifier(_alias)
                : null;
    }

    private sealed class DecliningObjectCreationHost : SemanticWalkerHost
    {
        public int RewriteCount { get; private set; }

        public override bool ShouldRewriteObjectCreation(IObjectCreationOperation operation)
            => true;

        public override Expression? RewriteObjectCreation(
            IObjectCreationOperation operation,
            SenseArgument argument,
            IReadOnlyList<Expression> arguments)
        {
            RewriteCount++;
            return null;
        }
    }

    private sealed class LocalReferenceAliasHost : SemanticWalkerHost
    {
        private readonly string _sourceName;
        private readonly string _alias;

        public LocalReferenceAliasHost(string sourceName, string alias)
        {
            _sourceName = sourceName;
            _alias = alias;
        }

        public int RewriteCount { get; private set; }

        public override Expression? RewriteLocalReference(ILocalReferenceOperation operation, SenseArgument argument)
        {
            if (!string.Equals(operation.Local.Name, _sourceName, StringComparison.Ordinal))
                return null;

            RewriteCount++;
            return new Identifier(_alias);
        }
    }

    private sealed class ParameterReferenceProjectionHost(string parameterName) : SemanticWalkerHost
    {
        public int RewriteCount { get; private set; }

        public override Expression? RewriteParameterReference(IParameterReferenceOperation operation, SenseArgument argument)
        {
            if (!string.Equals(operation.Parameter.Name, parameterName, StringComparison.Ordinal))
                return null;

            RewriteCount++;
            return new MemberExpression(
                new Identifier("props"),
                new Identifier(operation.Parameter.Name),
                computed: false,
                optional: false);
        }
    }

    private sealed class TypeObservationHost : SemanticWalkerHost
    {
        public int ObservationCount { get; private set; }

        public override void ObserveTypeReference(ITypeSymbol type, SenseArgument argument)
            => ObservationCount++;
    }

    private sealed class LocalFunctionSkipHost(bool shouldSkip) : SemanticWalkerHost
    {
        public int InvocationCount { get; private set; }

        public override bool ShouldSkipLocalFunctionDeclaration(ILocalFunctionOperation operation, SenseArgument argument)
        {
            InvocationCount++;
            return shouldSkip;
        }
    }

    private sealed class VariableDeclaratorSkipHost(string symbolName) : SemanticWalkerHost
    {
        public HashSet<string> ClaimedSymbols { get; } = [];

        public override bool ShouldSkipVariableDeclarator(
            IVariableDeclaratorOperation operation,
            SenseArgument argument)
        {
            if (!string.Equals(operation.Symbol.Name, symbolName, StringComparison.Ordinal))
                return false;

            ClaimedSymbols.Add(operation.Symbol.Name);
            return true;
        }
    }

    private sealed class ParseInvocationHost : SemanticWalkerHost
    {
        public int RewriteCount { get; private set; }

        public SenseArgument? Argument { get; private set; }

        public SemanticTypeMapping TypeMapping { get; private set; }

        public SemanticTypeMapping BooleanTypeMapping { get; private set; }

        public string? ModuleImportPath { get; private set; }

        public bool ImportedMemberResolved { get; private set; }

        public Expression? ImportedMember { get; private set; }

        public string[] TypeHierarchyNames { get; private set; } = [];

        public OperationTransformationException? Diagnostic { get; private set; }

        public override Expression? RewriteInvocationIntrinsic(
            IInvocationOperation operation,
            Expression? instance,
            IReadOnlyList<Expression> arguments,
            SemanticInvocationLoweringContext context)
        {
            if (!string.Equals(operation.TargetMethod.Name, "Parse", StringComparison.Ordinal))
                return null;

            RewriteCount++;
            Argument = context.Argument;
            TypeMapping = context.GetTypeMapping(operation.TargetMethod.ContainingType);
            BooleanTypeMapping = context.GetTypeMapping(
                operation.SemanticModel!.Compilation.GetSpecialType(SpecialType.System_Boolean));
            ModuleImportPath = context.GetModuleImportPath(operation.TargetMethod.ContainingType);
            ImportedMemberResolved = context.TryBuildImportedModuleMember(
                operation.TargetMethod.ContainingType,
                operation.TargetMethod.Name,
                out var importedMember);
            ImportedMember = importedMember;
            TypeHierarchyNames = context
                .EnumerateNamedTypeHierarchyBaseFirst(operation.TargetMethod.ContainingType)
                .Select(static type => type.Name)
                .ToArray();
            Diagnostic = context.CreateException(operation, "host intrinsic probe");
            return new Identifier("hostValue");
        }
    }

    private sealed class MethodReferenceCallbackHost(string methodName) : SemanticWalkerHost
    {
        public Expression? Receiver { get; private set; }

        public override Expression? RewriteMethodReference(
            IMethodReferenceOperation operation,
            SenseArgument argument,
            Expression? instance)
        {
            if (!string.Equals(operation.Method.Name, methodName, StringComparison.Ordinal))
                return null;

            Receiver = instance;
            return new Identifier("hostCallback");
        }
    }

    private sealed class PostorderAssignmentHost(string fieldName) : SemanticWalkerHost
    {
        public int RewriteCount { get; private set; }

        public Expression? LoweredValue { get; private set; }

        public override Expression? RewriteSimpleAssignmentPostorder(
            ISimpleAssignmentOperation operation,
            SenseArgument argument,
            Expression value)
        {
            if (operation.Target is not IFieldReferenceOperation fieldReference ||
                !string.Equals(fieldReference.Field.Name, fieldName, StringComparison.Ordinal))
            {
                return null;
            }

            RewriteCount++;
            LoweredValue = value;
            return new AssignmentExpression(Acornima.Operator.Assignment, new Identifier("hostValue"), value);
        }
    }

    private sealed class ReferenceAndInvocationHost : SemanticWalkerHost
    {
        public Expression? FieldReceiver { get; private set; }

        public Expression? PropertyReceiver { get; private set; }

        public IReadOnlyList<Expression> InvocationArguments { get; private set; } = [];

        public override Expression? RewriteFieldReference(
            IFieldReferenceOperation operation,
            SenseArgument argument,
            Expression? instance)
        {
            if (!string.Equals(operation.Field.Name, "_count", StringComparison.Ordinal))
                return null;

            FieldReceiver = instance;
            return new Identifier("hostField");
        }

        public override Expression? RewritePropertyReference(
            IPropertyReferenceOperation operation,
            SenseArgument argument,
            Expression? instance,
            IReadOnlyList<Expression> arguments)
        {
            if (!string.Equals(operation.Property.Name, "Count", StringComparison.Ordinal))
                return null;

            PropertyReceiver = instance;
            Assert.IsEmpty(arguments);
            return new Identifier("hostProperty");
        }

        public override Expression? RewriteInvocation(
            IInvocationOperation operation,
            SenseArgument argument,
            Expression? instance,
            IReadOnlyList<Expression> arguments)
        {
            if (!string.Equals(operation.TargetMethod.Name, "Add", StringComparison.Ordinal))
                return null;

            InvocationArguments = arguments;
            return new Identifier("hostCall");
        }
    }

    private sealed class ParseMethodGroupHost : SemanticWalkerHost
    {
        public int RewriteCount { get; private set; }

        public override Expression? RewriteMethodReferencePreorder(
            IMethodReferenceOperation operation,
            SenseArgument argument)
        {
            if (!string.Equals(operation.Method.Name, "Parse", StringComparison.Ordinal))
                return null;

            RewriteCount++;
            return new Identifier("hostParser");
        }
    }

    private sealed class InvocationArgumentHost : SemanticWalkerHost
    {
        public IOperation? ArgumentValue { get; private set; }

        public override Expression? RewriteInvocationArgumentPreorder(
            IInvocationOperation operation,
            IArgumentOperation argumentOperation,
            int argumentIndex,
            SenseArgument argument)
        {
            if (!string.Equals(operation.TargetMethod.Name, "Consume", StringComparison.Ordinal) || argumentIndex != 0)
                return null;

            ArgumentValue = argumentOperation.Value;
            return new Identifier("hostTimestamp");
        }
    }

    private sealed class PreorderClaimHost : SemanticWalkerHost
    {
        public int ConversionCount { get; private set; }

        public int AssignmentCount { get; private set; }

        public int CreationCount { get; private set; }

        public override Expression? RewriteConversionPreorder(IConversionOperation operation, SenseArgument argument)
        {
            if (operation.Type?.SpecialType != SpecialType.System_Int32 ||
                operation.Operand.Type?.SpecialType != SpecialType.System_Object)
            {
                return null;
            }

            ConversionCount++;
            return new Identifier("hostConversion");
        }

        public override Expression? RewriteSimpleAssignmentPreorder(
            ISimpleAssignmentOperation operation,
            SenseArgument argument)
        {
            if (operation.Target is not IFieldReferenceOperation { Field.Name: "_value" })
                return null;

            AssignmentCount++;
            return new Identifier("hostAssignment");
        }

        public override Expression? RewriteObjectCreationPreorder(IObjectCreationOperation operation, SenseArgument argument)
        {
            if (!string.Equals(operation.Type?.Name, "NestedValue", StringComparison.Ordinal))
                return null;

            CreationCount++;
            return new Identifier("hostCreation");
        }
    }

    private enum HostHook
    {
        ConversionPreorder,
        ObjectCreationPreorder,
        ShouldRewriteObjectCreation,
        RewriteObjectCreation,
        CatchClauseParameterIdentifier,
        SimpleAssignmentPreorder,
        SimpleAssignmentPostorder,
        ParameterReference,
        InvocationPreorder,
        InvocationArgumentPreorder,
        FieldReference,
        PropertyReference,
        MethodReferencePreorder,
        MethodReference,
        Invocation,
        InvocationIntrinsic,
        InstanceReference
    }

    private sealed class PassiveSemanticWalkerHost : SemanticWalkerHost;

    private sealed class RecordingPassThroughHost(bool shouldRewriteObjectCreation) : SemanticWalkerHost
    {
        public HashSet<HostHook> Calls { get; } = [];

        public override Expression? RewriteConversionPreorder(IConversionOperation operation, SenseArgument argument)
            => Record<Expression>(HostHook.ConversionPreorder);

        public override Expression? RewriteObjectCreationPreorder(IObjectCreationOperation operation, SenseArgument argument)
            => Record<Expression>(HostHook.ObjectCreationPreorder);

        public override bool ShouldRewriteObjectCreation(IObjectCreationOperation operation)
        {
            Calls.Add(HostHook.ShouldRewriteObjectCreation);
            return shouldRewriteObjectCreation;
        }

        public override Expression? RewriteObjectCreation(
            IObjectCreationOperation operation,
            SenseArgument argument,
            IReadOnlyList<Expression> arguments)
            => Record<Expression>(HostHook.RewriteObjectCreation);

        public override Identifier? RewriteCatchClauseParameterIdentifier(
            ICatchClauseOperation operation,
            ILocalSymbol local,
            SenseArgument argument)
            => Record<Identifier>(HostHook.CatchClauseParameterIdentifier);

        public override Expression? RewriteSimpleAssignmentPreorder(
            ISimpleAssignmentOperation operation,
            SenseArgument argument)
            => Record<Expression>(HostHook.SimpleAssignmentPreorder);

        public override Expression? RewriteSimpleAssignmentPostorder(
            ISimpleAssignmentOperation operation,
            SenseArgument argument,
            Expression value)
            => Record<Expression>(HostHook.SimpleAssignmentPostorder);

        public override Expression? RewriteParameterReference(
            IParameterReferenceOperation operation,
            SenseArgument argument)
            => Record<Expression>(HostHook.ParameterReference);

        public override Expression? RewriteInvocationPreorder(
            IInvocationOperation operation,
            SenseArgument argument)
            => Record<Expression>(HostHook.InvocationPreorder);

        public override Expression? RewriteInvocationArgumentPreorder(
            IInvocationOperation operation,
            IArgumentOperation argumentOperation,
            int argumentIndex,
            SenseArgument argument)
            => Record<Expression>(HostHook.InvocationArgumentPreorder);

        public override Expression? RewriteFieldReference(
            IFieldReferenceOperation operation,
            SenseArgument argument,
            Expression? instance)
            => Record<Expression>(HostHook.FieldReference);

        public override Expression? RewritePropertyReference(
            IPropertyReferenceOperation operation,
            SenseArgument argument,
            Expression? instance,
            IReadOnlyList<Expression> arguments)
            => Record<Expression>(HostHook.PropertyReference);

        public override Expression? RewriteMethodReferencePreorder(
            IMethodReferenceOperation operation,
            SenseArgument argument)
            => Record<Expression>(HostHook.MethodReferencePreorder);

        public override Expression? RewriteMethodReference(
            IMethodReferenceOperation operation,
            SenseArgument argument,
            Expression? instance)
            => Record<Expression>(HostHook.MethodReference);

        public override Expression? RewriteInvocation(
            IInvocationOperation operation,
            SenseArgument argument,
            Expression? instance,
            IReadOnlyList<Expression> arguments)
            => Record<Expression>(HostHook.Invocation);

        public override Expression? RewriteInvocationIntrinsic(
            IInvocationOperation operation,
            Expression? instance,
            IReadOnlyList<Expression> arguments,
            SemanticInvocationLoweringContext context)
            => Record<Expression>(HostHook.InvocationIntrinsic);

        public override Expression? RewriteInstanceReference(
            IInstanceReferenceOperation operation,
            SenseArgument argument)
            => Record<Expression>(HostHook.InstanceReference);

        private TNode? Record<TNode>(HostHook hook)
            where TNode : Acornima.Ast.Node
        {
            Calls.Add(hook);
            return null;
        }
    }
}
