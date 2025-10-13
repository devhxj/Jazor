using System.ComponentModel;
using System.Data;

namespace ECMAScript;


/// <summary>
/// ��������Ĵ���
/// </summary>
/// <typeparam name="TTarget">Ŀ���������</typeparam>
[ECMAScript]
[Description("@#Proxy")]
public class Proxy<TTarget> where TTarget : class
{
	/// <summary>
	/// ����һ���µĴ���ʵ��
	/// </summary>
	/// <param name="target">Ŀ�����</param>
	/// <param name="handler">��������</param>
	public extern Proxy(TTarget target, ProxyHandler<TTarget> handler);

	/// <summary>
	/// ��ȡ�����Ŀ�����
	/// </summary>
	[Description("@#target")]
	public extern TTarget Target { get; }

	/// <summary>
	/// ��ȡ����Ĵ���������
	/// </summary>
	[Description("@#handler")]
	public extern ProxyHandler<TTarget> Handler { get; }
}

/// <summary>
/// ��������������������Ϊ
/// </summary>
/// <typeparam name="TTarget">Ŀ���������</typeparam>
[ECMAScript]
public abstract class ProxyHandler<TTarget> where TTarget : class
{
	/// <summary>
	/// �������Զ�ȡ������������ handler.get()
	/// </summary>
	/// <param name="target">Ŀ�����</param>
	/// <param name="property">������</param>
	/// <param name="receiver">���ն���</param>
	/// <returns></returns>
	[Description("@#get")]
	public extern virtual object Get(TTarget target, string property, object receiver);

	/// <summary>
	/// �����������ò����������� handler.set()
	/// </summary>
	/// <param name="target">Ŀ�����</param>
	/// <param name="property">������</param>
	/// <param name="value">Ҫ���õ�ֵ</param>
	/// <param name="receiver">���ն���</param>
	/// <returns></returns>
	[Description("@#set")]
	public extern virtual bool Set(TTarget target, string property, object value, object receiver);

	/// <summary>
	/// ���� in �������������� handler.has()
	/// </summary>
	/// <param name="target">Ŀ�����</param>
	/// <param name="property">������</param>
	/// <returns></returns>
	[Description("@#has")]
	public extern virtual bool Has(TTarget target, string property);

	/// <summary>
	/// ���غ������ò����������� handler.apply()
	/// </summary>
	/// <param name="target">Ŀ�����</param>
	/// <param name="thisArg">this ֵ</param>
	/// <param name="argumentsList">�����б�</param>
	/// <returns></returns>
	[Description("@#apply")]
	public extern virtual object Apply(TTarget target, object thisArg, object[] argumentsList);
}

