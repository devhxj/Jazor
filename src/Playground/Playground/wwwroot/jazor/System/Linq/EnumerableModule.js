function materialize(source) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  let result = new Array;
  for (let item of source)
    result.push(item);
  return result;
}
export function _a0d3305d7a8d4c01(source, predicate) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (predicate === null)
    throw new Error("ArgumentNullException: predicate is null");
  return materialize(source).filter(item => {
    return predicate(item);
  });
}
export function _0f6f6fe4a8e94447(source, predicate) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (predicate === null)
    throw new Error("ArgumentNullException: predicate is null");
  return materialize(source).filter((item, index) => {
    return predicate(item, index);
  });
}
export function _0d5df18d09084f3b(source, selector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  return materialize(source).map(selector);
}
export function _aab4dc2444d44402(source, selector) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  if (selector === null)
    throw new Error("ArgumentNullException: selector is null");
  return materialize(source).map(selector);
}
export function _6293e95141f14a55(source) {
  return materialize(source);
}
export function _ea56f0fe56c44ae7(source) {
  return materialize(source);
}
