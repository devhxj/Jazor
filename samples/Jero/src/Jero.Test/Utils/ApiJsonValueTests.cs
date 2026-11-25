using System;
using System.Text.Json;
using Jero.Core.Service;
using Xunit;

namespace Jero.Test.Utils;

public class ApiJsonValueTests
{
    [Fact]
    public void SqlParameters_PreserveLargeIntegersAndExplicitNull()
    {
        var data = ApiJsonValue.ParseObject("""{"Tenant_ID":9007199254740993,"Cleared":null}""");

        Assert.Equal(9007199254740993L, Assert.IsType<long>(FuncList.TransJsonObjectToSugarPara(data["Tenant_ID"])));
        Assert.Same(DBNull.Value, FuncList.TransJsonObjectToSugarPara(data["Cleared"]));
        Assert.Equal("""{"Tenant_ID":9007199254740993,"Cleared":null}""", JsonSerializer.Serialize(data, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }

    [Fact]
    public void SqlArrayParameters_UseTypedClrArrays()
    {
        Assert.Equal(new[] { 9007199254740993L, 9007199254740995L },
            Assert.IsType<long[]>(ReadArray("""[9007199254740993,"9007199254740995"]""")));
        Assert.Equal(new[] { 1.25m, 2m },
            Assert.IsType<decimal[]>(ReadArray("""[1.25,2]""")));
        Assert.Equal(new[] { true, false },
            Assert.IsType<bool[]>(ReadArray("""[true,"false"]""")));
        Assert.Equal(new[] { new DateTime(2026, 11, 25, 13, 14, 15, DateTimeKind.Utc) },
            Assert.IsType<DateTime[]>(ReadArray("""["2026-11-25T13:14:15Z"]""")));
        Assert.Equal(new string?[] { "dbo.Account", null, "2026-11-25T13:14:15Z" },
            Assert.IsType<string[]>(ReadArray("""["dbo.Account",null,"2026-11-25T13:14:15Z"]""")));
    }

    private static object ReadArray(string json) =>
        FuncList.TransJsonArrayToSugarPara(ApiJsonValue.ParseArray(json));
}
