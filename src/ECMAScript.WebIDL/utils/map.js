export function addToArrayMap(map, name, value) {
    const array = map.get(name) || [];
    array.push(value);
    map.set(name, array);
}
export function addToStringMap(map, name, value) {
    const old = map.get(name) || "";
    map.set(name, `${old}\n${value}\n`);
}
export function addToNestedMap(map, name, key, value) {
    const nested = map.get(name) ?? new Map();
    nested.set(key, value);
    map.set(name, nested);
}
//# sourceMappingURL=map.js.map