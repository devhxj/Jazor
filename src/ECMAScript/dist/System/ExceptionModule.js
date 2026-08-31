let HelpLinks = new WeakMap;
let Sources = new WeakMap;
function EnsureInstance(instance) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
}
function GetInnerExceptionCore(instance) {
  let __trycast$59e596616718827fbf6944d6;
  EnsureInstance(instance);
  return __trycast$59e596616718827fbf6944d6 = instance.cause, __trycast$59e596616718827fbf6944d6 instanceof Error ? __trycast$59e596616718827fbf6944d6 : null;
}
/*jazor:clr-member System.Exception.Exception(string, System.Exception)*/
export function _553ffa41c7b954da(message, innerException) {
  return new Error(message ?? "", { cause: innerException });
}
/*jazor:clr-member virtual System.Exception.GetBaseException()*/
export function _f062594f9ecd0366(instance) {
  EnsureInstance(instance);
  let current = instance;
  let inner = GetInnerExceptionCore(current);
  while (inner !== null) {
    current = inner;
    inner = GetInnerExceptionCore(current);
  }
  return current;
}
/*jazor:clr-member System.Exception.InnerException.get*/
export function _463c6b2780b746af(instance) {
  return GetInnerExceptionCore(instance);
}
/*jazor:clr-member virtual System.Exception.HelpLink.get*/
export function _cbc65d16d0767d67(instance) {
  EnsureInstance(instance);
  return HelpLinks.has(instance) ? HelpLinks.get(instance) : null;
}
/*jazor:clr-member virtual System.Exception.HelpLink.set*/
export function _30c969b3bbd3fa2e(instance, value) {
  EnsureInstance(instance);
  HelpLinks.set(instance, value);
}
/*jazor:clr-member virtual System.Exception.Source.get*/
export function _21e71d416a10c806(instance) {
  EnsureInstance(instance);
  return Sources.has(instance) ? Sources.get(instance) : null;
}
/*jazor:clr-member virtual System.Exception.Source.set*/
export function _48095d5ec6492dcb(instance, value) {
  EnsureInstance(instance);
  Sources.set(instance, value);
}
/*jazor:clr-member static System.ArgumentNullException.ThrowIfNull(object, string)*/
export function _c80ae10aa1d0d795(argument, paramName) {
  if (argument === null)
    throw new TypeError(paramName ?? "Value cannot be null.");
}
