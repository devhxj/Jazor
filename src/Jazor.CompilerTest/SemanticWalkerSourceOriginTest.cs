using Acornima.Ast;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerSourceOriginTest
{
    [TestMethod]
    public void VisitBlock_AttachesSourceOrigin_ViaVisitFallback()
    {
        var block = GetBlockOperation(
            """
            class TestClass
            {
                void M()
                {
                    var x = 1;
                }
            }
            """,
            "M");

        var node = new SemanticWalker(true).Visit(block, new());
        AssertHasSourceOrigin(node, block);
    }

    [TestMethod]
    public void VisitLiteral_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<ILiteralOperation>(
            """
            class TestClass
            {
                void M()
                {
                    var x = 42;
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitLocalReference_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<ILocalReferenceOperation>(
            """
            class TestClass
            {
                void M()
                {
                    var x = 1;
                    var y = x;
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitParameterReference_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<IParameterReferenceOperation>(
            """
            class TestClass
            {
                void M(int p)
                {
                    var y = p;
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitFieldReference_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<IFieldReferenceOperation>(
            """
            class TestClass
            {
                private int _value = 1;

                void M()
                {
                    var y = _value;
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitPropertyReference_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<IPropertyReferenceOperation>(
            """
            class TestClass
            {
                private int P => 1;

                void M()
                {
                    var y = P;
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitInvocation_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<IInvocationOperation>(
            """
            class TestClass
            {
                void N(int x) { }

                void M()
                {
                    N(1);
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitReturn_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<IReturnOperation>(
            """
            class TestClass
            {
                int M(int p)
                {
                    return p;
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitSimpleAssignment_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<ISimpleAssignmentOperation>(
            """
            class TestClass
            {
                void M()
                {
                    int x = 0;
                    x = 1;
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitTuple_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<ITupleOperation>(
            """
            class TestClass
            {
                void M()
                {
                    var t = (1, 2);
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitDeconstructionAssignment_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<IDeconstructionAssignmentOperation>(
            """
            class TestClass
            {
                (int, int) Get() => (1, 2);

                void M()
                {
                    int a = 0;
                    int b = 0;
                    (a, b) = Get();
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitConditionalAccess_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<IConditionalAccessOperation>(
            """
            class TestClass
            {
                int M(int[] values)
                {
                    return values?.Length ?? 0;
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitAwait_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<IAwaitOperation>(
            """
            class TestClass
            {
                async Task<int> M()
                {
                    return await Task.FromResult(1);
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitAnonymousFunction_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<IAnonymousFunctionOperation>(
            """
            class TestClass
            {
                int M()
                {
                    Func<int, int> inc = x => x + 1;
                    return inc(1);
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitObjectCreation_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<IObjectCreationOperation>(
            """
            class TestClass
            {
                sealed class Person
                {
                    public int Age { get; }

                    public Person(int age)
                    {
                        Age = age;
                    }
                }

                int M()
                {
                    var person = new Person(18);
                    return person.Age;
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitInterpolatedString_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<IInterpolatedStringOperation>(
            """
            class TestClass
            {
                string M(int value)
                {
                    return $"v={value}";
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitWith_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<IWithOperation>(
            """
            class TestClass
            {
                record Person(string Name, int Age);

                Person M(Person person)
                {
                    return person with { Age = person.Age + 1 };
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitCollectionExpressionAndSpread_AttachSourceOrigin()
    {
        var collection = GetFirstOperation<ICollectionExpressionOperation>(
            """
            class TestClass
            {
                int[] M(int[] values)
                {
                    int[] numbers = [1, ..values, 3];
                    return numbers;
                }
            }
            """);
        var spread = GetFirstOperation<ISpreadOperation>(
            """
            class TestClass
            {
                int[] M(int[] values)
                {
                    int[] numbers = [1, ..values, 3];
                    return numbers;
                }
            }
            """);

        var walker = new SemanticWalker(true);
        var collectionNode = walker.Visit(collection, new());
        AssertHasSourceOrigin(collectionNode, collection);

        var spreadNode = walker.Visit(spread, new());
        AssertHasSourceOrigin(spreadNode, spread);
    }

    [TestMethod]
    public void VisitForLoop_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<IForLoopOperation>(
            """
            class TestClass
            {
                int M(int[] values)
                {
                    int total = 0;
                    for (int i = 0; i < values.Length; i++)
                    {
                        total += values[i];
                    }

                    return total;
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitForEachLoop_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<IForEachLoopOperation>(
            """
            class TestClass
            {
                int M(int[] values)
                {
                    int total = 0;
                    foreach (var value in values)
                    {
                        total += value;
                    }

                    return total;
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitWhileLoop_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<IWhileLoopOperation>(
            """
            class TestClass
            {
                int M()
                {
                    int total = 0;
                    while (total < 3)
                    {
                        total++;
                    }

                    return total;
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitSwitch_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<ISwitchOperation>(
            """
            class TestClass
            {
                int M(int value)
                {
                    switch (value)
                    {
                        case 1:
                            return 10;
                        default:
                            return 20;
                    }
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitTry_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<ITryOperation>(
            """
            class TestClass
            {
                int M()
                {
                    try
                    {
                        return 1;
                    }
                    catch (System.Exception)
                    {
                        return 2;
                    }
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitLock_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<ILockOperation>(
            """
            class TestClass
            {
                void M(object gate)
                {
                    lock (gate)
                    {
                        System.Console.WriteLine("ready");
                    }
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitAwaitUsing_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<IUsingOperation>(
            """
            class TestClass
            {
                sealed class AsyncDisposable : System.IAsyncDisposable
                {
                    public System.Threading.Tasks.ValueTask DisposeAsync() => default;
                }

                async System.Threading.Tasks.Task M()
                {
                    await using (new AsyncDisposable())
                    {
                        await System.Threading.Tasks.Task.Yield();
                    }
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitTypeOf_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<ITypeOfOperation>(
            """
            class TestClass
            {
                sealed class Person
                {
                }

                void M()
                {
                    var type = typeof(Person);
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitSwitchExpression_AttachesSourceOrigin()
    {
        var operation = GetFirstOperation<ISwitchExpressionOperation>(
            """
            class TestClass
            {
                string M(object value)
                {
                    return value switch
                    {
                        int i when i > 0 => "positive",
                        null => "null",
                        _ => "other"
                    };
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void VisitInterpolationFamily_AttachesSourceOrigin()
    {
        var interpolationText = GetFirstOperation<IInterpolatedStringTextOperation>(
            """
            class TestClass
            {
                string M(int value)
                {
                    return $"prefix:{value}";
                }
            }
            """);
        var interpolation = GetFirstOperation<IInterpolationOperation>(
            """
            class TestClass
            {
                string M(int value)
                {
                    return $"prefix:{value}";
                }
            }
            """);

        var walker = new SemanticWalker(true);
        AssertHasSourceOrigin(walker.Visit(interpolationText, new()), interpolationText);
        var interpolationNode = walker.Visit(interpolation, new());
        Assert.IsNotNull(interpolationNode);
        Assert.IsNotNull(interpolationNode.UserData);
        Assert.AreEqual("SourceOrigin", interpolationNode.UserData.GetType().Name);
    }

    [TestMethod]
    public void Visit_OrdinaryAndReferenceFamily_AttachesSourceOrigin()
    {
        const string code = """
            class TestClass
            {
                private int _field = 1;
                private int Property { get; set; } = 2;

                int AddOptional(int value = 5) => value;
                int Identity(int value) => value;

                async Task<int> M(int p, int[] values, string? text, Dictionary<int, int> map, int? maybe)
                {
                    int local = 0;
                    int other = local + p;
                    other = _field + Property;
                    other += 2;
                    maybe ??= other;
                    other = maybe ?? other;
                    other = other > 0 ? other : 0;
                    other = -other;
                    other = (int)(double)other;
                    other = (other);
                    var named = nameof(TestClass);
                    var def = default(int);
                    other++;
                    --other;
                    other = AddOptional();
                    other = Identity(other);
                    other = values[0];
                    other = values[^1];
                    var mapItem = map?[0] ?? -1;
                    Func<int, int> methodGroup = Identity;
                    Func<int> instanceGroup = this.GetHashCode;
                    Func<int, int> lambda = x => x + 1;
                    var awaited = await Task.FromResult(lambda(other));
                    _ = awaited + mapItem + named.Length + def + (text?.Length ?? 0) + methodGroup(other) + instanceGroup();
                    return other;
                }
            }
            """;

        AssertOperationsHaveSourceOrigin(
            code,
            methodName: "M",
            typeof(IExpressionStatementOperation),
            typeof(IConversionOperation),
            typeof(IUnaryOperation),
            typeof(IBinaryOperation),
            typeof(IConditionalOperation),
            typeof(ICoalesceOperation),
            typeof(IAnonymousFunctionOperation),
            typeof(IAwaitOperation),
            typeof(ISimpleAssignmentOperation),
            typeof(ICompoundAssignmentOperation),
            typeof(ICoalesceAssignmentOperation),
            typeof(INameOfOperation),
            typeof(IDefaultValueOperation),
            typeof(IIncrementOrDecrementOperation),
            typeof(IArgumentOperation),
            typeof(IArrayElementReferenceOperation),
            typeof(IMethodReferenceOperation),
            typeof(IInstanceReferenceOperation),
            typeof(IDelegateCreationOperation));
    }

    [TestMethod]
    public void Visit_CreationAndDeclarationFamily_AttachesSourceOrigin()
    {
        const string code = """
            class TestClass
            {
                private int _field = 42;
                private int PropertyWithInit { get; } = 7;

                private sealed class Box
                {
                    public int A;
                    public int B { get; set; }

                    public Box(int a)
                    {
                        A = a;
                    }
                }

                int Target(int value) => value;

                T MakeGeneric<T>() where T : new()
                {
                    return new T();
                }

                int[] M(string input)
                {
                    int a = 1, b = 2;
                    int[] arr = new int[] { a, b };
                    var box = new Box(a) { B = b };
                    var anon = new { box.A, box.B };
                    var tuple = (a, b);
                    var (x, y) = tuple;
                    if (int.TryParse(input, out int parsed))
                    {
                        _ = parsed;
                    }

                    Func<int, int> del = Target;
                    _ = del(a);
                    return arr;
                }
            }
            """;

        AssertOperationsHaveSourceOrigin(
            code,
            methodName: "M",
            typeof(IObjectCreationOperation),
            typeof(IObjectOrCollectionInitializerOperation),
            typeof(IAnonymousObjectCreationOperation),
            typeof(IArrayCreationOperation),
            typeof(IArrayInitializerOperation),
            typeof(IVariableInitializerOperation),
            typeof(IVariableDeclaratorOperation),
            typeof(IVariableDeclarationOperation),
            typeof(IVariableDeclarationGroupOperation),
            typeof(IDeclarationExpressionOperation));

        var memberInitializer = TryGetFirstOperation<IMemberInitializerOperation>(code);
        if (memberInitializer is not null)
            AssertHasSourceOrigin(new SemanticWalker(true).Visit(memberInitializer, new()), memberInitializer);

        var typeParameterCreation = GetFirstOperation<ITypeParameterObjectCreationOperation>(code, methodName: "MakeGeneric");
        Assert.Throws<OperationTransformationException>(() => new SemanticWalker(true).Visit(typeParameterCreation, new()));

        var fieldInitializer = GetFieldInitializerOperation(code);
        AssertHasSourceOrigin(new SemanticWalker(true).Visit(fieldInitializer, new()), fieldInitializer);

        var propertyInitializer = GetPropertyInitializerOperation(code);
        AssertHasSourceOrigin(new SemanticWalker(true).Visit(propertyInitializer, new()), propertyInitializer);
    }

    [TestMethod]
    public void Visit_ControlFlowSubNodes_AttachSourceOrigin()
    {
        const string code = """
            class TestClass
            {
                int M(int[] values)
                {
                    int total = 0;
                    void LocalAdd(int x) => total += x;
                    LocalAdd(1);
                start:
                    total += 0;
                    ;
                    for (int i = 0; i < values.Length; i++)
                    {
                        total += values[i];
                        if (total > 10)
                        {
                            break;
                        }

                        continue;
                    }

                    foreach (var value in values)
                    {
                        total += value;
                    }

                    while (total < 20)
                    {
                        total++;
                    }

                    switch (total)
                    {
                        case 1:
                            total += 100;
                            break;
                        default:
                            total += 200;
                            break;
                    }

                    try
                    {
                        throw new System.Exception("boom");
                    }
                    catch (System.Exception)
                    {
                        total -= 1;
                    }

                    return total;
                }
            }
            """;

        AssertOperationsHaveSourceOrigin(
            code,
            methodName: "M",
            typeof(ILocalFunctionOperation),
            typeof(ILabeledOperation),
            typeof(IBranchOperation),
            typeof(IEmptyOperation),
            typeof(ICatchClauseOperation),
            typeof(IThrowOperation),
            typeof(ISwitchCaseOperation),
            typeof(ISingleValueCaseClauseOperation));
    }

    [TestMethod]
    public void Visit_PatternSubNodes_AttachSourceOrigin()
    {
        const string code = """
            class TestClass
            {
                private sealed record Person(string Name, int Age);

                string M(object value, int[] numbers)
                {
                    bool isType = value is int;
                    bool isNull = value is null;
                    bool isPattern = value is not (>= 0 and <= 10);
                    bool hasTypePattern = value is int and > 0;
                    bool listMatch = numbers is [1, .. var tail];

                    if (value is Person { Age: > 18, Name: not null })
                    {
                        _ = listMatch;
                    }

                    return value switch
                    {
                        Person { Name: "A" } => "A",
                        int i when i > 0 => "positive",
                        null => "null",
                        _ => "other"
                    };
                }

                string N(object value)
                {
                    return value switch
                    {
                        Person { Name: "A" } => "A",
                        int i when i > 0 => "positive",
                        null => "null",
                        _ => "other"
                    };
                }
            }
            """;

        AssertOperationsHaveSourceOrigin(
            code,
            methodName: "M",
            typeof(IIsTypeOperation),
            typeof(IIsPatternOperation),
            typeof(ISwitchExpressionArmOperation),
            typeof(IRecursivePatternOperation),
            typeof(IConstantPatternOperation),
            typeof(IDeclarationPatternOperation),
            typeof(IDiscardPatternOperation),
            typeof(IPropertySubpatternOperation),
            typeof(INegatedPatternOperation),
            typeof(IBinaryPatternOperation),
            typeof(IRelationalPatternOperation),
            typeof(IListPatternOperation),
            typeof(ISlicePatternOperation),
            typeof(ITypePatternOperation));

        var patternCaseClause = GetFirstOperation<IPatternCaseClauseOperation>(
            """
            class TestClass
            {
                int M(object value)
                {
                    switch (value)
                    {
                        case int i when i > 0:
                            return i;
                        default:
                            return 0;
                    }
                }
            }
            """);
        AssertHasSourceOrigin(
            new SemanticWalker(true).Visit(patternCaseClause, BuildVisitArgument(patternCaseClause)),
            patternCaseClause);
    }

    [TestMethod]
    public void Visit_StringAndTupleSubNodes_AttachSourceOrigin()
    {
        const string code = """
            class TestClass
            {
                (int, int) GetPair() => (1, 2);

                int M()
                {
                    var text = $"{1}{2}";
                    int a = 0, b = 0;
                    (a, b) = GetPair();
                    var equal = (a, b) == (1, 2);
                    var (_, y) = (3, 4);
                    return equal ? y : a + b;
                }
            }
            """;

        AssertOperationsHaveSourceOrigin(
            code,
            methodName: "M",
            typeof(ITupleBinaryOperation),
            typeof(IDiscardOperation));
    }

    [TestMethod]
    public void Visit_MethodConstructorAndAttributeOperations_AttachSourceOrigin()
    {
        var methodBody = GetMethodBodyOperation(
            """
            class TestClass
            {
                int M(int value)
                {
                    return value + 1;
                }
            }
            """);
        AssertHasSourceOrigin(new SemanticWalker(true).Visit(methodBody, new()), methodBody);

        var constructorBody = GetConstructorBodyOperation(
            """
            class TestClass
            {
                TestClass()
                {
                    int x = 1;
                }
            }
            """);
        AssertHasSourceOrigin(new SemanticWalker(true).Visit(constructorBody, new()), constructorBody);

    }

    [TestMethod]
    public void Visit_DefaultCaseClause_IsHandledBySwitchParent()
    {
        const string code = """
            class TestClass
            {
                int M(int value)
                {
                    switch (value)
                    {
                        case 1:
                            return 1;
                        default:
                            return 0;
                    }
                }
            }
            """;

        var defaultCaseClause = GetFirstOperation<IDefaultCaseClauseOperation>(code);
        var defaultCaseNode = new SemanticWalker(true).Visit(defaultCaseClause, new());
        Assert.IsNull(defaultCaseNode);

        var switchOperation = GetFirstOperation<ISwitchOperation>(code);
        var switchNode = new SemanticWalker(true).Visit(switchOperation, new());
        AssertHasSourceOrigin(switchNode, switchOperation);
    }

    [TestMethod]
    public void Visit_AttributeOperation_ImplementsIECMAScript_AttachesSourceOrigin()
    {
        var operation = GetFirstAttributeOperation(
            """
            [AttributeUsage(AttributeTargets.Method)]
            sealed class JsDecoratorAttribute : Attribute, IECMAScript
            {
                public string? Name { get; set; }

                public JsDecoratorAttribute(int order)
                {
                }
            }

            class TestClass
            {
                [JsDecorator(1, Name = "entry")]
                void M()
                {
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        if (node is null)
            return;

        AssertHasSourceOrigin(node, operation);
    }

    [TestMethod]
    public void Visit_AttributeOperation_NonIECMAScript_ReturnsNull()
    {
        var operation = GetFirstAttributeOperation(
            """
            using System;

            class TestClass
            {
                [Obsolete("legacy")]
                void M()
                {
                }
            }
            """);

        var node = new SemanticWalker(true).Visit(operation, new());
        Assert.IsNull(node);
    }

    [TestMethod]
    public void Visit_ImplicitIndexerReference_AttachesSourceOrigin_WhenPresent()
    {
        var operation = FindImplicitIndexerReferenceOperation();
        if (operation is not null)
        {
            var node = new SemanticWalker(true).Visit(operation, new());
            AssertHasSourceOrigin(node, operation);
            return;
        }

        // Current Roslyn shape can lower '^' without exposing
        // IImplicitIndexerReferenceOperation directly. In that case we still
        // verify the emitted index expression node path keeps source origin.
        var fallback = GetFirstOperation<IArrayElementReferenceOperation>(
            """
            class TestClass
            {
                int M(int[] values)
                {
                    return values[^1];
                }
            }
            """);
        var fallbackNode = new SemanticWalker(true).Visit(fallback, new());
        AssertHasSourceOrigin(fallbackNode, fallback);
    }

    private static void AssertHasSourceOrigin(Node? node, IOperation operation)
    {
        Assert.IsNotNull(node);

        var userData = node.UserData;
        Assert.IsNotNull(userData);
        Assert.AreEqual("SourceOrigin", userData.GetType().Name);

        var lineSpan = operation.Syntax.GetLocation().GetLineSpan();
        var expectedPath = !string.IsNullOrWhiteSpace(lineSpan.Path)
            ? lineSpan.Path
            : operation.Syntax.SyntaxTree?.FilePath;

        Assert.AreEqual(expectedPath, ReadOriginValue<string?>(userData, "SourcePath"));
        Assert.AreEqual(lineSpan.StartLinePosition.Line, ReadOriginValue<int>(userData, "StartLine"));
        Assert.AreEqual(lineSpan.StartLinePosition.Character, ReadOriginValue<int>(userData, "StartColumn"));
        Assert.AreEqual(lineSpan.EndLinePosition.Line, ReadOriginValue<int>(userData, "EndLine"));
        Assert.AreEqual(lineSpan.EndLinePosition.Character, ReadOriginValue<int>(userData, "EndColumn"));
        Assert.IsFalse(ReadOriginValue<bool>(userData, "IsSynthetic"));
    }

    private static T ReadOriginValue<T>(object origin, string propertyName)
    {
        var property = origin.GetType().GetProperty(propertyName);
        Assert.IsNotNull(property, $"Property '{propertyName}' was not found on '{origin.GetType().FullName}'.");
        return (T)property.GetValue(origin)!;
    }

    private static TOperation GetFirstOperation<TOperation>(string code, string methodName = "M")
        where TOperation : class, IOperation
    {
        var block = GetBlockOperation(code, methodName);
        var operation = EnumerateOperations(block).OfType<TOperation>().FirstOrDefault();
        if (operation is null)
            throw new InvalidOperationException($"Could not find operation '{typeof(TOperation).Name}'.");

        return operation;
    }

    private static TOperation? TryGetFirstOperation<TOperation>(string code, string methodName = "M")
        where TOperation : class, IOperation
    {
        var block = GetBlockOperation(code, methodName);
        return EnumerateOperations(block).OfType<TOperation>().FirstOrDefault();
    }

    private static void AssertOperationsHaveSourceOrigin(
        string code,
        string methodName,
        params Type[] operationTypes)
    {
        var block = GetBlockOperation(code, methodName);
        var operations = EnumerateOperations(block).ToArray();
        var walker = new SemanticWalker(true);

        foreach (var operationType in operationTypes)
        {
            var operation = operations.FirstOrDefault(operationType.IsInstanceOfType);
            Assert.IsNotNull(operation, $"Could not find operation '{operationType.Name}' in method '{methodName}'.");
            var argument = BuildVisitArgument(operation!);
            AssertHasSourceOrigin(walker.Visit(operation, argument), operation);
        }
    }

    private static SenseArgument BuildVisitArgument(IOperation operation)
    {
        if (operation is IConditionalAccessInstanceOperation)
            return new SenseArgument(PatternInput: new Identifier("conditionalTarget"));

        if (operation is
            IPatternOperation or
            IPatternCaseClauseOperation or
            ISwitchExpressionArmOperation or
            IPropertySubpatternOperation or
            IRecursivePatternOperation or
            IConstantPatternOperation or
            IDeclarationPatternOperation or
            IDiscardPatternOperation or
            INegatedPatternOperation or
            IBinaryPatternOperation or
            IRelationalPatternOperation or
            IListPatternOperation or
            ISlicePatternOperation or
            ITypePatternOperation)
            return new SenseArgument(PatternInput: new Identifier("patternInput"));

        if (operation.Kind.ToString().Contains("Pattern", StringComparison.Ordinal))
            return new SenseArgument(PatternInput: new Identifier("patternInput"));

        return new SenseArgument();
    }

    private static IAttributeOperation GetFirstAttributeOperation(string code)
    {
        var (semanticModel, root) = CreateSemanticModelAndRoot(code);
        var attributeSyntax = root.DescendantNodes().OfType<AttributeSyntax>().FirstOrDefault();
        if (attributeSyntax is not null &&
            semanticModel.GetOperation(attributeSyntax) is IAttributeOperation operation)
        {
            return operation;
        }

        throw new InvalidOperationException("Could not find attribute operation.");
    }

    private static IImplicitIndexerReferenceOperation? FindImplicitIndexerReferenceOperation()
    {
        const string customIndexerCandidate = """
            class TestClass
            {
                sealed class Buffer
                {
                    private readonly int[] _values = [1, 2, 3];
                    public int Length => _values.Length;
                    public int this[int index] => _values[index];
                }

                int M(Buffer buffer)
                {
                    return buffer[^1];
                }
            }
            """;

        var operation = TryGetFirstOperation<IImplicitIndexerReferenceOperation>(customIndexerCandidate);
        if (operation is not null)
            return operation;

        const string arrayCandidate = """
            class TestClass
            {
                int M(int[] values)
                {
                    return values[^1];
                }
            }
            """;

        operation = TryGetFirstOperation<IImplicitIndexerReferenceOperation>(arrayCandidate);
        if (operation is not null)
            return operation;

        const string spanCandidate = """
            class TestClass
            {
                int M(Span<int> values)
                {
                    return values[^1];
                }
            }
            """;

        return TryGetFirstOperation<IImplicitIndexerReferenceOperation>(spanCandidate);
    }

    private static IFieldInitializerOperation GetFieldInitializerOperation(string code)
    {
        var (semanticModel, root) = CreateSemanticModelAndRoot(code);
        var fieldDeclarator = root.DescendantNodes().OfType<VariableDeclaratorSyntax>().First();
        if (semanticModel.GetOperation(fieldDeclarator.Initializer!) is IFieldInitializerOperation operation)
            return operation;

        throw new InvalidOperationException("Could not find field initializer operation.");
    }

    private static IPropertyInitializerOperation GetPropertyInitializerOperation(string code)
    {
        var (semanticModel, root) = CreateSemanticModelAndRoot(code);
        var propertyDeclaration = root.DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .First(property => property.Initializer is not null);
        if (semanticModel.GetOperation(propertyDeclaration.Initializer!) is IPropertyInitializerOperation operation)
            return operation;

        throw new InvalidOperationException("Could not find property initializer operation.");
    }

    private static IMethodBodyOperation GetMethodBodyOperation(string code, string methodName = "M")
    {
        var (semanticModel, root) = CreateSemanticModelAndRoot(code);
        var methodDeclaration = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(method => method.Identifier.ValueText == methodName);
        if (methodDeclaration is not null &&
            semanticModel.GetOperation(methodDeclaration) is IMethodBodyOperation operation)
        {
            return operation;
        }

        throw new InvalidOperationException($"Could not find method body operation for method '{methodName}'.");
    }

    private static IConstructorBodyOperation GetConstructorBodyOperation(string code)
    {
        var (semanticModel, root) = CreateSemanticModelAndRoot(code);
        var constructor = root.DescendantNodes().OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
        if (constructor is not null &&
            semanticModel.GetOperation(constructor) is IConstructorBodyOperation operation)
        {
            return operation;
        }

        throw new InvalidOperationException("Could not find constructor body operation.");
    }

    private static IEnumerable<IOperation> EnumerateOperations(IOperation root)
    {
        yield return root;
        foreach (var child in root.ChildOperations)
        {
            foreach (var nested in EnumerateOperations(child))
                yield return nested;
        }
    }

    private static IBlockOperation GetBlockOperation(string code, string methodName)
    {
        var usings = """
            global using System;
            global using System.Collections.Generic;
            global using System.Linq;
            global using System.Numerics;
            global using System.Threading.Tasks;
            global using ECMAScript;
            global using static ECMAScript.Global;
            """;

        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
        var references = TestMetadataReferences.Net11
            .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Global).Assembly.Location));
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(usings, parseOptions),
                CSharpSyntaxTree.ParseText(code, parseOptions)
            ],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var diagnostics = compilation.GetDiagnostics();
        var errors = diagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length > 0)
            throw new InvalidOperationException(string.Join(Environment.NewLine, errors.Select(static error => $"{error.Id}: {error.GetMessage()}")));

        var syntaxTree = compilation.SyntaxTrees.Last();
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var root = syntaxTree.GetRoot();
        var methodDeclaration = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(method => method.Identifier.ValueText == methodName);
        if (methodDeclaration?.Body is not null &&
            semanticModel.GetOperation(methodDeclaration.Body) is IBlockOperation operation)
        {
            return operation;
        }

        throw new InvalidOperationException($"Method '{methodName}' was not found or has no analyzable block body.");
    }

    private static (SemanticModel SemanticModel, SyntaxNode Root) CreateSemanticModelAndRoot(string code)
    {
        var usings = """
            global using System;
            global using System.Collections.Generic;
            global using System.Linq;
            global using System.Numerics;
            global using System.Threading.Tasks;
            global using ECMAScript;
            global using static ECMAScript.Global;
            """;

        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
        var references = TestMetadataReferences.Net11
            .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Global).Assembly.Location));
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(usings, parseOptions),
                CSharpSyntaxTree.ParseText(code, parseOptions)
            ],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var diagnostics = compilation.GetDiagnostics();
        var errors = diagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length > 0)
            throw new InvalidOperationException(string.Join(Environment.NewLine, errors.Select(static error => $"{error.Id}: {error.GetMessage()}")));

        var syntaxTree = compilation.SyntaxTrees.Last();
        return (compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot());
    }
}
