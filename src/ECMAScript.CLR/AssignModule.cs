using ECMAScript.Common;

namespace ECMAScript.CLR;

[WhiteList("void", "void", WhiteListOp.Allowed)]
[WhiteList("System.Nullable", "System.Nullable", WhiteListOp.Allowed)]
[WhiteList("System.ValueTuple", "System.ValueTuple", WhiteListOp.Allowed)]
[WhiteList("System.Array", "System.Array", WhiteListOp.Allowed)]
public class AssignModule
{
}
