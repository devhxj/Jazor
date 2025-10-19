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

	public void Aa()
    {
		ValueTuple<int,int> a = (7,8);
    }
}