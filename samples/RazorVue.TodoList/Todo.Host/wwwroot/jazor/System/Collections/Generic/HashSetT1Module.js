function ensureInstance(instance) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
}
function ensureOther(other) {
  if (other === null)
    throw new Error("ArgumentNullException: other is null");
}
export function createLookupSet(values) {
  ensureOther(values);
  let lookup = new Set;
  for (let value of values)
    lookup.add(value);
  return lookup;
}
export function unionWithCore(instance, other) {
  ensureInstance(instance);
  ensureOther(other);
  for (let item of other)
    instance.add(item);
}
export function intersectWithCore(instance, other) {
  ensureInstance(instance);
  let lookup = createLookupSet(other);
  for (let item of instance) {
    let current = item;
    if (!lookup.has(current))
      instance.delete(current);
  }
}
export function exceptWithCore(instance, other) {
  ensureInstance(instance);
  ensureOther(other);
  for (let item of other)
    instance.delete(item);
}
export function symmetricExceptWithCore(instance, other) {
  ensureInstance(instance);
  let lookup = createLookupSet(other);
  for (let item of lookup) {
    let current = item;
    if (instance.has(current))
      instance.delete(current);
    else
      instance.add(current);
  }
}
export function isSubsetOfCore(instance, other) {
  ensureInstance(instance);
  let lookup = createLookupSet(other);
  for (let item of instance) {
    let current = item;
    if (!lookup.has(current))
      return false;
  }
  return true;
}
export function isProperSubsetOfCore(instance, other) {
  ensureInstance(instance);
  let lookup = createLookupSet(other);
  if (instance.size >= lookup.size)
    return false;
  for (let item of instance) {
    let current = item;
    if (!lookup.has(current))
      return false;
  }
  return true;
}
export function isSupersetOfCore(instance, other) {
  ensureInstance(instance);
  ensureOther(other);
  for (let item of other) {
    if (!instance.has(item))
      return false;
  }
  return true;
}
export function isProperSupersetOfCore(instance, other) {
  ensureInstance(instance);
  let lookup = createLookupSet(other);
  if (instance.size <= lookup.size)
    return false;
  for (let item of lookup) {
    let current = item;
    if (!instance.has(current))
      return false;
  }
  return true;
}
export function overlapsCore(instance, other) {
  ensureInstance(instance);
  ensureOther(other);
  for (let item of other) {
    if (instance.has(item))
      return true;
  }
  return false;
}
export function setEqualsCore(instance, other) {
  ensureInstance(instance);
  let lookup = createLookupSet(other);
  if (instance.size !== lookup.size)
    return false;
  for (let item of lookup) {
    let current = item;
    if (!instance.has(current))
      return false;
  }
  return true;
}
export function _e1d2ba750a2788cb(instance, item) {
  ensureInstance(instance);
  let size = instance.size;
  instance.add(item);
  return instance.size > size;
}
export function _b2bd5d22aadd44a8(instance, other) {
  unionWithCore(instance, other);
}
export function _3a6a072035334578(instance, other) {
  intersectWithCore(instance, other);
}
export function _373e2e9ed1fb3f5b(instance, other) {
  exceptWithCore(instance, other);
}
export function _a22fe44dc0ae9ad2(instance, other) {
  symmetricExceptWithCore(instance, other);
}
export function _23c8bcfc6b71d2b1(instance, other) {
  return isSubsetOfCore(instance, other);
}
export function _fb8566ae66aa9591(instance, other) {
  return isProperSubsetOfCore(instance, other);
}
export function _3be7fbb1d68799fb(instance, other) {
  return isSupersetOfCore(instance, other);
}
export function _cc0cc2d0f5be70db(instance, other) {
  return isProperSupersetOfCore(instance, other);
}
export function _84709aa8ff70a52a(instance, other) {
  return overlapsCore(instance, other);
}
export function _55425d259e5f54ea(instance, other) {
  return setEqualsCore(instance, other);
}
export const HashSetT1Module = {
  ensureInstance,
  ensureOther,
  createLookupSet,
  unionWithCore,
  intersectWithCore,
  exceptWithCore,
  symmetricExceptWithCore,
  isSubsetOfCore,
  isProperSubsetOfCore,
  isSupersetOfCore,
  isProperSupersetOfCore,
  overlapsCore,
  setEqualsCore,
  _e1d2ba750a2788cb,
  _b2bd5d22aadd44a8,
  _3a6a072035334578,
  _373e2e9ed1fb3f5b,
  _a22fe44dc0ae9ad2,
  _23c8bcfc6b71d2b1,
  _fb8566ae66aa9591,
  _3be7fbb1d68799fb,
  _cc0cc2d0f5be70db,
  _84709aa8ff70a52a,
  _55425d259e5f54ea
};
