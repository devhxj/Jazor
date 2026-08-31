import { RegisterCancellationCallback } from "System/RuntimeModule.js";
let NoneSignal = (new AbortController).signal;
export function GetNoneSignal() {
  return NoneSignal;
}
/*jazor:clr-member System.Threading.CancellationToken.CancellationToken()*/
export function createDefaultToken() {
  return NoneSignal;
}
/*jazor:clr-member static System.Threading.CancellationToken.None.get*/
export function getNone() {
  return NoneSignal;
}
/*jazor:clr-member System.Threading.CancellationToken.CanBeCanceled.get*/
export function getCanBeCanceled(instance) {
  return instance !== NoneSignal;
}
/*jazor:clr-member System.Threading.CancellationToken.CancellationToken(bool)*/
export function createToken(canceled) {
  return canceled ? AbortSignal.abort() : NoneSignal;
}
/*jazor:clr-member System.Threading.CancellationToken.Register(System.Action)*/
export function register(instance, callback) {
  return RegisterCancellationCallback(instance, callback);
}
/*jazor:clr-member System.Threading.CancellationToken.Register(System.Action, bool)*/
export function registerWithSynchronizationContext(instance, callback, useSynchronizationContext) {
  return RegisterCancellationCallback(instance, callback);
}
/*jazor:clr-member System.Threading.CancellationToken.Register(System.Action<object>, object)*/
export function registerWithState(instance, callback, state) {
  return RegisterCancellationCallback(instance, () => {
    callback(state);
    return;
  });
}
/*jazor:clr-member System.Threading.CancellationToken.Register(System.Action<object, System.Threading.CancellationToken>, object)*/
export function registerWithStateAndToken(instance, callback, state) {
  return RegisterCancellationCallback(instance, () => {
    callback(state, instance);
    return;
  });
}
/*jazor:clr-member System.Threading.CancellationToken.Register(System.Action<object>, object, bool)*/
export function registerWithStateAndSynchronizationContext(instance, callback, state, useSynchronizationContext) {
  return RegisterCancellationCallback(instance, () => {
    callback(state);
    return;
  });
}
/*jazor:clr-member System.Threading.CancellationToken.UnsafeRegister(System.Action<object>, object)*/
export function unsafeRegisterWithState(instance, callback, state) {
  return RegisterCancellationCallback(instance, () => {
    callback(state);
    return;
  });
}
/*jazor:clr-member System.Threading.CancellationToken.UnsafeRegister(System.Action<object, System.Threading.CancellationToken>, object)*/
export function unsafeRegisterWithStateAndToken(instance, callback, state) {
  return RegisterCancellationCallback(instance, () => {
    callback(state, instance);
    return;
  });
}
/*jazor:clr-member System.Threading.CancellationToken.ThrowIfCancellationRequested()*/
export function throwIfCancellationRequested(instance) {
  if (instance.aborted)
    throw new Error("OperationCanceledException: The operation was canceled.");
}
