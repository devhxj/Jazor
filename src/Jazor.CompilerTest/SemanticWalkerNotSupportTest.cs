using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Reflection;
using EsNode = Acornima.Ast.Node;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerNotSupportTest
{
    public static IEnumerable<TestDataRow<UnsupportedOperationCase>> UnsupportedHandlers
        => LoadNotSupportHandlers().Select(static testCase =>
            new TestDataRow<UnsupportedOperationCase>(testCase)
            {
                DisplayName = testCase.Id
            });

    private static IBlockOperation GetBlockOperation(string code, string methodName = "TestMethod")
    {
        var usings = @"
          global using System;
          global using System.Collections.Generic;
          global using System.Linq;
          global using System.Numerics;
          global using ECMAScript;
          global using static ECMAScript.Global;";

        var references = TestMetadataReferences.Net11
            .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location));
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: [
                CSharpSyntaxTree.ParseText(usings),
                CSharpSyntaxTree.ParseText(code)
            ],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var diagnostics = compilation.GetDiagnostics();
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        if (errors.Count > 0)
        {
            var errorMessages = string.Join("\n", errors.Select(e => $"{e.Id}: {e.GetMessage()}"));
            throw new InvalidOperationException(errorMessages);
        }

        var syntaxTree = compilation.SyntaxTrees.Last();
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var root = syntaxTree.GetRoot();
        var methodDeclaration = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(method => method.Identifier.ValueText == methodName && method.Body is not null)
            ?? root.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault(static method => method.Body is not null);
        if (methodDeclaration?.Body is not null &&
            semanticModel.GetOperation(methodDeclaration.Body) is IBlockOperation operation)
            return operation;

        throw new InvalidOperationException("未找到可分析的操作");
    }

    private static TOperation FindFirstOperation<TOperation>(IOperation root)
        where TOperation : class, IOperation
    {
        var stack = new Stack<IOperation>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var operation = stack.Pop();
            if (operation is TOperation matched)
                return matched;

            foreach (var child in operation.ChildOperations)
            {
                if (child is not null)
                    stack.Push(child);
            }
        }

        throw new InvalidOperationException($"未找到可分析的操作 {typeof(TOperation).Name}");
    }

    private class OperationDispatchProxy : DispatchProxy
    {
        public OperationKind Kind { get; set; } = OperationKind.None;

        public SyntaxNode Syntax { get; set; } = SyntaxFactory.ParseStatement("int x = 0;");

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod is null)
                return null;

            return targetMethod.Name switch
            {
                "get_Kind" => Kind,
                "get_Syntax" => Syntax,
                "get_ChildOperations" => Array.Empty<IOperation>(),
                _ => targetMethod.ReturnType == typeof(void)
                    ? null
                    : targetMethod.ReturnType.IsValueType
                        ? Activator.CreateInstance(targetMethod.ReturnType)
                        : null
            };
        }
    }

    private static IOperation CreateOperationProxy(Type operationInterface, OperationKind kind)
    {
        var createMethod = typeof(DispatchProxy)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(method => method.Name == nameof(DispatchProxy.Create) && method.IsGenericMethodDefinition);
        var genericCreate = createMethod.MakeGenericMethod(operationInterface, typeof(OperationDispatchProxy));
        var instance = genericCreate.Invoke(null, null)
            ?? throw new InvalidOperationException($"无法创建代理实例: {operationInterface.FullName}");
        var proxy = instance as OperationDispatchProxy
            ?? throw new InvalidOperationException($"代理实例转换失败: {operationInterface.FullName}");
        proxy.Kind = kind;
        return (IOperation)instance;
    }

    private static string ResolveNotSupportSourcePath()
    {
        var current = new System.IO.DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var solutionPath = Path.Combine(current.FullName, "Jazor.slnx");
            if (System.IO.File.Exists(solutionPath))
            {
                var sourcePath = Path.Combine(current.FullName, "src", "Jazor.Compiler", "core", "SemanticWalker.cs.NotSupport.cs");
                if (System.IO.File.Exists(sourcePath))
                    return sourcePath;

                throw new InvalidOperationException($"未找到 NotSupport 源文件: {sourcePath}");
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("未找到仓库根目录 Jazor.slnx。");
    }

    private static IReadOnlyList<UnsupportedOperationCase> LoadNotSupportHandlers()
    {
        var sourcePath = ResolveNotSupportSourcePath();
        var source = System.IO.File.ReadAllText(sourcePath);
        var root = CSharpSyntaxTree.ParseText(source).GetRoot();
        var methodNames = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(static method => method.Modifiers.Any(SyntaxKind.PublicKeyword))
            .Where(static method => method.Modifiers.Any(SyntaxKind.OverrideKeyword))
            .Select(static method => method.Identifier.ValueText)
            .Where(static methodName => methodName.StartsWith("Visit", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToArray();

        var visitorMethods = typeof(SemanticWalker)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(static method => method.Name.StartsWith("Visit", StringComparison.Ordinal))
            .Where(static method =>
            {
                var parameters = method.GetParameters();
                return parameters.Length == 2 &&
                    typeof(IOperation).IsAssignableFrom(parameters[0].ParameterType) &&
                    parameters[1].ParameterType == typeof(SenseArgument);
            })
            .ToLookup(static method => method.Name, StringComparer.Ordinal);

        var handlers = new List<UnsupportedOperationCase>(methodNames.Length);
        foreach (var methodName in methodNames)
        {
            var method = visitorMethods[methodName].SingleOrDefault()
                ?? throw new InvalidOperationException($"无法绑定拒绝处理器: {methodName}");
            var operationType = method.GetParameters()[0].ParameterType;
            handlers.Add(new UnsupportedOperationCase(
                Id: methodName + "_RejectsWithTransformationException",
                MethodName: methodName,
                OperationType: operationType));
        }

        return handlers;
    }

    private static void AssertUnsupportedDirect<TOperation>(
        IBlockOperation block,
        Func<IBlockOperation, TOperation> selector,
        Action<SemanticWalker, TOperation> visitor,
        string expectedMessage)
        where TOperation : class, IOperation
    {
        var operation = selector(block);
        var walker = new SemanticWalker(true);
        var exception = Assert.Throws<OperationTransformationException>(() => visitor(walker, operation));
        Assert.AreEqual(operation.Kind, exception.Kind);
        StringAssert.Contains(exception.Message ?? string.Empty, expectedMessage);
    }

    private static void AssertUnsupportedByDispatch(
        string code,
        OperationKind expectedKind,
        string expectedMessage,
        string methodName = "TestMethod")
    {
        var block = GetBlockOperation(code, methodName);
        var walker = new SemanticWalker(true);
        var exception = Assert.Throws<OperationTransformationException>(() => walker.Visit(block, new()));
        Assert.AreEqual(expectedKind, exception.Kind);
        StringAssert.Contains(exception.Message ?? string.Empty, expectedMessage);
    }

    [TestMethod]
    public void VisitDynamicObjectCreation_NotSupported()
    {
        const string code = """
            class TestClass
            {
                void TestMethod(dynamic value)
                {
                    var builder = new System.Text.StringBuilder(value);
                }
            }
            """;

        var block = GetBlockOperation(code);
        AssertUnsupportedDirect(
            block,
            static x => FindFirstOperation<IDynamicObjectCreationOperation>(x),
            static (walker, operation) => walker.VisitDynamicObjectCreation(operation, new()),
            "Dynamic object creation is not supported");

        AssertUnsupportedByDispatch(code, OperationKind.DynamicObjectCreation, "Dynamic object creation is not supported");
    }

    [TestMethod]
    public void VisitDynamicMemberReference_NotSupported()
    {
        const string code = """
            class TestClass
            {
                void TestMethod(dynamic source)
                {
                    var value = source.Name;
                }
            }
            """;

        var block = GetBlockOperation(code);
        AssertUnsupportedDirect(
            block,
            static x => FindFirstOperation<IDynamicMemberReferenceOperation>(x),
            static (walker, operation) => walker.VisitDynamicMemberReference(operation, new()),
            "Dynamic member references are not supported");

        AssertUnsupportedByDispatch(code, OperationKind.DynamicMemberReference, "Dynamic member references are not supported");
    }

    [TestMethod]
    public void VisitDynamicInvocation_NotSupported()
    {
        const string code = """
            class TestClass
            {
                void TestMethod(dynamic source)
                {
                    source.Run(1);
                }
            }
            """;

        var block = GetBlockOperation(code);
        AssertUnsupportedDirect(
            block,
            static x => FindFirstOperation<IDynamicInvocationOperation>(x),
            static (walker, operation) => walker.VisitDynamicInvocation(operation, new()),
            "Dynamic method invocations are not supported");

        AssertUnsupportedByDispatch(code, OperationKind.DynamicInvocation, "Dynamic method invocations are not supported");
    }

    [TestMethod]
    public void VisitDynamicIndexerAccess_NotSupported()
    {
        const string code = """
            class TestClass
            {
                void TestMethod(dynamic source)
                {
                    var value = source["name"];
                }
            }
            """;

        var block = GetBlockOperation(code);
        AssertUnsupportedDirect(
            block,
            static x => FindFirstOperation<IDynamicIndexerAccessOperation>(x),
            static (walker, operation) => walker.VisitDynamicIndexerAccess(operation, new()),
            "Dynamic indexer access is not supported");

        AssertUnsupportedByDispatch(code, OperationKind.DynamicIndexerAccess, "Dynamic indexer access is not supported");
    }

    [TestMethod]
    public void VisitTranslatedQuery_NotSupported()
    {
        const string code = """
            class TestClass
            {
                void TestMethod(int[] source)
                {
                    var query = from item in source where item > 0 select item;
                }
            }
            """;

        var block = GetBlockOperation(code);
        AssertUnsupportedDirect(
            block,
            static x => FindFirstOperation<ITranslatedQueryOperation>(x),
            static (walker, operation) => walker.VisitTranslatedQuery(operation, new()),
            "Translated LINQ queries are not supported");

        AssertUnsupportedByDispatch(code, OperationKind.TranslatedQuery, "Translated LINQ queries are not supported");
    }

    [TestMethod]
    public void VisitTypeOf_Record_NotSupported()
    {
        const string code = """
            class TestClass
            {
                record Person(string Name);

                void TestMethod()
                {
                    var type = typeof(Person);
                }
            }
            """;

        var block = GetBlockOperation(code);
        AssertUnsupportedDirect(
            block,
            static x => FindFirstOperation<ITypeOfOperation>(x),
            static (walker, operation) => walker.VisitTypeOf(operation, new()),
            "does not expose a stable runtime type token");

        AssertUnsupportedByDispatch(code, OperationKind.TypeOf, "does not expose a stable runtime type token");
    }

    [TestMethod]
    public void VisitTypeOf_Tuple_NotSupported()
    {
        const string code = """
            class TestClass
            {
                void TestMethod()
                {
                    var type = typeof((int X, int Y));
                }
            }
            """;

        var block = GetBlockOperation(code);
        AssertUnsupportedDirect(
            block,
            static x => FindFirstOperation<ITypeOfOperation>(x),
            static (walker, operation) => walker.VisitTypeOf(operation, new()),
            "does not expose a stable runtime type token");

        AssertUnsupportedByDispatch(code, OperationKind.TypeOf, "does not expose a stable runtime type token");
    }

    [TestMethod]
    public void VisitTypeOf_Interface_NotSupported()
    {
        const string code = """
            interface IService
            {
            }

            class TestClass
            {
                void TestMethod()
                {
                    var type = typeof(IService);
                }
            }
            """;

        var block = GetBlockOperation(code);
        AssertUnsupportedDirect(
            block,
            static x => FindFirstOperation<ITypeOfOperation>(x),
            static (walker, operation) => walker.VisitTypeOf(operation, new()),
            "does not expose a stable runtime type token");

        AssertUnsupportedByDispatch(code, OperationKind.TypeOf, "does not expose a stable runtime type token");
    }

    [TestMethod]
    public void VisitTypeOf_ErasedTypeParameter_NotSupported()
    {
        const string code = """
            class TestClass
            {
                void TestMethod<T>()
                {
                    var type = typeof(T);
                }
            }
            """;

        var block = GetBlockOperation(code);
        AssertUnsupportedDirect(
            block,
            static x => FindFirstOperation<ITypeOfOperation>(x),
            static (walker, operation) => walker.VisitTypeOf(operation, new()),
            "does not expose a stable runtime type token");

        AssertUnsupportedByDispatch(code, OperationKind.TypeOf, "does not expose a stable runtime type token");
    }

    [TestMethod]
    public void VisitTypeOf_DateTime_NotSupported()
    {
        const string code = """
            class TestClass
            {
                void TestMethod()
                {
                    var type = typeof(System.DateTime);
                }
            }
            """;

        var block = GetBlockOperation(code);
        AssertUnsupportedDirect(
            block,
            static x => FindFirstOperation<ITypeOfOperation>(x),
            static (walker, operation) => walker.VisitTypeOf(operation, new()),
            "does not expose a stable runtime type token");

        AssertUnsupportedByDispatch(code, OperationKind.TypeOf, "does not expose a stable runtime type token");
    }

    [TestMethod]
    public void VisitRangeOperation_NotSupported()
    {
        const string code = """
            class TestClass
            {
                void TestMethod()
                {
                    System.Range range = 1..3;
                }
            }
            """;

        var block = GetBlockOperation(code);
        AssertUnsupportedDirect(
            block,
            static x => FindFirstOperation<IRangeOperation>(x),
            static (walker, operation) => walker.VisitRangeOperation(operation, new()),
            "Standalone System.Range values are not supported");

        AssertUnsupportedByDispatch(code, OperationKind.Range, "Standalone System.Range values are not supported");
    }

    [TestMethod]
    public void Visit_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        const string code = """
            class TestClass
            {
                void TestMethod()
                {
                    int value = 1;
                }
            }
            """;

        var block = GetBlockOperation(code);
        using var cts = new System.Threading.CancellationTokenSource();
        cts.Cancel();

        var walker = new SemanticWalker(true, cts.Token);
        Assert.Throws<OperationCanceledException>(() => walker.Visit(block, new()));
    }

    [TestMethod]
    public void Visit_WhenReportIsNull_AttachesLocationMetadataToTransformationException()
    {
        const string code = """
            class TestClass
            {
                void TestMethod()
                {
                    System.Range range = 1..3;
                }
            }
            """;

        var block = GetBlockOperation(code);
        var walker = new SemanticWalker(true);
        var exception = Assert.Throws<OperationTransformationException>(() => walker.Visit(block, new()));
        Assert.AreEqual(OperationKind.Range, exception.Kind);

        var path = exception.Data["location.path"] as string;
        Assert.IsFalse(string.IsNullOrWhiteSpace(path));

        var startLineRaw = exception.Data["location.startLine"];
        Assert.IsTrue(startLineRaw is int);
        var startLineValue = (int)startLineRaw!;
        Assert.IsTrue(startLineValue >= 1);

        Assert.IsTrue(exception.Data["location.startColumn"] is int startColumn && startColumn >= 1);

        var endLineRaw = exception.Data["location.endLine"];
        Assert.IsTrue(endLineRaw is int);
        var endLineValue = (int)endLineRaw!;
        Assert.IsTrue(endLineValue >= startLineValue);

        Assert.IsTrue(exception.Data["location.endColumn"] is int endColumn && endColumn >= 1);
    }

    [TestMethod]
    [DynamicData(nameof(UnsupportedHandlers))]
    public void VisitNotSupportHandler_RejectsWithTransformationException(UnsupportedOperationCase testCase)
    {
        var walker = new SemanticWalker(true);
        var method = typeof(SemanticWalker).GetMethod(
            testCase.MethodName,
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            types: [testCase.OperationType, typeof(SenseArgument)],
            modifiers: null);
        Assert.IsNotNull(method, $"未找到处理器方法: {testCase.MethodName}({testCase.OperationType.Name})");

        var operation = CreateOperationProxy(testCase.OperationType, OperationKind.None);
        try
        {
            method.Invoke(walker, [operation, new SenseArgument()]);
            Assert.Fail($"处理器未拒绝: {testCase.MethodName}({testCase.OperationType.Name})");
        }
        catch (TargetInvocationException invocation) when (invocation.InnerException is OperationTransformationException exception)
        {
            Assert.AreEqual(OperationKind.None, exception.Kind, $"处理器未透传 operation.Kind: {testCase.MethodName}");
            var message = (exception.Message ?? string.Empty).ToLowerInvariant();
            StringAssert.Contains(message, "not supported");
        }
    }

    [TestMethod]
    public void UnsupportedHandlerCatalog_HasUniqueIdsMethodsAndOperationTypes()
    {
        var handlers = LoadNotSupportHandlers();
        Assert.HasCount(32, handlers);
        Assert.HasCount(handlers.Count, handlers.Select(static item => item.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(handlers.Count, handlers.Select(static item => item.MethodName).Distinct(StringComparer.Ordinal));
        Assert.HasCount(handlers.Count, handlers.Select(static item => item.OperationType).Distinct());
    }

    [TestMethod]
    public void SemanticWalker_CoversCurrentRoslynOperationVisitorSurface()
    {
        var semanticWalkerMethods = typeof(SemanticWalker)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(static method => method.Name.StartsWith("Visit", StringComparison.Ordinal))
            .Where(static method =>
            {
                var parameters = method.GetParameters();
                return parameters.Length == 2 &&
                    typeof(IOperation).IsAssignableFrom(parameters[0].ParameterType) &&
                    parameters[1].ParameterType == typeof(SenseArgument);
            })
            .Select(static method => (method.Name, OperationType: method.GetParameters()[0].ParameterType))
            .ToHashSet();

        var visitorMethods = typeof(OperationVisitor<SenseArgument, EsNode?>)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(static method => method.Name.StartsWith("Visit", StringComparison.Ordinal))
            .Where(static method =>
            {
                var parameters = method.GetParameters();
                return parameters.Length == 2 &&
                    typeof(IOperation).IsAssignableFrom(parameters[0].ParameterType) &&
                    parameters[1].ParameterType == typeof(SenseArgument);
            })
            .Where(static method => ShouldHaveExplicitSemanticWalkerCoverage(method.GetParameters()[0].ParameterType))
            .Select(static method => (method.Name, OperationType: method.GetParameters()[0].ParameterType))
            .ToArray();

        var missing = visitorMethods
            .Where(method => !semanticWalkerMethods.Contains(method))
            .Select(static method => $"{method.Name}({method.OperationType.Name})")
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.IsTrue(
            missing.Length == 0,
            "SemanticWalker must explicitly support or reject every concrete Roslyn IOperation visitor method. Missing: " + string.Join(", ", missing));
    }

    private static bool ShouldHaveExplicitSemanticWalkerCoverage(Type operationType)
    {
        if (operationType == typeof(IOperation))
            return false;

        return operationType.Name is not (
            nameof(IAssignmentOperation) or
            "IAggregateQueryOperation" or
            nameof(IInterpolatedStringContentOperation) or
            nameof(ILoopOperation) or
            nameof(IMemberReferenceOperation) or
            nameof(IMethodBodyBaseOperation) or
            nameof(IPatternOperation) or
            "IPlaceholderOperation" or
            nameof(ISymbolInitializerOperation));
    }

    private static IBlockOperation GetBlockOperationUnsafe(string code, string methodName = "TestMethod")
    {
        var usings = @"
          global using System;
          global using System.Collections.Generic;
          global using System.Linq;
          global using System.Numerics;
          global using ECMAScript;
          global using static ECMAScript.Global;";

        var references = TestMetadataReferences.Net11
            .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location));
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: [
                CSharpSyntaxTree.ParseText(usings),
                CSharpSyntaxTree.ParseText(code)
            ],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        var diagnostics = compilation.GetDiagnostics();
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        if (errors.Count > 0)
        {
            var errorMessages = string.Join("\n", errors.Select(e => $"{e.Id}: {e.GetMessage()}"));
            throw new InvalidOperationException(errorMessages);
        }

        var syntaxTree = compilation.SyntaxTrees.Last();
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var root = syntaxTree.GetRoot();
        var methodDeclaration = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(method => method.Identifier.ValueText == methodName && method.Body is not null)
            ?? root.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault(static method => method.Body is not null);
        if (methodDeclaration?.Body is not null &&
            semanticModel.GetOperation(methodDeclaration.Body) is IBlockOperation operation)
            return operation;

        throw new InvalidOperationException("未找到可分析的操作");
    }

    [TestMethod]
    public void VisitAddressOf_UnsafePointer_NotSupported()
    {
        const string code = """
            class TestClass
            {
                unsafe void TestMethod()
                {
                    int x = 42;
                    int* ptr = &x;
                }
            }
            """;

        var block = GetBlockOperationUnsafe(code);
        var walker = new SemanticWalker(true);
        var exception = Assert.Throws<OperationTransformationException>(() => walker.Visit(block, new()));
        StringAssert.Contains(exception.Message ?? string.Empty, "not supported");
    }

    [TestMethod]
    public void VisitFunctionPointerInvocation_NotSupported()
    {
        const string code = """
            class TestClass
            {
                unsafe void TestMethod()
                {
                    delegate*<int, int> fptr = null;
                    int result = fptr(42);
                }
            }
            """;

        var block = GetBlockOperationUnsafe(code);
        var walker = new SemanticWalker(true);
        var exception = Assert.Throws<OperationTransformationException>(() => walker.Visit(block, new()));
        StringAssert.Contains(exception.Message ?? string.Empty, "not supported");
    }
}

public sealed record UnsupportedOperationCase(
    string Id,
    string MethodName,
    Type OperationType);
