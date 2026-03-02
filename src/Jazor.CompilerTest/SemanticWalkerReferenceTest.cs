using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerReferenceTest
{
	/// <summary>
	/// 编译代码并获取roslyn代码块
	/// </summary>
	/// <param name="code"></param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
	private static IBlockOperation GetBlockOperation(string code)
	{
		var usings = @"
        global using System;
        global using System.Collections.Generic;
        global using System.Linq;
        global using System.Numerics;
		global using ECMAScript;
		global using static ECMAScript.Global;";

		var references = Basic.Reference.Assemblies.Net100.References.All
			.Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location));
		var compilation = CSharpCompilation.Create(
			assemblyName: "TestAssembly",
			syntaxTrees: [
			  CSharpSyntaxTree.ParseText(usings),
			  CSharpSyntaxTree.ParseText(code)
			],
			references: references,
			options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		// 输出编译诊断信息
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

		// 查找第一个方法体
		var methodDeclaration = root.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault();
		if (methodDeclaration?.Body is not null)
		{
			var operation = semanticModel.GetOperation(methodDeclaration.Body) as IBlockOperation;
			if (operation is not null)
				return operation;
		}

		throw new InvalidOperationException("未找到可分析的操作");
	}
	
	#region VisitLocalReference - 局部变量引用

	/// <summary>
	/// 测试 VisitLocalReference - 简单局部变量引用
	/// C# 示例：int x = 5; Console.WriteLine(x);
	/// 转换结果：let x = 5; Console.WriteLine(x);
	/// </summary>
	[TestMethod]
	public void Visit_LocalReference_Simple()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5;
                    int y = x;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = 5;
  let y = x;
}", script);
	}

	/// <summary>
	/// 测试 VisitLocalReference - 多个局部变量引用
	/// </summary>
	[TestMethod]
	public void Visit_LocalReference_Multiple()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 1;
                    int b = 2;
                    int c = a + b;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let a = 1;
  let b = 2;
  let c = a + b;
}", script);
	}

	#endregion

	#region VisitParameterReference - 参数引用

	/// <summary>
	/// 测试 VisitParameterReference - 简单参数引用
	/// C# 示例：void Method(int x) { Console.WriteLine(x); }
	/// 转换结果：function Method(x) { Console.WriteLine(x); }
	/// </summary>
	[TestMethod]
	public void Visit_ParameterReference_Simple()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(int param)
                {
                    int x = param;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = param;
}", script);
	}

	/// <summary>
	/// 测试 VisitParameterReference - 多个参数引用
	/// </summary>
	[TestMethod]
	public void Visit_ParameterReference_Multiple()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(int a, int b, int c)
                {
                    int sum = a + b + c;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let sum = a + b + c;
}", script);
	}

	#endregion

	#region VisitFieldReference - 字段引用

	/// <summary>
	/// 测试 VisitFieldReference - 静态常量字段 PositiveInfinity
	/// C# 示例：double x = double.PositiveInfinity;
	/// 转换结果：let x = Infinity;
	/// </summary>
	[TestMethod]
	public void Visit_FieldReference_StaticPositiveInfinity()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    double x = double.PositiveInfinity;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = Infinity;
}", script);
	}

	/// <summary>
	/// 测试 VisitFieldReference - 静态常量字段 NegativeInfinity
	/// C# 示例：double x = double.NegativeInfinity;
	/// 转换结果：let x = -Infinity;
	/// </summary>
	[TestMethod]
	public void Visit_FieldReference_StaticNegativeInfinity()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    double x = double.NegativeInfinity;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = -Infinity;
}", script);
	}

	/// <summary>
	/// 测试 VisitFieldReference - 静态常量字段 NaN
	/// C# 示例：double x = double.NaN;
	/// 转换结果：let x = NaN;
	/// </summary>
	[TestMethod]
	public void Visit_FieldReference_StaticNaN()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    double x = double.NaN;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = NaN;
}", script);
	}

	/// <summary>
	/// 测试 VisitFieldReference - 静态常量字段 Epsilon
	/// C# 示例：double x = double.Epsilon;
	/// 转换结果：let x = Number.EPSILON;
	/// </summary>
	[TestMethod]
	public void Visit_FieldReference_StaticEpsilon()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    double x = double.Epsilon;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = Number.EPSILON;
}", script);
	}

	/// <summary>
	/// 测试 VisitFieldReference - 静态常量字段 MaxValue
	/// C# 示例：double x = double.MaxValue;
	/// 转换结果：let x = Number.MAX_VALUE;
	/// </summary>
	[TestMethod]
	public void Visit_FieldReference_StaticMaxValue()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    double x = double.MaxValue;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = Number.MAX_VALUE;
}", script);
	}

	/// <summary>
	/// 测试 VisitFieldReference - 静态常量字段 MinValue
	/// C# 示例：double x = double.MinValue;
	/// 转换结果：let x = Number.MIN_VALUE;
	/// </summary>
	[TestMethod]
	public void Visit_FieldReference_StaticMinValue()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    double x = double.MinValue;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = Number.MIN_VALUE;
}", script);
	}

	/// <summary>
	/// 测试 VisitFieldReference - long.MaxValue
	/// C# 示例：long x = long.MaxValue;
	/// 转换结果：let x = Number.MAX_SAFE_INTEGER;
	/// </summary>
	[TestMethod]
	public void Visit_FieldReference_LongMaxValue()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    long x = long.MaxValue;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = Number.MAX_SAFE_INTEGER;
}", script);
	}

	/// <summary>
	/// 测试 VisitFieldReference - long.MinValue
	/// C# 示例：long x = long.MinValue;
	/// 转换结果：let x = Number.MIN_SAFE_INTEGER;
	/// </summary>
	[TestMethod]
	public void Visit_FieldReference_LongMinValue()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    long x = long.MinValue;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = Number.MIN_SAFE_INTEGER;
}", script);
	}

	/// <summary>
	/// 测试 VisitFieldReference - 实例字段访问
	/// C# 示例：obj.field
	/// 转换结果：obj.field
	/// </summary>
	[TestMethod]
	public void Visit_FieldReference_InstanceField()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                public int Field;

                void TestMethod()
                {
                    TestClass obj = new TestClass();
                    int x = obj.Field;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let obj = new TestClass;
  let x = obj.Field;
}", script);
	}

	#endregion

	#region VisitPropertyReference - 属性引用

	/// <summary>
	/// 测试 VisitPropertyReference - 实例属性访问
	/// C# 示例：string str = "hello"; int len = str.Length;
	/// 转换结果：let str = "hello"; let len = str.Length;
	/// </summary>
	[TestMethod]
	public void Visit_PropertyReference_InstanceProperty()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string str = ""hello"";
                    int len = str.Length;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let str = ""hello"";
  let len = str.length;
}", script);

	}

	/// <summary>
	/// 测试 VisitPropertyReference - 静态属性访问
	/// C# 示例：DateTime.Now
	/// 转换结果：DateTime.Now
	/// </summary>
	[TestMethod]
	public void Visit_PropertyReference_StaticProperty()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    DateTime now = DateTime.Now;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let now = new Date;
}", script);

	}

	/// <summary>
	/// 测试 VisitPropertyReference - 链式属性访问
	/// C# 示例：obj.Prop1.Prop2
	/// 转换结果：obj.Prop1.Prop2
	/// </summary>
	[TestMethod]
	public void Visit_PropertyReference_Chained()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                public TestClass Prop { get; set; }

                void TestMethod()
                {
                    TestClass obj = new TestClass();
                    var value = obj.Prop.Prop;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let obj = new TestClass;
  let value = obj.Prop.Prop;
}", script);
	}

	#endregion

	#region VisitMethodReference - 方法引用

	/// <summary>
	/// 测试 VisitMethodReference - 静态方法引用
	/// C# 示例：Func<int, int> abs = Math.Abs;
	/// 转换结果：let abs = Math.Abs;
	/// </summary>
	[TestMethod]
	public void Visit_MethodReference_StaticMethod()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Func<int, int> abs = Math.Abs;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let abs = Math.abs;
}", script);

	}

	/// <summary>
	/// 测试 VisitMethodReference - 实例方法引用
	/// C# 示例：Action<string> write = Console.WriteLine;
	/// 转换结果：let write = Console.WriteLine;
	/// </summary>
	[TestMethod]
	public void Visit_MethodReference_InstanceMethod()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Action action = Console.WriteLine;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let action = console.log;
}", script);

	}

	[TestMethod]
	public void Visit_MethodReference_InstanceMethod1()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var x = new BigInteger(100);
                    var y = BigInteger.Zero;
					var z = BigInteger.Parse(""33"");
					var w = y++;
                    var v = z * 33;
					var a = y.CompareTo(y);
					var b = z.ToString();
					var c = w.Equals(v);
                    Console.WriteLine(z);
                    TestMethod(x,y);
                }

				BigInteger TestMethod(BigInteger a,BigInteger b)
                {
                    return a + b;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let x = BigInt(100);
  let y = 0n;
  let z = _155212572c9a3297(""33"");
  let w = y++;
  let v = z * BigInt(33);
  let a = y.CompareTo(y);
  let b = z.ToString();
  let c = w.Equals(v);
  console.log(z);
  this.TestMethod(x, y);
}", script);

	}

	#endregion

	#region VisitInstanceReference - 实例引用

	/// <summary>
	/// 测试 VisitInstanceReference - this 引用
	/// C# 示例：this.Property
	/// 转换结果：this.Property
	/// </summary>
	[TestMethod]
	public void Visit_InstanceReference_This()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                private int _field;

                void TestMethod()
                {
                    int x = this._field;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = this._field;
}", script);
	}

	/// <summary>
	/// 测试 VisitInstanceReference - this 关键字
	/// C# 示例：int x = this._field;
	/// 转换结果：let x = this._field;
	/// </summary>
	[TestMethod]
	public void Visit_InstanceReference_ThisMethodCall()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                private int _field;

                void TestMethod()
                {
                    int x = this._field;
                    int y = this._field + 1;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = this._field;
  let y = this._field + 1;
}", script);
	}

	#endregion

	#region VisitArrayElementReference - 数组元素访问

	/// <summary>
	/// 测试 VisitArrayElementReference - 简单数组索引
	/// C# 示例：array[0]
	/// 转换结果：array[0]
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_SimpleIndex()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3];
                    int x = array[0];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3];
  let x = array[0];
}", script);
	}

	/// <summary>
	/// 测试 VisitArrayElementReference - 变量索引
	/// C# 示例：array[i]
	/// 转换结果：array[i]
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_VariableIndex()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3];
                    int i = 1;
                    int x = array[i];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3];
  let i = 1;
  let x = array[i];
}", script);
	}

	/// <summary>
	/// 测试 VisitArrayElementReference - 从末尾索引 (^1)
	/// C# 示例：array[^1]
	/// 转换结果：array[array.length - 1]
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_FromEnd()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3];
                    int x = array[^1];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3];
  let x = array[array.length - 1];
}", script);
	}

	/// <summary>
	/// 测试 VisitArrayElementReference - 从末尾变量索引 (^n)
	/// C# 示例：array[^n]
	/// 转换结果：array[array.length - n]
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_FromEndVariable()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3];
                    int n = 2;
                    int x = array[^n];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3];
  let n = 2;
  let x = array[array.length - n];
}", script);
	}

	/// <summary>
	/// 测试 VisitArrayElementReference - 范围操作完整范围 [start..end]
	/// C# 示例：array[1..3]
	/// 转换结果：array.slice(1, 4)
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_RangeComplete()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    int[] slice = array[1..3];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let slice = array.slice(1, 3 + 1);
}", script);
	}

	/// <summary>
	/// 测试 VisitArrayElementReference - 范围操作只有起始 [start..]
	/// C# 示例：array[2..]
	/// 转换结果：array.slice(2)
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_RangeFromStart()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    int[] slice = array[2..];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let slice = array.slice(2);
}", script);
	}

	/// <summary>
	/// 测试 VisitArrayElementReference - 范围操作只有结束 [..end]
	/// C# 示例：array[..3]
	/// 转换结果：array.slice(0, 4)
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_RangeToEnd()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    int[] slice = array[..3];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let slice = array.slice(0, 3 + 1);
}", script);
	}

	/// <summary>
	/// 测试 VisitArrayElementReference - 范围操作全部 [..]
	/// C# 示例：array[..]
	/// 转换结果：array.slice()
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_RangeAll()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    int[] copy = array[..];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let copy = array.slice();
}", script);
	}

	/// <summary>
	/// 测试 VisitArrayElementReference - 范围操作从末尾 [^3..^1]
	/// C# 示例：array[^3..^1]
	/// 转换结果：array.slice(array.length - 3, array.length - 1 + 1)
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_RangeFromEnd()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    int[] slice = array[^3..^1];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let slice = array.slice(array.length - 3, array.length - 1 + 1);
}", script);
	}

	/// <summary>
	/// 测试 VisitArrayElementReference - 表达式索引
	/// C# 示例：array[i + 1]
	/// 转换结果：array[i + 1]
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_ExpressionIndex()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    int i = 1;
                    int x = array[i + 1];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let i = 1;
  let x = array[i + 1];
}", script);
	}

	#endregion

	#region VisitImplicitIndexerReference - 隐式索引器引用

	/// <summary>
	/// 测试 VisitImplicitIndexerReference - 隐式索引器从末尾
	/// C# 示例：array[^1]
	/// 转换结果：array[array.length - 1]
	/// </summary>
	[TestMethod]
	public void Visit_ImplicitIndexerReference_FromEnd()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3];
                    int x = array[^1];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3];
  let x = array[array.length - 1];
}", script);
	}

	#endregion

	#region 综合测试

	/// <summary>
	/// 综合测试 - 字段和属性引用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_FieldAndPropertyCombined()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                private int _field;
                public int Property { get; set; }

                void TestMethod(int param)
                {
                    int local = 5;
                    int a = local;
                    int b = param;
                    int c = this._field;
                    int d = this.Property;
                    double e = double.MaxValue;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let local = 5;
  let a = local;
  let b = param;
  let c = this._field;
  let d = this.Property;
  let e = Number.MAX_VALUE;
}", script);
	}

	/// <summary>
	/// 综合测试 - 数组操作
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ArrayOperationsCombined()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    int first = array[0];
                    int last = array[^1];
                    int[] middle = array[1..4];
                    int[] fromTwo = array[2..];
                    int[] uptoThree = array[..3];
                    int[] copy = array[..];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let first = array[0];
  let last = array[array.length - 1];
  let middle = array.slice(1, 4 + 1);
  let fromTwo = array.slice(2);
  let uptoThree = array.slice(0, 3 + 1);
  let copy = array.slice();
}", script);
	}

	/// <summary>
	/// 综合测试 - 特殊常量字段
	/// </summary>
	[TestMethod]
	public void Visit_Reference_SpecialConstants()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    double posInf = double.PositiveInfinity;
                    double negInf = double.NegativeInfinity;
                    double nan = double.NaN;
                    double epsilon = double.Epsilon;
                    double maxVal = double.MaxValue;
                    double minVal = double.MinValue;
                    long longMax = long.MaxValue;
                    long longMin = long.MinValue;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let posInf = Infinity;
  let negInf = -Infinity;
  let nan = NaN;
  let epsilon = Number.EPSILON;
  let maxVal = Number.MAX_VALUE;
  let minVal = Number.MIN_VALUE;
  let longMax = Number.MAX_SAFE_INTEGER;
  let longMin = Number.MIN_SAFE_INTEGER;
}", script);
	}

	#endregion
}
