namespace ECMAScript;

/// <summary>
/// �ṩ�� JavaScript Reflect �������ƵĹ��ܣ�����ִ�з������
/// </summary>
[ECMAScript]
[Description("@#Reflect")]
public static class Reflect
{
	/// <summary>
	/// ����Ŀ�꺯��
	/// </summary>
	/// <param name="target">Ҫ���õ�Ŀ�꺯��</param>
	/// <param name="thisArg">��������ʱ�� this ֵ</param>
	/// <param name="argumentsList">���ò����б�</param>
	/// <returns></returns>
	[Description("@#apply")]
	public extern static object Apply(object target, object thisArg, object[] argumentsList);

	/// <summary>
	/// ��Ϊ���캯������Ŀ�꺯��
	/// </summary>
	/// <param name="target">Ŀ�깹�캯��</param>
	/// <param name="argumentsList">���캯�������б�</param>
	/// <returns></returns>
	[Description("@#construct")]
	public extern static object Construct(object target, object[] argumentsList);

	/// <summary>
	/// ��ȡ�������Ե�ֵ
	/// </summary>
	/// <param name="target">Ŀ�����</param>
	/// <param name="propertyKey">���Լ�</param>
	/// <returns></returns>
	[Description("@#get")]
	public extern static object Get(object target, object propertyKey);

	/// <summary>
	/// ���ö������Ե�ֵ
	/// </summary>
	/// <param name="target">Ŀ�����</param>
	/// <param name="propertyKey">���Լ�</param>
	/// <param name="value">Ҫ���õ�ֵ</param>
	/// <returns></returns>
	[Description("@#set")]
	public extern static bool Set(object target, object propertyKey, object value);

	/// <summary>
	/// �������Ƿ����ָ������
	/// </summary>
	/// <param name="target">Ŀ�����</param>
	/// <param name="propertyKey">���Լ�</param>
	/// <returns></returns>
	[Description("@#has")]
	public extern static bool Has(object target, object propertyKey);
}

