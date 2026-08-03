using System.Reflection;
using ECMAScript.Contract;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class ScalarHashCodeWhitelistTests
{
    [TestMethod]
    public void ScalarHashCodes_UseCarrierAppropriateMappings()
    {
        var cases = new (Type Module, string Member, Op Op)[]
        {
            (typeof(Jazor.CLR.BooleanModule), "override bool.GetHashCode()", Op.Inline),
            (typeof(Jazor.CLR.ByteModule), "override byte.GetHashCode()", Op.Inline),
            (typeof(Jazor.CLR.SByteModule), "override sbyte.GetHashCode()", Op.Inline),
            (typeof(Jazor.CLR.Int16Module), "override short.GetHashCode()", Op.Inline),
            (typeof(Jazor.CLR.UInt16Module), "override ushort.GetHashCode()", Op.Inline),
            (typeof(Jazor.CLR.Int32Module), "override int.GetHashCode()", Op.Inline),
            (typeof(Jazor.CLR.UInt32Module), "override uint.GetHashCode()", Op.Inline),
            (typeof(Jazor.CLR.CharModule), "override char.GetHashCode()", Op.Inline),
            (typeof(Jazor.CLR.Int64Module), "override long.GetHashCode()", Op.Import),
            (typeof(Jazor.CLR.UInt64Module), "override ulong.GetHashCode()", Op.Import),
            (typeof(Jazor.CLR.Int128Module), "override System.Int128.GetHashCode()", Op.Import),
            (typeof(Jazor.CLR.UInt128Module), "override System.UInt128.GetHashCode()", Op.Import),
            (typeof(Jazor.CLR.HalfModule), "override System.Half.GetHashCode()", Op.Import),
            (typeof(Jazor.CLR.SingleModule), "override float.GetHashCode()", Op.Import),
            (typeof(Jazor.CLR.DoubleModule), "override double.GetHashCode()", Op.Import),
            (typeof(Jazor.CLR.StringModule), "override string.GetHashCode()", Op.Import)
        };

        foreach (var testCase in cases)
        {
            var mapping = testCase.Module
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Select(static method => method.GetCustomAttribute<JazorAttribute>())
                .Single(attribute => string.Equals(attribute?.Member, testCase.Member, StringComparison.Ordinal));

            Assert.IsNotNull(mapping, testCase.Member);
            Assert.AreEqual(testCase.Op, mapping.Op, testCase.Member);
        }
    }
}
