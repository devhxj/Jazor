namespace Jazor.CLR;

[Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.ParameterView")]
public static class ParameterViewModule
{
	[Jazor(Op.Discard ,"static Microsoft.AspNetCore.Components.ParameterView.Empty.get")]
	public extern static Object _266c57dcb70645ee();

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.ParameterView.GetEnumerator()")]
	public extern static Object _953f970337fd18b3(Object instance);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.ParameterView.TryGetValue<TValue>(string, out TValue)")]
	public extern static Array<object?> _056f011a869f60b0<TValue>(Object instance, string parameterName, TValue result);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.ParameterView.GetValueOrDefault<TValue>(string)")]
	public extern static TValue? _42a9174fe35278eb<TValue>(Object instance, string parameterName);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.ParameterView.GetValueOrDefault<TValue>(string, TValue)")]
	public extern static TValue _5510886a804b75d5<TValue>(Object instance, string parameterName, TValue defaultValue);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.ParameterView.ToDictionary()")]
	public extern static global::System.Collections.Generic.IReadOnlyDictionary<string, object> _354a84476950290e(Object instance);

	[Jazor(Op.Discard ,"static Microsoft.AspNetCore.Components.ParameterView.FromDictionary(System.Collections.Generic.IDictionary<string, object>)")]
	public extern static Object _29fafb80b90277e5(global::System.Collections.Generic.IDictionary<string, object> parameters);

	[Jazor(Op.Allowed ,"Microsoft.AspNetCore.Components.ParameterView.SetParameterProperties(object)")]
	public extern static void _d17aaef688558e60(Object instance, object target);
}
