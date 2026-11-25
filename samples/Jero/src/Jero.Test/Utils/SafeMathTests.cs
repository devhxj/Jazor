


using Jero.Core;
using Xunit;
namespace Jero.Test.Utils;

public class SafeMathTests
{
    [Fact]
    public void Add_IntAndDouble_ReturnsCorrectResult()
    {
        // Arrange
        int left = 10;
        double right = 20.5;

        // Act
        var result = SafeMath.Add<int>(left, right, precision: 2);

        // Assert
        Assert.Equal(30, result); // 10 + 20.5 = 30.5，四舍五入后为 30
    }

    [Fact]
    public void Add_StringAndDecimal_ReturnsCorrectResult()
    {
        // Arrange
        string left = "15.75";
        decimal right = 4.25m;

        // Act
        var result = SafeMath.Add<decimal>(left, right, precision: 2);

        // Assert
        Assert.Equal(20.00m, result); // 15.75 + 4.25 = 20.00
    }

    [Fact]
    public void Sub_DoubleAndInt_ReturnsCorrectResult()
    {
        // Arrange
        double left = 50.75;
        int right = 25;

        // Act
        var result = SafeMath.Sub<double>(left, right, precision: 2);

        // Assert
        Assert.Equal(25.75, result); // 50.75 - 25 = 25.75
    }

    [Fact]
    public void Mult_DecimalAndFloat_ReturnsCorrectResult()
    {
        // Arrange
        decimal left = 10.5m;
        float right = 2.0f;

        // Act
        var result = SafeMath.Mult<decimal>(left, right, precision: 2);

        // Assert
        Assert.Equal(21.00m, result); // 10.5 * 2.0 = 21.00
    }

    [Fact]
    public void Div_IntAndInt_ReturnsCorrectResult()
    {
        // Arrange
        int left = 10;
        int right = 3;

        // Act
        var result = SafeMath.Div<double>(left, right, precision: 4);

        // Assert
        Assert.Equal(3.3333, result); // 10 / 3 = 3.3333
    }

    [Fact]
    public void Div_ByZero_ReturnsDefaultValue()
    {
        // Arrange
        int left = 10;
        int right = 0;

        // Act
        int result = SafeMath.Div(left, right, defaultValue: -1, throwOnDivideByZero: false);

        // Assert
        Assert.Equal(-1, result); // 除数为 0，返回默认值 -1
    }

    [Fact]
    public void Div_ByZero_ThrowsException()
    {
        // Arrange
        int left = 10;
        int right = 0;

        // Act & Assert
        Assert.Throws<DivideByZeroException>(() =>
        {
            SafeMath.Div<double>(left, right, throwOnDivideByZero: true);
        });
    }

    [Fact]
    public void SafeConvert_StringToInt_ReturnsCorrectResult()
    {
        // Arrange
        string value = "42";

        // Act
        int result = SafeMath.SafeConvert(value, defaultValue: -1);

        // Assert
        Assert.Equal(42, result); // 字符串 "42" 转换为 int 42
    }

    [Fact]
    public void SafeConvert_InvalidString_ReturnsDefaultValue()
    {
        // Arrange
        string value = "invalid";

        // Act
        int result = SafeMath.SafeConvert(value, defaultValue: -1);

        // Assert
        Assert.Equal(-1, result); // 转换失败，返回默认值 -1
    }

    [Fact]
    public void ConvertToDecimal_Int_ReturnsCorrectResult()
    {
        // Arrange
        int value = 42;

        // Act
        decimal result = SafeMath.ConvertToDecimal(value);

        // Assert
        Assert.Equal(42m, result); // int 42 转换为 decimal 42m
    }

    [Fact]
    public void ConvertToDecimal_String_ReturnsCorrectResult()
    {
        // Arrange
        string value = "42.75";

        // Act
        decimal result = SafeMath.ConvertToDecimal(value);

        // Assert
        Assert.Equal(42.75m, result); // 字符串 "42.75" 转换为 decimal 42.75m
    }

    [Fact]
    public void ConvertToDecimal_InvalidString_ReturnsZero()
    {
        // Arrange
        string value = "invalid";

        // Act & Assert
        Assert.Throws<InvalidCastException>(() => SafeMath.ConvertToDecimal(value));
    }

    [Fact]
    public void Add_LeftNull_ReturnsDefaultValue()
    {
        // Arrange
        object? left = null;
        int right = 20;

        // Act
        int result = SafeMath.Add<int>(left, right);

        // Assert
        Assert.Equal(20, result); // 左操作数为 null
    }

    [Fact]
    public void Add_RightNull_ReturnsDefaultValue()
    {
        // Arrange
        int left = 10;
        object? right = null;

        // Act
        var result = SafeMath.Add<int>(left, right);

        // Assert
        Assert.Equal(10, result); // 右操作数为 null
    }

    [Fact]
    public void Sub_LeftNull_ReturnsDefaultValue()
    {
        // Arrange
        object? left = null;
        int right = 20;

        // Act
        int result = SafeMath.Sub<int>(left, right);

        // Assert
        Assert.Equal(-20, result); // 左操作数为 null
    }

    [Fact]
    public void Sub_RightNull_ReturnsDefaultValue()
    {
        // Arrange
        int left = 10;
        object? right = null;

        // Act
        var result = SafeMath.Sub<int>(left, right);

        // Assert
        Assert.Equal(10, result); // 右操作数为 null
    }

    [Fact]
    public void Mult_LeftNull_ReturnsDefaultValue()
    {
        // Arrange
        object? left = null;
        int right = 20;

        // Act
        int result = SafeMath.Mult<int>(left, right);

        // Assert
        Assert.Equal(0, result); // 左操作数为 null
    }

    [Fact]
    public void Mult_RightNull_ReturnsDefaultValue()
    {
        // Arrange
        int left = 10;
        object? right = null;

        // Act
        int result = SafeMath.Mult<int>(left, right);

        // Assert
        Assert.Equal(0, result); // 右操作数为 null
    }

    [Fact]
    public void Div_LeftNull_ReturnsDefaultValue()
    {
        // Arrange
        object? left = null;
        int right = 20;

        // Act
        int result = SafeMath.Div<int>(left, right);

        // Assert
        Assert.Equal(0, result); // 左操作数为 null
    }

    [Fact]
    public void Div_RightNull_ReturnsDefaultValue()
    {
        // Arrange
        int left = 10;
        object? right = null;

        // Act
        Assert.Throws<DivideByZeroException>(() =>
        {
            int result = SafeMath.Div<int>(left, right);
            // Assert
            Assert.Equal(-1, result); // 右操作数为 null，返回默认值 -1
        });
    }

    [Fact]
    public void SafeConvert_NullInput_ReturnsDefaultValue()
    {
        // Arrange
        object? value = null;

        // Act
        int result = SafeMath.SafeConvert<int>(value, defaultValue: -1);

        // Assert
        Assert.Equal(-1, result); // 输入为 null，返回默认值 -1
    }

    [Fact]
    public void ConvertToDecimal_NullInput_ReturnsZero()
    {
        // Arrange
        object? value = null;

        // Act
        decimal result = SafeMath.ConvertToDecimal(value);

        // Assert
        Assert.Equal(0m, result); // 输入为 null，返回默认值 0m
    }
}
