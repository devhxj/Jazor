using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

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

		// string.Empty 被白名单内联转换为空字符串 ""
		// 空字符串使用双引号
		Assert.AreEqual(@"{
  let empty1 = """";
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
  let result = nested(1)(2)(3);
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
    console.log(item);
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

	#region 扩展测试用例 - 数值溢出边界

	/// <summary>
	/// 测试 byte 边界值
	/// </summary>
	[TestMethod]
	public void NumericBoundary_ByteMaxMin()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					byte max = byte.MaxValue;
					byte min = byte.MinValue;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let max = 255;
  let min = 0;
}", script);
	}

	/// <summary>
	/// 测试 short 边界值
	/// </summary>
	[TestMethod]
	public void NumericBoundary_ShortMaxMin()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					short max = short.MaxValue;
					short min = short.MinValue;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let max = 32767;
  let min = -32768;
}", script);
	}

	/// <summary>
	/// 测试 uint 边界值
	/// </summary>
	[TestMethod]
	public void NumericBoundary_UIntMaxMin()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					uint max = uint.MaxValue;
					uint min = uint.MinValue;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let max = 4294967295;
  let min = 0;
}", script);
	}

	/// <summary>
	/// 测试 ulong 边界值
	/// </summary>
	[TestMethod]
	public void NumericBoundary_ULongMaxMin()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					ulong max = ulong.MaxValue;
					ulong min = ulong.MinValue;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let max = 18446744073709552000;
  let min = 0;
}", script);
	}

	/// <summary>
	/// 测试 float 边界值
	/// </summary>
	[TestMethod]
	public void NumericBoundary_FloatMaxMin()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					float max = float.MaxValue;
					float min = float.MinValue;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let max = 3.4028235e+38;
  let min = -3.4028235e+38;
}", script);
	}

	/// <summary>
	/// 测试 decimal 边界值
	/// </summary>
	[TestMethod]
	public void NumericBoundary_DecimalMaxMin()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					decimal max = decimal.MaxValue;
					decimal min = decimal.MinValue;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let max = 79228162514264300000000000000;
  let min = -79228162514264300000000000000;
}", script);
	}

	#endregion

	#region 扩展测试用例 - 特殊字符边界

	/// <summary>
	/// 测试 Unicode 字符串
	/// </summary>
	[TestMethod]
	public void StringBoundary_Unicode()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string chinese = ""你好世界"";
					string emoji = ""😀🎉"";
					string mixed = ""Hello世界"";
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let chinese = ""你好世界"";
  let emoji = ""😀🎉"";
  let mixed = ""Hello世界"";
}", script);
	}

	/// <summary>
	/// 测试转义字符
	/// </summary>
	[TestMethod]
	public void StringBoundary_EscapeCharacters()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string tab = ""\t"";
					string newline = ""\n"";
					string carriage = ""\r"";
					string backslash = ""\\"";
					string quote = ""\"""";
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let tab = ""\t"";
  let newline = ""\n"";
  let carriage = ""\r"";
  let backslash = ""\\"";
  let quote = ""\"""";
}", script);
	}

	/// <summary>
	/// 测试长字符串
	/// </summary>
	[TestMethod]
	public void StringBoundary_LongString()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string longText = ""This is a very long string that contains many characters and words to test how the compiler handles longer text content in string literals. It should be converted correctly to JavaScript without any issues."";
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.IsTrue(script!.Contains("let longText = "));
		Assert.IsTrue(script!.Contains("very long string"));
	}

	#endregion

	#region 扩展测试用例 - 数组边界

	/// <summary>
	/// 测试大数组初始化
	/// </summary>
	[TestMethod]
	public void ArrayBoundary_LargeArray()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int[] large = new int[1000];
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let large = new Array(1000);
}", script);
	}

	/// <summary>
	/// 测试多维数组
	/// </summary>
	[TestMethod]
	public void ArrayBoundary_MultiDimensional()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int[,] matrix2d = new int[3, 4];
					int[,,] matrix3d = new int[2, 3, 4];
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let matrix2d = new Array(3).fill().map(() => new Array(4));
  let matrix3d = new Array(2).fill().map(() => new Array(3).fill().map(() => new Array(4)));
}", script);
	}

	/// <summary>
	/// 测试数组元素类型边界
	/// </summary>
	[TestMethod]
	public void ArrayBoundary_ElementTypes()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int[] ints = { 1, 2, 3 };
					string[] strings = { ""a"", ""b"", ""c"" };
					bool[] bools = { true, false, true };
					double[] doubles = { 1.1, 2.2, 3.3 };
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let ints = [1, 2, 3];
  let strings = [""a"", ""b"", ""c""];
  let bools = [true, false, true];
  let doubles = [1.1, 2.2, 3.3];
}", script);
	}

	#endregion

	#region 扩展测试用例 - 对象边界

	/// <summary>
	/// 测试空对象初始化
	/// </summary>
	[TestMethod]
	public void ObjectBoundary_EmptyObject()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					object empty = new object();
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let empty = {};
}", script);
	}

	/// <summary>
	/// 测试匿名对象属性名边界
	/// </summary>
	[TestMethod]
	public void ObjectBoundary_AnonymousPropertyName()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					var obj = new
					{
						name = ""test"",
						Name = ""Test"",
						_name = ""_test"",
						Name123 = ""test123""
					};
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let obj = { name: ""test"", Name: ""Test"", _name: ""_test"", Name123: ""test123"" };
}", script);
	}

	/// <summary>
	/// 测试对象属性数量边界
	/// </summary>
	[TestMethod]
	public void ObjectBoundary_ManyProperties()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					var obj = new { a = 1, b = 2, c = 3, d = 4, e = 5, f = 6, g = 7, h = 8, i = 9, j = 10 };
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let obj = { a: 1, b: 2, c: 3, d: 4, e: 5, f: 6, g: 7, h: 8, i: 9, j: 10 };
}", script);
	}

	#endregion

	#region 扩展测试用例 - 表达式边界

	/// <summary>
	/// 测试深层嵌套表达式
	/// </summary>
	[TestMethod]
	public void ExpressionBoundary_DeepNesting()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int a = 1;
					int result = ((a + 1) + 2) + ((a + 3) + 4);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let a = 1;
  let result = a + 1 + 2 + a + 3 + 4;
}", script);
	}

	/// <summary>
	/// 测试复杂条件表达式
	/// </summary>
	[TestMethod]
	public void ExpressionBoundary_ComplexCondition()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					bool a = true, b = false, c = true, d = false;
					bool result = (a || b) && (c || d) || !(a && b);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let a = true, b = false, c = true, d = false;
  let result = (a || b) && (c || d) || !(a && b);
}", script);
	}

	/// <summary>
	/// 测试运算符优先级
	/// </summary>
	[TestMethod]
	public void ExpressionBoundary_OperatorPrecedence()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int a = 1, b = 2, c = 3;
					int result1 = a + b * c;
					int result2 = (a + b) * c;
					int result3 = a * b + c;
					int result4 = a * (b + c);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let a = 1, b = 2, c = 3;
  let result1 = a + b * c;
  let result2 = (a + b) * c;
  let result3 = a * b + c;
  let result4 = a * (b + c);
}", script);
	}

	#endregion

	#region 扩展测试用例 - 循环边界变体

	/// <summary>
	/// 测试零次迭代循环
	/// </summary>
	[TestMethod]
	public void LoopBoundary_ZeroIterations()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int count = 0;
					for (int i = 0; i < 0; i++)
					{
						count++;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let count = 0;
  for (let i = 0; i < 0; i++) {
    count++;
  }
}", script);
	}

	/// <summary>
	/// 测试单次迭代循环
	/// </summary>
	[TestMethod]
	public void LoopBoundary_SingleIteration()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					int count = 0;
					for (int i = 0; i < 1; i++)
					{
						count++;
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let count = 0;
  for (let i = 0; i < 1; i++) {
    count++;
  }
}", script);
	}

	/// <summary>
	/// 测试负步长循环
	/// </summary>
	[TestMethod]
	public void LoopBoundary_NegativeStep()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					for (int i = 10; i >= 0; i -= 2)
					{
						Console.WriteLine(i);
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  for (let i = 10; i >= 0; i -= 2) {
    console.log(i);
  }
}", script);
	}

	#endregion

	#region 扩展测试用例 - 递归边界

	/// <summary>
	/// 测试简单递归方法
	/// </summary>
	[TestMethod]
	public void RecursionBoundary_Simple()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				int Factorial(int n)
				{
					if (n <= 1) return 1;
					return n * Factorial(n - 1);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  if (n <= 1)
    return 1;
  return n * this.Factorial(n - 1);
}", script);
	}

	/// <summary>
	/// 测试相互递归
	/// </summary>
	[TestMethod]
	public void RecursionBoundary_Mutual()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				bool IsEven(int n)
				{
					if (n == 0) return true;
					return IsOdd(n - 1);
				}

				bool IsOdd(int n)
				{
					if (n == 0) return false;
					return IsEven(n - 1);
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.IsTrue(script!.Contains("IsEven"));
		Assert.IsTrue(script!.Contains("IsOdd"));
	}

	#endregion

	#region 扩展测试用例 - 更多数值边界

	/// <summary>
	/// 测试 byte 边界
	/// </summary>
	[TestMethod]
	public void NumericBoundary_ByteValues()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					byte max = 255;
					byte min = 0;
					byte mid = 128;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let max = 255;
  let min = 0;
  let mid = 128;
}", script);
	}

	/// <summary>
	/// 测试 short 边界
	/// </summary>
	[TestMethod]
	public void NumericBoundary_ShortValues()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					short max = 32767;
					short min = -32768;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let max = 32767;
  let min = -32768;
}", script);
	}

	/// <summary>
	/// 测试 uint 边界
	/// </summary>
	[TestMethod]
	public void NumericBoundary_UIntValues()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					uint max = 4294967295;
					uint min = 0;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.IsNotNull(script);
	}

	/// <summary>
	/// 测试 ulong 边界
	/// </summary>
	[TestMethod]
	public void NumericBoundary_ULongValues()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					ulong big = 18446744073709551615UL;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.IsNotNull(script);
	}

	/// <summary>
	/// 测试 float 精度
	/// </summary>
	[TestMethod]
	public void NumericBoundary_FloatPrecision()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					float f1 = 0.1f;
					float f2 = 0.2f;
					float sum = f1 + f2;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.IsNotNull(script);
	}

	/// <summary>
	/// 测试 decimal 精度
	/// </summary>
	[TestMethod]
	public void NumericBoundary_DecimalPrecision()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					decimal d1 = 0.1m;
					decimal d2 = 0.2m;
					decimal sum = d1 + d2;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.IsNotNull(script);
	}

	#endregion

	#region 扩展测试用例 - 更多字符串边界

	/// <summary>
	/// 测试特殊字符字符串
	/// </summary>
	[TestMethod]
	public void StringBoundary_SpecialChars()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string special = ""Tab:\\t Newline:\\n Quote:\\"" Backslash:\\"";
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.IsNotNull(script);
	}

	/// <summary>
	/// 测试原始字符串
	/// </summary>
	[TestMethod]
	public void StringBoundary_RawString()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					string raw = ""No escapes needed: \n \t \"" "";
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.IsNotNull(script);
	}

	#endregion

	#region 扩展测试用例 - 对象边界

	/// <summary>
	/// 测试空对象
	/// </summary>
	[TestMethod]
	public void ObjectBoundary_Null()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					object? obj = null;
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let obj = null;
}", script);
	}

	/// <summary>
	/// 测试空对象属性访问
	/// </summary>
	[TestMethod]
	public void ObjectBoundary_NullPropertyAccess()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					TestClass? obj = null;
					if (obj != null)
					{
						Console.WriteLine(obj.ToString());
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.IsNotNull(script);
	}

	#endregion

	#region 扩展测试用例 - 条件边界

	/// <summary>
	/// 测试始终为真的条件
	/// </summary>
	[TestMethod]
	public void ConditionBoundary_AlwaysTrue()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					if (true)
					{
						Console.WriteLine(""always"");
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  if (true) {
    console.log(""always"");
  }
}", script);
	}

	/// <summary>
	/// 测试始终为假的条件
	/// </summary>
	[TestMethod]
	public void ConditionBoundary_AlwaysFalse()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					if (false)
					{
						Console.WriteLine(""never"");
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  if (false) {
    console.log(""never"");
  }
}", script);
	}

	/// <summary>
	/// 测试复杂条件
	/// </summary>
	[TestMethod]
	public void ConditionBoundary_Complex()
	{
		var block = GetBlockOperation(@"
			class TestClass
			{
				void TestMethod()
				{
					bool a = true;
					bool b = false;
					bool c = true;
					if ((a && b) || (c && !b))
					{
						Console.WriteLine(""complex"");
					}
				}
			}
		");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.IsNotNull(script);
	}

	#endregion
}
