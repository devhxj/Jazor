using ECMAScript.Common;

namespace ECMAScript.CLR;

[WhiteList("void", WhiteListOp.Allowed)]
[WhiteList("System.Nullable", WhiteListOp.Allowed)]
[WhiteList("System.ValueTuple", WhiteListOp.Allowed)]
[WhiteList("System.Array", WhiteListOp.Allowed)]
public class AssignModule
{
}
