using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1;

record FileRequest(string Name, string DotExt, string FullPath, string Content);

public class Result
{
	public int errCode { get; set; }

	public string? errMsg { get; set; }

	public Data? data { get; set; }

	public bool success { get; set; }

	void TestMethod()
	{
		var x = 0;
		for (int i = 0, j = 1, z = x++; i < 10; i++, j++)
		{
			Console.WriteLine(i * j * z);
		}
	}
}

public class Data
{
	public int currentPage { get; set; }
	public int pageSize { get; set; }
	public Sort? sort { get; set; }
	public int recordCount { get; set; }
	public int pageCount { get; set; }
	public Buyer[]? resultList { get; set; }
	public int orgId { get; set; }
	public int orgLevel { get; set; }
	public object? userCode { get; set; }
	public object? gmf_mc { get; set; }
	public int startIndex { get; set; }
}

public class Sort
{
	public bool unsorted { get; set; }
	public bool sorted { get; set; }
	public bool empty { get; set; }
}

public class Buyer
{
	//public int id { get; set; }
	//public string? identification { get; set; }
	//public int authorCreated { get; set; }
	//public int authorUpdated { get; set; }
	public string? created { get; set; }
	public string? updated { get; set; }
	public string? enable { get; set; }
	public int ver { get; set; }
	//public string? userCode { get; set; }
	public string? gmf_nsrsbh { get; set; }
	public string? gmf_mc { get; set; }
	public string? gmf_dzdh { get; set; }
	//public string? gmf_dh { get; set; }
	public string? gmf_yhzh { get; set; }
	public string? email { get; set; }
	public string? phoneNumber { get; set; }
	public string? notes { get; set; }
	public string? company { get; set; }
}

class Point
{
	public int X { get; set; }
	public int Y { get; set; }

	public Point(int x, int y)
	{
		X = x;
		Y = y;
	}

	public void Deconstruct(out int x, out int y)
	{
		x = X;
		y = Y;
	}
}


class TestClass1
{
	void TestMethod()
	{
		var obj = new A { A1 = { B1 = "Test", B2 = { C1 = "a", C2 = 9 } }, A2 = "value" };
	}

	class A
	{
		public B? A1 { get; set; }
		public string? A2 { get; set; }
	}

	class B
	{
		public string? B1 { get; set; }
		public C? B2 { get; set; }
	}

	class C
	{
		public string? C1 { get; set; }
		public int C2 { get; set; }
	}

	class TestClass
	{
		void TestMethod()
		{
			var list = new List<List<int>> {
				new(){1},new(){2,4},new(){3},
			};
		}
	}
}

class TestClass2
{
	void TestMethod()
	{
		int[] array = [1, 2, 3, 4, 5];
		if (array is [.. var rest])
		{
			Console.WriteLine(rest.Length);
		}

		if (array is [_, _, .. var r1])
		{
			Console.WriteLine(r1.Length);
		}

		if (array is [_, _, .. var r2])
		{
			Console.WriteLine(r2.Length);
		}

		if (array is [var first, .. var rest1])
		{
			Console.WriteLine(first);
			Console.WriteLine(rest1.Length);
		}

		int value = 5;
		bool result = value > 9 && (value is > 0 and < 10 and not 5) && (value is var x && x < 10);
		switch (value)
		{
			case var s when s > 0:
				Console.WriteLine(">0");
				break;
			case 1:
				Console.WriteLine("1");
				break;
			case 2:
			default:
				Console.WriteLine("2");
				break;
		}


		object obj = "hello";
		bool result1 = obj is string { Length: > 0 };

		object obj1 = 42;
		bool result2 = obj1 is int x1 and int y1;
	}

	static string Demo1(int[] a) => a switch
	{
		[> 0, _, _, _, < 0] => "头正尾负",
		[> 0, _, _, < 0] => "头正尾负",
		[> 0, .., < 0] => "头正尾负",
		[> 0, .., >= 0] => "头正尾负",
		_ => "其它"
	};

	void TestMethod1()
	{
		object obj = DateTime.Now;
		bool a = obj is DateTime;
		string input = "123";
		if (int.TryParse(input, out var result))
		{
			// 使用 result
		}
		
		var dict = new System.Collections.Generic.Dictionary<string, int>();
		if (dict.TryGetValue("key", out int value))
		{
			// 使用 value
		}

		var v = 5;
		bool r = v is not 5;
	}	
}

class TestClass
{
	void TestMethod()
	{
		int value = 5;
		string result = Get5(value) switch
		{
			> 0 and < 10 => "Small",
			>= 10 => "Large",
			_ => "Unknown"
		};
	}

	static int Get5(int x)
	{
		return x switch
		{
			> 0 and < 10 => 5,
			_ => 0
		};
	}

}

class TestClass3
{
	void TestMethod()
	{
		var obj = new { X = 1, Y = 2 };
		string result = obj switch
		{
			{ X: 1, Y: 2 } => "Point(1, 2)",
			{ X: var x } when x > 0 => "Positive X",
			_ => "Other"
		};
	}

	void TestMethod1()
	{
		int value = 5;
		string result = value switch
		{
			var x when x > 0 && x < 10 => "Small",
			var x when x >= 10 => "Large",
			_ => "Unknown"
		};
	}
}