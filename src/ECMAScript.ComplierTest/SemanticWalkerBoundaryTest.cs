using Acornima.Ast;
using ECMAScript.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace ECMAScript.ComplierTest;

/// <summary>
/// SemanticWalker 边界条件测试类
///
/// 本测试类验证各模块的边界条件和极端场景，涵盖：
/// - 位运算符边界：零、负数、最大位移
/// - 数值边界：MaxValue、MinValue、NaN、Infinity
/// - 深度嵌套：对象、数组、表达式
/// - 空值处理：null、空数组、空字符串
/// - 循环边界：空体、大次数、深度嵌套
/// </summary>
[TestClass]
public sealed class SemanticWalkerBoundaryTest
{
	private static IBlockOperation GetBlockOperation(string code)
	{
		var usings = @"
        global using System;
        global using System.Collections.Generic;
        global using System.Linq;";

		var compilation = CSharpCompilation.Create(
			"TestAssembly",
			syntaxTrees: [
			  CSharpSyntaxTree.ParseText(usings),
				  CSharpSyntaxTree.ParseText(code)
			],
			references: Basic.Reference.Assemblies.Net100.References.All,
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

		var methodDeclaration = root.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault();
		if (methodDeclaration?.Body is not null)
		{
			var operation = semanticModel.GetOperation(methodDeclaration.Body) as IBlockOperation;
			if (operation is not null)
				return operation;
		}

		throw new InvalidOperationException("未找到可分析的操作");
	}

	#region 位运算符边界测试

	/// <summary>
	/// 测试位运算符 - 与零进行位运算
	/// C# 示例：x & 0, x | 0, x ^ 0
	/// 转换结果：JavaScript 位运算符
	/// </summary>
	[TestMethod]
	public void BitwiseOp_WithZero()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int x = 5;
					int andResult = x & 0;
					int orResult = x | 0;
					int xorResult = x ^ 0;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = 5;
  let andResult = x & 0;
  let orResult = x | 0;
  let xorResult = x ^ 0;
}", script);
	}

	/// <summary>
	/// 测试位运算符 - 与所有位为1的值进行运算
	/// C# 示例：x & 0xFF, x | 0xFF
	/// 转换结果：JavaScript 位运算符
	/// </summary>
	[TestMethod]
	public void BitwiseOp_WithAllOnes()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int x = 5;
					int andResult = x & 0xFF;
					int orResult = x | 0xFF;
					int xorResult = x ^ 0xFF;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = 5;
  let andResult = x & 255;
  let orResult = x | 255;
  let xorResult = x ^ 255;
}", script);
	}

	/// <summary>
	/// 测试位运算符 - 负数位运算
	/// C# 示例：-1 & x, -1 | x
	/// 转换结果：JavaScript 位运算符
	/// </summary>
	[TestMethod]
	public void BitwiseOp_WithNegativeNumbers()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int x = 5;
					int andResult = x & -1;
					int orResult = x | -1;
					int xorResult = x ^ -1;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = 5;
  let andResult = x & -1;
  let orResult = x | -1;
  let xorResult = x ^ -1;
}", script);
	}

	/// <summary>
	/// 测试位移运算符 - 移位0位
	/// C# 示例：x << 0, x >> 0, x >>> 0
	/// 转换结果：JavaScript 位移运算符
	/// </summary>
	[TestMethod]
	public void ShiftOp_ByZero()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int x = 5;
					int leftShift = x << 0;
					int rightShift = x >> 0;
					int unsignedRightShift = x >>> 0;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = 5;
  let leftShift = x << 0;
  let rightShift = x >> 0;
  let unsignedRightShift = x >>> 0;
}", script);
	}

	/// <summary>
	/// 测试位移运算符 - 移位31位（int 最大位移）
	/// C# 示例：x << 31, x >> 31
	/// 转换结果：JavaScript 位移运算符
	/// </summary>
	[TestMethod]
	public void ShiftOp_MaxBits()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int x = 1;
					int leftShift = x << 31;
					int rightShift = x >> 31;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = 1;
  let leftShift = x << 31;
  let rightShift = x >> 31;
}", script);
	}

	#endregion

	#region 数值边界测试

	/// <summary>
	/// 测试整数值边界 - int.MaxValue
	/// C# 示例：int x = int.MaxValue; x + 1
	/// 转换结果：JavaScript 数值运算
	/// </summary>
	[TestMethod]
	public void NumericBoundary_IntMaxValue()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int max = int.MaxValue;
					int result = max + 1;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let max = 2147483647;
  let result = max + 1;
}", script);
	}

	/// <summary>
	/// 测试整数值边界 - int.MinValue
	/// C# 示例：int x = int.MinValue; x - 1
	/// 转换结果：JavaScript 数值运算
	/// </summary>
	[TestMethod]
	public void NumericBoundary_IntMinValue()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int min = int.MinValue;
					int result = min - 1;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let min = -2147483648;
  let result = min - 1;
}", script);
	}

	/// <summary>
	/// 测试双精度浮点数边界 - double.MaxValue
	/// C# 示例：double x = double.MaxValue
	/// 转换结果：JavaScript Number.MAX_VALUE
	/// </summary>
	[TestMethod]
	public void NumericBoundary_DoubleMaxValue()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					double max = double.MaxValue;
					double result = max * 2;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let max = Number.MAX_VALUE;
  let result = max * 2;
}", script);
	}

	/// <summary>
	/// 测试特殊浮点值 - NaN
	/// C# 示例：double x = double.NaN; x != x
	/// 转换结果：JavaScript NaN
	/// </summary>
	[TestMethod]
	public void NumericBoundary_NaN()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					double nan = double.NaN;
					bool check = nan != nan;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		// C# != 转换为 JavaScript !=
		Assert.AreEqual(@"{
  let nan = NaN;
  let check = nan != nan;
}", script);
	}

	/// <summary>
	/// 测试特殊浮点值 - PositiveInfinity
	/// C# 示例：double x = double.PositiveInfinity
	/// 转换结果：JavaScript Infinity
	/// </summary>
	[TestMethod]
	public void NumericBoundary_PositiveInfinity()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					double inf = double.PositiveInfinity;
					bool check = inf > 0;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let inf = Infinity;
  let check = inf > 0;
}", script);
	}

	/// <summary>
	/// 测试特殊浮点值 - NegativeInfinity
	/// C# 示例：double x = double.NegativeInfinity
	/// 转换结果：JavaScript -Infinity
	/// </summary>
	[TestMethod]
	public void NumericBoundary_NegativeInfinity()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					double inf = double.NegativeInfinity;
					bool check = inf < 0;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let inf = -Infinity;
  let check = inf < 0;
}", script);
	}

	/// <summary>
	/// 测试除零操作（浮点）
	/// C# 示例：double x = 1.0 / 0.0, 0.0 / 0.0
	/// 转换结果：JavaScript Infinity/NaN
	/// </summary>
	[TestMethod]
	public void NumericBoundary_DivideByZero_Double()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					double x = 1.0 / 0.0;
					double y = -1.0 / 0.0;
					double z = 0.0 / 0.0;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = 1 / 0;
  let y = -1 / 0;
  let z = 0 / 0;
}", script);
	}

	#endregion

	#region 深度嵌套测试

	/// <summary>
	/// 测试深度嵌套的对象创建（5层）
	/// C# 示例：new { A = new { B = new { C = new { D = new { E = 1 } } } } }
	/// 转换结果：JavaScript 对象字面量（单行格式）
	/// </summary>
	[TestMethod]
	public void NestedObjectCreation_DeepNesting()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					var obj = new {
						Level1 = new {
							Level2 = new {
								Level3 = new {
									Level4 = new {
										Value = 42
									}
								}
							}
						}
					};
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let obj = { Level1: { Level2: { Level3: { Level4: { Value: 42 } } } } };
}", script);
	}

	/// <summary>
	/// 测试深度嵌套的属性访问（5层）
	/// C# 示例：obj.A.B.C.D.E
	/// 转换结果：JavaScript 属性访问链
	/// </summary>
	[TestMethod]
	public void NestedPropertyAccess_DeepChain()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					var obj = new {
						A = new {
							B = new {
								C = new {
									D = new {
										E = 42
									}
								}
							}
						}
					};
					int value = obj.A.B.C.D.E;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let obj = { A: { B: { C: { D: { E: 42 } } } } };
  let value = obj.A.B.C.D.E;
}", script);
	}

	/// <summary>
	/// 测试深度嵌套的条件表达式（4层）
	/// C# 示例：a ? (b ? (c ? 1 : 2) : 3) : 4
	/// 转换结果：JavaScript 嵌套三元运算符
	/// </summary>
	[TestMethod]
	public void NestedTernary_DeepNesting()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					bool a = true, b = true, c = true;
					int result = a ? (b ? (c ? 1 : 2) : 3) : 4;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		// JavaScript 三元运算符是右结合的
		Assert.AreEqual(@"{
  let a = true, b = true, c = true;
  let result = a ? b ? c ? 1 : 2 : 3 : 4;
}", script);
	}

	/// <summary>
	/// 测试深度嵌套的数组（3维）
	/// C# 示例：new int[][][] { new int[][] { new int[] { 1, 2 } } }
	/// 转换结果：JavaScript 多维数组
	/// </summary>
	[TestMethod]
	public void NestedArray_DeepNesting()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					var arr = new int[][][] {
						new int[][] {
							new int[] { 1, 2 }
						}
					};
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let arr = [[[1, 2]]];
}", script);
	}

	#endregion

	#region 空值和默认值测试

	/// <summary>
	/// 测试 null 合并运算符链
	/// C# 示例：a ?? b ?? c ?? d
	/// 转换结果：JavaScript ?? 链
	/// </summary>
	[TestMethod]
	public void NullCoalescing_LongChain()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string? a = null;
					string? b = null;
					string? c = null;
					string d = ""default"";
					string result = a ?? b ?? c ?? d;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		// 变量没有被合并，因为它们的类型可能不同
		// ?? 运算符转换为嵌套形式
		Assert.AreEqual(@"{
  let a = null;
  let b = null;
  let c = null;
  let d = ""default"";
  let result = a ?? (b ?? (c ?? d));
}", script);
	}

	/// <summary>
	/// 测试默认值表达式 - 复杂类型
	/// C# 示例：default(List<int>), default(string?)
	/// 转换结果：JavaScript 默认值
	/// </summary>
	[TestMethod]
	public void DefaultValue_ComplexType()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					List<int>? list = default(List<int>?);
					string? str = default(string?);
					int num = default(int);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		// string? default 被转换为空字符串
		Assert.AreEqual(@"{
  let list = null;
  let str = '';
  let num = 0;
}", script);
	}

	/// <summary>
	/// 测试空数组创建
	/// C# 示例：new int[0], new int[] { }
	/// 转换结果：JavaScript 空数组
	/// </summary>
	[TestMethod]
	public void EmptyArray_Creation()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int[] empty1 = new int[0];
					int[] empty2 = new int[] { };
					int[] empty3 = { };
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		// 数组变量声明没有被合并
		// new int[0] 被转换为 new Array(0)
		Assert.AreEqual(@"{
  let empty1 = new Array(0);
  let empty2 = [];
  let empty3 = [];
}", script);
	}

	/// <summary>
	/// 测试空字符串
	/// C# 示例：string.Empty, """"
	/// 转换结果：JavaScript 空字符串
	/// </summary>
	[TestMethod]
	public void EmptyString_Variants()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string empty1 = string.Empty;
					string empty2 = """";
					string empty3 = """";
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		// string.Empty 被转换为字段引用 Empty
		// 空字符串使用双引号
		Assert.AreEqual(@"{
  let empty1 = Empty;
  let empty2 = """";
  let empty3 = """";
}", script);
	}

	#endregion

	#region 复杂表达式边界测试

	/// <summary>
	/// 测试复杂的逻辑运算符组合
	/// C# 示例：a && b || c && d && e || f
	/// 转换结果：JavaScript 逻辑表达式
	/// </summary>
	[TestMethod]
	public void ComplexLogicalExpression_MixedOperators()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					bool a = true, b = false, c = true, d = true, e = false, f = true;
					bool result = a && b || c && d && e || f;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let a = true, b = false, c = true, d = true, e = false, f = true;
  let result = a && b || c && d && e || f;
}", script);
	}

	/// <summary>
	/// 测试复杂算术表达式（混合运算符）
	/// C# 示例：a + b * c - d / e % f
	/// 转换结果：JavaScript 算术表达式
	/// </summary>
	[TestMethod]
	public void ComplexArithmeticExpression_MixedOperators()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int a = 1, b = 2, c = 3, d = 4, e = 5, f = 6;
					int result = a + b * c - d / e % f;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let a = 1, b = 2, c = 3, d = 4, e = 5, f = 6;
  let result = a + b * c - d / e % f;
}", script);
	}

	/// <summary>
	/// 测试嵌套的 Lambda 表达式（3层）
	/// C# 示例：x => y => z => x + y + z
	/// 转换结果：JavaScript 箭头函数
	/// </summary>
	[TestMethod]
	public void NestedLambda_DeepNesting()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					Func<int, Func<int, Func<int, int>>> nested = x => y => z => x + y + z;
					var result = nested(1)(2)(3);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		// C# 表达式 lambda 被转换为带 return 的函数
		// 委托调用使用 Invoke 方法
		Assert.AreEqual(@"{
  let nested = x => {
    return y => {
      return z => {
        return x + y + z;
      };
    };
  };
  let result = nested.Invoke(1).Invoke(2).Invoke(3);
}", script);
	}

	#endregion

	#region 循环边界测试

	/// <summary>
	/// 测试循环 - 空循环体
	/// C# 示例：while (false) { }, for (int i = 0; i < 0; i++) { }
	/// 转换结果：JavaScript 空循环体
	/// </summary>
	[TestMethod]
	public void Loop_EmptyBody()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					while (false) { }
					for (int i = 0; i < 0; i++) { }
					do { } while (false);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		// 空语句被放在同一行，最后一个 do-while 的格式不同
		Assert.AreEqual(@"{
  while (false) { }
  for (let i = 0; i < 0; i++) { }
  do { }
  while (false);
}", script);
	}

	/// <summary>
	/// 测试循环 - 嵌套循环（3层）
	/// C# 示例：for (i) { for (j) { for (k) { } } }
	/// 转换结果：JavaScript 嵌套循环
	/// </summary>
	[TestMethod]
	public void Loop_DeepNesting()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					for (int i = 0; i < 10; i++)
					{
						for (int j = 0; j < 10; j++)
						{
							for (int k = 0; k < 10; k++)
							{
								int sum = i + j + k;
							}
						}
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		// 验证生成了三层 for 循环
		Assert.AreEqual(@"{
  for (let i = 0; i < 10; i++) {
    for (let j = 0; j < 10; j++) {
      for (let k = 0; k < 10; k++) {
        let sum = i + j + k;
      }
    }
  }
}", script);
	}

	/// <summary>
	/// 测试 foreach - 遍历空集合
	/// C# 示例：foreach (var item in new List<int>()) { }
	/// 转换结果：JavaScript for-of 循环
	/// </summary>
	[TestMethod]
	public void Foreach_EmptyCollection()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					var empty = new List<int>();
					foreach (var item in empty)
					{
						Console.WriteLine(item);
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let empty = [];
  for (item of empty) {
    Console.WriteLine(item);
  }
}", script);
	}

	/// <summary>
	/// 测试循环 - 大次数循环
	/// C# 示例：for (int i = 0; i < 1000000; i++)
	/// 转换结果：JavaScript for 循环
	/// </summary>
	[TestMethod]
	public void Loop_LargeIteration()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					for (int i = 0; i < 1000000; i++)
					{
						if (i == 500000) break;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		// C# == 被转换为 JavaScript ==
		Assert.AreEqual(@"{
  for (let i = 0; i < 1000000; i++) {
    if (i == 500000)
      break;
  }
}", script);
	}

	#endregion
}
