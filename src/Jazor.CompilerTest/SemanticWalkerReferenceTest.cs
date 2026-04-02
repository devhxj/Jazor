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

	[TestMethod]
	public void Visit_Reference_DescriptionBasedNamespaceAliasType_TranslatesToWebIdlNames()
	{
		var block = GetBlockOperation(@"
            using System.ComponentModel;
            using console = ECMAScript.Console.Console;

            class TestClass
            {
                void TestMethod()
                {
                    console.Log(""hello"");
                }
            }

            namespace ECMAScript.Console
            {
                [Description(""@#console"")]
                public static class Console
                {
                    [Description(""@#log"")]
                    public static void Log(string value) { }
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  console.log(""hello"");
}".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
	}

	[TestMethod]
	public void Visit_Reference_ActualEcmascriptBindings_EmitRealisticBrowserShapes()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var element = Global.Document.CreateElement(""button"", new ElementCreationOptions(Is: ""x-button""));
                    element.AddEventListener(""click"", null, new AddEventListenerOptions(Once: true));
                    var evt = new Event(""click"", new EventInit(Bubbles: true, Cancelable: true));
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.IsNotNull(script);
		StringAssert.Contains(script, @"document.createElement(""button"", {");
		StringAssert.Contains(script, @"is: ""x-button""");
		StringAssert.Contains(script, @"element.addEventListener(""click"", null, {");
		StringAssert.Contains(script, @"once: true");
		StringAssert.Contains(script, @"new Event(""click"", {");
		StringAssert.Contains(script, @"bubbles: true");
		StringAssert.Contains(script, @"cancelable: true");
	}

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
  let x = -Number.MAX_VALUE;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
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
  let x = 9223372036854775807n;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
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
  let x = -9223372036854775808n;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
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
  let now = _ee9dd166a34a2fa5();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());

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

	[TestMethod]
	public void Visit_Invocation_ObjectPrototypeMethod_HasOwnProperty()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                public int Value { get; set; }

                void TestMethod()
                {
                    var obj = new TestClass();
                    var hasValue = obj.HasOwnProperty(""Value"");
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let obj = new TestClass;
  let hasValue = obj.hasOwnProperty(""Value"");
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Invocation_ObjectStaticMethod_Keys()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var keys = Object.Keys(new TestClass());
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let keys = Object.keys(new TestClass);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Invocation_ObjectStaticMethod_Is()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var same = Object.Is(1, 1);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let same = Object.is(1, 1);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Invocation_ObjectStaticMethod_HasOwn_UsesJsMemberName()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                public int Value { get; set; }

                void TestMethod()
                {
                    var hasValue = Object.HasOwn(new TestClass(), ""Value"");
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let hasValue = Object.hasOwn(new TestClass, ""Value"");
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

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
  let a = y < y ? -1 : y > y ? 1 : 0;
  let b = z.toString();
  let c = w === v;
  console.log(z);
  this.TestMethod_86dba5f71d944ea3(x, y);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());

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
  let minVal = -Number.MAX_VALUE;
  let longMax = 9223372036854775807n;
  let longMin = -9223372036854775808n;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	#endregion

	#region 扩展测试用例 - 多维数组

	/// <summary>
	/// 测试二维数组元素访问
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_TwoDimensional()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[,] matrix = new int[2, 3];
                    int x = matrix[0, 1];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let matrix = new Array(2).fill().map(() => new Array(3));
  let x = matrix[0][1];
}", script);
	}

	/// <summary>
	/// 测试交错数组元素访问
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_Jagged()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[][] jagged = new int[2][];
                    int x = jagged[0][1];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let jagged = new Array(2);
  let x = jagged[0][1];
}", script);
	}

	/// <summary>
	/// 测试数组元素赋值
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_Assignment()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = new int[5];
                    array[0] = 10;
                    array[1] = 20;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = new Array(5);
  array[0] = 10;
  array[1] = 20;
}", script);
	}

	#endregion

	#region 扩展测试用例 - 属性引用变体

	/// <summary>
	/// 测试只读属性访问
	/// </summary>
	[TestMethod]
	public void Visit_PropertyReference_ReadOnly()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                public int ReadOnlyProp { get; }

                void TestMethod()
                {
                    TestClass obj = new TestClass();
                    int x = obj.ReadOnlyProp;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let obj = new TestClass;
  let x = obj.ReadOnlyProp;
}", script);
	}

	/// <summary>
	/// 测试静态属性访问 DateTime.UtcNow
	/// </summary>
	[TestMethod]
	public void Visit_PropertyReference_DateTimeUtcNow()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    DateTime utcNow = DateTime.UtcNow;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let utcNow = _d4c39bdf47f391cf();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试 DateTime 日期属性
	/// </summary>
	[TestMethod]
	public void Visit_PropertyReference_DateTimeProperties()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    DateTime now = DateTime.Now;
                    int year = now.Year;
                    int month = now.Month;
                    int day = now.Day;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let now = _ee9dd166a34a2fa5();
  let year = _9d56b09432f81c05(now);
  let month = _a8a6b6e36a0ea736(now);
  let day = _3b9ecf5fd3c301db(now);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试数组长度属性
	/// </summary>
	[TestMethod]
	public void Visit_PropertyReference_ArrayLength()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    int len = array.Length;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let len = array.length;
}", script);
	}

	/// <summary>
	/// 测试字符串长度属性
	/// </summary>
	[TestMethod]
	public void Visit_PropertyReference_StringLength()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string text = ""Hello"";
                    int len = text.Length;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let text = ""Hello"";
  let len = text.length;
}", script);
	}

	#endregion

	#region 扩展测试用例 - 方法引用变体

	/// <summary>
	/// 测试实例方法引用带绑定
	/// </summary>
	[TestMethod]
	public void Visit_MethodReference_BoundMethod()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    TestClass obj = new TestClass();
                    Action action = obj.DoSomething;
                }

                void DoSomething() { }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let obj = new TestClass;
  let action = obj.DoSomething.bind(obj);
}", script);
	}

	/// <summary>
	/// 测试带参数方法引用
	/// </summary>
	[TestMethod]
	public void Visit_MethodReference_WithParameters()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Func<int, int, int> add = Add;
                }

                int Add(int a, int b) => a + b;
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let add = this.Add.bind(this);
}", script);
	}

	#endregion

	#region 扩展测试用例 - this引用变体

	/// <summary>
	/// 测试this传递给方法
	/// </summary>
	[TestMethod]
	public void Visit_InstanceReference_ThisAsArgument()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Process(this);
                }

                void Process(TestClass obj) { }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  this.Process(this);
}", script);
	}

	/// <summary>
	/// 测试this返回
	/// </summary>
	[TestMethod]
	public void Visit_InstanceReference_ReturnThis()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                TestClass TestMethod()
                {
                    return this;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  return this;
}", script);
	}

	#endregion

	#region 扩展测试用例 - 索引器引用

	/// <summary>
	/// 测试列表索引器访问
	/// </summary>
	[TestMethod]
	public void Visit_IndexerReference_List()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new List<int> { 1, 2, 3 };
                    int first = list[0];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let list = [1, 2, 3];
  let first = _d389c31d59037b42(list, 0);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试字典索引器访问
	/// </summary>
	[TestMethod]
	public void Visit_IndexerReference_Dictionary()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var dict = new Dictionary<string, int>();
                    dict[""key""] = 42;
                    int value = dict[""key""];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let dict = new Map;
  dict.set(""key"", 42);
  let value = _e73dbdff85c46ddc(dict, ""key"");
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试字典索引器访问 - 匿名对象键
	/// </summary>
	[TestMethod]
	public void Visit_IndexerReference_Dictionary_AnonymousObjectKey()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var dict = new Dictionary<object, int>();
                    dict[new { Name = ""key"", Index = 1 }] = 42;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let dict = new Map;
  dict.set({ Name: ""key"", Index: 1 }, 42);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试字典索引器访问 - new 表达式键
	/// </summary>
	[TestMethod]
	public void Visit_IndexerReference_Dictionary_NewExpressionKey()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var dict = new Dictionary<object, int>();
                    dict[new TestClass()] = 42;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let dict = new Map;
  dict.set(new TestClass, 42);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试字典索引器访问 - 可选链键
	/// </summary>
	[TestMethod]
	public void Visit_IndexerReference_Dictionary_ConditionalAccessKey()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                class Person
                {
                    public string Name { get; set; }
                }

                void TestMethod()
                {
                    Person person = null;
                    var dict = new Dictionary<string, int>();
                    dict[person?.Name] = 42;
                    int value = dict[person?.Name];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let person = null;
  let dict = new Map;
  dict.set(person?.Name, 42);
  let value = _e73dbdff85c46ddc(dict, person?.Name);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	#endregion

	#region 扩展测试用例 - 复杂引用场景

	/// <summary>
	/// 测试链式方法调用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ChainedMethodCalls()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string text = ""  Hello World  "";
                    string result = text.Trim().ToUpper().Substring(0, 5);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let text = ""  Hello World  "";
  let result = text.trim().toUpperCase().substring(0, 0 + 5);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试内联方法调用 - 数组字面量参数
	/// </summary>
	[TestMethod]
	public void Visit_Invocation_ArrayClear_ArrayLiteralArgument()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Array.Clear(new int[] { 1, 2, 3 });
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  [1, 2, 3].length = 0;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试嵌套属性访问
	/// </summary>
	[TestMethod]
	public void Visit_Reference_NestedPropertyAccess()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                public InnerClass Inner { get; set; }

                void TestMethod()
                {
                    TestClass obj = new TestClass();
                    obj.Inner = new InnerClass();
                    int x = obj.Inner.Value;
                }
            }

            class InnerClass
            {
                public int Value { get; set; }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let obj = new TestClass;
  obj.Inner = new InnerClass;
  let x = obj.Inner.Value;
}", script);
	}

	/// <summary>
	/// 测试数组元素作为对象属性访问
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ArrayElementProperty()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var array = new[] { ""apple"", ""banana"", ""cherry"" };
                    int len = array[0].Length;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [""apple"", ""banana"", ""cherry""];
  let len = array[0].length;
}", script);
	}

	/// <summary>
	/// 测试数组元素方法调用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ArrayElementMethod()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var array = new[] { ""apple"", ""banana"", ""cherry"" };
                    string upper = array[0].ToUpper();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [""apple"", ""banana"", ""cherry""];
  let upper = array[0].toUpperCase();
}", script);
	}

	#region 扩展测试用例 - 更多属性引用

	/// <summary>
	/// 测试静态属性引用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_StaticProperty()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var now = System.DateTime.Now;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let now = _ee9dd166a34a2fa5();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_RuntimeStaticProperty_UsesImplicitEcmascriptMemberName()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var buffer = new ArrayBuffer(8);
                    var byteLength = buffer.ByteLength;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let buffer = new ArrayBuffer(8);
  let byteLength = buffer.byteLength;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试只读属性
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ReadOnlyProperty()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                public int ReadOnly => 42;

                void TestMethod()
                {
                    int value = ReadOnly;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let value = this.ReadOnly;
}", script);
	}

	/// <summary>
	/// 测试计算属性
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ComputedProperty()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                private int _value = 10;
                public int Doubled => _value * 2;

                void TestMethod()
                {
                    int result = Doubled;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let result = this.Doubled;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试索引器属性
	/// </summary>
	[TestMethod]
	public void Visit_Reference_IndexerProperty()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new System.Collections.Generic.List<int> { 1, 2, 3 };
                    int first = list[0];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let list = [1, 2, 3];
  let first = _d389c31d59037b42(list, 0);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试字典索引器
	/// </summary>
	[TestMethod]
	public void Visit_Reference_DictionaryIndexer()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var dict = new System.Collections.Generic.Dictionary<string, int>();
                    dict[""key""] = 42;
                    int value = dict[""key""];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let dict = new Map;
  dict.set(""key"", 42);
  let value = _e73dbdff85c46ddc(dict, ""key"");
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	#endregion

	#region 扩展测试用例 - 更多字段引用

	/// <summary>
	/// 测试静态字段引用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_StaticField()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                public static int Counter = 0;

                void TestMethod()
                {
                    int count = Counter;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let count = TestClass.Counter;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试只读字段
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ReadOnlyField()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                public readonly int Value = 42;

                void TestMethod()
                {
                    int v = Value;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let v = this.Value;
}", script);
	}

	/// <summary>
	/// 测试常量字段
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ConstField()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                public const int MaxValue = 100;

                void TestMethod()
                {
                    int max = MaxValue;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let max = 100;
}", script);
	}

	#endregion

	#region 扩展测试用例 - 数组引用

	/// <summary>
	/// 测试二维数组引用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_2DArray()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[,] matrix = new int[3, 3];
                    matrix[0, 0] = 1;
                    int value = matrix[1, 2];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let matrix = new Array(3).fill().map(() => new Array(3));
  matrix[0][0] = 1;
  let value = matrix[1][2];
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试锯齿数组引用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_JaggedArray()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[][] jagged = new int[3][];
                    jagged[0] = new int[] { 1, 2, 3 };
                    int value = jagged[0][1];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let jagged = new Array(3);
  jagged[0] = [1, 2, 3];
  let value = jagged[0][1];
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试数组 Length 属性
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ArrayLength()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] arr = new int[] { 1, 2, 3, 4, 5 };
                    int len = arr.Length;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let arr = [1, 2, 3, 4, 5];
  let len = arr.length;
}", script);
	}

	/// <summary>
	/// 测试数组 Rank 属性
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ArrayRank()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[,] matrix = new int[3, 4];
                    int rank = matrix.Rank;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let matrix = new Array(3).fill().map(() => new Array(4));
  let rank = 2;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	#endregion

	#region 扩展测试用例 - 方法引用

	/// <summary>
	/// 测试方法组引用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_MethodGroup()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    System.Func<int, int> func = Double;
                    int result = func(5);
                }

                int Double(int x) => x * 2;
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let func = this.Double.bind(this);
  let result = func(5);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试静态方法引用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_StaticMethod()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int result = Add(1, 2);
                }

                static int Add(int a, int b) => a + b;
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

	Assert.AreEqual(@"{
  let result = TestClass.Add(1, 2);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_NestedStaticTypeMembers_UseFlattenedRuntimeTypeName()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int field = A.Field;
                    int property = A.Value;
                    int result = A.GetNumbers();
                }

                static class A
                {
                    public static int Field = 1;
                    public static int Value => 2;
                    public static int GetNumbers() => 3;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let field = A.Field;
  let property = A.Value;
  let result = A.GetNumbers();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_RuntimeInstanceMethod_UsesImplicitEcmascriptMemberName()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var date = new Date(0);
                    var time = date.GetTime();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let date = new Date(0);
  let time = date.getTime();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_RuntimeStaticMethod_UsesImplicitEcmascriptMemberName()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var isInteger = Number.IsInteger(1);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let isInteger = Number.isInteger(1);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_ECMAScriptEnumField_EmitsStringLiteral()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var matcher = Intl.LocaleMatcher.BestFit;
                    var twoDigit = Intl.NumericTwoDigit.TwoDigit;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let matcher = ""best fit"";
  let twoDigit = ""2-digit"";
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_ReflectGet_UsesJsMemberName()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                public int Value { get; set; }

                void TestMethod()
                {
                    var value = Reflect.Get(new TestClass(), ""Value"");
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let value = Reflect.get(new TestClass, ""Value"");
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_NumberAndIntlApis_UseAlignedCSharpNames()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Number value = 1;
                    var text = value.ToString();
                    var localized = value.ToLocaleString();
                    var locales = Intl.NumberFormat.SupportedLocalesOf(""en-US"");
                    var options = new Intl.NumberFormat().ResolvedOptions();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let value = 1;
  let text = value.toString();
  let localized = value.toLocaleString();
  let locales = Intl.NumberFormat.supportedLocalesOf(""en-US"");
  let options = (new Intl.NumberFormat).resolvedOptions();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_RegExpProperties_UseJsMemberNames()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var regex = new RegExp(""a"", ""dguy"");
                    var flags = regex.Flags;
                    var sticky = regex.Sticky;
                    var unicode = regex.Unicode;
                    var dotAll = regex.DotAll;
                    var hasIndices = regex.HasIndices;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let regex = new RegExp(""a"", ""dguy"");
  let flags = regex.flags;
  let sticky = regex.sticky;
  let unicode = regex.unicode;
  let dotAll = regex.dotAll;
  let hasIndices = regex.hasIndices;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_ConsoleTimeEnd_UsesJsMemberName()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Console.Time(""work"");
                    Console.TimeEnd(""work"");
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  console.time(""work"");
  console.timeEnd(""work"");
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_DateNow_UsesJsMemberName()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var now = Date.Now();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let now = Date.now();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_ArrayFromAsync_UsesJsMemberName()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var items = Array<int>.FromAsync(new int[] { 1, 2, 3 });
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let items = Array.fromAsync([1, 2, 3]);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_ArrayVariadicApis_UseJsShapes()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var array = Array<int>.Of(1, 2, 3);
                    array.Push(4, 5);
                    array.Unshift(0);
                    array.Splice(2, 1, 8, 9);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = Array.of(1, 2, 3);
  array.push(4, 5);
  array.unshift(0);
  array.splice(2, 1, 8, 9);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_TypedArrayStaticApis_UseJsShapes()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var bytes = Uint8Array.Of(1, 2, 3);
                    var mapped = Uint8Array.From(new int[] { 1, 2, 3 }, (value, index) => (byte)(value + index));
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let bytes = Uint8Array.of(1, 2, 3);
  let mapped = Uint8Array.from([1, 2, 3], (value, index) => {
    return value + index;
  }, null);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_GlobalSymbolFactory_UsesJsHostName()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var token = Symbol_(""value"");
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

	Assert.AreEqual(@"{
  let token = Symbol(""value"");
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_ErrorMembers_UseJsMemberNames()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var error = new Error(""boom"");
                    var message = error.Message;
                    var cause = error.Cause;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let error = new Error(""boom"");
  let message = error.message;
  let cause = error.cause;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_ErrorOptionsConstructor_PreservesJsShape()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var error = new Error(""boom"", new ErrorOptions
                    {
                        Cause = ""root""
                    });
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let error = new Error(""boom"", { cause: ""root"" });
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_UserStaticMethod_DoesNotUseImplicitEcmascriptMemberName()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var value = Helper.DoWork();
                }
            }

            class Helper
            {
                public static int DoWork() => 1;
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let value = Helper.DoWork();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试扩展方法引用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ExtensionMethod()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new System.Collections.Generic.List<int> { 1, 2, 3 };
                    var doubled = list.Select(x => x * 2).ToList();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let list = [1, 2, 3];
  let doubled = Array.from(Array.from(list).map(x => {
    return x * 2;
  }));
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	#endregion

	#region 扩展测试用例 - 链式调用

	/// <summary>
	/// 测试链式方法调用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ChainedMethods()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string result = ""hello world"".ToUpper().Trim().Substring(0, 5);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let result = ""hello world"".toUpperCase().trim().substring(0, 0 + 5);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试链式属性访问
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ChainedProperties()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var date = System.DateTime.Now.Date;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let date = _d77d20d9d04e2b6b(_ee9dd166a34a2fa5());
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试混合链式调用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_MixedChaining()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int len = ""hello"".ToUpper().Length;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let len = ""hello"".toUpperCase().length;
}", script);
	}

	#endregion

	#region 扩展测试用例 - this和base

	/// <summary>
	/// 测试 this 引用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_This()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                private int _value;

                void TestMethod()
                {
                    this._value = 10;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  this._value = 10;
}", script);
	}

	/// <summary>
	/// 测试 this 作为参数
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ThisAsArgument()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Process(this);
                }

                void Process(TestClass obj) { }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  this.Process(this);
}", script);
	}

	#endregion
}
#endregion
