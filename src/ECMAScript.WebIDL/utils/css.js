export function hyphenToCamelCase(name) {
    const camel = name
        .replace(/^-(\w)/, (_, c) => c)
        .replace(/-(\w)/g, (_, c) => c.toUpperCase());
    return camel === "float" ? "_float" : camel;
}
export function camelToHyphenCase(name) {
    const dashPrefix = name.startsWith("webkit") ? "-" : "";
    return dashPrefix + name.replace(/[A-Z]/g, (c) => "-" + c.toLowerCase());
}
//# sourceMappingURL=css.js.map