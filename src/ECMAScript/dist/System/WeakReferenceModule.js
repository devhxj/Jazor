let ReplacementReferences = new WeakMap;
let StrongTargets = new WeakMap;
function CanUseWeakReference(value) {
  if (value === null)
    return false;
  let type = typeof value;
  return type === "object" || type === "function";
}
function Create(target) {
  if (CanUseWeakReference(target))
    return new WeakRef(target);
  let instance = new WeakRef(new Error);
  StrongTargets.set(instance, target);
  return instance;
}
function GetTarget(instance) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  if (StrongTargets.has(instance))
    return StrongTargets.get(instance);
  let reference = ReplacementReferences.has(instance) ? ReplacementReferences.get(instance) : instance;
  return reference.deref() ?? null;
}
function SetTarget(instance, value) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  if (CanUseWeakReference(value)) {
    StrongTargets.delete(instance);
    ReplacementReferences.set(instance, new WeakRef(value));
    return;
  }
  ReplacementReferences.delete(instance);
  StrongTargets.set(instance, value);
}
/*jazor:clr-member System.WeakReference.WeakReference(object)*/
export function _9a41b3fc95053633(target) {
  return Create(target);
}
/*jazor:clr-member System.WeakReference.WeakReference(object, bool)*/
export function _bb3cf7219c9626be(target, trackResurrection) {
  if (trackResurrection)
    throw new Error("NotSupportedException: WeakReference resurrection tracking is not available in the JavaScript runtime.");
  return Create(target);
}
/*jazor:clr-member virtual System.WeakReference.IsAlive.get*/
export function _c3d16f7de644412a(instance) {
  return GetTarget(instance) !== null;
}
/*jazor:clr-member virtual System.WeakReference.Target.get*/
export function _ba77d80a1e80efa6(instance) {
  return GetTarget(instance);
}
/*jazor:clr-member virtual System.WeakReference.Target.set*/
export function _6576d2b2ae762786(instance, value) {
  SetTarget(instance, value);
}
