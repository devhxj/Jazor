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

class TestClass
{
	void TestMethod()
	{
		int[] array = [1, 2, 3, 4, 5];
		if (array is [.. var rest])
		{
			Console.WriteLine(rest.Length);
		}
	}

	static string Demo1(int[] a) => a switch
	{
		[> 0, _, _, _, < 0] => "头正尾负",
		[> 0, _, _, < 0] => "头正尾负",
		[> 0, .., < 0] => "头正尾负",
		[> 0, .., >=0] => "头正尾负",
		_ => "其它"
	};
}