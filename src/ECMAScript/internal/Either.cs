using System.ComponentModel;

namespace ECMAScript;

[ECMAScript]
public interface IEither
{
}

[ECMAScript]
[Description("@#")]
public readonly struct Either<T0, T1> : IEither
{
	[Description("@#")]
	public extern T0? AsT0 { get; }

	[Description("@#")]
	public extern T1? AsT1 { get; }

	public extern static implicit operator Either<T0, T1>(T0 t);

	public extern static implicit operator Either<T0, T1>(T1 t);

}

[ECMAScript]
[Description("@#")]
public readonly struct Either<T0, T1, T2> : IEither
{
	[Description("@#")]
	public extern T0? AsT0 { get; }

	[Description("@#")]
	public extern T1? AsT1 { get; }

	[Description("@#")]
	public extern T2? AsT2 { get; }

	public extern static implicit operator Either<T0, T1, T2>(T0 t);

	public extern static implicit operator Either<T0, T1, T2>(T1 t);

	public extern static implicit operator Either<T0, T1, T2>(T2 t);

}

[ECMAScript]
[Description("@#")]
public readonly struct Either<T0, T1, T2, T3> : IEither
{
	[Description("@#")]
	public extern T0? AsT0 { get; }

	[Description("@#")]
	public extern T1? AsT1 { get; }

	[Description("@#")]
	public extern T2? AsT2 { get; }

	[Description("@#")]
	public extern T3? AsT3 { get; }

	public extern static implicit operator Either<T0, T1, T2, T3>(T0 t);

	public extern static implicit operator Either<T0, T1, T2, T3>(T1 t);

	public extern static implicit operator Either<T0, T1, T2, T3>(T2 t);

	public extern static implicit operator Either<T0, T1, T2, T3>(T3 t);

}

[ECMAScript]
[Description("@#")]
public readonly struct Either<T0, T1, T2, T3, T4> : IEither
{
	[Description("@#")]
	public extern T0? AsT0 { get; }

	[Description("@#")]
	public extern T1? AsT1 { get; }

	[Description("@#")]
	public extern T2? AsT2 { get; }

	[Description("@#")]
	public extern T3? AsT3 { get; }

	[Description("@#")]
	public extern T4? AsT4 { get; }

	public extern static implicit operator Either<T0, T1, T2, T3, T4>(T0 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4>(T1 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4>(T2 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4>(T3 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4>(T4 t);

}

[ECMAScript]
[Description("@#")]
public readonly struct Either<T0, T1, T2, T3, T4, T5> : IEither
{
	[Description("@#")]
	public extern T0? AsT0 { get; }

	[Description("@#")]
	public extern T1? AsT1 { get; }

	[Description("@#")]
	public extern T2? AsT2 { get; }

	[Description("@#")]
	public extern T3? AsT3 { get; }

	[Description("@#")]
	public extern T4? AsT4 { get; }

	[Description("@#")]
	public extern T5? AsT5 { get; }

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5>(T0 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5>(T1 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5>(T2 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5>(T3 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5>(T4 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5>(T5 t);

}

[ECMAScript]
[Description("@#")]
public readonly struct Either<T0, T1, T2, T3, T4, T5, T6> : IEither
{
	[Description("@#")]
	public extern T0? AsT0 { get; }

	[Description("@#")]
	public extern T1? AsT1 { get; }

	[Description("@#")]
	public extern T2? AsT2 { get; }

	[Description("@#")]
	public extern T3? AsT3 { get; }

	[Description("@#")]
	public extern T4? AsT4 { get; }

	[Description("@#")]
	public extern T5? AsT5 { get; }

	[Description("@#")]
	public extern T6? AsT6 { get; }

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6>(T0 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6>(T1 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6>(T2 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6>(T3 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6>(T4 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6>(T5 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6>(T6 t);

}

[ECMAScript]
[Description("@#")]
public readonly struct Either<T0, T1, T2, T3, T4, T5, T6, T7> : IEither
{
	[Description("@#")]
	public extern T0? AsT0 { get; }

	[Description("@#")]
	public extern T1? AsT1 { get; }

	[Description("@#")]
	public extern T2? AsT2 { get; }

	[Description("@#")]
	public extern T3? AsT3 { get; }

	[Description("@#")]
	public extern T4? AsT4 { get; }

	[Description("@#")]
	public extern T5? AsT5 { get; }

	[Description("@#")]
	public extern T6? AsT6 { get; }

	[Description("@#")]
	public extern T7? AsT7 { get; }

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7>(T0 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7>(T1 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7>(T2 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7>(T3 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7>(T4 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7>(T5 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7>(T6 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7>(T7 t);

}

[ECMAScript]
[Description("@#")]
public readonly struct Either<T0, T1, T2, T3, T4, T5, T6, T7, T8> : IEither
{
	[Description("@#")]
	public extern T0? AsT0 { get; }

	[Description("@#")]
	public extern T1? AsT1 { get; }

	[Description("@#")]
	public extern T2? AsT2 { get; }

	[Description("@#")]
	public extern T3? AsT3 { get; }

	[Description("@#")]
	public extern T4? AsT4 { get; }

	[Description("@#")]
	public extern T5? AsT5 { get; }

	[Description("@#")]
	public extern T6? AsT6 { get; }

	[Description("@#")]
	public extern T7? AsT7 { get; }

	[Description("@#")]
	public extern T8? AsT8 { get; }

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8>(T0 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8>(T1 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8>(T2 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8>(T3 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8>(T4 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8>(T5 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8>(T6 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8>(T7 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8>(T8 t);

}

[ECMAScript]
[Description("@#")]
public readonly struct Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> : IEither
{
	[Description("@#")]
	public extern T0? AsT0 { get; }

	[Description("@#")]
	public extern T1? AsT1 { get; }

	[Description("@#")]
	public extern T2? AsT2 { get; }

	[Description("@#")]
	public extern T3? AsT3 { get; }

	[Description("@#")]
	public extern T4? AsT4 { get; }

	[Description("@#")]
	public extern T5? AsT5 { get; }

	[Description("@#")]
	public extern T6? AsT6 { get; }

	[Description("@#")]
	public extern T7? AsT7 { get; }

	[Description("@#")]
	public extern T8? AsT8 { get; }

	[Description("@#")]
	public extern T9? AsT9 { get; }

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T0 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T2 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T3 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T4 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T5 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T6 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T7 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T8 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T9 t);

}

[ECMAScript]
[Description("@#")]
public readonly struct Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IEither
{
	[Description("@#")]
	public extern T0? AsT0 { get; }

	[Description("@#")]
	public extern T1? AsT1 { get; }

	[Description("@#")]
	public extern T2? AsT2 { get; }

	[Description("@#")]
	public extern T3? AsT3 { get; }

	[Description("@#")]
	public extern T4? AsT4 { get; }

	[Description("@#")]
	public extern T5? AsT5 { get; }

	[Description("@#")]
	public extern T6? AsT6 { get; }

	[Description("@#")]
	public extern T7? AsT7 { get; }

	[Description("@#")]
	public extern T8? AsT8 { get; }

	[Description("@#")]
	public extern T9? AsT9 { get; }

	[Description("@#")]
	public extern T10? AsT10 { get; }

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T0 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T2 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T3 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T4 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T5 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T6 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T7 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T8 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T9 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T10 t);

}

[ECMAScript]
[Description("@#")]
public readonly struct Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : IEither
{
	[Description("@#")]
	public extern T0? AsT0 { get; }

	[Description("@#")]
	public extern T1? AsT1 { get; }

	[Description("@#")]
	public extern T2? AsT2 { get; }

	[Description("@#")]
	public extern T3? AsT3 { get; }

	[Description("@#")]
	public extern T4? AsT4 { get; }

	[Description("@#")]
	public extern T5? AsT5 { get; }

	[Description("@#")]
	public extern T6? AsT6 { get; }

	[Description("@#")]
	public extern T7? AsT7 { get; }

	[Description("@#")]
	public extern T8? AsT8 { get; }

	[Description("@#")]
	public extern T9? AsT9 { get; }

	[Description("@#")]
	public extern T10? AsT10 { get; }

	[Description("@#")]
	public extern T11? AsT11 { get; }

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T0 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T2 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T3 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T4 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T5 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T6 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T7 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T8 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T9 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T10 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T11 t);

}

[ECMAScript]
[Description("@#")]
public readonly struct Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : IEither
{
	[Description("@#")]
	public extern T0? AsT0 { get; }

	[Description("@#")]
	public extern T1? AsT1 { get; }

	[Description("@#")]
	public extern T2? AsT2 { get; }

	[Description("@#")]
	public extern T3? AsT3 { get; }

	[Description("@#")]
	public extern T4? AsT4 { get; }

	[Description("@#")]
	public extern T5? AsT5 { get; }

	[Description("@#")]
	public extern T6? AsT6 { get; }

	[Description("@#")]
	public extern T7? AsT7 { get; }

	[Description("@#")]
	public extern T8? AsT8 { get; }

	[Description("@#")]
	public extern T9? AsT9 { get; }

	[Description("@#")]
	public extern T10? AsT10 { get; }

	[Description("@#")]
	public extern T11? AsT11 { get; }

	[Description("@#")]
	public extern T12? AsT12 { get; }

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T0 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T2 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T3 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T4 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T5 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T6 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T7 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T8 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T9 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T10 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T11 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T12 t);

}

[ECMAScript]
[Description("@#")]
public readonly struct Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : IEither
{
	[Description("@#")]
	public extern T0? AsT0 { get; }

	[Description("@#")]
	public extern T1? AsT1 { get; }

	[Description("@#")]
	public extern T2? AsT2 { get; }

	[Description("@#")]
	public extern T3? AsT3 { get; }

	[Description("@#")]
	public extern T4? AsT4 { get; }

	[Description("@#")]
	public extern T5? AsT5 { get; }

	[Description("@#")]
	public extern T6? AsT6 { get; }

	[Description("@#")]
	public extern T7? AsT7 { get; }

	[Description("@#")]
	public extern T8? AsT8 { get; }

	[Description("@#")]
	public extern T9? AsT9 { get; }

	[Description("@#")]
	public extern T10? AsT10 { get; }

	[Description("@#")]
	public extern T11? AsT11 { get; }

	[Description("@#")]
	public extern T12? AsT12 { get; }

	[Description("@#")]
	public extern T13? AsT13 { get; }

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T0 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T2 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T3 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T4 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T5 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T6 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T7 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T8 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T9 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T10 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T11 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T12 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T13 t);

}

[ECMAScript]
[Description("@#")]
public readonly struct Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : IEither
{
	[Description("@#")]
	public extern T0? AsT0 { get; }

	[Description("@#")]
	public extern T1? AsT1 { get; }

	[Description("@#")]
	public extern T2? AsT2 { get; }

	[Description("@#")]
	public extern T3? AsT3 { get; }

	[Description("@#")]
	public extern T4? AsT4 { get; }

	[Description("@#")]
	public extern T5? AsT5 { get; }

	[Description("@#")]
	public extern T6? AsT6 { get; }

	[Description("@#")]
	public extern T7? AsT7 { get; }

	[Description("@#")]
	public extern T8? AsT8 { get; }

	[Description("@#")]
	public extern T9? AsT9 { get; }

	[Description("@#")]
	public extern T10? AsT10 { get; }

	[Description("@#")]
	public extern T11? AsT11 { get; }

	[Description("@#")]
	public extern T12? AsT12 { get; }

	[Description("@#")]
	public extern T13? AsT13 { get; }

	[Description("@#")]
	public extern T14? AsT14 { get; }

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T0 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T2 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T3 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T4 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T5 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T6 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T7 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T8 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T9 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T10 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T11 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T12 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T13 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T14 t);

}

[ECMAScript]
[Description("@#")]
public readonly struct Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : IEither
{
	[Description("@#")]
	public extern T0? AsT0 { get; }

	[Description("@#")]
	public extern T1? AsT1 { get; }

	[Description("@#")]
	public extern T2? AsT2 { get; }

	[Description("@#")]
	public extern T3? AsT3 { get; }

	[Description("@#")]
	public extern T4? AsT4 { get; }

	[Description("@#")]
	public extern T5? AsT5 { get; }

	[Description("@#")]
	public extern T6? AsT6 { get; }

	[Description("@#")]
	public extern T7? AsT7 { get; }

	[Description("@#")]
	public extern T8? AsT8 { get; }

	[Description("@#")]
	public extern T9? AsT9 { get; }

	[Description("@#")]
	public extern T10? AsT10 { get; }

	[Description("@#")]
	public extern T11? AsT11 { get; }

	[Description("@#")]
	public extern T12? AsT12 { get; }

	[Description("@#")]
	public extern T13? AsT13 { get; }

	[Description("@#")]
	public extern T14? AsT14 { get; }

	[Description("@#")]
	public extern T15? AsT15 { get; }

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T0 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T1 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T2 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T3 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T4 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T5 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T6 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T7 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T8 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T9 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T10 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T11 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T12 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T13 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T14 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T15 t);

}

[ECMAScript]
[Description("@#")]
public readonly struct Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : IEither
{
	[Description("@#")]
	public extern T0? AsT0 { get; }

	[Description("@#")]
	public extern T1? AsT1 { get; }

	[Description("@#")]
	public extern T2? AsT2 { get; }

	[Description("@#")]
	public extern T3? AsT3 { get; }

	[Description("@#")]
	public extern T4? AsT4 { get; }

	[Description("@#")]
	public extern T5? AsT5 { get; }

	[Description("@#")]
	public extern T6? AsT6 { get; }

	[Description("@#")]
	public extern T7? AsT7 { get; }

	[Description("@#")]
	public extern T8? AsT8 { get; }

	[Description("@#")]
	public extern T9? AsT9 { get; }

	[Description("@#")]
	public extern T10? AsT10 { get; }

	[Description("@#")]
	public extern T11? AsT11 { get; }

	[Description("@#")]
	public extern T12? AsT12 { get; }

	[Description("@#")]
	public extern T13? AsT13 { get; }

	[Description("@#")]
	public extern T14? AsT14 { get; }

	[Description("@#")]
	public extern T15? AsT15 { get; }

	[Description("@#")]
	public extern T16? AsT16 { get; }

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T0 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T1 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T2 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T3 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T4 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T5 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T6 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T7 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T8 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T9 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T10 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T11 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T12 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T13 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T14 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T15 t);

	public extern static implicit operator Either<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T16 t);

}
