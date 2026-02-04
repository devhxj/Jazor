using ECMAScript.Common;

namespace ECMAScript.CLR;

[WhiteList("void", WhiteListOp.Allowed)]
public record VoidModule;

[WhiteList("System.Nullable", WhiteListOp.Allowed)]
public record NullableModule;

[WhiteList("System.Array", WhiteListOp.Allowed)]
public record ArrayModule;
