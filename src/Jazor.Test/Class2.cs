using ECMAScript;
using static ECMAScript.Global;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1;

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


class TestClass
{
	void TestMethod()
	{
		var a = Math.PI * Math.SQRT1_2;
		var b = TypeOf(a);
		var c = RegExp("a");
		Console.Log("Hello, World!");

		var ab = int.TryParse("1", out var bb);

		TryParseDelegate cc = int.TryParse;
		cc("1", out var dd);

	}

	delegate bool TryParseDelegate(string s, out int result);

	[Description("@#test")]
	void TestMethod1(int a, string b)
	{

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
}


class TestClass22
{
	void TestMethod()
	{
		var point = new Point(1, 2);
		var (x, y) = point;
	}

	class Point
	{
		public int X { get; }
		public int Y { get; }

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
}

public static class TestClass34
{
	public static int A = 42;
	public static int ReadOnlyProperty
	{
		get => A;
	} 
}
			
public static class TestClass2222
{
	public static int A = 1;
	public static string B = "456";
	public const int C = 42;
	public static int P1 { get; set; }
	public static int P2 { get; }
	public static int P3
	{
		get { return P1; }
		set { }
	}

	public static int P4 => P1;

	public static string P5
	{
		get => B;
		set => B = value;
	}

	public static string? P6
	{
		get => field;
		set => field = value;
	}

	public static string? P7
	{
		get;
		set => field = value?.Trim();
	}

	public static string P8
	{
		get => B;
		set => B = value.Trim();
	}

	public static void Method() { }

	public static void Method(int a) { }

	public static int Method(int a, int b) => a + b;
}