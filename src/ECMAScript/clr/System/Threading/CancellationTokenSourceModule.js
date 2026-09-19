let PendingCancelTimers = new WeakMap;
function ScheduleCancel(controller, millisecondsDelay) {
  if (millisecondsDelay < -1 || millisecondsDelay > 2147483647)
    throw new Error("ArgumentOutOfRangeException: The delay must be -1 or between 0 and Int32.MaxValue milliseconds.");
  ClearPendingCancel(controller);
  if (millisecondsDelay === -1)
    return;
  PendingCancelTimers.set(controller, setTimeout(() => {
    controller.abort();
    return;
  }, millisecondsDelay));
}
function ClearPendingCancel(controller) {
  if (PendingCancelTimers.has(controller)) {
    clearTimeout(PendingCancelTimers.get(controller));
    PendingCancelTimers.delete(controller);
  }
}
function ToMillisecondsDelay(delay) {
  return Number(delay.ticks / BigInt(10000));
}
function CreateWithMillisecondsDelay(millisecondsDelay) {
  let controller = new AbortController;
  ScheduleCancel(controller, millisecondsDelay);
  return controller;
}
function CreateLinked(tokens) {
  let linked = new AbortController;
  let source = AbortSignal.any(tokens);
  if (source.aborted)
    linked.abort();
  else
    source.addEventListener("abort", _ => {
      linked.abort();
      return;
    }, false);
  return linked;
}
/*jazor:clr-member System.Threading.CancellationTokenSource.CancellationTokenSource(System.TimeSpan)*/
export function createWithDelay(delay) {
  return CreateWithMillisecondsDelay(ToMillisecondsDelay(delay));
}
/*jazor:clr-member System.Threading.CancellationTokenSource.CancellationTokenSource(int)*/
export function createWithMillisecondsDelay(millisecondsDelay) {
  return CreateWithMillisecondsDelay(millisecondsDelay);
}
/*jazor:clr-member System.Threading.CancellationTokenSource.CancelAfter(System.TimeSpan)*/
export function cancelAfterDelay(instance, delay) {
  ScheduleCancel(instance, ToMillisecondsDelay(delay));
}
/*jazor:clr-member System.Threading.CancellationTokenSource.CancelAfter(int)*/
export function cancelAfter(instance, millisecondsDelay) {
  ScheduleCancel(instance, millisecondsDelay);
}
/*jazor:clr-member System.Threading.CancellationTokenSource.Dispose()*/
export function dispose(instance) {
  ClearPendingCancel(instance);
}
/*jazor:clr-member static System.Threading.CancellationTokenSource.CreateLinkedTokenSource(System.Threading.CancellationToken, System.Threading.CancellationToken)*/
export function createLinkedTokenSourceFromPair(token1, token2) {
  return CreateLinked([token1, token2]);
}
/*jazor:clr-member static System.Threading.CancellationTokenSource.CreateLinkedTokenSource(System.Threading.CancellationToken)*/
export function createLinkedTokenSource(token) {
  return CreateLinked([token]);
}
