export function filterMapRecord(object, mapper, forNamespace) {
    if (!object) {
        return;
    }
    const result = {};
    for (const [key, value] of Object.entries(object)) {
        const mdnKey = forNamespace || ("static" in value && value.static)
            ? `${key}_static`
            : key;
        const newValue = mapper(mdnKey, value);
        if (newValue !== undefined) {
            result[key] = newValue;
        }
    }
    return result;
}
export function isEmptyRecord(o) {
    return !o || !Object.keys(o).length;
}
//# sourceMappingURL=record.js.map